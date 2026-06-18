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

    public async Task<ReportJob?> GetReportForDownload(Guid jobId)
    {
        return await _db.ReportJobs.FindAsync(jobId);
    }

    public async Task<object> GetMyReports(int page = 1, int pageSize = 10)
    {
        var query = _db.ReportJobs
            .OrderByDescending(x => x.CreatedAt);

        var totalCount = await query.CountAsync();

        var items = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new
        {
            items,
            totalCount,
            page,
            pageSize,
            totalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<object> GetDashboardSummary()
    {
        return new
        {
            totalReports = await _db.ReportJobs.CountAsync(),
            completedReports = await _db.ReportJobs.CountAsync(x => x.Status == "Completed"),
            inProgressReports = await _db.ReportJobs.CountAsync(x => x.Status == "InProgress"),
            failedReports = await _db.ReportJobs.CountAsync(x => x.Status == "Failed")
        };
    }
}