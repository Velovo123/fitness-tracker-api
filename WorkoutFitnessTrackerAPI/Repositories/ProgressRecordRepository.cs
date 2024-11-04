using AutoMapper;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Helpers;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class ProgressRecordRepository : IProgressRecordRepository
    {
        private readonly WFTDbContext _context;
        private readonly IExerciseService _exerciseService;
        private readonly IMapper _mapper;

        public ProgressRecordRepository(WFTDbContext context, IExerciseService exerciseService, IMapper mapper)
        {
            _context = context;
            _exerciseService = exerciseService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProgressRecordDto>> GetProgressRecordsAsync(Guid userId, ProgressRecordQueryParams queryParams)
        {
            var progressRecordsQuery = _context.ProgressRecords
                .Where(pr => pr.UserId == userId)
                .Include(pr => pr.Exercise)
                .AsQueryable();

            progressRecordsQuery = ApplyFilters(progressRecordsQuery, queryParams);
            progressRecordsQuery = ApplySorting(progressRecordsQuery, queryParams);
            progressRecordsQuery = ApplyPaging(progressRecordsQuery, queryParams);

            var progressRecords = await progressRecordsQuery.ToListAsync();
            return _mapper.Map<IEnumerable<ProgressRecordDto>>(progressRecords);
        }

        public async Task<ProgressRecordDto?> GetProgressRecordByDateAsync(Guid userId, DateTime date, string exerciseName)
        {
            var normalizedExerciseName = NameNormalizationHelper.NormalizeName(exerciseName);
            var progressRecord = await _context.ProgressRecords
                .Include(pr => pr.Exercise)
                .FirstOrDefaultAsync(pr => pr.UserId == userId && pr.Date.Date == date.Date && pr.Exercise.Name == normalizedExerciseName);

            return progressRecord == null ? null : _mapper.Map<ProgressRecordDto>(progressRecord);
        }

        public async Task<bool> SaveProgressRecordAsync(Guid userId, ProgressRecordDto progressRecordDto, bool overwrite = false)
        {
            if (progressRecordDto == null)
                throw new ArgumentNullException(nameof(progressRecordDto));

            if (string.IsNullOrWhiteSpace(progressRecordDto.ExerciseName))
                throw new ArgumentException("Exercise name cannot be null or empty.", nameof(progressRecordDto.ExerciseName));

            if (string.IsNullOrWhiteSpace(progressRecordDto.Progress))
                throw new ArgumentException("Progress cannot be null or empty.", nameof(progressRecordDto.Progress));

            var exercise = await _exerciseService.GetExerciseByNormalizedNameAsync(progressRecordDto.ExerciseName);
            if (exercise == null)
            {
                throw new InvalidOperationException("The specified exercise does not exist.");
            }

            var isUserLinkedToExercise = await _context.UserExercises
                .AnyAsync(ue => ue.UserId == userId && ue.ExerciseId == exercise.Id);

            if (!isUserLinkedToExercise)
            {
                throw new InvalidOperationException("User is not linked to the specified exercise.");
            }

            var existingRecord = await _context.ProgressRecords
                .FirstOrDefaultAsync(pr => pr.UserId == userId && pr.Date.Date == progressRecordDto.Date.Date && pr.ExerciseId == exercise.Id);

            if (existingRecord != null)
            {
                if (!overwrite)
                {
                    throw new InvalidOperationException("A progress record for this exercise on the given date already exists. Confirm overwrite.");
                }
                UpdateExistingProgressRecord(existingRecord, progressRecordDto.Progress);
            }
            else
            {
                CreateNewProgressRecord(userId, exercise.Id, progressRecordDto.Date, progressRecordDto.Progress);
            }

            return await _context.SaveChangesAsync() > 0;
        }


        public async Task<bool> DeleteProgressRecordAsync(Guid userId, DateTime date, string exerciseName)
        {
            var normalizedExerciseName = NameNormalizationHelper.NormalizeName(exerciseName);
            var progressRecord = await _context.ProgressRecords
                .FirstOrDefaultAsync(pr => pr.UserId == userId && pr.Date.Date == date.Date && pr.Exercise.Name == normalizedExerciseName);

            if (progressRecord == null) return false;

            _context.ProgressRecords.Remove(progressRecord);
            return await _context.SaveChangesAsync() > 0;
        }

        private IQueryable<ProgressRecord> ApplyFilters(IQueryable<ProgressRecord> query, ProgressRecordQueryParams queryParams)
        {
            if (queryParams.StartDate.HasValue) query = query.Where(pr => pr.Date >= queryParams.StartDate.Value);
            if (queryParams.EndDate.HasValue) query = query.Where(pr => pr.Date <= queryParams.EndDate.Value);
            if (!string.IsNullOrEmpty(queryParams.ExerciseName))
            {
                var normalizedExerciseName = NameNormalizationHelper.NormalizeName(queryParams.ExerciseName);
                query = query.Where(pr => pr.Exercise.Name == normalizedExerciseName);
            }
            return query;
        }

        private IQueryable<ProgressRecord> ApplySorting(IQueryable<ProgressRecord> query, ProgressRecordQueryParams queryParams)
        {
            return queryParams.SortBy?.ToLower() switch
            {
                "date" => queryParams.SortDescending == true ? query.OrderByDescending(pr => pr.Date) : query.OrderBy(pr => pr.Date),
                _ => query.OrderBy(pr => pr.Date)
            };
        }

        private IQueryable<ProgressRecord> ApplyPaging(IQueryable<ProgressRecord> query, ProgressRecordQueryParams queryParams)
        {
            return query.Skip(((queryParams.PageNumber ?? 1) - 1) * (queryParams.PageSize ?? 10))
                        .Take(queryParams.PageSize ?? 10);
        }


        private IQueryable<ProgressRecord> ApplySortingAndPaging(IQueryable<ProgressRecord> query, ProgressRecordQueryParams queryParams)
        {
            query = queryParams.SortBy?.ToLower() switch
            {
                "date" => queryParams.SortDescending == true ? query.OrderByDescending(pr => pr.Date) : query.OrderBy(pr => pr.Date),
                _ => query.OrderBy(pr => pr.Date)
            };

            return query
                .Skip(((queryParams.PageNumber ?? 1) - 1) * (queryParams.PageSize ?? 10))
                .Take(queryParams.PageSize ?? 10);
        }

        private void UpdateExistingProgressRecord(ProgressRecord existingRecord, string newProgress)
        {
            existingRecord.Progress = newProgress;
        }

        private void CreateNewProgressRecord(Guid userId, Guid exerciseId, DateTime date, string progress)
        {
            var progressRecord = new ProgressRecord
            {
                UserId = userId,
                ExerciseId = exerciseId,
                Date = date,
                Progress = progress
            };
            _context.ProgressRecords.Add(progressRecord);
        }
    }
}
