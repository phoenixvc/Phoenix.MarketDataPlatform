using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Domain.Models;

/// <summary>
/// Represents the base implementation of a market data object, providing core properties
/// and functionality for identifying and managing market data records.
/// </summary>
public abstract class BaseMarketData : IMarketData
{
    private string? _id; // Backing field for Id
    private DateTimeOffset? _createTimeStamp; // Backing field for CreateTimestamp
    private int? _version;
    private string _schemaVersion = string.Empty;
    private string _assetId = string.Empty;
    private string _assetClass = string.Empty;
    private string _dataType = string.Empty;
    private string _region = string.Empty;
    private string _documentType = string.Empty;
    private DateOnly _asOfDate;

    public string Id => _id ??= CalculateId(); // If not set, calculate it

    public required string SchemaVersion
    {
        get => _schemaVersion;
        set
        {
            if (_schemaVersion == value)
                return;
            
            _schemaVersion = value.ToLowerInvariant();
        }
    }

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
    
    public required string AssetId 
    { 
        get => _assetId; 
        set
        {
            if (_assetId != value)
            {
                _assetId = value.ToLowerInvariant();
                _id = CalculateId();
            }
        }
    }

    public required string AssetClass 
    { 
        get => _assetClass; 
        set
        {
            if (_assetClass != value)
            {
                _assetClass = value.ToLowerInvariant();
                _id = CalculateId();
            }
        }
    }

    public required string DataType 
    { 
        get => _dataType; 
        set
        {
            if (_dataType != value)
            {
                _dataType = value.ToLowerInvariant();
                _id = CalculateId();
            }
        }
    }

    public required string Region 
    { 
        get => _region; 
        set
        {
            if (_region != value)
            {
                _region = value.ToLowerInvariant();
                _id = CalculateId();
            }
        }
    }
    
    public required string DocumentType 
    { 
        get => _documentType; 
        set
        {
            if (_documentType != value)
            {
                _documentType = value.ToLowerInvariant();
                _id = CalculateId();
            }
        }
    }

    public required DateOnly AsOfDate 
    { 
        get => _asOfDate; 
        set
        {
            if (_asOfDate != value)
            {
                _asOfDate = value;
                _id = CalculateId();
            }
        }
    }
    
    private List<string> _tags = new();
    public List<string> Tags 
    { 
        get => _tags;
        set => _tags = value?.ToList() ?? new List<string>();
    }

    public DateTimeOffset CreateTimestamp
    {
        get => _createTimeStamp ??= DateTimeOffset.UtcNow;
        set => _createTimeStamp = value;
    }
    
    public TimeOnly? AsOfTime { get; set; }

    private string CalculateId()
    {
        // Validate required properties
        if (string.IsNullOrEmpty(DataType) 
            || string.IsNullOrEmpty(AssetClass) 
            || string.IsNullOrEmpty(AssetId) 
            || string.IsNullOrEmpty(Region) 
            || string.IsNullOrEmpty(DocumentType) 
            || AsOfDate == default)
        {
            return string.Empty;
        }

        var id = string.Join("__", new[] { DataType, AssetClass, AssetId, Region, AsOfDate.ToString("yyyy-MM-dd"), DocumentType });
        if (Version != null)
            id += $"__{Version}";

        id = id.ToLowerInvariant();
        return id;
    }
}