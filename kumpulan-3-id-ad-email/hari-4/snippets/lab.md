# Lab · Kumpulan 3 · Hari 4 — Skema Akaun & Akses

> Konsep: [`../README.md`](../README.md) · Kanun: [`../../../SPEC-KURSUS.md`](../../../SPEC-KURSUS.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula hari dengan betul

```bash
git switch kump-3/id-ad-email
git pull --rebase origin master
git switch -c kump-3/feat/skema-akaun-akses
dotnet build
```

**Semakan "sudah wujud?"**

```bash
grep -rn "ApprovalStep" Nres.Onboarding.Web/Models/Shared/
grep -rn "SupervisorApproved" Nres.Onboarding.Web/Models/Shared/
```

Kedua-duanya **sudah wujud** — `ApprovalStep` dengan `StepOrder`, dan status `SupervisorApproved`. Ia diletakkan dalam asas kongsi Hari 3 **untuk modul anda**.

**Prompt AI hari ini:**

```text
Merujuk AGENTS.md dan SPEC-KURSUS.md: saya Kumpulan 3, modul ID/AD/Email.
Modul saya memerlukan kelulusan DUA peringkat (Penyelia, kemudian ICT).
Adakah repo ini sudah ada cara memodelkan laluan kelulusan berbilang peringkat?
Jika ya, beritahu di mana. JANGAN tulis kod baharu.
```

> Jawapan betul: `ApprovalStep` dengan `StepOrder` dan `RoleRequired`. Jika AI mencadangkan menambah lajur `SupervisorDecision`/`IctDecision` ke jadual anda, tolak — dan fahami kenapa (README, bahagian "kenapa jadual dan bukan dua lajur").

### ✅ Semakan

- [ ] `dotnet build` berjaya
- [ ] Anda mengesahkan `ApprovalStep` dan `SupervisorApproved` sudah wujud
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Entiti `AccountRequest`

**Objektif:** Satu jadual permohonan, empat jenis — tanpa menyimpan kata laluan.

### Langkah

`Models/Akaun/AccountRequest.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Akaun;

public enum JenisPermohonanAkaun
{
    AkaunBaharu = 1,
    TukarAkses = 2,
    TukarMaklumat = 3,
    Nyahaktif = 4
}

/// <summary>
/// Permohonan akaun pengguna & akses sistem.
///
/// 🔒 KESELAMATAN: kelas ini TIDAK mengandungi medan kata laluan, dan tidak
/// akan pernah mengandunginya. Kata laluan ditetapkan dalam Active Directory
/// oleh ICT dan tidak pernah melalui sistem ini. Kita menyimpan FAKTA bahawa
/// akaun telah dicipta dan diserahkan — bukan kelayakannya.
/// </summary>
public class AccountRequest
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public JenisPermohonanAkaun Jenis { get; set; } = JenisPermohonanAkaun.AkaunBaharu;

    // --- Staf yang akaunnya dipohon (mungkin bukan pemohon) ---
    public string StaffName { get; set; } = string.Empty;
    public string StaffIdentityNo { get; set; } = string.Empty;

    public int? DepartmentId { get; set; }
    public LookupDepartment? Department { get; set; }

    public int? PositionId { get; set; }
    public LookupPosition? Position { get; set; }

    /// <summary>Penyelia yang meluluskan peringkat 1. Id pengguna Identity.</summary>
    public string SupervisorUserId { get; set; } = string.Empty;

    /// <summary>Justifikasi permohonan — wajib untuk semua jenis.</summary>
    public string Justifikasi { get; set; } = string.Empty;

    // --- Khusus jenis ---
    /// <summary>AkaunBaharu: tarikh staf mula bertugas.</summary>
    public DateTime? TarikhMula { get; set; }

    /// <summary>Nyahaktif: tarikh akhir perkhidmatan.</summary>
    public DateTime? TarikhTamat { get; set; }

    /// <summary>TukarMaklumat: apa yang berubah.</summary>
    public string? ButiranPerubahan { get; set; }

    // --- Diisi oleh ICT semasa pemprosesan (Hari 7–9) ---
    /// <summary>Nama akaun AD yang dicipta, cth. "ahmad.zulkifli".</summary>
    public string? AdAccountName { get; set; }

    /// <summary>E-mel rasmi yang diberikan.</summary>
    public string? OfficialEmail { get; set; }

    /// <summary>
    /// Fakta bahawa kelayakan telah diserahkan kepada staf — BUKAN kelayakan itu.
    /// </summary>
    public bool KelayakanDiserahkan { get; set; }
    public DateTime? TarikhSerahan { get; set; }
    public string? CatatanIct { get; set; }

    public ICollection<RequestedSystemAccess> AccessRequests { get; set; } = [];
}
```

> **Perhatikan `= []`** — collection expression (C# 12/13). Lebih ringkas daripada `new List<...>()`.

### ✅ Semakan

- [ ] Fail dalam `Models/Akaun/`
- [ ] **Sifar** medan kata laluan — semak semula
- [ ] Sifar medan diduplikasi dari `Submission`
- [ ] Komen keselamatan ada dan diterangkan dalam kumpulan anda

---

## Latihan 2 — Akses sistem (banyak-ke-banyak eksplisit)

**Objektif:** Satu permohonan, banyak akses — setiap satu boleh diluluskan secara berasingan.

### Langkah

1. `Models/Akaun/LookupSystemAccess.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Akaun;

public enum KategoriAkses
{
    Infrastruktur = 1,   // AD, e-mel, VPN
    FolderKongsi = 2,
    SistemDalaman = 3
}

/// <summary>Katalog sistem/akses yang boleh dipohon. Data rujukan berseed.</summary>
public class LookupSystemAccess
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public KategoriAkses Kategori { get; set; }

    /// <summary>Adakah akses ini memerlukan justifikasi tambahan?</summary>
    public bool PerluJustifikasi { get; set; }

    public bool IsActive { get; set; } = true;
}
```

2. `Models/Akaun/RequestedSystemAccess.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Akaun;

public enum TahapAkses
{
    BacaSahaja = 1,
    BacaTulis = 2,
    Pentadbir = 3
}

/// <summary>
/// Jadual penghubung EKSPLISIT antara permohonan dan akses sistem.
///
/// Kami tidak menggunakan many-to-many tersirat EF Core kerana setiap baris
/// membawa datanya sendiri — terutamanya `Diluluskan`, yang membolehkan ICT
/// meluluskan SEBAHAGIAN akses dan menolak yang lain.
/// </summary>
public class RequestedSystemAccess
{
    public int Id { get; set; }

    public int AccountRequestId { get; set; }
    public AccountRequest? AccountRequest { get; set; }

    public int SystemAccessId { get; set; }
    public LookupSystemAccess? SystemAccess { get; set; }

    public TahapAkses Tahap { get; set; } = TahapAkses.BacaSahaja;

    /// <summary>Kenapa akses INI diperlukan.</summary>
    public string? Justifikasi { get; set; }

    /// <summary>
    /// null = belum diputuskan · true = diluluskan · false = ditolak.
    /// Membolehkan kelulusan separa — 3 daripada 5 akses.
    /// </summary>
    public bool? Diluluskan { get; set; }

    /// <summary>Sebab akses ini ditolak, jika ditolak.</summary>
    public string? CatatanIct { get; set; }
}
```

> **`bool?` dan bukan `bool`** — tiga keadaan diperlukan: belum diputuskan, diluluskan, ditolak. `bool` sahaja tidak boleh menyatakan "belum diputuskan".

### ✅ Semakan

- [ ] Kedua-dua fail dalam `Models/Akaun/`
- [ ] `Diluluskan` ialah `bool?`, bukan `bool`
- [ ] Anda boleh menerangkan kenapa jadual penghubung eksplisit digunakan

---

## Latihan 3 — Konfigurasi EF Core

**Objektif:** Daftar entiti **tanpa menyentuh `ApplicationDbContext`**.

### Langkah

`Models/Akaun/Configurations/AkaunConfigurations.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Akaun.Configurations;

public class AccountRequestConfiguration : IEntityTypeConfiguration<AccountRequest>
{
    public void Configure(EntityTypeBuilder<AccountRequest> builder)
    {
        builder.ToTable("AccountRequests");

        builder.Property(a => a.StaffName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.StaffIdentityNo).HasMaxLength(20).IsRequired();
        builder.Property(a => a.SupervisorUserId).HasMaxLength(450).IsRequired();
        builder.Property(a => a.Justifikasi).HasMaxLength(1000).IsRequired();
        builder.Property(a => a.ButiranPerubahan).HasMaxLength(1000);
        builder.Property(a => a.AdAccountName).HasMaxLength(100);
        builder.Property(a => a.OfficialEmail).HasMaxLength(200);
        builder.Property(a => a.CatatanIct).HasMaxLength(1000);
        builder.Property(a => a.Jenis).HasConversion<int>();

        builder.HasIndex(a => a.SubmissionId).IsUnique();
        builder.HasOne(a => a.Submission).WithOne()
            .HasForeignKey<AccountRequest>(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Department).WithMany()
            .HasForeignKey(a => a.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(a => a.Position).WithMany()
            .HasForeignKey(a => a.PositionId).OnDelete(DeleteBehavior.Restrict);

        // Nama akaun AD mesti unik apabila diberikan (null semasa draf).
        builder.HasIndex(a => a.AdAccountName)
            .IsUnique()
            .HasFilter("\"AdAccountName\" IS NOT NULL");

        // E-mel rasmi juga mesti unik.
        builder.HasIndex(a => a.OfficialEmail)
            .IsUnique()
            .HasFilter("\"OfficialEmail\" IS NOT NULL");

        // Baris gilir penyelia: "permohonan menunggu kelulusan saya".
        builder.HasIndex(a => a.SupervisorUserId);
    }
}

public class RequestedSystemAccessConfiguration
    : IEntityTypeConfiguration<RequestedSystemAccess>
{
    public void Configure(EntityTypeBuilder<RequestedSystemAccess> builder)
    {
        builder.ToTable("RequestedSystemAccesses");

        builder.Property(r => r.Justifikasi).HasMaxLength(500);
        builder.Property(r => r.CatatanIct).HasMaxLength(500);
        builder.Property(r => r.Tahap).HasConversion<int>();

        builder.HasOne(r => r.AccountRequest)
            .WithMany(a => a.AccessRequests)
            .HasForeignKey(r => r.AccountRequestId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.SystemAccess).WithMany()
            .HasForeignKey(r => r.SystemAccessId)
            .OnDelete(DeleteBehavior.Restrict);

        // Satu akses tidak boleh dipohon dua kali dalam permohonan yang sama.
        builder.HasIndex(r => new { r.AccountRequestId, r.SystemAccessId }).IsUnique();
    }
}

public class LookupSystemAccessConfiguration : IEntityTypeConfiguration<LookupSystemAccess>
{
    public void Configure(EntityTypeBuilder<LookupSystemAccess> builder)
    {
        builder.ToTable("LookupSystemAccesses");

        builder.Property(l => l.Code).HasMaxLength(30).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(150).IsRequired();
        builder.Property(l => l.Kategori).HasConversion<int>();
        builder.HasIndex(l => l.Code).IsUnique();

        // Data SINTETIK untuk latihan.
        builder.HasData(
            new LookupSystemAccess { Id = 1, Code = "AD",       Name = "Akaun Active Directory",  Kategori = KategoriAkses.Infrastruktur },
            new LookupSystemAccess { Id = 2, Code = "EMAIL",    Name = "E-mel Rasmi NRES",        Kategori = KategoriAkses.Infrastruktur },
            new LookupSystemAccess { Id = 3, Code = "VPN",      Name = "Akses VPN",               Kategori = KategoriAkses.Infrastruktur, PerluJustifikasi = true },
            new LookupSystemAccess { Id = 4, Code = "SHARE-BPM",Name = "Folder Kongsi BPM",       Kategori = KategoriAkses.FolderKongsi },
            new LookupSystemAccess { Id = 5, Code = "SHARE-KEW",Name = "Folder Kongsi Kewangan",  Kategori = KategoriAkses.FolderKongsi,  PerluJustifikasi = true },
            new LookupSystemAccess { Id = 6, Code = "HRMIS",    Name = "Sistem HRMIS",            Kategori = KategoriAkses.SistemDalaman },
            new LookupSystemAccess { Id = 7, Code = "EPEROLEHAN",Name = "Sistem ePerolehan",      Kategori = KategoriAkses.SistemDalaman, PerluJustifikasi = true },
            new LookupSystemAccess { Id = 8, Code = "ONBOARD",  Name = "Sistem Onboarding NRES",  Kategori = KategoriAkses.SistemDalaman });
    }
}
```

> **Perhatikan indeks unik ditapis pada `AdAccountName` dan `OfficialEmail`.** Dua staf tidak boleh mendapat nama akaun yang sama — dan pangkalan data menguatkuasakannya, bukan hanya kod anda.

Sahkan:

```bash
git diff --name-only master     # Data/ApplicationDbContext.cs TIDAK sepatutnya muncul
```

### ✅ Semakan

- [ ] Konfigurasi dalam `Models/Akaun/Configurations/`
- [ ] Indeks unik ditapis pada nama AD & e-mel
- [ ] Lapan jenis akses berseed
- [ ] `git diff` menunjukkan tiada fail kongsi

---

## Latihan 4 — Pendaftaran modul & navigasi

### Langkah

1. `Models/Akaun/AkaunModuleDescriptor.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Akaun;

public class AkaunModuleDescriptor : IModuleDescriptorProvider
{
    public ModuleDescriptor Describe() => new(
        Code: ModuleCodes.IdAdEmail,
        Nama: "ID, AD & Email",
        Controller: "AccountRequest",
        Ikon: "bi-person-badge",
        // Supervisor DISENARAIKAN — modul kami satu-satunya dengan kelulusan
        // peringkat 1 oleh penyelia.
        Roles: ["Applicant", "Supervisor", "IctAdmin", "SystemAdmin"],
        Urutan: 3);
}
```

2. `Services/Akaun/AkaunModule.cs`:

```csharp
using Nres.Onboarding.Web.Models.Akaun;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Akaun;

public static class AkaunModule
{
    public static IServiceCollection AddAkaunModule(this IServiceCollection services)
    {
        services.AddScoped<IModuleDescriptorProvider, AkaunModuleDescriptor>();
        return services;
    }
}
```

3. Nyahkomen **satu baris** dalam `Program.cs` (beritahu jurulatih dahulu):

```csharp
using Nres.Onboarding.Web.Services.Akaun;

builder.Services.AddAkaunModule();       // Kumpulan 3   ← nyahkomen INI sahaja
```

### ✅ Semakan

- [ ] Descriptor & modul dalam folder anda
- [ ] `Supervisor` disenaraikan dalam `Roles`
- [ ] Tepat satu baris dinyahkomen dalam `Program.cs`

---

## Latihan 5 — Migration (slot!)

### Langkah

1. Umumkan: *"Kumpulan 3 mengambil slot migration."*

2. ```bash
   git pull --rebase origin master
   cd Nres.Onboarding.Web
   dotnet ef migrations add AkaunPermohonanDanAkses
   ```

3. **Baca fail yang dijana.** Sahkan ia mencipta `AccountRequests`, `RequestedSystemAccesses`, `LookupSystemAccesses` — dan tiada jadual kumpulan lain.

4. ```bash
   dotnet ef database update
   dotnet run
   cd ..
   ```

5. Commit, push, lepaskan slot.

### ✅ Semakan

- [ ] Slot diumumkan & dilepaskan
- [ ] Tiga jadual dicipta; lapan baris akses berseed
- [ ] "ID, AD & Email" muncul dalam navigasi

---

## Latihan 6 — Servis laluan kelulusan

**Objektif:** Cipta laluan dua peringkat pada penghantaran — corak yang menakrifkan modul anda.

### Langkah

1. `Services/Akaun/IApprovalRouteService.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Akaun;

public interface IApprovalRouteService
{
    /// <summary>
    /// Cipta laluan kelulusan dua peringkat untuk permohonan.
    /// Dipanggil pada PENGHANTARAN, bukan pada cipta draf — penyelia
    /// mungkin berubah semasa draf masih disunting.
    /// </summary>
    Task CreateRouteAsync(int submissionId, string supervisorUserId,
        CancellationToken ct = default);

    /// <summary>Langkah yang sedang menunggu keputusan, atau null jika selesai.</summary>
    Task<ApprovalStep?> CurrentStepAsync(int submissionId, CancellationToken ct = default);

    /// <summary>Rekod keputusan pada langkah tertentu.</summary>
    Task DecideAsync(int submissionId, int stepOrder, ApprovalDecision keputusan,
        string decidedByUserId, string? remarks, CancellationToken ct = default);
}
```

2. `Services/Akaun/ApprovalRouteService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Akaun;

public class ApprovalRouteService(ApplicationDbContext db) : IApprovalRouteService
{
    public async Task CreateRouteAsync(int submissionId, string supervisorUserId,
        CancellationToken ct = default)
    {
        // Idempoten: jangan cipta dua kali jika dipanggil semula.
        var sudahAda = await db.ApprovalSteps
            .AnyAsync(s => s.SubmissionId == submissionId, ct);
        if (sudahAda) return;

        db.ApprovalSteps.AddRange(
            new ApprovalStep
            {
                SubmissionId = submissionId,
                StepOrder = 1,
                RoleRequired = "Supervisor",
                Decision = ApprovalDecision.Pending
            },
            new ApprovalStep
            {
                SubmissionId = submissionId,
                StepOrder = 2,
                RoleRequired = "IctAdmin",
                Decision = ApprovalDecision.Pending
            });

        await db.SaveChangesAsync(ct);
    }

    public async Task<ApprovalStep?> CurrentStepAsync(
        int submissionId, CancellationToken ct = default) =>
        await db.ApprovalSteps
            .Where(s => s.SubmissionId == submissionId
                     && s.Decision == ApprovalDecision.Pending)
            .OrderBy(s => s.StepOrder)
            .FirstOrDefaultAsync(ct);

    public async Task DecideAsync(int submissionId, int stepOrder,
        ApprovalDecision keputusan, string decidedByUserId, string? remarks,
        CancellationToken ct = default)
    {
        var step = await db.ApprovalSteps
            .FirstOrDefaultAsync(s => s.SubmissionId == submissionId
                                   && s.StepOrder == stepOrder, ct)
            ?? throw new InvalidOperationException(
                $"Langkah kelulusan {stepOrder} tidak dijumpai.");

        if (step.Decision != ApprovalDecision.Pending)
            throw new InvalidOperationException(
                $"Langkah {stepOrder} telah pun diputuskan.");

        // Langkah mesti diputuskan mengikut TURUTAN — ICT tidak boleh
        // meluluskan sebelum Penyelia.
        var langkahTerdahulu = await db.ApprovalSteps
            .Where(s => s.SubmissionId == submissionId && s.StepOrder < stepOrder)
            .ToListAsync(ct);

        if (langkahTerdahulu.Any(s => s.Decision != ApprovalDecision.Approved))
            throw new InvalidOperationException(
                "Langkah terdahulu belum diluluskan.");

        step.Decision = keputusan;
        step.DecidedByUserId = decidedByUserId;
        step.DecidedAt = DateTime.UtcNow;
        step.Remarks = remarks;

        await db.SaveChangesAsync(ct);
    }
}
```

3. Daftar dalam `AkaunModule`:

```csharp
services.AddScoped<IApprovalRouteService, ApprovalRouteService>();
```

> **Semakan turutan dalam `DecideAsync`** ialah bahagian paling penting. Tanpanya, `IctAdmin` boleh meluluskan permohonan yang penyelia belum lihat — memintas keseluruhan tujuan kelulusan dua peringkat.

### ✅ Semakan

- [ ] Servis dalam `Services/Akaun/`
- [ ] `CreateRouteAsync` idempoten
- [ ] `DecideAsync` menguatkuasakan turutan langkah
- [ ] Melontar jika langkah sudah diputuskan
- [ ] Didaftar dalam `AkaunModule`

---

## Latihan 7 — Halaman utama modul

**Objektif:** Landing dengan empat jenis permohonan dan permohonan saya.

### Langkah

1. `Controllers/AccountRequestController.cs` — permulaan:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Akaun;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels.Akaun;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class AccountRequestController(
    ApplicationDbContext db,
    IWorkflowService workflow,
    INotificationService notifications,
    ICurrentUserService currentUser)
    : SubmissionControllerBase(db, workflow, notifications)
{
    protected override string ModuleCode => ModuleCodes.IdAdEmail;

    // Peringkat 2 kami ialah ICT. Peringkat 1 (Supervisor) dikendalikan
    // oleh tindakan berasingan — lihat Hari 5–6.
    protected override string AdminRole => "IctAdmin";

    public async Task<IActionResult> Index()
    {
        var userId = currentUser.UserId!;

        var senarai = await Db.Submissions.AsNoTracking()
            .Where(s => s.ModuleCode == ModuleCode && s.ApplicantUserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(20)
            .ToListAsync();

        var ids = senarai.Select(s => s.Id).ToList();

        var detail = await Db.Set<AccountRequest>().AsNoTracking()
            .Where(a => ids.Contains(a.SubmissionId))
            .ToDictionaryAsync(a => a.SubmissionId, a => new { a.Id, a.Jenis, a.StaffName });

        var vm = new AkaunIndexViewModel
        {
            Permohonan = senarai.Select(s =>
            {
                var d = detail.GetValueOrDefault(s.Id);
                return new AkaunIndexViewModel.PermohonanRingkas(
                    s.Id, d?.Id ?? 0, s.ReferenceNo,
                    d?.Jenis ?? JenisPermohonanAkaun.AkaunBaharu,
                    d?.StaffName ?? "—", s.Status, s.CreatedAt);
            }).ToList()
        };

        return View(vm);
    }
}
```

2. `ViewModels/Akaun/AkaunIndexViewModel.cs`:

```csharp
using Nres.Onboarding.Web.Models.Akaun;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Akaun;

public class AkaunIndexViewModel
{
    public IReadOnlyList<PermohonanRingkas> Permohonan { get; set; } = [];

    public record PermohonanRingkas(
        int SubmissionId, int ApplicationId, string ReferenceNo,
        JenisPermohonanAkaun Jenis, string StaffName,
        SubmissionStatus Status, DateTime CreatedAt);
}
```

3. `Views/Akaun/Index.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Akaun.AkaunIndexViewModel
@using Nres.Onboarding.Web.Models.Akaun
@{ ViewData["Title"] = "ID, AD & Email"; }

<h2>@ViewData["Title"]</h2>
<p class="text-muted">
    Permohonan akaun pengguna dan akses sistem.
    Setiap permohonan melalui kelulusan <strong>dua peringkat</strong>:
    Penyelia Jabatan, kemudian Pentadbir ICT.
</p>

<div class="row g-3 my-4">
    <div class="col-md-3">
        <div class="card h-100"><div class="card-body">
            <h6 class="card-title">Akaun Baharu</h6>
            <p class="card-text small text-muted">AD, e-mel dan akses untuk staf baharu.</p>
            <a asp-action="Create" asp-route-jenis="@((int)JenisPermohonanAkaun.AkaunBaharu)"
               class="btn btn-sm btn-primary">Mohon</a>
        </div></div>
    </div>
    <div class="col-md-3">
        <div class="card h-100"><div class="card-body">
            <h6 class="card-title">Tukar Akses</h6>
            <p class="card-text small text-muted">Tambah atau ubah akses sistem sedia ada.</p>
            <a asp-action="Create" asp-route-jenis="@((int)JenisPermohonanAkaun.TukarAkses)"
               class="btn btn-sm btn-primary">Mohon</a>
        </div></div>
    </div>
    <div class="col-md-3">
        <div class="card h-100"><div class="card-body">
            <h6 class="card-title">Tukar Maklumat</h6>
            <p class="card-text small text-muted">Kemas kini nama, jabatan atau jawatan.</p>
            <a asp-action="Create" asp-route-jenis="@((int)JenisPermohonanAkaun.TukarMaklumat)"
               class="btn btn-sm btn-primary">Mohon</a>
        </div></div>
    </div>
    <div class="col-md-3">
        <div class="card h-100"><div class="card-body">
            <h6 class="card-title">Nyahaktif</h6>
            <p class="card-text small text-muted">Tutup akaun staf yang berhenti/bertukar.</p>
            <a asp-action="Create" asp-route-jenis="@((int)JenisPermohonanAkaun.Nyahaktif)"
               class="btn btn-sm btn-primary">Mohon</a>
        </div></div>
    </div>
</div>

<h5 class="mt-4">Permohonan Saya</h5>
<table class="table table-hover">
    <thead>
        <tr><th>No. Rujukan</th><th>Jenis</th><th>Staf</th><th>Status</th><th>Tarikh</th><th></th></tr>
    </thead>
    <tbody>
    @if (!Model.Permohonan.Any())
    {
        <tr><td colspan="6" class="text-muted">Tiada permohonan lagi.</td></tr>
    }
    @foreach (var p in Model.Permohonan)
    {
        <tr>
            <td>@(string.IsNullOrEmpty(p.ReferenceNo) ? "(draf)" : p.ReferenceNo)</td>
            <td>@p.Jenis</td>
            <td>@p.StaffName</td>
            <td><partial name="_StatusBadge" model="p.Status" /></td>
            <td>@p.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy")</td>
            <td class="text-end">
                <a asp-action="Edit" asp-route-id="@p.ApplicationId"
                   class="btn btn-sm btn-outline-primary">Buka</a>
            </td>
        </tr>
    }
    </tbody>
</table>
```

> Pautan `Create`/`Edit` menunjuk kepada tindakan yang anda bina pada **Hari 5–6** — 404 hari ini, itu dijangka.

### ✅ Semakan

- [ ] Halaman utama memaparkan empat jenis permohonan
- [ ] Guna `_StatusBadge` **kongsi**
- [ ] Modul boleh dicapai daripada navigasi

---

## Latihan 8 — Tutup hari

```bash
git diff --name-only master     # hanya fail Akaun + 1 baris Program.cs
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Binaan bersih; halaman utama modul berfungsi
- [ ] **Sifar medan kata laluan** dalam mana-mana entiti
- [ ] `ApprovalStep` kongsi digunakan — bukan lajur keputusan sendiri
- [ ] Hanya fail Kumpulan 3 disentuh (+1 baris `Program.cs`)
- [ ] Migration melalui slot
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 4

| Artifak | Lokasi |
|---------|--------|
| `AccountRequest`, `RequestedSystemAccess`, `LookupSystemAccess` | `Models/Akaun/` |
| Konfigurasi + seed 8 jenis akses | `Models/Akaun/Configurations/` |
| Pendaftaran modul + descriptor | `Services/Akaun/`, `Models/Akaun/` |
| Migration `AkaunPermohonanDanAkses` | `Migrations/` |
| `IApprovalRouteService` (2 peringkat) | `Services/Akaun/` |
| Halaman utama modul | `Controllers/AccountRequestController.cs`, `Views/Akaun/Index.cshtml` |

**Seterusnya (Hari 5–6):** borang permohonan akaun dan **skrin kelulusan Penyelia peringkat 1**.
