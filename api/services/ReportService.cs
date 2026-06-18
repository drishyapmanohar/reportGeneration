using api.Data;
using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Services;

public class ReportService
{
    private readonly AppDbContext _db;
    private readonly ServiceBusQueueService _queueService;

    public ReportService(
        AppDbContext db,
        ServiceBusQueueService queueService)
    {
        _db = db;
        _queueService = queueService;
    }

    public async Task<object> GenerateReport()
    {
        var job = new ReportJob
        {
            Id = Guid.NewGuid(),
            ReportType = "Credit Report",
            Status = "Pending",
            CreatedAt = DateTime.UtcNow
        };

        _db.ReportJobs.Add(job);
        await _db.SaveChangesAsync();

        await _queueService.SendReportJobAsync(job.Id);

        return new
        {
            jobId = job.Id,
            message = "Report generation started"
        };
    }

    public async Task<ReportJob?> GetStatus(Guid jobId)
    {
        return await _db.ReportJobs.FindAsync(jobId);
    }

    public async Task<List<ReportJob>> GetMyReports()
    {
        return await _db.ReportJobs
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();
    }

    public async Task<ReportJob?> GetReportForDownload(Guid jobId)
    {
        return await _db.ReportJobs.FindAsync(jobId);
    }
}