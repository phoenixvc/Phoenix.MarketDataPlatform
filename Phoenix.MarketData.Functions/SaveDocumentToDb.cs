using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Cosmos;
using Phoenix.MarketData.Infrastructure.Mapping;
using Phoenix.MarketData.Infrastructure.Serialization;

namespace Phoenix.MarketData.Functions;

public class SaveDocumentToDb
{
    private readonly ILogger<SaveDocumentToDb> _logger;
    private readonly MarketDataRepository _repository;
    
    public SaveDocumentToDb(ILogger<SaveDocumentToDb> logger, MarketDataRepository repository)
    {
        _repository = repository;
        _logger = logger;
    }

    [Function("SaveDocumentToDb")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        var dataType = req.Query["datatype"];
        var assetClass = req.Query["assetclass"];

        if (string.IsNullOrWhiteSpace(dataType) || string.IsNullOrWhiteSpace(assetClass))
        {
            return new BadRequestObjectResult("Please provide both 'datatype' and 'assetclass' as query parameters.");
        }

        if (dataType == "price.spot" && assetClass == "fx")
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            FxSpotPriceDataDto? requestData;
            
            try
            {
                requestData = System.Text.Json.JsonSerializer.Deserialize<FxSpotPriceDataDto>(requestBody, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (requestData == null)
                {
                    throw new ArgumentNullException(nameof(requestData));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing request body for fx spot price.");
                return new BadRequestObjectResult("Invalid request body.");
            }
            
            var result = await _repository.SaveAsync(FxSpotPriceDataMapper.ToDomain(requestData));
            return ProcessActionResult(result);
        }
        else if (dataType == "price.ordinal.spot" && assetClass == "crypto")
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            CryptoOrdinalSpotPriceDataDto? requestData;
            
            try
            {
                requestData = System.Text.Json.JsonSerializer.Deserialize<CryptoOrdinalSpotPriceDataDto>(requestBody, new System.Text.Json.JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                });

                if (requestData == null)
                {
                    throw new ArgumentNullException(nameof(requestData));
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deserializing request body for fx spot price.");
                return new BadRequestObjectResult("Invalid request body.");
            }
            
            var result = await _repository.SaveAsync(CryptoOrdinalSpotPriceDataMapper.ToDomain(requestData));
            return ProcessActionResult(result);
        }
        else
        {
            return new BadRequestObjectResult("Invalid request. Expected 'datatype' and 'assetclass' to be a valid combination.");
        }
    }

    private static IActionResult ProcessActionResult(SaveMarketDataResult result)
    {
        if (result.Success)
            return new OkObjectResult(result.Message != string.Empty
                ? result.Message
                : $"Document saved successfully to {result.Id}.");
            
        var message = result.Message != string.Empty ? result.Message : $"Error saving document to {result.Id}.";
        if (result.Exception != null)
            message += $" Exception: {result.Exception.Message}";
        return new BadRequestObjectResult(message);
    }
}