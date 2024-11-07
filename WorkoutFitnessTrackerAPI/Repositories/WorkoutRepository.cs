using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class WorkoutRepository : IWorkoutRepository
    {
        private readonly WFTDbContext _context;

        public WorkoutRepository(WFTDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Workout>> GetWorkoutsAsync(Guid userId, WorkoutQueryParams queryParams)
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

            return await workoutsQuery.ToListAsync();
        }

        public async Task<IEnumerable<Workout>> GetWorkoutsByDateAsync(Guid userId, DateTime date)
        {
            return await _context.Workouts
                .AsNoTracking()
                .Where(w => w.UserId == userId && w.Date.Date == date.Date)
                .Include(w => w.WorkoutExercises)
                    .ThenInclude(we => we.Exercise)
                .ToListAsync();
        }

        public async Task<bool> CreateWorkoutAsync(Workout workout)
        {
            _context.Workouts.Add(workout);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateWorkoutAsync(Workout workout)
        {
            _context.Workouts.Update(workout);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date)
        {
            var workout = await _context.Workouts
                .FirstOrDefaultAsync(w => w.UserId == userId && w.Date == date);
            if (workout == null) return false;

            _context.Workouts.Remove(workout);
            return await _context.SaveChangesAsync() > 0;
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
