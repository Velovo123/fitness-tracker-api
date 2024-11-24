using WorkoutFitnessTracker.API.Models.Dto_s.Summary;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTracker.API.Services.IServices
{
    public interface IWorkoutService
    {
        Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(Guid userId, WorkoutQueryParams queryParams);
        Task<IEnumerable<WorkoutDto>> GetWorkoutsByDateAsync(Guid userId, DateTime date);
        Task<bool> CreateWorkoutAsync(Guid userId, WorkoutDto workoutDto, bool overwrite = false);
        Task<bool> UpdateWorkoutAsync(Guid userId, WorkoutDto workoutDto);
        Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date);

    }
}
