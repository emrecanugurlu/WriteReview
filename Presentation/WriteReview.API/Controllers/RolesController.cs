using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WriteReview.Domain.Dtos.RequestDto;
using WriteReview.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;
using System;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class RolesController : ControllerBase
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly UserManager<AppUser> _userManager;

        public RolesController(RoleManager<AppRole> roleManager, UserManager<AppUser> userManager)
        {
            _roleManager = roleManager;
            _userManager = userManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllRoles()
        {
            var roles = await _roleManager.Roles
                .Select(r => new { Id = r.Id, Name = r.Name })
                .ToListAsync();
            return Ok(roles);
        }

        [HttpGet("withusers")]
        public async Task<IActionResult> GetAllRolesWithUsers()
        {
            var roles = await _roleManager.Roles.ToListAsync();
            var result = new List<object>();

            foreach (var role in roles)
            {
                var users = await _userManager.GetUsersInRoleAsync(role.Name!);
                result.Add(new
                {
                    Id = role.Id,
                    Name = role.Name,
                    Users = users.Select(u => new { Id = u.Id, Name = u.FullName })
                });
            }

            return Ok(result);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetRoleDetail(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null) return NotFound(new { Message = "Rol bulunamadı." });

            var usersInRole = await _userManager.GetUsersInRoleAsync(role.Name);

            return Ok(new
            {
                Id = role.Id,
                Name = role.Name,
                UserCount = usersInRole.Count
            });
        }

        [HttpPost]
        public async Task<IActionResult> CreateRole([FromBody] CreateRoleRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Name))
                return BadRequest(new { Message = "Rol adı boş olamaz." });

            var roleExists = await _roleManager.RoleExistsAsync(request.Name);
            if (roleExists)
                return BadRequest(new { Message = "Bu rol zaten mevcut." });

            var role = new AppRole { Name = request.Name, Id = Guid.NewGuid() };
            var result = await _roleManager.CreateAsync(role);

            if (result.Succeeded)
                return Ok(new { Message = "Rol başarıyla oluşturuldu.", Role = new { Id = role.Id, Name = role.Name } });

            return BadRequest(new { Message = "Rol oluşturulamadı.", Errors = result.Errors });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRole(Guid id)
        {
            var role = await _roleManager.FindByIdAsync(id.ToString());
            if (role == null)
                return NotFound(new { Message = "Rol bulunamadı." });

            if (role.Name == "Admin" || role.Name == "Author" || role.Name == "Manager" || role.Name == "Expert")
                return BadRequest(new { Message = "Sistem standart rolleri silinemez." });

            var result = await _roleManager.DeleteAsync(role);
            if (result.Succeeded)
                return Ok(new { Message = "Rol başarıyla silindi." });

            return BadRequest(new { Message = "Rol silinemedi.", Errors = result.Errors });
        }
    }
}
