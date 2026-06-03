using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisApp.Infrastructure.Persistence.Configurations;

public class AuditConfiguration : IEntityTypeConfiguration<Audit>
{
    public void Configure(EntityTypeBuilder<Audit> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PromptVersion)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(a => a.Chat)
            .WithOne(c => c.Audit)
            .HasForeignKey<Audit>(a => a.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}