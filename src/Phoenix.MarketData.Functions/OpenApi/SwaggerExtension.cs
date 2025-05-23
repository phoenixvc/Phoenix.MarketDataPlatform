using System;
using System.IO;
using System.Reflection;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;

namespace Phoenix.MarketData.Functions.OpenApi
{
    public static class SwaggerExtension
    {
        public static IHostBuilder ConfigureSwagger(this IHostBuilder builder)
        {
            return builder.ConfigureServices(static (hostContext, services) =>
            {
                // Create and configure our custom options
                var options = new PhoenixOpenApiConfigurationOptions();

                // Get logger factory from services
                var loggerFactory = services.BuildServiceProvider().GetService<ILoggerFactory>();
                var logger = loggerFactory?.CreateLogger<SwaggerExtension>();

                try
                {
                    // Add null safety for assembly location
                    string assemblyLocation = Assembly.GetExecutingAssembly().Location;
                    if (!string.IsNullOrEmpty(assemblyLocation))
                    {
                        // Use the pre-defined OpenAPI document for advanced customization
                        var customOpenApiPath = Path.Combine(
                            Path.GetDirectoryName(assemblyLocation) ?? string.Empty,
                            "openapi.json");

                        try
                        {
                            if (File.Exists(customOpenApiPath))
                            {
                                options.CustomDocument = File.ReadAllText(customOpenApiPath);
                            }
                        }
                        catch (IOException ex)
                        {
                            // Log the error but continue execution without the custom document
                            logger?.LogWarning(ex, "Unable to read OpenAPI custom document: {Message}", ex.Message);
                        }
                        catch (UnauthorizedAccessException ex)
                        {
                            // Handle permission issues
                            logger?.LogWarning(ex, "Permission denied when accessing OpenAPI custom document: {Message}", ex.Message);
                        }
                        catch (Exception ex)
                        {
                            // Handle any other unexpected exceptions
                            logger?.LogWarning(ex, "Unexpected error when reading OpenAPI custom document: {Message}", ex.Message);
                        }
                    }
                }
                catch (Exception ex)
                {
                    // Handle any exceptions during the entire process
                    logger?.LogError(ex, "Failed to configure OpenAPI custom document: {Message}", ex.Message);
                }

                // Register our custom options
                services.AddSingleton<IOpenApiConfigurationOptions>(options);
            })
            // Configure OpenAPI with the basic setup
            .ConfigureOpenApi();
        }
    }
}