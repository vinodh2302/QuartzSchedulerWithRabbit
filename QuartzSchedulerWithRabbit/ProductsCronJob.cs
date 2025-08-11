using Newtonsoft.Json;
using Quartz;
using System.IO;
using System.Text.Json;
using System.Xml;
using System.Xml.Linq;

public class ProductsCronJob : IJob
{
    private readonly IRabbitMqPublisher _publisher;
    private readonly ILogger<CustomersCronJob> _logger;
    public ProductsCronJob(IRabbitMqPublisher publisher, ILogger<CustomersCronJob> logger)
    {
        _publisher = publisher;
        _logger = logger;
    }

    public Task Execute(IJobExecutionContext context)
    {
      string folderPath = Environment.GetEnvironmentVariable("INCOMING_XML_PATH") ?? "/app/IncomingXml"; // Change to your folder path
        _logger.LogInformation("Customer Cron job triggered at: {time}", DateTimeOffset.Now);
        string processedPath = folderPath + "/Processed";
        folderPath += "/Products";
        
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
