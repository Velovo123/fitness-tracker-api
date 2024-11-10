using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using AutoMapper;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Services.IServices;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class WorkoutService : IWorkoutService
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IExerciseService _exerciseService;
        private readonly IMapper _mapper;

        public WorkoutService(IWorkoutRepository workoutRepository, IExerciseService exerciseService, IMapper mapper)
        {
            _workoutRepository = workoutRepository;
            _exerciseService = exerciseService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsAsync(Guid userId, WorkoutQueryParams queryParams)
        {
            var workouts = await _workoutRepository.GetWorkoutsAsync(userId, queryParams);
            return _mapper.Map<IEnumerable<WorkoutDto>>(workouts);
        }

        public async Task<IEnumerable<WorkoutDto>> GetWorkoutsByDateAsync(Guid userId, DateTime date)
        {
            var workouts = await _workoutRepository.GetWorkoutsByDateTimeAsync(userId, date);
            return _mapper.Map<IEnumerable<WorkoutDto>>(workouts);
        }

        public async Task<bool> CreateWorkoutAsync(Guid userId, WorkoutDto workoutDto, bool overwrite = false)
        {
            var existingWorkouts = await _workoutRepository.GetWorkoutsByDateTimeAsync(userId, workoutDto.Date);
            if (existingWorkouts.Any() && !overwrite)
            {
                throw new InvalidOperationException("Workout already exists for the specified date and time. Enable overwrite to update.");
            }
            else if (existingWorkouts.Any() && overwrite)
            {
                return await UpdateWorkoutAsync(userId, workoutDto);
            }

            var workout = _mapper.Map<Workout>(workoutDto);
            workout.UserId = userId;

            // Prepare and deduplicate exercises
            var workoutExercises = await _exerciseService.PrepareExercises<WorkoutExercise>(userId, workoutDto.Exercises);
            workout.WorkoutExercises = workoutExercises;

            return await _workoutRepository.CreateWorkoutAsync(workout);
        }

        public async Task<bool> UpdateWorkoutAsync(Guid userId, WorkoutDto workoutDto)
        {
            // Delete the existing workout for the specified user and date
            var deleteSuccess = await _workoutRepository.DeleteWorkoutAsync(userId, workoutDto.Date);
            if (!deleteSuccess)
            {
                return false;
            }

            // Create a new workout instance with the updated details
            var newWorkout = _mapper.Map<Workout>(workoutDto);
            newWorkout.UserId = userId;

            // Prepare and add exercises as new entries
            var workoutExercises = await _exerciseService.PrepareExercises<WorkoutExercise>(userId, workoutDto.Exercises);
            newWorkout.WorkoutExercises = workoutExercises;

            return await _workoutRepository.CreateWorkoutAsync(newWorkout);
        }

        public async Task<bool> DeleteWorkoutAsync(Guid userId, DateTime date)
        {
            return await _workoutRepository.DeleteWorkoutAsync(userId, date);
        }
    }
}
