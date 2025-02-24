using ValidatorService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerDocumentation(); // Register Swagger
builder.Services.ConfigureHttps(builder.Configuration, builder.Environment); // Configure HTTPS & HSTS via extension method

var app = builder.Build();

app.UseSwaggerDocumentation(); // Apply Swagger settings

app.UseHttpsRedirection();

app.Run();
