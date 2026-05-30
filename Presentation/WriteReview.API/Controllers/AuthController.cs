using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
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
        [EnableRateLimiting("login")]
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

            var refreshToken = GenerateRefreshToken();
            user.RefreshToken = refreshToken;
            user.RefreshTokenExpiryTime = DateTime.UtcNow.AddDays(7);
            await _userManager.UpdateAsync(user);

            var jwtString = new JwtSecurityTokenHandler().WriteToken(token);

            Response.Cookies.Append("jwt", jwtString, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                message = "Giriş başarılı",
                role = roles,
                token = jwtString,
                refreshToken = refreshToken
            });
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> RefreshToken([FromBody] TokenDto tokenDto)
        {
            if (tokenDto is null)
                return BadRequest("Invalid client request");

            var principal = GetPrincipalFromExpiredToken(tokenDto.Token);
            if (principal == null)
                return BadRequest("Invalid access token or refresh token");

            var email = principal.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email)?.Value;
            if (email == null)
                return BadRequest("Invalid access token or refresh token");

            var user = await _userManager.FindByEmailAsync(email);
            if (user == null || user.RefreshToken != tokenDto.RefreshToken || user.RefreshTokenExpiryTime <= DateTime.UtcNow)
                return BadRequest("Invalid access token or refresh token");

            var roles = await _userManager.GetRolesAsync(user);

            var claims = new[] { new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()), new Claim(ClaimTypes.Email, user.Email!) }
                .Concat(roles.Select(role => new Claim(ClaimTypes.Role, role)))
                .ToArray();

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.Now.AddMinutes(Convert.ToDouble(_configuration["Jwt:ExpireMinutes"]));

            var newToken = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds);

            var newRefreshToken = GenerateRefreshToken();
            user.RefreshToken = newRefreshToken;
            await _userManager.UpdateAsync(user);

            var jwtString = new JwtSecurityTokenHandler().WriteToken(newToken);

            Response.Cookies.Append("jwt", jwtString, new CookieOptions
            {
                HttpOnly = false,
                Secure = true,
                SameSite = SameSiteMode.None,
                Path = "/",
                Expires = DateTimeOffset.UtcNow.AddDays(7)
            });

            return Ok(new
            {
                token = jwtString,
                refreshToken = newRefreshToken
            });
        }

        private string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            using var rng = RandomNumberGenerator.Create();
            rng.GetBytes(randomNumber);
            return Convert.ToBase64String(randomNumber);
        }

        private ClaimsPrincipal? GetPrincipalFromExpiredToken(string? token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateAudience = false,
                ValidateIssuer = false,
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!)),
                ValidateLifetime = false // Bu önemli: süresi geçmiş token'ı kabul ediyoruz
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out SecurityToken securityToken);
            var jwtSecurityToken = securityToken as JwtSecurityToken;
            if (jwtSecurityToken == null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                throw new SecurityTokenException("Invalid token");

            return principal;
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
    public record TokenDto(string Token, string RefreshToken);
}

