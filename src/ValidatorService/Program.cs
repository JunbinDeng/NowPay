using ValidatorService.Data;
using ValidatorService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureControllers(); // Configure controllers and JSON settings
builder.Services.AddSwaggerDocumentation(); // Register Swagger
builder.Services.ConfigureHttps(builder.Configuration, builder.Environment); // Configure HTTPS & HSTS via extension method
builder.ConfigureKestrelServer(); // Configure Kestrel
builder.Services.AddScoped<IValidatorService, LuhnValidatorService>();
builder.Services.AddHealthChecks();

var app = builder.Build();

app.UseGlobalExceptionHandling(); // Apply global exception handling middleware

app.MapHealthChecks("/healthz"); // Monitor the service's health status

app.UseSwaggerDocumentation(); // Apply Swagger settings

app.UseHttpsConfig(builder.Configuration); // Apply HTTPS settings

app.MapControllers();

app.Run();

/// <summary>
/// Partial Program class used for integration testing.
/// This allows test projects to reference and extend the main application entry point.
/// </summary>
public partial class Program { }
