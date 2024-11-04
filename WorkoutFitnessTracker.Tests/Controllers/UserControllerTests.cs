using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Controllers;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Models.Dto_s.User;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using Xunit;

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
            var result = await _userController.RegisterUser(registrationDto);

            // Assert
            var okResult = result.Result as OkObjectResult; 
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
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
            var result = await _userController.RegisterUser(registrationDto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            var response = badRequestResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Registration failed", response.Message);
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
            var result = await _userController.LoginUser(loginDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
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
            var result = await _userController.LoginUser(loginDto);

            // Assert
            Assert.NotNull(result);
            var unauthorizedResult = result.Result as UnauthorizedObjectResult; 
            Assert.NotNull(unauthorizedResult);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);

            var response = unauthorizedResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("Invalid credentials", response.Message);
        }


        [Fact]
        public async Task GetUserProfile_ShouldReturnOk_WhenUserProfileIsFound()
        {
            // Arrange
            var userId = "d2719a63-3c3f-4a53-b2ad-7631c0f8a5e8";
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
        new Claim(ClaimTypes.NameIdentifier, userId)
            }));
            _userController.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };

            var userProfile = new UserProfileDto { Name = "Test User", Email = "test@example.com" };
            _userRepositoryMock.Setup(repo => repo.GetUserByIdAsync(It.IsAny<Guid>()))
                .ReturnsAsync(userProfile);

            // Act
            var result = await _userController.GetUserProfile();

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<UserProfileDto>;
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
            var result = await _userController.GetUserByEmail(email);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result); 
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.NotNull(notFoundResult);

            var response = notFoundResult.Value as ResponseWrapper<UserProfileDto>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("User not found", response.Message);
        }

        [Fact]
        public async Task RegisterUser_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            _userController.ModelState.AddModelError("Email", "Required");

            // Act
            var result = await _userController.RegisterUser(new UserRegistrationDto("", "", ""));

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            var response = badRequestResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Contains("Required", response.Message);
        }

        [Fact]
        public async Task GetUserProfile_ShouldReturnUnauthorized_WhenUserIdIsMissing()
        {
            // Arrange
            _userController.ControllerContext.HttpContext = new DefaultHttpContext(); // No user claims set

            // Act
            var result = await _userController.GetUserProfile();

            // Assert
            var unauthorizedResult = result.Result as UnauthorizedObjectResult;
            Assert.NotNull(unauthorizedResult);
            Assert.Equal(StatusCodes.Status401Unauthorized, unauthorizedResult.StatusCode);

            var response = unauthorizedResult.Value as ResponseWrapper<UserProfileDto>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("User ID is missing from the token.", response.Message);
        }

        [Fact]
        public async Task GetUserByEmail_ShouldReturnOk_WhenUserIsFound()
        {
            // Arrange
            var email = "test@example.com";
            var userProfile = new UserProfileDto { Name = "Test User", Email = email };
            _userRepositoryMock.Setup(repo => repo.GetUserByEmailAsync(email))
                .ReturnsAsync(userProfile);

            // Act
            var result = await _userController.GetUserByEmail(email);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<UserProfileDto>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(userProfile, response.Data);
        }


    }
}
