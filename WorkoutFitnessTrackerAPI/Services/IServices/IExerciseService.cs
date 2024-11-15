using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Models.Dto_s.Exercise;

namespace WorkoutFitnessTrackerAPI.Services.IServices
{
    public interface IExerciseService
    {
        Task<List<T>> PrepareExercises<T>(Guid userId, IEnumerable<IExerciseDto> exercises) where T : class, new();
        Task<string> NormalizeExerciseNameAsync(string exerciseName);
        Task<Exercise> GetExerciseByNormalizedNameAsync(string exerciseName);
        Task<IEnumerable<Exercise>> GetAllExercisesAsync();
        Task<bool> EnsureUserExerciseLinkAsync(Guid userId, string exerciseName);
    }
}
