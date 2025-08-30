namespace Planday.Schedule.Api.Models;

public class ShiftDetailsDto
{
    public long Id { get; set; }
    public long? EmployeeId { get; set; }
    public EmployeeDetailDto? Employee { get; set; }
    public DateTime StartTime { get; set; }
    public DateTime EndTime { get; set; }
}

public class EmployeeDetailDto
{
    public long Id { get; set; }
    public string Name { get; set; }
    public string Email { get; set; }
}