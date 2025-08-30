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
            Start = startTime.ToString("O"),
            End = endTime.ToString("O")
        };

        var id = await connection.ExecuteScalarAsync<long>(Sql, parameters);
        return new Shift(id, null, startTime, endTime);
    }
}

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

public class HasOverlappingShiftsQuery : IHasOverlappingShiftsQuery
{
    private const string Sql = @"
            SELECT COUNT(*) 
            FROM Shift 
            WHERE EmployeeId = @EmployeeId 
                AND Id != @ExcludeShiftId
                AND ((@Start >= Start AND @Start < End) 
                    OR (@End > Start AND @End <= End)
                    OR (Start >= @Start AND End <= @End))";

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
            Start = startTime.ToString("O"),
            End = endTime.ToString("O"),
            ExcludeShiftId = excludeShiftId
        };

        var count = await connection.ExecuteScalarAsync<int>(Sql, parameters);
        return count > 0;
    }
}

public class GetAllShiftsWithEmployeeQuery : IGetAllShiftsWithEmployeeQuery
{
    private const string Sql = @"
            SELECT s.Id, s.EmployeeId, s.Start, s.End, e.Name AS EmployeeName
            FROM Shift s
            LEFT JOIN Employee e ON s.EmployeeId = e.Id;";

    private readonly IConnectionStringProvider _connectionStringProvider;

    public GetAllShiftsWithEmployeeQuery(IConnectionStringProvider connectionStringProvider)
    {
        _connectionStringProvider = connectionStringProvider;
    }

    public async Task<IReadOnlyCollection<ShiftWithEmployee>> QueryAsync()
    {
        await using var sqlConnection = new SqliteConnection(_connectionStringProvider.GetConnectionString());

        var shiftDtos = await sqlConnection.QueryAsync<ShiftWithEmployeeDto>(Sql);

        var shifts = shiftDtos.Select(x =>
            new ShiftWithEmployee(
                x.Id,
                x.EmployeeId,
                x.EmployeeName,
                string.Empty,
                DateTime.Parse(x.Start),
                DateTime.Parse(x.End)));

        return shifts.ToList();
    }

    private record ShiftWithEmployeeDto(
        long Id,
        long? EmployeeId,
        string Start,
        string End,
        string EmployeeName
    );
}

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