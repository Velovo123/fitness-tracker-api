using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Helpers;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgressRecordController : BaseApiController
    {
        private readonly IProgressRecordRepository _progressRecordRepository;

        public ProgressRecordController(IProgressRecordRepository progressRecordRepository)
        {
            _progressRecordRepository = progressRecordRepository;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<ProgressRecordDto>>>> GetProgressRecords([FromQuery] ProgressRecordQueryParams queryParams)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<ActionResult<ResponseWrapper<string>>> SaveProgressRecord([FromBody] ProgressRecordDto progressRecordDto, [FromQuery] bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        [HttpGet("date/{date}/exercise/{exerciseName}")]
        public async Task<ActionResult<ResponseWrapper<ProgressRecordDto>>> GetProgressRecordByDate(DateTime date, string exerciseName)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("date/{date}/exercise/{exerciseName}")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteProgressRecord(DateTime date, string exerciseName)
        {
            throw new NotImplementedException();
        }
    }
}
