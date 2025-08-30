namespace Planday.Schedule.Queries;

public interface IGetShiftByIdWithEmployeeQuery
{
    Task<ShiftWithEmployee> QueryAsync(int id);
}