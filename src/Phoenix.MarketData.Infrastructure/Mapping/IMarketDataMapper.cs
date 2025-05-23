using Phoenix.MarketData.Domain.Models;

namespace Phoenix.MarketData.Infrastructure.Mapping
{
    public interface IMarketDataMapper<TDomain, TDto>
        where TDomain : IMarketDataEntity
        where TDto : class
    {
        TDto ToDto(TDomain domain);
        TDomain ToDomain(TDto dto);
    }
}