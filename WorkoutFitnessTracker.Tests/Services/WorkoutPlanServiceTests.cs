using Xunit;
using Moq;
using System;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Services;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using AutoMapper;
using System.Collections.Generic;
using WorkoutFitnessTrackerAPI.Models;
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
    public async Task CreateWorkoutPlanAsync_ShouldCreateWorkoutPlan_WhenOverwriteIsFalseAndNoConflict()
    {
        var userId = Guid.NewGuid();
        var workoutPlanDto = new WorkoutPlanDto { Name = "New Plan", Goal = "Cardio" };

        _repositoryMock.Setup(r => r.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name)).ReturnsAsync((WorkoutPlan?)null);
        _repositoryMock.Setup(r => r.CreateWorkoutPlanAsync(It.IsAny<WorkoutPlan>())).ReturnsAsync(true);

        var result = await _service.CreateWorkoutPlanAsync(userId, workoutPlanDto);

        Assert.True(result);
        _repositoryMock.Verify(r => r.CreateWorkoutPlanAsync(It.IsAny<WorkoutPlan>()), Times.Once);
    }

    [Fact]
    public async Task CreateWorkoutPlanAsync_ShouldOverwriteWorkoutPlan_WhenOverwriteIsTrue()
    {
        var userId = Guid.NewGuid();
        var workoutPlanDto = new WorkoutPlanDto { Name = "Plan to Overwrite", Goal = "Strength" };

        _repositoryMock.Setup(r => r.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name))
            .ReturnsAsync(new WorkoutPlan { Name = workoutPlanDto.Name });
        _repositoryMock.Setup(r => r.UpdateWorkoutPlanAsync(It.IsAny<WorkoutPlan>())).ReturnsAsync(true);

        var result = await _service.CreateWorkoutPlanAsync(userId, workoutPlanDto, overwrite: true);

        Assert.True(result);
        _repositoryMock.Verify(r => r.UpdateWorkoutPlanAsync(It.IsAny<WorkoutPlan>()), Times.Once);
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
    public async Task DeleteWorkoutPlanAsync_ShouldReturnTrue_WhenWorkoutPlanExists()
    {
        var userId = Guid.NewGuid();
        var planName = "Plan to Delete";

        _repositoryMock.Setup(r => r.DeleteWorkoutPlanAsync(userId, planName)).ReturnsAsync(true);

        var result = await _service.DeleteWorkoutPlanAsync(userId, planName);

        Assert.True(result);
        _repositoryMock.Verify(r => r.DeleteWorkoutPlanAsync(userId, planName), Times.Once);
    }
}
