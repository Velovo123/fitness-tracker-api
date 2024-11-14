using WorkoutFitnessTracker.API.Models.Dto_s.Summary;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Services.IServices;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class InsightsService : IInsightsService
    {
        private readonly IWorkoutRepository _workoutRepository;
        private readonly IWorkoutPlanRepository _workoutPlanRepository;
        private readonly IProgressRecordRepository _progressRecordRepository;
        private readonly IExerciseService _exerciseService;

        public InsightsService(IWorkoutRepository workoutRepository,IWorkoutPlanRepository workoutPlanRepository, IProgressRecordRepository progressRecordRepository, IExerciseService exerciseService)
        {
            _workoutRepository = workoutRepository;
            _workoutPlanRepository = workoutPlanRepository;
            _progressRecordRepository = progressRecordRepository;
            _exerciseService = exerciseService;
        }

        public async Task<WorkoutStatisticsDto> CalculateAverageWorkoutDurationAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var workouts = await _workoutRepository.GetWorkoutsByDateRangeAsync(userId, startDate, endDate);

            if (!workouts.Any())
            {
                throw new InvalidOperationException("No workouts found for the specified date range.");
            }

            double averageDuration = workouts.Average(w => w.Duration);
            return new WorkoutStatisticsDto
            {
                AverageWorkoutDuration = averageDuration,
                WorkoutCount = workouts.Count()
            };
        }

        public async Task<List<MostFrequentExercisesDto>> GetMostFrequentExercisesAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var workouts = await _workoutRepository.GetWorkoutsByDateRangeAsync(userId, startDate, endDate);

            var mostFrequentExercises = workouts
                .SelectMany(w => w.WorkoutExercises)
                .GroupBy(we => we.Exercise.Name)
                .Select(group => new MostFrequentExercisesDto(group.Key, group.Count()))
                .OrderByDescending(dto => dto.Frequency)
                .Take(5)
                .ToList();

            return mostFrequentExercises;
        }

        public async Task<WeeklyMonthlySummaryDto> GetWeeklyMonthlySummaryAsync(Guid userId, DateTime startDate, DateTime endDate)
        {
            var workouts = await _workoutRepository.GetWorkoutsByDateRangeAsync(userId, startDate, endDate);

            if (!workouts.Any())
            {
                throw new InvalidOperationException("No workouts found for the specified date range.");
            }

            int totalWorkouts = workouts.Count();
            double averageDuration = workouts.Average(w => w.Duration);
            int totalReps = workouts.Sum(w => w.WorkoutExercises.Sum(we => we.Reps));
            int totalSets = workouts.Sum(w => w.WorkoutExercises.Sum(we => we.Sets));

            return new WeeklyMonthlySummaryDto
            {
                TotalWorkouts = totalWorkouts,
                AverageDuration = averageDuration,
                TotalReps = totalReps,
                TotalSets = totalSets,
                StartDate = startDate,
                EndDate = endDate
            };
        }

        public async Task<List<PeriodComparisonDto>> GetWeeklyMonthlyComparisonAsync(Guid userId, DateTime startDate, DateTime endDate, string intervalType)
        {
            var workouts = await _workoutRepository.GetWorkoutsByDateRangeAsync(userId, startDate, endDate);

            if (!workouts.Any())
            {
                throw new InvalidOperationException("No workouts found for the specified date range.");
            }

            IEnumerable<IGrouping<object, Workout>> groupedWorkouts = intervalType.ToLower() switch
            {
                "weekly" => workouts.GroupBy(w => new { w.Date.Year, Week = ISOWeek.GetWeekOfYear(w.Date) }),
                "monthly" => workouts.GroupBy(w => new { w.Date.Year, w.Date.Month }),
                _ => throw new ArgumentException("Invalid interval type. Use 'weekly' or 'monthly'.")
            };


            var comparisonData = groupedWorkouts.Select(group =>
            {
                var periodStart = group.Min(w => w.Date);
                var periodEnd = group.Max(w => w.Date);
                var totalWorkouts = group.Count();
                var averageDuration = group.Average(w => w.Duration);
                var totalReps = group.Sum(w => w.WorkoutExercises.Sum(we => we.Reps));
                var totalSets = group.Sum(w => w.WorkoutExercises.Sum(we => we.Sets));

                return new PeriodComparisonDto
                {
                    PeriodStart = periodStart,
                    PeriodEnd = periodEnd,
                    TotalWorkouts = totalWorkouts,
                    AverageDuration = averageDuration,
                    TotalReps = totalReps,
                    TotalSets = totalSets
                };
            }).ToList();

            return comparisonData;
        }

        public async Task<List<ExerciseProgressTrendDto>> GetExerciseProgressTrendAsync(Guid userId, string exerciseName, DateTime? startDate = null, DateTime? endDate = null)
        {
            var exercise = await _exerciseService.GetExerciseByNormalizedNameAsync(exerciseName)
                          ?? throw new InvalidOperationException("The specified exercise does not exist.");

            await _exerciseService.EnsureUserExerciseLinkAsync(userId, exercise.Id);

            var progressRecords = await _progressRecordRepository.GetProgressRecordsByDateRangeAsync(userId, exercise.Id, startDate, endDate);

            return progressRecords
                .OrderBy(pr => pr.Date)
                .Select(pr => new ExerciseProgressTrendDto
                {
                    Date = pr.Date,
                    Progress = pr.Progress
                })
                .ToList();
        }

        public async Task<DailyProgressDto> GetDailyProgressAsync(Guid userId, DateTime date)
        {
            var workouts = await _workoutRepository.GetWorkoutsByDateTimeAsync(userId, date);
            var workoutPlans = await _workoutPlanRepository.GetWorkoutPlansForDateAsync(userId, date); 

            if (!workouts.Any() && !workoutPlans.Any())
            {
                throw new InvalidOperationException("No workouts or workout plans found for the specified date.");
            }

            int totalSets = workouts.Sum(w => w.WorkoutExercises.Sum(we => we.Sets));
            int totalReps = workouts.Sum(w => w.WorkoutExercises.Sum(we => we.Reps));
            double averageDuration = workouts.Any() ? workouts.Average(w => w.Duration) : 0;
            int totalWorkouts = workouts.Count();

            var exerciseProgress = workoutPlans
                .SelectMany(plan => plan.WorkoutPlanExercises)
                .GroupBy(e => e.Exercise.Name)
                .Select(g =>
                {
                    var plannedExercise = g.First();
                    var completedExercise = workouts
                        .SelectMany(w => w.WorkoutExercises)
                        .FirstOrDefault(we => we.Exercise.Name == plannedExercise.Exercise.Name);

                    return new ExerciseProgressDto
                    {
                        ExerciseName = plannedExercise.Exercise.Name,
                        PlannedSets = plannedExercise.Sets,
                        PlannedReps = plannedExercise.Reps,
                        ActualSets = completedExercise?.Sets ?? 0,
                        ActualReps = completedExercise?.Reps ?? 0
                    };
                })
                .ToList();

            return new DailyProgressDto
            {
                Date = date,
                TotalWorkouts = totalWorkouts,
                AverageDuration = averageDuration,
                TotalReps = totalReps,
                TotalSets = totalSets,
                Exercises = exerciseProgress
            };
        }

    }
}
