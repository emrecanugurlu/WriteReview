using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Security;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Domain.Security;

namespace WriteReview.Persistence.Articles
{
    public class ArticleStateService
    {
        private readonly IActorContextAccessor _actor;

        public ArticleStateService(IActorContextAccessor actor)
        {
            _actor = actor;
        }

        private static bool HasRole(ActorContext a, string role)
            => a.Roles.Contains(role, StringComparer.OrdinalIgnoreCase);

        public void DraftToSubmitted(Article article)
        {
            var actor = _actor.GetCurrent();

            if (article is null) throw new ArgumentNullException(nameof(article));
            if (article.Status != ArticleStatus.Draft)
                throw new InvalidOperationException("Makale Draft değil.");
            if (!HasRole(actor, "Author"))
                throw new UnauthorizedAccessException("Sadece Author gönderebilir.");
            if (article.AuthorId != actor.UserId)
                throw new UnauthorizedAccessException("Sadece sahibi gönderebilir.");
            if (string.IsNullOrWhiteSpace(article.Title) || string.IsNullOrWhiteSpace(article.ContentPath))
                throw new ArgumentException("Başlık ve içerik zorunludur.");

            article.Status = ArticleStatus.Submitted;
            article.UpdatedAt = DateTime.UtcNow;
        }
    }
}
