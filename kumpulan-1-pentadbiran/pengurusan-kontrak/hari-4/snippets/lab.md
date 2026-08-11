# Lab · Kumpulan 1 · Hari 4 — Skema Kontrak & Borang Draf

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
grep -ri "ContractRecord"    Nres.Onboarding.Web/
grep -ri "ContractMilestone" Nres.Onboarding.Web/
grep -ri "ReferenceNumber"   Nres.Onboarding.Web/Services/
```

Anda sepatutnya menemui `IReferenceNumberService` **sudah wujud** (Hari 3). Anda akan menggunakannya pada Hari 5–6 — jangan tulis satu lagi. Tiada `ContractRecord`, `ContractParty`, atau `ContractMilestone` lagi — itu kerja anda hari ini.

3. Cipta cabang ciri:

```bash
git switch -c kump-1/feat/kontrak-skema-dan-borang-draf
```

> **Nota cabang:** ketiga-tiga projek Kumpulan 1 (Lapor Diri, PKS, Kontrak) berkongsi cabang kumpulan `kump-1/pentadbiran`. Guna cabang ciri berasingan bagi setiap kerja supaya PR kecil dan bersih.

### ✅ Semakan

- [ ] `dotnet build` berjaya pada cabang kumpulan anda
- [ ] Anda mengesahkan `IReferenceNumberService` sudah wujud
- [ ] Anda berada pada cabang ciri, bukan terus pada `kump-1/pentadbiran`

---

## Latihan 1 — Entiti kontrak & enum

**Objektif:** Modelkan header kontrak, pihak terlibat, dan milestone bayaran — tanpa menduplikasi medan `Submission`.

### Langkah

1. `Models/Kontrak/ContractType.cs` — jenis kontrak & status milestone:

```csharp
namespace Nres.Onboarding.Web.Models.Kontrak;

/// <summary>Jenis kontrak/perjanjian ICT yang didaftarkan.</summary>
public enum ContractType
{
    Supply = 1,        // Bekalan (cth storan/backup, perkakasan)
    Support = 2,       // Sokongan teknikal
    Maintenance = 3,   // Penyelenggaraan
    Licensing = 4,     // Pelesenan perisian (cth antivirus)
    Consultancy = 5    // Perundingan
}

/// <summary>Peranan sesuatu pihak dalam kontrak.</summary>
public enum ContractPartyRole
{
    MainContractor = 1,
    Subcontractor = 2,
    Vendor = 3,
    Consultant = 4
}

/// <summary>Status sesuatu milestone bayaran/penyerahan.</summary>
public enum MilestoneStatus
{
    Pending = 1,               // Belum sampai tarikh / belum bermula
    DeliverablesReceived = 2,  // Dokumen penyerahan diterima, menunggu bayaran
    Paid = 3                   // Sudah dibayar
}

/// <summary>
/// Dokumen penyerahan yang diperlukan bagi sesuatu milestone. [Flags] — satu
/// milestone boleh memerlukan BEBERAPA dokumen sekali gus (cth Invois + DO).
/// </summary>
[Flags]
public enum MilestoneDeliverable
{
    None = 0,
    Invois = 1,          // Invois
    DeliveryOrder = 2,   // DO
    SuratWarranti = 4,   // Surat Warranti
    Eat = 8,             // Entry Acceptance Test
    Uat = 16,            // User Acceptance Test
    Fat = 32             // Final Acceptance Test
}
```

2. `Models/Kontrak/ContractLifecycleState.cs` — status kitaran hayat yang **dikira**:

```csharp
namespace Nres.Onboarding.Web.Models.Kontrak;

/// <summary>
/// Status kitaran hayat kontrak berbanding tarikh hari ini. Ini DIKIRA daripada
/// (IsTerminated, ExpiryDate, hari ini), BUKAN disimpan — supaya ia tidak pernah
/// menjadi tidak segerak apabila masa berlalu. Pengiraan penuh datang Hari 10–12;
/// hari ini kita hanya mentakrif jenisnya.
/// </summary>
public enum ContractLifecycleState
{
    Active = 0,        // Sah dan belum hampir tamat
    ExpiringSoon = 1,  // Tamat dalam ambang hari yang ditetapkan
    Expired = 2,       // ExpiryDate sudah berlalu
    Terminated = 3     // Ditamatkan awal (IsTerminated)
}
```

3. `Models/Kontrak/ContractRecord.cs` — header kontrak (jadual detail):

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Kontrak;

/// <summary>
/// Jadual DETAIL bagi pendaftaran kontrak. Nombor rujukan (KON-2026-####), status
/// workflow, pemohon, dan tarikh hantar tinggal dalam Submission induk — JANGAN
/// pendua di sini. ContractNo & FileNo pula ialah pengecam SEDIA ADA yang ditaip
/// pengguna, bukan dijana sistem.
/// </summary>
public class ContractRecord
{
    public int Id { get; set; }

    /// <summary>Kunci asing ke Submission induk. Unik — satu-ke-satu.</summary>
    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    // --- Pengecam sedia ada (ditaip pengguna, BUKAN dijana) ---
    /// <summary>No. sistem kontrak sebenar, cth "CT250000000029728".</summary>
    public string ContractNo { get; set; } = string.Empty;

    /// <summary>No. fail rasmi, cth "NRES.400-5/6/40(S)-7".</summary>
    public string FileNo { get; set; } = string.Empty;

    // --- Butiran kontrak ---
    public string Title { get; set; } = string.Empty;
    public ContractType ContractType { get; set; } = ContractType.Supply;

    /// <summary>Nilai kontrak dalam RM.</summary>
    public decimal Amount { get; set; }

    public DateTime EffectiveDate { get; set; }
    public DateTime ExpiryDate { get; set; }

    /// <summary>Bahagian NRES yang memiliki kontrak.</summary>
    public string Division { get; set; } = string.Empty;

    // --- Penamatan awal (DISIMPAN; status kitaran hayat lain DIKIRA) ---
    public bool IsTerminated { get; set; }
    public DateTime? TerminatedAt { get; set; }
    public string? TerminationReason { get; set; }

    // --- Anak (satu-ke-banyak) ---
    public ICollection<ContractParty> Parties { get; set; } = [];
    public ICollection<ContractMilestone> Milestones { get; set; } = [];
}
```

4. `Models/Kontrak/ContractParty.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Kontrak;

/// <summary>Syarikat yang terlibat dalam kontrak. Satu kontrak boleh ada banyak.</summary>
public class ContractParty
{
    public int Id { get; set; }

    public int ContractRecordId { get; set; }
    public ContractRecord? ContractRecord { get; set; }

    public string CompanyName { get; set; } = string.Empty;

    /// <summary>No. pendaftaran syarikat (SSM), cth "202301012345 (1500123-A)".</summary>
    public string RegistrationNo { get; set; } = string.Empty;

    public ContractPartyRole Role { get; set; } = ContractPartyRole.MainContractor;

    public string? ContactPerson { get; set; }
    public string? ContactEmail { get; set; }
}
```

5. `Models/Kontrak/ContractMilestone.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Kontrak;

/// <summary>
/// Milestone bayaran/penyerahan. Menjejaki jadual bayaran kontrak: setiap bayaran
/// ada amaun, dokumen penyerahan yang diperlukan, tarikh akhir, dan status.
/// </summary>
public class ContractMilestone
{
    public int Id { get; set; }

    public int ContractRecordId { get; set; }
    public ContractRecord? ContractRecord { get; set; }

    /// <summary>Nombor jujukan bayaran dalam kontrak (1, 2, 3, …).</summary>
    public int PaymentNo { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>Amaun bayaran milestone ini (RM).</summary>
    public decimal Amount { get; set; }

    /// <summary>Dokumen penyerahan yang diperlukan (boleh gabungan, cth Invois|DO).</summary>
    public MilestoneDeliverable Deliverables { get; set; } = MilestoneDeliverable.None;

    public DateTime DueDate { get; set; }

    public MilestoneStatus Status { get; set; } = MilestoneStatus.Pending;

    /// <summary>Bila milestone ditandakan dibayar (Hari 7–9).</summary>
    public DateTime? PaidAt { get; set; }
}
```

6. Perhatikan apa yang **tiada** dalam `ContractRecord`: `ReferenceNo`, `Status` (workflow), `ApplicantUserId`, `SubmittedAt`. Semak sendiri — jika anda tergoda menambahnya, baca semula [`../README.md`](../README.md).

### ✅ Semakan

- [ ] Kelima-lima fail dalam `Models/Kontrak/`, bukan `Models/Shared/`
- [ ] Namespace `Nres.Onboarding.Web.Models.Kontrak`
- [ ] **Sifar** medan diduplikasi dari `Submission`
- [ ] `MilestoneDeliverable` menggunakan `[Flags]`
- [ ] `dotnet build` berjaya

---

## Latihan 2 — Konfigurasi EF Core (corak anti-konflik)

**Objektif:** Daftar entiti anda dengan EF Core **tanpa menyentuh `ApplicationDbContext`**.

### Langkah

1. `Models/Kontrak/Configurations/ContractRecordConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Kontrak.Configurations;

public class ContractRecordConfiguration : IEntityTypeConfiguration<ContractRecord>
{
    public void Configure(EntityTypeBuilder<ContractRecord> builder)
    {
        builder.ToTable("ContractRecords");

        builder.Property(c => c.ContractNo).HasMaxLength(50).IsRequired();
        builder.Property(c => c.FileNo).HasMaxLength(80).IsRequired();
        builder.Property(c => c.Title).HasMaxLength(300).IsRequired();
        builder.Property(c => c.Division).HasMaxLength(150).IsRequired();
        builder.Property(c => c.TerminationReason).HasMaxLength(500);

        builder.Property(c => c.ContractType).HasConversion<int>();

        // Wang: ketepatan eksplisit. Tanpa ini, EF Core memberi amaran & lalai
        // penyedia berbeza. 18 digit, 2 perpuluhan mencukupi untuk nilai kontrak.
        builder.Property(c => c.Amount).HasPrecision(18, 2);

        // Satu-ke-satu dengan Submission induk, dikuatkuasakan indeks unik.
        builder.HasIndex(c => c.SubmissionId).IsUnique();
        builder.HasOne(c => c.Submission)
            .WithOne()
            .HasForeignKey<ContractRecord>(c => c.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // ContractNo dicari kerap — indeks. Unik supaya satu kontrak sebenar tidak
        // didaftarkan dua kali. DITAPIS supaya banyak draf boleh berkongsi "" kosong.
        builder.HasIndex(c => c.ContractNo)
            .IsUnique()
            .HasFilter("[ContractNo] <> ''");

        // Anak: satu-ke-banyak. Buang kontrak → buang pihak & milestonenya.
        builder.HasMany(c => c.Parties)
            .WithOne(p => p.ContractRecord!)
            .HasForeignKey(p => p.ContractRecordId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasMany(c => c.Milestones)
            .WithOne(m => m.ContractRecord!)
            .HasForeignKey(m => m.ContractRecordId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
```

2. `Models/Kontrak/Configurations/ContractPartyConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Kontrak.Configurations;

public class ContractPartyConfiguration : IEntityTypeConfiguration<ContractParty>
{
    public void Configure(EntityTypeBuilder<ContractParty> builder)
    {
        builder.ToTable("ContractParties");

        builder.Property(p => p.CompanyName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.RegistrationNo).HasMaxLength(60).IsRequired();
        builder.Property(p => p.ContactPerson).HasMaxLength(150);
        builder.Property(p => p.ContactEmail).HasMaxLength(200);
        builder.Property(p => p.Role).HasConversion<int>();

        builder.HasIndex(p => p.ContractRecordId);
    }
}
```

3. `Models/Kontrak/Configurations/ContractMilestoneConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Kontrak.Configurations;

public class ContractMilestoneConfiguration : IEntityTypeConfiguration<ContractMilestone>
{
    public void Configure(EntityTypeBuilder<ContractMilestone> builder)
    {
        builder.ToTable("ContractMilestones");

        builder.Property(m => m.Description).HasMaxLength(300).IsRequired();
        builder.Property(m => m.Amount).HasPrecision(18, 2);
        builder.Property(m => m.Status).HasConversion<int>();
        builder.Property(m => m.Deliverables).HasConversion<int>();

        // Satu kontrak tidak boleh ada dua milestone dengan PaymentNo sama.
        builder.HasIndex(m => new { m.ContractRecordId, m.PaymentNo }).IsUnique();
    }
}
```

4. **Sahkan anda tidak menyentuh `ApplicationDbContext`:**

```bash
git diff --name-only master
```

Senarai itu **tidak** sepatutnya mengandungi `Data/ApplicationDbContext.cs`. `ApplyConfigurationsFromAssembly()` menemui kelas anda secara automatik.

### ✅ Semakan

- [ ] Ketiga-tiga fail konfigurasi dalam `Models/Kontrak/Configurations/`
- [ ] `Amount` menggunakan `HasPrecision(18, 2)`
- [ ] Indeks unik ditapis pada `ContractNo`
- [ ] Indeks unik komposit pada `(ContractRecordId, PaymentNo)`
- [ ] `git diff --name-only master` menunjukkan **tiada** fail kongsi
- [ ] `dotnet build` berjaya

---

## Latihan 3 — Pendaftaran modul & navigasi

**Objektif:** Sambungkan modul Kontrak ke aplikasi dengan menambah fail, bukan menyunting fail.

### Langkah

1. `Services/Kontrak/KontrakModule.cs`:

```csharp
using Nres.Onboarding.Web.Models.Kontrak;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Kontrak;

/// <summary>
/// Pendaftaran servis modul Pengurusan Kontrak. Program.cs memanggil
/// AddKontrakModule() dan tidak pernah perlu berubah lagi — kami menambah
/// servis DI SINI.
/// </summary>
public static class KontrakModule
{
    public static IServiceCollection AddKontrakModule(this IServiceCollection services)
    {
        services.AddScoped<IModuleDescriptorProvider, ContractModuleDescriptor>();
        // Servis modul lain ditambah di sini pada Hari 5–6 dan seterusnya.
        return services;
    }
}
```

2. `Models/Kontrak/ContractModuleDescriptor.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Kontrak;

/// <summary>
/// Menjadikan modul Kontrak muncul dalam navigasi untuk peranan yang betul.
/// Dikumpul automatik oleh ModuleNavViewComponent — tiada suntingan
/// pada _Layout.cshtml.
/// </summary>
public class ContractModuleDescriptor : IModuleDescriptorProvider
{
    public ModuleDescriptor Describe() => new(
        Code: ModuleCodes.PengurusanKontrak,
        Nama: "Pengurusan Kontrak",
        Controller: "Contract",
        Ikon: "bi-file-earmark-text",
        Roles: ["Applicant", "IctAdmin", "SystemAdmin"],
        Urutan: 3);
}
```

> **`ModuleCodes.PengurusanKontrak`** ialah pemalar kongsi (nilai `"KON"`) yang ditakrif dalam `Models/Shared/ModuleCodes.cs` pada Hari 3. Jika ia belum wujud, buka isu `shared` — **jangan** cipta pemalar anda sendiri dalam folder modul.

3. **Satu-satunya suntingan fail kongsi hari ini.** Beritahu jurulatih, kemudian nyahkomen **satu baris** dalam `Program.cs`:

```csharp
using Nres.Onboarding.Web.Services.Kontrak;    // ← tambah using

// ...
builder.Services.AddLaporDiriModule();      // Kumpulan 1 · Lapor Diri
builder.Services.AddPksModule();            // Kumpulan 1 · Pematuhan PKS
builder.Services.AddKontrakModule();        // Kumpulan 1 · Pengurusan Kontrak  ← nyahkomen INI
// builder.Services.AddAksesModule();       // Kumpulan 2
// builder.Services.AddAkaunModule();       // Kumpulan 3
// builder.Services.AddFasilitiModule();    // Kumpulan 4
```

> ⚠️ **Nyahkomen baris ANDA sahaja.** Jika anda menyahkomen baris projek/kumpulan lain, binaan gagal untuk semua orang kerana kaedah mereka belum wujud.

### ✅ Semakan

- [ ] `KontrakModule.cs` dan `ContractModuleDescriptor.cs` wujud dalam folder anda
- [ ] Descriptor menggunakan peranan `IctAdmin`
- [ ] Tepat **satu** baris dinyahkomen dalam `Program.cs`
- [ ] `dotnet build` berjaya

---

## Latihan 4 — Migration (slot!)

**Objektif:** Cipta tiga jadual anda dalam pangkalan data.

### Langkah

1. **Umumkan slot migration:** *"Kumpulan 1 (Kontrak) mengambil slot migration."* Tunggu pengesahan jurulatih.

2. Segerak dahulu — sentiasa:

```bash
git pull --rebase origin master
```

3. Jana:

```bash
cd Nres.Onboarding.Web
dotnet ef migrations add KontrakContractSchema
```

4. **Baca fail yang dijana.** Sahkan ia mencipta `ContractRecords`, `ContractParties`, dan `ContractMilestones` dengan indeks unik pada `SubmissionId`, indeks ditapis pada `ContractNo`, indeks komposit pada `(ContractRecordId, PaymentNo)` — dan **tiada apa-apa lagi**. Jika ia menyentuh jadual projek lain, anda tidak menyegerak dengan betul.

5. Guna pakai dan uji:

```bash
dotnet ef database update
dotnet run
```

6. Commit, push, dan **lepaskan slot**: *"Kumpulan 1 (Kontrak) selesai slot migration."*

```bash
cd ..
git add .
git commit -m "kontrak: entiti ContractRecord/Party/Milestone, konfigurasi, pendaftaran modul dan migration"
git push -u origin kump-1/feat/kontrak-skema-dan-borang-draf
```

### Jika snapshot berkonflik

Jangan baiki dengan tangan:

```bash
git checkout --theirs Migrations/ApplicationDbContextModelSnapshot.cs
rm Migrations/*_KontrakContractSchema.cs Migrations/*_KontrakContractSchema.Designer.cs
git pull --rebase origin master
dotnet ef migrations add KontrakContractSchema
dotnet ef database update
```

### ✅ Semakan

- [ ] Slot diumumkan sebelum menjana
- [ ] Migration hanya menyentuh jadual **anda** (`ContractRecords`, `ContractParties`, `ContractMilestones`)
- [ ] Aplikasi bermula; "Pengurusan Kontrak" muncul dalam navigasi
- [ ] Slot dilepaskan

---

## Latihan 5 — View model dengan validation dua peringkat

**Objektif:** Satu view model yang membenarkan draf tidak lengkap tetapi menguatkuasakan penghantaran lengkap.

### Langkah

1. `ViewModels/Kontrak/ContractFormViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models.Kontrak;

namespace Nres.Onboarding.Web.ViewModels.Kontrak;

/// <summary>
/// Borang mengikat kelas INI, bukan entiti — supaya penyerang tidak boleh
/// menghantar Status=AdminApproved bersama borang (over-posting).
///
/// Validation dua peringkat: [Required] terpakai pada HANTAR sahaja; simpan
/// draf memintasnya dalam controller. Semakan silang tarikh dilaksanakan
/// melalui IValidatableObject.
/// </summary>
public class ContractFormViewModel : IValidatableObject
{
    public int? Id { get; set; }
    public int? SubmissionId { get; set; }

    [Display(Name = "No. kontrak")]
    [Required(ErrorMessage = "No. kontrak wajib diisi.")]
    [StringLength(50)]
    public string ContractNo { get; set; } = string.Empty;

    [Display(Name = "No. fail")]
    [Required(ErrorMessage = "No. fail wajib diisi.")]
    [StringLength(80)]
    public string FileNo { get; set; } = string.Empty;

    [Display(Name = "Tajuk kontrak")]
    [Required(ErrorMessage = "Tajuk kontrak wajib diisi.")]
    [StringLength(300)]
    public string Title { get; set; } = string.Empty;

    [Display(Name = "Jenis kontrak")]
    public ContractType ContractType { get; set; } = ContractType.Supply;

    [Display(Name = "Nilai kontrak (RM)")]
    [Range(0.01, 1_000_000_000, ErrorMessage = "Nilai kontrak mesti lebih daripada 0.")]
    public decimal Amount { get; set; }

    [Display(Name = "Tarikh kuat kuasa")]
    [DataType(DataType.Date)]
    public DateTime? EffectiveDate { get; set; }

    [Display(Name = "Tarikh tamat")]
    [DataType(DataType.Date)]
    public DateTime? ExpiryDate { get; set; }

    [Display(Name = "Bahagian")]
    [Required(ErrorMessage = "Bahagian wajib diisi.")]
    [StringLength(150)]
    public string Division { get; set; } = string.Empty;

    /// <summary>Draf boleh disunting; selepas dihantar, borang dikunci.</summary>
    public bool IsEditable { get; set; } = true;

    /// <summary>
    /// Semakan silang yang tidak boleh diungkap dengan satu atribut. Dijalankan
    /// oleh MVC selepas [Required] biasa, di pelayan.
    /// </summary>
    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (EffectiveDate is not null && ExpiryDate is not null
            && ExpiryDate <= EffectiveDate)
        {
            yield return new ValidationResult(
                "Tarikh tamat mesti selepas tarikh kuat kuasa.",
                [nameof(ExpiryDate)]);
        }
    }
}
```

### ✅ Semakan

- [ ] View model dalam `ViewModels/Kontrak/`
- [ ] Medan wajib mempunyai `[Required]` dengan mesej Bahasa Melayu
- [ ] `IValidatableObject.Validate` menolak `ExpiryDate <= EffectiveDate`
- [ ] Tiada sifat `Status` atau `ReferenceNo` — itu milik `Submission`
- [ ] `dotnet build` berjaya

---

## Latihan 6 — Controller: cipta, sunting, simpan draf

**Objektif:** Aliran draf yang berfungsi, mewarisi kelas asas kongsi.

### Langkah

1. `Controllers/ContractController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Kontrak;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels.Kontrak;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class ContractController(
    ApplicationDbContext db,
    IWorkflowService workflow,
    INotificationService notifications,
    ICurrentUserService currentUser)
    : SubmissionControllerBase(db, workflow, notifications)
{
    // Kelas asas menyediakan Approve/Reject/SubmitForReview — kami TIDAK
    // menulis semula logik kelulusan.
    protected override string ModuleCode => ModuleCodes.PengurusanKontrak;
    protected override string AdminRole => "IctAdmin";

    /// <summary>Senarai pendaftaran kontrak pengguna semasa.</summary>
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
    public IActionResult Create() => View("Form", new ContractFormViewModel());

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var kontrak = await Db.Set<ContractRecord>()
            .Include(c => c.Submission)
            .FirstOrDefaultAsync(c => c.Id == id);

        if (kontrak is null) return NotFound();

        // Pemohon hanya boleh melihat miliknya sendiri.
        if (kontrak.Submission!.ApplicantUserId != currentUser.UserId
            && !currentUser.IsInRole(AdminRole)) return Forbid();

        return View("Form", KeViewModel(kontrak));
    }

    /// <summary>
    /// Simpan draf. Validation SENGAJA dilonggarkan: pengguna mungkin perlu
    /// menyemak butiran kontrak dan kembali kemudian. Validation penuh berlaku
    /// pada HANTAR (Hari 5–6).
    /// </summary>
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SaveDraft(ContractFormViewModel vm)
    {
        // Draf memerlukan cukup untuk mengenal pasti rekod sahaja.
        if (string.IsNullOrWhiteSpace(vm.Title))
        {
            ModelState.Clear();
            ModelState.AddModelError(nameof(vm.Title),
                "Tajuk kontrak diperlukan walaupun untuk draf.");
            return View("Form", vm);
        }

        // Buang ralat validation lain — ini draf, bukan penghantaran.
        ModelState.Clear();

        ContractRecord kontrak;

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

            kontrak = new ContractRecord { SubmissionId = submission.Id };
            Db.Set<ContractRecord>().Add(kontrak);
        }
        else
        {
            kontrak = (await Db.Set<ContractRecord>()
                .Include(c => c.Submission)
                .FirstOrDefaultAsync(c => c.Id == vm.Id))!;

            if (kontrak is null) return NotFound();
            if (kontrak.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();

            // Draf yang sudah dihantar tidak boleh disunting.
            if (kontrak.Submission.Status != SubmissionStatus.Draft) return Forbid();
        }

        SalinKeEntiti(vm, kontrak);
        await Db.SaveChangesAsync();

        TempData["Mesej"] = "Draf disimpan.";
        return RedirectToAction(nameof(Edit), new { id = kontrak.Id });
    }

    // ----- pembantu peribadi -----

    private static void SalinKeEntiti(ContractFormViewModel vm, ContractRecord kontrak)
    {
        kontrak.ContractNo = vm.ContractNo.Trim();
        kontrak.FileNo = vm.FileNo.Trim();
        kontrak.Title = vm.Title.Trim();
        kontrak.ContractType = vm.ContractType;
        kontrak.Amount = vm.Amount;
        kontrak.EffectiveDate = vm.EffectiveDate ?? default;
        kontrak.ExpiryDate = vm.ExpiryDate ?? default;
        kontrak.Division = vm.Division.Trim();
    }

    private static ContractFormViewModel KeViewModel(ContractRecord kontrak) => new()
    {
        Id = kontrak.Id,
        SubmissionId = kontrak.SubmissionId,
        ContractNo = kontrak.ContractNo,
        FileNo = kontrak.FileNo,
        Title = kontrak.Title,
        ContractType = kontrak.ContractType,
        Amount = kontrak.Amount,
        EffectiveDate = kontrak.EffectiveDate == default ? null : kontrak.EffectiveDate,
        ExpiryDate = kontrak.ExpiryDate == default ? null : kontrak.ExpiryDate,
        Division = kontrak.Division,
        IsEditable = kontrak.Submission?.Status == SubmissionStatus.Draft
    };
}
```

2. Perhatikan: kami **tidak** menulis `Approve` atau `Reject`. Ia diwarisi dari `SubmissionControllerBase`.

### ✅ Semakan

- [ ] Controller mewarisi `SubmissionControllerBase`
- [ ] `ModuleCode` = `ModuleCodes.PengurusanKontrak`; `AdminRole` = `"IctAdmin"`
- [ ] **Tiada** logik Approve/Reject ditulis dalam controller anda
- [ ] Semakan pemilikan: pemohon tidak boleh membuka draf orang lain
- [ ] `dotnet build` berjaya

---

## Latihan 7 — Razor view

**Objektif:** Borang header kontrak yang boleh diisi dan disimpan sebagai draf.

### Langkah

1. `Views/Contract/Form.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Kontrak.ContractFormViewModel
@using Nres.Onboarding.Web.Models.Kontrak
@{
    ViewData["Title"] = Model.Id is null
        ? "Daftar Kontrak Baharu" : "Sunting Pendaftaran Kontrak";
}

<h2>@ViewData["Title"]</h2>

@if (TempData["Mesej"] is string mesej)
{
    <div class="alert alert-success">@mesej</div>
}

@if (!Model.IsEditable)
{
    <div class="alert alert-secondary">
        Pendaftaran ini telah dihantar dan tidak boleh disunting lagi.
    </div>
}

<form asp-action="SaveDraft" method="post">
    @Html.AntiForgeryToken()
    <input type="hidden" asp-for="Id" />
    <input type="hidden" asp-for="SubmissionId" />

    <div asp-validation-summary="All" class="text-danger mb-3"></div>

    <fieldset disabled="@(!Model.IsEditable)">

        <div class="row g-3">
            <div class="col-md-6">
                <label asp-for="ContractNo" class="form-label"></label>
                <input asp-for="ContractNo" class="form-control"
                       placeholder="CT250000000029728" />
                <span asp-validation-for="ContractNo" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="FileNo" class="form-label"></label>
                <input asp-for="FileNo" class="form-control"
                       placeholder="NRES.400-5/6/40(S)-7" />
                <span asp-validation-for="FileNo" class="text-danger"></span>
            </div>
            <div class="col-12">
                <label asp-for="Title" class="form-label"></label>
                <input asp-for="Title" class="form-control"
                       placeholder="Perkhidmatan Storan & Sandaran Berpusat" />
                <span asp-validation-for="Title" class="text-danger"></span>
            </div>
            <div class="col-md-4">
                <label asp-for="ContractType" class="form-label"></label>
                <select asp-for="ContractType" class="form-select"
                        asp-items="Html.GetEnumSelectList<ContractType>()"></select>
            </div>
            <div class="col-md-4">
                <label asp-for="Amount" class="form-label"></label>
                <input asp-for="Amount" class="form-control" type="number" step="0.01" />
                <span asp-validation-for="Amount" class="text-danger"></span>
            </div>
            <div class="col-md-4">
                <label asp-for="Division" class="form-label"></label>
                <input asp-for="Division" class="form-control"
                       placeholder="Bahagian Pengurusan Maklumat" />
                <span asp-validation-for="Division" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="EffectiveDate" class="form-label"></label>
                <input asp-for="EffectiveDate" type="date" class="form-control" />
                <span asp-validation-for="EffectiveDate" class="text-danger"></span>
            </div>
            <div class="col-md-6">
                <label asp-for="ExpiryDate" class="form-label"></label>
                <input asp-for="ExpiryDate" type="date" class="form-control" />
                <span asp-validation-for="ExpiryDate" class="text-danger"></span>
            </div>
        </div>

        <div class="mt-4">
            <button type="submit" class="btn btn-secondary">Simpan Draf</button>
            <a asp-action="Index" class="btn btn-link">Kembali</a>
            @* Pihak terlibat, milestone & butang Hantar ditambah pada Hari 5–6. *@
        </div>

    </fieldset>
</form>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
}
```

2. `Views/Contract/Index.cshtml`:

```cshtml
@model IEnumerable<Nres.Onboarding.Web.Models.Shared.Submission>
@{ ViewData["Title"] = "Pendaftaran Kontrak Saya"; }

<div class="d-flex justify-content-between align-items-center">
    <h2>@ViewData["Title"]</h2>
    <a asp-action="Create" class="btn btn-primary">Daftar Kontrak</a>
</div>

<table class="table table-hover mt-3">
    <thead>
        <tr><th>No. Rujukan</th><th>Status</th><th>Dicipta</th><th></th></tr>
    </thead>
    <tbody>
    @if (!Model.Any())
    {
        <tr><td colspan="4" class="text-muted">Tiada pendaftaran lagi.</td></tr>
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

Log masuk sebagai `applicant@nres.test` / `Nres@2026!` → Pengurusan Kontrak → Daftar Kontrak → isi tajuk sahaja → Simpan Draf. Buka semula → lengkapkan medan → simpan.

### ✅ Semakan

- [ ] Borang dipaparkan dengan semua medan header kontrak
- [ ] Simpan draf berfungsi dengan medan tidak lengkap (tajuk sahaja)
- [ ] Draf muncul dalam senarai `Index` dengan lencana "(draf)"
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

Sahkan: hanya fail dalam folder `Kontrak`/`Contract` Kumpulan 1, **ditambah** satu baris dinyahkomen dalam `Program.cs`.

2. **Semakan AI** — prompt dari `docs/kumpulan-1/nota-ai.md`:

```text
Merujuk AGENTS.md dan KOLABORASI.md, semak diff ini:
1. Adakah ia menduplikasi apa-apa dalam daftar komponen kongsi?
2. Adakah ia menyentuh fail di luar folder modul Kontrak Kumpulan 1?
3. Adakah authorization dan validation pelayan lengkap?
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
- [ ] Hanya fail Kumpulan 1 (Kontrak) disentuh (+1 baris `Program.cs`)
- [ ] Validation di pelayan (semakan tajuk pada draf, semakan silang tarikh)
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
| Entiti `ContractRecord`, `ContractParty`, `ContractMilestone` + enum | `Models/Kontrak/` |
| Konfigurasi (1-1 Submission, 1-banyak anak, ketepatan wang) | `Models/Kontrak/Configurations/` |
| Pendaftaran modul + descriptor | `Services/Kontrak/`, `Models/Kontrak/` |
| Migration `KontrakContractSchema` | `Migrations/` |
| View model | `ViewModels/Kontrak/` |
| Controller | `Controllers/ContractController.cs` |
| View borang & senarai | `Views/Contract/` |

**Esok (Hari 5–6):** borang pihak terlibat, jadual milestone bayaran, jana nombor rujukan `KON-2026-####`, dan hantar dengan validation penuh (jumlah milestone = amaun kontrak).
