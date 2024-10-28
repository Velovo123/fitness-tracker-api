using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;

namespace WorkoutFitnessTrackerAPI.Services.IServices
{
    public interface IExerciseService
    {
       
        Task<List<T>> PrepareExercises<T>(Guid userId, IEnumerable<IExerciseDto> exercises) where T : class, new();

        
        Task<Exercise> GetExerciseByNormalizedNameAsync(string exerciseName);

       
        Task<bool> EnsureUserExerciseLinkAsync(Guid userId, Guid exerciseId);
    }
}
