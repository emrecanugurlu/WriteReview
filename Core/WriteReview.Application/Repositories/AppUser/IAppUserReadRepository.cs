using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Dtos.ResponseDto;

namespace WriteReview.Application.Repositories.AppUser
{
    public interface IAppUserReadRepository: IReadRepository<Domain.Entities.AppUser>
    {
        public List<UserResponseDto> GetAllUsers();
    }
}
