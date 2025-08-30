using Planday.Schedule.Api.Extensions;
using Planday.Schedule.Api.Middleware;
using Serilog;

var builder = WebApplication.CreateBuilder(args);

try
{
    builder.Host.ConfigureLogging();

    builder.Services.ConfigureServices(builder.Configuration);

    var app = builder.Build();

    if (app.Environment.IsDevelopment())
    {
        app.UseSwagger();
        app.UseSwaggerUI();
    }

    app.UseSerilogRequestLogging();
    app.UseMiddleware<ErrorHandlingMiddleware>();
    app.UseHttpsRedirection();
    app.UseAuthorization();
    app.MapControllers();


    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
    throw;
}
finally
{
    Log.CloseAndFlush();
}