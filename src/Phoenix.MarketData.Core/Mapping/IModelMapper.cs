using System.Collections.Generic;

namespace Phoenix.MarketData.Core.Mapping
{
    /// <summary>
    /// Defines mapping capabilities between different object types
    /// </summary>
    public interface IModelMapper
    {
        /// <summary>
        /// Maps from source object to a new destination object
        /// </summary>
        TDestination Map<TSource, TDestination>(TSource source);

        /// <summary>
        /// Maps from source object to an existing destination object
        /// </summary>
        void Map<TSource, TDestination>(TSource source, TDestination destination);

        /// <summary>
        /// Maps a collection of source objects to a collection of new destination objects
        /// </summary>
        IEnumerable<TDestination> MapCollection<TSource, TDestination>(IEnumerable<TSource> sources);
    }
}