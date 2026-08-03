# Lab · Kumpulan 4 · Hari 4 — Katalog Aset & Perisian

> Konsep: [`../README.md`](../README.md) · Kanun: [`../../../SPEC-KURSUS.md`](../../../SPEC-KURSUS.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula hari dengan betul

```bash
git switch kump-4/perisian-aset
git pull --rebase origin master
git switch -c kump-4/feat/katalog-aset-perisian
dotnet build
```

**Semakan "sudah wujud?"**

```bash
grep -rn "SubmissionStatus" Nres.Onboarding.Web/Models/Shared/SubmissionStatus.cs
grep -ri "Asset\|Inventory" Nres.Onboarding.Web/
```

**Prompt AI hari ini:**

```text
Merujuk AGENTS.md dan SPEC-KURSUS.md: saya Kumpulan 4, modul Perisian & Aset ICT.
Aset saya memerlukan status sendiri (Available, OnLoan, UnderMaintenance, Lost).
Patutkah saya menambah nilai ini ke enum SubmissionStatus yang dikongsi?
```

> Jawapan betul: **tidak**. `SubmissionStatus` menjejaki kitaran hayat permohonan dan dikongsi keempat-empat modul. Status aset ialah konsep berbeza dalam domain anda — ia memerlukan enum sendiri dalam `Models/Aset/`. Jika AI mencadangkan menambah ke enum kongsi, tolak dan fahami sebabnya.

### ✅ Semakan

- [ ] `dotnet build` berjaya
- [ ] Anda boleh menyatakan kenapa status aset ≠ `SubmissionStatus`
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Entiti `Asset` dengan status sendiri

**Objektif:** Barang fizikal dengan kitaran hayatnya sendiri.

### Langkah

`Models/Aset/Asset.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Aset;

public enum KategoriAset
{
    Laptop = 1,
    Desktop = 2,
    Monitor = 3,
    Pencetak = 4,
    Projektor = 5,
    Telefon = 6,
    Lain = 9
}

/// <summary>
/// Status FIZIKAL aset — BERASINGAN daripada SubmissionStatus.
///
/// SubmissionStatus menjejaki kitaran hayat PERMOHONAN (Draft → Submitted → ...).
/// AssetStatus menjejaki kitaran hayat BARANG (Available → OnLoan → ...).
///
/// Satu permohonan boleh Rejected sementara asetnya kekal OnLoan kepada
/// orang lain. Kedua-dua status tidak berkaitan — jangan campurkan.
/// </summary>
public enum AssetStatus
{
    Available = 1,
    OnLoan = 2,
    UnderMaintenance = 3,
    Lost = 4,
    Retired = 5
}

/// <summary>
/// Satu keping perkakasan fizikal. Dijejak secara INDIVIDU (tidak seperti
/// lesen perisian, yang dijejak dengan kiraan).
/// </summary>
public class Asset
{
    public int Id { get; set; }

    /// <summary>Tag inventori NRES, cth. "NRES-LT-0042". Unik.</summary>
    public string AssetTag { get; set; } = string.Empty;

    /// <summary>Nombor siri pengeluar. Unik. Boleh berubah selepas pembaikan.</summary>
    public string SerialNumber { get; set; } = string.Empty;

    public KategoriAset Kategori { get; set; } = KategoriAset.Laptop;
    public string Nama { get; set; } = string.Empty;
    public string? Jenama { get; set; }
    public string? Model { get; set; }

    /// <summary>Status FIZIKAL — bukan status permohonan.</summary>
    public AssetStatus Status { get; set; } = AssetStatus.Available;

    public DateTime? TarikhPerolehan { get; set; }
    public decimal? Harga { get; set; }
    public string? Lokasi { get; set; }
    public string? Catatan { get; set; }

    public bool IsActive { get; set; } = true;
}
```

### ✅ Semakan

- [ ] Fail dalam `Models/Aset/`
- [ ] `AssetStatus` ialah enum **anda**, bukan tambahan kepada `SubmissionStatus`
- [ ] Kedua-dua `AssetTag` dan `SerialNumber` wujud
- [ ] Komen menjelaskan perbezaan status

---

## Latihan 2 — Katalog perisian (dijejak dengan kiraan)

**Objektif:** Lesen — model berbeza daripada perkakasan.

### Langkah

`Models/Aset/SoftwareCatalogItem.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Aset;

public enum JenisLesen
{
    /// <summary>Terikat kepada seorang pengguna.</summary>
    PerPengguna = 1,
    /// <summary>Terikat kepada satu mesin.</summary>
    PerPeranti = 2,
    /// <summary>Bilangan pengguna serentak.</summary>
    Serentak = 3,
    /// <summary>Tiada had — percuma atau lesen tapak.</summary>
    TanpaHad = 4
}

/// <summary>
/// Perisian dalam katalog. Dijejak dengan KIRAAN lesen, bukan item individu —
/// tidak seperti Asset.
/// </summary>
public class SoftwareCatalogItem
{
    public int Id { get; set; }

    public string Nama { get; set; } = string.Empty;
    public string? Vendor { get; set; }
    public string? Versi { get; set; }

    public JenisLesen JenisLesen { get; set; } = JenisLesen.PerPengguna;

    /// <summary>
    /// Jumlah lesen yang NRES miliki. Null bermakna tiada had
    /// (perisian percuma atau lesen tapak).
    /// </summary>
    public int? JumlahLesen { get; set; }

    /// <summary>
    /// Adakah perisian ini memerlukan kelulusan tambahan (kos tinggi,
    /// atau sekatan pematuhan)?
    /// </summary>
    public bool PerluJustifikasi { get; set; }

    public decimal? KosSetahun { get; set; }
    public string? Catatan { get; set; }
    public bool IsActive { get; set; } = true;
}
```

> **Perhatikan tiada medan `LesenDiguna`.** Kita **mengira** lesen yang digunakan daripada permohonan aktif — medan kiraan yang disimpan tidak segerak, dan itu punca aduan inventori sebenar. Keputusan ini didokumenkan pada Hari 13–14.

### ✅ Semakan

- [ ] `SoftwareCatalogItem` dijejak dengan kiraan, bukan item
- [ ] **Tiada** medan `LesenDiguna` yang disimpan
- [ ] `JumlahLesen` nullable untuk lesen tanpa had

---

## Latihan 3 — Entiti permohonan (tiga jenis)

**Objektif:** Permohonan perisian, pinjaman aset, dan pemulangan aset.

### Langkah

`Models/Aset/AsetApplications.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Aset;

/// <summary>Permohonan lesen perisian. Prefix SW.</summary>
public class SoftwareRequest
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public int SoftwareCatalogItemId { get; set; }
    public SoftwareCatalogItem? SoftwareCatalogItem { get; set; }

    public string Justifikasi { get; set; } = string.Empty;

    /// <summary>Untuk berapa lama lesen diperlukan (null = kekal).</summary>
    public DateTime? TarikhTamat { get; set; }

    /// <summary>Diisi ICT semasa kelulusan.</summary>
    public string? KunciLesen { get; set; }
    public DateTime? TarikhDiaktifkan { get; set; }
}

/// <summary>Permohonan pinjaman aset. Prefix AST-L.</summary>
public class AssetLoanRequest
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    /// <summary>Kategori yang dipohon. Aset SEBENAR diperuntukkan semasa kelulusan.</summary>
    public KategoriAset KategoriDipohon { get; set; }

    /// <summary>Diperuntukkan ICT semasa kelulusan — null semasa draf.</summary>
    public int? AssetId { get; set; }
    public Asset? Asset { get; set; }

    public string Justifikasi { get; set; } = string.Empty;

    public DateTime? TarikhPinjam { get; set; }

    /// <summary>Tarikh jangkaan pulang — asas untuk peringatan lewat tempoh.</summary>
    public DateTime? TarikhJangkaPulang { get; set; }

    /// <summary>Pemohon mengaku menerima aset.</summary>
    public bool AkuanTerima { get; set; }
    public DateTime? TarikhAkuanTerima { get; set; }
}

public enum KondisiPulangan
{
    Baik = 1,
    Rosak = 2,
    Hilang = 3
}

/// <summary>Rekod pemulangan aset. Prefix AST-R.</summary>
public class AssetReturn
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    /// <summary>Pinjaman yang dipulangkan.</summary>
    public int AssetLoanRequestId { get; set; }
    public AssetLoanRequest? AssetLoanRequest { get; set; }

    public DateTime? TarikhPulang { get; set; }

    /// <summary>Kondisi semasa diterima — menentukan status aset selepas ini.</summary>
    public KondisiPulangan Kondisi { get; set; } = KondisiPulangan.Baik;

    public string? CatatanKondisi { get; set; }

    /// <summary>Diisi ICT semasa pemeriksaan.</summary>
    public string? CatatanIct { get; set; }
    public string? DiperiksaOlehUserId { get; set; }
}
```

> **`AssetId` nullable pada `AssetLoanRequest`** — pemohon meminta *"satu laptop"*, bukan *"laptop NRES-LT-0042"*. ICT memperuntukkan unit sebenar semasa kelulusan. Corak yang sama seperti nombor lot Kumpulan 2.

### ✅ Semakan

- [ ] Tiga entiti permohonan dalam `Models/Aset/`
- [ ] `AssetId` nullable — diperuntukkan semasa kelulusan
- [ ] `AssetReturn` memaut ke `AssetLoanRequest`, bukan terus ke `Asset`
- [ ] Sifar medan diduplikasi dari `Submission`

---

## Latihan 4 — Konfigurasi & seed katalog

**Objektif:** Daftar entiti + data katalog untuk bekerja dengannya.

### Langkah

`Models/Aset/Configurations/AsetConfigurations.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Aset.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.ToTable("Assets");

        builder.Property(a => a.AssetTag).HasMaxLength(30).IsRequired();
        builder.Property(a => a.SerialNumber).HasMaxLength(60).IsRequired();
        builder.Property(a => a.Nama).HasMaxLength(150).IsRequired();
        builder.Property(a => a.Jenama).HasMaxLength(60);
        builder.Property(a => a.Model).HasMaxLength(60);
        builder.Property(a => a.Lokasi).HasMaxLength(100);
        builder.Property(a => a.Catatan).HasMaxLength(1000);
        builder.Property(a => a.Harga).HasPrecision(12, 2);
        builder.Property(a => a.Status).HasConversion<int>();
        builder.Property(a => a.Kategori).HasConversion<int>();

        // Kedua-duanya unik — kita mencari mengikut kedua-duanya.
        builder.HasIndex(a => a.AssetTag).IsUnique();
        builder.HasIndex(a => a.SerialNumber).IsUnique();

        // "Aset apa yang tersedia dalam kategori ini?" — query terpanas kami.
        builder.HasIndex(a => new { a.Kategori, a.Status });

        // Data SINTETIK untuk latihan.
        builder.HasData(
            new Asset { Id = 1, AssetTag = "NRES-LT-0001", SerialNumber = "5CD001AB", Kategori = KategoriAset.Laptop,  Nama = "Laptop Pejabat", Jenama = "Dell",   Model = "Latitude 5450", Status = AssetStatus.Available, Lokasi = "Stor ICT" },
            new Asset { Id = 2, AssetTag = "NRES-LT-0002", SerialNumber = "5CD002AB", Kategori = KategoriAset.Laptop,  Nama = "Laptop Pejabat", Jenama = "Dell",   Model = "Latitude 5450", Status = AssetStatus.Available, Lokasi = "Stor ICT" },
            new Asset { Id = 3, AssetTag = "NRES-LT-0003", SerialNumber = "5CD003AB", Kategori = KategoriAset.Laptop,  Nama = "Laptop Pejabat", Jenama = "HP",     Model = "ProBook 450",   Status = AssetStatus.Available, Lokasi = "Stor ICT" },
            new Asset { Id = 4, AssetTag = "NRES-LT-0004", SerialNumber = "5CD004AB", Kategori = KategoriAset.Laptop,  Nama = "Laptop Pejabat", Jenama = "HP",     Model = "ProBook 450",   Status = AssetStatus.UnderMaintenance, Lokasi = "Bengkel" },
            new Asset { Id = 5, AssetTag = "NRES-PJ-0001", SerialNumber = "PJ001XY",  Kategori = KategoriAset.Projektor, Nama = "Projektor Bilik Mesyuarat", Jenama = "Epson", Model = "EB-X51", Status = AssetStatus.Available, Lokasi = "Stor ICT" },
            new Asset { Id = 6, AssetTag = "NRES-PJ-0002", SerialNumber = "PJ002XY",  Kategori = KategoriAset.Projektor, Nama = "Projektor Mudah Alih",      Jenama = "Epson", Model = "EB-W52", Status = AssetStatus.Available, Lokasi = "Stor ICT" },
            new Asset { Id = 7, AssetTag = "NRES-MN-0001", SerialNumber = "MN001ZZ",  Kategori = KategoriAset.Monitor, Nama = "Monitor 24\"",   Jenama = "Dell",  Model = "P2422H",       Status = AssetStatus.Available, Lokasi = "Stor ICT" },
            new Asset { Id = 8, AssetTag = "NRES-MN-0002", SerialNumber = "MN002ZZ",  Kategori = KategoriAset.Monitor, Nama = "Monitor 24\"",   Jenama = "Dell",  Model = "P2422H",       Status = AssetStatus.Available, Lokasi = "Stor ICT" });
    }
}

public class SoftwareCatalogItemConfiguration : IEntityTypeConfiguration<SoftwareCatalogItem>
{
    public void Configure(EntityTypeBuilder<SoftwareCatalogItem> builder)
    {
        builder.ToTable("SoftwareCatalogItems");

        builder.Property(s => s.Nama).HasMaxLength(150).IsRequired();
        builder.Property(s => s.Vendor).HasMaxLength(100);
        builder.Property(s => s.Versi).HasMaxLength(40);
        builder.Property(s => s.Catatan).HasMaxLength(1000);
        builder.Property(s => s.KosSetahun).HasPrecision(12, 2);
        builder.Property(s => s.JenisLesen).HasConversion<int>();

        builder.HasIndex(s => s.Nama);

        builder.HasData(
            new SoftwareCatalogItem { Id = 1, Nama = "Microsoft Office 365",  Vendor = "Microsoft", JenisLesen = JenisLesen.PerPengguna, JumlahLesen = 200, KosSetahun = 450m },
            new SoftwareCatalogItem { Id = 2, Nama = "Adobe Acrobat Pro",     Vendor = "Adobe",     JenisLesen = JenisLesen.PerPengguna, JumlahLesen = 15,  KosSetahun = 890m, PerluJustifikasi = true },
            new SoftwareCatalogItem { Id = 3, Nama = "AutoCAD",               Vendor = "Autodesk",  JenisLesen = JenisLesen.PerPeranti,  JumlahLesen = 5,   KosSetahun = 6500m, PerluJustifikasi = true },
            new SoftwareCatalogItem { Id = 4, Nama = "ArcGIS Desktop",        Vendor = "Esri",      JenisLesen = JenisLesen.Serentak,    JumlahLesen = 8,   KosSetahun = 9200m, PerluJustifikasi = true },
            new SoftwareCatalogItem { Id = 5, Nama = "Visual Studio Pro",     Vendor = "Microsoft", JenisLesen = JenisLesen.PerPengguna, JumlahLesen = 10,  KosSetahun = 5400m, PerluJustifikasi = true },
            new SoftwareCatalogItem { Id = 6, Nama = "7-Zip",                 Vendor = "Igor Pavlov", JenisLesen = JenisLesen.TanpaHad,  JumlahLesen = null },
            new SoftwareCatalogItem { Id = 7, Nama = "Mozilla Firefox",       Vendor = "Mozilla",   JenisLesen = JenisLesen.TanpaHad,    JumlahLesen = null },
            new SoftwareCatalogItem { Id = 8, Nama = "Zoom Workplace",        Vendor = "Zoom",      JenisLesen = JenisLesen.PerPengguna, JumlahLesen = 50,  KosSetahun = 720m });
    }
}

public class SoftwareRequestConfiguration : IEntityTypeConfiguration<SoftwareRequest>
{
    public void Configure(EntityTypeBuilder<SoftwareRequest> builder)
    {
        builder.ToTable("SoftwareRequests");

        builder.Property(r => r.Justifikasi).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.KunciLesen).HasMaxLength(200);

        builder.HasIndex(r => r.SubmissionId).IsUnique();
        builder.HasOne(r => r.Submission).WithOne()
            .HasForeignKey<SoftwareRequest>(r => r.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.SoftwareCatalogItem).WithMany()
            .HasForeignKey(r => r.SoftwareCatalogItemId)
            .OnDelete(DeleteBehavior.Restrict);

        // Kiraan lesen digunakan berjalan pada indeks ini.
        builder.HasIndex(r => r.SoftwareCatalogItemId);
    }
}

public class AssetLoanRequestConfiguration : IEntityTypeConfiguration<AssetLoanRequest>
{
    public void Configure(EntityTypeBuilder<AssetLoanRequest> builder)
    {
        builder.ToTable("AssetLoanRequests");

        builder.Property(r => r.Justifikasi).HasMaxLength(1000).IsRequired();
        builder.Property(r => r.KategoriDipohon).HasConversion<int>();

        builder.HasIndex(r => r.SubmissionId).IsUnique();
        builder.HasOne(r => r.Submission).WithOne()
            .HasForeignKey<AssetLoanRequest>(r => r.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Aset TIDAK dipadam bila permohonan dipadam.
        builder.HasOne(r => r.Asset).WithMany()
            .HasForeignKey(r => r.AssetId)
            .OnDelete(DeleteBehavior.Restrict);

        // "Adakah aset ini sedang dipinjam?" — semakan ketersediaan.
        builder.HasIndex(r => r.AssetId);

        // Peringatan lewat tempoh mengimbas medan ini.
        builder.HasIndex(r => r.TarikhJangkaPulang);
    }
}

public class AssetReturnConfiguration : IEntityTypeConfiguration<AssetReturn>
{
    public void Configure(EntityTypeBuilder<AssetReturn> builder)
    {
        builder.ToTable("AssetReturns");

        builder.Property(r => r.CatatanKondisi).HasMaxLength(1000);
        builder.Property(r => r.CatatanIct).HasMaxLength(1000);
        builder.Property(r => r.DiperiksaOlehUserId).HasMaxLength(450);
        builder.Property(r => r.Kondisi).HasConversion<int>();

        builder.HasIndex(r => r.SubmissionId).IsUnique();
        builder.HasOne(r => r.Submission).WithOne()
            .HasForeignKey<AssetReturn>(r => r.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Satu pinjaman = satu pemulangan.
        builder.HasIndex(r => r.AssetLoanRequestId).IsUnique();
        builder.HasOne(r => r.AssetLoanRequest).WithOne()
            .HasForeignKey<AssetReturn>(r => r.AssetLoanRequestId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
```

Sahkan:

```bash
git diff --name-only master     # Data/ApplicationDbContext.cs TIDAK sepatutnya muncul
```

### ✅ Semakan

- [ ] Lima konfigurasi dalam `Models/Aset/Configurations/`
- [ ] Indeks unik pada `AssetTag` **dan** `SerialNumber`
- [ ] Indeks komposit `(Kategori, Status)` untuk semakan ketersediaan
- [ ] 8 aset + 8 perisian berseed
- [ ] `git diff` menunjukkan tiada fail kongsi

---

## Latihan 5 — Pendaftaran modul & migration

### Langkah

1. `Models/Aset/AsetModuleDescriptor.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Aset;

public class AsetModuleDescriptor : IModuleDescriptorProvider
{
    public ModuleDescriptor Describe() => new(
        Code: ModuleCodes.PinjamanAset,
        Nama: "Perisian & Aset ICT",
        Controller: "Aset",
        Ikon: "bi-laptop",
        Roles: ["Applicant", "IctAdmin", "SystemAdmin"],
        Urutan: 4);
}
```

2. `Services/Aset/AsetModule.cs`:

```csharp
using Nres.Onboarding.Web.Models.Aset;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Aset;

public static class AsetModule
{
    public static IServiceCollection AddAsetModule(this IServiceCollection services)
    {
        services.AddScoped<IModuleDescriptorProvider, AsetModuleDescriptor>();
        return services;
    }
}
```

3. Nyahkomen **satu baris** dalam `Program.cs` (beritahu jurulatih):

```csharp
using Nres.Onboarding.Web.Services.Aset;

builder.Services.AddAsetModule();        // Kumpulan 4   ← nyahkomen INI sahaja
```

4. **Migration (slot!)** — umumkan, `pull --rebase`:

```bash
cd Nres.Onboarding.Web
dotnet ef migrations add AsetKatalogDanPermohonan
dotnet ef database update
dotnet run
cd ..
```

Lepaskan slot.

### ✅ Semakan

- [ ] Tepat satu baris dinyahkomen dalam `Program.cs`
- [ ] Migration mencipta 5 jadual + seed
- [ ] "Perisian & Aset ICT" muncul dalam navigasi

---

## Latihan 6 — Servis inventori

**Objektif:** Satu tempat yang menjawab "apa yang tersedia?"

### Langkah

1. `Services/Aset/IInventoryService.cs`:

```csharp
using Nres.Onboarding.Web.Models.Aset;

namespace Nres.Onboarding.Web.Services.Aset;

public record AsetTersedia(int Id, string AssetTag, string Nama,
    string? Jenama, string? Model);

public record LesenStatus(int Id, string Nama, int? Jumlah, int Diguna,
    int? Baki, bool Tersedia);

public interface IInventoryService
{
    /// <summary>Aset berstatus Available dalam kategori ini.</summary>
    Task<IReadOnlyList<AsetTersedia>> AvailableAssetsAsync(
        KategoriAset kategori, CancellationToken ct = default);

    /// <summary>Adakah aset ini bebas untuk dipinjamkan?</summary>
    Task<bool> IsAssetAvailableAsync(int assetId, CancellationToken ct = default);

    /// <summary>
    /// Status lesen bagi satu perisian. Kiraan DIKIRA daripada permohonan
    /// aktif — bukan medan tersimpan yang boleh tidak segerak.
    /// </summary>
    Task<LesenStatus> LicenceStatusAsync(int softwareId, CancellationToken ct = default);

    Task<IReadOnlyList<LesenStatus>> AllLicenceStatusAsync(
        CancellationToken ct = default);
}
```

2. `Services/Aset/InventoryService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Aset;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Aset;

public class InventoryService(ApplicationDbContext db) : IInventoryService
{
    /// <summary>Status permohonan yang MENGGUNAKAN lesen.</summary>
    private static readonly SubmissionStatus[] StatusAktif =
    [
        SubmissionStatus.Submitted,
        SubmissionStatus.SupervisorApproved,
        SubmissionStatus.AdminApproved,
        SubmissionStatus.Completed
    ];

    public async Task<IReadOnlyList<AsetTersedia>> AvailableAssetsAsync(
        KategoriAset kategori, CancellationToken ct = default) =>
        await db.Set<Asset>().AsNoTracking()
            .Where(a => a.IsActive
                     && a.Kategori == kategori
                     && a.Status == AssetStatus.Available)
            .OrderBy(a => a.AssetTag)
            .Select(a => new AsetTersedia(a.Id, a.AssetTag, a.Nama, a.Jenama, a.Model))
            .ToListAsync(ct);

    public async Task<bool> IsAssetAvailableAsync(
        int assetId, CancellationToken ct = default) =>
        await db.Set<Asset>().AsNoTracking()
            .AnyAsync(a => a.Id == assetId
                        && a.IsActive
                        && a.Status == AssetStatus.Available, ct);

    public async Task<LesenStatus> LicenceStatusAsync(
        int softwareId, CancellationToken ct = default)
    {
        var sw = await db.Set<SoftwareCatalogItem>().AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == softwareId, ct)
            ?? throw new InvalidOperationException("Perisian tidak dijumpai.");

        var diguna = await KiraDigunaAsync(softwareId, ct);
        return Bina(sw, diguna);
    }

    public async Task<IReadOnlyList<LesenStatus>> AllLicenceStatusAsync(
        CancellationToken ct = default)
    {
        var senarai = await db.Set<SoftwareCatalogItem>().AsNoTracking()
            .Where(s => s.IsActive).OrderBy(s => s.Nama).ToListAsync(ct);

        // Satu query kumpulan untuk SEMUA kiraan — bukan satu per perisian (N+1).
        var kiraan = await (
            from r in db.Set<SoftwareRequest>().AsNoTracking()
            join s in db.Submissions.AsNoTracking() on r.SubmissionId equals s.Id
            where StatusAktif.Contains(s.Status)
            group r by r.SoftwareCatalogItemId into g
            select new { SoftwareId = g.Key, Kiraan = g.Count() })
            .ToDictionaryAsync(x => x.SoftwareId, x => x.Kiraan, ct);

        return senarai
            .Select(sw => Bina(sw, kiraan.GetValueOrDefault(sw.Id)))
            .ToList();
    }

    private async Task<int> KiraDigunaAsync(int softwareId, CancellationToken ct) =>
        await (from r in db.Set<SoftwareRequest>().AsNoTracking()
               join s in db.Submissions.AsNoTracking() on r.SubmissionId equals s.Id
               where r.SoftwareCatalogItemId == softwareId
                  && StatusAktif.Contains(s.Status)
               select r.Id).CountAsync(ct);

    private static LesenStatus Bina(SoftwareCatalogItem sw, int diguna)
    {
        // JumlahLesen null = tanpa had (perisian percuma / lesen tapak).
        var baki = sw.JumlahLesen is null ? (int?)null : sw.JumlahLesen - diguna;
        var tersedia = sw.JumlahLesen is null || baki > 0;

        return new LesenStatus(sw.Id, sw.Nama, sw.JumlahLesen, diguna, baki, tersedia);
    }
}
```

3. Daftar dalam `AsetModule`:

```csharp
services.AddScoped<IInventoryService, InventoryService>();
```

> **`AllLicenceStatusAsync` menggunakan satu query kumpulan** untuk semua kiraan. Versi naif memanggil `LicenceStatusAsync` dalam gelung — 8 perisian = 16 query. Anda akan mengukur ini pada Hari 13–14.

### ✅ Semakan

- [ ] Servis dalam `Services/Aset/`
- [ ] Kiraan lesen **dikira**, bukan disimpan
- [ ] `AllLicenceStatusAsync` menggunakan satu query kumpulan (tiada N+1)
- [ ] Lesen tanpa had (`JumlahLesen = null`) sentiasa tersedia
- [ ] Didaftar dalam `AsetModule`

---

## Latihan 7 — Halaman utama modul

**Objektif:** Katalog yang boleh dilihat pemohon.

### Langkah

`Controllers/AsetController.cs`:

```csharp
[Authorize]
public class AsetController(
    ApplicationDbContext db,
    ICurrentUserService currentUser,
    IInventoryService inventory) : Controller
{
    private static readonly string[] KodModul =
        [ModuleCodes.Perisian, ModuleCodes.PinjamanAset, ModuleCodes.PemulanganAset];

    public async Task<IActionResult> Index()
    {
        var userId = currentUser.UserId!;

        var vm = new AsetIndexViewModel
        {
            Lesen = await inventory.AllLicenceStatusAsync(),

            AsetMengikutKategori = await db.Set<Asset>().AsNoTracking()
                .Where(a => a.IsActive)
                .GroupBy(a => a.Kategori)
                .Select(g => new AsetIndexViewModel.KategoriRingkas(
                    g.Key,
                    g.Count(),
                    g.Count(a => a.Status == AssetStatus.Available)))
                .ToListAsync(),

            Permohonan = await db.Submissions.AsNoTracking()
                .Where(s => KodModul.Contains(s.ModuleCode)
                         && s.ApplicantUserId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .Take(15)
                .Select(s => new AsetIndexViewModel.PermohonanRingkas(
                    s.Id, s.ReferenceNo, s.ModuleCode, s.Status, s.CreatedAt))
                .ToListAsync()
        };

        return View(vm);
    }
}
```

View — bahagian katalog:

```cshtml
<h5 class="mt-4">Katalog Perisian</h5>
<table class="table table-sm">
    <thead><tr><th>Perisian</th><th>Jenis lesen</th><th class="text-end">Baki</th><th></th></tr></thead>
    <tbody>
    @foreach (var l in Model.Lesen)
    {
        <tr>
            <td>@l.Nama</td>
            <td>@(l.Jumlah is null ? "Tanpa had" : $"{l.Diguna}/{l.Jumlah} diguna")</td>
            <td class="text-end">
                @if (l.Jumlah is null)
                {
                    <span class="badge bg-success">∞</span>
                }
                else
                {
                    <span class="badge @(l.Baki > 3 ? "bg-success" : l.Baki > 0 ? "bg-warning text-dark" : "bg-danger")">
                        @l.Baki
                    </span>
                }
            </td>
            <td class="text-end">
                @if (l.Tersedia)
                {
                    <a asp-controller="Software" asp-action="Create" asp-route-id="@l.Id"
                       class="btn btn-sm btn-primary">Mohon</a>
                }
                else
                {
                    <span class="text-muted small">Tiada lesen</span>
                }
            </td>
        </tr>
    }
    </tbody>
</table>

<h5 class="mt-4">Aset Boleh Dipinjam</h5>
<div class="row g-3">
@foreach (var k in Model.AsetMengikutKategori)
{
    <div class="col-md-3">
        <div class="card h-100"><div class="card-body">
            <h6 class="card-title">@k.Kategori</h6>
            <p class="card-text">
                <span class="fs-3">@k.Tersedia</span>
                <span class="text-muted">/ @k.Jumlah tersedia</span>
            </p>
            @if (k.Tersedia > 0)
            {
                <a asp-controller="Asset" asp-action="Create"
                   asp-route-kategori="@((int)k.Kategori)"
                   class="btn btn-sm btn-primary">Mohon Pinjam</a>
            }
            else
            {
                <span class="text-muted small">Tiada unit tersedia</span>
            }
        </div></div>
    </div>
}
</div>
```

> Pautan `Create` menunjuk ke controller yang anda bina pada **Hari 5–6** — 404 hari ini, dijangka.

### ✅ Semakan

- [ ] Katalog perisian menunjukkan baki lesen dengan lencana berwarna
- [ ] Lesen tanpa had menunjukkan ∞
- [ ] Aset dikumpulkan mengikut kategori dengan kiraan tersedia
- [ ] Butang "Mohon" disembunyikan bila tiada stok

---

## Latihan 8 — Tutup hari

```bash
git diff --name-only master
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Binaan bersih; katalog dipaparkan dengan data berseed
- [ ] `AssetStatus` ialah enum **anda** — `SubmissionStatus` tidak disentuh
- [ ] Kiraan lesen dikira, bukan disimpan
- [ ] Hanya fail Kumpulan 4 disentuh (+1 baris `Program.cs`)
- [ ] Migration melalui slot
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 4

| Artifak | Lokasi |
|---------|--------|
| `Asset` + `AssetStatus`, `SoftwareCatalogItem` | `Models/Aset/` |
| 3 entiti permohonan | `Models/Aset/AsetApplications.cs` |
| 5 konfigurasi + seed (8 aset, 8 perisian) | `Models/Aset/Configurations/` |
| Migration `AsetKatalogDanPermohonan` | `Migrations/` |
| `IInventoryService` | `Services/Aset/` |
| Halaman utama katalog | `Controllers/AsetController.cs`, `Views/Aset/Index.cshtml` |

**Seterusnya (Hari 5–6):** borang permohonan dengan **semakan stok masa-nyata** dan borang akuan penerimaan.
