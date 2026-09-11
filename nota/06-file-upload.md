# Muat Naik Fail Selamat 📎

> Nota konsep untuk **Hari 3** (modul Lapor Diri — lampiran sokongan). Lihat [`04-validation-viewmodels.md`](./04-validation-viewmodels.md) untuk validation borang am, dan [`09-keselamatan.md`](./09-keselamatan.md) untuk prinsip keselamatan keseluruhan.

---

## Kenapa muat naik fail berisiko?

Fungsi muat naik fail ialah salah satu **permukaan serangan** paling biasa dalam aplikasi web:

- Fail berniat jahat (skrip, *executable*) disamarkan sebagai dokumen.
- Nama fail yang dihantar pengguna boleh mengandungi aksara berbahaya (`../../etc/passwd`, `<script>`).
- Fail besar boleh digunakan untuk serangan *denial-of-service* (penuhkan cakera).
- Jika fail disimpan dalam `wwwroot` (folder awam), sesiapa boleh capai terus melalui URL tanpa *authorization*.

Modul Lapor Diri NRES (Hari 3) memerlukan muat naik lampiran (cth. salinan IC, sijil) — jadi corak selamat ini mesti difahami sebelum menulis kod muat naik.

---

## Prinsip #1: Simpan di luar `wwwroot`

```text
Nres.Onboarding.Web/
  wwwroot/              ← boleh diakses TERUS oleh browser (URL awam) — JANGAN simpan lampiran di sini
  App_Data/
    uploads/
      {submissionId}/   ← lampiran disimpan di sini, TIDAK boleh diakses terus melalui URL
        3f2a9c1e.pdf
        8b7d0a22.jpg
```

Kerana `App_Data/uploads/` berada **di luar** `wwwroot`, ia tidak boleh dicapai terus oleh *browser* melalui URL statik. Untuk memuat turun/paparkan fail, mesti melalui satu Action Controller yang **menyemak *authorization*** dahulu:

```csharp
[Authorize]
public class AttachmentController : Controller
{
    private readonly IFileStorageService _fileStorage;

    [HttpGet("Attachment/Download/{id}")]
    public async Task<IActionResult> Download(int id)
    {
        var attachment = await _context.Attachments.FindAsync(id);
        if (attachment is null) return NotFound();

        // semak: adakah pengguna semasa dibenarkan lihat Submission ini?
        if (!await _authService.CanViewSubmissionAsync(attachment.SubmissionId, User))
            return Forbid();

        var stream = _fileStorage.OpenRead(attachment.StoredFileName, attachment.SubmissionId);
        return File(stream, attachment.ContentType, attachment.OriginalFileName);
    }
}
```

---

## Prinsip #2: Validasi saiz & jenis fail

```csharp
public class FileUploadValidator
{
    private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
    private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5MB

    public static (bool IsValid, string? Error) Validate(IFormFile file)
    {
        if (file.Length == 0)
            return (false, "Fail kosong.");

        if (file.Length > MaxFileSizeBytes)
            return (false, "Saiz fail melebihi had 5MB.");

        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!AllowedExtensions.Contains(extension))
            return (false, "Jenis fail tidak dibenarkan. Hanya PDF, JPG, PNG.");

        return (true, null);
    }
}
```

> **Amaran penting:** semak sambungan fail (*extension*) sahaja **tidak mencukupi** — penyerang boleh namakan semula fail `.exe` kepada `.pdf`. Untuk keselamatan lanjutan (di luar skop asas kursus), semak juga **magic bytes**/*file signature* sebenar kandungan fail, bukan hanya nama.

---

## Prinsip #3: Jangan sesekali percaya nama fail yang dimuat naik

Nama fail asal (`OriginalFileName`) yang dihantar pengguna **hanya untuk paparan** — jangan sesekali guna terus sebagai nama fail fizikal di cakera server. Ia boleh mengandungi:

- Aksara *path traversal* (`../../../Program.cs`)
- Aksara istimewa OS yang menyebabkan ralat/kelakuan tidak dijangka
- Nama fail pendua yang menimpa fail sedia ada

### Jana nama simpanan selamat (`StoredFileName`)

```csharp
public class FileStorageService : IFileStorageService
{
    private readonly string _uploadRoot;   // App_Data/uploads

    public async Task<string> SaveAsync(IFormFile file, int submissionId)
    {
        var safeExtension = Path.GetExtension(file.FileName).ToLowerInvariant();
        var storedFileName = $"{Guid.NewGuid():N}{safeExtension}";  // nama rawak, TIDAK guna nama asal

        var folder = Path.Combine(_uploadRoot, submissionId.ToString());
        Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, storedFileName);
        await using var stream = new FileStream(fullPath, FileMode.Create);
        await file.CopyToAsync(stream);

        return storedFileName;
    }
}
```

> `Guid.NewGuid()` menjamin nama fail unik & tidak boleh diteka — mengelakkan pertindihan nama dan serangan *path traversal* kerana input pengguna langsung tidak digunakan dalam laluan fail.

---

## Prinsip #4: Simpan metadata dalam `Attachment`

Nama fail asal, jenis kandungan, dan saiz **disimpan dalam pangkalan data** (bukan bergantung pada nama fizikal fail), supaya boleh dipaparkan semula dengan tepat kepada pengguna:

```csharp
public class Attachment
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public string OriginalFileName { get; set; } = string.Empty;  // untuk paparan sahaja
    public string StoredFileName { get; set; } = string.Empty;    // nama fizikal sebenar (GUID)
    public string ContentType { get; set; } = string.Empty;
    public long FileSizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
    public string UploadedByUserId { get; set; } = string.Empty;
}
```

### Controller — gabungkan semua prinsip

```csharp
[HttpPost]
public async Task<IActionResult> UploadAttachment(int submissionId, IFormFile file)
{
    var (isValid, error) = FileUploadValidator.Validate(file);
    if (!isValid)
    {
        ModelState.AddModelError(nameof(file), error!);
        return View();
    }

    var storedFileName = await _fileStorage.SaveAsync(file, submissionId);

    _context.Attachments.Add(new Attachment
    {
        SubmissionId = submissionId,
        OriginalFileName = file.FileName,
        StoredFileName = storedFileName,
        ContentType = file.ContentType,
        FileSizeBytes = file.Length,
        UploadedByUserId = _currentUserService.UserId
    });
    await _context.SaveChangesAsync();

    await _auditLogService.LogAsync(submissionId, "AttachmentUploaded", _currentUserService.UserId);

    return RedirectToAction(nameof(Details), new { id = submissionId });
}
```

---

## Senarai Semak Muat Naik Fail

- [ ] Simpan di luar `wwwroot` (`App_Data/uploads/{submissionId}/`)
- [ ] Semak saiz fail maksimum
- [ ] Semak sambungan/jenis fail dibenarkan
- [ ] Jana nama fail simpanan (GUID) — jangan guna nama asal pengguna
- [ ] Simpan metadata (`OriginalFileName`, `StoredFileName`, `ContentType`, saiz) dalam `Attachment`
- [ ] Muat turun hanya melalui Action Controller yang menyemak *authorization*
- [ ] Rekod `AuditLog` untuk setiap muat naik

---

## Kaitan dengan hari-hari lain

- **Hari 3** — muat naik lampiran pertama (modul Lapor Diri).
- **Hari 4–6** — lampiran turut digunakan untuk permohonan Pas/Parking/Pelekat.
- Lihat [`09-keselamatan.md`](./09-keselamatan.md) untuk senarai keselamatan menyeluruh, dan [`07-testing-xunit.md`](./07-testing-xunit.md) untuk cara uji validation muat naik.

---

## Sumber Rasmi

- **[Upload files in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads)**
- **[Security considerations for file uploads](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads#security-considerations)**
