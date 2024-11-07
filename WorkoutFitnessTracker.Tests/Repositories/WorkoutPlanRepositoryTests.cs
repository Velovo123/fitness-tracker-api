using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using WorkoutFitnessTrackerAPI.Mappings;

namespace WorkoutFitnessTrackerAPI.Tests.Repositories
{
    public class WorkoutPlanRepositoryTests
    {
        private readonly WFTDbContext _context;
        private readonly WorkoutPlanRepository _repository;
        private readonly IMapper _mapper;
        private readonly Mock<IExerciseService> _exerciseServiceMock;

        public WorkoutPlanRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<WFTDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new WFTDbContext(options);

            var config = new MapperConfiguration(cfg => cfg.AddProfile<WorkoutPlanMappingProfile>());
            _mapper = config.CreateMapper();
            _exerciseServiceMock = new Mock<IExerciseService>();

            _repository = new WorkoutPlanRepository(_context, _exerciseServiceMock.Object, _mapper);
        }

        [Fact]
        public async Task GetWorkoutPlansAsync_ShouldReturnFilteredAndSortedPlans()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _context.WorkoutPlans.AddRange(
                new WorkoutPlan { UserId = userId, Name = "Plan A", Goal = "Strength" },
                new WorkoutPlan { UserId = userId, Name = "Plan B", Goal = "Endurance" },
                new WorkoutPlan { UserId = userId, Name = "Plan C", Goal = "Strength" }
            );
            await _context.SaveChangesAsync();

            var queryParams = new WorkoutPlanQueryParams { Goal = "Strength", SortBy = "name", SortDescending = false };

            // Act
            var result = await _repository.GetWorkoutPlansAsync(userId, queryParams);

            // Assert
            Assert.Equal(2, result.Count());
            Assert.Equal("Plan A", result.First().Name);
        }

        [Fact]
        public async Task GetWorkoutPlanByNameAsync_ShouldReturnPlanByName()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var planName = "strengthplan";
            _context.WorkoutPlans.Add(new WorkoutPlan { UserId = userId, Name = planName, Goal = "Strength" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetWorkoutPlanByNameAsync(userId, planName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(planName, result.Name);
        }

        [Fact]
        public async Task CreateWorkoutPlanAsync_ShouldAddPlanToDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutPlanDto = new WorkoutPlanDto
            {
                Name = "Endurance Plan",
                Goal = "Endurance",
                Exercises = new List<WorkoutPlanExerciseDto> { new WorkoutPlanExerciseDto { ExerciseName = "Squat", Sets = 3, Reps = 15 } }
            };

            _exerciseServiceMock
                .Setup(service => service.PrepareExercises<WorkoutPlanExercise>(userId, workoutPlanDto.Exercises))
                .ReturnsAsync(new List<WorkoutPlanExercise> { new WorkoutPlanExercise { ExerciseId = Guid.NewGuid(), Sets = 3, Reps = 15 } });

            // Act
            var result = await _repository.CreateWorkoutPlanAsync(userId, workoutPlanDto);
            var createdPlan = await _context.WorkoutPlans.FirstOrDefaultAsync(wp => wp.UserId == userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(createdPlan);
            Assert.Equal("Endurance Plan", createdPlan.Name);
        }

        [Fact]
        public async Task UpdateWorkoutPlanAsync_ShouldUpdateExistingPlan()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutPlan = new WorkoutPlan
            {
                UserId = userId,
                Name = "Endurance Plan",
                Goal = "Endurance"
            };
            _context.WorkoutPlans.Add(workoutPlan);
            await _context.SaveChangesAsync();

            var workoutPlanDto = new WorkoutPlanDto
            {
                Name = "Endurance Plan",
                Goal = "Updated Endurance",
                Exercises = new List<WorkoutPlanExerciseDto> { new WorkoutPlanExerciseDto { ExerciseName = "Bench Press", Sets = 4, Reps = 10 } }
            };

            _exerciseServiceMock
                .Setup(service => service.PrepareExercises<WorkoutPlanExercise>(userId, workoutPlanDto.Exercises))
                .ReturnsAsync(new List<WorkoutPlanExercise> { new WorkoutPlanExercise { ExerciseId = Guid.NewGuid(), Sets = 4, Reps = 10 } });

            // Act
            var result = await _repository.UpdateWorkoutPlanAsync(userId, workoutPlanDto);
            var updatedPlan = await _context.WorkoutPlans.FirstOrDefaultAsync(wp => wp.UserId == userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedPlan);
            Assert.Equal("Updated Endurance", updatedPlan.Goal);
        }

        [Fact]
        public async Task DeleteWorkoutPlanAsync_ShouldRemovePlanFromDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var planName = "Endurance Plan";
            _context.WorkoutPlans.Add(new WorkoutPlan { UserId = userId, Name = planName, Goal = "Endurance" });
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteWorkoutPlanAsync(userId, planName);
            var deletedPlan = await _context.WorkoutPlans.FirstOrDefaultAsync(wp => wp.UserId == userId && wp.Name == planName);

            // Assert
            Assert.True(result);
            Assert.Null(deletedPlan);
        }

        [Fact]
        public async Task DeleteWorkoutPlanAsync_NonExistentPlan_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var nonExistentPlanName = "NonExistent Plan";

            // Act
            var result = await _repository.DeleteWorkoutPlanAsync(userId, nonExistentPlanName);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public async Task GetWorkoutPlansAsync_NoPlans_ShouldReturnEmptyList()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var queryParams = new WorkoutPlanQueryParams();

            // Act
            var result = await _repository.GetWorkoutPlansAsync(userId, queryParams);

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetWorkoutPlansAsync_WithPaging_ShouldReturnCorrectPage()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _context.WorkoutPlans.AddRange(
                new WorkoutPlan { UserId = userId, Name = "Plan A" },
                new WorkoutPlan { UserId = userId, Name = "Plan B" },
                new WorkoutPlan { UserId = userId, Name = "Plan C" }
            );
            await _context.SaveChangesAsync();

            var queryParams = new WorkoutPlanQueryParams { PageNumber = 1, PageSize = 2 };

            // Act
            var result = await _repository.GetWorkoutPlansAsync(userId, queryParams);

            // Assert
            Assert.Equal(2, result.Count());
        }
    }
}
