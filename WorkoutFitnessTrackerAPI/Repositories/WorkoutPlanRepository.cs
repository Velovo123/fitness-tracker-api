using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class WorkoutPlanRepository : IWorkoutPlanRepository
    {
        private readonly WFTDbContext _context;

        public WorkoutPlanRepository(WFTDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<WorkoutPlan>> GetWorkoutPlansAsync(Guid userId, WorkoutPlanQueryParams queryParams)
        {
            var plansQuery = _context.WorkoutPlans
                .Where(wp => wp.UserId == userId)
                .Include(wp => wp.WorkoutPlanExercises)
                .ThenInclude(wpe => wpe.Exercise)
                .AsQueryable();

            plansQuery = ApplyFilters(plansQuery, queryParams);
            plansQuery = ApplySorting(plansQuery, queryParams);
            plansQuery = ApplyPaging(plansQuery, queryParams);

            return await plansQuery.ToListAsync();
        }

        public async Task<WorkoutPlan?> GetWorkoutPlanByNameAsync(Guid userId, string planName)
        {
            return await _context.WorkoutPlans
                .Include(wp => wp.WorkoutPlanExercises)
                .ThenInclude(wpe => wpe.Exercise)
                .FirstOrDefaultAsync(wp => wp.UserId == userId && wp.Name == planName);
        }

        public async Task<bool> CreateWorkoutPlanAsync(WorkoutPlan workoutPlan)
        {
            _context.WorkoutPlans.Add(workoutPlan);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateWorkoutPlanAsync(WorkoutPlan workoutPlan)
        {
            _context.WorkoutPlans.Update(workoutPlan);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteWorkoutPlanAsync(Guid userId, string planName)
        {
            var plan = await GetWorkoutPlanByNameAsync(userId, planName);
            if (plan == null) return false;

            _context.WorkoutPlans.Remove(plan);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<IEnumerable<WorkoutPlan>> GetWorkoutPlansForDateAsync(Guid userId, DateTime date)
        {
            return await _context.WorkoutPlans
                .Where(wp => wp.UserId == userId)
                .Include(wp => wp.WorkoutPlanExercises)
                    .ThenInclude(wpe => wpe.Exercise)
                .ToListAsync();
        }
        private IQueryable<WorkoutPlan> ApplyFilters(IQueryable<WorkoutPlan> query, WorkoutPlanQueryParams queryParams)
        {
            if (!string.IsNullOrEmpty(queryParams.Goal))
                query = query.Where(wp => wp.Goal == queryParams.Goal);

            return query;
        }

        private IQueryable<WorkoutPlan> ApplySorting(IQueryable<WorkoutPlan> query, WorkoutPlanQueryParams queryParams)
        {
            return queryParams.SortBy?.ToLower() switch
            {
                "name" => queryParams.SortDescending == true ? query.OrderByDescending(wp => wp.Name) : query.OrderBy(wp => wp.Name),
                _ => query.OrderBy(wp => wp.Name)
            };
        }

        private IQueryable<WorkoutPlan> ApplyPaging(IQueryable<WorkoutPlan> query, WorkoutPlanQueryParams queryParams)
        {
            return query.Skip(((queryParams.PageNumber ?? 1) - 1) * (queryParams.PageSize ?? 10))
                        .Take(queryParams.PageSize ?? 10);
        }
    }
}
    