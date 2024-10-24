using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
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
        public async Task<IActionResult> RegisterUser([FromBody] UserRegistrationDto registrationDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userRepository.RegisterUserAsync(registrationDto);

            if (!result.Success)
            {
                return BadRequest(new { result.Errors });
            }

            return Ok(new { Message = "User registered successfully!" });
        }
        /*
         {
    "email": "john@example.com",
    "password": "Password123!"
}
         */
        // POST: api/User/login
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status400BadRequest)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> LoginUser([FromBody] UserLoginDto loginDto)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var result = await _userRepository.LoginUserAsync(loginDto);

            if (!result.Success)
            {
                return Unauthorized(new { result.Errors });
            }

            return Ok(new { result.Token });
        }

        // GET: api/User/profile
        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetUserProfile()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier); 

            var user = await _userRepository.GetUserByIdAsync(Guid.Parse(userId!));
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            return Ok(new { user.Name, user.Email });
        }

        // GET: api/User/by-email/{email}
        [Authorize(Roles = "Admin")]
        [HttpGet("by-email/{email}")]
        [ProducesResponseType(StatusCodes.Status200OK)] 
        [ProducesResponseType(StatusCodes.Status401Unauthorized)] 
        [ProducesResponseType(StatusCodes.Status403Forbidden)] 
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserByEmail(string email)
        {
            var user = await _userRepository.GetUserByEmailAsync(email);
            if (user == null)
            {
                return NotFound(new { Message = "User not found" });
            }

            return Ok(new { user.Name, user.Email });
        }
    }
}
