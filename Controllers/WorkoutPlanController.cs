using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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

        // /api/WorkoutPlan
        [HttpGet]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<WorkoutPlanDto>>>> GetWorkoutPlans([FromQuery] WorkoutPlanQueryParams queryParams)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ResponseWrapper<string>(false, null, "User ID is missing from the token."));

            var userGuid = Guid.Parse(userId);
            var workoutPlans = await _workoutPlanRepository.GetWorkoutPlansAsync(userGuid, queryParams);

            return workoutPlans == null || !workoutPlans.Any()
                ? NotFound(new ResponseWrapper<IEnumerable<WorkoutPlanDto>>(false, null, "No workout plans found for this user."))
                : WrapResponse(true, workoutPlans, "Workout plans retrieved successfully.");
        }

        // /api/WorkoutPlan
        [HttpPost]
        public async Task<ActionResult<ResponseWrapper<string>>> SaveWorkoutPlan([FromBody] WorkoutPlanDto workoutPlanDto, [FromQuery] bool overwrite = false)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ResponseWrapper<string>(false, null, string.Join(", ", errors)));
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ResponseWrapper<string>(false, null, "User ID is missing from the token."));

            var userGuid = Guid.Parse(userId);
            var result = await _workoutPlanRepository.SaveWorkoutPlanAsync(userGuid, workoutPlanDto, overwrite);

            return result
                ? WrapResponse(true, "Workout plan saved successfully.", "Workout plan saved successfully.")
                : WrapResponse<string>(false, null, "Failed to save the workout plan.");
        }

        // /api/WorkoutPlan/name/{name}
        [HttpGet("name/{name}")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<ResponseWrapper<WorkoutPlanDto>>> GetWorkoutPlanByName(string name)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ResponseWrapper<string>(false, null, "User ID is missing from the token."));

            var userGuid = Guid.Parse(userId);
            var workoutPlan = await _workoutPlanRepository.GetWorkoutPlanByNameAsync(userGuid, name);

            return workoutPlan == null
                ? NotFound(new ResponseWrapper<WorkoutPlanDto>(false, null, $"No workout plan found with the name {name}."))
                : WrapResponse(true, workoutPlan, "Workout plan retrieved successfully.");
        }

        // /api/WorkoutPlan/name/{name}
        [HttpDelete("name/{name}")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteWorkoutPlan(string name)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ResponseWrapper<string>(false, null, "User ID is missing from the token."));

            var userGuid = Guid.Parse(userId);
            var result = await _workoutPlanRepository.DeleteWorkoutPlanAsync(userGuid, name);

            return result
                ? WrapResponse(true, $"Workout plan '{name}' deleted successfully.", "Workout plan deleted successfully.")
                : NotFound(new ResponseWrapper<string>(false, null, $"No workout plan found with the name {name} to delete."));
        }
    }
}
