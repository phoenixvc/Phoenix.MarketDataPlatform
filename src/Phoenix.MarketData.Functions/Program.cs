using System.Linq;
using System.Text.Json;
using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Phoenix.MarketData.Domain.Events;
using Phoenix.MarketData.Infrastructure.Events;
using Phoenix.MarketData.Infrastructure.Repositories;
using Phoenix.MarketData.Infrastructure.Serialization.JsonConverters;
using Phoenix.MarketData.Domain.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Abstractions;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Configurations;
using Microsoft.OpenApi.Models;
using Phoenix.MarketData.Domain.Models;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureAppConfiguration(builder =>
    {
        builder.AddEnvironmentVariables()
               .AddJsonFile("prod.settings.json", optional: true, reloadOnChange: true)
               .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true);
    })
    .ConfigureServices((context, services) =>
    {
        // Register OpenAPI services
        services.AddSingleton<IOpenApiConfigurationOptions>(_ => new OpenApiConfigurationOptions()
        {
            Info = new OpenApiInfo()
            {
                Version = "1.0.0",
                Title = "Phoenix Market Data API",
                Description = "API for managing and retrieving market data for various asset classes",
                Contact = new OpenApiContact()
                {
                    Name = "Phoenix Team"
                }
            },
            Servers = DefaultOpenApiConfigurationOptions.GetHostNames(),
            OpenApiVersion = OpenApiVersionType.V3
        });

        // App Insights
        services
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

        services.Configure<JsonSerializerOptions>(options =>
        {
            options.Converters.Add(new DateOnlyJsonConverter());
            options.Converters.Add(new TimeOnlyJsonConverter());
            // You can add more converters here
        });

        var configuration = context.Configuration;

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
        services.AddSingleton(sp =>
            new CosmosClient(cosmosConnectionString, new CosmosClientOptions
            {
                ConnectionMode = ConnectionMode.Direct,
                ConsistencyLevel = ConsistencyLevel.Session
            }));

        services.AddSingleton<IEventPublisher, EventGridPublisher>();

        // Register repositories
        services.AddScoped<IRepository<FxSpotPriceData>>(sp =>
            new CosmosRepository<FxSpotPriceData>(
                sp.GetRequiredService<CosmosClient>().GetContainer(cosmosDatabaseId, cosmosContainerId),
                sp.GetRequiredService<ILogger<CosmosRepository<FxSpotPriceData>>>(),
                sp.GetRequiredService<IEventPublisher>()
            )
        );

        services.AddScoped<IRepository<FxVolSurfaceData>>(sp =>
            new CosmosRepository<FxVolSurfaceData>(
                sp.GetRequiredService<CosmosClient>().GetContainer(cosmosDatabaseId, cosmosContainerId),
                sp.GetRequiredService<ILogger<CosmosRepository<FxVolSurfaceData>>>(),
                sp.GetRequiredService<IEventPublisher>()
            )
        );

        services.AddScoped<IRepository<CryptoOrdinalSpotPriceData>>(sp =>
            new CosmosRepository<CryptoOrdinalSpotPriceData>(
                sp.GetRequiredService<CosmosClient>().GetContainer(cosmosDatabaseId, cosmosContainerId),
                sp.GetRequiredService<ILogger<CosmosRepository<CryptoOrdinalSpotPriceData>>>(),
                sp.GetRequiredService<IEventPublisher>()
            )
        );

        // Register CosmosRepository<IMarketDataEntity> needed by SaveDocumentToDb
        services.AddScoped<CosmosRepository<IMarketDataEntity>>(sp =>
            new CosmosRepository<IMarketDataEntity>(
                sp.GetRequiredService<CosmosClient>().GetContainer(cosmosDatabaseId, cosmosContainerId),
                sp.GetRequiredService<ILogger<CosmosRepository<IMarketDataEntity>>>(),
                sp.GetRequiredService<IEventPublisher>()
            )
        );
    })
    .Build();

host.Run();
