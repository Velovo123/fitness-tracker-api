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
    public class WorkoutPlanController : ControllerBase
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;

        public WorkoutPlanController(IWorkoutPlanRepository workoutPlanRepository)
        {
            _workoutPlanRepository = workoutPlanRepository;
        }

        // /api/WorkoutPlan
        [HttpGet]
        public async Task<ActionResult<IEnumerable<WorkoutPlanDto>>> GetWorkoutPlans([FromQuery] WorkoutPlanQueryParams? queryParams = null)
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized("User ID is missing from the token.");
                }

                var userGuid = Guid.Parse(userId);

                var workoutPlans = await _workoutPlanRepository.GetWorkoutPlansAsync(userGuid, queryParams!);

                if (workoutPlans == null || !workoutPlans.Any())
                {
                    return NotFound("No workout plans found for this user.");
                }

                return Ok(workoutPlans);
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

        // /api/WorkoutPlan
        [HttpPost]
        public async Task<ActionResult> SaveWorkoutPlan([FromBody] WorkoutPlanDto workoutPlanDto, [FromQuery] bool overwrite = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID is missing from the token.");

            var userGuid = Guid.Parse(userId);

            try
            {
                var result = await _workoutPlanRepository.SaveWorkoutPlanAsync(userGuid, workoutPlanDto, overwrite);

                if (result)
                    return Ok("Workout plan saved successfully.");
                else
                    return BadRequest("Failed to save the workout plan.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);  
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // /api/WorkoutPlan/name/{name}
        [HttpGet("name/{name}")]
        public async Task<ActionResult<WorkoutPlanDto>> GetWorkoutPlanByName(string name)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID is missing from the token.");

            var userGuid = Guid.Parse(userId);

            var workoutPlan = await _workoutPlanRepository.GetWorkoutPlanByNameAsync(userGuid, name);

            if (workoutPlan == null)
                return NotFound($"No workout plan found with the name {name}.");

            return Ok(workoutPlan);
        }

        // /api/WorkoutPlan/name/{name}
        [HttpDelete("name/{name}")]
        public async Task<ActionResult> DeleteWorkoutPlan(string name)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID is missing from the token.");

            var userGuid = Guid.Parse(userId);

            var result = await _workoutPlanRepository.DeleteWorkoutPlanAsync(userGuid, name);

            if (!result)
                return NotFound($"No workout plan found with the name {name} to delete.");

            return Ok($"Workout plan '{name}' deleted successfully.");
        }
    }
}
