using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisApp.Infrastructure.Persistence.Configurations;

public class DocumentAnalysisConfiguration : IEntityTypeConfiguration<DocumentAnalysis>
{
    public void Configure(EntityTypeBuilder<DocumentAnalysis> builder)
    {
        builder.HasKey(da => da.Id);

        builder.Property(da => da.Summary).IsRequired();
        builder.Property(da => da.Risks).IsRequired();
        builder.Property(da => da.Recommendations).IsRequired();
        builder.Property(da => da.References).IsRequired();

        builder.Property(da => da.Type)
            .IsRequired()
            .HasConversion<string>();

        builder.HasOne(da => da.Document)
            .WithOne(d => d.Analysis)
            .HasForeignKey<DocumentAnalysis>(da => da.DocumentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}