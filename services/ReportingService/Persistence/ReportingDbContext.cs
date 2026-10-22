using Microsoft.EntityFrameworkCore;
using ReportingService.Domain.Entities;

namespace ReportingService.Persistence;

public class ReportingDbContext : DbContext
{
    public ReportingDbContext(DbContextOptions<ReportingDbContext> options) : base(options)
    {
    }

    public DbSet<ReportLog> ReportLogs => Set<ReportLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<ReportLog>(entity =>
        {
            entity.ToTable("ReportLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.AccountNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.ReportUrl).IsRequired().HasMaxLength(512);
            entity.Property(e => e.GeneratedAt).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
