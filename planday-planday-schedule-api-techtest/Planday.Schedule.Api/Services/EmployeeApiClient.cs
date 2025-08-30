namespace Planday.Schedule.Api.Services;

public class EmployeeApiClient : IEmployeeApiClient
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EmployeeApiClient> _logger;

    public EmployeeApiClient(HttpClient httpClient, ILogger<EmployeeApiClient> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<EmployeeDto> GetEmployeeAsync(long employeeId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"employee/{employeeId}");

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning("Failed to get employee {EmployeeId}. Status code: {StatusCode}",
                    employeeId, response.StatusCode);
                return null;
            }

            return await response.Content.ReadFromJsonAsync<EmployeeDto>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting employee {EmployeeId}", employeeId);
            throw;
        }
    }
}