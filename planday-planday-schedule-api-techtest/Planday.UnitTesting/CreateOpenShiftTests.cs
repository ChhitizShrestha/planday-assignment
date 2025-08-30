using FakeItEasy;
using Planday.Schedule.Api.Models;
using Planday.Schedule.Api.Models.Validators;
using Planday.Schedule.Api.Services;

namespace Planday.UnitTesting;

public class CreateOpenShiftTests
{
    [Fact]
    public async Task CreateOpenShiftAsync_ReturnsShift_WhenRequestIsValid()
    {
        // Arrange
        var fakeService = A.Fake<IShiftService>();
        var request = new CreateOpenShiftRequest
        {
            StartTime = new DateTime(2022, 6, 29, 11, 0, 0),
            EndTime = new DateTime(2022, 6, 29, 19, 0, 0)
        };
        var expectedShift = new ShiftDetailsDto { Id = 2 };
        A.CallTo(() => fakeService.CreateOpenShiftAsync(request)).Returns(expectedShift);

        // Act
        var result = await fakeService.CreateOpenShiftAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Id);
    }

    [Fact]
    public void CreateOpenShiftRequestValidator_Fails_WhenStartTimeAfterEndTime()
    {
        // Arrange
        var validator = new CreateOpenShiftRequestValidator();
        var request = new CreateOpenShiftRequest
        {
            StartTime = new DateTime(2022, 6, 29, 20, 0, 0),
            EndTime = new DateTime(2022, 6, 29, 19, 0, 0)
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "StartTime");
    }

    [Fact]
    public void CreateOpenShiftRequestValidator_Fails_WhenStartAndEndNotSameDay()
    {
        // Arrange
        var validator = new CreateOpenShiftRequestValidator();
        var request = new CreateOpenShiftRequest
        {
            StartTime = new DateTime(2022, 6, 29, 11, 0, 0),
            EndTime = new DateTime(2022, 6, 30, 19, 0, 0)
        };

        // Act
        var result = validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.PropertyName == "EndTime");
    }

    [Fact]
    public void CreateOpenShiftRequestValidator_Fails_WhenEmployeeAssigned()
    {
        // Arrange
        var validator = new CreateOpenShiftRequestValidator();
        var request = new CreateOpenShiftRequest
        {
            StartTime = new DateTime(2022, 6, 29, 11, 0, 0),
            EndTime = new DateTime(2022, 6, 29, 19, 0, 0)
        };
        // Act
        var result = validator.Validate(request);
        // Assert
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateOpenShiftRequestValidator_Fails_WhenStartEqualsEndTime()
    {
        // Arrange
        var validator = new CreateOpenShiftRequestValidator();
        var request = new CreateOpenShiftRequest
        {
            StartTime = new DateTime(2022, 6, 29, 11, 0, 0),
            EndTime = new DateTime(2022, 6, 29, 11, 0, 0)
        };
        // Act
        var result = validator.Validate(request);
        // Assert
        Assert.False(result.IsValid);
    }


    [Theory]
    [InlineData("2022-06-29T00:00:00", "2022-06-29T23:59:59")]
    [InlineData("2022-06-29T23:59:59", "2022-06-29T23:59:59")]
    public void CreateOpenShiftRequestValidator_BoundaryTimes(string start, string end)
    {
        var validator = new CreateOpenShiftRequestValidator();
        var request = new CreateOpenShiftRequest
        {
            StartTime = DateTime.Parse(start),
            EndTime = DateTime.Parse(end)
        };
        var result = validator.Validate(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateOpenShiftRequestValidator_Fails_WhenStartTimeIsMinValue()
    {
        var validator = new CreateOpenShiftRequestValidator();
        var request = new CreateOpenShiftRequest
        {
            StartTime = DateTime.MinValue,
            EndTime = new DateTime(2022, 6, 29, 19, 0, 0)
        };
        var result = validator.Validate(request);
        Assert.False(result.IsValid);
    }

    [Fact]
    public void CreateOpenShiftRequestValidator_Fails_WhenEndTimeIsMaxValue()
    {
        var validator = new CreateOpenShiftRequestValidator();
        var request = new CreateOpenShiftRequest
        {
            StartTime = new DateTime(2022, 6, 29, 11, 0, 0),
            EndTime = DateTime.MaxValue
        };
        var result = validator.Validate(request);
        Assert.False(result.IsValid);
    }
}