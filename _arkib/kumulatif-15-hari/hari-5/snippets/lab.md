# Lab Hari 5 — Borang & Peraturan Perniagaan Modul 2

Lab ini mengiringi [`README.md`](../README.md) Hari 5, dan menyambung terus daripada [Lab Hari 4](../../hari-4/snippets/lab.md) (entiti `Vehicle`, `AccessPassApplication`, `VehicleStickerApplication`, `ParkingApplication` sudah wujud, dengan tiga controller placeholder). Rujuk [`../../projek/`](../../projek/) untuk banding kod anda selepas cuba sendiri.

---

## Latihan 1 — `VehicleInputModel` Kongsi & Senarai Kenderaan Pemohon

**Objektif:** Cipta satu view model kenderaan yang dikongsi oleh borang Pelekat Kenderaan dan Parkir, membenarkan pemohon **pilih kenderaan sedia ada ATAU daftar kenderaan baharu** dalam borang yang sama.

### 1.1 — View model kenderaan

Cipta `ViewModels/VehicleInputModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models;

namespace Nres.Onboarding.Web.ViewModels;

public class VehicleInputModel
{
    /// <summary>Jika &gt; 0, pemohon memilih kenderaan sedia ada — medan di bawah diabaikan.</summary>
    public int ExistingVehicleId { get; set; }

    [StringLength(15)]
    [Display(Name = "Nombor Pendaftaran")]
    public string? RegistrationNo { get; set; }

    [Display(Name = "Jenis Kenderaan")]
    public VehicleType? Type { get; set; }

    [StringLength(100)]
    [Display(Name = "Jenama/Model")]
    public string? MakeModel { get; set; }

    [StringLength(30)]
    public string? Color { get; set; }

    [StringLength(100)]
    [Display(Name = "Nama Pemilik")]
    public string? OwnerName { get; set; }

    [Display(Name = "Hubungan Dengan Pemohon")]
    public OwnerRelationship? OwnerRelationship { get; set; }

    /// <summary>Wajib panggil dari IValidatableObject borang induk — bukan sendiri, kerana perlu tahu konteks (adakah medan ini "aktif").</summary>
    public IEnumerable<ValidationResult> ValidateNewVehicleFields(string memberPrefix)
    {
        if (ExistingVehicleId > 0)
        {
            yield break; // kenderaan sedia ada dipilih — tiada medan baharu perlu disahkan.
        }

        if (string.IsNullOrWhiteSpace(RegistrationNo))
        {
            yield return new ValidationResult(
                "Nombor pendaftaran kenderaan wajib diisi jika tiada kenderaan sedia ada dipilih.",
                new[] { $"{memberPrefix}.{nameof(RegistrationNo)}" });
        }

        if (Type is null)
        {
            yield return new ValidationResult(
                "Jenis kenderaan wajib dipilih.",
                new[] { $"{memberPrefix}.{nameof(Type)}" });
        }

        if (string.IsNullOrWhiteSpace(MakeModel))
        {
            yield return new ValidationResult(
                "Jenama/model kenderaan wajib diisi.",
                new[] { $"{memberPrefix}.{nameof(MakeModel)}" });
        }

        if (string.IsNullOrWhiteSpace(OwnerName))
        {
            yield return new ValidationResult(
                "Nama pemilik kenderaan wajib diisi.",
                new[] { $"{memberPrefix}.{nameof(OwnerName)}" });
        }

        if (OwnerRelationship is null)
        {
            yield return new ValidationResult(
                "Hubungan pemilik dengan pemohon wajib dipilih.",
                new[] { $"{memberPrefix}.{nameof(OwnerRelationship)}" });
        }
    }
}
```

**Kenapa `ValidateNewVehicleFields` ialah kaedah biasa, bukan `IValidatableObject.Validate` terus pada `VehicleInputModel`?** Kerana `VehicleInputModel` **disarangkan** (nested) di dalam view model borang Pelekat/Parkir — `IValidatableObject` pada kelas bersarang tidak dipanggil secara automatik oleh ASP.NET Core MVC melainkan objek induk memanggilnya secara eksplisit. Jadi kita panggil kaedah ini **dari dalam** `Validate()` view model induk (Latihan 3 & 4), lulus `memberPrefix` supaya mesej ralat validation summary menunjuk ke medan borang yang betul (cth. `Vehicle.RegistrationNo`).

✅ **Semakan:** `dotnet build` berjaya.

---

## Latihan 2 — Borang Pas Keselamatan (`AccessPassController`)

**Objektif:** Bina borang pas keselamatan penuh — conditional validation untuk `ReplacementReason`, semakan pendua "satu pas aktif per pemohon", dan draf/submit.

### 2.1 — View model

Cipta `ViewModels/AccessPassCreateViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models;

namespace Nres.Onboarding.Web.ViewModels;

public class AccessPassCreateViewModel : IValidatableObject
{
    [Required]
    [Display(Name = "Jenis Pas")]
    public SecurityPassType PassType { get; set; } = SecurityPassType.New;

    [StringLength(300)]
    [Display(Name = "Sebab Penggantian")]
    public string? ReplacementReason { get; set; }

    [Required(ErrorMessage = "Kawasan akses yang dipohon wajib diisi.")]
    [StringLength(300)]
    [Display(Name = "Kawasan Akses Dipohon")]
    public string AccessAreaRequested { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    [Display(Name = "Sah Mulai")]
    public DateTime ValidFrom { get; set; } = DateTime.Today;

    [DataType(DataType.Date)]
    [Display(Name = "Sah Sehingga")]
    public DateTime? ValidTo { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (PassType == SecurityPassType.Replacement && string.IsNullOrWhiteSpace(ReplacementReason))
        {
            yield return new ValidationResult(
                "Sebab penggantian wajib diisi untuk permohonan pas ganti.",
                new[] { nameof(ReplacementReason) });
        }

        if (ValidTo is not null && ValidTo < ValidFrom)
        {
            yield return new ValidationResult(
                "Tarikh 'Sah Sehingga' tidak boleh sebelum 'Sah Mulai'.",
                new[] { nameof(ValidTo) });
        }
    }
}
```

### 2.2 — Controller penuh

Gantikan **seluruh kandungan** `Controllers/AccessPassController.cs` (placeholder Hari 4) dengan:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels;

namespace Nres.Onboarding.Web.Controllers;

[Authorize(Roles = "Applicant")]
public class AccessPassController : Controller
{
    private const string ModuleCode = "PAS";

    private readonly ApplicationDbContext _db;
    private readonly IReferenceNumberService _referenceNumberService;
    private readonly IAuditLogService _auditLogService;
    private readonly ICurrentUserService _currentUserService;

    public AccessPassController(
        ApplicationDbContext db,
        IReferenceNumberService referenceNumberService,
        IAuditLogService auditLogService,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _referenceNumberService = referenceNumberService;
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _currentUserService.UserId;

        var myApplications = await _db.AccessPassApplications
            .Include(a => a.Submission)
            .Where(a => a.Submission.ApplicantUserId == userId)
            .OrderByDescending(a => a.Submission.CreatedAt)
            .ToListAsync();

        return View(myApplications);
    }

    public IActionResult Create()
    {
        return View(new AccessPassCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AccessPassCreateViewModel model, string formAction)
    {
        var isDraft = string.Equals(formAction, "draft", StringComparison.OrdinalIgnoreCase);

        if (isDraft)
        {
            // Draf dibenarkan tidak lengkap — buang semua ralat DataAnnotations/IValidatableObject.
            ModelState.Clear();
        }
        else if (!ModelState.IsValid)
        {
            return View(model);
        }

        var userId = _currentUserService.UserId;

        if (!isDraft)
        {
            var hasActiveApplication = await _db.AccessPassApplications
                .AnyAsync(x =>
                    x.Submission.ApplicantUserId == userId &&
                    x.Submission.Status != SubmissionStatus.Rejected &&
                    x.Submission.Status != SubmissionStatus.Cancelled &&
                    x.Submission.Status != SubmissionStatus.Completed);

            if (hasActiveApplication)
            {
                ModelState.AddModelError(string.Empty,
                    "Anda sudah mempunyai permohonan pas keselamatan yang aktif. Selesaikan atau batalkan permohonan sedia ada dahulu.");
                return View(model);
            }
        }

        var submission = new Submission
        {
            ModuleCode = ModuleCode,
            ApplicantUserId = userId,
            Status = SubmissionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var application = new AccessPassApplication
        {
            Submission = submission,
            PassType = model.PassType,
            ReplacementReason = model.ReplacementReason,
            AccessAreaRequested = model.AccessAreaRequested,
            ValidFrom = model.ValidFrom,
            ValidTo = model.ValidTo
        };

        _db.AccessPassApplications.Add(application);

        if (isDraft)
        {
            await _db.SaveChangesAsync();
            await _auditLogService.RecordAsync(submission.Id, "SaveDraft");
            TempData["Message"] = "Draf permohonan pas keselamatan disimpan.";
        }
        else
        {
            submission.ReferenceNo = await _referenceNumberService.GenerateAsync(ModuleCode);
            submission.Status = SubmissionStatus.Submitted;
            submission.SubmittedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _auditLogService.RecordAsync(submission.Id, "Submit");
            TempData["Message"] = $"Permohonan pas keselamatan dihantar. Nombor rujukan: {submission.ReferenceNo}";
        }

        return RedirectToAction(nameof(Index));
    }
}
```

**Perhatikan corak "dua butang, satu action":** borang Razor (2.3) menghantar `formAction` bernilai `"draft"` atau `"submit"` bergantung butang mana ditekan — satu kaedah `Create(POST)` sahaja mengendalikan kedua-dua kes, ganti pengulangan kod.

### 2.3 — View `Create.cshtml`

Cipta `Views/AccessPass/Create.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.AccessPassCreateViewModel
@{
    ViewData["Title"] = "Permohonan Pas Keselamatan";
}

<h1>@ViewData["Title"]</h1>

<form asp-action="Create" method="post">
    <div asp-validation-summary="All" class="text-danger"></div>

    <div class="mb-3">
        <label asp-for="PassType" class="form-label"></label>
        <select asp-for="PassType" asp-items="Html.GetEnumSelectList<Nres.Onboarding.Web.Models.SecurityPassType>()" class="form-select"></select>
    </div>

    <div class="mb-3">
        <label asp-for="ReplacementReason" class="form-label"></label>
        <textarea asp-for="ReplacementReason" class="form-control" rows="2"
                  placeholder="Wajib diisi hanya untuk pas ganti (hilang/rosak)"></textarea>
        <span asp-validation-for="ReplacementReason" class="text-danger"></span>
    </div>

    <div class="mb-3">
        <label asp-for="AccessAreaRequested" class="form-label"></label>
        <input asp-for="AccessAreaRequested" class="form-control" placeholder="cth. Blok A, Bilik Server" />
        <span asp-validation-for="AccessAreaRequested" class="text-danger"></span>
    </div>

    <div class="row">
        <div class="col-md-6 mb-3">
            <label asp-for="ValidFrom" class="form-label"></label>
            <input asp-for="ValidFrom" class="form-control" />
            <span asp-validation-for="ValidFrom" class="text-danger"></span>
        </div>
        <div class="col-md-6 mb-3">
            <label asp-for="ValidTo" class="form-label"></label>
            <input asp-for="ValidTo" class="form-control" />
            <span asp-validation-for="ValidTo" class="text-danger"></span>
        </div>
    </div>

    <button type="submit" name="formAction" value="draft" class="btn btn-secondary">Simpan Draf</button>
    <button type="submit" name="formAction" value="submit" class="btn btn-primary">Hantar Permohonan</button>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

### 2.4 — View `Index.cshtml` (senarai ringkas)

Cipta `Views/AccessPass/Index.cshtml`:

```cshtml
@model List<Nres.Onboarding.Web.Models.AccessPassApplication>
@{
    ViewData["Title"] = "Permohonan Pas Keselamatan Saya";
}

<h1>@ViewData["Title"]</h1>

@if (TempData["Message"] is string message)
{
    <div class="alert alert-info">@message</div>
}

<a asp-action="Create" class="btn btn-primary mb-3">+ Permohonan Baharu</a>

<table class="table">
    <thead>
        <tr>
            <th>No. Rujukan</th>
            <th>Jenis</th>
            <th>Status</th>
            <th>Dihantar</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var item in Model)
        {
            <tr>
                <td>@(string.IsNullOrEmpty(item.Submission.ReferenceNo) ? "(Draf)" : item.Submission.ReferenceNo)</td>
                <td>@item.PassType</td>
                <td>@item.Submission.Status</td>
                <td>@(item.Submission.SubmittedAt?.ToString("dd/MM/yyyy") ?? "-")</td>
            </tr>
        }
    </tbody>
</table>
```

✅ **Semakan:** Cuba hantar borang dengan `PassType = Replacement` dan `ReplacementReason` kosong — validation summary mesti papar ralat. Hantar dua permohonan pas berturutan tanpa selesaikan yang pertama — permohonan kedua mesti disekat dengan mesej pendua.

---

## Latihan 3 — Borang Pelekat Kenderaan (`VehicleStickerController`)

**Objektif:** Bina borang pelekat kenderaan menggunakan `VehicleInputModel` (Latihan 1), dengan semakan pendua **per kenderaan** (bukan per pemohon).

### 3.1 — View model

Cipta `ViewModels/VehicleStickerCreateViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Nres.Onboarding.Web.ViewModels;

public class VehicleStickerCreateViewModel : IValidatableObject
{
    public VehicleInputModel Vehicle { get; set; } = new();

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        return Vehicle.ValidateNewVehicleFields(nameof(Vehicle));
    }
}
```

### 3.2 — Controller penuh

Gantikan **seluruh kandungan** `Controllers/VehicleStickerController.cs` dengan:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels;

namespace Nres.Onboarding.Web.Controllers;

[Authorize(Roles = "Applicant")]
public class VehicleStickerController : Controller
{
    private const string ModuleCode = "STK";

    private readonly ApplicationDbContext _db;
    private readonly IReferenceNumberService _referenceNumberService;
    private readonly IAuditLogService _auditLogService;
    private readonly ICurrentUserService _currentUserService;

    public VehicleStickerController(
        ApplicationDbContext db,
        IReferenceNumberService referenceNumberService,
        IAuditLogService auditLogService,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _referenceNumberService = referenceNumberService;
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _currentUserService.UserId;

        var myApplications = await _db.VehicleStickerApplications
            .Include(a => a.Submission)
            .Include(a => a.Vehicle)
            .Where(a => a.Submission.ApplicantUserId == userId)
            .OrderByDescending(a => a.Submission.CreatedAt)
            .ToListAsync();

        return View(myApplications);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateMyVehiclesAsync();
        return View(new VehicleStickerCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(VehicleStickerCreateViewModel model, string formAction)
    {
        var isDraft = string.Equals(formAction, "draft", StringComparison.OrdinalIgnoreCase);
        var userId = _currentUserService.UserId;

        if (isDraft)
        {
            ModelState.Clear();
        }
        else if (!ModelState.IsValid)
        {
            await PopulateMyVehiclesAsync();
            return View(model);
        }

        // Selesaikan kenderaan: sedia ada, atau daftar baharu.
        Vehicle vehicle;
        if (model.Vehicle.ExistingVehicleId > 0)
        {
            var existing = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Id == model.Vehicle.ExistingVehicleId && v.ApplicantUserId == userId);

            if (existing is null)
            {
                ModelState.AddModelError(string.Empty, "Kenderaan yang dipilih tidak sah.");
                await PopulateMyVehiclesAsync();
                return View(model);
            }

            vehicle = existing;
        }
        else if (!isDraft)
        {
            vehicle = new Vehicle
            {
                ApplicantUserId = userId,
                RegistrationNo = model.Vehicle.RegistrationNo!.ToUpperInvariant(),
                Type = model.Vehicle.Type!.Value,
                MakeModel = model.Vehicle.MakeModel!,
                Color = model.Vehicle.Color ?? string.Empty,
                OwnerName = model.Vehicle.OwnerName!,
                OwnerRelationship = model.Vehicle.OwnerRelationship!.Value
            };
            _db.Vehicles.Add(vehicle);
        }
        else
        {
            // Draf tanpa kenderaan langsung dipilih/diisi — belum boleh cipta rekod anak, hanya maklumkan pengguna.
            TempData["Message"] = "Draf tidak dapat disimpan tanpa maklumat kenderaan asas. Isi sekurang-kurangnya nombor pendaftaran.";
            await PopulateMyVehiclesAsync();
            return View(model);
        }

        if (!isDraft)
        {
            var hasActiveSticker = await _db.VehicleStickerApplications
                .AnyAsync(x =>
                    x.VehicleId == vehicle.Id &&
                    x.Submission.Status != SubmissionStatus.Rejected &&
                    x.Submission.Status != SubmissionStatus.Cancelled &&
                    x.Submission.Status != SubmissionStatus.Completed);

            if (hasActiveSticker)
            {
                ModelState.AddModelError(string.Empty,
                    "Kenderaan ini sudah mempunyai permohonan pelekat yang aktif.");
                await PopulateMyVehiclesAsync();
                return View(model);
            }
        }

        var submission = new Submission
        {
            ModuleCode = ModuleCode,
            ApplicantUserId = userId,
            Status = SubmissionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var application = new VehicleStickerApplication
        {
            Submission = submission,
            Vehicle = vehicle
        };

        _db.VehicleStickerApplications.Add(application);

        if (isDraft)
        {
            await _db.SaveChangesAsync();
            await _auditLogService.RecordAsync(submission.Id, "SaveDraft");
            TempData["Message"] = "Draf permohonan pelekat kenderaan disimpan.";
        }
        else
        {
            submission.ReferenceNo = await _referenceNumberService.GenerateAsync(ModuleCode);
            submission.Status = SubmissionStatus.Submitted;
            submission.SubmittedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _auditLogService.RecordAsync(submission.Id, "Submit");
            TempData["Message"] = $"Permohonan pelekat kenderaan dihantar. Nombor rujukan: {submission.ReferenceNo}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateMyVehiclesAsync()
    {
        var userId = _currentUserService.UserId;
        ViewBag.MyVehicles = await _db.Vehicles
            .Where(v => v.ApplicantUserId == userId)
            .OrderBy(v => v.RegistrationNo)
            .ToListAsync();
    }
}
```

### 3.3 — View `Create.cshtml`

Cipta `Views/VehicleSticker/Create.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.VehicleStickerCreateViewModel
@{
    ViewData["Title"] = "Permohonan Pelekat Kenderaan";
    var myVehicles = ViewBag.MyVehicles as List<Nres.Onboarding.Web.Models.Vehicle> ?? new();
}

<h1>@ViewData["Title"]</h1>

<form asp-action="Create" method="post">
    <div asp-validation-summary="All" class="text-danger"></div>

    <div class="mb-3">
        <label class="form-label">Kenderaan Sedia Ada</label>
        <select asp-for="Vehicle.ExistingVehicleId" class="form-select">
            <option value="0">— Daftar kenderaan baharu —</option>
            @foreach (var v in myVehicles)
            {
                <option value="@v.Id">@v.RegistrationNo (@v.MakeModel)</option>
            }
        </select>
        <div class="form-text">Pilih kenderaan sedia ada, ATAU biarkan "Daftar kenderaan baharu" dan isi medan di bawah.</div>
    </div>

    <fieldset class="border p-3 mb-3">
        <legend class="fs-6">Kenderaan Baharu (kosongkan jika memilih kenderaan sedia ada di atas)</legend>

        <div class="mb-3">
            <label asp-for="Vehicle.RegistrationNo" class="form-label"></label>
            <input asp-for="Vehicle.RegistrationNo" class="form-control" placeholder="cth. WXX1234" />
            <span asp-validation-for="Vehicle.RegistrationNo" class="text-danger"></span>
        </div>

        <div class="mb-3">
            <label asp-for="Vehicle.Type" class="form-label"></label>
            <select asp-for="Vehicle.Type" asp-items="Html.GetEnumSelectList<Nres.Onboarding.Web.Models.VehicleType>()" class="form-select">
                <option value="">-- Pilih --</option>
            </select>
            <span asp-validation-for="Vehicle.Type" class="text-danger"></span>
        </div>

        <div class="mb-3">
            <label asp-for="Vehicle.MakeModel" class="form-label"></label>
            <input asp-for="Vehicle.MakeModel" class="form-control" />
            <span asp-validation-for="Vehicle.MakeModel" class="text-danger"></span>
        </div>

        <div class="mb-3">
            <label asp-for="Vehicle.Color" class="form-label"></label>
            <input asp-for="Vehicle.Color" class="form-control" />
        </div>

        <div class="mb-3">
            <label asp-for="Vehicle.OwnerName" class="form-label"></label>
            <input asp-for="Vehicle.OwnerName" class="form-control" />
            <span asp-validation-for="Vehicle.OwnerName" class="text-danger"></span>
        </div>

        <div class="mb-3">
            <label asp-for="Vehicle.OwnerRelationship" class="form-label"></label>
            <select asp-for="Vehicle.OwnerRelationship" asp-items="Html.GetEnumSelectList<Nres.Onboarding.Web.Models.OwnerRelationship>()" class="form-select">
                <option value="">-- Pilih --</option>
            </select>
            <span asp-validation-for="Vehicle.OwnerRelationship" class="text-danger"></span>
        </div>
    </fieldset>

    <button type="submit" name="formAction" value="draft" class="btn btn-secondary">Simpan Draf</button>
    <button type="submit" name="formAction" value="submit" class="btn btn-primary">Hantar Permohonan</button>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

Cipta juga `Views/VehicleSticker/Index.cshtml` mengikut corak yang sama seperti `Views/AccessPass/Index.cshtml` (Latihan 2.4), tambah lajur `@item.Vehicle.RegistrationNo`.

✅ **Semakan:** Daftar kenderaan baharu dan hantar — sahkan rekod `Vehicles` baharu dicipta. Hantar **permohonan kedua** untuk **kenderaan yang sama** — mesti disekat dengan mesej pendua. Pilih kenderaan sedia ada dari dropdown untuk permohonan seterusnya (kenderaan lain) — mesti berjaya tanpa perlu isi semula medan kenderaan.

---

## Latihan 4 — Borang Parkir (`ParkingController`)

**Objektif:** Bina borang parkir dengan conditional validation "parkir khas perlu justifikasi" — **tiada** semakan pendua untuk parkir (rujuk jadual peraturan dalam README).

### 4.1 — View model

Cipta `ViewModels/ParkingCreateViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models;

namespace Nres.Onboarding.Web.ViewModels;

public class ParkingCreateViewModel : IValidatableObject
{
    public VehicleInputModel Vehicle { get; set; } = new();

    [Required]
    [Display(Name = "Jenis Parkir")]
    public ParkingType ParkingType { get; set; } = ParkingType.Biasa;

    [StringLength(500)]
    [Display(Name = "Justifikasi")]
    public string? Justification { get; set; }

    [Required(ErrorMessage = "Zon parkir yang dipohon wajib diisi.")]
    [StringLength(100)]
    [Display(Name = "Zon Parkir Dipohon")]
    public string ParkingZoneRequested { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        foreach (var result in Vehicle.ValidateNewVehicleFields(nameof(Vehicle)))
        {
            yield return result;
        }

        if (ParkingType == ParkingType.Khas && string.IsNullOrWhiteSpace(Justification))
        {
            yield return new ValidationResult(
                "Justifikasi wajib diisi untuk permohonan parkir khas.",
                new[] { nameof(Justification) });
        }
    }
}
```

**Perhatikan:** `Validate()` menggabungkan **dua** sumber ralat — ralat kenderaan (delegasi ke `VehicleInputModel`) **dan** ralat khusus parkir (`Justification`). Ini corak yang sama seperti Latihan 3, ditambah satu peraturan lagi — tunjuk bagaimana `IValidatableObject` **berskala** apabila borang makin kompleks.

### 4.2 — Controller penuh

Gantikan **seluruh kandungan** `Controllers/ParkingController.cs` dengan (struktur serupa Latihan 3, **tanpa** blok `AnyAsync` pendua):

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels;

namespace Nres.Onboarding.Web.Controllers;

[Authorize(Roles = "Applicant")]
public class ParkingController : Controller
{
    private const string ModuleCode = "PKR";

    private readonly ApplicationDbContext _db;
    private readonly IReferenceNumberService _referenceNumberService;
    private readonly IAuditLogService _auditLogService;
    private readonly ICurrentUserService _currentUserService;

    public ParkingController(
        ApplicationDbContext db,
        IReferenceNumberService referenceNumberService,
        IAuditLogService auditLogService,
        ICurrentUserService currentUserService)
    {
        _db = db;
        _referenceNumberService = referenceNumberService;
        _auditLogService = auditLogService;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _currentUserService.UserId;

        var myApplications = await _db.ParkingApplications
            .Include(a => a.Submission)
            .Include(a => a.Vehicle)
            .Where(a => a.Submission.ApplicantUserId == userId)
            .OrderByDescending(a => a.Submission.CreatedAt)
            .ToListAsync();

        return View(myApplications);
    }

    public async Task<IActionResult> Create()
    {
        await PopulateMyVehiclesAsync();
        return View(new ParkingCreateViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ParkingCreateViewModel model, string formAction)
    {
        var isDraft = string.Equals(formAction, "draft", StringComparison.OrdinalIgnoreCase);
        var userId = _currentUserService.UserId;

        if (isDraft)
        {
            ModelState.Clear();
        }
        else if (!ModelState.IsValid)
        {
            await PopulateMyVehiclesAsync();
            return View(model);
        }

        Vehicle vehicle;
        if (model.Vehicle.ExistingVehicleId > 0)
        {
            var existing = await _db.Vehicles
                .FirstOrDefaultAsync(v => v.Id == model.Vehicle.ExistingVehicleId && v.ApplicantUserId == userId);

            if (existing is null)
            {
                ModelState.AddModelError(string.Empty, "Kenderaan yang dipilih tidak sah.");
                await PopulateMyVehiclesAsync();
                return View(model);
            }

            vehicle = existing;
        }
        else if (!isDraft)
        {
            vehicle = new Vehicle
            {
                ApplicantUserId = userId,
                RegistrationNo = model.Vehicle.RegistrationNo!.ToUpperInvariant(),
                Type = model.Vehicle.Type!.Value,
                MakeModel = model.Vehicle.MakeModel!,
                Color = model.Vehicle.Color ?? string.Empty,
                OwnerName = model.Vehicle.OwnerName!,
                OwnerRelationship = model.Vehicle.OwnerRelationship!.Value
            };
            _db.Vehicles.Add(vehicle);
        }
        else
        {
            TempData["Message"] = "Draf tidak dapat disimpan tanpa maklumat kenderaan asas.";
            await PopulateMyVehiclesAsync();
            return View(model);
        }

        var submission = new Submission
        {
            ModuleCode = ModuleCode,
            ApplicantUserId = userId,
            Status = SubmissionStatus.Draft,
            CreatedAt = DateTime.UtcNow
        };

        var application = new ParkingApplication
        {
            Submission = submission,
            Vehicle = vehicle,
            ParkingType = model.ParkingType,
            Justification = model.Justification,
            ParkingZoneRequested = model.ParkingZoneRequested
        };

        _db.ParkingApplications.Add(application);

        if (isDraft)
        {
            await _db.SaveChangesAsync();
            await _auditLogService.RecordAsync(submission.Id, "SaveDraft");
            TempData["Message"] = "Draf permohonan parkir disimpan.";
        }
        else
        {
            submission.ReferenceNo = await _referenceNumberService.GenerateAsync(ModuleCode);
            submission.Status = SubmissionStatus.Submitted;
            submission.SubmittedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            await _auditLogService.RecordAsync(submission.Id, "Submit");
            TempData["Message"] = $"Permohonan parkir dihantar. Nombor rujukan: {submission.ReferenceNo}";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task PopulateMyVehiclesAsync()
    {
        var userId = _currentUserService.UserId;
        ViewBag.MyVehicles = await _db.Vehicles
            .Where(v => v.ApplicantUserId == userId)
            .OrderBy(v => v.RegistrationNo)
            .ToListAsync();
    }
}
```

### 4.3 — Views

Cipta `Views/Parking/Create.cshtml` dan `Views/Parking/Index.cshtml` mengikut corak **sama** seperti Latihan 3.3 (`VehicleSticker/Create.cshtml`), dengan **tambahan** dua medan di bahagian atas borang (sebelum fieldset kenderaan):

```cshtml
<div class="mb-3">
    <label asp-for="ParkingType" class="form-label"></label>
    <select asp-for="ParkingType" asp-items="Html.GetEnumSelectList<Nres.Onboarding.Web.Models.ParkingType>()" class="form-select"></select>
</div>

<div class="mb-3">
    <label asp-for="Justification" class="form-label"></label>
    <textarea asp-for="Justification" class="form-control" rows="2"
              placeholder="Wajib diisi hanya untuk parkir khas"></textarea>
    <span asp-validation-for="Justification" class="text-danger"></span>
</div>

<div class="mb-3">
    <label asp-for="ParkingZoneRequested" class="form-label"></label>
    <input asp-for="ParkingZoneRequested" class="form-control" placeholder="cth. Zon C, Tingkat Bawah Tanah" />
    <span asp-validation-for="ParkingZoneRequested" class="text-danger"></span>
</div>
```

✅ **Semakan:** Pilih `ParkingType = Khas` dengan `Justification` kosong — validation summary mesti papar ralat. Pilih `ParkingType = Biasa` tanpa justifikasi — mesti berjaya dihantar. Hantar **dua** permohonan parkir khas untuk kenderaan yang sama — **kedua-duanya mesti berjaya** (tiada peraturan pendua untuk parkir).

---

## Latihan 5 — Skrip Pengesahan Manual Peraturan Perniagaan

**Objektif:** Sahkan **kesemua** peraturan perniagaan Hari 5 berfungsi bersama, sebagai satu senarai semak akhir hari.

Jalankan `dotnet run`, log masuk sebagai pengguna berperanan `Applicant`, dan ikut skrip ini:

| # | Tindakan | Jangkaan |
|---|----------|----------|
| 1 | Mohon pas keselamatan baharu (`PassType = New`), hantar | Berjaya, `ReferenceNo` bermula `PAS-` |
| 2 | Mohon pas keselamatan kedua sebelum yang pertama selesai | **Disekat** — mesej pendua |
| 3 | Mohon pas ganti (`PassType = Replacement`) tanpa `ReplacementReason` | **Disekat** — validation summary |
| 4 | Daftar kenderaan baharu, mohon pelekat, hantar | Berjaya, `ReferenceNo` bermula `STK-` |
| 5 | Mohon pelekat kedua untuk **kenderaan yang sama** | **Disekat** — mesej pendua |
| 6 | Mohon pelekat untuk kenderaan **lain** (pilih dari dropdown/daftar baharu) | Berjaya |
| 7 | Mohon parkir khas (`ParkingType = Khas`) tanpa `Justification` | **Disekat** — validation summary |
| 8 | Mohon parkir khas **dengan** `Justification` diisi, hantar | Berjaya, `ReferenceNo` bermula `PKR-` |
| 9 | Simpan draf pas keselamatan dengan medan kosong (klik "Simpan Draf") | Berjaya walaupun tidak lengkap — status `Draft`, tiada `ReferenceNo` |

✅ **Semakan akhir Hari 5:** Kesemua 9 baris skrip di atas berfungsi seperti dijangka. Semua tiga borang menyokong Simpan Draf & Hantar. Semua peraturan pendua & conditional validation berfungsi.

**Seterusnya:** [Hari 6](../../hari-6/) — bina sisi admin (senarai, filter, kelulusan, cetakan) untuk permohonan yang sudah dihantar ini.
