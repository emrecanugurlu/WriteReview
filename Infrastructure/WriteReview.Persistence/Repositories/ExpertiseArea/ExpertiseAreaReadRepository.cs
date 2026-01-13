using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Repositories.ExpertiseArea;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Repositories.ExpertiseArea
{
    public class ExpertiseAreaReadRepository : ReadRepository<Domain.Entities.ExpertiseArea>, IExpertiseAreaReadRepository
    {
        public ExpertiseAreaReadRepository(WriteReviewDbContext context) : base(context)
        {
        }
    }
}
