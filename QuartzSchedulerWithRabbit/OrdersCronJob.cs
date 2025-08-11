using Newtonsoft.Json;
using Quartz;
using System.IO;
using System.Xml.Linq;

public class OrdersCronJob : IJob
{
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<OrdersCronJob> _logger;
    public OrdersCronJob(IRabbitMqPublisher publisher, ILogger<OrdersCronJob> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
        string folderPath = Environment.GetEnvironmentVariable("INCOMING_XML_PATH") ?? "/app/IncomingXml"; // Change to your folder path
        _logger.LogInformation("Order Cron job triggered at: {time}", DateTimeOffset.Now);
        string processedPath = folderPath + "/Processed";
        folderPath += "/Orders";

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);



        foreach (var file in Directory.GetFiles(folderPath, "*.xml"))
        {

            var content = File.ReadAllText(file);
            var doc = XDocument.Parse(content);

            // Convert XML to JSON string
            string jsonContent = JsonConvert.SerializeXNode(doc, Newtonsoft.Json.Formatting.None, omitRootObject: true);

            _publisher.Publish("OrderProcessing", jsonContent);

            if (!Directory.Exists(processedPath))
                Directory.CreateDirectory(processedPath);

            string destFile = Path.Combine(processedPath, Path.GetFileName(file));
            File.Move(file, destFile, true);

            _logger.LogInformation($"Published and moved file: {Path.GetFileName(file)}");
        }


        return Task.CompletedTask;
    }
}
