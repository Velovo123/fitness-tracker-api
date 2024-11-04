using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;
using WorkoutFitnessTrackerAPI.Services;
using Xunit;

namespace WorkoutFitnessTrackerAPI.Tests.Services
{
    public class ExerciseServiceTests
    {
        private readonly WFTDbContext _dbContext;
        private readonly ExerciseService _exerciseService;
        private readonly Guid _userGuid;

        public ExerciseServiceTests()
        {
            // Configure DbContextOptions to use InMemory database
            var options = new DbContextOptionsBuilder<WFTDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            // Initialize DbContext and Service
            _dbContext = new WFTDbContext(options);
            _exerciseService = new ExerciseService(_dbContext);
            _userGuid = Guid.NewGuid();
        }

        [Fact]
        public async Task PrepareExercises_ShouldReturnCorrectlyPreparedWorkoutExercises()
        {
            // Arrange
            var exercises = new List<WorkoutExerciseDto>
            {
                new WorkoutExerciseDto { ExerciseName = "Bench Press", Sets = 3, Reps = 10 },
                new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 12 }
            };

            // Act
            var result = await _exerciseService.PrepareExercises<WorkoutExercise>(_userGuid, exercises);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, ex =>
            {
                Assert.NotEqual(Guid.Empty, ex.ExerciseId);
                Assert.True(ex.Sets > 0);
                Assert.True(ex.Reps > 0);
            });
        }

        [Fact]
        public async Task GetExerciseByNormalizedNameAsync_ShouldReturnExercise_WhenFound()
        {
            // Arrange
            var exerciseName = "Squat";
            var normalizedExerciseName = NameNormalizationHelper.NormalizeName(exerciseName);
            var exercise = new Exercise { Id = Guid.NewGuid(), Name = normalizedExerciseName };
            await _dbContext.Exercises.AddAsync(exercise);
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _exerciseService.GetExerciseByNormalizedNameAsync(exerciseName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(normalizedExerciseName, result.Name);
        }

        [Fact]
        public async Task GetExerciseByNormalizedNameAsync_ShouldReturnNull_WhenNotFound()
        {
            // Arrange
            var exerciseName = "Nonexistent Exercise";

            // Act
            var result = await _exerciseService.GetExerciseByNormalizedNameAsync(exerciseName);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task EnsureUserExerciseLinkAsync_ShouldCreateLink_WhenLinkDoesNotExist()
        {
            // Arrange
            var exerciseId = Guid.NewGuid();

            // Act
            var result = await _exerciseService.EnsureUserExerciseLinkAsync(_userGuid, exerciseId);

            // Assert
            Assert.True(result);
            var userExerciseLink = await _dbContext.UserExercises
                .FirstOrDefaultAsync(ue => ue.UserId == _userGuid && ue.ExerciseId == exerciseId);
            Assert.NotNull(userExerciseLink);
        }

        [Fact]
        public async Task EnsureUserExerciseLinkAsync_ShouldNotCreateLink_WhenLinkExists()
        {
            // Arrange
            var exerciseId = Guid.NewGuid();
            _dbContext.UserExercises.Add(new UserExercise
            {
                UserId = _userGuid,
                ExerciseId = exerciseId
            });
            await _dbContext.SaveChangesAsync();

            // Act
            var result = await _exerciseService.EnsureUserExerciseLinkAsync(_userGuid, exerciseId);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task PrepareExercises_ShouldLinkUserWithExercisesAndReturnCorrectlyPreparedExercises()
        {
            // Arrange
            var exercises = new List<WorkoutExerciseDto>
            {
                new WorkoutExerciseDto { ExerciseName = "Bench Press", Sets = 3, Reps = 10 },
                new WorkoutExerciseDto { ExerciseName = "Deadlift", Sets = 4, Reps = 12 }
            };

            // Act
            var result = await _exerciseService.PrepareExercises<WorkoutExercise>(_userGuid, exercises);

            // Assert
            Assert.NotEmpty(result);
            Assert.Equal(exercises.Count, result.Count);
            foreach (var exercise in result)
            {
                Assert.NotEqual(Guid.Empty, exercise.ExerciseId);
                Assert.True(exercise.Sets > 0);
                Assert.True(exercise.Reps > 0);

                // Check if user-exercise link was created
                var userExerciseLink = await _dbContext.UserExercises
                    .AnyAsync(ue => ue.UserId == _userGuid && ue.ExerciseId == exercise.ExerciseId);
                Assert.True(userExerciseLink);
            }
        }

        [Fact]
        public async Task PrepareExercises_WithEmptyExercisesList_ShouldReturnEmptyList()
        {
            // Arrange
            var exercises = new List<WorkoutExerciseDto>();

            // Act
            var result = await _exerciseService.PrepareExercises<WorkoutExercise>(_userGuid, exercises);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task PrepareExercises_WithDifferentExerciseTypes_ShouldReturnCorrectlyPreparedExercises()
        {
            // Arrange
            var exercises = new List<WorkoutExerciseDto>
            {
                new WorkoutExerciseDto { ExerciseName = "Bench Press", Sets = 3, Reps = 10 },
                new WorkoutExerciseDto { ExerciseName = "Deadlift", Sets = 4, Reps = 8 }
            };

            // Act
            var result = await _exerciseService.PrepareExercises<WorkoutPlanExercise>(_userGuid, exercises);

            // Assert
            Assert.All(result, ex =>
            {
                Assert.IsType<WorkoutPlanExercise>(ex);
                Assert.True(ex.Sets > 0);
                Assert.True(ex.Reps > 0);
            });
        }

        public class UnsupportedExerciseType { }

        [Fact]
        public async Task PrepareExercises_ShouldReturnEmptyList_WhenUnsupportedTypeIsProvided()
        {
            // Arrange
            var exercises = new List<WorkoutExerciseDto>
            {
                new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 3, Reps = 10 }
            };

            // Act
            var result = await _exerciseService.PrepareExercises<UnsupportedExerciseType>(_userGuid, exercises);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task EnsureUserExerciseLinkAsync_ShouldNotCreateDuplicateLinks_OnConcurrentRequests()
        {
            // Arrange
            var exerciseId = Guid.NewGuid();

            // First call to EnsureUserExerciseLinkAsync to create the link
            var firstResult = await _exerciseService.EnsureUserExerciseLinkAsync(_userGuid, exerciseId);

            // Act - Call EnsureUserExerciseLinkAsync again for the same user and exercise
            var secondResult = await _exerciseService.EnsureUserExerciseLinkAsync(_userGuid, exerciseId);

            // Assert
            Assert.True(firstResult);
            Assert.False(secondResult);  // Should not create a duplicate link
            var linkCount = await _dbContext.UserExercises.CountAsync(ue => ue.UserId == _userGuid && ue.ExerciseId == exerciseId);
            Assert.Equal(1, linkCount);
        }

        [Fact]
        public async Task PrepareExercises_ShouldNormalizeExerciseNames()
        {
            // Arrange
            var exercises = new List<WorkoutExerciseDto>
            {
                new WorkoutExerciseDto { ExerciseName = "  BENCH press ", Sets = 3, Reps = 10 }
            };

            // Act
            var result = await _exerciseService.PrepareExercises<WorkoutExercise>(_userGuid, exercises);

            // Assert
            Assert.NotEmpty(result);
            Assert.All(result, async ex =>
            {
                var normalizedExerciseName = NameNormalizationHelper.NormalizeName("BENCH press");

                var exerciseInDb = await _dbContext.Exercises.FirstOrDefaultAsync(e => e.Name == normalizedExerciseName);

                Assert.NotNull(exerciseInDb);
                Assert.Equal(normalizedExerciseName, exerciseInDb.Name);
            });
        }
    }
}
