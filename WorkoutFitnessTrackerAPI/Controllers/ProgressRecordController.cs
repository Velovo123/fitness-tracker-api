using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Services.IServices;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTracker.API.Services.IServices;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgressRecordController : BaseApiController
    {
        private readonly IProgressRecordService _progressRecordService;

        public ProgressRecordController(IProgressRecordService progressRecordService)
        {
            _progressRecordService = progressRecordService;
        }

        [HttpGet]
        public async Task<IActionResult> GetProgressRecords([FromQuery] ProgressRecordQueryParams queryParams)
        {
            var userId = GetUserId();
            var progressRecords = await _progressRecordService.GetProgressRecordsAsync(userId, queryParams);

            if (!progressRecords.Any())
            {
                return NotFound(WrapResponse(false, Enumerable.Empty<ProgressRecordDto>(), "No progress records found for the specified criteria."));
            }
            return Ok(WrapResponse(true, progressRecords, "Progress records retrieved successfully."));
        }

        [HttpPost]
        public async Task<IActionResult> SaveProgressRecord([FromBody] ProgressRecordDto progressRecordDto, [FromQuery] bool overwrite = false)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(WrapResponse(false, (string?)null, "Invalid progress record data."));
            }

            var userId = GetUserId();
            var success = await _progressRecordService.CreateProgressRecordAsync(userId, progressRecordDto, overwrite);

            if (success)
            {
                return CreatedAtAction(nameof(GetProgressRecordByDate), new { date = progressRecordDto.Date, exerciseName = progressRecordDto.ExerciseName },
                    WrapResponse(true, (string?)null, "Progress record saved successfully."));
            }
            return BadRequest(WrapResponse(false, (string?)null, "Failed to save progress record."));
        }

        [HttpGet("date/{date}/exercise/{exerciseName}")]
        public async Task<IActionResult> GetProgressRecordByDate(DateTime date, string exerciseName)
        {
            var userId = GetUserId();
            var progressRecord = await _progressRecordService.GetProgressRecordByDateAsync(userId, date, exerciseName);

            if (progressRecord == null)
            {
                return NotFound(WrapResponse(false, (ProgressRecordDto?)null, "Progress record not found for the specified date and exercise name."));
            }
            return Ok(WrapResponse(true, progressRecord, "Progress record retrieved successfully."));
        }

        [HttpDelete("date/{date}/exercise/{exerciseName}")]
        public async Task<IActionResult> DeleteProgressRecord(DateTime date, string exerciseName)
        {
            var userId = GetUserId();
            var success = await _progressRecordService.DeleteProgressRecordAsync(userId, date, exerciseName);

            if (success)
            {
                return NoContent();
            }
            return NotFound(WrapResponse(false, (string?)null, "Progress record not found for the specified date and exercise name."));
        }
    }
}
