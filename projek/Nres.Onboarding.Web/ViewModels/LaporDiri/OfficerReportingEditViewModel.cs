using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.LaporDiri;

/// <summary>
/// Form model for editing an existing draft. Adds the identifiers and the read-only
/// workflow fields the edit screen needs on top of the create form.
/// </summary>
public class OfficerReportingEditViewModel : OfficerReportingCreateViewModel
{
    /// <summary>Parent <see cref="Submission"/> id - every route in this module is keyed by it.</summary>
    public int SubmissionId { get; set; }

    public string ReferenceNo { get; set; } = string.Empty;

    public SubmissionStatus Status { get; set; }

    /// <summary>Files already attached to this draft.</summary>
    public IReadOnlyList<AttachmentViewModel> Attachments { get; set; } = [];
}
