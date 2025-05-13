using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Phoenix.MarketData.Domain;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Cosmos;
using Phoenix.MarketData.Infrastructure.Schemas;

namespace Phoenix.MarketData.Functions;

public class SaveDocumentToDb
{
    private readonly ILogger<SaveDocumentToDb> _logger;
    private MarketDataRepository _repository;

    public SaveDocumentToDb(ILogger<SaveDocumentToDb> logger, MarketDataRepository repository)
    {
        _repository = repository;
        _logger = logger;
    }

    [Function("SaveDocumentToDb")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var doc = new FxSpotPriceData
        {
            SchemaVersion = SchemaVersions.V0,
            AssetId = "BTCUSD",
            AssetClass = "fx",
            DataType = "price.spot",
            Region = Regions.NewYork,
            Tags = ["spot"],
            DocumentType = "official",
            AsOfDate = new DateOnly(2025, 4, 20),
            AsOfTime = new TimeOnly(15, 30, 5),
            Price = 95710.96m
        };
        await _repository.SaveAsync(doc);

        return new OkObjectResult("Welcome to Azure Functions!");
    }

}