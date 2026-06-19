using api.Services;
using Microsoft.AspNetCore.Mvc;
using Azure.Storage.Blobs;
using Microsoft.EntityFrameworkCore;
using api.Data;
using api.Hubs;
using Microsoft.AspNetCore.SignalR;
using Azure.Storage.Sas;

namespace api.Controllers;

[ApiController]
[Route("api/reports")]
public class ReportsController : ControllerBase
{
    private readonly ReportService _reportService;
    private readonly IConfiguration _configuration;
    private readonly AppDbContext _db;
    private readonly IHubContext<ReportHub> _hubContext;

    public ReportsController(
    ReportService reportService,
    IConfiguration configuration,
    AppDbContext db,
    IHubContext<ReportHub> hubContext)
    {
        _reportService = reportService;
        _configuration = configuration;
        _db = db;
        _hubContext = hubContext;
    }

    [HttpPost("generate")]
    public async Task<IActionResult> GenerateReport()
    {
        var result = await _reportService.GenerateReport();
        return Ok(result);
    }

    [HttpGet("status/{jobId:guid}")]
    public async Task<IActionResult> GetStatus(Guid jobId)
    {
        var job = await _reportService.GetStatus(jobId);

        if (job == null)
        {
            return NotFound();
        }

        return Ok(job);
    }

    [HttpGet("download/{jobId:guid}")]
    public async Task<IActionResult> DownloadReport(Guid jobId)
    {
        var job = await _reportService.GetReportForDownload(jobId);

        if (job == null || job.Status != "Completed" || string.IsNullOrWhiteSpace(job.FilePath))
        {
            return NotFound();
        }

        var connectionString =
            _configuration.GetConnectionString("BlobStorage")
            ?? _configuration["BlobStorage"];

        var containerName =
            _configuration["AzureStorage:ContainerName"]
            ?? _configuration["AzureStorageContainerName"]
            ?? "reports";
Console.WriteLine($"BlobStorage = [{connectionString}]");
        var blobServiceClient = new BlobServiceClient(connectionString);
        var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
        var blobClient = containerClient.GetBlobClient(job.FilePath);

        if (!await blobClient.ExistsAsync())
        {
            return NotFound();
        }

        var sasUrl = blobClient.GenerateSasUri(
            Azure.Storage.Sas.BlobSasPermissions.Read,
            DateTimeOffset.UtcNow.AddMinutes(5)
        );

        return Redirect(sasUrl.ToString());
    }

    [HttpGet("notifications/count")]
    public async Task<IActionResult> GetNotificationCount()
    {
        var count = await _db.ReportJobs
            .CountAsync(x =>
                x.Status == "Completed" &&
                !x.IsRead);

        return Ok(count);
    }

    [HttpPost("notifications/read")]
    public async Task<IActionResult> MarkNotificationsRead()
    {
        var reports = await _db.ReportJobs
            .Where(x => x.Status == "Completed" && !x.IsRead)
            .ToListAsync();

        foreach (var report in reports)
        {
            report.IsRead = true;
        }

        await _db.SaveChangesAsync();

        return Ok();
    }

    [HttpPost("notify-status/{jobId:guid}")]
    public async Task<IActionResult> NotifyStatus(Guid jobId)
    {
        var job = await _db.ReportJobs.FindAsync(jobId);

        if (job == null)
        {
            return NotFound();
        }

        await _hubContext.Clients.All.SendAsync("ReportStatusUpdated", job);
        Console.WriteLine($"Broadcasting SignalR status: {job.Status} - {job.Id}");
        return Ok();
    }

    [HttpGet("my-reports")]
    public async Task<IActionResult> GetMyReports(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10)
    {
        var result = await _reportService.GetMyReports(page, pageSize);
        return Ok(result);
    }

    [HttpGet("dashboard-summary")]
    public async Task<IActionResult> GetDashboardSummary()
    {
        var result = await _reportService.GetDashboardSummary();
        return Ok(result);
    }
}
