using AutoMapper;
using Microsoft.AspNetCore.Identity;
using System.Data;
using System.Diagnostics.CodeAnalysis;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.User;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using WorkoutFitnessTrackerAPI.Services;
using WorkoutFitnessTrackerAPI.Services.IServices;

namespace WorkoutFitnessTrackerAPI.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IMapper _mapper;
        public UserRepository(UserManager<User> userManager, SignInManager<User> signInManager, ITokenService tokenService, IMapper mapper)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _mapper = mapper;
        }

        public async Task<UserProfileDto?> GetUserByEmailAsync(string email)
        {
            var user = await _userManager.FindByEmailAsync(email);
            return user == null ? null : _mapper.Map<UserProfileDto>(user);
        }

        public async Task<UserProfileDto?> GetUserByIdAsync(Guid id)
        {
            var user =  await _userManager.FindByIdAsync(id.ToString());
            return user == null ? null : _mapper.Map<UserProfileDto>(user);
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
                var roleResult = await _userManager.AddToRoleAsync(user, "User");

                if (!roleResult.Succeeded)
                {
                    await _userManager.DeleteAsync(user);

                    return new AuthResult(
                        Success: false,
                        Errors: roleResult.Errors.Select(e => e.Description).ToList()
                    );
                }

                return new AuthResult(Success: true);
            }

            return new AuthResult(
                Success: false,
                Errors: result.Errors.Select(e => e.Description).ToList()
            );
        }

    }
}
