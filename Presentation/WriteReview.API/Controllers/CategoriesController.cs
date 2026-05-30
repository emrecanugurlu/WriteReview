using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities;
using WriteReview.Persistence.Contexts;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CategoriesController : ControllerBase
    {
        private readonly WriteReviewDbContext _db;

        public CategoriesController(WriteReviewDbContext db)
        {
            _db = db;
        }

        [Authorize]
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var categories = await _db.Categories
                .AsNoTracking()
                .OrderBy(c => c.Name)
                .Select(c => new { id = c.Id, name = c.Name })
                .ToListAsync();

            return Ok(categories);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> Create([FromBody] CreateCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { Message = "Yayın adı boş olamaz." });

            var category = new Category
            {
                Id = Guid.NewGuid(),
                Name = request.Name.Trim()
            };

            _db.Categories.Add(category);
            await _db.SaveChangesAsync();

            return Ok(new { id = category.Id, name = category.Name });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateCategoryRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { Message = "Yayın adı boş olamaz." });

            var category = await _db.Categories.FindAsync(id);
            if (category == null)
                return NotFound(new { Message = "Yayın bulunamadı." });

            category.Name = request.Name.Trim();
            await _db.SaveChangesAsync();

            return Ok(new { id = category.Id, name = category.Name });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var category = await _db.Categories.Include(c => c.Articles).FirstOrDefaultAsync(c => c.Id == id);
            if (category == null)
                return NotFound(new { Message = "Yayın bulunamadı." });

            if (category.Articles.Any())
                return BadRequest(new { Message = "Bu yayına ait makaleler bulunduğu için yayın silinemez." });

            _db.Categories.Remove(category);
            await _db.SaveChangesAsync();

            return Ok();
        }
    }
}
