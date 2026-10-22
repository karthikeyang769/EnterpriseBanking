using Microsoft.EntityFrameworkCore;
using AuditService.Domain.Entities;

namespace AuditService.Persistence;

public class AuditDbContext : DbContext
{
    public AuditDbContext(DbContextOptions<AuditDbContext> options) : base(options)
    {
    }

    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<AuditLog>(entity =>
        {
            entity.ToTable("AuditLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.EventName).IsRequired().HasMaxLength(100);
            entity.Property(e => e.EventData).IsRequired().HasColumnType("text");
            entity.Property(e => e.OccurredOn).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
