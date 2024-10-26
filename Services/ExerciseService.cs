using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Models.Dto_s.IDto_s;

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
                var standardizedExerciseName = NormalizeName(exerciseDto.ExerciseName);

                var exercise = await _context.Exercises
                    .FirstOrDefaultAsync(ex => ex.Name == standardizedExerciseName && ex.UserId == userId);

                if (exercise == null)
                {
                    exercise = new Exercise
                    {
                        Id = Guid.NewGuid(),
                        Name = standardizedExerciseName,  
                        UserId = userId,
                        Type = exerciseDto is WorkoutExerciseDto workoutExerciseDto
                            ? workoutExerciseDto.Type
                            : null
                    };
                    _context.Exercises.Add(exercise);
                    await _context.SaveChangesAsync();
                }

                T preparedExercise = null!;

                if (typeof(T) == typeof(WorkoutExercise))
                {
                    preparedExercise = (T)(object)new WorkoutExercise
                    {
                        ExerciseId = exercise.Id,
                        Sets = exerciseDto.Sets,
                        Reps = exerciseDto.Reps
                    };
                }
                else if (typeof(T) == typeof(WorkoutPlanExercise))
                {
                    preparedExercise = (T)(object)new WorkoutPlanExercise
                    {
                        ExerciseId = exercise.Id,
                        Sets = exerciseDto.Sets,
                        Reps = exerciseDto.Reps
                    };
                }

                if (preparedExercise != null)
                    preparedExercises.Add(preparedExercise);
            }

            return preparedExercises;
        }

        private static string NormalizeName(string name)
        {
            return string.Concat(name.ToLower().Where(char.IsLetterOrDigit));
        }

    }
}