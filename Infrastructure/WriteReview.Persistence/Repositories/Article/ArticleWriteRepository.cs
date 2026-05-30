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

            Guid? categoryId = Guid.TryParse(createArticleDto.CategoryId, out var parsedCategoryId)
                ? parsedCategoryId
                : null;

            var article = new Domain.Entities.Article
            {
                Id = Guid.NewGuid(),
                Title = createArticleDto.Title.Trim(),
                ContentPath = createArticleDto.Content,
                Status = ArticleStatus.Draft,
                CategoryId = categoryId,
                AuthorId = me,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            if (isSubmit)
            {
                articleStateService.DraftToSubmitted(article);
            }

            await this.AddAsync(article);
            await this.SaveChangesAsync();
            return "Kayıt İşlemi Başarıyla Gerçekleşti...";

        }
    }
}
