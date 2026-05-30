using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Services.Articles;
using Microsoft.AspNetCore.SignalR;
using WriteReview.API.Hubs;

namespace WriteReview.API.Controllers
{
    [Route("api/managers/articles")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class ArticleStatesController : ControllerBase
    {

        private readonly WriteReviewDbContext _db;
        private readonly ArticleStateService _state;
        private readonly IHubContext<NotificationHub> _hubContext;

        public ArticleStatesController(WriteReviewDbContext db, ArticleStateService state, IHubContext<NotificationHub> hubContext)
        {
            _db = db;
            _state = state;
            _hubContext = hubContext;
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
                    
                    var notif = new AppNotification { UserId = expertId, Title = "Yeni Makale Atandı", Message = $"'{article.Title}' başlıklı makaleyi değerlendirmeniz için atandınız." };
                    _db.Notifications.Add(notif);
                    await _hubContext.Clients.Group(expertId.ToString()).SendAsync("ReceiveNotification", notif);
                }
            }
            
            var authorNotif = new AppNotification { UserId = article.AuthorId, Title = "Makale İncelemeye Alındı", Message = $"'{article.Title}' başlıklı makaleniz incelemeye alınmıştır." };
            _db.Notifications.Add(authorNotif);
            await _hubContext.Clients.Group(article.AuthorId.ToString()).SendAsync("ReceiveNotification", authorNotif);
            
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
            
            var notif = new AppNotification { UserId = article.AuthorId, Title = "Makaleniz Onaylandı", Message = $"Tebrikler! '{article.Title}' başlıklı makaleniz onaylandı." };
            _db.Notifications.Add(notif);
            
            await _db.SaveChangesAsync();
            await _hubContext.Clients.Group(article.AuthorId.ToString()).SendAsync("ReceiveNotification", notif);
            return Ok(new { id = article.Id, status = (int)article.Status });
        }

        [HttpPost("{id:guid}/reject")]
        public async Task<IActionResult> Reject(Guid id, [FromBody] RejectionDto dto)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article is null) return NotFound();

            _state.InReviewToRejected(article, dto);

            var notif = new AppNotification { UserId = article.AuthorId, Title = "Makaleniz Reddedildi", Message = $"'{article.Title}' başlıklı makaleniz maalesef reddedildi." };
            _db.Notifications.Add(notif);

            await _db.SaveChangesAsync();
            await _hubContext.Clients.Group(article.AuthorId.ToString()).SendAsync("ReceiveNotification", notif);
            return Ok(new { id = article.Id, status = (int)article.Status });
        }

        [HttpPost("{id:guid}/request-revision")]
        public async Task<IActionResult> RequestRevision(Guid id, [FromBody] ModerationNoteDto dto)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article is null) return NotFound();

            _state.InReviewToRevisionsRequested(article,dto.Note);

            var notif = new AppNotification { UserId = article.AuthorId, Title = "Revizyon İstendi", Message = $"'{article.Title}' başlıklı makaleniz için revizyon istenmektedir." };
            _db.Notifications.Add(notif);

            await _db.SaveChangesAsync();
            await _hubContext.Clients.Group(article.AuthorId.ToString()).SendAsync("ReceiveNotification", notif);
            return Ok(new { id = article.Id, status = (int)article.Status });
        }

        /// <summary>
        /// Manager bir itirazı kabul eder; makale InReview durumuna geçer.
        /// İsteğe bağlı olarak eski veya yeni hakem ID'leri gönderilebilir.
        /// </summary>
        [HttpPost("{id:guid}/appeal-accept")]
        public async Task<IActionResult> AcceptAppeal(Guid id, [FromBody] AcceptAppealRequest? dto)
        {
            var article = await _db.Articles
                .Include(a => a.ExpertAssignments)
                .FirstOrDefaultAsync(a => a.Id == id);
            if (article is null) return NotFound();

            try
            {
                _state.AppealPendingToInReview(article, dto?.Note);

                // İsteğe bağlı hakem ataması (eski veya yeni)
                if (dto?.ExpertIds is { Count: > 0 })
                {
                    var existingExpertIds = article.ExpertAssignments.Select(e => e.ExpertId).ToHashSet();
                    foreach (var expertId in dto.ExpertIds.Distinct())
                    {
                        if (!existingExpertIds.Contains(expertId))
                        {
                            article.ExpertAssignments.Add(new ArticleExpertAssignment
                            {
                                ArticleId = article.Id,
                                ExpertId = expertId,
                                Status = ExpertAssignmentStatus.Pending
                            });
                        }
                    }
                }

                await _db.SaveChangesAsync();
                return Ok(new { id = article.Id, status = (int)article.Status });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }

        /// <summary>
        /// Manager bir itirazı reddeder; makale kesin olarak Rejected durumuna döner.
        /// </summary>
        [HttpPost("{id:guid}/appeal-deny")]
        public async Task<IActionResult> DenyAppeal(Guid id, [FromBody] RejectionDto dto)
        {
            var article = await _db.Articles.FirstOrDefaultAsync(a => a.Id == id);
            if (article is null) return NotFound();

            try
            {
                _state.AppealPendingToRejected(article, dto?.Reason ?? "");
                await _db.SaveChangesAsync();
                return Ok(new { id = article.Id, status = (int)article.Status });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { Message = ex.Message });
            }
        }
    }
}
