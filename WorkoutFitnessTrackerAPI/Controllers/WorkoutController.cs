using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkoutController : BaseApiController
    {
        private readonly IWorkoutRepository _workoutRepository;

        public WorkoutController(IWorkoutRepository workoutRepository)
        {
            _workoutRepository = workoutRepository;
        }

        [HttpGet]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<WorkoutDto>>>> GetWorkouts([FromQuery] WorkoutQueryParams? queryParams = null)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<ActionResult<ResponseWrapper<string>>> SaveWorkout([FromBody] WorkoutDto workoutDto, [FromQuery] bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        [HttpGet("date/{date}")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<WorkoutDto>>>> GetWorkoutByDate(DateTime date)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("date/{date}")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteWorkout(DateTime date)
        {
            throw new NotImplementedException();
        }
    }
}
