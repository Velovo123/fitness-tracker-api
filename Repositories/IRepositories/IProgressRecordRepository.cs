using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IProgressRecordRepository
    {
        Task<IEnumerable<ProgressRecordDto>> GetProgressRecordsAsync(Guid userId, ProgressRecordQueryParams queryParams);
        Task<ProgressRecordDto?> GetProgressRecordByDateAsync(Guid userId, DateTime date, string exerciseName);
        Task<bool> SaveProgressRecordAsync(Guid userId, ProgressRecordDto progressRecordDto, bool overwrite = false);
        Task<bool> DeleteProgressRecordAsync(Guid userId, DateTime date, string exerciseName);
    }
}
