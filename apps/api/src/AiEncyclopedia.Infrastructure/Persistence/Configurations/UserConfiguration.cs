using AiEncyclopedia.Domain.Entities;
using AiEncyclopedia.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace AiEncyclopedia.Infrastructure.Persistence.Configurations;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Email).HasMaxLength(255).IsRequired();
        builder.HasIndex(u => u.Email).IsUnique();
        builder.Property(u => u.DisplayName).HasMaxLength(100).IsRequired();
        builder.Property(u => u.SubscriptionTier)
            .HasConversion<string>()
            .HasMaxLength(20)
            .HasDefaultValue(SubscriptionTier.Free);

        builder.HasMany(u => u.RefreshTokens)
            .WithOne(r => r.User)
            .HasForeignKey(r => r.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Subscription)
            .WithOne(s => s.User)
            .HasForeignKey<Subscription>(s => s.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
