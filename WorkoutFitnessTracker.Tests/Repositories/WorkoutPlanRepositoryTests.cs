using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Mappings;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Xunit;

namespace WorkoutFitnessTrackerAPI.Tests.Repositories
{
    public class WorkoutPlanRepositoryTests : IDisposable
    {
        private readonly WFTDbContext _context;
        private readonly WorkoutPlanRepository _repository;
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly IMapper _mapper;
        private readonly Guid _testUserId = Guid.NewGuid();

        public WorkoutPlanRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<WFTDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new WFTDbContext(options);

            _exerciseServiceMock = new Mock<IExerciseService>();

            // Setup AutoMapper for DTOs in tests
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<WorkoutPlanMappingProfile>(); // Ensure this profile is defined in your API project
            });
            _mapper = config.CreateMapper();

            _repository = new WorkoutPlanRepository(_context, _exerciseServiceMock.Object, _mapper);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var exercise1 = new Exercise { Name = "Push Up" };
            var exercise2 = new Exercise { Name = "Squat" };

            var plan1 = new WorkoutPlan
            {
                UserId = _testUserId,
                Name = "plana",
                Goal = "Strength",
                WorkoutPlanExercises = new List<WorkoutPlanExercise>
                {
                    new WorkoutPlanExercise { Sets = 3, Reps = 10, Exercise = exercise1 },
                    new WorkoutPlanExercise { Sets = 4, Reps = 15, Exercise = exercise2 }
                }
            };

            _context.WorkoutPlans.Add(plan1);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetWorkoutPlansAsync_ReturnsPlansWithCorrectData()
        {
            // Arrange
            var queryParams = new WorkoutPlanQueryParams();

            // Act
            var result = await _repository.GetWorkoutPlansAsync(_testUserId, queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, plan =>
            {
                Assert.NotEmpty(plan.Name);
                Assert.NotNull(plan.Goal);
            });
        }

        [Fact]
        public async Task GetWorkoutPlansAsync_ReturnsFilteredPlansByGoal()
        {
            // Arrange
            var queryParams = new WorkoutPlanQueryParams { Goal = "Strength" };

            // Act
            var result = await _repository.GetWorkoutPlansAsync(_testUserId, queryParams);

            // Assert
            Assert.All(result, plan => Assert.Equal("Strength", plan.Goal));
        }

        [Fact]
        public async Task GetWorkoutPlanByNameAsync_ReturnsCorrectPlan()
        {
            // Act
            var result = await _repository.GetWorkoutPlanByNameAsync(_testUserId, "Plan A");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("plana", result!.Name);
        }

        [Fact]
        public async Task SaveWorkoutPlanAsync_CreatesOrUpdatesPlanCorrectly()
        {
            // Arrange - Initial creation
            var workoutPlanDto = new WorkoutPlanDto
            {
                Name = "Plan B",
                Goal = "Endurance",
                Exercises = new List<WorkoutPlanExerciseDto>
        {
            new WorkoutPlanExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 10 },
            new WorkoutPlanExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 15 }
        }
            };

            _exerciseServiceMock.Setup(x => x.PrepareExercises<WorkoutPlanExercise>(_testUserId, workoutPlanDto.Exercises))
                .ReturnsAsync(new List<WorkoutPlanExercise>
                {
            new WorkoutPlanExercise { ExerciseId = Guid.NewGuid(), Sets = 3, Reps = 10 },
            new WorkoutPlanExercise { ExerciseId = Guid.NewGuid(), Sets = 4, Reps = 15 }
                });

            // Act - Test creating a new plan
            var createResult = await _repository.SaveWorkoutPlanAsync(_testUserId, workoutPlanDto, overwrite: false);

            // Arrange - Create new instance for updating
            var updatedWorkoutPlanDto = new WorkoutPlanDto
            {
                Name = "Plan B", // Same name to simulate an update
                Goal = "Updated Goal", // New goal
                Exercises = workoutPlanDto.Exercises // Use the same exercises
            };

            // Act - Test updating the plan with overwrite set to true
            var updateResult = await _repository.SaveWorkoutPlanAsync(_testUserId, updatedWorkoutPlanDto, overwrite: true);

            // Assert
            Assert.True(createResult);
            Assert.True(updateResult);
        }


        [Fact]
        public async Task DeleteWorkoutPlanAsync_RemovesPlanIfItExists()
        {
            // Arrange
            var planName = "Plan A";

            // Act
            var deleteResult = await _repository.DeleteWorkoutPlanAsync(_testUserId, planName);
            var planExists = await _context.WorkoutPlans
                .AnyAsync(wp => wp.UserId == _testUserId && wp.Name == planName);

            // Assert
            Assert.True(deleteResult);
            Assert.False(planExists);
        }

        [Fact]
        public async Task SaveWorkoutPlanAsync_ThrowsWhenOverwriteIsFalseAndPlanExists()
        {
            // Arrange
            var workoutPlanDto = new WorkoutPlanDto { Name = "Plan A", Goal = "Strength", Exercises = new List<WorkoutPlanExerciseDto>
                {
                    new WorkoutPlanExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 10 },
                    new WorkoutPlanExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 15 }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.SaveWorkoutPlanAsync(_testUserId, workoutPlanDto, overwrite: false));
        }

        [Fact]
        public async Task DeleteWorkoutPlanAsync_ReturnsFalseWhenPlanNotFound()
        {
            // Arrange
            var nonExistentPlanName = "Nonexistent Plan";

            // Act
            var result = await _repository.DeleteWorkoutPlanAsync(_testUserId, nonExistentPlanName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task SaveWorkoutPlanAsync_CallsPrepareExercisesWithCorrectParameters()
        {
            // Arrange
            var workoutPlanDto = new WorkoutPlanDto
            {
                Name = "Plan C",
                Goal = "Strength",
                Exercises = new List<WorkoutPlanExerciseDto>
                {
                    new WorkoutPlanExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 10 },
                    new WorkoutPlanExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 15 }
                }
            };

            _exerciseServiceMock.Setup(x => x.PrepareExercises<WorkoutPlanExercise>(_testUserId, workoutPlanDto.Exercises))
                .ReturnsAsync(new List<WorkoutPlanExercise>
                {
                    new WorkoutPlanExercise { ExerciseId = Guid.NewGuid(), Sets = 3, Reps = 10 },
                    new WorkoutPlanExercise { ExerciseId = Guid.NewGuid(), Sets = 4, Reps = 15 }
                });

            // Act
            await _repository.SaveWorkoutPlanAsync(_testUserId, workoutPlanDto);

            // Assert
            _exerciseServiceMock.Verify(x => x.PrepareExercises<WorkoutPlanExercise>(_testUserId, workoutPlanDto.Exercises), Times.Once);
        }

        [Fact]
        public async Task SaveWorkoutPlanAsync_ThrowsArgumentNullException_WhenWorkoutPlanDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.SaveWorkoutPlanAsync(_testUserId, null));
        }

        [Fact]
        public async Task SaveWorkoutPlanAsync_ThrowsArgumentException_WhenWorkoutPlanDtoNameIsEmpty()
        {
            // Arrange
            var workoutPlanDto = new WorkoutPlanDto
            {
                Name = string.Empty, // Invalid name
                Goal = "Strength",
                Exercises = new List<WorkoutPlanExerciseDto>
        {
            new WorkoutPlanExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 10 }
        }
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.SaveWorkoutPlanAsync(_testUserId, workoutPlanDto));
        }

        [Fact]
        public async Task SaveWorkoutPlanAsync_ThrowsArgumentException_WhenWorkoutPlanDtoExercisesIsEmpty()
        {
            // Arrange
            var workoutPlanDto = new WorkoutPlanDto
            {
                Name = "Plan D",
                Goal = "Strength",
                Exercises = new List<WorkoutPlanExerciseDto>() // Empty list of exercises
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.SaveWorkoutPlanAsync(_testUserId, workoutPlanDto));
        }

    }
}
