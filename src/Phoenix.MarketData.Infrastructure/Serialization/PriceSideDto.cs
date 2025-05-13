using System.Runtime.Serialization;

namespace Phoenix.MarketData.Infrastructure.Serialization;

public enum PriceSideDto
{
    [EnumMember(Value = "Mid")]
    Mid = 0,
    
    [EnumMember(Value = "Bid")]
    Bid = 1,
    
    [EnumMember(Value = "Ask")]
    Ask = 2,
}