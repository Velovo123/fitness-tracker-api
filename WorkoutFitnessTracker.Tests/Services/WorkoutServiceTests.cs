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

    [Fact]
    public async Task GetMostFrequentExercisesAsync_ReturnsTopExercises_WithCorrectFrequencies()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workouts = new List<Workout>
        {
            new Workout
            {
                UserId = userId,
                WorkoutExercises = new List<WorkoutExercise>
                {
                    new WorkoutExercise { Exercise = new Exercise { Name = "Bench Press" } },
                    new WorkoutExercise { Exercise = new Exercise { Name = "Bench Press" } },
                    new WorkoutExercise { Exercise = new Exercise { Name = "Squat" } },
                }
            },
            new Workout
            {
                UserId = userId,
                WorkoutExercises = new List<WorkoutExercise>
                {
                    new WorkoutExercise { Exercise = new Exercise { Name = "Bench Press" } },
                    new WorkoutExercise { Exercise = new Exercise { Name = "Deadlift" } },
                }
            }
        };

        _workoutRepositoryMock
            .Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, null, null))
            .ReturnsAsync(workouts);

        // Act
        var result = await _workoutService.GetMostFrequentExercisesAsync(userId);

        // Assert
        Assert.Equal(3, result.Count);
        Assert.Equal("Bench Press", result[0].ExerciseName);
        Assert.Equal(3, result[0].Frequency);
        Assert.Equal("Squat", result[1].ExerciseName);
        Assert.Equal(1, result[1].Frequency);
        Assert.Equal("Deadlift", result[2].ExerciseName);
        Assert.Equal(1, result[2].Frequency);
    }

    [Fact]
    public async Task GetMostFrequentExercisesAsync_ReturnsTopFiveExercises_WhenMoreThanFiveExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workouts = new List<Workout>
        {
            new Workout
            {
                UserId = userId,
                WorkoutExercises = new List<WorkoutExercise>
                {
                    new WorkoutExercise { Exercise = new Exercise { Name = "Bench Press" } },
                    new WorkoutExercise { Exercise = new Exercise { Name = "Squat" } },
                    new WorkoutExercise { Exercise = new Exercise { Name = "Deadlift" } },
                    new WorkoutExercise { Exercise = new Exercise { Name = "Pull Up" } },
                    new WorkoutExercise { Exercise = new Exercise { Name = "Push Up" } },
                    new WorkoutExercise { Exercise = new Exercise { Name = "Overhead Press" } },
                }
            }
        };

        _workoutRepositoryMock
            .Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, null, null))
            .ReturnsAsync(workouts);

        // Act
        var result = await _workoutService.GetMostFrequentExercisesAsync(userId);

        // Assert
        Assert.Equal(5, result.Count);
    }

    [Fact]
    public async Task GetMostFrequentExercisesAsync_ReturnsEmptyList_WhenNoWorkoutsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _workoutRepositoryMock
            .Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, null, null))
            .ReturnsAsync(new List<Workout>());

        // Act
        var result = await _workoutService.GetMostFrequentExercisesAsync(userId);

        // Assert
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetWeeklyMonthlySummaryAsync_ShouldThrowException_WhenNoWorkoutsFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        _workoutRepositoryMock.Setup(r => r.GetWorkoutsByDateRangeAsync(userId, startDate, endDate))
                              .ReturnsAsync(new List<Workout>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _workoutService.GetWeeklyMonthlySummaryAsync(userId, startDate, endDate));
    }

    [Fact]
    public async Task GetWeeklyMonthlySummaryAsync_ShouldReturnCorrectSummary_WhenWorkoutsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-30);
        var endDate = DateTime.UtcNow;

        var workouts = new List<Workout>
        {
            new Workout
            {
                Duration = 60,
                WorkoutExercises = new List<WorkoutExercise>
                {
                    new WorkoutExercise { Reps = 10, Sets = 3 },
                    new WorkoutExercise { Reps = 8, Sets = 3 }
                }
            },
            new Workout
            {
                Duration = 45,
                WorkoutExercises = new List<WorkoutExercise>
                {
                    new WorkoutExercise { Reps = 12, Sets = 4 }
                }
            }
        };

        _workoutRepositoryMock.Setup(r => r.GetWorkoutsByDateRangeAsync(userId, startDate, endDate))
                              .ReturnsAsync(workouts);

        // Act
        var result = await _workoutService.GetWeeklyMonthlySummaryAsync(userId, startDate, endDate);

        // Assert
        Assert.Equal(2, result.TotalWorkouts);
        Assert.Equal(52.5, result.AverageDuration);
        Assert.Equal(30, result.TotalReps);
        Assert.Equal(10, result.TotalSets);
        Assert.Equal(startDate, result.StartDate);
        Assert.Equal(endDate, result.EndDate);
    }

    [Fact]
    public async Task GetWeeklyMonthlyComparisonAsync_ShouldThrowException_WhenNoWorkoutsFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;

        _workoutRepositoryMock
            .Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, startDate, endDate))
            .ReturnsAsync(new List<Workout>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _workoutService.GetWeeklyMonthlyComparisonAsync(userId, startDate, endDate, "weekly"));
    }

    [Fact]
    public async Task GetWeeklyMonthlyComparisonAsync_ShouldReturnWeeklyData_WhenIntervalIsWeekly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-14);
        var endDate = DateTime.UtcNow;

        var workouts = new List<Workout>
        {
            new Workout { Date = startDate.AddDays(1), Duration = 30, WorkoutExercises = new List<WorkoutExercise> { new WorkoutExercise { Sets = 3, Reps = 10 } } },
            new Workout { Date = startDate.AddDays(7), Duration = 45, WorkoutExercises = new List<WorkoutExercise> { new WorkoutExercise { Sets = 4, Reps = 12 } } }
        };

        _workoutRepositoryMock
            .Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, startDate, endDate))
            .ReturnsAsync(workouts);

        // Act
        var result = await _workoutService.GetWeeklyMonthlyComparisonAsync(userId, startDate, endDate, "weekly");

        // Assert
        Assert.Equal(2, result.Count); // Expect two weekly groups
        Assert.Equal(30, result[0].AverageDuration);
        Assert.Equal(45, result[1].AverageDuration);
    }

    [Fact]
    public async Task GetWeeklyMonthlyComparisonAsync_ShouldReturnMonthlyData_WhenIntervalIsMonthly()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var endDate = startDate.AddMonths(1).AddDays(-1);

        var workouts = new List<Workout>
        {
            new Workout { Date = startDate.AddDays(1), Duration = 30, WorkoutExercises = new List<WorkoutExercise> { new WorkoutExercise { Sets = 3, Reps = 10 } } },
            new Workout { Date = startDate.AddDays(15), Duration = 45, WorkoutExercises = new List<WorkoutExercise> { new WorkoutExercise { Sets = 4, Reps = 12 } } }
        };

        _workoutRepositoryMock
            .Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, startDate, endDate))
            .ReturnsAsync(workouts);

        // Act
        var result = await _workoutService.GetWeeklyMonthlyComparisonAsync(userId, startDate, endDate, "monthly");

        // Assert
        Assert.Single(result); // Expect a single monthly group
        Assert.Equal(2, result[0].TotalWorkouts);
        Assert.Equal(37.5, result[0].AverageDuration);
        Assert.Equal(22, result[0].TotalReps);
        Assert.Equal(7, result[0].TotalSets);
    }

    [Fact]
    public async Task GetWeeklyMonthlyComparisonAsync_ShouldThrowException_WhenInvalidIntervalType()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddMonths(-1);
        var endDate = DateTime.UtcNow;

        _workoutRepositoryMock
            .Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, startDate, endDate))
            .ReturnsAsync(new List<Workout>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _workoutService.GetWeeklyMonthlyComparisonAsync(userId, startDate, endDate, "daily"));
    }

}
