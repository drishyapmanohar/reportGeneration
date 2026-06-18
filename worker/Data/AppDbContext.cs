using Microsoft.EntityFrameworkCore;
using worker.Models;

namespace worker.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ReportJob> ReportJobs => Set<ReportJob>();
}