using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WriteReview.Domain.Dtos;
using WriteReview.Persistence.Contexts;
using WriteReview.Persistence.Services.ExpertiseArea;

namespace WriteReview.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ExpertiseAreasController : ControllerBase
    {
        private readonly ExpertiseAreaService _expertiseAreaService;
        private readonly WriteReviewDbContext _db;

        public ExpertiseAreasController(ExpertiseAreaService expertiseAreaService, WriteReviewDbContext db)
        {
            _expertiseAreaService = expertiseAreaService;
            this._db = db;
        }

        [HttpGet("withoutusers")]
        public  IActionResult GetAllExpertiseAreasWithoutUsers()
        {
            var expertiseAreas = _expertiseAreaService.GetAllExpertiseAreaWithoutUsers(this._db);
            return Ok(expertiseAreas);
        }

        [HttpGet("withusers")]
        public IActionResult GetAllExpertiseAreasWithUsers()
        {
            var expertiseAreas = _expertiseAreaService.GetAllExpertiseAreaWithUsers(this._db);
            return Ok(expertiseAreas);
        }

        [HttpPost]
        public IActionResult AddExpertiseArea([FromBody] AddExpertiseAreaDto dto)
        {
            string  message =_expertiseAreaService.AddExpertiseArea(_db,dto);
            return Ok(message);
        }
    }
}
