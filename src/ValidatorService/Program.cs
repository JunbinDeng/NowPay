using ValidatorService.Extensions;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddSwaggerDocumentation(); // Register Swagger

var app = builder.Build();

app.UseSwaggerDocumentation(); // Apply Swagger settings

app.UseHttpsRedirection();

app.Run();
