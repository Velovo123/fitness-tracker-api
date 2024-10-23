using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTrackerAPI.Repositories.IRepositories
{
    public interface IUserRepository
    {
        Task<AuthResult> RegisterUserAsync(UserRegistrationDto registrationDto);
        Task<AuthResult> LoginUserAsync(UserLoginDto loginDto);
        Task<User?> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(Guid userId);
    }
}
