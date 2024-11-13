using Microsoft.AspNetCore.Mvc;
using System.Runtime.CompilerServices;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Controllers;

namespace WorkoutFitnessTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsightsController : BaseApiController
    {
        private readonly IWorkoutService _workoutService;
        public InsightsController(IWorkoutService workoutService)
        {
            _workoutService = workoutService;   
        }

        [HttpGet("average-workout-duration")]
        public async Task<IActionResult> GetAverageWorkoutDuration([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var userId = GetUserId(); // Retrieves the user ID from the token

            var result = await _workoutService.CalculateAverageWorkoutDurationAsync(userId, startDate, endDate);

            return WrapResponse(true, result, "Average workout duration calculated successfully.");
        }
    }
}
