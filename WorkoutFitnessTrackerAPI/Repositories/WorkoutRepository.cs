using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using AutoMapper;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly WFTDbContext _context;
        private readonly IExerciseService _exerciseService;
        private readonly IMapper _mapper;

        public WorkoutRepository(WFTDbContext context, IExerciseService exerciseService, IMapper mapper)
        {
            _context = context;
            _exerciseService = exerciseService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(Guid userId, WorkoutQueryParams queryParams)
        {
            var workoutsQuery = _context.Workouts
                .Where(w => w.UserId == userId)
                .AsNoTracking()
                .Include(w => w.WorkoutExercises)
                .ThenInclude(we => we.Exercise)
                .AsQueryable();

            workoutsQuery = ApplyFilters(workoutsQuery, queryParams);
            workoutsQuery = ApplySorting(workoutsQuery, queryParams);
            workoutsQuery = ApplyPaging(workoutsQuery, queryParams);

            var workouts = await workoutsQuery.ToListAsync();
            return _mapper.Map<IEnumerable<WorkoutDto>>(workouts);
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsByDateAsync(Guid userId, DateTime date)
        {
            var workouts = await _context.Workouts
                .AsNoTracking()
                .Where(w => w.UserId == userId && w.Date.Date == date.Date)
                .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Exercise)
                .ToListAsync();

            return _mapper.Map<IEnumerable<WorkoutDto>>(workouts);
        }

        public async Task<bool> CreateWorkoutAsync(Guid userId, WorkoutDto workoutDto)
        {
            if (workoutDto == null) throw new ArgumentNullException(nameof(workoutDto));

            var exercisesInWorkout = await _exerciseService.PrepareExercises<WorkoutExercise>(userId, workoutDto.Exercises);
            var workout = _mapper.Map<Workout>(workoutDto);
            workout.UserId = userId;
            workout.WorkoutExercises = exercisesInWorkout;

            _context.Workouts.Add(workout);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateWorkoutAsync(Guid userId, WorkoutDto workoutDto)
        {
            var existingWorkout = await FindWorkoutAsync(userId, workoutDto.Date);
            if (existingWorkout == null) throw new KeyNotFoundException("Workout not found.");

            var exercisesInWorkout = await _exerciseService.PrepareExercises<WorkoutExercise>(userId, workoutDto.Exercises);

            existingWorkout.Duration = workoutDto.Duration;
            _context.WorkoutExercises.RemoveRange(existingWorkout.WorkoutExercises);
            existingWorkout.WorkoutExercises = exercisesInWorkout;

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date)
        {
            var workout = await FindWorkoutAsync(userId, date);
            if (workout == null) return false;

            _context.Workouts.Remove(workout);
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task<Workout?> FindWorkoutAsync(Guid userId, DateTime date)
        {
            return await _context.Workouts
                .Include(w => w.WorkoutExercises)
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Date == date);
        }

        private IQueryable<Workout> ApplyFilters(IQueryable<Workout> query, WorkoutQueryParams queryParams)
        {
            if (queryParams.MinDuration.HasValue) query = query.Where(w => w.Duration >= queryParams.MinDuration.Value);
            if (queryParams.MaxDuration.HasValue) query = query.Where(w => w.Duration <= queryParams.MaxDuration.Value);
            if (queryParams.StartDate.HasValue) query = query.Where(w => w.Date >= queryParams.StartDate.Value);
            if (queryParams.EndDate.HasValue) query = query.Where(w => w.Date <= queryParams.EndDate.Value);

            return query;
        }

        private IQueryable<Workout> ApplySorting(IQueryable<Workout> query, WorkoutQueryParams queryParams)
        {
            return queryParams.SortBy?.ToLower() switch
            {
                "duration" => queryParams.SortDescending == true ? query.OrderByDescending(w => w.Duration) : query.OrderBy(w => w.Duration),
                "date" => queryParams.SortDescending == true ? query.OrderByDescending(w => w.Date) : query.OrderBy(w => w.Date),
                _ => query.OrderBy(w => w.Date)
            };
        }

        private IQueryable<Workout> ApplyPaging(IQueryable<Workout> query, WorkoutQueryParams queryParams)
        {
            return query.Skip(((queryParams.PageNumber ?? 1) - 1) * (queryParams.PageSize ?? 10))
                        .Take(queryParams.PageSize ?? 10);
        }
    }
}
