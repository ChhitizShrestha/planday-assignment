namespace Planday.Schedule.Queries;

public interface ICreateShiftCommand
{
    Task<Shift> ExecuteAsync(DateTime startTime, DateTime endTime);
}