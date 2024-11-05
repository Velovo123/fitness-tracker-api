using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Services;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;

namespace WorkoutFitnessTracker.Tests.Services
{
    public class InsightsServiceTests
    {
        private readonly Mock<IWorkoutRepository> _workoutRepositoryMock;
        private readonly Mock<IProgressRecordRepository> _progressRecordRepositoryMock;
        private readonly InsightsService _insightsService;

        public InsightsServiceTests()
        {
            _workoutRepositoryMock = new Mock<IWorkoutRepository>();
            _progressRecordRepositoryMock = new Mock<IProgressRecordRepository>();
            _insightsService = new InsightsService(_workoutRepositoryMock.Object, _progressRecordRepositoryMock.Object);
        }

        [Fact]
        public async Task GetWeeklyOrMonthlySummaryAsync_WithMultipleWorkoutsAndProgress_ShouldReturnExpectedSummary()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = new DateTime(2023, 10, 1);
            var endDate = new DateTime(2023, 10, 31);

            var workoutDtoList = new List<WorkoutDto>
            {
                new WorkoutDto
                {
                    Date = new DateTime(2023, 10, 5),
                    Duration = 60,
                    Exercises = new List<WorkoutExerciseDto>
                    {
                        new WorkoutExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 15 }
                    }
                },
                new WorkoutDto
                {
                    Date = new DateTime(2023, 10, 15),
                    Duration = 45,
                    Exercises = new List<WorkoutExerciseDto>
                    {
                        new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 10 },
                        new WorkoutExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 12 }
                    }
                }
            };

            var progressRecordList = new List<ProgressRecordDto>
            {
                new ProgressRecordDto
                {
                    Date = new DateTime(2023, 10, 5),
                    Progress = "Improved form",
                    ExerciseName = "Push Up"
                },
                new ProgressRecordDto
                {
                    Date = new DateTime(2023, 10, 15),
                    Progress = "Increased reps",
                    ExerciseName = "Squat"
                }
            };

            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(userId, It.IsAny<WorkoutQueryParams>()))
                .ReturnsAsync(workoutDtoList);

            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordsAsync(userId, It.IsAny<ProgressRecordQueryParams>()))
                .ReturnsAsync(progressRecordList);

            // Act
            var result = await _insightsService.GetWeeklyOrMonthlySummaryAsync(userId, startDate, endDate);

            // Assert
            Assert.Equal(2, result.TotalWorkouts);
            Assert.Equal(105, result.TotalDuration);
            Assert.Equal(2, result.UniqueExercises);
            Assert.Equal(2, result.ExerciseFrequency["Push Up"]);
            Assert.Equal(1, result.ExerciseFrequency["Squat"]);
            Assert.Contains("Improved in Push Up: Improved form", result.Achievements);
            Assert.Contains("Improved in Squat: Increased reps", result.Achievements);
        }

        [Fact]
        public async Task GetWeeklyOrMonthlySummaryAsync_WithNoWorkouts_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = new DateTime(2023, 10, 1);
            var endDate = new DateTime(2023, 10, 31);

            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(userId, It.IsAny<WorkoutQueryParams>()))
                .ReturnsAsync(new List<WorkoutDto>()); // No workouts found

            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordsAsync(userId, It.IsAny<ProgressRecordQueryParams>()))
                .ReturnsAsync(new List<ProgressRecordDto>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _insightsService.GetWeeklyOrMonthlySummaryAsync(userId, startDate, endDate));

            Assert.Equal("No workouts found within the specified date range.", exception.Message);
        }


        [Fact]
        public async Task GetWeeklyOrMonthlySummaryAsync_SingleWorkoutWithMultipleExercises_ShouldCalculateExerciseFrequencyCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = new DateTime(2023, 10, 1);
            var endDate = new DateTime(2023, 10, 31);

            var workoutDtoList = new List<WorkoutDto>
            {
                new WorkoutDto
                {
                    Date = new DateTime(2023, 10, 5),
                    Duration = 60,
                    Exercises = new List<WorkoutExerciseDto>
                    {
                        new WorkoutExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 15 },
                        new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 10 }
                    }
                }
            };

            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(userId, It.IsAny<WorkoutQueryParams>()))
                .ReturnsAsync(workoutDtoList);

            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordsAsync(userId, It.IsAny<ProgressRecordQueryParams>()))
                .ReturnsAsync(new List<ProgressRecordDto>());

            // Act
            var result = await _insightsService.GetWeeklyOrMonthlySummaryAsync(userId, startDate, endDate);

            // Assert
            Assert.Equal(1, result.TotalWorkouts);
            Assert.Equal(60, result.TotalDuration);
            Assert.Equal(2, result.UniqueExercises);
            Assert.Equal(1, result.ExerciseFrequency["Push Up"]);
            Assert.Equal(1, result.ExerciseFrequency["Squat"]);
        }

        [Fact]
        public async Task GetWeeklyOrMonthlySummaryAsync_WorkoutOnEndDate_ShouldIncludeEndDateData()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = new DateTime(2023, 10, 1);
            var endDate = new DateTime(2023, 10, 31);

            var workoutDtoList = new List<WorkoutDto>
            {
                new WorkoutDto
                {
                    Date = endDate, // Exact end date
                    Duration = 50,
                    Exercises = new List<WorkoutExerciseDto>
                    {
                        new WorkoutExerciseDto { ExerciseName = "Bench Press", Sets = 3, Reps = 10 }
                    }
                }
            };

            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(userId, It.IsAny<WorkoutQueryParams>()))
                .ReturnsAsync(workoutDtoList);

            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordsAsync(userId, It.IsAny<ProgressRecordQueryParams>()))
                .ReturnsAsync(new List<ProgressRecordDto>());

            // Act
            var result = await _insightsService.GetWeeklyOrMonthlySummaryAsync(userId, startDate, endDate);

            // Assert
            Assert.Equal(1, result.TotalWorkouts);
            Assert.Equal(50, result.TotalDuration);
            Assert.Equal(1, result.UniqueExercises);
            Assert.Equal(1, result.ExerciseFrequency["Bench Press"]);
        }

        [Fact]
        public async Task GetWeeklyOrMonthlySummaryAsync_LargeDataset_ShouldReturnCorrectSummary()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = new DateTime(2023, 10, 1);
            var endDate = new DateTime(2023, 10, 31);

            // Generate a large number of workouts
            var workoutDtoList = new List<WorkoutDto>();
            for (int i = 0; i < 50; i++)
            {
                workoutDtoList.Add(new WorkoutDto
                {
                    Date = startDate.AddDays(i),
                    Duration = 45,
                    Exercises = new List<WorkoutExerciseDto>
                    {
                        new WorkoutExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 12 },
                        new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 10 }
                    }
                });
            }

            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(userId, It.IsAny<WorkoutQueryParams>()))
                .ReturnsAsync(workoutDtoList);

            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordsAsync(userId, It.IsAny<ProgressRecordQueryParams>()))
                .ReturnsAsync(new List<ProgressRecordDto>());

            // Act
            var result = await _insightsService.GetWeeklyOrMonthlySummaryAsync(userId, startDate, endDate);

            // Assert
            Assert.Equal(50, result.TotalWorkouts);
            Assert.Equal(2250, result.TotalDuration); // 50 workouts * 45 minutes each
            Assert.Equal(2, result.UniqueExercises);
            Assert.Equal(50, result.ExerciseFrequency["Push Up"]);
            Assert.Equal(50, result.ExerciseFrequency["Squat"]);
        }

        [Fact]
        public async Task GetWeeklyOrMonthlySummaryAsync_OverlappingAchievements_ShouldReturnDistinctAchievements()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = new DateTime(2023, 10, 1);
            var endDate = new DateTime(2023, 10, 31);

            var workoutDtoList = new List<WorkoutDto>
            {
                new WorkoutDto
                {
                    Date = new DateTime(2023, 10, 5),
                    Duration = 60,
                    Exercises = new List<WorkoutExerciseDto>
                    {
                        new WorkoutExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 15 }
                    }
                }
            };

            var progressRecordList = new List<ProgressRecordDto>
            {
                new ProgressRecordDto
                {
                    Date = new DateTime(2023, 10, 5),
                    Progress = "Improved form",
                    ExerciseName = "Push Up"
                },
                new ProgressRecordDto
                {
                    Date = new DateTime(2023, 10, 10),
                    Progress = "Increased reps",
                    ExerciseName = "Push Up"
                }
            };

            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(userId, It.IsAny<WorkoutQueryParams>()))
                .ReturnsAsync(workoutDtoList);

            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordsAsync(userId, It.IsAny<ProgressRecordQueryParams>()))
                .ReturnsAsync(progressRecordList);

            // Act
            var result = await _insightsService.GetWeeklyOrMonthlySummaryAsync(userId, startDate, endDate);

            // Assert
            Assert.Contains("Improved in Push Up: Improved form", result.Achievements);
            Assert.Contains("Improved in Push Up: Increased reps", result.Achievements);
        }

        [Fact]
        public async Task GetWeeklyOrMonthlySummaryAsync_NoWorkoutsFound_ShouldThrowKeyNotFoundException()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = new DateTime(2023, 10, 1);
            var endDate = new DateTime(2023, 10, 31);

            // Mock repositories to return no workouts
            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(userId, It.IsAny<WorkoutQueryParams>()))
                .ReturnsAsync(new List<WorkoutDto>());

            // Act & Assert
            var exception = await Assert.ThrowsAsync<KeyNotFoundException>(() =>
                _insightsService.GetWeeklyOrMonthlySummaryAsync(userId, startDate, endDate));

            Assert.Equal("No workouts found within the specified date range.", exception.Message);
        }

        [Fact]
        public async Task GetWeeklyOrMonthlySummaryAsync_NoProgressRecordsFound_ShouldReturnEmptyAchievements()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var startDate = new DateTime(2023, 10, 1);
            var endDate = new DateTime(2023, 10, 31);

            // Mock repositories to return workouts but no progress records
            var workoutDtoList = new List<WorkoutDto>
            {
                new WorkoutDto
                {
                    Date = new DateTime(2023, 10, 5),
                    Duration = 60,
                    Exercises = new List<WorkoutExerciseDto>
                    {
                        new WorkoutExerciseDto { ExerciseName = "Push Up", Sets = 3, Reps = 15 }
                    }
                }
            };

            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(userId, It.IsAny<WorkoutQueryParams>()))
                .ReturnsAsync(workoutDtoList);

            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordsAsync(userId, It.IsAny<ProgressRecordQueryParams>()))
                .ReturnsAsync(new List<ProgressRecordDto>());

            // Act
            var result = await _insightsService.GetWeeklyOrMonthlySummaryAsync(userId, startDate, endDate);

            // Assert
            Assert.Equal(1, result.TotalWorkouts);
            Assert.Equal(60, result.TotalDuration);
            Assert.Equal(1, result.UniqueExercises);
            Assert.Equal(1, result.ExerciseFrequency["Push Up"]);
            Assert.Empty(result.Achievements); // Expect achievements to be empty when no progress records are found
        }

    }
}
