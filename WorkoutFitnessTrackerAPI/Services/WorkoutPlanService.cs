using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using AutoMapper;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTracker.API.Services.IServices;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class WorkoutPlanService : IWorkoutPlanService
    {
        private readonly IWorkoutPlanRepository _workoutPlanRepository;
        private readonly IExerciseService _exerciseService;
        private readonly IMapper _mapper;

        public WorkoutPlanService(IWorkoutPlanRepository workoutPlanRepository, IExerciseService exerciseService, IMapper mapper)
        {
            _workoutPlanRepository = workoutPlanRepository;
            _exerciseService = exerciseService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkoutPlanDto>> GetWorkoutPlansAsync(Guid userId, WorkoutPlanQueryParams queryParams)
        {
            var plans = await _workoutPlanRepository.GetWorkoutPlansAsync(userId, queryParams);
            return _mapper.Map<IEnumerable<WorkoutPlanDto>>(plans);
        }

        public async Task<WorkoutPlanDto?> GetWorkoutPlanByNameAsync(Guid userId, string planName)
        {
            var plan = await _workoutPlanRepository.GetWorkoutPlanByNameAsync(userId, planName);
            return plan == null ? null : _mapper.Map<WorkoutPlanDto>(plan);
        }

        public async Task<bool> CreateWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto, bool overwrite = false)
        {
            var existingPlan = await _workoutPlanRepository.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name);
            if (existingPlan != null)
            {
                if (!overwrite) throw new InvalidOperationException("Workout plan already exists. Enable overwrite to update.");
                return await UpdateWorkoutPlanAsync(userId, workoutPlanDto);
            }

            var exercisesInPlan = await _exerciseService.PrepareExercises<WorkoutPlanExercise>(userId, workoutPlanDto.Exercises);
            var workoutPlan = _mapper.Map<WorkoutPlan>(workoutPlanDto);
            workoutPlan.UserId = userId;
            workoutPlan.WorkoutPlanExercises = exercisesInPlan;

            return await _workoutPlanRepository.CreateWorkoutPlanAsync(workoutPlan);
        }

        public async Task<bool> UpdateWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto)
        {
            var existingPlan = await _workoutPlanRepository.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name);
            if (existingPlan == null) return false;

            var exercisesInPlan = await _exerciseService.PrepareExercises<WorkoutPlanExercise>(userId, workoutPlanDto.Exercises);
            existingPlan.Goal = workoutPlanDto.Goal;
            existingPlan.WorkoutPlanExercises = exercisesInPlan;

            return await _workoutPlanRepository.UpdateWorkoutPlanAsync(existingPlan);
        }

        public async Task<bool> DeleteWorkoutPlanAsync(Guid userId, string planName)
        {
            return await _workoutPlanRepository.DeleteWorkoutPlanAsync(userId, planName);
        }
    }
}
