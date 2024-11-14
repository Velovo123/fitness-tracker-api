using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Controllers;

namespace WorkoutFitnessTracker.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InsightsController : BaseApiController
    {
        private readonly IInsightsService _insightsService;

        public InsightsController(IInsightsService insightsService)
        {
            _insightsService = insightsService;
        }

        [HttpGet("average-workout-duration")]
        public async Task<IActionResult> GetAverageWorkoutDuration([FromQuery] DateTime? startDate, [FromQuery] DateTime? endDate)
        {
            var userId = GetUserId();

            if (startDate.HasValue && endDate.HasValue && endDate <= startDate)
            {
                return BadRequest("End date must be greater than start date.");
            }

            var result = await _insightsService.CalculateAverageWorkoutDurationAsync(userId, startDate, endDate);
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

            var result = await _insightsService.GetMostFrequentExercisesAsync(userId, startDate, endDate);
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

            var result = await _insightsService.GetExerciseProgressTrendAsync(userId, exerciseName, startDate, endDate);
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
            var result = await _insightsService.GetWeeklyMonthlySummaryAsync(userId, startDate, endDate);
            return WrapResponse(true, result, "Weekly/Monthly summary retrieved successfully.");
        }

        [HttpGet("weekly-monthly-comparison")]
        public async Task<IActionResult> GetWeeklyMonthlyComparison([FromQuery] DateTime startDate, [FromQuery] DateTime endDate, [FromQuery] string intervalType)
        {
            if (string.IsNullOrWhiteSpace(intervalType) || (intervalType.ToLower() != "weekly" && intervalType.ToLower() != "monthly"))
            {
                return BadRequest("Invalid interval type. Please use 'weekly' or 'monthly'.");
            }

            if (endDate <= startDate)
            {
                return BadRequest("End date must be greater than start date.");
            }

            var userId = GetUserId();
            var result = await _insightsService.GetWeeklyMonthlyComparisonAsync(userId, startDate, endDate, intervalType);
            return WrapResponse(true, result, "Weekly/Monthly comparison data retrieved successfully.");
        }

        [HttpGet("daily-progress")]
        public async Task<IActionResult> GetDailyProgress([FromQuery][Required] DateTime date)
        {
            var userId = GetUserId();

            var result = await _insightsService.GetDailyProgressAsync(userId, date);
            return WrapResponse(true, result, "Daily progress data retrieved successfully.");
        }
    }
}
