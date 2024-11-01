using AutoMapper;
using Moq;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System.Collections.Generic;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Models;
using WorkoutFitnessTrackerAPI.Models.Dto_s.User;
using WorkoutFitnessTrackerAPI.Repositories;
using WorkoutFitnessTrackerAPI.Services.IServices;
using Xunit;
using WorkoutFitnessTrackerAPI.Models.Dto_s;

namespace WorkoutFitnessTracker.Tests.Repositories
{
    public class UserRepositoryTests
    {
        private readonly UserRepository _userRepository;
        private readonly Mock<UserManager<User>> _userManagerMock;
        private readonly Mock<SignInManager<User>> _signInManagerMock;
        private readonly Mock<ITokenService> _tokenServiceMock;
        private readonly Mock<IMapper> _mapperMock;

        public UserRepositoryTests()
        {
            // Mocking dependencies for UserManager
            var userStoreMock = new Mock<IUserStore<User>>();
            var optionsMock = new Mock<IOptions<IdentityOptions>>();
            var passwordHasherMock = new Mock<IPasswordHasher<User>>();
            var userValidators = new List<IUserValidator<User>>();
            var passwordValidators = new List<IPasswordValidator<User>>();
            var keyNormalizerMock = new Mock<ILookupNormalizer>();
            var errorDescriberMock = new Mock<IdentityErrorDescriber>();
            var serviceProviderMock = new Mock<IServiceProvider>();
            var loggerUserManagerMock = new Mock<ILogger<UserManager<User>>>();

            _userManagerMock = new Mock<UserManager<User>>(
                userStoreMock.Object,
                optionsMock.Object,
                passwordHasherMock.Object,
                userValidators,
                passwordValidators,
                keyNormalizerMock.Object,
                errorDescriberMock.Object,
                serviceProviderMock.Object,
                loggerUserManagerMock.Object
            );

            // Mocking dependencies for SignInManager
            var httpContextAccessorMock = new Mock<IHttpContextAccessor>();
            var userClaimsPrincipalFactoryMock = new Mock<IUserClaimsPrincipalFactory<User>>();
            var loggerSignInManagerMock = new Mock<ILogger<SignInManager<User>>>();
            var authenticationSchemeProviderMock = new Mock<IAuthenticationSchemeProvider>();
            var userConfirmationMock = new Mock<IUserConfirmation<User>>();

            _signInManagerMock = new Mock<SignInManager<User>>(
                _userManagerMock.Object,
                httpContextAccessorMock.Object,
                userClaimsPrincipalFactoryMock.Object,
                optionsMock.Object,
                loggerSignInManagerMock.Object,
                authenticationSchemeProviderMock.Object,
                userConfirmationMock.Object
            );

            // Mocking other dependencies for UserRepository
            _tokenServiceMock = new Mock<ITokenService>();
            _mapperMock = new Mock<IMapper>();

            // Creating UserRepository with mocks
            _userRepository = new UserRepository(
                _userManagerMock.Object,
                _signInManagerMock.Object,
                _tokenServiceMock.Object,
                _mapperMock.Object
            );
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnSuccess_WhenRegistrationIsSuccessful()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto("Test User", "test@example.com", "Password123");

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Success);

            // Act
            var result = await _userRepository.RegisterUserAsync(registrationDto);

            // Assert
            Assert.True(result.Success);
            Assert.Null(result.Errors);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldReturnFailure_WhenRegistrationFails()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto("Test User", "test@example.com", "Password123");

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Password is too weak" }));

            // Act
            var result = await _userRepository.RegisterUserAsync(registrationDto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Password is too weak", result.Errors);
        }

        [Fact]
        public async Task RegisterUserAsync_ShouldDeleteUser_WhenRoleAdditionFails()
        {
            // Arrange
            var registrationDto = new UserRegistrationDto("Test User", "test@example.com", "Password123");

            _userManagerMock.Setup(x => x.CreateAsync(It.IsAny<User>(), It.IsAny<string>()))
                .ReturnsAsync(IdentityResult.Success);
            _userManagerMock.Setup(x => x.AddToRoleAsync(It.IsAny<User>(), "User"))
                .ReturnsAsync(IdentityResult.Failed(new IdentityError { Description = "Role addition failed" }));

            // Act
            var result = await _userRepository.RegisterUserAsync(registrationDto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Role addition failed", result.Errors);
            _userManagerMock.Verify(x => x.DeleteAsync(It.IsAny<User>()), Times.Once);
        }

        [Fact]
        public async Task LoginUserAsync_ShouldReturnToken_WhenLoginIsSuccessful()
        {
            // Arrange
            var loginDto = new UserLoginDto("test@example.com", "Password123");
            var user = new User { Email = loginDto.Email };

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync(user);
            _signInManagerMock.Setup(x => x.CheckPasswordSignInAsync(user, loginDto.Password, false))
                .ReturnsAsync(SignInResult.Success);
            _userManagerMock.Setup(x => x.GetRolesAsync(user))
                .ReturnsAsync(new List<string> { "User" });
            _tokenServiceMock.Setup(x => x.GenerateJwtToken(user, It.IsAny<IEnumerable<string>>()))
                .Returns("mock-token");

            // Act
            var result = await _userRepository.LoginUserAsync(loginDto);

            // Assert
            Assert.True(result.Success);
            Assert.Equal("mock-token", result.Token);
        }

        [Fact]
        public async Task LoginUserAsync_ShouldReturnError_WhenLoginFails()
        {
            // Arrange
            var loginDto = new UserLoginDto("test@example.com", "WrongPassword");

            _userManagerMock.Setup(x => x.FindByEmailAsync(loginDto.Email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userRepository.LoginUserAsync(loginDto);

            // Assert
            Assert.False(result.Success);
            Assert.Contains("Invalid credentials", result.Errors);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnUserProfileDto_WhenUserExists()
        {
            // Arrange
            var user = new User { Email = "test@example.com", Name = "Test User" };
            var userProfileDto = new UserProfileDto { Email = user.Email, Name = user.Name };

            _userManagerMock.Setup(x => x.FindByEmailAsync(user.Email))
                .ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<UserProfileDto>(user))
                .Returns(userProfileDto);

            // Act
            var result = await _userRepository.GetUserByEmailAsync(user.Email);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.Name, result.Name);
        }

        [Fact]
        public async Task GetUserByEmailAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var email = "nonexistent@example.com";

            _userManagerMock.Setup(x => x.FindByEmailAsync(email))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userRepository.GetUserByEmailAsync(email);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnUserProfileDto_WhenUserExists()
        {
            // Arrange
            var user = new User { Id = Guid.NewGuid(), Email = "test@example.com", Name = "Test User" };
            var userProfileDto = new UserProfileDto { Email = user.Email, Name = user.Name };

            _userManagerMock.Setup(x => x.FindByIdAsync(user.Id.ToString()))
                .ReturnsAsync(user);
            _mapperMock.Setup(m => m.Map<UserProfileDto>(user))
                .Returns(userProfileDto);

            // Act
            var result = await _userRepository.GetUserByIdAsync(user.Id);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(user.Email, result.Email);
            Assert.Equal(user.Name, result.Name);
        }

        [Fact]
        public async Task GetUserByIdAsync_ShouldReturnNull_WhenUserDoesNotExist()
        {
            // Arrange
            var userId = Guid.NewGuid();

            _userManagerMock.Setup(x => x.FindByIdAsync(userId.ToString()))
                .ReturnsAsync((User)null);

            // Act
            var result = await _userRepository.GetUserByIdAsync(userId);

            // Assert
            Assert.Null(result);
        }
    }
}
