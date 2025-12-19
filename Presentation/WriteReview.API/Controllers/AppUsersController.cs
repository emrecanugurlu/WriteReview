using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WriteReview.Application.Repositories.AppUser;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]  
    public class AppUsersController : ControllerBase
    {
        IAppUserReadRepository _appUserReadRepository;
        public AppUsersController(IAppUserReadRepository appUserReadRepository)
        {
            _appUserReadRepository = appUserReadRepository;
        }

        [HttpGet()]
        public IActionResult GetAllUsers()
        {
            var users = _appUserReadRepository.GetAll();
            return Ok(users);
        }
    }
}
