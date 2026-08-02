using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Data;

/// <summary>
/// Applies pending migrations and seeds the reference data the application cannot start
/// without: lookups, the seven roles, and two demo accounts.
/// Every step is idempotent, so running the application repeatedly never duplicates data.
/// </summary>
public static class DbInitializer
{
    /// <summary>Demo password for the seeded training accounts. Never do this in production.</summary>
    public const string DemoPassword = "Password123!";

    public const string DemoApplicantEmail = "applicant@nres.demo";
    public const string DemoHrAdminEmail = "hradmin@nres.demo";

    public static async Task SeedAsync(IServiceProvider services, CancellationToken cancellationToken = default)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var logger = services.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DbInitializer));

        // Creates the SQLite file and applies every migration that has not run yet.
        await db.Database.MigrateAsync(cancellationToken);

        await SeedLookupsAsync(db, cancellationToken);
        await SeedRolesAsync(roleManager, logger);
        await SeedDemoUsersAsync(db, userManager, logger, cancellationToken);

        logger.LogInformation("Database initialisation complete.");
    }

    private static async Task SeedLookupsAsync(ApplicationDbContext db, CancellationToken cancellationToken)
    {
        if (!await db.LookupDepartments.AnyAsync(cancellationToken))
        {
            db.LookupDepartments.AddRange(
                new LookupDepartment { Code = "PENTADBIRAN", Name = "Jabatan Pentadbiran", SortOrder = 1 },
                new LookupDepartment { Code = "KEWANGAN", Name = "Jabatan Kewangan", SortOrder = 2 },
                new LookupDepartment { Code = "ICT", Name = "Jabatan ICT", SortOrder = 3 },
                new LookupDepartment { Code = "HR", Name = "Jabatan Sumber Manusia", SortOrder = 4 });
        }

        if (!await db.LookupGrades.AnyAsync(cancellationToken))
        {
            db.LookupGrades.AddRange(
                new LookupGrade { Code = "N19", Name = "Gred N19", SortOrder = 1 },
                new LookupGrade { Code = "N22", Name = "Gred N22", SortOrder = 2 },
                new LookupGrade { Code = "41", Name = "Gred 41", SortOrder = 3 },
                new LookupGrade { Code = "44", Name = "Gred 44", SortOrder = 4 },
                new LookupGrade { Code = "48", Name = "Gred 48", SortOrder = 5 });
        }

        if (!await db.LookupPositions.AnyAsync(cancellationToken))
        {
            db.LookupPositions.AddRange(
                new LookupPosition { Code = "PT", Name = "Pegawai Tadbir", SortOrder = 1 },
                new LookupPosition { Code = "PPT", Name = "Penolong Pegawai Tadbir", SortOrder = 2 },
                new LookupPosition { Code = "JK", Name = "Juruteknik Komputer", SortOrder = 3 },
                new LookupPosition { Code = "PSM", Name = "Pegawai Sumber Manusia", SortOrder = 4 });
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    private static async Task SeedRolesAsync(RoleManager<IdentityRole> roleManager, ILogger logger)
    {
        // All seven roles exist from day one even though Module 1 only uses two of them.
        // Later modules can then be added without another data migration.
        foreach (var role in ApplicationRoles.All)
        {
            if (await roleManager.RoleExistsAsync(role))
            {
                continue;
            }

            var result = await roleManager.CreateAsync(new IdentityRole(role));

            if (!result.Succeeded)
            {
                logger.LogError(
                    "Failed to create role {Role}: {Errors}",
                    role,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
            }
        }
    }

    private static async Task SeedDemoUsersAsync(
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager,
        ILogger logger,
        CancellationToken cancellationToken)
    {
        var hrDepartment = await db.LookupDepartments.FirstAsync(d => d.Code == "HR", cancellationToken);
        var adminDepartment = await db.LookupDepartments.FirstAsync(d => d.Code == "PENTADBIRAN", cancellationToken);
        var officerPosition = await db.LookupPositions.FirstAsync(p => p.Code == "PT", cancellationToken);
        var hrPosition = await db.LookupPositions.FirstAsync(p => p.Code == "PSM", cancellationToken);
        var grade41 = await db.LookupGrades.FirstAsync(g => g.Code == "41", cancellationToken);
        var grade44 = await db.LookupGrades.FirstAsync(g => g.Code == "44", cancellationToken);

        await EnsureUserAsync(
            db, userManager, logger, cancellationToken,
            email: DemoApplicantEmail,
            role: ApplicationRoles.Applicant,
            fullName: "Ahmad bin Ismail",
            identityNo: "900101101234",
            phone: "012-3456789",
            departmentId: adminDepartment.Id,
            positionId: officerPosition.Id,
            gradeId: grade41.Id);

        await EnsureUserAsync(
            db, userManager, logger, cancellationToken,
            email: DemoHrAdminEmail,
            role: ApplicationRoles.HrAdmin,
            fullName: "Siti binti Rahman",
            identityNo: "850505105678",
            phone: "013-7654321",
            departmentId: hrDepartment.Id,
            positionId: hrPosition.Id,
            gradeId: grade44.Id);
    }

    private static async Task EnsureUserAsync(
        ApplicationDbContext db,
        UserManager<IdentityUser> userManager,
        ILogger logger,
        CancellationToken cancellationToken,
        string email,
        string role,
        string fullName,
        string identityNo,
        string phone,
        int departmentId,
        int positionId,
        int gradeId)
    {
        var user = await userManager.FindByNameAsync(email);

        if (user is null)
        {
            user = new IdentityUser
            {
                UserName = email,
                Email = email,
                // Skips the confirmation e-mail flow, which has no mail server in training.
                EmailConfirmed = true
            };

            var result = await userManager.CreateAsync(user, DemoPassword);

            if (!result.Succeeded)
            {
                logger.LogError(
                    "Failed to create demo user {Email}: {Errors}",
                    email,
                    string.Join("; ", result.Errors.Select(e => e.Description)));
                return;
            }
        }

        if (!await userManager.IsInRoleAsync(user, role))
        {
            await userManager.AddToRoleAsync(user, role);
        }

        // The staff details live in UserProfile, not in AspNetUsers.
        var hasProfile = await db.UserProfiles.AnyAsync(p => p.UserId == user.Id, cancellationToken);

        if (!hasProfile)
        {
            db.UserProfiles.Add(new UserProfile
            {
                UserId = user.Id,
                FullName = fullName,
                IdentityNo = identityNo,
                Phone = phone,
                DepartmentId = departmentId,
                PositionId = positionId,
                GradeId = gradeId
            });

            await db.SaveChangesAsync(cancellationToken);
        }
    }
}
