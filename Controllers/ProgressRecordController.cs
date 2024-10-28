using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgressRecordController : ControllerBase
    {
        private readonly IProgressRecordRepository _progressRecordRepository;

        public ProgressRecordController(IProgressRecordRepository progressRecordRepository)
        {
            _progressRecordRepository = progressRecordRepository;
        }

        // /api/ProgressRecord
        [HttpGet]
        public async Task<ActionResult<IEnumerable<ProgressRecordDto>>> GetProgressRecords([FromQuery] ProgressRecordQueryParams queryParams)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized("User ID is missing from the token.");
            }

            var userGuid = Guid.Parse(userId);

            var progressRecords = await _progressRecordRepository.GetProgressRecordsAsync(userGuid, queryParams);

            if (progressRecords == null || !progressRecords.Any())
            {
                return NotFound("No progress records found for this user.");
            }

            return Ok(progressRecords);
        }

        // /api/ProgressRecord
        [HttpPost]
        public async Task<ActionResult> SaveProgressRecord([FromBody] ProgressRecordDto progressRecordDto, [FromQuery] bool overwrite = false)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID is missing from the token.");

            var userGuid = Guid.Parse(userId);

            try
            {
                var result = await _progressRecordRepository.SaveProgressRecordAsync(userGuid, progressRecordDto, overwrite);

                if (result)
                    return Ok("Progress record saved successfully.");
                else
                    return BadRequest("Failed to save the progress record: No changes were detected, or an error occurred.");
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(ex.Message);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // /api/ProgressRecord/date/{date}/exercise/{exerciseName}
        [HttpGet("date/{date}/exercise/{exerciseName}")]
        public async Task<ActionResult<ProgressRecordDto>> GetProgressRecordByDate(DateTime date, string exerciseName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID is missing from the token.");

            var userGuid = Guid.Parse(userId);

            var progressRecord = await _progressRecordRepository.GetProgressRecordByDateAsync(userGuid, date, exerciseName);

            if (progressRecord == null)
                return NotFound($"No progress record found for {exerciseName} on {date}.");

            return Ok(progressRecord);
        }

        // /api/ProgressRecord/date/{date}/exercise/{exerciseName}
        [HttpDelete("date/{date}/exercise/{exerciseName}")]
        public async Task<ActionResult> DeleteProgressRecord(DateTime date, string exerciseName)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized("User ID is missing from the token.");

            var userGuid = Guid.Parse(userId);

            var result = await _progressRecordRepository.DeleteProgressRecordAsync(userGuid, date, exerciseName);

            if (!result)
                return NotFound($"No progress record found for {exerciseName} on {date} to delete.");

            return Ok($"Progress record for {exerciseName} on {date} deleted successfully.");
        }
    }
}
