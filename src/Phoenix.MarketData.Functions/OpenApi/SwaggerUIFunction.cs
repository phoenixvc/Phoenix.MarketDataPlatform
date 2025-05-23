using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace Phoenix.MarketData.Functions.OpenApi
{
    public class SwaggerUIFunction
    {
        private readonly ILogger<SwaggerUIFunction> _logger;

        public SwaggerUIFunction(ILogger<SwaggerUIFunction> logger)
        {
            _logger = logger;
        }

        [Function("SwaggerUI")]
        public HttpResponseData GetSwaggerUI([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/ui")] HttpRequestData req)
        {
            _logger.LogInformation("Serving Swagger UI");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "text/html; charset=utf-8");

            var html = @"
<!DOCTYPE html>
<html lang=""en"">
<head>
    <meta charset=""UTF-8"">
    <meta name=""viewport"" content=""width=device-width, initial-scale=1.0"">
    <title>Phoenix Market Data API - Swagger</title>
    <link rel=""stylesheet"" type=""text/css"" href=""https://unpkg.com/swagger-ui-dist@4.5.0/swagger-ui.css"">
    <style>
        html {
            box-sizing: border-box;
            overflow: -moz-scrollbars-vertical;
            overflow-y: scroll;
        }
        *, *:before, *:after {
            box-sizing: inherit;
        }
        body {
            margin: 0;
            background: #fafafa;
        }
        .swagger-ui .topbar {
            background-color: #1f1f1f;
        }
        .swagger-ui .topbar .download-url-wrapper .select-label {
            color: white;
        }
    </style>
</head>
<body>
    <div id=""swagger-ui""></div>
    <script src=""https://unpkg.com/swagger-ui-dist/swagger-ui-bundle.js""></script>
    <script src=""https://unpkg.com/swagger-ui-dist/swagger-ui-standalone-preset.js""></script>
    <!-- Consider pinning to a vetted version and reviewing regularly for security updates -->
    <script>
        window.onload = function() {
            const ui = SwaggerUIBundle({
                url: ""/api/swagger/openapi.json"",
                dom_id: '#swagger-ui',
                deepLinking: true,
                presets: [
                    SwaggerUIBundle.presets.apis,
                    SwaggerUIStandalonePreset
                ],
                plugins: [
                    SwaggerUIBundle.plugins.DownloadUrl
                ],
                layout: ""StandaloneLayout""
            });
            window.ui = ui;
        };
    </script>
</body>
</html>";

            response.WriteString(html);
            return response;
        }

        [Function("SwaggerJson")]
        public HttpResponseData GetSwaggerJson([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "swagger/openapi.json")] HttpRequestData req)
        {
            _logger.LogInformation("Serving OpenAPI specification");

            var response = req.CreateResponse(HttpStatusCode.OK);
            response.Headers.Add("Content-Type", "application/json");

            string openApiPath = Path.Combine(Directory.GetCurrentDirectory(), "openapi.json");
            if (File.Exists(openApiPath))
            {
                string jsonContent = File.ReadAllText(openApiPath);
                response.WriteString(jsonContent);
            }
            else
            {
                _logger.LogError($"OpenAPI file not found at: {openApiPath}");
                response = req.CreateResponse(HttpStatusCode.NotFound);
                response.WriteString("OpenAPI specification not found.");
            }

            return response;
        }
    }
}