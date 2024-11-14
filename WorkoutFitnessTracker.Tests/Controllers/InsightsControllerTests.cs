using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Controllers;
using WorkoutFitnessTracker.API.Models.Dto_s.Summary;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Helpers;
using Xunit;

public class InsightsControllerTests
{
    private readonly Mock<IInsightsService> _insightsServiceMock;
    private readonly InsightsController _controller;

    public InsightsControllerTests()
    {
        _insightsServiceMock = new Mock<IInsightsService>();
        _controller = new InsightsController(_insightsServiceMock.Object);

        var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
        {
            new Claim(ClaimTypes.NameIdentifier, Guid.NewGuid().ToString())
        }));

        _controller.ControllerContext = new ControllerContext
        {
            HttpContext = new DefaultHttpContext { User = user }
        };
    }

    [Fact]
    public async Task GetAverageWorkoutDuration_ReturnsOk_WhenWorkoutsExist()
    {
        // Arrange
        var workoutStatisticsDto = new WorkoutStatisticsDto
        {
            AverageWorkoutDuration = 60,
            WorkoutCount = 5
        };
        _insightsServiceMock.Setup(s => s.CalculateAverageWorkoutDurationAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                            .ReturnsAsync(workoutStatisticsDto);

        // Act
        var result = await _controller.GetAverageWorkoutDuration(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseWrapper<WorkoutStatisticsDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Average workout duration calculated successfully.", response.Message);
        Assert.Equal(workoutStatisticsDto, response.Data);
    }

    [Fact]
    public async Task GetMostFrequentExercises_ReturnsOk_WithFrequentExercisesData()
    {
        // Arrange
        var mostFrequentExercises = new List<MostFrequentExercisesDto>
        {
            new MostFrequentExercisesDto("Bench Press", 10),
            new MostFrequentExercisesDto("Squat", 8),
            new MostFrequentExercisesDto("Deadlift", 5)
        };

        _insightsServiceMock
            .Setup(s => s.GetMostFrequentExercisesAsync(It.IsAny<Guid>(), null, null))
            .ReturnsAsync(mostFrequentExercises);

        // Act
        var result = await _controller.GetMostFrequentExercises(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseWrapper<List<MostFrequentExercisesDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Most frequent exercises retrieved successfully.", response.Message);
        Assert.Equal(mostFrequentExercises, response.Data);
    }

    [Fact]
    public async Task GetExerciseProgressTrend_ReturnsOk_WithTrendData()
    {
        // Arrange
        var exerciseName = "Bench Press";
        var trendData = new List<ExerciseProgressTrendDto>
        {
            new ExerciseProgressTrendDto { Date = DateTime.UtcNow.AddDays(-2), Progress = "Reps: 10" },
            new ExerciseProgressTrendDto { Date = DateTime.UtcNow.AddDays(-1), Progress = "Reps: 12" }
        };

        _insightsServiceMock
            .Setup(s => s.GetExerciseProgressTrendAsync(It.IsAny<Guid>(), exerciseName, null, null))
            .ReturnsAsync(trendData);

        // Act
        var result = await _controller.GetExerciseProgressTrend(exerciseName, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseWrapper<List<ExerciseProgressTrendDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Exercise progress trend retrieved successfully.", response.Message);
        Assert.Equal(trendData, response.Data);
    }

    [Fact]
    public async Task GetWeeklyMonthlySummary_ReturnsOk_WhenWorkoutsExist()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var summaryDto = new WeeklyMonthlySummaryDto
        {
            TotalWorkouts = 5,
            AverageDuration = 45,
            TotalReps = 200,
            TotalSets = 50,
            StartDate = startDate,
            EndDate = endDate
        };

        _insightsServiceMock
            .Setup(s => s.GetWeeklyMonthlySummaryAsync(It.IsAny<Guid>(), startDate, endDate))
            .ReturnsAsync(summaryDto);

        // Act
        var result = await _controller.GetWeeklyMonthlySummary(startDate, endDate);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseWrapper<WeeklyMonthlySummaryDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Weekly/Monthly summary retrieved successfully.", response.Message);
        Assert.Equal(summaryDto, response.Data);
    }

    [Fact]
    public async Task GetWeeklyMonthlyComparison_ReturnsOkResult_WithValidInput()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;
        var intervalType = "weekly";
        var comparisonData = new List<PeriodComparisonDto>
        {
            new PeriodComparisonDto
            {
                PeriodStart = startDate,
                PeriodEnd = endDate,
                TotalWorkouts = 5,
                AverageDuration = 60,
                TotalReps = 100,
                TotalSets = 30
            }
        };

        _insightsServiceMock
            .Setup(s => s.GetWeeklyMonthlyComparisonAsync(It.IsAny<Guid>(), startDate, endDate, intervalType))
            .ReturnsAsync(comparisonData);

        // Act
        var result = await _controller.GetWeeklyMonthlyComparison(startDate, endDate, intervalType);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseWrapper<List<PeriodComparisonDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Weekly/Monthly comparison data retrieved successfully.", response.Message);
        Assert.Equal(comparisonData, response.Data);
    }

    [Fact]
    public async Task GetDailyProgress_ReturnsOk_WithDailyProgressData()
    {
        // Arrange
        var date = DateTime.UtcNow.Date;
        var dailyProgressDto = new DailyProgressDto
        {
            Date = date,
            TotalWorkouts = 2,
            AverageDuration = 45,
            TotalReps = 100,
            TotalSets = 30,
            Exercises = new List<ExerciseProgressDto>
            {
                new ExerciseProgressDto
                {
                    ExerciseName = "Bench Press",
                    PlannedSets = 3,
                    PlannedReps = 10,
                    ActualSets = 3,
                    ActualReps = 10
                }
            }
        };

        _insightsServiceMock
            .Setup(s => s.GetDailyProgressAsync(It.IsAny<Guid>(), date))
            .ReturnsAsync(dailyProgressDto);

        // Act
        var result = await _controller.GetDailyProgress(date);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseWrapper<DailyProgressDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal("Daily progress data retrieved successfully.", response.Message);
        Assert.Equal(dailyProgressDto, response.Data);
    }
}
