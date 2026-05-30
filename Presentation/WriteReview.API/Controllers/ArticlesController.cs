using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Security.Claims;
using WriteReview.Application.Repositories.Article;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Dtos.RequestDto;
using WriteReview.Domain.Dtos.ResponseDto;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Domain.Security;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Repositories.Article;
using WriteReview.Persistence.Services.Articles;
using Microsoft.AspNetCore.SignalR;
using WriteReview.API.Hubs;
using Ganss.Xss;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly WriteReviewDbContext _db;
        private readonly IActorContextAccessor _actor;
        private readonly ArticleService _articleService;
        private readonly ArticleStateService _articleStateService;
        private readonly IArticleWriteRepository _articleWriteRepository;
        private readonly IArticleReadRepository _articleReadRepository;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ArticlesController(
            WriteReviewDbContext db, 
            IActorContextAccessor actor, 
            ArticleService articleService, 
            ArticleStateService articleStateService, 
            IArticleWriteRepository articleWriteRepository, 
            IArticleReadRepository articleReadRepository,
            IHubContext<NotificationHub> hubContext)
        {
            _db = db;
            _actor = actor;
            _articleService = articleService;
            _articleStateService = articleStateService;
            _articleWriteRepository = articleWriteRepository;
            _articleReadRepository = articleReadRepository;
            _hubContext = hubContext;
        }

        [Authorize(Roles = Roles.Author)]
        [HttpGet("mine")]
        public async Task<IActionResult> Mine(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] ArticleStatus? status = null
            )
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;
            var me = _actor.GetCurrent().UserId;

            var q = _db.Articles
                .AsNoTracking()
                .Where(a => a.AuthorId == me);

            if (status.HasValue)
                q = q.Where(a => a.Status == status.Value);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(a => a.UpdatedAt)
                .Include(a => a.Category)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArticleListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Status = (int)a.Status,
                    UpdatedAt = a.UpdatedAt,
                    Category = a.Category != null ? a.Category.Name : null
                })
                .ToListAsync();

            return Ok(new PagedResult<ArticleListItemDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            });

        }


        [Authorize(Roles = Roles.Author)]
        [HttpPost]
        public async Task<IActionResult> CreateArticle(
            [FromBody] CreateArticleRequest createArticleRequest
            )
        {
            try
            {
                var createArticleDto = createArticleRequest.ArticleDto;
                var isSubmit = createArticleRequest.IsSubmit;

                // XSS Sanitization
                if (!string.IsNullOrWhiteSpace(createArticleDto.Content))
                {
                    createArticleDto.Content = new HtmlSanitizer().Sanitize(createArticleDto.Content);
                }

                var addArticleMessage = await _articleWriteRepository.AddArticleWithAuthor(
                    _actor,
                    createArticleDto,
                    isSubmit,
                    _articleStateService);

                if (isSubmit)
                {
                    await NotifyManagersAsync($"'{createArticleDto.Title}' başlıklı yeni bir makale inceleme bekliyor.");
                }

                return Ok(new AddArticleResponse { Message = addArticleMessage });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Message = "Kayıt işlemi sırasında hata oluştu.", Detail = ex.Message });
            }
        }



        [Authorize(Roles = Roles.Author)]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateArticle(Guid id, [FromBody] CreateArticleRequest request)
        {
            var me = _actor.GetCurrent().UserId;

            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article is null)
                return NotFound(new { Message = "Makale bulunamadı." });

            if (article.AuthorId != me)
                return Forbid();

            if (article.Status != ArticleStatus.Draft && article.Status != ArticleStatus.Submitted && article.Status != ArticleStatus.RevisionsRequested)
                return BadRequest(new { Message = "Yalnızca taslak, gönderilmiş veya revizyon beklenen makaleler düzenlenebilir." });

            if (article.Status == ArticleStatus.RevisionsRequested)
            {
                var currentVersionsCount = await _db.ArticleVersions.CountAsync(v => v.ArticleId == article.Id);
                var expectedVersionNumber = currentVersionsCount + 1;
                var snapshotExists = await _db.ArticleVersions.AnyAsync(v => v.ArticleId == article.Id && v.VersionNumber == expectedVersionNumber);
                
                if (!snapshotExists)
                {
                    var version = new ArticleVersion
                    {
                        ArticleId = article.Id,
                        VersionNumber = expectedVersionNumber,
                        ContentPath = article.ContentPath,
                        CreatedAt = DateTime.UtcNow
                    };
                    _db.ArticleVersions.Add(version);

                    var oldReviews = await _db.ArticleReviews
                        .Where(r => r.ArticleId == article.Id && r.ArticleVersionId == null)
                        .ToListAsync();
                    
                    foreach (var review in oldReviews)
                    {
                        review.ArticleVersionId = version.Id;
                    }
                }
            }

            var dto = request.ArticleDto;

            if (!string.IsNullOrWhiteSpace(dto.Title))
                article.Title = dto.Title.Trim();

            if (!string.IsNullOrWhiteSpace(dto.Content))
                article.ContentPath = new HtmlSanitizer().Sanitize(dto.Content);

            if (Guid.TryParse(dto.CategoryId, out var categoryGuid))
                article.CategoryId = categoryGuid;

            article.UpdatedAt = DateTime.UtcNow;

            if (request.IsSubmit)
            {
                if (article.Status == ArticleStatus.Draft)
                {
                    _articleStateService.DraftToSubmitted(article);
                    await NotifyManagersAsync($"'{article.Title}' başlıklı makale inceleme için gönderildi.");
                }
                else
                {
                    _articleStateService.RevisionsRequestedToSubmitted(article);
                    await NotifyManagersAsync($"'{article.Title}' başlıklı makalenin revizyonu gönderildi.");
                    
                    // Reset expert assignments so they review V2
                    var assignments = await _db.ArticleExpertAssignments.Where(a => a.ArticleId == article.Id).ToListAsync();
                    foreach (var assignment in assignments)
                    {
                        assignment.Status = ExpertAssignmentStatus.Pending;
                        assignment.Feedback = null;
                        assignment.Score = null;
                        assignment.ReviewedAt = DateTime.UtcNow;
                    }
                }
            }

            await _db.SaveChangesAsync();
            return Ok(new { id = article.Id, status = (int)article.Status });
        }

        [Authorize(Roles = "Admin,Editor,Manager,Author,Expert")]
        [HttpGet("{articleId}/versions")]
        public async Task<IActionResult> GetArticleVersions(Guid articleId)
        {
            var versions = await _db.ArticleVersions
                .Where(v => v.ArticleId == articleId)
                .OrderBy(v => v.VersionNumber)
                .Select(v => new
                {
                    v.Id,
                    v.VersionNumber,
                    v.ContentPath,
                    v.CreatedAt
                })
                .ToListAsync();

            return Ok(versions);
        }

        [Authorize(Roles = "Admin,Editor,Manager,Author,Expert")]
        [HttpGet("{articleId}")]
        public async Task<IActionResult> GetArticleWithId(string articleId)
        {
            var articleDto = await _articleReadRepository.GetArticleWithCategoryAndAuthor(articleId);

            if (User.IsInRole("Author") && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Editor"))
            {
                if (articleDto.Experts != null)
                {
                    foreach (var expert in articleDto.Experts)
                    {
                        expert.ExpertName = "Gizli Hakem";
                    }
                }
            }

            return Ok(articleDto);
        }

        /// <summary>
        /// Belirtilen id değerinde sahip uzmanın belirtilen id değerine sahip makaleye yaptığı değerlendirmeyi getirir.
        /// </summary>
        /// <param name="articleId"></param>
        /// <param name="expertId"></param>
        /// <returns></returns>
        [Authorize(Roles = "Admin,Manager,Expert,Author")]
        [HttpGet("{articleId}/review")]
        public async Task<IActionResult> GetArticleExpertReview(string articleId, [FromQuery] string expertId)
        {
            var article = await _db.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync(i => i.Id == Guid.Parse(articleId));

            if (article is null)
                return NotFound();

            var review = _db.Articles.Include(a => a.ExpertAssignments)
                .Where(a=> a.AuthorId == Guid.Parse(expertId))
                .Select(a => a.ExpertAssignments);

            return Ok( review);
        }



        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
        [HttpGet("{id:guid}/reviews")]
        public async Task<IActionResult> GetReviews(Guid id)
        {

            var article = await _db.Articles
                .AsNoTracking()
                .FirstOrDefaultAsync(a => a.Id == id);

            if (article is null)
                return NotFound();

            var staffReviews = await _db.ArticleReviews
                .AsNoTracking()
                .Where(r => r.ArticleId == id)
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    type = "staff",
                    action = r.Action,
                    note = r.Note,
                    reason = r.Reason,
                    fromStatus = r.FromStatus,
                    toStatus = r.ToStatus,
                    createdAt = r.CreatedAt,
                    reviewerId = r.ReviewerId
                })
                .ToListAsync();

            var isAuthorOnly = User.IsInRole("Author") && !User.IsInRole("Admin") && !User.IsInRole("Manager") && !User.IsInRole("Editor");

            var expertReviews = await _db.ArticleExpertAssignments
                .AsNoTracking()
                .Include(x => x.Expert)
                .Where(x => x.ArticleId == id)
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.ReviewedAt)
                .Select(x => new
                {
                    type = "expert",
                    expertId = isAuthorOnly ? Guid.Empty : x.ExpertId,
                    expertEmail = isAuthorOnly ? "gizli@hakem.com" : x.Expert.Email,
                    feedback = x.Feedback,
                    score = x.Score,
                    status = x.Status,
                    reviewedAt = x.ReviewedAt
                })
                .ToListAsync();

            return Ok(new
            {
                articleId = id,
                staff = staffReviews,
                experts = expertReviews
            });


            
            



            //var me = _actor.GetCurrent().UserId;
            //var roles = User.FindAll(ClaimTypes.Role).Select(r => r.Value).ToHashSet(StringComparer.OrdinalIgnoreCase);
            //var isStaff = roles.Contains("Admin") || roles.Contains("Editor") || roles.Contains("Manager");

            //var canSee = await _db.Articles
            //    .AnyAsync(a => a.Id == id && (isStaff || a.AuthorId == me));
            //if (!canSee) return NotFound();

            //var list = await _db.ArticleReviews
            //    .AsNoTracking()
            //    .Where(r => r.ArticleId == id)
            //    .OrderBy(r => r.CreatedAt)
            //    .Select(r => new ArticleReviewDto
            //    {
            //        Id = r.Id,
            //        Action = (int)r.Action,
            //        Note = r.Note,
            //        Reason = r.Reason,
            //        FromStatus = (int)r.FromStatus,
            //        ToStatus = (int)r.ToStatus,
            //        ReviewerId = r.ReviewerId,
            //        CreatedAt = r.CreatedAt
            //    })
            //    .ToListAsync();

            //return Ok(list);
        }

        [Authorize(Roles = "Admin,Manager")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAll(
            [FromQuery] ArticleStatus? status = null,
            [FromQuery] string? search = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;

            var q = _db.Articles.AsNoTracking();

            if (status.HasValue)
                q = q.Where(a => a.Status == status.Value);
            else
                q = q.Where(a => a.Status != ArticleStatus.Draft);

            if (!string.IsNullOrWhiteSpace(search))
            {
                var lowerSearch = search.ToLower();
                q = q.Where(a => (a.Title != null && a.Title.ToLower().Contains(lowerSearch)) || 
                                 (a.Author != null && a.Author.FullName != null && a.Author.FullName.ToLower().Contains(lowerSearch)) ||
                                 (a.Category != null && a.Category.Name != null && a.Category.Name.ToLower().Contains(lowerSearch)));
            }

            var total = await q.CountAsync();
            var items = await q
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ExpertAssignments)
                .OrderByDescending(a => a.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new WriteReview.Domain.Dtos.Staff.ManagerArticleListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    AuthorId = a.AuthorId,
                    AuthorName = a.Author.FullName,
                    Status = (int)a.Status,
                    Experts = a.ExpertAssignments
                        .Select(ea => ea.Expert.FullName)
                        .ToList(),
                    Category = a.Category != null ? a.Category.Name : null,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(new PagedResult<WriteReview.Domain.Dtos.Staff.ManagerArticleListItemDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteArticle(Guid id)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article == null)
            {
                return NotFound(new { Message = "Makale bulunamadı." });
            }

            _db.Articles.Remove(article);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Makale başarıyla silindi." });
        }

        /// <summary>
        /// Yazarın kendi makalesini silmesini sağlar.
        /// Yalnızca Draft, Submitted veya RevisionsRequested durumundaki makaleler silinebilir.
        /// İncelemeye alınmış (InReview, Approved, Rejected) makaleler silinemez.
        /// </summary>
        [Authorize(Roles = Roles.Author)]
        [HttpDelete("{id:guid}/author-delete")]
        public async Task<IActionResult> AuthorDeleteArticle(Guid id)
        {
            var me = _actor.GetCurrent().UserId;

            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article is null)
                return NotFound(new { Message = "Makale bulunamadı." });

            if (article.AuthorId != me)
                return Forbid();

            var deletableStatuses = new[] { ArticleStatus.Draft, ArticleStatus.Submitted, ArticleStatus.RevisionsRequested };
            if (!deletableStatuses.Contains(article.Status))
                return BadRequest(new { Message = "İncelemeye alınmış makaleler silinemez." });

            _db.Articles.Remove(article);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Makale başarıyla silindi." });
        }

        /// <summary>
        /// Yazar, reddedilen makalesine itiraz eder.
        /// Her makale için yalnızca 1 itiraz hakkı vardır.
        /// </summary>
        [Authorize(Roles = Roles.Author)]
        [HttpPost("{id:guid}/appeal")]
        public async Task<IActionResult> SubmitAppeal(Guid id, [FromBody] AppealRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.Reason) || request.Reason.Trim().Length < 20)
                return BadRequest(new { Message = "İtiraz gerekçesi en az 20 karakter olmalıdır." });

            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article is null)
                return NotFound(new { Message = "Makale bulunamadı." });

            try
            {
                _articleStateService.RejectedToAppealPending(article, request.Reason);
                await _db.SaveChangesAsync();
                return Ok(new { id = article.Id, status = (int)article.Status, Message = "İtirazınız başarıyla iletildi." });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        private async Task NotifyManagersAsync(string message)
        {
            var managerRole = await _db.Roles.FirstOrDefaultAsync(r => r.Name == Roles.Manager);
            if (managerRole != null)
            {
                var managerIds = await _db.UserRoles
                    .Where(ur => ur.RoleId == managerRole.Id)
                    .Select(ur => ur.UserId)
                    .ToListAsync();

                foreach (var mId in managerIds)
                {
                    var notif = new AppNotification
                    {
                        UserId = mId,
                        Title = "Yeni Makale İşlemi",
                        Message = message
                    };
                    _db.Notifications.Add(notif);
                    await _hubContext.Clients.Group(mId.ToString()).SendAsync("ReceiveNotification", notif);
                }
                await _db.SaveChangesAsync();
            }
        }
    }
}
