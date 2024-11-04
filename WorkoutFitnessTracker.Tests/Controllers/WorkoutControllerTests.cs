using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Controllers;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using Xunit;

namespace WorkoutFitnessTrackerAPI.Tests.Controllers
{
    public class WorkoutControllerTests
    {
        private readonly Mock<IWorkoutRepository> _workoutRepositoryMock;
        private readonly WorkoutController _workoutController;
        private readonly Guid _userGuid;

        public WorkoutControllerTests()
        {
            _workoutRepositoryMock = new Mock<IWorkoutRepository>();
            _workoutController = new WorkoutController(_workoutRepositoryMock.Object);
            _userGuid = Guid.NewGuid();

            // Mock User identity
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userGuid.ToString())
            }));
            _workoutController.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };
        }

        [Fact]
        public async Task GetWorkouts_ShouldReturnOk_WhenWorkoutsAreFound()
        {
            // Arrange
            var workouts = new List<WorkoutDto>
            {
                new WorkoutDto
                {
                    Date = DateTime.Now,
                    Duration = 60,
                    Exercises = new List<WorkoutExerciseDto>
                    {
                        new WorkoutExerciseDto { ExerciseName = "Push-up", Sets = 3, Reps = 10 }
                    }
                }
            };
            var queryParams = new WorkoutQueryParams { MinDuration = 30, MaxDuration = 120 };
            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(_userGuid, queryParams))
                .ReturnsAsync(workouts);

            // Act
            var result = await _workoutController.GetWorkouts(queryParams);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<IEnumerable<WorkoutDto>>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(workouts, response.Data);
            Assert.Equal("Workouts retrieved successfully.", response.Message);
        }

        [Fact]
        public async Task GetWorkouts_ShouldReturnNotFound_WhenNoWorkoutsAreFound()
        {
            // Arrange
            var queryParams = new WorkoutQueryParams();
            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsAsync(_userGuid, queryParams))
                .ReturnsAsync((IEnumerable<WorkoutDto>)null);

            // Act
            var result = await _workoutController.GetWorkouts(queryParams);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

            var response = notFoundResult.Value as ResponseWrapper<IEnumerable<WorkoutDto>>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("No workouts found for this user.", response.Message);
        }

        [Fact]
        public async Task SaveWorkout_ShouldReturnOk_WhenWorkoutIsSavedSuccessfully()
        {
            // Arrange
            var workoutDto = new WorkoutDto
            {
                Date = DateTime.Now,
                Duration = 45,
                Exercises = new List<WorkoutExerciseDto>
                {
                    new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 12 }
                }
            };
            _workoutRepositoryMock.Setup(repo => repo.SaveWorkoutAsync(_userGuid, workoutDto, false))
                .ReturnsAsync(true);

            // Act
            var result = await _workoutController.SaveWorkout(workoutDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("Workout saved successfully.", response.Message);
        }

        [Fact]
        public async Task SaveWorkout_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var invalidWorkoutDto = new WorkoutDto
            {
                Date = DateTime.Now,
                Duration = 0, // Invalid duration
                Exercises = new List<WorkoutExerciseDto>
                {
                    new WorkoutExerciseDto
                    {
                        ExerciseName = "", // Invalid name
                        Sets = 0,          // Invalid sets
                        Reps = 200         // Invalid reps (exceeds maximum allowed)
                    }
                }
            };
            _workoutController.ModelState.AddModelError("Duration", "Duration must be between 1 and 300 minutes.");
            _workoutController.ModelState.AddModelError("Exercises[0].ExerciseName", "Exercise name is required.");
            _workoutController.ModelState.AddModelError("Exercises[0].Sets", "Sets must be between 1 and 100.");
            _workoutController.ModelState.AddModelError("Exercises[0].Reps", "Reps must be between 1 and 100.");

            // Act
            var result = await _workoutController.SaveWorkout(invalidWorkoutDto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            var response = badRequestResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Contains("Duration must be between 1 and 300 minutes.", response.Message);
            Assert.Contains("Exercise name is required.", response.Message);
            Assert.Contains("Sets must be between 1 and 100.", response.Message);
            Assert.Contains("Reps must be between 1 and 100.", response.Message);
        }

        [Fact]
        public async Task GetWorkoutByDate_ShouldReturnOk_WhenWorkoutIsFound()
        {
            // Arrange
            var date = DateTime.Today;
            var workouts = new List<WorkoutDto>
            {
                new WorkoutDto
                {
                    Date = date,
                    Duration = 45,
                    Exercises = new List<WorkoutExerciseDto>
                    {
                        new WorkoutExerciseDto { ExerciseName = "Lunges", Sets = 3, Reps = 15 }
                    }
                }
            };
            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateAsync(_userGuid, date))
                .ReturnsAsync(workouts);

            // Act
            var result = await _workoutController.GetWorkoutByDate(date);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<IEnumerable<WorkoutDto>>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("Workout retrieved successfully.", response.Message);
            Assert.Equal(workouts, response.Data);
        }

        [Fact]
        public async Task GetWorkoutByDate_ShouldReturnNotFound_WhenNoWorkoutIsFound()
        {
            // Arrange
            var date = DateTime.Today;
            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateAsync(_userGuid, date))
                .ReturnsAsync((IEnumerable<WorkoutDto>)null);

            // Act
            var result = await _workoutController.GetWorkoutByDate(date);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

            var response = notFoundResult.Value as ResponseWrapper<IEnumerable<WorkoutDto>>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal($"No workout found for the date {date.ToShortDateString()}.", response.Message);
        }

        [Fact]
        public async Task DeleteWorkout_ShouldReturnOk_WhenWorkoutIsDeletedSuccessfully()
        {
            // Arrange
            var date = DateTime.Today;
            _workoutRepositoryMock.Setup(repo => repo.DeleteWorkoutAsync(_userGuid, date))
                .ReturnsAsync(true);

            // Act
            var result = await _workoutController.DeleteWorkout(date);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal($"Workout for the date {date.ToShortDateString()} deleted successfully.", response.Data);
        }

        [Fact]
        public async Task DeleteWorkout_ShouldReturnNotFound_WhenWorkoutIsNotFound()
        {
            // Arrange
            var date = DateTime.Today;
            _workoutRepositoryMock.Setup(repo => repo.DeleteWorkoutAsync(_userGuid, date))
                .ReturnsAsync(false);

            // Act
            var result = await _workoutController.DeleteWorkout(date);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

            var response = notFoundResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal($"No workout found for the date {date.ToShortDateString()} to delete.", response.Message);
        }

        [Fact]
        public async Task GetWorkouts_ShouldThrowUnauthorizedAccessException_WhenUserClaimIsMissing()
        {
            // Arrange
            _workoutController.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // No user claims

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _workoutController.GetWorkouts(new WorkoutQueryParams()));
            Assert.Equal("User ID is missing from the token.", exception.Message);
        }

        [Fact]
        public async Task SaveWorkout_ShouldReturnOk_WhenWorkoutIsSavedSuccessfully_WithOverwrite()
        {
            // Arrange
            var workoutDto = new WorkoutDto
            {
                Date = DateTime.Now,
                Duration = 45,
                Exercises = new List<WorkoutExerciseDto>
                {
                    new WorkoutExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 12 }
                }
            };
            _workoutRepositoryMock.Setup(repo => repo.SaveWorkoutAsync(_userGuid, workoutDto, true))
                .ReturnsAsync(true);

            // Act
            var result = await _workoutController.SaveWorkout(workoutDto, overwrite: true);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("Workout saved successfully.", response.Message);
        }

        [Fact]
        public async Task GetWorkouts_ShouldReturnBadRequest_WhenQueryParamsAreInvalid()
        {
            // Arrange
            var queryParams = new WorkoutQueryParams { PageNumber = -1, PageSize = 500 }; // Invalid params
            _workoutController.ModelState.AddModelError("PageNumber", "Page number must be greater than or equal to 1.");
            _workoutController.ModelState.AddModelError("PageSize", "Page size must be between 1 and 100.");

            // Act
            var result = await _workoutController.GetWorkouts(queryParams);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            var response = badRequestResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Contains("Page number must be greater than or equal to 1.", response.Message);
            Assert.Contains("Page size must be between 1 and 100.", response.Message);
        }

        [Fact]
        public async Task GetWorkoutByDate_ShouldReturnInternalServerError_OnRepositoryException()
        {
            // Arrange
            var date = DateTime.Today;
            _workoutRepositoryMock.Setup(repo => repo.GetWorkoutsByDateAsync(_userGuid, date))
                .ThrowsAsync(new Exception("Database error"));

            // Act
            Func<Task> action = async () => await _workoutController.GetWorkoutByDate(date);

            // Assert
            var exception = await Assert.ThrowsAsync<Exception>(action);
            Assert.Equal("Database error", exception.Message);
        }

    }
}
