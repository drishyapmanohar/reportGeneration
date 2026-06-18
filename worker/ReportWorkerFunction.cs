using System.Text;
using Azure.Storage.Blobs;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using worker.Data;

namespace worker;

public class ReportWorkerFunction
{
    private readonly ILogger<ReportWorkerFunction> _logger;
    private readonly AppDbContext _db;

    public ReportWorkerFunction(
        ILogger<ReportWorkerFunction> logger,
        AppDbContext db)
    {
        _logger = logger;
        _db = db;
    }

    [Function(nameof(ReportWorkerFunction))]
    public async Task Run(
        [ServiceBusTrigger("report-jobs", Connection = "ServiceBusConnection")]
        string jobId)
    {
        var id = Guid.Parse(jobId);
        var job = await _db.ReportJobs.FindAsync(id);

        if (job == null)
        {
            _logger.LogWarning("Job not found: {JobId}", jobId);
            return;
        }

        if (job.Status == "Completed" || job.Status == "Failed")
        {
            _logger.LogInformation("Job already finished. Skipping: {JobId}", jobId);
            return;
        }

        try
        {
            await Task.Delay(TimeSpan.FromSeconds(5));
            job.Status = "InProgress";
            job.StartedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();
            await NotifyApi(id);
            await Task.Delay(TimeSpan.FromSeconds(10));

            var csv =
                "CustomerId,CustomerName,CreditScore,RiskLevel,Status\n" +
                "1001,John Smith,780,Low,Approved\n" +
                "1002,Sarah Lee,620,High,Review Required\n" +
                "1003,Michael Brown,710,Medium,Pending\n";

            var blobConnectionString = Environment.GetEnvironmentVariable("BlobStorage");
            var containerName = Environment.GetEnvironmentVariable("AzureStorageContainerName") ?? "reports";

            var blobServiceClient = new BlobServiceClient(blobConnectionString);
            var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
            await containerClient.CreateIfNotExistsAsync();

            var fileName = $"credit-report-{jobId}.csv";
            var blobClient = containerClient.GetBlobClient(fileName);

            using var stream = new MemoryStream(Encoding.UTF8.GetBytes(csv));
            await blobClient.UploadAsync(stream, overwrite: true);

            job.Status = "Completed";
            job.IsRead = false;
            job.CompletedAt = DateTime.UtcNow;
            job.FilePath = fileName;
            job.FileUrl = $"/api/reports/download/{jobId}";
            job.ErrorMessage = null;

            await _db.SaveChangesAsync();
            Console.WriteLine($"NOTIFY: {job.Status} - {job.Id}");
            await NotifyApi(id);

            _logger.LogInformation("Report completed: {JobId}", jobId);
        }
        catch (Exception ex)
        {
            job.Status = "Failed";
            await NotifyApi(id);
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();

            _logger.LogError(ex, "Report failed: {JobId}", jobId);
            throw;
        }
    }

    private static async Task NotifyApi(Guid jobId)
    {
        var apiBaseUrl = Environment.GetEnvironmentVariable("ApiBaseUrl");

        if (string.IsNullOrWhiteSpace(apiBaseUrl))
        {
            Console.WriteLine("ApiBaseUrl missing");
            return;
        }

        using var httpClient = new HttpClient();

        var url = $"{apiBaseUrl}/api/reports/notify-status/{jobId}";

        Console.WriteLine($"Calling notify API: {url}");

        var response = await httpClient.PostAsync(url, null);

        Console.WriteLine($"Notify API response: {response.StatusCode}");
    }
}