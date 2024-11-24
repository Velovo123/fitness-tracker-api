using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTracker.API.Services.IServices;

namespace WorkoutFitnessTrackerAPI.Services
{
    public class ProgressRecordService : IProgressRecordService
    {
        private readonly IProgressRecordRepository _progressRecordRepository;
        private readonly IExerciseService _exerciseService;
        private readonly IMapper _mapper;

        public ProgressRecordService(IProgressRecordRepository progressRecordRepository, IExerciseService exerciseService, IMapper mapper)
        {
            _progressRecordRepository = progressRecordRepository;
            _exerciseService = exerciseService;
            _mapper = mapper;
        }

        public async Task<IEnumerable<ProgressRecordDto>> GetProgressRecordsAsync(Guid userId, ProgressRecordQueryParams queryParams)
        {
            var records = await _progressRecordRepository.GetProgressRecordsAsync(userId, queryParams);
            return _mapper.Map<IEnumerable<ProgressRecordDto>>(records);
        }

        public async Task<ProgressRecordDto?> GetProgressRecordByDateAsync(Guid userId, DateTime date, string exerciseName)
        {
            var normalizedExerciseName = await _exerciseService.NormalizeExerciseNameAsync(exerciseName);
            var record = await _progressRecordRepository.GetProgressRecordByDateAsync(userId, date, normalizedExerciseName);
            return record == null ? null : _mapper.Map<ProgressRecordDto>(record);
        }

        public async Task<bool> CreateProgressRecordAsync(Guid userId, ProgressRecordDto progressRecordDto, bool overwrite = false)
        {
            var normalizedExerciseName = await _exerciseService.NormalizeExerciseNameAsync(progressRecordDto.ExerciseName);
            var exercise = await _exerciseService.GetExerciseByNormalizedNameAsync(normalizedExerciseName)
                          ?? throw new InvalidOperationException("The specified exercise does not exist.");

            // Ensure the user is linked to the exercise
            await _exerciseService.EnsureUserExerciseLinkAsync(userId, exercise.Name);

            var existingRecord = await _progressRecordRepository.GetProgressRecordByDateAsync(userId, progressRecordDto.Date, normalizedExerciseName);
            if (existingRecord != null)
            {
                if (!overwrite) throw new InvalidOperationException("Progress record already exists. Enable overwrite to update.");
                return await UpdateProgressRecordAsync(userId, progressRecordDto);
            }

            var newRecord = new ProgressRecord
            {
                UserId = userId,
                ExerciseId = exercise.Id,
                Date = progressRecordDto.Date,
                Progress = progressRecordDto.Progress
            };

            return await _progressRecordRepository.CreateProgressRecordAsync(newRecord);
        }

        public async Task<bool> UpdateProgressRecordAsync(Guid userId, ProgressRecordDto progressRecordDto)
        {
            var normalizedExerciseName = await _exerciseService.NormalizeExerciseNameAsync(progressRecordDto.ExerciseName);
            var exercise = await _exerciseService.GetExerciseByNormalizedNameAsync(normalizedExerciseName)
                          ?? throw new InvalidOperationException("The specified exercise does not exist.");

            // Ensure the user is linked to the exercise
            await _exerciseService.EnsureUserExerciseLinkAsync(userId, exercise.Name);

            var recordToUpdate = _mapper.Map<ProgressRecord>(progressRecordDto);
            recordToUpdate.UserId = userId;
            recordToUpdate.ExerciseId = exercise.Id;
            return await _progressRecordRepository.UpdateProgressRecordAsync(recordToUpdate);
        }

        public async Task<bool> DeleteProgressRecordAsync(Guid userId, DateTime date, string exerciseName)
        {
            var normalizedExerciseName = await _exerciseService.NormalizeExerciseNameAsync(exerciseName);
            return await _progressRecordRepository.DeleteProgressRecordAsync(userId, date, normalizedExerciseName);
        }
    }
}
