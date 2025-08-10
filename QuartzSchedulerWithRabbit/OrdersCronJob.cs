using Quartz;
using System.IO;

public class OrdersCronJob : IJob
{
    private readonly IRabbitMqPublisher _publisher;

    public OrdersCronJob(IRabbitMqPublisher publisher)
    {
        _publisher = publisher;
    }

    public Task Execute(IJobExecutionContext context)
    {
        string folderPath = Environment.GetEnvironmentVariable("INCOMING_XML_PATH") ?? "/app/IncomingXml/Orders";

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        foreach (var file in Directory.GetFiles(folderPath, "*.xml"))
        {
            var content = File.ReadAllText(file);
            _publisher.Publish("OrderProcessing", content);
        }

        return Task.CompletedTask;
    }
}
