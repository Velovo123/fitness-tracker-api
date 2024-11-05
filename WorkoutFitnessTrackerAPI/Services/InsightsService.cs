using WorkoutFitnessTracker.API.Models.Dto_s.Summary;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;

namespace WorkoutFitnessTracker.API.Services
{
    public class InsightsService : IInsightsService
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IProgressRecordRepository _progressRecordRepository;

        public InsightsService(IWorkoutRepository workoutRepository, IProgressRecordRepository progressRecordRepository)
        {
            _workoutRepository = workoutRepository;
            _progressRecordRepository = progressRecordRepository;
        }
        public Task<IEnumerable<string>> DetectPlateausAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetGoalAlignmentRecommendationsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<string?> GetIntensityAdjustmentsAsync(Guid userId, Guid exerciseId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetRecoveryInsightsAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetSuggestedExercisesAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<string>> GetUserPreferredExercisesAsync(Guid userId)
        {
            throw new NotImplementedException();
        }

        public async Task<ProgressSummaryDto> GetWeeklyOrMonthlySummaryAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            // Retrieve all workouts for the user within the specified date range
            var workouts = await _workoutRepository.GetWorkoutsAsync(userId, new WorkoutQueryParams
            {
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = 1,
                PageSize = int.MaxValue
            });

            if (!workouts.Any())
            {       
                throw new KeyNotFoundException("No workouts found within the specified date range.");
            }

            int totalWorkouts = workouts.Count();
            int totalDuration = workouts.Sum(w => w.Duration);

            // Calculate exercise frequency
            var exerciseFrequency = new Dictionary<string, int>();
            foreach (var workout in workouts)
            {
                foreach (var exercise in workout.Exercises)
                {
                    if (exerciseFrequency.ContainsKey(exercise.ExerciseName))
                    {
                        exerciseFrequency[exercise.ExerciseName]++;
                    }
                    else
                    {
                        exerciseFrequency[exercise.ExerciseName] = 1;
                    }
                }
            }

            int uniqueExercises = exerciseFrequency.Count;

            // Retrieve progress records and analyze achievements
            var achievements = new List<string>();
            var progressRecords = await _progressRecordRepository.GetProgressRecordsAsync(userId, new ProgressRecordQueryParams
            {
                StartDate = startDate,
                EndDate = endDate,
                PageNumber = 1,
                PageSize = int.MaxValue
            });

            if (progressRecords.Any())
            {
                foreach (var progressRecord in progressRecords)
                {
                    achievements.Add($"Improved in {progressRecord.ExerciseName}: {progressRecord.Progress}");
                }
            }

            return new ProgressSummaryDto
            {
                TotalWorkouts = totalWorkouts,
                TotalDuration = totalDuration,
                UniqueExercises = uniqueExercises,
                ExerciseFrequency = exerciseFrequency,
                Achievements = achievements,
                StartDate = startDate,
                EndDate = endDate
            };
        }

    }
}
