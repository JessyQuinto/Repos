using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Infrastructure.Configurations;

public class ProducerConfiguration : IEntityTypeConfiguration<Producer>
{
    public void Configure(EntityTypeBuilder<Producer> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(p => p.Description)
            .HasMaxLength(1000);

        builder.Property(p => p.Location)
            .HasMaxLength(255);

        builder.Property(p => p.Image)
            .HasMaxLength(500);

        builder.Property(p => p.Featured)
            .IsRequired();

        builder.Property(p => p.FoundedYear);

        builder.Property(p => p.CreatedAt)
            .IsRequired();

        builder.Property(p => p.UpdatedAt)
            .IsRequired();

        // Relationships
        builder.HasMany(p => p.Products)
            .WithOne(pr => pr.Producer)
            .HasForeignKey(pr => pr.ProducerId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
