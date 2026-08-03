# Lab · Kumpulan 4 · Hari 5–6 — Borang & Semakan Stok

> Konsep: [`../README.md`](../README.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok

```bash
git switch kump-4/perisian-aset
git pull --rebase origin master
git switch -c kump-4/feat/borang-dan-semakan-stok
dotnet build
```

**Semakan "sudah wujud?"**

```bash
grep -rn "IInventoryService" Nres.Onboarding.Web/Services/Aset/
grep -rn "IReferenceNumberService" Nres.Onboarding.Web/Services/
```

**Prompt AI:**

```text
Merujuk AGENTS.md: saya Kumpulan 4. Saya perlu menyekat permohonan pinjaman
aset pendua (satu pinjaman aktif setiap kategori setiap pemohon).
Adakah repo ini sudah ada semakan pendua yang boleh saya guna semula?
```

> Jawapan: Kumpulan 2 mempunyai `IDuplicateCheckService` dalam `Services/Akses/` — **milik mereka**, khusus nombor plat. Anda memerlukan versi anda sendiri dalam `Services/Aset/`. Coraknya serupa; datanya berbeza. Ini **bukan** duplikasi — ia domain berbeza. Sebutkan dalam semakan silang AI supaya kedua-dua kumpulan sedar.

### ✅ Semakan

- [ ] `IInventoryService` anda dari Hari 4 wujud
- [ ] Anda menilai semakan pendua sebagai khusus modul, dan boleh menyatakan sebabnya
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Semakan kelayakan permohonan

**Objektif:** Peraturan perniagaan dalam satu servis boleh diuji.

### Langkah

`Services/Aset/IEligibilityService.cs`:

```csharp
using Nres.Onboarding.Web.Models.Aset;

namespace Nres.Onboarding.Web.Services.Aset;

public record EligibilityResult(bool Layak, string? Sebab);

public interface IEligibilityService
{
    /// <summary>Bolehkah pengguna ini memohon perisian ini?</summary>
    Task<EligibilityResult> CanRequestSoftwareAsync(string userId, int softwareId,
        int? kecualiSubmissionId = null, CancellationToken ct = default);

    /// <summary>Bolehkah pengguna ini meminjam aset dalam kategori ini?</summary>
    Task<EligibilityResult> CanBorrowAssetAsync(string userId, KategoriAset kategori,
        int? kecualiSubmissionId = null, CancellationToken ct = default);
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Aset;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Aset;

public class EligibilityService(ApplicationDbContext db, IInventoryService inventory)
    : IEligibilityService
{
    /// <summary>
    /// Status yang bermakna permohonan MASIH memegang sumber.
    /// Rejected dan Cancelled TIDAK disenaraikan — permohonan yang ditolak
    /// mesti boleh dibetulkan dan dihantar semula.
    /// </summary>
    private static readonly SubmissionStatus[] StatusAktif =
    [
        SubmissionStatus.Submitted,
        SubmissionStatus.SupervisorApproved,
        SubmissionStatus.AdminApproved,
        SubmissionStatus.Completed
    ];

    public async Task<EligibilityResult> CanRequestSoftwareAsync(
        string userId, int softwareId, int? kecualiSubmissionId = null,
        CancellationToken ct = default)
    {
        // 1. Sudah memiliki / memohon lesen ini?
        var q = from r in db.Set<SoftwareRequest>().AsNoTracking()
                join s in db.Submissions.AsNoTracking() on r.SubmissionId equals s.Id
                where r.SoftwareCatalogItemId == softwareId
                   && s.ApplicantUserId == userId
                   && StatusAktif.Contains(s.Status)
                select new { s.Id, s.ReferenceNo, s.Status };

        if (kecualiSubmissionId is not null)
            q = q.Where(x => x.Id != kecualiSubmissionId);

        var sediaAda = await q.FirstOrDefaultAsync(ct);
        if (sediaAda is not null)
        {
            return new EligibilityResult(false,
                $"Anda sudah mempunyai permohonan aktif untuk perisian ini " +
                $"({sediaAda.ReferenceNo}). Lesen berharga — sila semak permohonan " +
                "tersebut sebelum memohon semula.");
        }

        // 2. Ada lesen baki?
        var lesen = await inventory.LicenceStatusAsync(softwareId, ct);
        if (!lesen.Tersedia)
        {
            return new EligibilityResult(false,
                $"Tiada lesen {lesen.Nama} yang tinggal " +
                $"({lesen.Diguna}/{lesen.Jumlah} telah diguna). " +
                "Sila hubungi Unit Aset ICT untuk pembelian tambahan.");
        }

        return new EligibilityResult(true, null);
    }

    public async Task<EligibilityResult> CanBorrowAssetAsync(
        string userId, KategoriAset kategori, int? kecualiSubmissionId = null,
        CancellationToken ct = default)
    {
        // 1. Sudah ada pinjaman aktif dalam kategori ini?
        var q = from r in db.Set<AssetLoanRequest>().AsNoTracking()
                join s in db.Submissions.AsNoTracking() on r.SubmissionId equals s.Id
                where r.KategoriDipohon == kategori
                   && s.ApplicantUserId == userId
                   && StatusAktif.Contains(s.Status)
                select new { s.Id, s.ReferenceNo };

        if (kecualiSubmissionId is not null)
            q = q.Where(x => x.Id != kecualiSubmissionId);

        var sediaAda = await q.FirstOrDefaultAsync(ct);
        if (sediaAda is not null)
        {
            return new EligibilityResult(false,
                $"Anda sudah mempunyai pinjaman aktif untuk kategori {kategori} " +
                $"({sediaAda.ReferenceNo}). Sila pulangkan aset tersebut dahulu.");
        }

        // 2. Ada unit tersedia?
        var tersedia = await inventory.AvailableAssetsAsync(kategori, ct);
        if (tersedia.Count == 0)
        {
            return new EligibilityResult(false,
                $"Tiada unit {kategori} yang tersedia buat masa ini. " +
                "Sila cuba semula kemudian atau hubungi Unit Aset ICT.");
        }

        return new EligibilityResult(true, null);
    }
}
```

Daftar dalam `AsetModule`:

```csharp
services.AddScoped<IEligibilityService, EligibilityService>();
```

> **Mesej memberitahu pengguna apa yang perlu dilakukan seterusnya** — bukan hanya "tidak dibenarkan". Bandingkan dengan mesej pendua Kumpulan 2; corak yang sama.

### ✅ Semakan

- [ ] `StatusAktif` **tidak** termasuk `Rejected`/`Cancelled`
- [ ] `kecualiSubmissionId` disokong
- [ ] Mesej menamakan rujukan konflik dan langkah seterusnya
- [ ] Didaftar dalam `AsetModule`

---

## Latihan 2 — Borang lesen perisian

**Objektif:** Borang dengan validation bersyarat dan petunjuk stok.

### Langkah

1. `ViewModels/Aset/SoftwareRequestFormViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services.Aset;

namespace Nres.Onboarding.Web.ViewModels.Aset;

public class SoftwareRequestFormViewModel : IValidatableObject
{
    public int? Id { get; set; }

    [Display(Name = "Perisian")]
    [Required(ErrorMessage = "Sila pilih perisian.")]
    public int? SoftwareCatalogItemId { get; set; }

    [Display(Name = "Justifikasi")]
    [Required(ErrorMessage = "Justifikasi wajib diisi.")]
    [StringLength(1000)]
    public string Justifikasi { get; set; } = string.Empty;

    [Display(Name = "Diperlukan sehingga")]
    [DataType(DataType.Date)]
    public DateTime? TarikhTamat { get; set; }

    // --- Data sokongan ---
    public IReadOnlyList<LesenStatus> Katalog { get; set; } = [];
    public bool PerluJustifikasiTambahan { get; set; }
    public string? NamaPerisian { get; set; }

    public string? ReferenceNo { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public bool IsEditable { get; set; } = true;
    public string? KunciLesen { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        // Perisian kos tinggi memerlukan justifikasi terperinci.
        if (PerluJustifikasiTambahan && Justifikasi.Trim().Length < 30)
        {
            yield return new ValidationResult(
                "Perisian ini memerlukan justifikasi terperinci " +
                "(sekurang-kurangnya 30 aksara) kerana kos lesen yang tinggi.",
                [nameof(Justifikasi)]);
        }

        if (TarikhTamat is not null && TarikhTamat < DateTime.Today)
        {
            yield return new ValidationResult(
                "Tarikh tamat tidak boleh pada masa lalu.",
                [nameof(TarikhTamat)]);
        }
    }
}
```

2. Controller `Controllers/SoftwareController.cs` — bahagian `Submit`:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Submit(SoftwareRequestFormViewModel vm)
{
    if (vm.Id is null) return NotFound();

    var app = await Db.Set<SoftwareRequest>()
        .Include(r => r.Submission)
        .Include(r => r.SoftwareCatalogItem)
        .FirstOrDefaultAsync(r => r.Id == vm.Id);

    if (app is null) return NotFound();
    if (app.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();
    if (app.Submission.Status != SubmissionStatus.Draft)
    {
        TempData["Ralat"] = "Permohonan ini telah pun dihantar.";
        return RedirectToAction(nameof(Edit), new { id = app.Id });
    }

    vm.PerluJustifikasiTambahan =
        app.SoftwareCatalogItem?.PerluJustifikasi ?? false;

    if (!ModelState.IsValid)
    {
        await IsiSokonganAsync(vm);
        return View("Form", vm);
    }

    // --- SEMAKAN STOK MASA-NYATA ---
    // Katalog yang pemohon lihat mungkin 20 minit lama. Semak SEMULA di sini.
    var layak = await eligibility.CanRequestSoftwareAsync(
        currentUser.UserId!, app.SoftwareCatalogItemId,
        kecualiSubmissionId: app.SubmissionId);

    if (!layak.Layak)
    {
        ModelState.AddModelError(string.Empty, layak.Sebab!);
        await IsiSokonganAsync(vm);
        return View("Form", vm);
    }

    app.Justifikasi = vm.Justifikasi;
    app.TarikhTamat = vm.TarikhTamat;
    await Db.SaveChangesAsync();

    app.Submission.ReferenceNo =
        await referenceNumbers.GenerateAsync(ModuleCodes.Perisian);

    await Workflow.TransitionAsync(app.Submission, SubmissionStatus.Submitted,
        "Submitted", $"Lesen {app.SoftwareCatalogItem?.Nama}");

    TempData["Mesej"] = $"Permohonan dihantar. No. rujukan: {app.Submission.ReferenceNo}";
    return RedirectToAction(nameof(Edit), new { id = app.Id });
}
```

### ✅ Semakan

- [ ] Perisian kos tinggi memerlukan justifikasi ≥30 aksara
- [ ] Semakan stok berjalan pada **penghantaran**, bukan hanya paparan
- [ ] Mesej ralat menamakan langkah seterusnya
- [ ] Menghantar semula permohonan sama tidak menyekat dirinya

---

## Latihan 3 — Borang pinjaman aset

**Objektif:** Pemohon memilih **kategori**, bukan unit.

### Langkah

1. View model:

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models.Aset;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services.Aset;

namespace Nres.Onboarding.Web.ViewModels.Aset;

public class AssetLoanFormViewModel : IValidatableObject
{
    public int? Id { get; set; }

    [Display(Name = "Kategori aset")]
    [Required(ErrorMessage = "Sila pilih kategori aset.")]
    public KategoriAset? KategoriDipohon { get; set; }

    [Display(Name = "Justifikasi")]
    [Required(ErrorMessage = "Justifikasi wajib diisi.")]
    [StringLength(1000)]
    public string Justifikasi { get; set; } = string.Empty;

    [Display(Name = "Tarikh pinjam")]
    [Required(ErrorMessage = "Tarikh pinjam wajib diisi.")]
    [DataType(DataType.Date)]
    public DateTime? TarikhPinjam { get; set; }

    [Display(Name = "Tarikh jangka pulang")]
    [Required(ErrorMessage = "Tarikh jangka pulang wajib diisi.")]
    [DataType(DataType.Date)]
    public DateTime? TarikhJangkaPulang { get; set; }

    // --- Data sokongan ---
    public IReadOnlyList<AsetTersedia> UnitTersedia { get; set; } = [];

    /// <summary>Aset yang DIPERUNTUKKAN ICT — null sehingga diluluskan.</summary>
    public string? AssetTagDiperuntukkan { get; set; }

    public string? ReferenceNo { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public bool IsEditable { get; set; } = true;
    public bool BolehAkuTerima { get; set; }
    public bool AkuanTerima { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (TarikhPinjam is null || TarikhJangkaPulang is null) yield break;

        if (TarikhJangkaPulang <= TarikhPinjam)
        {
            yield return new ValidationResult(
                "Tarikh jangka pulang mesti selepas tarikh pinjam.",
                [nameof(TarikhJangkaPulang)]);
        }

        // Pinjaman tanpa had ialah pemberian. Had 6 bulan.
        var bulan = (TarikhJangkaPulang.Value - TarikhPinjam.Value).TotalDays / 30;
        if (bulan > 6)
        {
            yield return new ValidationResult(
                "Tempoh pinjaman tidak boleh melebihi 6 bulan. " +
                "Untuk keperluan jangka panjang, sila mohon peruntukan aset tetap.",
                [nameof(TarikhJangkaPulang)]);
        }

        if (TarikhPinjam < DateTime.Today.AddDays(-1))
        {
            yield return new ValidationResult(
                "Tarikh pinjam tidak boleh pada masa lalu.",
                [nameof(TarikhPinjam)]);
        }
    }
}
```

2. Borang menunjukkan **kiraan tersedia**, bukan senarai unit:

```cshtml
<div class="col-md-6">
    <label asp-for="KategoriDipohon" class="form-label"></label>
    <select asp-for="KategoriDipohon" class="form-select" id="kategori">
        <option value="">— Pilih kategori —</option>
        @foreach (var k in Model.KategoriDenganStok)
        {
            <option value="@((int)k.Kategori)" disabled="@(k.Tersedia == 0)">
                @k.Kategori — @k.Tersedia unit tersedia
            </option>
        }
    </select>
    <div class="form-text">
        Unit sebenar akan diperuntukkan oleh Unit Aset ICT semasa kelulusan.
    </div>
    <span asp-validation-for="KategoriDipohon" class="text-danger"></span>
</div>
```

> **Nota bantuan "unit sebenar akan diperuntukkan"** menetapkan jangkaan. Tanpanya, pemohon bertanya kenapa mereka tidak boleh memilih laptop tertentu.

3. `Submit` menjalankan semakan yang sama:

```csharp
var layak = await eligibility.CanBorrowAssetAsync(
    currentUser.UserId!, vm.KategoriDipohon!.Value,
    kecualiSubmissionId: app.SubmissionId);

if (!layak.Layak)
{
    ModelState.AddModelError(string.Empty, layak.Sebab!);
    await IsiSokonganAsync(vm);
    return View("Form", vm);
}
```

### ✅ Semakan

- [ ] Pemohon memilih kategori, bukan unit
- [ ] Kategori tanpa stok dilumpuhkan dalam dropdown
- [ ] Nota bantuan menerangkan peruntukan
- [ ] Tempoh maksimum 6 bulan dikuatkuasakan
- [ ] Semakan kelayakan berjalan pada penghantaran

---

## Latihan 4 — Akuan penerimaan

**Objektif:** Tutup gelung — inventori mencerminkan realiti.

### Langkah

```csharp
/// <summary>
/// Pemohon mengakui menerima aset secara fizikal.
///
/// Tanpa langkah ini, inventori berbohong — ia menunjukkan OnLoan untuk
/// aset yang masih dalam stor menunggu penyerahan.
/// </summary>
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> AkuTerima(int id)
{
    var app = await Db.Set<AssetLoanRequest>()
        .Include(r => r.Submission)
        .Include(r => r.Asset)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (app is null) return NotFound();

    // Hanya PEMOHON boleh mengakui — bukan ICT bagi pihak mereka.
    if (app.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();

    if (app.Submission.Status != SubmissionStatus.AdminApproved)
    {
        TempData["Ralat"] = "Akuan hanya boleh dibuat selepas permohonan diluluskan.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    if (app.AssetId is null)
    {
        TempData["Ralat"] = "Tiada aset diperuntukkan lagi.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    if (app.AkuanTerima)
    {
        TempData["Ralat"] = "Anda telah pun mengakui penerimaan.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    app.AkuanTerima = true;
    app.TarikhAkuanTerima = DateTime.UtcNow;
    await Db.SaveChangesAsync();

    // Audit — ini bukti jika aset kemudian hilang.
    await auditLog.LogAsync(app.SubmissionId, "AssetReceived",
        remarks: $"Pemohon mengakui menerima {app.Asset?.AssetTag} " +
                 $"pada {DateTime.UtcNow:dd/MM/yyyy HH:mm}.");

    TempData["Mesej"] = $"Terima kasih. Penerimaan {app.Asset?.AssetTag} direkodkan.";
    return RedirectToAction(nameof(Edit), new { id });
}
```

Borang akuan dalam view:

```cshtml
@if (Model.BolehAkuTerima && !Model.AkuanTerima)
{
    <div class="alert alert-info">
        <h6>Akuan Penerimaan</h6>
        <p class="mb-2">
            Aset <strong>@Model.AssetTagDiperuntukkan</strong> telah diluluskan
            untuk anda. Sila akui setelah anda menerimanya secara fizikal
            daripada Unit Aset ICT.
        </p>
        <form asp-action="AkuTerima" method="post">
            @Html.AntiForgeryToken()
            <input type="hidden" name="id" value="@Model.Id" />
            <button type="submit" class="btn btn-success"
                    onclick="return confirm('Sahkan anda telah menerima aset ini?');">
                Saya mengaku telah menerima aset ini
            </button>
        </form>
    </div>
}
else if (Model.AkuanTerima)
{
    <div class="alert alert-success">
        ✓ Penerimaan @Model.AssetTagDiperuntukkan telah diakui.
    </div>
}
```

### ✅ Semakan

- [ ] Hanya **pemohon** boleh mengakui
- [ ] Akuan hanya selepas kelulusan dan peruntukan
- [ ] Akuan dua kali disekat
- [ ] Akuan diaudit

---

## Latihan 5 — Ujian

Rekod dalam `docs/kumpulan-4/ujian-manual.md`:

| # | Ujian | Jangkaan | Keputusan |
|---|-------|----------|-----------|
| 1 | Mohon AutoCAD (5 lesen, 0 diguna) | Berjaya | |
| 2 | Mohon AutoCAD **lagi** (pemohon sama) | Disekat — sudah ada | |
| 3 | Mohon AutoCAD (pemohon berbeza) | Berjaya | |
| 4 | Tolak permohonan #1, mohon semula | **Berjaya** | |
| 5 | Habiskan semua 5 lesen, mohon ke-6 | Disekat — tiada baki | |
| 6 | Mohon 7-Zip (tanpa had) 10 kali | Berjaya setiap kali | |
| 7 | Justifikasi 10 aksara untuk AutoCAD | Ditolak — perlu ≥30 | |
| 8 | Justifikasi 10 aksara untuk 7-Zip | Berjaya — tiada keperluan | |
| 9 | Pinjam laptop | Berjaya | |
| 10 | Pinjam laptop **lagi** | Disekat — satu setiap kategori | |
| 11 | Pinjam projektor (kategori berbeza) | **Berjaya** | |
| 12 | Tempoh pinjaman 8 bulan | Ditolak — maks 6 bulan | |
| 13 | Kategori tanpa stok | Dilumpuhkan dalam dropdown | |
| 14 | Aku terima sebelum kelulusan | Disekat | |
| 15 | Aku terima permohonan orang lain | 403 | |

> **Ujian 3, 4, 6, 8, 11 ialah kes yang mesti DIBENARKAN.** Sama seperti Kumpulan 2 — semakan yang terlalu ketat menghalang kerja sebenar.

### ✅ Semakan

- [ ] Kesemua 15 ujian dijalankan
- [ ] Ujian 3, 4, 6, 8, 11 **berjaya**
- [ ] Ujian 2, 5, 7, 10, 12, 14, 15 disekat dengan mesej berguna

---

## Latihan 6 — Tutup blok

```bash
git diff --name-only master
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Kedua-dua borang berfungsi dengan validation bersyarat
- [ ] **Semakan stok berjalan pada penghantaran**, bukan hanya paparan
- [ ] Kes sah tidak disekat
- [ ] Pemohon memilih kategori; unit diperuntukkan kemudian
- [ ] Akuan penerimaan berfungsi dan diaudit
- [ ] Hanya fail Kumpulan 4 disentuh
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 5–6

| Artifak | Lokasi |
|---------|--------|
| `IEligibilityService` | `Services/Aset/` |
| View model lesen & pinjaman | `ViewModels/Aset/` |
| `SoftwareController`, `AssetController` | `Controllers/` |
| Borang + akuan penerimaan | `Views/Aset/` |
| Ujian manual | `docs/kumpulan-4/ujian-manual.md` |

**Seterusnya (Hari 7–9):** kelulusan ICT dengan **peruntukan aset**, pemulangan dengan pemeriksaan kondisi, dan **transaksi inventori**.
