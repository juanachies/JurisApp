using JurisApp.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisApp.Infrastructure.Persistence.Configurations;

public class FolderConfiguration : IEntityTypeConfiguration<Folder>
{
    public void Configure(EntityTypeBuilder<Folder> builder)
    {
        builder.HasKey(f => f.Id);

        builder.Property(f => f.Name)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(f => f.LegalContext)
            .HasMaxLength(2000);

        builder.HasOne(f => f.LawyerProfile)
            .WithMany(lp => lp.Folders)
            .HasForeignKey(f => f.LawyerProfileId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}