using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Services.Articles;

namespace WriteReview.API.Controllers
{
    [Route("api/staff/articles")]
    [ApiController]
    public class ArticleStatesController : ControllerBase
    {

        private readonly WriteReviewDbContext _db;
        private readonly ArticleStateService _state;

        public ArticleStatesController(WriteReviewDbContext db, ArticleStateService state)
        {
            _db = db;
            _state = state;
        }

        [HttpPost("{id:guid}/review")]
        public async Task<IActionResult> TakeToReview(Guid id, [FromBody] TakeToReviewRequest? dto)
        {
            var article = await _db.Articles.Include(a => a.ExpertAssignments).FirstOrDefaultAsync(a => a.Id == id);
            if (article is null) return NotFound();

            _state.SubmittedToInReview(article,dto?.Note);

            if(dto?.ExpertIds is not null && dto.ExpertIds.Count > 0)
            {
                foreach (var expertId in dto.ExpertIds.Distinct())
                {
                    article.ExpertAssignments.Add(new ArticleExpertAssignment
                    {
                        ArticleId = article.Id,
                        ExpertId = expertId,
                        Status = ExpertAssignmentStatus.Pending
                    });
                }
            }
            await _db.SaveChangesAsync();
            return Ok(
                new
                {
                    id = article.Id,
                    status = (int)article.Status,
                    assignedExperts = article.ExpertAssignments.Select(e => new { e.ExpertId, e.Status }).ToList()
                });
        }

        [HttpPost("{id:guid}/approve")]
        public async Task<IActionResult> Approve(Guid id, ModerationNoteDto? dto)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article is null) return NotFound();

            _state.InReviewToApproved(article);
            await _db.SaveChangesAsync();
            return Ok(new { id = article.Id, status = (int)article.Status });
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectionDto dto)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article is null) return NotFound();

            _state.InReviewToRejected(article, dto);
            await _db.SaveChangesAsync();
            return Ok(new { id = article.Id, status = (int)article.Status });
        }

        [HttpPost("{id:guid}/request-revision")]
        public async Task<IActionResult> RequestRevision(Guid id, [FromBody] ModerationNoteDto dto)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article is null) return NotFound();

            _state.InReviewToRevisionsRequested(article,dto.Note);
            await _db.SaveChangesAsync();
            return Ok(new { id = article.Id, status = (int)article.Status });
        }
    }
}
