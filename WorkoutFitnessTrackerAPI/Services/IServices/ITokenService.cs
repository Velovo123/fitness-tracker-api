using WorkoutFitnessTrackerAPI.Models;

namespace WorkoutFitnessTrackerAPI.Services.IServices
{
    public interface ITokenService
    {
        string GenerateJwtToken(User user, IEnumerable<string> roles);
    }
}
