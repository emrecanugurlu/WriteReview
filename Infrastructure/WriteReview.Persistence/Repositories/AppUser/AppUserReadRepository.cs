using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Repositories.AppUser;
using WriteReview.Domain.Dtos.ResponseDto;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Repositories.AppUser
{
    public class AppUserReadRepository : ReadRepository<Domain.Entities.AppUser>, IAppUserReadRepository
    {
        public AppUserReadRepository(WriteReviewDbContext context) : base(context)
        {
        }

        public List<UserResponseDto> GetAllUsers()
        {
            var users = this.GetAll();

            var usersDto = new List<UserResponseDto>();

            foreach (var user in users)
            {
                usersDto.Add(new UserResponseDto
                {
                    Id = user.Id,
                    FullName = user.FullName,
                    UserName = user.UserName,
                    Email = user.Email
                });
            }

            return usersDto;
        }
    }
}
