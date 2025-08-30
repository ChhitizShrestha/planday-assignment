using Dapper;
using Microsoft.Data.Sqlite;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using Planday.Schedule.Queries;

namespace Planday.Schedule.Infrastructure.Queries;

public class CreateShiftCommand : ICreateShiftCommand
{
    private const string Sql = @"
            INSERT INTO Shift (Start, End)
            VALUES (@Start, @End);
            SELECT last_insert_rowid();";

    private readonly IConnectionStringProvider _connectionProvider;

    public CreateShiftCommand(IConnectionStringProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<Shift> ExecuteAsync(DateTime startTime, DateTime endTime)
    {
        await using var connection = new SqliteConnection(_connectionProvider.GetConnectionString());

        var parameters = new
        {
            Start = startTime.ToString("yyyy-MM-dd HH:mm:ss.fff"),
            End = endTime.ToString("yyyy-MM-dd HH:mm:ss.fff")
        };

        var id = await connection.ExecuteScalarAsync<long>(Sql, parameters);
        return new Shift(id, null, startTime, endTime);
    }
}