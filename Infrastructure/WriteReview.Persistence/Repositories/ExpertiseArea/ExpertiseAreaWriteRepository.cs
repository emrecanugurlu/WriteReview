using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Repositories.ExpertiseArea;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Repositories.ExpertiseArea
{
    public class ExpertiseAreaWriteRepository : WriteRepository<Domain.Entities.ExpertiseArea>, IExpertiseAreaWriteRepository
    {
        public ExpertiseAreaWriteRepository(WriteReviewDbContext context) : base(context)
        {
        }

        public void AddExpertiseAreaWithUser()
        {
            throw new NotImplementedException();
        }

        public void AddExpertiseAreaWithUsers()
        {
            throw new NotImplementedException();
        }

        public void UpdateExpertiseAreaWithUser()
        {
            throw new NotImplementedException();
        }
    }
}
