using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IWorkoutRepository _workoutRepository;

        public WorkoutService(IWorkoutRepository workoutRepository)
        {
            _workoutRepository = workoutRepository;
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(Guid userId, WorkoutQueryParams queryParams)
        {
            return await _workoutRepository.GetWorkoutsAsync(userId, queryParams);
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsByDateAsync(Guid userId, DateTime date)
        {
            return await _workoutRepository.GetWorkoutsByDateAsync(userId, date);
        }

        public async Task<bool> CreateWorkoutAsync(Guid userId, WorkoutDto workoutDto, bool overwrite = false)
        {
            var existingWorkout = await _workoutRepository.GetWorkoutsByDateAsync(userId, workoutDto.Date);
            if (existingWorkout.Any())
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException("Workout already exists for the specified date. Enable overwrite to update.");
                }
                else
                {
                    // Overwrite existing workout
                    return await UpdateWorkoutAsync(userId, workoutDto);
                }
            }

            return await _workoutRepository.CreateWorkoutAsync(userId, workoutDto);
        }

        public async Task<bool> UpdateWorkoutAsync(Guid userId, WorkoutDto workoutDto)
        {
            return await _workoutRepository.UpdateWorkoutAsync(userId, workoutDto);
        }

        public async Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date)
        {
            return await _workoutRepository.DeleteWorkoutAsync(userId, date);
        }
    }
}
