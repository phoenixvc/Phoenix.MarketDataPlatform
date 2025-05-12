using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Domain.Schemas;
using Phoenix.MarketData.Infrastructure.Cosmos;

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

        var doc = new FxSpotPriceObject
        {
            SchemaVersion = SchemaVersions.V0,
            AssetId = "BTCUSD",
            AssetClass = "fx",
            DataType = "price.spot",
            Region = "NY",
            Tags = ["spot"],
            DocumentType = "official",
            AsOfDate = new DateOnly(2025, 4, 20),
            AsOfTime = new TimeOnly(15, 30, 05),
            Price = 95710.96
        };
        await _repository.SaveAsync(doc);

        return new OkObjectResult("Welcome to Azure Functions!");
    }

}