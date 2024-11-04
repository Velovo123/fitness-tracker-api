using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkoutFitnessTrackerAPI.Controllers;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using WorkoutFitnessTrackerAPI.Repositories.IRepositories;
using Xunit;

namespace WorkoutFitnessTrackerAPI.Tests.Controllers
{
    public class WorkoutPlanControllerTests
    {
        private readonly Mock<IWorkoutPlanRepository> _workoutPlanRepositoryMock;
        private readonly WorkoutPlanController _workoutPlanController;
        private readonly Guid _userGuid;

        public WorkoutPlanControllerTests()
        {
            _workoutPlanRepositoryMock = new Mock<IWorkoutPlanRepository>();
            _workoutPlanController = new WorkoutPlanController(_workoutPlanRepositoryMock.Object);
            _userGuid = Guid.NewGuid();

            // Mock User identity
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userGuid.ToString())
            }));
            _workoutPlanController.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };
        }

        [Fact]
        public async Task GetWorkoutPlans_ShouldReturnOk_WhenWorkoutPlansAreFound()
        {
            // Arrange
            var workoutPlans = new List<WorkoutPlanDto>
            {
                new WorkoutPlanDto
                {
                    Name = "Plan A",
                    Goal = "Build Strength",
                    Exercises = new List<WorkoutPlanExerciseDto>
                    {
                        new WorkoutPlanExerciseDto { ExerciseName = "Squat", Sets = 4, Reps = 12 }
                    }
                }
            };
            var queryParams = new WorkoutPlanQueryParams { PageNumber = 1, PageSize = 10, Goal = "Build Strength" };
            _workoutPlanRepositoryMock.Setup(repo => repo.GetWorkoutPlansAsync(_userGuid, queryParams))
                .ReturnsAsync(workoutPlans);

            // Act
            var result = await _workoutPlanController.GetWorkoutPlans(queryParams);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<IEnumerable<WorkoutPlanDto>>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("Workout plans retrieved successfully.", response.Message);
            Assert.Equal(workoutPlans, response.Data);
        }

        [Fact]
        public async Task GetWorkoutPlans_ShouldReturnNotFound_WhenNoWorkoutPlansAreFound()
        {
            // Arrange
            var queryParams = new WorkoutPlanQueryParams();
            _workoutPlanRepositoryMock.Setup(repo => repo.GetWorkoutPlansAsync(_userGuid, queryParams))
                .ReturnsAsync((IEnumerable<WorkoutPlanDto>)null);

            // Act
            var result = await _workoutPlanController.GetWorkoutPlans(queryParams);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

            var response = notFoundResult.Value as ResponseWrapper<IEnumerable<WorkoutPlanDto>>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("No workout plans found for this user.", response.Message);
        }

        [Fact]
        public async Task SaveWorkoutPlan_ShouldReturnOk_WhenWorkoutPlanIsSavedSuccessfully()
        {
            // Arrange
            var workoutPlanDto = new WorkoutPlanDto
            {
                Name = "Plan A",
                Goal = "Build Strength",
                Exercises = new List<WorkoutPlanExerciseDto>
                {
                    new WorkoutPlanExerciseDto { ExerciseName = "Deadlift", Sets = 3, Reps = 10 }
                }
            };
            _workoutPlanRepositoryMock.Setup(repo => repo.SaveWorkoutPlanAsync(_userGuid, workoutPlanDto, false))
                .ReturnsAsync(true);

            // Act
            var result = await _workoutPlanController.SaveWorkoutPlan(workoutPlanDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("Workout plan saved successfully.", response.Message);
        }

        [Fact]
        public async Task SaveWorkoutPlan_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var invalidWorkoutPlanDto = new WorkoutPlanDto
            {
                Name = "",
                Goal = "Build Strength",
                Exercises = new List<WorkoutPlanExerciseDto>
                {
                    new WorkoutPlanExerciseDto { ExerciseName = "Deadlift", Sets = 0, Reps = 200 } // Invalid sets and reps
                }
            };
            _workoutPlanController.ModelState.AddModelError("Name", "Name is required.");
            _workoutPlanController.ModelState.AddModelError("Exercises[0].Sets", "Sets must be between 1 and 100.");
            _workoutPlanController.ModelState.AddModelError("Exercises[0].Reps", "Reps must be between 1 and 100.");

            // Act
            var result = await _workoutPlanController.SaveWorkoutPlan(invalidWorkoutPlanDto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            var response = badRequestResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Contains("Name is required.", response.Message);
            Assert.Contains("Sets must be between 1 and 100.", response.Message);
            Assert.Contains("Reps must be between 1 and 100.", response.Message);
        }

        [Fact]
        public async Task GetWorkoutPlanByName_ShouldReturnOk_WhenWorkoutPlanIsFound()
        {
            // Arrange
            var name = "Plan A";
            var workoutPlanDto = new WorkoutPlanDto
            {
                Name = name,
                Goal = "Build Strength",
                Exercises = new List<WorkoutPlanExerciseDto>
                {
                    new WorkoutPlanExerciseDto { ExerciseName = "Squat", Sets = 3, Reps = 15 }
                }
            };
            _workoutPlanRepositoryMock.Setup(repo => repo.GetWorkoutPlanByNameAsync(_userGuid, name))
                .ReturnsAsync(workoutPlanDto);

            // Act
            var result = await _workoutPlanController.GetWorkoutPlanByName(name);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<WorkoutPlanDto>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("Workout plan retrieved successfully.", response.Message);
            Assert.Equal(workoutPlanDto, response.Data);
        }

        [Fact]
        public async Task DeleteWorkoutPlan_ShouldReturnOk_WhenWorkoutPlanIsDeletedSuccessfully()
        {
            // Arrange
            var name = "Plan A";
            _workoutPlanRepositoryMock.Setup(repo => repo.DeleteWorkoutPlanAsync(_userGuid, name))
                .ReturnsAsync(true);

            // Act
            var result = await _workoutPlanController.DeleteWorkoutPlan(name);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal($"Workout plan '{name}' deleted successfully.", response.Data);
        }

        [Fact]
        public async Task GetWorkoutPlans_ShouldThrowUnauthorizedAccessException_WhenUserClaimIsMissing()
        {
            // Arrange
            _workoutPlanController.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // No user claims

            // Act & Assert
            await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _workoutPlanController.GetWorkoutPlans(new WorkoutPlanQueryParams()));
        }

        [Fact]
        public async Task SaveWorkoutPlan_ShouldReturnOk_WhenWorkoutPlanIsSavedSuccessfully_WithOverwrite()
        {
            // Arrange
            var workoutPlanDto = new WorkoutPlanDto
            {
                Name = "Plan A",
                Goal = "Build Muscle",
                Exercises = new List<WorkoutPlanExerciseDto>
                {
                    new WorkoutPlanExerciseDto { ExerciseName = "Bench Press", Sets = 4, Reps = 10 }
                }
            };
            _workoutPlanRepositoryMock.Setup(repo => repo.SaveWorkoutPlanAsync(_userGuid, workoutPlanDto, true))
                .ReturnsAsync(true);

            // Act
            var result = await _workoutPlanController.SaveWorkoutPlan(workoutPlanDto, overwrite: true);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("Workout plan saved successfully.", response.Message);
        }

        [Fact]
        public async Task GetWorkoutPlans_ShouldReturnBadRequest_WhenQueryParamsAreInvalid()
        {
            // Arrange
            var queryParams = new WorkoutPlanQueryParams { PageNumber = -1, PageSize = 200 }; // Invalid params
            _workoutPlanController.ModelState.AddModelError("PageNumber", "Page number must be greater than 0.");
            _workoutPlanController.ModelState.AddModelError("PageSize", "Page size must be between 1 and 100.");

            // Act
            var result = await _workoutPlanController.GetWorkoutPlans(queryParams);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            var response = badRequestResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Contains("Page number must be greater than 0.", response.Message);
            Assert.Contains("Page size must be between 1 and 100.", response.Message);
        }

        [Fact]
        public async Task GetWorkoutPlanByName_ShouldReturnInternalServerError_OnRepositoryException()
        {
            // Arrange
            var name = "Plan A";
            _workoutPlanRepositoryMock.Setup(repo => repo.GetWorkoutPlanByNameAsync(_userGuid, name))
                .ThrowsAsync(new Exception("Database error"));

            // Act & Assert
            var exception = await Assert.ThrowsAsync<Exception>(() => _workoutPlanController.GetWorkoutPlanByName(name));
            Assert.Equal("Database error", exception.Message);
        }

    }
}
