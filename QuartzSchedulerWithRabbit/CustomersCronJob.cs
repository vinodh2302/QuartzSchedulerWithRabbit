using Newtonsoft.Json;
using Quartz;
using System.IO;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

public class CustomersCronJob : IJob
{
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<CustomersCronJob> _logger;
    public CustomersCronJob(IRabbitMqPublisher publisher, ILogger<CustomersCronJob> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
      string folderPath = Environment.GetEnvironmentVariable("INCOMING_XML_PATH") ?? "/app/IncomingXml"; // Change to your folder path
        _logger.LogInformation("Customer Cron job triggered at: {time}", DateTimeOffset.Now);
        string processedPath = folderPath + "/Processed";
        folderPath += "/Customers";
        
        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);
        _logger.LogInformation("11"+ folderPath);


        foreach (var file in Directory.GetFiles(folderPath, "*.xml"))
        {
            _logger.LogInformation("33");
            var content = File.ReadAllText(file);
            var doc = XDocument.Parse(content);
            _logger.LogInformation("22");
            // Convert XML to JSON string
            string jsonContent = JsonConvert.SerializeXNode(doc, Newtonsoft.Json.Formatting.None, omitRootObject: true);

            _publisher.Publish("OrderProcessing", jsonContent);
            _logger.LogInformation("44");
            if (!Directory.Exists(processedPath))
                Directory.CreateDirectory(processedPath);
            _logger.LogInformation("55");
            string destFile = Path.Combine(processedPath, Path.GetFileName(file));
            File.Move(file, destFile, true);
            _logger.LogInformation("66"+ destFile);
            _logger.LogInformation($"Published and moved file: {Path.GetFileName(file)}");
        }


        return Task.CompletedTask;
    }
}
