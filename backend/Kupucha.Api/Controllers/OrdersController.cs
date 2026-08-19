using Microsoft.AspNetCore.Mvc;
using Kupucha.Api.Models;
using RabbitMQ.Client;

namespace Kupucha.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    [HttpGet]
    public IActionResult GetOrders()
    {
        return Ok(InMemoryStore.Orders.Values);
    }

    [HttpPost]
    public IActionResult CreateOrder([FromBody] OrderRequest req)
    {
        var order = new Order { Symbol = req.Symbol.ToUpperInvariant(), Quantity = req.Quantity, Price = req.Price, Side = req.Side };

        // If market open, execute immediately (mock)
        var utc = DateTime.UtcNow.TimeOfDay;
        var open = new TimeSpan(9, 30, 0);
        var close = new TimeSpan(16, 0, 0);
        var marketOpen = utc >= open && utc <= close;

        if (marketOpen)
        {
            order = order with { Status = "executed" };
            InMemoryStore.Orders[order.Id] = order;
            return Accepted(new { status = "executed", id = order.Id });
        }

        InMemoryStore.Orders[order.Id] = order with { Status = "scheduled" };

        // publish to RabbitMQ for worker consumption
        try
        {
            var factory = new ConnectionFactory() { HostName = Environment.GetEnvironmentVariable("RABBIT_HOST") ?? "rabbitmq" };
            using var conn = factory.CreateConnection();
            using var channel = conn.CreateModel();
            channel.QueueDeclare(queue: "orders", durable: true, exclusive: false, autoDelete: false, arguments: null);
            var body = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes(order);
            var props = channel.CreateBasicProperties();
            props.Persistent = true;
            channel.BasicPublish(exchange: "", routingKey: "orders", basicProperties: props, body: body);
        }
        catch
        {
            // ignore publish errors for demo
        }

        return CreatedAtAction(nameof(GetOrders), new { id = order.Id }, order);
    }

    [HttpPut("{id}")]
    public IActionResult UpdateOrder(Guid id, [FromBody] OrderUpdateRequest req)
    {
        if (!InMemoryStore.Orders.TryGetValue(id, out var existing)) return NotFound();
        if (existing.Status == "executed") return BadRequest(new { error = "Order already executed" });

        var updated = existing with { Quantity = req.Quantity ?? existing.Quantity, Price = req.Price ?? existing.Price };
        InMemoryStore.Orders[id] = updated;

        return Ok(updated);
    }

    [HttpPost("{id}/cancel")]
    public IActionResult CancelOrder(Guid id)
    {
        if (!InMemoryStore.Orders.TryGetValue(id, out var existing)) return NotFound();
        if (existing.Status == "executed") return BadRequest(new { error = "Order already executed" });

        var updated = existing with { Status = "cancelled" };
        InMemoryStore.Orders[id] = updated;

        return Ok(updated);
    }
}

public record OrderRequest
{
    public string Symbol { get; init; } = string.Empty;
    public int Quantity { get; init; }
    public decimal? Price { get; init; }
    public string Side { get; init; } = "buy";
}

public record OrderUpdateRequest
{
    public int? Quantity { get; init; }
    public decimal? Price { get; init; }
}
