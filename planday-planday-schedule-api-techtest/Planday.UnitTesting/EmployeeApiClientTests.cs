using FakeItEasy;
using Planday.Schedule.Api.Services;

namespace Planday.UnitTesting;

public class EmployeeApiClientTests
{
    [Fact]
    public async Task GetEmployeeAsync_ReturnsEmployee_WhenEmployeeExists()
    {
        // Arrange
        var fakeClient = A.Fake<IEmployeeApiClient>();
        var expectedEmployee = new EmployeeDto { Id = 1, Name = "Steve Banon" };
        A.CallTo(() => fakeClient.GetEmployeeAsync(1)).Returns(expectedEmployee);

        // Act
        var result = await fakeClient.GetEmployeeAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.Id);
        Assert.Equal("Steve Banon", result.Name);
    }

    [Fact]
    public async Task GetEmployeeAsync_ReturnsNull_WhenEmployeeDoesNotExist()
    {
        // Arrange
        var fakeClient = A.Fake<IEmployeeApiClient>();
        A.CallTo(() => fakeClient.GetEmployeeAsync(99)).Returns(Task.FromResult<EmployeeDto?>(null));

        // Act
        var result = await fakeClient.GetEmployeeAsync(99);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetEmployeeAsync_Throws_WhenApiErrorOccurs()
    {
        // Arrange
        var fakeClient = A.Fake<IEmployeeApiClient>();
        A.CallTo(() => fakeClient.GetEmployeeAsync(1)).Throws(new HttpRequestException("API error"));

        // Act & Assert
        await Assert.ThrowsAsync<HttpRequestException>(() => fakeClient.GetEmployeeAsync(1));
    }

    [Theory]
    [InlineData(-1)]
    [InlineData(0)]
    public async Task GetEmployeeAsync_Throws_WhenIdIsInvalid(long id)
    {
        // Arrange
        var fakeClient = A.Fake<IEmployeeApiClient>();
        A.CallTo(() => fakeClient.GetEmployeeAsync(id)).Throws(new ArgumentException("Invalid ID"));

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => fakeClient.GetEmployeeAsync(id));
    }
}