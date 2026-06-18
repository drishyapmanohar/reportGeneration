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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportJob>()
            .Property(x => x.ReportType)
            .HasMaxLength(100);

        modelBuilder.Entity<ReportJob>()
            .Property(x => x.Status)
            .HasMaxLength(50);

        modelBuilder.Entity<ReportJob>()
            .Property(x => x.FileUrl)
            .HasMaxLength(500);

        modelBuilder.Entity<ReportJob>()
            .Property(x => x.FilePath)
            .HasMaxLength(500);

        modelBuilder.Entity<ReportJob>()
            .HasIndex(x => x.CreatedAt);

        modelBuilder.Entity<ReportJob>()
            .HasIndex(x => x.Status);

        base.OnModelCreating(modelBuilder);
    }
}