# Lab Hari 13 — Aset ICT: Pemodelan

Lab ini mengiringi [`../README.md`](../README.md) Hari 13. Ikut latihan **secara berurutan** — setiap latihan bina di atas latihan sebelumnya, dan di atas projek `Nres.Onboarding.Web` yang sudah wujud sejak Hari 1. Rujuk [`../../projek/`](../../projek/) untuk **banding** kod anda selepas cuba sendiri dahulu.

> **Peraturan lab:** Taip kod ini **sendiri** ke dalam projek anda — jangan salin-tampal tanpa faham. Semua kod di bawah **valid, boleh-taip, boleh-jalan** untuk .NET 10 SDK / EF Core 10.

---

## Senarai Semak Sebelum Mula

- [ ] `dotnet --version` menunjukkan `10.x`
- [ ] Projek `Nres.Onboarding.Web` sedia ada (dari Hari 1), berjalan dengan `dotnet run`
- [ ] `ApplicationDbContext` sudah wujud di `Data/ApplicationDbContext.cs` dengan `DbSet<Submission>`, `DbSet<Attachment>`, `DbSet<AuditLog>`, dan `DbSet` bagi Modul 1–4
- [ ] Migration terdahulu (`InitialShared` dan seterusnya) sudah di-`database update`
- [ ] Pakej `Microsoft.EntityFrameworkCore.Sqlite` & `Microsoft.EntityFrameworkCore.Design` sudah dipasang

Jika mana-mana belum sedia, semak semula Hari 1 sebelum teruskan.

---

## Latihan 1 — Enum `AssetStatus`

**Objektif:** Tulis enum status aset fizikal — **berasingan** daripada `SubmissionStatus` yang sudah dikongsi sejak Hari 1.

Cipta fail `Models/AssetStatus.cs`:

```csharp
namespace Nres.Onboarding.Web.Models;

/// <summary>
/// Status inventori FIZIKAL sesuatu aset ICT.
/// BUKAN status permohonan — lihat SubmissionStatus untuk itu.
/// Satu Asset berulang-alik antara status ini sepanjang hayatnya.
/// </summary>
public enum AssetStatus
{
    Available = 0,
    Reserved = 1,
    OnLoan = 2,
    Returned = 3,
    UnderMaintenance = 4,
    Retired = 5
}
```

✅ **Semakan:** Fail `Models/AssetStatus.cs` wujud, projek masih `dotnet build` tanpa ralat.

---

## Latihan 2 — Entiti `Asset`

**Objektif:** Model inventori fizikal — medan mengikut jadual "Asset fields" dalam README.

Cipta fail `Models/Asset.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Nres.Onboarding.Web.Models;

/// <summary>
/// Satu rekod aset ICT fizikal (laptop, monitor, printer, dll.).
/// Wujud SECARA BEBAS daripada mana-mana permohonan — kekal dalam
/// sistem selepas sesuatu pinjaman selesai, sedia untuk pinjaman seterusnya.
/// </summary>
public class Asset
{
    public int Id { get; set; }

    /// <summary>Label fizikal ditampal pada peranti, cth. "ICT-AST-0001". Unik.</summary>
    [Required]
    [StringLength(30)]
    public string AssetTag { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string SerialNumber { get; set; } = string.Empty;

    /// <summary>Cth. Laptop, Desktop, Monitor, Printer, MobilePhone, NetworkEquipment.</summary>
    [Required]
    [StringLength(50)]
    public string Category { get; set; } = string.Empty;

    [Required]
    [StringLength(150)]
    public string BrandModel { get; set; } = string.Empty;

    public AssetStatus Status { get; set; } = AssetStatus.Available;

    /// <summary>UserId pemegang semasa. Null bermaksud aset di gudang, tiada pemegang.</summary>
    public string? CurrentHolderUserId { get; set; }

    [Required]
    [StringLength(50)]
    public string Condition { get; set; } = "Baik";

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ICollection<AssetLoanRequest> LoanRequests { get; set; } = new List<AssetLoanRequest>();
    public ICollection<AssetReturn> Returns { get; set; } = new List<AssetReturn>();
}
```

✅ **Semakan:** `Asset` ada 7 medan domain (`AssetTag`, `SerialNumber`, `Category`, `BrandModel`, `Status`, `CurrentHolderUserId`, `Condition`) sepadan jadual dalam README.

---

## Latihan 3 — Entiti `SoftwareCatalogItem`

**Objektif:** Model katalog perisian yang sah dimohon.

Cipta fail `Models/SoftwareCatalogItem.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Nres.Onboarding.Web.Models;

/// <summary>
/// Satu perisian dalam "menu" katalog yang boleh dipohon kakitangan.
/// Pemohon PILIH daripada senarai ini — tidak taip nama perisian bebas.
/// </summary>
public class SoftwareCatalogItem
{
    public int Id { get; set; }

    [Required]
    [StringLength(150)]
    public string Name { get; set; } = string.Empty;

    [Required]
    [StringLength(100)]
    public string Vendor { get; set; } = string.Empty;

    [StringLength(30)]
    public string Version { get; set; } = string.Empty;

    /// <summary>Cth. Freeware, PerpetualLicense, Subscription.</summary>
    [Required]
    [StringLength(30)]
    public string LicenseType { get; set; } = string.Empty;

    public bool RequiresApproval { get; set; } = true;

    public bool IsActive { get; set; } = true;

    public ICollection<SoftwareRequest> Requests { get; set; } = new List<SoftwareRequest>();
}
```

✅ **Semakan:** `SoftwareCatalogItem` boleh sedia dijadikan senarai dropdown Hari 14 (`IsActive` untuk tapis perisian yang sudah *discontinued*).

---

## Latihan 4 — Entiti `SoftwareRequest`

**Objektif:** Permohonan perisian — anak kepada `Submission` induk (corak sama seperti `OfficerReportingApplication` Hari 2).

Cipta fail `Models/SoftwareRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Nres.Onboarding.Web.Models;

/// <summary>
/// Butiran khusus permohonan perisian. Status & nombor rujukan
/// disimpan pada Submission induk (SubmissionId) — bukan di sini.
/// </summary>
public class SoftwareRequest
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public int SoftwareCatalogItemId { get; set; }
    public SoftwareCatalogItem SoftwareCatalogItem { get; set; } = null!;

    [Required]
    [StringLength(500)]
    public string Justification { get; set; } = string.Empty;

    /// <summary>Nama komputer/aset sasaran pemasangan, jika diketahui.</summary>
    [StringLength(100)]
    public string? TargetComputerName { get; set; }
}
```

✅ **Semakan:** `SoftwareRequest` **tiada** medan `Status`/`ReferenceNo` sendiri — ia rujuk `Submission.Status` & `Submission.ReferenceNo`, sama seperti semua modul terdahulu.

---

## Latihan 5 — Entiti `AssetLoanRequest`

**Objektif:** Permohonan pinjaman aset. Perhatikan `AssetId` **nullable** — aset sebenar hanya ditetapkan semasa fulfillment (Hari 14).

Cipta fail `Models/AssetLoanRequest.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Nres.Onboarding.Web.Models;

/// <summary>
/// Permohonan pinjaman aset. Pemohon nyatakan KATEGORI yang diperlukan;
/// ICT Admin tetapkan AssetId sebenar semasa fulfillment (Hari 14) —
/// itulah sebabnya AssetId nullable pada peringkat draf/submit.
/// </summary>
public class AssetLoanRequest
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    /// <summary>Kategori aset yang dipohon, cth. "Laptop". Belum tentu aset sebenar.</summary>
    [Required]
    [StringLength(50)]
    public string RequestedCategory { get; set; } = string.Empty;

    [Required]
    [StringLength(500)]
    public string Purpose { get; set; } = string.Empty;

    [DataType(DataType.Date)]
    public DateTime NeededFrom { get; set; }

    [DataType(DataType.Date)]
    public DateTime? ExpectedReturnDate { get; set; }

    /// <summary>Ditetapkan oleh ICT Admin semasa fulfillment. Null = belum ditetapkan.</summary>
    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }
}
```

✅ **Semakan:** `AssetId` bertaip `int?` (nullable) — bukan `int`. Ini **sengaja**, bukan silap taip.

---

## Latihan 6 — Entiti `AssetReturn`

**Objektif:** Pemulangan aset — permohonan berasingan yang rujuk kembali kepada `AssetLoanRequest` asal.

Cipta fail `Models/AssetReturn.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace Nres.Onboarding.Web.Models;

/// <summary>
/// Permohonan pemulangan aset. Submission BERASINGAN daripada
/// Submission pinjaman asal (prefix AST-R, bukan AST-L) — sebab
/// pemulangan boleh berlaku lama selepas pinjaman selesai diproses.
/// </summary>
public class AssetReturn
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public int AssetLoanRequestId { get; set; }
    public AssetLoanRequest AssetLoanRequest { get; set; } = null!;

    public int AssetId { get; set; }
    public Asset Asset { get; set; } = null!;

    [Required]
    [StringLength(50)]
    public string ConditionOnReturn { get; set; } = string.Empty;

    [StringLength(500)]
    public string? Remarks { get; set; }

    public bool RequiresMaintenance { get; set; }
}
```

✅ **Semakan:** `AssetReturn` ada **dua** rujukan FK — `SubmissionId` (permohonan pemulangan itu sendiri) **dan** `AssetLoanRequestId` (permohonan pinjaman asal). Jangan keliru dua ini.

---

## Latihan 7 — Kemas Kini `ApplicationDbContext`

**Objektif:** Daftar 5 entiti baharu, konfigurasi relasi & index unik.

Buka `Data/ApplicationDbContext.cs` (fail sedia ada sejak Hari 1). **Tambah** (jangan padam) `DbSet` berikut di dalam kelas:

```csharp
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<SoftwareCatalogItem> SoftwareCatalogItems => Set<SoftwareCatalogItem>();
    public DbSet<SoftwareRequest> SoftwareRequests => Set<SoftwareRequest>();
    public DbSet<AssetLoanRequest> AssetLoanRequests => Set<AssetLoanRequest>();
    public DbSet<AssetReturn> AssetReturns => Set<AssetReturn>();
```

Dalam kaedah `OnModelCreating(ModelBuilder modelBuilder)` sedia ada, **tambah** blok konfigurasi berikut sebelum baris `base.OnModelCreating(modelBuilder);` (atau selepasnya, ikut susunan sedia ada dalam fail anda):

```csharp
        modelBuilder.Entity<Asset>(entity =>
        {
            entity.HasIndex(a => a.AssetTag).IsUnique();
            entity.HasIndex(a => a.SerialNumber).IsUnique();
        });

        modelBuilder.Entity<SoftwareRequest>(entity =>
        {
            // Satu Submission cuma boleh ada SATU SoftwareRequest berkaitan.
            entity.HasIndex(sr => sr.SubmissionId).IsUnique();

            entity.HasOne(sr => sr.Submission)
                  .WithOne()
                  .HasForeignKey<SoftwareRequest>(sr => sr.SubmissionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(sr => sr.SoftwareCatalogItem)
                  .WithMany(c => c.Requests)
                  .HasForeignKey(sr => sr.SoftwareCatalogItemId)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AssetLoanRequest>(entity =>
        {
            entity.HasIndex(alr => alr.SubmissionId).IsUnique();

            entity.HasOne(alr => alr.Submission)
                  .WithOne()
                  .HasForeignKey<AssetLoanRequest>(alr => alr.SubmissionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(alr => alr.Asset)
                  .WithMany(a => a.LoanRequests)
                  .HasForeignKey(alr => alr.AssetId)
                  .IsRequired(false)
                  .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<AssetReturn>(entity =>
        {
            entity.HasIndex(ar => ar.SubmissionId).IsUnique();

            entity.HasOne(ar => ar.Submission)
                  .WithOne()
                  .HasForeignKey<AssetReturn>(ar => ar.SubmissionId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(ar => ar.AssetLoanRequest)
                  .WithMany()
                  .HasForeignKey(ar => ar.AssetLoanRequestId)
                  .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(ar => ar.Asset)
                  .WithMany(a => a.Returns)
                  .HasForeignKey(ar => ar.AssetId)
                  .OnDelete(DeleteBehavior.Restrict);
        });
```

**Kenapa `OnDelete(DeleteBehavior.Restrict)` untuk `Asset`, tapi `Cascade` untuk `Submission`?** Jika `Submission` dipadam, wajar padam sekali `SoftwareRequest`/`AssetLoanRequest`/`AssetReturn` berkaitan (data induk-anak). Tetapi jika seseorang cuba padam `Asset` yang **masih** ada sejarah pinjaman, kita **mahu EF Core menghalang** (bukan padam senyap sejarah audit) — itulah `Restrict`.

✅ **Semakan:** `dotnet build` berjaya, tiada ralat konfigurasi model.

---

## Latihan 8 — Migration `AddIctAssets`

**Objektif:** Jana & jalankan migration untuk 5 jadual baharu, **tanpa** menyentuh jadual modul sedia ada.

```bash
dotnet ef migrations add AddIctAssets
dotnet ef database update
```

**Apa yang patut berlaku:** Fail migration baharu dijana dalam `Migrations/`, mengandungi **hanya** `CreateTable` untuk `Assets`, `SoftwareCatalogItems`, `SoftwareRequests`, `AssetLoanRequests`, `AssetReturns` (dan `CreateIndex` untuk index unik) — **tiada** perubahan pada jadual Modul 1–4.

Sahkan skema dengan `sqlite3` (gantikan nama fail DB jika berbeza):

```bash
sqlite3 nres_onboarding.db ".tables"
sqlite3 nres_onboarding.db ".schema Assets"
```

✅ **Semakan:** `.tables` menyenaraikan `Assets`, `SoftwareCatalogItems`, `SoftwareRequests`, `AssetLoanRequests`, `AssetReturns` bersama jadual sedia ada. `.schema Assets` menunjukkan lajur `AssetTag`, `SerialNumber`, `Category`, `BrandModel`, `Status`, `CurrentHolderUserId`, `Condition`.

---

## Latihan 9 — Seed Katalog Perisian & Aset Contoh

**Objektif:** Isi data contoh supaya Hari 14 ada perisian & aset sebenar untuk diuji.

Buka (atau cipta jika belum wujud) `Data/DbSeeder.cs` — kelas seed kongsi yang sudah bermula sejak Hari 1 untuk lookup jabatan/gred/jawatan. **Tambah** kaedah berikut ke kelas `DbSeeder` sedia ada:

```csharp
    public static async Task SeedIctAssetsAsync(ApplicationDbContext db)
    {
        if (!await db.SoftwareCatalogItems.AnyAsync())
        {
            db.SoftwareCatalogItems.AddRange(
                new SoftwareCatalogItem
                {
                    Name = "Microsoft Office 365",
                    Vendor = "Microsoft",
                    Version = "2024",
                    LicenseType = "Subscription",
                    RequiresApproval = true,
                    IsActive = true
                },
                new SoftwareCatalogItem
                {
                    Name = "Adobe Acrobat Pro",
                    Vendor = "Adobe",
                    Version = "2024",
                    LicenseType = "PerpetualLicense",
                    RequiresApproval = true,
                    IsActive = true
                },
                new SoftwareCatalogItem
                {
                    Name = "7-Zip",
                    Vendor = "Igor Pavlov",
                    Version = "23.01",
                    LicenseType = "Freeware",
                    RequiresApproval = false,
                    IsActive = true
                },
                new SoftwareCatalogItem
                {
                    Name = "AutoCAD",
                    Vendor = "Autodesk",
                    Version = "2025",
                    LicenseType = "Subscription",
                    RequiresApproval = true,
                    IsActive = true
                }
            );
        }

        if (!await db.Assets.AnyAsync())
        {
            db.Assets.AddRange(
                new Asset
                {
                    AssetTag = "ICT-AST-0001",
                    SerialNumber = "SN-DL-0001",
                    Category = "Laptop",
                    BrandModel = "Dell Latitude 5440",
                    Status = AssetStatus.Available,
                    Condition = "Baik"
                },
                new Asset
                {
                    AssetTag = "ICT-AST-0002",
                    SerialNumber = "SN-DL-0002",
                    Category = "Laptop",
                    BrandModel = "Dell Latitude 5440",
                    Status = AssetStatus.Available,
                    Condition = "Baik"
                },
                new Asset
                {
                    AssetTag = "ICT-AST-0003",
                    SerialNumber = "SN-HP-0010",
                    Category = "Desktop",
                    BrandModel = "HP EliteDesk 800",
                    Status = AssetStatus.OnLoan,
                    CurrentHolderUserId = null,
                    Condition = "Baik"
                },
                new Asset
                {
                    AssetTag = "ICT-AST-0004",
                    SerialNumber = "SN-DELL-M01",
                    Category = "Monitor",
                    BrandModel = "Dell UltraSharp U2422H",
                    Status = AssetStatus.UnderMaintenance,
                    Condition = "Perlu Baik Pulih — skrin berkelip"
                },
                new Asset
                {
                    AssetTag = "ICT-AST-0005",
                    SerialNumber = "SN-CAN-P01",
                    Category = "Printer",
                    BrandModel = "Canon imageCLASS LBP226dw",
                    Status = AssetStatus.Retired,
                    Condition = "Dilupuskan — melebihi tempoh guna"
                }
            );
        }

        await db.SaveChangesAsync();
    }
```

> **Nota:** Aset `ICT-AST-0003` sengaja diseed dengan status `OnLoan` tetapi `CurrentHolderUserId = null` — ini **sengaja tidak realistik** untuk demo Hari 14 (ICT Admin akan "betulkan" data ini semasa lab pemulangan). Dalam data sebenar, `OnLoan` **mesti** disertai `CurrentHolderUserId` yang sah.

Panggil kaedah ini dalam `Program.cs`, di dalam blok seed sedia ada (selepas `await DbSeeder.SeedLookupsAsync(db);` atau seumpamanya dari hari-hari sebelumnya):

```csharp
    await DbSeeder.SeedIctAssetsAsync(db);
```

Jalankan aplikasi sekali untuk cetus seed:

```bash
dotnet run
```

Sahkan dengan `sqlite3`:

```bash
sqlite3 nres_onboarding.db "SELECT AssetTag, Category, Status FROM Assets;"
sqlite3 nres_onboarding.db "SELECT Name, LicenseType FROM SoftwareCatalogItems;"
```

✅ **Semakan:** Query pertama memulangkan **5 baris** (`ICT-AST-0001` hingga `ICT-AST-0005`) dengan status berbeza-beza (`Available`, `Available`, `OnLoan`, `UnderMaintenance`, `Retired` — nilai enum disimpan sebagai integer `0/2/4/5`; guna `CAST(Status AS INTEGER)` jika perlu lihat nombor). Query kedua memulangkan **4 baris** perisian.

---

## Latihan 10 — Sahkan Perbezaan Status (Renungan Kod)

**Objektif:** Buktikan kefahaman "status permohonan ≠ status aset" secara praktikal, bukan sekadar teori.

Tulis (boleh dalam fail sementara `Program.cs` atau via `dotnet run` dengan kod ujian ringkas — **jangan commit kod sementara ini**) satu query LINQ yang:

```csharp
// Contoh renungan — cari semua aset yang SEDANG OnLoan,
// tanpa merujuk Submission/SubmissionStatus langsung.
var assetsOnLoan = await db.Assets
    .Where(a => a.Status == AssetStatus.OnLoan)
    .Select(a => new { a.AssetTag, a.BrandModel, a.CurrentHolderUserId })
    .ToListAsync();

foreach (var a in assetsOnLoan)
{
    Console.WriteLine($"{a.AssetTag} ({a.BrandModel}) — pemegang: {a.CurrentHolderUserId ?? "TIADA (data tidak konsisten!)"}");
}
```

Jalankan dan perhatikan output — `ICT-AST-0003` akan tercetak dengan amaran "TIADA (data tidak konsisten!)" kerana kita sengaja seed data tidak lengkap di Latihan 9.

✅ **Semakan akhir Hari 13:**
- Kelima-lima entiti (`Asset`, `SoftwareCatalogItem`, `SoftwareRequest`, `AssetLoanRequest`, `AssetReturn`) wujud dalam `Models/`.
- Migration `AddIctAssets` berjaya, jadual wujud dalam SQLite.
- Seed katalog perisian (4 item) & aset contoh (5 aset, pelbagai status) berjaya.
- Anda boleh terangkan **kenapa** `AssetLoanRequest.AssetId` nullable, dan **kenapa** `AssetStatus` berasingan daripada `SubmissionStatus`.
- `dotnet build` bersih, tiada ralat.

---

**Cross-ref rujukan:** Banding struktur entiti anda dengan `../../projek/Nres.Onboarding.Web/Models/` (folder rujukan penuh) selepas cuba sendiri.
