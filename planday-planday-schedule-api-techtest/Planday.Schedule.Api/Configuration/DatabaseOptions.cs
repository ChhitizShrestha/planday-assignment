namespace Planday.Schedule.Api.Configuration;

public class DatabaseOptions
{
    public const string SectionName = "ConnectionStrings";

    public string Database { get; set; } = string.Empty;
}