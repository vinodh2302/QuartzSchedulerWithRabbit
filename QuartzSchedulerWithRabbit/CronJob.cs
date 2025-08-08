using Quartz;
using Microsoft.Extensions.Logging;
using RabbitMQ.Client;
using System.Text;

public class CronJob : IJob
{
    private readonly ILogger<CronJob> _logger;

    public CronJob(ILogger<CronJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Cron job triggered at: {time}", DateTimeOffset.Now);

        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq", // IMPORTANT: use RabbitMQ container name, not localhost
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        string message = $"Scheduled Message at {DateTime.Now}";
        var body = Encoding.UTF8.GetBytes(message);

        channel.BasicPublish(
            exchange: "",
            routingKey: "OrderProcessing", // your existing queue name
            basicProperties: null,
            body: body
        );

        _logger.LogInformation("Published to queue: {queue} | Message: {msg}", "demo-queue", message);

        return Task.CompletedTask;
    }
}
