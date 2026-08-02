using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.LaporDiri.Configurations;

/// <summary>
/// Kumpulan 1 owns this file. It lives in the module folder, not in the shared
/// DbContext - which is what lets four teams add entities in parallel.
/// </summary>
public class OfficerReportingApplicationConfiguration
    : IEntityTypeConfiguration<OfficerReportingApplication>
{
    public void Configure(EntityTypeBuilder<OfficerReportingApplication> builder)
    {
        builder.ToTable("OfficerReportingApplications");

        builder.Property(a => a.FullName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.IdentityNo).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Phone).HasMaxLength(30).IsRequired();
        builder.Property(a => a.PreviousAgency).HasMaxLength(200);
        builder.Property(a => a.EmergencyContact).HasMaxLength(200);

        // One-to-one with the parent submission, enforced by a unique index.
        builder.HasIndex(a => a.SubmissionId).IsUnique();

        builder.HasOne(a => a.Submission)
            .WithOne()
            .HasForeignKey<OfficerReportingApplication>(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Department).WithMany()
            .HasForeignKey(a => a.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Position).WithMany()
            .HasForeignKey(a => a.PositionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Grade).WithMany()
            .HasForeignKey(a => a.GradeId).OnDelete(DeleteBehavior.Restrict);
    }
}
