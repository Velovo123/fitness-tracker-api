using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutFitnessTracker.API.Services.IServices;
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
        private readonly IWorkoutService _workoutService;

        public WorkoutController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;
        }

        [HttpGet]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<WorkoutDto>>>> GetWorkouts([FromQuery] WorkoutQueryParams? queryParams = null)
        {
            var userId = GetUserId();
            var workouts = await _workoutService.GetWorkoutsAsync(userId, queryParams);
            return WrapResponse(true, workouts, "Workouts retrieved successfully.");
        }

        [HttpPost]
        public async Task<ActionResult<ResponseWrapper<string>>> SaveWorkout([FromBody] WorkoutDto workoutDto, [FromQuery] bool overwrite = false)
        {
            if (!ModelState.IsValid)
                return WrapResponse(false, (string?)null, "Invalid workout data.");

            var userId = GetUserId();
            var success = await _workoutService.CreateWorkoutAsync(userId, workoutDto, overwrite);
            return success
                ? WrapResponse(true, (string?)null, "Workout saved successfully.")
                : WrapResponse(false, (string?)null, "Failed to save workout.");
        }

        [HttpGet("date/{date}")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<WorkoutDto>>>> GetWorkoutByDate(DateTime date)
        {
            var userId = GetUserId();
            var workouts = await _workoutService.GetWorkoutsByDateAsync(userId, date);
            return WrapResponse(true, workouts, "Workout for the specified date retrieved successfully.");
        }

        [HttpDelete("date/{date}")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteWorkout(DateTime date)
        {
            var userId = GetUserId();
            var success = await _workoutService.DeleteWorkoutAsync(userId, date);
            return success
                ? WrapResponse(true, (string?)null, "Workout deleted successfully.")
                : WrapResponse(false, (string?)null, "Workout not found for the specified date.");
        }
    }
}
