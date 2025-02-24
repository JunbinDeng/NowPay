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
}