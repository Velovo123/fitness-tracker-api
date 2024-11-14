using WorkoutFitnessTracker.API.Models.Dto_s.Summary;

namespace WorkoutFitnessTracker.API.Services.IServices
{
    public interface IInsightsService
    {
        Task<WorkoutStatisticsDto> CalculateAverageWorkoutDurationAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<List<MostFrequentExercisesDto>> GetMostFrequentExercisesAsync(Guid userId, DateTime? startDate = null, DateTime? endDate = null);
        Task<WeeklyMonthlySummaryDto> GetWeeklyMonthlySummaryAsync(Guid userId, DateTime startDate, DateTime endDate);
        Task<List<PeriodComparisonDto>> GetWeeklyMonthlyComparisonAsync(Guid userId, DateTime startDate, DateTime endDate, string intervalType);
        Task<List<ExerciseProgressTrendDto>> GetExerciseProgressTrendAsync(Guid userId, string exerciseName, DateTime? startDate = null, DateTime? endDate = null);
        Task<DailyProgressDto> GetDailyProgressAsync(Guid userId, DateTime date);
    }
}
