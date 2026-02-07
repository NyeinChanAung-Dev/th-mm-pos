using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using th_mm_pos.domain.Entities;

namespace th_mm_pos.infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.ToTable("Orders");
        
        builder.HasKey(o => o.Id);
        
        builder.Property(o => o.CustomerName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(o => o.CustomerPhone)
            .HasMaxLength(20);
        
        builder.Property(o => o.OrderDate)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasIndex(o => o.OrderDate);
        
        builder.Property(o => o.ExpectedFulfillmentDate)
            .IsRequired(false);
        
        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<int>();
        
        builder.HasIndex(o => o.Status);
        
        builder.Property(o => o.CreatedByUserId)
            .IsRequired();
        
        builder.Property(o => o.CompletedTransactionId)
            .IsRequired(false);
        
        builder.HasOne(o => o.CreatedBy)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.CreatedByUserId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasOne(o => o.CompletedTransaction)
            .WithMany()
            .HasForeignKey(o => o.CompletedTransactionId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
