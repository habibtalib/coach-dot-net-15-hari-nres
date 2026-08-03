# Lab · Kumpulan 1 · Hari 7–9 — Dashboard HR & Kelulusan

> Konsep: [`../README.md`](../README.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok

```bash
git switch kump-1/lapor-diri
git pull --rebase origin master
git switch -c kump-1/feat/dashboard-hr
dotnet build
```

**Semakan "sudah wujud?"** — kritikal untuk blok ini:

```bash
grep -n "Approve\|Reject" Nres.Onboarding.Web/Controllers/SubmissionControllerBase.cs
grep -rn "_ApprovalPanel\|_FilterBar" Nres.Onboarding.Web/Views/Shared/
```

Kedua-duanya **sudah wujud**. Anda membina skrin yang menggunakannya.

### ✅ Semakan

- [ ] Anda mengesahkan `Approve`/`Reject` wujud dalam kelas asas
- [ ] Anda mengesahkan `_ApprovalPanel` dan `_FilterBar` wujud
- [ ] Anda **tidak** akan menulis semula mana-mana daripadanya

---

## Latihan 1 — View model dashboard & senarai

**Objektif:** Bentuk data untuk skrin HR.

### Langkah

1. `ViewModels/LaporDiri/HrDashboardViewModel.cs`:

```csharp
namespace Nres.Onboarding.Web.ViewModels.LaporDiri;

public class HrDashboardViewModel
{
    public int MenungguSemakan { get; set; }
    public int DiluluskanBulanIni { get; set; }
    public int DitolakBulanIni { get; set; }
    public int JumlahDraf { get; set; }

    public IReadOnlyList<HrQueueItem> Terkini { get; set; } = [];
}

/// <summary>
/// Baris ringkas untuk skrin senarai. Kami memproject KEPADA ini dalam query
/// supaya pangkalan data hanya menghantar lajur yang kami paparkan.
/// </summary>
public class HrQueueItem
{
    public int ApplicationId { get; set; }
    public int SubmissionId { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public DateTime? ReportingDate { get; set; }
    public DateTime? SubmittedAt { get; set; }
    public Nres.Onboarding.Web.Models.Shared.SubmissionStatus Status { get; set; }
}
```

2. `ViewModels/LaporDiri/HrReviewListViewModel.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.Rendering;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.LaporDiri;

public class HrReviewListViewModel
{
    // --- Penapis (terikat dari querystring) ---
    public SubmissionStatus? Status { get; set; }
    public int? DepartmentId { get; set; }
    public DateTime? DariTarikh { get; set; }
    public DateTime? HinggaTarikh { get; set; }
    public string? Carian { get; set; }

    // --- Paging ---
    public int Halaman { get; set; } = 1;
    public int SaizHalaman { get; set; } = 20;
    public int JumlahRekod { get; set; }
    public int JumlahHalaman => (int)Math.Ceiling(JumlahRekod / (double)SaizHalaman);

    // --- Hasil ---
    public IReadOnlyList<HrQueueItem> Items { get; set; } = [];
    public IEnumerable<SelectListItem> Departments { get; set; } = [];
}
```

### ✅ Semakan

- [ ] Kedua-dua view model dalam `ViewModels/LaporDiri/`
- [ ] `HrQueueItem` mengandungi hanya medan yang dipaparkan
- [ ] `dotnet build` berjaya

---

## Latihan 2 — Servis query HR

**Objektif:** Query yang cekap, dipisahkan dari controller supaya boleh diuji pada Hari 13–14.

### Langkah

1. `Services/LaporDiri/IHrReviewService.cs`:

```csharp
using Nres.Onboarding.Web.ViewModels.LaporDiri;

namespace Nres.Onboarding.Web.Services.LaporDiri;

public interface IHrReviewService
{
    Task<HrDashboardViewModel> DashboardAsync(CancellationToken ct = default);
    Task<HrReviewListViewModel> SearchAsync(HrReviewListViewModel penapis,
                                            CancellationToken ct = default);
}
```

2. `Services/LaporDiri/HrReviewService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.ViewModels.LaporDiri;

namespace Nres.Onboarding.Web.Services.LaporDiri;

public class HrReviewService(ApplicationDbContext db) : IHrReviewService
{
    /// <summary>
    /// Query asas: gabung Submission induk dengan jadual detail kami.
    /// AsNoTracking kerana skrin ini baca-sahaja — EF Core tidak perlu
    /// menjejaki perubahan.
    /// </summary>
    private IQueryable<HrQueueItem> BaseQuery() =>
        from s in db.Submissions.AsNoTracking()
        join a in db.Set<OfficerReportingApplication>().AsNoTracking()
            on s.Id equals a.SubmissionId
        where s.ModuleCode == ModuleCodes.LaporDiri
        select new HrQueueItem
        {
            ApplicationId = a.Id,
            SubmissionId = s.Id,
            ReferenceNo = s.ReferenceNo,
            FullName = a.FullName,
            DepartmentName = a.Department != null ? a.Department.Name : null,
            ReportingDate = a.ReportingDate,
            SubmittedAt = s.SubmittedAt,
            Status = s.Status
        };

    public async Task<HrDashboardViewModel> DashboardAsync(CancellationToken ct = default)
    {
        var sebulanLalu = DateTime.UtcNow.AddDays(-30);

        // Kiraan dijalankan di PANGKALAN DATA — tiada baris ditarik ke memori.
        var submissions = db.Submissions.AsNoTracking()
            .Where(s => s.ModuleCode == ModuleCodes.LaporDiri);

        return new HrDashboardViewModel
        {
            MenungguSemakan = await submissions
                .CountAsync(s => s.Status == SubmissionStatus.Submitted, ct),

            DiluluskanBulanIni = await submissions
                .CountAsync(s => s.Status == SubmissionStatus.AdminApproved
                              && s.CompletedAt >= sebulanLalu, ct),

            DitolakBulanIni = await submissions
                .CountAsync(s => s.Status == SubmissionStatus.Rejected
                              && s.SubmittedAt >= sebulanLalu, ct),

            JumlahDraf = await submissions
                .CountAsync(s => s.Status == SubmissionStatus.Draft, ct),

            Terkini = await BaseQuery()
                .Where(x => x.Status == SubmissionStatus.Submitted)
                .OrderBy(x => x.SubmittedAt)      // yang paling lama menunggu dahulu
                .Take(10)
                .ToListAsync(ct)
        };
    }

    public async Task<HrReviewListViewModel> SearchAsync(
        HrReviewListViewModel penapis, CancellationToken ct = default)
    {
        var q = BaseQuery();

        // Setiap penapis ditambah ke IQueryable — tiada apa dijalankan lagi.
        if (penapis.Status is not null)
            q = q.Where(x => x.Status == penapis.Status);

        if (penapis.DepartmentId is not null)
            q = q.Where(x => x.DepartmentName != null &&
                db.LookupDepartments
                  .Where(d => d.Id == penapis.DepartmentId)
                  .Select(d => d.Name)
                  .Contains(x.DepartmentName));

        if (penapis.DariTarikh is not null)
            q = q.Where(x => x.SubmittedAt >= penapis.DariTarikh);

        if (penapis.HinggaTarikh is not null)
            q = q.Where(x => x.SubmittedAt <= penapis.HinggaTarikh!.Value.AddDays(1));

        if (!string.IsNullOrWhiteSpace(penapis.Carian))
        {
            var cari = penapis.Carian.Trim();
            q = q.Where(x => x.ReferenceNo.Contains(cari) || x.FullName.Contains(cari));
        }

        // Kira DAHULU (satu query), kemudian ambil satu halaman (query kedua).
        penapis.JumlahRekod = await q.CountAsync(ct);

        penapis.Items = await q
            .OrderByDescending(x => x.SubmittedAt)
            .Skip((penapis.Halaman - 1) * penapis.SaizHalaman)
            .Take(penapis.SaizHalaman)
            .ToListAsync(ct);

        return penapis;
    }
}
```

3. Daftar dalam modul anda:

```csharp
services.AddScoped<IHrReviewService, HrReviewService>();
```

### ✅ Semakan

- [ ] Servis dalam `Services/LaporDiri/`
- [ ] `AsNoTracking()` digunakan pada query baca-sahaja
- [ ] Penapisan berlaku dalam `IQueryable`, sebelum `ToListAsync()`
- [ ] Kiraan menggunakan `CountAsync`, bukan `.ToList().Count`
- [ ] Didaftar dalam `LaporDiriModule`

---

## Latihan 3 — Action controller HR

**Objektif:** Skrin dashboard, senarai, dan butiran — dengan authorization sebenar.

### Langkah

1. Suntik servis dan tambah action:

```csharp
// tambah ke primary constructor: IHrReviewService hrReview

[Authorize(Roles = "HrAdmin")]
public async Task<IActionResult> Dashboard()
{
    return View(await hrReview.DashboardAsync());
}

[Authorize(Roles = "HrAdmin")]
public async Task<IActionResult> Review(HrReviewListViewModel penapis)
{
    penapis.Halaman = Math.Max(1, penapis.Halaman);
    var hasil = await hrReview.SearchAsync(penapis);

    hasil.Departments = await Db.LookupDepartments.AsNoTracking()
        .Where(l => l.IsActive).OrderBy(l => l.Name)
        .Select(l => new SelectListItem(l.Name, l.Id.ToString()))
        .ToListAsync();

    return View(hasil);
}

/// <summary>Skrin butiran HR — permohonan penuh, lampiran, audit, keputusan.</summary>
[Authorize(Roles = "HrAdmin")]
public async Task<IActionResult> Details(int id)
{
    var app = await Db.Set<OfficerReportingApplication>()
        .AsNoTracking()
        .Include(a => a.Submission)
        .Include(a => a.Department)
        .Include(a => a.Position)
        .Include(a => a.Grade)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (app is null) return NotFound();

    var vm = new HrDetailsViewModel
    {
        Application = app,
        Submission = app.Submission!,
        Attachments = await attachments.ListAsync(app.SubmissionId),
        AuditLogs = await Db.AuditLogs.AsNoTracking()
            .Where(l => l.SubmissionId == app.SubmissionId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync(),
        BolehDiputuskan = app.Submission!.Status == SubmissionStatus.Submitted
    };

    return View(vm);
}
```

2. `ViewModels/LaporDiri/HrDetailsViewModel.cs`:

```csharp
using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.LaporDiri;

public class HrDetailsViewModel
{
    public OfficerReportingApplication Application { get; set; } = null!;
    public Submission Submission { get; set; } = null!;
    public IReadOnlyList<(int AttachmentId, JenisDokumen Jenis, string FileName, long Size)>
        Attachments { get; set; } = [];
    public IReadOnlyList<AuditLog> AuditLogs { get; set; } = [];

    /// <summary>Panel kelulusan hanya dipaparkan jika status masih Submitted.</summary>
    public bool BolehDiputuskan { get; set; }
}
```

3. **Perhatikan apa yang tiada:** tiada action `Approve` atau `Reject`. Ia diwarisi dari `SubmissionControllerBase`.

### ✅ Semakan

- [ ] Ketiga-tiga action mempunyai `[Authorize(Roles = "HrAdmin")]`
- [ ] **Tiada** `Approve`/`Reject` ditulis dalam controller anda
- [ ] `dotnet build` berjaya

---

## Latihan 4 — View dashboard & senarai

**Objektif:** Skrin yang HR sebenarnya boleh gunakan.

### Langkah

1. `Views/OfficerReporting/Dashboard.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.LaporDiri.HrDashboardViewModel
@{ ViewData["Title"] = "Papan Pemuka HR — Lapor Diri"; }

<h2>@ViewData["Title"]</h2>

<div class="row g-3 mt-2">
    <div class="col-md-3">
        <div class="card text-bg-primary">
            <div class="card-body">
                <div class="display-6">@Model.MenungguSemakan</div>
                <div>Menunggu semakan</div>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card text-bg-success">
            <div class="card-body">
                <div class="display-6">@Model.DiluluskanBulanIni</div>
                <div>Diluluskan (30 hari)</div>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card text-bg-danger">
            <div class="card-body">
                <div class="display-6">@Model.DitolakBulanIni</div>
                <div>Ditolak (30 hari)</div>
            </div>
        </div>
    </div>
    <div class="col-md-3">
        <div class="card text-bg-secondary">
            <div class="card-body">
                <div class="display-6">@Model.JumlahDraf</div>
                <div>Draf belum dihantar</div>
            </div>
        </div>
    </div>
</div>

<h5 class="mt-4">Menunggu semakan paling lama</h5>
<table class="table table-hover">
    <thead>
        <tr><th>No. Rujukan</th><th>Nama</th><th>Bahagian</th><th>Dihantar</th><th></th></tr>
    </thead>
    <tbody>
    @if (!Model.Terkini.Any())
    {
        <tr><td colspan="5" class="text-muted">Tiada permohonan menunggu. 🎉</td></tr>
    }
    @foreach (var x in Model.Terkini)
    {
        <tr>
            <td>@x.ReferenceNo</td>
            <td>@x.FullName</td>
            <td>@x.DepartmentName</td>
            <td>@x.SubmittedAt?.ToLocalTime().ToString("dd/MM/yyyy")</td>
            <td class="text-end">
                <a asp-action="Details" asp-route-id="@x.ApplicationId"
                   class="btn btn-sm btn-primary">Semak</a>
            </td>
        </tr>
    }
    </tbody>
</table>

<a asp-action="Review" class="btn btn-outline-secondary">Lihat semua permohonan</a>
```

2. `Views/OfficerReporting/Review.cshtml` — senarai dengan penapis dan paging:

```cshtml
@model Nres.Onboarding.Web.ViewModels.LaporDiri.HrReviewListViewModel
@using Nres.Onboarding.Web.Models.Shared
@{ ViewData["Title"] = "Semakan Permohonan Lapor Diri"; }

<h2>@ViewData["Title"]</h2>

<form method="get" class="row g-2 align-items-end my-3">
    <div class="col-md-2">
        <label class="form-label">Status</label>
        <select asp-for="Status" class="form-select"
                asp-items="Html.GetEnumSelectList<SubmissionStatus>()">
            <option value="">— Semua —</option>
        </select>
    </div>
    <div class="col-md-2">
        <label class="form-label">Bahagian</label>
        <select asp-for="DepartmentId" asp-items="Model.Departments" class="form-select">
            <option value="">— Semua —</option>
        </select>
    </div>
    <div class="col-md-2">
        <label class="form-label">Dari</label>
        <input asp-for="DariTarikh" type="date" class="form-control" />
    </div>
    <div class="col-md-2">
        <label class="form-label">Hingga</label>
        <input asp-for="HinggaTarikh" type="date" class="form-control" />
    </div>
    <div class="col-md-3">
        <label class="form-label">Carian (rujukan / nama)</label>
        <input asp-for="Carian" class="form-control" />
    </div>
    <div class="col-md-1">
        <button type="submit" class="btn btn-primary w-100">Tapis</button>
    </div>
</form>

<p class="text-muted">@Model.JumlahRekod rekod ditemui.</p>

<table class="table table-hover">
    <thead>
        <tr>
            <th>No. Rujukan</th><th>Nama</th><th>Bahagian</th>
            <th>Tarikh lapor</th><th>Status</th><th></th>
        </tr>
    </thead>
    <tbody>
    @foreach (var x in Model.Items)
    {
        <tr>
            <td>@(string.IsNullOrEmpty(x.ReferenceNo) ? "(draf)" : x.ReferenceNo)</td>
            <td>@x.FullName</td>
            <td>@x.DepartmentName</td>
            <td>@x.ReportingDate?.ToString("dd/MM/yyyy")</td>
            <td><partial name="_StatusBadge" model="x.Status" /></td>
            <td class="text-end">
                <a asp-action="Details" asp-route-id="@x.ApplicationId"
                   class="btn btn-sm btn-outline-primary">Buka</a>
            </td>
        </tr>
    }
    </tbody>
</table>

@if (Model.JumlahHalaman > 1)
{
    <nav>
        <ul class="pagination">
        @for (var i = 1; i <= Model.JumlahHalaman; i++)
        {
            <li class="page-item @(i == Model.Halaman ? "active" : "")">
                <a class="page-link"
                   asp-action="Review"
                   asp-route-Halaman="@i"
                   asp-route-Status="@Model.Status"
                   asp-route-DepartmentId="@Model.DepartmentId"
                   asp-route-DariTarikh="@Model.DariTarikh?.ToString("yyyy-MM-dd")"
                   asp-route-HinggaTarikh="@Model.HinggaTarikh?.ToString("yyyy-MM-dd")"
                   asp-route-Carian="@Model.Carian">@i</a>
            </li>
        }
        </ul>
    </nav>
}
```

### ✅ Semakan

- [ ] Dashboard memaparkan empat kiraan
- [ ] Baris gilir menunjukkan yang paling lama menunggu dahulu
- [ ] Penapis berfungsi dan kekal apabila menukar halaman
- [ ] Lencana status menggunakan `_StatusBadge` kongsi

---

## Latihan 5 — Skrin butiran & panel kelulusan

**Objektif:** HR melihat segalanya dan membuat keputusan — menggunakan panel kongsi.

### Langkah

1. `Views/OfficerReporting/Details.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.LaporDiri.HrDetailsViewModel
@using Nres.Onboarding.Web.Models.LaporDiri
@{ ViewData["Title"] = $"Semakan — {Model.Submission.ReferenceNo}"; }

<div class="d-flex justify-content-between align-items-start">
    <div>
        <h2>@Model.Submission.ReferenceNo</h2>
        <partial name="_StatusBadge" model="Model.Submission.Status" />
    </div>
    <a asp-action="Review" class="btn btn-outline-secondary">Kembali ke senarai</a>
</div>

<div class="row mt-4">
    <div class="col-lg-8">
        <div class="card mb-3">
            <div class="card-header">Maklumat Pemohon</div>
            <div class="card-body">
                <dl class="row mb-0">
                    <dt class="col-sm-4">Nama penuh</dt>
                    <dd class="col-sm-8">@Model.Application.FullName</dd>
                    <dt class="col-sm-4">No. kad pengenalan</dt>
                    <dd class="col-sm-8">@Model.Application.IdentityNo</dd>
                    <dt class="col-sm-4">E-mel</dt>
                    <dd class="col-sm-8">@Model.Application.Email</dd>
                    <dt class="col-sm-4">Telefon</dt>
                    <dd class="col-sm-8">@Model.Application.Phone</dd>
                    <dt class="col-sm-4">Tarikh lapor diri</dt>
                    <dd class="col-sm-8">@Model.Application.ReportingDate?.ToString("dd/MM/yyyy")</dd>
                    <dt class="col-sm-4">Bahagian</dt>
                    <dd class="col-sm-8">@Model.Application.Department?.Name</dd>
                    <dt class="col-sm-4">Jawatan</dt>
                    <dd class="col-sm-8">@Model.Application.Position?.Name</dd>
                    <dt class="col-sm-4">Gred</dt>
                    <dd class="col-sm-8">@Model.Application.Grade?.Name</dd>
                    <dt class="col-sm-4">Agensi sebelum ini</dt>
                    <dd class="col-sm-8">@(Model.Application.PreviousAgency ?? "—")</dd>
                    <dt class="col-sm-4">Waris kecemasan</dt>
                    <dd class="col-sm-8">
                        @Model.Application.EmergencyContactName
                        @if (!string.IsNullOrWhiteSpace(Model.Application.EmergencyContactPhone))
                        {
                            <text>(@Model.Application.EmergencyContactPhone)</text>
                        }
                    </dd>
                </dl>
            </div>
        </div>

        <div class="card mb-3">
            <div class="card-header">Dokumen Sokongan</div>
            <ul class="list-group list-group-flush">
            @foreach (var a in Model.Attachments)
            {
                <li class="list-group-item d-flex justify-content-between">
                    <span>@DokumenSokongan.Nama(a.Jenis)</span>
                    <a asp-action="DownloadAttachment" asp-route-attachmentId="@a.AttachmentId">
                        @a.FileName (@(a.Size / 1024) KB)
                    </a>
                </li>
            }
            </ul>
        </div>
    </div>

    <div class="col-lg-4">
        @if (Model.BolehDiputuskan)
        {
            @* Panel KONGSI — kami tidak menulis borang kelulusan sendiri. *@
            <partial name="_ApprovalPanel" model="Model.Submission" />
        }
        else
        {
            <div class="alert alert-secondary">
                Permohonan ini telah pun diputuskan.
            </div>
        }

        <div class="mt-3">
            <partial name="_AuditTrail" model="Model.AuditLogs" />
        </div>
    </div>
</div>
```

2. Uji aliran kelulusan penuh:
   - Log masuk sebagai `applicant@nres.test`, hantar permohonan
   - Log masuk sebagai `hr@nres.test` → Dashboard → Semak → Luluskan
   - Sahkan status bertukar dan audit merekodnya
   - Ulang dengan Tolak — sahkan sebab **wajib**

### ✅ Semakan

- [ ] Butiran memaparkan medan penuh + lampiran
- [ ] Panel kelulusan menggunakan `_ApprovalPanel` **kongsi**
- [ ] Audit trail menggunakan `_AuditTrail` **kongsi**
- [ ] Meluluskan berfungsi; status → `AdminApproved`
- [ ] Menolak tanpa sebab **ditolak**
- [ ] Menolak dengan sebab berfungsi; sebab muncul dalam audit
- [ ] Panel kelulusan hilang selepas keputusan dibuat

---

## Latihan 6 — Ujian authorization & concurrency

**Objektif:** Buktikan kawalan berfungsi.

### Langkah

1. **Ujian authorization** — jalankan kesemuanya:

| Ujian | Jangkaan |
|-------|----------|
| Log masuk `applicant@nres.test`, lawati `/OfficerReporting/Dashboard` | 403 |
| Log masuk `applicant@nres.test`, lawati `/OfficerReporting/Review` | 403 |
| Log masuk `keselamatan@nres.test` (SecurityAdmin), lawati `/OfficerReporting/Details/1` | 403 |
| Log masuk `hr@nres.test`, kesemuanya | 200 |
| Pemohon A cuba `Details` permohonan pemohon B | 403 |

2. **Ujian concurrency** — buka dua pelayar (satu incognito), kedua-dua log masuk sebagai HR:
   - Kedua-dua buka permohonan `Submitted` yang sama
   - Pelayar 1: Luluskan → berjaya
   - Pelayar 2: Tolak → **mesti gagal** dengan ralat peralihan tidak sah

   Jika pelayar 2 berjaya, `IWorkflowService` tidak dipanggil di suatu tempat — cari dan betulkan.

3. Rekod hasil dalam `docs/kumpulan-1/ujian-manual.md`:

```markdown
# Ujian manual — Kumpulan 1, Hari 7–9

| # | Ujian | Jangkaan | Keputusan |
|---|-------|----------|-----------|
| 1 | Applicant → /Dashboard | 403 | ✅ |
| 2 | SecurityAdmin → /Details/1 | 403 | ✅ |
| 3 | Pemohon A → Details pemohon B | 403 | ✅ |
| 4 | Tolak tanpa sebab | Ditolak | ✅ |
| 5 | Dua HR, satu permohonan | Kedua gagal | ✅ |
```

### ✅ Semakan

- [ ] Kelima-lima ujian authorization lulus
- [ ] Ujian concurrency menunjukkan keputusan kedua gagal
- [ ] Hasil direkod dalam `docs/kumpulan-1/ujian-manual.md`

---

## Latihan 7 — Tutup blok

```bash
git diff --name-only master     # hanya fail LaporDiri
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → kemas kini board.

### ✅ Semakan (Definition of Done)

- [ ] Binaan bersih, aliran HR berfungsi hujung-ke-hujung
- [ ] `Approve`/`Reject` **diwarisi**, bukan ditulis semula
- [ ] `_ApprovalPanel`, `_AuditTrail`, `_StatusBadge`, `_FilterBar` digunakan
- [ ] Hanya fail Kumpulan 1 disentuh
- [ ] `[Authorize(Roles)]` pada setiap action HR, **diuji**
- [ ] Ujian manual didokumenkan
- [ ] Disemak rakan sekumpulan
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 7–9

| Artifak | Lokasi |
|---------|--------|
| View model dashboard/senarai/butiran | `ViewModels/LaporDiri/` |
| `IHrReviewService` + pelaksanaan | `Services/LaporDiri/` |
| Action Dashboard/Review/Details | `OfficerReportingController` |
| View Dashboard/Review/Details | `Views/OfficerReporting/` |
| Rekod ujian manual | `docs/kumpulan-1/ujian-manual.md` |

**Seterusnya (Hari 10–12):** notifikasi e-mel, Slip Akuan PDF, dan papan pemuka analitis HR.
