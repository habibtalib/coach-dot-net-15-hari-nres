# Lab · Kumpulan 1 · Hari 5–6 — Lampiran, Nombor Rujukan & Hantar

> Konsep: [`../README.md`](../README.md) · Kanun: [`../../../SPEC-KURSUS.md`](../../../SPEC-KURSUS.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula hari & semakan "sudah wujud?"

**Objektif:** Sahkan anda tidak akan menulis semula apa yang sudah wujud.

### Langkah

```bash
git switch kump-1/lapor-diri
git pull --rebase origin master
git switch -c kump-1/feat/lampiran-dan-hantar
dotnet build
```

**Semakan wajib sebelum menulis apa-apa hari ini:**

```bash
grep -ri "IFileStorageService"     Nres.Onboarding.Web/Services/
grep -ri "IReferenceNumberService" Nres.Onboarding.Web/Services/
grep -ri "_AttachmentList"         Nres.Onboarding.Web/Views/Shared/
```

Ketiga-tiganya **sudah wujud**. Blok ini menggunakannya — anda tidak menulis satu pun.

**Prompt AI hari ini** (jalankan sekali, awal):

```text
Merujuk AGENTS.md: saya Kumpulan 1, modul Lapor Diri. Saya perlu menambah
muat naik dokumen sokongan dan penjanaan nombor rujukan.
Adakah repo ini sudah ada cara untuk kedua-duanya? Jika ya, beritahu di mana
dan bagaimana saya patut menggunakannya. JANGAN tulis kod baharu.
```

### ✅ Semakan

- [ ] Ketiga-tiga komponen kongsi disahkan wujud
- [ ] AI menunjuk ke servis sedia ada, bukan mencadangkan yang baharu
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Jenis dokumen sokongan

**Objektif:** Modelkan dokumen apa yang NRES perlukan.

### Langkah

1. `Models/LaporDiri/DokumenSokongan.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.LaporDiri;

/// <summary>
/// Jenis dokumen sokongan Lapor Diri. Semak senarai ini terhadap URS Hari 1
/// dan jawapan NRES kepada soalan terbuka anda.
/// </summary>
public enum JenisDokumen
{
    KadPengenalan = 1,
    SuratTawaran = 2,
    SijilAkademik = 3,
    SuratAkuanSumpah = 4,
    SlipGajiTerakhir = 5
}

public static class DokumenSokongan
{
    /// <summary>Dokumen yang MESTI ada sebelum permohonan boleh dihantar.</summary>
    public static readonly JenisDokumen[] Wajib =
    [
        JenisDokumen.KadPengenalan,
        JenisDokumen.SuratTawaran,
        JenisDokumen.SijilAkademik
    ];

    public static string Nama(JenisDokumen jenis) => jenis switch
    {
        JenisDokumen.KadPengenalan    => "Salinan Kad Pengenalan",
        JenisDokumen.SuratTawaran     => "Surat Tawaran / Lantikan",
        JenisDokumen.SijilAkademik    => "Sijil Akademik",
        JenisDokumen.SuratAkuanSumpah => "Surat Akuan Sumpah",
        JenisDokumen.SlipGajiTerakhir => "Slip Gaji Terakhir",
        _ => jenis.ToString()
    };
}
```

2. **Masalah:** `Attachment` kongsi tidak mempunyai medan "jenis dokumen". Kita perlukannya untuk mengetahui dokumen wajib mana yang hilang.

   **Salah:** menambah medan ke `Attachment` kongsi — itu fail beku, dan tiga modul lain tidak memerlukan jenis dokumen Lapor Diri.

   **Betul:** jadual detail modul anda sendiri yang memaut ke `Attachment`:

`Models/LaporDiri/OfficerReportingAttachment.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.LaporDiri;

/// <summary>
/// Melanjutkan Attachment kongsi dengan metadata khusus Lapor Diri.
/// Kami TIDAK menambah medan ke Attachment — ia fail beku yang dikongsi
/// empat modul, dan hanya kami memerlukan jenis dokumen.
/// </summary>
public class OfficerReportingAttachment
{
    public int Id { get; set; }

    public int AttachmentId { get; set; }
    public Attachment? Attachment { get; set; }

    public JenisDokumen Jenis { get; set; }
}
```

3. Konfigurasi `Models/LaporDiri/Configurations/OfficerReportingAttachmentConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.LaporDiri.Configurations;

public class OfficerReportingAttachmentConfiguration
    : IEntityTypeConfiguration<OfficerReportingAttachment>
{
    public void Configure(EntityTypeBuilder<OfficerReportingAttachment> builder)
    {
        builder.ToTable("OfficerReportingAttachments");
        builder.Property(a => a.Jenis).HasConversion<int>();

        builder.HasIndex(a => a.AttachmentId).IsUnique();

        builder.HasOne(a => a.Attachment)
            .WithOne()
            .HasForeignKey<OfficerReportingAttachment>(a => a.AttachmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

> **Corak ini penting.** Apabila anda memerlukan sesuatu daripada entiti kongsi yang ia tidak ada, **lanjutkan** ia dengan jadual anda sendiri. Jangan sunting yang kongsi, dan jangan salin ia.

### ✅ Semakan

- [ ] Enum & pembantu dalam `Models/LaporDiri/`
- [ ] `Attachment` kongsi **tidak** diubah suai
- [ ] Konfigurasi dalam folder anda
- [ ] `dotnet build` berjaya

---

## Latihan 2 — Servis lampiran modul

**Objektif:** Bungkus `IFileStorageService` kongsi dengan peraturan Lapor Diri.

### Langkah

1. `Services/LaporDiri/IOfficerReportingAttachmentService.cs`:

```csharp
using Nres.Onboarding.Web.Models.LaporDiri;

namespace Nres.Onboarding.Web.Services.LaporDiri;

public interface IOfficerReportingAttachmentService
{
    Task UploadAsync(int submissionId, JenisDokumen jenis, IFormFile file,
                     CancellationToken ct = default);

    Task<IReadOnlyList<(int AttachmentId, JenisDokumen Jenis, string FileName, long Size)>>
        ListAsync(int submissionId, CancellationToken ct = default);

    /// <summary>Jenis dokumen wajib yang MASIH hilang.</summary>
    Task<IReadOnlyList<JenisDokumen>> MissingRequiredAsync(
        int submissionId, CancellationToken ct = default);

    Task DeleteAsync(int attachmentId, CancellationToken ct = default);
}
```

2. `Services/LaporDiri/OfficerReportingAttachmentService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.LaporDiri;

/// <summary>
/// Membungkus IFileStorageService KONGSI dengan peraturan khusus Lapor Diri.
/// Kami tidak menyentuh cakera sendiri — servis kongsi melakukannya, termasuk
/// semua semakan keselamatan.
/// </summary>
public class OfficerReportingAttachmentService(
    ApplicationDbContext db,
    IFileStorageService storage,
    ICurrentUserService currentUser) : IOfficerReportingAttachmentService
{
    public async Task UploadAsync(int submissionId, JenisDokumen jenis,
        IFormFile file, CancellationToken ct = default)
    {
        // Satu fail setiap jenis: muat naik baharu menggantikan yang lama.
        var sediaAda = await db.Attachments
            .Where(a => a.SubmissionId == submissionId)
            .Join(db.Set<OfficerReportingAttachment>(),
                  a => a.Id, o => o.AttachmentId, (a, o) => new { a, o })
            .FirstOrDefaultAsync(x => x.o.Jenis == jenis, ct);

        if (sediaAda is not null)
        {
            storage.Delete(submissionId, sediaAda.a.StoredFileName);
            db.Set<OfficerReportingAttachment>().Remove(sediaAda.o);
            db.Attachments.Remove(sediaAda.a);
            await db.SaveChangesAsync(ct);
        }

        // Servis kongsi mengendalikan semakan jenis, had saiz, dan nama selamat.
        var (storedFileName, size) = await storage.SaveAsync(submissionId, file, ct);

        var attachment = new Attachment
        {
            SubmissionId = submissionId,
            OriginalFileName = file.FileName,
            StoredFileName = storedFileName,
            ContentType = file.ContentType,
            SizeBytes = size,
            UploadedByUserId = currentUser.UserId ?? string.Empty
        };
        db.Attachments.Add(attachment);
        await db.SaveChangesAsync(ct);

        db.Set<OfficerReportingAttachment>().Add(new OfficerReportingAttachment
        {
            AttachmentId = attachment.Id,
            Jenis = jenis
        });
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<(int, JenisDokumen, string, long)>> ListAsync(
        int submissionId, CancellationToken ct = default)
    {
        return await db.Attachments
            .Where(a => a.SubmissionId == submissionId)
            .Join(db.Set<OfficerReportingAttachment>(),
                  a => a.Id, o => o.AttachmentId,
                  (a, o) => new ValueTuple<int, JenisDokumen, string, long>(
                      a.Id, o.Jenis, a.OriginalFileName, a.SizeBytes))
            .ToListAsync(ct);
    }

    public async Task<IReadOnlyList<JenisDokumen>> MissingRequiredAsync(
        int submissionId, CancellationToken ct = default)
    {
        var ada = await db.Attachments
            .Where(a => a.SubmissionId == submissionId)
            .Join(db.Set<OfficerReportingAttachment>(),
                  a => a.Id, o => o.AttachmentId, (a, o) => o.Jenis)
            .ToListAsync(ct);

        return DokumenSokongan.Wajib.Except(ada).ToList();
    }

    public async Task DeleteAsync(int attachmentId, CancellationToken ct = default)
    {
        var attachment = await db.Attachments
            .FirstOrDefaultAsync(a => a.Id == attachmentId, ct);
        if (attachment is null) return;

        var detail = await db.Set<OfficerReportingAttachment>()
            .FirstOrDefaultAsync(o => o.AttachmentId == attachmentId, ct);

        storage.Delete(attachment.SubmissionId, attachment.StoredFileName);
        if (detail is not null) db.Set<OfficerReportingAttachment>().Remove(detail);
        db.Attachments.Remove(attachment);
        await db.SaveChangesAsync(ct);
    }
}
```

3. Daftarkan dalam **modul anda** — bukan `Program.cs`:

```csharp
// Services/LaporDiri/LaporDiriModule.cs — kemas kini
public static IServiceCollection AddLaporDiriModule(this IServiceCollection services)
{
    services.AddScoped<IModuleDescriptorProvider, LaporDiriModuleDescriptor>();
    services.AddScoped<IOfficerReportingAttachmentService,
                       OfficerReportingAttachmentService>();
    return services;
}
```

### ✅ Semakan

- [ ] Servis dalam `Services/LaporDiri/`
- [ ] Ia **memanggil** `IFileStorageService`, tidak menulis ke cakera sendiri
- [ ] Didaftar dalam `LaporDiriModule`, bukan `Program.cs`
- [ ] `git diff --name-only master` menunjukkan tiada fail kongsi

---

## Latihan 3 — Migration lampiran (slot!)

**Objektif:** Cipta jadual `OfficerReportingAttachments`.

### Langkah

1. Umumkan: *"Kumpulan 1 mengambil slot migration."*

2. ```bash
   git pull --rebase origin master
   cd Nres.Onboarding.Web
   dotnet ef migrations add LaporDiriAttachment
   dotnet ef database update
   cd ..
   ```

3. Sahkan migration hanya menyentuh jadual anda. Lepaskan slot.

### ✅ Semakan

- [ ] Slot diumumkan & dilepaskan
- [ ] Migration hanya `OfficerReportingAttachments`
- [ ] `dotnet ef database update` berjaya

---

## Latihan 4 — Muat naik & muat turun dalam controller

**Objektif:** Muat naik yang berfungsi, dengan muat turun berperanan yang selamat.

### Langkah

1. Tambah ke `OfficerReportingController` — **suntik servis baharu** dahulu:

```csharp
public class OfficerReportingController(
    ApplicationDbContext db,
    IWorkflowService workflow,
    INotificationService notifications,
    ICurrentUserService currentUser,
    IOfficerReportingAttachmentService attachments,
    IFileStorageService fileStorage,
    IReferenceNumberService referenceNumbers)
    : SubmissionControllerBase(db, workflow, notifications)
```

2. Tindakan muat naik:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[RequestSizeLimit(6 * 1024 * 1024)]   // sedikit di atas had 5 MB servis
public async Task<IActionResult> UploadAttachment(
    int id, JenisDokumen jenis, IFormFile file)
{
    var app = await Db.Set<OfficerReportingApplication>()
        .Include(a => a.Submission)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (app is null) return NotFound();
    if (app.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();

    // Selepas dihantar, lampiran dikunci — dikuatkuasakan di PELAYAN.
    if (app.Submission.Status != SubmissionStatus.Draft)
    {
        TempData["Ralat"] = "Permohonan telah dihantar; lampiran tidak boleh diubah.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    if (file is null || file.Length == 0)
    {
        TempData["Ralat"] = "Sila pilih fail.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    try
    {
        await attachments.UploadAsync(app.SubmissionId, jenis, file);
        TempData["Mesej"] = $"{DokumenSokongan.Nama(jenis)} dimuat naik.";
    }
    catch (InvalidOperationException ex)
    {
        // Servis kongsi membuang ini untuk fail terlalu besar / jenis salah.
        TempData["Ralat"] = ex.Message;
    }

    return RedirectToAction(nameof(Edit), new { id });
}
```

3. Muat turun selamat — **ini sebab fail berada di luar `wwwroot`**:

```csharp
[HttpGet]
public async Task<IActionResult> DownloadAttachment(int attachmentId)
{
    var attachment = await Db.Attachments
        .Include(a => a.Submission)
        .FirstOrDefaultAsync(a => a.Id == attachmentId);

    if (attachment is null) return NotFound();

    // KEBENARAN DISEMAK DI SINI — inilah tujuan keseluruhan menyimpan
    // fail di luar wwwroot. Pemohon boleh melihat miliknya; HR boleh
    // melihat semua dalam modul ini; orang lain tidak boleh.
    var milikSaya = attachment.Submission!.ApplicantUserId == currentUser.UserId;
    var sayaHr = currentUser.IsInRole(AdminRole);
    if (!milikSaya && !sayaHr) return Forbid();

    var stream = fileStorage.OpenRead(
        attachment.SubmissionId, attachment.StoredFileName);

    return File(stream, attachment.ContentType, attachment.OriginalFileName);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> DeleteAttachment(int id, int attachmentId)
{
    var app = await Db.Set<OfficerReportingApplication>()
        .Include(a => a.Submission)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (app is null) return NotFound();
    if (app.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();
    if (app.Submission.Status != SubmissionStatus.Draft) return Forbid();

    await attachments.DeleteAsync(attachmentId);
    return RedirectToAction(nameof(Edit), new { id });
}
```

4. **Uji serangan.** Log masuk sebagai `applicant@nres.test`, muat naik fail, catat `attachmentId`. Log keluar, log masuk sebagai pemohon lain, dan lawati `/OfficerReporting/DownloadAttachment?attachmentId=<id>`.

   Anda sepatutnya mendapat **403 Forbidden**. Jika anda mendapat fail, semakan kebenaran anda rosak.

### ✅ Semakan

- [ ] Muat naik berfungsi untuk PDF/JPG/PNG di bawah 5 MB
- [ ] Fail 6 MB ditolak dengan mesej yang jelas
- [ ] Fail `.exe` ditolak
- [ ] **Ujian serangan lulus** — pemohon lain mendapat 403
- [ ] HR boleh memuat turun mana-mana lampiran dalam modul ini
- [ ] Lampiran tidak boleh ditambah selepas hantar

---

## Latihan 5 — Nombor rujukan & hantar

**Objektif:** Penghantaran rasmi dengan validation penuh.

### Langkah

1. Tambah tindakan `Submit`:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Submit(OfficerReportingFormViewModel vm)
{
    if (vm.Id is null) return NotFound();

    var app = await Db.Set<OfficerReportingApplication>()
        .Include(a => a.Submission)
        .FirstOrDefaultAsync(a => a.Id == vm.Id);

    if (app is null) return NotFound();
    if (app.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();
    if (app.Submission.Status != SubmissionStatus.Draft)
    {
        TempData["Ralat"] = "Permohonan ini telah pun dihantar.";
        return RedirectToAction(nameof(Edit), new { id = app.Id });
    }

    // --- 1. Validation PENUH (tidak seperti SaveDraft) ---
    if (!vm.DeclarationAccepted)
        ModelState.AddModelError(nameof(vm.DeclarationAccepted),
            "Anda mesti mengesahkan maklumat sebelum menghantar.");

    // --- 2. Lampiran wajib ---
    var hilang = await attachments.MissingRequiredAsync(app.SubmissionId);
    foreach (var jenis in hilang)
        ModelState.AddModelError(string.Empty,
            $"Dokumen wajib belum dimuat naik: {DokumenSokongan.Nama(jenis)}");

    if (!ModelState.IsValid)
    {
        await IsiDropdownAsync(vm);
        vm.Attachments = await attachments.ListAsync(app.SubmissionId);
        vm.MissingRequired = hilang;
        return View("Form", vm);
    }

    // Simpan perubahan terakhir borang sebelum mengunci.
    SalinKeEntiti(vm, app);
    await Db.SaveChangesAsync();

    // --- 3. Jana nombor rujukan (guna servis KONGSI) ---
    app.Submission.ReferenceNo =
        await referenceNumbers.GenerateAsync(ModuleCode);

    // --- 4 & 5. Peralihan status + audit, secara atomik ---
    await Workflow.TransitionAsync(app.Submission, SubmissionStatus.Submitted,
        "Submitted", $"Dihantar oleh pemohon. {hilang.Count} dokumen hilang: tiada.");

    TempData["Mesej"] =
        $"Permohonan dihantar. No. rujukan anda: {app.Submission.ReferenceNo}";

    return RedirectToAction(nameof(Edit), new { id = app.Id });
}
```

2. Tambah medan sokongan ke view model:

```csharp
// ViewModels/LaporDiri/OfficerReportingFormViewModel.cs — tambah
public IReadOnlyList<(int AttachmentId, JenisDokumen Jenis, string FileName, long Size)>
    Attachments { get; set; } = [];

public IReadOnlyList<JenisDokumen> MissingRequired { get; set; } = [];

public string? ReferenceNo { get; set; }
public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
```

3. Kemas kini `Edit` dan `KeViewModel` untuk mengisinya:

```csharp
// dalam Edit, selepas KeViewModel:
vm.Attachments = await attachments.ListAsync(app.SubmissionId);
vm.MissingRequired = await attachments.MissingRequiredAsync(app.SubmissionId);

// dalam KeViewModel, tambah:
ReferenceNo = app.Submission?.ReferenceNo,
Status = app.Submission?.Status ?? SubmissionStatus.Draft,
```

### ✅ Semakan

- [ ] Menghantar tanpa dokumen wajib ditolak dengan mesej yang menamakannya
- [ ] Menghantar tanpa menanda akuan ditolak
- [ ] Penghantaran berjaya menjana `LD-2026-0001`
- [ ] Permohonan kedua mendapat `LD-2026-0002`
- [ ] Status bertukar kepada `Submitted`
- [ ] Audit log mengandungi baris "Submitted"
- [ ] Menghantar dua kali ditolak

---

## Latihan 6 — UI lampiran & butang hantar

**Objektif:** Selesaikan borang.

### Langkah

1. Tambah ke `Views/OfficerReporting/Form.cshtml`, selepas `</fieldset>`:

```cshtml
</form>

@if (Model.Id is not null)
{
    <hr class="my-4" />
    <h5>Dokumen Sokongan</h5>

    @if (Model.MissingRequired.Any())
    {
        <div class="alert alert-warning">
            <strong>Dokumen wajib belum dimuat naik:</strong>
            <ul class="mb-0">
                @foreach (var j in Model.MissingRequired)
                {
                    <li>@DokumenSokongan.Nama(j)</li>
                }
            </ul>
        </div>
    }

    <table class="table table-sm">
        <thead>
            <tr><th>Dokumen</th><th>Fail</th><th>Saiz</th><th></th></tr>
        </thead>
        <tbody>
        @foreach (var a in Model.Attachments)
        {
            <tr>
                <td>@DokumenSokongan.Nama(a.Jenis)</td>
                <td>
                    <a asp-action="DownloadAttachment"
                       asp-route-attachmentId="@a.AttachmentId">@a.FileName</a>
                </td>
                <td>@(a.Size / 1024) KB</td>
                <td class="text-end">
                    @if (Model.IsEditable)
                    {
                        <form asp-action="DeleteAttachment" method="post" class="d-inline">
                            @Html.AntiForgeryToken()
                            <input type="hidden" name="id" value="@Model.Id" />
                            <input type="hidden" name="attachmentId" value="@a.AttachmentId" />
                            <button class="btn btn-sm btn-outline-danger">Buang</button>
                        </form>
                    }
                </td>
            </tr>
        }
        </tbody>
    </table>

    @if (Model.IsEditable)
    {
        <form asp-action="UploadAttachment" method="post" enctype="multipart/form-data"
              class="row g-2 align-items-end">
            @Html.AntiForgeryToken()
            <input type="hidden" name="id" value="@Model.Id" />
            <div class="col-md-4">
                <label class="form-label">Jenis dokumen</label>
                <select name="jenis" class="form-select">
                    @foreach (JenisDokumen j in Enum.GetValues<JenisDokumen>())
                    {
                        <option value="@((int)j)">@DokumenSokongan.Nama(j)</option>
                    }
                </select>
            </div>
            <div class="col-md-5">
                <label class="form-label">Fail (PDF/JPG/PNG, maks 5 MB)</label>
                <input type="file" name="file" class="form-control"
                       accept=".pdf,.jpg,.jpeg,.png" required />
            </div>
            <div class="col-md-3">
                <button type="submit" class="btn btn-outline-primary w-100">Muat Naik</button>
            </div>
        </form>

        <hr class="my-4" />

        <form asp-action="Submit" method="post"
              onsubmit="return confirm('Hantar permohonan? Ia tidak boleh disunting selepas ini.');">
            @Html.AntiForgeryToken()
            @* Hantar semula medan borang supaya perubahan terakhir disimpan *@
            <input type="hidden" asp-for="Id" />
            <input type="hidden" asp-for="DeclarationAccepted" id="hantarAkuan" />
            <button type="submit" class="btn btn-primary btn-lg">Hantar Permohonan</button>
        </form>
    }
    else
    {
        <div class="alert alert-success">
            <strong>No. Rujukan: @Model.ReferenceNo</strong> —
            <partial name="_StatusBadge" model="Model.Status" />
        </div>
    }
}
```

> **Nota jujur:** menghantar semula medan borang melalui `hidden` seperti di atas berfungsi untuk latihan tetapi kekok. Pendekatan yang lebih bersih ialah satu borang dengan dua butang `submit` yang berbeza `formaction`. Cuba itu jika kumpulan anda ada masa — ia lebih baik.

2. Tambah `@using` di bahagian atas view:

```cshtml
@using Nres.Onboarding.Web.Models.LaporDiri
@using Nres.Onboarding.Web.Models.Shared
```

### ✅ Semakan

- [ ] Senarai lampiran dipaparkan dengan pautan muat turun
- [ ] Dokumen wajib yang hilang disenaraikan dengan jelas
- [ ] Muat naik berfungsi dan menggantikan jenis yang sama
- [ ] Selepas hantar, borang menjadi baca-sahaja dan nombor rujukan dipaparkan
- [ ] Kawalan muat naik hilang selepas hantar

---

## Latihan 7 — Audit trail pada halaman

**Objektif:** Guna partial view kongsi — jangan tulis sendiri.

### Langkah

1. Muatkan audit dalam `Edit`:

```csharp
vm.AuditLogs = await Db.AuditLogs
    .Where(l => l.SubmissionId == app.SubmissionId)
    .OrderByDescending(l => l.CreatedAt)
    .ToListAsync();
```

2. Tambah ke view model:

```csharp
public IReadOnlyList<AuditLog> AuditLogs { get; set; } = [];
```

3. Render dengan partial **kongsi**:

```cshtml
@if (Model.Id is not null && Model.AuditLogs.Any())
{
    <hr class="my-4" />
    <partial name="_AuditTrail" model="Model.AuditLogs" />
}
```

### ✅ Semakan

- [ ] Audit trail dipaparkan menggunakan `_AuditTrail` **kongsi**
- [ ] Anda **tidak** menulis paparan audit sendiri
- [ ] Menghantar menambah baris audit yang kelihatan

---

## Latihan 8 — Tutup blok

### Langkah

1. Semakan diri:

```bash
git diff --name-only master
```

Hanya fail `LaporDiri` + `Migrations/`. Tiada fail kongsi.

2. Semakan AI (prompt semakan dari `nota-ai.md`).

3. PR → review → gabung ke `kump-1/lapor-diri`.

4. **Gabungan latihan ke `master`.**

5. Kemas kini board.

### ✅ Semakan (Definition of Done)

- [ ] Binaan bersih, ciri berfungsi manual
- [ ] Servis kongsi digunakan (`IFileStorageService`, `IReferenceNumberService`, `IWorkflowService`, `_AuditTrail`)
- [ ] Hanya fail Kumpulan 1 disentuh
- [ ] Validation pelayan penuh pada hantar
- [ ] Kebenaran muat turun diuji dengan dua akaun
- [ ] Migration melalui slot
- [ ] Kod jana-AI difahami
- [ ] Disemak rakan sekumpulan
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 5–6

| Artifak | Lokasi |
|---------|--------|
| Jenis dokumen + jadual lanjutan | `Models/LaporDiri/` |
| Servis lampiran modul | `Services/LaporDiri/` |
| Migration `LaporDiriAttachment` | `Migrations/` |
| Muat naik / muat turun / buang | `OfficerReportingController` |
| Hantar + nombor rujukan | `OfficerReportingController.Submit` |
| UI lampiran + butang hantar | `Views/OfficerReporting/Form.cshtml` |

**Seterusnya (Hari 7–9):** dashboard HR, skrin semakan, approve/reject dengan ulasan, dan penapisan.
