using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Services.IServices;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkoutPlanController : BaseApiController
    {
        private readonly IWorkoutPlanService _workoutPlanService;

        public WorkoutPlanController(IWorkoutPlanService workoutPlanService)
        {
            _workoutPlanService = workoutPlanService;
        }

        [HttpGet]
        [ResponseCache(Duration = 60)]
        [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<WorkoutPlanDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<WorkoutPlanDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkoutPlans([FromQuery] WorkoutPlanQueryParams queryParams)
        {
            var userId = GetUserId();
            var workoutPlans = await _workoutPlanService.GetWorkoutPlansAsync(userId, queryParams);

            if (!workoutPlans.Any())
            {
                return NotFound(WrapResponse(false, Enumerable.Empty<WorkoutPlanDto>(), "No workout plans found for the specified criteria."));
            }
            return Ok(WrapResponse(true, workoutPlans, "Workout plans retrieved successfully."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveWorkoutPlan([FromBody] WorkoutPlanDto workoutPlanDto, [FromQuery] bool overwrite = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(WrapResponse(false, (string?)null, "Invalid workout plan data."));
            }

            var userId = GetUserId();
            var success = await _workoutPlanService.CreateWorkoutPlanAsync(userId, workoutPlanDto, overwrite);

            if (success)
            {
                return CreatedAtAction(nameof(GetWorkoutPlanByName), new { name = workoutPlanDto.Name }, WrapResponse(true, (string?)null, "Workout plan saved successfully."));
            }
            return BadRequest(WrapResponse(false, (string?)null, "Failed to save workout plan."));
        }

        [HttpGet("name/{name}")]
        [ResponseCache(Duration = 30)]
        [ProducesResponseType(typeof(ResponseWrapper<WorkoutPlanDto>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseWrapper<WorkoutPlanDto>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkoutPlanByName(string name)
        {
            var userId = GetUserId();
            var workoutPlan = await _workoutPlanService.GetWorkoutPlanByNameAsync(userId, name);

            if (workoutPlan == null)
            {
                return NotFound(WrapResponse(false, (WorkoutPlanDto?)null, "Workout plan not found for the specified name."));
            }
            return Ok(WrapResponse(true, workoutPlan, "Workout plan retrieved successfully."));
        }

        [HttpDelete("name/{name}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteWorkoutPlan(string name)
        {
            var userId = GetUserId();
            var success = await _workoutPlanService.DeleteWorkoutPlanAsync(userId, name);

            if (success)
            {
                return NoContent();
            }
            return NotFound(WrapResponse(false, (string?)null, "Workout plan not found for the specified name."));
        }
    }
}
