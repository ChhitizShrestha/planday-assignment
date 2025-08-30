namespace Planday.Schedule.Api.Configuration;

public class HttpClientPolicyOptions
{
    public int RetryCount { get; set; }
    public double RetryBaseSeconds { get; set; }
    public int CircuitBreakerFailureThreshold { get; set; }
    public int CircuitBreakerDurationSeconds { get; set; }
}