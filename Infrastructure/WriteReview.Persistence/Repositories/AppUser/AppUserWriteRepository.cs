using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Repositories.AppUser;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Repositories.AppUser
{
    public class AppUserWriteRepository : WriteRepository<Domain.Entities.AppUser>, IAppUserWriteRepository
    {
        public AppUserWriteRepository(WriteReviewDbContext context) : base(context)
        {
        }
    }
}
