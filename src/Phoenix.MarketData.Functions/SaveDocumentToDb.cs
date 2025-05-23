using Microsoft.Azure.Functions.Worker;
using Microsoft.Azure.Functions.Worker.Http;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Phoenix.MarketData.Infrastructure.Repositories;
using Phoenix.MarketData.Infrastructure.Mapping;
using Phoenix.MarketData.Infrastructure.Schemas;
using Phoenix.MarketData.Infrastructure.Serialization;
using Phoenix.MarketData.Domain.Models;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Net;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.OpenApi.Models;
using Phoenix.MarketData.Functions.OpenApi;
using System.ComponentModel.DataAnnotations;

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
    [OpenApiOperation(operationId: "SaveDocumentToDb", tags: new[] { "Market Data" },
                     Summary = "Save market data to the database",
                     Description = "Saves a market data document to the database based on the data type and asset class")]
    [OpenApiParameter(name: "datatype", In = ParameterLocation.Query, Required = true, Type = typeof(string),
                     Description = "The type of market data (e.g., price.spot, price.ordinals.spot)")]
    [OpenApiParameter(name: "assetclass", In = ParameterLocation.Query, Required = true, Type = typeof(string),
                     Description = "The asset class (e.g., fx, crypto)")]
    [OpenApiParameter(name: "schemaversion", In = ParameterLocation.Query, Required = true, Type = typeof(string),
                     Description = "The version of the schema to use for validation")]
    [OpenApiRequestBody(contentType: "application/json", bodyType: typeof(object),
                     Description = "Market data document to save",
                     Example = typeof(FxSpotPriceDataExample))]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.OK, contentType: "application/json", bodyType: typeof(string),
                     Description = "Document saved successfully")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.BadRequest, contentType: "application/json", bodyType: typeof(string),
                     Description = "Bad request, validation error")]
    [OpenApiResponseWithBody(statusCode: HttpStatusCode.InternalServerError, contentType: "application/json", bodyType: typeof(string),
                     Description = "Server error")]
    public async Task<IActionResult> Run([HttpTrigger(AuthorizationLevel.Function, "post", Route = "SaveDocumentToDb")] HttpRequest req)
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
            return await HandleDocumentAsync<FxSpotPriceDataDto>(req, dataType, assetClass, schemaVersion, FxSpotPriceDataMapper.MapToDomain);
        }

        if (dataType == "price.ordinals.spot" && assetClass == "crypto")
        {
            return await HandleDocumentAsync<CryptoOrdinalSpotPriceDataDto>(req, dataType, assetClass, schemaVersion, CryptoOrdinalSpotPriceDataMapper.MapToDomain);
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

        // Add error handling around domain mapping
        IMarketDataEntity domainData;
        try
        {
            domainData = toDomainMapper(requestData);

            if (domainData == null)
            {
                _logger.LogError($"Domain mapping returned null for {dataType}/{assetClass}.");
                return new BadRequestObjectResult("Error processing market data: Mapping failed.");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error mapping DTO to domain object for {dataType}/{assetClass}.");
            return new ObjectResult("Error processing market data: Invalid format.")
            {
                StatusCode = (int)HttpStatusCode.BadRequest
            };
        }

        // Add error handling around repository save operation
        IMarketDataEntity result;
        try
        {
            result = await _repository.AddAsync(domainData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error saving {dataType}/{assetClass} data to repository.");

            // Refined error handling: treat only known validation/argument errors as client errors
            if (ex is ValidationException ||
                (ex is ArgumentException argEx && argEx.ParamName != null && argEx.ParamName.StartsWith("input", StringComparison.OrdinalIgnoreCase)))
            {
                return new BadRequestObjectResult($"Invalid data: {ex.Message}");
            }

            // All other exceptions are server errors
            return new ObjectResult("Error saving document to database.")
            {
                StatusCode = (int)HttpStatusCode.InternalServerError
            };
        }

        return ProcessActionResult(result);
    }

    // Example result handler
    private IActionResult ProcessActionResult(IMarketDataEntity? result)
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