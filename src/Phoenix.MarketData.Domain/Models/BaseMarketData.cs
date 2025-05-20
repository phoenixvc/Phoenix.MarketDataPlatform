
using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Core.Models;

public abstract class BaseMarketData : IMarketDataEntity
{
    private string? _id;
    private DateTimeOffset? _createTimestamp;
    private int? _version;
    private string _schemaVersion = string.Empty;
    private string _assetId = string.Empty;
    private string _assetClass = string.Empty;
    private string _dataType = string.Empty;
    private string _region = string.Empty;
    private string _documentType = string.Empty;
    private DateOnly _asOfDate;

    public string Id => _id ??= CalculateId();

    public required string SchemaVersion
    {
        get => _schemaVersion;
        set
        {
            if (_schemaVersion != value)
                _schemaVersion = value.ToLowerInvariant();
        }
    }

    public int? Version
    {
        get => _version;
        set
        {
            if (_version != value)
            {
                _version = value;
                _id = null; // Invalidate for recalculation
            }
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
                _id = null;
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
                _id = null;
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
                _id = null;
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
                _id = null;
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
                _id = null;
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
                _id = null;
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
        get => _createTimestamp ??= DateTimeOffset.UtcNow;
        set => _createTimestamp = value;
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

        var id = string.Join("__", new[]
        {
            DataType, AssetClass, AssetId, Region, AsOfDate.ToString("yyyy-MM-dd"), DocumentType
        });

        if (Version != null)
            id += $"__{Version}";

        return id.ToLowerInvariant();
    }
}
