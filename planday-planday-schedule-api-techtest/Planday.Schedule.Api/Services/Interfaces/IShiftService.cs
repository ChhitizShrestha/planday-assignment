using Planday.Schedule.Api.Models;

namespace Planday.Schedule.Api.Services;

public interface IShiftService
{
    Task<IEnumerable<ShiftDetailsDto>> GetAllShiftsAsync();
    Task<ShiftDetailsDto> GetShiftByIdAsync(int id);
    Task<ShiftDetailsDto> CreateOpenShiftAsync(CreateOpenShiftRequest request);
    Task<ShiftDetailsDto> AssignShiftAsync(AssignShiftRequest request);
}