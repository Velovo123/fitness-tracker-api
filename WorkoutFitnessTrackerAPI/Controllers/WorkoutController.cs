using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkoutController : BaseApiController
    {
        private readonly IWorkoutService _workoutService;

        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }

        [HttpGet]
        [ResponseCache(Duration = 60)]
        [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<WorkoutDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<WorkoutDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkouts([FromQuery] WorkoutQueryParams? queryParams = null)
        {
            var userId = GetUserId();
            var workouts = await _workoutService.GetWorkoutsAsync(userId, queryParams);
            if (!workouts.Any())
            {
                return NotFound(WrapResponse(false, Enumerable.Empty<WorkoutDto>(), "No workouts found for the specified criteria."));
            }
            return Ok(WrapResponse(true, workouts, "Workouts retrieved successfully."));
        }

        [HttpPost]
        [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> SaveWorkout([FromBody] WorkoutDto workoutDto, [FromQuery] bool overwrite = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(WrapResponse(false, (string?)null, "Invalid workout data."));
            }

            var userId = GetUserId();
            var success = await _workoutService.CreateWorkoutAsync(userId, workoutDto, overwrite);
            if (success)
            {
                return CreatedAtAction(nameof(GetWorkoutByDate), new { date = workoutDto.Date }, WrapResponse(true, (string?)null, "Workout saved successfully."));
            }
            return BadRequest(WrapResponse(false, (string?)null, "Failed to save workout."));
        }

        [HttpGet("date/{date}")]
        [ResponseCache(Duration = 30)]
        [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<WorkoutDto>>), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<WorkoutDto>>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetWorkoutByDate(DateTime date)
        {
            var userId = GetUserId();
            var workouts = await _workoutService.GetWorkoutsByDateAsync(userId, date);
            if (!workouts.Any())
            {
                return NotFound(WrapResponse(false, Enumerable.Empty<WorkoutDto>(), "No workout found for the specified date."));
            }
            return Ok(WrapResponse(true, workouts, "Workout for the specified date retrieved successfully."));
        }

        [HttpDelete("date/{date}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status404NotFound)]
        public async Task<IActionResult> DeleteWorkout(DateTime date)
        {
            var userId = GetUserId();
            var success = await _workoutService.DeleteWorkoutAsync(userId, date);
            if (success)
            {
                return NoContent();
            }
            return NotFound(WrapResponse(false, (string?)null, "Workout not found for the specified date."));
        }
    }
}
