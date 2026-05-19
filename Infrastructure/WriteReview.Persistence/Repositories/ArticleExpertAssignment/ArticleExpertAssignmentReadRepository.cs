using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WriteReview.Application.Repositories.ArticleExpertAssignment;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Repositories.ArticleExpertAssignment
{
    public class ArticleExpertAssignmentReadRepository : ReadRepository<Domain.Entities.ArticleExpertAssignment>, IArticleExpertAssignmentReadRepository
    {
        public ArticleExpertAssignmentReadRepository(WriteReviewDbContext context) : base(context)
        {
        }

        public async Task<List<ExpertAssignmentDto>> GetMyAssignedArticle(IActorContextAccessor _actor)
        {
            var me = _actor.GetCurrent().UserId;

            return await Table
                .AsNoTracking()
                .Include(x => x.Article)
                .Where(x => x.ExpertId == me)
                .Select(x => new ExpertAssignmentDto
                {
                    ArticleId = x.ArticleId,
                    ArticleTitle = x.Article.Title,
                    Status = (int)x.Status,
                    ReviewedAt = x.ReviewedAt
                })
                .ToListAsync();
        }
    }
}
