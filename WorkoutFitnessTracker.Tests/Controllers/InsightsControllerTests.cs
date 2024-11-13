using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
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
    private readonly InsightsController _controller;

    public InsightsControllerTests()
    {
        _workoutServiceMock = new Mock<IWorkoutService>();
        _controller = new InsightsController(_workoutServiceMock.Object);

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
}
