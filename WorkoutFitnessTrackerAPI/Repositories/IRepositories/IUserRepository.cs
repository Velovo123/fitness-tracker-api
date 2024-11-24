using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.User;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<AuthResult> RegisterUserAsync(UserRegistrationDto registrationDto);
        Task<AuthResult> LoginUserAsync(UserLoginDto loginDto);
        Task<UserProfileDto?> GetUserByEmailAsync(string email);
        Task<UserProfileDto?> GetUserByIdAsync(Guid userId);
    }
}
