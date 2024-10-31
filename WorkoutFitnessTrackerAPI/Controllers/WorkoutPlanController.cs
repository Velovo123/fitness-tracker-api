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
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ResponseWrapper<string>(false, null, string.Join("; ", errors)));
            }
            var userGuid = GetUserId();
            var workoutPlans = await _workoutPlanRepository.GetWorkoutPlansAsync(userGuid, queryParams);

            return workoutPlans == null || !workoutPlans.Any()
                ? NotFound(new ResponseWrapper<IEnumerable<WorkoutPlanDto>>(false, null, "No workout plans found for this user."))
                : WrapResponse(true, workoutPlans, "Workout plans retrieved successfully.");
        }

        [HttpPost]
        public async Task<ActionResult<ResponseWrapper<string>>> SaveWorkoutPlan([FromBody] WorkoutPlanDto workoutPlanDto, [FromQuery] bool overwrite = false)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ResponseWrapper<string>(false, null, string.Join(", ", errors)));
            }

            var userGuid = GetUserId();
            var result = await _workoutPlanRepository.SaveWorkoutPlanAsync(userGuid, workoutPlanDto, overwrite);

            return result
                ? WrapResponse(true, "Workout plan saved successfully.", "Workout plan saved successfully.")
                : WrapResponse<string>(false, null, "Failed to save the workout plan.");
        }

        [HttpGet("name/{name}")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<ResponseWrapper<WorkoutPlanDto>>> GetWorkoutPlanByName(string name)
        {
            var userGuid = GetUserId();
            var workoutPlan = await _workoutPlanRepository.GetWorkoutPlanByNameAsync(userGuid, name);

            return workoutPlan == null
                ? NotFound(new ResponseWrapper<WorkoutPlanDto>(false, null, $"No workout plan found with the name {name}."))
                : WrapResponse(true, workoutPlan, "Workout plan retrieved successfully.");
        }

        [HttpDelete("name/{name}")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteWorkoutPlan(string name)
        {
            var userGuid = GetUserId();
            var result = await _workoutPlanRepository.DeleteWorkoutPlanAsync(userGuid, name);

            return result
                ? WrapResponse(true, $"Workout plan '{name}' deleted successfully.", "Workout plan deleted successfully.")
                : NotFound(new ResponseWrapper<string>(false, null, $"No workout plan found with the name {name} to delete."));
        }
    }
}
