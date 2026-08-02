using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.LaporDiri;

/// <summary>One row in the applicant list or the HR review queue.</summary>
public class OfficerReportingListItemViewModel
{
    public int SubmissionId { get; set; }

    public string ReferenceNo { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    public string DepartmentName { get; set; } = string.Empty;

    public DateTime ReportingDate { get; set; }

    public SubmissionStatus Status { get; set; }

    public DateTime CreatedAt { get; set; }

    public DateTime? SubmittedAt { get; set; }

    public int AttachmentCount { get; set; }

    /// <summary>Reference number, or a clear placeholder while the record is still a draft.</summary>
    public string ReferenceDisplay =>
        string.IsNullOrWhiteSpace(ReferenceNo) ? "(draf - belum dihantar)" : ReferenceNo;
}
