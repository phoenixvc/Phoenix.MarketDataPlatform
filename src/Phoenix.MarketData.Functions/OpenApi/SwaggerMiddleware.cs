using System.IO;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;

namespace Phoenix.MarketData.Functions.OpenApi
{
    public class SwaggerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<SwaggerMiddleware> _logger;

        public SwaggerMiddleware(RequestDelegate next, ILogger<SwaggerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/api/swagger.json") ||
                context.Request.Path.StartsWithSegments("/api/openapi.json"))
            {
                _logger.LogInformation("Serving OpenAPI specification");

                context.Response.ContentType = "application/json";
                var openApiFilePath = Path.Combine(Directory.GetCurrentDirectory(), "openapi.json");

                try
                {
                    if (File.Exists(openApiFilePath))
                    {
                        await context.Response.SendFileAsync(openApiFilePath);
                        return;
                    }
                    else
                    {
                        _logger.LogError($"OpenAPI file not found at: {openApiFilePath}");
                        context.Response.StatusCode = StatusCodes.Status404NotFound;
                        await context.Response.WriteAsync("OpenAPI specification not found.");
                        return;
                    }
                }
                catch (Exception ex) when (ex is System.Security.SecurityException || ex is UnauthorizedAccessException || ex is IOException)
                {
                    _logger.LogError(ex, $"Error accessing OpenAPI file at: {openApiFilePath}");
                    context.Response.StatusCode = StatusCodes.Status500InternalServerError;
                    await context.Response.WriteAsync("Error accessing OpenAPI specification.");
                    return;
                }
            }

            await _next(context);
        }
    }
}