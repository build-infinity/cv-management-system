using CvManagementSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace CvManagementSystem.Infrastructure.Persistence.Configurations;
public class EmailMessageConfiguration : IEntityTypeConfiguration<EmailMessage>
{
    public void Configure(EntityTypeBuilder<EmailMessage> builder)
    {
        builder.HasKey(email => email.Id);
        builder.Property(email => email.Recipient).HasMaxLength(254).IsRequired();
        builder.Property(email => email.Subject).HasMaxLength(200).IsRequired();
        builder.Property(email => email.Body).IsRequired();
        builder.HasIndex(email => email.NextAttemptAtUtc).HasFilter("\"SentAtUtc\" IS NULL");
    }
}
