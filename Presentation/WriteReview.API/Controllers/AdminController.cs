using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;

namespace WriteReview.API.Controllers
{
    [Route("api/admin")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly WriteReviewDbContext _db;
        private readonly UserManager<AppUser> _userManager;

        public AdminController(WriteReviewDbContext db, UserManager<AppUser> userManager)
        {
            _db = db;
            _userManager = userManager;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var userCount = await _userManager.Users.CountAsync();
            var expertiseAreaCount = await _db.ExpertiseAreas.CountAsync();

            var articleCounts = await _db.Articles
                .GroupBy(a => a.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync();

            int CountByStatus(ArticleStatus s) =>
                articleCounts.FirstOrDefault(x => x.Status == s)?.Count ?? 0;

            var recentArticles = await _db.Articles
                .AsNoTracking()
                .Include(a => a.Author)
                .Where(a => a.Status != ArticleStatus.Draft)
                .OrderByDescending(a => a.UpdatedAt)
                .Take(5)
                .Select(a => new
                {
                    id = a.Id,
                    title = a.Title,
                    authorName = a.Author.FullName,
                    status = (int)a.Status,
                    updatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(new
            {
                userCount,
                expertiseAreaCount,
                submitted = CountByStatus(ArticleStatus.Submitted),
                inReview = CountByStatus(ArticleStatus.InReview),
                approved = CountByStatus(ArticleStatus.Approved),
                rejected = CountByStatus(ArticleStatus.Rejected),
                revisionsRequested = CountByStatus(ArticleStatus.RevisionsRequested),
                recentArticles
            });
        }
    }
}
