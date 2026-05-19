using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Security;
using WriteReview.Application.Services;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Domain.Security;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Services.Articles
{
    public class ArticleStateService : IArticleStateService
    {
        private readonly IActorContextAccessor _actor;
        private readonly WriteReviewDbContext _writeReviewDbContext;

        public ArticleStateService(IActorContextAccessor actor, WriteReviewDbContext writeReviewDbContext)
        {
            _actor = actor;
            _writeReviewDbContext = writeReviewDbContext;
        }

        private static bool HasRole(ActorContext a, string role)
            => a.Roles.Contains(role, StringComparer.OrdinalIgnoreCase);
        private static bool HasAnyRole(ActorContext a, params string[] roles) 
            => roles.Any(r => a.Roles.Contains(r, StringComparer.OrdinalIgnoreCase));

        private void AddReview(Article article, ReviewAction action, Guid reviewerId,
                           ArticleStatus from, ArticleStatus to,
                           string? note = null, string? reason = null)
        {
            _writeReviewDbContext.ArticleReviews.Add(new ArticleReview
            {
                ArticleId = article.Id,
                ReviewerId = reviewerId,
                Action = action,
                Note = note,
                Reason = reason,
                FromStatus = from,
                ToStatus = to,
                CreatedAt = DateTime.UtcNow
            });
        }

        public void DraftToSubmitted(Article article)
        {
            var actor = _actor.GetCurrent();
            if (article is null) throw new ArgumentNullException(nameof(article));
            if (article.Status != ArticleStatus.Draft) throw new InvalidOperationException("Makale Draft değil.");
            if (!HasAnyRole(actor, "Author")) throw new UnauthorizedAccessException("Sadece Author gönderebilir.");
            if (article.AuthorId != actor.UserId) throw new UnauthorizedAccessException("Sadece sahibi gönderebilir.");

            if (string.IsNullOrWhiteSpace(article.Title) || string.IsNullOrWhiteSpace(article.ContentPath))
                throw new ArgumentException("Başlık ve içerik zorunludur.");

            var from = article.Status;
            article.Status = ArticleStatus.Submitted;
            article.UpdatedAt = DateTime.UtcNow;
            AddReview(article, ReviewAction.TakeToReview, actor.UserId, from, ArticleStatus.Submitted, note: "Author submitted");
        }

        public void SubmittedToInReview(Article article, string? note = null)
        {
            var actor = _actor.GetCurrent();
            if (article is null) throw new ArgumentNullException(nameof(article));
            if (article.Status != ArticleStatus.Submitted)
                throw new InvalidOperationException("Sadece Submitted durumundaki makaleler incelemeye alınabilir.");
            if (!HasAnyRole(actor, "Admin", "Editor", "Manager"))
                throw new UnauthorizedAccessException("Bu işlem için staff rolü gerekir.");

            var from = article.Status;
            article.Status = ArticleStatus.InReview;
            article.UpdatedAt = DateTime.UtcNow;

            AddReview(article, ReviewAction.TakeToReview, actor.UserId, from, ArticleStatus.InReview, note);
        }

        public void InReviewToApproved(Article article, string? note = null)
        {
            var actor = _actor.GetCurrent();
            if (article is null) throw new ArgumentNullException(nameof(article));
            if (article.Status != ArticleStatus.InReview)
                throw new InvalidOperationException("Sadece InReview durumundaki makaleler onaylanabilir.");
            if (!HasAnyRole(actor, "Admin", "Editor", "Manager"))
                throw new UnauthorizedAccessException("Bu işlem için staff rolü gerekir.");

            var from = article.Status;
            article.Status = ArticleStatus.Approved;
            article.UpdatedAt = DateTime.UtcNow;

            AddReview(article, ReviewAction.Approve, actor.UserId, from, ArticleStatus.Approved, note);
        }

        public void InReviewToRejected(Article article, RejectionDto dto)
        {
            var actor = _actor.GetCurrent();
            if (article is null) throw new ArgumentNullException(nameof(article));
            if (!HasAnyRole(actor, "Admin", "Editor", "Manager"))
                throw new UnauthorizedAccessException("Bu işlem için staff rolü gerekir.");
            if (string.IsNullOrWhiteSpace(dto.Reason))
                throw new ArgumentException("Red nedeni boş olamaz.", nameof(dto.Reason));

            var from = article.Status;
            article.Status = ArticleStatus.Rejected;
            article.UpdatedAt = DateTime.UtcNow;

            AddReview(article, ReviewAction.Reject, actor.UserId, from, ArticleStatus.Rejected, reason: dto.Reason.Trim());
        }

        public void InReviewToRevisionsRequested(Article article, string note)
        {
            var actor = _actor.GetCurrent();
            if (article is null) throw new ArgumentNullException(nameof(article));
            if (article.Status != ArticleStatus.InReview)
                throw new InvalidOperationException("Sadece InReview durumundaki makaleler için düzeltme istenebilir.");
            if (!HasAnyRole(actor, "Admin", "Editor", "Manager"))
                throw new UnauthorizedAccessException("Bu işlem için staff rolü gerekir.");
            if (string.IsNullOrWhiteSpace(note))
                throw new ArgumentException("Düzeltme notu boş olamaz.", nameof(note));

            var from = article.Status;
            article.Status = ArticleStatus.RevisionsRequested;
            article.UpdatedAt = DateTime.UtcNow;

            AddReview(article, ReviewAction.RequestRevision, actor.UserId, from, ArticleStatus.RevisionsRequested, note: note.Trim());
        }

        public void RevisionsRequestedToSubmitted(Article article)
        {
            var actor = _actor.GetCurrent();
            if (article is null) throw new ArgumentNullException(nameof(article));
            if (article.Status != ArticleStatus.RevisionsRequested)
                throw new InvalidOperationException("Sadece RevisionsRequested durumundaki makaleler yeniden gönderilebilir.");
            if (!HasAnyRole(actor, "Author")) throw new UnauthorizedAccessException("Sadece Author gönderebilir.");
            if (article.AuthorId != actor.UserId) throw new UnauthorizedAccessException("Sadece sahibi gönderebilir.");

            var from = article.Status;
            article.Status = ArticleStatus.Submitted;
            article.UpdatedAt = DateTime.UtcNow;

            AddReview(article, ReviewAction.TakeToReview, actor.UserId, from, ArticleStatus.Submitted, note: "Author resubmitted after revision");
        }
    }
}
