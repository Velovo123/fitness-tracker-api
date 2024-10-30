using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using AutoMapper;
using WorkoutFitnessTrackerAPI.Helpers;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class WorkoutPlanRepository : IWorkoutPlanRepository
    {
        private readonly WFTDbContext _context;
        private readonly IExerciseService _exerciseService;
        private readonly IMapper _mapper;

        public WorkoutPlanRepository(WFTDbContext context, IExerciseService exerciseService, IMapper mapper)
        {
            _context = context;
            _exerciseService = exerciseService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, WorkoutPlanQueryParams queryParams)
        {
            var plansQuery = _context.WorkoutPlans
                .Where(wp => wp.UserId == userId)
                .Include(wp => wp.WorkoutPlanExercises)
                .ThenInclude(wpe => wpe.Exercise)
                .AsQueryable();

            plansQuery = ApplyFilters(plansQuery, queryParams);
            plansQuery = ApplySorting(plansQuery, queryParams);
            plansQuery = ApplyPaging(plansQuery, queryParams);

            var plans = await plansQuery.ToListAsync();
            return _mapper.Map<IEnumerable<WorkoutPlanDto>>(plans);
        }

        public async Task<WorkoutPlanDto?> GetWorkoutPlanByNameAsync(Guid userId, string planName)
        {
            var normalizedPlanName = NameNormalizationHelper.NormalizeName(planName);
            var plan = await FindWorkoutPlanByName(userId, normalizedPlanName);
            return plan == null ? null : _mapper.Map<WorkoutPlanDto>(plan);
        }

        public async Task<bool> SaveWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto, bool overwrite = false)
        {
            var normalizedPlanName = NameNormalizationHelper.NormalizeName(workoutPlanDto.Name);
            var existingPlan = await FindWorkoutPlanByName(userId, normalizedPlanName);

            var exercisesInPlan = await _exerciseService.PrepareExercises<WorkoutPlanExercise>(userId, workoutPlanDto.Exercises);

            if (existingPlan != null)
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException("A workout plan with this name already exists. Confirm overwrite.");
                }
                UpdateExistingWorkoutPlan(existingPlan, workoutPlanDto.Goal, exercisesInPlan);
            }
            else
            {
                CreateNewWorkoutPlan(userId, normalizedPlanName, workoutPlanDto.Goal, exercisesInPlan);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteWorkoutPlanAsync(Guid userId, string planName)
        {
            var normalizedPlanName = NameNormalizationHelper.NormalizeName(planName);
            var plan = await FindWorkoutPlanByName(userId, normalizedPlanName);
            if (plan == null) return false;

            _context.WorkoutPlans.Remove(plan);
            return await _context.SaveChangesAsync() > 0;
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

        private async Task<WorkoutPlan?> FindWorkoutPlanByName(Guid userId, string normalizedPlanName)
        {
            return await _context.WorkoutPlans
                .Include(wp => wp.WorkoutPlanExercises)
                .ThenInclude(wpe => wpe.Exercise)
                .FirstOrDefaultAsync(wp => wp.UserId == userId && wp.Name == normalizedPlanName);
        }

        private void UpdateExistingWorkoutPlan(WorkoutPlan plan, string? goal, List<WorkoutPlanExercise> exercisesInPlan)
        {
            plan.Goal = goal!;
            _context.WorkoutPlanExercises.RemoveRange(plan.WorkoutPlanExercises);
            plan.WorkoutPlanExercises = exercisesInPlan;
        }

        private void CreateNewWorkoutPlan(Guid userId, string normalizedName, string? goal, List<WorkoutPlanExercise> exercisesInPlan)
        {
            var workoutPlan = new WorkoutPlan
            {
                UserId = userId,
                Name = normalizedName,
                Goal = goal!,
                WorkoutPlanExercises = exercisesInPlan
            };
            _context.WorkoutPlans.Add(workoutPlan);
        }
    }
}
