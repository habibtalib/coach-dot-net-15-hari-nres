# Lab Hari 4 — Pemodelan Modul 2 (Pas, Parking & Pelekat Kenderaan)

Lab ini mengiringi [`README.md`](../README.md) Hari 4. Ikut latihan **secara berurutan** — setiap latihan bina di atas latihan sebelumnya, dan di atas projek `Nres.Onboarding.Web` yang sudah wujud sejak Hari 1. Rujuk [`../../projek/`](../../projek/) untuk **banding** kod anda selepas cuba sendiri (projek rujukan penuh dikemas kini secara kumulatif sepanjang kursus).

> **Peraturan lab:** Cuba tulis kod **sendiri** dahulu berdasarkan penerangan dalam README sebelum tengok fail rujukan.

---

## Latihan 0 — Sahkan Projek Sedia Ada

**Objektif:** Pastikan projek Hari 1–3 masih berjalan sebelum menambah Modul 2.

1. Buka terminal di root projek `Nres.Onboarding.Web` dan jalankan:

   ```bash
   dotnet build
   dotnet ef migrations list
   ```

2. Sahkan senarai migration memaparkan sekurang-kurangnya `InitialShared` (Hari 1) dan migration modul Lapor Diri (Hari 2/3) tanpa tanda `(Pending)`.
3. Jalankan aplikasi (`dotnet run`) dan sahkan navigasi Lapor Diri masih berfungsi.
4. Sahkan versi SDK:

   ```bash
   dotnet --version
   ```

   Mesti bermula dengan `10.`.

✅ **Semakan:** `dotnet build` berjaya tanpa ralat, migration sedia ada semua sudah `database update`, aplikasi masih boleh dijalankan.

---

## Latihan 1 — Entiti `Vehicle`

**Objektif:** Cipta entiti `Vehicle` — kenderaan yang dimiliki/digunakan seorang staf, boleh lebih daripada satu.

Cipta fail `Models/Vehicle.cs`:

```csharp
namespace Nres.Onboarding.Web.Models;

public enum VehicleType
{
    Kereta = 0,
    Motosikal = 1,
    Van = 2,
    Lori = 3,
    Lain = 4
}

public enum OwnerRelationship
{
    Sendiri = 0,
    Pasangan = 1,
    AnakAtauIbuBapa = 2,
    Lain = 3
}

public class Vehicle
{
    public int Id { get; set; }

    /// <summary>Staf (AspNetUsers.Id) yang mendaftarkan kenderaan ini.</summary>
    public string ApplicantUserId { get; set; } = string.Empty;

    public string RegistrationNo { get; set; } = string.Empty;

    public VehicleType Type { get; set; }

    public string MakeModel { get; set; } = string.Empty;

    public string Color { get; set; } = string.Empty;

    /// <summary>Nama pemilik berdaftar kenderaan (mungkin bukan pemohon sendiri, cth. kereta pasangan).</summary>
    public string OwnerName { get; set; } = string.Empty;

    public OwnerRelationship OwnerRelationship { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public List<VehicleStickerApplication> StickerApplications { get; set; } = new();

    public List<ParkingApplication> ParkingApplications { get; set; } = new();
}
```

**Perhatikan:**

- `ApplicantUserId` ialah `string` kerana `AspNetUsers.Id` (ASP.NET Core Identity) berjenis `string` (GUID), bukan `int`.
- `List<VehicleStickerApplication>` dan `List<ParkingApplication>` ialah **navigation properties** — sisi "banyak" bagi hubungan satu-ke-banyak yang akan kita konfigur di Latihan 3.
- `Vehicle` **tidak** ada `SubmissionId` — ia bukan permohonan, ia data induk kenderaan yang **dikongsi** oleh berbilang permohonan sepanjang hayatnya.

✅ **Semakan:** Fail `Models/Vehicle.cs` wujud, `dotnet build` masih berjaya (ralat rujukan ke `VehicleStickerApplication`/`ParkingApplication` yang belum wujud akan hilang selepas Latihan 2).

---

## Latihan 2 — Tiga Entiti Permohonan

**Objektif:** Cipta `AccessPassApplication`, `VehicleStickerApplication`, `ParkingApplication` — setiap satu berkongsi `Submission` induk melalui hubungan satu-ke-satu.

### 2.1 — `AccessPassApplication`

Cipta `Models/AccessPassApplication.cs`:

```csharp
namespace Nres.Onboarding.Web.Models;

public enum SecurityPassType
{
    New = 0,
    Replacement = 1
}

public class AccessPassApplication
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public SecurityPassType PassType { get; set; }

    /// <summary>Wajib diisi jika PassType == Replacement (dikuatkuasakan Hari 5).</summary>
    public string? ReplacementReason { get; set; }

    public string AccessAreaRequested { get; set; } = string.Empty;

    public DateTime ValidFrom { get; set; }

    public DateTime? ValidTo { get; set; }
}
```

### 2.2 — `VehicleStickerApplication`

Cipta `Models/VehicleStickerApplication.cs`:

```csharp
namespace Nres.Onboarding.Web.Models;

public class VehicleStickerApplication
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    /// <summary>Nombor siri pelekat fizikal — diisi oleh SecurityAdmin semasa Approve (Hari 6), bukan semasa Submit.</summary>
    public string? StickerNoIssued { get; set; }
}
```

### 2.3 — `ParkingApplication`

Cipta `Models/ParkingApplication.cs`:

```csharp
namespace Nres.Onboarding.Web.Models;

public enum ParkingType
{
    Biasa = 0,
    Khas = 1
}

public class ParkingApplication
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;

    public ParkingType ParkingType { get; set; }

    /// <summary>Wajib diisi jika ParkingType == Khas (dikuatkuasakan Hari 5).</summary>
    public string? Justification { get; set; }

    public string ParkingZoneRequested { get; set; } = string.Empty;
}
```

**Perhatikan corak berulang:** ketiga-tiga kelas ada `SubmissionId` + `Submission Submission` — ini ialah "separuh" hubungan satu-ke-satu dengan jadual induk. Kita konfigur separuh lagi (`WithOne`, indeks unik) dalam `ApplicationDbContext` di Latihan 3 — EF Core **tidak** boleh teka hubungan satu-ke-satu secara automatik daripada konvensyen semata-mata, ia mesti dinyatakan secara eksplisit.

✅ **Semakan:** Ketiga-tiga fail wujud dalam `Models/`, `dotnet build` masih ada ralat (kerana `ApplicationDbContext` belum tahu tentang jadual baharu) — ini **dijangka**, akan hilang di Latihan 3.

---

## Latihan 3 — Daftar ke `ApplicationDbContext`

**Objektif:** Tambah `DbSet` baharu dan konfigur hubungan (relationships) dalam `OnModelCreating`.

Buka `Data/ApplicationDbContext.cs` (fail sedia ada sejak Hari 1). **Tambah** empat `DbSet` baharu ke senarai sedia ada (`Submissions`, `Attachments`, `AuditLogs`, dll. — jangan buang yang sedia ada):

```csharp
public DbSet<Vehicle> Vehicles => Set<Vehicle>();
public DbSet<AccessPassApplication> AccessPassApplications => Set<AccessPassApplication>();
public DbSet<VehicleStickerApplication> VehicleStickerApplications => Set<VehicleStickerApplication>();
public DbSet<ParkingApplication> ParkingApplications => Set<ParkingApplication>();
```

Kemudian dalam kaedah `OnModelCreating(ModelBuilder modelBuilder)` sedia ada, **tambah** blok konfigurasi berikut (selepas konfigurasi `Submission`/`Attachment`/`AuditLog` Hari 1, jangan gantikan):

```csharp
// ── Vehicle ──────────────────────────────────────────────
modelBuilder.Entity<Vehicle>(entity =>
{
    entity.Property(e => e.RegistrationNo).HasMaxLength(15).IsRequired();
    entity.Property(e => e.MakeModel).HasMaxLength(100).IsRequired();
    entity.Property(e => e.Color).HasMaxLength(30).IsRequired();
    entity.Property(e => e.OwnerName).HasMaxLength(100).IsRequired();

    // Nombor pendaftaran kenderaan mesti unik merentasi seluruh sistem —
    // dua kenderaan berbeza tidak boleh guna plat yang sama.
    entity.HasIndex(e => e.RegistrationNo).IsUnique();
});

// ── AccessPassApplication (one-to-one dengan Submission) ─
modelBuilder.Entity<AccessPassApplication>(entity =>
{
    entity.Property(e => e.AccessAreaRequested).HasMaxLength(300).IsRequired();
    entity.Property(e => e.ReplacementReason).HasMaxLength(300);

    entity.HasIndex(e => e.SubmissionId).IsUnique();
    entity.HasOne(e => e.Submission)
        .WithOne()
        .HasForeignKey<AccessPassApplication>(e => e.SubmissionId)
        .OnDelete(DeleteBehavior.Cascade);
});

// ── VehicleStickerApplication (one-to-one dengan Submission, many-to-one dengan Vehicle) ─
modelBuilder.Entity<VehicleStickerApplication>(entity =>
{
    entity.Property(e => e.StickerNoIssued).HasMaxLength(30);

    entity.HasIndex(e => e.SubmissionId).IsUnique();
    entity.HasOne(e => e.Submission)
        .WithOne()
        .HasForeignKey<VehicleStickerApplication>(e => e.SubmissionId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Vehicle)
        .WithMany(v => v.StickerApplications)
        .HasForeignKey(e => e.VehicleId)
        .OnDelete(DeleteBehavior.Restrict);
});

// ── ParkingApplication (one-to-one dengan Submission, many-to-one dengan Vehicle) ─
modelBuilder.Entity<ParkingApplication>(entity =>
{
    entity.Property(e => e.Justification).HasMaxLength(500);
    entity.Property(e => e.ParkingZoneRequested).HasMaxLength(100).IsRequired();

    entity.HasIndex(e => e.SubmissionId).IsUnique();
    entity.HasOne(e => e.Submission)
        .WithOne()
        .HasForeignKey<ParkingApplication>(e => e.SubmissionId)
        .OnDelete(DeleteBehavior.Cascade);

    entity.HasOne(e => e.Vehicle)
        .WithMany(v => v.ParkingApplications)
        .HasForeignKey(e => e.VehicleId)
        .OnDelete(DeleteBehavior.Restrict);
});
```

**Kenapa `OnDelete(DeleteBehavior.Cascade)` untuk `Submission`, tapi `Restrict` untuk `Vehicle`?**

- Jika satu `Submission` dipadam, rekod anak (`AccessPassApplication`/dll.) **tidak berguna** tanpa induknya — jadi padam sekali (`Cascade`).
- Jika satu `Vehicle` cuba dipadam sedangkan ia **masih** dirujuk oleh permohonan pelekat/parkir lama, kita **halang** (`Restrict`) — sejarah permohonan tidak patut hilang secara senyap hanya kerana rekod kenderaan dipadam. (Latihan lanjutan: dalam sistem sebenar, guna medan `IsActive`/soft-delete pada `Vehicle` dan bukan padam terus.)

✅ **Semakan:** `dotnet build` berjaya **tanpa ralat**. Jika ada ralat "The entity type requires a primary key" atau serupa, semak semula anda tidak terlupa `public int Id { get; set; }` pada mana-mana kelas.

---

## Latihan 4 — Migration `Module2Initial`

**Objektif:** Jana migration EF Core baharu dan kemas kini pangkalan data SQLite latihan.

1. Jana migration:

   ```bash
   dotnet ef migrations add Module2Initial
   ```

2. **Semak** fail yang dijana dalam `Migrations/` — cari kaedah `Up()`. Sahkan ia mengandungi `migrationBuilder.CreateTable(name: "Vehicles", ...)`, `"AccessPassApplications"`, `"VehicleStickerApplications"`, `"ParkingApplications"`, serta `CreateIndex(... unique: true ...)` untuk `SubmissionId` pada tiga jadual permohonan dan untuk `RegistrationNo` pada `Vehicles`.

3. Kemas kini pangkalan data:

   ```bash
   dotnet ef database update
   ```

4. Sahkan skema (pilih salah satu):

   ```bash
   dotnet ef migrations script --idempotent
   ```

   atau buka fail SQLite (`*.db`) dengan alat pilihan anda (`sqlite3`, DB Browser for SQLite, atau extension VS Code) dan sahkan jadual `Vehicles`, `AccessPassApplications`, `VehicleStickerApplications`, `ParkingApplications` wujud dengan lajur yang betul.

✅ **Semakan:** `dotnet ef migrations list` memaparkan `Module2Initial` **tanpa** `(Pending)`. Empat jadual baharu wujud dalam fail SQLite.

---

## Latihan 5 — Halaman Landing Modul 2 & Navigasi

**Objektif:** Cipta laluan (route) dan skrin awal Modul 2 supaya struktur tiga jenis permohonan kelihatan sebelum borang sebenar wujud.

### 5.1 — Controller

Cipta `Controllers/Module2Controller.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class Module2Controller : Controller
{
    public IActionResult Index()
    {
        return View();
    }
}
```

### 5.2 — View landing

Cipta `Views/Module2/Index.cshtml`:

```cshtml
@{
    ViewData["Title"] = "Pas, Parking & Pelekat Kenderaan";
}

<h1>@ViewData["Title"]</h1>
<p>Pilih jenis permohonan yang ingin anda buat:</p>

<div class="row g-3">
    <div class="col-md-4">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title">Pas Keselamatan</h5>
                <p class="card-text">Permohonan pas akses baharu atau pas ganti (replacement).</p>
                <a class="btn btn-primary" asp-controller="AccessPass" asp-action="Index">Mohon Pas Keselamatan</a>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title">Pelekat Kenderaan</h5>
                <p class="card-text">Permohonan pelekat akses kawasan untuk kenderaan berdaftar.</p>
                <a class="btn btn-primary" asp-controller="VehicleSticker" asp-action="Index">Mohon Pelekat Kenderaan</a>
            </div>
        </div>
    </div>
    <div class="col-md-4">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title">Parkir</h5>
                <p class="card-text">Permohonan ruang parkir biasa atau parkir khas (perlu justifikasi).</p>
                <a class="btn btn-primary" asp-controller="Parking" asp-action="Index">Mohon Parkir</a>
            </div>
        </div>
    </div>
</div>
```

### 5.3 — Placeholder controller untuk tiga laluan

Borang penuh dibina Hari 5 — buat masa ini cipta **stub** supaya pautan di atas tidak pecah (404). Cipta tiga fail controller ringkas:

`Controllers/AccessPassController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nres.Onboarding.Web.Controllers;

[Authorize(Roles = "Applicant")]
public class AccessPassController : Controller
{
    public IActionResult Index()
    {
        // TODO (Hari 5): senarai permohonan pas keselamatan pemohon semasa + borang Create.
        return Content("Borang Pas Keselamatan — dibina Hari 5.");
    }
}
```

`Controllers/VehicleStickerController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nres.Onboarding.Web.Controllers;

[Authorize(Roles = "Applicant")]
public class VehicleStickerController : Controller
{
    public IActionResult Index()
    {
        // TODO (Hari 5): senarai permohonan pelekat kenderaan + borang Create.
        return Content("Borang Pelekat Kenderaan — dibina Hari 5.");
    }
}
```

`Controllers/ParkingController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Nres.Onboarding.Web.Controllers;

[Authorize(Roles = "Applicant")]
public class ParkingController : Controller
{
    public IActionResult Index()
    {
        // TODO (Hari 5): senarai permohonan parkir + borang Create.
        return Content("Borang Parkir — dibina Hari 5.");
    }
}
```

### 5.4 — Tambah pautan menu utama

Buka `Views/Shared/_Layout.cshtml` (fail sedia ada sejak Hari 1) dan tambah satu item navigasi baharu di sebelah pautan Lapor Diri sedia ada:

```cshtml
<li class="nav-item">
    <a class="nav-link text-dark" asp-controller="Module2" asp-action="Index">Pas/Parking/Pelekat</a>
</li>
```

### 5.5 — Jalankan dan navigasi

```bash
dotnet run
```

Layari `/Module2`, sahkan tiga kad kelihatan, dan setiap butang membawa ke placeholder yang betul (`/AccessPass`, `/VehicleSticker`, `/Parking`).

✅ **Semakan akhir Hari 4:**

- `dotnet build` bersih, `dotnet ef migrations list` memaparkan `Module2Initial` tiada `(Pending)`.
- Navigasi `/Module2` memaparkan tiga kad, setiap satu membawa ke placeholder controller yang betul.
- Anda boleh terangkan **kenapa** `Vehicle` berasingan daripada permohonan, dan **kenapa** ketiga-tiga jenis permohonan berkongsi `Submission` induk yang sama.
- Banding struktur `Models/` anda dengan [`../../projek/`](../../projek/) jika tersedia.

**Seterusnya:** [Hari 5](../../hari-5/) — bina borang sebenar menggantikan placeholder `Content(...)` di atas, dengan validation bersyarat & semakan pendua.
