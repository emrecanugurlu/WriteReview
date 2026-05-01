using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Repositories.Article;
using WriteReview.Application.Security;
using WriteReview.Application.Services;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Security;
using WriteReview.Persistence.Services.Articles;

namespace WriteReview.Persistence.Repositories.Article
{
    public class ArticleWriteRepository : WriteRepository<Domain.Entities.Article>, IArticleWriteRepository
    {
        
        public ArticleWriteRepository(WriteReviewDbContext context) : base(context)
        {
        }

        public async Task<string> AddArticleWithAuthor(IActorContextAccessor actor, 
            CreateArticleDto createArticleDto, 
            bool isSubmit, 
            IArticleStateService articleStateService)
        {
            var me = actor.GetCurrent().UserId;

            var article = new Domain.Entities.Article
            {
                Id = Guid.NewGuid(),
                Title = createArticleDto.Title.Trim(),
                Summary = createArticleDto.Summary.Trim(),
                ContentPath = createArticleDto.Content,
                Status = ArticleStatus.Draft,
                CategoryId = Guid.Parse("48e3cf91-ca10-4c78-b0fe-aa17d87d2e3b"),
                Tags = createArticleDto.Tags.Split(',').Select(t => t.Trim()).ToArray(),
                AuthorId = me,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (isSubmit)
            {
                articleStateService.DraftToSubmitted(article);
            }

            try
            {
                await this.AddAsync(article);
                await this.SaveChangesAsync();
                return "Kayıt İşlemi Başarıyla Gerçekleşti...";
            }
            catch
            {
                return "Kayıt İşlemi Sırasında Hata Oluştu";
            }

        }
    }
}
