using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace WriteReview.Application.Repositories.ExpertiseArea
{
    public interface IExpertiseAreaWriteRepository: IWriteRepository<Domain.Entities.ExpertiseArea>
    {
        public void AddExpertiseAreaWithUser();
        public void AddExpertiseAreaWithUsers();
       
        public void UpdateExpertiseAreaWithUser();
    }
}
