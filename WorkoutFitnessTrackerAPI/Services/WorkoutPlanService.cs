using WorkoutFitnessTrackerAPI.Services.IServices;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTracker.API.Services.IServices;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class WorkoutPlanService : IWorkoutPlanService
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;

        public WorkoutPlanService(IWorkoutPlanRepository workoutPlanRepository)
        {
            _workoutPlanRepository = workoutPlanRepository;
        }

        public async Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, WorkoutPlanQueryParams queryParams)
        {
            return await _workoutPlanRepository.GetWorkoutPlansAsync(userId, queryParams);
        }

        public async Task<WorkoutPlanDto?> GetWorkoutPlanByNameAsync(Guid userId, string planName)
        {
            return await _workoutPlanRepository.GetWorkoutPlanByNameAsync(userId, planName);
        }

        public async Task<bool> CreateWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto, bool overwrite = false)
        {
            var existingPlan = await _workoutPlanRepository.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name);
            if (existingPlan != null)
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException("A workout plan with this name already exists. Enable overwrite to update.");
                }
                else
                {
                    // Overwrite existing workout plan
                    return await UpdateWorkoutPlanAsync(userId, workoutPlanDto);
                }
            }

            return await _workoutPlanRepository.CreateWorkoutPlanAsync(userId, workoutPlanDto);
        }

        public async Task<bool> UpdateWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto)
        {
            return await _workoutPlanRepository.UpdateWorkoutPlanAsync(userId, workoutPlanDto);
        }

        public async Task<bool> DeleteWorkoutPlanAsync(Guid userId, string planName)
        {
            return await _workoutPlanRepository.DeleteWorkoutPlanAsync(userId, planName);
        }
    }
}
