using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TesorosChoco.Domain.Entities;

namespace TesorosChoco.Infrastructure.Configurations;

public class NewsletterSubscriptionConfiguration : IEntityTypeConfiguration<NewsletterSubscription>
{
    public void Configure(EntityTypeBuilder<NewsletterSubscription> builder)
    {
        builder.HasKey(ns => ns.Id);

        builder.Property(ns => ns.Email)
            .IsRequired()
            .HasMaxLength(255);

        builder.HasIndex(ns => ns.Email)
            .IsUnique();

        builder.Property(ns => ns.IsActive)
            .IsRequired();

        builder.Property(ns => ns.SubscribedAt)
            .IsRequired();

        builder.Property(ns => ns.UnsubscribedAt);
    }
}
