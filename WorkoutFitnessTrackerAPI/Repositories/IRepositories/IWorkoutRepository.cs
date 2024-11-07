using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IWorkoutRepository
    {
        Task<IEnumerable<Workout>> GetWorkoutsAsync(Guid userId, WorkoutQueryParams queryParams);
        Task<IEnumerable<Workout>> GetWorkoutsByDateAsync(Guid userId, DateTime date);
        Task<bool> CreateWorkoutAsync(Workout workout);
        Task<bool> UpdateWorkoutAsync(Workout workout);
        Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date);
    }
}
