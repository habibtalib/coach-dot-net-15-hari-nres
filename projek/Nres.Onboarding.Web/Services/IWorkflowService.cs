using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services;

/// <summary>
/// Owns the rules for moving a <see cref="Submission"/> from one status to another,
/// and writes the audit entry atomically with the change.
///
/// Every module uses this. Do NOT write status transitions directly in a controller:
/// that is how four modules end up with four slightly different sets of rules, three
/// of which have gaps.
/// </summary>
public interface IWorkflowService
{
    bool CanTransition(SubmissionStatus from, SubmissionStatus to);

    /// <summary>Change status AND write the audit log as one operation.</summary>
    Task TransitionAsync(Submission submission, SubmissionStatus to, string action,
        string? remarks = null, CancellationToken ct = default);
}

public class WorkflowService(ApplicationDbContext db, IAuditLogService audit)
    : IWorkflowService
{
    /// <summary>
    /// Allowed transitions, declared ONCE so all four modules enforce the same rules.
    ///
    /// Rejected, Completed and Cancelled are terminal - which is what stops a second
    /// admin from overwriting a decision that was already made.
    /// </summary>
    private static readonly Dictionary<SubmissionStatus, SubmissionStatus[]> Allowed = new()
    {
        [SubmissionStatus.Draft] =
            [SubmissionStatus.Submitted, SubmissionStatus.Cancelled],

        [SubmissionStatus.Submitted] =
            [SubmissionStatus.SupervisorApproved, SubmissionStatus.AdminApproved,
             SubmissionStatus.Rejected, SubmissionStatus.Cancelled],

        // Two-stage route (Kumpulan 3) passes through here.
        [SubmissionStatus.SupervisorApproved] =
            [SubmissionStatus.AdminApproved, SubmissionStatus.Rejected,
             SubmissionStatus.Cancelled],

        [SubmissionStatus.AdminApproved] =
            [SubmissionStatus.Completed, SubmissionStatus.Cancelled],

        [SubmissionStatus.Rejected] = [],
        [SubmissionStatus.Completed] = [],
        [SubmissionStatus.Cancelled] = []
    };

    public bool CanTransition(SubmissionStatus from, SubmissionStatus to) =>
        Allowed.TryGetValue(from, out var valid) && valid.Contains(to);

    public async Task TransitionAsync(Submission submission, SubmissionStatus to,
        string action, string? remarks = null, CancellationToken ct = default)
    {
        var from = submission.Status;

        if (!CanTransition(from, to))
        {
            throw new InvalidOperationException(
                $"Peralihan tidak sah: {from} → {to} bagi " +
                $"{(string.IsNullOrEmpty(submission.ReferenceNo) ? "(draf)" : submission.ReferenceNo)}");
        }

        submission.Status = to;
        if (to == SubmissionStatus.Submitted) submission.SubmittedAt = DateTime.UtcNow;
        if (to is SubmissionStatus.Completed or SubmissionStatus.AdminApproved)
            submission.CompletedAt ??= DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await audit.LogAsync(submission.Id, action, from, to, remarks, ct);
    }
}
