using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IWorkoutRepository
    {
        Task<IEnumerable<Workout>> GetWorkoutsAsync(Guid userId, WorkoutQueryParams queryParams);
        Task<IEnumerable<Workout>> GetWorkoutsByDateTimeAsync(Guid userId, DateTime dateTime);
        Task<IEnumerable<Workout>> GetWorkoutsByDateRangeAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<bool> CreateWorkoutAsync(Workout workout);
        Task<bool> UpdateWorkoutAsync(Workout workout);
        Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date);

    }
}
