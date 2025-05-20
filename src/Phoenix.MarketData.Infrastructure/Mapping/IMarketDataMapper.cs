using Phoenix.MarketData.Domain.Models.Interfaces;

namespace Phoenix.MarketData.Infrastructure.Mapping
{
    public interface IMarketDataMapper<TDomain, TDto>
        where TDomain : IMarketData
        where TDto : class
    {
        TDto ToDto(TDomain domain);
        TDomain ToDomain(TDto dto);
    }
}