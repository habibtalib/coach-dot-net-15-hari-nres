namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Department lookup (Jabatan). Lookup tables replace hard-coded dropdown values so
/// a SystemAdmin can maintain them without a code change.
/// </summary>
public class LookupDepartment
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}

/// <summary>Grade lookup (Gred), e.g. Gred 41.</summary>
public class LookupGrade
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}

/// <summary>Position lookup (Jawatan), e.g. Pegawai Tadbir.</summary>
public class LookupPosition
{
    public int Id { get; set; }

    public string Code { get; set; } = string.Empty;

    public string Name { get; set; } = string.Empty;

    public bool IsActive { get; set; } = true;

    public int SortOrder { get; set; }
}
