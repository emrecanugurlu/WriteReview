
using Azure;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Net.NetworkInformation;
using WriteReview.Application.Repositories.ArticleExpertAssignment;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Dtos.RequestDto;
using WriteReview.Domain.Dtos.ResponseDto;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Repositories.ArticleExpertAssignment;
using WriteReview.Persistence.Services.Articles;
using WriteReview.Persistence.Services.Expert;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpertsController : ControllerBase
    {
        private readonly IActorContextAccessor _actor;
        private readonly WriteReviewDbContext _db;
        private readonly IArticleExpertAssignmentWriteRepository _articleExpertAssignmentWriteRepository;
        private readonly IArticleExpertAssignmentReadRepository _articleExpertAssignmentReadRepository;
        private readonly ArticleStateService _articleStateService;
        private readonly ArticleService _articleService;
        private readonly ExpertService _expertService;


        public ExpertsController(IActorContextAccessor actor, WriteReviewDbContext db, ExpertService expertService, ArticleStateService articleStateService, ArticleService articleService, IArticleExpertAssignmentWriteRepository articleExpertAssignmentWriteRepository, IArticleExpertAssignmentReadRepository articleExpertAssignmentReadRepository)
        {
            _actor = actor;
            _db = db;
            _articleStateService = articleStateService;
            _articleService = articleService;
            _articleExpertAssignmentWriteRepository = articleExpertAssignmentWriteRepository;
            _articleExpertAssignmentReadRepository = articleExpertAssignmentReadRepository;
            _expertService = expertService;
        }

        [HttpGet("get-assigned-articles")]
        public async Task<IActionResult> GetMyAssignments(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] ExpertAssignmentStatus? status = null
            )
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;

            var me = _actor.GetCurrent().UserId;

            var q = _db.ArticleExpertAssignments
                .AsNoTracking()
                .Where(a => a.ExpertId == me);

            if (status.HasValue)
                q = q.Where(a => a.Status == status.Value);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(a => a.ReviewedAt)
                .Include(a => a.Article)
                .ThenInclude(a => a.Author)
                .Include(a => a.Article.Category)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ExpertAssignmentDto
                {
                    ArticleId = a.ArticleId,
                    ArticleTitle = a.Article.Title ?? "",
                    Status = (int)a.Status,
                    AuthorName = a.Article!.Author.FullName ?? "",
                    ArticleCategory = a.Article!.Category.Name ?? "",
                    ReviewedAt = a.ReviewedAt
                })
                .ToListAsync();

            return Ok(new PagedResult<ExpertAssignmentDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            });
        }

        [HttpGet("get-assigned-article-detail")]
        public async Task<IActionResult> GetMyAssignment([FromQuery] string articleId)
        {
 
            var me = _actor.GetCurrent().UserId;

            var q = _db.ArticleExpertAssignments
                .AsNoTracking()
                .Where(a => a.ExpertId == me);

            var items = await q
                .OrderByDescending(a => a.ReviewedAt)
                .Include(a => a.Article)
                .ThenInclude(a => a.Author)
                .Include(a => a.Article.Category)
                .FirstOrDefaultAsync(a => a.ArticleId == Guid.Parse(articleId));

            return Ok(new AssignmentArticleDetailDto
            {
                ArticleId = items!.ArticleId,
                ArticleCategory = items.Article.Category.Name,
                ArticleContent=items.Article.ContentPath,
                ArticleTitle = items.Article.Title,
                ArticleSummary = items.Article.Summary,
                AuthorName = items.Article.Author.FullName,
                Status = (int)items.Status,
                Feedback = items.Feedback,
                Score = items.Score,
                ReviewedAt = items.ReviewedAt
            });
        }


        /// <summary>
        /// Makaleleri uzmana atamak için kullanılan endpoint. Bir makaleyi bir uzmana atar.
        /// </summary>
        /// <param name="dto"></param>
        /// <returns></returns>
        [HttpPost]
        public async Task<IActionResult> AddAssignmentAsync([FromBody] AddArticleExpertsRequestDto dto)
        {
            var response = await _articleExpertAssignmentWriteRepository.AddArticleExpertsAssignment(dto);
            return Ok(new { message = response });
        }




        [HttpPost("assignments/{id:guid}/feedback")]
        public async Task<IActionResult> SendFeedback(Guid id, [FromBody] ExpertFeedbackDto dto)
        {
            var me = _actor.GetCurrent().UserId;

            var assignment = await _db.ArticleExpertAssignments
                .Include(x => x.Article)
                .FirstOrDefaultAsync(x => x.ExpertId == me);

            if (assignment is null)
                return NotFound();

            assignment.Feedback = dto.Feedback;
            assignment.Score = dto.Score;
            assignment.Status = ExpertAssignmentStatus.Revision;
            assignment.ReviewedAt = DateTime.UtcNow;


            var allDone = await _db.ArticleExpertAssignments
                .Where(x => x.ArticleId == assignment.ArticleId)
                .AllAsync(x => x.Status == ExpertAssignmentStatus.Revision);


            if (allDone)
            {
                assignment.Article.UpdatedAt = DateTime.UtcNow;
            }

            await _db.SaveChangesAsync();

            return Ok(new
            {
                assignment.Status,
                assignment.ReviewedAt,
                allDone
            });
        }

        [HttpGet()]
        public async Task<IActionResult> GetAllExperts()
        {
            var expertsDto = await _expertService.GetAllExpert();
            return Ok(expertsDto);
        }
    }
}
