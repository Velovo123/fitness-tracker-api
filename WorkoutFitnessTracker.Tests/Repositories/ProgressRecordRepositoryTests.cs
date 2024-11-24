using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories;
using Xunit;

namespace WorkoutFitnessTrackerAPI.Tests.Repositories
{
    public class ProgressRecordRepositoryTests
    {
        private readonly WFTDbContext _context;
        private readonly ProgressRecordRepository _repository;

        public ProgressRecordRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<WFTDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new WFTDbContext(options);
            _repository = new ProgressRecordRepository(_context);
        }

        [Fact]
        public async Task GetProgressRecordsAsync_ShouldReturnFilteredRecords()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _context.ProgressRecords.AddRange(
                new ProgressRecord { UserId = userId, Date = DateTime.Now, Progress = "50 reps", Exercise = new Exercise { Name = "Squat" } },
                new ProgressRecord { UserId = userId, Date = DateTime.Now.AddDays(-1), Progress = "60 reps", Exercise = new Exercise { Name = "Deadlift" } }
            );
            await _context.SaveChangesAsync();

            var queryParams = new ProgressRecordQueryParams { PageNumber = 1, PageSize = 10 };

            // Act
            var result = await _repository.GetProgressRecordsAsync(userId, queryParams);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(2, result.Count());
        }

        [Fact]
        public async Task GetProgressRecordByDateAsync_ShouldReturnSpecificRecord()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;

            var progressRecord = new ProgressRecord
            {
                UserId = userId,
                Date = date,
                Progress = "75 reps",
                Exercise = new Exercise { Name = "Bench Press" }
            };
            _context.ProgressRecords.Add(progressRecord);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.GetProgressRecordByDateAsync(userId, date, "Bench Press");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("75 reps", result.Progress);
        }

        [Fact]
        public async Task CreateProgressRecordAsync_ShouldAddRecordToDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var progressRecord = new ProgressRecord
            {
                UserId = userId,
                Date = DateTime.Now,
                Progress = "85 reps",
                ExerciseId = Guid.NewGuid()
            };

            // Act
            var result = await _repository.CreateProgressRecordAsync(progressRecord);
            var createdRecord = await _context.ProgressRecords.FirstOrDefaultAsync(pr => pr.UserId == userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(createdRecord);
            Assert.Equal("85 reps", createdRecord.Progress);
        }

        [Fact]
        public async Task UpdateProgressRecordAsync_ShouldUpdateExistingRecord()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now;

            var progressRecord = new ProgressRecord
            {
                UserId = userId,
                Date = date,
                Progress = "65 reps",
                ExerciseId = Guid.NewGuid()
            };
            _context.ProgressRecords.Add(progressRecord);
            await _context.SaveChangesAsync();

            // Modify Progress
            progressRecord.Progress = "95 reps";

            // Act
            var result = await _repository.UpdateProgressRecordAsync(progressRecord);
            var updatedRecord = await _context.ProgressRecords.FirstOrDefaultAsync(pr => pr.UserId == userId && pr.Date == date);

            // Assert
            Assert.True(result);
            Assert.Equal("95 reps", updatedRecord.Progress);
        }

        [Fact]
        public async Task DeleteProgressRecordAsync_ShouldRemoveRecordFromDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now;

            var progressRecord = new ProgressRecord
            {
                UserId = userId,
                Date = date,
                Progress = "70 reps",
                Exercise = new Exercise { Name = "Pull Up" }
            };
            _context.ProgressRecords.Add(progressRecord);
            await _context.SaveChangesAsync();

            // Act
            var result = await _repository.DeleteProgressRecordAsync(userId, date, "Pull Up");
            var deletedRecord = await _context.ProgressRecords.FirstOrDefaultAsync(pr => pr.UserId == userId && pr.Date == date);

            // Assert
            Assert.True(result);
            Assert.Null(deletedRecord);
        }

        [Fact]
        public async Task GetProgressRecordsAsync_WithPaging_ShouldReturnLimitedResults()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _context.ProgressRecords.AddRange(
                new ProgressRecord { UserId = userId, Date = DateTime.Now, Progress = "50 reps", Exercise = new Exercise { Name = "Squat" } },
                new ProgressRecord { UserId = userId, Date = DateTime.Now.AddDays(-1), Progress = "60 reps", Exercise = new Exercise { Name = "Deadlift" } }
            );
            await _context.SaveChangesAsync();

            var queryParams = new ProgressRecordQueryParams { PageNumber = 1, PageSize = 1 };

            // Act
            var result = await _repository.GetProgressRecordsAsync(userId, queryParams);

            // Assert
            Assert.Single(result);
        }

        [Fact]
        public async Task GetProgressRecordsAsync_WithFilters_ShouldApplyFilterCorrectly()
        {
            // Arrange
            var userId = Guid.NewGuid();
            _context.ProgressRecords.AddRange(
                new ProgressRecord { UserId = userId, Date = DateTime.Now, Progress = "50 reps", Exercise = new Exercise { Name = "Squat" } },
                new ProgressRecord { UserId = userId, Date = DateTime.Now.AddDays(-1), Progress = "60 reps", Exercise = new Exercise { Name = "Deadlift" } }
            );
            await _context.SaveChangesAsync();

            var queryParams = new ProgressRecordQueryParams { ExerciseName = "Squat" };

            // Act
            var result = await _repository.GetProgressRecordsAsync(userId, queryParams);

            // Assert
            Assert.Single(result);
            Assert.Equal("Squat", result.First().Exercise.Name);
        }
    }
}
