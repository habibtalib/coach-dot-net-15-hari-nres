using Nres.Onboarding.Web.Models.Shared;
using System.ComponentModel.DataAnnotations;

namespace Nres.Onboarding.Web.Models.LaporDiri;

/// <summary>
/// Module 1 (Lapor Diri / Officer Reporting) detail record.
/// One-to-one with <see cref="Submission"/>: <see cref="SubmissionId"/> carries a unique
/// index so a submission can never have two Lapor Diri detail rows. The parent holds the
/// workflow (status, reference number, dates); this table holds only the module fields.
/// </summary>
public class OfficerReportingApplication
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    /// <summary>IC / identity card number.</summary>
    [Required]
    [StringLength(20)]
    public string IdentityNo { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    public int DepartmentId { get; set; }

    public int PositionId { get; set; }

    public int GradeId { get; set; }

    [DataType(DataType.Date)]
    public DateTime ReportingDate { get; set; }

    [StringLength(200)]
    public string? PreviousAgency { get; set; }

    [StringLength(200)]
    public string? EmergencyContact { get; set; }

    // Navigation properties
    public Submission? Submission { get; set; }

    public LookupDepartment? Department { get; set; }

    public LookupPosition? Position { get; set; }

    public LookupGrade? Grade { get; set; }
}
