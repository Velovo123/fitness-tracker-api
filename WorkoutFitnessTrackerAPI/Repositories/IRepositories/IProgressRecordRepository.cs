using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IProgressRecordRepository
    {
        Task<IEnumerable<ProgressRecord>> GetProgressRecordsAsync(Guid userId, ProgressRecordQueryParams queryParams);
        Task<ProgressRecord?> GetProgressRecordByDateAsync(Guid userId, DateTime date, string exerciseName);
        Task<bool> CreateProgressRecordAsync(ProgressRecord progressRecord);
        Task<bool> UpdateProgressRecordAsync(ProgressRecord progressRecord);
        Task<bool> DeleteProgressRecordAsync(Guid userId, DateTime date, string exerciseName);
    }
}
