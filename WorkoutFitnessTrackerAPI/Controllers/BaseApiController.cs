using Microsoft.AspNetCore.Mvc;
using WorkoutFitnessTrackerAPI.Helpers;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult WrapResponse<T>(bool success, T? data, string message)
        {
            return Ok(new ResponseWrapper<T>(success, data, message));
        }
    }
}
