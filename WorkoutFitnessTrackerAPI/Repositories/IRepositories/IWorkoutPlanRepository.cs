using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IWorkoutPlanRepository
    {
        Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, WorkoutPlanQueryParams queryParams);
        Task<WorkoutPlanDto?> GetWorkoutPlanByNameAsync(Guid userId, string planName);
        Task<bool> SaveWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto, bool overwrite = false);
        Task<bool> DeleteWorkoutPlanAsync(Guid userId, string planName);
    }
}
