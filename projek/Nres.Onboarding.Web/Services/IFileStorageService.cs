namespace Nres.Onboarding.Web.Services;

/// <summary>Everything needed to create an <see cref="Models.Attachment"/> row after a save.</summary>
public sealed record StoredFileInfo(
    string OriginalFileName,
    string StoredFileName,
    string ContentType,
    long FileSizeBytes);

/// <summary>
/// Saves and reads uploaded files. Files live under <c>App_Data/uploads/{submissionId}/</c>,
/// which is outside <c>wwwroot</c>, so they are not statically servable and can only be
/// reached through a controller action that authorises the caller first.
/// </summary>
public interface IFileStorageService
{
    /// <summary>Writes the upload to disk under a freshly generated safe name.</summary>
    Task<StoredFileInfo> SaveAsync(int submissionId, IFormFile file, CancellationToken cancellationToken = default);

    /// <summary>
    /// Opens a previously stored file for reading.
    /// Only the server generated <paramref name="storedFileName"/> is accepted - never a
    /// client supplied path.
    /// </summary>
    Stream OpenRead(int submissionId, string storedFileName);

    /// <summary>Deletes a stored file. Missing files are ignored.</summary>
    void Delete(int submissionId, string storedFileName);
}

/// <summary>Rules applied to every upload in the system.</summary>
public static class FileUploadRules
{
    /// <summary>5 MB, per the course guide.</summary>
    public const long MaxFileSizeBytes = 5 * 1024 * 1024;

    /// <summary>
    /// Allow-list, not a block-list. A block-list is always incomplete; an allow-list
    /// fails closed on anything unexpected.
    /// </summary>
    public static readonly string[] AllowedExtensions = [".pdf", ".jpg", ".jpeg", ".png", ".doc", ".docx"];

    public static readonly string[] AllowedContentTypes =
    [
        "application/pdf",
        "image/jpeg",
        "image/png",
        "application/msword",
        "application/vnd.openxmlformats-officedocument.wordprocessingml.document"
    ];

    public static string AllowedExtensionsDisplay => string.Join(", ", AllowedExtensions);

    /// <summary>
    /// Validates size, extension and content type. Returns <c>false</c> and an error
    /// message the controller can push into <c>ModelState</c>.
    /// </summary>
    public static bool TryValidate(IFormFile? file, out string error)
    {
        error = string.Empty;

        if (file is null || file.Length == 0)
        {
            error = "Sila pilih fail yang sah.";
            return false;
        }

        if (file.Length > MaxFileSizeBytes)
        {
            error = "Saiz fail tidak boleh melebihi 5 MB.";
            return false;
        }

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

        if (!AllowedExtensions.Contains(extension))
        {
            error = $"Jenis fail tidak dibenarkan. Yang dibenarkan: {AllowedExtensionsDisplay}.";
            return false;
        }

        if (!AllowedContentTypes.Contains(file.ContentType, StringComparer.OrdinalIgnoreCase))
        {
            error = "Kandungan fail tidak sepadan dengan jenis fail yang dibenarkan.";
            return false;
        }

        return true;
    }
}

/// <inheritdoc cref="IFileStorageService" />
public class FileStorageService : IFileStorageService
{
    private readonly string _rootPath;
    private readonly ILogger<FileStorageService> _logger;

    public FileStorageService(IWebHostEnvironment environment, ILogger<FileStorageService> logger)
    {
        // ContentRootPath is the project folder - wwwroot is WebRootPath, which we avoid.
        _rootPath = Path.Combine(environment.ContentRootPath, "App_Data", "uploads");
        _logger = logger;
    }

    public async Task<StoredFileInfo> SaveAsync(
        int submissionId,
        IFormFile file,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(file);

        if (!FileUploadRules.TryValidate(file, out var error))
        {
            throw new InvalidOperationException(error);
        }

        var folder = GetSubmissionFolder(submissionId);
        Directory.CreateDirectory(folder);

        // The browser supplied name is never used to build a path: it can contain
        // "../", a device name, or an executable extension. A GUID plus the validated
        // extension is safe by construction.
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid():N}{extension}";
        var fullPath = Path.Combine(folder, storedFileName);

        await using (var stream = new FileStream(fullPath, FileMode.CreateNew, FileAccess.Write, FileShare.None))
        {
            await file.CopyToAsync(stream, cancellationToken);
        }

        _logger.LogInformation(
            "Stored attachment for submission {SubmissionId} as {StoredFileName} ({Bytes} bytes).",
            submissionId,
            storedFileName,
            file.Length);

        return new StoredFileInfo(
            OriginalFileName: Path.GetFileName(file.FileName),
            StoredFileName: storedFileName,
            ContentType: file.ContentType,
            FileSizeBytes: file.Length);
    }

    public Stream OpenRead(int submissionId, string storedFileName)
    {
        var fullPath = ResolveStoredPath(submissionId, storedFileName);

        if (!File.Exists(fullPath))
        {
            throw new FileNotFoundException("Stored file not found.", storedFileName);
        }

        return new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read);
    }

    public void Delete(int submissionId, string storedFileName)
    {
        var fullPath = ResolveStoredPath(submissionId, storedFileName);

        if (File.Exists(fullPath))
        {
            File.Delete(fullPath);
        }
    }

    private string GetSubmissionFolder(int submissionId) =>
        Path.Combine(_rootPath, submissionId.ToString());

    /// <summary>
    /// Turns a stored file name into a full path, rejecting anything that is not a bare
    /// file name. This is the last line of defence against path traversal.
    /// </summary>
    private string ResolveStoredPath(int submissionId, string storedFileName)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(storedFileName);

        if (!string.Equals(Path.GetFileName(storedFileName), storedFileName, StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Invalid stored file name.");
        }

        var folder = GetSubmissionFolder(submissionId);
        var fullPath = Path.GetFullPath(Path.Combine(folder, storedFileName));

        // Belt and braces: the resolved path must still sit under the uploads root.
        if (!fullPath.StartsWith(Path.GetFullPath(_rootPath), StringComparison.Ordinal))
        {
            throw new InvalidOperationException("Resolved path escapes the uploads folder.");
        }

        return fullPath;
    }
}
