using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Attributes;
using Microsoft.Azure.WebJobs.Extensions.OpenApi.Core.Enums;
using Microsoft.OpenApi.Models;
using Newtonsoft.Json.Serialization;
using System;
using System.Collections.Generic;

namespace Phoenix.MarketData.Functions.OpenApi
{
    /// <summary>
    /// Model representing FX spot price data for OpenAPI documentation
    /// </summary>
    public class FxSpotPriceDataModel
    {
        [OpenApiProperty(Description = "The spot price of the FX pair")]
        public double Price { get; set; }

        [OpenApiProperty(Description = "The side of the price (bid, ask, or mid)")]
        public string? Side { get; set; }

        [OpenApiProperty(Description = "The version of the schema being used")]
        public string? SchemaVersion { get; set; }

        [OpenApiProperty(Description = "The unique identifier of the asset")]
        public string? AssetId { get; set; }

        [OpenApiProperty(Description = "The asset class (fx, crypto, etc.)")]
        public string? AssetClass { get; set; }

        [OpenApiProperty(Description = "The type of data being represented")]
        public string? DataType { get; set; }

        [OpenApiProperty(Description = "The region applicable for the market data")]
        public string? Region { get; set; }

        [OpenApiProperty(Description = "Tags associated with the data for categorization")]
        public List<string>? Tags { get; set; }

        [OpenApiProperty(Description = "The type of document (official, intraday, etc.)")]
        public string? DocumentType { get; set; }

        [OpenApiProperty(Description = "The effective date of the data in YYYY-MM-DD format")]
        public string? AsOfDate { get; set; }

        [OpenApiProperty(Description = "The specific time corresponding to when the market data is relevant in HH:MM:SS format")]
        public string? AsOfTime { get; set; }
    }

    /// <summary>
    /// Model representing crypto ordinal spot price data for OpenAPI documentation
    /// </summary>
    public class CryptoOrdinalSpotPriceDataModel
    {
        [OpenApiProperty(Description = "The spot price of the ordinal")]
        public double Price { get; set; }

        [OpenApiProperty(Description = "The currency in which the price is denominated")]
        public string? Currency { get; set; }

        [OpenApiProperty(Description = "The side of the price (bid, ask, or mid)")]
        public string? Side { get; set; }

        [OpenApiProperty(Description = "The version of the schema being used")]
        public string? SchemaVersion { get; set; }

        [OpenApiProperty(Description = "The unique identifier of the asset")]
        public string? AssetId { get; set; }

        [OpenApiProperty(Description = "The asset class (fx, crypto, etc.)")]
        public string? AssetClass { get; set; }

        [OpenApiProperty(Description = "The type of data being represented")]
        public string? DataType { get; set; }

        [OpenApiProperty(Description = "The region applicable for the market data")]
        public string? Region { get; set; }

        [OpenApiProperty(Description = "Tags associated with the data for categorization")]
        public List<string>? Tags { get; set; }

        [OpenApiProperty(Description = "The type of document (official, intraday, etc.)")]
        public string? DocumentType { get; set; }

        [OpenApiProperty(Description = "The effective date of the data in YYYY-MM-DD format")]
        public string? AsOfDate { get; set; }

        [OpenApiProperty(Description = "The ordinal inscription number")]
        public int InscriptionNumber { get; set; }

        [OpenApiProperty(Description = "The unique transaction+output identifier")]
        public string? InscriptionId { get; set; }

        [OpenApiProperty(Description = "The parent inscription identifier")]
        public string? ParentInscriptionId { get; set; }

        [OpenApiProperty(Description = "The name of the collection")]
        public string? CollectionName { get; set; }
    }

    /// <summary>
    /// Example data for FX spot price documentation
    /// </summary>
    public class FxSpotPriceDataExample
    {
        public double price { get; set; } = 1.09;
        public string? side { get; set; } = "mid";
        public string? schemaVersion { get; set; } = "1.0.0";
        public string? assetId { get; set; } = "eurusd";
        public string? assetClass { get; set; } = "fx";
        public string? dataType { get; set; } = "price.spot";
        public string? region { get; set; } = "ny";
        public List<string>? tags { get; set; } = new List<string> { "spot" };
        public string? documentType { get; set; } = "official";
        public string? asOfDate { get; set; } = "2025-05-13";
        public string? asOfTime { get; set; } = "15:30:05";
    }

    /// <summary>
    /// Example data for crypto ordinal spot price documentation
    /// </summary>
    public class CryptoOrdinalSpotPriceDataExample
    {
        public double price { get; set; } = 1.13;
        public string? currency { get; set; } = "btc";
        public string? side { get; set; } = "bid";
        public string? schemaVersion { get; set; } = "1.0.0";
        public string? assetId { get; set; } = "quantum_cats_1";
        public string? assetClass { get; set; } = "crypto";
        public string? dataType { get; set; } = "price.ordinals.spot";
        public string? region { get; set; } = "global";
        public List<string>? tags { get; set; } = new List<string> { "spot" };
        public string? documentType { get; set; } = "official";
        public string? asOfDate { get; set; } = "2025-05-13";
        public int inscriptionNumber { get; set; } = 77777;
        public string? inscriptionId { get; set; } = "12345";
        public string? parentInscriptionId { get; set; } = "1234";
        public string? collectionName { get; set; } = "quantum cats";
    }
}