using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Repositories.Article;
using WriteReview.Domain.Dtos;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Repositories.Article
{
    public class ArticleReadRepository : ReadRepository<Domain.Entities.Article>, IArticleReadRepository
    {
        public ArticleReadRepository(WriteReviewDbContext context) : base(context)
        {
        }

        async public Task<ArticleDto> GetArticleWithCategoryAndAuthor(string articleId)
        {
            var article = await Table.Include(a => a.Category).Include(a => a.Author).FirstOrDefaultAsync(a => a.Id.ToString() == articleId);


            var articleDto = new Domain.Dtos.ArticleDto
            {
                Status = (int)article.Status,
                Title = article.Title,
                Summary = article.Summary,
                Content = article.ContentPath,
                Category = article.Category.Name,
                AuthorName = article.Author.FullName,
                Id = article.Id,
                UpdatedAt = article.UpdatedAt
            };

            return articleDto;
        }
    }
}
