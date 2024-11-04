using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Security.Claims;
using WorkoutFitnessTrackerAPI.Controllers;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.User;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;

namespace WorkoutFitnessTrackerAPI.Tests.Controllers
{
    public class UserControllerTests
    {
        private readonly Mock<IUserRepository> _userRepositoryMock;
        private readonly UserController _userController;

        public UserControllerTests()
        {
            _userRepositoryMock = new Mock<IUserRepository>();
            _userController = new UserController(_userRepositoryMock.Object);
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnOk_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto("Test User", "test@example.com", "Password123!");
            var authResult = new AuthResult(Success: true);
            _userRepositoryMock.Setup(repo => repo.RegisterUserAsync(registrationDto))
                .ReturnsAsync(authResult);

            // Act
            var result = await _userController.RegisterUser(registrationDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var response = result.Value as ResponseWrapper<object>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("User registered successfully!", response.Message);
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnBadRequest_WhenRegistrationFails()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto("Test User", "test@example.com", "Password123!");
            var authResult = new AuthResult(Success: false, Errors: new List<string> { "Error" });
            _userRepositoryMock.Setup(repo => repo.RegisterUserAsync(registrationDto))
                .ReturnsAsync(authResult);

            // Act
            var result = await _userController.RegisterUser(registrationDto) as BadRequestObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status400BadRequest, result.StatusCode);
            var response = result.Value as ResponseWrapper<object>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Error", ((List<string>)response.Data)[0]);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnOk_WhenLoginIsSuccessful()
        {
            // Arrange
            var loginDto = new UserLoginDto("test@example.com", "Password123!");
            var authResult = new AuthResult(Success: true, Token: "sample-token");
            _userRepositoryMock.Setup(repo => repo.LoginUserAsync(loginDto))
                .ReturnsAsync(authResult);

            // Act
            var result = await _userController.LoginUser(loginDto) as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var response = result.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("sample-token", response.Data);
        }

        [Fact]
        public async Task LoginUser_ShouldReturnUnauthorized_WhenLoginFails()
        {
            // Arrange
            var loginDto = new UserLoginDto("test@example.com", "Password123!");
            var authResult = new AuthResult(Success: false, Errors: new List<string> { "Invalid credentials" });
            _userRepositoryMock.Setup(repo => repo.LoginUserAsync(loginDto))
                .ReturnsAsync(authResult);

            // Act
            var result = await _userController.LoginUser(loginDto) as UnauthorizedObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status401Unauthorized, result.StatusCode);
            var response = result.Value as ResponseWrapper<object>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Invalid credentials", ((List<string>)response.Data)[0]);
        }

        [Fact]
        public async Task GetUserProfile_ShouldReturnOk_WhenUserProfileIsFound()
        {
            // Arrange
            var userId = "sample-user-id";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));
            _userController.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var userProfile = new UserProfileDto { Name = "Test User", Email = "test@example.com" };
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(userProfile);

            // Act
            var result = await _userController.GetUserProfile() as OkObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status200OK, result.StatusCode);
            var response = result.Value as ResponseWrapper<UserProfileDto>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(userProfile, response.Data);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnNotFound_WhenUserIsNotFound()
        {
            // Arrange
            var email = "test@example.com";
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(email))
                .ReturnsAsync((UserProfileDto?)null);

            // Act
            var result = await _userController.GetUserByEmail(email) as NotFoundObjectResult;

            // Assert
            Assert.NotNull(result);
            Assert.Equal(StatusCodes.Status404NotFound, result.StatusCode);
            var response = result.Value as ResponseWrapper<object>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("User not found", response.Message);
        }
    }
}
