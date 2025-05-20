using System.Collections.Generic;
using System.Linq;

namespace Phoenix.MarketData.Core.Validation;

public class ValidationError
{
    public string? PropertyName { get; set; }
    public string ErrorMessage { get; set; }
    public string? ErrorCode { get; set; }
    public string? Source { get; set; }

    public override string ToString()
        => string.IsNullOrWhiteSpace(PropertyName)
            ? ErrorMessage
                : $"{PropertyName}: {ErrorMessage}";

    public ValidationError()
    {
        ErrorMessage = string.Empty;
    }
}
