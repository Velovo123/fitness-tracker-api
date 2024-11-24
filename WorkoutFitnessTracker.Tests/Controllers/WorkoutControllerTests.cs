using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Controllers;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using Xunit;

public class WorkoutControllerTests
{
    private readonly Mock<IWorkoutService> _workoutServiceMock;
    private readonly WorkoutController _controller;

    public WorkoutControllerTests()
    {
        _workoutServiceMock = new Mock<IWorkoutService>();
        _controller = new WorkoutController(_workoutServiceMock.Object);

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
    public async Task GetWorkouts_ReturnsOk_WhenWorkoutsExist()
    {
        var queryParams = new WorkoutQueryParams();
        var workouts = new List<WorkoutDto>
        {
            new WorkoutDto { Date = DateTime.Now, Duration = 60, Exercises = new List<WorkoutExerciseDto> { new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 3, Reps = 10 } } }
        };
        _workoutServiceMock.Setup(s => s.GetWorkoutsAsync(It.IsAny<Guid>(), queryParams))
                           .ReturnsAsync(workouts);

        var result = await _controller.GetWorkouts(queryParams);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetWorkouts_ReturnsNotFound_WhenNoWorkoutsExist()
    {
        var queryParams = new WorkoutQueryParams();
        _workoutServiceMock.Setup(s => s.GetWorkoutsAsync(It.IsAny<Guid>(), queryParams))
                           .ReturnsAsync(new List<WorkoutDto>());

        var result = await _controller.GetWorkouts(queryParams);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task SaveWorkout_ReturnsCreated_WhenWorkoutIsSaved()
    {
        var workoutDto = new WorkoutDto { Date = DateTime.Now, Duration = 60, Exercises = new List<WorkoutExerciseDto> { new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 3, Reps = 10 } } };
        _workoutServiceMock.Setup(s => s.CreateWorkoutAsync(It.IsAny<Guid>(), workoutDto, It.IsAny<bool>()))
                           .ReturnsAsync(true);

        var result = await _controller.SaveWorkout(workoutDto);

        Assert.IsType<CreatedAtActionResult>(result);
    }

    [Fact]
    public async Task SaveWorkout_ReturnsBadRequest_WhenWorkoutFailsToSave()
    {
        var workoutDto = new WorkoutDto { Date = DateTime.Now, Duration = 60, Exercises = new List<WorkoutExerciseDto> { new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 3, Reps = 10 } } };
        _workoutServiceMock.Setup(s => s.CreateWorkoutAsync(It.IsAny<Guid>(), workoutDto, It.IsAny<bool>()))
                           .ReturnsAsync(false);

        var result = await _controller.SaveWorkout(workoutDto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task SaveWorkout_ReturnsBadRequest_WhenModelStateIsInvalid()
    {
        var workoutDto = new WorkoutDto { Date = DateTime.Now, Duration = 0, Exercises = new List<WorkoutExerciseDto>() }; // Invalid data
        _controller.ModelState.AddModelError("Duration", "Duration is required and must be greater than 0.");
        _controller.ModelState.AddModelError("Exercises", "At least one exercise is required.");

        var result = await _controller.SaveWorkout(workoutDto);

        Assert.IsType<BadRequestObjectResult>(result);
    }

    [Fact]
    public async Task GetWorkoutByDate_ReturnsOk_WhenWorkoutExistsForDate()
    {
        var date = DateTime.Now.Date;
        var workouts = new List<WorkoutDto>
        {
            new WorkoutDto { Date = date, Duration = 60, Exercises = new List<WorkoutExerciseDto> { new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 3, Reps = 10 } } }
        };
        _workoutServiceMock.Setup(s => s.GetWorkoutsByDateAsync(It.IsAny<Guid>(), date))
                           .ReturnsAsync(workouts);

        var result = await _controller.GetWorkoutByDate(date);

        Assert.IsType<OkObjectResult>(result);
    }

    [Fact]
    public async Task GetWorkoutByDate_ReturnsNotFound_WhenNoWorkoutExistsForDate()
    {
        var date = DateTime.Now.Date;
        _workoutServiceMock.Setup(s => s.GetWorkoutsByDateAsync(It.IsAny<Guid>(), date))
                           .ReturnsAsync(new List<WorkoutDto>());

        var result = await _controller.GetWorkoutByDate(date);

        Assert.IsType<NotFoundObjectResult>(result);
    }

    [Fact]
    public async Task DeleteWorkout_ReturnsNoContent_WhenWorkoutIsDeleted()
    {
        var date = DateTime.Now.Date;
        _workoutServiceMock.Setup(s => s.DeleteWorkoutAsync(It.IsAny<Guid>(), date))
                           .ReturnsAsync(true);

        var result = await _controller.DeleteWorkout(date);

        Assert.IsType<NoContentResult>(result);
    }

    [Fact]
    public async Task DeleteWorkout_ReturnsNotFound_WhenWorkoutNotFoundForDate()
    {
        var date = DateTime.Now.Date;
        _workoutServiceMock.Setup(s => s.DeleteWorkoutAsync(It.IsAny<Guid>(), date))
                           .ReturnsAsync(false);

        var result = await _controller.DeleteWorkout(date);

        Assert.IsType<NotFoundObjectResult>(result);
    }
}
