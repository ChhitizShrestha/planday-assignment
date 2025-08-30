using Dapper;
using Microsoft.Data.Sqlite;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using Planday.Schedule.Queries;

namespace Planday.Schedule.Infrastructure.Queries;

public class GetShiftByIdQuery : IGetShiftByIdQuery
{
    private const string Sql = @"SELECT Id, EmployeeId, Start, End FROM Shift WHERE Id = @Id";
    private readonly IConnectionStringProvider _connectionProvider;

    public GetShiftByIdQuery(IConnectionStringProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<Shift> QueryAsync(int id)
    {
        await using var connection = new SqliteConnection(_connectionProvider.GetConnectionString());

        var shiftDto = await connection.QueryFirstOrDefaultAsync<ShiftDto>(Sql, new { Id = id });

        if (shiftDto is null)
            return null;

        return new Shift(shiftDto.Id, shiftDto.EmployeeId, DateTime.Parse(shiftDto.Start),
            DateTime.Parse(shiftDto.End));
    }

    private record ShiftDto(long Id, long? EmployeeId, string Start, string End);
}