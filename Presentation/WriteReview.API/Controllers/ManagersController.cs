using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System;
using WriteReview.Domain.Dtos.Staff;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin,Manager")]
    public class ManagersController : ControllerBase
    {
        private readonly WriteReviewDbContext _db;
        public ManagersController(WriteReviewDbContext db) => _db = db;

        [HttpGet("articles")]
        public async Task<IActionResult> ListArticleForManager(
            [FromQuery] ArticleStatus? status = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;

            var q = _db.Articles.AsNoTracking();

            if (status.HasValue)
                q = q.Where(a => a.Status == status.Value);
            else
                q = q.Where(a => a.Status != ArticleStatus.Draft);

            var total = await q.CountAsync();
            var items = await q
                .Include(a => a.Author)
                .Include(a => a.Category)
                .Include(a => a.ExpertAssignments)
                .OrderBy(a => a.Status)
                .ThenByDescending(a => a.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ManagerArticleListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    AuthorId = a.AuthorId,
                    AuthorName = a.Author.FullName,
                    Status = (int)a.Status,
                    Experts = a.ExpertAssignments
                        .Select(ea => ea.Expert.FullName)
                        .ToList(),
                    Category = a.Category.Name,
                    CreatedAt = a.CreatedAt,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(new PagedResult<ManagerArticleListItemDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            });
        }
    }
}

