using Azure.Messaging.ServiceBus;

namespace api.Services;

public class ServiceBusQueueService
{
    private readonly IConfiguration _configuration;

    public ServiceBusQueueService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public async Task SendReportJobAsync(Guid jobId)
    {
        var connectionString = _configuration.GetConnectionString("ServiceBus");
        var queueName = _configuration["ServiceBus:QueueName"] ?? "report-jobs";

        await using var client = new ServiceBusClient(connectionString);
        ServiceBusSender sender = client.CreateSender(queueName);

        var message = new ServiceBusMessage(jobId.ToString())
        {
            MessageId = jobId.ToString(),
            Subject = "ReportGeneration"
        };

        await sender.SendMessageAsync(message);
    }
}