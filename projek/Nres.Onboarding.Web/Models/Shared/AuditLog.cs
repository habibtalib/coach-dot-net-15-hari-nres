namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Append-only history of everything that happened to a <see cref="Submission"/>.
/// Rows are never updated or deleted - that is what makes the trail trustworthy.
/// </summary>
public class AuditLog
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    /// <summary>Identity user id of the person who performed the action.</summary>
    public string ActorUserId { get; set; } = string.Empty;

    /// <summary>Action name. Use the constants in <see cref="AuditActions"/>.</summary>
    public string Action { get; set; } = string.Empty;

    /// <summary>Status before the change. Null for actions that do not move status.</summary>
    public SubmissionStatus? FromStatus { get; set; }

    /// <summary>Status after the change. Null for actions that do not move status.</summary>
    public SubmissionStatus? ToStatus { get; set; }

    public string? Remarks { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Submission? Submission { get; set; }
}

/// <summary>
/// Well known audit action names. Using constants instead of loose strings keeps
/// the audit trail queryable and prevents typos such as "Submited".
/// </summary>
public static class AuditActions
{
    public const string Created = "Created";
    public const string Updated = "Updated";
    public const string AttachmentUploaded = "AttachmentUploaded";
    public const string Submitted = "Submitted";
    public const string Approved = "Approved";
    public const string Rejected = "Rejected";
}
