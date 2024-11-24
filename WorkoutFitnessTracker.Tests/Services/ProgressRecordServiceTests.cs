using Xunit;
using Moq;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services;
using WorkoutFitnessTrackerAPI.Services.IServices;
using AutoMapper;
using WorkoutFitnessTrackerAPI.Mappings;

public class ProgressRecordServiceTests
{
    private readonly Mock<IProgressRecordRepository> _progressRecordRepositoryMock;
    private readonly Mock<IExerciseService> _exerciseServiceMock;
    private readonly IMapper _mapper;
    private readonly ProgressRecordService _progressRecordService;

    public ProgressRecordServiceTests()
    {
        _progressRecordRepositoryMock = new Mock<IProgressRecordRepository>();
        _exerciseServiceMock = new Mock<IExerciseService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<ProgressRecordMappingProfile>();
        });
        _mapper = config.CreateMapper();

        _progressRecordService = new ProgressRecordService(
            _progressRecordRepositoryMock.Object,
            _exerciseServiceMock.Object,
            _mapper);
    }

    [Fact]
    public async Task GetProgressRecordsAsync_ShouldReturnMappedDtos()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var queryParams = new ProgressRecordQueryParams();
        var records = new List<ProgressRecord> { new ProgressRecord() };
        _progressRecordRepositoryMock.Setup(r => r.GetProgressRecordsAsync(userId, queryParams)).ReturnsAsync(records);

        // Act
        var result = await _progressRecordService.GetProgressRecordsAsync(userId, queryParams);

        // Assert
        Assert.NotNull(result);
        _progressRecordRepositoryMock.Verify(r => r.GetProgressRecordsAsync(userId, queryParams), Times.Once);
    }

    [Fact]
    public async Task GetProgressRecordByDateAsync_ShouldReturnMappedDto_WhenRecordExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.Now;
        var exerciseName = "Bench Press";
        var normalizedExerciseName = exerciseName.ToLowerInvariant();
        var record = new ProgressRecord { Date = date };

        _exerciseServiceMock.Setup(s => s.NormalizeExerciseNameAsync(exerciseName)).ReturnsAsync(normalizedExerciseName);
        _progressRecordRepositoryMock.Setup(r => r.GetProgressRecordByDateAsync(userId, date, normalizedExerciseName)).ReturnsAsync(record);

        // Act
        var result = await _progressRecordService.GetProgressRecordByDateAsync(userId, date, exerciseName);

        // Assert
        Assert.NotNull(result);
        _progressRecordRepositoryMock.Verify(r => r.GetProgressRecordByDateAsync(userId, date, normalizedExerciseName), Times.Once);
    }

    [Fact]
    public async Task CreateProgressRecordAsync_ShouldUpdateExistingRecord_WhenOverwriteIsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var progressRecordDto = new ProgressRecordDto { ExerciseName = "Bench Press", Date = DateTime.Now };
        var normalizedExerciseName = progressRecordDto.ExerciseName.ToLowerInvariant();
        var existingRecord = new ProgressRecord { Date = progressRecordDto.Date };
        var exercise = new Exercise { Id = Guid.NewGuid(), Name = normalizedExerciseName };

        _exerciseServiceMock.Setup(s => s.NormalizeExerciseNameAsync(progressRecordDto.ExerciseName)).ReturnsAsync(normalizedExerciseName);
        _exerciseServiceMock.Setup(s => s.GetExerciseByNormalizedNameAsync(normalizedExerciseName)).ReturnsAsync(exercise);
        _exerciseServiceMock.Setup(s => s.EnsureUserExerciseLinkAsync(userId, exercise.Name)).ReturnsAsync(true);
        _progressRecordRepositoryMock.Setup(r => r.GetProgressRecordByDateAsync(userId, progressRecordDto.Date, normalizedExerciseName)).ReturnsAsync(existingRecord);
        _progressRecordRepositoryMock.Setup(r => r.UpdateProgressRecordAsync(It.IsAny<ProgressRecord>())).ReturnsAsync(true);

        // Act
        var result = await _progressRecordService.CreateProgressRecordAsync(userId, progressRecordDto, overwrite: true);

        // Assert
        Assert.True(result);
        _progressRecordRepositoryMock.Verify(r => r.UpdateProgressRecordAsync(It.IsAny<ProgressRecord>()), Times.Once);
    }

    [Fact]
    public async Task CreateProgressRecordAsync_ShouldThrowException_WhenRecordExistsAndOverwriteIsFalse()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var progressRecordDto = new ProgressRecordDto { ExerciseName = "Bench Press", Date = DateTime.Now };
        var normalizedExerciseName = progressRecordDto.ExerciseName.ToLowerInvariant();
        var existingRecord = new ProgressRecord { Date = progressRecordDto.Date };
        var exercise = new Exercise { Id = Guid.NewGuid(), Name = normalizedExerciseName };

        _exerciseServiceMock.Setup(s => s.NormalizeExerciseNameAsync(progressRecordDto.ExerciseName)).ReturnsAsync(normalizedExerciseName);
        _exerciseServiceMock.Setup(s => s.GetExerciseByNormalizedNameAsync(normalizedExerciseName)).ReturnsAsync(exercise);
        _exerciseServiceMock.Setup(s => s.EnsureUserExerciseLinkAsync(userId, exercise.Name)).ReturnsAsync(true);
        _progressRecordRepositoryMock.Setup(r => r.GetProgressRecordByDateAsync(userId, progressRecordDto.Date, normalizedExerciseName)).ReturnsAsync(existingRecord);

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => _progressRecordService.CreateProgressRecordAsync(userId, progressRecordDto, overwrite: false));
    }

    [Fact]
    public async Task UpdateProgressRecordAsync_ShouldUpdateRecord_WhenRecordExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var progressRecordDto = new ProgressRecordDto { ExerciseName = "Bench Press", Date = DateTime.Now };
        var normalizedExerciseName = progressRecordDto.ExerciseName.ToLowerInvariant();
        var exercise = new Exercise { Id = Guid.NewGuid(), Name = normalizedExerciseName };

        _exerciseServiceMock.Setup(s => s.NormalizeExerciseNameAsync(progressRecordDto.ExerciseName)).ReturnsAsync(normalizedExerciseName);
        _exerciseServiceMock.Setup(s => s.GetExerciseByNormalizedNameAsync(normalizedExerciseName)).ReturnsAsync(exercise);
        _exerciseServiceMock.Setup(s => s.EnsureUserExerciseLinkAsync(userId, exercise.Name)).ReturnsAsync(true);
        _progressRecordRepositoryMock.Setup(r => r.UpdateProgressRecordAsync(It.IsAny<ProgressRecord>())).ReturnsAsync(true);

        // Act
        var result = await _progressRecordService.UpdateProgressRecordAsync(userId, progressRecordDto);

        // Assert
        Assert.True(result);
        _progressRecordRepositoryMock.Verify(r => r.UpdateProgressRecordAsync(It.IsAny<ProgressRecord>()), Times.Once);
    }

    [Fact]
    public async Task DeleteProgressRecordAsync_ShouldReturnTrue_WhenRecordExists()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.Now;
        var exerciseName = "Bench Press";
        var normalizedExerciseName = exerciseName.ToLowerInvariant();

        _exerciseServiceMock.Setup(s => s.NormalizeExerciseNameAsync(exerciseName)).ReturnsAsync(normalizedExerciseName);
        _progressRecordRepositoryMock.Setup(r => r.DeleteProgressRecordAsync(userId, date, normalizedExerciseName)).ReturnsAsync(true);

        // Act
        var result = await _progressRecordService.DeleteProgressRecordAsync(userId, date, exerciseName);

        // Assert
        Assert.True(result);
        _progressRecordRepositoryMock.Verify(r => r.DeleteProgressRecordAsync(userId, date, normalizedExerciseName), Times.Once);
    }

    [Fact]
    public async Task DeleteProgressRecordAsync_ShouldReturnFalse_WhenRecordDoesNotExist()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var date = DateTime.Now;
        var exerciseName = "Bench Press";
        var normalizedExerciseName = exerciseName.ToLowerInvariant();

        _exerciseServiceMock.Setup(s => s.NormalizeExerciseNameAsync(exerciseName)).ReturnsAsync(normalizedExerciseName);
        _progressRecordRepositoryMock.Setup(r => r.DeleteProgressRecordAsync(userId, date, normalizedExerciseName)).ReturnsAsync(false);

        // Act
        var result = await _progressRecordService.DeleteProgressRecordAsync(userId, date, exerciseName);

        // Assert
        Assert.False(result);
        _progressRecordRepositoryMock.Verify(r => r.DeleteProgressRecordAsync(userId, date, normalizedExerciseName), Times.Once);
    }
}
