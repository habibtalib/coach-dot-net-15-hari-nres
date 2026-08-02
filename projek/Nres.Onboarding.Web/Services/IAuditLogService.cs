using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services;

/// <summary>
/// Records what happened to a submission. Every state change must go through here -
/// an approval with no audit row is indistinguishable from one that never happened.
/// </summary>
public interface IAuditLogService
{
    /// <summary>
    /// Appends an <see cref="AuditLog"/> row for <paramref name="submissionId"/> and saves it.
    ///
    /// The acting user is taken from <see cref="ICurrentUserService"/>, so callers never
    /// pass it in and cannot get it wrong. That matters: if the actor could be supplied
    /// by the caller, a crafted form post could forge who approved something.
    /// </summary>
    Task LogAsync(
        int submissionId,
        string action,
        SubmissionStatus? from = null,
        SubmissionStatus? to = null,
        string? remarks = null,
        CancellationToken ct = default);
}

/// <inheritdoc cref="IAuditLogService" />
public class AuditLogService(ApplicationDbContext db, ICurrentUserService currentUser)
    : IAuditLogService
{
    public async Task LogAsync(
        int submissionId,
        string action,
        SubmissionStatus? from = null,
        SubmissionStatus? to = null,
        string? remarks = null,
        CancellationToken ct = default)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        db.AuditLogs.Add(new AuditLog
        {
            SubmissionId = submissionId,
            ActorUserId = currentUser.UserId ?? "system",
            Action = action,
            FromStatus = from,
            ToStatus = to,
            Remarks = remarks,
            CreatedAt = DateTime.UtcNow
        });

        // Saves immediately. When the caller has already opened a transaction this save
        // joins it, so the audit row and the state change commit or roll back together.
        await db.SaveChangesAsync(ct);
    }
}
