using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using Kupucha.Api.Models;

Console.WriteLine("Kupucha.Worker starting - will consume scheduled orders (demo)...");

var factory = new ConnectionFactory() { HostName = Environment.GetEnvironmentVariable("RABBIT_HOST") ?? "rabbitmq" };
try
{
    using var connection = factory.CreateConnection();
    using var channel = connection.CreateModel();
    channel.QueueDeclare(queue: "orders", durable: true, exclusive: false, autoDelete: false, arguments: null);

    var consumer = new EventingBasicConsumer(channel);
    consumer.Received += (model, ea) =>
    {
        var body = ea.Body.ToArray();
        var msg = Encoding.UTF8.GetString(body);
        Console.WriteLine($"[Worker] Received scheduled order: {msg}");

        // Simple market-open check (demo): only execute if between 09:30-16:00 UTC
        var utc = DateTime.UtcNow.TimeOfDay;
        var open = new TimeSpan(9, 30, 0);
        var close = new TimeSpan(16, 0, 0);
        var marketOpen = utc >= open && utc <= close;

        try
        {
            var order = System.Text.Json.JsonSerializer.Deserialize<Order>(msg);
            if (order == null)
            {
                Console.WriteLine("[Worker] Failed to deserialize order");
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
                return;
            }

            if (marketOpen)
            {
                Console.WriteLine($"[Worker] Executing order: {order.Id} {order.Symbol} x{order.Quantity}");
                // mark executed in InMemoryStore if present
                if (InMemoryStore.Orders.TryGetValue(order.Id, out var existing))
                {
                    var executed = existing with { Status = "executed", ExecutedAt = DateTime.UtcNow, ExecutedPrice = order.Price ?? existing.Price };
                    InMemoryStore.Orders[order.Id] = executed;
                    Console.WriteLine($"[Worker] Order {order.Id} marked executed.");
                }

                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            }
            else
            {
                Console.WriteLine($"[Worker] Market closed, requeueing: {order.Id}");
                channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
                Thread.Sleep(1000);
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"[Worker] Error processing message: {ex.Message}");
            // ack to drop bad message
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
    };

    channel.BasicConsume(queue: "orders", autoAck: false, consumer: consumer);

    Console.WriteLine("Worker connected and waiting for messages. Press Ctrl+C to exit.");
    await Task.Delay(-1);
}
catch (Exception ex)
{
    Console.WriteLine($"Worker error connecting to RabbitMQ: {ex.Message}");
}
