using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Runtime.CompilerServices;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Controllers;
using WorkoutFitnessTracker.API.Models.Dto_s.Summary;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Controllers;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Xunit;

public class InsightsControllerTests
{
    private readonly Mock<IWorkoutService> _workoutServiceMock;
    private readonly Mock<IProgressRecordService> _progressRecordServiceMock;
    private readonly InsightsController _controller;

    public InsightsControllerTests()
    {
        _workoutServiceMock = new Mock<IWorkoutService>();
        _progressRecordServiceMock = new Mock<IProgressRecordService>();
        _controller = new InsightsController(_workoutServiceMock.Object, _progressRecordServiceMock.Object);

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
        _workoutServiceMock.Setup(s => s.CalculateAverageWorkoutDurationAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
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
    public async Task GetAverageWorkoutDuration_ThrowsInvalidOperationException_WhenNoWorkoutsFound()
    {
        // Arrange
        _workoutServiceMock.Setup(s => s.CalculateAverageWorkoutDurationAsync(It.IsAny<Guid>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
                           .ThrowsAsync(new InvalidOperationException("No workouts found for the specified date range."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetAverageWorkoutDuration(null, null));
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

        _workoutServiceMock
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
    public async Task GetMostFrequentExercises_ReturnsEmptyList_WhenNoExercisesFound()
    {
        // Arrange
        _workoutServiceMock
            .Setup(s => s.GetMostFrequentExercisesAsync(It.IsAny<Guid>(), null, null))
            .ReturnsAsync(new List<MostFrequentExercisesDto>());

        // Act
        var result = await _controller.GetMostFrequentExercises(null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseWrapper<List<MostFrequentExercisesDto>>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Most frequent exercises retrieved successfully.", response.Message);
        Assert.Empty(response.Data);
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

        _progressRecordServiceMock
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
    public async Task GetExerciseProgressTrend_ReturnsEmptyList_WhenNoProgressDataFound()
    {
        // Arrange
        var exerciseName = "Bench Press";

        _progressRecordServiceMock
            .Setup(s => s.GetExerciseProgressTrendAsync(It.IsAny<Guid>(), exerciseName, null, null))
            .ReturnsAsync(new List<ExerciseProgressTrendDto>());

        // Act
        var result = await _controller.GetExerciseProgressTrend(exerciseName, null, null);

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ResponseWrapper<List<ExerciseProgressTrendDto>>>(okResult.Value);

        Assert.True(response.Success);
        Assert.Equal("Exercise progress trend retrieved successfully.", response.Message);
        Assert.Empty(response.Data);
    }

    [Fact]
    public async Task GetExerciseProgressTrend_ThrowsInvalidOperationException_WhenUserNotLinkedToExercise()
    {
        // Arrange
        var exerciseName = "Bench Press";

        _progressRecordServiceMock
            .Setup(s => s.GetExerciseProgressTrendAsync(It.IsAny<Guid>(), exerciseName, null, null))
            .ThrowsAsync(new InvalidOperationException("User is not linked to the specified exercise."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetExerciseProgressTrend(exerciseName, null, null));
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

        _workoutServiceMock
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
    public async Task GetWeeklyMonthlySummary_ThrowsInvalidOperationException_WhenNoWorkoutsFound()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        _workoutServiceMock
            .Setup(s => s.GetWeeklyMonthlySummaryAsync(It.IsAny<Guid>(), startDate, endDate))
            .ThrowsAsync(new InvalidOperationException("No workouts found for the specified date range."));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _controller.GetWeeklyMonthlySummary(startDate, endDate));
    }

    [Fact]
    public async Task GetWeeklyMonthlySummary_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        // Arrange
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        // Simulate invalid model state (e.g., missing required date range parameters)
        _controller.ModelState.AddModelError("startDate", "The startDate field is required.");

        // Act
        var result = await _controller.GetWeeklyMonthlySummary(startDate, endDate);

        // Assert
        var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
        Assert.Equal(400, badRequestResult.StatusCode);
    }
}
