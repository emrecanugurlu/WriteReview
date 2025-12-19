using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Dtos;

namespace WriteReview.Application.Repositories.Article
{
    public interface IArticleReadRepository : IReadRepository<Domain.Entities.Article>
    {

        Task<ArticleDto> GetArticleWithCategoryAndAuthor(string articleId);


    }
}
