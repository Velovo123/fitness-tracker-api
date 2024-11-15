using WorkoutFitnessTrackerAPI.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IExerciseRepository
    {
        Task<Exercise?> GetByNameAsync(string name);
        Task<Exercise> AddAsync(Exercise exercise);
        Task<IEnumerable<Exercise>> GetAllExercisesAsync();
        public Task EnsureUserExerciseLinkAsync(Guid userId, Guid exerciseId);
    }
}
