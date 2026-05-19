using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using WriteReview.Domain.Entities;
using System.Linq;
using System.Threading.Tasks;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class AppUsersController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public AppUsersController(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userManager.Users.ToListAsync();
            var userList = new System.Collections.Generic.List<object>();

            foreach (var user in users)
            {
                var roles = await _userManager.GetRolesAsync(user);
                userList.Add(new
                {
                    Id = user.Id,
                    UserName = user.UserName,
                    FullName = user.FullName,
                    Email = user.Email,
                    Roles = roles
                });
            }

            return Ok(userList);
        }

        public class CreateUserRequest
        {
            public string FullName { get; set; } = default!;
            public string UserName { get; set; } = default!;
            public string Email { get; set; } = default!;
            public string Password { get; set; } = default!;
        }

        public class UpdateUserRequest
        {
            public string FullName { get; set; } = default!;
            public string Email { get; set; } = default!;
        }

        [HttpPost]
        public async Task<IActionResult> CreateUser([FromBody] CreateUserRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.FullName) || string.IsNullOrWhiteSpace(req.Email) ||
                string.IsNullOrWhiteSpace(req.UserName) || string.IsNullOrWhiteSpace(req.Password))
                return BadRequest(new { Message = "Tüm alanlar zorunludur." });

            var user = new AppUser
            {
                FullName = req.FullName.Trim(),
                UserName = req.UserName.Trim(),
                Email = req.Email.Trim()
            };

            var result = await _userManager.CreateAsync(user, req.Password);
            if (!result.Succeeded)
                return BadRequest(new { Message = "Kullanıcı oluşturulamadı.", Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { id = user.Id, fullName = user.FullName, email = user.Email });
        }

        [HttpPut("{userId}")]
        public async Task<IActionResult> UpdateUser(string userId, [FromBody] UpdateUserRequest req)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            if (!string.IsNullOrWhiteSpace(req.FullName))
                user.FullName = req.FullName.Trim();

            if (!string.IsNullOrWhiteSpace(req.Email))
            {
                user.Email = req.Email.Trim();
                user.UserName = req.Email.Trim();
            }

            var result = await _userManager.UpdateAsync(user);
            if (!result.Succeeded)
                return BadRequest(new { Message = "Güncelleme başarısız.", Errors = result.Errors.Select(e => e.Description) });

            return Ok(new { id = user.Id, fullName = user.FullName, email = user.Email });
        }

        public class AssignRoleRequest { public string RoleName { get; set; } }

        [HttpPost("{userId}/roles")]
        public async Task<IActionResult> AssignRole(string userId, [FromBody] AssignRoleRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.RoleName)) return BadRequest(new { Message = "Rol adı zorunludur." });

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            var roleExists = await _roleManager.RoleExistsAsync(request.RoleName);
            if (!roleExists) return NotFound(new { Message = "Sistemde böyle bir rol bulunamadı." });

            var result = await _userManager.AddToRoleAsync(user, request.RoleName);
            if (result.Succeeded) return Ok(new { Message = "Rol başarıyla atandı." });

            return BadRequest(new { Message = "Rol atanamadı.", Errors = result.Errors });
        }

        [HttpDelete("{userId}/roles/{roleName}")]
        public async Task<IActionResult> RemoveRole(string userId, string roleName)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            if (roleName == "Admin" && await _userManager.GetUsersInRoleAsync("Admin") is var admins && admins.Count == 1 && admins.First().Id == user.Id)
                return BadRequest(new { Message = "Sistemdeki son Admin kullanıcısının rolü silinemez." });

            var result = await _userManager.RemoveFromRoleAsync(user, roleName);
            if (result.Succeeded) return Ok(new { Message = "Rol başarıyla kaldırıldı." });

            return BadRequest(new { Message = "Rol kaldırılamadı.", Errors = result.Errors });
        }

        [HttpDelete("{userId}")]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound(new { Message = "Kullanıcı bulunamadı." });

            var admins = await _userManager.GetUsersInRoleAsync("Admin");
            if (admins.Count == 1 && admins.First().Id.ToString() == userId)
                return BadRequest(new { Message = "Sistemdeki son Admin kullanıcısı silinemez." });

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded) return Ok(new { Message = "Kullanıcı silindi." });

            return BadRequest(new { Message = "Kullanıcı silinemedi.", Errors = result.Errors });
        }
    }
}
