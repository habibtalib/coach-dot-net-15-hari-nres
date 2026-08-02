namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>Decision recorded against a single <see cref="ApprovalStep"/>.</summary>
public enum ApprovalDecision
{
    Pending = 0,
    Approved = 1,
    Rejected = 2
}

/// <summary>
/// One step in a submission's approval route.
/// Lapor Diri (Module 1) only ever creates a single step for the <c>HrAdmin</c> role,
/// but the shape is generic on purpose: the multi-step modules built from Day 7
/// onwards (supervisor then ICT, for example) reuse this same table by inserting
/// several rows with increasing <see cref="StepOrder"/>.
/// </summary>
public class ApprovalStep
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    /// <summary>1-based position in the route. Steps are actioned in ascending order.</summary>
    public int StepOrder { get; set; }

    /// <summary>Role allowed to decide this step, e.g. <c>HrAdmin</c>. Null when a named approver is used.</summary>
    public string? RoleRequired { get; set; }

    /// <summary>Specific approver, when the route targets a person rather than a role.</summary>
    public string? ApproverUserId { get; set; }

    public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;

    public string? DecidedByUserId { get; set; }

    public DateTime? DecidedAt { get; set; }

    public string? Remarks { get; set; }

    // Navigation property
    public Submission? Submission { get; set; }
}
