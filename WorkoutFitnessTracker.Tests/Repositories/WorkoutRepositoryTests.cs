using Xunit;
using Moq;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

public class WorkoutRepositoryTests
{
    private readonly WFTDbContext _context;
    private readonly WorkoutRepository _workoutRepository;

    public WorkoutRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<WFTDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new WFTDbContext(options);

        _workoutRepository = new WorkoutRepository(_context);
    }

    [Fact]
    public async Task GetWorkoutsAsync_ShouldReturnFilteredWorkouts()
    {
        // Arrange
        var userId = Guid.NewGuid();
        _context.Workouts.AddRange(new List<Workout>
        {
            new Workout { UserId = userId, Date = DateTime.UtcNow.AddDays(-1), Duration = 30 },
            new Workout { UserId = userId, Date = DateTime.UtcNow, Duration = 45 }
        });
        await _context.SaveChangesAsync();

        var queryParams = new WorkoutQueryParams { MinDuration = 40 };

        // Act
        var result = await _workoutRepository.GetWorkoutsAsync(userId, queryParams);

        // Assert
        Assert.Single(result);
        Assert.Equal(45, result.First().Duration);
    }

    [Fact]
    public async Task CreateWorkoutAsync_ShouldAddWorkoutToDatabase()
    {
        // Arrange
        var workout = new Workout { UserId = Guid.NewGuid(), Date = DateTime.UtcNow, Duration = 60 };

        // Act
        var result = await _workoutRepository.CreateWorkoutAsync(workout);

        // Assert
        Assert.True(result);
        Assert.Equal(1, _context.Workouts.Count());
    }

    [Fact]
    public async Task UpdateWorkoutAsync_ShouldModifyExistingWorkout()
    {
        // Arrange
        var workout = new Workout { UserId = Guid.NewGuid(), Date = DateTime.UtcNow, Duration = 60 };
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        workout.Duration = 75;

        // Act
        var result = await _workoutRepository.UpdateWorkoutAsync(workout);

        // Assert
        Assert.True(result);
        Assert.Equal(75, _context.Workouts.First().Duration);
    }

    [Fact]
    public async Task DeleteWorkoutAsync_ShouldRemoveWorkoutFromDatabase()
    {
        // Arrange
        var workout = new Workout { UserId = Guid.NewGuid(), Date = DateTime.UtcNow, Duration = 60 };
        _context.Workouts.Add(workout);
        await _context.SaveChangesAsync();

        // Act
        var result = await _workoutRepository.DeleteWorkoutAsync(workout.UserId, workout.Date);

        // Assert
        Assert.True(result);
        Assert.Empty(_context.Workouts);
    }

    [Fact]
    public async Task GetWorkoutsByDateRangeAsync_ShouldReturnWorkoutsWithinDateRange()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var startDate = DateTime.UtcNow.AddDays(-3);
        var endDate = DateTime.UtcNow.AddDays(-1);

        _context.Workouts.AddRange(new List<Workout>
    {
        new Workout { UserId = userId, Date = DateTime.UtcNow.AddDays(-4), Duration = 30 }, // Outside range
        new Workout { UserId = userId, Date = DateTime.UtcNow.AddDays(-2), Duration = 45 }, // Inside range
        new Workout { UserId = userId, Date = DateTime.UtcNow, Duration = 60 }               // Outside range
    });
        await _context.SaveChangesAsync();

        // Act
        var result = await _workoutRepository.GetWorkoutsByDateRangeAsync(userId, startDate, endDate);

        // Assert
        Assert.Single(result);
        Assert.Equal(45, result.First().Duration);
        Assert.Equal(userId, result.First().UserId);
    }

}
