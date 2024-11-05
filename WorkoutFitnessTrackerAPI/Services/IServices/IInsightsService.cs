using WorkoutFitnessTracker.API.Models.Dto_s.Summary;

namespace WorkoutFitnessTracker.API.Services.IServices
{
    public interface IInsightsService
    {
        Task<IEnumerable<string>> GetSuggestedExercisesAsync(Guid userId);
        Task<string?> GetIntensityAdjustmentsAsync(Guid userId, Guid exerciseId);
        Task<IEnumerable<string>> GetGoalAlignmentRecommendationsAsync(Guid userId);
        Task<IEnumerable<string>> DetectPlateausAsync(Guid userId);
        Task<IEnumerable<string>> GetRecoveryInsightsAsync(Guid userId);
        Task<IEnumerable<string>> GetUserPreferredExercisesAsync(Guid userId);
        Task<ProgressSummaryDto> GetWeeklyOrMonthlySummaryAsync(Guid userId, DateTime startDate, DateTime endDate);
    }
}
