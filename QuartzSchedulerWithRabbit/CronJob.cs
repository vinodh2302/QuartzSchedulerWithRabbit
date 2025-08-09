using Microsoft.Extensions.Logging;
using Quartz;
using RabbitMQ.Client;
using System.IO;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Xml.Serialization;
using WMSSystems.Models;

public class CronJob : IJob
{
    private readonly ILogger<CronJob> _logger;
    private readonly string _incomingFolder = Environment.GetEnvironmentVariable("INCOMING_XML_PATH") ?? "/app/IncomingXml"; // Change to your folder path

    public CronJob(ILogger<CronJob> logger)
    {
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        _logger.LogInformation("Cron job triggered at: {time}", DateTimeOffset.Now);

        var factory = new ConnectionFactory()
        {
            HostName = "rabbitmq",
            Port = 5672,
            UserName = "guest",
            Password = "guest"
        };

        using var connection = factory.CreateConnection();
        using var channel = connection.CreateModel();

        // Get all XML files in the incoming folder
        var xmlFiles = Directory.GetFiles(_incomingFolder, "*.xml");

        if (xmlFiles.Length == 0)
        {
            _logger.LogInformation("No XML files found in folder: {folder}", _incomingFolder);
            return Task.CompletedTask;
        }

        foreach (var filePath in xmlFiles)
        {
            try
            {
                var customer = DeserializeXml<Customer>(filePath);
                string jsonMessage = JsonSerializer.Serialize(customer);
                var body = Encoding.UTF8.GetBytes(jsonMessage);

                channel.BasicPublish(
                    exchange: "",
                    routingKey: "OrderProcessing", // your queue name
                    basicProperties: null,
                    body: body
                );

                _logger.LogInformation("Published XML file {file} to queue: {queue}", Path.GetFileName(filePath), "OrderProcessing");

                // Optionally, move the processed file to a processed folder or delete it
                var processedFolder = Path.Combine(_incomingFolder, "processed");
                if (!Directory.Exists(processedFolder))
                    Directory.CreateDirectory(processedFolder);

                var destFile = Path.Combine(processedFolder, Path.GetFileName(filePath));
                File.Move(filePath, destFile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to process XML file {file}", Path.GetFileName(filePath));
            }
        }

        return Task.CompletedTask;
    }

    private T DeserializeXml<T>(string filePath)
    {
        using var stream = new FileStream(filePath, FileMode.Open);
        var serializer = new XmlSerializer(typeof(T));
        return (T)serializer.Deserialize(stream);
    }
}
