using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisApp.Infrastructure.Persistence.Configurations;

public class AITaskStepConfiguration : IEntityTypeConfiguration<AITaskStep>
{
    public void Configure(EntityTypeBuilder<AITaskStep> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.Title)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(s => s.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.Property(s => s.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(s => s.Result)
            .HasMaxLength(16000);

        builder.HasIndex(s => new { s.AITaskId, s.Order })
            .IsUnique();

        builder.HasOne(s => s.AITask)
            .WithMany(t => t.Steps)
            .HasForeignKey(s => s.AITaskId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
