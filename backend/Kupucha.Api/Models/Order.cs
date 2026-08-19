namespace Kupucha.Api.Models;

public record Order
{
    public Guid Id { get; init; } = Guid.NewGuid();
    public string Symbol { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal? Price { get; init; }
    public string Side { get; init; } = "buy";
    public string Status { get; init; } = "scheduled"; // scheduled, executed
    public DateTime CreatedAt { get; init; } = DateTime.UtcNow;
}
