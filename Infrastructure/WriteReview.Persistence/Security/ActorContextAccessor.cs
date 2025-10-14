using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
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

        public ActorContext GetCurrent()
        {
            var user = _http.HttpContext?.User
                      ?? throw new UnauthorizedAccessException("Kullanıcı oturumu yok.");

            var idStr = user.FindFirstValue(ClaimTypes.NameIdentifier)
                      ?? user.FindFirstValue("sub")
                      ?? throw new UnauthorizedAccessException("Kullanıcı kimliği bulunamadı.");

            var userId = Guid.Parse(idStr);

            var roles = user.FindAll(ClaimTypes.Role)
                            .Select(c => c.Value)
                            .ToArray();

            return new ActorContext
            {
                UserId = userId,
                Roles = roles
            };
        }
    }
}
