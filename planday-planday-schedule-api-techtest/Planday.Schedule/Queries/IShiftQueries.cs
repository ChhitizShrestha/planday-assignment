namespace Planday.Schedule.Queries;

public interface IGetShiftByIdQuery
{
    Task<Shift> QueryAsync(int id);
}

public interface IGetShiftByIdWithEmployeeQuery
{
    Task<ShiftWithEmployee> QueryAsync(int id);
}

public interface IGetAllShiftsWithEmployeeQuery
{
    Task<IReadOnlyCollection<ShiftWithEmployee>> QueryAsync();
}

public interface ICreateShiftCommand
{
    Task<Shift> ExecuteAsync(DateTime startTime, DateTime endTime);
}

public interface IAssignShiftCommand
{
    Task<Shift> ExecuteAsync(int shiftId, int employeeId);
}

public interface IHasOverlappingShiftsQuery
{
    Task<bool> QueryAsync(int employeeId, DateTime startTime, DateTime endTime, int excludeShiftId);
}