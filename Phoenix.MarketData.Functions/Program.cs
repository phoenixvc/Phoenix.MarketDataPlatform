using Microsoft.Azure.Cosmos;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Phoenix.MarketData.Infrastructure.Cosmos;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();

// Application Insights isn't enabled by default. See https://aka.ms/AAt8mw4.
builder.Services
    .AddApplicationInsightsTelemetryWorkerService()
    .ConfigureFunctionsApplicationInsights()
    .AddLogging(logging =>
    {
        logging.AddApplicationInsights();
        
        logging.Services.Configure<LoggerFilterOptions>(options =>
        {
            LoggerFilterRule defaultRule = options.Rules.FirstOrDefault(rule => rule.ProviderName
                == "Microsoft.Extensions.Logging.ApplicationInsights.ApplicationInsightsLoggerProvider");
            if (defaultRule is not null)
            {
                options.Rules.Remove(defaultRule);
            }
        });
    });

// Add configuration from appsettings.json and environment variables
builder.Configuration
    .AddJsonFile("prod.settings.json", optional: true, reloadOnChange: true)
    .AddJsonFile("local.settings.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();

// Retrieve the IConfiguration instance
var configuration = builder.Configuration;

builder.Services
    .AddSingleton<CosmosClient>(sp => 
        CosmosClientFactory.CreateClient(configuration["MarketDataHistoryCosmosDb:ConnectionString"]))
    .AddScoped(sp => new MarketDataRepository(
        sp.GetRequiredService<CosmosClient>(),
        configuration["MarketDataHistoryCosmosDb:DatabaseId"],
        configuration["MarketDataHistoryCosmosDb:ContainerId"]))
    .AddScoped<VersionManager>();

builder.Build().Run();