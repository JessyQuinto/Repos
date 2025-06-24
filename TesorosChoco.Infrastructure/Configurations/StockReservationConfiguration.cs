using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Infrastructure.Configurations;

public class StockReservationConfiguration : IEntityTypeConfiguration<StockReservation>
{
    public void Configure(EntityTypeBuilder<StockReservation> builder)
    {
        builder.HasKey(sr => sr.Id);

        builder.Property(sr => sr.Quantity)
            .IsRequired();

        builder.Property(sr => sr.ExpiresAt)
            .IsRequired();

        builder.Property(sr => sr.SessionId)
            .HasMaxLength(100);

        builder.Property(sr => sr.IsActive)
            .IsRequired();

        builder.Property(sr => sr.CreatedAt)
            .IsRequired();

        builder.Property(sr => sr.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(sr => sr.Product)
            .WithMany()
            .HasForeignKey(sr => sr.ProductId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(sr => sr.User)
            .WithMany()
            .HasForeignKey(sr => sr.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Indexes for performance
        builder.HasIndex(sr => new { sr.ProductId, sr.UserId, sr.IsActive });
        builder.HasIndex(sr => new { sr.ExpiresAt, sr.IsActive });
        builder.HasIndex(sr => sr.SessionId);
    }
}
