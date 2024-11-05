using WorkoutFitnessTracker.API.Models.Dto_s.Exercise;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IExerciseRepository
    {
        Task<ExerciseDto?> GetByNameAsync(string name);
        Task<ExerciseDto> AddAsync(ExerciseDto exercise);
        Task<IEnumerable<ExerciseDto>> GetAllExercisesAsync();
    }
}
