using Dapper;
using Microsoft.Data.Sqlite;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using Planday.Schedule.Queries;

namespace Planday.Schedule.Infrastructure.Queries;

public class GetShiftByIdWithEmployeeQuery : IGetShiftByIdWithEmployeeQuery
{
    private const string Sql = @"
            SELECT s.Id, s.EmployeeId, s.Start, s.End, e.Name AS EmployeeName
            FROM Shift s
            LEFT JOIN Employee e ON s.EmployeeId = e.Id 
             WHERE s.Id = @Id;";

    private readonly IConnectionStringProvider _connectionStringProvider;

    public GetShiftByIdWithEmployeeQuery(IConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task<ShiftWithEmployee> QueryAsync(int id)
    {
        await using var sqlConnection = new SqliteConnection(_connectionStringProvider.GetConnectionString());

        var shiftDtos = await sqlConnection.QueryAsync<ShiftWithEmployeeDto>(Sql, new { Id = id });

        var shifts = shiftDtos.Select(x =>
            new ShiftWithEmployee(
                x.Id,
                x.EmployeeId,
                x.EmployeeName,
                string.Empty,
                DateTime.Parse(x.Start),
                DateTime.Parse(x.End)));

        return shifts.FirstOrDefault();
    }

    private record ShiftWithEmployeeDto(
        long Id,
        long? EmployeeId,
        string Start,
        string End,
        string EmployeeName
    );
}