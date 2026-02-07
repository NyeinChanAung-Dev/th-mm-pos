using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using th_mm_pos.domain.Entities;

namespace th_mm_pos.infrastructure.Data.Configurations;

public class TransactionItemConfiguration : IEntityTypeConfiguration<TransactionItem>
{
    public void Configure(EntityTypeBuilder<TransactionItem> builder)
    {
        builder.ToTable("TransactionItems");
        
        builder.HasKey(ti => ti.Id);
        
        builder.Property(ti => ti.ProductName)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(ti => ti.Quantity)
            .IsRequired();
        
        builder.Property(ti => ti.UnitPrice)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(ti => ti.LineTotal)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.HasOne(ti => ti.Transaction)
            .WithMany(t => t.Items)
            .HasForeignKey(ti => ti.TransactionId)
            .OnDelete(DeleteBehavior.Cascade);
        
        builder.HasOne(ti => ti.Product)
            .WithMany(p => p.TransactionItems)
            .HasForeignKey(ti => ti.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
