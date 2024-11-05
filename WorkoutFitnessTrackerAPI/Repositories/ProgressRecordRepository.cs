using AutoMapper;
using WorkoutFitnessTrackerAPI.Data;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Microsoft.EntityFrameworkCore;
using WorkoutFitnessTrackerAPI.Helpers;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Threading.Tasks;

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

        public async Task<bool> CreateProgressRecordAsync(Guid userId, ProgressRecordDto progressRecordDto)
        {
            var exercise = await _exerciseService.GetExerciseByNormalizedNameAsync(progressRecordDto.ExerciseName)
                          ?? throw new InvalidOperationException("The specified exercise does not exist.");

            await EnsureUserExerciseLinkAsync(userId, exercise.Id);

            var progressRecord = new ProgressRecord
            {
                UserId = userId,
                ExerciseId = exercise.Id,
                Date = progressRecordDto.Date,
                Progress = progressRecordDto.Progress
            };

            _context.ProgressRecords.Add(progressRecord);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> UpdateProgressRecordAsync(Guid userId, ProgressRecordDto progressRecordDto)
        {
            var exercise = await _exerciseService.GetExerciseByNormalizedNameAsync(progressRecordDto.ExerciseName)
                          ?? throw new InvalidOperationException("The specified exercise does not exist.");

            var existingRecord = await _context.ProgressRecords
                .FirstOrDefaultAsync(pr => pr.UserId == userId && pr.Date.Date == progressRecordDto.Date.Date && pr.ExerciseId == exercise.Id);

            if (existingRecord == null)
                throw new KeyNotFoundException("Progress record not found for the specified date and exercise.");

            existingRecord.Progress = progressRecordDto.Progress;
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

        private async Task EnsureUserExerciseLinkAsync(Guid userId, Guid exerciseId)
        {
            var userExerciseLinkExists = await _context.UserExercises
                .AnyAsync(ue => ue.UserId == userId && ue.ExerciseId == exerciseId);

            if (!userExerciseLinkExists)
            {
                _context.UserExercises.Add(new UserExercise { UserId = userId, ExerciseId = exerciseId });
                await _context.SaveChangesAsync();
            }
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
    }
}
