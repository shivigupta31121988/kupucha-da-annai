namespace Kupucha.Api.Models;

public record Stock
{
    public string Symbol { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal MarketRate { get; init; }
    public decimal Yesterday { get; init; }
    public decimal Predicted { get; init; }
}
