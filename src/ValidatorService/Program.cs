using ValidatorService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.ConfigureControllers(); // Configure controllers and JSON settings
builder.Services.AddSwaggerDocumentation(); // Register Swagger
builder.Services.ConfigureHttps(builder.Configuration, builder.Environment); // Configure HTTPS & HSTS via extension method
builder.ConfigureKestrelServer(); // Configure Kestrel

var app = builder.Build();

app.UseGlobalExceptionHandling(); // Apply global exception handling middleware

app.UseSwaggerDocumentation(); // Apply Swagger settings

app.UseHttpsConfig(builder.Configuration); // Apply HTTPS settings

app.Run();
