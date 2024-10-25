using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkoutController : ControllerBase
    {
        private readonly IWorkoutRepository _workoutRepository;

        public WorkoutController(IWorkoutRepository workoutRepository)
        {
            _workoutRepository = workoutRepository;
        }

        // /api/Workout
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkoutDto>>> GetWorkouts([FromQuery] WorkoutQueryParams? queryParams = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID is missing from the token.");
                }

                var userGuid = Guid.Parse(userId);

                var workouts = await _workoutRepository.GetWorkoutsAsync(userGuid, queryParams!);

                if (workouts == null || !workouts.Any())
                {
                    return NotFound("No workouts found for this user.");
                }

                return Ok(workouts);
            }
            catch (FormatException)
            {
                return BadRequest("Invalid user ID format.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, "Internal server error: " + ex.Message);
            }
        }

        // /api/Workout
        [HttpPost]
        public async Task<ActionResult> SaveWorkout([FromBody] WorkoutDto workoutDto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID is missing from the token.");

            var userGuid = Guid.Parse(userId);

            try
            {
                var result = await _workoutRepository.SaveWorkoutAsync(userGuid, workoutDto);

                if (result)
                    return Ok("Workout saved successfully.");
                else
                    return BadRequest("Failed to save the workout.");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // /api/Workout/date/{date}
        [HttpGet("date/{date}")]
        public async Task<ActionResult<WorkoutDto>> GetWorkoutByDate(DateTime date)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID is missing from the token.");

            var userGuid = Guid.Parse(userId);

            var workout = await _workoutRepository.GetWorkoutsByDateAsync(userGuid, date);

            if (workout == null)
                return NotFound($"No workout found for the date {date.ToShortDateString()}.");

            return Ok(workout);
        }

        // /api/Workout/date/{date}
        [HttpDelete("date/{date}")]
        public async Task<ActionResult> DeleteWorkout(DateTime date)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID is missing from the token.");

            var userGuid = Guid.Parse(userId);

            var result = await _workoutRepository.DeleteWorkoutAsync(userGuid, date);

            if (!result)
                return NotFound($"No workout found for the date {date.ToShortDateString()} to delete.");

            return Ok($"Workout for the date {date.ToShortDateString()} deleted successfully.");
        }
    }
}
