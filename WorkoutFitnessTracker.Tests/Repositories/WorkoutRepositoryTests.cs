using Xunit;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Collections.Generic;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using WorkoutFitnessTrackerAPI.Mappings;
using Moq;

namespace WorkoutFitnessTrackerAPI.Tests.Repositories
{
    public class WorkoutRepositoryTests
    {
        private readonly WFTDbContext _context;
        private readonly WorkoutRepository _workoutRepository;
        private readonly IMapper _mapper;

        public WorkoutRepositoryTests()
        {
            // Configure in-memory database
            var options = new DbContextOptionsBuilder<WFTDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new WFTDbContext(options);

            // Configure AutoMapper
            var config = new MapperConfiguration(cfg => cfg.AddProfile<WorkoutMappingProfile>());
            _mapper = config.CreateMapper();

            // Initialize WorkoutRepository with in-memory context and mapper
            _workoutRepository = new WorkoutRepository(_context, Mock.Of<IExerciseService>(), _mapper);
        }

        [Fact]
        public async Task GetWorkoutsAsync_ShouldReturnFilteredWorkouts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _context.Workouts.AddRange(
                new Workout { UserId = userId, Date = DateTime.Now, Duration = 60 },
                new Workout { UserId = userId, Date = DateTime.Now.AddDays(-1), Duration = 45 }
            );
            await _context.SaveChangesAsync();

            var queryParams = new WorkoutQueryParams { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await _workoutRepository.GetWorkoutsAsync(userId, queryParams);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetWorkoutsByDateAsync_ShouldReturnWorkoutsOnSpecificDate()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;

            _context.Workouts.AddRange(
                new Workout { UserId = userId, Date = date, Duration = 60 },
                new Workout { UserId = userId, Date = date, Duration = 45 }
            );
            await _context.SaveChangesAsync();

            // Act
            var result = await _workoutRepository.GetWorkoutsByDateAsync(userId, date);

            // Assert
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task CreateWorkoutAsync_ShouldAddWorkoutToDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutDto = new WorkoutDto
            {
                Date = DateTime.Now,
                Duration = 60,
                Exercises = new List<WorkoutExerciseDto>
                {
                    new WorkoutExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 15 }
                }
            };

            // Act
            var result = await _workoutRepository.CreateWorkoutAsync(userId, workoutDto);
            var createdWorkout = await _context.Workouts.FirstOrDefaultAsync(w => w.UserId == userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(createdWorkout);
            Assert.Equal(60, createdWorkout.Duration);
        }

        [Fact]
        public async Task UpdateWorkoutAsync_ShouldUpdateExistingWorkout()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;

            var workout = new Workout { UserId = userId, Date = date, Duration = 45 };
            _context.Workouts.Add(workout);
            await _context.SaveChangesAsync();

            var updatedWorkoutDto = new WorkoutDto
            {
                Date = date,
                Duration = 90,
                Exercises = new List<WorkoutExerciseDto>
                {
                    new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 10 }
                }
            };

            // Act
            var result = await _workoutRepository.UpdateWorkoutAsync(userId, updatedWorkoutDto);
            var updatedWorkout = await _context.Workouts.FirstOrDefaultAsync(w => w.UserId == userId && w.Date == date);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedWorkout);
            Assert.Equal(90, updatedWorkout.Duration);
        }

        [Fact]
        public async Task DeleteWorkoutAsync_ShouldRemoveWorkoutFromDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;

            _context.Workouts.Add(new Workout { UserId = userId, Date = date, Duration = 45 });
            await _context.SaveChangesAsync();

            // Act
            var result = await _workoutRepository.DeleteWorkoutAsync(userId, date);
            var deletedWorkout = await _context.Workouts.FirstOrDefaultAsync(w => w.UserId == userId && w.Date == date);

            // Assert
            Assert.True(result);
            Assert.Null(deletedWorkout);
        }

        [Fact]
        public async Task UpdateWorkoutAsync_NonExistentWorkout_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutDto = new WorkoutDto
            {
                Date = DateTime.Now.AddDays(-10), // Non-existent date
                Duration = 90,
                Exercises = new List<WorkoutExerciseDto>
                {
                    new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 10 }
                }
            };

            // Act & Assert
            await Assert.ThrowsAsync<KeyNotFoundException>(() => _workoutRepository.UpdateWorkoutAsync(userId, workoutDto));
        }

        [Fact]
        public async Task DeleteWorkoutAsync_NonExistentWorkout_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.AddDays(-10); // Date with no workout

            // Act
            var result = await _workoutRepository.DeleteWorkoutAsync(userId, date);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetWorkoutsAsync_NoWorkouts_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var queryParams = new WorkoutQueryParams { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await _workoutRepository.GetWorkoutsAsync(userId, queryParams);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWorkoutsAsync_PaginationOutOfRange_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _context.Workouts.Add(new Workout { UserId = userId, Date = DateTime.Now, Duration = 60 });
            await _context.SaveChangesAsync();

            var queryParams = new WorkoutQueryParams { PageNumber = 2, PageSize = 10 }; // Requesting second page when only one page is available

            // Act
            var result = await _workoutRepository.GetWorkoutsAsync(userId, queryParams);

            // Assert
            Assert.Empty(result);
        }
    }
}
