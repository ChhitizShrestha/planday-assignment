using Dapper;
using Microsoft.Data.Sqlite;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using Planday.Schedule.Queries;

namespace Planday.Schedule.Infrastructure.Queries;

public class HasOverlappingShiftsQuery : IHasOverlappingShiftsQuery
{
    private const string Sql = @"
            SELECT COUNT(1) FROM Shift 
                            WHERE EmployeeId = @EmployeeId 
                              AND Id != @ExcludeShiftId
                              AND @End > Start 
                              AND @Start < End;";

    private readonly IConnectionStringProvider _connectionProvider;

    public HasOverlappingShiftsQuery(IConnectionStringProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<bool> QueryAsync(int employeeId, DateTime startTime, DateTime endTime, int excludeShiftId)
    {
        await using var connection = new SqliteConnection(_connectionProvider.GetConnectionString());

        var parameters = new
        {
            EmployeeId = employeeId,
            Start = startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            End = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            ExcludeShiftId = excludeShiftId
        };

        var count = await connection.ExecuteScalarAsync<int>(Sql, parameters);
        return count > 0;
    }
}