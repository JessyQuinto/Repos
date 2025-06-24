using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TesorosChoco.Domain.Entities;
using TesorosChoco.Domain.ValueObjects;

namespace TesorosChoco.Infrastructure.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId)
            .IsRequired();

        builder.Property(o => o.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.OwnsOne(o => o.ShippingAddress, sa =>
        {
            sa.Property(s => s.Name)
                .IsRequired()
                .HasMaxLength(255);

            sa.Property(s => s.Address)
                .IsRequired()
                .HasMaxLength(500);

            sa.Property(s => s.City)
                .IsRequired()
                .HasMaxLength(255);

            sa.Property(s => s.ZipCode)
                .IsRequired()
                .HasMaxLength(20);

            sa.Property(s => s.Phone)
                .IsRequired()
                .HasMaxLength(20);
        });

        builder.Property(o => o.PaymentMethod)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(o => o.Total)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(o => o.CreatedAt)
            .IsRequired();

        builder.Property(o => o.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(o => o.User)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(o => o.Items)
            .WithOne(oi => oi.Order)
            .HasForeignKey(oi => oi.OrderId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
