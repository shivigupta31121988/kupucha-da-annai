using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc;
using RabbitMQ.Client;
using Kupucha.Api.Models;

namespace Kupucha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class StocksController : ControllerBase
{
    [HttpGet]
    public IActionResult GetStocks()
    {
        var stocks = InMemoryStore.Stocks.Values.Select(s => new {
            s.Symbol, s.Name, MarketRate = s.MarketRate, Yesterday = s.Yesterday, Predicted = s.Predicted
        });
        return Ok(stocks);
    }

    [HttpGet("{symbol}")]
    public IActionResult GetStock(string symbol)
    {
        if (InMemoryStore.Stocks.TryGetValue(symbol.ToUpperInvariant(), out var s)) return Ok(s);
        return NotFound();
    }

    [HttpPost]
    public IActionResult CreateStock([FromBody] StockCreateRequest req)
    {
        var key = req.Symbol.ToUpperInvariant();
        var stock = new Stock { Symbol = key, Name = req.Name, MarketRate = req.MarketRate, Yesterday = req.Yesterday, Predicted = req.Predicted };
        InMemoryStore.Stocks[key] = stock;
        return CreatedAtAction(nameof(GetStock), new { symbol = key }, stock);
    }

    [HttpPut("{symbol}")]
    public IActionResult UpdateStock(string symbol, [FromBody] StockUpdateRequest req)
    {
        var key = symbol.ToUpperInvariant();
        if (!InMemoryStore.Stocks.TryGetValue(key, out var existing)) return NotFound();
        existing = existing with { Name = req.Name ?? existing.Name, MarketRate = req.MarketRate ?? existing.MarketRate, Yesterday = req.Yesterday ?? existing.Yesterday, Predicted = req.Predicted ?? existing.Predicted };
        InMemoryStore.Stocks[key] = existing;
        return Ok(existing);
    }
}

public record StockCreateRequest
{
    public string Symbol { get; init; } = string.Empty;
    public string Name { get; init; } = string.Empty;
    public decimal MarketRate { get; init; }
    public decimal Yesterday { get; init; }
    public decimal Predicted { get; init; }
}

public record StockUpdateRequest
{
    public string? Name { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? MarketRate { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Yesterday { get; init; }
    [JsonIgnore(Condition = JsonIgnoreCondition.WhenWritingNull)]
    public decimal? Predicted { get; init; }
}
