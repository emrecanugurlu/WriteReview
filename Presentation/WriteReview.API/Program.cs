using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using System;
using System.Security.Claims;
using System.Text;
using System.Threading.RateLimiting;
using WriteReview.API.Middleware;
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

Log.Logger = new LoggerConfiguration()
    .WriteTo.Console()
    .CreateBootstrapLogger();

var builder = WebApplication.CreateBuilder(args);

builder.Host.UseSerilog((context, services, config) =>
    config.ReadFrom.Configuration(context.Configuration)
          .ReadFrom.Services(services));


builder.Services.AddControllers();
builder.Services.AddSignalR();
builder.Services.AddDbContext<WriteReviewDbContext>(options =>
    options.UseNpgsql(builder.Configuration.GetConnectionString("WriteReviewDatabase")));

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

var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? Array.Empty<string>();
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAngular", policy =>
    {
        policy.WithOrigins(allowedOrigins)
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

builder.Services.AddRateLimiter(options =>
{
    // Login endpoint: 5 istek / 1 dakika (brute-force koruması)
    options.AddFixedWindowLimiter("login", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 5;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    // Genel API: 200 istek / 1 dakika
    options.AddFixedWindowLimiter("general", o =>
    {
        o.Window = TimeSpan.FromMinutes(1);
        o.PermitLimit = 200;
        o.QueueLimit = 0;
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
    options.OnRejected = async (context, ct) =>
    {
        context.HttpContext.Response.StatusCode = StatusCodes.Status429TooManyRequests;
        context.HttpContext.Response.ContentType = "application/json";
        await context.HttpContext.Response.WriteAsync(
            "{\"message\":\"Çok fazla istek gönderdiniz. Lütfen bir süre bekleyip tekrar deneyin.\"}", ct);
    };
});


var app = builder.Build();



using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();
    var db = scope.ServiceProvider.GetRequiredService<WriteReviewDbContext>();

    await db.Database.MigrateAsync();
    await ApplicationDbInitializer.SeedAsync(roleManager, userManager, db);
}

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapGet("/api/debug/whoami",
        [Authorize(AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
    (HttpContext ctx) =>
        {
            var isAuth = ctx.User?.Identity?.IsAuthenticated ?? false;
            var claims = ctx.User?.Claims.Select(c => new { c.Type, c.Value }) ?? [];
            return Results.Ok(new { isAuth, claims });
        });
}

// Cloud Run / reverse proxy: orijinal scheme ve client IP'yi X-Forwarded-* başlıklarından al
var forwardedOptions = new ForwardedHeadersOptions
{
    ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
};
forwardedOptions.KnownNetworks.Clear();
forwardedOptions.KnownProxies.Clear();
app.UseForwardedHeaders(forwardedOptions);

app.UseMiddleware<ExceptionHandlerMiddleware>();
app.UseSerilogRequestLogging();

// Production'da TLS Cloud Run edge'inde sonlanır; in-app redirect gereksiz (loop önlemi)
if (app.Environment.IsDevelopment())
    app.UseHttpsRedirection();

app.UseCors("AllowAngular");
app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHub<WriteReview.API.Hubs.NotificationHub>("/notificationHub");

app.Run();
