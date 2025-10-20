using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Numerics;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Domain.Security;
using WriteReview.Persistence.Contexts;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArticlesController : ControllerBase
    {
        private readonly WriteReviewDbContext _db;
        private readonly IActorContextAccessor _actor;

        public ArticlesController(WriteReviewDbContext db, IActorContextAccessor actor)
        {
            _db = db;
            _actor = actor;
        }

        [Authorize(Roles = Roles.Author)]
        [HttpGet("mine")]
        public async Task<IActionResult> Mine(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10,
            [FromQuery] ArticleStatus? status = null
            )
        {
            if (page < 1) page = 1;
            if (pageSize is < 1 or > 50) pageSize = 10;
            var me = _actor.GetCurrent().UserId;

            var q = _db.Articles
                .AsNoTracking()
                .Where(a => a.AuthorId == me);

            if (status.HasValue)
                q = q.Where(a => a.Status == status.Value);

            var total = await q.CountAsync();

            var items = await q
                .OrderByDescending(a => a.UpdatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(a => new ArticleListItemDto
                {
                    Id = a.Id,
                    Title = a.Title,
                    Status = (int)a.Status,
                    UpdatedAt = a.UpdatedAt
                })
                .ToListAsync();

            return Ok(new PagedResult<ArticleListItemDto>
            {
                Page = page,
                PageSize = pageSize,
                Total = total,
                Items = items
            });

        }

        [Authorize(Roles = Roles.Author)]
        [HttpPost]
        public IActionResult CreateDraft( [FromBody] CreateDraftDto createDraftDto)
        {
            var articleService = new WriteReview.Persistence.Articles.ArticleService();
            articleService.CreateDraft(createDraftDto, _actor, _db);
            return Ok();
        }

    }
}
