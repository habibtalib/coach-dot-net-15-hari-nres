# Lab · Kumpulan 1 · Hari 4 — Skema & Borang Draf

> Konsep: [`../README.md`](../README.md) · Kanun: [`../../../SPEC-KURSUS.md`](../../../SPEC-KURSUS.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md)

## Latihan 0 — Mula hari dengan betul

**Objektif:** Segerak, semak, dan sahkan sebelum menaip.

### Langkah

1. Segerak dengan `master`:

```bash
git switch kump-1/lapor-diri
git pull --rebase origin master
dotnet build
```

2. **Semakan "sudah wujud?"** — sebelum menulis apa-apa hari ini:

```bash
grep -ri "OfficerReporting" Nres.Onboarding.Web/
grep -ri "ReferenceNumber"  Nres.Onboarding.Web/Services/
```

Anda sepatutnya menemui `IReferenceNumberService` **sudah wujud**. Anda akan menggunakannya pada Hari 5–6 — jangan tulis satu lagi.

3. Cipta cabang ciri:

```bash
git switch -c kump-1/feat/skema-dan-borang-draf
```

### ✅ Semakan

- [ ] `dotnet build` berjaya pada cabang kumpulan anda
- [ ] Anda mengesahkan `IReferenceNumberService` sudah wujud
- [ ] Anda berada pada cabang ciri, bukan terus pada cabang kumpulan

---

## Latihan 1 — Entiti `OfficerReportingApplication`

**Objektif:** Jadual detail yang memaut ke `Submission` induk tanpa menduplikasi medannya.

### Langkah

1. `Models/LaporDiri/OfficerReportingApplication.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.LaporDiri;

/// <summary>
/// Jadual DETAIL bagi permohonan lapor diri. Nombor rujukan, status, pemohon,
/// dan tarikh hantar tinggal dalam Submission induk — JANGAN pendua di sini.
/// </summary>
public class OfficerReportingApplication
{
    public int Id { get; set; }

    /// <summary>Kunci asing ke Submission induk. Unik — satu-ke-satu.</summary>
    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    // --- Maklumat peribadi ---
    public string FullName { get; set; } = string.Empty;
    public string IdentityNo { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Phone { get; set; } = string.Empty;
    public DateTime? DateOfBirth { get; set; }

    // --- Maklumat perkhidmatan ---
    /// <summary>Tarikh melapor diri di NRES.</summary>
    public DateTime? ReportingDate { get; set; }

    public int? DepartmentId { get; set; }
    public LookupDepartment? Department { get; set; }

    public int? PositionId { get; set; }
    public LookupPosition? Position { get; set; }

    public int? GradeId { get; set; }
    public LookupGrade? Grade { get; set; }

    /// <summary>Agensi sebelum ini — kosong jika lantikan baharu.</summary>
    public string? PreviousAgency { get; set; }

    // --- Kecemasan ---
    public string? EmergencyContactName { get; set; }
    public string? EmergencyContactPhone { get; set; }

    // --- Akuan ---
    /// <summary>Pemohon mengesahkan maklumat benar. Wajib untuk HANTAR, bukan draf.</summary>
    public bool DeclarationAccepted { get; set; }
}
```

2. Perhatikan apa yang **tiada**: `ReferenceNo`, `Status`, `ApplicantUserId`, `SubmittedAt`. Semak sendiri — jika anda tergoda menambahnya, baca semula [`../README.md`](../README.md).

### ✅ Semakan

- [ ] Fail dalam `Models/LaporDiri/`, bukan `Models/Shared/`
- [ ] Namespace `Nres.Onboarding.Web.Models.LaporDiri`
- [ ] **Sifar** medan diduplikasi dari `Submission`
- [ ] `dotnet build` berjaya

---

## Latihan 2 — Konfigurasi EF Core (corak anti-konflik)

**Objektif:** Daftar entiti anda dengan EF Core **tanpa menyentuh `ApplicationDbContext`**.

### Langkah

1. `Models/LaporDiri/Configurations/OfficerReportingApplicationConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.LaporDiri.Configurations;

public class OfficerReportingApplicationConfiguration
    : IEntityTypeConfiguration<OfficerReportingApplication>
{
    public void Configure(EntityTypeBuilder<OfficerReportingApplication> builder)
    {
        builder.ToTable("OfficerReportingApplications");

        builder.Property(a => a.FullName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.IdentityNo).HasMaxLength(20).IsRequired();
        builder.Property(a => a.Email).HasMaxLength(200).IsRequired();
        builder.Property(a => a.Phone).HasMaxLength(30).IsRequired();
        builder.Property(a => a.PreviousAgency).HasMaxLength(200);
        builder.Property(a => a.EmergencyContactName).HasMaxLength(200);
        builder.Property(a => a.EmergencyContactPhone).HasMaxLength(30);

        // Satu-ke-satu dengan Submission induk, dikuatkuasakan indeks unik.
        builder.HasIndex(a => a.SubmissionId).IsUnique();

        builder.HasOne(a => a.Submission)
            .WithOne()
            .HasForeignKey<OfficerReportingApplication>(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Lookup ialah data rujukan — sekat pemadaman semasa masih dirujuk.
        builder.HasOne(a => a.Department).WithMany()
            .HasForeignKey(a => a.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Position).WithMany()
            .HasForeignKey(a => a.PositionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Grade).WithMany()
            .HasForeignKey(a => a.GradeId).OnDelete(DeleteBehavior.Restrict);
    }
}
```

2. **Sahkan anda tidak menyentuh `ApplicationDbContext`:**

```bash
git diff --name-only master
```

Senarai itu **tidak** sepatutnya mengandungi `Data/ApplicationDbContext.cs`. `ApplyConfigurationsFromAssembly()` menemui kelas anda secara automatik.

### ✅ Semakan

- [ ] Fail konfigurasi dalam `Models/LaporDiri/Configurations/`
- [ ] `git diff --name-only master` menunjukkan **tiada** fail kongsi
- [ ] `dotnet build` berjaya

---

## Latihan 3 — Pendaftaran modul & navigasi

**Objektif:** Sambungkan modul anda ke aplikasi dengan menambah fail, bukan menyunting fail.

### Langkah

1. `Services/LaporDiri/LaporDiriModule.cs`:

```csharp
namespace Nres.Onboarding.Web.Services.LaporDiri;

/// <summary>
/// Pendaftaran servis Kumpulan 1. Program.cs memanggil AddLaporDiriModule()
/// dan tidak pernah perlu berubah lagi — kami menambah servis DI SINI.
/// </summary>
public static class LaporDiriModule
{
    public static IServiceCollection AddLaporDiriModule(this IServiceCollection services)
    {
        // Servis modul ditambah di sini apabila kami menciptanya (Hari 5–6 dan seterusnya).
        return services;
    }
}
```

2. `Models/LaporDiri/LaporDiriModuleDescriptor.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.LaporDiri;

/// <summary>
/// Menjadikan modul muncul dalam navigasi untuk peranan yang betul.
/// Dikumpul secara automatik oleh ModuleNavViewComponent — tiada suntingan
/// pada _Layout.cshtml.
/// </summary>
public class LaporDiriModuleDescriptor : IModuleDescriptorProvider
{
    public ModuleDescriptor Describe() => new(
        Code: ModuleCodes.LaporDiri,
        Nama: "Lapor Diri",
        Controller: "OfficerReporting",
        Ikon: "bi-person-plus",
        Roles: ["Applicant", "HrAdmin", "SystemAdmin"],
        Urutan: 1);
}
```

3. Daftarkan descriptor dalam modul anda:

```csharp
// Services/LaporDiri/LaporDiriModule.cs — kemas kini
using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.LaporDiri;

public static class LaporDiriModule
{
    public static IServiceCollection AddLaporDiriModule(this IServiceCollection services)
    {
        services.AddScoped<IModuleDescriptorProvider, LaporDiriModuleDescriptor>();
        return services;
    }
}
```

4. **Satu-satunya suntingan fail kongsi hari ini.** Beritahu jurulatih, kemudian nyahkomen **satu baris** dalam `Program.cs`:

```csharp
using Nres.Onboarding.Web.Services.LaporDiri;   // ← tambah using

// ...
builder.Services.AddLaporDiriModule();   // Kumpulan 1   ← nyahkomen baris INI sahaja
// builder.Services.AddAksesModule();       // Kumpulan 2
// builder.Services.AddAkaunModule();       // Kumpulan 3
// builder.Services.AddAsetModule();        // Kumpulan 4
```

> ⚠️ **Nyahkomen baris ANDA sahaja.** Jika anda menyahkomen baris kumpulan lain, binaan gagal untuk semua orang kerana kaedah mereka belum wujud.

### ✅ Semakan

- [ ] `LaporDiriModule.cs` dan `LaporDiriModuleDescriptor.cs` wujud dalam folder anda
- [ ] Tepat **satu** baris dinyahkomen dalam `Program.cs`
- [ ] `dotnet build` berjaya

---

## Latihan 4 — Migration (slot!)

**Objektif:** Cipta jadual anda dalam pangkalan data.

### Langkah

1. **Umumkan slot migration:** *"Kumpulan 1 mengambil slot migration."* Tunggu pengesahan jurulatih.

2. Segerak dahulu — sentiasa:

```bash
git pull --rebase origin master
```

3. Jana:

```bash
cd Nres.Onboarding.Web
dotnet ef migrations add LaporDiriApplication
```

4. **Baca fail yang dijana.** Sahkan ia mencipta `OfficerReportingApplications` dengan indeks unik pada `SubmissionId` — dan **tiada apa-apa lagi**. Jika ia menyentuh jadual kumpulan lain, anda tidak menyegerak dengan betul.

5. Guna pakai dan uji:

```bash
dotnet ef database update
dotnet run
```

6. Commit, push, dan **lepaskan slot**: *"Kumpulan 1 selesai slot migration."*

```bash
cd ..
git add .
git commit -m "lapor-diri: entiti, konfigurasi, pendaftaran modul dan migration"
git push -u origin kump-1/feat/skema-dan-borang-draf
```

### Jika snapshot berkonflik

Jangan baiki dengan tangan:

```bash
git checkout --theirs Migrations/ApplicationDbContextModelSnapshot.cs
rm Migrations/*_LaporDiriApplication.cs Migrations/*_LaporDiriApplication.Designer.cs
git pull --rebase origin master
dotnet ef migrations add LaporDiriApplication
dotnet ef database update
```

### ✅ Semakan

- [ ] Slot diumumkan sebelum menjana
- [ ] Migration hanya menyentuh jadual **anda**
- [ ] `dotnet ef database update` berjaya
- [ ] Aplikasi bermula; "Lapor Diri" muncul dalam navigasi
- [ ] Slot dilepaskan

---

## Latihan 5 — View model dengan dua kumpulan validation

**Objektif:** Satu view model yang membenarkan draf tidak lengkap tetapi menguatkuasakan penghantaran lengkap.

### Langkah

1. `ViewModels/LaporDiri/OfficerReportingFormViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace Nres.Onboarding.Web.ViewModels.LaporDiri;

/// <summary>
/// Borang mengikat kelas INI, bukan entiti — supaya penyerang tidak boleh
/// menghantar Status=AdminApproved bersama borang (over-posting).
///
/// Validation dua peringkat: [Required] terpakai pada HANTAR sahaja.
/// Simpan draf memintasnya melalui ModelState.Clear() dalam controller —
/// lihat Latihan 6.
/// </summary>
public class OfficerReportingFormViewModel
{
    public int? Id { get; set; }
    public int? SubmissionId { get; set; }

    [Display(Name = "Nama penuh")]
    [Required(ErrorMessage = "Nama penuh wajib diisi.")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "No. kad pengenalan")]
    [Required(ErrorMessage = "No. kad pengenalan wajib diisi.")]
    [RegularExpression(@"^\d{6}-\d{2}-\d{4}$",
        ErrorMessage = "Format: 010203-14-5678")]
    public string IdentityNo { get; set; } = string.Empty;

    [Display(Name = "E-mel")]
    [Required(ErrorMessage = "E-mel wajib diisi.")]
    [EmailAddress(ErrorMessage = "Format e-mel tidak sah.")]
    [StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Display(Name = "No. telefon")]
    [Required(ErrorMessage = "No. telefon wajib diisi.")]
    [StringLength(30)]
    public string Phone { get; set; } = string.Empty;

    [Display(Name = "Tarikh lahir")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Tarikh lapor diri")]
    [Required(ErrorMessage = "Tarikh lapor diri wajib diisi.")]
    [DataType(DataType.Date)]
    public DateTime? ReportingDate { get; set; }

    [Display(Name = "Bahagian")]
    [Required(ErrorMessage = "Sila pilih bahagian.")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Jawatan")]
    [Required(ErrorMessage = "Sila pilih jawatan.")]
    public int? PositionId { get; set; }

    [Display(Name = "Gred")]
    [Required(ErrorMessage = "Sila pilih gred.")]
    public int? GradeId { get; set; }

    [Display(Name = "Agensi sebelum ini")]
    [StringLength(200)]
    public string? PreviousAgency { get; set; }

    [Display(Name = "Nama waris kecemasan")]
    [StringLength(200)]
    public string? EmergencyContactName { get; set; }

    [Display(Name = "Telefon waris kecemasan")]
    [StringLength(30)]
    public string? EmergencyContactPhone { get; set; }

    [Display(Name = "Saya mengesahkan maklumat di atas adalah benar")]
    public bool DeclarationAccepted { get; set; }

    // --- Data sokongan untuk dropdown (bukan input pengguna) ---
    public IEnumerable<SelectListItem> Departments { get; set; } = [];
    public IEnumerable<SelectListItem> Positions { get; set; } = [];
    public IEnumerable<SelectListItem> Grades { get; set; } = [];

    /// <summary>Draf boleh disunting; selepas dihantar, borang dikunci.</summary>
    public bool IsEditable { get; set; } = true;
}
```

### ✅ Semakan

- [ ] View model dalam `ViewModels/LaporDiri/`
- [ ] Medan wajib mempunyai `[Required]` dengan mesej Bahasa Melayu
- [ ] Tiada sifat `Status` atau `ReferenceNo` — itu milik `Submission`
- [ ] `dotnet build` berjaya

---

## Latihan 6 — Controller: cipta, sunting, simpan draf

**Objektif:** Aliran draf yang berfungsi, mewarisi kelas asas kongsi.

### Langkah

1. `Controllers/OfficerReportingController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels.LaporDiri;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class OfficerReportingController(
    ApplicationDbContext db,
    IWorkflowService workflow,
    INotificationService notifications,
    ICurrentUserService currentUser)
    : SubmissionControllerBase(db, workflow, notifications)
{
    // Kelas asas menggunakan kedua-dua ini untuk Approve/Reject —
    // kami TIDAK menulis semula logik kelulusan.
    protected override string ModuleCode => ModuleCodes.LaporDiri;
    protected override string AdminRole => "HrAdmin";

    /// <summary>Senarai permohonan pemohon semasa.</summary>
    public async Task<IActionResult> Index()
    {
        var userId = currentUser.UserId!;

        var senarai = await Db.Submissions
            .Where(s => s.ModuleCode == ModuleCode && s.ApplicantUserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .ToListAsync();

        return View(senarai);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var vm = new OfficerReportingFormViewModel();
        await IsiDropdownAsync(vm);
        return View("Form", vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var app = await Db.Set<OfficerReportingApplication>()
            .Include(a => a.Submission)
            .FirstOrDefaultAsync(a => a.Id == id);

        if (app is null) return NotFound();

        // Pemohon hanya boleh melihat miliknya sendiri.
        if (app.Submission!.ApplicantUserId != currentUser.UserId
            && !currentUser.IsInRole(AdminRole)) return Forbid();

        var vm = KeViewModel(app);
        await IsiDropdownAsync(vm);
        return View("Form", vm);
    }

    /// <summary>
    /// Simpan draf. Validation SENGAJA dilonggarkan: pemohon mungkin
    /// perlu mencari dokumen dan kembali kemudian. Validation penuh
    /// berlaku pada HANTAR (Hari 5–6).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(OfficerReportingFormViewModel vm)
    {
        // Draf memerlukan cukup untuk mengenal pasti rekod sahaja.
        if (string.IsNullOrWhiteSpace(vm.FullName))
        {
            ModelState.Clear();
            ModelState.AddModelError(nameof(vm.FullName),
                "Nama penuh diperlukan walaupun untuk draf.");
            await IsiDropdownAsync(vm);
            return View("Form", vm);
        }

        // Buang ralat validation lain — ini draf, bukan penghantaran.
        ModelState.Clear();

        OfficerReportingApplication app;

        if (vm.Id is null)
        {
            // Cipta Submission induk DAHULU — jadual detail memerlukan idnya.
            var submission = new Submission
            {
                ModuleCode = ModuleCode,
                ApplicantUserId = currentUser.UserId!,
                Status = SubmissionStatus.Draft
            };
            Db.Submissions.Add(submission);
            await Db.SaveChangesAsync();

            app = new OfficerReportingApplication { SubmissionId = submission.Id };
            Db.Set<OfficerReportingApplication>().Add(app);
        }
        else
        {
            app = (await Db.Set<OfficerReportingApplication>()
                .Include(a => a.Submission)
                .FirstOrDefaultAsync(a => a.Id == vm.Id))!;

            if (app is null) return NotFound();
            if (app.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();

            // Draf yang sudah dihantar tidak boleh disunting.
            if (app.Submission.Status != SubmissionStatus.Draft) return Forbid();
        }

        SalinKeEntiti(vm, app);
        await Db.SaveChangesAsync();

        TempData["Mesej"] = "Draf disimpan.";
        return RedirectToAction(nameof(Edit), new { id = app.Id });
    }

    // ----- pembantu peribadi -----

    private async Task IsiDropdownAsync(OfficerReportingFormViewModel vm)
    {
        vm.Departments = await Db.LookupDepartments.Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .Select(l => new SelectListItem(l.Name, l.Id.ToString()))
            .ToListAsync();

        vm.Positions = await Db.LookupPositions.Where(l => l.IsActive)
            .OrderBy(l => l.Name)
            .Select(l => new SelectListItem(l.Name, l.Id.ToString()))
            .ToListAsync();

        vm.Grades = await Db.LookupGrades.Where(l => l.IsActive)
            .OrderBy(l => l.Code)
            .Select(l => new SelectListItem(l.Name, l.Id.ToString()))
            .ToListAsync();
    }

    private static void SalinKeEntiti(
        OfficerReportingFormViewModel vm, OfficerReportingApplication app)
    {
        app.FullName = vm.FullName;
        app.IdentityNo = vm.IdentityNo;
        app.Email = vm.Email;
        app.Phone = vm.Phone;
        app.DateOfBirth = vm.DateOfBirth;
        app.ReportingDate = vm.ReportingDate;
        app.DepartmentId = vm.DepartmentId;
        app.PositionId = vm.PositionId;
        app.GradeId = vm.GradeId;
        app.PreviousAgency = vm.PreviousAgency;
        app.EmergencyContactName = vm.EmergencyContactName;
        app.EmergencyContactPhone = vm.EmergencyContactPhone;
        app.DeclarationAccepted = vm.DeclarationAccepted;
    }

    private static OfficerReportingFormViewModel KeViewModel(
        OfficerReportingApplication app) => new()
    {
        Id = app.Id,
        SubmissionId = app.SubmissionId,
        FullName = app.FullName,
        IdentityNo = app.IdentityNo,
        Email = app.Email,
        Phone = app.Phone,
        DateOfBirth = app.DateOfBirth,
        ReportingDate = app.ReportingDate,
        DepartmentId = app.DepartmentId,
        PositionId = app.PositionId,
        GradeId = app.GradeId,
        PreviousAgency = app.PreviousAgency,
        EmergencyContactName = app.EmergencyContactName,
        EmergencyContactPhone = app.EmergencyContactPhone,
        DeclarationAccepted = app.DeclarationAccepted,
        IsEditable = app.Submission?.Status == SubmissionStatus.Draft
    };
}
```

2. Perhatikan: kami **tidak** menulis `Approve` atau `Reject`. Ia diwarisi.

### ✅ Semakan

- [ ] Controller mewarisi `SubmissionControllerBase`
- [ ] `ModuleCode` dan `AdminRole` diatasi (override)
- [ ] **Tiada** logik Approve/Reject ditulis dalam controller anda
- [ ] Entiti modul diakses melalui `Db.Set<OfficerReportingApplication>()`
- [ ] Semakan pemilikan: pemohon tidak boleh melihat permohonan orang lain
- [ ] `dotnet build` berjaya

---

## Latihan 7 — Razor view

**Objektif:** Borang yang boleh diisi dan disimpan.

### Langkah

1. `Views/OfficerReporting/Form.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.LaporDiri.OfficerReportingFormViewModel
@{
    ViewData["Title"] = Model.Id is null ? "Permohonan Lapor Diri Baharu" : "Sunting Permohonan Lapor Diri";
}

<h2>@ViewData["Title"]</h2>

@if (TempData["Mesej"] is string mesej)
{
    <div class="alert alert-success">@mesej</div>
}

@if (!Model.IsEditable)
{
    <div class="alert alert-info">
        Permohonan ini telah dihantar dan tidak boleh disunting lagi.
    </div>
}

<form asp-action="SaveDraft" method="post">
    @Html.AntiForgeryToken()
    <input type="hidden" asp-for="Id" />
    <input type="hidden" asp-for="SubmissionId" />

    <div asp-validation-summary="All" class="text-danger mb-3"></div>

    <fieldset disabled="@(!Model.IsEditable)">

        <h5 class="mt-4">Maklumat Peribadi</h5>
        <div class="row g-3">
            <div class="col-md-6">
                <label asp-for="FullName" class="form-label"></label>
                <input asp-for="FullName" class="form-control" />
                <span asp-validation-for="FullName" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="IdentityNo" class="form-label"></label>
                <input asp-for="IdentityNo" class="form-control" placeholder="010203-14-5678" />
                <span asp-validation-for="IdentityNo" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="Email" class="form-label"></label>
                <input asp-for="Email" class="form-control" />
                <span asp-validation-for="Email" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="Phone" class="form-label"></label>
                <input asp-for="Phone" class="form-control" />
                <span asp-validation-for="Phone" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="DateOfBirth" class="form-label"></label>
                <input asp-for="DateOfBirth" class="form-control" type="date" />
            </div>
        </div>

        <h5 class="mt-4">Maklumat Perkhidmatan</h5>
        <div class="row g-3">
            <div class="col-md-6">
                <label asp-for="ReportingDate" class="form-label"></label>
                <input asp-for="ReportingDate" class="form-control" type="date" />
                <span asp-validation-for="ReportingDate" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="DepartmentId" class="form-label"></label>
                <select asp-for="DepartmentId" asp-items="Model.Departments" class="form-select">
                    <option value="">— Pilih bahagian —</option>
                </select>
                <span asp-validation-for="DepartmentId" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="PositionId" class="form-label"></label>
                <select asp-for="PositionId" asp-items="Model.Positions" class="form-select">
                    <option value="">— Pilih jawatan —</option>
                </select>
                <span asp-validation-for="PositionId" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="GradeId" class="form-label"></label>
                <select asp-for="GradeId" asp-items="Model.Grades" class="form-select">
                    <option value="">— Pilih gred —</option>
                </select>
                <span asp-validation-for="GradeId" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="PreviousAgency" class="form-label"></label>
                <input asp-for="PreviousAgency" class="form-control" />
            </div>
        </div>

        <h5 class="mt-4">Waris Kecemasan</h5>
        <div class="row g-3">
            <div class="col-md-6">
                <label asp-for="EmergencyContactName" class="form-label"></label>
                <input asp-for="EmergencyContactName" class="form-control" />
            </div>
            <div class="col-md-6">
                <label asp-for="EmergencyContactPhone" class="form-label"></label>
                <input asp-for="EmergencyContactPhone" class="form-control" />
            </div>
        </div>

        <div class="form-check mt-4">
            <input asp-for="DeclarationAccepted" class="form-check-input" />
            <label asp-for="DeclarationAccepted" class="form-check-label"></label>
        </div>

        <div class="mt-4">
            <button type="submit" class="btn btn-secondary">Simpan Draf</button>
            <a asp-action="Index" class="btn btn-link">Kembali</a>
            @* Butang Hantar ditambah pada Hari 5–6, selepas lampiran wujud. *@
        </div>

    </fieldset>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

2. `Views/OfficerReporting/Index.cshtml`:

```cshtml
@model IEnumerable<Nres.Onboarding.Web.Models.Shared.Submission>
@{ ViewData["Title"] = "Permohonan Lapor Diri Saya"; }

<div class="d-flex justify-content-between align-items-center">
    <h2>@ViewData["Title"]</h2>
    <a asp-action="Create" class="btn btn-primary">Permohonan Baharu</a>
</div>

<table class="table table-hover mt-3">
    <thead>
        <tr>
            <th>No. Rujukan</th>
            <th>Status</th>
            <th>Dicipta</th>
            <th></th>
        </tr>
    </thead>
    <tbody>
    @if (!Model.Any())
    {
        <tr><td colspan="4" class="text-muted">Tiada permohonan lagi.</td></tr>
    }
    @foreach (var s in Model)
    {
        <tr>
            <td>@(string.IsNullOrEmpty(s.ReferenceNo) ? "(draf)" : s.ReferenceNo)</td>
            <td><partial name="_StatusBadge" model="s.Status" /></td>
            <td>@s.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy")</td>
            <td class="text-end">
                <a asp-action="Edit" asp-route-id="@s.Id" class="btn btn-sm btn-outline-primary">
                    Buka
                </a>
            </td>
        </tr>
    }
    </tbody>
</table>
```

> **Perhatikan `<partial name="_StatusBadge" ... />`** — anda menggunakan komponen kongsi, bukan menulis lencana anda sendiri. Setiap modul akan memaparkan status dengan cara yang sama.

3. Uji:

```bash
dotnet run
```

Log masuk sebagai `applicant@nres.test` / `Nres@2026!` → Lapor Diri → Permohonan Baharu → isi separuh → Simpan Draf.

### ✅ Semakan

- [ ] Borang dipaparkan dengan dropdown berisi
- [ ] Simpan draf berfungsi dengan medan tidak lengkap
- [ ] Draf muncul dalam senarai `Index` dengan lencana "Draf"
- [ ] Membuka semula draf memaparkan data tersimpan
- [ ] Anda menggunakan `_StatusBadge` kongsi, bukan menulis sendiri
- [ ] Log masuk sebagai pemohon lain **tidak** boleh membuka draf anda

---

## Latihan 8 — Code review, PR & gabungan latihan

**Objektif:** Tutup hari mengikut kontrak.

### Langkah

1. **Semakan diri** — jalankan sebelum meminta review:

```bash
git diff --name-only master
```

Sahkan: hanya fail dalam folder Kumpulan 1, **ditambah** satu baris dinyahkomen dalam `Program.cs`.

2. **Semakan AI** — prompt dari `docs/kumpulan-1/nota-ai.md`:

```text
Semak diff ini terhadap AGENTS.md dan KOLABORASI.md:
1. Adakah ia menduplikasi apa-apa dalam daftar komponen kongsi?
2. Adakah ia menyentuh fail di luar folder Kumpulan 1?
3. Adakah authorization dan validation pelayan lengkap?
Senaraikan masalah. JANGAN tulis semula kod.
```

3. Buka PR ke `kump-1/lapor-diri`, minta rakan sekumpulan menyemak menggunakan empat soalan penyemak.

4. Selepas digabung, lakukan **gabungan latihan** ke `master`:

```bash
git switch kump-1/lapor-diri
git pull --rebase origin master
git push origin kump-1/lapor-diri
```

Buka PR `kump-1/lapor-diri` → `master`. Selesaikan sebarang konflik **hari ini**, bukan pada Hari 15.

5. Kemas kini board: pindahkan isu yang selesai ke **Done**.

### ✅ Semakan (Definition of Done)

- [ ] `dotnet build` bersih; aplikasi bermula; borang berfungsi manual
- [ ] Servis/komponen kongsi digunakan — tiada logik didup
- [ ] Hanya fail Kumpulan 1 disentuh (+1 baris `Program.cs`)
- [ ] Validation di pelayan
- [ ] Semakan pemilikan pada `Edit`
- [ ] Migration melalui slot yang betul
- [ ] Kod jana-AI difahami dan boleh diterangkan
- [ ] PR ada perihalan BM + langkah ujian
- [ ] Disemak & diluluskan rakan sekumpulan
- [ ] **Gabungan latihan ke `master` selesai**
- [ ] Isu board dipindah ke Done

---

## Deliverable Hari 4

| Artifak | Lokasi |
|---------|--------|
| Entiti + konfigurasi | `Models/LaporDiri/` |
| Pendaftaran modul + descriptor | `Services/LaporDiri/`, `Models/LaporDiri/` |
| Migration `LaporDiriApplication` | `Migrations/` |
| View model | `ViewModels/LaporDiri/` |
| Controller | `Controllers/OfficerReportingController.cs` |
| View borang & senarai | `Views/OfficerReporting/` |

**Esok (Hari 5–6):** muat naik dokumen sokongan, jana nombor rujukan `LD-2026-####`, dan hantar permohonan dengan validation penuh.
