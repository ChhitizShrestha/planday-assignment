namespace Planday.Schedule.Queries;

public interface IAssignShiftCommand
{
    Task<Shift> ExecuteAsync(int shiftId, int employeeId);
}