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

            var sixMonthsAgo = DateTime.UtcNow.AddMonths(-6);
            var monthlySubmissionsRaw = await _db.Articles
                .Where(a => a.CreatedAt >= sixMonthsAgo)
                .GroupBy(a => new { a.CreatedAt.Year, a.CreatedAt.Month })
                .Select(g => new { Year = g.Key.Year, Month = g.Key.Month, Count = g.Count() })
                .ToListAsync();

            var monthlySubmissions = monthlySubmissionsRaw
                .Select(x => new { Month = new DateTime(x.Year, x.Month, 1).ToString("MMM yyyy"), Count = x.Count })
                .ToList();

            var categoryDistribution = await _db.Articles
                .Include(a => a.Category)
                .Where(a => a.Category != null)
                .GroupBy(a => a.Category.Name)
                .Select(g => new { CategoryName = g.Key, Count = g.Count() })
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
                recentArticles,
                monthlySubmissions,
                categoryDistribution
            });
        }
    }
}
