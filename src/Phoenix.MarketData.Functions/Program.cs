using System.Linq;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Core.Events;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Infrastructure.Events;
using Phoenix.MarketData.Infrastructure.Repositories;
using Phoenix.MarketData.Infrastructure.Serialization.JsonConverters;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// App Insights
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddLogging(logging =>
    {
        logging.AddApplicationInsights();

        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            var defaultRule = options.Rules.FirstOrDefault(rule =>
                rule.ProviderName == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    });

builder.Services.Configure<JsonSerializerOptions>(options =>
{
    options.Converters.Add(new DateOnlyJsonConverter());
    options.Converters.Add(new TimeOnlyJsonConverter());
    // You can add more converters here
});

// Add configuration (env first, then files)
builder.Configuration
    .AddEnvironmentVariables()
    .AddJsonFile("prod.settings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);

var configuration = builder.Configuration;

// Cosmos DB registration (fail fast if missing config)
var cosmosConnectionString = configuration["MarketDataHistoryCosmosDb:ConnectionString"];
var cosmosDatabaseId = configuration["MarketDataHistoryCosmosDb:DatabaseId"];
var cosmosContainerId = configuration["MarketDataHistoryCosmosDb:ContainerId"];

if (string.IsNullOrWhiteSpace(cosmosConnectionString))
    throw new InvalidOperationException("Missing configuration: MarketDataHistoryCosmosDb:ConnectionString");
if (string.IsNullOrWhiteSpace(cosmosDatabaseId))
    throw new InvalidOperationException("Missing configuration: MarketDataHistoryCosmosDb:DatabaseId");
if (string.IsNullOrWhiteSpace(cosmosContainerId))
    throw new InvalidOperationException("Missing configuration: MarketDataHistoryCosmosDb:ContainerId");

// Register CosmosClient as singleton
builder.Services.AddSingleton(sp =>
    new CosmosClient(cosmosConnectionString, new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Direct,
        ConsistencyLevel = ConsistencyLevel.Session
    }));

builder.Services.AddSingleton<IEventPublisher, EventGridPublisher>();

builder.Services.AddSingleton(sp =>
    new CosmosClient(cosmosConnectionString, new CosmosClientOptions
    {
        ConnectionMode = ConnectionMode.Direct,
        ConsistencyLevel = ConsistencyLevel.Session
    }));

builder.Services.AddScoped<IRepository<FxSpotPriceData>>(sp =>
    new CosmosRepository<FxSpotPriceData>(
        sp.GetRequiredService<CosmosClient>().GetContainer(cosmosDatabaseId, cosmosContainerId),
        sp.GetRequiredService<ILogger<CosmosRepository<FxSpotPriceData>>>(),
        sp.GetRequiredService<IEventPublisher>()
    )
);

builder.Services.AddScoped<IRepository<FxVolSurfaceData>>(sp =>
    new CosmosRepository<FxVolSurfaceData>(
        sp.GetRequiredService<CosmosClient>().GetContainer(cosmosDatabaseId, cosmosContainerId),
        sp.GetRequiredService<ILogger<CosmosRepository<FxVolSurfaceData>>>(),
        sp.GetRequiredService<IEventPublisher>()
    )
);

builder.Services.AddScoped<IRepository<CryptoOrdinalSpotPriceData>>(sp =>
    new CosmosRepository<CryptoOrdinalSpotPriceData>(
        sp.GetRequiredService<CosmosClient>().GetContainer(cosmosDatabaseId, cosmosContainerId),
        sp.GetRequiredService<ILogger<CosmosRepository<CryptoOrdinalSpotPriceData>>>(),
        sp.GetRequiredService<IEventPublisher>()
    )
);

builder.Build().Run();
