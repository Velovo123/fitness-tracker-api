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
    public class ProgressRecordRepositoryTests
    {
        private readonly WFTDbContext _context;
        private readonly ProgressRecordRepository _progressRecordRepository;
        private readonly IMapper _mapper;
        private readonly Mock<IExerciseService> _exerciseServiceMock;

        public ProgressRecordRepositoryTests()
        {
            var options = new DbContextOptionsBuilder<WFTDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new WFTDbContext(options);

            var config = new MapperConfiguration(cfg => cfg.AddProfile<ProgressRecordMappingProfile>());
            _mapper = config.CreateMapper();

            _exerciseServiceMock = new Mock<IExerciseService>();
            _progressRecordRepository = new ProgressRecordRepository(_context, _exerciseServiceMock.Object, _mapper);
        }

        [Fact]
        public async Task GetProgressRecordByDateAsync_ShouldReturnRecordByDateAndExercise()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;
            var normalizedExerciseName = "pushup";
            var exercise = new Exercise { Name = normalizedExerciseName };
            _context.Exercises.Add(exercise);

            _context.ProgressRecords.Add(new ProgressRecord
            {
                UserId = userId,
                Exercise = exercise,
                Date = date,
                Progress = "Improved form"
            });
            await _context.SaveChangesAsync();

            // Act
            var result = await _progressRecordRepository.GetProgressRecordByDateAsync(userId, date, "Push Up");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Improved form", result.Progress);
        }

        [Fact]
        public async Task CreateProgressRecordAsync_ShouldAddRecordToDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var normalizedExerciseName = "pushup";

            // Add the exercise with the normalized name directly to the database
            var exercise = new Exercise { Name = normalizedExerciseName };
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();

            // Mock the exercise service to return the exercise based on the normalized name
            _exerciseServiceMock
                .Setup(service => service.GetExerciseByNormalizedNameAsync(normalizedExerciseName))
                .ReturnsAsync(exercise);

            // Create the DTO with the normalized name, simulating the input as normalized
            var progressRecordDto = new ProgressRecordDto
            {
                Date = DateTime.Now,
                ExerciseName = "pushup", // Ensure name is already normalized
                Progress = "New milestone"
            };

            // Act
            var result = await _progressRecordRepository.CreateProgressRecordAsync(userId, progressRecordDto);
            var createdRecord = await _context.ProgressRecords.FirstOrDefaultAsync(pr => pr.UserId == userId);

            // Assert
            Assert.True(result);
            Assert.NotNull(createdRecord);
            Assert.Equal("New milestone", createdRecord.Progress);
            Assert.Equal(normalizedExerciseName, createdRecord.Exercise.Name);
        }


        [Fact]
        public async Task UpdateProgressRecordAsync_ShouldUpdateExistingRecord()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;
            var normalizedExerciseName = "pushup";
            var exercise = new Exercise { Id = Guid.NewGuid(), Name = normalizedExerciseName };
            _context.Exercises.Add(exercise);

            var progressRecord = new ProgressRecord
            {
                UserId = userId,
                ExerciseId = exercise.Id,
                Date = date,
                Progress = "Initial progress"
            };
            _context.ProgressRecords.Add(progressRecord);
            await _context.SaveChangesAsync();

            var updatedDto = new ProgressRecordDto
            {
                Date = date,
                ExerciseName = "pushup",
                Progress = "Updated progress"
            };

            _exerciseServiceMock
                .Setup(service => service.GetExerciseByNormalizedNameAsync(normalizedExerciseName))
                .ReturnsAsync(exercise);

            // Act
            var result = await _progressRecordRepository.UpdateProgressRecordAsync(userId, updatedDto);
            var updatedRecord = await _context.ProgressRecords.FirstOrDefaultAsync(pr => pr.UserId == userId && pr.Date == date);

            // Assert
            Assert.True(result);
            Assert.NotNull(updatedRecord);
            Assert.Equal("Updated progress", updatedRecord.Progress);
        }

        [Fact]
        public async Task DeleteProgressRecordAsync_ShouldRemoveRecordFromDatabase()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;
            var normalizedExerciseName = "pushup";
            var exercise = new Exercise { Name = normalizedExerciseName };
            _context.Exercises.Add(exercise);

            var progressRecord = new ProgressRecord
            {
                UserId = userId,
                Exercise = exercise,
                Date = date,
                Progress = "To be deleted"
            };
            _context.ProgressRecords.Add(progressRecord);
            await _context.SaveChangesAsync();

            // Act
            var result = await _progressRecordRepository.DeleteProgressRecordAsync(userId, date, "Push Up");
            var deletedRecord = await _context.ProgressRecords.FirstOrDefaultAsync(pr => pr.UserId == userId && pr.Date == date);

            // Assert
            Assert.True(result);
            Assert.Null(deletedRecord);
        }

        [Fact]
        public async Task DeleteProgressRecordAsync_NonExistentRecord_ShouldReturnFalse()
        {
            // Arrange
            var userId = Guid.NewGuid();
            var date = DateTime.Now.Date;

            // Act
            var result = await _progressRecordRepository.DeleteProgressRecordAsync(userId, date, "NonExistent Exercise");

            // Assert
            Assert.False(result);
        }
    }
}
