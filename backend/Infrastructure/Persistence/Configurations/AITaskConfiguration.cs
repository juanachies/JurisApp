using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisApp.Infrastructure.Persistence.Configurations;

public class AITaskConfiguration : IEntityTypeConfiguration<AITask>
{
    public void Configure(EntityTypeBuilder<AITask> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.Plan)
            .IsRequired();

        builder.Property(t => t.Status)
            .IsRequired()
            .HasConversion<string>();

        builder.Property(t => t.Result)
            .HasMaxLength(5000);

        builder.HasOne(t => t.Chat)
            .WithMany(c => c.Tasks)
            .HasForeignKey(t => t.ChatId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}