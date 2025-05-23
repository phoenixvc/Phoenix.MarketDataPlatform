namespace Phoenix.MarketData.Domain.Models
{
    /// <summary>
    /// Interface for entities that support soft deletion
    /// </summary>
    public interface ISoftDeletable
    {
        /// <summary>
        /// Gets or sets a value indicating whether this entity has been logically deleted
        /// </summary>
        bool IsDeleted { get; set; }
    }
}