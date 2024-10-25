using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Services.IServices
{
    public interface IExerciseService
    {
        Task<List<WorkoutExercise>> PrepareExercisesForWorkout(Guid userId, List<WorkoutExerciseDto> exercises);
    }
}
