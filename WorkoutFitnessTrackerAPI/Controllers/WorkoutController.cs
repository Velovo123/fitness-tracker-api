using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class WorkoutController : BaseApiController
    {
        private readonly IWorkoutRepository _workoutRepository;

        public WorkoutController(IWorkoutRepository workoutRepository)
        {
            _workoutRepository = workoutRepository;
        }

        [HttpGet]
        [ResponseCache(Duration = 60)]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<WorkoutDto>>>> GetWorkouts([FromQuery] WorkoutQueryParams? queryParams = null)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ResponseWrapper<string>(false, null, string.Join("; ", errors)));
            }
            var userGuid = GetUserId();
            var workouts = await _workoutRepository.GetWorkoutsAsync(userGuid, queryParams!);

            return workouts == null || !workouts.Any()
                ? NotFound(new ResponseWrapper<IEnumerable<WorkoutDto>>(false, null, "No workouts found for this user."))
                : WrapResponse(true, workouts, "Workouts retrieved successfully.");
        }

        [HttpPost]
        public async Task<ActionResult<ResponseWrapper<string>>> SaveWorkout([FromBody] WorkoutDto workoutDto, [FromQuery] bool overwrite = false)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ResponseWrapper<string>(false, null, string.Join(", ", errors)));
            }

            var userGuid = GetUserId();
            var result = await _workoutRepository.SaveWorkoutAsync(userGuid, workoutDto, overwrite);

            return result
                ? WrapResponse(true, "Workout saved successfully.", "Workout saved successfully.")
                : WrapResponse<string>(false, null, "Failed to save the workout.");
        }

        [HttpGet("date/{date}")]
        [ResponseCache(Duration = 30)]
        public async Task<ActionResult<ResponseWrapper<IEnumerable<WorkoutDto>>>> GetWorkoutByDate(DateTime date)
        {
            var userGuid = GetUserId();
            var workouts = await _workoutRepository.GetWorkoutsByDateAsync(userGuid, date);

            return workouts == null || !workouts.Any()
                ? NotFound(new ResponseWrapper<IEnumerable<WorkoutDto>>(false, null, $"No workout found for the date {date.ToShortDateString()}."))
                : WrapResponse(true, workouts, "Workout retrieved successfully.");
        }

        [HttpDelete("date/{date}")]
        public async Task<ActionResult<ResponseWrapper<string>>> DeleteWorkout(DateTime date)
        {
            var userGuid = GetUserId();
            var result = await _workoutRepository.DeleteWorkoutAsync(userGuid, date);

            return result
                ? WrapResponse(true, $"Workout for the date {date.ToShortDateString()} deleted successfully.", "Workout deleted successfully.")
                : NotFound(new ResponseWrapper<string>(false, null, $"No workout found for the date {date.ToShortDateString()} to delete."));
        }
    }
}
