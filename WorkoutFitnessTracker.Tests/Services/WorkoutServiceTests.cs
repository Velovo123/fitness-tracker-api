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
    public class WorkoutServiceTests
    {
        private readonly Mock<IWorkoutRepository> _workoutRepositoryMock;
        private readonly WorkoutService _workoutService;

        public WorkoutServiceTests()
        {
            _workoutRepositoryMock = new Mock<IWorkoutRepository>();
            _workoutService = new WorkoutService(_workoutRepositoryMock.Object);
        }

        [Fact]
        public async Task GetWorkoutsAsync_ShouldReturnWorkouts()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var queryParams = new WorkoutQueryParams();
            var expectedWorkouts = new List<WorkoutDto>
            {
                new WorkoutDto { Date = DateTime.Now, Duration = 60 },
                new WorkoutDto { Date = DateTime.Now.AddDays(-1), Duration = 45 }
            };

            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(userId, queryParams))
                .ReturnsAsync(expectedWorkouts);

            // Act
            var result = await _workoutService.GetWorkoutsAsync(userId, queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
            _workoutRepositoryMock.Verify(repo => repo.GetWorkoutsAsync(userId, queryParams), Times.Once);
        }

        [Fact]
        public async Task GetWorkoutsByDateAsync_ShouldReturnWorkoutByDate()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;
            var expectedWorkouts = new List<WorkoutDto>
            {
                new WorkoutDto { Date = date, Duration = 60 }
            };

            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateAsync(userId, date))
                .ReturnsAsync(expectedWorkouts);

            // Act
            var result = await _workoutService.GetWorkoutsByDateAsync(userId, date);

            // Assert
            Assert.NotNull(result);
            Assert.Single(result);
            Assert.Equal(60, result.First().Duration);
            _workoutRepositoryMock.Verify(repo => repo.GetWorkoutsByDateAsync(userId, date), Times.Once);
        }

        [Fact]
        public async Task CreateWorkoutAsync_ShouldCreateWorkout_WhenNoExistingWorkout()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutDto = new WorkoutDto { Date = DateTime.Now, Duration = 45 };
            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateAsync(userId, workoutDto.Date))
                .ReturnsAsync(new List<WorkoutDto>());
            _workoutRepositoryMock.Setup(repo => repo.CreateWorkoutAsync(userId, workoutDto))
                .ReturnsAsync(true);

            // Act
            var result = await _workoutService.CreateWorkoutAsync(userId, workoutDto);

            // Assert
            Assert.True(result);
            _workoutRepositoryMock.Verify(repo => repo.GetWorkoutsByDateAsync(userId, workoutDto.Date), Times.Once);
            _workoutRepositoryMock.Verify(repo => repo.CreateWorkoutAsync(userId, workoutDto), Times.Once);
        }

        [Fact]
        public async Task CreateWorkoutAsync_ShouldThrowException_WhenWorkoutExistsAndOverwriteIsFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutDto = new WorkoutDto { Date = DateTime.Now, Duration = 45 };
            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateAsync(userId, workoutDto.Date))
                .ReturnsAsync(new List<WorkoutDto> { workoutDto });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _workoutService.CreateWorkoutAsync(userId, workoutDto));
            _workoutRepositoryMock.Verify(repo => repo.GetWorkoutsByDateAsync(userId, workoutDto.Date), Times.Once);
            _workoutRepositoryMock.Verify(repo => repo.CreateWorkoutAsync(It.IsAny<Guid>(), It.IsAny<WorkoutDto>()), Times.Never);
        }

        [Fact]
        public async Task CreateWorkoutAsync_ShouldOverwriteWorkout_WhenWorkoutExistsAndOverwriteIsTrue()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutDto = new WorkoutDto { Date = DateTime.Now, Duration = 45 };
            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateAsync(userId, workoutDto.Date))
                .ReturnsAsync(new List<WorkoutDto> { workoutDto });
            _workoutRepositoryMock.Setup(repo => repo.UpdateWorkoutAsync(userId, workoutDto))
                .ReturnsAsync(true);

            // Act
            var result = await _workoutService.CreateWorkoutAsync(userId, workoutDto, overwrite: true);

            // Assert
            Assert.True(result);
            _workoutRepositoryMock.Verify(repo => repo.GetWorkoutsByDateAsync(userId, workoutDto.Date), Times.Once);
            _workoutRepositoryMock.Verify(repo => repo.UpdateWorkoutAsync(userId, workoutDto), Times.Once);
        }

        [Fact]
        public async Task UpdateWorkoutAsync_ShouldReturnTrue_WhenWorkoutIsUpdated()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var workoutDto = new WorkoutDto { Date = DateTime.Now, Duration = 60 };
            _workoutRepositoryMock.Setup(repo => repo.UpdateWorkoutAsync(userId, workoutDto)).ReturnsAsync(true);

            // Act
            var result = await _workoutService.UpdateWorkoutAsync(userId, workoutDto);

            // Assert
            Assert.True(result);
            _workoutRepositoryMock.Verify(repo => repo.UpdateWorkoutAsync(userId, workoutDto), Times.Once);
        }

        [Fact]
        public async Task DeleteWorkoutAsync_ShouldReturnTrue_WhenWorkoutIsDeleted()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;
            _workoutRepositoryMock.Setup(repo => repo.DeleteWorkoutAsync(userId, date)).ReturnsAsync(true);

            // Act
            var result = await _workoutService.DeleteWorkoutAsync(userId, date);

            // Assert
            Assert.True(result);
            _workoutRepositoryMock.Verify(repo => repo.DeleteWorkoutAsync(userId, date), Times.Once);
        }

        [Fact]
        public async Task DeleteWorkoutAsync_ShouldReturnFalse_WhenWorkoutDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;
            _workoutRepositoryMock.Setup(repo => repo.DeleteWorkoutAsync(userId, date)).ReturnsAsync(false);

            // Act
            var result = await _workoutService.DeleteWorkoutAsync(userId, date);

            // Assert
            Assert.False(result);
            _workoutRepositoryMock.Verify(repo => repo.DeleteWorkoutAsync(userId, date), Times.Once);
        }
    }
}
