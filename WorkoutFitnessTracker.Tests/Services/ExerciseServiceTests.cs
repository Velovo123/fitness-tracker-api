using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Moq;
using WorkoutFitnessTracker.API.Models.Dto_s.Exercise;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services;
using Xunit;

public class ExerciseServiceTests
{
    private readonly ExerciseService _service;
    private readonly Mock<IExerciseRepository> _exerciseRepositoryMock;

    public ExerciseServiceTests()
    {
        _exerciseRepositoryMock = new Mock<IExerciseRepository>();
        _service = new ExerciseService(_exerciseRepositoryMock.Object);

        ResetCache();
    }

    private void ResetCache()
    {
        typeof(ExerciseService)
            .GetField("_exerciseCache", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, null);

        typeof(ExerciseService)
            .GetField("_cacheExpirationTime", System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)
            ?.SetValue(null, DateTime.MinValue);
    }

    [Fact]
    public void NormalizeExerciseNameAsync_ShouldReturnNormalizedName()
    {
        var exerciseName = " Bench Press ";
        var expectedNormalizedName = NameNormalizationHelper.NormalizeName(exerciseName);

        var result = _service.NormalizeExerciseNameAsync(exerciseName).Result;

        Assert.Equal(expectedNormalizedName, result);
    }

    [Fact]
    public async Task PrepareExercises_ShouldPrepareExercises_WhenExercisesAreValid()
    {
        var userId = Guid.NewGuid();
        var exercises = new List<IExerciseDto>
        {
            new WorkoutExerciseDto { ExerciseName = "Bench Press", Sets = 3, Reps = 10 },
            new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 8 }
        };

        // Mock GetOrCreateExerciseAsync behavior
        _exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => new Exercise { Id = Guid.NewGuid(), Name = name });

        var result = await _service.PrepareExercises<WorkoutExercise>(userId, exercises);

        Assert.Equal(2, result.Count);
    }



    [Fact]
    public async Task PrepareExercises_ShouldRemoveDuplicateExercises_WhenExercisesHaveSameNameSetsAndReps()
    {
        var userId = Guid.NewGuid();
        var squatId = Guid.NewGuid();
        var benchPressId = Guid.NewGuid();

        var exercises = new List<IExerciseDto>
    {
        new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 3, Reps = 10 },
        new WorkoutExerciseDto { ExerciseName = "squat", Sets = 3, Reps = 10 },
        new WorkoutExerciseDto { ExerciseName = "Bench Press", Sets = 4, Reps = 8 }
    };

        // Set up the mock repository to return an exercise with consistent IDs based on the normalized name
        _exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.Is<string>(name => name.Equals("squat", StringComparison.OrdinalIgnoreCase))))
            .ReturnsAsync(new Exercise { Id = squatId, Name = "squat" });

        _exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.Is<string>(name => name.Equals("benchpress", StringComparison.OrdinalIgnoreCase))))
            .ReturnsAsync(new Exercise { Id = benchPressId, Name = "bench press" });

        // Act
        var result = await _service.PrepareExercises<WorkoutExercise>(userId, exercises);

        // Assert
        Assert.Equal(2, result.Count); // Expecting two unique exercises: Squat and Bench Press
    }


    [Fact]
    public async Task PrepareExercises_ShouldAllowSameExerciseWithDifferentSetsOrReps()
    {
        var userId = Guid.NewGuid();
        var exercises = new List<IExerciseDto>
        {
            new WorkoutExerciseDto { ExerciseName = "Push-up", Sets = 3, Reps = 15 },
            new WorkoutExerciseDto { ExerciseName = "pushup", Sets = 4, Reps = 15 },
            new WorkoutExerciseDto { ExerciseName = "pushup", Sets = 3, Reps = 10 }
        };

        _exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => new Exercise { Id = Guid.NewGuid(), Name = name });

        var result = await _service.PrepareExercises<WorkoutExercise>(userId, exercises);

        Assert.Equal(3, result.Count);
    }

    [Fact]
    public async Task PrepareExercises_ShouldNormalizeExerciseNames_WhenNamesAreInconsistent()
    {
        var userId = Guid.NewGuid();
        var normalizedExerciseName = "benchpress"; // Expected normalized name based on the normalization logic.
        var exerciseId = Guid.NewGuid();

        var exercises = new List<IExerciseDto>
        {
            new WorkoutExerciseDto { ExerciseName = "Bench Press", Sets = 3, Reps = 10 },
            new WorkoutExerciseDto { ExerciseName = "bench press", Sets = 3, Reps = 10 }
        };

        // Set up the mock to return the same Exercise object for normalized names
        _exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(normalizedExerciseName))
            .ReturnsAsync(new Exercise { Id = exerciseId, Name = "Bench Press" });

        // Act
        var result = await _service.PrepareExercises<WorkoutExercise>(userId, exercises);

        // Assert
        Assert.Single(result); // Expecting only one unique entry
    }

    [Fact]
    public async Task PrepareExercises_ShouldLinkUserToEachUniqueExercise()
    {
        var userId = Guid.NewGuid();
        var exercises = new List<IExerciseDto>
        {
            new WorkoutExerciseDto { ExerciseName = "Pull-up", Sets = 3, Reps = 10 },
            new WorkoutExerciseDto { ExerciseName = "Pull-up", Sets = 4, Reps = 8 }
        };

        _exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => new Exercise { Id = Guid.NewGuid(), Name = name });

        var result = await _service.PrepareExercises<WorkoutExercise>(userId, exercises);

        _exerciseRepositoryMock.Verify(repo => repo.EnsureUserExerciseLinkAsync(userId, It.IsAny<Guid>()), Times.Exactly(2));
    }

    [Fact]
    public async Task GetUserLinkedExercisesAsync_ShouldReturnLinkedExercises_WhenUserHasLinkedExercises()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exercise1 = new Exercise { Id = Guid.NewGuid(), Name = "Bench Press", PrimaryMuscles = "chest" };
        var exercise2 = new Exercise { Id = Guid.NewGuid(), Name = "Squat", PrimaryMuscles = "legs" };

        var linkedExercises = new List<Exercise> { exercise1, exercise2 };

        // Set up the mock repository
        _exerciseRepositoryMock
            .Setup(repo => repo.GetUserLinkedExercisesAsync(userId))
            .ReturnsAsync(linkedExercises);

        // Act
        var result = await _service.GetUserLinkedExercisesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count());
        Assert.Contains(result, e => e.Name == "Bench Press");
        Assert.Contains(result, e => e.Name == "Squat");
    }

    [Fact]
    public async Task GetUserLinkedExercisesAsync_ShouldReturnEmpty_WhenUserHasNoLinkedExercises()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Set up the mock repository to return an empty list
        _exerciseRepositoryMock
            .Setup(repo => repo.GetUserLinkedExercisesAsync(userId))
            .ReturnsAsync(new List<Exercise>());

        // Act
        var result = await _service.GetUserLinkedExercisesAsync(userId);

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetUserLinkedExercisesAsync_ShouldNotReturnNull()
    {
        // Arrange
        var userId = Guid.NewGuid();

        // Ensure the mock repository returns an empty list rather than null
        _exerciseRepositoryMock
            .Setup(repo => repo.GetUserLinkedExercisesAsync(userId))
            .ReturnsAsync(Enumerable.Empty<Exercise>());

        // Act
        var result = await _service.GetUserLinkedExercisesAsync(userId);

        // Assert
        Assert.NotNull(result); // Ensure the result is not null
    }

    [Fact]
    public async Task PrepareExercises_ShouldSuggestExercises_WhenExactMatchNotFound()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var exercises = new List<IExerciseDto>
    {
        new WorkoutExerciseDto { ExerciseName = "Push Ups", Sets = 3, Reps = 12 }
    };

        // Mock the repository for fuzzy matching
        _exerciseRepositoryMock
            .Setup(repo => repo.GetAllExerciseNamesAsync())
            .ReturnsAsync(new List<string> { "Push-ups", "Bench Press", "Squats" });

        _exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((Exercise)null);

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.PrepareExercises<WorkoutExercise>(userId, exercises));

        Assert.Contains("Push Ups", exception.Message);
        Assert.Contains("Push-ups", exception.Message); // Suggested alternative
    }

    [Fact]
    public async Task SuggestExercises_ShouldRefreshCache_WhenCacheExpires()
    {
        // Arrange
        var exerciseNames = new List<string> { "Bench Press", "Push-ups", "Squats" };
        _exerciseRepositoryMock
            .SetupSequence(repo => repo.GetAllExerciseNamesAsync())
            .ReturnsAsync(exerciseNames) // Return valid cache initially
            .ReturnsAsync(new List<string>()); // Simulate cache refresh with no data

        var exercises = new List<IExerciseDto>
    {
        new WorkoutExerciseDto { ExerciseName = "bench prs", Sets = 3, Reps = 10 }
    };

        var userId = Guid.NewGuid();

        // Act & Assert
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await _service.PrepareExercises<WorkoutExercise>(userId, exercises));

        Assert.Contains("Exercise 'bench prs' not found. Did you mean: Bench Press, Push-ups, Squats?", exception.Message);
    }

    [Fact]
    public async Task SuggestExercises_ShouldReturnTopMatches_WhenInputIsProvided()
    {
        // Arrange
        var input = "bench prs";
        var exerciseNames = new List<string> { "Bench Press", "Push-ups", "Squats" };

        _exerciseRepositoryMock
            .Setup(repo => repo.GetAllExerciseNamesAsync())
            .ReturnsAsync(exerciseNames);

        _exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((Exercise)null);

        // Act
        var exception = await Assert.ThrowsAsync<InvalidOperationException>(() =>
            _service.PrepareExercises<WorkoutExercise>(Guid.NewGuid(), new List<IExerciseDto> {
            new WorkoutExerciseDto { ExerciseName = input, Sets = 3, Reps = 10 }
            }));

        // Assert
        Assert.Contains("Exercise 'bench prs' not found.", exception.Message);
        Assert.Contains("Bench Press", exception.Message);
        Assert.Contains("Push-ups", exception.Message);
        Assert.Contains("Squats", exception.Message);
    }

    [Fact]
    public async Task PrepareExercises_ShouldReturnEmptyList_WhenInputIsEmpty()
    {
        var userId = Guid.NewGuid();
        var exercises = new List<IExerciseDto>();

        var result = await _service.PrepareExercises<WorkoutExercise>(userId, exercises);   

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllExercisesAsync_ShouldThrowException_WhenRepositoryFails()
    {
        // Arrange
        _exerciseRepositoryMock
            .Setup(repo => repo.GetAllExercisesAsync())
            .ThrowsAsync(new Exception("Repository failure"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => _service.GetAllExercisesAsync());
    }




}
