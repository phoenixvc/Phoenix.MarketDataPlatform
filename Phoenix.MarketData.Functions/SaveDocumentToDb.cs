using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Phoenix.MarketData.Infrastructure.Cosmos;
using Phoenix.MarketData.Infrastructure.Mapping;
using Phoenix.MarketData.Infrastructure.Schemas;
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
        var schemaVersion = req.Query["schemaversion"];

        if (string.IsNullOrWhiteSpace(dataType) || string.IsNullOrWhiteSpace(assetClass) || string.IsNullOrEmpty("schemaversion"))
        {
            return new BadRequestObjectResult("Please provide 'datatype', 'assetclass' and 'schema' as query parameters.");
        }

        if (dataType == "price.spot" && assetClass == "fx" && !string.IsNullOrEmpty(schemaVersion))
        {
            var requestBody = await new StreamReader(req.Body).ReadToEndAsync();
            FxSpotPriceDataDto? requestData;
            
            try
            {
                // validate
                var validated = JsonSchemaValidatorRegistry.Validator.Validate(dataType!, assetClass!, schemaVersion!, requestBody, out var errorMessage);
                if (!validated)
                {
                    _logger.LogError($"Error validating request body for fx spot price against schema. {errorMessage}");
                    return new BadRequestObjectResult("Could not validate request body against schema.");
                }
                
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
                _logger.LogError(ex, $"Error deserializing request body for fx spot price. {ex.Message}");
                return new BadRequestObjectResult("Invalid request body.");
            }

            var data = FxSpotPriceDataMapper.ToDomain(requestData);
            var result = await _repository.SaveMarketDataAsync(data);
            return ProcessActionResult(result);
        }

        if (dataType == "price.ordinals.spot" && assetClass == "crypto" && schemaVersion != string.Empty)
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
            
            var result = await _repository.SaveMarketDataAsync(CryptoOrdinalSpotPriceDataMapper.ToDomain(requestData));
            return ProcessActionResult(result);
        }
        else
        {
            return new BadRequestObjectResult("Invalid request. Expected 'datatype' and 'assetclass' to be a valid combination.");
        }
    }

    private IActionResult ProcessActionResult(SaveMarketDataResult result)
    {
        if (result.Success)
        {
            var msg = result.Message != string.Empty
                ? result.Message
                : $"Document saved successfully to {result.Id}.";
            _logger.LogInformation(msg);
            return new OkObjectResult(msg);
        }
            
        var message = result.Message != string.Empty ? result.Message : $"Error saving document to {result.Id}.";
        if (result.Exception != null)
            message += $" Exception: {result.Exception.Message}";
        _logger.LogError(message);
        return new BadRequestObjectResult(message);
    }
}