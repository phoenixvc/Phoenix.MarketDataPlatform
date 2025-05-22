using System.Reflection;
using Microsoft.Azure.Functions.Worker.Extensions.OpenApi.Extensions;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.OpenApi.Models;
using System.IO;

namespace Phoenix.MarketData.Functions.OpenApi
{
    public static class SwaggerExtension
    {
        public static IHostBuilder ConfigureSwagger(this IHostBuilder builder)
        {
            return builder.ConfigureOpenApi((options) =>
            {
                options.DocumentTitle = "Phoenix Market Data API";
                options.DocumentName = "v1";
                options.Version = "1.0.0";
                options.OpenApiVersion = Microsoft.OpenApi.OpenApiSpecVersion.V3_0;
                options.Info = new OpenApiInfo
                {
                    Title = "Phoenix Market Data API",
                    Version = "1.0.0",
                    Description = "API for managing and retrieving market data for various asset classes",
                    Contact = new OpenApiContact
                    {
                        Name = "Phoenix Team"
                    }
                };

                // Use the pre-defined OpenAPI document for advanced customization
                var customOpenApiPath = Path.Combine(
                    Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
                    "openapi.json");
                
                if (File.Exists(customOpenApiPath))
                {
                    options.CustomOpenApiDocument = File.ReadAllText(customOpenApiPath);
                    options.UseCustomOpenApiDocument = true;
                }
            });
        }
    }
}