using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Routing;
using WriteReview.Application.Repositories.ExpertiseArea;
using WriteReview.Domain.Dtos;
using WriteReview.Domain.Dtos.ResponseDto;
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
        private readonly IExpertiseAreaWriteRepository _expertiseAreaWriteRepository;
        private readonly IExpertiseAreaReadRepository _expertiseAreaReadRepository;
        

        public ExpertiseAreasController(ExpertiseAreaService expertiseAreaService, WriteReviewDbContext db, IExpertiseAreaWriteRepository expertiseAreaWriteRepository, IExpertiseAreaReadRepository expertiseAreaReadRepository)
        {
            _expertiseAreaService = expertiseAreaService;
            _db = db;
            _expertiseAreaWriteRepository = expertiseAreaWriteRepository;
            _expertiseAreaReadRepository = expertiseAreaReadRepository;
        }

        [HttpGet("withoutusers")]
        public  IActionResult GetAllExpertiseAreasWithoutUsers()
        {
            var expertiseAreas = _expertiseAreaReadRepository.GetAll();
            return Ok(expertiseAreas);
        }

        [HttpGet("withusers")]
        public IActionResult GetAllExpertiseAreasWithUsers()
        {
            var expertiseAreas = _expertiseAreaService.GetAllExpertiseAreaWithUsers(_db);
            return Ok(expertiseAreas);
        }

        [HttpPost]
        public async Task<IActionResult> AddExpertiseArea([FromBody] AddExpertiseAreaDto dto)
        {
            bool isAdded = await _expertiseAreaWriteRepository.AddAsync(new Domain.Entities.ExpertiseArea()
            {
                Id = new Guid(),
                Name = dto.Name
            });
            await _expertiseAreaWriteRepository.SaveChangesAsync();
            return Ok(isAdded);
        }

    }
}
