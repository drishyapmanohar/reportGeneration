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

var app = builder.Build();

app.UseCors("AngularApp");

var reportJobs = new List<ReportJob>();

app.MapGet("/", () => "API Running");

app.MapPost("/api/reports/generate", () =>
{
    var job = new ReportJob
    {
        Id = Guid.NewGuid(),
        ReportType = "Credit Report",
        Status = "Pending",
        CreatedAt = DateTime.UtcNow
    };

    reportJobs.Add(job);

    return Results.Ok(new
    {
        jobId = job.Id,
        message = "Report generation started"
    });
});

app.MapGet("/api/reports/status/{jobId:guid}", (Guid jobId) =>
{
    var job = reportJobs.FirstOrDefault(x => x.Id == jobId);

    return job is null
        ? Results.NotFound()
        : Results.Ok(job);
});

app.MapPost("/api/reports/mock-complete/{jobId:guid}", (Guid jobId) =>
{
    var job = reportJobs.FirstOrDefault(x => x.Id == jobId);

    if (job is null)
        return Results.NotFound();

    job.Status = "Completed";
    job.CompletedAt = DateTime.UtcNow;
    job.FileUrl = $"https://demo-storage/reports/{jobId}.csv";

    return Results.Ok(job);
});

app.MapGet("/api/reports/my-reports", () =>
{
    return Results.Ok(reportJobs.OrderByDescending(x => x.CreatedAt));
});

app.Run();

public class ReportJob
{
    public Guid Id { get; set; }
    public string ReportType { get; set; } = "";
    public string Status { get; set; } = "Pending";
    public string? FileUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}