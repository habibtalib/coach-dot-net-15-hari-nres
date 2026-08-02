# Lab · Kumpulan 2 · Hari 5–6 — Borang & Semakan Pendua

> Konsep: [`../README.md`](../README.md) · Kanun: [`../../../SPEC-KURSUS.md`](../../../SPEC-KURSUS.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok

```bash
git switch kump-2/akses-kenderaan
git pull --rebase origin master
git switch -c kump-2/feat/borang-dan-pendua
dotnet build
```

**Semakan "sudah wujud?"**

```bash
grep -rn "IReferenceNumberService\|IWorkflowService" Nres.Onboarding.Web/Services/
grep -rn "SubmissionControllerBase" Nres.Onboarding.Web/Controllers/
```

**Prompt AI hari ini:**

```text
Merujuk AGENTS.md: saya Kumpulan 2, modul Pas/Parkir/Pelekat. Saya perlu
menyekat permohonan pelekat pendua bagi nombor plat yang sama.
Adakah repo ini sudah ada apa-apa untuk semakan pendua atau normalisasi
nombor plat? Jika ya, beritahu di mana. JANGAN tulis kod baharu.
```

> Jawapan yang betul: `Vehicle.Normalize` wujud (Hari 4, milik anda); semakan pendua **belum** wujud dan khusus modul anda — jadi anda membinanya dalam `Services/Akses/`.

### ✅ Semakan

- [ ] Servis kongsi disahkan wujud
- [ ] AI menunjuk ke `Vehicle.Normalize` sedia ada
- [ ] Anda pada cabang ciri

---

## Latihan 1 — View model pas dengan validation bersyarat

**Objektif:** Peraturan yang berubah mengikut jenis pas.

### Langkah

`ViewModels/Akses/AccessPassFormViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Akses;

/// <summary>
/// Melaksanakan IValidatableObject kerana peraturan kami merentas MEDAN:
/// "Tujuan lawatan wajib BILA jenis pas ialah Pelawat atau Kontraktor".
/// [Required] tidak boleh menyatakan itu.
/// </summary>
public class AccessPassFormViewModel : IValidatableObject
{
    public int? Id { get; set; }

    [Display(Name = "Jenis pas")]
    public JenisPas JenisPas { get; set; } = JenisPas.Staf;

    [Display(Name = "Nama pemegang pas")]
    [Required(ErrorMessage = "Nama pemegang pas wajib diisi.")]
    [StringLength(200)]
    public string HolderName { get; set; } = string.Empty;

    [Display(Name = "No. kad pengenalan pemegang")]
    [Required(ErrorMessage = "No. kad pengenalan wajib diisi.")]
    [RegularExpression(@"^\d{6}-\d{2}-\d{4}$", ErrorMessage = "Format: 010203-14-5678")]
    public string HolderIdentityNo { get; set; } = string.Empty;

    [Display(Name = "Tujuan lawatan")]
    [StringLength(500)]
    public string? PurposeOfVisit { get; set; }

    [Display(Name = "Nama syarikat")]
    [StringLength(200)]
    public string? CompanyName { get; set; }

    [Display(Name = "Sah dari")]
    [DataType(DataType.Date)]
    public DateTime? ValidFrom { get; set; }

    [Display(Name = "Sah hingga")]
    [DataType(DataType.Date)]
    public DateTime? ValidTo { get; set; }

    public string? ReferenceNo { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public bool IsEditable { get; set; } = true;

    /// <summary>
    /// Peraturan bersyarat. Dijalankan SELEPAS validation atribut lulus.
    /// Ini berjalan di PELAYAN — pelayar boleh dipintas.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        // Tujuan lawatan: wajib untuk Pelawat & Kontraktor.
        if (JenisPas is JenisPas.Pelawat or JenisPas.Kontraktor
            && string.IsNullOrWhiteSpace(PurposeOfVisit))
        {
            yield return new ValidationResult(
                "Tujuan lawatan wajib diisi untuk pas Pelawat dan Kontraktor.",
                [nameof(PurposeOfVisit)]);
        }

        // Nama syarikat: wajib untuk Kontraktor.
        if (JenisPas is JenisPas.Kontraktor && string.IsNullOrWhiteSpace(CompanyName))
        {
            yield return new ValidationResult(
                "Nama syarikat wajib diisi untuk pas Kontraktor.",
                [nameof(CompanyName)]);
        }

        // Tarikh mesti masuk akal.
        if (ValidFrom is not null && ValidTo is not null && ValidTo < ValidFrom)
        {
            yield return new ValidationResult(
                "Tarikh 'sah hingga' tidak boleh lebih awal daripada 'sah dari'.",
                [nameof(ValidTo)]);
        }

        // Had tempoh berbeza mengikut jenis pas.
        if (ValidFrom is not null && ValidTo is not null)
        {
            var hari = (ValidTo.Value - ValidFrom.Value).TotalDays;

            var had = JenisPas switch
            {
                JenisPas.Pelawat    => 7,
                JenisPas.Kontraktor => 90,
                _                   => (int?)null    // Staf: tiada had
            };

            if (had is not null && hari > had)
            {
                yield return new ValidationResult(
                    $"Tempoh pas {JenisPas} tidak boleh melebihi {had} hari.",
                    [nameof(ValidTo)]);
            }
        }
    }
}
```

> **Perhatikan:** `Validate()` menggunakan `yield return`, jadi ia melaporkan **semua** masalah sekaligus. Mengembalikan awal selepas ralat pertama memaksa pengguna membetulkan satu perkara pada satu masa.

### ✅ Semakan

- [ ] View model dalam `ViewModels/Akses/`
- [ ] Melaksanakan `IValidatableObject`
- [ ] Peraturan meliputi: tujuan, syarikat, susunan tarikh, had tempoh
- [ ] Semua ralat dilaporkan sekaligus (`yield return`)

---

## Latihan 2 — Controller pas keselamatan

**Objektif:** Cipta, sunting, simpan draf — mewarisi kelas asas kongsi.

### Langkah

`Controllers/AccessPassController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels.Akses;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class AccessPassController(
    ApplicationDbContext db,
    IWorkflowService workflow,
    INotificationService notifications,
    ICurrentUserService currentUser,
    IReferenceNumberService referenceNumbers)
    : SubmissionControllerBase(db, workflow, notifications)
{
    protected override string ModuleCode => ModuleCodes.PasKeselamatan;
    protected override string AdminRole => "SecurityAdmin";

    [HttpGet]
    public IActionResult Create() => View("Form", new AccessPassFormViewModel());

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var app = await Db.Set<AccessPassApplication>()
            .Include(a => a.Submission)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (app is null) return NotFound();
        if (app.Submission!.ApplicantUserId != currentUser.UserId
            && !currentUser.IsInRole(AdminRole)) return Forbid();

        return View("Form", KeViewModel(app));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(AccessPassFormViewModel vm)
    {
        // Draf: cukup untuk mengenal pasti rekod sahaja.
        if (string.IsNullOrWhiteSpace(vm.HolderName))
        {
            ModelState.Clear();
            ModelState.AddModelError(nameof(vm.HolderName),
                "Nama pemegang pas diperlukan walaupun untuk draf.");
            return View("Form", vm);
        }
        ModelState.Clear();

        var app = await MuatAtauCiptaAsync(vm);
        if (app is null) return Forbid();

        SalinKeEntiti(vm, app);
        await Db.SaveChangesAsync();

        TempData["Mesej"] = "Draf disimpan.";
        return RedirectToAction(nameof(Edit), new { id = app.Id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Submit(AccessPassFormViewModel vm)
    {
        if (vm.Id is null) return NotFound();

        var app = await Db.Set<AccessPassApplication>()
            .Include(a => a.Submission)
            .FirstOrDefaultAsync(a => a.Id == vm.Id);

        if (app is null) return NotFound();
        if (app.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();
        if (app.Submission.Status != SubmissionStatus.Draft)
        {
            TempData["Ralat"] = "Permohonan ini telah pun dihantar.";
            return RedirectToAction(nameof(Edit), new { id = app.Id });
        }

        // Validation PENUH — IValidatableObject berjalan di sini.
        if (!ModelState.IsValid)
        {
            vm.IsEditable = true;
            return View("Form", vm);
        }

        SalinKeEntiti(vm, app);
        await Db.SaveChangesAsync();

        app.Submission.ReferenceNo = await referenceNumbers.GenerateAsync(ModuleCode);
        await Workflow.TransitionAsync(app.Submission, SubmissionStatus.Submitted,
            "Submitted", $"Pas {app.JenisPas} untuk {app.HolderName}");

        TempData["Mesej"] = $"Permohonan dihantar. No. rujukan: {app.Submission.ReferenceNo}";
        return RedirectToAction(nameof(Edit), new { id = app.Id });
    }

    // ----- pembantu -----

    private async Task<AccessPassApplication?> MuatAtauCiptaAsync(AccessPassFormViewModel vm)
    {
        if (vm.Id is null)
        {
            var submission = new Submission
            {
                ModuleCode = ModuleCode,
                ApplicantUserId = currentUser.UserId!,
                Status = SubmissionStatus.Draft
            };
            Db.Submissions.Add(submission);
            await Db.SaveChangesAsync();

            var baharu = new AccessPassApplication { SubmissionId = submission.Id };
            Db.Set<AccessPassApplication>().Add(baharu);
            return baharu;
        }

        var app = await Db.Set<AccessPassApplication>()
            .Include(a => a.Submission)
            .FirstOrDefaultAsync(a => a.Id == vm.Id);

        if (app is null) return null;
        if (app.Submission!.ApplicantUserId != currentUser.UserId) return null;
        if (app.Submission.Status != SubmissionStatus.Draft) return null;

        return app;
    }

    private static void SalinKeEntiti(AccessPassFormViewModel vm, AccessPassApplication app)
    {
        app.JenisPas = vm.JenisPas;
        app.HolderName = vm.HolderName;
        app.HolderIdentityNo = vm.HolderIdentityNo;
        app.PurposeOfVisit = vm.PurposeOfVisit;
        app.CompanyName = vm.CompanyName;
        app.ValidFrom = vm.ValidFrom;
        app.ValidTo = vm.ValidTo;
    }

    private static AccessPassFormViewModel KeViewModel(AccessPassApplication app) => new()
    {
        Id = app.Id,
        JenisPas = app.JenisPas,
        HolderName = app.HolderName,
        HolderIdentityNo = app.HolderIdentityNo,
        PurposeOfVisit = app.PurposeOfVisit,
        CompanyName = app.CompanyName,
        ValidFrom = app.ValidFrom,
        ValidTo = app.ValidTo,
        ReferenceNo = app.Submission?.ReferenceNo,
        Status = app.Submission?.Status ?? SubmissionStatus.Draft,
        IsEditable = app.Submission?.Status == SubmissionStatus.Draft
    };
}
```

### ✅ Semakan

- [ ] Mewarisi `SubmissionControllerBase`
- [ ] **Tiada** `Approve`/`Reject` ditulis
- [ ] Semakan pemilikan pada `Edit`, `SaveDraft`, `Submit`
- [ ] Guna `IReferenceNumberService` kongsi

---

## Latihan 3 — Servis semakan pendua (teras modul anda)

**Objektif:** Sekat permohonan pendua dengan mesej yang berguna.

### Langkah

1. `Services/Akses/IDuplicateCheckService.cs`:

```csharp
namespace Nres.Onboarding.Web.Services.Akses;

/// <summary>Hasil semakan pendua — konflik dengan konteks untuk mesej berguna.</summary>
public record DuplicateHit(string ReferenceNo, string StatusNama, int Tahun);

public interface IDuplicateCheckService
{
    /// <summary>Permohonan pelekat aktif bagi kenderaan ini pada tahun ini.</summary>
    Task<DuplicateHit?> ActiveStickerAsync(int vehicleId, int tahun,
        int? kecualiSubmissionId = null, CancellationToken ct = default);

    /// <summary>Permohonan parkir aktif bagi kenderaan ini.</summary>
    Task<DuplicateHit?> ActiveParkingAsync(int vehicleId,
        int? kecualiSubmissionId = null, CancellationToken ct = default);

    /// <summary>Pas aktif bagi nombor IC pemegang ini.</summary>
    Task<DuplicateHit?> ActivePassAsync(string holderIdentityNo,
        int? kecualiSubmissionId = null, CancellationToken ct = default);
}
```

2. `Services/Akses/DuplicateCheckService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Akses;

public class DuplicateCheckService(ApplicationDbContext db) : IDuplicateCheckService
{
    /// <summary>
    /// Status yang MENYEKAT permohonan baharu.
    ///
    /// Rejected, Cancelled dan Completed TIDAK disenaraikan dengan sengaja:
    /// permohonan yang ditolak mesti boleh dibetulkan dan dihantar semula.
    /// Menyekat pada mana-mana permohonan sedia ada ialah pepijat paling
    /// biasa dalam ciri ini — ia mengunci kenderaan selamanya.
    /// </summary>
    private static readonly SubmissionStatus[] StatusAktif =
    [
        SubmissionStatus.Submitted,
        SubmissionStatus.SupervisorApproved,
        SubmissionStatus.AdminApproved
    ];

    public async Task<DuplicateHit?> ActiveStickerAsync(int vehicleId, int tahun,
        int? kecualiSubmissionId = null, CancellationToken ct = default)
    {
        var q = from a in db.Set<VehicleStickerApplication>().AsNoTracking()
                join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
                where a.VehicleId == vehicleId
                   && a.TahunPelekat == tahun
                   && StatusAktif.Contains(s.Status)
                select new { s.ReferenceNo, s.Status, s.Id, a.TahunPelekat };

        if (kecualiSubmissionId is not null)
            q = q.Where(x => x.Id != kecualiSubmissionId);

        var hit = await q.FirstOrDefaultAsync(ct);
        return hit is null ? null
            : new DuplicateHit(hit.ReferenceNo, NamaStatus(hit.Status), hit.TahunPelekat);
    }

    public async Task<DuplicateHit?> ActiveParkingAsync(int vehicleId,
        int? kecualiSubmissionId = null, CancellationToken ct = default)
    {
        var q = from a in db.Set<ParkingApplication>().AsNoTracking()
                join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
                where a.VehicleId == vehicleId && StatusAktif.Contains(s.Status)
                select new { s.ReferenceNo, s.Status, s.Id };

        if (kecualiSubmissionId is not null)
            q = q.Where(x => x.Id != kecualiSubmissionId);

        var hit = await q.FirstOrDefaultAsync(ct);
        return hit is null ? null
            : new DuplicateHit(hit.ReferenceNo, NamaStatus(hit.Status), DateTime.UtcNow.Year);
    }

    public async Task<DuplicateHit?> ActivePassAsync(string holderIdentityNo,
        int? kecualiSubmissionId = null, CancellationToken ct = default)
    {
        var q = from a in db.Set<AccessPassApplication>().AsNoTracking()
                join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
                where a.HolderIdentityNo == holderIdentityNo
                   && StatusAktif.Contains(s.Status)
                select new { s.ReferenceNo, s.Status, s.Id };

        if (kecualiSubmissionId is not null)
            q = q.Where(x => x.Id != kecualiSubmissionId);

        var hit = await q.FirstOrDefaultAsync(ct);
        return hit is null ? null
            : new DuplicateHit(hit.ReferenceNo, NamaStatus(hit.Status), DateTime.UtcNow.Year);
    }

    private static string NamaStatus(SubmissionStatus s) => s switch
    {
        SubmissionStatus.Submitted          => "Dihantar",
        SubmissionStatus.SupervisorApproved => "Lulus Penyelia",
        SubmissionStatus.AdminApproved      => "Diluluskan",
        _                                   => s.ToString()
    };
}
```

3. Daftar dalam modul anda:

```csharp
services.AddScoped<IDuplicateCheckService, DuplicateCheckService>();
```

### ✅ Semakan

- [ ] `StatusAktif` **tidak** termasuk `Rejected`/`Cancelled`
- [ ] Parameter `kecualiSubmissionId` wujud (supaya permohonan tidak menyekat dirinya sendiri)
- [ ] Mengembalikan nombor rujukan konflik, bukan hanya `true`/`false`
- [ ] Didaftar dalam `AksesModule`

---

## Latihan 4 — Borang pelekat dengan pendaftaran kenderaan & semakan pendua

**Objektif:** Satukan `IVehicleService` dan `IDuplicateCheckService`.

### Langkah

1. `ViewModels/Akses/VehicleStickerFormViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Akses;

public class VehicleStickerFormViewModel
{
    public int? Id { get; set; }

    [Display(Name = "No. pendaftaran kenderaan")]
    [Required(ErrorMessage = "No. pendaftaran kenderaan wajib diisi.")]
    [StringLength(20)]
    public string PlateNumber { get; set; } = string.Empty;

    [Display(Name = "Jenis kenderaan")]
    public JenisKenderaan Jenis { get; set; } = JenisKenderaan.Kereta;

    [Display(Name = "Jenama")]
    [StringLength(60)]
    public string? Jenama { get; set; }

    [Display(Name = "Model")]
    [StringLength(60)]
    public string? Model { get; set; }

    [Display(Name = "Warna")]
    [StringLength(40)]
    public string? Warna { get; set; }

    [Display(Name = "Tahun pelekat")]
    [Range(2020, 2100, ErrorMessage = "Tahun pelekat tidak sah.")]
    public int TahunPelekat { get; set; } = DateTime.UtcNow.Year;

    [Display(Name = "Salinan geran/kad pendaftaran dilampirkan")]
    public bool GeranDilampirkan { get; set; }

    public string? ReferenceNo { get; set; }
    public string? StickerSerialNo { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public bool IsEditable { get; set; } = true;
}
```

2. `Controllers/VehicleStickerController.cs` — bahagian penting ialah `Submit`:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Submit(VehicleStickerFormViewModel vm)
{
    if (vm.Id is null) return NotFound();

    var app = await Db.Set<VehicleStickerApplication>()
        .Include(a => a.Submission)
        .Include(a => a.Vehicle)
        .FirstOrDefaultAsync(a => a.Id == vm.Id);

    if (app is null) return NotFound();
    if (app.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();
    if (app.Submission.Status != SubmissionStatus.Draft)
    {
        TempData["Ralat"] = "Permohonan ini telah pun dihantar.";
        return RedirectToAction(nameof(Edit), new { id = app.Id });
    }

    if (!ModelState.IsValid) return View("Form", vm);

    // --- 1. Daftar atau dapatkan kenderaan ---
    Vehicle kenderaan;
    try
    {
        kenderaan = await vehicles.RegisterOrGetAsync(
            currentUser.UserId!, vm.PlateNumber, vm.Jenis,
            vm.Jenama, vm.Model, vm.Warna);
    }
    catch (InvalidOperationException ex)
    {
        // Plat didaftar oleh staf LAIN — isu keselamatan, bukan pendua biasa.
        ModelState.AddModelError(nameof(vm.PlateNumber), ex.Message);
        return View("Form", vm);
    }

    // --- 2. SEMAKAN PENDUA — teras modul kami ---
    var pendua = await duplicates.ActiveStickerAsync(
        kenderaan.Id, vm.TahunPelekat, kecualiSubmissionId: app.SubmissionId);

    if (pendua is not null)
    {
        ModelState.AddModelError(string.Empty,
            $"Kenderaan {kenderaan.PlateNumber} sudah mempunyai permohonan pelekat " +
            $"aktif untuk tahun {pendua.Tahun} ({pendua.ReferenceNo}, status: " +
            $"{pendua.StatusNama}). Sila semak permohonan tersebut atau batalkannya " +
            "sebelum memohon semula.");
        return View("Form", vm);
    }

    // --- 3. Simpan, jana rujukan, hantar ---
    app.VehicleId = kenderaan.Id;
    app.TahunPelekat = vm.TahunPelekat;
    app.GeranDilampirkan = vm.GeranDilampirkan;
    await Db.SaveChangesAsync();

    app.Submission.ReferenceNo = await referenceNumbers.GenerateAsync(
        ModuleCodes.PelekatKenderaan);

    await Workflow.TransitionAsync(app.Submission, SubmissionStatus.Submitted,
        "Submitted", $"Pelekat {vm.TahunPelekat} untuk {kenderaan.PlateNumber}");

    TempData["Mesej"] = $"Permohonan dihantar. No. rujukan: {app.Submission.ReferenceNo}";
    return RedirectToAction(nameof(Edit), new { id = app.Id });
}
```

> **Perhatikan `kecualiSubmissionId`.** Tanpanya, menghantar semula permohonan yang sama akan mendapati dirinya sendiri sebagai pendua.

3. `ModuleCode` bagi controller ini ialah `ModuleCodes.PelekatKenderaan`, dan `AdminRole` ialah `SecurityAdmin`.

### ✅ Semakan

- [ ] Kenderaan didaftarkan secara automatik daripada borang
- [ ] Plat milik staf lain memberi mesej keselamatan yang jelas
- [ ] Pendua disekat dengan **nombor rujukan konflik dalam mesej**
- [ ] `kecualiSubmissionId` dilalukan
- [ ] Permohonan yang **ditolak** tidak menyekat permohonan baharu

---

## Latihan 5 — Borang parkir

**Objektif:** Corak yang sama, dengan justifikasi bersyarat.

### Langkah

1. View model dengan `IValidatableObject`:

```csharp
public IEnumerable<ValidationResult> Validate(ValidationContext context)
{
    // Justifikasi wajib untuk semua kecuali parkir Biasa.
    if (JenisParkir != JenisParkir.Biasa && string.IsNullOrWhiteSpace(Justifikasi))
    {
        yield return new ValidationResult(
            $"Justifikasi wajib diisi untuk permohonan parkir {JenisParkir}.",
            [nameof(Justifikasi)]);
    }

    // Parkir OKU memerlukan dokumen sokongan (disemak pada Hari 7–9).
    if (JenisParkir == JenisParkir.OKU && !DokumenOkuDilampirkan)
    {
        yield return new ValidationResult(
            "Kad OKU atau surat pengesahan perlu dilampirkan.",
            [nameof(DokumenOkuDilampirkan)]);
    }
}
```

2. `Submit` mengikut corak yang sama seperti pelekat, tetapi memanggil `ActiveParkingAsync`:

```csharp
var pendua = await duplicates.ActiveParkingAsync(
    kenderaan.Id, kecualiSubmissionId: app.SubmissionId);

if (pendua is not null)
{
    ModelState.AddModelError(string.Empty,
        $"Kenderaan {kenderaan.PlateNumber} sudah mempunyai permohonan parkir aktif " +
        $"({pendua.ReferenceNo}, status: {pendua.StatusNama}).");
    return View("Form", vm);
}
```

3. **Jangan** letak `LotNumber` pada borang — ia diperuntukkan semasa kelulusan (Hari 7–9).

### ✅ Semakan

- [ ] Justifikasi wajib untuk OKU/Eksekutif/Sementara, bukan Biasa
- [ ] Semakan pendua parkir berfungsi
- [ ] `LotNumber` **tiada** pada borang pemohon

---

## Latihan 6 — Razor view (tiga borang)

**Objektif:** Borang yang menunjukkan medan bersyarat.

### Langkah

1. `Views/AccessPass/Form.cshtml` — bahagian penting ialah medan bersyarat:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Akses.AccessPassFormViewModel
@using Nres.Onboarding.Web.Models.Akses
@{ ViewData["Title"] = Model.Id is null ? "Permohonan Pas Keselamatan" : "Sunting Permohonan Pas"; }

<h2>@ViewData["Title"]</h2>

@if (TempData["Mesej"] is string m) { <div class="alert alert-success">@m</div> }
@if (TempData["Ralat"] is string r) { <div class="alert alert-danger">@r</div> }

<form asp-action="SaveDraft" method="post">
    @Html.AntiForgeryToken()
    <input type="hidden" asp-for="Id" />
    <div asp-validation-summary="All" class="text-danger mb-3"></div>

    <fieldset disabled="@(!Model.IsEditable)">
        <div class="row g-3">
            <div class="col-md-4">
                <label asp-for="JenisPas" class="form-label"></label>
                <select asp-for="JenisPas" class="form-select" id="jenisPas"
                        asp-items="Html.GetEnumSelectList<JenisPas>()"></select>
            </div>
            <div class="col-md-4">
                <label asp-for="HolderName" class="form-label"></label>
                <input asp-for="HolderName" class="form-control" />
                <span asp-validation-for="HolderName" class="text-danger"></span>
            </div>
            <div class="col-md-4">
                <label asp-for="HolderIdentityNo" class="form-label"></label>
                <input asp-for="HolderIdentityNo" class="form-control"
                       placeholder="010203-14-5678" />
                <span asp-validation-for="HolderIdentityNo" class="text-danger"></span>
            </div>

            <div class="col-md-6" id="blokTujuan">
                <label asp-for="PurposeOfVisit" class="form-label"></label>
                <textarea asp-for="PurposeOfVisit" class="form-control" rows="2"></textarea>
                <span asp-validation-for="PurposeOfVisit" class="text-danger"></span>
            </div>

            <div class="col-md-6" id="blokSyarikat">
                <label asp-for="CompanyName" class="form-label"></label>
                <input asp-for="CompanyName" class="form-control" />
                <span asp-validation-for="CompanyName" class="text-danger"></span>
            </div>

            <div class="col-md-3">
                <label asp-for="ValidFrom" class="form-label"></label>
                <input asp-for="ValidFrom" type="date" class="form-control" />
            </div>
            <div class="col-md-3">
                <label asp-for="ValidTo" class="form-label"></label>
                <input asp-for="ValidTo" type="date" class="form-control" />
                <span asp-validation-for="ValidTo" class="text-danger"></span>
                <div class="form-text" id="petunjukTempoh"></div>
            </div>
        </div>

        <div class="mt-4">
            <button type="submit" class="btn btn-secondary">Simpan Draf</button>
            @if (Model.Id is not null)
            {
                <button type="submit" formaction="@Url.Action("Submit")"
                        class="btn btn-primary"
                        onclick="return confirm('Hantar permohonan?');">
                    Hantar Permohonan
                </button>
            }
            <a asp-controller="Akses" asp-action="Index" class="btn btn-link">Kembali</a>
        </div>
    </fieldset>
</form>

@if (!Model.IsEditable)
{
    <div class="alert alert-success mt-4">
        <strong>No. Rujukan: @Model.ReferenceNo</strong> —
        <partial name="_StatusBadge" model="Model.Status" />
    </div>
}

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script>
        // Petunjuk UI sahaja — validation SEBENAR berlaku di pelayan
        // dalam IValidatableObject. Ini hanya mengurangkan kekeliruan.
        function kemasKiniMedan() {
            const jenis = document.getElementById('jenisPas').value;
            const syarikat = document.getElementById('blokSyarikat');
            const petunjuk = document.getElementById('petunjukTempoh');

            // 1 = Staf, 2 = Pelawat, 3 = Kontraktor
            syarikat.style.display = (jenis === '3') ? '' : 'none';
            petunjuk.textContent =
                jenis === '2' ? 'Maksimum 7 hari untuk pas Pelawat.' :
                jenis === '3' ? 'Maksimum 90 hari untuk pas Kontraktor.' : '';
        }
        document.getElementById('jenisPas').addEventListener('change', kemasKiniMedan);
        kemasKiniMedan();
    </script>
}
```

> **Perhatikan penggunaan `formaction`** — satu borang, dua butang hantar. Ini lebih bersih daripada medan tersembunyi. (Kumpulan 1 menggunakan pendekatan medan tersembunyi; bandingkan keduanya.)

> **Perhatikan komen dalam JavaScript.** Menyembunyikan medan ialah petunjuk UI, **bukan** validation. Pelayan sentiasa menyemak semula.

2. Bina `Views/VehicleSticker/Form.cshtml` dan `Views/Parking/Form.cshtml` mengikut corak yang sama.

### ✅ Semakan

- [ ] Ketiga-tiga borang dipaparkan dan menyimpan draf
- [ ] Medan syarikat disembunyikan kecuali jenis Kontraktor
- [ ] Menghantar pas Kontraktor tanpa syarikat **ditolak di pelayan**
- [ ] Guna `_StatusBadge` kongsi

---

## Latihan 7 — Uji semakan pendua secara menyeluruh

**Objektif:** Buktikan peraturan betul, termasuk kes yang mesti **dibenarkan**.

### Langkah

Jalankan setiap senario dan rekod dalam `docs/kumpulan-2/ujian-manual.md`:

| # | Senario | Jangkaan | Keputusan |
|---|---------|----------|-----------|
| 1 | Mohon pelekat 2026 untuk WXY1234 | Berjaya, `STK-2026-####` | |
| 2 | Mohon **lagi** pelekat 2026 untuk WXY1234 | **Disekat**, mesej menamakan STK sebelumnya | |
| 3 | Mohon pelekat **2027** untuk WXY1234 | **Berjaya** — tahun berbeza | |
| 4 | Tolak permohonan #1, kemudian mohon semula 2026 | **Berjaya** — ditolak tidak menyekat | |
| 5 | Batalkan permohonan, kemudian mohon semula | **Berjaya** | |
| 6 | Taip `wxy 1234` (huruf kecil, ruang) | Dikenali sebagai kenderaan **sama** | |
| 7 | Staf lain mohon pelekat untuk WXY1234 | **Disekat**, mesej "hubungi Bahagian Keselamatan" | |
| 8 | Hantar semula draf yang sama dua kali | Tidak menyekat dirinya sendiri | |
| 9 | Mohon parkir untuk kenderaan dengan parkir aktif | **Disekat** | |
| 10 | Pas Kontraktor tanpa nama syarikat | **Ditolak** validation | |
| 11 | Pas Pelawat, tempoh 10 hari | **Ditolak** — maks 7 hari | |
| 12 | Pas Staf, tempoh 200 hari | **Berjaya** — tiada had untuk staf | |

> **Ujian 3, 4, 5, 12 ialah yang paling penting.** Ia mengesahkan anda **tidak** terlalu ketat. Semakan pendua yang menyekat kes sah lebih teruk daripada yang tiada — ia menghalang kerja sebenar.

### ✅ Semakan

- [ ] Kesemua 12 senario diuji
- [ ] Ujian 3, 4, 5, 12 **berjaya** (tidak disekat)
- [ ] Ujian 2, 7, 9, 10, 11 **disekat** dengan mesej berguna
- [ ] Ujian 6 mengenali plat yang dinormalkan
- [ ] Keputusan didokumenkan

---

## Latihan 8 — Tutup blok

```bash
git diff --name-only master     # hanya fail Akses
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Ketiga-tiga borang berfungsi dengan validation bersyarat
- [ ] Semakan pendua menyekat kes sebenar, membenarkan kes sah
- [ ] Mesej ralat menamakan nombor rujukan yang bertindih
- [ ] Validation di **pelayan** (JavaScript hanya petunjuk)
- [ ] Servis kongsi digunakan; tiada logik didup
- [ ] Hanya fail Kumpulan 2 disentuh
- [ ] Ujian manual didokumenkan
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 5–6

| Artifak | Lokasi |
|---------|--------|
| Tiga view model dengan validation bersyarat | `ViewModels/Akses/` |
| `IDuplicateCheckService` | `Services/Akses/` |
| Tiga controller | `Controllers/{AccessPass,VehicleSticker,Parking}Controller.cs` |
| Tiga borang Razor | `Views/{AccessPass,VehicleSticker,Parking}/Form.cshtml` |
| Rekod ujian manual | `docs/kumpulan-2/ujian-manual.md` |

**Seterusnya (Hari 7–9):** skrin Pegawai Keselamatan, kelulusan bersyarat, dan peruntukan lot/pelekat.
