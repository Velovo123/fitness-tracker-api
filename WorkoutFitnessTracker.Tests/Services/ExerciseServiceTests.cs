using System;
using System.Collections.Generic;
using System.Linq;
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
}
