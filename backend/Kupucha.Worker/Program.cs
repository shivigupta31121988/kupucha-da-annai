using System.Text;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

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

        if (marketOpen)
        {
            Console.WriteLine($"[Worker] Executing order: {msg}");
            // ack
            channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
        }
        else
        {
            Console.WriteLine($"[Worker] Market closed, requeueing: {msg}");
            // requeue for later (simple approach)
            channel.BasicNack(deliveryTag: ea.DeliveryTag, multiple: false, requeue: true);
            Thread.Sleep(1000); // small delay to avoid tight loop in demo
        }
    };

    channel.BasicConsume(queue: "orders", autoAck: false, consumer: consumer);

    Console.WriteLine("Worker connected and waiting for messages. Press Ctrl+C to exit.");
    // keep running
    await Task.Delay(-1);
}
catch (Exception ex)
{
    Console.WriteLine($"Worker error connecting to RabbitMQ: {ex.Message}");
}
