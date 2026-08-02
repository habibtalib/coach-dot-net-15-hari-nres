using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Shared.Configurations;

// Each entity carries its own configuration in its own file, inside the folder
// that owns it. ApplicationDbContext discovers them all with a single
// ApplyConfigurationsFromAssembly() call - which is why four teams can add
// entities for eleven days without ever editing the DbContext.
// See KOLABORASI.md §3.2.

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submissions");

        builder.Property(s => s.ReferenceNo).HasMaxLength(30).IsRequired();
        builder.Property(s => s.ModuleCode).HasMaxLength(10).IsRequired();
        builder.Property(s => s.ApplicantUserId).HasMaxLength(450).IsRequired();
        builder.Property(s => s.Status).HasConversion<int>();

        // A draft has no reference number yet, so several rows legitimately hold "".
        // A filtered unique index keeps issued numbers unique without blocking drafts.
        builder.HasIndex(s => s.ReferenceNo)
            .IsUnique()
            .HasFilter("\"ReferenceNo\" <> ''")
            .HasDatabaseName("IX_Submissions_ReferenceNo");

        // Supports the "my submissions" and "review queue" screens of every module.
        builder.HasIndex(s => new { s.ModuleCode, s.Status });
        builder.HasIndex(s => new { s.ApplicantUserId, s.ModuleCode });
    }
}

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.Property(a => a.OriginalFileName).HasMaxLength(260).IsRequired();
        builder.Property(a => a.StoredFileName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(a => a.UploadedByUserId).HasMaxLength(450);

        builder.HasOne(a => a.Submission)
            .WithMany(s => s.Attachments)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.SubmissionId);
    }
}

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.Property(a => a.ActorUserId).HasMaxLength(450).IsRequired();
        builder.Property(a => a.Action).HasMaxLength(60).IsRequired();
        builder.Property(a => a.Remarks).HasMaxLength(1000);

        builder.HasOne(a => a.Submission)
            .WithMany(s => s.AuditLogs)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.SubmissionId, a.CreatedAt });
    }
}

public class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("ApprovalSteps");

        builder.Property(a => a.RoleRequired).HasMaxLength(60);
        builder.Property(a => a.ApproverUserId).HasMaxLength(450);
        builder.Property(a => a.DecidedByUserId).HasMaxLength(450);
        builder.Property(a => a.Remarks).HasMaxLength(1000);
        builder.Property(a => a.Decision).HasConversion<int>();

        builder.HasOne(a => a.Submission)
            .WithMany(s => s.ApprovalSteps)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // One row per position in the route.
        builder.HasIndex(a => new { a.SubmissionId, a.StepOrder }).IsUnique();
    }
}

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.Property(p => p.UserId).HasMaxLength(450).IsRequired();
        builder.Property(p => p.FullName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.IdentityNo).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Phone).HasMaxLength(30);

        // Exactly one profile per Identity user.
        builder.HasIndex(p => p.UserId).IsUnique();

        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Lookups are reference data: block deletion while still referenced.
        builder.HasOne(p => p.Department).WithMany()
            .HasForeignKey(p => p.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Position).WithMany()
            .HasForeignKey(p => p.PositionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Grade).WithMany()
            .HasForeignKey(p => p.GradeId).OnDelete(DeleteBehavior.Restrict);
    }
}

public class LookupDepartmentConfiguration : IEntityTypeConfiguration<LookupDepartment>
{
    public void Configure(EntityTypeBuilder<LookupDepartment> builder)
    {
        builder.ToTable("LookupDepartments");
        builder.Property(l => l.Code).HasMaxLength(20).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();
    }
}

public class LookupGradeConfiguration : IEntityTypeConfiguration<LookupGrade>
{
    public void Configure(EntityTypeBuilder<LookupGrade> builder)
    {
        builder.ToTable("LookupGrades");
        builder.Property(l => l.Code).HasMaxLength(20).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();
    }
}

public class LookupPositionConfiguration : IEntityTypeConfiguration<LookupPosition>
{
    public void Configure(EntityTypeBuilder<LookupPosition> builder)
    {
        builder.ToTable("LookupPositions");
        builder.Property(l => l.Code).HasMaxLength(20).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();
    }
}
