using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisApp.Infrastructure.Persistence.Configurations;

public class ChatCustomSkillConfiguration : IEntityTypeConfiguration<ChatCustomSkill>
{
    public void Configure(EntityTypeBuilder<ChatCustomSkill> builder)
    {
        builder.HasKey(cs => cs.Id);

        builder.HasIndex(cs => new { cs.ChatId, cs.CustomSkillId })
            .IsUnique();

        builder.HasOne(cs => cs.Chat)
            .WithMany(c => c.AppliedSkills)
            .HasForeignKey(cs => cs.ChatId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(cs => cs.CustomSkill)
            .WithMany(s => s.ChatUsages)
            .HasForeignKey(cs => cs.CustomSkillId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}