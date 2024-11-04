using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Mappings;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Xunit;

namespace WorkoutFitnessTrackerAPI.Tests.Repositories
{
    public class ProgressRecordRepositoryTests : IDisposable
    {
        private readonly WFTDbContext _context;
        private readonly ProgressRecordRepository _repository;
        private readonly Mock<IExerciseService> _exerciseServiceMock;
        private readonly IMapper _mapper;
        private readonly Guid _testUserId = Guid.NewGuid();

        public ProgressRecordRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<WFTDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDatabase")
                .Options;

            _context = new WFTDbContext(options);
            _exerciseServiceMock = new Mock<IExerciseService>();

            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddProfile<ProgressRecordMappingProfile>(); 
            });
            _mapper = config.CreateMapper();

            _repository = new ProgressRecordRepository(_context, _exerciseServiceMock.Object, _mapper);

            SeedDatabase();
        }

        private void SeedDatabase()
        {
            var exercise = new Exercise { Name = "pushup" };
            var progressRecord = new ProgressRecord
            {
                UserId = _testUserId,
                Exercise = exercise,
                Date = DateTime.Today,
                Progress = "15 reps"
            };

            _context.Exercises.Add(exercise);
            _context.ProgressRecords.Add(progressRecord);
            _context.SaveChanges();
        }

        public void Dispose()
        {
            _context.Dispose();
        }

        [Fact]
        public async Task GetProgressRecordsAsync_ReturnsRecordsWithCorrectData()
        {
            // Arrange
            var queryParams = new ProgressRecordQueryParams();

            // Act
            var result = await _repository.GetProgressRecordsAsync(_testUserId, queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.All(result, record =>
            {
                Assert.NotNull(record.ExerciseName);
                Assert.NotEqual(DateTime.MinValue, record.Date);
                Assert.False(string.IsNullOrEmpty(record.Progress));
            });
        }

        [Fact]
        public async Task GetProgressRecordByDateAsync_ReturnsCorrectRecord()
        {
            // Act
            var result = await _repository.GetProgressRecordByDateAsync(_testUserId, DateTime.Today, "Push Up");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("pushup", result!.ExerciseName);
        }

        [Fact]
        public async Task SaveProgressRecordAsync_CreatesOrUpdatesRecordCorrectly()
        {
            // Arrange
            var exerciseName = "Push Up";
            var exercise = new Exercise { Id = Guid.NewGuid(), Name = "pushup" };

            _context.Exercises.Add(exercise);

            _context.UserExercises.Add(new UserExercise
            {
                UserId = _testUserId,
                ExerciseId = exercise.Id
            });

            await _context.SaveChangesAsync();

            var progressRecordDto = new ProgressRecordDto
            {
                ExerciseName = exerciseName,
                Date = DateTime.Today,
                Progress = "20 reps"
            };

            _exerciseServiceMock.Setup(x => x.GetExerciseByNormalizedNameAsync(exerciseName))
                .ReturnsAsync(exercise);

            // Act - Test creating a new record
            var createResult = await _repository.SaveProgressRecordAsync(_testUserId, progressRecordDto, overwrite: false);

            // Assert
            Assert.True(createResult);
        }


        [Fact]
        public async Task DeleteProgressRecordAsync_RemovesRecordIfExists()
        {
            // Arrange
            var date = DateTime.Today;
            var exerciseName = "Push Up";

            // Act
            var deleteResult = await _repository.DeleteProgressRecordAsync(_testUserId, date, exerciseName);
            var recordExists = await _context.ProgressRecords
                .AnyAsync(pr => pr.UserId == _testUserId && pr.Date.Date == date && pr.Exercise.Name == exerciseName);

            // Assert
            Assert.True(deleteResult);
            Assert.False(recordExists);
        }

        [Fact]
        public async Task SaveProgressRecordAsync_ThrowsWhenExerciseNotLinkedToUser()
        {
            // Arrange
            var progressRecordDto = new ProgressRecordDto
            {
                ExerciseName = "Nonexistent Exercise",
                Date = DateTime.Today,
                Progress = "20 reps"
            };

            _exerciseServiceMock.Setup(x => x.GetExerciseByNormalizedNameAsync("nonexistent-exercise"))
                .ReturnsAsync((Exercise)null);

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.SaveProgressRecordAsync(_testUserId, progressRecordDto, overwrite: false));
        }

        [Fact]
        public async Task SaveProgressRecordAsync_ThrowsWhenRecordAlreadyExistsAndNoOverwrite()
        {
            // Arrange
            var progressRecordDto = new ProgressRecordDto
            {
                ExerciseName = "Push Up",
                Date = DateTime.Today,
                Progress = "20 reps"
            };

            _exerciseServiceMock.Setup(x => x.GetExerciseByNormalizedNameAsync("push-up"))
                .ReturnsAsync(new Exercise { Name = "Push Up" });

            // Act & Assert
            await Assert.ThrowsAsync<InvalidOperationException>(() => _repository.SaveProgressRecordAsync(_testUserId, progressRecordDto, overwrite: false));
        }

        [Fact]
        public async Task SaveProgressRecordAsync_ThrowsArgumentNullException_WhenProgressRecordDtoIsNull()
        {
            // Act & Assert
            await Assert.ThrowsAsync<ArgumentNullException>(() => _repository.SaveProgressRecordAsync(_testUserId, null));
        }

        [Fact]
        public async Task SaveProgressRecordAsync_ThrowsArgumentException_WhenProgressRecordDtoExerciseNameIsEmpty()
        {
            // Arrange
            var progressRecordDto = new ProgressRecordDto
            {
                ExerciseName = string.Empty, // Invalid name
                Date = DateTime.Today,
                Progress = "20 reps"
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.SaveProgressRecordAsync(_testUserId, progressRecordDto));
        }

        [Fact]
        public async Task SaveProgressRecordAsync_ThrowsArgumentException_WhenProgressRecordDtoProgressIsEmpty()
        {
            // Arrange
            var progressRecordDto = new ProgressRecordDto
            {
                ExerciseName = "Push Up",
                Date = DateTime.Today,
                Progress = string.Empty // Empty progress field
            };

            // Act & Assert
            await Assert.ThrowsAsync<ArgumentException>(() => _repository.SaveProgressRecordAsync(_testUserId, progressRecordDto));
        }
    }
}
