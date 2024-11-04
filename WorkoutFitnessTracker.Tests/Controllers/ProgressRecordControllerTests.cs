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
    public class ProgressRecordControllerTests
    {
        private readonly Mock<IProgressRecordRepository> _progressRecordRepositoryMock;
        private readonly ProgressRecordController _progressRecordController;
        private readonly Guid _userGuid;

        public ProgressRecordControllerTests()
        {
            _progressRecordRepositoryMock = new Mock<IProgressRecordRepository>();
            _progressRecordController = new ProgressRecordController(_progressRecordRepositoryMock.Object);
            _userGuid = Guid.NewGuid();

            // Mock User identity
            var claimsPrincipal = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, _userGuid.ToString())
            }));
            _progressRecordController.ControllerContext.HttpContext = new DefaultHttpContext { User = claimsPrincipal };
        }

        [Fact]
        public async Task GetProgressRecords_ShouldReturnOk_WhenRecordsAreFound()
        {
            // Arrange
            var records = new List<ProgressRecordDto>
            {
                new ProgressRecordDto { Date = DateTime.Today, Progress = "50 kg", ExerciseName = "Bench Press" }
            };
            var queryParams = new ProgressRecordQueryParams { PageNumber = 1, PageSize = 10 };
            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordsAsync(_userGuid, queryParams))
                .ReturnsAsync(records);

            // Act
            var result = await _progressRecordController.GetProgressRecords(queryParams);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<IEnumerable<ProgressRecordDto>>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal(records, response.Data);
            Assert.Equal("Progress records retrieved successfully.", response.Message);
        }

        [Fact]
        public async Task GetProgressRecords_ShouldReturnNotFound_WhenNoRecordsAreFound()
        {
            // Arrange
            var queryParams = new ProgressRecordQueryParams();
            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordsAsync(_userGuid, queryParams))
                .ReturnsAsync((IEnumerable<ProgressRecordDto>)null);

            // Act
            var result = await _progressRecordController.GetProgressRecords(queryParams);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

            var response = notFoundResult.Value as ResponseWrapper<IEnumerable<ProgressRecordDto>>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal("No progress records found for this user.", response.Message);
        }

        [Fact]
        public async Task SaveProgressRecord_ShouldReturnOk_WhenRecordIsSavedSuccessfully()
        {
            // Arrange
            var recordDto = new ProgressRecordDto
            {
                Date = DateTime.Today,
                Progress = "50 kg",
                ExerciseName = "Bench Press"
            };
            _progressRecordRepositoryMock.Setup(repo => repo.SaveProgressRecordAsync(_userGuid, recordDto, false))
                .ReturnsAsync(true);

            // Act
            var result = await _progressRecordController.SaveProgressRecord(recordDto);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("Progress record saved successfully.", response.Message);
        }

        [Fact]
        public async Task SaveProgressRecord_ShouldReturnBadRequest_WhenModelStateIsInvalid()
        {
            // Arrange
            var invalidRecordDto = new ProgressRecordDto
            {
                Date = DateTime.Today,
                Progress = "",
                ExerciseName = "" // Invalid ExerciseName
            };
            _progressRecordController.ModelState.AddModelError("Progress", "Progress is required.");
            _progressRecordController.ModelState.AddModelError("ExerciseName", "Exercise name is required.");

            // Act
            var result = await _progressRecordController.SaveProgressRecord(invalidRecordDto);

            // Assert
            var badRequestResult = result.Result as BadRequestObjectResult;
            Assert.NotNull(badRequestResult);
            Assert.Equal(StatusCodes.Status400BadRequest, badRequestResult.StatusCode);

            var response = badRequestResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Contains("Progress is required.", response.Message);
            Assert.Contains("Exercise name is required.", response.Message);
        }

        [Fact]
        public async Task GetProgressRecordByDate_ShouldReturnOk_WhenRecordIsFound()
        {
            // Arrange
            var date = DateTime.Today;
            var exerciseName = "Bench Press";
            var record = new ProgressRecordDto { Date = date, Progress = "50 kg", ExerciseName = exerciseName };
            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordByDateAsync(_userGuid, date, exerciseName))
                .ReturnsAsync(record);

            // Act
            var result = await _progressRecordController.GetProgressRecordByDate(date, exerciseName);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<ProgressRecordDto>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("Progress record retrieved successfully.", response.Message);
            Assert.Equal(record, response.Data);
        }

        [Fact]
        public async Task GetProgressRecords_ShouldThrowUnauthorizedAccessException_WhenUserClaimIsMissing()
        {
            // Arrange
            _progressRecordController.ControllerContext.HttpContext.User = new ClaimsPrincipal(); // No user claims
            var queryParams = new ProgressRecordQueryParams();

            // Act & Assert
            var exception = await Assert.ThrowsAsync<UnauthorizedAccessException>(() => _progressRecordController.GetProgressRecords(queryParams));
            Assert.Equal("User ID is missing from the token.", exception.Message);
        }

        [Fact]
        public async Task DeleteProgressRecord_ShouldReturnOk_WhenRecordIsDeletedSuccessfully()
        {
            // Arrange
            var date = DateTime.Today;
            var exerciseName = "Bench Press";
            _progressRecordRepositoryMock.Setup(repo => repo.DeleteProgressRecordAsync(_userGuid, date, exerciseName))
                .ReturnsAsync(true);

            // Act
            var result = await _progressRecordController.DeleteProgressRecord(date, exerciseName);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal($"Progress record for {exerciseName} on {date} deleted successfully.", response.Data);
        }

        [Fact]
        public async Task GetProgressRecordByDate_ShouldReturnNotFound_WhenRecordIsNotFound()
        {
            // Arrange
            var date = DateTime.Today;
            var exerciseName = "Bench Press";
            _progressRecordRepositoryMock.Setup(repo => repo.GetProgressRecordByDateAsync(_userGuid, date, exerciseName))
                .ReturnsAsync((ProgressRecordDto)null);

            // Act
            var result = await _progressRecordController.GetProgressRecordByDate(date, exerciseName);

            // Assert
            var notFoundResult = result.Result as NotFoundObjectResult;
            Assert.NotNull(notFoundResult);
            Assert.Equal(StatusCodes.Status404NotFound, notFoundResult.StatusCode);

            var response = notFoundResult.Value as ResponseWrapper<ProgressRecordDto>;
            Assert.NotNull(response);
            Assert.False(response.Success);
            Assert.Equal($"No progress record found for {exerciseName} on {date}.", response.Message);
        }

        [Fact]
        public async Task SaveProgressRecord_ShouldReturnOk_WhenRecordIsSavedSuccessfully_WithOverwrite()
        {
            // Arrange
            var recordDto = new ProgressRecordDto
            {
                Date = DateTime.Today,
                Progress = "60 kg",
                ExerciseName = "Bench Press"
            };
            _progressRecordRepositoryMock.Setup(repo => repo.SaveProgressRecordAsync(_userGuid, recordDto, true))
                .ReturnsAsync(true);

            // Act
            var result = await _progressRecordController.SaveProgressRecord(recordDto, overwrite: true);

            // Assert
            var okResult = result.Result as OkObjectResult;
            Assert.NotNull(okResult);
            Assert.Equal(StatusCodes.Status200OK, okResult.StatusCode);

            var response = okResult.Value as ResponseWrapper<string>;
            Assert.NotNull(response);
            Assert.True(response.Success);
            Assert.Equal("Progress record saved successfully.", response.Message);
        }

        [Fact]
        public async Task GetProgressRecords_ShouldReturnBadRequest_WhenQueryParamsAreInvalid()
        {
            // Arrange
            var invalidQueryParams = new ProgressRecordQueryParams
            {
                PageNumber = -1, // Invalid page number
                PageSize = 200   // Invalid page size
            };
            _progressRecordController.ModelState.AddModelError("PageNumber", "Page number must be greater than 0.");
            _progressRecordController.ModelState.AddModelError("PageSize", "Page size must be between 1 and 100.");

            // Act
            var result = await _progressRecordController.GetProgressRecords(invalidQueryParams);

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

    }
}
