using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Models.Dto_s.Exercise;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly WFTDbContext _context;

        public ExerciseService(WFTDbContext context)
        {
            _context = context;
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
                var standardizedExerciseName = await NormalizeExerciseNameAsync(exerciseDto.ExerciseName);
                var exercise = await GetOrCreateExerciseAsync(standardizedExerciseName, exerciseDto);

                await EnsureUserExerciseLinkAsync(userId, exercise.Id);

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

        public async Task<Exercise> GetExerciseByNormalizedNameAsync(string exerciseName)
        {
            var normalizedExerciseName = NameNormalizationHelper.NormalizeName(exerciseName);
            return await _context.Exercises.FirstOrDefaultAsync(ex => ex.Name == normalizedExerciseName);
        }

        public async Task<bool> EnsureUserExerciseLinkAsync(Guid userId, Guid exerciseId)
        {
            var userExerciseLinkExists = await _context.UserExercises
                .AnyAsync(ue => ue.UserId == userId && ue.ExerciseId == exerciseId);

            if (!userExerciseLinkExists)
            {
                _context.UserExercises.Add(new UserExercise
                {
                    UserId = userId,
                    ExerciseId = exerciseId
                });

                await _context.SaveChangesAsync();
            }

            return true;
        }

        public async Task<IEnumerable<Exercise>> GetAllExercisesAsync()
        {
            return await _context.Exercises.AsNoTracking().ToListAsync();
        }

        public async Task<Exercise> AddExerciseAsync(ExerciseDto exerciseDto)
        {
            var normalizedExerciseName = await NormalizeExerciseNameAsync(exerciseDto.Name);
            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = normalizedExerciseName,
                Type = exerciseDto.Type
            };
            _context.Exercises.Add(exercise);
            await _context.SaveChangesAsync();
            return exercise;
        }

        private async Task<Exercise> GetOrCreateExerciseAsync(string standardizedExerciseName, IExerciseDto exerciseDto)
        {
            var exercise = await _context.Exercises.FirstOrDefaultAsync(ex => ex.Name == standardizedExerciseName);
            if (exercise == null)
            {
                exercise = new Exercise
                {
                    Id = Guid.NewGuid(),
                    Name = standardizedExerciseName,
                    Type = (exerciseDto as WorkoutExerciseDto)?.Type
                };

                _context.Exercises.Add(exercise);
                await _context.SaveChangesAsync();
            }
            return exercise;
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
