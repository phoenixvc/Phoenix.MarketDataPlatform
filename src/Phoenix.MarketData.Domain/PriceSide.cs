﻿using System.ComponentModel;

namespace Phoenix.MarketData.Domain;

public enum PriceSide
{
    [Description("Mid")]
    Mid = 0,
    
    [Description("Bid")]
    Bid = 1,
    
    [Description("Ask")]
    Ask = 2,
}