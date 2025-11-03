using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Dtos.RequestDto;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Services.Expert;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpertsController : ControllerBase
    {
        private readonly IActorContextAccessor _actor;
        private readonly WriteReviewDbContext _db;
        private readonly ExpertService _expertService;
        public ExpertsController(IActorContextAccessor actor, WriteReviewDbContext db, ExpertService expertService)
        {
            _actor = actor;
            _db = db;
            _expertService = expertService;
        }

        [HttpGet("assignments")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var me = _actor.GetCurrent().UserId;

            var assignments = await _db.ArticleExpertAssignments
                         .AsNoTracking()
                         .Include(x => x.Article)
                         .Where(x => x.ExpertId == me)
                         .OrderBy(x => x.Status)
                         .ThenByDescending(x => x.Article.UpdatedAt)
                         .ToListAsync();

            var list = assignments
                      .Select(x => new ExpertAssignmentDto
                      {
                          ArticleId = x.ArticleId,
                          ArticleTitle = x.Article?.Title ?? "",
                          ArticleStatus = x.Article != null ? (int)x.Article.Status : 0,
                          Status = (int)x.Status,
                          Feedback = x.Feedback,
                          Score = x.Score,
                          ReviewedAt = x.ReviewedAt
                      })
                      .ToList();
            return Ok(list);
        }


        [HttpPost]
        public async Task<IActionResult> AddAssignmentAsync([FromBody] AddArticleExpertRequestDto dto)
        {
            await _expertService.AddAssignmentAsync(_db,dto);
            return Ok("Başarılı");
        }



        [HttpPost("assignments/{id:guid}/feedback")]
        public async Task<IActionResult> SendFeedback(Guid id, [FromBody] ExpertFeedbackDto dto)
        {
            var me = _actor.GetCurrent().UserId;

            var assignment = await _db.ArticleExpertAssignments
                .Include(x => x.Article)
                .FirstOrDefaultAsync(x=>x.ExpertId == me);

            if (assignment is null)
                return NotFound(); 

            assignment.Feedback = dto.Feedback;
            assignment.Score = dto.Score;
            assignment.Status = ExpertAssignmentStatus.Completed;
            assignment.ReviewedAt = DateTime.UtcNow;

            
            var allDone = await _db.ArticleExpertAssignments
                .Where(x => x.ArticleId == assignment.ArticleId)
                .AllAsync(x => x.Status == ExpertAssignmentStatus.Completed);

            
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
    }
}
