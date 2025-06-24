using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Infrastructure.Configurations;

public class ProductConfiguration : IEntityTypeConfiguration<Product>
{
    public void Configure(EntityTypeBuilder<Product> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Slug)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(p => p.Slug)
            .IsUnique();

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Price)
            .IsRequired()
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.DiscountedPrice)
            .HasColumnType("decimal(18,2)");

        builder.Property(p => p.Image)
            .HasMaxLength(500);

        builder.Property(p => p.Images)
            .HasConversion(
                v => string.Join(',', v),
                v => v.Split(',', StringSplitOptions.RemoveEmptyEntries).ToList()
            );

        builder.Property(p => p.Stock)
            .IsRequired();

        builder.Property(p => p.Featured)
            .IsRequired();

        builder.Property(p => p.Rating)
            .HasColumnType("decimal(3,2)");

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasOne(p => p.Category)
            .WithMany(c => c.Products)
            .HasForeignKey(p => p.CategoryId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Producer)
            .WithMany(pr => pr.Products)
            .HasForeignKey(p => p.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
