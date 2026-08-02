using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.LaporDiri;

/// <summary>Read-only projection of one attachment row for display.</summary>
public class AttachmentViewModel
{
    public int Id { get; set; }

    public string OriginalFileName { get; set; } = string.Empty;

    public long FileSizeBytes { get; set; }

    public DateTime UploadedAt { get; set; }

    public string SizeDisplay => FileSizeBytes < 1024
        ? $"{FileSizeBytes} B"
        : FileSizeBytes < 1024 * 1024
            ? $"{FileSizeBytes / 1024.0:0.#} KB"
            : $"{FileSizeBytes / (1024.0 * 1024.0):0.#} MB";
}

/// <summary>Read-only projection of one audit row for display.</summary>
public class AuditLogViewModel
{
    public string Action { get; set; } = string.Empty;

    public string ActorDisplayName { get; set; } = string.Empty;

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; }
}

/// <summary>Full detail screen for one Lapor Diri application.</summary>
public class OfficerReportingDetailsViewModel
{
    public int SubmissionId { get; set; }

    public string ReferenceNo { get; set; } = string.Empty;

    public SubmissionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    public string ApplicantDisplayName { get; set; } = string.Empty;

    [Display(Name = "Nama Penuh")]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "No. Kad Pengenalan")]
    public string IdentityNo { get; set; } = string.Empty;

    [Display(Name = "Emel")]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "No. Telefon")]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Jabatan")]
    public string DepartmentName { get; set; } = string.Empty;

    [Display(Name = "Jawatan")]
    public string PositionName { get; set; } = string.Empty;

    [Display(Name = "Gred")]
    public string GradeName { get; set; } = string.Empty;

    [Display(Name = "Tarikh Lapor Diri")]
    [DataType(DataType.Date)]
    public DateTime ReportingDate { get; set; }

    [Display(Name = "Agensi Terdahulu")]
    public string? PreviousAgency { get; set; }

    [Display(Name = "Hubungan Kecemasan")]
    public string? EmergencyContact { get; set; }

    public IReadOnlyList<AttachmentViewModel> Attachments { get; set; } = [];

    public IReadOnlyList<AuditLogViewModel> AuditTrail { get; set; } = [];

    /// <summary>True when the signed-in user owns this submission and it is still a draft.</summary>
    public bool CanEdit { get; set; }

    /// <summary>True when the signed-in user may submit this draft for review.</summary>
    public bool CanSubmit { get; set; }

    /// <summary>True when the signed-in user is an HR admin and the submission awaits a decision.</summary>
    public bool CanReview { get; set; }
}
