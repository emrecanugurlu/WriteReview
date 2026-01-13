using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Security.Claims;
using System.Text;
using WriteReview.Application.Repositories.AppUser;
using WriteReview.Application.Repositories.Article;
using WriteReview.Application.Repositories.ArticleExpertAssignment;
using WriteReview.Application.Repositories.ExpertiseArea;
using WriteReview.Application.Security;
using WriteReview.Application.Services;
using WriteReview.Domain.Entities;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Repositories.AppUser;
using WriteReview.Persistence.Repositories.Article;
using WriteReview.Persistence.Repositories.ArticleExpertAssignment;
using WriteReview.Persistence.Repositories.ExpertiseArea;
using WriteReview.Persistence.Security;
using WriteReview.Persistence.Seed;
using WriteReview.Persistence.Services.Articles;
using WriteReview.Persistence.Services.Expert;
using WriteReview.Persistence.Services.ExpertiseArea;

var builder = WebApplication.CreateBuilder(args);


builder.Services.AddControllers();
builder.Services.AddDbContext<WriteReviewDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("WriteReviewDatabase")));

builder.Services.AddOpenApi();
builder.Services.AddIdentity<AppUser, AppRole>(options =>
{
    options.Password.RequireDigit = true;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = true;
    options.Password.RequireLowercase = true;
})
.AddEntityFrameworkStores<WriteReviewDbContext>()
.AddDefaultTokenProviders();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins("http://localhost:4200")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials();
    });
});

builder.Services
    .AddAuthentication(options =>
    {
        options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
        options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
    })
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!)
            ),
            NameClaimType = ClaimTypes.NameIdentifier,
            RoleClaimType = ClaimTypes.Role
        };
    });




builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<IActorContextAccessor, ActorContextAccessor>();
builder.Services.AddScoped<IArticleWriteRepository, ArticleWriteRepository>();
builder.Services.AddScoped<IArticleReadRepository, ArticleReadRepository>();
builder.Services.AddScoped<IExpertiseAreaWriteRepository, ExpertiseAreaWriteRepository>();
builder.Services.AddScoped<IExpertiseAreaReadRepository, ExpertiseAreaReadRepository>();
builder.Services.AddScoped<IAppUserReadRepository, AppUserReadRepository>();
builder.Services.AddScoped<IArticleStateService, ArticleStateService>();
builder.Services.AddScoped<IArticleExpertAssignmentWriteRepository, ArticleExpertAssignmentWriteRepository>();
builder.Services.AddScoped<IArticleExpertAssignmentReadRepository, ArticleExpertAssignmentReadRepository>();
builder.Services.AddScoped<ArticleStateService>();
builder.Services.AddScoped<ArticleService>();
builder.Services.AddScoped<ExpertiseAreaService>();
builder.Services.AddScoped<ExpertService>();



var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

    await ApplicationDbInitializer.SeedAsync(roleManager, userManager);

}

app.MapGet("/api/debug/whoami",
    [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
(HttpContext ctx) =>
    {
        var isAuth = ctx.User?.Identity?.IsAuthenticated ?? false;
        var claims = ctx.User?.Claims.Select(c => new { c.Type, c.Value }) ?? [];
        return Results.Ok(new { isAuth, claims });
    });


if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection(); 
app.UseCors("AllowAngular");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
