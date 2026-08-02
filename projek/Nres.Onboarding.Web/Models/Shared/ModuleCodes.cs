namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Module prefixes used for reference numbers and for <see cref="Submission.ModuleCode"/>.
/// The values come straight from SPEC-KURSUS.md and must not be changed - reference
/// numbers already issued would stop matching their module.
/// Only <see cref="LaporDiri"/> is used by this reference project; the remaining codes
/// are listed so participants can see the full intended shape of the system.
/// </summary>
public static class ModuleCodes
{
    // Kumpulan 1
    public const string LaporDiri = "LD";

    // Kumpulan 2
    public const string PasKeselamatan = "PAS";
    public const string Parkir = "PKR";
    public const string PelekatKenderaan = "STK";

    // Kumpulan 3
    public const string IdAdEmail = "ICT-ID";

    // Kumpulan 4
    public const string Perisian = "SW";
    public const string PinjamanAset = "AST-L";
    public const string PemulanganAset = "AST-R";
}

/// <summary>
/// Identity role names shared by every module. All six roles exist from Day 3 even
/// though Module 1 only exercises <see cref="Applicant"/> and <see cref="HrAdmin"/>.
/// </summary>
public static class ApplicationRoles
{
    public const string Applicant = "Applicant";
    public const string Supervisor = "Supervisor";
    public const string HrAdmin = "HrAdmin";
    public const string SecurityAdmin = "SecurityAdmin";
    public const string IctAdmin = "IctAdmin";
    public const string SystemAdmin = "SystemAdmin";

    public static readonly string[] All =
    [
        Applicant,
        Supervisor,
        HrAdmin,
        SecurityAdmin,
        IctAdmin,
        SystemAdmin
    ];
}
