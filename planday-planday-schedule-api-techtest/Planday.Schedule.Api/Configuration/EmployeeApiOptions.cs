namespace Planday.Schedule.Api.Configuration;

public class EmployeeApiOptions
{
    public const string SectionName = "EmployeeApi";

    public string BaseUrl { get; set; } = string.Empty;
    public string ApiKey { get; set; } = string.Empty;
    public int TimeoutSeconds { get; set; } = 30;
}