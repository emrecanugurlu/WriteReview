using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.EntityFrameworkCore;
using WriteReview.Application.Repositories.ExpertiseArea;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Dtos.ResponseDto;
using WriteReview.Domain.Entities;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Services.ExpertiseArea;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpertiseAreasController : ControllerBase
    {
        private readonly ExpertiseAreaService _expertiseAreaService;
        private readonly WriteReviewDbContext _db;
        private readonly IExpertiseAreaWriteRepository _expertiseAreaWriteRepository;
        private readonly IExpertiseAreaReadRepository _expertiseAreaReadRepository;
        private readonly IActorContextAccessor _actor;
        private readonly UserManager<AppUser> _userManager;

        public ExpertiseAreasController(ExpertiseAreaService expertiseAreaService, WriteReviewDbContext db, IExpertiseAreaWriteRepository expertiseAreaWriteRepository, IExpertiseAreaReadRepository expertiseAreaReadRepository, IActorContextAccessor actor, UserManager<AppUser> userManager)
        {
            _expertiseAreaService = expertiseAreaService;
            _db = db;
            _expertiseAreaWriteRepository = expertiseAreaWriteRepository;
            _expertiseAreaReadRepository = expertiseAreaReadRepository;
            _actor = actor;
            _userManager = userManager;
        }

        [Authorize]
        [HttpGet("withoutusers")]
        public IActionResult GetAllExpertiseAreasWithoutUsers()
        {
            var expertiseAreas = _expertiseAreaReadRepository.GetAll()
                .Select(x => new ExpertiseAreaWithoutUsers { Id = x.Id, Name = x.Name })
                .ToList();
            return Ok(expertiseAreas);
        }

        [Authorize]
        [HttpGet("withusers")]
        public IActionResult GetAllExpertiseAreasWithUsers()
        {
            var expertiseAreas = _expertiseAreaService.GetAllExpertiseAreaWithUsers(_db);
            return Ok(expertiseAreas);
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<IActionResult> AddExpertiseArea([FromBody] AddExpertiseAreaDto dto)
        {
            bool isAdded = await _expertiseAreaWriteRepository.AddAsync(new Domain.Entities.ExpertiseArea()
            {
                Id = Guid.NewGuid(),
                Name = dto.Name.Trim()
            });
            await _expertiseAreaWriteRepository.SaveChangesAsync();
            return Ok(isAdded);
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateExpertiseArea(Guid id, [FromBody] AddExpertiseAreaDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.Name))
                return BadRequest(new { Message = "Alan adı boş olamaz." });

            var area = await _db.ExpertiseAreas.FirstOrDefaultAsync(a => a.Id == id);
            if (area is null)
                return NotFound(new { Message = "Uzmanlık alanı bulunamadı." });

            area.Name = dto.Name.Trim();
            await _db.SaveChangesAsync();

            return Ok(new { id = area.Id, name = area.Name });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMyExpertiseAreas()
        {
            var userId = _actor.GetCurrent().UserId;
            var areas = await _db.UserExpertiseAreas
                .Where(ue => ue.UserId == userId)
                .Select(ue => new { id = ue.ExpertiseAreaId, name = ue.ExpertiseArea.Name })
                .ToListAsync();
            return Ok(areas);
        }

        [Authorize]
        [HttpPost("me/{areaId:guid}")]
        public async Task<IActionResult> AddMyExpertiseArea(Guid areaId)
        {
            var userId = _actor.GetCurrent().UserId;
            var exists = await _db.UserExpertiseAreas.AnyAsync(ue => ue.UserId == userId && ue.ExpertiseAreaId == areaId);
            if (exists) return Conflict(new { Message = "Bu uzmanlık alanı zaten eklenmiş." });

            var areaExists = await _db.ExpertiseAreas.AnyAsync(a => a.Id == areaId);
            if (!areaExists) return NotFound(new { Message = "Uzmanlık alanı bulunamadı." });

            _db.UserExpertiseAreas.Add(new UserExpertiseArea { UserId = userId, ExpertiseAreaId = areaId });
            await _db.SaveChangesAsync();
            return Ok();
        }

        [Authorize]
        [HttpDelete("me/{areaId:guid}")]
        public async Task<IActionResult> RemoveMyExpertiseArea(Guid areaId)
        {
            var userId = _actor.GetCurrent().UserId;
            var entry = await _db.UserExpertiseAreas.FirstOrDefaultAsync(ue => ue.UserId == userId && ue.ExpertiseAreaId == areaId);
            if (entry is null) return NotFound(new { Message = "Bu uzmanlık alanı mevcut değil." });

            _db.UserExpertiseAreas.Remove(entry);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("{areaId:guid}/users/{userId:guid}")]
        public async Task<IActionResult> AssignUserToArea(Guid areaId, Guid userId)
        {
            var areaExists = await _db.ExpertiseAreas.AnyAsync(a => a.Id == areaId);
            if (!areaExists) return NotFound(new { Message = "Uzmanlık alanı bulunamadı." });

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user is null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            var isExpert = await _userManager.IsInRoleAsync(user, "Expert");
            if (!isExpert) return BadRequest(new { Message = "Uzmanlık alanı yalnızca Expert rolündeki kullanıcılara atanabilir." });

            var already = await _db.UserExpertiseAreas.AnyAsync(ue => ue.UserId == userId && ue.ExpertiseAreaId == areaId);
            if (already) return Conflict(new { Message = "Bu atama zaten mevcut." });

            _db.UserExpertiseAreas.Add(new UserExpertiseArea { UserId = userId, ExpertiseAreaId = areaId });
            await _db.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{areaId:guid}/users/{userId:guid}")]
        public async Task<IActionResult> RemoveUserFromArea(Guid areaId, Guid userId)
        {
            var entry = await _db.UserExpertiseAreas
                .FirstOrDefaultAsync(ue => ue.UserId == userId && ue.ExpertiseAreaId == areaId);
            if (entry is null) return NotFound(new { Message = "Bu atama mevcut değil." });

            _db.UserExpertiseAreas.Remove(entry);
            await _db.SaveChangesAsync();
            return Ok();
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> DeleteExpertiseArea(Guid id)
        {
            var area = await _db.ExpertiseAreas
                .Include(a => a.Users)
                .FirstOrDefaultAsync(a => a.Id == id);

            if (area is null)
                return NotFound(new { Message = "Uzmanlık alanı bulunamadı." });

            if (area.Users.Count > 0)
                return BadRequest(new { Message = "Bu alana atanmış uzmanlar olduğu için silinemez." });

            _db.ExpertiseAreas.Remove(area);
            await _db.SaveChangesAsync();

            return Ok(new { Message = "Uzmanlık alanı silindi." });
        }

    }
}
