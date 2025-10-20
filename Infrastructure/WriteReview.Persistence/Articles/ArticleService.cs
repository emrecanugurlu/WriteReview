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

namespace WriteReview.Persistence.Articles
{
    public class ArticleService
    {                                 
        public void CreateDraft(CreateDraftDto createDraftDto, IActorContextAccessor actor, WriteReviewDbContext writeReviewDbContext)
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


            writeReviewDbContext.Articles.Add(entity);
            writeReviewDbContext.SaveChanges();

        }

        
    }
}
