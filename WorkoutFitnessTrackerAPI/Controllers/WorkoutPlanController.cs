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
    public class WorkoutPlanController : BaseApiController
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;

        public WorkoutPlanController(IWorkoutPlanRepository workoutPlanRepository)
        {
            _workoutPlanRepository = workoutPlanRepository;
        }

        [HttpGet]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<WorkoutPlanDto>>>> GetWorkoutPlans([FromQuery] WorkoutPlanQueryParams queryParams)
        {
            throw new NotImplementedException();
        }

        [HttpPost]
        public async Task<ActionResult<ResponseWrapper<string>>> SaveWorkoutPlan([FromBody] WorkoutPlanDto workoutPlanDto, [FromQuery] bool overwrite = false)
        {
            throw new NotImplementedException();
        }

        [HttpGet("name/{name}")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<ResponseWrapper<WorkoutPlanDto>>> GetWorkoutPlanByName(string name)
        {
            throw new NotImplementedException();
        }

        [HttpDelete("name/{name}")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteWorkoutPlan(string name)
        {
            throw new NotImplementedException();
        }
    }
}
