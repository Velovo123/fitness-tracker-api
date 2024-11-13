using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using AutoMapper;
using Moq;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Xunit;

public class WorkoutServiceTests
{
    private readonly WorkoutService _workoutService;
    private readonly Mock<IWorkoutRepository> _workoutRepositoryMock;
    private readonly Mock<IExerciseService> _exerciseServiceMock;
    private readonly Mock<IMapper> _mapperMock;

    public WorkoutServiceTests()
    {
        _workoutRepositoryMock = new Mock<IWorkoutRepository>();
        _exerciseServiceMock = new Mock<IExerciseService>();
        _mapperMock = new Mock<IMapper>();

        _workoutService = new WorkoutService(
            _workoutRepositoryMock.Object,
            _exerciseServiceMock.Object,
            _mapperMock.Object
        );
    }

    [Fact]
    public async Task CreateWorkoutAsync_ShouldCreateWorkout_WhenNoExistingWorkout()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workoutDto = new WorkoutDto { Date = DateTime.Now, Duration = 45 };
        var workout = new Workout { UserId = userId, Date = workoutDto.Date, Duration = workoutDto.Duration };

        _mapperMock.Setup(m => m.Map<Workout>(workoutDto)).Returns(workout);
        _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateTimeAsync(userId, workoutDto.Date)).ReturnsAsync(new List<Workout>());
        _exerciseServiceMock.Setup(service => service.PrepareExercises<WorkoutExercise>(userId, workoutDto.Exercises)).ReturnsAsync(new List<WorkoutExercise>());
        _workoutRepositoryMock.Setup(repo => repo.CreateWorkoutAsync(workout)).ReturnsAsync(true);

        // Act
        var result = await _workoutService.CreateWorkoutAsync(userId, workoutDto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task CreateWorkoutAsync_ShouldThrowException_WhenWorkoutExistsAndOverwriteFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workoutDto = new WorkoutDto { Date = DateTime.Now, Duration = 45 };

        _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateTimeAsync(userId, workoutDto.Date))
            .ReturnsAsync(new List<Workout> { new Workout() });

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _workoutService.CreateWorkoutAsync(userId, workoutDto, overwrite: false));
    }

    [Fact]
    public async Task UpdateWorkoutAsync_ShouldReplaceWorkout_WhenOverwriteIsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workoutDto = new WorkoutDto { Date = DateTime.Now, Duration = 60 };
        var newWorkout = new Workout { UserId = userId, Date = workoutDto.Date, Duration = workoutDto.Duration };

        _workoutRepositoryMock.Setup(repo => repo.DeleteWorkoutAsync(userId, workoutDto.Date)).ReturnsAsync(true);
        _mapperMock.Setup(m => m.Map<Workout>(workoutDto)).Returns(newWorkout);
        _exerciseServiceMock.Setup(service => service.PrepareExercises<WorkoutExercise>(userId, workoutDto.Exercises))
            .ReturnsAsync(new List<WorkoutExercise>());
        _workoutRepositoryMock.Setup(repo => repo.CreateWorkoutAsync(newWorkout)).ReturnsAsync(true);

        // Act
        var result = await _workoutService.UpdateWorkoutAsync(userId, workoutDto);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task DeleteWorkoutAsync_ShouldReturnTrue_WhenWorkoutDeleted()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.Now;

        _workoutRepositoryMock.Setup(repo => repo.DeleteWorkoutAsync(userId, date)).ReturnsAsync(true);

        // Act
        var result = await _workoutService.DeleteWorkoutAsync(userId, date);

        // Assert
        Assert.True(result);
    }

    [Fact]
    public async Task GetWorkoutsAsync_ShouldReturnMappedWorkouts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var queryParams = new WorkoutQueryParams();
        var workouts = new List<Workout> { new Workout { UserId = userId, Date = DateTime.Now, Duration = 45 } };
        var workoutDtos = new List<WorkoutDto> { new WorkoutDto { Date = DateTime.Now, Duration = 45 } };

        _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(userId, queryParams)).ReturnsAsync(workouts);
        _mapperMock.Setup(m => m.Map<IEnumerable<WorkoutDto>>(workouts)).Returns(workoutDtos);

        // Act
        var result = await _workoutService.GetWorkoutsAsync(userId, queryParams);

        // Assert
        Assert.Equal(workoutDtos, result);
    }

    [Fact]
    public async Task CalculateAverageWorkoutDurationAsync_ShouldThrowException_WhenNoWorkoutsFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _workoutRepositoryMock
            .Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(new List<Workout>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _workoutService.CalculateAverageWorkoutDurationAsync(userId));
    }

    [Fact]
    public async Task CalculateAverageWorkoutDurationAsync_ShouldReturnSingleWorkoutDuration_WhenOneWorkoutExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workouts = new List<Workout> { new Workout { UserId = userId, Duration = 60 } };
        _workoutRepositoryMock
            .Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(workouts);

        // Act
        var result = await _workoutService.CalculateAverageWorkoutDurationAsync(userId);

        // Assert
        Assert.Equal(60, result.AverageWorkoutDuration);
        Assert.Equal(1, result.WorkoutCount);
    }

    [Fact]
    public async Task CalculateAverageWorkoutDurationAsync_ShouldReturnCorrectAverageDuration_WhenMultipleWorkoutsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workouts = new List<Workout>
    {
        new Workout { UserId = userId, Duration = 30 },
        new Workout { UserId = userId, Duration = 60 },
        new Workout { UserId = userId, Duration = 90 }
    };
        _workoutRepositoryMock
            .Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
            .ReturnsAsync(workouts);

        // Act
        var result = await _workoutService.CalculateAverageWorkoutDurationAsync(userId);

        // Assert
        Assert.Equal(60, result.AverageWorkoutDuration); // (30 + 60 + 90) / 3
        Assert.Equal(3, result.WorkoutCount);
    }


}
