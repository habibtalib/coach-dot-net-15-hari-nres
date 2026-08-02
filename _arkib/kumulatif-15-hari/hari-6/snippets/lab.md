# Lab Hari 6 — Kelulusan, Penapisan & Cetakan Modul 2

Lab ini mengiringi [`README.md`](../README.md) Hari 6, dan menyambung terus daripada [Lab Hari 5](../../hari-5/snippets/lab.md) (tiga borang siap: pas keselamatan, pelekat kenderaan, parkir — semuanya boleh disimpan sebagai draf atau dihantar dengan nombor rujukan `PAS`/`STK`/`PKR`). Rujuk [`../../projek/`](../../projek/) untuk banding kod anda selepas cuba sendiri.

---

## Latihan 1 — Senarai Admin Bersepadu (3 Jadual → 1 Senarai)

**Objektif:** Bina satu view model gabungan dan satu action `Index` yang memaparkan permohonan daripada ketiga-tiga jadual, dengan filter jenis/status/jabatan/tarikh.

### 1.1 — View model senarai & filter

Cipta `ViewModels/Module2AdminListViewModel.cs`:

```csharp
using Nres.Onboarding.Web.Models;

namespace Nres.Onboarding.Web.ViewModels;

public enum Module2RequestType
{
    SecurityPass = 0,
    VehicleSticker = 1,
    Parking = 2
}

public class Module2ListItemViewModel
{
    public int SubmissionId { get; set; }
    public int DetailId { get; set; }
    public Module2RequestType RequestType { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public string ApplicantUserId { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? SubmittedAt { get; set; }

    public string RequestTypeLabel => RequestType switch
    {
        Module2RequestType.SecurityPass => "Pas Keselamatan",
        Module2RequestType.VehicleSticker => "Pelekat Kenderaan",
        Module2RequestType.Parking => "Parkir",
        _ => "Tidak Diketahui"
    };
}

public class Module2AdminFilterViewModel
{
    public Module2RequestType? RequestType { get; set; }
    public SubmissionStatus? Status { get; set; }
    public string? Department { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }

    public List<Module2ListItemViewModel> Items { get; set; } = new();
}
```

> **Nota:** `Department` di sini ditapis melalui `UserProfile` (entiti kongsi Hari 1) yang menyimpan jabatan setiap staf — bukan pada `Submission` itu sendiri. Jika `UserProfile` projek anda ada nama medan berbeza, laraskan `join`/`Include` di bawah mengikutnya.

### 1.2 — Controller `Module2AdminController`

Cipta `Controllers/Module2AdminController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.ViewModels;

namespace Nres.Onboarding.Web.Controllers;

[Authorize(Roles = "SecurityAdmin")]
public class Module2AdminController : Controller
{
    private readonly ApplicationDbContext _db;

    public Module2AdminController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(Module2AdminFilterViewModel filter)
    {
        // Setiap sub-query ditapis (status/tarikh) SEBELUM digabung — supaya SQL Server/SQLite
        // yang tapis, bukan senarai C# selepas semua data ditarik ke memori.
        var passQuery = _db.AccessPassApplications
            .Include(a => a.Submission)
            .Where(a => filter.RequestType == null || filter.RequestType == Module2RequestType.SecurityPass)
            .Select(a => new Module2ListItemViewModel
            {
                SubmissionId = a.SubmissionId,
                DetailId = a.Id,
                RequestType = Module2RequestType.SecurityPass,
                ReferenceNo = a.Submission.ReferenceNo,
                ApplicantUserId = a.Submission.ApplicantUserId,
                Status = a.Submission.Status,
                CreatedAt = a.Submission.CreatedAt,
                SubmittedAt = a.Submission.SubmittedAt
            });

        var stickerQuery = _db.VehicleStickerApplications
            .Include(a => a.Submission)
            .Where(a => filter.RequestType == null || filter.RequestType == Module2RequestType.VehicleSticker)
            .Select(a => new Module2ListItemViewModel
            {
                SubmissionId = a.SubmissionId,
                DetailId = a.Id,
                RequestType = Module2RequestType.VehicleSticker,
                ReferenceNo = a.Submission.ReferenceNo,
                ApplicantUserId = a.Submission.ApplicantUserId,
                Status = a.Submission.Status,
                CreatedAt = a.Submission.CreatedAt,
                SubmittedAt = a.Submission.SubmittedAt
            });

        var parkingQuery = _db.ParkingApplications
            .Include(a => a.Submission)
            .Where(a => filter.RequestType == null || filter.RequestType == Module2RequestType.Parking)
            .Select(a => new Module2ListItemViewModel
            {
                SubmissionId = a.SubmissionId,
                DetailId = a.Id,
                RequestType = Module2RequestType.Parking,
                ReferenceNo = a.Submission.ReferenceNo,
                ApplicantUserId = a.Submission.ApplicantUserId,
                Status = a.Submission.Status,
                CreatedAt = a.Submission.CreatedAt,
                SubmittedAt = a.Submission.SubmittedAt
            });

        // Gabung tiga jadual berbeza -> satu senarai. EF Core menterjemah Concat() kepada UNION ALL.
        var combined = passQuery.Concat(stickerQuery).Concat(parkingQuery);

        if (filter.Status is not null)
        {
            combined = combined.Where(x => x.Status == filter.Status);
        }

        if (filter.DateFrom is not null)
        {
            combined = combined.Where(x => x.SubmittedAt >= filter.DateFrom);
        }

        if (filter.DateTo is not null)
        {
            var inclusiveEnd = filter.DateTo.Value.Date.AddDays(1);
            combined = combined.Where(x => x.SubmittedAt < inclusiveEnd);
        }

        var items = await combined
            .OrderByDescending(x => x.CreatedAt)
            .ToListAsync();

        // Filter jabatan disemak selepas gabung, kerana ia memerlukan join ke UserProfile
        // berasaskan ApplicantUserId yang hanya kita ada selepas projection selesai.
        if (!string.IsNullOrWhiteSpace(filter.Department))
        {
            var userIdsInDept = await _db.UserProfiles
                .Where(p => p.Department == filter.Department)
                .Select(p => p.UserId)
                .ToListAsync();

            items = items.Where(x => userIdsInDept.Contains(x.ApplicantUserId)).ToList();
        }

        filter.Items = items;
        return View(filter);
    }
}
```

> **Nota tentang `UserProfiles`:** entiti ini dikongsi sejak Hari 1 (`UserProfile` dengan sekurang-kurangnya `UserId`, `FullName`, `Department`). Jika struktur sebenar projek anda berbeza sedikit, laraskan nama medan filter jabatan — konsepnya (tapis melalui jadual profil, bukan Submission) kekal sama.

### 1.3 — View senarai dengan borang filter

Cipta `Views/Module2Admin/Index.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Module2AdminFilterViewModel
@{
    ViewData["Title"] = "Semakan Admin — Pas/Parking/Pelekat";
}

<h1>@ViewData["Title"]</h1>

<form method="get" class="row g-2 mb-4">
    <div class="col-md-2">
        <select asp-for="RequestType" asp-items="Html.GetEnumSelectList<Nres.Onboarding.Web.ViewModels.Module2RequestType>()" class="form-select">
            <option value="">-- Semua Jenis --</option>
        </select>
    </div>
    <div class="col-md-2">
        <select asp-for="Status" asp-items="Html.GetEnumSelectList<Nres.Onboarding.Web.Models.SubmissionStatus>()" class="form-select">
            <option value="">-- Semua Status --</option>
        </select>
    </div>
    <div class="col-md-2">
        <input asp-for="Department" class="form-control" placeholder="Jabatan" />
    </div>
    <div class="col-md-2">
        <input asp-for="DateFrom" class="form-control" placeholder="Dari tarikh" />
    </div>
    <div class="col-md-2">
        <input asp-for="DateTo" class="form-control" placeholder="Hingga tarikh" />
    </div>
    <div class="col-md-2">
        <button type="submit" class="btn btn-primary w-100">Tapis</button>
    </div>
</form>

<table class="table table-striped">
    <thead>
        <tr>
            <th>No. Rujukan</th>
            <th>Jenis</th>
            <th>Status</th>
            <th>Dihantar</th>
            <th></th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model.Items)
        {
            <tr>
                <td>@item.ReferenceNo</td>
                <td>@item.RequestTypeLabel</td>
                <td>@item.Status</td>
                <td>@(item.SubmittedAt?.ToString("dd/MM/yyyy") ?? "-")</td>
                <td>
                    <a class="btn btn-sm btn-outline-primary"
                       asp-action="Details"
                       asp-route-requestType="@item.RequestType"
                       asp-route-id="@item.DetailId">Semak</a>
                </td>
            </tr>
        }
        @if (!Model.Items.Any())
        {
            <tr><td colspan="5" class="text-center text-muted">Tiada permohonan sepadan penapis semasa.</td></tr>
        }
    </tbody>
</table>
```

✅ **Semakan:** `/Module2Admin` memaparkan permohonan daripada **ketiga-tiga** jenis dalam satu jadual. Cuba tapis mengikut `RequestType = VehicleSticker` — hanya pelekat kenderaan kelihatan. Cuba tapis mengikut julat tarikh yang tidak merangkumi mana-mana permohonan — jadual kosong dengan mesej "Tiada permohonan sepadan".

---

## Latihan 2 — Halaman Detail Semakan

**Objektif:** Bina satu halaman `Details` yang memuatkan rekod yang betul daripada jadual yang betul, bergantung parameter `requestType`.

Tambah kaedah berikut ke `Module2AdminController`:

```csharp
public async Task<IActionResult> Details(Module2RequestType requestType, int id)
{
    object? detail = requestType switch
    {
        Module2RequestType.SecurityPass => await _db.AccessPassApplications
            .Include(a => a.Submission)
            .FirstOrDefaultAsync(a => a.Id == id),

        Module2RequestType.VehicleSticker => await _db.VehicleStickerApplications
            .Include(a => a.Submission)
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.Id == id),

        Module2RequestType.Parking => await _db.ParkingApplications
            .Include(a => a.Submission)
            .Include(a => a.Vehicle)
            .FirstOrDefaultAsync(a => a.Id == id),

        _ => null
    };

    if (detail is null)
    {
        return NotFound();
    }

    ViewBag.RequestType = requestType;

    var auditLogs = await _db.AuditLogs
        .Where(l => l.SubmissionId == GetSubmissionId(detail))
        .OrderByDescending(l => l.CreatedAt)
        .ToListAsync();
    ViewBag.AuditLogs = auditLogs;

    return View(detail);
}

private static int GetSubmissionId(object detail) => detail switch
{
    Models.AccessPassApplication a => a.SubmissionId,
    Models.VehicleStickerApplication s => s.SubmissionId,
    Models.ParkingApplication p => p.SubmissionId,
    _ => 0
};
```

Cipta `Views/Module2Admin/Details.cshtml` (guna `@model dynamic` supaya satu view boleh terima ketiga-tiga jenis kelas — pendekatan ringkas untuk latihan; projek produksi sebenar mungkin pilih tiga view berasingan atau satu view model gabungan):

```cshtml
@model dynamic
@{
    ViewData["Title"] = "Butiran Permohonan";
    var requestType = (Nres.Onboarding.Web.ViewModels.Module2RequestType)ViewBag.RequestType;
    var auditLogs = ViewBag.AuditLogs as List<Nres.Onboarding.Web.Models.AuditLog> ?? new();
}

<h1>@ViewData["Title"]</h1>

<dl class="row">
    <dt class="col-sm-3">No. Rujukan</dt>
    <dd class="col-sm-9">@Model.Submission.ReferenceNo</dd>

    <dt class="col-sm-3">Jenis</dt>
    <dd class="col-sm-9">@requestType</dd>

    <dt class="col-sm-3">Status</dt>
    <dd class="col-sm-9">@Model.Submission.Status</dd>

    <dt class="col-sm-3">Dihantar</dt>
    <dd class="col-sm-9">@(Model.Submission.SubmittedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-")</dd>

    @if (requestType == Nres.Onboarding.Web.ViewModels.Module2RequestType.SecurityPass)
    {
        <dt class="col-sm-3">Jenis Pas</dt>
        <dd class="col-sm-9">@Model.PassType</dd>
        <dt class="col-sm-3">Kawasan Akses</dt>
        <dd class="col-sm-9">@Model.AccessAreaRequested</dd>
    }
    else
    {
        <dt class="col-sm-3">Kenderaan</dt>
        <dd class="col-sm-9">@Model.Vehicle.RegistrationNo — @Model.Vehicle.MakeModel (@Model.Vehicle.Color)</dd>
    }
</dl>

<h4>Sejarah Audit</h4>
<ul class="list-group mb-4">
    @foreach (var log in auditLogs)
    {
        <li class="list-group-item">
            <strong>@log.Action</strong> — @log.CreatedAt.ToString("dd/MM/yyyy HH:mm")
            @if (!string.IsNullOrEmpty(log.Remarks)) { <span>: @log.Remarks</span> }
        </li>
    }
</ul>

@if (Model.Submission.Status == Nres.Onboarding.Web.Models.SubmissionStatus.Submitted)
{
    <form asp-action="Approve" method="post" class="d-inline">
        <input type="hidden" name="requestType" value="@requestType" />
        <input type="hidden" name="id" value="@Model.Id" />
        @if (requestType == Nres.Onboarding.Web.ViewModels.Module2RequestType.VehicleSticker)
        {
            <input name="stickerNoIssued" class="form-control d-inline w-auto" placeholder="No. Pelekat Fizikal" required />
        }
        <button type="submit" class="btn btn-success">Luluskan</button>
    </form>

    <form asp-action="Reject" method="post" class="d-inline">
        <input type="hidden" name="requestType" value="@requestType" />
        <input type="hidden" name="id" value="@Model.Id" />
        <input name="remarks" class="form-control d-inline w-auto" placeholder="Sebab tolak (wajib)" required />
        <button type="submit" class="btn btn-danger">Tolak</button>
    </form>
}

<a asp-action="Print" asp-route-requestType="@requestType" asp-route-id="@Model.Id" class="btn btn-outline-secondary" target="_blank">
    🖨️ Cetak Ringkasan
</a>
```

✅ **Semakan:** Klik "Semak" daripada senarai membawa ke halaman detail yang betul, papar butiran khusus jenis (pas / kenderaan), dan sejarah audit.

---

## Latihan 3 — Approve (Dengan Nombor Pelekat untuk Jenis Sticker)

**Objektif:** Kuatkuasakan kelulusan hanya oleh `SecurityAdmin`, hanya untuk permohonan berstatus `Submitted`, dan wajib nombor pelekat fizikal untuk jenis pelekat kenderaan.

Tambah kaedah berikut ke `Module2AdminController`:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Approve(Module2RequestType requestType, int id, string? stickerNoIssued)
{
    Models.Submission? submission = requestType switch
    {
        Module2RequestType.SecurityPass => (await _db.AccessPassApplications
            .Include(a => a.Submission).FirstOrDefaultAsync(a => a.Id == id))?.Submission,

        Module2RequestType.VehicleSticker => (await _db.VehicleStickerApplications
            .Include(a => a.Submission).FirstOrDefaultAsync(a => a.Id == id))?.Submission,

        Module2RequestType.Parking => (await _db.ParkingApplications
            .Include(a => a.Submission).FirstOrDefaultAsync(a => a.Id == id))?.Submission,

        _ => null
    };

    if (submission is null)
    {
        return NotFound();
    }

    if (submission.Status != Models.SubmissionStatus.Submitted)
    {
        TempData["Message"] = "Permohonan ini sudah diproses dan tidak boleh diluluskan semula.";
        return RedirectToAction(nameof(Details), new { requestType, id });
    }

    if (requestType == Module2RequestType.VehicleSticker)
    {
        if (string.IsNullOrWhiteSpace(stickerNoIssued))
        {
            TempData["Message"] = "Nombor pelekat fizikal wajib diisi semasa kelulusan pelekat kenderaan.";
            return RedirectToAction(nameof(Details), new { requestType, id });
        }

        var sticker = await _db.VehicleStickerApplications.FirstAsync(a => a.Id == id);
        sticker.StickerNoIssued = stickerNoIssued;
    }

    submission.Status = Models.SubmissionStatus.AdminApproved;
    await _db.SaveChangesAsync();

    await _auditLogService.RecordAsync(submission.Id, "Approve");

    TempData["Message"] = $"Permohonan {submission.ReferenceNo} diluluskan.";
    return RedirectToAction(nameof(Details), new { requestType, id });
}
```

> Tambah `IAuditLogService _auditLogService` ke constructor `Module2AdminController` (sama corak seperti controller Hari 5) jika belum ada.

**Kenapa semak `submission.Status != Submitted` dahulu?** Ini elak permohonan yang **sudah** diluluskan/ditolak diproses **semula** (cth. dua tab pelayar terbuka serentak, admin klik "Luluskan" dua kali). Semakan status sebelum tindakan ialah pertahanan asas terhadap "double action" — konsep yang diperhalusi dengan `IWorkflowService` formal di Hari 8.

✅ **Semakan:** Luluskan satu permohonan pelekat kenderaan **tanpa** isi `stickerNoIssued` — mesti disekat dengan mesej. Isi `stickerNoIssued` dan luluskan — status bertukar `AdminApproved`, audit log baharu "Approve" kelihatan. Cuba luluskan permohonan yang **sama** sekali lagi — mesti disekat kerana status bukan lagi `Submitted`.

---

## Latihan 4 — Reject (Wajib Remarks)

**Objektif:** Kuatkuasakan sebab penolakan wajib diisi, konsisten dengan peraturan modul Lapor Diri Hari 3.

Tambah kaedah berikut ke `Module2AdminController`:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Reject(Module2RequestType requestType, int id, string remarks)
{
    if (string.IsNullOrWhiteSpace(remarks))
    {
        TempData["Message"] = "Sebab penolakan wajib diisi.";
        return RedirectToAction(nameof(Details), new { requestType, id });
    }

    Models.Submission? submission = requestType switch
    {
        Module2RequestType.SecurityPass => (await _db.AccessPassApplications
            .Include(a => a.Submission).FirstOrDefaultAsync(a => a.Id == id))?.Submission,

        Module2RequestType.VehicleSticker => (await _db.VehicleStickerApplications
            .Include(a => a.Submission).FirstOrDefaultAsync(a => a.Id == id))?.Submission,

        Module2RequestType.Parking => (await _db.ParkingApplications
            .Include(a => a.Submission).FirstOrDefaultAsync(a => a.Id == id))?.Submission,

        _ => null
    };

    if (submission is null)
    {
        return NotFound();
    }

    if (submission.Status != Models.SubmissionStatus.Submitted)
    {
        TempData["Message"] = "Permohonan ini sudah diproses.";
        return RedirectToAction(nameof(Details), new { requestType, id });
    }

    submission.Status = Models.SubmissionStatus.Rejected;
    await _db.SaveChangesAsync();

    await _auditLogService.RecordAsync(submission.Id, "Reject", remarks);

    TempData["Message"] = $"Permohonan {submission.ReferenceNo} ditolak.";
    return RedirectToAction(nameof(Details), new { requestType, id });
}
```

✅ **Semakan:** Cuba tolak dengan medan `remarks` kosong (padam nilai placeholder sebelum hantar) — mesti disekat. Tolak dengan sebab diisi — status bertukar `Rejected`, audit log "Reject" memaparkan sebab tersebut.

---

## Latihan 5 — Ringkasan Boleh Cetak (`@media print`)

**Objektif:** Bina satu Razor view boleh cetak yang berfungsi untuk ketiga-tiga jenis permohonan, dengan butang/navigasi tersembunyi semasa cetak.

Tambah kaedah `Print` ke `Module2AdminController` (boleh guna semula logik `Details` di atas):

```csharp
public async Task<IActionResult> Print(Module2RequestType requestType, int id)
{
    return await Details(requestType, id) switch
    {
        ViewResult { Model: not null } detailsResult => View("Print", detailsResult.Model),
        var other => other
    };
}
```

Cipta `Views/Module2Admin/Print.cshtml`:

```cshtml
@model dynamic
@{
    Layout = null;
    var requestType = (Nres.Onboarding.Web.ViewModels.Module2RequestType)ViewBag.RequestType;
}
<!DOCTYPE html>
<html lang="ms">
<head>
    <meta charset="utf-8" />
    <title>Ringkasan Permohonan — @Model.Submission.ReferenceNo</title>
    <style>
        body { font-family: Arial, sans-serif; margin: 2rem; color: #111; }
        h1 { font-size: 1.4rem; border-bottom: 2px solid #333; padding-bottom: 0.5rem; }
        dl { display: grid; grid-template-columns: 200px 1fr; row-gap: 0.4rem; }
        dt { font-weight: bold; }
        .no-print { margin-top: 2rem; }

        @@media print {
            .no-print {
                display: none;
            }
            body {
                margin: 0.5cm;
            }
        }
    </style>
</head>
<body>
    <h1>Ringkasan Permohonan — Modul Pas, Parking &amp; Pelekat Kenderaan</h1>

    <dl>
        <dt>No. Rujukan</dt>
        <dd>@Model.Submission.ReferenceNo</dd>

        <dt>Jenis Permohonan</dt>
        <dd>@requestType</dd>

        <dt>Status</dt>
        <dd>@Model.Submission.Status</dd>

        <dt>Tarikh Dihantar</dt>
        <dd>@(Model.Submission.SubmittedAt?.ToString("dd/MM/yyyy HH:mm") ?? "-")</dd>

        @if (requestType == Nres.Onboarding.Web.ViewModels.Module2RequestType.SecurityPass)
        {
            <dt>Jenis Pas</dt>
            <dd>@Model.PassType</dd>
            <dt>Kawasan Akses</dt>
            <dd>@Model.AccessAreaRequested</dd>
        }
        else
        {
            <dt>Kenderaan</dt>
            <dd>@Model.Vehicle.RegistrationNo — @Model.Vehicle.MakeModel</dd>
        }
    </dl>

    <p class="no-print">
        <button onclick="window.print()">🖨️ Cetak</button>
    </p>
</body>
</html>
```

**Perhatikan:**

- `Layout = null;` — halaman cetak **tidak** guna `_Layout.cshtml` biasa (elak navbar/menu ikut tercetak).
- `@@media` — kerana Razor guna `@` sebagai aksara khas, `@@` ialah cara "escape" untuk hasilkan literal `@media` dalam CSS output.
- `.no-print` menyembunyikan butang "Cetak" itu sendiri **semasa** cetak sebenar — pengguna tidak mahu butang UI muncul dalam kertas/PDF hasil cetakan.

✅ **Semakan:** Buka `/Module2Admin/Print?requestType=SecurityPass&id=1` (atau melalui butang "Cetak Ringkasan" di halaman Details) — sahkan ringkasan penuh dipaparkan tanpa navbar. Tekan **Ctrl+P**/**Cmd+P** pratonton cetakan pelayar — sahkan butang "🖨️ Cetak" **hilang** dalam pratonton tersebut.

---

## Latihan 6 — Skrip Ujian Manual Hujung-ke-Hujung Modul 2

**Objektif:** Sahkan keseluruhan Modul 2 (Hari 4–6) berfungsi sebagai satu aliran lengkap.

| # | Peranan | Tindakan | Jangkaan |
|---|---------|----------|----------|
| 1 | Applicant | Hantar permohonan pelekat kenderaan baharu | `ReferenceNo` bermula `STK-`, status `Submitted` |
| 2 | SecurityAdmin | Buka `/Module2Admin`, tapis `RequestType = VehicleSticker`, `Status = Submitted` | Permohonan di #1 kelihatan |
| 3 | SecurityAdmin | Buka Details, cuba Approve **tanpa** nombor pelekat | Disekat |
| 4 | SecurityAdmin | Approve **dengan** nombor pelekat | Status `AdminApproved`, audit "Approve" tercatat |
| 5 | Applicant | Hantar permohonan pas keselamatan ganti (`Replacement`) tanpa sebab | Disekat di borang (Hari 5) |
| 6 | Applicant | Hantar dengan sebab diisi | Berjaya, `ReferenceNo` bermula `PAS-` |
| 7 | SecurityAdmin | Tolak permohonan di #6 **tanpa** remarks | Disekat |
| 8 | SecurityAdmin | Tolak **dengan** remarks | Status `Rejected`, audit "Reject" dengan sebab tercatat |
| 9 | SecurityAdmin | Buka halaman Print bagi permohonan #4 | Ringkasan penuh tanpa navbar, `@media print` sembunyikan butang cetak |

✅ **Semakan akhir Hari 6:**

- Kesemua 9 langkah di atas lulus.
- `/Module2Admin` boleh tapis merentasi tiga jenis permohonan serentak dalam satu senarai.
- Setiap kelulusan/penolakan mencatat `AuditLog`.
- Peserta boleh terangkan **kenapa** `Concat()` (bukan gabung manual dalam memori) digunakan untuk senarai admin.

**Modul 2 (Pas, Parking & Pelekat Kenderaan) kini lengkap hujung ke hujung** — sambung ke [Hari 7](../../hari-7/) untuk mula Modul 3 (ID, AD & Email).
