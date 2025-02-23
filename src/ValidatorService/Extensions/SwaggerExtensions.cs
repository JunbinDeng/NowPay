using Microsoft.OpenApi.Models;
using ValidatorService.Documentation;

namespace ValidatorService.Extensions;

public static class SwaggerExtensions
{
    public static IServiceCollection AddSwaggerDocumentation(this IServiceCollection services)
    {
        services.AddEndpointsApiExplorer();
        services.AddSwaggerGen(c =>
        {
            c.SwaggerDoc("v1", new OpenApiInfo
            {
                Title = "ValidatorService API",
                Version = "v1",
                Description = "API for validating credit card numbers using the Luhn algorithm."
            });

            c.OperationFilter<SwaggerResponseExamples>();
        });
        return services;
    }

    public static IApplicationBuilder UseSwaggerDocumentation(this WebApplication app)
    {
        var isSwaggerEnabled = app.Configuration.GetValue<bool>("SwaggerEnabled");
        if (isSwaggerEnabled || app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI(c =>
            {
                c.SwaggerEndpoint("/swagger/v1/swagger.json", "ValidatorService API v1");
                c.RoutePrefix = string.Empty; // Swagger loads at root URL
            });
        }
        return app;
    }
}
