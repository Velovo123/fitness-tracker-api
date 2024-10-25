using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class ExerciseService : IExerciseService
    {
        private readonly WFTDbContext _context;

        public ExerciseService(WFTDbContext context)
        {
            _context = context;
        }

        public async Task<List<WorkoutExercise>> PrepareExercisesForWorkout(Guid userId, List<WorkoutExerciseDto> exercises)
        {
            var workoutExercises = new List<WorkoutExercise>();

            foreach (var exerciseDto in exercises)
            {
                var standardizedExerciseName = exerciseDto.ExerciseName.Trim().ToLower();

                var exercise = await _context.Exercises
                    .FirstOrDefaultAsync(ex => ex.Name == standardizedExerciseName && ex.UserId == userId);

                if (exercise == null)
                {
                    exercise = new Exercise
                    {
                        Id = Guid.NewGuid(),
                        Name = standardizedExerciseName,
                        UserId = userId,
                        Type = !string.IsNullOrEmpty(exerciseDto.Type) ? exerciseDto.Type : null
                    };
                    _context.Exercises.Add(exercise);
                    await _context.SaveChangesAsync();
                }

                workoutExercises.Add(new WorkoutExercise
                {
                    ExerciseId = exercise.Id,
                    Sets = exerciseDto.Sets,
                    Reps = exerciseDto.Reps
                });
            }

            return workoutExercises;
        }
    }
}
