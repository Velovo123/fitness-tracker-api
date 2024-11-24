using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using AutoMapper;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Helpers;

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
            var normalizedPlanName = NameNormalizationHelper.NormalizeName(planName);
            var plan = await _workoutPlanRepository.GetWorkoutPlanByNameAsync(userId, normalizedPlanName);
            return plan == null ? null : _mapper.Map<WorkoutPlanDto>(plan);
        }

        public async Task<bool> CreateWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto, bool overwrite = false)
        {
            workoutPlanDto = workoutPlanDto with { Name = NameNormalizationHelper.NormalizeName(workoutPlanDto.Name) };

            var existingPlan = await _workoutPlanRepository.GetWorkoutPlanByNameAsync(userId, workoutPlanDto.Name);

            if (existingPlan != null && !overwrite)
            {
                throw new InvalidOperationException("Workout plan already exists for the specified name. Enable overwrite to update.");
            }
            else if (existingPlan != null && overwrite)
            {
                return await UpdateWorkoutPlanAsync(userId, workoutPlanDto);
            }

            var workoutPlan = _mapper.Map<WorkoutPlan>(workoutPlanDto);
            workoutPlan.UserId = userId;

            var exercisesInPlan = await _exerciseService.PrepareExercises<WorkoutPlanExercise>(userId, workoutPlanDto.Exercises);
            workoutPlan.WorkoutPlanExercises = exercisesInPlan;

            return await _workoutPlanRepository.CreateWorkoutPlanAsync(workoutPlan);
        }

        public async Task<bool> UpdateWorkoutPlanAsync(Guid userId, WorkoutPlanDto workoutPlanDto)
        {
            workoutPlanDto = workoutPlanDto with { Name = NameNormalizationHelper.NormalizeName(workoutPlanDto.Name) };

            var deleteSuccess = await _workoutPlanRepository.DeleteWorkoutPlanAsync(userId, workoutPlanDto.Name);
            if (!deleteSuccess) return false;

            var newWorkoutPlan = _mapper.Map<WorkoutPlan>(workoutPlanDto);
            newWorkoutPlan.UserId = userId;

            var exercisesInPlan = await _exerciseService.PrepareExercises<WorkoutPlanExercise>(userId, workoutPlanDto.Exercises);
            newWorkoutPlan.WorkoutPlanExercises = exercisesInPlan;

            return await _workoutPlanRepository.CreateWorkoutPlanAsync(newWorkoutPlan);
        }

        public async Task<bool> DeleteWorkoutPlanAsync(Guid userId, string planName)
        {
            var normalizedPlanName = NameNormalizationHelper.NormalizeName(planName);
            return await _workoutPlanRepository.DeleteWorkoutPlanAsync(userId, normalizedPlanName);
        }
    }
}
