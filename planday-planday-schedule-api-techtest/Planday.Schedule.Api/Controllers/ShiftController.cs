using Microsoft.AspNetCore.Mvc;
using Planday.Schedule.Api.Models;
using Planday.Schedule.Api.Services;

namespace Planday.Schedule.Api.Controllers;

[ApiController]
[Route("[controller]")]
[ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status500InternalServerError)]
public class ShiftController : ControllerBase
{
    private readonly ILogger<ShiftController> _logger;
    private readonly IShiftService _shiftService;

    public ShiftController(
        IShiftService shiftService,
        IEmployeeApiClient employeeApiClient,
        ILogger<ShiftController> logger)
    {
        _shiftService = shiftService;
        _logger = logger;
    }

    /// <summary>
    ///     Get all shifts
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<IEnumerable<ShiftDetailsDto>>), StatusCodes.Status200OK)]
    public async Task<IActionResult> GetAllShifts()
    {
        _logger.LogInformation("Getting all shifts");
        var shifts = await _shiftService.GetAllShiftsAsync();
        return Ok(ApiResponse<IEnumerable<ShiftDetailsDto>>.Ok(shifts));
    }

    /// <summary>
    ///     Get a shift by its ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<ShiftDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetShift(int id)
    {
        _logger.LogInformation("Getting shift with ID {ShiftId}", id);

        var shift = await _shiftService.GetShiftByIdAsync(id);
        if (shift is null)
            return NotFound(ApiResponse<object>.Fail($"Shift with ID {id} not found", "ShiftNotFound"));

        return Ok(ApiResponse<ShiftDetailsDto>.Ok(shift));
    }

    /// <summary>
    ///     Create a new open shift
    /// </summary>
    [HttpPost("open")]
    [ProducesResponseType(typeof(ApiResponse<ShiftDetailsDto>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> CreateOpenShift(CreateOpenShiftRequest request)
    {
        _logger.LogInformation("Creating open shift from {StartTime} to {EndTime}",
            request.StartTime, request.EndTime);

        var shift = await _shiftService.CreateOpenShiftAsync(request);

        return CreatedAtAction(
            nameof(GetShift),
            new { id = shift.Id },
            ApiResponse<ShiftDetailsDto>.Ok(shift, "Open shift created successfully"));
    }

    /// <summary>
    ///     Assign a shift to an employee
    /// </summary>
    [HttpPut("{id}/assign")]
    [ProducesResponseType(typeof(ApiResponse<ShiftDetailsDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status404NotFound)]
    [ProducesResponseType(typeof(ApiResponse<object>), StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> AssignShift(int id, AssignShiftRequest request)
    {
        _logger.LogInformation("Assigning shift {ShiftId} to employee {EmployeeId}",
            id, request.EmployeeId);

        if (id != request.ShiftId)
            return BadRequest(ApiResponse<object>.Fail(
                "ShiftId in URL must match request body", "InvalidShiftId"));

        var shift = await _shiftService.AssignShiftAsync(request);
        return Ok(ApiResponse<ShiftDetailsDto>.Ok(shift, "Shift assigned successfully"));
    }
}