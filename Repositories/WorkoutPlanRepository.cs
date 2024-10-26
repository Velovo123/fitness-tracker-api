using AutoMapper;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;

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

        public async Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, WorkoutPlanQueryParams? queryParams = null)
        {
            var plansQuery = _context.WorkoutPlans
                .Where(wp => wp.UserId == userId)
                .Include(wp => wp.WorkoutPlanExercises)
                .ThenInclude(wpe => wpe.Exercise)
                .AsQueryable();

            if (queryParams != null)
            {
                plansQuery = queryParams.SortBy?.ToLower() switch
                {
                    "name" => queryParams.SortDescending == true ? plansQuery.OrderByDescending(wp => wp.Name) : plansQuery.OrderBy(wp => wp.Name),
                    _ => plansQuery.OrderBy(wp => wp.Name)
                };

                plansQuery = plansQuery
                    .Skip(((queryParams.PageNumber ?? 1) - 1) * (queryParams.PageSize ?? 10))
                    .Take(queryParams.PageSize ?? 10);
            }

            _mapper.ConfigurationProvider.AssertConfigurationIsValid();
            var plans = await plansQuery.ToListAsync();
            return _mapper.Map<IEnumerable<WorkoutPlanDto>>(plans);
        }

        public async Task<WorkoutPlanDto?> GetWorkoutPlanByNameAsync(Guid userId, string planName)
        {
            var normalizedPlanName = planName.Trim().ToLower();
            var plan = await _context.WorkoutPlans
                .Include(wp => wp.WorkoutPlanExercises)
                .ThenInclude(wpe => wpe.Exercise)
                .FirstOrDefaultAsync(wp => wp.UserId == userId && wp.Name == normalizedPlanName);

            return plan == null ? null : _mapper.Map<WorkoutPlanDto>(plan);
        }

        public async Task<bool> SaveWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto, bool overwrite = false)
        {
            var existingPlan = await FindWorkoutPlanAsync(userId, workoutPlanDto.Name);

            if (existingPlan != null)
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException("A workout plan with this name already exists. Confirm overwrite.");
                }

                var exercisesInPlan = await _exerciseService.PrepareExercises<WorkoutPlanExercise>(userId, workoutPlanDto.Exercises);
                UpdateExistingWorkoutPlan(existingPlan, workoutPlanDto.Goal, exercisesInPlan);
            }
            else
            {
                var exercisesInPlan = await _exerciseService.PrepareExercises<WorkoutPlanExercise>(userId, workoutPlanDto.Exercises);
                CreateNewWorkoutPlan(userId, workoutPlanDto.Name, workoutPlanDto.Goal, exercisesInPlan);
            }

            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteWorkoutPlanAsync(Guid userId, string planName)
        {
            var normalizedPlanName = planName.Trim().ToLower();
            var plan = await FindWorkoutPlanAsync(userId, normalizedPlanName);
            if (plan == null) return false;

            _context.WorkoutPlans.Remove(plan);
            return await _context.SaveChangesAsync() > 0;
        }

        private async Task<WorkoutPlan?> FindWorkoutPlanAsync(Guid userId, string normalizedPlanName)
        {
            return await _context.WorkoutPlans
                .Include(wp => wp.WorkoutPlanExercises)
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
