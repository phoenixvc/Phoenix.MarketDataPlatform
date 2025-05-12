using Phoenix.MarketData.Domain.Models;
using Phoenix.MarketData.Infrastructure.Serialization;

namespace Phoenix.MarketData.Infrastructure.Mapping
{
    public static class BaseMarketDataMapper
    {
        public static BaseMarketDataDto ToDto(BaseMarketData domain)
        {
            ArgumentNullException.ThrowIfNull(domain);
            
            return new BaseMarketDataDto
            {
                Id = domain.Id,
                SchemaVersion = domain.SchemaVersion,
                Version = domain.Version,
                AssetId = domain.AssetId,
                AssetClass = domain.AssetClass,
                DataType = domain.DataType,
                Region = domain.Region,
                DocumentType = domain.DocumentType,
                CreateTimestamp = domain.CreateTimestamp,
                AsOfDate = domain.AsOfDate,
                AsOfTime = domain.AsOfTime,
                Tags = domain.Tags
            };
        }

        public static void ApplyToDomain(BaseMarketDataDto dto, BaseMarketData domain)
        {
            ArgumentNullException.ThrowIfNull(dto);
            ArgumentNullException.ThrowIfNull(domain);

            domain.SchemaVersion = dto.SchemaVersion;
            domain.Version = dto.Version;
            domain.AssetId = dto.AssetId;
            domain.AssetClass = dto.AssetClass;
            domain.DataType = dto.DataType;
            domain.Region = dto.Region;
            domain.DocumentType = dto.DocumentType;
            domain.AsOfDate = dto.AsOfDate;
            domain.AsOfTime = dto.AsOfTime;
            domain.Tags = dto.Tags?.ToList() ?? new List<string>();
            domain.CreateTimestamp = dto.CreateTimestamp;
        }
    }
}