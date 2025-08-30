namespace Planday.Schedule.Queries;

public interface IGetAllShiftsWithEmployeeQuery
{
    Task<IReadOnlyCollection<ShiftWithEmployee>> QueryAsync();
}