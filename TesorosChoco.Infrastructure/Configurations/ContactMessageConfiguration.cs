using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Infrastructure.Configurations;

public class ContactMessageConfiguration : IEntityTypeConfiguration<ContactMessage>
{
    public void Configure(EntityTypeBuilder<ContactMessage> builder)
    {
        builder.HasKey(cm => cm.Id);

        builder.Property(cm => cm.Name)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(cm => cm.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(cm => cm.Subject)
            .IsRequired()
            .HasMaxLength(255);

        builder.Property(cm => cm.Message)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(cm => cm.CreatedAt)
            .IsRequired();
    }
}
