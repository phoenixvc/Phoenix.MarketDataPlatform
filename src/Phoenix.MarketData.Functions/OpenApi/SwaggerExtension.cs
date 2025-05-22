using System.IO;
using System.Reflection;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;

namespace Phoenix.MarketData.Functions.OpenApi
{
    public static class SwaggerExtension
    {
        public static IHostBuilder ConfigureSwagger(this IHostBuilder builder)
        {
            return builder.ConfigureServices((hostContext, services) =>
            {
                // Create and configure our custom options
                var options = new PhoenixOpenApiConfigurationOptions();

                // Use the pre-defined OpenAPI document for advanced customization
                var customOpenApiPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) ?? string.Empty,
                    "openapi.json");

                if (File.Exists(customOpenApiPath))
                {
                    options.CustomDocument = File.ReadAllText(customOpenApiPath);
                }

                // Register our custom options
                services.AddSingleton<IOpenApiConfigurationOptions>(options);
            })
            // Configure OpenAPI with the basic setup
            .ConfigureOpenApi();
        }
    }
}