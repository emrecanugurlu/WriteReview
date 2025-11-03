using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using System.Security.Claims;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Dtos.RequestDto;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Domain.Security;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Services.Articles;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly WriteReviewDbContext _db;
        private readonly IActorContextAccessor _actor;
        private readonly ArticleService _articleService;
        private readonly ArticleStateService _state;

        public ArticlesController(WriteReviewDbContext db, IActorContextAccessor actor, ArticleService articleService, ArticleStateService articleStateService)
        {
            _db = db;
            _actor = actor;
            _articleService = articleService;
            _state = articleStateService;
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
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArticleListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Status = (int)a.Status,
                    UpdatedAt = a.UpdatedAt
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
        public IActionResult CreateArticle([FromBody] CreateArticleRequest createArticleRequest)
        {
            var createArticleDto = createArticleRequest.ArticleDto;
            var isSubmit = createArticleRequest.IsSubmit;
            _articleService.CreateArticle(createArticleDto, _actor, _db, isSubmit, _state);
            return Ok();
        }

 

        [Authorize(Roles = "Admin,Editor,Manager,Author")]
        [HttpGet("{articleId}")]
        public IActionResult GetArticleWithId(string articleId)
        {
            var article = _articleService.GetArticleById(articleId, _db);
            article = new ArticleDto
            {
                Id = article.Id,
                Status = article.Status,
                Title = article.Title,
                UpdatedAt = article.UpdatedAt
            };
            return Ok(article);
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
                .Select(r => new {
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

            var expertReviews = await _db.ArticleExpertAssignments
                .AsNoTracking()
                .Include(x => x.Expert)
                .Where(x => x.ArticleId == id)
                .OrderBy(x => x.Status)
                .ThenByDescending(x => x.ReviewedAt)
                .Select(x => new {
                    type = "expert",
                    expertId = x.ExpertId,
                    expertEmail = x.Expert.Email,     
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
    }
}
