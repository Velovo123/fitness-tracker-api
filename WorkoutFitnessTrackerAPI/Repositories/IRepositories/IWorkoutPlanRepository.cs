using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IWorkoutPlanRepository
    {
        Task<IEnumerable<WorkoutPlan>> GetWorkoutPlansAsync(Guid userId, WorkoutPlanQueryParams queryParams);
        Task<WorkoutPlan?> GetWorkoutPlanByNameAsync(Guid userId, string planName);
        Task<bool> CreateWorkoutPlanAsync(WorkoutPlan workoutPlan);
        Task<bool> UpdateWorkoutPlanAsync(WorkoutPlan workoutPlan);
        Task<bool> DeleteWorkoutPlanAsync(Guid userId, string planName);
    }
}
