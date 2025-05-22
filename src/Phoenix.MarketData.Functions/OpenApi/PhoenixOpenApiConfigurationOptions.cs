using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;

namespace Phoenix.MarketData.Functions.OpenApi
{
    public class PhoenixOpenApiConfigurationOptions : DefaultOpenApiConfigurationOptions
    {
        public override OpenApiInfo Info { get; set; } = new OpenApiInfo
        {
            Title = "Phoenix Market Data API",
            Version = "1.0.0",
            Description = "API for managing and retrieving market data for various asset classes",
            Contact = new OpenApiContact
            {
                Name = "Phoenix Team"
            }
        };

        public override OpenApiVersionType OpenApiVersion { get; set; } = OpenApiVersionType.V3;

        // Fix: Use nullable string type to properly handle null values
        private string? _customDocument = null;

        // Use a property to store the custom document
        public string? CustomDocument
        {
            get => _customDocument;
            set => _customDocument = value;
        }
    }
}