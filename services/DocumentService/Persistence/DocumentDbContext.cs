using Microsoft.EntityFrameworkCore;
using DocumentService.Domain.Entities;

namespace DocumentService.Persistence;

public class DocumentDbContext : DbContext
{
    public DocumentDbContext(DbContextOptions<DocumentDbContext> options) : base(options)
    {
    }

    public DbSet<DocumentLog> DocumentLogs => Set<DocumentLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DocumentLog>(entity =>
        {
            entity.ToTable("DocumentLogs");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.DocumentType).IsRequired().HasMaxLength(100);
            entity.Property(e => e.FileName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.ObjectName).IsRequired().HasMaxLength(256);
            entity.Property(e => e.UploadedAt).IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
