using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services;
using WorkoutFitnessTrackerAPI.Services.IServices;

namespace WorkoutFitnessTrackerAPI.Tests.Services
{
    public class WorkoutPlanServiceTests
    {
        private readonly Mock<IWorkoutPlanRepository> _workoutPlanRepositoryMock;
        private readonly WorkoutPlanService _workoutPlanService;

        public WorkoutPlanServiceTests()
        {
            _workoutPlanRepositoryMock = new Mock<IWorkoutPlanRepository>();
            _workoutPlanService = new WorkoutPlanService(_workoutPlanRepositoryMock.Object);
        }

        [Fact]
        public async Task GetWorkoutPlansAsync_ShouldReturnWorkoutPlans()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var queryParams = new WorkoutPlanQueryParams();
            var expectedPlans = new List<WorkoutPlanDto>
            {
                new WorkoutPlanDto { Name = "Plan A", Goal = "Strength" },
                new WorkoutPlanDto { Name = "Plan B", Goal = "Endurance" }
            };

            _workoutPlanRepositoryMock.Setup(repo => repo.GetWorkoutPlansAsync(userId, queryParams))
                .ReturnsAsync(expectedPlans);

            // Act
            var result = await _workoutPlanService.GetWorkoutPlansAsync(userId, queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _workoutPlanRepositoryMock.Verify(repo => repo.GetWorkoutPlansAsync(userId, queryParams), Times.Once);
        }

        [Fact]
        public async Task GetWorkoutPlanByNameAsync_ShouldReturnWorkoutPlan()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var planName = "Plan A";
            var expectedPlan = new WorkoutPlanDto { Name = planName, Goal = "Strength" };

            _workoutPlanRepositoryMock.Setup(repo => repo.GetWorkoutPlanByNameAsync(userId, planName))
                .ReturnsAsync(expectedPlan);

            // Act
            var result = await _workoutPlanService.GetWorkoutPlanByNameAsync(userId, planName);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(planName, result!.Name);
            _workoutPlanRepositoryMock.Verify(repo => repo.GetWorkoutPlanByNameAsync(userId, planName), Times.Once);
        }

        [Fact]
        public async Task CreateWorkoutPlanAsync_ShouldCreatePlan_WhenNoExistingPlan()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutPlanDto = new WorkoutPlanDto { Name = "Plan A", Goal = "Strength" };

            _workoutPlanRepositoryMock.Setup(repo => repo.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name))
                .ReturnsAsync((WorkoutPlanDto?)null);
            _workoutPlanRepositoryMock.Setup(repo => repo.CreateWorkoutPlanAsync(userId, workoutPlanDto))
                .ReturnsAsync(true);

            // Act
            var result = await _workoutPlanService.CreateWorkoutPlanAsync(userId, workoutPlanDto);

            // Assert
            Assert.True(result);
            _workoutPlanRepositoryMock.Verify(repo => repo.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name), Times.Once);
            _workoutPlanRepositoryMock.Verify(repo => repo.CreateWorkoutPlanAsync(userId, workoutPlanDto), Times.Once);
        }

        [Fact]
        public async Task CreateWorkoutPlanAsync_ShouldThrowException_WhenPlanExistsAndOverwriteIsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutPlanDto = new WorkoutPlanDto { Name = "Plan A", Goal = "Strength" };
            _workoutPlanRepositoryMock.Setup(repo => repo.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name))
                .ReturnsAsync(workoutPlanDto);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _workoutPlanService.CreateWorkoutPlanAsync(userId, workoutPlanDto));
            _workoutPlanRepositoryMock.Verify(repo => repo.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name), Times.Once);
            _workoutPlanRepositoryMock.Verify(repo => repo.CreateWorkoutPlanAsync(It.IsAny<Guid>(), It.IsAny<WorkoutPlanDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateWorkoutPlanAsync_ShouldOverwritePlan_WhenPlanExistsAndOverwriteIsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutPlanDto = new WorkoutPlanDto { Name = "Plan A", Goal = "Strength" };

            _workoutPlanRepositoryMock.Setup(repo => repo.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name))
                .ReturnsAsync(workoutPlanDto);
            _workoutPlanRepositoryMock.Setup(repo => repo.UpdateWorkoutPlanAsync(userId, workoutPlanDto))
                .ReturnsAsync(true);

            // Act
            var result = await _workoutPlanService.CreateWorkoutPlanAsync(userId, workoutPlanDto, overwrite: true);

            // Assert
            Assert.True(result);
            _workoutPlanRepositoryMock.Verify(repo => repo.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name), Times.Once);
            _workoutPlanRepositoryMock.Verify(repo => repo.UpdateWorkoutPlanAsync(userId, workoutPlanDto), Times.Once);
        }

        [Fact]
        public async Task UpdateWorkoutPlanAsync_ShouldReturnTrue_WhenPlanIsUpdated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutPlanDto = new WorkoutPlanDto { Name = "Plan A", Goal = "Strength" };
            _workoutPlanRepositoryMock.Setup(repo => repo.UpdateWorkoutPlanAsync(userId, workoutPlanDto)).ReturnsAsync(true);

            // Act
            var result = await _workoutPlanService.UpdateWorkoutPlanAsync(userId, workoutPlanDto);

            // Assert
            Assert.True(result);
            _workoutPlanRepositoryMock.Verify(repo => repo.UpdateWorkoutPlanAsync(userId, workoutPlanDto), Times.Once);
        }

        [Fact]
        public async Task DeleteWorkoutPlanAsync_ShouldReturnTrue_WhenPlanIsDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var planName = "Plan A";
            _workoutPlanRepositoryMock.Setup(repo => repo.DeleteWorkoutPlanAsync(userId, planName)).ReturnsAsync(true);

            // Act
            var result = await _workoutPlanService.DeleteWorkoutPlanAsync(userId, planName);

            // Assert
            Assert.True(result);
            _workoutPlanRepositoryMock.Verify(repo => repo.DeleteWorkoutPlanAsync(userId, planName), Times.Once);
        }

        [Fact]
        public async Task DeleteWorkoutPlanAsync_ShouldReturnFalse_WhenPlanDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var planName = "NonExistentPlan";
            _workoutPlanRepositoryMock.Setup(repo => repo.DeleteWorkoutPlanAsync(userId, planName)).ReturnsAsync(false);

            // Act
            var result = await _workoutPlanService.DeleteWorkoutPlanAsync(userId, planName);

            // Assert
            Assert.False(result);
            _workoutPlanRepositoryMock.Verify(repo => repo.DeleteWorkoutPlanAsync(userId, planName), Times.Once);
        }
    }
}
