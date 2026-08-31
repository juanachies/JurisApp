using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisApp.Infrastructure.Persistence.Configurations;

public class ChatAuditConfiguration : IEntityTypeConfiguration<ChatAudit>
{
    public void Configure(EntityTypeBuilder<ChatAudit> builder)
    {
        builder.ToTable("Audits");
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Model)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(a => a.PromptVersion)
            .IsRequired()
            .HasMaxLength(50);

        builder.HasOne(a => a.Chat)
            .WithOne(c => c.Audit)
            .HasForeignKey<ChatAudit>(a => a.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.ChatId)
            .IsUnique();
    }
}
