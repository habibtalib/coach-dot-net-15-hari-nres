# Lab Hari 14 — Aset ICT: Borang, Kelulusan & Inventori

Lab ini mengiringi [`../README.md`](../README.md) Hari 14 dan **bina di atas** entiti Hari 13 (`Asset`, `SoftwareCatalogItem`, `SoftwareRequest`, `AssetLoanRequest`, `AssetReturn`). Ikut latihan secara berurutan. Rujuk [`../../projek/`](../../projek/) untuk banding selepas cuba sendiri.

> **Andaian shared services (sedia ada sejak Hari 1/3/8):** `IReferenceNumberService.GenerateAsync(string moduleCode)`, `IAuditLogService.RecordAsync(int submissionId, string action, string? remarks = null)`, `IWorkflowService.CanTransition(SubmissionStatus from, SubmissionStatus to)`, `ICurrentUserService.UserId`. Kod di bawah **guna** servis ini — jangan cipta semula.

> Borang di bawah **tidak** ulang penuh corak CRUD asas (Index/Details senarai) — anda sudah kuasai itu sejak Hari 2–12. Fokus lab ini ialah **konsep baharu**: semakan availability & transaksi inventori.

---

## Senarai Semak Sebelum Mula

- [ ] Hari 13 selesai — 5 entiti Modul 5 wujud, migration `AddIctAssets` dijalankan, seed berjaya
- [ ] Roles `Applicant` dan `IctAdmin` sudah wujud dalam Identity (dari Hari 1 seed roles) dan anda ada sekurang-kurangnya satu akaun ujian bagi setiap role
- [ ] `ApplicationDbContext`, `Submission`, `SubmissionStatus` sedia ada

---

## Latihan 1 — View Models

**Objektif:** Asingkan borang (view model) daripada entiti — corak sama sejak Hari 2.

Cipta fail `ViewModels/SoftwareRequestCreateViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nres.Onboarding.Web.ViewModels;

public class SoftwareRequestCreateViewModel
{
    [Required(ErrorMessage = "Sila pilih perisian.")]
    [Display(Name = "Perisian")]
    public int SoftwareCatalogItemId { get; set; }

    public List<SelectListItem> AvailableSoftware { get; set; } = new();

    [Required(ErrorMessage = "Sebab/justifikasi permohonan wajib diisi.")]
    [StringLength(500)]
    [Display(Name = "Sebab / Justifikasi")]
    public string Justification { get; set; } = string.Empty;

    [StringLength(100)]
    [Display(Name = "Nama Komputer Sasaran (jika ada)")]
    public string? TargetComputerName { get; set; }
}
```

Cipta fail `ViewModels/AssetLoanRequestCreateViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nres.Onboarding.Web.ViewModels;

public class AssetLoanRequestCreateViewModel
{
    [Required(ErrorMessage = "Sila pilih kategori aset.")]
    [Display(Name = "Kategori Aset")]
    public string RequestedCategory { get; set; } = string.Empty;

    public List<SelectListItem> AvailableCategories { get; set; } = new();

    [Required(ErrorMessage = "Tujuan pinjaman wajib diisi.")]
    [StringLength(500)]
    [Display(Name = "Tujuan Pinjaman")]
    public string Purpose { get; set; } = string.Empty;

    [Required(ErrorMessage = "Tarikh diperlukan wajib diisi.")]
    [DataType(DataType.Date)]
    [Display(Name = "Diperlukan Dari")]
    public DateTime NeededFrom { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Jangka Pemulangan (anggaran)")]
    public DateTime? ExpectedReturnDate { get; set; }
}
```

Cipta fail `ViewModels/AssetReturnCreateViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nres.Onboarding.Web.ViewModels;

public class AssetReturnCreateViewModel
{
    [Required(ErrorMessage = "Sila pilih pinjaman yang hendak dipulangkan.")]
    [Display(Name = "Pinjaman Aset")]
    public int AssetLoanRequestId { get; set; }

    public List<SelectListItem> MyActiveLoans { get; set; } = new();

    [Required(ErrorMessage = "Sila nyatakan kondisi aset semasa pulang.")]
    [StringLength(50)]
    [Display(Name = "Kondisi Aset")]
    public string ConditionOnReturn { get; set; } = string.Empty;

    [StringLength(500)]
    [Display(Name = "Catatan")]
    public string? Remarks { get; set; }
}
```

Cipta fail `ViewModels/AssetLoanFulfillmentViewModel.cs` (untuk ICT Admin tetapkan aset sebenar):

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nres.Onboarding.Web.ViewModels;

public class AssetLoanFulfillmentViewModel
{
    public int AssetLoanRequestId { get; set; }
    public string RequestedCategory { get; set; } = string.Empty;
    public string ApplicantUserId { get; set; } = string.Empty;

    [Required(ErrorMessage = "Sila pilih aset untuk diserahkan.")]
    [Display(Name = "Aset Sedia Ada (Available)")]
    public int AssetId { get; set; }

    public List<SelectListItem> AvailableAssets { get; set; } = new();
}
```

Cipta fail `ViewModels/AssetReturnCompletionViewModel.cs` (untuk ICT Admin selesaikan pemulangan):

```csharp
using System.ComponentModel.DataAnnotations;

namespace Nres.Onboarding.Web.ViewModels;

public class AssetReturnCompletionViewModel
{
    public int AssetReturnId { get; set; }
    public string AssetTag { get; set; } = string.Empty;

    [Display(Name = "Perlu Penyelenggaraan?")]
    public bool RequiresMaintenance { get; set; }

    [StringLength(500)]
    [Display(Name = "Catatan ICT")]
    public string? AdminRemarks { get; set; }
}
```

Cipta fail `ViewModels/RejectViewModel.cs` (digunakan semula ketiga-tiga borang):

```csharp
using System.ComponentModel.DataAnnotations;

namespace Nres.Onboarding.Web.ViewModels;

public class RejectViewModel
{
    public int SubmissionRelatedId { get; set; }

    [Required(ErrorMessage = "Sebab penolakan wajib diisi.")]
    [StringLength(500)]
    [Display(Name = "Sebab Penolakan")]
    public string RejectReason { get; set; } = string.Empty;
}
```

✅ **Semakan:** `dotnet build` berjaya. 7 fail view model baharu wujud dalam `ViewModels/`.

---

## Latihan 2 — Borang Permohonan Perisian (`SoftwareRequestsController`)

**Objektif:** Borang pertama — draf dibenarkan data tidak lengkap, submit perlu validation penuh + nombor rujukan `SW`.

Cipta `Controllers/SoftwareRequestsController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class SoftwareRequestsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IReferenceNumberService _referenceNumberService;
    private readonly IAuditLogService _auditLogService;
    private readonly IWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUserService;

    public SoftwareRequestsController(
        ApplicationDbContext db,
        IReferenceNumberService referenceNumberService,
        IAuditLogService auditLogService,
        IWorkflowService workflowService,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _referenceNumberService = referenceNumberService;
        _auditLogService = auditLogService;
        _workflowService = workflowService;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index()
    {
        var myRequests = await _db.SoftwareRequests
            .Include(sr => sr.Submission)
            .Include(sr => sr.SoftwareCatalogItem)
            .Where(sr => sr.Submission.ApplicantUserId == _currentUserService.UserId)
            .OrderByDescending(sr => sr.Submission.CreatedAt)
            .ToListAsync();

        return View(myRequests);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new SoftwareRequestCreateViewModel
        {
            AvailableSoftware = await GetSoftwareOptionsAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SoftwareRequestCreateViewModel model, string submitAction)
    {
        var isSubmitting = submitAction == "Submit";

        if (isSubmitting && !ModelState.IsValid)
        {
            model.AvailableSoftware = await GetSoftwareOptionsAsync();
            return View(model);
        }

        if (!isSubmitting)
        {
            // Simpan draf: benarkan data tidak lengkap (corak sama sejak Hari 2).
            ModelState.Clear();
        }

        var submission = new Submission
        {
            ModuleCode = "SW",
            ApplicantUserId = _currentUserService.UserId,
            Status = SubmissionStatus.Draft
        };

        var softwareRequest = new SoftwareRequest
        {
            Submission = submission,
            SoftwareCatalogItemId = model.SoftwareCatalogItemId,
            Justification = model.Justification,
            TargetComputerName = model.TargetComputerName
        };

        _db.SoftwareRequests.Add(softwareRequest);
        await _db.SaveChangesAsync();

        if (isSubmitting)
        {
            submission.ReferenceNo = await _referenceNumberService.GenerateAsync("SW");
            submission.Status = SubmissionStatus.Submitted;
            submission.SubmittedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _auditLogService.RecordAsync(submission.Id, "Submitted",
                $"Permohonan perisian dihantar: {submission.ReferenceNo}");

            TempData["Success"] = $"Permohonan perisian dihantar. Nombor rujukan: {submission.ReferenceNo}";
        }
        else
        {
            await _auditLogService.RecordAsync(submission.Id, "DraftSaved");
            TempData["Success"] = "Draf permohonan perisian disimpan.";
        }

        return RedirectToAction(nameof(Details), new { id = softwareRequest.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _db.SoftwareRequests
            .Include(sr => sr.Submission)
            .Include(sr => sr.SoftwareCatalogItem)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (request is null) return NotFound();

        return View(request);
    }

    [Authorize(Roles = "IctAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id)
    {
        var request = await _db.SoftwareRequests
            .Include(sr => sr.Submission)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (request is null) return NotFound();

        if (!_workflowService.CanTransition(request.Submission.Status, SubmissionStatus.Completed))
        {
            TempData["Error"] = "Peralihan status tidak sah untuk permohonan ini.";
            return RedirectToAction(nameof(Details), new { id });
        }

        request.Submission.Status = SubmissionStatus.Completed;
        request.Submission.CompletedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();

        await _auditLogService.RecordAsync(request.SubmissionId, "SoftwareRequestApproved",
            $"Perisian {request.SoftwareCatalogItem?.Name} diluluskan.");

        TempData["Success"] = "Permohonan perisian diluluskan & selesai.";
        return RedirectToAction(nameof(Details), new { id });
    }

    [Authorize(Roles = "IctAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, RejectViewModel model)
    {
        var request = await _db.SoftwareRequests
            .Include(sr => sr.Submission)
            .FirstOrDefaultAsync(sr => sr.Id == id);

        if (request is null) return NotFound();

        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(model.RejectReason))
        {
            TempData["Error"] = "Sebab penolakan wajib diisi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        request.Submission.Status = SubmissionStatus.Rejected;
        await _db.SaveChangesAsync();

        await _auditLogService.RecordAsync(request.SubmissionId, "Rejected", model.RejectReason);

        TempData["Success"] = "Permohonan perisian ditolak.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<List<SelectListItem>> GetSoftwareOptionsAsync()
    {
        return await _db.SoftwareCatalogItems
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .Select(c => new SelectListItem($"{c.Name} ({c.Vendor}) — {c.LicenseType}", c.Id.ToString()))
            .ToListAsync();
    }
}
```

**Perhatikan corak `submitAction`:** butang "Simpan Draf" hantar `submitAction=Draft`, butang "Hantar" hantar `submitAction=Submit`. Bila draf, `ModelState.Clear()` **sengaja** buang semua ralat validation supaya data tidak lengkap boleh disimpan — ini **bukan** cara mengelak validation secara tidak sengaja, ia keputusan reka bentuk eksplisit yang kita dokumenkan dengan komen.

Cipta `Views/SoftwareRequests/Create.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.SoftwareRequestCreateViewModel
@{
    ViewData["Title"] = "Permohonan Perisian";
}

<h1>Permohonan Perisian</h1>

<form asp-action="Create" method="post">
    <div asp-validation-summary="All" class="text-danger"></div>

    <div class="mb-3">
        <label asp-for="SoftwareCatalogItemId" class="form-label"></label>
        <select asp-for="SoftwareCatalogItemId" asp-items="Model.AvailableSoftware" class="form-select">
            <option value="">-- Pilih Perisian --</option>
        </select>
        <span asp-validation-for="SoftwareCatalogItemId" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="Justification" class="form-label"></label>
        <textarea asp-for="Justification" class="form-control" rows="3"></textarea>
        <span asp-validation-for="Justification" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="TargetComputerName" class="form-label"></label>
        <input asp-for="TargetComputerName" class="form-control" />
    </div>

    <button type="submit" name="submitAction" value="Draft" class="btn btn-secondary">Simpan Draf</button>
    <button type="submit" name="submitAction" value="Submit" class="btn btn-primary">Hantar Permohonan</button>
</form>
```

✅ **Semakan:** `dotnet run`, log masuk sebagai `Applicant`, layari `/SoftwareRequests/Create`. Klik "Simpan Draf" dengan `Justification` kosong — berjaya (rekod `Draft` tercipta). Klik "Hantar Permohonan" tanpa `Justification` — validation summary papar ralat.

---

## Latihan 3 — Borang Pinjaman Aset (`AssetLoanRequestsController`)

**Objektif:** Pemohon nyatakan **kategori** sahaja — bukan aset spesifik (ulang kaji README: kenapa).

Cipta `Controllers/AssetLoanRequestsController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class AssetLoanRequestsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IReferenceNumberService _referenceNumberService;
    private readonly IAuditLogService _auditLogService;
    private readonly IWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUserService;

    public AssetLoanRequestsController(
        ApplicationDbContext db,
        IReferenceNumberService referenceNumberService,
        IAuditLogService auditLogService,
        IWorkflowService workflowService,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _referenceNumberService = referenceNumberService;
        _auditLogService = auditLogService;
        _workflowService = workflowService;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index()
    {
        var myLoans = await _db.AssetLoanRequests
            .Include(l => l.Submission)
            .Include(l => l.Asset)
            .Where(l => l.Submission.ApplicantUserId == _currentUserService.UserId)
            .OrderByDescending(l => l.Submission.CreatedAt)
            .ToListAsync();

        return View(myLoans);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new AssetLoanRequestCreateViewModel
        {
            AvailableCategories = await GetCategoryOptionsAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetLoanRequestCreateViewModel model, string submitAction)
    {
        var isSubmitting = submitAction == "Submit";

        if (isSubmitting && !ModelState.IsValid)
        {
            model.AvailableCategories = await GetCategoryOptionsAsync();
            return View(model);
        }

        if (!isSubmitting)
        {
            ModelState.Clear();
        }

        var submission = new Submission
        {
            ModuleCode = "AST-L",
            ApplicantUserId = _currentUserService.UserId,
            Status = SubmissionStatus.Draft
        };

        var loanRequest = new AssetLoanRequest
        {
            Submission = submission,
            RequestedCategory = model.RequestedCategory,
            Purpose = model.Purpose,
            NeededFrom = model.NeededFrom,
            ExpectedReturnDate = model.ExpectedReturnDate
        };

        _db.AssetLoanRequests.Add(loanRequest);
        await _db.SaveChangesAsync();

        if (isSubmitting)
        {
            submission.ReferenceNo = await _referenceNumberService.GenerateAsync("AST-L");
            submission.Status = SubmissionStatus.Submitted;
            submission.SubmittedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _auditLogService.RecordAsync(submission.Id, "Submitted",
                $"Permohonan pinjaman aset dihantar: {submission.ReferenceNo}");

            TempData["Success"] = $"Permohonan pinjaman dihantar. Nombor rujukan: {submission.ReferenceNo}";
        }
        else
        {
            await _auditLogService.RecordAsync(submission.Id, "DraftSaved");
            TempData["Success"] = "Draf permohonan pinjaman disimpan.";
        }

        return RedirectToAction(nameof(Details), new { id = loanRequest.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var loanRequest = await _db.AssetLoanRequests
            .Include(l => l.Submission)
            .Include(l => l.Asset)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loanRequest is null) return NotFound();

        return View(loanRequest);
    }

    private async Task<List<SelectListItem>> GetCategoryOptionsAsync()
    {
        var categories = await _db.Assets
            .Select(a => a.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return categories.Select(c => new SelectListItem(c, c)).ToList();
    }

    // Approve/Reject/Fulfill ICT actions — Latihan 4.
}
```

Cipta `Views/AssetLoanRequests/Create.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.AssetLoanRequestCreateViewModel
@{
    ViewData["Title"] = "Permohonan Pinjaman Aset";
}

<h1>Permohonan Pinjaman Aset</h1>

<form asp-action="Create" method="post">
    <div asp-validation-summary="All" class="text-danger"></div>

    <div class="mb-3">
        <label asp-for="RequestedCategory" class="form-label"></label>
        <select asp-for="RequestedCategory" asp-items="Model.AvailableCategories" class="form-select">
            <option value="">-- Pilih Kategori --</option>
        </select>
        <span asp-validation-for="RequestedCategory" class="text-danger"></span>
        <div class="form-text">Anda pilih KATEGORI sahaja — ICT Admin akan tetapkan aset sebenar semasa fulfillment.</div>
    </div>

    <div class="mb-3">
        <label asp-for="Purpose" class="form-label"></label>
        <textarea asp-for="Purpose" class="form-control" rows="3"></textarea>
        <span asp-validation-for="Purpose" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="NeededFrom" class="form-label"></label>
        <input asp-for="NeededFrom" class="form-control" type="date" />
        <span asp-validation-for="NeededFrom" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="ExpectedReturnDate" class="form-label"></label>
        <input asp-for="ExpectedReturnDate" class="form-control" type="date" />
    </div>

    <button type="submit" name="submitAction" value="Draft" class="btn btn-secondary">Simpan Draf</button>
    <button type="submit" name="submitAction" value="Submit" class="btn btn-primary">Hantar Permohonan</button>
</form>
```

✅ **Semakan:** Borang berjaya hantar permohonan dengan `RequestedCategory` (cth. "Laptop"), `AssetId` **masih null** dalam pangkalan data (sahkan dengan `sqlite3 nres_onboarding.db "SELECT Id, RequestedCategory, AssetId FROM AssetLoanRequests;"`).

---

## Latihan 4 — Semakan Availability & Fulfillment (ICT Admin)

**Objektif:** Ini **teras** hari ini — ICT Admin lihat aset `Available` bagi kategori dipohon, pilih satu, dan sistem kemas kini **status permohonan + status aset serentak** dalam satu transaksi.

Tambah kaedah berikut ke `AssetLoanRequestsController` (gantikan komen `// Approve/Reject/Fulfill...`):

```csharp
    [Authorize(Roles = "IctAdmin")]
    [HttpGet]
    public async Task<IActionResult> Fulfill(int id)
    {
        var loanRequest = await _db.AssetLoanRequests
            .Include(l => l.Submission)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loanRequest is null) return NotFound();

        if (loanRequest.Submission.Status != SubmissionStatus.Submitted)
        {
            TempData["Error"] = "Permohonan ini tidak dalam status sedia untuk fulfillment.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new AssetLoanFulfillmentViewModel
        {
            AssetLoanRequestId = loanRequest.Id,
            RequestedCategory = loanRequest.RequestedCategory,
            ApplicantUserId = loanRequest.Submission.ApplicantUserId,
            AvailableAssets = await GetAvailableAssetOptionsAsync(loanRequest.RequestedCategory)
        };

        if (model.AvailableAssets.Count == 0)
        {
            TempData["Warning"] = $"Tiada aset kategori '{loanRequest.RequestedCategory}' berstatus Available buat masa ini.";
        }

        return View(model);
    }

    [Authorize(Roles = "IctAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fulfill(AssetLoanFulfillmentViewModel model)
    {
        var loanRequest = await _db.AssetLoanRequests
            .Include(l => l.Submission)
            .FirstOrDefaultAsync(l => l.Id == model.AssetLoanRequestId);

        if (loanRequest is null) return NotFound();

        if (!ModelState.IsValid)
        {
            model.AvailableAssets = await GetAvailableAssetOptionsAsync(loanRequest.RequestedCategory);
            return View(model);
        }

        // Semakan availability SEKALI LAGI di sini (bukan hanya di GET) — mengelakkan
        // race condition jika aset ditukar status oleh permohonan lain antara GET dan POST.
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == model.AssetId);

        if (asset is null || asset.Status != AssetStatus.Available || asset.Category != loanRequest.RequestedCategory)
        {
            ModelState.AddModelError(string.Empty, "Aset yang dipilih sudah tidak Available. Sila pilih semula.");
            model.AvailableAssets = await GetAvailableAssetOptionsAsync(loanRequest.RequestedCategory);
            return View(model);
        }

        if (!_workflowService.CanTransition(loanRequest.Submission.Status, SubmissionStatus.Completed))
        {
            TempData["Error"] = "Peralihan status tidak sah untuk permohonan ini.";
            return RedirectToAction(nameof(Details), new { id = loanRequest.Id });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            loanRequest.AssetId = asset.Id;
            loanRequest.Submission.Status = SubmissionStatus.Completed;
            loanRequest.Submission.CompletedAt = DateTime.UtcNow;

            asset.Status = AssetStatus.OnLoan;
            asset.CurrentHolderUserId = loanRequest.Submission.ApplicantUserId;

            await _db.SaveChangesAsync();

            await _auditLogService.RecordAsync(
                loanRequest.SubmissionId,
                "AssetLoanCompleted",
                $"Aset {asset.AssetTag} diserahkan kepada {loanRequest.Submission.ApplicantUserId}");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        TempData["Success"] = $"Pinjaman selesai. Aset {asset.AssetTag} kini OnLoan.";
        return RedirectToAction(nameof(Details), new { id = loanRequest.Id });
    }

    [Authorize(Roles = "IctAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, RejectViewModel model)
    {
        var loanRequest = await _db.AssetLoanRequests
            .Include(l => l.Submission)
            .FirstOrDefaultAsync(l => l.Id == id);

        if (loanRequest is null) return NotFound();

        if (string.IsNullOrWhiteSpace(model.RejectReason))
        {
            TempData["Error"] = "Sebab penolakan wajib diisi.";
            return RedirectToAction(nameof(Details), new { id });
        }

        loanRequest.Submission.Status = SubmissionStatus.Rejected;
        await _db.SaveChangesAsync();

        await _auditLogService.RecordAsync(loanRequest.SubmissionId, "Rejected", model.RejectReason);

        TempData["Success"] = "Permohonan pinjaman ditolak.";
        return RedirectToAction(nameof(Details), new { id });
    }

    private async Task<List<SelectListItem>> GetAvailableAssetOptionsAsync(string category)
    {
        var assets = await _db.Assets
            .Where(a => a.Category == category && a.Status == AssetStatus.Available)
            .OrderBy(a => a.AssetTag)
            .ToListAsync();

        return assets
            .Select(a => new SelectListItem($"{a.AssetTag} — {a.BrandModel} (SN: {a.SerialNumber})", a.Id.ToString()))
            .ToList();
    }
```

Cipta `Views/AssetLoanRequests/Fulfill.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.AssetLoanFulfillmentViewModel
@{
    ViewData["Title"] = "Fulfillment Pinjaman Aset";
}

<h1>Fulfillment Pinjaman Aset</h1>
<p>Kategori dipohon: <strong>@Model.RequestedCategory</strong> · Pemohon: <strong>@Model.ApplicantUserId</strong></p>

<form asp-action="Fulfill" method="post">
    <input type="hidden" asp-for="AssetLoanRequestId" />
    <input type="hidden" asp-for="RequestedCategory" />
    <input type="hidden" asp-for="ApplicantUserId" />
    <div asp-validation-summary="All" class="text-danger"></div>

    <div class="mb-3">
        <label asp-for="AssetId" class="form-label"></label>
        <select asp-for="AssetId" asp-items="Model.AvailableAssets" class="form-select">
            <option value="">-- Pilih Aset Available --</option>
        </select>
        <span asp-validation-for="AssetId" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-primary" @(Model.AvailableAssets.Count == 0 ? "disabled" : "")>
        Sahkan Serahan Aset
    </button>
</form>
```

Perhatikan butang **dilumpuhkan** (`disabled`) apabila tiada aset `Available` — menghalang ICT Admin cuba fulfillment tanpa stok, selari dengan semakan server-side yang sudah kita tulis.

✅ **Semakan:**
1. Log masuk sebagai `IctAdmin`, layari `/AssetLoanRequests/Fulfill/{id}` bagi permohonan yang `Submitted`.
2. Pilih satu aset `Available`, klik "Sahkan Serahan Aset".
3. Sahkan dengan `sqlite3`: `SELECT Status, CurrentHolderUserId FROM Assets WHERE Id = <assetId>;` — status mesti `2` (`OnLoan`), `CurrentHolderUserId` mesti diisi.
4. Sahkan `Submissions.Status` bagi permohonan itu = `5` (`Completed`).
5. Cuba fulfillment **semula** kategori yang sama selepas semua aset habis — sahkan mesej "Tiada aset ... Available" terpapar & butang dilumpuhkan.

---

## Latihan 5 — Borang Pemulangan Aset (`AssetReturnsController`)

**Objektif:** Pemohon pilih **pinjaman aktif** miliknya sendiri untuk dipulangkan; ICT Admin selesaikan dengan menetapkan `Available` atau `UnderMaintenance`.

Cipta `Controllers/AssetReturnsController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class AssetReturnsController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly IReferenceNumberService _referenceNumberService;
    private readonly IAuditLogService _auditLogService;
    private readonly IWorkflowService _workflowService;
    private readonly ICurrentUserService _currentUserService;

    public AssetReturnsController(
        ApplicationDbContext db,
        IReferenceNumberService referenceNumberService,
        IAuditLogService auditLogService,
        IWorkflowService workflowService,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _referenceNumberService = referenceNumberService;
        _auditLogService = auditLogService;
        _workflowService = workflowService;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index()
    {
        var myReturns = await _db.AssetReturns
            .Include(r => r.Submission)
            .Include(r => r.Asset)
            .Where(r => r.Submission.ApplicantUserId == _currentUserService.UserId)
            .OrderByDescending(r => r.Submission.CreatedAt)
            .ToListAsync();

        return View(myReturns);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new AssetReturnCreateViewModel
        {
            MyActiveLoans = await GetMyActiveLoanOptionsAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetReturnCreateViewModel model, string submitAction)
    {
        var isSubmitting = submitAction == "Submit";

        if (isSubmitting && !ModelState.IsValid)
        {
            model.MyActiveLoans = await GetMyActiveLoanOptionsAsync();
            return View(model);
        }

        if (!isSubmitting)
        {
            ModelState.Clear();
        }

        var loanRequest = await _db.AssetLoanRequests
            .Include(l => l.Submission)
            .FirstOrDefaultAsync(l => l.Id == model.AssetLoanRequestId);

        if (loanRequest?.AssetId is null)
        {
            ModelState.AddModelError(string.Empty, "Pinjaman aset tidak sah atau belum lengkap.");
            model.MyActiveLoans = await GetMyActiveLoanOptionsAsync();
            return View(model);
        }

        var submission = new Submission
        {
            ModuleCode = "AST-R",
            ApplicantUserId = _currentUserService.UserId,
            Status = SubmissionStatus.Draft
        };

        var assetReturn = new AssetReturn
        {
            Submission = submission,
            AssetLoanRequestId = loanRequest.Id,
            AssetId = loanRequest.AssetId.Value,
            ConditionOnReturn = model.ConditionOnReturn,
            Remarks = model.Remarks
        };

        _db.AssetReturns.Add(assetReturn);
        await _db.SaveChangesAsync();

        if (isSubmitting)
        {
            submission.ReferenceNo = await _referenceNumberService.GenerateAsync("AST-R");
            submission.Status = SubmissionStatus.Submitted;
            submission.SubmittedAt = DateTime.UtcNow;
            await _db.SaveChangesAsync();

            await _auditLogService.RecordAsync(submission.Id, "Submitted",
                $"Permohonan pemulangan aset dihantar: {submission.ReferenceNo}");

            TempData["Success"] = $"Permohonan pemulangan dihantar. Nombor rujukan: {submission.ReferenceNo}";
        }
        else
        {
            await _auditLogService.RecordAsync(submission.Id, "DraftSaved");
            TempData["Success"] = "Draf permohonan pemulangan disimpan.";
        }

        return RedirectToAction(nameof(Details), new { id = assetReturn.Id });
    }

    public async Task<IActionResult> Details(int id)
    {
        var assetReturn = await _db.AssetReturns
            .Include(r => r.Submission)
            .Include(r => r.Asset)
            .Include(r => r.AssetLoanRequest)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (assetReturn is null) return NotFound();

        return View(assetReturn);
    }

    [Authorize(Roles = "IctAdmin")]
    [HttpGet]
    public async Task<IActionResult> Complete(int id)
    {
        var assetReturn = await _db.AssetReturns
            .Include(r => r.Submission)
            .Include(r => r.Asset)
            .FirstOrDefaultAsync(r => r.Id == id);

        if (assetReturn is null) return NotFound();

        var model = new AssetReturnCompletionViewModel
        {
            AssetReturnId = assetReturn.Id,
            AssetTag = assetReturn.Asset.AssetTag
        };

        return View(model);
    }

    [Authorize(Roles = "IctAdmin")]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Complete(AssetReturnCompletionViewModel model)
    {
        var assetReturn = await _db.AssetReturns
            .Include(r => r.Submission)
            .Include(r => r.Asset)
            .FirstOrDefaultAsync(r => r.Id == model.AssetReturnId);

        if (assetReturn is null) return NotFound();

        if (!_workflowService.CanTransition(assetReturn.Submission.Status, SubmissionStatus.Completed))
        {
            TempData["Error"] = "Peralihan status tidak sah untuk permohonan ini.";
            return RedirectToAction(nameof(Details), new { id = assetReturn.Id });
        }

        await using var transaction = await _db.Database.BeginTransactionAsync();
        try
        {
            assetReturn.RequiresMaintenance = model.RequiresMaintenance;
            assetReturn.Remarks = model.AdminRemarks ?? assetReturn.Remarks;

            assetReturn.Submission.Status = SubmissionStatus.Completed;
            assetReturn.Submission.CompletedAt = DateTime.UtcNow;

            assetReturn.Asset.Status = model.RequiresMaintenance
                ? AssetStatus.UnderMaintenance
                : AssetStatus.Available;
            assetReturn.Asset.CurrentHolderUserId = null;

            await _db.SaveChangesAsync();

            await _auditLogService.RecordAsync(
                assetReturn.SubmissionId,
                "AssetReturnCompleted",
                $"Aset {assetReturn.Asset.AssetTag} dipulangkan — status baharu: {assetReturn.Asset.Status}");

            await transaction.CommitAsync();
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }

        TempData["Success"] = $"Pemulangan selesai. Aset kini {assetReturn.Asset.Status}.";
        return RedirectToAction(nameof(Details), new { id = assetReturn.Id });
    }

    private async Task<List<SelectListItem>> GetMyActiveLoanOptionsAsync()
    {
        var loans = await _db.AssetLoanRequests
            .Include(l => l.Submission)
            .Include(l => l.Asset)
            .Where(l => l.Submission.ApplicantUserId == _currentUserService.UserId
                     && l.Submission.Status == SubmissionStatus.Completed
                     && l.AssetId != null
                     && l.Asset!.Status == AssetStatus.OnLoan)
            .ToListAsync();

        return loans
            .Select(l => new SelectListItem(
                $"{l.Submission.ReferenceNo} — {l.Asset!.AssetTag} ({l.Asset.BrandModel})",
                l.Id.ToString()))
            .ToList();
    }
}
```

**Perhatikan query `GetMyActiveLoanOptionsAsync`:** ia gabungkan **tiga** syarat — pinjaman milik pengguna semasa, `Submission.Status == Completed` (pinjaman sudah diproses penuh), **dan** `Asset.Status == OnLoan` (aset belum dipulangkan lagi). Tanpa syarat ketiga, pengguna boleh cuba "pulangkan" aset yang **sudah** dipulangkan sebelum ini — pendua yang mengelirukan rekod inventori.

Cipta `Views/AssetReturns/Create.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.AssetReturnCreateViewModel
@{
    ViewData["Title"] = "Permohonan Pemulangan Aset";
}

<h1>Permohonan Pemulangan Aset</h1>

<form asp-action="Create" method="post">
    <div asp-validation-summary="All" class="text-danger"></div>

    <div class="mb-3">
        <label asp-for="AssetLoanRequestId" class="form-label"></label>
        <select asp-for="AssetLoanRequestId" asp-items="Model.MyActiveLoans" class="form-select">
            <option value="">-- Pilih Pinjaman --</option>
        </select>
        <span asp-validation-for="AssetLoanRequestId" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="ConditionOnReturn" class="form-label"></label>
        <input asp-for="ConditionOnReturn" class="form-control" placeholder="Cth: Baik / Rosak Ringan" />
        <span asp-validation-for="ConditionOnReturn" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="Remarks" class="form-label"></label>
        <textarea asp-for="Remarks" class="form-control" rows="3"></textarea>
    </div>

    <button type="submit" name="submitAction" value="Draft" class="btn btn-secondary">Simpan Draf</button>
    <button type="submit" name="submitAction" value="Submit" class="btn btn-primary">Hantar Permohonan</button>
</form>
```

Cipta `Views/AssetReturns/Complete.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.AssetReturnCompletionViewModel
@{
    ViewData["Title"] = "Selesaikan Pemulangan Aset";
}

<h1>Selesaikan Pemulangan — @Model.AssetTag</h1>

<form asp-action="Complete" method="post">
    <input type="hidden" asp-for="AssetReturnId" />
    <input type="hidden" asp-for="AssetTag" />

    <div class="mb-3 form-check">
        <input asp-for="RequiresMaintenance" class="form-check-input" />
        <label asp-for="RequiresMaintenance" class="form-check-label"></label>
        <div class="form-text">Jika ditanda, aset akan bertukar ke <code>UnderMaintenance</code> (bukan <code>Available</code>).</div>
    </div>

    <div class="mb-3">
        <label asp-for="AdminRemarks" class="form-label"></label>
        <textarea asp-for="AdminRemarks" class="form-control" rows="3"></textarea>
    </div>

    <button type="submit" class="btn btn-primary">Sahkan Pemulangan</button>
</form>
```

✅ **Semakan:**
1. Sebagai `Applicant` yang aset `ICT-AST-0001` (contoh) sedang `OnLoan` padanya, hantar permohonan pemulangan.
2. Sebagai `IctAdmin`, layari `/AssetReturns/Complete/{id}`. Cuba **tanpa** tanda `RequiresMaintenance` — sahkan `Asset.Status` bertukar `Available`, `CurrentHolderUserId` jadi `NULL`.
3. Ulang dengan pinjaman lain, kali ini **tanda** `RequiresMaintenance` — sahkan `Asset.Status` bertukar `UnderMaintenance`.
4. `sqlite3 nres_onboarding.db "SELECT AssetTag, Status, CurrentHolderUserId FROM Assets;"` — sahkan kedua-dua senario di atas.

---

## Latihan 6 — Uji Transaksi: Simulasi Kegagalan Separa

**Objektif:** Buktikan **kenapa** `BeginTransactionAsync` penting — bukan sekadar teori.

Buat **sementara** (jangan commit) satu ujian dalam kaedah `Fulfill` POST — selepas `asset.Status = AssetStatus.OnLoan;`, tambah baris yang sengaja gagal:

```csharp
asset.Status = AssetStatus.OnLoan;
asset.CurrentHolderUserId = loanRequest.Submission.ApplicantUserId;

throw new InvalidOperationException("UJIAN SENGAJA — simulasi kegagalan selepas kemas kini status");

await _db.SaveChangesAsync();
```

Jalankan fulfillment. **Apa yang patut berlaku:** exception ditangkap oleh `catch`, `transaction.RollbackAsync()` dipanggil, dan `dotnet run` papar ralat 500. Sahkan dengan `sqlite3` — `Asset.Status` **MASIH** `Available` (bukan `OnLoan`), kerana transaksi di-rollback **sebelum** apa-apa disimpan ke pangkalan data.

**Buang** baris `throw` selepas eksperimen ini selesai — ini eksperimen renungan sahaja, sama seperti eksperimen "buang `setState()`" dalam kelas Flutter.

✅ **Semakan:** Anda boleh terangkan lisan: kalau `throw` diletak **sebelum** `BeginTransactionAsync()` dipanggil langsung, adakah kesannya sama? (Jawapan: ya — tiada apa pun disimpan sebab `SaveChangesAsync()` belum dipanggil.) Kalau diletak **selepas** `transaction.CommitAsync()`, adakah rollback masih berlaku? (Jawapan: **tidak** — data sudah komited secara kekal; ini sebab urutan kod dalam blok transaksi penting.)

---

## Latihan 7 — Semakan Akhir Hari 14

Jalankan senario penuh ini sekali dari mula:

1. `Applicant` hantar permohonan perisian (`SW`) → sahkan nombor rujukan dijana.
2. `IctAdmin` approve permohonan perisian → `Submission.Status = Completed`.
3. `Applicant` hantar permohonan pinjaman aset kategori "Laptop" (`AST-L`).
4. `IctAdmin` fulfillment — pilih laptop `Available` → sahkan `Asset.Status = OnLoan`, `CurrentHolderUserId` diisi, `Submission.Status = Completed`.
5. `Applicant` (pemegang laptop tadi) hantar permohonan pemulangan (`AST-R`).
6. `IctAdmin` selesaikan pemulangan **tanpa** `RequiresMaintenance` → sahkan `Asset.Status = Available` semula.

✅ **Semakan akhir:**
- Ketiga-tiga borang berfungsi hujung-ke-hujung (draf, submit, nombor rujukan `SW`/`AST-L`/`AST-R`).
- Fulfillment pinjaman **menghalang** pemilihan aset yang bukan `Available`.
- Transaksi memastikan status permohonan & status aset sentiasa **selari** — tiada senario permohonan `Completed` tapi aset masih `Available` (atau sebaliknya).
- `dotnet build` bersih.

---

**Cross-ref rujukan:** Banding kod anda dengan `../../projek/Nres.Onboarding.Web/Controllers/` dan `../../projek/Nres.Onboarding.Web/Views/` selepas cuba sendiri.
