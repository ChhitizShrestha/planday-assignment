namespace Planday.Schedule.Queries;

public interface IGetShiftByIdQuery
{
    Task<Shift> QueryAsync(int id);
}