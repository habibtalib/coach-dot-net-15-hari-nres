namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Metadata for one uploaded file. The bytes themselves live on disk under
/// <c>App_Data/uploads/{SubmissionId}/</c> - never inside the database and never
/// under <c>wwwroot</c>, so files can only be reached through a controller action
/// that performs an authorisation check first.
/// </summary>
public class Attachment
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }

    /// <summary>The name the browser sent. Kept for display only - never used to build a path.</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Server generated safe file name (GUID + original extension).</summary>
    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;

    public long SizeBytes { get; set; }

    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Identity user id of whoever uploaded the file.</summary>
    public string UploadedByUserId { get; set; } = string.Empty;

    // Navigation property
    public Submission? Submission { get; set; }
}
