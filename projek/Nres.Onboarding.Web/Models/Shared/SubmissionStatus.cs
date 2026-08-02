namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Single status enum shared by every module in the system.
/// Do not create a per-module copy of this enum - one workflow vocabulary keeps
/// reporting, filtering and the audit trail consistent across all five modules.
/// </summary>
public enum SubmissionStatus
{
    Draft = 0,
    Submitted = 1,
    SupervisorApproved = 2,
    AdminApproved = 3,
    Rejected = 4,
    Completed = 5,
    Cancelled = 6
}
