using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IWorkoutRepository
    {
        Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(Guid userId, WorkoutQueryParams queryParams);
        Task<IEnumerable<WorkoutDto>> GetWorkoutsByDateAsync(Guid userId, DateTime date);
        Task<bool> SaveWorkoutAsync(Guid userId, WorkoutDto workoutDto, bool overwrite = false); 
        Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date);
    }
}
