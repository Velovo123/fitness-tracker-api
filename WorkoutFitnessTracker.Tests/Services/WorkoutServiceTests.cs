using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Services;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTracker.API.Services.IServices;
using AutoMapper;
using System.Collections.Generic;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Mappings;
using WorkoutFitnessTrackerAPI.Services.IServices;

public class WorkoutServiceTests
{
    private readonly WorkoutService _service;
    private readonly Mock<IWorkoutRepository> _repositoryMock;
    private readonly Mock<IExerciseService> _exerciseServiceMock;
    private readonly IMapper _mapper;

    public WorkoutServiceTests()
    {
        _repositoryMock = new Mock<IWorkoutRepository>();
        _exerciseServiceMock = new Mock<IExerciseService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<WorkoutMappingProfile>(); // Use the actual mapping profile
        });
        _mapper = config.CreateMapper();

        _service = new WorkoutService(_repositoryMock.Object, _mapper);
    }

    [Fact]
    public async Task CreateWorkoutAsync_ShouldCreateWorkout_WhenOverwriteIsFalseAndNoConflict()
    {
        var userId = Guid.NewGuid();
        var workoutDto = new WorkoutDto { Date = DateTime.UtcNow, Duration = 60, Exercises = new List<WorkoutExerciseDto>() };

        _repositoryMock.Setup(r => r.GetWorkoutsByDateAsync(userId, workoutDto.Date)).ReturnsAsync(new List<Workout>());
        _repositoryMock.Setup(r => r.CreateWorkoutAsync(It.IsAny<Workout>())).ReturnsAsync(true);

        var result = await _service.CreateWorkoutAsync(userId, workoutDto);

        Assert.True(result);
        _repositoryMock.Verify(r => r.CreateWorkoutAsync(It.IsAny<Workout>()), Times.Once);
    }

    [Fact]
    public async Task CreateWorkoutAsync_ShouldOverwriteWorkout_WhenOverwriteIsTrue()
    {
        var userId = Guid.NewGuid();
        var workoutDto = new WorkoutDto { Date = DateTime.UtcNow, Duration = 60, Exercises = new List<WorkoutExerciseDto>() };

        _repositoryMock.Setup(r => r.GetWorkoutsByDateAsync(userId, workoutDto.Date))
            .ReturnsAsync(new List<Workout> { new Workout { UserId = userId, Date = workoutDto.Date } });
        _repositoryMock.Setup(r => r.UpdateWorkoutAsync(It.IsAny<Workout>())).ReturnsAsync(true);

        var result = await _service.CreateWorkoutAsync(userId, workoutDto, overwrite: true);

        Assert.True(result);
        _repositoryMock.Verify(r => r.UpdateWorkoutAsync(It.IsAny<Workout>()), Times.Once);
    }

    [Fact]
    public async Task GetWorkoutsAsync_ShouldReturnWorkouts_ForUserId()
    {
        var userId = Guid.NewGuid();
        var queryParams = new WorkoutQueryParams();
        var workouts = new List<Workout> { new Workout { Date = DateTime.UtcNow, Duration = 60 }, new Workout { Date = DateTime.UtcNow.AddDays(-1), Duration = 30 } };

        _repositoryMock.Setup(r => r.GetWorkoutsAsync(userId, queryParams)).ReturnsAsync(workouts);

        var result = await _service.GetWorkoutsAsync(userId, queryParams);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task DeleteWorkoutAsync_ShouldReturnTrue_WhenWorkoutExists()
    {
        var userId = Guid.NewGuid();
        var date = DateTime.UtcNow;

        _repositoryMock.Setup(r => r.DeleteWorkoutAsync(userId, date)).ReturnsAsync(true);

        var result = await _service.DeleteWorkoutAsync(userId, date);

        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteWorkoutAsync(userId, date), Times.Once);
    }
}
