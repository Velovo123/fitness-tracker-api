using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Moq;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Xunit;

public class InsightsServiceTests
{
    private readonly InsightsService _insightsService;
    private readonly Mock<IWorkoutRepository> _workoutRepositoryMock;
    private readonly Mock<IWorkoutPlanRepository> _workoutPlanRepositoryMock;
    private readonly Mock<IProgressRecordRepository> _progressRecordRepositoryMock;
    private readonly Mock<IExerciseService> _exerciseServiceMock;

    public InsightsServiceTests()
    {
        _workoutRepositoryMock = new Mock<IWorkoutRepository>();
        _workoutPlanRepositoryMock = new Mock<IWorkoutPlanRepository>();
        _progressRecordRepositoryMock = new Mock<IProgressRecordRepository>();
        _exerciseServiceMock = new Mock<IExerciseService>();

        _insightsService = new InsightsService(
            _workoutRepositoryMock.Object,
            _workoutPlanRepositoryMock.Object,
            _progressRecordRepositoryMock.Object,
            _exerciseServiceMock.Object
        );
    }

    [Fact]
    public async Task CalculateAverageWorkoutDurationAsync_ShouldThrowException_WhenNoWorkoutsFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, null, null))
                              .ReturnsAsync(new List<Workout>());

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _insightsService.CalculateAverageWorkoutDurationAsync(userId));
    }

    [Fact]
    public async Task CalculateAverageWorkoutDurationAsync_ShouldReturnAverage_WhenWorkoutsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workouts = new List<Workout>
        {
            new Workout { Duration = 30 },
            new Workout { Duration = 60 },
            new Workout { Duration = 90 }
        };
        _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, null, null))
                              .ReturnsAsync(workouts);

        // Act
        var result = await _insightsService.CalculateAverageWorkoutDurationAsync(userId);

        // Assert
        Assert.Equal(60, result.AverageWorkoutDuration);
        Assert.Equal(3, result.WorkoutCount);
    }

    [Fact]
    public async Task GetMostFrequentExercisesAsync_ShouldReturnTopExercises()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workouts = new List<Workout>
        {
            new Workout
            {
                WorkoutExercises = new List<WorkoutExercise>
                {
                    new WorkoutExercise { Exercise = new Exercise { Name = "Bench Press" } },
                    new WorkoutExercise { Exercise = new Exercise { Name = "Squat" } }
                }
            },
            new Workout
            {
                WorkoutExercises = new List<WorkoutExercise>
                {
                    new WorkoutExercise { Exercise = new Exercise { Name = "Bench Press" } }
                }
            }
        };
        _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, null, null))
                              .ReturnsAsync(workouts);

        // Act
        var result = await _insightsService.GetMostFrequentExercisesAsync(userId);

        // Assert
        Assert.Equal("Bench Press", result[0].ExerciseName);
        Assert.Equal(2, result[0].Frequency);
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
                    new WorkoutExercise { Sets = 3, Reps = 10 },
                    new WorkoutExercise { Sets = 4, Reps = 12 }
                }
            }
        };
        _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateRangeAsync(userId, startDate, endDate))
                              .ReturnsAsync(workouts);

        // Act
        var result = await _insightsService.GetWeeklyMonthlySummaryAsync(userId, startDate, endDate);

        // Assert
        Assert.Equal(1, result.TotalWorkouts);
        Assert.Equal(60, result.AverageDuration);
        Assert.Equal(22, result.TotalReps);
        Assert.Equal(7, result.TotalSets);
    }

    [Fact]
    public async Task GetExerciseProgressTrendAsync_ShouldReturnTrendData_WhenProgressRecordsExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exerciseName = "Bench Press";
        var exercise = new Exercise { Id = Guid.NewGuid(), Name = exerciseName };
        var progressRecords = new List<ProgressRecord>
        {
            new ProgressRecord { Date = DateTime.UtcNow.AddDays(-2), Progress = "10 reps" },
            new ProgressRecord { Date = DateTime.UtcNow.AddDays(-1), Progress = "12 reps" }
        };

        _exerciseServiceMock.Setup(service => service.GetExerciseByNormalizedNameAsync(exerciseName)).ReturnsAsync(exercise);
        _exerciseServiceMock.Setup(service => service.EnsureUserExerciseLinkAsync(userId, exercise.Id)).Returns(Task.FromResult(true));
        _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordsByDateRangeAsync(userId, exercise.Id, null, null))
                                     .ReturnsAsync(progressRecords);

        // Act
        var result = await _insightsService.GetExerciseProgressTrendAsync(userId, exerciseName);

        // Assert
        Assert.Equal(2, result.Count);
        Assert.Equal("10 reps", result[0].Progress);
        Assert.Equal("12 reps", result[1].Progress);
    }

    [Fact]
    public async Task GetDailyProgressAsync_ShouldReturnCorrectData_WhenWorkoutsAndPlansExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.UtcNow;
        var workouts = new List<Workout>
        {
            new Workout
            {
                WorkoutExercises = new List<WorkoutExercise>
                {
                    new WorkoutExercise { Exercise = new Exercise { Name = "Bench Press" }, Sets = 3, Reps = 10 }
                },
                Duration = 45
            }
        };
        var workoutPlans = new List<WorkoutPlan>
        {
            new WorkoutPlan
            {
                WorkoutPlanExercises = new List<WorkoutPlanExercise>
                {
                    new WorkoutPlanExercise { Exercise = new Exercise { Name = "Bench Press" }, Sets = 3, Reps = 10 }
                }
            }
        };
        _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateTimeAsync(userId, date)).ReturnsAsync(workouts);
        _workoutPlanRepositoryMock.Setup(repo => repo.GetWorkoutPlansForDateAsync(userId, date)).ReturnsAsync(workoutPlans);

        // Act
        var result = await _insightsService.GetDailyProgressAsync(userId, date);

        // Assert
        Assert.Equal(1, result.TotalWorkouts);
        Assert.Equal(45, result.AverageDuration);
        Assert.Equal(10, result.TotalReps);
        Assert.Equal(3, result.TotalSets);
        Assert.Single(result.Exercises);
        Assert.Equal("Bench Press", result.Exercises[0].ExerciseName);
        Assert.Equal(3, result.Exercises[0].PlannedSets);
        Assert.Equal(10, result.Exercises[0].PlannedReps);
        Assert.Equal(3, result.Exercises[0].ActualSets);
        Assert.Equal(10, result.Exercises[0].ActualReps);
    }
}
