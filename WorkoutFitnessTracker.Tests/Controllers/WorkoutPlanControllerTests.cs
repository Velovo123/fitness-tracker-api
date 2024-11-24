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
    public class WorkoutPlanControllerTests
    {
        private readonly Mock<IWorkoutPlanService> _workoutPlanServiceMock;
        private readonly WorkoutPlanController _controller;

        public WorkoutPlanControllerTests()
        {
            _workoutPlanServiceMock = new Mock<IWorkoutPlanService>();
            _controller = new WorkoutPlanController(_workoutPlanServiceMock.Object);

            // Set up mock User with claims
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
        public async Task GetWorkoutPlans_ReturnsOk_WhenPlansExist()
        {
            var workoutPlans = new List<WorkoutPlanDto> { new WorkoutPlanDto { Name = "Plan1" } };
            _workoutPlanServiceMock.Setup(service => service.GetWorkoutPlansAsync(It.IsAny<Guid>(), It.IsAny<WorkoutPlanQueryParams>()))
                .ReturnsAsync(workoutPlans);

            var result = await _controller.GetWorkoutPlans(new WorkoutPlanQueryParams());

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetWorkoutPlans_ReturnsNotFound_WhenNoPlansExist()
        {
            _workoutPlanServiceMock.Setup(service => service.GetWorkoutPlansAsync(It.IsAny<Guid>(), It.IsAny<WorkoutPlanQueryParams>()))
                .ReturnsAsync(new List<WorkoutPlanDto>());

            var result = await _controller.GetWorkoutPlans(new WorkoutPlanQueryParams());

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task SaveWorkoutPlan_ReturnsCreated_WhenSuccess()
        {
            var workoutPlanDto = new WorkoutPlanDto { Name = "Plan1" };
            _workoutPlanServiceMock.Setup(service => service.CreateWorkoutPlanAsync(It.IsAny<Guid>(), workoutPlanDto, false))
                .ReturnsAsync(true);

            var result = await _controller.SaveWorkoutPlan(workoutPlanDto);

            Assert.IsType<CreatedAtActionResult>(result);
        }

        [Fact]
        public async Task SaveWorkoutPlan_ReturnsBadRequest_WhenFails()
        {
            var workoutPlanDto = new WorkoutPlanDto { Name = "Plan1" };
            _workoutPlanServiceMock.Setup(service => service.CreateWorkoutPlanAsync(It.IsAny<Guid>(), workoutPlanDto, false))
                .ReturnsAsync(false);

            var result = await _controller.SaveWorkoutPlan(workoutPlanDto);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public async Task GetWorkoutPlanByName_ReturnsOk_WhenPlanExists()
        {
            var workoutPlan = new WorkoutPlanDto { Name = "Plan1" };
            _workoutPlanServiceMock.Setup(service => service.GetWorkoutPlanByNameAsync(It.IsAny<Guid>(), "Plan1"))
                .ReturnsAsync(workoutPlan);

            var result = await _controller.GetWorkoutPlanByName("Plan1");

            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public async Task GetWorkoutPlanByName_ReturnsNotFound_WhenPlanDoesNotExist()
        {
            _workoutPlanServiceMock.Setup(service => service.GetWorkoutPlanByNameAsync(It.IsAny<Guid>(), "NonExistingPlan"))
                .ReturnsAsync((WorkoutPlanDto?)null);

            var result = await _controller.GetWorkoutPlanByName("NonExistingPlan");

            Assert.IsType<NotFoundObjectResult>(result);
        }

        [Fact]
        public async Task DeleteWorkoutPlan_ReturnsNoContent_WhenSuccess()
        {
            _workoutPlanServiceMock.Setup(service => service.DeleteWorkoutPlanAsync(It.IsAny<Guid>(), "PlanToDelete"))
                .ReturnsAsync(true);

            var result = await _controller.DeleteWorkoutPlan("PlanToDelete");

            Assert.IsType<NoContentResult>(result);
        }

        [Fact]
        public async Task DeleteWorkoutPlan_ReturnsNotFound_WhenFails()
        {
            _workoutPlanServiceMock.Setup(service => service.DeleteWorkoutPlanAsync(It.IsAny<Guid>(), "NonExistingPlan"))
                .ReturnsAsync(false);

            var result = await _controller.DeleteWorkoutPlan("NonExistingPlan");

            Assert.IsType<NotFoundObjectResult>(result);
        }

    }
}
