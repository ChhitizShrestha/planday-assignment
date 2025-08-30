namespace Planday.Schedule.Api.Services;

public interface IEmployeeApiClient
{
    Task<EmployeeDto> GetEmployeeAsync(long employeeId);
}

public class EmployeeDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}