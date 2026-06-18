using api.Data;
using api.Services;
using Microsoft.EntityFrameworkCore;
using api.Hubs;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AngularApp", policy =>
    {
       policy.AllowAnyHeader()
        .AllowAnyMethod()
        .AllowCredentials()
        .SetIsOriginAllowed(_ => true);
    });
});

builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
builder.Services.AddScoped<ReportService>();
builder.Services.AddScoped<ServiceBusQueueService>();
builder.Services.AddSignalR();

var app = builder.Build();

app.UseCors("AngularApp");

Directory.CreateDirectory("GeneratedReports");

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    db.Database.EnsureCreated();
}

app.MapGet("/", () => "API Running");

app.MapControllers();
app.MapHub<ReportHub>("/reportHub");

app.Run();