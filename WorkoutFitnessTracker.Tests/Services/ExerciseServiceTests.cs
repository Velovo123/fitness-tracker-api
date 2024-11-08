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
        _service = new ExerciseService(_dbContext);
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

        var result = await _service.PrepareExercises<WorkoutExercise>(userId, exercises);

        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task PrepareExercises_ShouldRemoveDuplicateExercises_WhenExercisesHaveSameNameSetsAndReps()
    {
        var userId = Guid.NewGuid();
        var exercises = new List<IExerciseDto>
        {
            new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 3, Reps = 10 },
            new WorkoutExerciseDto { ExerciseName = "squat", Sets = 3, Reps = 10 },
            new WorkoutExerciseDto { ExerciseName = "Bench Press", Sets = 4, Reps = 8 }
        };

        _exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => new Exercise { Id = Guid.NewGuid(), Name = name });

        var result = await _service.PrepareExercises<WorkoutExercise>(userId, exercises);

        Assert.Equal(2, result.Count);
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
        var exercises = new List<IExerciseDto>
        {
            new WorkoutExerciseDto { ExerciseName = "Bench Press", Sets = 3, Reps = 10 },
            new WorkoutExerciseDto { ExerciseName = "bench press", Sets = 3, Reps = 10 }
        };

        _exerciseRepositoryMock
            .Setup(repo => repo.GetByNameAsync(It.IsAny<string>()))
            .ReturnsAsync((string name) => new Exercise { Id = Guid.NewGuid(), Name = name });

        var result = await _service.PrepareExercises<WorkoutExercise>(userId, exercises);

        Assert.Single(result);
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

        foreach (var exercise in result)
        {
            Assert.Contains(_dbContext.UserExercises, ue => ue.UserId == userId && ue.ExerciseId == ((WorkoutExercise)exercise).ExerciseId);
        }
    }
}
