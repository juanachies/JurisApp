using JurisApp.Domain.Entities;
using JurisApp.Domain.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace JurisApp.Infrastructure.Persistence.Configurations;

public class LawyerProfileConfiguration : IEntityTypeConfiguration<LawyerProfile>
{
    public void Configure(EntityTypeBuilder<LawyerProfile> builder)
    {
        builder.HasKey(lp => lp.Id);

        builder.Property(lp => lp.LicenseNumber)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(lp => lp.BarAssociation)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(lp => lp.Province)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(lp => lp.Specialty)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(lp => lp.VerificationStatus)
            .IsRequired()
            .HasConversion<string>();

        builder.HasOne(lp => lp.User)
            .WithOne(u => u.LawyerProfile)
            .HasForeignKey<LawyerProfile>(lp => lp.UserId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}