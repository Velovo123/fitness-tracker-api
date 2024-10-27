using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;
using WorkoutFitnessTrackerAPI.Helpers;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly WFTDbContext _context;

        public ExerciseService(WFTDbContext context)
        {
            _context = context;
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

        private async Task<Exercise> GetOrCreateExerciseAsync(string standardizedExerciseName, IExerciseDto exerciseDto)
        {
            var exercise = await _context.Exercises
                .FirstOrDefaultAsync(ex => ex.Name == standardizedExerciseName);

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

        private async Task EnsureUserExerciseLinkAsync(Guid userId, Guid exerciseId)
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
