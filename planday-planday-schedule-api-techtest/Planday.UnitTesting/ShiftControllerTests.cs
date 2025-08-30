using FakeItEasy;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Planday.Schedule.Api.Controllers;
using Planday.Schedule.Api.Models;
using Planday.Schedule.Api.Services;

namespace Planday.UnitTesting;

public class ShiftControllerTests
{
    private ShiftController CreateController(IShiftService service)
    {
        var logger = A.Fake<ILogger<ShiftController>>();
        var employeeApiClient = A.Fake<IEmployeeApiClient>();
        return new ShiftController(service, employeeApiClient, logger);
    }

    [Fact]
    public async Task AssignShift_ReturnsOk_WhenAssignmentIsValid()
    {
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = 1, EmployeeId = 1 };
        var expectedShift = new ShiftDetailsDto { Id = 1, EmployeeId = 1 };
        A.CallTo(() => fakeService.AssignShiftAsync(request)).Returns(expectedShift);
        var controller = CreateController(fakeService);

        var result = await controller.AssignShift(1, request);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<ShiftDetailsDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.EmployeeId);
    }

    [Fact]
    public async Task AssignShift_ReturnsBadRequest_WhenShiftIdMismatch()
    {
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = 2, EmployeeId = 1 };
        var controller = CreateController(fakeService);

        var result = await controller.AssignShift(1, request);
        var badRequest = Assert.IsType<BadRequestObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(badRequest.Value);
        Assert.False(response.Success);
        Assert.Equal("InvalidShiftId", response.ErrorCode);
    }

    [Fact]
    public async Task AssignShift_Throws_WhenShiftDoesNotExist()
    {
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = 99, EmployeeId = 1 };
        A.CallTo(() => fakeService.AssignShiftAsync(request)).Throws(new KeyNotFoundException("Shift not found"));
        var controller = CreateController(fakeService);
        await Assert.ThrowsAsync<KeyNotFoundException>(() => controller.AssignShift(99, request));
    }

    [Fact]
    public async Task AssignShift_Throws_WhenOverlappingShift()
    {
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = 1, EmployeeId = 1 };
        A.CallTo(() => fakeService.AssignShiftAsync(request))
            .Throws(new InvalidOperationException("Overlapping shift"));
        var controller = CreateController(fakeService);
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.AssignShift(1, request));
    }

    [Fact]
    public async Task GetShift_ReturnsOk_WhenShiftExists()
    {
        var fakeService = A.Fake<IShiftService>();
        var expectedShift = new ShiftDetailsDto { Id = 1 };
        A.CallTo(() => fakeService.GetShiftByIdAsync(1)).Returns(expectedShift);
        var controller = CreateController(fakeService);
        var result = await controller.GetShift(1);
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<ShiftDetailsDto>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(1, response.Data.Id);
    }

    [Fact]
    public async Task GetShift_ReturnsNotFound_WhenShiftDoesNotExist()
    {
        var fakeService = A.Fake<IShiftService>();
        A.CallTo(() => fakeService.GetShiftByIdAsync(99)).Returns(Task.FromResult<ShiftDetailsDto?>(null));
        var controller = CreateController(fakeService);
        var result = await controller.GetShift(99);
        var notFound = Assert.IsType<NotFoundObjectResult>(result);
        var response = Assert.IsType<ApiResponse<object>>(notFound.Value);
        Assert.False(response.Success);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task GetShift_Throws_WhenIdIsInvalid(int id)
    {
        var fakeService = A.Fake<IShiftService>();
        A.CallTo(() => fakeService.GetShiftByIdAsync(id)).Throws(new ArgumentException("Invalid ID"));
        var controller = CreateController(fakeService);
        await Assert.ThrowsAsync<ArgumentException>(() => controller.GetShift(id));
    }

    [Fact]
    public async Task GetAllShifts_ReturnsOk_WithList()
    {
        var fakeService = A.Fake<IShiftService>();
        var expectedList = new List<ShiftDetailsDto>
            { new() { Id = 1 }, new() { Id = 2 } };
        A.CallTo(() => fakeService.GetAllShiftsAsync()).Returns(expectedList);
        var controller = CreateController(fakeService);
        var result = await controller.GetAllShifts();
        var okResult = Assert.IsType<OkObjectResult>(result);
        var response = Assert.IsType<ApiResponse<IEnumerable<ShiftDetailsDto>>>(okResult.Value);
        Assert.True(response.Success);
        Assert.Equal(2, response.Data.Count());
    }
}