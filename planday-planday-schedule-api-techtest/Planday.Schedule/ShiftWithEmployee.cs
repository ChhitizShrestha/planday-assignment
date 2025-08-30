namespace Planday.Schedule;

public record ShiftWithEmployee(
    long Id,
    long? EmployeeId,
    string? EmployeeName,
    string? EmployeeEmail,
    DateTime Start,
    DateTime End
);