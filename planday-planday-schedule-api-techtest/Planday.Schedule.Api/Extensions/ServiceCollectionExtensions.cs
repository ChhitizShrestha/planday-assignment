using FluentValidation;
using Microsoft.Extensions.Options;
using Planday.Schedule.Api.Configuration;
using Planday.Schedule.Api.Middleware;
using Planday.Schedule.Api.Services;
using Planday.Schedule.Infrastructure.Providers;
using Planday.Schedule.Infrastructure.Providers.Interfaces;
using Planday.Schedule.Infrastructure.Queries;
using Planday.Schedule.Queries;
using Polly;
using Polly.Extensions.Http;
using Serilog;

namespace Planday.Schedule.Api.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddApplicationConfiguration(this IServiceCollection services,
        IConfiguration configuration)
    {
        services.Configure<DatabaseOptions>(configuration.GetSection(DatabaseOptions.SectionName));
        services.Configure<EmployeeApiOptions>(configuration.GetSection(EmployeeApiOptions.SectionName));
        services.Configure<HttpClientPolicyOptions>(configuration.GetSection("HttpClientPolicy"));
        services.Configure<SerilogOptions>(configuration.GetSection(SerilogOptions.SectionName));
        services.AddExceptionHandler<ApiExceptionHandler>();
        services.AddProblemDetails();
        return services;
    }

    public static IServiceCollection AddApplicationServices(this IServiceCollection services)
    {
        services.AddScoped<IShiftService, ShiftService>();
        services.AddScoped<IGetAllShiftsQuery, GetAllShiftsQuery>();
        services.AddScoped<IGetAllShiftsWithEmployeeQuery, GetAllShiftsWithEmployeeQuery>();
        services.AddScoped<IGetShiftByIdWithEmployeeQuery, GetShiftByIdWithEmployeeQuery>();
        services.AddScoped<IGetShiftByIdQuery, GetShiftByIdQuery>();
        services.AddScoped<ICreateShiftCommand, CreateShiftCommand>();
        services.AddScoped<IAssignShiftCommand, AssignShiftCommand>();
        services.AddScoped<IHasOverlappingShiftsQuery, HasOverlappingShiftsQuery>();

        return services;
    }

    public static IServiceCollection AddEmployeeApiClient(this IServiceCollection services,
        IConfiguration configuration)
    {
        var employeeApiOptions = services.BuildServiceProvider()
            .GetRequiredService<IOptions<EmployeeApiOptions>>().Value;

        if (string.IsNullOrEmpty(employeeApiOptions.BaseUrl))
            throw new InvalidOperationException("Employee API base URL not configured");
        if (string.IsNullOrEmpty(employeeApiOptions.ApiKey))
            throw new InvalidOperationException("Employee API key not configured");

        services.AddHttpClient<IEmployeeApiClient, EmployeeApiClient>(client =>
            {
                client.BaseAddress = new Uri(employeeApiOptions.BaseUrl);
                client.DefaultRequestHeaders.Add("Authorization", employeeApiOptions.ApiKey);
                client.Timeout = TimeSpan.FromSeconds(employeeApiOptions.TimeoutSeconds);
            })
            .AddPolicyHandler((provider, _) => GetRetryPolicy(provider))
            .AddPolicyHandler((provider, _) => GetCircuitBreakerPolicy(provider));

        return services;
    }

    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var dbOptions = services.BuildServiceProvider()
            .GetRequiredService<IOptions<DatabaseOptions>>().Value;

        if (string.IsNullOrEmpty(dbOptions.Database))
            throw new ArgumentNullException(nameof(dbOptions.Database), "Database connection string not configured");

        services.AddSingleton<IConnectionStringProvider>(new ConnectionStringProvider(dbOptions.Database));
        return services;
    }

    public static IServiceCollection AddValidation(this IServiceCollection services)
    {
        services.AddValidatorsFromAssemblyContaining<Program>();
        return services;
    }

    public static IServiceCollection ConfigureServices(this IServiceCollection services, IConfiguration configuration)
    {
        services
            .AddControllers()
            .Services
            .AddEndpointsApiExplorer()
            .AddSwaggerGen()
            .AddApplicationConfiguration(configuration)
            .AddApplicationServices()
            .AddEmployeeApiClient(configuration)
            .AddInfrastructure(configuration)
            .AddValidation();

        return services;
    }

    public static IHostBuilder ConfigureLogging(this IHostBuilder builder)
    {
        builder.UseSerilog((context, services, configuration) => configuration
            .ReadFrom.Configuration(context.Configuration)
            .ReadFrom.Services(services)
            .Enrich.FromLogContext()
            .WriteTo.Console());

        return builder;
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy(IServiceProvider services)
    {
        var options = services.GetRequiredService<IOptions<HttpClientPolicyOptions>>().Value;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .WaitAndRetryAsync(
                options.RetryCount,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(options.RetryBaseSeconds, retryAttempt)),
                (result, timeSpan, retryCount, _) =>
                {
                    Log.Warning(
                        "Error calling employee API. Retry attempt {RetryCount} after {RetryInterval}s. Status code: {StatusCode}",
                        retryCount,
                        timeSpan.TotalSeconds,
                        result.Result?.StatusCode);
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy(IServiceProvider services)
    {
        var options = services.GetRequiredService<IOptions<HttpClientPolicyOptions>>().Value;

        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                options.CircuitBreakerFailureThreshold,
                TimeSpan.FromSeconds(options.CircuitBreakerDurationSeconds),
                (result, duration) =>
                {
                    Log.Warning(
                        "Circuit breaker opened for {DurationSeconds}s. Status code: {StatusCode}",
                        duration.TotalSeconds,
                        result.Result?.StatusCode);
                },
                () => { Log.Information("Circuit breaker reset, resuming normal operation"); });
    }
}