using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly WFTDbContext _context;
        private readonly IExerciseService _exerciseService;

        public WorkoutRepository(WFTDbContext context, IExerciseService exerciseService)
        {
            _context = context;
            _exerciseService = exerciseService;
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(Guid userId)
        {
            return await FetchWorkoutsAsync(userId);
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsByDateAsync(Guid userId, DateTime date)
        {
            return await FetchWorkoutsAsync(userId, date);
        }

        public async Task<bool> SaveWorkoutAsync(Guid userId, WorkoutDto workoutDto)
        {
            var standardizedDate = workoutDto.Date;
            var existingWorkout = await FindWorkoutAsync(userId, standardizedDate);

            var exercisesInWorkout = await _exerciseService.PrepareExercisesForWorkout(userId, workoutDto.Exercises);

            if (existingWorkout != null)
            {
                UpdateExistingWorkout(existingWorkout, workoutDto.Duration, exercisesInWorkout);
            }
            else
            {
                CreateNewWorkout(userId, standardizedDate, workoutDto.Duration, exercisesInWorkout);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date)
        {
            var workout = await FindWorkoutAsync(userId, date);
            if (workout == null) return false;

            _context.Workouts.Remove(workout);
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task<IEnumerable<WorkoutDto>> FetchWorkoutsAsync(Guid userId, DateTime? date = null)
        {
            var workoutsQuery = _context.Workouts
                .Where(w => w.UserId == userId && (date == null || w.Date.Date == date.Value.Date))
                .OrderBy(w => w.Date)
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
                ));

            return await workoutsQuery.ToListAsync();
        }

        private async Task<Workout?> FindWorkoutAsync(Guid userId, DateTime date)
        {
            return await _context.Workouts
                .Include(w => w.WorkoutExercises)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Date == date);
        }

        private void UpdateExistingWorkout(Workout workout, int duration, List<WorkoutExercise> exercisesInWorkout)
        {
            workout.Duration = duration;
            _context.WorkoutExercises.RemoveRange(workout.WorkoutExercises);
            workout.WorkoutExercises = exercisesInWorkout;
        }

        private void CreateNewWorkout(Guid userId, DateTime date, int duration, List<WorkoutExercise> exercisesInWorkout)
        {
            var workout = new Workout
            {
                UserId = userId,
                Date = date,
                Duration = duration,
                WorkoutExercises = exercisesInWorkout
            };
            _context.Workouts.Add(workout);
        }
    }
}
