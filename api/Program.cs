using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite("Data Source=reports.db"));

builder.Services.AddSingleton<ReportWorkerService>();

var app = builder.Build();

app.UseCors("AngularApp");

Directory.CreateDirectory("GeneratedReports");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "API Running");

app.MapPost("/api/reports/generate", async (
    AppDbContext db,
    ReportWorkerService worker) =>
{
    var job = new ReportJob
    {
        Id = Guid.NewGuid(),
        ReportType = "Credit Report",
        Status = "Pending",
        CreatedAt = DateTime.UtcNow
    };

    db.ReportJobs.Add(job);
    await db.SaveChangesAsync();

    worker.StartJob(job.Id);

    return Results.Ok(new
    {
        jobId = job.Id,
        message = "Report generation started"
    });
});

app.MapGet("/api/reports/status/{jobId:guid}", async (
    Guid jobId,
    AppDbContext db) =>
{
    var job = await db.ReportJobs.FindAsync(jobId);

    return job is null
        ? Results.NotFound()
        : Results.Ok(job);
});

app.MapGet("/api/reports/my-reports", async (AppDbContext db) =>
{
    var jobs = await db.ReportJobs
        .OrderByDescending(x => x.CreatedAt)
        .ToListAsync();

    return Results.Ok(jobs);
});

app.MapGet("/api/reports/download/{jobId:guid}", async (
    Guid jobId,
    AppDbContext db) =>
{
    var job = await db.ReportJobs.FindAsync(jobId);

    if (job is null || job.Status != "Completed" || string.IsNullOrWhiteSpace(job.FilePath))
    {
        return Results.NotFound();
    }

    var filePath = Path.Combine(Directory.GetCurrentDirectory(), job.FilePath);

    if (!File.Exists(filePath))
    {
        return Results.NotFound();
    }

    return Results.File(
        filePath,
        "text/csv",
        $"credit-report-{jobId}.csv"
    );
});

app.MapPost("/api/reports/generate-failed", async (
    AppDbContext db,
    ReportWorkerService worker) =>
{
    var job = new ReportJob
    {
        Id = Guid.NewGuid(),
        ReportType = "Credit Report",
        Status = "Pending",
        CreatedAt = DateTime.UtcNow
    };

    db.ReportJobs.Add(job);
    await db.SaveChangesAsync();

    worker.StartJob(job.Id, shouldFail: true);

    return Results.Ok(new
    {
        jobId = job.Id,
        message = "Failed report simulation started"
    });
});

app.Run();

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ReportJob> ReportJobs => Set<ReportJob>();
}

public class ReportJob
{
    public Guid Id { get; set; }
    public string ReportType { get; set; } = "Credit Report";
    public string Status { get; set; } = "Pending";
    public string? FileUrl { get; set; }
    public string? FilePath { get; set; }
    public string? ErrorMessage { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? StartedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class ReportWorkerService
{
    private readonly IServiceScopeFactory _scopeFactory;

    public ReportWorkerService(IServiceScopeFactory scopeFactory)
    {
        _scopeFactory = scopeFactory;
    }

    public void StartJob(Guid jobId, bool shouldFail = false)
    {
         _ = Task.Run(async () => await ProcessJob(jobId, shouldFail));
    }

    private async Task ProcessJob(Guid jobId, bool shouldFail)
    {
        try
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var job = await db.ReportJobs.FindAsync(jobId);
            if (job == null) return;

            await Task.Delay(TimeSpan.FromSeconds(5));

            job.Status = "InProgress";
            job.StartedAt = DateTime.UtcNow;
            await db.SaveChangesAsync();

            await Task.Delay(TimeSpan.FromSeconds(20));
                if (shouldFail)
                {
                    throw new Exception("Simulated report generation failure.");
                }
            var folder = "GeneratedReports";
            Directory.CreateDirectory(folder);

            var fileName = $"credit-report-{jobId}.csv";
            var relativePath = Path.Combine(folder, fileName);

            var csv =
                "CustomerId,CustomerName,CreditScore,RiskLevel,Status\n" +
                "1001,John Smith,780,Low,Approved\n" +
                "1002,Sarah Lee,620,High,Review Required\n" +
                "1003,Michael Brown,710,Medium,Pending\n";

            await File.WriteAllTextAsync(relativePath, csv);

            job.Status = "Completed";
            job.CompletedAt = DateTime.UtcNow;
            job.FilePath = relativePath;
            job.FileUrl = $"/api/reports/download/{jobId}";

            await db.SaveChangesAsync();
        }
        catch (Exception ex)
        {
            using var scope = _scopeFactory.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

            var job = await db.ReportJobs.FindAsync(jobId);
            if (job == null) return;

            job.Status = "Failed";
            job.ErrorMessage = ex.Message;
            job.CompletedAt = DateTime.UtcNow;

            await db.SaveChangesAsync();
        }
    }
}