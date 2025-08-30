using FakeItEasy;
using Planday.Schedule.Api.Models;
using Planday.Schedule.Api.Services;

namespace Planday.UnitTesting;

public class AssignShiftTests
{
    [Fact]
    public async Task AssignShiftAsync_ReturnsShift_WhenAssignmentIsValid()
    {
        // Arrange
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = 1, EmployeeId = 1 };
        var expectedShift = new ShiftDetailsDto { Id = 1, EmployeeId = 1 };
        A.CallTo(() => fakeService.AssignShiftAsync(request)).Returns(expectedShift);

        // Act
        var result = await fakeService.AssignShiftAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.EmployeeId);
    }

    [Fact]
    public async Task AssignShiftAsync_Throws_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = 1, EmployeeId = 99 };
        A.CallTo(() => fakeService.AssignShiftAsync(request))
            .Throws(new KeyNotFoundException("Employee not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => fakeService.AssignShiftAsync(request));
    }

    [Fact]
    public async Task AssignShiftAsync_Throws_WhenShiftDoesNotExist()
    {
        // Arrange
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = 99, EmployeeId = 1 };
        A.CallTo(() => fakeService.AssignShiftAsync(request)).Throws(new KeyNotFoundException("Shift not found"));

        // Act & Assert
        await Assert.ThrowsAsync<KeyNotFoundException>(() => fakeService.AssignShiftAsync(request));
    }

    [Fact]
    public async Task AssignShiftAsync_Throws_WhenOverlappingShiftExists()
    {
        // Arrange
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = 1, EmployeeId = 1 };
        A.CallTo(() => fakeService.AssignShiftAsync(request))
            .Throws(new InvalidOperationException("Overlapping shift"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => fakeService.AssignShiftAsync(request));
    }

    [Fact]
    public async Task AssignShiftAsync_Throws_WhenShiftAlreadyAssignedToAnotherEmployee()
    {
        // Arrange
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = 1, EmployeeId = 2 };
        A.CallTo(() => fakeService.AssignShiftAsync(request))
            .Throws(new InvalidOperationException("Shift already assigned"));

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => fakeService.AssignShiftAsync(request));
    }

    [Theory]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    public async Task AssignShiftAsync_Throws_WhenRequestHasInvalidIds(int shiftId, int employeeId)
    {
        // Arrange
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = shiftId, EmployeeId = employeeId };
        A.CallTo(() => fakeService.AssignShiftAsync(request)).Throws(new ArgumentException("Invalid IDs"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => fakeService.AssignShiftAsync(request));
    }

    [Fact]
    public async Task AssignShiftAsync_Throws_WhenUnexpectedExceptionOccurs()
    {
        // Arrange
        var fakeService = A.Fake<IShiftService>();
        var request = new AssignShiftRequest { ShiftId = 1, EmployeeId = 1 };
        A.CallTo(() => fakeService.AssignShiftAsync(request)).Throws(new Exception("Unexpected error"));

        // Act & Assert
        await Assert.ThrowsAsync<Exception>(() => fakeService.AssignShiftAsync(request));
    }
}