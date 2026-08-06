# Lab Hari 3 — Refresher .NET & Asas Kongsi

> ⚠️ **Nota migrasi (poly-repo):** lab ini masih mengikut model **lama (monorepo)** — ia membina **satu** `ApplicationDbContext` + `Models/Shared` + migration `InitialShared`, kemudian gabung ke `master`. Dalam seni bina **poly-repo** terkini, Hari 3 = **terbitkan kontrak Profile DB (repo `profile`) + setiap pasukan scaffold repo sendiri** (setiap sistem ada `Submission`/`Attachment`/`AuditLog` sendiri; **hanya Profile DB dikongsi**). Lab ini akan **ditulis semula**. **Kanun muktamad:** [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md) · [`../../AGENTS.md`](../../AGENTS.md) · [`../../KOLABORASI.md`](../../KOLABORASI.md).

> Konsep di [`../README.md`](../README.md). Kanun: [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md). Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md).
>
> **Semua kumpulan bekerja bersama hari ini pada cabang yang sama: `asas/shared-foundation`.** Semua yang dibina hari ini dikongsi. Esok anda bercabang.

## Persediaan

- .NET 10 SDK (`dotnet --version` → `10.x`)
- `dotnet ef` (`dotnet ef --version`)
- Repo kursus di-clone, pada `master` yang terkini

```bash
git switch master
git pull --rebase origin master
git switch -c asas/shared-foundation
```

> **Cara kelas bekerja hari ini:** jurulatih memandu di skrin; setiap peserta menaip pada mesinnya sendiri. Satu peserta setiap kumpulan dilantik untuk push ke `asas/shared-foundation` di penghujung setiap latihan besar supaya semua orang kekal segerak.

---

## Pemanasan — C# tanpa projek (10 minit)

**Objektif:** Sahkan C# 14 aktif, dan ulangkaji LINQ + `async` tanpa overhead projek.

> Ciri **file-based apps** ialah baharu dalam C# 14: satu fail `.cs`, tiada `.csproj`, `dotnet run` terus. Sempurna untuk mencuba idea.

### Langkah

1. Sahkan versi SDK dan bahasa:

```bash
dotnet --version        # 10.x
```

2. Cipta `pemanasan.cs` **di luar** repo (cth. dalam `~/latihan/`):

```csharp
// Tiada .csproj. Tiada namespace. Tiada kelas Program.
// Ini "file-based app" — ciri C# 14.

// --- Rekod ringkas untuk data contoh ---
record Permohonan(string Rujukan, string Modul, string Status, int Hari);

Permohonan[] permohonan =
[
    new("LD-2026-0001",  "LD",     "Submitted",     3),
    new("LD-2026-0002",  "LD",     "AdminApproved", 7),
    new("PAS-2026-0001", "PAS",    "Submitted",     1),
    new("PAS-2026-0002", "PAS",    "Rejected",      5),
    new("ICT-ID-2026-0001", "ICT-ID", "Submitted", 12),
];

// --- LINQ: menapis, mengumpul, mengagregat ---
Console.WriteLine("== Menunggu semakan, paling lama dahulu ==");
foreach (var p in permohonan
    .Where(p => p.Status == "Submitted")
    .OrderByDescending(p => p.Hari))
{
    Console.WriteLine($"  {p.Rujukan,-20} {p.Hari} hari");
}

Console.WriteLine("\n== Kiraan mengikut modul ==");
foreach (var kumpulan in permohonan.GroupBy(p => p.Modul))
{
    Console.WriteLine($"  {kumpulan.Key,-8} {kumpulan.Count()}");
}

Console.WriteLine($"\nPurata umur permohonan: {permohonan.Average(p => p.Hari):0.0} hari");

// --- async/await ---
Console.WriteLine("\n== Async ==");
var hasil = await MuatDataAsync();
Console.WriteLine($"  {hasil}");

async Task<string> MuatDataAsync()
{
    await Task.Delay(200);          // simulasi panggilan pangkalan data
    return "Data dimuat.";
}
```

3. Jalankan:

```bash
dotnet run pemanasan.cs
```

4. **Cuba ciri C# 14 sendiri.** Tambah ke fail dan jalankan semula:

```csharp
// --- field keyword (C# 14) ---
Console.WriteLine("\n== field keyword ==");
var borang = new BorangRingkas();
borang.Catatan = "   ada ruang di hujung   ";
Console.WriteLine($"  '{borang.Catatan}'");   // dipangkas oleh setter

class BorangRingkas
{
    // Tiada medan sokongan dinamakan — pengkompil menciptanya.
    public string Catatan
    {
        get => field;
        set => field = value.Trim();
    } = string.Empty;
}
```

```csharp
// --- Null-conditional assignment (C# 14) ---
Console.WriteLine("\n== Null-conditional assignment ==");
Kotak? kotakNull = null;
var kotakSah = new Kotak();

kotakNull?.Nilai = "tidak akan dijalankan";   // selamat, tiada pengecualian
kotakSah?.Nilai = "ditetapkan";

Console.WriteLine($"  kotakSah.Nilai = {kotakSah.Nilai}");

class Kotak { public string? Nilai { get; set; } }
```

5. **Perbincangan (5 minit):**
   - Apa yang berlaku jika anda menukar `.Where()` dan `.OrderByDescending()`? *(Tiada apa — LINQ malas sehingga dijalankan)*
   - Kenapa `kotakNull?.Nilai = ...` tidak melontar? *(Sebelah kanan tidak dinilai langsung)*
   - Bilakah anda **tidak** patut guna `field`? *(Entiti EF Core — lihat amaran dalam README)*

### ✅ Semakan

- [ ] `dotnet --version` → `10.x`
- [ ] `dotnet run pemanasan.cs` berjalan **tanpa** `.csproj`
- [ ] Output LINQ, async, `field`, dan null-conditional assignment betul
- [ ] Anda boleh menyatakan bila **tidak** menggunakan `field`

> Buang `pemanasan.cs` selepas ini — ia bukan sebahagian projek kursus.

---

## Latihan 0 — Cipta projek & sahkan ia berjalan

**Objektif:** Aplikasi ASP.NET Core MVC yang berjalan.

### Langkah

1. Cipta projek dalam root repo:

```bash
dotnet new mvc -o Nres.Onboarding.Web
cd Nres.Onboarding.Web
```

2. Jalankannya:

```bash
dotnet run
```

Buka URL yang dipaparkan (biasanya `https://localhost:7xxx`). Anda sepatutnya melihat halaman selamat datang lalai. Tekan `Ctrl+C` untuk henti.

3. Periksa struktur yang dijana:

```text
Nres.Onboarding.Web/
  Controllers/HomeController.cs
  Models/ErrorViewModel.cs
  Views/
  wwwroot/
  Program.cs
  appsettings.json
  Nres.Onboarding.Web.csproj
```

4. Cipta struktur folder muktamad kita (rujuk `SPEC-KURSUS.md`):

```bash
mkdir -p Models/Shared Models/LaporDiri Models/Akses Models/Akaun Models/Aset
mkdir -p Models/Shared/Configurations
mkdir -p ViewModels Services Data
mkdir -p App_Data/uploads
touch App_Data/uploads/.gitkeep
```

5. Sahkan `.gitignore` melindungi kita:

```bash
cd ..
grep -E "App_Data|\.db|bin/|obj/" .gitignore
```

Jika tiada, tambah:

```gitignore
[Bb]in/
[Oo]bj/
*.db
*.db-shm
*.db-wal
App_Data/uploads/*
!App_Data/uploads/.gitkeep
.vs/
```

### ✅ Semakan

- [ ] `dotnet run` memaparkan halaman selamat datang
- [ ] Folder muktamad wujud
- [ ] `git status` **tidak** menunjukkan `bin/`, `obj/`, atau `*.db`

---

## Latihan 1 — Pakej NuGet

**Objektif:** Tambah EF Core, SQLite, dan Identity.

### Langkah

```bash
cd Nres.Onboarding.Web
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

| Pakej | Kegunaan |
|-------|----------|
| `EntityFrameworkCore` | Teras ORM |
| `EntityFrameworkCore.Design` | Sokongan `dotnet ef` (migration) |
| `EntityFrameworkCore.Sqlite` | Penyedia SQLite |
| `Identity.EntityFrameworkCore` | Identity + storan EF Core |

Sahkan:

```bash
dotnet build
grep PackageReference Nres.Onboarding.Web.csproj
```

### ✅ Semakan

- [ ] Empat `PackageReference` wujud
- [ ] `dotnet build` berjaya tanpa amaran

---

## Latihan 2 — Entiti kongsi

**Objektif:** Tulis entiti yang keempat-empat modul kongsi.

> Semua fail dalam latihan ini masuk ke `Models/Shared/`. Selepas hari ini, folder itu **beku**.

### Langkah

1. `Models/Shared/SubmissionStatus.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Satu enum status untuk SEMUA modul. Ini yang membolehkan satu dashboard,
/// satu panel audit, dan satu carian global melayan keempat-empat modul.
/// Nilai integer ditetapkan secara eksplisit — jangan susun semula.
/// </summary>
public enum SubmissionStatus
{
    Draft = 0,
    Submitted = 1,
    SupervisorApproved = 2,
    AdminApproved = 3,
    Rejected = 4,
    Completed = 5,
    Cancelled = 6
}
```

2. `Models/Shared/ModuleCodes.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>Prefix modul — rujuk SPEC-KURSUS.md. Jangan cipta variasi.</summary>
public static class ModuleCodes
{
    public const string LaporDiri = "LD";
    public const string PematuhanPks = "PKS";            // Kumpulan 1
    public const string PengurusanKontrak = "KON";       // Kumpulan 1
    public const string PasKeselamatan = "PAS";
    public const string Parkir = "PKR";
    public const string PelekatKenderaan = "STK";
    public const string IdAdEmail = "ICT-ID";
    public const string TempahanFasilitiSukan = "TFS";   // Kumpulan 4
}
```

3. `Models/Shared/Submission.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Rekod induk bagi SETIAP permohonan, tidak kira modul. Data khusus modul
/// tinggal dalam jadual detail sendiri yang menunjuk ke sini melalui SubmissionId.
///
/// JANGAN pendua ReferenceNo, Status, ApplicantUserId, atau tarikh ke dalam
/// jadual modul anda — dua salinan bermakna dua sumber kebenaran.
/// </summary>
public class Submission
{
    public int Id { get; set; }

    /// <summary>Contoh: LD-2026-0001. Kosong sehingga dihantar.</summary>
    public string ReferenceNo { get; set; } = string.Empty;

    /// <summary>Prefix modul — lihat <see cref="ModuleCodes"/>.</summary>
    public string ModuleCode { get; set; } = string.Empty;

    /// <summary>
    /// Id pengguna Identity. Sengaja string biasa tanpa kunci asing supaya
    /// jadual aliran kerja tidak terikat kepada skema authentication.
    /// </summary>
    public string ApplicantUserId { get; set; } = string.Empty;

    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
    public ICollection<ApprovalStep> ApprovalSteps { get; set; } = new List<ApprovalStep>();
}
```

4. `Models/Shared/Attachment.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// METADATA fail sahaja. Kandungan fail sebenar tinggal di
/// App_Data/uploads/{SubmissionId}/ — di LUAR wwwroot, supaya ia tidak
/// boleh dicapai tanpa melalui semakan kebenaran kita.
/// </summary>
public class Attachment
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    /// <summary>Nama yang pengguna muat naik — untuk paparan sahaja.</summary>
    public string OriginalFileName { get; set; } = string.Empty;

    /// <summary>Nama dijana (GUID) di cakera — tidak pernah nama pengguna.</summary>
    public string StoredFileName { get; set; } = string.Empty;

    public string ContentType { get; set; } = string.Empty;
    public long SizeBytes { get; set; }
    public string UploadedByUserId { get; set; } = string.Empty;
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
}
```

5. `Models/Shared/AuditLog.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>Satu baris setiap tindakan bermakna terhadap satu Submission.</summary>
public class AuditLog
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    /// <summary>Contoh: "Created", "Submitted", "Approved", "Rejected".</summary>
    public string Action { get; set; } = string.Empty;

    public string ActorUserId { get; set; } = string.Empty;
    public SubmissionStatus? FromStatus { get; set; }
    public SubmissionStatus? ToStatus { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
```

6. `Models/Shared/ApprovalStep.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Shared;

public enum ApprovalDecision { Pending = 0, Approved = 1, Rejected = 2 }

/// <summary>
/// Satu baris setiap kedudukan dalam laluan kelulusan. Modul satu-peringkat
/// (Kumpulan 1, 2, 4) menggunakan satu langkah; Kumpulan 3 menggunakan dua
/// (Penyelia, kemudian ICT).
/// </summary>
public class ApprovalStep
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    /// <summary>1-based. Unik setiap submission.</summary>
    public int StepOrder { get; set; }

    /// <summary>Peranan yang boleh memutuskan langkah ini, cth. "HrAdmin".</summary>
    public string RoleRequired { get; set; } = string.Empty;

    public ApprovalDecision Decision { get; set; } = ApprovalDecision.Pending;
    public string? DecidedByUserId { get; set; }
    public DateTime? DecidedAt { get; set; }
    public string? Remarks { get; set; }
}
```

7. `Models/Shared/UserProfile.cs`:

```csharp
using Microsoft.AspNetCore.Identity;

namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>
/// Maklumat perniagaan staf. BERASINGAN daripada AspNetUsers dengan sengaja:
/// AspNetUsers untuk authentication (hash kata laluan, token), bukan untuk
/// jabatan dan gred.
/// </summary>
public class UserProfile
{
    public int Id { get; set; }

    /// <summary>Kunci asing kepada IdentityUser.Id.</summary>
    public string UserId { get; set; } = string.Empty;
    public IdentityUser? User { get; set; }

    public string FullName { get; set; } = string.Empty;
    public string IdentityNo { get; set; } = string.Empty;
    public string? Phone { get; set; }

    public int? DepartmentId { get; set; }
    public LookupDepartment? Department { get; set; }
    public int? PositionId { get; set; }
    public LookupPosition? Position { get; set; }
    public int? GradeId { get; set; }
    public LookupGrade? Grade { get; set; }
}
```

8. `Models/Shared/Lookups.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Shared;

public class LookupDepartment
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class LookupPosition
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}

public class LookupGrade
{
    public int Id { get; set; }
    public string Code { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; } = true;
}
```

9. Bina:

```bash
dotnet build
```

### ✅ Semakan

- [ ] Lapan fail wujud dalam `Models/Shared/`
- [ ] `dotnet build` berjaya
- [ ] Setiap kelas dalam namespace `Nres.Onboarding.Web.Models.Shared`
- [ ] Anda boleh menerangkan kenapa `UserProfile` berasingan daripada `AspNetUsers`

---

## Latihan 3 — `IEntityTypeConfiguration<T>` (corak anti-konflik)

**Objektif:** Konfigurasikan pemetaan pangkalan data dalam fail berasingan — corak yang keempat-empat kumpulan akan ikut selama 11 hari.

> **Ini latihan paling penting hari ini bagi kolaborasi.** Corak yang anda pelajari di sini ialah sebab `ApplicationDbContext` tidak akan pernah berkonflik.

### Langkah

1. `Models/Shared/Configurations/SubmissionConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Shared.Configurations;

public class SubmissionConfiguration : IEntityTypeConfiguration<Submission>
{
    public void Configure(EntityTypeBuilder<Submission> builder)
    {
        builder.ToTable("Submissions");

        builder.Property(s => s.ReferenceNo).HasMaxLength(30).IsRequired();
        builder.Property(s => s.ModuleCode).HasMaxLength(10).IsRequired();
        builder.Property(s => s.ApplicantUserId).HasMaxLength(450).IsRequired();
        builder.Property(s => s.Status).HasConversion<int>();

        // Draf belum ada nombor rujukan, jadi banyak baris sah menyimpan "".
        // Indeks unik BERTAPIS memastikan nombor yang dikeluarkan unik
        // tanpa menyekat draf.
        builder.HasIndex(s => s.ReferenceNo)
            .IsUnique()
            .HasFilter("\"ReferenceNo\" <> ''")
            .HasDatabaseName("IX_Submissions_ReferenceNo");

        // Menyokong skrin "permohonan saya" dan "baris gilir semakan" setiap modul.
        builder.HasIndex(s => new { s.ModuleCode, s.Status });
        builder.HasIndex(s => new { s.ApplicantUserId, s.ModuleCode });
    }
}
```

2. `Models/Shared/Configurations/AttachmentConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Shared.Configurations;

public class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.ToTable("Attachments");

        builder.Property(a => a.OriginalFileName).HasMaxLength(260).IsRequired();
        builder.Property(a => a.StoredFileName).HasMaxLength(100).IsRequired();
        builder.Property(a => a.ContentType).HasMaxLength(150).IsRequired();
        builder.Property(a => a.UploadedByUserId).HasMaxLength(450);

        builder.HasOne(a => a.Submission)
            .WithMany(s => s.Attachments)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.SubmissionId);
    }
}
```

3. `Models/Shared/Configurations/AuditLogConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Shared.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.ToTable("AuditLogs");

        builder.Property(a => a.Action).HasMaxLength(60).IsRequired();
        builder.Property(a => a.ActorUserId).HasMaxLength(450).IsRequired();
        builder.Property(a => a.Remarks).HasMaxLength(1000);
        builder.Property(a => a.FromStatus).HasConversion<int?>();
        builder.Property(a => a.ToStatus).HasConversion<int?>();

        builder.HasOne(a => a.Submission)
            .WithMany(s => s.AuditLogs)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => new { a.SubmissionId, a.CreatedAt });
    }
}
```

4. `Models/Shared/Configurations/ApprovalStepConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Shared.Configurations;

public class ApprovalStepConfiguration : IEntityTypeConfiguration<ApprovalStep>
{
    public void Configure(EntityTypeBuilder<ApprovalStep> builder)
    {
        builder.ToTable("ApprovalSteps");

        builder.Property(a => a.RoleRequired).HasMaxLength(60).IsRequired();
        builder.Property(a => a.DecidedByUserId).HasMaxLength(450);
        builder.Property(a => a.Remarks).HasMaxLength(1000);
        builder.Property(a => a.Decision).HasConversion<int>();

        builder.HasOne(a => a.Submission)
            .WithMany(s => s.ApprovalSteps)
            .HasForeignKey(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Satu baris setiap kedudukan dalam laluan.
        builder.HasIndex(a => new { a.SubmissionId, a.StepOrder }).IsUnique();
    }
}
```

5. `Models/Shared/Configurations/UserProfileConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Shared.Configurations;

public class UserProfileConfiguration : IEntityTypeConfiguration<UserProfile>
{
    public void Configure(EntityTypeBuilder<UserProfile> builder)
    {
        builder.ToTable("UserProfiles");

        builder.Property(p => p.UserId).HasMaxLength(450).IsRequired();
        builder.Property(p => p.FullName).HasMaxLength(200).IsRequired();
        builder.Property(p => p.IdentityNo).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Phone).HasMaxLength(30);

        // Tepat satu profil setiap pengguna Identity.
        builder.HasIndex(p => p.UserId).IsUnique();

        builder.HasOne(p => p.User)
            .WithOne()
            .HasForeignKey<UserProfile>(p => p.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        // Lookup ialah data rujukan: sekat pemadaman semasa masih dirujuk.
        builder.HasOne(p => p.Department).WithMany()
            .HasForeignKey(p => p.DepartmentId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Position).WithMany()
            .HasForeignKey(p => p.PositionId).OnDelete(DeleteBehavior.Restrict);
        builder.HasOne(p => p.Grade).WithMany()
            .HasForeignKey(p => p.GradeId).OnDelete(DeleteBehavior.Restrict);
    }
}
```

6. `Models/Shared/Configurations/LookupConfigurations.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Shared.Configurations;

public class LookupDepartmentConfiguration : IEntityTypeConfiguration<LookupDepartment>
{
    public void Configure(EntityTypeBuilder<LookupDepartment> builder)
    {
        builder.ToTable("LookupDepartments");
        builder.Property(l => l.Code).HasMaxLength(20).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();
    }
}

public class LookupPositionConfiguration : IEntityTypeConfiguration<LookupPosition>
{
    public void Configure(EntityTypeBuilder<LookupPosition> builder)
    {
        builder.ToTable("LookupPositions");
        builder.Property(l => l.Code).HasMaxLength(20).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();
    }
}

public class LookupGradeConfiguration : IEntityTypeConfiguration<LookupGrade>
{
    public void Configure(EntityTypeBuilder<LookupGrade> builder)
    {
        builder.ToTable("LookupGrades");
        builder.Property(l => l.Code).HasMaxLength(20).IsRequired();
        builder.Property(l => l.Name).HasMaxLength(150).IsRequired();
        builder.HasIndex(l => l.Code).IsUnique();
    }
}
```

7. **Berhenti dan fahami corak ini.** Perbincangan kumpulan (5 minit):

   - Setiap konfigurasi ialah **fail berasingan**
   - Setiap satu tinggal dalam folder **pemiliknya**
   - Esok, Kumpulan 2 menambah `Models/Akses/Configurations/VehicleConfiguration.cs` — **fail baharu**, bukan suntingan
   - Tiada siapa akan pernah mengedit `ApplicationDbContext` lagi

   **Soalan untuk dijawab kumpulan anda:** jika keempat-empat kumpulan menambah konfigurasinya ke dalam `OnModelCreating` sebaliknya, berapa kali fail itu akan berkonflik dalam 11 hari?

### ✅ Semakan

- [ ] Enam fail konfigurasi dalam `Models/Shared/Configurations/`
- [ ] `dotnet build` berjaya
- [ ] Setiap satu melaksanakan `IEntityTypeConfiguration<T>`
- [ ] Kumpulan anda boleh menerangkan kenapa ini menghalang konflik

---

## Latihan 4 — `ApplicationDbContext`

**Objektif:** Satu `DbContext` yang tidak akan pernah perlu disunting lagi.

### Langkah

1. `Data/ApplicationDbContext.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Data;

/// <summary>
/// Satu DbContext untuk seluruh aplikasi. Ia mewarisi IdentityDbContext supaya
/// jadual Identity dan jadual perniagaan berkongsi satu pangkalan data dan satu
/// skop transaksi.
///
/// ⚠️ FAIL INI BEKU SELEPAS HARI 3.
/// Modul TIDAK menambah DbSet di sini. Sebaliknya, setiap entiti membawa
/// IEntityTypeConfiguration&lt;T&gt; dalam folder modulnya, dan
/// ApplyConfigurationsFromAssembly() di bawah menemuinya secara automatik.
/// Akses entiti modul melalui context.Set&lt;T&gt;().
/// Rujuk KOLABORASI.md §3.2.
/// </summary>
public class ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
    : IdentityDbContext<IdentityUser>(options)
{
    // Asas kongsi sahaja — jangan tambah DbSet modul di sini.
    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
    public DbSet<UserProfile> UserProfiles => Set<UserProfile>();
    public DbSet<LookupDepartment> LookupDepartments => Set<LookupDepartment>();
    public DbSet<LookupPosition> LookupPositions => Set<LookupPosition>();
    public DbSet<LookupGrade> LookupGrades => Set<LookupGrade>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Panggil base dahulu: ia memetakan jadual Identity.
        base.OnModelCreating(modelBuilder);

        // SATU baris yang menemui SETIAP IEntityTypeConfiguration<T> dalam
        // assembly ini — termasuk yang keempat-empat kumpulan tambah esok.
        // Inilah sebab fail ini tidak pernah perlu berubah.
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
    }
}
```

2. `Data/ApplicationDbContextFactory.cs` — supaya `dotnet ef` boleh mencipta context pada masa reka bentuk:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Nres.Onboarding.Web.Data;

/// <summary>
/// Digunakan oleh `dotnet ef` sahaja. Tanpa ini, alat migration perlu
/// membina keseluruhan aplikasi untuk mendapatkan DbContext.
/// </summary>
public class ApplicationDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite("Data Source=App_Data/nres-onboarding.db")
            .Options;

        return new ApplicationDbContext(options);
    }
}
```

3. Tambah connection string ke `appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=App_Data/nres-onboarding.db"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

### ✅ Semakan

- [ ] `dotnet build` berjaya
- [ ] `ApplicationDbContext` mengandungi **tepat satu** baris dalam `OnModelCreating` selepas `base`
- [ ] Tiada `DbSet` khusus modul
- [ ] Komen amaran "BEKU SELEPAS HARI 3" ada dalam fail

---

## Latihan 5 — Servis kongsi

**Objektif:** Bina enam servis yang keempat-empat modul kongsi — daftar anti-redundan.

### Langkah

1. `Services/ICurrentUserService.cs`:

```csharp
using System.Security.Claims;

namespace Nres.Onboarding.Web.Services;

public interface ICurrentUserService
{
    string? UserId { get; }
    bool IsInRole(string role);
}

public class CurrentUserService(IHttpContextAccessor accessor) : ICurrentUserService
{
    public string? UserId =>
        accessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

    public bool IsInRole(string role) =>
        accessor.HttpContext?.User.IsInRole(role) ?? false;
}
```

2. `Services/IReferenceNumberService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;

namespace Nres.Onboarding.Web.Services;

public interface IReferenceNumberService
{
    /// <summary>Jana rujukan seterusnya, cth. LD-2026-0001.</summary>
    Task<string> GenerateAsync(string moduleCode, CancellationToken ct = default);
}

public class ReferenceNumberService(ApplicationDbContext db) : IReferenceNumberService
{
    public async Task<string> GenerateAsync(string moduleCode, CancellationToken ct = default)
    {
        var tahun = DateTime.UtcNow.Year;
        var prefix = $"{moduleCode}-{tahun}-";

        // Kira rekod yang SUDAH mempunyai nombor tahun ini bagi modul ini.
        var kiraan = await db.Submissions
            .Where(s => s.ModuleCode == moduleCode && s.ReferenceNo.StartsWith(prefix))
            .CountAsync(ct);

        return $"{prefix}{(kiraan + 1):D4}";
    }
}
```

> **Nota pengajaran:** pendekatan kira-dan-tambah ini mudah dibaca dan memadai untuk latihan, tetapi ia **tidak selamat** apabila dua orang menghantar pada saat yang sama — kedua-duanya boleh mendapat nombor yang sama. Kita membincangkan jujukan pangkalan data sebenar pada Hari 13–14. Kenal pasti hadnya sekarang; jangan pura-pura ia tiada.

3. `Services/IAuditLogService.cs`:

```csharp
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services;

public interface IAuditLogService
{
    Task LogAsync(int submissionId, string action, SubmissionStatus? from = null,
                  SubmissionStatus? to = null, string? remarks = null,
                  CancellationToken ct = default);
}

public class AuditLogService(ApplicationDbContext db, ICurrentUserService currentUser)
    : IAuditLogService
{
    public async Task LogAsync(int submissionId, string action,
        SubmissionStatus? from = null, SubmissionStatus? to = null,
        string? remarks = null, CancellationToken ct = default)
    {
        db.AuditLogs.Add(new AuditLog
        {
            SubmissionId = submissionId,
            Action = action,
            ActorUserId = currentUser.UserId ?? "system",
            FromStatus = from,
            ToStatus = to,
            Remarks = remarks
        });

        await db.SaveChangesAsync(ct);
    }
}
```

4. `Services/IWorkflowService.cs`:

```csharp
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services;

public interface IWorkflowService
{
    bool CanTransition(SubmissionStatus from, SubmissionStatus to);

    /// <summary>Tukar status DAN tulis audit log secara atomik.</summary>
    Task TransitionAsync(Submission submission, SubmissionStatus to,
                         string action, string? remarks = null,
                         CancellationToken ct = default);
}

public class WorkflowService(ApplicationDbContext db, IAuditLogService audit)
    : IWorkflowService
{
    /// <summary>
    /// Peralihan yang dibenarkan. Ditulis SEKALI di sini supaya keempat-empat
    /// modul menguatkuasakan peraturan yang sama. Jangan salin logik ini ke
    /// dalam controller.
    /// </summary>
    private static readonly Dictionary<SubmissionStatus, SubmissionStatus[]> Dibenarkan = new()
    {
        [SubmissionStatus.Draft] =
            [SubmissionStatus.Submitted, SubmissionStatus.Cancelled],
        [SubmissionStatus.Submitted] =
            [SubmissionStatus.SupervisorApproved, SubmissionStatus.AdminApproved,
             SubmissionStatus.Rejected, SubmissionStatus.Cancelled],
        [SubmissionStatus.SupervisorApproved] =
            [SubmissionStatus.AdminApproved, SubmissionStatus.Rejected,
             SubmissionStatus.Cancelled],
        [SubmissionStatus.AdminApproved] =
            [SubmissionStatus.Completed, SubmissionStatus.Cancelled],
        [SubmissionStatus.Rejected]  = [],
        [SubmissionStatus.Completed] = [],
        [SubmissionStatus.Cancelled] = []
    };

    public bool CanTransition(SubmissionStatus from, SubmissionStatus to) =>
        Dibenarkan.TryGetValue(from, out var sah) && sah.Contains(to);

    public async Task TransitionAsync(Submission submission, SubmissionStatus to,
        string action, string? remarks = null, CancellationToken ct = default)
    {
        var from = submission.Status;

        if (!CanTransition(from, to))
            throw new InvalidOperationException(
                $"Peralihan tidak sah: {from} → {to} bagi {submission.ReferenceNo}");

        submission.Status = to;
        if (to == SubmissionStatus.Submitted) submission.SubmittedAt = DateTime.UtcNow;
        if (to == SubmissionStatus.Completed) submission.CompletedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(ct);
        await audit.LogAsync(submission.Id, action, from, to, remarks, ct);
    }
}
```

5. `Services/IFileStorageService.cs`:

```csharp
namespace Nres.Onboarding.Web.Services;

public interface IFileStorageService
{
    Task<(string StoredFileName, long SizeBytes)> SaveAsync(
        int submissionId, IFormFile file, CancellationToken ct = default);

    Stream OpenRead(int submissionId, string storedFileName);
    void Delete(int submissionId, string storedFileName);
}

public class FileStorageService(IWebHostEnvironment env) : IFileStorageService
{
    private static readonly string[] JenisDibenarkan =
        [".pdf", ".jpg", ".jpeg", ".png"];
    private const long SaizMaks = 5 * 1024 * 1024; // 5 MB

    private string RootUploads =>
        Path.Combine(env.ContentRootPath, "App_Data", "uploads");

    public async Task<(string, long)> SaveAsync(int submissionId, IFormFile file,
        CancellationToken ct = default)
    {
        if (file.Length == 0)
            throw new InvalidOperationException("Fail kosong.");
        if (file.Length > SaizMaks)
            throw new InvalidOperationException("Fail melebihi 5 MB.");

        var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
        if (!JenisDibenarkan.Contains(ext))
            throw new InvalidOperationException($"Jenis fail '{ext}' tidak dibenarkan.");

        // JANGAN guna nama fail yang pengguna beri — ia boleh mengandungi
        // laluan traversal (../../) atau menimpa fail sedia ada.
        var storedFileName = $"{Guid.NewGuid():N}{ext}";

        var folder = Path.Combine(RootUploads, submissionId.ToString());
        Directory.CreateDirectory(folder);

        var fullPath = Path.Combine(folder, storedFileName);
        await using var stream = File.Create(fullPath);
        await file.CopyToAsync(stream, ct);

        return (storedFileName, file.Length);
    }

    public Stream OpenRead(int submissionId, string storedFileName)
    {
        // Path.GetFileName membuang sebarang komponen laluan — pertahanan
        // terhadap ../../etc/passwd walaupun pemanggil ceroboh.
        var selamat = Path.GetFileName(storedFileName);
        var fullPath = Path.Combine(RootUploads, submissionId.ToString(), selamat);
        return File.OpenRead(fullPath);
    }

    public void Delete(int submissionId, string storedFileName)
    {
        var selamat = Path.GetFileName(storedFileName);
        var fullPath = Path.Combine(RootUploads, submissionId.ToString(), selamat);
        if (File.Exists(fullPath)) File.Delete(fullPath);
    }
}
```

6. `Services/INotificationService.cs`:

```csharp
namespace Nres.Onboarding.Web.Services;

public interface INotificationService
{
    Task NotifyAsync(string toUserId, string subject, string body,
                     CancellationToken ct = default);
}

/// <summary>
/// Latihan sahaja: log ke konsol. Kumpulan 1 menggantikan ini dengan
/// penghantar e-mel sebenar pada Hari 10–12 — dengan MENAMBAH pelaksanaan
/// baharu, bukan mengedit yang ini.
/// </summary>
public class ConsoleNotificationService(ILogger<ConsoleNotificationService> logger)
    : INotificationService
{
    public Task NotifyAsync(string toUserId, string subject, string body,
        CancellationToken ct = default)
    {
        logger.LogInformation("NOTIFIKASI → {UserId} | {Subject} | {Body}",
            toUserId, subject, body);
        return Task.CompletedTask;
    }
}
```

### ✅ Semakan

- [ ] Enam fail servis wujud dalam `Services/`
- [ ] `dotnet build` berjaya
- [ ] `WorkflowService` menolak peralihan tidak sah
- [ ] `FileStorageService` menolak fail >5 MB dan sambungan tidak dibenarkan
- [ ] Anda boleh menyatakan kenapa `SaveAsync` tidak menggunakan nama fail asal

---

## Latihan 6 — `SubmissionControllerBase` & partial view kongsi

**Objektif:** Bina logik kelulusan **sekali** supaya empat modul tidak menulisnya empat kali.

### Langkah

1. `Controllers/SubmissionControllerBase.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Web.Controllers;

/// <summary>
/// Tindakan aliran kerja yang dikongsi SEMUA modul. Controller modul mewarisi
/// kelas ini dan menulis hanya apa yang khusus kepada modulnya.
///
/// ⚠️ Jangan salin logik Approve/Reject ke dalam controller modul anda.
/// Jika modul anda memerlukan tingkah laku berbeza, buka isu `shared`.
/// </summary>
[Authorize]
public abstract class SubmissionControllerBase(
    ApplicationDbContext db,
    IWorkflowService workflow,
    INotificationService notifications) : Controller
{
    protected readonly ApplicationDbContext Db = db;
    protected readonly IWorkflowService Workflow = workflow;

    /// <summary>Prefix modul — setiap subclass menyediakannya.</summary>
    protected abstract string ModuleCode { get; }

    /// <summary>Peranan yang boleh meluluskan dalam modul ini.</summary>
    protected abstract string AdminRole { get; }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Approve(int id, string? remarks)
    {
        if (!User.IsInRole(AdminRole)) return Forbid();

        var submission = await Db.Submissions.FirstOrDefaultAsync(
            s => s.Id == id && s.ModuleCode == ModuleCode);
        if (submission is null) return NotFound();

        await Workflow.TransitionAsync(submission, SubmissionStatus.AdminApproved,
            "Approved", remarks);

        await notifications.NotifyAsync(submission.ApplicantUserId,
            $"Permohonan {submission.ReferenceNo} diluluskan", remarks ?? string.Empty);

        return RedirectToAction("Details", new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public virtual async Task<IActionResult> Reject(int id, string remarks)
    {
        if (!User.IsInRole(AdminRole)) return Forbid();

        // Sebab penolakan WAJIB — dikuatkuasakan di sini supaya keempat-empat
        // modul berkelakuan sama.
        if (string.IsNullOrWhiteSpace(remarks))
        {
            ModelState.AddModelError(nameof(remarks), "Sebab penolakan wajib diisi.");
            return RedirectToAction("Details", new { id });
        }

        var submission = await Db.Submissions.FirstOrDefaultAsync(
            s => s.Id == id && s.ModuleCode == ModuleCode);
        if (submission is null) return NotFound();

        await Workflow.TransitionAsync(submission, SubmissionStatus.Rejected,
            "Rejected", remarks);

        await notifications.NotifyAsync(submission.ApplicantUserId,
            $"Permohonan {submission.ReferenceNo} ditolak", remarks);

        return RedirectToAction("Details", new { id });
    }
}
```

2. `Views/Shared/_StatusBadge.cshtml`:

```cshtml
@model Nres.Onboarding.Web.Models.Shared.SubmissionStatus
@{
    var (css, teks) = Model switch
    {
        SubmissionStatus.Draft              => ("secondary", "Draf"),
        SubmissionStatus.Submitted          => ("primary",   "Dihantar"),
        SubmissionStatus.SupervisorApproved => ("info",      "Lulus Penyelia"),
        SubmissionStatus.AdminApproved      => ("success",   "Diluluskan"),
        SubmissionStatus.Rejected           => ("danger",    "Ditolak"),
        SubmissionStatus.Completed          => ("dark",      "Selesai"),
        SubmissionStatus.Cancelled          => ("warning",   "Dibatalkan"),
        _                                   => ("secondary", "Tidak diketahui")
    };
}
<span class="badge bg-@css">@teks</span>
```

3. `Views/Shared/_AuditTrail.cshtml`:

```cshtml
@model IEnumerable<Nres.Onboarding.Web.Models.Shared.AuditLog>

<div class="card">
    <div class="card-header">Sejarah Audit</div>
    <ul class="list-group list-group-flush">
    @if (!Model.Any())
    {
        <li class="list-group-item text-muted">Tiada rekod audit lagi.</li>
    }
    @foreach (var log in Model.OrderByDescending(l => l.CreatedAt))
    {
        <li class="list-group-item">
            <strong>@log.Action</strong>
            <small class="text-muted">@log.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")</small>
            @if (log.FromStatus is not null && log.ToStatus is not null)
            {
                <span class="ms-2">
                    <partial name="_StatusBadge" model="log.FromStatus.Value" />
                    →
                    <partial name="_StatusBadge" model="log.ToStatus.Value" />
                </span>
            }
            @if (!string.IsNullOrWhiteSpace(log.Remarks))
            {
                <div class="small mt-1">@log.Remarks</div>
            }
        </li>
    }
    </ul>
</div>
```

4. `Views/Shared/_ApprovalPanel.cshtml`:

```cshtml
@model Nres.Onboarding.Web.Models.Shared.Submission
@{
    var controller = ViewContext.RouteData.Values["controller"];
}

<div class="card">
    <div class="card-header">Keputusan</div>
    <div class="card-body">
        <form asp-action="Approve" asp-controller="@controller" method="post" class="mb-3">
            @Html.AntiForgeryToken()
            <input type="hidden" name="id" value="@Model.Id" />
            <textarea name="remarks" class="form-control mb-2" rows="2"
                      placeholder="Catatan (pilihan)"></textarea>
            <button type="submit" class="btn btn-success">Luluskan</button>
        </form>

        <form asp-action="Reject" asp-controller="@controller" method="post">
            @Html.AntiForgeryToken()
            <input type="hidden" name="id" value="@Model.Id" />
            <textarea name="remarks" class="form-control mb-2" rows="2"
                      placeholder="Sebab penolakan (WAJIB)" required></textarea>
            <button type="submit" class="btn btn-danger">Tolak</button>
        </form>
    </div>
</div>
```

5. Cipta juga `_AttachmentList.cshtml`, `_FilterBar.cshtml`, dan `_ValidationSummary.cshtml` — jurulatih membekalkan fail ini atau kelas menulisnya bersama jika masa mengizinkan.

### ✅ Semakan

- [ ] `SubmissionControllerBase` wujud dengan `Approve` dan `Reject`
- [ ] Sebab penolakan dikuatkuasakan dalam kelas asas, bukan diserahkan kepada modul
- [ ] Partial view kongsi wujud dalam `Views/Shared/`
- [ ] `dotnet build` berjaya
- [ ] Kumpulan anda faham: **anda tidak akan menulis Approve/Reject sendiri**

---

## Latihan 7 — Modul mendaftar diri & navigasi didorong data

**Objektif:** Jadikan `Program.cs` dan `_Layout.cshtml` beku selamanya.

### Langkah

1. `Models/Shared/ModuleDescriptor.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Shared;

/// <summary>Metadata satu modul untuk navigasi & dashboard induk.</summary>
public record ModuleDescriptor(
    string Code,
    string Nama,
    string Controller,
    string Ikon,
    string[] Roles,
    int Urutan);

/// <summary>
/// Setiap modul melaksanakan ini dalam folder SENDIRI. View component navigasi
/// mengumpul kesemuanya secara automatik — jadi menambah modul bermakna
/// menambah fail, bukan mengedit layout.
/// </summary>
public interface IModuleDescriptorProvider
{
    ModuleDescriptor Describe();
}
```

2. `ViewComponents/ModuleNavViewComponent.cs`:

```csharp
using Microsoft.AspNetCore.Mvc;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewComponents;

public class ModuleNavViewComponent(IEnumerable<IModuleDescriptorProvider> providers)
    : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        var modul = providers
            .Select(p => p.Describe())
            .Where(m => m.Roles.Any(r => UserClaimsPrincipal.IsInRole(r)))
            .OrderBy(m => m.Urutan)
            .ToList();

        return View(modul);
    }
}
```

3. `Views/Shared/Components/ModuleNav/Default.cshtml`:

```cshtml
@model IEnumerable<Nres.Onboarding.Web.Models.Shared.ModuleDescriptor>

@foreach (var m in Model)
{
    <li class="nav-item">
        <a class="nav-link" asp-controller="@m.Controller" asp-action="Index">
            <i class="@m.Ikon"></i> @m.Nama
        </a>
    </li>
}
```

4. Dalam `Views/Shared/_Layout.cshtml`, cari senarai navigasi dan tambah **satu** baris:

```cshtml
<ul class="navbar-nav flex-grow-1">
    <li class="nav-item">
        <a class="nav-link" asp-controller="Home" asp-action="Index">Utama</a>
    </li>
    @await Component.InvokeAsync("ModuleNav")   @* ← modul muncul di sini automatik *@
</ul>
```

5. **Contoh untuk esok** — inilah yang setiap kumpulan akan tambah (jangan buat sekarang):

```csharp
// Models/Akses/AksesModuleDescriptor.cs — Kumpulan 2, ESOK
public class AksesModuleDescriptor : IModuleDescriptorProvider
{
    public ModuleDescriptor Describe() => new(
        Code: ModuleCodes.PasKeselamatan,
        Nama: "Pas, Parkir & Pelekat",
        Controller: "AccessPass",
        Ikon: "bi-shield-check",
        Roles: ["Applicant", "SecurityAdmin"],
        Urutan: 2);
}
```

6. **Sekarang `Program.cs`** — tulis sekali, jangan sentuh lagi:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Services;

var builder = WebApplication.CreateBuilder(args);

// --- Pangkalan data ---
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' tiada.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// App_Data berada DI LUAR wwwroot, jadi ia tidak pernah dihidangkan sebagai fail statik.
Directory.CreateDirectory(
    Path.Combine(builder.Environment.ContentRootPath, "App_Data", "uploads"));

// --- Identity ---
builder.Services
    .AddIdentity<IdentityUser, IdentityRole>(options =>
    {
        options.Password.RequiredLength = 8;
        options.Password.RequireDigit = true;
        options.Password.RequireUppercase = true;
        options.Password.RequireNonAlphanumeric = true;
        options.Lockout.MaxFailedAccessAttempts = 5;
        options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(5);
        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedAccount = false;   // latihan sahaja
    })
    .AddEntityFrameworkStores<ApplicationDbContext>()
    .AddDefaultTokenProviders();

builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Account/Login";
    options.AccessDeniedPath = "/Account/AccessDenied";
    options.ExpireTimeSpan = TimeSpan.FromHours(8);
});

// --- Servis kongsi (Hari 3) ---
builder.Services.AddHttpContextAccessor();
builder.Services.AddScoped<ICurrentUserService, CurrentUserService>();
builder.Services.AddScoped<IReferenceNumberService, ReferenceNumberService>();
builder.Services.AddScoped<IAuditLogService, AuditLogService>();
builder.Services.AddScoped<IWorkflowService, WorkflowService>();
builder.Services.AddSingleton<IFileStorageService, FileStorageService>();
builder.Services.AddSingleton<INotificationService, ConsoleNotificationService>();

// ==========================================================================
// PENDAFTARAN MODUL — setiap kumpulan menyahkomen barisnya pada Hari 4.
// ⚠️ FAIL INI BEKU. Tambah servis dalam Services/<Modul>/<Modul>Module.cs
//    kumpulan anda, BUKAN di sini. Rujuk KOLABORASI.md §3.1.
// ==========================================================================
// builder.Services.AddLaporDiriModule();   // Kumpulan 1
// builder.Services.AddAksesModule();       // Kumpulan 2
// builder.Services.AddAkaunModule();       // Kumpulan 3
// builder.Services.AddAsetModule();        // Kumpulan 4

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    await DbInitializer.SeedAsync(scope.ServiceProvider);
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();

// Authentication ("siapa anda?") MESTI sebelum Authorization ("dibenarkan?").
app.UseAuthentication();
app.UseAuthorization();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

> **Perhatikan empat baris berkomen.** Itu **satu-satunya** perubahan yang mana-mana kumpulan akan buat pada fail ini — menyahkomen satu baris pada Hari 4, di bawah pengawasan jurulatih, satu kumpulan pada satu masa.

### ✅ Semakan

- [ ] `_Layout.cshtml` memanggil `ModuleNav` view component
- [ ] `Program.cs` mempunyai empat baris pendaftaran modul berkomen
- [ ] Amaran "FAIL INI BEKU" ada dalam `Program.cs`
- [ ] `dotnet build` berjaya
- [ ] Kumpulan anda boleh menerangkan cara modul akan muncul dalam navigasi tanpa mengedit layout

---

## Latihan 8 — Seed peranan & data demo

**Objektif:** Enam peranan, pengguna demo, dan data lookup.

### Langkah

1. `Data/DbInitializer.cs`:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Data;

public static class DbInitializer
{
    public static readonly string[] Roles =
    [
        "Applicant", "Supervisor", "HrAdmin", "IctSecurityOfficer",
        "IctAdmin", "SecurityAdmin", "FacilityAdmin", "SystemAdmin"
    ];

    public static async Task SeedAsync(IServiceProvider services)
    {
        var db = services.GetRequiredService<ApplicationDbContext>();
        var userManager = services.GetRequiredService<UserManager<IdentityUser>>();
        var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

        await db.Database.MigrateAsync();

        // --- Peranan ---
        foreach (var role in Roles)
        {
            if (!await roleManager.RoleExistsAsync(role))
                await roleManager.CreateAsync(new IdentityRole(role));
        }

        // --- Lookup (data SINTETIK — bukan rekod NRES sebenar) ---
        if (!await db.LookupDepartments.AnyAsync())
        {
            db.LookupDepartments.AddRange(
                new LookupDepartment { Code = "BPM",  Name = "Bahagian Pengurusan Maklumat" },
                new LookupDepartment { Code = "BKP",  Name = "Bahagian Khidmat Pengurusan" },
                new LookupDepartment { Code = "BPSM", Name = "Bahagian Pengurusan Sumber Manusia" },
                new LookupDepartment { Code = "BPA",  Name = "Bahagian Pembangunan & Aset" });

            db.LookupPositions.AddRange(
                new LookupPosition { Code = "PTM",  Name = "Pegawai Teknologi Maklumat" },
                new LookupPosition { Code = "PTD",  Name = "Pegawai Tadbir & Diplomatik" },
                new LookupPosition { Code = "PT",   Name = "Penolong Pegawai Teknologi Maklumat" },
                new LookupPosition { Code = "PAWM", Name = "Pembantu Awam" });

            db.LookupGrades.AddRange(
                new LookupGrade { Code = "F41", Name = "Gred F41" },
                new LookupGrade { Code = "F44", Name = "Gred F44" },
                new LookupGrade { Code = "F48", Name = "Gred F48" },
                new LookupGrade { Code = "N19", Name = "Gred N19" });

            await db.SaveChangesAsync();
        }

        // --- Pengguna demo (latihan sahaja) ---
        await CreateUserAsync(userManager, db, "applicant@nres.test", "Ali bin Ahmad",     "Applicant");
        await CreateUserAsync(userManager, db, "penyelia@nres.test",  "Siti binti Osman",  "Supervisor");
        await CreateUserAsync(userManager, db, "hr@nres.test",        "Nurul binti Hakim", "HrAdmin");
        await CreateUserAsync(userManager, db, "kesel-ict@nres.test", "Faridah binti Noor","IctSecurityOfficer");
        await CreateUserAsync(userManager, db, "keselamatan@nres.test","Rahim bin Yusof",  "SecurityAdmin");
        await CreateUserAsync(userManager, db, "fasiliti@nres.test",  "Zaid bin Hassan",   "FacilityAdmin");
        await CreateUserAsync(userManager, db, "ict@nres.test",       "Chan Wei Ming",     "IctAdmin");
        await CreateUserAsync(userManager, db, "admin@nres.test",     "Admin Sistem",      "SystemAdmin");
    }

    private static async Task CreateUserAsync(
        UserManager<IdentityUser> userManager, ApplicationDbContext db,
        string email, string fullName, string role)
    {
        if (await userManager.FindByEmailAsync(email) is not null) return;

        var user = new IdentityUser { UserName = email, Email = email, EmailConfirmed = true };

        // ⚠️ Kata laluan demo, dalam kod, untuk latihan SAHAJA.
        // Dalam sistem sebenar: jangan sekali-kali. Guna konfigurasi selamat
        // atau paksa tetapan semula kata laluan pada log masuk pertama.
        var result = await userManager.CreateAsync(user, "Nres@2026!");
        if (!result.Succeeded) return;

        await userManager.AddToRoleAsync(user, role);

        db.UserProfiles.Add(new UserProfile
        {
            UserId = user.Id,
            FullName = fullName,
            IdentityNo = $"{Random.Shared.Next(700101, 991231)}-14-{Random.Shared.Next(1000, 9999)}",
            DepartmentId = 1
        });
        await db.SaveChangesAsync();
    }
}
```

> **Titik pengajaran keselamatan:** kata laluan demo berada dalam kod di sini kerana ini makmal latihan tanpa pelayan mel. Ini **tepat** apa yang kita ajar peserta **jangan** lakukan dalam sistem sebenar — dan Kumpulan 3 akan membincangkannya semula apabila mereka membina modul ID/AD/Email, di mana godaan menyimpan kata laluan adalah paling kuat.

### ✅ Semakan

- [ ] `DbInitializer` menyemai 6 peranan
- [ ] 6 pengguna demo dicipta dengan peranan
- [ ] Data lookup disemai
- [ ] `dotnet build` berjaya

---

## Latihan 9 — Migration `InitialShared`

**Objektif:** Cipta skema pangkalan data.

### Langkah

1. Jana migration:

```bash
cd Nres.Onboarding.Web
dotnet ef migrations add InitialShared
```

2. **Baca fail yang dijana** — jangan hanya jalankannya. Buka `Migrations/<cap masa>_InitialShared.cs` dan cari:
   - Jadual Identity (`AspNetUsers`, `AspNetRoles`, …)
   - Jadual kongsi kita (`Submissions`, `Attachments`, `AuditLogs`, `ApprovalSteps`, `UserProfiles`, `Lookup*`)
   - Indeks unik bertapis pada `ReferenceNo`

3. Guna pakai:

```bash
dotnet ef database update
```

4. Sahkan pangkalan data:

```bash
ls -la App_Data/nres-onboarding.db
```

5. Jalankan aplikasi:

```bash
dotnet run
```

Log masuk sebagai `hr@nres.test` / `Nres@2026!`. Sahkan anda log masuk (navigasi modul kosong buat masa ini — itu betul, tiada modul mendaftar lagi).

### ✅ Semakan

- [ ] Migration `InitialShared` wujud
- [ ] `App_Data/nres-onboarding.db` dicipta
- [ ] Aplikasi bermula tanpa ralat
- [ ] Anda boleh log masuk dengan pengguna demo
- [ ] Anda telah **membaca** fail migration, bukan hanya menjalankannya

---

## Latihan 10 — Gabung ke `master` & buka cabang kumpulan

**Objektif:** Asas kongsi masuk ke `master`; empat kumpulan bercabang daripadanya.

### Langkah

1. Commit asas kongsi:

```bash
cd ..
git add .
git commit -m "asas: entiti kongsi, servis, controller base dan migration InitialShared"
git push -u origin asas/shared-foundation
```

2. Buka PR `asas/shared-foundation` → `master`. **Seluruh kelas menyemaknya bersama** di skrin. Semak:
   - Tiada `DbSet` khusus modul dalam `ApplicationDbContext`
   - `Program.cs` mempunyai empat baris pendaftaran berkomen
   - Setiap servis kongsi wujud dan didaftar
   - `.gitignore` menghalang `*.db` dan `bin/`

3. Gabung PR.

4. **Setiap** peserta menyegerak:

```bash
git switch master
git pull --rebase origin master
```

5. **Satu peserta setiap kumpulan** mencipta cabang kumpulan:

| Kumpulan | Arahan |
|----------|--------|
| 1 | `git switch -c kump-1/lapor-diri && git push -u origin kump-1/lapor-diri` |
| 2 | `git switch -c kump-2/akses-kenderaan && git push -u origin kump-2/akses-kenderaan` |
| 3 | `git switch -c kump-3/id-ad-email && git push -u origin kump-3/id-ad-email` |
| 4 | `git switch -c kump-4/perisian-aset && git push -u origin kump-4/perisian-aset` |

6. **Setiap** ahli kumpulan bertukar kepadanya:

```bash
git fetch origin
git switch kump-N/<slug>
dotnet build          # sahkan asas kongsi wujud pada mesin anda
```

7. **Ikrar penutup — baca kuat-kuat sebagai kumpulan:**

   > Kami hanya mencipta fail dalam folder modul kami.
   > Kami tidak menyunting `Program.cs`, `ApplicationDbContext`, atau `_Layout.cshtml`.
   > Kami tidak menulis semula servis kongsi.
   > Kami mengumumkan sebelum mengambil slot migration.
   > Kami `git pull --rebase origin master` setiap pagi.

### ✅ Semakan

- [ ] `asas/shared-foundation` digabung ke `master`
- [ ] Keempat-empat cabang kumpulan wujud pada `origin`
- [ ] Setiap peserta berada pada cabang kumpulannya dan `dotnet build` berjaya
- [ ] Setiap kumpulan boleh menyenaraikan folder yang dimilikinya tanpa melihat
- [ ] Setiap kumpulan boleh menamakan enam servis kongsi yang tidak boleh ditulis semula

---

## Deliverable Hari 3

| Artifak | Status |
|---------|--------|
| `Nres.Onboarding.Web` berjalan | ✅ pada `master` |
| Entiti kongsi + konfigurasi | `Models/Shared/` |
| `ApplicationDbContext` (beku) | `Data/` |
| Enam servis kongsi | `Services/` |
| `SubmissionControllerBase` + partial kongsi | `Controllers/`, `Views/Shared/` |
| Corak modul mendaftar diri | `Program.cs` (beku), `ModuleNav` |
| Migration `InitialShared` | `Migrations/` |
| Identity + 6 peranan + pengguna demo | `DbInitializer` |
| 4 cabang kumpulan | `origin/kump-1..4/…` |

## Bermula esok

Fasa 2. Kumpulan anda bekerja pada modulnya sendiri selama 11 hari. Setiap pagi:

```bash
git pull --rebase origin master     # 9.00 — sebelum apa-apa
```

Kemudian stand-up, semakan silang AI, dan bina. Rujuk trek kumpulan anda:

[Kumpulan 1](../../kumpulan-1-lapor-diri/) · [Kumpulan 2](../../kumpulan-2-pas-parkir-pelekat/) · [Kumpulan 3](../../kumpulan-3-id-ad-email/) · [Kumpulan 4](../../kumpulan-4-perisian-aset-ict/)
