﻿using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using WorkoutFitnessTracker.API.Models.Dto_s.Summary;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Controllers;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

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
    [ProducesResponseType(typeof(ResponseWrapper<double?>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<ProgressRecordDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<WeeklyMonthlySummaryDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<PeriodComparisonDto>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status400BadRequest)]
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
    [ProducesResponseType(typeof(ResponseWrapper<DailyProgressDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetDailyProgress([FromQuery][Required] DateTime date)
    {
        var userId = GetUserId();

        var result = await _insightsService.GetDailyProgressAsync(userId, date);
        return WrapResponse(true, result, "Daily progress data retrieved successfully.");
    }

    [HttpGet("recommendations/underutilized-exercises")]
    [ProducesResponseType(typeof(ResponseWrapper<IEnumerable<string>>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ResponseWrapper<string>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> GetUnderutilizedExercises([FromQuery] string? category = null)
    {
        var userId = GetUserId();

        var result = await _insightsService.RecommendUnderutilizedExercisesAsync(userId, category);

        return WrapResponse(true, result, "Underutilized exercises retrieved successfully.");
    }
}
