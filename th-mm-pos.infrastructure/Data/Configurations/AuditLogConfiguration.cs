using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using th_mm_pos.domain.Entities;

namespace th_mm_pos.infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");
        
        builder.HasKey(al => al.Id);
        
        builder.Property(al => al.UserId)
            .IsRequired();
        
        builder.Property(al => al.Action)
            .IsRequired()
            .HasMaxLength(100);
        
        builder.Property(al => al.EntityType)
            .IsRequired()
            .HasMaxLength(50);
        
        builder.Property(al => al.EntityId)
            .IsRequired();
        
        builder.HasIndex(al => new { al.EntityType, al.EntityId });
        
        builder.Property(al => al.OldValue)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(al => al.NewValue)
            .HasColumnType("NVARCHAR(MAX)");
        
        builder.Property(al => al.Timestamp)
            .IsRequired()
            .HasDefaultValueSql("GETUTCDATE()");
        
        builder.HasIndex(al => al.Timestamp);
        
        builder.HasOne(al => al.User)
            .WithMany()
            .HasForeignKey(al => al.UserId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
