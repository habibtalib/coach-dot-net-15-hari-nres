# Lab · Kumpulan 4 · Hari 4 — Skema Katalog & Tempahan

> Konsep: [`../README.md`](../README.md) · Kanun: [`../../../SPEC-KURSUS.md`](../../../SPEC-KURSUS.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula hari dengan betul

```bash
git switch kump-4/tempahan-fasiliti
git pull --rebase origin master
git switch -c kump-4/feat/skema-fasiliti
dotnet build
```

**Semakan "sudah wujud?"** sebelum menulis apa-apa:

```bash
grep -rn "TempahanFasilitiSukan\|SportsFacility" Nres.Onboarding.Web/
grep -rn "IReferenceNumberService" Nres.Onboarding.Web/Services/
grep -rn "FacilityAdmin" Nres.Onboarding.Web/Data/
```

- `IReferenceNumberService` **sudah wujud** — anda menggunakannya pada Hari 5–6, tidak menulis satu lagi.
- `ModuleCodes.TempahanFasilitiSukan` (`"TFS"`) dan peranan `FacilityAdmin` sepatutnya berseed pada Hari 3. Jika `grep` tidak menemuinya, **jangan tambah sendiri ke fail kongsi** — buka isu berlabel `shared` dan beritahu jurulatih. Anda boleh teruskan Lab 1–3 tanpanya; ia hanya diperlukan mulai migration (Lab 5).

### ✅ Semakan

- [ ] `dotnet build` berjaya pada cabang kumpulan
- [ ] Anda mengesahkan servis kongsi wujud
- [ ] Anda mengesahkan `ModuleCodes.TempahanFasilitiSukan` wujud (atau membuka isu `shared`)
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Entiti katalog `SportsFacility`

**Objektif:** Fasiliti sebagai data rujukan kelas pertama, dengan waktu operasi.

### Langkah

1. `Models/Fasiliti/SportsFacility.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Fasiliti;

public enum FacilityType
{
    GelanggangBadminton = 1,
    GelanggangSepakTakraw = 2,
    GelanggangTenis = 3,
    DewanSerbaguna = 4,
    PadangBolaSepak = 5,
    BilikGimnasium = 6
}

/// <summary>
/// Katalog fasiliti sukan. Ini DATA RUJUKAN — ia dimiliki NRES dan diseed,
/// BUKAN dimohon. Tiada SubmissionId, sama seperti Vehicle/ParkingLot
/// Kumpulan 2.
///
/// Waktu operasi hidup di sini kerana ia sifat fasiliti: gelanggang mungkin
/// buka 8 pagi–11 malam, bilik gimnasium 6 pagi–10 malam. Borang tempahan
/// (Hari 5–6) menyemak masa yang diminta terhadap tetingkap ini.
/// </summary>
public class SportsFacility
{
    public int Id { get; set; }

    /// <summary>Nama unik dalam katalog, cth. "Gelanggang Badminton A".</summary>
    public string Name { get; set; } = string.Empty;

    public FacilityType FacilityType { get; set; }

    /// <summary>Lokasi fizikal untuk paparan, cth. "Aras Bawah, Kompleks Sukan".</summary>
    public string Location { get; set; } = string.Empty;

    /// <summary>Muatan maksimum orang — dipaparkan, dan disemak terhadap bilangan peserta.</summary>
    public int Capacity { get; set; }

    /// <summary>Fasiliti buka dari jam ini (termasuk).</summary>
    public TimeOnly OpenTime { get; set; } = new(8, 0);

    /// <summary>Fasiliti tutup pada jam ini (tempahan mesti tamat pada/sebelum ini).</summary>
    public TimeOnly CloseTime { get; set; } = new(22, 0);

    /// <summary>Fasiliti dinyahaktifkan (naik taik/tutup) tidak boleh ditempah.</summary>
    public bool IsActive { get; set; } = true;
}
```

2. Perhatikan `OpenTime`/`CloseTime` ialah `TimeOnly` (.NET 6+). Ia lebih tepat daripada `DateTime` untuk "jam pada hari" — tiada komponen tarikh yang mengelirukan.

### ✅ Semakan

- [ ] Fail dalam `Models/Fasiliti/`
- [ ] `SportsFacility` **tiada** `SubmissionId` — ia data rujukan
- [ ] Waktu operasi ialah `TimeOnly`
- [ ] `dotnet build` berjaya

---

## Latihan 2 — Entiti permohonan & slot

**Objektif:** Permohonan tempahan dan slot yang ditempah — dua jadual, satu perhubungan 1:1.

### Langkah

1. `Models/Fasiliti/FacilityBookingApplication.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Fasiliti;

/// <summary>
/// Permohonan tempahan fasiliti. Nombor rujukan, status, pemohon, tarikh
/// hantar SEMUA hidup dalam Submission induk — jangan pendua ke sini.
///
/// Slot yang diminta ialah entiti berasingan (FacilityBookingSlot) supaya
/// semakan bertindih Hari 5–6 boleh menyoalnya terus.
/// </summary>
public class FacilityBookingApplication
{
    public int Id { get; set; }

    public int SubmissionId { get; set; }
    public Submission? Submission { get; set; }

    public int FacilityId { get; set; }
    public SportsFacility? Facility { get; set; }

    /// <summary>Tujuan tempahan, cth. "Latihan mingguan kelab badminton".</summary>
    public string Purpose { get; set; } = string.Empty;

    /// <summary>Bilangan peserta dijangka — disemak terhadap Capacity fasiliti.</summary>
    public int ExpectedAttendees { get; set; }

    /// <summary>Akuan pemohon (Hari 5–6) — mesti true sebelum penghantaran.</summary>
    public bool DeclarationAccepted { get; set; }

    /// <summary>Slot yang diminta. Perhubungan satu-ke-satu.</summary>
    public FacilityBookingSlot? Slot { get; set; }
}
```

2. `Models/Fasiliti/FacilityBookingSlot.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Fasiliti;

/// <summary>
/// Slot masa yang ditempah: fasiliti X, tarikh D, dari StartTime hingga EndTime.
///
/// Konvensyen SELANG SEPARUH-TERBUKA [StartTime, EndTime):
///   slot meliputi StartTime (termasuk) hingga EndTime (TIDAK termasuk).
///   Maka 10:00–11:00 dan 11:00–12:00 TIDAK bertindih. Ini yang
///   membenarkan tempahan bersebelahan yang sah.
///
/// FacilityId disimpan DI SINI walaupun permohonan juga memilikinya. Ini
/// denormalisasi yang disengajakan: semakan bertindih menyoal
/// "apa yang ditempah untuk fasiliti ini pada tarikh ini?" dan indeks
/// komposit (FacilityId, BookingDate) menjawabnya tanpa join ke permohonan.
/// </summary>
public class FacilityBookingSlot
{
    public int Id { get; set; }

    public int FacilityBookingApplicationId { get; set; }
    public FacilityBookingApplication? Application { get; set; }

    /// <summary>Denormalisasi dari permohonan — untuk indeks query panas.</summary>
    public int FacilityId { get; set; }

    public DateOnly BookingDate { get; set; }
    public TimeOnly StartTime { get; set; }
    public TimeOnly EndTime { get; set; }
}
```

3. **Semakan anti-duplikasi.** Sahkan kedua-dua kelas **tiada**: `ReferenceNo`, `Status`, `ApplicantUserId`, `SubmittedAt`. Kesemuanya ada dalam `Submission`.

> **Kenapa `FacilityId` pada slot bukan duplikasi terlarang?** Ia bukan medan `Submission` — ia sebahagian daripada identiti slot ("fasiliti mana yang ditempah"). Peraturan AGENTS.md melarang menyalin `ReferenceNo`/`Status`/tarikh `Submission`, bukan denormalisasi dalam jadual modul anda sendiri untuk prestasi. Nyatakan justifikasi ini dalam komen — itu yang membezakan denormalisasi berdisiplin daripada kecuaian.

### ✅ Semakan

- [ ] Kedua-dua kelas dalam `Models/Fasiliti/`
- [ ] `FacilityBookingApplication` memaut ke `Submission` melalui `SubmissionId`
- [ ] `FacilityBookingSlot` guna `DateOnly` + `TimeOnly`, bukan `DateTime`
- [ ] **Sifar** medan `Submission` diduplikasi
- [ ] Komen menjelaskan konvensyen separuh-terbuka & denormalisasi `FacilityId`

---

## Latihan 3 — Konfigurasi EF Core + seed katalog

**Objektif:** Daftar ketiga-tiga entiti **tanpa menyentuh `ApplicationDbContext`**, dan seed katalog sintetik.

### Langkah

1. `Models/Fasiliti/Configurations/SportsFacilityConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Fasiliti.Configurations;

public class SportsFacilityConfiguration : IEntityTypeConfiguration<SportsFacility>
{
    public void Configure(EntityTypeBuilder<SportsFacility> builder)
    {
        builder.ToTable("SportsFacilities");

        builder.Property(f => f.Name).HasMaxLength(120).IsRequired();
        builder.Property(f => f.Location).HasMaxLength(200).IsRequired();
        builder.Property(f => f.FacilityType).HasConversion<int>();

        builder.HasIndex(f => f.Name).IsUnique();

        // Data SINTETIK untuk latihan — bukan fasiliti NRES sebenar.
        builder.HasData(
            new SportsFacility { Id = 1, Name = "Gelanggang Badminton A", FacilityType = FacilityType.GelanggangBadminton, Location = "Aras Bawah, Kompleks Sukan", Capacity = 4,  OpenTime = new(8, 0),  CloseTime = new(23, 0) },
            new SportsFacility { Id = 2, Name = "Gelanggang Badminton B", FacilityType = FacilityType.GelanggangBadminton, Location = "Aras Bawah, Kompleks Sukan", Capacity = 4,  OpenTime = new(8, 0),  CloseTime = new(23, 0) },
            new SportsFacility { Id = 3, Name = "Gelanggang Sepak Takraw", FacilityType = FacilityType.GelanggangSepakTakraw, Location = "Aras Bawah, Kompleks Sukan", Capacity = 6, OpenTime = new(8, 0), CloseTime = new(22, 0) },
            new SportsFacility { Id = 4, Name = "Gelanggang Tenis",       FacilityType = FacilityType.GelanggangTenis,     Location = "Luar, Sebelah Padang",     Capacity = 4,  OpenTime = new(7, 0),  CloseTime = new(19, 0) },
            new SportsFacility { Id = 5, Name = "Dewan Serbaguna",        FacilityType = FacilityType.DewanSerbaguna,       Location = "Aras 1, Bangunan Utama",   Capacity = 200, OpenTime = new(8, 0), CloseTime = new(22, 0) },
            new SportsFacility { Id = 6, Name = "Padang Bola Sepak",      FacilityType = FacilityType.PadangBolaSepak,      Location = "Kawasan Belakang",         Capacity = 30, OpenTime = new(7, 0),  CloseTime = new(19, 0) },
            new SportsFacility { Id = 7, Name = "Bilik Gimnasium",        FacilityType = FacilityType.BilikGimnasium,       Location = "Aras Bawah, Kompleks Sukan", Capacity = 20, OpenTime = new(6, 0), CloseTime = new(22, 0) });
    }
}
```

2. `Models/Fasiliti/Configurations/FacilityBookingConfigurations.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Fasiliti.Configurations;

public class FacilityBookingApplicationConfiguration
    : IEntityTypeConfiguration<FacilityBookingApplication>
{
    public void Configure(EntityTypeBuilder<FacilityBookingApplication> builder)
    {
        builder.ToTable("FacilityBookingApplications");

        builder.Property(a => a.Purpose).HasMaxLength(500).IsRequired();

        builder.HasIndex(a => a.SubmissionId).IsUnique();
        builder.HasOne(a => a.Submission).WithOne()
            .HasForeignKey<FacilityBookingApplication>(a => a.SubmissionId)
            .OnDelete(DeleteBehavior.Cascade);

        // Katalog TIDAK dipadam apabila permohonan dipadam.
        builder.HasOne(a => a.Facility).WithMany()
            .HasForeignKey(a => a.FacilityId)
            .OnDelete(DeleteBehavior.Restrict);

        // Satu permohonan = satu slot. Slot dipadam bersama permohonan.
        builder.HasOne(a => a.Slot).WithOne(s => s.Application!)
            .HasForeignKey<FacilityBookingSlot>(s => s.FacilityBookingApplicationId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(a => a.FacilityId);
    }
}

public class FacilityBookingSlotConfiguration
    : IEntityTypeConfiguration<FacilityBookingSlot>
{
    public void Configure(EntityTypeBuilder<FacilityBookingSlot> builder)
    {
        builder.ToTable("FacilityBookingSlots");

        builder.HasIndex(s => s.FacilityBookingApplicationId).IsUnique();

        // Indeks TERAS modul: semakan bertindih menapis fasiliti + tarikh
        // dahulu, kemudian menyaring julat masa dalam memori/SQL.
        builder.HasIndex(s => new { s.FacilityId, s.BookingDate })
            .HasDatabaseName("IX_Slots_Facility_Date");
    }
}
```

3. **Sahkan anda tidak menyentuh fail kongsi:**

```bash
git diff --name-only master
```

`Data/ApplicationDbContext.cs` **tidak** sepatutnya muncul.

### ✅ Semakan

- [ ] Konfigurasi dalam `Models/Fasiliti/Configurations/`
- [ ] Katalog berseed dengan 7 fasiliti sintetik
- [ ] Indeks komposit `(FacilityId, BookingDate)` pada slot
- [ ] `Facility` guna `DeleteBehavior.Restrict` — katalog bertahan lebih lama daripada permohonan
- [ ] Slot guna `Cascade` — ia mati bersama permohonannya
- [ ] `git diff` menunjukkan tiada fail kongsi

---

## Latihan 4 — Pendaftaran modul & navigasi

**Objektif:** Sambungkan modul dengan menambah fail.

### Langkah

1. `Models/Fasiliti/FasilitiModuleDescriptor.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Models.Fasiliti;

public class FasilitiModuleDescriptor : IModuleDescriptorProvider
{
    public ModuleDescriptor Describe() => new(
        Code: ModuleCodes.TempahanFasilitiSukan,
        Nama: "Tempahan Fasiliti Sukan",
        Controller: "FacilityBooking",
        Ikon: "bi-calendar-check",
        Roles: ["Applicant", "FacilityAdmin", "SystemAdmin"],
        Urutan: 4);
}
```

2. `Services/Fasiliti/FasilitiModule.cs`:

```csharp
using Nres.Onboarding.Web.Models.Fasiliti;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Fasiliti;

public static class FasilitiModule
{
    public static IServiceCollection AddFasilitiModule(this IServiceCollection services)
    {
        services.AddScoped<IModuleDescriptorProvider, FasilitiModuleDescriptor>();
        // Servis modul ditambah DI SINI apabila kami menciptanya.
        return services;
    }
}
```

3. **Satu-satunya suntingan fail kongsi hari ini.** Beritahu jurulatih, kemudian nyahkomen **satu baris** dalam `Program.cs`:

```csharp
using Nres.Onboarding.Web.Services.Fasiliti;   // ← tambah using

builder.Services.AddFasilitiModule();       // Kumpulan 4   ← nyahkomen INI sahaja
```

> ⚠️ Jangan nyahkomen baris kumpulan lain — binaan gagal untuk semua orang.

### ✅ Semakan

- [ ] Descriptor & modul dalam folder anda
- [ ] Tepat **satu** baris dinyahkomen dalam `Program.cs`
- [ ] `dotnet build` berjaya

---

## Latihan 5 — Migration (slot!)

### Langkah

1. Umumkan: *"Kumpulan 4 mengambil slot migration."*

2. ```bash
   git pull --rebase origin master
   cd Nres.Onboarding.Web
   dotnet ef migrations add FasilitiKatalogDanTempahan
   ```

3. **Baca fail yang dijana.** Sahkan ia mencipta `SportsFacilities`, `FacilityBookingApplications`, `FacilityBookingSlots` — dan **tiada jadual kumpulan lain**. Sahkan juga baris `HasData` katalog muncul sebagai `InsertData`.

4. ```bash
   dotnet ef database update
   dotnet run
   cd ..
   ```

5. Commit, push, lepaskan slot: *"Kumpulan 4 selesai slot migration."*

### Jika snapshot berkonflik

```bash
git checkout --theirs Migrations/ApplicationDbContextModelSnapshot.cs
rm Migrations/*_FasilitiKatalogDanTempahan.cs Migrations/*_FasilitiKatalogDanTempahan.Designer.cs
git pull --rebase origin master
dotnet ef migrations add FasilitiKatalogDanTempahan
dotnet ef database update
```

Buang dan jana semula. Jangan sekali-kali baiki snapshot dengan tangan.

### ✅ Semakan

- [ ] Slot diumumkan & dilepaskan
- [ ] Migration hanya menyentuh tiga jadual anda
- [ ] Katalog di-`InsertData` (7 fasiliti)
- [ ] Aplikasi bermula

---

## Latihan 6 — Servis katalog & halaman utama

**Objektif:** Satu tempat untuk menyoal katalog, dan landing modul.

### Langkah

1. `Services/Fasiliti/IFacilityCatalogService.cs`:

```csharp
using Nres.Onboarding.Web.Models.Fasiliti;

namespace Nres.Onboarding.Web.Services.Fasiliti;

public interface IFacilityCatalogService
{
    /// <summary>Fasiliti aktif untuk dropdown & senarai katalog.</summary>
    Task<IReadOnlyList<SportsFacility>> ActiveFacilitiesAsync(CancellationToken ct = default);

    Task<SportsFacility?> FindAsync(int facilityId, CancellationToken ct = default);
}
```

2. `Services/Fasiliti/FacilityCatalogService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Fasiliti;

namespace Nres.Onboarding.Web.Services.Fasiliti;

public class FacilityCatalogService(ApplicationDbContext db) : IFacilityCatalogService
{
    public async Task<IReadOnlyList<SportsFacility>> ActiveFacilitiesAsync(
        CancellationToken ct = default) =>
        await db.Set<SportsFacility>().AsNoTracking()
            .Where(f => f.IsActive)
            .OrderBy(f => f.Name)
            .ToListAsync(ct);

    public async Task<SportsFacility?> FindAsync(int facilityId, CancellationToken ct = default) =>
        await db.Set<SportsFacility>().AsNoTracking()
            .FirstOrDefaultAsync(f => f.Id == facilityId, ct);
}
```

3. Daftar dalam modul anda (`FasilitiModule.AddFasilitiModule`):

```csharp
services.AddScoped<IFacilityCatalogService, FacilityCatalogService>();
```

4. `ViewModels/Fasiliti/FacilityIndexViewModel.cs`:

```csharp
using Nres.Onboarding.Web.Models.Fasiliti;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Fasiliti;

public class FacilityIndexViewModel
{
    public IReadOnlyList<SportsFacility> Facilities { get; set; } = [];
    public IReadOnlyList<BookingRingkas> Bookings { get; set; } = [];

    public record BookingRingkas(
        int ApplicationId, string ReferenceNo, string FacilityName,
        DateOnly BookingDate, TimeOnly StartTime, TimeOnly EndTime,
        SubmissionStatus Status);
}
```

5. `Controllers/FacilityBookingController.cs` — mulakan dengan `Index` sahaja (borang datang Hari 5–6):

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.Services.Fasiliti;
using Nres.Onboarding.Web.ViewModels.Fasiliti;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class FacilityBookingController(
    ApplicationDbContext db,
    ICurrentUserService currentUser,
    IFacilityCatalogService catalog) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = currentUser.UserId!;

        var bookings = await (
            from a in db.Set<Models.Fasiliti.FacilityBookingApplication>().AsNoTracking()
            join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
            join f in db.Set<Models.Fasiliti.SportsFacility>().AsNoTracking() on a.FacilityId equals f.Id
            where s.ApplicantUserId == userId
            orderby s.CreatedAt descending
            select new { a, s, f })
            .Take(20)
            .ToListAsync();

        var slotByApp = await db.Set<Models.Fasiliti.FacilityBookingSlot>().AsNoTracking()
            .Where(sl => bookings.Select(b => b.a.Id).Contains(sl.FacilityBookingApplicationId))
            .ToDictionaryAsync(sl => sl.FacilityBookingApplicationId);

        var senarai = bookings.Select(b =>
        {
            var slot = slotByApp.GetValueOrDefault(b.a.Id);
            return new FacilityIndexViewModel.BookingRingkas(
                b.a.Id, b.s.ReferenceNo, b.f.Name,
                slot?.BookingDate ?? default,
                slot?.StartTime ?? default,
                slot?.EndTime ?? default,
                b.s.Status);
        }).ToList();

        return View(new FacilityIndexViewModel
        {
            Facilities = await catalog.ActiveFacilitiesAsync(),
            Bookings = senarai
        });
    }
}
```

6. `Views/FacilityBooking/Index.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Fasiliti.FacilityIndexViewModel
@{ ViewData["Title"] = "Tempahan Fasiliti Sukan"; }

<h2>@ViewData["Title"]</h2>
<p class="text-muted">Tempah gelanggang dan kemudahan sukan NRES.</p>

<h5 class="mt-4">Fasiliti Tersedia</h5>
<div class="row g-3 my-2">
@foreach (var f in Model.Facilities)
{
    <div class="col-md-4">
        <div class="card h-100">
            <div class="card-body">
                <h5 class="card-title">@f.Name</h5>
                <p class="card-text small text-muted mb-1">@f.Location</p>
                <p class="card-text small mb-2">
                    Muatan: @f.Capacity orang ·
                    Waktu: @f.OpenTime.ToString("HH:mm")–@f.CloseTime.ToString("HH:mm")
                </p>
                <a asp-action="Create" asp-route-facilityId="@f.Id" class="btn btn-primary btn-sm">
                    Tempah
                </a>
            </div>
        </div>
    </div>
}
</div>

<h5 class="mt-4">Tempahan Saya</h5>
<table class="table table-hover">
    <thead>
        <tr><th>No. Rujukan</th><th>Fasiliti</th><th>Tarikh</th><th>Masa</th><th>Status</th><th></th></tr>
    </thead>
    <tbody>
    @if (!Model.Bookings.Any())
    {
        <tr><td colspan="6" class="text-muted">Tiada tempahan lagi.</td></tr>
    }
    @foreach (var b in Model.Bookings)
    {
        <tr>
            <td>@(string.IsNullOrEmpty(b.ReferenceNo) ? "(draf)" : b.ReferenceNo)</td>
            <td>@b.FacilityName</td>
            <td>@(b.BookingDate == default ? "—" : b.BookingDate.ToString("dd/MM/yyyy"))</td>
            <td>@(b.BookingDate == default ? "—" : $"{b.StartTime:HH\\:mm}–{b.EndTime:HH\\:mm}")</td>
            <td><partial name="_StatusBadge" model="b.Status" /></td>
            <td class="text-end">
                <a asp-action="Edit" asp-route-id="@b.ApplicationId"
                   class="btn btn-sm btn-outline-primary">Buka</a>
            </td>
        </tr>
    }
    </tbody>
</table>
```

> Pautan `Create`/`Edit` menunjuk kepada action yang anda bina pada **Hari 5–6**. Ia akan 404 hari ini — itu dijangka.

### ✅ Semakan

- [ ] Servis katalog dalam `Services/Fasiliti/` dan didaftar dalam `FasilitiModule`
- [ ] Halaman utama menyenaraikan 7 fasiliti berseed
- [ ] Guna `_StatusBadge` **kongsi**
- [ ] Modul boleh dicapai daripada navigasi

---

## Latihan 7 — Tutup hari

```bash
git diff --name-only master     # hanya fail Fasiliti + 1 baris Program.cs
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Binaan bersih; aplikasi bermula; halaman utama modul berfungsi
- [ ] Servis/komponen kongsi digunakan
- [ ] Hanya fail Kumpulan 4 disentuh (+1 baris `Program.cs`)
- [ ] Migration melalui slot
- [ ] Kod jana-AI difahami
- [ ] Disemak rakan sekumpulan
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 4

| Artifak | Lokasi |
|---------|--------|
| `SportsFacility` + `FacilityBookingApplication` + `FacilityBookingSlot` | `Models/Fasiliti/` |
| Konfigurasi EF Core + seed katalog | `Models/Fasiliti/Configurations/` |
| Pendaftaran modul + descriptor | `Services/Fasiliti/`, `Models/Fasiliti/` |
| Migration `FasilitiKatalogDanTempahan` | `Migrations/` |
| `IFacilityCatalogService` | `Services/Fasiliti/` |
| Halaman utama modul | `Controllers/FacilityBookingController.cs`, `Views/FacilityBooking/Index.cshtml` |

**Seterusnya (Hari 5–6):** borang tempahan, akuan, no. rujukan `TFS`, dan **semakan slot bertindih** — teras modul anda.
