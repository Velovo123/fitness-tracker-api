using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkoutFitnessTrackerAPI.Helpers;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    public abstract class BaseApiController : ControllerBase
    {
        protected ActionResult WrapResponse<T>(bool success, T? data, string message)
        {
            return Ok(new ResponseWrapper<T>(success, data, message));
        }

        protected Guid GetUserId()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
            {
                throw new UnauthorizedAccessException("User ID is missing from the token.");
            }
            return Guid.Parse(userId);
        }
    }
}
