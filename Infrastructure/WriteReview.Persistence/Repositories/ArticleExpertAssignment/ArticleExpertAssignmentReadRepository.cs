using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Entity;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Repositories.ArticleExpertAssignment;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Dtos.ResponseDto;
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
            var assignments = await Table.AsNoTracking().Include(x=>x.Article).Where(x=>x.ExpertId==me).ToListAsync();

            var list = assignments.Select(x=> new AssignmentArticleDetailDto
            {
                ArticleId = x.ArticleId,
                ArticleTitle = x.Article.Title,
                Status = (int)x.Status,
                Feedback = x.Feedback,
                Score = x.Score,
                ReviewedAt = x.ReviewedAt
            });

            return (List<ExpertAssignmentDto>)list;

        }
    }
}
