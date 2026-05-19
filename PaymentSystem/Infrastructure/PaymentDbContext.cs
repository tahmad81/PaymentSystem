using Microsoft.EntityFrameworkCore;
using PaymentSystem.Domain;

namespace PaymentSystem.Infrastructure;

public sealed class PaymentDbContext : DbContext
{
    public PaymentDbContext(DbContextOptions<PaymentDbContext> options)
        : base(options)
    {
    }

    public DbSet<Payment> Payments => Set<Payment>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(p => p.Id);
            entity.Property(p => p.Amount).IsRequired();
            entity.Property(p => p.Currency).IsRequired().HasMaxLength(10);
            entity.Property(p => p.Method).IsRequired();
            entity.Property(p => p.Status).IsRequired();
            entity.Property(p => p.CreatedAt).IsRequired();
        });
    }
}
