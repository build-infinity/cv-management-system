using CvManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManagementSystem.Infrastructure.Persistence.Configurations;
public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.HasKey(user => user.Id);
        builder.Property(user => user.FirstName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.LastName).HasMaxLength(100).IsRequired();
        builder.Property(user => user.Email).HasMaxLength(254).IsRequired();
        builder.HasIndex(user => user.Email).IsUnique();
        builder.Property(user => user.PasswordHash).HasMaxLength(200);
        builder.Property(user => user.GoogleId).HasMaxLength(255);
        builder.HasIndex(user => user.GoogleId).IsUnique();
        builder.Property(user => user.EmailVerificationTokenHash).HasMaxLength(64);
    }
}
