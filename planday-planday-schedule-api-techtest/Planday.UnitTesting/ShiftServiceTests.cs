using FakeItEasy;
using Planday.Schedule.Api.Models;
using Planday.Schedule.Api.Services;

namespace Planday.UnitTesting;

public class ShiftServiceTests
{
    [Fact]
    public async Task GetShiftByIdAsync_ReturnsShift_WhenShiftExists()
    {
        // Arrange
        var fakeService = A.Fake<IShiftService>();
        var expectedShift = new ShiftDetailsDto { Id = 1 };
        A.CallTo(() => fakeService.GetShiftByIdAsync(1)).Returns(expectedShift);

        // Act
        var result = await fakeService.GetShiftByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
    }

    [Fact]
    public async Task GetShiftByIdAsync_ReturnsNull_WhenShiftDoesNotExist()
    {
        // Arrange
        var fakeService = A.Fake<IShiftService>();
        A.CallTo(() => fakeService.GetShiftByIdAsync(99)).Returns(Task.FromResult<ShiftDetailsDto?>(null));

        // Act
        var result = await fakeService.GetShiftByIdAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task GetShiftByIdAsync_Throws_WhenIdIsInvalid(int id)
    {
        var fakeService = A.Fake<IShiftService>();
        A.CallTo(() => fakeService.GetShiftByIdAsync(id)).Throws(new ArgumentException("Invalid ID"));
        await Assert.ThrowsAsync<ArgumentException>(() => fakeService.GetShiftByIdAsync(id));
    }

    [Fact]
    public async Task GetShiftByIdAsync_Throws_WhenUnexpectedExceptionOccurs()
    {
        var fakeService = A.Fake<IShiftService>();
        A.CallTo(() => fakeService.GetShiftByIdAsync(1)).Throws(new Exception("Unexpected error"));
        await Assert.ThrowsAsync<Exception>(() => fakeService.GetShiftByIdAsync(1));
    }
}