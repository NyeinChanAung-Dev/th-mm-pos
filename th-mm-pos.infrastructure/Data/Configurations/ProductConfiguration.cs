using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using th_mm_pos.domain.Entities;

namespace th_mm_pos.infrastructure.Data.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.ToTable("Products");
        
        builder.HasKey(p => p.Id);
        
        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(p => p.SKU)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.HasIndex(p => p.SKU)
            .IsUnique();
        
        builder.Property(p => p.Price)
            .IsRequired()
            .HasPrecision(18, 2);
        
        builder.Property(p => p.Quantity)
            .IsRequired();
        
        builder.Property(p => p.ReorderLevel)
            .IsRequired()
            .HasDefaultValue(10);
        
        builder.Property(p => p.Category)
            .HasMaxLength(50);
        
        builder.HasIndex(p => p.Category);
        
        builder.Property(p => p.IsActive)
            .IsRequired()
            .HasDefaultValue(true);
        
        builder.HasIndex(p => p.IsActive);
        
        builder.Property(p => p.CreatedAt)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        
        builder.Property(p => p.ModifiedAt)
            .IsRequired(false);
        
        // Row versioning for optimistic concurrency
        builder.Property<byte[]>("RowVersion")
            .IsRowVersion();
        
        builder.HasMany(p => p.TransactionItems)
            .WithOne(ti => ti.Product)
            .HasForeignKey(ti => ti.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
        
        builder.HasMany(p => p.OrderItems)
            .WithOne(oi => oi.Product)
            .HasForeignKey(oi => oi.ProductId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
