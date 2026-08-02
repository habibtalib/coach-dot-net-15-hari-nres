using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Data;

/// <summary>
/// Single DbContext for the whole application. It inherits <see cref="IdentityDbContext{TUser}"/>
/// so the Identity tables (AspNetUsers, AspNetRoles, ...) and the business tables share one
/// database and one transaction scope.
/// Plain <see cref="IdentityUser"/> is used on purpose: staff details belong in
/// <see cref="UserProfile"/>, not in the authentication table.
///
/// ⚠️ FAIL INI BEKU SELEPAS HARI 3.
///
/// Modules do NOT add DbSet properties here, and they do NOT add Fluent API code
/// here either. Each entity carries its own IEntityTypeConfiguration&lt;T&gt; inside
/// its module folder, and the single ApplyConfigurationsFromAssembly() call below
/// discovers them automatically.
///
/// Access module entities with context.Set&lt;T&gt;().
/// See KOLABORASI.md §3.2.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    // Shared foundation only - do NOT add module DbSets here.
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

    // Lookups
    public DbSet<LookupDepartment> LookupDepartments => Set<LookupDepartment>();
    public DbSet<LookupGrade> LookupGrades => Set<LookupGrade>();
    public DbSet<LookupPosition> LookupPositions => Set<LookupPosition>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Always call the base implementation first: it maps the Identity tables.
        base.OnModelCreating(modelBuilder);

        // ONE line that finds EVERY IEntityTypeConfiguration<T> in this assembly -
        // including the ones all four teams add during Fasa 2. This is exactly why
        // this file never has to change again.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
