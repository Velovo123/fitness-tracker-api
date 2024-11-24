using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class ProgressRecordRepository : IProgressRecordRepository
    {
        private readonly WFTDbContext _context;

        public ProgressRecordRepository(WFTDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProgressRecord>> GetProgressRecordsAsync(Guid userId, ProgressRecordQueryParams queryParams)
        {
            var progressRecordsQuery = _context.ProgressRecords
                .Where(pr => pr.UserId == userId)
                .Include(pr => pr.Exercise)
                .AsQueryable();

            progressRecordsQuery = ApplyFilters(progressRecordsQuery, queryParams);
            progressRecordsQuery = ApplySorting(progressRecordsQuery, queryParams);
            progressRecordsQuery = ApplyPaging(progressRecordsQuery, queryParams);

            return await progressRecordsQuery.ToListAsync();
        }

        public async Task<ProgressRecord?> GetProgressRecordByDateAsync(Guid userId, DateTime date, string exerciseName)
        {
            return await _context.ProgressRecords
                .Include(pr => pr.Exercise)
                .FirstOrDefaultAsync(pr => pr.UserId == userId && pr.Date.Date == date.Date && pr.Exercise.Name == exerciseName);
        }

        public async Task<bool> CreateProgressRecordAsync(ProgressRecord progressRecord)
        {
            _context.ProgressRecords.Add(progressRecord);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateProgressRecordAsync(ProgressRecord progressRecord)
        {
            _context.ProgressRecords.Update(progressRecord);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> DeleteProgressRecordAsync(Guid userId, DateTime date, string exerciseName)
        {
            var progressRecord = await GetProgressRecordByDateAsync(userId, date, exerciseName);
            if (progressRecord == null) return false;

            _context.ProgressRecords.Remove(progressRecord);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<List<ProgressRecord>> GetProgressRecordsByDateRangeAsync(Guid userId, Guid exerciseId, DateTime? startDate = null, DateTime? endDate = null)
        {
            var query = _context.ProgressRecords
                .Where(pr => pr.UserId == userId && pr.ExerciseId == exerciseId);

            if (startDate.HasValue)
            {
                query = query.Where(pr => pr.Date >= startDate.Value);
            }

            if (endDate.HasValue)
            {
                query = query.Where(pr => pr.Date <= endDate.Value);
            }

            return await query.OrderBy(pr => pr.Date).AsNoTracking().ToListAsync();
        }

        private IQueryable<ProgressRecord> ApplyFilters(IQueryable<ProgressRecord> query, ProgressRecordQueryParams queryParams)
        {
            if (queryParams.StartDate.HasValue)
                query = query.Where(pr => pr.Date >= queryParams.StartDate.Value);
            if (queryParams.EndDate.HasValue)
                query = query.Where(pr => pr.Date <= queryParams.EndDate.Value);
            if (!string.IsNullOrEmpty(queryParams.ExerciseName))
                query = query.Where(pr => pr.Exercise.Name == queryParams.ExerciseName);

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
    }
}
