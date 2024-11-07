using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Moq;
using WorkoutFitnessTracker.API.Models.Dto_s.Exercise;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services;
using Xunit;

public class ExerciseServiceTests
{
    private readonly ExerciseService _service;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;
    private readonly WFTDbContext _dbContext;

    public ExerciseServiceTests()
    {
        var options = new DbContextOptionsBuilder<WFTDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;
        _dbContext = new WFTDbContext(options);
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        _service = new ExerciseService(_dbContext, _exerciseRepositoryMock.Object);
    }

    [Fact]
    public void NormalizeExerciseNameAsync_ShouldReturnNormalizedName()
    {
        // Arrange
        var exerciseName = " Bench Press ";
        var expectedNormalizedName = NameNormalizationHelper.NormalizeName(exerciseName);

        // Act
        var result = _service.NormalizeExerciseNameAsync(exerciseName).Result;

        // Assert
        Assert.Equal(expectedNormalizedName, result);
    }

    [Fact]
    public async Task PrepareExercises_ShouldPrepareExercises_WhenExercisesAreValid()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exercises = new List<IExerciseDto>
        {
            new WorkoutExerciseDto { ExerciseName = "Bench Press", Sets = 3, Reps = 10 },
            new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 8 }
        };

        // Act
        var result = await _service.PrepareExercises<WorkoutExercise>(userId, exercises);

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetExerciseByNormalizedNameAsync_ShouldReturnExercise_WhenExerciseExists()
    {
        // Arrange
        var exerciseName = "Bench Press";
        var normalizedExerciseName = NameNormalizationHelper.NormalizeName(exerciseName);
        var exercise = new Exercise { Id = Guid.NewGuid(), Name = normalizedExerciseName };

        _exerciseRepositoryMock.Setup(r => r.GetByNameAsync(normalizedExerciseName))
            .ReturnsAsync(exercise);

        // Act
        var result = await _service.GetExerciseByNormalizedNameAsync(exerciseName);

        // Assert
        Assert.Equal(exercise.Id, result.Id);
        Assert.Equal(exercise.Name, result.Name);
    }

    [Fact]
    public async Task EnsureUserExerciseLinkAsync_ShouldReturnTrue_WhenLinkDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();

        // Act
        var result = await _service.EnsureUserExerciseLinkAsync(userId, exerciseId);

        // Assert
        Assert.True(result);
        Assert.Single(_dbContext.UserExercises.Where(ue => ue.UserId == userId && ue.ExerciseId == exerciseId));
    }

    [Fact]
    public async Task EnsureUserExerciseLinkAsync_ShouldReturnFalse_WhenLinkExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exerciseId = Guid.NewGuid();

        _dbContext.UserExercises.Add(new UserExercise { UserId = userId, ExerciseId = exerciseId });
        await _dbContext.SaveChangesAsync();

        // Act
        var result = await _service.EnsureUserExerciseLinkAsync(userId, exerciseId);

        // Assert
        Assert.False(result);
        Assert.Single(_dbContext.UserExercises.Where(ue => ue.UserId == userId && ue.ExerciseId == exerciseId));
    }

    [Fact]
    public async Task GetAllExercisesAsync_ShouldReturnAllExercises()
    {
        // Arrange
        var exercises = new List<Exercise>
        {
            new Exercise { Id = Guid.NewGuid(), Name = "Bench Press", Type = "Strength" },
            new Exercise { Id = Guid.NewGuid(), Name = "Squat", Type = "Strength" }
        };
        _exerciseRepositoryMock.Setup(r => r.GetAllExercisesAsync()).ReturnsAsync(exercises);

        // Act
        var result = await _service.GetAllExercisesAsync();

        // Assert
        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task AddExerciseAsync_ShouldAddExercise_WhenExerciseIsValid()
    {
        // Arrange
        var exerciseDto = new ExerciseDto { Name = "Deadlift", Type = "Strength" };
        var normalizedExerciseName = NameNormalizationHelper.NormalizeName(exerciseDto.Name);

        var exercise = new Exercise
        {
            Id = Guid.NewGuid(),
            Name = normalizedExerciseName,
            Type = exerciseDto.Type
        };
        _exerciseRepositoryMock.Setup(r => r.AddAsync(It.IsAny<Exercise>())).ReturnsAsync(exercise);

        // Act
        var result = await _service.AddExerciseAsync(exerciseDto);

        // Assert
        Assert.Equal(normalizedExerciseName, result.Name);
        Assert.Equal(exerciseDto.Type, result.Type);
    }
}
