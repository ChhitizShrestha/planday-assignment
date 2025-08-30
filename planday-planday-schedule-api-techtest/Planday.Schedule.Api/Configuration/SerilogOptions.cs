namespace Planday.Schedule.Api.Configuration;

public class SerilogOptions
{
    public const string SectionName = "Serilog";

    public MinimumLevel MinimumLevel { get; set; } = new();
    public WriteToOptions[] WriteTo { get; set; } = Array.Empty<WriteToOptions>();
    public string[] Enrich { get; set; } = Array.Empty<string>();
}

public class MinimumLevel
{
    public string Default { get; set; } = "Information";
    public Dictionary<string, string> Override { get; set; } = new();
}

public class WriteToOptions
{
    public string Name { get; set; } = string.Empty;
    public FileArgs? Args { get; set; }
}

public class FileArgs
{
    public string Path { get; set; } = string.Empty;
    public string RollingInterval { get; set; } = "Day";
    public int RetainedFileCountLimit { get; set; } = 7;
}