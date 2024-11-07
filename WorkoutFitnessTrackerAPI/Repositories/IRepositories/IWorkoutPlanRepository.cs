using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IWorkoutPlanRepository
    {
        Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, WorkoutPlanQueryParams queryParams);
        Task<WorkoutPlanDto?> GetWorkoutPlanByNameAsync(Guid userId, string planName);
        Task<bool> CreateWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto);
        Task<bool> UpdateWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto);
        Task<bool> DeleteWorkoutPlanAsync(Guid userId, string planName);
    }
}
