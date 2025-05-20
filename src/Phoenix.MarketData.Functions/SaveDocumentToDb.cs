using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Phoenix.MarketData.Infrastructure.Repositories; // New repo
using Phoenix.MarketData.Infrastructure.Mapping;
using Phoenix.MarketData.Infrastructure.Schemas;
using Phoenix.MarketData.Infrastructure.Serialization;
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Functions;

public class SaveDocumentToDb
{
    private readonly ILogger<SaveDocumentToDb> _logger;
    private readonly CosmosRepository<IMarketDataEntity> _repository;

    public SaveDocumentToDb(ILogger<SaveDocumentToDb> logger, CosmosRepository<IMarketDataEntity> repository)
    {
        _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    [Function("SaveDocumentToDb")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "get", "post")] HttpRequest req)
    {
        _logger.LogInformation("C# HTTP trigger function processed a request.");

        string? dataType = req.Query["datatype"];
        string? assetClass = req.Query["assetclass"];
        string? schemaVersion = req.Query["schemaversion"];

        if (string.IsNullOrWhiteSpace(dataType) || string.IsNullOrWhiteSpace(assetClass) || string.IsNullOrWhiteSpace(schemaVersion))
        {
            return new BadRequestObjectResult("Please provide 'datatype', 'assetclass' and 'schemaversion' as query parameters.");
        }

        // Validate/dispatch per combination
        if (dataType == "price.spot" && assetClass == "fx")
        {
            return await HandleDocumentAsync<FxSpotPriceDataDto>(req, dataType, assetClass, schemaVersion, FxSpotPriceDataMapper.ToDomain);
        }

        if (dataType == "price.ordinals.spot" && assetClass == "crypto")
        {
            return await HandleDocumentAsync<CryptoOrdinalSpotPriceDataDto>(req, dataType, assetClass, schemaVersion, CryptoOrdinalSpotPriceDataMapper.ToDomain);
        }

        return new BadRequestObjectResult("Invalid request. Expected 'datatype' and 'assetclass' to be a valid combination.");
    }

    // DRY handler for each DTO type
    private async Task<IActionResult> HandleDocumentAsync<TDto>(
        HttpRequest req,
        string dataType,
        string assetClass,
        string schemaVersion,
        Func<TDto, IMarketDataEntity> toDomainMapper)
    {
        string requestBody = await new StreamReader(req.Body).ReadToEndAsync();

        // Schema validation
        var validated = JsonSchemaValidatorRegistry.Validator.Validate(dataType, assetClass, schemaVersion, requestBody, out var errorMessage);
        if (!validated)
        {
            _logger.LogError($"Schema validation failed for {dataType}/{assetClass}: {errorMessage}");
            return new BadRequestObjectResult("Could not validate request body against schema.");
        }

        TDto? requestData;
        try
        {
            requestData = System.Text.Json.JsonSerializer.Deserialize<TDto>(requestBody, new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            if (requestData == null)
                throw new ArgumentNullException(nameof(requestData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deserializing request body for {dataType}/{assetClass}.");
            return new BadRequestObjectResult("Invalid request body.");
        }

        var domainData = toDomainMapper(requestData);
        var result = await _repository.AddAsync(domainData);
        return ProcessActionResult(result);
    }

    // Example result handler; you may want to adapt if SaveMarketDataResult differs from repo result
    private IActionResult ProcessActionResult(IMarketDataEntity result)
    {
        if (result != null)
        {
            var msg = $"Document saved successfully to {result.Id}.";
            _logger.LogInformation(msg);
            return new OkObjectResult(msg);
        }
        _logger.LogError("Error saving document (null result).");
        return new BadRequestObjectResult("Error saving document.");
    }
}
