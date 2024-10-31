using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Helpers;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ProgressRecordController : BaseApiController
    {
        private readonly IProgressRecordRepository _progressRecordRepository;

        public ProgressRecordController(IProgressRecordRepository progressRecordRepository)
        {
            _progressRecordRepository = progressRecordRepository;
        }

        [HttpGet]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<ProgressRecordDto>>>> GetProgressRecords([FromQuery] ProgressRecordQueryParams queryParams)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ResponseWrapper<string>(false, null, string.Join("; ", errors)));
            }
            var userGuid = GetUserId();
            var progressRecords = await _progressRecordRepository.GetProgressRecordsAsync(userGuid, queryParams);

            return progressRecords == null || !progressRecords.Any()
                ? NotFound(new ResponseWrapper<IEnumerable<ProgressRecordDto>>(false, null, "No progress records found for this user."))
                : WrapResponse(true, progressRecords, "Progress records retrieved successfully.");
        }

        [HttpPost]
        public async Task<ActionResult<ResponseWrapper<string>>> SaveProgressRecord([FromBody] ProgressRecordDto progressRecordDto, [FromQuery] bool overwrite = false)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ResponseWrapper<string>(false, null, string.Join(", ", errors)));
            }

            var userGuid = GetUserId();
            var result = await _progressRecordRepository.SaveProgressRecordAsync(userGuid, progressRecordDto, overwrite);

            return result
                ? WrapResponse(true, "Progress record saved successfully.", "Progress record saved successfully.")
                : WrapResponse<string>(false, null, "Failed to save the progress record: No changes were detected, or an error occurred.");
        }

        [HttpGet("date/{date}/exercise/{exerciseName}")]
        public async Task<ActionResult<ResponseWrapper<ProgressRecordDto>>> GetProgressRecordByDate(DateTime date, string exerciseName)
        {
            var userGuid = GetUserId();
            var progressRecord = await _progressRecordRepository.GetProgressRecordByDateAsync(userGuid, date, exerciseName);

            return progressRecord == null
                ? NotFound(new ResponseWrapper<ProgressRecordDto>(false, null, $"No progress record found for {exerciseName} on {date}."))
                : WrapResponse(true, progressRecord, "Progress record retrieved successfully.");
        }

        [HttpDelete("date/{date}/exercise/{exerciseName}")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteProgressRecord(DateTime date, string exerciseName)
        {
            var userGuid = GetUserId();
            var result = await _progressRecordRepository.DeleteProgressRecordAsync(userGuid, date, exerciseName);

            return result
                ? WrapResponse(true, $"Progress record for {exerciseName} on {date} deleted successfully.", "Progress record deleted successfully.")
                : NotFound(new ResponseWrapper<string>(false, null, $"No progress record found for {exerciseName} on {date} to delete."));
        }
    }
}
