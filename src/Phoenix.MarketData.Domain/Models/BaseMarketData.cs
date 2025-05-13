using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Domain.Models;

/// <summary>
/// Represents the base implementation of a market data object, providing core properties
/// and functionality for identifying and managing market data records.
/// </summary>
public abstract class BaseMarketData : IMarketDataObject
{
    private string? _id; // Backing field for Id
    private DateTimeOffset? _createTimeStamp; // Backing field for CreateTimestamp
    private int? _version;

    public string Id
    {
        get => _id ??= CalculateId(); // If not set, calculate it
        private set => _id = value;  // Can only be set during deserialization
    }

    public required string SchemaVersion { get; set; }

    public int? Version
    {
        get => _version;
        set
        {
            if (_version == value)
                return;
            
            _version = value;

            // Invalidate the ID if the Version changes
            _id = CalculateId();
        }
    }

    public required string AssetId { get; set; }

    public required string AssetClass { get; set; }

    public required string DataType { get; set; }

    public required string Region { get; set; }

    private List<string> _tags = new();
    public List<string> Tags 
    { 
        get => _tags;
        set => _tags = value?.ToList() ?? new List<string>();
    }

    public required string DocumentType { get; set; }

    public DateTimeOffset CreateTimestamp
    {
        get => _createTimeStamp ??= DateTimeOffset.UtcNow;
        set => _createTimeStamp = value;
    }

    public required DateOnly AsOfDate { get; set; }
    
    public TimeOnly? AsOfTime { get; set; }

    private string CalculateId()
    {
        // Validate required properties
        if (string.IsNullOrEmpty(DataType) || string.IsNullOrEmpty(AssetClass) || string.IsNullOrEmpty(AssetId) || 
            string.IsNullOrEmpty(Region) || string.IsNullOrEmpty(DocumentType))
        {
            throw new InvalidOperationException("Cannot calculate ID: one or more required properties are null or empty.");
        }
        
        var id = string.Join("__", new[] {
            DataType, AssetClass, AssetId, Region, AsOfDate.ToString("yyyyMMdd"), DocumentType});
        if (Version != null)
            id += $"__{Version}";

        return id;
    }
}