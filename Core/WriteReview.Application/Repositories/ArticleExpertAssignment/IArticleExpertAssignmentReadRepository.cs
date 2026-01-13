using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;

namespace WriteReview.Application.Repositories.ArticleExpertAssignment
{
    public interface IArticleExpertAssignmentReadRepository : IReadRepository<Domain.Entities.ArticleExpertAssignment>
    {
        public Task<List<ExpertAssignmentDto>> GetMyAssignedArticle(IActorContextAccessor _actor);
    }
}
