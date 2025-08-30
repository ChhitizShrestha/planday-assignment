using Dapper;
using Microsoft.Data.Sqlite;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using Planday.Schedule.Queries;

namespace Planday.Schedule.Infrastructure.Queries;

public class AssignShiftCommand : IAssignShiftCommand
{
    private const string Sql = @"
            UPDATE Shift 
            SET EmployeeId = @EmployeeId
            WHERE Id = @ShiftId AND (EmployeeId IS NULL OR EmployeeId = @EmployeeId);
            
            SELECT Id, EmployeeId, Start, End 
            FROM Shift 
            WHERE Id = @ShiftId;";

    private readonly IConnectionStringProvider _connectionProvider;

    public AssignShiftCommand(IConnectionStringProvider connectionProvider)
    {
        _connectionProvider = connectionProvider;
    }

    public async Task<Shift> ExecuteAsync(int shiftId, int employeeId)
    {
        await using var connection = new SqliteConnection(_connectionProvider.GetConnectionString());

        var shiftDto = await connection.QuerySingleOrDefaultAsync<ShiftDto>(Sql,
            new { ShiftId = shiftId, EmployeeId = employeeId });

        if (shiftDto is null)
            throw new InvalidOperationException("Shift not found or already assigned to another employee");

        return new Shift(shiftDto.Id, shiftDto.EmployeeId, DateTime.Parse(shiftDto.Start),
            DateTime.Parse(shiftDto.End));
    }

    private record ShiftDto(long Id, long? EmployeeId, string Start, string End);
}