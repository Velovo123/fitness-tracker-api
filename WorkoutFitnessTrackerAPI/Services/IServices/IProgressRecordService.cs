using WorkoutFitnessTracker.API.Models.Dto_s.Summary;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTracker.API.Services.IServices
{
    public interface IProgressRecordService
    {
        Task<IEnumerable<ProgressRecordDto>> GetProgressRecordsAsync(Guid userId, ProgressRecordQueryParams queryParams);
        Task<ProgressRecordDto?> GetProgressRecordByDateAsync(Guid userId, DateTime date, string exerciseName);
        Task<bool> CreateProgressRecordAsync(Guid userId, ProgressRecordDto progressRecordDto, bool overwrite = false);
        Task<bool> UpdateProgressRecordAsync(Guid userId, ProgressRecordDto progressRecordDto);
        Task<bool> DeleteProgressRecordAsync(Guid userId, DateTime date, string exerciseName);
        Task<List<ExerciseProgressTrendDto>> GetExerciseProgressTrendAsync(Guid userId, string exerciseName, DateTime? startDate = null, DateTime? endDate = null);
    }
}
