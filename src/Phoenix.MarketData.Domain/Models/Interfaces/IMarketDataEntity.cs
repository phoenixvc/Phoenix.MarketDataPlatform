namespace Phoenix.MarketData.Domain.Models
{
    public interface IEntity
    {
        string Id { get; }
    }

    public interface IMarketDataEntity : IEntity
    {
        string SchemaVersion { get; set; }
        int? Version { get; set; }
        string AssetId { get; set; }
        string AssetClass { get; set; }
        string DataType { get; set; }
        string Region { get; set; }
        List<string> Tags { get; set; }
        string DocumentType { get; set; }
        DateTimeOffset CreateTimestamp { get; }
        DateOnly AsOfDate { get; set; }
        TimeOnly? AsOfTime { get; set; }
    }

    // For strict versioning (non-null version)
    public interface IVersionedMarketDataEntity : IMarketDataEntity
    {
        new int Version { get; set; }
    }
}
