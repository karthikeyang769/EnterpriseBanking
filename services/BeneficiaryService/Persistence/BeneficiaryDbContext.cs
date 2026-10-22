using Microsoft.EntityFrameworkCore;
using BeneficiaryService.Domain.Entities;

namespace BeneficiaryService.Persistence;

public class BeneficiaryDbContext : DbContext
{
    public BeneficiaryDbContext(DbContextOptions<BeneficiaryDbContext> options) : base(options)
    {
    }

    public DbSet<Beneficiary> Beneficiaries => Set<Beneficiary>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Beneficiary>(entity =>
        {
            entity.ToTable("Beneficiaries");
            entity.HasKey(e => e.Id);
            entity.Property(e => e.BeneficiaryName).IsRequired().HasMaxLength(150);
            entity.Property(e => e.BeneficiaryAccountNumber).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Nickname).HasMaxLength(100);
            entity.Property(e => e.TransferLimit).HasColumnType("decimal(18,2)").IsRequired();
        });

        base.OnModelCreating(modelBuilder);
    }
}
