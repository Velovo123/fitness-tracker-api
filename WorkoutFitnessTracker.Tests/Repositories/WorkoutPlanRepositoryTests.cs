using Xunit;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

public class WorkoutPlanRepositoryTests : IDisposable
{
    private readonly WFTDbContext _context;
    private readonly WorkoutPlanRepository _repository;

    public WorkoutPlanRepositoryTests()
    {
        var options = new DbContextOptionsBuilder<WFTDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        _context = new WFTDbContext(options);
        _repository = new WorkoutPlanRepository(_context);
    }

    public void Dispose()
    {
        _context.Dispose();
    }

    [Fact]
    public async Task CreateWorkoutPlanAsync_ShouldAddWorkoutPlan()
    {
        var workoutPlan = new WorkoutPlan { UserId = Guid.NewGuid(), Name = "Strength Plan", Goal = "Strength Building" };

        var result = await _repository.CreateWorkoutPlanAsync(workoutPlan);

        Assert.True(result);
        Assert.Equal(1, _context.WorkoutPlans.Count());
    }

    [Fact]
    public async Task GetWorkoutPlansAsync_ShouldReturnWorkoutPlans_ForGivenUserId()
    {
        var userId = Guid.NewGuid();
        _context.WorkoutPlans.AddRange(new WorkoutPlan { UserId = userId, Name = "Plan A" }, new WorkoutPlan { UserId = userId, Name = "Plan B" });
        _context.SaveChanges();

        var result = await _repository.GetWorkoutPlansAsync(userId, new WorkoutPlanQueryParams());

        Assert.Equal(2, result.Count());
    }

    [Fact]
    public async Task GetWorkoutPlanByNameAsync_ShouldReturnCorrectWorkoutPlan()
    {
        var userId = Guid.NewGuid();
        var planName = "Plan A";
        _context.WorkoutPlans.Add(new WorkoutPlan { UserId = userId, Name = planName });
        _context.SaveChanges();

        var result = await _repository.GetWorkoutPlanByNameAsync(userId, planName);

        Assert.NotNull(result);
        Assert.Equal(planName, result?.Name);
    }

    [Fact]
    public async Task DeleteWorkoutPlanAsync_ShouldRemoveWorkoutPlan()
    {
        var userId = Guid.NewGuid();
        var planName = "Plan to Delete";
        var plan = new WorkoutPlan { UserId = userId, Name = planName };
        _context.WorkoutPlans.Add(plan);
        _context.SaveChanges();

        var result = await _repository.DeleteWorkoutPlanAsync(userId, planName);

        Assert.True(result);
        Assert.Empty(_context.WorkoutPlans.Where(wp => wp.Name == planName));
    }
}
