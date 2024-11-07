using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using AutoMapper;
using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IMapper _mapper;

        public WorkoutService(IWorkoutRepository workoutRepository, IMapper mapper)
        {
            _workoutRepository = workoutRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(Guid userId, WorkoutQueryParams queryParams)
        {
            var workouts = await _workoutRepository.GetWorkoutsAsync(userId, queryParams);
            return _mapper.Map<IEnumerable<WorkoutDto>>(workouts);
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsByDateAsync(Guid userId, DateTime date)
        {
            var workouts = await _workoutRepository.GetWorkoutsByDateAsync(userId, date);
            return _mapper.Map<IEnumerable<WorkoutDto>>(workouts);
        }

        public async Task<bool> CreateWorkoutAsync(Guid userId, WorkoutDto workoutDto, bool overwrite = false)
        {
            var existingWorkouts = await _workoutRepository.GetWorkoutsByDateAsync(userId, workoutDto.Date);
            if (existingWorkouts.Any() && !overwrite)
            {
                throw new InvalidOperationException("Workout already exists for the specified date. Enable overwrite to update.");
            }
            else if (existingWorkouts.Any() && overwrite)
            {
                return await UpdateWorkoutAsync(userId, workoutDto);
            }

            var workout = _mapper.Map<Workout>(workoutDto);
            workout.UserId = userId;
            return await _workoutRepository.CreateWorkoutAsync(workout);
        }

        public async Task<bool> UpdateWorkoutAsync(Guid userId, WorkoutDto workoutDto)
        {
            var workout = _mapper.Map<Workout>(workoutDto);
            workout.UserId = userId;
            return await _workoutRepository.UpdateWorkoutAsync(workout);
        }

        public async Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date)
        {
            return await _workoutRepository.DeleteWorkoutAsync(userId, date);
        }
    }
}
