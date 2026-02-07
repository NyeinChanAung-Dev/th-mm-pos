using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using th_mm_pos.domain.Entities;

namespace th_mm_pos.infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.ToTable("Transactions");
        
        builder.HasKey(t => t.Id);
        
        builder.Property(t => t.TransactionDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasIndex(t => t.TransactionDate);
        
        builder.Property(t => t.Subtotal)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(t => t.Tax)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(t => t.Discount)
            .IsRequired()
            .HasPrecision(18, 2)
            .HasDefaultValue(0);
        
        builder.Property(t => t.Total)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(t => t.PaymentMethod)
            .IsRequired();
        
        builder.Property(t => t.CashierId)
            .IsRequired();
        
        builder.HasIndex(t => t.CashierId);
        
        builder.Property(t => t.IsVoided)
            .IsRequired()
            .HasDefaultValue(false);
        
        builder.HasOne(t => t.Cashier)
            .WithMany(u => u.Transactions)
            .HasForeignKey(t => t.CashierId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(t => t.Items)
            .WithOne(ti => ti.Transaction)
            .HasForeignKey(ti => ti.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
