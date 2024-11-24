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
using F23.StringSimilarity;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class ExerciseService : IExerciseService
    {
        private static List<string>? _exerciseCache;
        private static DateTime _cacheExpirationTime;
        private readonly IExerciseRepository _exerciseRepository;
        private readonly object _cacheLock = new(); // Ensure thread safety for the cache

        public ExerciseService(IExerciseRepository exerciseRepository)
        {
            _exerciseRepository = exerciseRepository ?? throw new ArgumentNullException(nameof(exerciseRepository));
        }

        public async Task<string> NormalizeExerciseNameAsync(string exerciseName)
        {
            return await Task.FromResult(NameNormalizationHelper.NormalizeName(exerciseName));
        }

        private async Task<List<string>> SuggestExercisesAsync(string input)
        {
            // Normalize user input
            var normalizedInput = await NormalizeExerciseNameAsync(input);

            // Fetch all exercise names from the cache
            var allExerciseNames = await GetCachedExerciseNamesAsync();

            // Use Levenshtein for fuzzy matching
            var levenshtein = new Levenshtein();
            var suggestions = allExerciseNames
                .Select(name => new
                {
                    Name = name,
                    Distance = levenshtein.Distance(normalizedInput, name.ToLower())
                })
                .OrderBy(x => x.Distance) // Closest matches first
                .Take(5)                  // Limit to top 5
                .Select(x => x.Name)
                .ToList();

            return suggestions;
        }

        public async Task<List<T>> PrepareExercises<T>(Guid userId, IEnumerable<IExerciseDto> exercises) where T : class, new()
        {
            var preparedExercises = new List<T>();

            foreach (var exerciseDto in exercises)
            {
                var standardizedExerciseName = await NormalizeExerciseNameAsync(exerciseDto.ExerciseName);
                var exercise = await _exerciseRepository.GetByNameAsync(standardizedExerciseName);

                if (exercise == null)
                {
                    var suggestions = await SuggestExercisesAsync(exerciseDto.ExerciseName);
                    throw new InvalidOperationException(
                        $"Exercise '{exerciseDto.ExerciseName}' not found. Did you mean: {string.Join(", ", suggestions)}?");
                }

                await _exerciseRepository.EnsureUserExerciseLinkAsync(userId, exercise.Id);

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

        private async Task<List<string>> GetCachedExerciseNamesAsync()
        {
            lock (_cacheLock)
            {
                if (_exerciseCache != null && DateTime.UtcNow < _cacheExpirationTime)
                {
                    return _exerciseCache;
                }
            }

            // Fetch fresh data outside the lock to avoid deadlocks
            var freshExerciseNames = await _exerciseRepository.GetAllExerciseNamesAsync();
            if (freshExerciseNames == null || !freshExerciseNames.Any())
            {
                throw new InvalidOperationException("Failed to fetch exercise names from the repository.");
            }

            lock (_cacheLock)
            {
                // Update cache
                _exerciseCache = freshExerciseNames;
                _cacheExpirationTime = DateTime.UtcNow.AddMinutes(30);
            }

            return _exerciseCache;
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
