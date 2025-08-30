namespace Planday.Schedule.Queries;

public interface IHasOverlappingShiftsQuery
{
    Task<bool> QueryAsync(int employeeId, DateTime startTime, DateTime endTime, int excludeShiftId);
}