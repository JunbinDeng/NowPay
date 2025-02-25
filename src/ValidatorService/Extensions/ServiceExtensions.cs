using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using ValidatorService.Middleware;

namespace ValidatorService.Extensions;

public static class ServiceExtensions
{
    /// <summary>
    /// Configures HTTPS redirection and HSTS settings dynamically from appsettings.json.
    /// </summary>
    public static void ConfigureHttps(this IServiceCollection services, IConfiguration configuration, IWebHostEnvironment environment)
    {
        // Enable HTTPS Redirection (Dynamic Port from Config)
        services.AddHttpsRedirection(options =>
        {
            var httpsUrl = configuration["Kestrel:Endpoints:Https:Url"];
            if (httpsUrl is not null && Uri.TryCreate(httpsUrl, UriKind.Absolute, out var uri))
            {
                options.HttpsPort = uri.Port; // Dynamically use the port from config
            }
        });

        // Enable HSTS only in Production
        if (configuration.GetValue<bool>("UseHsts") && !environment.IsDevelopment())
        {
            services.AddHsts(options =>
            {
                options.Preload = true;
                options.IncludeSubDomains = true;
                options.MaxAge = TimeSpan.FromDays(365);
            });
        }
    }

    /// <summary>
    /// Configures Kestrel to listen on HTTP and HTTPS ports dynamically from configuration.
    /// </summary>
    public static void ConfigureKestrelServer(this WebApplicationBuilder builder)
    {
        builder.WebHost.ConfigureKestrel(options =>
        {
            var configuration = builder.Configuration;

            // Configure HTTPS
            var httpsUrl = configuration["Kestrel:Endpoints:Https:Url"];
            var certPath = configuration["Kestrel:Endpoints:Https:Certificates:Path"]; // Default path for Docker
            var certPassword = configuration["Kestrel:Endpoints:Https:Certificates:Password"]; // Default password

            if (!string.IsNullOrEmpty(httpsUrl) && Uri.TryCreate(httpsUrl, UriKind.Absolute, out var uriHttps))
            {
                if (File.Exists(certPath))
                {
                    options.ListenAnyIP(uriHttps.Port, listenOptions =>
                    {
                        listenOptions.UseHttps(certPath, certPassword); // Load HTTPS certificate
                    });
                }
                else
                {
                    Console.WriteLine($"Warning: HTTPS certificate not found at {certPath}. HTTPS might not work.");
                }
            }
            else
            {
                Console.WriteLine("Warning: HTTPS URL is not properly configured.");
            }

            // Configure HTTP
            var httpUrl = configuration["Kestrel:Endpoints:Http:Url"];
            if (!string.IsNullOrEmpty(httpUrl) && Uri.TryCreate(httpUrl, UriKind.Absolute, out var uriHttp))
            {
                options.ListenAnyIP(uriHttp.Port); // Enable HTTP
            }
        });
    }

    /// <summary>
    /// Applies HTTPS Redirection and HSTS middleware at runtime.
    /// </summary>
    public static void UseHttpsConfig(this WebApplication app, IConfiguration configuration)
    {
        if (configuration.GetValue<bool>("UseHsts") && !app.Environment.IsDevelopment())
        {
            app.UseHsts(); // Enforce HSTS only in Production
        }

        app.UseHttpsRedirection(); // Redirect HTTP to HTTPS
    }

    /// <summary>
    /// Configures essential services including JSON serialization options and API behavior.
    /// </summary>
    public static void ConfigureControllers(this IServiceCollection services)
    {
        services.AddControllers()
            .AddJsonOptions(options =>
            {
                options.JsonSerializerOptions.DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull;
                options.JsonSerializerOptions.PropertyNamingPolicy = JsonNamingPolicy.CamelCase;
            });

        services.Configure<ApiBehaviorOptions>(options =>
        {
            options.SuppressModelStateInvalidFilter = true; // Allow middleware to catch JSON errors
        });
    }

    /// <summary>
    /// Adds global exception handling middleware to the application.
    /// </summary>
    public static void UseGlobalExceptionHandling(this WebApplication app)
    {
        app.UseMiddleware<ExceptionHandlingMiddleware>(); // Apply global exception handling
    }
}