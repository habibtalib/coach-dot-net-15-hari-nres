namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Shared parent record for every application in the system, regardless of module.
/// Module specific data lives in its own detail table (for example
/// <see cref="OfficerReportingApplication"/>) which points back here through
/// <c>SubmissionId</c>. Keeping the parent module-agnostic is what allows one
/// status enum, one audit trail and one attachment table to serve all modules.
/// </summary>
public class Submission
{
    public int Id { get; set; }

    /// <summary>Human readable reference, e.g. <c>LD-2026-0001</c>. Empty until submitted.</summary>
    public string ReferenceNo { get; set; } = string.Empty;

    /// <summary>Module prefix, e.g. <c>LD</c>. See <see cref="ModuleCodes"/>.</summary>
    public string ModuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Identity user id of the applicant. Deliberately a plain string (no foreign key)
    /// so the workflow tables stay decoupled from the authentication schema.
    /// </summary>
    public string ApplicantUserId { get; set; } = string.Empty;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public DateTime? SubmittedAt { get; set; }

    public DateTime? CompletedAt { get; set; }

    // Navigation properties
    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();

    public ICollection<ApprovalStep> ApprovalSteps { get; set; } = new List<ApprovalStep>();
}
