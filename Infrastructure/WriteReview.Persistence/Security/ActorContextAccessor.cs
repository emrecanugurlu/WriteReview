using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using WriteReview.Application.Security;
using WriteReview.Domain.Security;

namespace WriteReview.Persistence.Security
{
    public sealed class ActorContextAccessor : IActorContextAccessor
    {
        private readonly IHttpContextAccessor _http;

        public ActorContextAccessor(IHttpContextAccessor http)
            => _http = http;

        public string? GetUserId()
        {
            return _http.HttpContext?.User?
                .FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        public string? GetUserEmail()
        {
            return _http.HttpContext?.User?
                .FindFirst(ClaimTypes.Email)?.Value;
        }

        public List<string>? GetUserRole()
        {
            return _http.HttpContext?.User?
                .FindAll(ClaimTypes.Role)
                .Select(c=>c.Value)
                .ToList();
        }


        public ActorContext GetCurrent()
        {
            Console.WriteLine(_http.HttpContext?.User);
            Console.WriteLine(Guid.Parse(GetUserId()!));
            Console.WriteLine(GetUserRole()!);

            return new ActorContext { UserId = Guid.Parse(GetUserId()!), Roles = GetUserRole()!};
        }
    }
}
