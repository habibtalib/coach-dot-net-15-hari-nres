# Lab · Kumpulan 1 · Hari 4 — Skema PKS & Borang Draf

> Konsep: [`../README.md`](../README.md) · Kanun: [`../../../../SPEC-KURSUS.md`](../../../../SPEC-KURSUS.md) · Kontrak: [`../../../../KOLABORASI.md`](../../../../KOLABORASI.md)

## Latihan 0 — Mula hari dengan betul

**Objektif:** Segerak, semak, dan sahkan sebelum menaip.

### Langkah

1. Segerak dengan `master`:

```bash
git switch kump-1/pentadbiran
git pull --rebase origin master
dotnet build
```

2. **Semakan "sudah wujud?"** — sebelum menulis apa-apa hari ini:

```bash
grep -ri "ComplianceDeclaration" Nres.Onboarding.Web/
grep -ri "PolicyVersion"         Nres.Onboarding.Web/
grep -ri "ReferenceNumber"       Nres.Onboarding.Web/Services/
```

Anda sepatutnya menemui `IReferenceNumberService` **sudah wujud** (Hari 3). Anda akan menggunakannya pada Hari 5–6 — jangan tulis satu lagi. Tiada `ComplianceDeclaration` atau `PolicyVersion` lagi — itu kerja anda hari ini.

3. Cipta cabang ciri:

```bash
git switch -c kump-1/feat/pks-skema-dan-borang-draf
```

> **Nota cabang:** ketiga-tiga projek Kumpulan 1 (Lapor Diri, PKS, Kontrak) berkongsi cabang kumpulan `kump-1/pentadbiran`. Guna cabang ciri berasingan bagi setiap kerja supaya PR kecil dan bersih.

### ✅ Semakan

- [ ] `dotnet build` berjaya pada cabang kumpulan anda
- [ ] Anda mengesahkan `IReferenceNumberService` sudah wujud
- [ ] Anda berada pada cabang ciri, bukan terus pada `kump-1/pentadbiran`

---

## Latihan 1 — Entiti `PolicyVersion` & `ComplianceDeclaration`

**Objektif:** Modelkan versi polisi dan akuan pematuhan dua varian, tanpa menduplikasi medan `Submission`.

### Langkah

1. `Models/Pks/PolicyVersion.cs` — versi Polisi Keselamatan Siber:

```csharp
namespace Nres.Onboarding.Web.Models.Pks;

/// <summary>
/// Satu versi Polisi Keselamatan Siber NRES. Data rujukan yang ditadbir BPM,
/// BUKAN permohonan. Hanya SATU versi menjadi semasa (IsCurrent) pada satu masa —
/// dikuatkuasakan oleh indeks unik ditapis dalam konfigurasi.
/// </summary>
public class PolicyVersion
{
    public int Id { get; set; }

    /// <summary>Label versi yang dibaca manusia, cth "PKS-2026 v1.0".</summary>
    public string VersionLabel { get; set; } = string.Empty;

    public string Title { get; set; } = string.Empty;

    /// <summary>Ringkasan perubahan berbanding versi terdahulu.</summary>
    public string? Summary { get; set; }

    public DateTime EffectiveDate { get; set; }

    /// <summary>Versi yang staf/kontraktor mesti akui sekarang. Tepat satu = true.</summary>
    public bool IsCurrent { get; set; }

    public DateTime CreatedAt { get; set; }
}
```

2. `Models/Pks/PksDeclarantType.cs` — diskriminator varian:

```csharp
namespace Nres.Onboarding.Web.Models.Pks;

/// <summary>
/// Varian borang akuan. Satu entiti, dua bentuk — kontraktor menambah
/// maklumat syarikat pada medan staf yang sama.
/// </summary>
public enum PksDeclarantType
{
    Staff = 1,
    Contractor = 2
}
```

3. `Models/Pks/ComplianceDeclaration.cs` — jadual detail:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Pks;

/// <summary>
/// Jadual DETAIL bagi Akuan Pematuhan Polisi Keselamatan Siber. Nombor rujukan,
/// status, pemohon, dan tarikh hantar tinggal dalam Submission induk — JANGAN
/// pendua di sini. Setiap akuan memaut ke PolicyVersion yang diakui pemilik.
/// </summary>
public class ComplianceDeclaration
{
    public int Id { get; set; }

    /// <summary>Kunci asing ke Submission induk. Unik — satu-ke-satu.</summary>
    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    // --- Varian ---
    public PksDeclarantType DeclarantType { get; set; } = PksDeclarantType.Staff;

    // --- Maklumat pengaku (staf & kontraktor) ---
    public string FullName { get; set; } = string.Empty;
    public string IcNo { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Division { get; set; } = string.Empty;

    // --- Maklumat syarikat (kontraktor sahaja) ---
    public string? CompanyName { get; set; }
    public string? CompanyRegNo { get; set; }

    // --- Versi polisi yang diakui ---
    /// <summary>Versi Polisi Keselamatan Siber yang pemilik akui patuh.</summary>
    public int PolicyVersionId { get; set; }
    public PolicyVersion? PolicyVersion { get; set; }

    /// <summary>Bila pengaku menandatangani akuan (diisi pada HANTAR).</summary>
    public DateTime? AcknowledgedAt { get; set; }

    // --- Akuan (wajib untuk HANTAR, bukan draf) ---
    /// <summary>Pengaku bersetuju dengan NDA di bawah Akta Rahsia Rasmi 1972.</summary>
    public bool NdaAccepted { get; set; }

    /// <summary>Pengaku mengesahkan akan mematuhi Polisi Keselamatan Siber.</summary>
    public bool DeclarationAccepted { get; set; }
}
```

4. Perhatikan apa yang **tiada**: `ReferenceNo`, `Status`, `ApplicantUserId`, `SubmittedAt`. Semak sendiri — jika anda tergoda menambahnya, baca semula [`../README.md`](../README.md).

### ✅ Semakan

- [ ] Kedua-dua fail dalam `Models/Pks/`, bukan `Models/Shared/`
- [ ] Namespace `Nres.Onboarding.Web.Models.Pks`
- [ ] **Sifar** medan diduplikasi dari `Submission`
- [ ] Medan kontraktor (`CompanyName`, `CompanyRegNo`) ialah `nullable`
- [ ] `dotnet build` berjaya

---

## Latihan 2 — Konfigurasi EF Core (corak anti-konflik)

**Objektif:** Daftar entiti anda dengan EF Core **tanpa menyentuh `ApplicationDbContext`**, dan seed satu versi polisi semasa.

### Langkah

1. `Models/Pks/Configurations/PolicyVersionConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Pks.Configurations;

public class PolicyVersionConfiguration : IEntityTypeConfiguration<PolicyVersion>
{
    public void Configure(EntityTypeBuilder<PolicyVersion> builder)
    {
        builder.ToTable("PolicyVersions");

        builder.Property(p => p.VersionLabel).HasMaxLength(50).IsRequired();
        builder.Property(p => p.Title).HasMaxLength(200).IsRequired();
        builder.Property(p => p.Summary).HasMaxLength(1000);

        builder.HasIndex(p => p.VersionLabel).IsUnique();

        // HANYA satu versi boleh IsCurrent = true. Indeks unik DITAPIS
        // menguatkuasakannya di peringkat pangkalan data — bukan hanya dalam kod.
        builder.HasIndex(p => p.IsCurrent)
            .IsUnique()
            .HasFilter("[IsCurrent] = 1");

        // Seed satu versi semasa supaya modul berfungsi sebaik migration digunakan.
        // HasData memerlukan nilai statik — tiada DateTime.Now.
        builder.HasData(new PolicyVersion
        {
            Id = 1,
            VersionLabel = "PKS-2026 v1.0",
            Title = "Polisi Keselamatan Siber NRES 2026",
            Summary = "Versi awal untuk latihan.",
            EffectiveDate = new DateTime(2026, 1, 1),
            IsCurrent = true,
            CreatedAt = new DateTime(2026, 1, 1)
        });
    }
}
```

2. `Models/Pks/Configurations/ComplianceDeclarationConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Pks.Configurations;

public class ComplianceDeclarationConfiguration
    : IEntityTypeConfiguration<ComplianceDeclaration>
{
    public void Configure(EntityTypeBuilder<ComplianceDeclaration> builder)
    {
        builder.ToTable("ComplianceDeclarations");

        builder.Property(d => d.DeclarantType).HasConversion<int>();

        builder.Property(d => d.FullName).HasMaxLength(200).IsRequired();
        builder.Property(d => d.IcNo).HasMaxLength(20).IsRequired();
        builder.Property(d => d.Position).HasMaxLength(150).IsRequired();
        builder.Property(d => d.Division).HasMaxLength(150).IsRequired();
        builder.Property(d => d.CompanyName).HasMaxLength(200);
        builder.Property(d => d.CompanyRegNo).HasMaxLength(50);

        // Satu-ke-satu dengan Submission induk, dikuatkuasakan indeks unik.
        builder.HasIndex(d => d.SubmissionId).IsUnique();
        builder.HasOne(d => d.Submission)
            .WithOne()
            .HasForeignKey<ComplianceDeclaration>(d => d.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // PolicyVersion ialah data rujukan — sekat pemadaman semasa masih dirujuk.
        builder.HasOne(d => d.PolicyVersion)
            .WithMany()
            .HasForeignKey(d => d.PolicyVersionId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

3. **Sahkan anda tidak menyentuh `ApplicationDbContext`:**

```bash
git diff --name-only master
```

Senarai itu **tidak** sepatutnya mengandungi `Data/ApplicationDbContext.cs`. `ApplyConfigurationsFromAssembly()` menemui kelas anda secara automatik.

### ✅ Semakan

- [ ] Kedua-dua fail konfigurasi dalam `Models/Pks/Configurations/`
- [ ] Indeks unik ditapis pada `IsCurrent` (hanya satu versi semasa)
- [ ] Satu `PolicyVersion` diseed dengan `HasData`
- [ ] `git diff --name-only master` menunjukkan **tiada** fail kongsi
- [ ] `dotnet build` berjaya

---

## Latihan 3 — Pendaftaran modul & navigasi

**Objektif:** Sambungkan modul PKS ke aplikasi dengan menambah fail, bukan menyunting fail.

### Langkah

1. `Services/Pks/PksModule.cs`:

```csharp
using Nres.Onboarding.Web.Models.Pks;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Pks;

/// <summary>
/// Pendaftaran servis modul Pematuhan PKS. Program.cs memanggil AddPksModule()
/// dan tidak pernah perlu berubah lagi — kami menambah servis DI SINI.
/// </summary>
public static class PksModule
{
    public static IServiceCollection AddPksModule(this IServiceCollection services)
    {
        services.AddScoped<IModuleDescriptorProvider, PksModuleDescriptor>();
        // Servis modul lain ditambah di sini pada Hari 5–6 dan seterusnya.
        return services;
    }
}
```

2. `Models/Pks/PksModuleDescriptor.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Pks;

/// <summary>
/// Menjadikan modul PKS muncul dalam navigasi untuk peranan yang betul.
/// Dikumpul automatik oleh ModuleNavViewComponent — tiada suntingan
/// pada _Layout.cshtml.
/// </summary>
public class PksModuleDescriptor : IModuleDescriptorProvider
{
    public ModuleDescriptor Describe() => new(
        Code: ModuleCodes.PematuhanPks,
        Nama: "Pematuhan PKS",
        Controller: "Compliance",
        Ikon: "bi-shield-lock",
        Roles: ["Applicant", "IctSecurityOfficer", "SystemAdmin"],
        Urutan: 2);
}
```

> **`ModuleCodes.PematuhanPks`** ialah pemalar kongsi (nilai `"PKS"`) yang ditakrif dalam `Models/Shared/ModuleCodes.cs` pada Hari 3. Jika ia belum wujud, buka isu `shared` — **jangan** cipta pemalar anda sendiri dalam folder modul.

3. **Satu-satunya suntingan fail kongsi hari ini.** Beritahu jurulatih, kemudian nyahkomen **satu baris** dalam `Program.cs`:

```csharp
using Nres.Onboarding.Web.Services.Pks;        // ← tambah using

// ...
builder.Services.AddLaporDiriModule();      // Kumpulan 1 · Lapor Diri
builder.Services.AddPksModule();            // Kumpulan 1 · Pematuhan PKS  ← nyahkomen INI
// builder.Services.AddKontrakModule();     // Kumpulan 1 · Pengurusan Kontrak
// builder.Services.AddAksesModule();       // Kumpulan 2
// builder.Services.AddAkaunModule();       // Kumpulan 3
// builder.Services.AddFasilitiModule();    // Kumpulan 4
```

> ⚠️ **Nyahkomen baris ANDA sahaja.** Jika anda menyahkomen baris projek/kumpulan lain, binaan gagal untuk semua orang kerana kaedah mereka belum wujud.

### ✅ Semakan

- [ ] `PksModule.cs` dan `PksModuleDescriptor.cs` wujud dalam folder anda
- [ ] Descriptor menggunakan peranan `IctSecurityOfficer` (bukan "BPM" sebagai peranan)
- [ ] Tepat **satu** baris dinyahkomen dalam `Program.cs`
- [ ] `dotnet build` berjaya

---

## Latihan 4 — Migration (slot!)

**Objektif:** Cipta jadual anda dan seed versi polisi dalam pangkalan data.

### Langkah

1. **Umumkan slot migration:** *"Kumpulan 1 (PKS) mengambil slot migration."* Tunggu pengesahan jurulatih.

2. Segerak dahulu — sentiasa:

```bash
git pull --rebase origin master
```

3. Jana:

```bash
cd Nres.Onboarding.Web
dotnet ef migrations add PksComplianceSchema
```

4. **Baca fail yang dijana.** Sahkan ia mencipta `PolicyVersions` dan `ComplianceDeclarations` dengan indeks unik pada `SubmissionId`, indeks ditapis pada `IsCurrent`, dan satu baris seed `PolicyVersions` — dan **tiada apa-apa lagi**. Jika ia menyentuh jadual projek lain, anda tidak menyegerak dengan betul.

5. Guna pakai dan uji:

```bash
dotnet ef database update
dotnet run
```

6. Commit, push, dan **lepaskan slot**: *"Kumpulan 1 (PKS) selesai slot migration."*

```bash
cd ..
git add .
git commit -m "pks: entiti PolicyVersion & ComplianceDeclaration, konfigurasi, pendaftaran modul dan migration"
git push -u origin kump-1/feat/pks-skema-dan-borang-draf
```

### Jika snapshot berkonflik

Jangan baiki dengan tangan:

```bash
git checkout --theirs Migrations/ApplicationDbContextModelSnapshot.cs
rm Migrations/*_PksComplianceSchema.cs Migrations/*_PksComplianceSchema.Designer.cs
git pull --rebase origin master
dotnet ef migrations add PksComplianceSchema
dotnet ef database update
```

### ✅ Semakan

- [ ] Slot diumumkan sebelum menjana
- [ ] Migration hanya menyentuh jadual **anda** (`PolicyVersions`, `ComplianceDeclarations`)
- [ ] Baris seed `PolicyVersions` (Id 1, IsCurrent) wujud selepas `database update`
- [ ] Aplikasi bermula; "Pematuhan PKS" muncul dalam navigasi
- [ ] Slot dilepaskan

---

## Latihan 5 — View model dengan validation dua peringkat + bersyarat

**Objektif:** Satu view model yang membenarkan draf tidak lengkap, menguatkuasakan penghantaran lengkap, dan mewajibkan medan syarikat **hanya** bila varian = kontraktor.

### Langkah

1. `ViewModels/Pks/ComplianceFormViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models.Pks;

namespace Nres.Onboarding.Web.ViewModels.Pks;

/// <summary>
/// Borang mengikat kelas INI, bukan entiti — supaya penyerang tidak boleh
/// menghantar Status=AdminApproved bersama borang (over-posting).
///
/// Validation dua peringkat: [Required] terpakai pada HANTAR sahaja; simpan
/// draf memintasnya dalam controller. Validation bersyarat (medan syarikat)
/// dilaksanakan melalui IValidatableObject.
/// </summary>
public class ComplianceFormViewModel : IValidatableObject
{
    public int? Id { get; set; }
    public int? SubmissionId { get; set; }

    [Display(Name = "Jenis pengaku")]
    public PksDeclarantType DeclarantType { get; set; } = PksDeclarantType.Staff;

    [Display(Name = "Nama penuh")]
    [Required(ErrorMessage = "Nama penuh wajib diisi.")]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Display(Name = "No. kad pengenalan")]
    [Required(ErrorMessage = "No. kad pengenalan wajib diisi.")]
    [RegularExpression(@"^\d{6}-\d{2}-\d{4}$", ErrorMessage = "Format: 010203-14-5678")]
    public string IcNo { get; set; } = string.Empty;

    [Display(Name = "Jawatan")]
    [Required(ErrorMessage = "Jawatan wajib diisi.")]
    [StringLength(150)]
    public string Position { get; set; } = string.Empty;

    [Display(Name = "Bahagian")]
    [Required(ErrorMessage = "Bahagian wajib diisi.")]
    [StringLength(150)]
    public string Division { get; set; } = string.Empty;

    // --- Kontraktor sahaja ---
    [Display(Name = "Nama syarikat")]
    [StringLength(200)]
    public string? CompanyName { get; set; }

    [Display(Name = "No. pendaftaran syarikat (SSM)")]
    [StringLength(50)]
    public string? CompanyRegNo { get; set; }

    // --- Akuan ---
    [Display(Name = "Saya bersetuju dengan Akta Rahsia Rasmi 1972 (NDA)")]
    public bool NdaAccepted { get; set; }

    [Display(Name = "Saya mengaku akan mematuhi Polisi Keselamatan Siber NRES")]
    public bool DeclarationAccepted { get; set; }

    // --- Konteks polisi (paparan, bukan input) ---
    public int PolicyVersionId { get; set; }
    public string? PolicyVersionLabel { get; set; }
    public string? PolicyTitle { get; set; }

    /// <summary>Draf boleh disunting; selepas dihantar, borang dikunci.</summary>
    public bool IsEditable { get; set; } = true;

    /// <summary>
    /// Validation bersyarat: kontraktor mesti isi maklumat syarikat.
    /// Dijalankan oleh MVC selepas [Required] biasa, di pelayan.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (DeclarantType == PksDeclarantType.Contractor)
        {
            if (string.IsNullOrWhiteSpace(CompanyName))
                yield return new ValidationResult(
                    "Nama syarikat wajib untuk pengaku kontraktor.",
                    [nameof(CompanyName)]);

            if (string.IsNullOrWhiteSpace(CompanyRegNo))
                yield return new ValidationResult(
                    "No. pendaftaran syarikat wajib untuk pengaku kontraktor.",
                    [nameof(CompanyRegNo)]);
        }
    }
}
```

### ✅ Semakan

- [ ] View model dalam `ViewModels/Pks/`
- [ ] Medan wajib mempunyai `[Required]` dengan mesej Bahasa Melayu
- [ ] `IValidatableObject.Validate` mewajibkan medan syarikat hanya untuk kontraktor
- [ ] Tiada sifat `Status` atau `ReferenceNo` — itu milik `Submission`
- [ ] `dotnet build` berjaya

---

## Latihan 6 — Controller: cipta, sunting, simpan draf

**Objektif:** Aliran draf yang berfungsi, mewarisi kelas asas kongsi, mengait versi polisi semasa pada draf baharu.

### Langkah

1. `Controllers/ComplianceController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Pks;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels.Pks;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class ComplianceController(
    ApplicationDbContext db,
    IWorkflowService workflow,
    INotificationService notifications,
    ICurrentUserService currentUser)
    : SubmissionControllerBase(db, workflow, notifications)
{
    // Kelas asas menyediakan Approve/Reject/SubmitForReview — kami TIDAK
    // menulis semula logik kelulusan.
    protected override string ModuleCode => ModuleCodes.PematuhanPks;
    protected override string AdminRole => "IctSecurityOfficer";

    /// <summary>Senarai akuan pengaku semasa.</summary>
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
        var semasa = await VersiPolisiSemasaAsync();
        var vm = new ComplianceFormViewModel
        {
            PolicyVersionId = semasa.Id,
            PolicyVersionLabel = semasa.VersionLabel,
            PolicyTitle = semasa.Title
        };
        return View("Form", vm);
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var akuan = await Db.Set<ComplianceDeclaration>()
            .Include(d => d.Submission)
            .Include(d => d.PolicyVersion)
            .FirstOrDefaultAsync(d => d.Id == id);

        if (akuan is null) return NotFound();

        // Pengaku hanya boleh melihat miliknya sendiri.
        if (akuan.Submission!.ApplicantUserId != currentUser.UserId
            && !currentUser.IsInRole(AdminRole)) return Forbid();

        return View("Form", KeViewModel(akuan));
    }

    /// <summary>
    /// Simpan draf. Validation SENGAJA dilonggarkan: pengaku mungkin perlu
    /// menyemak butiran dan kembali kemudian. Validation penuh berlaku pada
    /// HANTAR (Hari 5–6).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(ComplianceFormViewModel vm)
    {
        // Draf memerlukan cukup untuk mengenal pasti rekod sahaja.
        if (string.IsNullOrWhiteSpace(vm.FullName))
        {
            ModelState.Clear();
            ModelState.AddModelError(nameof(vm.FullName),
                "Nama penuh diperlukan walaupun untuk draf.");
            await IsiKonteksPolisiAsync(vm);
            return View("Form", vm);
        }

        // Buang ralat validation lain — ini draf, bukan penghantaran.
        ModelState.Clear();

        ComplianceDeclaration akuan;

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

            // Kait versi polisi SEMASA pada saat draf dicipta.
            var semasa = await VersiPolisiSemasaAsync();

            akuan = new ComplianceDeclaration
            {
                SubmissionId = submission.Id,
                PolicyVersionId = semasa.Id
            };
            Db.Set<ComplianceDeclaration>().Add(akuan);
        }
        else
        {
            akuan = (await Db.Set<ComplianceDeclaration>()
                .Include(d => d.Submission)
                .FirstOrDefaultAsync(d => d.Id == vm.Id))!;

            if (akuan is null) return NotFound();
            if (akuan.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();

            // Draf yang sudah dihantar tidak boleh disunting.
            if (akuan.Submission.Status != SubmissionStatus.Draft) return Forbid();
        }

        SalinKeEntiti(vm, akuan);
        await Db.SaveChangesAsync();

        TempData["Mesej"] = "Draf disimpan.";
        return RedirectToAction(nameof(Edit), new { id = akuan.Id });
    }

    // ----- pembantu peribadi -----

    /// <summary>Versi polisi semasa. Sentiasa wujud kerana satu diseed Hari 4.</summary>
    private async Task<PolicyVersion> VersiPolisiSemasaAsync() =>
        await Db.Set<PolicyVersion>().AsNoTracking()
            .FirstOrDefaultAsync(p => p.IsCurrent)
        ?? throw new InvalidOperationException(
            "Tiada versi Polisi Keselamatan Siber semasa. Hubungi BPM.");

    private async Task IsiKonteksPolisiAsync(ComplianceFormViewModel vm)
    {
        var semasa = await VersiPolisiSemasaAsync();
        vm.PolicyVersionId = semasa.Id;
        vm.PolicyVersionLabel = semasa.VersionLabel;
        vm.PolicyTitle = semasa.Title;
    }

    private static void SalinKeEntiti(
        ComplianceFormViewModel vm, ComplianceDeclaration akuan)
    {
        akuan.DeclarantType = vm.DeclarantType;
        akuan.FullName = vm.FullName;
        akuan.IcNo = vm.IcNo;
        akuan.Position = vm.Position;
        akuan.Division = vm.Division;
        akuan.NdaAccepted = vm.NdaAccepted;
        akuan.DeclarationAccepted = vm.DeclarationAccepted;

        // Medan syarikat hanya bermakna untuk kontraktor — kosongkan jika staf.
        if (vm.DeclarantType == PksDeclarantType.Contractor)
        {
            akuan.CompanyName = vm.CompanyName;
            akuan.CompanyRegNo = vm.CompanyRegNo;
        }
        else
        {
            akuan.CompanyName = null;
            akuan.CompanyRegNo = null;
        }
    }

    private static ComplianceFormViewModel KeViewModel(ComplianceDeclaration akuan) => new()
    {
        Id = akuan.Id,
        SubmissionId = akuan.SubmissionId,
        DeclarantType = akuan.DeclarantType,
        FullName = akuan.FullName,
        IcNo = akuan.IcNo,
        Position = akuan.Position,
        Division = akuan.Division,
        CompanyName = akuan.CompanyName,
        CompanyRegNo = akuan.CompanyRegNo,
        NdaAccepted = akuan.NdaAccepted,
        DeclarationAccepted = akuan.DeclarationAccepted,
        PolicyVersionId = akuan.PolicyVersionId,
        PolicyVersionLabel = akuan.PolicyVersion?.VersionLabel,
        PolicyTitle = akuan.PolicyVersion?.Title,
        IsEditable = akuan.Submission?.Status == SubmissionStatus.Draft
    };
}
```

2. Perhatikan: kami **tidak** menulis `Approve` atau `Reject`. Ia diwarisi dari `SubmissionControllerBase`.

### ✅ Semakan

- [ ] Controller mewarisi `SubmissionControllerBase`
- [ ] `ModuleCode` = `ModuleCodes.PematuhanPks`; `AdminRole` = `"IctSecurityOfficer"`
- [ ] **Tiada** logik Approve/Reject ditulis dalam controller anda
- [ ] Draf baharu dikait ke versi polisi **semasa**
- [ ] Semakan pemilikan: pengaku tidak boleh membuka draf orang lain
- [ ] `dotnet build` berjaya

---

## Latihan 7 — Razor view

**Objektif:** Borang dua varian yang boleh diisi dan disimpan, memaparkan versi polisi semasa.

### Langkah

1. `Views/Compliance/Form.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Pks.ComplianceFormViewModel
@using Nres.Onboarding.Web.Models.Pks
@{
    ViewData["Title"] = Model.Id is null
        ? "Akuan Pematuhan PKS Baharu" : "Sunting Akuan Pematuhan PKS";
}

<h2>@ViewData["Title"]</h2>

@if (TempData["Mesej"] is string mesej)
{
    <div class="alert alert-success">@mesej</div>
}

<div class="alert alert-info">
    <strong>Polisi semasa:</strong> @Model.PolicyVersionLabel — @Model.PolicyTitle
    <br />
    <small>Akuan ini mengesahkan pematuhan terhadap versi polisi di atas.</small>
</div>

@if (!Model.IsEditable)
{
    <div class="alert alert-secondary">
        Akuan ini telah dihantar dan tidak boleh disunting lagi.
    </div>
}

<form asp-action="SaveDraft" method="post">
    @Html.AntiForgeryToken()
    <input type="hidden" asp-for="Id" />
    <input type="hidden" asp-for="SubmissionId" />
    <input type="hidden" asp-for="PolicyVersionId" />

    <div asp-validation-summary="All" class="text-danger mb-3"></div>

    <fieldset disabled="@(!Model.IsEditable)">

        <div class="mb-3">
            <label asp-for="DeclarantType" class="form-label"></label>
            <select asp-for="DeclarantType" class="form-select" id="declarantType"
                    asp-items="Html.GetEnumSelectList<PksDeclarantType>()"></select>
        </div>

        <h5 class="mt-4">Maklumat Pengaku</h5>
        <div class="row g-3">
            <div class="col-md-6">
                <label asp-for="FullName" class="form-label"></label>
                <input asp-for="FullName" class="form-control" />
                <span asp-validation-for="FullName" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="IcNo" class="form-label"></label>
                <input asp-for="IcNo" class="form-control" placeholder="010203-14-5678" />
                <span asp-validation-for="IcNo" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="Position" class="form-label"></label>
                <input asp-for="Position" class="form-control" />
                <span asp-validation-for="Position" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="Division" class="form-label"></label>
                <input asp-for="Division" class="form-control" />
                <span asp-validation-for="Division" class="text-danger"></span>
            </div>
        </div>

        <h5 class="mt-4" id="companySection">Maklumat Syarikat (kontraktor sahaja)</h5>
        <div class="row g-3" id="companyFields">
            <div class="col-md-6">
                <label asp-for="CompanyName" class="form-label"></label>
                <input asp-for="CompanyName" class="form-control" />
                <span asp-validation-for="CompanyName" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="CompanyRegNo" class="form-label"></label>
                <input asp-for="CompanyRegNo" class="form-control" />
                <span asp-validation-for="CompanyRegNo" class="text-danger"></span>
            </div>
        </div>

        <div class="form-check mt-4">
            <input asp-for="NdaAccepted" class="form-check-input" />
            <label asp-for="NdaAccepted" class="form-check-label"></label>
        </div>
        <div class="form-check">
            <input asp-for="DeclarationAccepted" class="form-check-input" />
            <label asp-for="DeclarationAccepted" class="form-check-label"></label>
        </div>

        <div class="mt-4">
            <button type="submit" class="btn btn-secondary">Simpan Draf</button>
            <a asp-action="Index" class="btn btn-link">Kembali</a>
            @* Butang Hantar ditambah pada Hari 5–6, selepas versi polisi dikunci. *@
        </div>

    </fieldset>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script>
        // Tunjuk/sembunyi medan syarikat mengikut varian. Ini kemudahan UI SAHAJA —
        // validation sebenar tetap berlaku di PELAYAN melalui IValidatableObject.
        const jenis = document.getElementById('declarantType');
        const seksyen = document.getElementById('companySection');
        const medan = document.getElementById('companyFields');
        function togol() {
            const kontraktor = jenis.value === '@((int)PksDeclarantType.Contractor)';
            seksyen.style.display = kontraktor ? '' : 'none';
            medan.style.display = kontraktor ? '' : 'none';
        }
        jenis.addEventListener('change', togol);
        togol();
    </script>
}
```

2. `Views/Compliance/Index.cshtml`:

```cshtml
@model IEnumerable<Nres.Onboarding.Web.Models.Shared.Submission>
@{ ViewData["Title"] = "Akuan Pematuhan PKS Saya"; }

<div class="d-flex justify-content-between align-items-center">
    <h2>@ViewData["Title"]</h2>
    <a asp-action="Create" class="btn btn-primary">Akuan Baharu</a>
</div>

<table class="table table-hover mt-3">
    <thead>
        <tr><th>No. Rujukan</th><th>Status</th><th>Dicipta</th><th></th></tr>
    </thead>
    <tbody>
    @if (!Model.Any())
    {
        <tr><td colspan="4" class="text-muted">Tiada akuan lagi.</td></tr>
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

> **Perhatikan `<partial name="_StatusBadge" ... />`** — anda menggunakan komponen kongsi, bukan menulis lencana anda sendiri. Setiap modul memaparkan status dengan cara yang sama.

3. Uji:

```bash
dotnet run
```

Log masuk sebagai `applicant@nres.test` / `Nres@2026!` → Pematuhan PKS → Akuan Baharu → pilih **Kontraktor**, isi nama sahaja → Simpan Draf. Buka semula → tukar ke **Staf** → simpan.

### ✅ Semakan

- [ ] Borang dipaparkan dengan banner versi polisi semasa
- [ ] Menukar varian menunjuk/menyembunyikan medan syarikat
- [ ] Simpan draf berfungsi dengan medan tidak lengkap
- [ ] Draf muncul dalam senarai `Index` dengan lencana "(draf)"
- [ ] Membuka semula draf memaparkan data tersimpan
- [ ] Anda menggunakan `_StatusBadge` kongsi, bukan menulis sendiri
- [ ] Log masuk sebagai pengaku lain **tidak** boleh membuka draf anda

---

## Latihan 8 — Code review, PR & gabungan latihan

**Objektif:** Tutup hari mengikut kontrak.

### Langkah

1. **Semakan diri** — jalankan sebelum meminta review:

```bash
git diff --name-only master
```

Sahkan: hanya fail dalam folder `Pks`/`Compliance` Kumpulan 1, **ditambah** satu baris dinyahkomen dalam `Program.cs`.

2. **Semakan AI** — prompt dari `docs/kumpulan-1/nota-ai.md`:

```text
Merujuk AGENTS.md dan KOLABORASI.md, semak diff ini:
1. Adakah ia menduplikasi apa-apa dalam daftar komponen kongsi?
2. Adakah ia menyentuh fail di luar folder modul PKS Kumpulan 1?
3. Adakah authorization dan validation pelayan lengkap (termasuk medan kontraktor)?
Senaraikan masalah. JANGAN tulis semula kod.
```

3. Buka PR ke `kump-1/pentadbiran`, minta rakan sekumpulan menyemak menggunakan empat soalan penyemak.

4. Selepas digabung, lakukan **gabungan latihan** ke `master`:

```bash
git switch kump-1/pentadbiran
git pull --rebase origin master
git push origin kump-1/pentadbiran
```

Buka PR `kump-1/pentadbiran` → `master`. Selesaikan sebarang konflik **hari ini**, bukan pada Hari 15.

5. Kemas kini board: pindahkan isu selesai ke **Done**.

### ✅ Semakan (Definition of Done)

- [ ] `dotnet build` bersih; aplikasi bermula; borang berfungsi manual
- [ ] Servis/komponen kongsi digunakan — tiada logik didup
- [ ] Hanya fail Kumpulan 1 (PKS) disentuh (+1 baris `Program.cs`)
- [ ] Validation di pelayan, termasuk `IValidatableObject` kontraktor
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
| Entiti `PolicyVersion`, `ComplianceDeclaration` + enum | `Models/Pks/` |
| Konfigurasi + seed versi polisi | `Models/Pks/Configurations/` |
| Pendaftaran modul + descriptor | `Services/Pks/`, `Models/Pks/` |
| Migration `PksComplianceSchema` | `Migrations/` |
| View model dua varian | `ViewModels/Pks/` |
| Controller | `Controllers/ComplianceController.cs` |
| View borang & senarai | `Views/Compliance/` |

**Esok (Hari 5–6):** lampiran akuan bertandatangan, kunci versi polisi pada penghantaran, jana nombor rujukan `PKS-2026-####`, dan hantar dengan validation penuh + NDA.
