using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTracker.API.Services.IServices
{
    public interface IWorkoutPlanService
    {
        Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, WorkoutPlanQueryParams queryParams);
        Task<WorkoutPlanDto?> GetWorkoutPlanByNameAsync(Guid userId, string planName);
        Task<bool> CreateWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto, bool overwrite = false);
        Task<bool> UpdateWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto);
        Task<bool> DeleteWorkoutPlanAsync(Guid userId, string planName);
    }
}
