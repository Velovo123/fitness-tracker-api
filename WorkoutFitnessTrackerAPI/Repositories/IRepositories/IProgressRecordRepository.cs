using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IProgressRecordRepository
    {
        Task<IEnumerable<ProgressRecordDto>> GetProgressRecordsAsync(Guid userId, ProgressRecordQueryParams queryParams);
        Task<ProgressRecordDto?> GetProgressRecordByDateAsync(Guid userId, DateTime date, string exerciseName);
        Task<bool> CreateProgressRecordAsync(Guid userId, ProgressRecordDto progressRecordDto);
        Task<bool> UpdateProgressRecordAsync(Guid userId, ProgressRecordDto progressRecordDto);
        Task<bool> DeleteProgressRecordAsync(Guid userId, DateTime date, string exerciseName);
    }
}
