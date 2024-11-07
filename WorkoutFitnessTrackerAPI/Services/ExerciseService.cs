using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using WorkoutFitnessTrackerAPI.Helpers;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Models.Dto_s.Exercise;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly WFTDbContext _context;
        private readonly IExerciseRepository _exerciseRepository;

        public ExerciseService(WFTDbContext context, IExerciseRepository exerciseRepository)
        {
            _context = context;
            _exerciseRepository = exerciseRepository;
        }

        public Task<string> NormalizeExerciseNameAsync(string exerciseName)
        {
            return Task.FromResult(NameNormalizationHelper.NormalizeName(exerciseName));
        }

        public async Task<List<T>> PrepareExercises<T>(Guid userId, IEnumerable<IExerciseDto> exercises) where T : class, new()
        {
            var preparedExercises = new List<T>();

            foreach (var exerciseDto in exercises)
            {
                var standardizedExerciseName = NameNormalizationHelper.NormalizeName(exerciseDto.ExerciseName);
                var exercise = await GetOrCreateExerciseAsync(standardizedExerciseName, exerciseDto);

                await EnsureUserExerciseLinkAsync(userId, exercise.Id);

                var preparedExercise = CreateExerciseInstance<T>(exercise.Id, exerciseDto);
                if (preparedExercise != null)
                {
                    preparedExercises.Add(preparedExercise);
                }
            }

            return preparedExercises;
        }

        public async Task<Exercise> GetExerciseByNormalizedNameAsync(string exerciseName)
        {
            var normalizedExerciseName = NameNormalizationHelper.NormalizeName(exerciseName);
            return await _exerciseRepository.GetByNameAsync(normalizedExerciseName)
                   ?? await _context.Exercises.FirstOrDefaultAsync(ex => ex.Name == normalizedExerciseName);
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
                return true;
            }

            return false;
        }

        private async Task<Exercise> GetOrCreateExerciseAsync(string standardizedExerciseName, IExerciseDto exerciseDto)
        {
            var exercise = await _exerciseRepository.GetByNameAsync(standardizedExerciseName);
            if (exercise == null)
            {
                exercise = new Exercise
                {
                    Id = Guid.NewGuid(),
                    Name = standardizedExerciseName,
                    Type = (exerciseDto as WorkoutExerciseDto)?.Type
                };
                await _exerciseRepository.AddAsync(exercise);
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

        public async Task<IEnumerable<Exercise>> GetAllExercisesAsync()
        {
            return await _exerciseRepository.GetAllExercisesAsync();
        }

        public async Task<Exercise> AddExerciseAsync(ExerciseDto exerciseDto)
        {
            var normalizedExerciseName = NameNormalizationHelper.NormalizeName(exerciseDto.Name);
            var exercise = new Exercise
            {
                Id = Guid.NewGuid(),
                Name = normalizedExerciseName,
                Type = exerciseDto.Type
            };
            return await _exerciseRepository.AddAsync(exercise);
        }
    }
}
