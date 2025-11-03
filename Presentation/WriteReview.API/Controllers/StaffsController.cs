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
    public class StaffsController : ControllerBase
    {
        private readonly WriteReviewDbContext _db;
        public StaffsController(WriteReviewDbContext db) => _db = db;

        [HttpGet("articles")]
        public async Task<IActionResult> ListArticle(
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
                q = q.Where(a => a.Status == ArticleStatus.Submitted || a.Status == ArticleStatus.InReview);

            var total = await q.CountAsync();
            var items = await q
                .OrderBy(a => a.Status)
                .ThenByDescending(a => a.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new StaffArticleListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    AuthorId = a.AuthorId,
                    Status = (int)a.Status,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(new PagedResult<StaffArticleListItemDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            });
        }
    }
}

