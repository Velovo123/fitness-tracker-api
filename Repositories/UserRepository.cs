using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly TokenService _tokenService;
        public UserRepository(UserManager<User> userManager, SignInManager<User> signInManager, TokenService tokenService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            return await _userManager.FindByEmailAsync(email);
        }

        public async Task<User?> GetUserByIdAsync(Guid id)
        {
            return await _userManager.FindByIdAsync(id.ToString());
        }

        public async Task<AuthResult> LoginUserAsync(UserLoginDto loginDto)
        {
            var user = await _userManager.FindByEmailAsync(loginDto.Email);
            if(user == null)
            {
                return new AuthResult(
                    Success: false,
                    Errors: ["Invalid credentials"]
                );
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, loginDto.Password, false);

            if (result.Succeeded)
            {
                var roles = await _userManager.GetRolesAsync(user);
                var token = _tokenService.GenerateJwtToken(user,roles);
                return new AuthResult(Success: true, Token: token);
            }

            return new AuthResult(
                Success: false,
                Errors: ["Invalid credentials"]
            );
        }

        public async Task<AuthResult> RegisterUserAsync(UserRegistrationDto registrationDto)
        {
            var user = new User
            {
                UserName = registrationDto.Email,
                Email = registrationDto.Email,
                Name = registrationDto.Name
            };

            var result = await _userManager.CreateAsync(user, registrationDto.Password);

            if (result.Succeeded) 
            {
                return new AuthResult(Success: true);
            }

            return new AuthResult(
                Success: false, 
                Errors: result.Errors.Select(e => e.Description).ToList()
            );
        }
    }
}
