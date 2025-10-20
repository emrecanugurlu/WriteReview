using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WriteReview.API.Controllers
{
    [Route("api/debug")]
    [ApiController]
    public class DebugController : ControllerBase
    {

            [Authorize]
            [HttpGet("whoami1")]
            public IActionResult WhoAmI()
            {
                return Ok(new
                {
                    isAuth = User.Identity?.IsAuthenticated ?? false,
                    claims = User.Claims.Select(c => new { c.Type, c.Value })
                });
            }
        
    }
}