# Lab · Kumpulan 2 · Hari 4 — Skema Akses & Kenderaan

> Konsep: [`../README.md`](../README.md) · Kanun: [`../../../SPEC-KURSUS.md`](../../../SPEC-KURSUS.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula hari dengan betul

```bash
git switch kump-2/akses-kenderaan
git pull --rebase origin master
git switch -c kump-2/feat/skema-akses-kenderaan
dotnet build
```

**Semakan "sudah wujud?"** sebelum menulis apa-apa:

```bash
grep -ri "Vehicle\|AccessPass" Nres.Onboarding.Web/
grep -ri "IReferenceNumberService" Nres.Onboarding.Web/Services/
```

`IReferenceNumberService` **sudah wujud** — anda menggunakannya pada Hari 5–6, tidak menulis satu lagi.

### ✅ Semakan

- [ ] `dotnet build` berjaya pada cabang kumpulan
- [ ] Anda mengesahkan servis kongsi wujud
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Entiti `Vehicle`

**Objektif:** Kenderaan sebagai entiti kelas pertama, dengan nombor plat dinormalkan.

### Langkah

1. `Models/Akses/Vehicle.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Akses;

public enum JenisKenderaan
{
    Kereta = 1,
    Motosikal = 2,
    Van = 3,
    Lain = 9
}

/// <summary>
/// Kenderaan yang didaftarkan oleh staf. BEBAS daripada mana-mana permohonan —
/// satu staf boleh ada beberapa kenderaan, dan satu kenderaan boleh ada banyak
/// permohonan (pelekat tahun ini, pelekat tahun depan, parkir).
///
/// Ini yang menjadikan semakan pendua nombor plat mudah.
/// </summary>
public class Vehicle
{
    public int Id { get; set; }

    /// <summary>Pemilik — id pengguna Identity.</summary>
    public string OwnerUserId { get; set; } = string.Empty;

    /// <summary>Seperti ditaip pengguna, untuk PAPARAN. Cth: "WXY 1234".</summary>
    public string PlateNumber { get; set; } = string.Empty;

    /// <summary>
    /// Huruf besar, tiada ruang/sengkang. Untuk CARIAN dan kekangan unik.
    /// Satu medan untuk manusia, satu untuk mesin.
    /// </summary>
    public string PlateNumberNormalized { get; set; } = string.Empty;

    public JenisKenderaan Jenis { get; set; } = JenisKenderaan.Kereta;
    public string? Jenama { get; set; }
    public string? Model { get; set; }
    public string? Warna { get; set; }

    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>Normalkan nombor plat — SATU tempat, digunakan di mana-mana.</summary>
    public static string Normalize(string plat) =>
        new string(plat.Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
}
```

2. Perhatikan kaedah statik `Normalize`. Ia hidup pada entiti supaya **setiap** tempat yang menormalkan nombor plat menggunakan logik yang sama. Jika ia disalin ke tiga controller, satu daripadanya akan terlepas kes tepi.

### ✅ Semakan

- [ ] Fail dalam `Models/Akses/`
- [ ] Kedua-dua `PlateNumber` dan `PlateNumberNormalized` wujud
- [ ] `Normalize` ialah kaedah statik tunggal
- [ ] `dotnet build` berjaya

---

## Latihan 2 — Tiga jadual permohonan

**Objektif:** Satu jadual detail bagi setiap jenis permohonan.

### Langkah

1. `Models/Akses/AccessPassApplication.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Akses;

public enum JenisPas
{
    Staf = 1,
    Pelawat = 2,
    Kontraktor = 3
}

/// <summary>
/// Permohonan pas keselamatan. TIDAK terikat kenderaan.
/// Nombor rujukan, status, pemohon ada dalam Submission induk.
/// </summary>
public class AccessPassApplication
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public JenisPas JenisPas { get; set; } = JenisPas.Staf;

    /// <summary>Nama pemegang pas — mungkin bukan pemohon (cth. pelawat).</summary>
    public string HolderName { get; set; } = string.Empty;
    public string HolderIdentityNo { get; set; } = string.Empty;

    /// <summary>Wajib untuk Pelawat & Kontraktor; pilihan untuk Staf.</summary>
    public string? PurposeOfVisit { get; set; }

    /// <summary>Syarikat — wajib untuk Kontraktor.</summary>
    public string? CompanyName { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }

    /// <summary>Kawasan yang dibenarkan, dipisah koma. Diisi semasa kelulusan.</summary>
    public string? AllowedAreas { get; set; }

    /// <summary>Nombor siri pas fizikal — diberikan semasa kelulusan.</summary>
    public string? PassSerialNo { get; set; }
}
```

2. `Models/Akses/VehicleStickerApplication.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Akses;

/// <summary>Permohonan pelekat kenderaan. Terikat kepada satu Vehicle.</summary>
public class VehicleStickerApplication
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    /// <summary>Tahun pelekat, cth. 2026.</summary>
    public int TahunPelekat { get; set; } = DateTime.UtcNow.Year;

    /// <summary>Salinan geran/kad pendaftaran dilampirkan — semakan pada Hari 5–6.</summary>
    public bool GeranDilampirkan { get; set; }

    /// <summary>Nombor siri pelekat — diberikan semasa kelulusan.</summary>
    public string? StickerSerialNo { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
```

3. `Models/Akses/ParkingApplication.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Akses;

public enum JenisParkir
{
    Biasa = 1,
    OKU = 2,
    Eksekutif = 3,
    Sementara = 4
}

/// <summary>Permohonan lot parkir. Terikat kepada satu Vehicle.</summary>
public class ParkingApplication
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public int VehicleId { get; set; }
    public Vehicle? Vehicle { get; set; }

    public JenisParkir JenisParkir { get; set; } = JenisParkir.Biasa;

    /// <summary>Wajib untuk OKU, Eksekutif dan Sementara — bukan Biasa.</summary>
    public string? Justifikasi { get; set; }

    /// <summary>Nombor lot — diperuntukkan semasa kelulusan, bukan dimohon.</summary>
    public string? LotNumber { get; set; }

    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
}
```

4. **Semakan anti-duplikasi.** Bagi setiap ketiga-tiga kelas, sahkan **tiada**: `ReferenceNo`, `Status`, `ApplicantUserId`, `SubmittedAt`. Kesemuanya ada dalam `Submission`.

> **Perhatikan `PassSerialNo`, `StickerSerialNo`, `LotNumber` semuanya nullable.** Ia diberikan **semasa kelulusan**, bukan dimohon. Ini akan penting pada Hari 7–9.

### ✅ Semakan

- [ ] Ketiga-tiga kelas dalam `Models/Akses/`
- [ ] Setiap satu memaut ke `Submission` melalui `SubmissionId`
- [ ] Pelekat & parkir memaut ke `Vehicle`; pas tidak
- [ ] **Sifar** medan diduplikasi dari `Submission`
- [ ] Medan yang diberikan semasa kelulusan adalah nullable

---

## Latihan 3 — Konfigurasi EF Core

**Objektif:** Daftar keempat-empat entiti **tanpa menyentuh `ApplicationDbContext`**.

### Langkah

1. `Models/Akses/Configurations/VehicleConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Akses.Configurations;

public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");

        builder.Property(v => v.OwnerUserId).HasMaxLength(450).IsRequired();
        builder.Property(v => v.PlateNumber).HasMaxLength(20).IsRequired();
        builder.Property(v => v.PlateNumberNormalized).HasMaxLength(20).IsRequired();
        builder.Property(v => v.Jenama).HasMaxLength(60);
        builder.Property(v => v.Model).HasMaxLength(60);
        builder.Property(v => v.Warna).HasMaxLength(40);
        builder.Property(v => v.Jenis).HasConversion<int>();

        // Satu kenderaan = satu pendaftaran dalam sistem. Kekangan UNIK ini
        // ialah pertahanan terakhir semakan pendua kami — walaupun kod
        // terlepas, pangkalan data tidak.
        builder.HasIndex(v => v.PlateNumberNormalized)
            .IsUnique()
            .HasDatabaseName("IX_Vehicles_PlateNormalized");

        // "Kenderaan saya" muncul pada setiap borang.
        builder.HasIndex(v => v.OwnerUserId);
    }
}
```

2. `Models/Akses/Configurations/AksesApplicationConfigurations.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Akses.Configurations;

public class AccessPassApplicationConfiguration
    : IEntityTypeConfiguration<AccessPassApplication>
{
    public void Configure(EntityTypeBuilder<AccessPassApplication> builder)
    {
        builder.ToTable("AccessPassApplications");

        builder.Property(a => a.HolderName).HasMaxLength(200).IsRequired();
        builder.Property(a => a.HolderIdentityNo).HasMaxLength(20).IsRequired();
        builder.Property(a => a.PurposeOfVisit).HasMaxLength(500);
        builder.Property(a => a.CompanyName).HasMaxLength(200);
        builder.Property(a => a.AllowedAreas).HasMaxLength(500);
        builder.Property(a => a.PassSerialNo).HasMaxLength(40);
        builder.Property(a => a.JenisPas).HasConversion<int>();

        builder.HasIndex(a => a.SubmissionId).IsUnique();
        builder.HasOne(a => a.Submission).WithOne()
            .HasForeignKey<AccessPassApplication>(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Nombor siri pas mesti unik apabila diberikan (kosong semasa draf).
        builder.HasIndex(a => a.PassSerialNo)
            .IsUnique()
            .HasFilter("\"PassSerialNo\" IS NOT NULL");
    }
}

public class VehicleStickerApplicationConfiguration
    : IEntityTypeConfiguration<VehicleStickerApplication>
{
    public void Configure(EntityTypeBuilder<VehicleStickerApplication> builder)
    {
        builder.ToTable("VehicleStickerApplications");

        builder.Property(a => a.StickerSerialNo).HasMaxLength(40);

        builder.HasIndex(a => a.SubmissionId).IsUnique();
        builder.HasOne(a => a.Submission).WithOne()
            .HasForeignKey<VehicleStickerApplication>(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Kenderaan TIDAK dipadam apabila permohonan dipadam.
        builder.HasOne(a => a.Vehicle).WithMany()
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Semakan pendua berjalan pada gabungan ini pada setiap penghantaran.
        builder.HasIndex(a => new { a.VehicleId, a.TahunPelekat });

        builder.HasIndex(a => a.StickerSerialNo)
            .IsUnique()
            .HasFilter("\"StickerSerialNo\" IS NOT NULL");
    }
}

public class ParkingApplicationConfiguration
    : IEntityTypeConfiguration<ParkingApplication>
{
    public void Configure(EntityTypeBuilder<ParkingApplication> builder)
    {
        builder.ToTable("ParkingApplications");

        builder.Property(a => a.Justifikasi).HasMaxLength(1000);
        builder.Property(a => a.LotNumber).HasMaxLength(20);
        builder.Property(a => a.JenisParkir).HasConversion<int>();

        builder.HasIndex(a => a.SubmissionId).IsUnique();
        builder.HasOne(a => a.Submission).WithOne()
            .HasForeignKey<ParkingApplication>(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(a => a.Vehicle).WithMany()
            .HasForeignKey(a => a.VehicleId)
            .OnDelete(DeleteBehavior.Restrict);

        // Satu lot = satu peruntukan aktif. Dikuatkuasakan pada Hari 7–9.
        builder.HasIndex(a => a.LotNumber);
        builder.HasIndex(a => a.VehicleId);
    }
}
```

3. **Sahkan anda tidak menyentuh fail kongsi:**

```bash
git diff --name-only master
```

`Data/ApplicationDbContext.cs` **tidak** sepatutnya muncul.

### ✅ Semakan

- [ ] Konfigurasi dalam `Models/Akses/Configurations/`
- [ ] Indeks unik pada `PlateNumberNormalized`
- [ ] `Vehicle` guna `DeleteBehavior.Restrict` — kenderaan bertahan lebih lama daripada permohonan
- [ ] Indeks unik ditapis pada nombor siri
- [ ] `git diff` menunjukkan tiada fail kongsi

---

## Latihan 4 — Pendaftaran modul & navigasi

**Objektif:** Sambungkan modul dengan menambah fail.

### Langkah

1. `Models/Akses/AksesModuleDescriptor.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Akses;

public class AksesModuleDescriptor : IModuleDescriptorProvider
{
    public ModuleDescriptor Describe() => new(
        Code: ModuleCodes.PasKeselamatan,
        Nama: "Pas, Parkir & Pelekat",
        Controller: "Akses",
        Ikon: "bi-shield-check",
        Roles: ["Applicant", "SecurityAdmin", "SystemAdmin"],
        Urutan: 2);
}
```

2. `Services/Akses/AksesModule.cs`:

```csharp
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Akses;

public static class AksesModule
{
    public static IServiceCollection AddAksesModule(this IServiceCollection services)
    {
        services.AddScoped<IModuleDescriptorProvider, AksesModuleDescriptor>();
        // Servis modul ditambah DI SINI apabila kami menciptanya.
        return services;
    }
}
```

3. **Satu-satunya suntingan fail kongsi hari ini.** Beritahu jurulatih, kemudian nyahkomen **satu baris** dalam `Program.cs`:

```csharp
using Nres.Onboarding.Web.Services.Akses;   // ← tambah using

builder.Services.AddAksesModule();       // Kumpulan 2   ← nyahkomen INI sahaja
```

> ⚠️ Jangan nyahkomen baris kumpulan lain — binaan gagal untuk semua orang.

### ✅ Semakan

- [ ] Descriptor & modul dalam folder anda
- [ ] Tepat **satu** baris dinyahkomen dalam `Program.cs`
- [ ] `dotnet build` berjaya

---

## Latihan 5 — Migration (slot!)

### Langkah

1. Umumkan: *"Kumpulan 2 mengambil slot migration."*

2. ```bash
   git pull --rebase origin master
   cd Nres.Onboarding.Web
   dotnet ef migrations add AksesVehicleDanPermohonan
   ```

3. **Baca fail yang dijana.** Sahkan ia mencipta `Vehicles`, `AccessPassApplications`, `VehicleStickerApplications`, `ParkingApplications` — dan **tiada jadual kumpulan lain**.

4. ```bash
   dotnet ef database update
   dotnet run
   cd ..
   ```

5. Commit, push, lepaskan slot.

### Jika snapshot berkonflik

```bash
git checkout --theirs Migrations/ApplicationDbContextModelSnapshot.cs
rm Migrations/*_AksesVehicleDanPermohonan.cs Migrations/*_AksesVehicleDanPermohonan.Designer.cs
git pull --rebase origin master
dotnet ef migrations add AksesVehicleDanPermohonan
dotnet ef database update
```

### ✅ Semakan

- [ ] Slot diumumkan & dilepaskan
- [ ] Migration hanya menyentuh empat jadual anda
- [ ] Aplikasi bermula; "Pas, Parkir & Pelekat" muncul dalam navigasi

---

## Latihan 6 — Servis kenderaan

**Objektif:** Satu tempat untuk mendaftar dan mencari kenderaan.

### Langkah

1. `Services/Akses/IVehicleService.cs`:

```csharp
using Nres.Onboarding.Web.Models.Akses;

namespace Nres.Onboarding.Web.Services.Akses;

public interface IVehicleService
{
    Task<IReadOnlyList<Vehicle>> MyVehiclesAsync(string userId, CancellationToken ct = default);

    /// <summary>
    /// Daftar kenderaan, atau kembalikan yang sedia ada jika plat sudah didaftar
    /// oleh pengguna INI. Melontar jika plat didaftar oleh orang LAIN.
    /// </summary>
    Task<Vehicle> RegisterOrGetAsync(string userId, string plateNumber,
        JenisKenderaan jenis, string? jenama, string? model, string? warna,
        CancellationToken ct = default);

    Task<Vehicle?> FindByPlateAsync(string plateNumber, CancellationToken ct = default);
}
```

2. `Services/Akses/VehicleService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Akses;

namespace Nres.Onboarding.Web.Services.Akses;

public class VehicleService(ApplicationDbContext db) : IVehicleService
{
    public async Task<IReadOnlyList<Vehicle>> MyVehiclesAsync(
        string userId, CancellationToken ct = default) =>
        await db.Set<Vehicle>().AsNoTracking()
            .Where(v => v.OwnerUserId == userId && v.IsActive)
            .OrderBy(v => v.PlateNumber)
            .ToListAsync(ct);

    public async Task<Vehicle> RegisterOrGetAsync(string userId, string plateNumber,
        JenisKenderaan jenis, string? jenama, string? model, string? warna,
        CancellationToken ct = default)
    {
        var normalized = Vehicle.Normalize(plateNumber);

        if (string.IsNullOrWhiteSpace(normalized))
            throw new InvalidOperationException("Nombor plat tidak sah.");

        var sediaAda = await db.Set<Vehicle>()
            .FirstOrDefaultAsync(v => v.PlateNumberNormalized == normalized, ct);

        if (sediaAda is not null)
        {
            // Kenderaan yang sama, pemilik yang berbeza — ini masalah sebenar,
            // bukan pendua yang tidak berbahaya. Pegawai Keselamatan perlu tahu.
            if (sediaAda.OwnerUserId != userId)
                throw new InvalidOperationException(
                    $"Nombor plat {sediaAda.PlateNumber} telah didaftarkan oleh " +
                    "staf lain. Sila hubungi Bahagian Keselamatan.");

            return sediaAda;
        }

        var kenderaan = new Vehicle
        {
            OwnerUserId = userId,
            PlateNumber = plateNumber.Trim(),
            PlateNumberNormalized = normalized,
            Jenis = jenis,
            Jenama = jenama,
            Model = model,
            Warna = warna
        };

        db.Set<Vehicle>().Add(kenderaan);
        await db.SaveChangesAsync(ct);
        return kenderaan;
    }

    public async Task<Vehicle?> FindByPlateAsync(
        string plateNumber, CancellationToken ct = default)
    {
        var normalized = Vehicle.Normalize(plateNumber);
        return await db.Set<Vehicle>().AsNoTracking()
            .FirstOrDefaultAsync(v => v.PlateNumberNormalized == normalized, ct);
    }
}
```

3. Daftar dalam modul anda:

```csharp
services.AddScoped<IVehicleService, VehicleService>();
```

> **Perhatikan** `RegisterOrGetAsync` menggunakan `Vehicle.Normalize` — bukan salinannya sendiri. Satu logik normalisasi, satu tempat.

### ✅ Semakan

- [ ] Servis dalam `Services/Akses/`
- [ ] Guna `Vehicle.Normalize`, tidak menyalin logiknya
- [ ] Plat yang didaftar oleh pengguna lain melontar mesej jelas
- [ ] Didaftar dalam `AksesModule`

---

## Latihan 7 — Halaman utama modul

**Objektif:** Landing yang menunjukkan tiga laluan permohonan dan permohonan saya.

### Langkah

1. `ViewModels/Akses/AksesIndexViewModel.cs`:

```csharp
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Akses;

public class AksesIndexViewModel
{
    public IReadOnlyList<Vehicle> Kenderaan { get; set; } = [];
    public IReadOnlyList<PermohonanRingkas> Permohonan { get; set; } = [];

    public record PermohonanRingkas(
        int SubmissionId, int ApplicationId, string ReferenceNo,
        string JenisNama, string Controller,
        SubmissionStatus Status, DateTime CreatedAt);
}
```

2. `Controllers/AksesController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.Services.Akses;
using Nres.Onboarding.Web.ViewModels.Akses;

namespace Nres.Onboarding.Web.Controllers;

/// <summary>Halaman utama modul — tiga laluan permohonan + permohonan saya.</summary>
[Authorize]
public class AksesController(
    ApplicationDbContext db,
    ICurrentUserService currentUser,
    IVehicleService vehicles) : Controller
{
    private static readonly string[] KodModul =
        [ModuleCodes.PasKeselamatan, ModuleCodes.Parkir, ModuleCodes.PelekatKenderaan];

    public async Task<IActionResult> Index()
    {
        var userId = currentUser.UserId!;

        var submissions = await db.Submissions.AsNoTracking()
            .Where(s => KodModul.Contains(s.ModuleCode) && s.ApplicantUserId == userId)
            .OrderByDescending(s => s.CreatedAt)
            .Take(20)
            .ToListAsync();

        var ids = submissions.Select(s => s.Id).ToList();

        // Cari id permohonan detail bagi setiap jenis, dalam tiga query kecil.
        var pas = await db.Set<AccessPassApplication>().AsNoTracking()
            .Where(a => ids.Contains(a.SubmissionId))
            .ToDictionaryAsync(a => a.SubmissionId, a => a.Id);
        var pelekat = await db.Set<VehicleStickerApplication>().AsNoTracking()
            .Where(a => ids.Contains(a.SubmissionId))
            .ToDictionaryAsync(a => a.SubmissionId, a => a.Id);
        var parkir = await db.Set<ParkingApplication>().AsNoTracking()
            .Where(a => ids.Contains(a.SubmissionId))
            .ToDictionaryAsync(a => a.SubmissionId, a => a.Id);

        var senarai = submissions.Select(s =>
        {
            var (nama, controller, appId) = s.ModuleCode switch
            {
                ModuleCodes.PasKeselamatan   =>
                    ("Pas Keselamatan",  "AccessPass",     pas.GetValueOrDefault(s.Id)),
                ModuleCodes.PelekatKenderaan =>
                    ("Pelekat Kenderaan", "VehicleSticker", pelekat.GetValueOrDefault(s.Id)),
                ModuleCodes.Parkir           =>
                    ("Lot Parkir",       "Parking",        parkir.GetValueOrDefault(s.Id)),
                _ => (s.ModuleCode, "Akses", 0)
            };

            return new AksesIndexViewModel.PermohonanRingkas(
                s.Id, appId, s.ReferenceNo, nama, controller, s.Status, s.CreatedAt);
        }).ToList();

        return View(new AksesIndexViewModel
        {
            Kenderaan = await vehicles.MyVehiclesAsync(userId),
            Permohonan = senarai
        });
    }
}
```

3. `Views/Akses/Index.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Akses.AksesIndexViewModel
@{ ViewData["Title"] = "Pas, Parkir & Pelekat Kenderaan"; }

<h2>@ViewData["Title"]</h2>
<p class="text-muted">Permohonan akses kawasan dan keselamatan kenderaan.</p>

<div class="row g-3 my-4">
    <div class="col-md-4">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title">Pas Keselamatan</h5>
                <p class="card-text small text-muted">
                    Akses kawasan untuk staf, pelawat atau kontraktor.
                </p>
                <a asp-controller="AccessPass" asp-action="Create" class="btn btn-primary">
                    Mohon Pas
                </a>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title">Pelekat Kenderaan</h5>
                <p class="card-text small text-muted">
                    Pelekat tahunan untuk kenderaan berdaftar.
                </p>
                <a asp-controller="VehicleSticker" asp-action="Create" class="btn btn-primary">
                    Mohon Pelekat
                </a>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title">Lot Parkir</h5>
                <p class="card-text small text-muted">
                    Peruntukan lot parkir biasa, OKU atau eksekutif.
                </p>
                <a asp-controller="Parking" asp-action="Create" class="btn btn-primary">
                    Mohon Parkir
                </a>
            </div>
        </div>
    </div>
</div>

<h5 class="mt-4">Kenderaan Berdaftar Saya</h5>
<table class="table table-sm">
    <thead><tr><th>No. Plat</th><th>Jenis</th><th>Jenama / Model</th><th>Warna</th></tr></thead>
    <tbody>
    @if (!Model.Kenderaan.Any())
    {
        <tr><td colspan="4" class="text-muted">
            Tiada kenderaan berdaftar. Kenderaan didaftarkan secara automatik
            apabila anda memohon pelekat atau parkir.
        </td></tr>
    }
    @foreach (var v in Model.Kenderaan)
    {
        <tr>
            <td><strong>@v.PlateNumber</strong></td>
            <td>@v.Jenis</td>
            <td>@v.Jenama @v.Model</td>
            <td>@v.Warna</td>
        </tr>
    }
    </tbody>
</table>

<h5 class="mt-4">Permohonan Saya</h5>
<table class="table table-hover">
    <thead>
        <tr><th>No. Rujukan</th><th>Jenis</th><th>Status</th><th>Tarikh</th><th></th></tr>
    </thead>
    <tbody>
    @if (!Model.Permohonan.Any())
    {
        <tr><td colspan="5" class="text-muted">Tiada permohonan lagi.</td></tr>
    }
    @foreach (var p in Model.Permohonan)
    {
        <tr>
            <td>@(string.IsNullOrEmpty(p.ReferenceNo) ? "(draf)" : p.ReferenceNo)</td>
            <td>@p.JenisNama</td>
            <td><partial name="_StatusBadge" model="p.Status" /></td>
            <td>@p.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy")</td>
            <td class="text-end">
                <a asp-controller="@p.Controller" asp-action="Edit" asp-route-id="@p.ApplicationId"
                   class="btn btn-sm btn-outline-primary">Buka</a>
            </td>
        </tr>
    }
    </tbody>
</table>
```

> Pautan `Create`/`Edit` menunjuk kepada controller yang anda bina pada **Hari 5–6**. Ia akan 404 hari ini — itu dijangka.

### ✅ Semakan

- [ ] Halaman utama memaparkan tiga kad permohonan
- [ ] Kenderaan berdaftar disenaraikan (kosong buat masa ini)
- [ ] Guna `_StatusBadge` **kongsi**
- [ ] Modul boleh dicapai daripada navigasi

---

## Latihan 8 — Tutup hari

```bash
git diff --name-only master     # hanya fail Akses + 1 baris Program.cs
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Binaan bersih; aplikasi bermula; halaman utama modul berfungsi
- [ ] Servis/komponen kongsi digunakan
- [ ] Hanya fail Kumpulan 2 disentuh (+1 baris `Program.cs`)
- [ ] Migration melalui slot
- [ ] Kod jana-AI difahami
- [ ] Disemak rakan sekumpulan
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 4

| Artifak | Lokasi |
|---------|--------|
| `Vehicle` + 3 entiti permohonan | `Models/Akses/` |
| Konfigurasi EF Core | `Models/Akses/Configurations/` |
| Pendaftaran modul + descriptor | `Services/Akses/`, `Models/Akses/` |
| Migration `AksesVehicleDanPermohonan` | `Migrations/` |
| `IVehicleService` | `Services/Akses/` |
| Halaman utama modul | `Controllers/AksesController.cs`, `Views/Akses/Index.cshtml` |

**Seterusnya (Hari 5–6):** tiga borang permohonan, conditional validation, dan **semakan pendua nombor plat**.
