using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Services.IServices;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Models.Dto_s.Exercise;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly IExerciseRepository _exerciseRepository;

        public ExerciseService(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
        }

        public async Task<string> NormalizeExerciseNameAsync(string exerciseName)
        {
            return await Task.FromResult(NameNormalizationHelper.NormalizeName(exerciseName));
        }

        public async Task<List<T>> PrepareExercises<T>(Guid userId, IEnumerable<IExerciseDto> exercises) where T : class, new()
        {
            var preparedExercises = new List<T>();

            foreach (var exerciseDto in exercises)
            {
                // Normalize the exercise name and retrieve the exercise
                var standardizedExerciseName = await NormalizeExerciseNameAsync(exerciseDto.ExerciseName);
                var exercise = await _exerciseRepository.GetByNameAsync(standardizedExerciseName);

                if (exercise == null)
                {
                    throw new InvalidOperationException($"Exercise '{exerciseDto.ExerciseName}' not found.");
                }

                // Ensure user-exercise link
                await _exerciseRepository.EnsureUserExerciseLinkAsync(userId, exercise.Id);

                // Create an instance of the target type (WorkoutExercise or WorkoutPlanExercise)
                var preparedExercise = CreateExerciseInstance<T>(exercise.Id, exerciseDto);
                if (preparedExercise != null)
                {
                    bool isDuplicate = preparedExercises.Any(e =>
                        e is WorkoutExercise we && we.ExerciseId == exercise.Id && we.Sets == exerciseDto.Sets && we.Reps == exerciseDto.Reps ||
                        e is WorkoutPlanExercise wpe && wpe.ExerciseId == exercise.Id && wpe.Sets == exerciseDto.Sets && wpe.Reps == exerciseDto.Reps);

                    if (!isDuplicate)
                    {
                        preparedExercises.Add(preparedExercise);
                    }
                }
            }

            return preparedExercises;
        }

        public async Task<bool> EnsureUserExerciseLinkAsync(Guid userId, string exerciseName)
        {
            var normalizedExerciseName = await NormalizeExerciseNameAsync(exerciseName);
            var exercise = await _exerciseRepository.GetByNameAsync(normalizedExerciseName);

            if (exercise == null)
            {
                throw new InvalidOperationException($"Exercise with name '{exerciseName}' not found.");
            }

            await _exerciseRepository.EnsureUserExerciseLinkAsync(userId, exercise.Id);
            return true;
        }

        public async Task<Exercise> GetExerciseByNormalizedNameAsync(string exerciseName)
        {
            var normalizedExerciseName = NameNormalizationHelper.NormalizeName(exerciseName);
            return await _exerciseRepository.GetByNameAsync(normalizedExerciseName);
        }

        public async Task<IEnumerable<Exercise>> GetUserLinkedExercisesAsync(Guid userId)
        {
            return await _exerciseRepository.GetUserLinkedExercisesAsync(userId);
        }


        public async Task<IEnumerable<Exercise>> GetAllExercisesAsync()
        {
            return await _exerciseRepository.GetAllExercisesAsync();
        }

        private static T? CreateExerciseInstance<T>(Guid exerciseId, IExerciseDto exerciseDto) where T : class, new()
        {
            if (typeof(T) == typeof(WorkoutExercise))
            {
                return (T)(object)new WorkoutExercise
                {
                    ExerciseId = exerciseId,
                    Sets = exerciseDto.Sets,
                    Reps = exerciseDto.Reps
                };
            }
            else if (typeof(T) == typeof(WorkoutPlanExercise))
            {
                return (T)(object)new WorkoutPlanExercise
                {
                    ExerciseId = exerciseId,
                    Sets = exerciseDto.Sets,
                    Reps = exerciseDto.Reps
                };
            }
            return null;
        }
    }
}
