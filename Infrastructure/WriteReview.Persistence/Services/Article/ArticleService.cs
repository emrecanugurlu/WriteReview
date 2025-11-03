using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Services.Articles
{
    public class ArticleService
    {

        public void CreateArticle(CreateArticleDto createDraftDto, IActorContextAccessor actor, WriteReviewDbContext writeReviewDbContext, bool isSubmit, ArticleStateService articleStateService)
        {
            var me = actor.GetCurrent().UserId;

            var entity = new Article
            {
                Id = Guid.NewGuid(),
                Title = createDraftDto.Title.Trim(),
                Summary = createDraftDto.Summary.Trim(),
                ContentPath = createDraftDto.Content,
                Status = ArticleStatus.Draft,
                AuthorId = actor.GetCurrent().UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (isSubmit)
            {
                articleStateService.DraftToSubmitted(entity);
            }

            writeReviewDbContext.Articles.Add(entity);
            writeReviewDbContext.SaveChanges();

        }

        public ArticleDto GetArticleById(string articleId, WriteReviewDbContext writeReviewDbContext)
        {
            var article = writeReviewDbContext.Articles.Find(Guid.Parse(articleId));

            if (article == null)
            {
                throw new Exception("Article not found or access denied.");
            }

            var articleDto = new ArticleDto
            {
                Id = article.Id,
                Title = article.Title,
                Status = (int)article.Status,
                UpdatedAt = article.UpdatedAt
            };

            return articleDto;

        }

    }
}
