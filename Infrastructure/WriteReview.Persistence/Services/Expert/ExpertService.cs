
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Domain.Dtos.RequestDto;
using WriteReview.Domain.Dtos.ResponseDto;
using WriteReview.Domain.Entities;
using WriteReview.Persistence.Contexts;

namespace WriteReview.Persistence.Services.Expert
{
    public class ExpertService
    {

       private readonly UserManager<AppUser> _userManager;
        private readonly WriteReviewDbContext _context;

        public ExpertService(UserManager<AppUser> userManager, WriteReviewDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<List<ExpertDto>> GetAllExpert()
        {

            var expertsInRole = await _userManager.GetUsersInRoleAsync("Expert");
            var userIds = expertsInRole.Select(u => u.Id).ToList();

            var experts = await _context.Users.Include(u => u.ExpertiseAreas).ThenInclude(u => u.ExpertiseArea)
                                            .Where(u => userIds.Contains(u.Id))
                                            .ToListAsync();


            var expertsDto = experts.Select(e=> new ExpertDto
            {
                Id = e.Id,
                Name = e.FullName,
                ExpertiseAreas = e.ExpertiseAreas.Select(x => x.ExpertiseArea.Name).ToList(),
                ActiveTasks = 0 
            }).ToList();

            return expertsDto;
        }
    }
}
