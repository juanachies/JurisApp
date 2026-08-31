using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisApp.Infrastructure.Persistence.Configurations;

public class DocumentConfiguration : IEntityTypeConfiguration<Document>
{
    public void Configure(EntityTypeBuilder<Document> builder)
    {
        builder.HasKey(d => d.Id);

        builder.Property(d => d.Title)
            .IsRequired()
            .HasMaxLength(300);

        builder.Property(d => d.Url)
            .IsRequired()
            .HasMaxLength(500);

        builder.HasOne(d => d.Chat)
            .WithMany(c => c.Documents)
            .HasForeignKey(d => d.ChatId)
            .OnDelete(DeleteBehavior.Cascade)
            .IsRequired(false);

        builder.HasOne(d => d.Folder)
            .WithMany(f => f.Documents)
            .HasForeignKey(d => d.FolderId)
            .OnDelete(DeleteBehavior.SetNull);
    }
}
