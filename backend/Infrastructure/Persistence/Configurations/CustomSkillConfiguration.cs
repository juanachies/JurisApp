using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisApp.Infrastructure.Persistence.Configurations;

public class CustomSkillConfiguration : IEntityTypeConfiguration<CustomSkill>
{
    public void Configure(EntityTypeBuilder<CustomSkill> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(s => s.WhenToUse).IsRequired();
        builder.Property(s => s.Instructions).IsRequired();
        builder.Property(s => s.Examples).IsRequired();
        builder.Property(s => s.RedFlags).IsRequired();
        builder.Property(s => s.OutputFormat).IsRequired();

        builder.HasOne(s => s.LawyerProfile)
            .WithMany(lp => lp.CustomSkills)
            .HasForeignKey(s => s.LawyerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
