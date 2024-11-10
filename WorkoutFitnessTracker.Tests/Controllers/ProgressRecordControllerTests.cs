using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Threading.Tasks;
using WorkoutFitnessTracker.API.Services.IServices;
using WorkoutFitnessTrackerAPI.Controllers;
using WorkoutFitnessTrackerAPI.Helpers;
using WorkoutFitnessTrackerAPI.Models.Dto_s;
using Xunit;

namespace WorkoutFitnessTrackerAPI.Tests.Controllers
{
    public class ProgressRecordControllerTests
    {
        private readonly Mock<IProgressRecordService> _progressRecordServiceMock;
        private readonly ProgressRecordController _controller;

        public ProgressRecordControllerTests()
        {
            _progressRecordServiceMock = new Mock<IProgressRecordService>();
            _controller = new ProgressRecordController(_progressRecordServiceMock.Object);

            var userId = Guid.NewGuid().ToString();
            var user = new ClaimsPrincipal(new ClaimsIdentity(new Claim[]
            {
                new Claim(ClaimTypes.NameIdentifier, userId)
            }));
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = new DefaultHttpContext { User = user }
            };
        }

        [Fact]
        public async Task GetProgressRecords_ReturnsOk_WhenRecordsExist()
        {
            // Arrange
            var queryParams = new ProgressRecordQueryParams();
            var records = new List<ProgressRecordDto>
            {
                new ProgressRecordDto { Date = DateTime.Now, Progress = "Progress", ExerciseName = "Squat" }
            };
            _progressRecordServiceMock.Setup(s => s.GetProgressRecordsAsync(It.IsAny<Guid>(), queryParams))
                                      .ReturnsAsync(records);

            // Act
            var result = await _controller.GetProgressRecords(queryParams);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetProgressRecords_ReturnsNotFound_WhenNoRecordsExist()
        {
            // Arrange
            var queryParams = new ProgressRecordQueryParams();
            _progressRecordServiceMock.Setup(s => s.GetProgressRecordsAsync(It.IsAny<Guid>(), queryParams))
                                      .ReturnsAsync(new List<ProgressRecordDto>());

            // Act
            var result = await _controller.GetProgressRecords(queryParams);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task SaveProgressRecord_ReturnsCreated_WhenRecordIsSaved()
        {
            // Arrange
            var progressRecordDto = new ProgressRecordDto { Date = DateTime.Now, Progress = "Progress", ExerciseName = "Squat" };
            _progressRecordServiceMock.Setup(s => s.CreateProgressRecordAsync(It.IsAny<Guid>(), progressRecordDto, It.IsAny<bool>()))
                                      .ReturnsAsync(true);

            // Act
            var result = await _controller.SaveProgressRecord(progressRecordDto);

            // Assert
            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task SaveProgressRecord_ReturnsBadRequest_WhenFailsToSave()
        {
            // Arrange
            var progressRecordDto = new ProgressRecordDto { Date = DateTime.Now, Progress = "Progress", ExerciseName = "Squat" };
            _progressRecordServiceMock.Setup(s => s.CreateProgressRecordAsync(It.IsAny<Guid>(), progressRecordDto, It.IsAny<bool>()))
                                      .ReturnsAsync(false);

            // Act
            var result = await _controller.SaveProgressRecord(progressRecordDto);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetProgressRecordByDate_ReturnsOk_WhenRecordExists()
        {
            // Arrange
            var date = DateTime.Now;
            var exerciseName = "Squat";
            var record = new ProgressRecordDto { Date = date, Progress = "Progress", ExerciseName = exerciseName };
            _progressRecordServiceMock.Setup(s => s.GetProgressRecordByDateAsync(It.IsAny<Guid>(), date, exerciseName))
                                      .ReturnsAsync(record);

            // Act
            var result = await _controller.GetProgressRecordByDate(date, exerciseName);

            // Assert
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetProgressRecordByDate_ReturnsNotFound_WhenRecordDoesNotExist()
        {
            // Arrange
            var date = DateTime.Now;
            var exerciseName = "Squat";
            _progressRecordServiceMock.Setup(s => s.GetProgressRecordByDateAsync(It.IsAny<Guid>(), date, exerciseName))
                                      .ReturnsAsync((ProgressRecordDto?)null);

            // Act
            var result = await _controller.GetProgressRecordByDate(date, exerciseName);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteProgressRecord_ReturnsNoContent_WhenRecordIsDeleted()
        {
            // Arrange
            var date = DateTime.Now;
            var exerciseName = "Squat";
            _progressRecordServiceMock.Setup(s => s.DeleteProgressRecordAsync(It.IsAny<Guid>(), date, exerciseName))
                                      .ReturnsAsync(true);

            // Act
            var result = await _controller.DeleteProgressRecord(date, exerciseName);

            // Assert
            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteProgressRecord_ReturnsNotFound_WhenRecordDoesNotExist()
        {
            // Arrange
            var date = DateTime.Now;
            var exerciseName = "Squat";
            _progressRecordServiceMock.Setup(s => s.DeleteProgressRecordAsync(It.IsAny<Guid>(), date, exerciseName))
                                      .ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteProgressRecord(date, exerciseName);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
        }
    }
}
