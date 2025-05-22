using Phoenix.MarketData.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Phoenix.MarketData.Core.Models;

public abstract class BaseMarketData : IMarketDataEntity, ISoftDeletable
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
    private string _displayAssetId = string.Empty;
    private List<string> _tags = new();

    // Set of property names that affect ID calculation
    private static readonly HashSet<string> IdDependentProperties = new(StringComparer.OrdinalIgnoreCase)
    {
        nameof(SchemaVersion),
        nameof(Version),
        nameof(AssetId),
        nameof(AssetClass),
        nameof(DataType),
        nameof(Region),
        nameof(DocumentType),
        nameof(AsOfDate)
    };

    /// <summary>
    /// Centralized property change handler that automatically invalidates ID when necessary
    /// </summary>
    /// <typeparam name="T">Type of the property</typeparam>
    /// <param name="storage">Reference to the backing field</param>
    /// <param name="value">New value to set</param>
    /// <param name="propertyName">Name of the property being changed</param>
    /// <param name="transform">Optional transformation to apply to the value before storing</param>
    /// <returns>True if the value was changed, false otherwise</returns>
    protected bool SetProperty<T>(ref T storage, T value, string propertyName, Func<T, T>? transform = null)
    {
        if (EqualityComparer<T>.Default.Equals(storage, value))
            return false;

        // Apply transformation if provided
        if (transform != null)
            value = transform(value);

        storage = value;

        // Invalidate ID if this property affects ID calculation
        if (IdDependentProperties.Contains(propertyName))
            _id = null;

        return true;
    }

    public string Id => _id ??= CalculateId();

    public required string SchemaVersion
    {
        get => _schemaVersion;
        set => SetProperty(ref _schemaVersion, value, nameof(SchemaVersion), v => v.ToLowerInvariant());
    }

    public int? Version
    {
        get => _version;
        set => SetProperty(ref _version, value, nameof(Version));
    }

    public required string AssetId
    {
        get => _assetId;
        set => SetProperty(ref _assetId, value, nameof(AssetId), v => v.ToLowerInvariant());
    }

    /// <summary>
    /// Gets or sets the display version of the asset ID with preserved case formatting.
    /// This is used for UI/reporting but not for ID calculation or matching.
    /// </summary>
    public string DisplayAssetId
    {
        get => string.IsNullOrEmpty(_displayAssetId) ? _assetId : _displayAssetId;
        set => SetProperty(ref _displayAssetId, value, nameof(DisplayAssetId));
    }

    public required string AssetClass
    {
        get => _assetClass;
        set => SetProperty(ref _assetClass, value, nameof(AssetClass), v => v.ToLowerInvariant());
    }

    public required string DataType
    {
        get => _dataType;
        set => SetProperty(ref _dataType, value, nameof(DataType), v => v.ToLowerInvariant());
    }

    public required string Region
    {
        get => _region;
        set => SetProperty(ref _region, value, nameof(Region), v => v.ToLowerInvariant());
    }

    public required string DocumentType
    {
        get => _documentType;
        set => SetProperty(ref _documentType, value, nameof(DocumentType), v => v.ToLowerInvariant());
    }

    public required DateOnly AsOfDate
    {
        get => _asOfDate;
        set => SetProperty(ref _asOfDate, value, nameof(AsOfDate));
    }

    /// <summary>
    /// Gets a read-only view of the tags collection.
    /// Implements IMarketDataEntityBase.Tags
    /// </summary>
    public IReadOnlyList<string> Tags => _tags.AsReadOnly();

    /// <summary>
    /// Sets the tags collection.
    /// </summary>
    /// <param name="tags">The collection of tags to set</param>
    public void SetTags(IEnumerable<string> tags)
    {
        _tags = tags?.ToList() ?? new List<string>();
    }

    /// <summary>
    /// Adds a tag to the collection if it doesn't already exist.
    /// </summary>
    /// <param name="tag">The tag to add</param>
    /// <returns>True if the tag was added, false if it already existed</returns>
    public bool AddTag(string tag)
    {
        if (string.IsNullOrWhiteSpace(tag))
            return false;

        if (!_tags.Contains(tag))
        {
            _tags.Add(tag);
            return true;
        }
        return false;
    }

    /// <summary>
    /// Removes a tag from the collection if it exists.
    /// </summary>
    /// <param name="tag">The tag to remove</param>
    /// <returns>True if the tag was removed, false if it didn't exist</returns>
    public bool RemoveTag(string tag)
    {
        return _tags.Remove(tag);
    }

    public DateTimeOffset CreateTimestamp
    {
        get => _createTimestamp ??= DateTimeOffset.UtcNow;
        set => SetProperty(ref _createTimestamp, value, nameof(CreateTimestamp));
    }

    public TimeOnly? AsOfTime { get; set; }
    public bool IsDeleted { get; set; }

    private string CalculateId()
    {
        // Validate required properties
        if (string.IsNullOrEmpty(DataType)
            || string.IsNullOrEmpty(AssetClass)
            || string.IsNullOrEmpty(AssetId)
            || string.IsNullOrEmpty(Region)
            || string.IsNullOrEmpty(DocumentType)
            || AsOfDate == default
            || string.IsNullOrEmpty(SchemaVersion))
        {
            return string.Empty;
        }

        var id = string.Join("__", new[]
        {
            DataType, AssetClass, AssetId, Region, AsOfDate.ToString("yyyy-MM-dd"), DocumentType, SchemaVersion
        });

        if (Version != null)
            id += $"__{Version}";

        return id.ToLowerInvariant();
    }
}