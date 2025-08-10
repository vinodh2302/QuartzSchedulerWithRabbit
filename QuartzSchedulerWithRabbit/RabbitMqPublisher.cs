using RabbitMQ.Client;
using System.Text;

public interface IRabbitMqPublisher
{
    void Publish(string queueName, string message);
}

public class RabbitMqPublisher : IRabbitMqPublisher, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _channel;
    private readonly ILogger<RabbitMqPublisher> _logger;
    public RabbitMqPublisher(ILogger<RabbitMqPublisher> logger)
    {
        var factory = new ConnectionFactory
        {
            HostName = "rabbitmq", // service name in docker-compose
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };
        _connection = factory.CreateConnection();
        _channel = _connection.CreateModel();
        _logger = logger;
    }

    public void Publish(string queueName, string message)
    {
        _channel.QueueDeclare(queue: queueName, durable: true, exclusive: false, autoDelete: false, arguments: null);

        var body = Encoding.UTF8.GetBytes(message);

        _channel.BasicPublish(exchange: "", routingKey: queueName, basicProperties: null, body: body);
    

        _logger.LogInformation($"[x] Sent to {queueName}: {message}");
    }

    public void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
    }
}
