using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using WriteReview.Application.Security;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Entities;
using WriteReview.Domain.Entities.EnumClass;
using WriteReview.Persistence.Contexts;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly IConfiguration _configuration;
        private readonly WriteReviewDbContext _db;
        private readonly IActorContextAccessor _actor;

        public AuthController(UserManager<AppUser> userManager, SignInManager<AppUser> signInManager,
            IConfiguration configuration, WriteReviewDbContext db, IActorContextAccessor actor)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _configuration = configuration;
            _db = db;
            _actor = actor;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto model)
        {
            var user = await _userManager.FindByEmailAsync(model.email);
            if (user == null)
                return Unauthorized("Kullanıcı bulunamadı.");

            var check = await _signInManager.CheckPasswordSignInAsync(user, model.password, false);
            if (!check.Succeeded)
                return Unauthorized("Geçersiz e-posta veya şifre.");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email!) }
                .Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)))
                .ToArray();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"]));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            Response.Cookies.Append("jwt", new JwtSecurityTokenHandler().WriteToken(token), new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddHours(12)
            });

            return Ok(new
            {
                message = "Giriş başarılı",
                role = roles,
                token = new JwtSecurityTokenHandler().WriteToken(token)
            });
        }

        [Authorize]
        [HttpGet("me")]
        public async Task<IActionResult> GetMe()
        {
            var me = _actor.GetCurrent().UserId;
            var user = await _userManager.FindByIdAsync(me.ToString());
            if (user is null) return NotFound();

            var roles = await _userManager.GetRolesAsync(user);

            var articleStats = await _db.Articles
                .Where(a => a.AuthorId == me)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    total = g.Count(),
                    approved = g.Count(a => a.Status == ArticleStatus.Approved),
                    pending = g.Count(a => a.Status == ArticleStatus.Submitted || a.Status == ArticleStatus.InReview),
                    firstYear = (int?)g.Min(a => a.CreatedAt.Year)
                })
                .FirstOrDefaultAsync();

            var assignmentStats = await _db.ArticleExpertAssignments
                .Where(a => a.ExpertId == me)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    total = g.Count(),
                    pending = g.Count(a => a.Status == ExpertAssignmentStatus.Pending),
                    accepted = g.Count(a => a.Status == ExpertAssignmentStatus.Accepted),
                    firstYear = (int?)g.Min(a => a.ReviewedAt.Year)
                })
                .FirstOrDefaultAsync();

            var reviewStats = await _db.ArticleReviews
                .Where(r => r.ReviewerId == me)
                .GroupBy(_ => 1)
                .Select(g => new
                {
                    total = g.Count(),
                    firstYear = (int?)g.Min(r => r.CreatedAt.Year)
                })
                .FirstOrDefaultAsync();

            var managedPending = await _db.Articles
                .CountAsync(a => a.Status == ArticleStatus.Submitted);

            return Ok(new
            {
                id = user.Id,
                fullName = user.FullName,
                email = user.Email,
                roles,
                stats = new
                {
                    articleCount = articleStats?.total ?? 0,
                    approvedCount = articleStats?.approved ?? 0,
                    pendingCount = articleStats?.pending ?? 0,
                    reviewedCount = reviewStats?.total ?? 0,
                    assignedCount = assignmentStats?.total ?? 0,
                    acceptedCount = assignmentStats?.accepted ?? 0,
                    expertPendingCount = assignmentStats?.pending ?? 0,
                    managerPendingCount = managedPending,
                    joinYear = articleStats?.firstYear ?? assignmentStats?.firstYear ?? reviewStats?.firstYear ?? DateTime.UtcNow.Year
                }
            });
        }

        [Authorize]
        [HttpPut("me")]
        public async Task<IActionResult> UpdateMe([FromBody] UpdateProfileDto dto)
        {
            if (string.IsNullOrWhiteSpace(dto.FullName))
                return BadRequest(new { Message = "Ad Soyad boş olamaz." });

            var me = _actor.GetCurrent().UserId;
            var user = await _userManager.FindByIdAsync(me.ToString());
            if (user is null) return NotFound();

            user.FullName = dto.FullName.Trim();
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
                return BadRequest(new { Message = "Güncelleme başarısız.", Errors = result.Errors });

            return Ok(new { id = user.Id, fullName = user.FullName, email = user.Email });
        }
    }

    public record UpdateProfileDto(string FullName);
}
