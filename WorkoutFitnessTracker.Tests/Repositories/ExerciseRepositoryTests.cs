using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories;
using Xunit;

public class ExerciseRepositoryTests
{
    private readonly WFTDbContext _context;
    private readonly ExerciseRepository _repository;

    public ExerciseRepositoryTests()
    {
        // Set up in-memory database options
        var options = new DbContextOptionsBuilder<WFTDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        _context = new WFTDbContext(options);
        _repository = new ExerciseRepository(_context);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsExercise_WhenExerciseExists()
    {
        // Arrange
        var exercise = new Exercise { Id = Guid.NewGuid(), Name = "Existing Exercise" };
        await _context.Exercises.AddAsync(exercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetByNameAsync("Existing Exercise");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Existing Exercise", result.Name);
    }

    [Fact]
    public async Task GetByNameAsync_ReturnsNull_WhenExerciseDoesNotExist()
    {
        // Act
        var result = await _repository.GetByNameAsync("Nonexistent Exercise");

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_AddsExerciseSuccessfully()
    {
        // Arrange
        var exercise = new Exercise { Id = Guid.NewGuid(), Name = "New Exercise" };

        // Act
        var result = await _repository.AddAsync(exercise);
        var addedExercise = await _context.Exercises.FirstOrDefaultAsync(e => e.Name == "New Exercise");

        // Assert
        Assert.NotNull(result);
        Assert.Equal("New Exercise", result.Name);
        Assert.Equal("New Exercise", addedExercise?.Name);
    }

    [Fact]
    public async Task GetAllExercisesAsync_ReturnsAllExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { Id = Guid.NewGuid(), Name = "Exercise 1" },
            new Exercise { Id = Guid.NewGuid(), Name = "Exercise 2" }
        };
        await _context.Exercises.AddRangeAsync(exercises);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetAllExercisesAsync();

        // Assert
        Assert.Equal(2, result.Count());
        Assert.Contains(result, e => e.Name == "Exercise 1");
        Assert.Contains(result, e => e.Name == "Exercise 2");
    }
    [Fact]
    public async Task GetUserLinkedExercisesAsync_ReturnsLinkedExercises_WhenUserHasLinkedExercises()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exercise1 = new Exercise { Id = Guid.NewGuid(), Name = "Bench Press", PrimaryMuscles = "chest" };
        var exercise2 = new Exercise { Id = Guid.NewGuid(), Name = "Squat", PrimaryMuscles = "legs" };

        var userExercise1 = new UserExercise { UserId = userId, ExerciseId = exercise1.Id, Exercise = exercise1 };
        var userExercise2 = new UserExercise { UserId = userId, ExerciseId = exercise2.Id, Exercise = exercise2 };

        await _context.Exercises.AddRangeAsync(exercise1, exercise2);
        await _context.UserExercises.AddRangeAsync(userExercise1, userExercise2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserLinkedExercisesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, e => e.Name == "Bench Press");
        Assert.Contains(result, e => e.Name == "Squat");
    }

    [Fact]
    public async Task GetUserLinkedExercisesAsync_ReturnsEmpty_WhenUserHasNoLinkedExercises()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Act
        var result = await _repository.GetUserLinkedExercisesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserLinkedExercisesAsync_DoesNotReturnUnlinkedExercises()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var otherUserId = Guid.NewGuid();

        var exercise1 = new Exercise { Id = Guid.NewGuid(), Name = "Deadlift", PrimaryMuscles = "back" };
        var exercise2 = new Exercise { Id = Guid.NewGuid(), Name = "Plank", PrimaryMuscles = "core" };

        var userExercise1 = new UserExercise { UserId = userId, ExerciseId = exercise1.Id, Exercise = exercise1 };
        var otherUserExercise = new UserExercise { UserId = otherUserId, ExerciseId = exercise2.Id, Exercise = exercise2 };

        await _context.Exercises.AddRangeAsync(exercise1, exercise2);
        await _context.UserExercises.AddRangeAsync(userExercise1, otherUserExercise);
        await _context.SaveChangesAsync();

        // Act
        var result = await _repository.GetUserLinkedExercisesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Contains(result, e => e.Name == "Deadlift");
        Assert.DoesNotContain(result, e => e.Name == "Plank");
    }

}
