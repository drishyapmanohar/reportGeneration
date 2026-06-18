using api.Models;
using Microsoft.EntityFrameworkCore;

namespace api.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<ReportJob> ReportJobs => Set<ReportJob>();
}