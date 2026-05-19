
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WriteReview.Domain.Dtos.ResponseDto;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
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

            var experts = await _context.Users
                .Include(u => u.ExpertiseAreas).ThenInclude(u => u.ExpertiseArea)
                .Where(u => userIds.Contains(u.Id))
                .ToListAsync();

            var activeTaskCounts = await _context.ArticleExpertAssignments
                .Where(a => userIds.Contains(a.ExpertId) &&
                            (a.Status == ExpertAssignmentStatus.Pending || a.Status == ExpertAssignmentStatus.Accepted))
                .GroupBy(a => a.ExpertId)
                .Select(g => new { ExpertId = g.Key, Count = g.Count() })
                .ToListAsync();

            var countLookup = activeTaskCounts.ToDictionary(x => x.ExpertId, x => x.Count);

            return experts.Select(e => new ExpertDto
            {
                Id = e.Id,
                Name = e.FullName,
                ExpertiseAreas = e.ExpertiseAreas.Select(x => x.ExpertiseArea.Name).ToList(),
                ActiveTasks = countLookup.TryGetValue(e.Id, out var count) ? count : 0
            }).ToList();
        }
    }
}
