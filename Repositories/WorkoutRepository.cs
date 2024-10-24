using Dapper;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using System.Data;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly WFTDbContext _context;

        public WorkoutRepository(WFTDbContext context)
        {
            _context = context;
        }

        public async Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date)
        {
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Date.Date == date.Date);

            if (workout == null)
                return false;

            _context.Workouts.Remove(workout);
            var result = await _context.SaveChangesAsync();

            return result > 0;
        }

        public async Task<WorkoutDto?> GetWorkoutByDateAsync(Guid userId, DateTime date)
        {
            var workout = await _context.Workouts
                .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Date.Date == date.Date);

            if (workout == null)
                return null;

            var workoutDto = new WorkoutDto(
                workout.Date,
                workout.Duration,
                workout.WorkoutExercises.Select(we => new WorkoutExerciseDto(
                    we.Exercise.Name,
                    we.Sets,
                    we.Reps,
                    we.Exercise.Type
                )).ToList()
            );

            return workoutDto;
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(Guid userId)
        {
            var workouts = await _context.Workouts
                .Where(w => w.UserId == userId)
                .Include(w => w.WorkoutExercises)            
                    .ThenInclude(we => we.Exercise)          
                .Select(w => new WorkoutDto(
                    w.Date,                                   
                    w.Duration,                               
                    w.WorkoutExercises.Select(we => new WorkoutExerciseDto(
                        we.Exercise.Name,                    
                        we.Sets,                             
                        we.Reps,
                        we.Exercise.Type
                    )).ToList()                              
                ))
                .ToListAsync();

            return workouts; 
        }

        public async Task<bool> SaveWorkoutAsync(Guid userId, WorkoutDto workoutDto)
        {
            var existingWorkout = await _context.Workouts
                .Include(w => w.WorkoutExercises)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Date.Date == workoutDto.Date.Date);

            var exercisesInWorkout = new List<WorkoutExercise>();
            foreach (var exerciseDto in workoutDto.Exercises)
            {
                var exercise = await _context.Exercises
                    .FirstOrDefaultAsync(ex => ex.Name == exerciseDto.ExerciseName && ex.UserId == userId);

                if (exercise == null)
                {
                    exercise = new Exercise
                    {
                        Id = Guid.NewGuid(),   
                        Name = exerciseDto.ExerciseName,
                        UserId = userId
                    };

                    if (!string.IsNullOrEmpty(exerciseDto.Type))
                    {
                        exercise.Type = exerciseDto.Type;
                    }

                    _context.Exercises.Add(exercise);
                    await _context.SaveChangesAsync();  
                }

                var workoutExercise = new WorkoutExercise
                {
                    ExerciseId = exercise.Id,   
                    Sets = exerciseDto.Sets,
                    Reps = exerciseDto.Reps
                };
                exercisesInWorkout.Add(workoutExercise);
            }

            if (existingWorkout != null)
            {
                existingWorkout.Duration = workoutDto.Duration;

                _context.WorkoutExercises.RemoveRange(existingWorkout.WorkoutExercises);

                existingWorkout.WorkoutExercises = exercisesInWorkout;
            }
            else
            {
                var workout = new Workout
                {
                    UserId = userId,
                    Date = workoutDto.Date,
                    Duration = workoutDto.Duration,
                    WorkoutExercises = exercisesInWorkout
                };

                _context.Workouts.Add(workout);
            }

            var result = await _context.SaveChangesAsync();
            return result > 0;
        }


    }
}