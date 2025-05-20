using Phoenix.MarketData.Core;
using Phoenix.MarketData.Core.Models;
using Phoenix.MarketData.Infrastructure.Serialization;

namespace Phoenix.MarketData.Infrastructure.Mapping;

/// <summary>
/// Provides mapping functionality between the domain model
/// <see cref="CryptoOrdinalSpotPriceData"/> and its corresponding DTO
/// <see cref="CryptoOrdinalSpotPriceDataDto"/>.
/// </summary>
public static class CryptoOrdinalSpotPriceDataMapper
{
    /// <summary>
    /// Converts a <see cref="CryptoOrdinalSpotPriceData"/> domain model into a <see cref="CryptoOrdinalSpotPriceDataDto"/> data transfer object.
    /// </summary>
    /// <param name="domain">The domain model instance of type <see cref="CryptoOrdinalSpotPriceData"/> to be converted.</param>
    /// <returns>A <see cref="CryptoOrdinalSpotPriceDataDto"/> representation of the provided domain model.</returns>
    public static CryptoOrdinalSpotPriceDataDto ToDto(CryptoOrdinalSpotPriceData domain)
    {
        ArgumentNullException.ThrowIfNull(domain);

var dto = new CryptoOrdinalSpotPriceDataDto(
    id: domain.Id,
    schemaVersion: domain.SchemaVersion,
    version: domain.Version,
    assetId: domain.AssetId,
    assetClass: domain.AssetClass,
    dataType: domain.DataType,
    region: domain.Region,
    documentType: domain.DocumentType,
    createTimestamp: domain.CreateTimestamp,
    asOfDate: domain.AsOfDate,
    asOfTime: domain.AsOfTime,
    tags: domain.Tags,
    price: domain.Price,
    side: domain.Side,
    collectionName: domain.CollectionName,
    parentInscriptionId: domain.ParentInscriptionId,
    inscriptionId: domain.InscriptionId,
    inscriptionNumber: domain.InscriptionNumber,
    currency: domain.Currency
);

        return dto;
    }

    /// <summary>
    /// Converts a <see cref="CryptoOrdinalSpotPriceDataDto"/> data transfer object into a <see cref="CryptoOrdinalSpotPriceData"/> domain model.
    /// </summary>
    /// <param name="dto">The data transfer object instance of type <see cref="CryptoOrdinalSpotPriceDataDto"/> to be converted.</param>
    /// <returns>A <see cref="CryptoOrdinalSpotPriceData"/> representation of the provided data transfer object.</returns>
    public static CryptoOrdinalSpotPriceData ToDomain(CryptoOrdinalSpotPriceDataDto dto)
    {
        ArgumentNullException.ThrowIfNull(dto);

        var domain = new CryptoOrdinalSpotPriceData
        {
            SchemaVersion = dto.SchemaVersion,
            Version = dto.Version,
            AssetId = dto.AssetId,
            AssetClass = dto.AssetClass,
            DataType = dto.DataType,
            Region = dto.Region,
            DocumentType = dto.DocumentType,
            CreateTimestamp = dto.CreateTimestamp ?? DateTime.UtcNow,
            AsOfDate = dto.AsOfDate,
            AsOfTime = dto.AsOfTime,
            Price = dto.Price,
            Tags = dto.Tags?.ToList() ?? new List<string>(),
            Side = dto.Side switch
            {
                PriceSideDto.Mid => PriceSide.Mid,
                PriceSideDto.Bid => PriceSide.Bid,
                PriceSideDto.Ask => PriceSide.Ask,
                _ => PriceSide.Mid,
            },
            CollectionName = dto.CollectionName,
            ParentInscriptionId = dto.ParentInscriptionId,
            InscriptionId = dto.InscriptionId,
            InscriptionNumber = dto.InscriptionNumber,
            Currency = dto.Currency
        };

        return domain;
    }
}