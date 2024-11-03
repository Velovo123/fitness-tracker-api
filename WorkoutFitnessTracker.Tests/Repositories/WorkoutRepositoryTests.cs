using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Xunit;

namespace WorkoutFitnessTrackerAPI.Tests.Repositories
{
    public class WorkoutRepositoryTests : IDisposable
    {
        private readonly WFTDbContext _context;
        private readonly WorkoutRepository _repository;
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly Mock<IMapper> _mapperMock;
        private readonly Guid _testUserId = Guid.NewGuid();

        public WorkoutRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<WFTDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new WFTDbContext(options);

            _exerciseServiceMock = new Mock<IExerciseService>();
            _mapperMock = new Mock<IMapper>();

            // Setup AutoMapper for DTOs in tests
            _mapperMock.Setup(m => m.Map<IEnumerable<WorkoutDto>>(It.IsAny<IEnumerable<Workout>>()))
                .Returns((IEnumerable<Workout> workouts) =>
                    workouts.Select(w => new WorkoutDto
                    {
                        Date = w.Date,
                        Duration = w.Duration,
                        Exercises = w.WorkoutExercises.Select(e => new WorkoutExerciseDto
                        {
                            ExerciseName = e.Exercise.Name,
                            Sets = e.Sets,
                            Reps = e.Reps
                        }).ToList()
                    })
                );

            _repository = new WorkoutRepository(_context, _exerciseServiceMock.Object, _mapperMock.Object);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var exercise1 = new Exercise { Name = "Push Up" };
            var exercise2 = new Exercise { Name = "Squat" };

            var workout1 = new Workout
            {
                UserId = _testUserId,
                Date = DateTime.Today,
                Duration = 60,
                WorkoutExercises = new List<WorkoutExercise>
                {
                    new WorkoutExercise { Sets = 3, Reps = 10, Exercise = exercise1 },
                    new WorkoutExercise { Sets = 4, Reps = 15, Exercise = exercise2 }
                }
            };

            var workout2 = new Workout
            {
                UserId = _testUserId,
                Date = DateTime.Today.AddDays(-1),
                Duration = 45,
                WorkoutExercises = new List<WorkoutExercise>
                {
                    new WorkoutExercise { Sets = 5, Reps = 12, Exercise = exercise1 },
                    new WorkoutExercise { Sets = 3, Reps = 20, Exercise = exercise2 }
                }
            };

            _context.Workouts.AddRange(workout1, workout2);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetWorkoutsAsync_ReturnsWorkoutsWithCorrectData()
        {
            // Arrange
            var queryParams = new WorkoutQueryParams();

            // Act
            var result = await _repository.GetWorkoutsAsync(_testUserId, queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, workout =>
            {
                Assert.NotEqual(DateTime.MinValue, workout.Date);
                Assert.InRange(workout.Duration, 1, 300);
            });
        }

        [Fact]
        public async Task GetWorkoutsAsync_ReturnsPaginatedSortedWorkouts()
        {
            // Arrange
            var queryParams = new WorkoutQueryParams
            {
                PageNumber = 1,
                PageSize = 1,
                SortBy = "duration",
                SortDescending = true
            };

            // Act
            var result = await _repository.GetWorkoutsAsync(_testUserId, queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(60, result.First().Duration);
        }

        [Fact]
        public async Task GetWorkoutsByDateAsync_ReturnsWorkoutsForSpecificDate()
        {
            // Arrange
            var date = DateTime.Today;

            // Act
            var result = await _repository.GetWorkoutsByDateAsync(_testUserId, date);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, workout => Assert.Equal(date, workout.Date));
        }

        [Fact]
        public async Task SaveWorkoutAsync_CreatesOrUpdatesWorkoutCorrectly()
        {
            // Arrange
            var workoutDto = new WorkoutDto
            {
                Date = DateTime.Today.AddDays(1),
                Duration = 30,
                Exercises = new List<WorkoutExerciseDto>
                {
                    new WorkoutExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 10 },
                    new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 15 }
                }
            };

            _exerciseServiceMock.Setup(x => x.PrepareExercises<WorkoutExercise>(_testUserId, workoutDto.Exercises))
                .ReturnsAsync(new List<WorkoutExercise>
                {
                    new WorkoutExercise { ExerciseId = Guid.NewGuid(), Sets = 3, Reps = 10 },
                    new WorkoutExercise { ExerciseId = Guid.NewGuid(), Sets = 4, Reps = 15 }
                });

            // Act - Test creating a new workout
            var createResult = await _repository.SaveWorkoutAsync(_testUserId, workoutDto, overwrite: false);

            // Act - Test updating the workout with overwrite set to true
            var updateResult = await _repository.SaveWorkoutAsync(_testUserId, workoutDto, overwrite: true);

            // Assert
            Assert.True(createResult);
            Assert.True(updateResult);
        }

        [Fact]
        public async Task DeleteWorkoutAsync_RemovesWorkoutIfItExists()
        {
            // Arrange
            var date = _context.Workouts.First(w => w.UserId == _testUserId).Date;

            // Act
            var deleteResult = await _repository.DeleteWorkoutAsync(_testUserId, date);
            var workoutExists = await _context.Workouts
                .AnyAsync(w => w.UserId == _testUserId && w.Date == date);

            // Assert
            Assert.True(deleteResult);
            Assert.False(workoutExists);
        }

        [Fact]
        public async Task SaveWorkoutAsync_ThrowsWhenOverwriteIsFalseAndWorkoutExists()
        {
            // Arrange
            var workoutDto = new WorkoutDto { Date = DateTime.Today, Duration = 30, Exercises = new List<WorkoutExerciseDto>() };
            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.SaveWorkoutAsync(_testUserId, workoutDto, overwrite: false));
        }

        [Fact]
        public async Task DeleteWorkoutAsync_ReturnsFalseWhenWorkoutNotFound()
        {
            // Arrange
            var nonExistentDate = DateTime.Today.AddDays(5);
            // Act
            var result = await _repository.DeleteWorkoutAsync(_testUserId, nonExistentDate);
            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetWorkoutsAsync_FiltersWorkoutsByDuration()
        {
            // Arrange
            var queryParams = new WorkoutQueryParams { MinDuration = 50 };
            // Act
            var result = await _repository.GetWorkoutsAsync(_testUserId, queryParams);
            // Assert
            Assert.All(result, workout => Assert.True(workout.Duration >= 50));
        }
    }
}
