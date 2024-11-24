using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using System.Collections.Generic;
using WorkoutFitnessTrackerAPI.Services;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using AutoMapper;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Mappings;

public class WorkoutPlanServiceTests
{
    private readonly WorkoutPlanService _service;
    private readonly Mock<IWorkoutPlanRepository> _repositoryMock;
    private readonly Mock<IExerciseService> _exerciseServiceMock;
    private readonly IMapper _mapper;

    public WorkoutPlanServiceTests()
    {
        _repositoryMock = new Mock<IWorkoutPlanRepository>();
        _exerciseServiceMock = new Mock<IExerciseService>();

        var config = new MapperConfiguration(cfg =>
        {
            cfg.AddProfile<WorkoutPlanMappingProfile>(); // Use the actual mapping profile
        });
        _mapper = config.CreateMapper();

        _service = new WorkoutPlanService(_repositoryMock.Object, _exerciseServiceMock.Object, _mapper);
    }

    [Fact]
    public async Task GetWorkoutPlansAsync_ShouldReturnWorkoutPlans_ForUserId()
    {
        var userId = Guid.NewGuid();
        var queryParams = new WorkoutPlanQueryParams();
        var workoutPlans = new List<WorkoutPlan> { new WorkoutPlan { Name = "Plan 1" }, new WorkoutPlan { Name = "Plan 2" } };

        _repositoryMock.Setup(r => r.GetWorkoutPlansAsync(userId, queryParams)).ReturnsAsync(workoutPlans);

        var result = await _service.GetWorkoutPlansAsync(userId, queryParams);

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetWorkoutPlanByNameAsync_ShouldReturnWorkoutPlan_WhenPlanExists()
    {
        var userId = Guid.NewGuid();
        var planName = "Existing Plan";
        var normalizedPlanName = NameNormalizationHelper.NormalizeName(planName);
        var existingPlan = new WorkoutPlan { Name = normalizedPlanName };

        _repositoryMock.Setup(r => r.GetWorkoutPlanByNameAsync(userId, normalizedPlanName)).ReturnsAsync(existingPlan);

        var result = await _service.GetWorkoutPlanByNameAsync(userId, planName);

        Assert.NotNull(result);
        Assert.Equal(normalizedPlanName, result!.Name);
    }

    [Fact]
    public async Task CreateWorkoutPlanAsync_ShouldCreateWorkoutPlan_WhenOverwriteIsFalseAndNoConflict()
    {
        var userId = Guid.NewGuid();
        var workoutPlanDto = new WorkoutPlanDto { Name = "New Plan", Goal = "Cardio" };
        var normalizedPlanName = NameNormalizationHelper.NormalizeName(workoutPlanDto.Name);

        _repositoryMock.Setup(r => r.GetWorkoutPlanByNameAsync(userId, normalizedPlanName)).ReturnsAsync((WorkoutPlan?)null);
        _repositoryMock.Setup(r => r.CreateWorkoutPlanAsync(It.IsAny<WorkoutPlan>())).ReturnsAsync(true);

        var result = await _service.CreateWorkoutPlanAsync(userId, workoutPlanDto);

        Assert.True(result);
        _repositoryMock.Verify(r => r.CreateWorkoutPlanAsync(It.IsAny<WorkoutPlan>()), Times.Once);
    }

    [Fact]
    public async Task CreateWorkoutPlanAsync_ShouldOverwriteWorkoutPlan_WhenOverwriteIsTrue()
    {
        // Arrange
        var userId = Guid.NewGuid();
        var workoutPlanDto = new WorkoutPlanDto { Name = "Plan to Overwrite", Goal = "Strength" };
        var normalizedPlanName = NameNormalizationHelper.NormalizeName(workoutPlanDto.Name);

        // Mock retrieval of the existing plan by name to simulate a conflict
        _repositoryMock.Setup(r => r.GetWorkoutPlanByNameAsync(userId, normalizedPlanName))
                       .ReturnsAsync(new WorkoutPlan { Name = normalizedPlanName });

        // Mock the deletion and creation steps within UpdateWorkoutPlanAsync
        _repositoryMock.Setup(r => r.DeleteWorkoutPlanAsync(userId, normalizedPlanName)).ReturnsAsync(true);
        _repositoryMock.Setup(r => r.CreateWorkoutPlanAsync(It.IsAny<WorkoutPlan>())).ReturnsAsync(true);

        // Act
        var result = await _service.CreateWorkoutPlanAsync(userId, workoutPlanDto, overwrite: true);

        // Assert
        Assert.True(result, "Expected the workout plan to be overwritten successfully.");
        _repositoryMock.Verify(r => r.DeleteWorkoutPlanAsync(userId, normalizedPlanName), Times.Once);
        _repositoryMock.Verify(r => r.CreateWorkoutPlanAsync(It.IsAny<WorkoutPlan>()), Times.Once);
        _repositoryMock.Verify(r => r.GetWorkoutPlanByNameAsync(userId, normalizedPlanName), Times.Once);
    }


    [Fact]
    public async Task CreateWorkoutPlanAsync_ShouldThrowException_WhenPlanExistsAndOverwriteIsFalse()
    {
        var userId = Guid.NewGuid();
        var workoutPlanDto = new WorkoutPlanDto { Name = "Duplicate Plan", Goal = "Strength" };
        var normalizedPlanName = NameNormalizationHelper.NormalizeName(workoutPlanDto.Name);

        _repositoryMock.Setup(r => r.GetWorkoutPlanByNameAsync(userId, normalizedPlanName))
                       .ReturnsAsync(new WorkoutPlan { Name = normalizedPlanName });

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CreateWorkoutPlanAsync(userId, workoutPlanDto));
    }

    [Fact]
    public async Task DeleteWorkoutPlanAsync_ShouldReturnTrue_WhenWorkoutPlanExists()
    {
        var userId = Guid.NewGuid();
        var planName = "Plan to Delete";
        var normalizedPlanName = NameNormalizationHelper.NormalizeName(planName);

        _repositoryMock.Setup(r => r.DeleteWorkoutPlanAsync(userId, normalizedPlanName)).ReturnsAsync(true);

        var result = await _service.DeleteWorkoutPlanAsync(userId, planName);

        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteWorkoutPlanAsync(userId, normalizedPlanName), Times.Once);
    }

    [Fact]
    public async Task DeleteWorkoutPlanAsync_ShouldReturnFalse_WhenWorkoutPlanDoesNotExist()
    {
        var userId = Guid.NewGuid();
        var planName = "Non-Existent Plan";
        var normalizedPlanName = NameNormalizationHelper.NormalizeName(planName);

        _repositoryMock.Setup(r => r.DeleteWorkoutPlanAsync(userId, normalizedPlanName)).ReturnsAsync(false);

        var result = await _service.DeleteWorkoutPlanAsync(userId, planName);

        Assert.False(result);
    }
}
