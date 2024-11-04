using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.User;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;

namespace WorkoutFitnessTrackerAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UserController : ControllerBase
    {
        private readonly IUserRepository _userRepository;

        public UserController(IUserRepository userRepository)
        {
            _userRepository = userRepository;
        }

        // POST: api/User/register
        [HttpPost("register")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<ActionResult<ResponseWrapper<string>>> RegisterUser([FromBody] UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ResponseWrapper<string>(false, null, string.Join("; ", errors)));
            }

            var result = await _userRepository.RegisterUserAsync(registrationDto);

            if (!result.Success)
            {
                return BadRequest(new ResponseWrapper<string>(false, string.Join(", ", result.Errors), "Registration failed"));
            }

            return Ok(new ResponseWrapper<string>(true, "User registered successfully.", "User registered successfully!"));
        }

        // POST: api/User/login
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<ActionResult<ResponseWrapper<string>>> LoginUser([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                return BadRequest(new ResponseWrapper<string>(false, null, string.Join("; ", errors)));
            }

            var result = await _userRepository.LoginUserAsync(loginDto);

            if (!result.Success)
            {
                return Unauthorized(new ResponseWrapper<string>(false, null, "Invalid credentials"));
            }

            return Ok(new ResponseWrapper<string>(true, result.Token, "Login successful"));
        }

        // GET: api/User/profile
        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<ActionResult<ResponseWrapper<UserProfileDto>>> GetUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userId))
                return Unauthorized(new ResponseWrapper<UserProfileDto>(false, null, "User ID is missing from the token."));

            var userProfile = await _userRepository.GetUserByIdAsync(Guid.Parse(userId));
            if (userProfile == null)
            {
                return NotFound(new ResponseWrapper<UserProfileDto>(false, null, "User not found"));
            }

            return Ok(new ResponseWrapper<UserProfileDto>(true, userProfile, "User profile retrieved successfully"));
        }

        // GET: api/User/by-email/{email}
        [Authorize(Roles = "Admin")]
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<ResponseWrapper<UserProfileDto>>> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new ResponseWrapper<UserProfileDto>(false, null, "User not found"));
            }

            return Ok(new ResponseWrapper<UserProfileDto>(true, user, "User retrieved successfully"));
        }
    }
}
