using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Controllers;
using WorkoutFitnessTrackerAPI.Services;

namespace WorkoutFitnessTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsightsController : BaseApiController
    {
        private readonly IWorkoutService _workoutService;
        private readonly IProgressRecordService _progressRecordService;

        public InsightsController(IWorkoutService workoutService, IProgressRecordService progressRecordService)
        {
            _workoutService = workoutService;
            _progressRecordService = progressRecordService;
        }

        [HttpGet("average-workout-duration")]
        public async Task<IActionResult> GetAverageWorkoutDuration([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var userId = GetUserId();

            if (startDate.HasValue && endDate.HasValue && endDate <= startDate)
            {
                return BadRequest("End date must be greater than start date.");
            }

            var result = await _workoutService.CalculateAverageWorkoutDurationAsync(userId, startDate, endDate);
            return WrapResponse(true, result, "Average workout duration calculated successfully.");
        }

        [HttpGet("most-frequent-exercises")]
        public async Task<IActionResult> GetMostFrequentExercises([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var userId = GetUserId();

            if (startDate.HasValue && endDate.HasValue && endDate <= startDate)
            {
                return BadRequest("End date must be greater than start date.");
            }

            var result = await _workoutService.GetMostFrequentExercisesAsync(userId, startDate, endDate);
            return WrapResponse(true, result, "Most frequent exercises retrieved successfully.");
        }

        [HttpGet("exercise-progress-trend")]
        public async Task<IActionResult> GetExerciseProgressTrend([FromQuery][Required] string exerciseName, [FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var userId = GetUserId();

            if (startDate.HasValue && endDate.HasValue && endDate <= startDate)
            {
                return BadRequest("End date must be greater than start date.");
            }

            var result = await _progressRecordService.GetExerciseProgressTrendAsync(userId, exerciseName, startDate, endDate);
            return WrapResponse(true, result, "Exercise progress trend retrieved successfully.");
        }

        [HttpGet("summary")]
        public async Task<IActionResult> GetWeeklyMonthlySummary([FromQuery][Required] DateTime startDate, [FromQuery][Required] DateTime endDate)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            if (endDate <= startDate)
            {
                return BadRequest("End date must be greater than start date.");
            }

            var userId = GetUserId();
            var result = await _workoutService.GetWeeklyMonthlySummaryAsync(userId, startDate, endDate);
            return WrapResponse(true, result, "Weekly/Monthly summary retrieved successfully.");
        }
    }
}
