using Planday.Schedule.Api.Models;
using Planday.Schedule.Queries;

namespace Planday.Schedule.Api.Services;

public class ShiftService : IShiftService
{
    private readonly IAssignShiftCommand _assignShiftCommand;
    private readonly ICreateShiftCommand _createShiftCommand;
    private readonly IEmployeeApiClient _employeeApiClient;
    private readonly IGetAllShiftsWithEmployeeQuery _getAllShiftsWithEmployeeQuery;
    private readonly IGetShiftByIdQuery _getShiftByIdQuery;
    private readonly IGetShiftByIdWithEmployeeQuery _getShiftByIdWithEmployeeQuery;
    private readonly IHasOverlappingShiftsQuery _hasOverlappingShiftsQuery;

    public ShiftService(
        IGetShiftByIdQuery getShiftByIdQuery,
        IGetAllShiftsWithEmployeeQuery getAllShiftsWithEmployeeQuery,
        ICreateShiftCommand createShiftCommand,
        IAssignShiftCommand assignShiftCommand,
        IEmployeeApiClient employeeApiClient,
        IHasOverlappingShiftsQuery hasOverlappingShiftsQuery,
        IGetShiftByIdWithEmployeeQuery getShiftByIdWithEmployeeQuery)
    {
        _getShiftByIdQuery = getShiftByIdQuery;
        _getAllShiftsWithEmployeeQuery = getAllShiftsWithEmployeeQuery;
        _createShiftCommand = createShiftCommand;
        _assignShiftCommand = assignShiftCommand;
        _employeeApiClient = employeeApiClient;
        _hasOverlappingShiftsQuery = hasOverlappingShiftsQuery;
        _getShiftByIdWithEmployeeQuery = getShiftByIdWithEmployeeQuery;
    }

    public async Task<ShiftDetailsDto> GetShiftByIdAsync(int id)
    {
        var shift = await _getShiftByIdWithEmployeeQuery.QueryAsync(id);
        if (shift is null) return null;

        var shiftDto = new ShiftDetailsDto
        {
            Id = (int)shift.Id,
            EmployeeId = (int?)shift.EmployeeId,
            StartTime = shift.Start,
            EndTime = shift.End
        };

        if (shift.EmployeeId.HasValue)
        {
            var employee = await _employeeApiClient.GetEmployeeAsync(shift.EmployeeId.Value);
            if (employee is not null)
                shiftDto.Employee = new EmployeeDetailDto
                {
                    Id = shift.EmployeeId.Value,
                    Email = employee.Email,
                    Name = shift.EmployeeName
                };
        }

        return shiftDto;
    }

    public async Task<IEnumerable<ShiftDetailsDto>> GetAllShiftsAsync()
    {
        var shifts = await _getAllShiftsWithEmployeeQuery.QueryAsync();
        var dtos = new List<ShiftDetailsDto>();

        foreach (var shift in shifts)
        {
            var dto = new ShiftDetailsDto
            {
                Id = shift.Id,
                EmployeeId = shift.EmployeeId,
                StartTime = shift.Start,
                EndTime = shift.End
            };

            if (shift.EmployeeId.HasValue)
            {
                var employee = await _employeeApiClient.GetEmployeeAsync(shift.EmployeeId.Value);
                if (employee is not null)
                    dto.Employee = new EmployeeDetailDto
                    {
                        Id = shift.EmployeeId.Value,
                        Email = employee.Email,
                        Name = shift.EmployeeName
                    };
            }

            dtos.Add(dto);
        }

        return dtos;
    }

    public async Task<ShiftDetailsDto> CreateOpenShiftAsync(CreateOpenShiftRequest request)
    {
        if (request.StartTime >= request.EndTime)
            throw new ArgumentException("End time must be after start time");

        if (request.StartTime.Date != request.EndTime.Date)
            throw new ArgumentException("Start time and end time must be on the same day");

        var shift = await _createShiftCommand.ExecuteAsync(request.StartTime, request.EndTime);

        return new ShiftDetailsDto
        {
            Id = (int)shift.Id,
            StartTime = shift.Start,
            EndTime = shift.End
        };
    }

    public async Task<ShiftDetailsDto> AssignShiftAsync(AssignShiftRequest request)
    {
        var existingShift = await _getShiftByIdQuery.QueryAsync(request.ShiftId);
        if (existingShift is null)
            throw new ArgumentException($"Shift with ID {request.ShiftId} not found");

        // Verify employee exist
        var employeeFromApi = await _employeeApiClient.GetEmployeeAsync(request.EmployeeId);
        if (employeeFromApi is null)
            throw new ArgumentException($"Employee with ID {request.EmployeeId} not found");

        // Check for overlapping shifts
        if (await _hasOverlappingShiftsQuery.QueryAsync(
                request.EmployeeId, existingShift.Start, existingShift.End, request.ShiftId))
            throw new InvalidOperationException("Employee has overlapping shifts during this time period");

        var updatedShift = await _assignShiftCommand.ExecuteAsync(request.ShiftId, request.EmployeeId);

        return new ShiftDetailsDto
        {
            Id = (int)updatedShift.Id,
            EmployeeId = (int?)updatedShift.EmployeeId,
            StartTime = updatedShift.Start,
            EndTime = updatedShift.End
        };
    }
}