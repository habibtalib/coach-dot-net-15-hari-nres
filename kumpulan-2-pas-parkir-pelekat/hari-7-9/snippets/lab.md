# Lab · Kumpulan 2 · Hari 7–9 — Semakan Keselamatan & Peruntukan

> Konsep: [`../README.md`](../README.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok

```bash
git switch kump-2/akses-kenderaan
git pull --rebase origin master
git switch -c kump-2/feat/semakan-keselamatan
dotnet build
```

**Semakan "sudah wujud?"** — kritikal untuk blok ini:

```bash
grep -n "virtual" Nres.Onboarding.Web/Controllers/SubmissionControllerBase.cs
grep -rn "_ApprovalPanel\|_FilterBar\|_AttachmentList" Nres.Onboarding.Web/Views/Shared/
```

`Approve` dan `Reject` ialah **`virtual`** — anda akan **mengatasi**, bukan menulis semula.

**Prompt AI:**

```text
Merujuk AGENTS.md: saya Kumpulan 2. Kelulusan modul kami mesti juga
memperuntukkan nombor siri pas/pelekat dan nombor lot parkir — kelas asas
SubmissionControllerBase.Approve tidak melakukannya.
Apakah cara yang betul untuk melanjutkan tingkah laku kelas asas tanpa
menulis semula logik peralihan status dan auditnya?
```

> Jawapan betul: `override` + panggil `base.Approve(...)`. Jika AI mencadangkan menyalin badan kelas asas, tolak.

### ✅ Semakan

- [ ] Anda mengesahkan `Approve`/`Reject` ialah `virtual`
- [ ] Anda faham anda akan `override`, bukan menyalin
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Medan peruntukan & syarat

**Objektif:** Tambah medan yang diisi semasa kelulusan.

### Langkah

1. Tambah ke ketiga-tiga entiti permohonan dalam `Models/Akses/`:

```csharp
// AccessPassApplication.cs — tambah
/// <summary>Syarat kelulusan bertulis. Null = kelulusan penuh tanpa syarat.</summary>
public string? SyaratKelulusan { get; set; }

// VehicleStickerApplication.cs — tambah (medan yang sama)
public string? SyaratKelulusan { get; set; }

// ParkingApplication.cs — tambah (medan yang sama)
public string? SyaratKelulusan { get; set; }
```

> **Kenapa medan pada tiga jadual dan bukan status baharu?** `SubmissionStatus` dikongsi keempat-empat modul. Menambah `ConditionallyApproved` memaksa tiga kumpulan lain mengendalikannya. Lanjutkan jadual anda, bukan enum kongsi.

2. Kemas kini konfigurasi:

```csharp
// dalam setiap konfigurasi permohonan
builder.Property(a => a.SyaratKelulusan).HasMaxLength(1000);
```

3. Jadual peruntukan lot — `Models/Akses/ParkingLot.cs`:

```csharp
namespace Nres.Onboarding.Web.Models.Akses;

/// <summary>
/// Lot parkir fizikal. Sumber TERHAD — tidak seperti nombor siri yang
/// hanya bertambah, terdapat bilangan lot tetap yang wujud.
/// </summary>
public class ParkingLot
{
    public int Id { get; set; }

    /// <summary>Cth. "A-12", "B-05".</summary>
    public string LotNumber { get; set; } = string.Empty;

    public JenisParkir Jenis { get; set; } = JenisParkir.Biasa;

    /// <summary>Blok/lokasi untuk paparan.</summary>
    public string? Lokasi { get; set; }

    public bool IsActive { get; set; } = true;
}
```

4. Konfigurasi + seed dalam `Models/Akses/Configurations/ParkingLotConfiguration.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Nres.Onboarding.Web.Models.Akses.Configurations;

public class ParkingLotConfiguration : IEntityTypeConfiguration<ParkingLot>
{
    public void Configure(EntityTypeBuilder<ParkingLot> builder)
    {
        builder.ToTable("ParkingLots");
        builder.Property(l => l.LotNumber).HasMaxLength(20).IsRequired();
        builder.Property(l => l.Lokasi).HasMaxLength(100);
        builder.Property(l => l.Jenis).HasConversion<int>();
        builder.HasIndex(l => l.LotNumber).IsUnique();

        // Data SINTETIK untuk latihan.
        builder.HasData(
            new ParkingLot { Id = 1, LotNumber = "A-01", Jenis = JenisParkir.Eksekutif, Lokasi = "Blok A" },
            new ParkingLot { Id = 2, LotNumber = "A-02", Jenis = JenisParkir.Eksekutif, Lokasi = "Blok A" },
            new ParkingLot { Id = 3, LotNumber = "B-01", Jenis = JenisParkir.OKU,       Lokasi = "Blok B" },
            new ParkingLot { Id = 4, LotNumber = "B-02", Jenis = JenisParkir.OKU,       Lokasi = "Blok B" },
            new ParkingLot { Id = 5, LotNumber = "C-01", Jenis = JenisParkir.Biasa,     Lokasi = "Blok C" },
            new ParkingLot { Id = 6, LotNumber = "C-02", Jenis = JenisParkir.Biasa,     Lokasi = "Blok C" },
            new ParkingLot { Id = 7, LotNumber = "C-03", Jenis = JenisParkir.Biasa,     Lokasi = "Blok C" },
            new ParkingLot { Id = 8, LotNumber = "C-04", Jenis = JenisParkir.Biasa,     Lokasi = "Blok C" });
    }
}
```

5. **Migration (slot!)** — umumkan, `pull --rebase`, kemudian:

```bash
cd Nres.Onboarding.Web
dotnet ef migrations add AksesPeruntukanDanLot
dotnet ef database update
cd ..
```

Lepaskan slot.

### ✅ Semakan

- [ ] `SyaratKelulusan` ditambah pada ketiga-tiga permohonan
- [ ] `ParkingLot` dengan lapan lot berseed
- [ ] **Tiada** ahli enum baharu ditambah ke `SubmissionStatus`
- [ ] Migration melalui slot

---

## Latihan 2 — Servis peruntukan

**Objektif:** Nombor siri unik dan peruntukan lot, dengan semakan ketersediaan.

### Langkah

1. `Services/Akses/IAllocationService.cs`:

```csharp
using Nres.Onboarding.Web.Models.Akses;

namespace Nres.Onboarding.Web.Services.Akses;

public record LotTersedia(int Id, string LotNumber, string? Lokasi);

public interface IAllocationService
{
    /// <summary>Nombor siri pas seterusnya, cth. PS-2026-0001.</summary>
    Task<string> NextPassSerialAsync(CancellationToken ct = default);

    /// <summary>Nombor siri pelekat seterusnya, cth. SK-2026-0001.</summary>
    Task<string> NextStickerSerialAsync(int tahun, CancellationToken ct = default);

    /// <summary>Lot yang belum diperuntukkan kepada permohonan aktif.</summary>
    Task<IReadOnlyList<LotTersedia>> AvailableLotsAsync(
        JenisParkir jenis, CancellationToken ct = default);

    /// <summary>Adakah lot ini bebas? Semakan sebelum memperuntukkan.</summary>
    Task<bool> IsLotFreeAsync(string lotNumber,
        int? kecualiSubmissionId = null, CancellationToken ct = default);
}
```

2. `Services/Akses/AllocationService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Akses;

public class AllocationService(ApplicationDbContext db) : IAllocationService
{
    private static readonly SubmissionStatus[] StatusAktif =
    [
        SubmissionStatus.Submitted,
        SubmissionStatus.SupervisorApproved,
        SubmissionStatus.AdminApproved
    ];

    public async Task<string> NextPassSerialAsync(CancellationToken ct = default)
    {
        var tahun = DateTime.UtcNow.Year;
        var prefix = $"PS-{tahun}-";

        var kiraan = await db.Set<AccessPassApplication>()
            .CountAsync(a => a.PassSerialNo != null
                          && a.PassSerialNo.StartsWith(prefix), ct);

        return $"{prefix}{(kiraan + 1):D4}";
    }

    public async Task<string> NextStickerSerialAsync(int tahun, CancellationToken ct = default)
    {
        var prefix = $"SK-{tahun}-";

        var kiraan = await db.Set<VehicleStickerApplication>()
            .CountAsync(a => a.StickerSerialNo != null
                          && a.StickerSerialNo.StartsWith(prefix), ct);

        return $"{prefix}{(kiraan + 1):D4}";
    }

    public async Task<IReadOnlyList<LotTersedia>> AvailableLotsAsync(
        JenisParkir jenis, CancellationToken ct = default)
    {
        // Lot yang SEDANG diperuntukkan kepada permohonan aktif.
        var diguna = await (
            from a in db.Set<ParkingApplication>().AsNoTracking()
            join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
            where a.LotNumber != null && StatusAktif.Contains(s.Status)
            select a.LotNumber!).ToListAsync(ct);

        return await db.Set<ParkingLot>().AsNoTracking()
            .Where(l => l.IsActive && l.Jenis == jenis && !diguna.Contains(l.LotNumber))
            .OrderBy(l => l.LotNumber)
            .Select(l => new LotTersedia(l.Id, l.LotNumber, l.Lokasi))
            .ToListAsync(ct);
    }

    public async Task<bool> IsLotFreeAsync(string lotNumber,
        int? kecualiSubmissionId = null, CancellationToken ct = default)
    {
        var q = from a in db.Set<ParkingApplication>().AsNoTracking()
                join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
                where a.LotNumber == lotNumber && StatusAktif.Contains(s.Status)
                select s.Id;

        if (kecualiSubmissionId is not null)
            q = q.Where(id => id != kecualiSubmissionId);

        return !await q.AnyAsync(ct);
    }
}
```

3. Daftar dalam `AksesModule`:

```csharp
services.AddScoped<IAllocationService, AllocationService>();
```

> **Had yang jujur:** `NextPassSerialAsync` mempunyai masalah perlumbaan yang sama seperti `IReferenceNumberService` — dua kelulusan serentak boleh mendapat siri yang sama. Indeks unik yang anda cipta pada Hari 4 menangkapnya sebagai pengecualian. Kita membincangkan penyelesaian sebenar pada Hari 13–14.

### ✅ Semakan

- [ ] Servis dalam `Services/Akses/`
- [ ] `AvailableLotsAsync` mengecualikan lot yang sedang diperuntukkan
- [ ] `IsLotFreeAsync` menyokong `kecualiSubmissionId`
- [ ] Anda memahami had perlumbaan

---

## Latihan 3 — Dashboard & baris gilir Keselamatan

**Objektif:** Satu skrin yang menunjukkan ketiga-tiga jenis permohonan yang menunggu.

### Langkah

1. `ViewModels/Akses/SecurityQueueViewModel.cs`:

```csharp
using Microsoft.AspNetCore.Mvc.Rendering;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Akses;

public class SecurityQueueViewModel
{
    // Kiraan mengikut jenis
    public int PasMenunggu { get; set; }
    public int PelekatMenunggu { get; set; }
    public int ParkirMenunggu { get; set; }

    // Penapis
    public string? JenisModul { get; set; }     // PAS / STK / PKR
    public SubmissionStatus? Status { get; set; }
    public string? Carian { get; set; }
    public DateTime? DariTarikh { get; set; }
    public DateTime? HinggaTarikh { get; set; }

    // Paging
    public int Halaman { get; set; } = 1;
    public int SaizHalaman { get; set; } = 20;
    public int JumlahRekod { get; set; }
    public int JumlahHalaman => (int)Math.Ceiling(JumlahRekod / (double)SaizHalaman);

    public IReadOnlyList<QueueItem> Items { get; set; } = [];

    public record QueueItem(
        int ApplicationId, int SubmissionId, string ReferenceNo,
        string ModuleCode, string JenisNama, string Controller,
        string Pemohon, string? PlateNumber,
        SubmissionStatus Status, DateTime? SubmittedAt);
}
```

2. `Services/Akses/ISecurityReviewService.cs` + pelaksanaan — query gabungan merentas tiga jenis:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.ViewModels.Akses;

namespace Nres.Onboarding.Web.Services.Akses;

public interface ISecurityReviewService
{
    Task<SecurityQueueViewModel> QueueAsync(SecurityQueueViewModel penapis,
        CancellationToken ct = default);
}

public class SecurityReviewService(ApplicationDbContext db) : ISecurityReviewService
{
    public async Task<SecurityQueueViewModel> QueueAsync(
        SecurityQueueViewModel f, CancellationToken ct = default)
    {
        // Tiga query kecil, disatukan dalam memori. Ini pilihan yang SEDAR:
        // menggabungkan tiga jadual detail dalam satu query SQL memerlukan
        // UNION yang tidak boleh dibaca. Set hasil kecil (dihalaman),
        // jadi kos memori boleh diterima. Diukur pada Hari 13–14.

        var pas = await (
            from a in db.Set<AccessPassApplication>().AsNoTracking()
            join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
            join p in db.UserProfiles.AsNoTracking() on s.ApplicantUserId equals p.UserId
            select new SecurityQueueViewModel.QueueItem(
                a.Id, s.Id, s.ReferenceNo, s.ModuleCode, "Pas Keselamatan",
                "AccessPass", p.FullName, null, s.Status, s.SubmittedAt))
            .ToListAsync(ct);

        var pelekat = await (
            from a in db.Set<VehicleStickerApplication>().AsNoTracking()
            join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
            join p in db.UserProfiles.AsNoTracking() on s.ApplicantUserId equals p.UserId
            join v in db.Set<Vehicle>().AsNoTracking() on a.VehicleId equals v.Id
            select new SecurityQueueViewModel.QueueItem(
                a.Id, s.Id, s.ReferenceNo, s.ModuleCode, "Pelekat Kenderaan",
                "VehicleSticker", p.FullName, v.PlateNumber, s.Status, s.SubmittedAt))
            .ToListAsync(ct);

        var parkir = await (
            from a in db.Set<ParkingApplication>().AsNoTracking()
            join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
            join p in db.UserProfiles.AsNoTracking() on s.ApplicantUserId equals p.UserId
            join v in db.Set<Vehicle>().AsNoTracking() on a.VehicleId equals v.Id
            select new SecurityQueueViewModel.QueueItem(
                a.Id, s.Id, s.ReferenceNo, s.ModuleCode, "Lot Parkir",
                "Parking", p.FullName, v.PlateNumber, s.Status, s.SubmittedAt))
            .ToListAsync(ct);

        var semua = pas.Concat(pelekat).Concat(parkir).ToList();

        f.PasMenunggu     = pas.Count(x => x.Status == SubmissionStatus.Submitted);
        f.PelekatMenunggu = pelekat.Count(x => x.Status == SubmissionStatus.Submitted);
        f.ParkirMenunggu  = parkir.Count(x => x.Status == SubmissionStatus.Submitted);

        var q = semua.AsEnumerable();

        if (!string.IsNullOrWhiteSpace(f.JenisModul))
            q = q.Where(x => x.ModuleCode == f.JenisModul);
        if (f.Status is not null)
            q = q.Where(x => x.Status == f.Status);
        if (f.DariTarikh is not null)
            q = q.Where(x => x.SubmittedAt >= f.DariTarikh);
        if (f.HinggaTarikh is not null)
            q = q.Where(x => x.SubmittedAt <= f.HinggaTarikh.Value.AddDays(1));
        if (!string.IsNullOrWhiteSpace(f.Carian))
        {
            var cari = f.Carian.Trim();
            q = q.Where(x => x.ReferenceNo.Contains(cari, StringComparison.OrdinalIgnoreCase)
                          || x.Pemohon.Contains(cari, StringComparison.OrdinalIgnoreCase)
                          || (x.PlateNumber ?? "").Contains(cari, StringComparison.OrdinalIgnoreCase));
        }

        var senarai = q.ToList();
        f.JumlahRekod = senarai.Count;

        f.Items = senarai
            .OrderBy(x => x.Status == SubmissionStatus.Submitted ? 0 : 1)  // menunggu dahulu
            .ThenBy(x => x.SubmittedAt)                                    // paling lama dahulu
            .Skip((f.Halaman - 1) * f.SaizHalaman)
            .Take(f.SaizHalaman)
            .ToList();

        return f;
    }
}
```

> **Nota jujur:** pendekatan tiga-query-satukan-dalam-memori adalah **memadai untuk saiz NRES** tetapi tidak akan berskala ke ratusan ribu baris. Pada Hari 13–14 anda mengukurnya dan memutuskan sama ada ia perlu diubah. Mengakui had ialah sebahagian daripada reka bentuk yang jujur.

3. Controller — tambah ke `AksesController`:

```csharp
[Authorize(Roles = "SecurityAdmin")]
public async Task<IActionResult> Queue(SecurityQueueViewModel penapis)
{
    penapis.Halaman = Math.Max(1, penapis.Halaman);
    return View(await securityReview.QueueAsync(penapis));
}
```

### ✅ Semakan

- [ ] Baris gilir menunjukkan ketiga-tiga jenis dalam satu senarai
- [ ] Kiraan setiap jenis dipaparkan
- [ ] Penapis: jenis, status, tarikh, carian (rujukan/pemohon/plat)
- [ ] Menunggu dahulu, kemudian paling lama dahulu
- [ ] `[Authorize(Roles = "SecurityAdmin")]`

---

## Latihan 4 — Kelulusan dengan peruntukan (`override`)

**Objektif:** Lanjutkan kelas asas — jangan tulis semula.

### Langkah

1. Dalam `VehicleStickerController`, **atasi** `Approve`:

```csharp
/// <summary>
/// Kelulusan pelekat MESTI juga memperuntukkan nombor siri pelekat.
/// Kami MENGATASI kelas asas dan memanggil base.Approve() — peralihan
/// status, penulisan audit, dan notifikasi kekal ditakrifkan SEKALI
/// dalam SubmissionControllerBase.
/// </summary>
[HttpPost]
[ValidateAntiForgeryToken]
public override async Task<IActionResult> Approve(int id, string? remarks)
{
    if (!User.IsInRole(AdminRole)) return Forbid();

    var app = await Db.Set<VehicleStickerApplication>()
        .Include(a => a.Submission)
        .Include(a => a.Vehicle)
        .FirstOrDefaultAsync(a => a.Submission!.Id == id);

    if (app is null) return NotFound();

    // --- Peraturan modul kami: peruntukkan siri sebelum meluluskan ---
    if (string.IsNullOrWhiteSpace(app.StickerSerialNo))
    {
        app.StickerSerialNo = await allocation.NextStickerSerialAsync(app.TahunPelekat);
        app.ValidFrom = new DateTime(app.TahunPelekat, 1, 1);
        app.ValidTo   = new DateTime(app.TahunPelekat, 12, 31);
        await Db.SaveChangesAsync();
    }

    // --- Kemudian delegasikan: status + audit + notifikasi ---
    var catatan = string.IsNullOrWhiteSpace(remarks)
        ? $"Pelekat {app.StickerSerialNo} diperuntukkan."
        : $"{remarks} (Pelekat {app.StickerSerialNo})";

    return await base.Approve(id, catatan);
}

/// <summary>Kelulusan bersyarat — sama seperti Approve, dengan syarat direkod.</summary>
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "SecurityAdmin")]
public async Task<IActionResult> ApproveWithConditions(
    int id, string syarat, string? remarks)
{
    if (string.IsNullOrWhiteSpace(syarat))
    {
        TempData["Ralat"] = "Syarat kelulusan wajib diisi untuk kelulusan bersyarat.";
        return RedirectToAction(nameof(Review), new { id });
    }

    var app = await Db.Set<VehicleStickerApplication>()
        .Include(a => a.Submission)
        .FirstOrDefaultAsync(a => a.Submission!.Id == id);

    if (app is null) return NotFound();

    app.SyaratKelulusan = syarat;
    await Db.SaveChangesAsync();

    // Status kekal AdminApproved — kami TIDAK menambah ahli enum baharu.
    return await Approve(id, $"Lulus bersyarat: {syarat}. {remarks}".Trim());
}
```

2. Dalam `ParkingController`, peruntukan **memerlukan input** (nombor lot dipilih Keselamatan):

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public override async Task<IActionResult> Approve(int id, string? remarks)
{
    // Parkir memerlukan nombor lot — guna ApproveWithLot sebaliknya.
    TempData["Ralat"] = "Sila pilih nombor lot untuk meluluskan permohonan parkir.";
    return RedirectToAction(nameof(Review), new { id });
}

[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "SecurityAdmin")]
public async Task<IActionResult> ApproveWithLot(
    int id, string lotNumber, string? syarat, string? remarks)
{
    if (string.IsNullOrWhiteSpace(lotNumber))
    {
        TempData["Ralat"] = "Nombor lot wajib dipilih.";
        return RedirectToAction(nameof(Review), new { id });
    }

    var app = await Db.Set<ParkingApplication>()
        .Include(a => a.Submission)
        .Include(a => a.Vehicle)
        .FirstOrDefaultAsync(a => a.Submission!.Id == id);

    if (app is null) return NotFound();

    // Semakan ketersediaan lot — pendua KEDUA modul kami.
    if (!await allocation.IsLotFreeAsync(lotNumber, kecualiSubmissionId: id))
    {
        TempData["Ralat"] =
            $"Lot {lotNumber} telah diperuntukkan kepada permohonan lain yang aktif.";
        return RedirectToAction(nameof(Review), new { id });
    }

    app.LotNumber = lotNumber;
    app.SyaratKelulusan = string.IsNullOrWhiteSpace(syarat) ? null : syarat;
    app.ValidFrom = DateTime.UtcNow.Date;
    app.ValidTo = new DateTime(DateTime.UtcNow.Year, 12, 31);
    await Db.SaveChangesAsync();

    var catatan = $"Lot {lotNumber} diperuntukkan."
        + (string.IsNullOrWhiteSpace(syarat) ? "" : $" Syarat: {syarat}.")
        + (string.IsNullOrWhiteSpace(remarks) ? "" : $" {remarks}");

    return await base.Approve(id, catatan);
}
```

> **Perhatikan:** kedua-dua kaedah berakhir dengan `base.Approve(...)`. Peralihan status, audit, dan notifikasi **tidak pernah** ditulis semula.

### ✅ Semakan

- [ ] `Approve` **diatasi**, bukan digantikan — `base.Approve` dipanggil
- [ ] Kelulusan pelekat memperuntukkan nombor siri
- [ ] Kelulusan parkir memerlukan nombor lot dan menyemak ketersediaan
- [ ] Kelulusan bersyarat merekod syarat, **tiada status baharu**
- [ ] Nombor siri & lot muncul dalam catatan audit

---

## Latihan 5 — Skrin semakan

**Objektif:** Pegawai Keselamatan melihat segalanya dan memutuskan.

### Langkah

`Views/Parking/Review.cshtml` — bahagian keputusan (contoh paling kompleks):

```cshtml
@model Nres.Onboarding.Web.ViewModels.Akses.ParkingReviewViewModel
@{ ViewData["Title"] = $"Semakan Parkir — {Model.Submission.ReferenceNo}"; }

<h2>@Model.Submission.ReferenceNo</h2>
<partial name="_StatusBadge" model="Model.Submission.Status" />

<div class="row mt-4">
    <div class="col-lg-8">
        <div class="card mb-3">
            <div class="card-header">Butiran Permohonan</div>
            <div class="card-body">
                <dl class="row mb-0">
                    <dt class="col-sm-4">Pemohon</dt>
                    <dd class="col-sm-8">@Model.Pemohon</dd>
                    <dt class="col-sm-4">Kenderaan</dt>
                    <dd class="col-sm-8">
                        <strong>@Model.Application.Vehicle?.PlateNumber</strong>
                        (@Model.Application.Vehicle?.Jenama @Model.Application.Vehicle?.Model,
                         @Model.Application.Vehicle?.Warna)
                    </dd>
                    <dt class="col-sm-4">Jenis parkir</dt>
                    <dd class="col-sm-8">@Model.Application.JenisParkir</dd>
                    <dt class="col-sm-4">Justifikasi</dt>
                    <dd class="col-sm-8">@(Model.Application.Justifikasi ?? "—")</dd>
                </dl>
            </div>
        </div>

        <partial name="_AttachmentList" model="Model.Attachments" />
    </div>

    <div class="col-lg-4">
        @if (Model.BolehDiputuskan)
        {
            <div class="card mb-3">
                <div class="card-header">Keputusan</div>
                <div class="card-body">

                    <form asp-action="ApproveWithLot" method="post" class="mb-4">
                        @Html.AntiForgeryToken()
                        <input type="hidden" name="id" value="@Model.Submission.Id" />

                        <label class="form-label">Peruntukkan lot</label>
                        <select name="lotNumber" class="form-select mb-2" required>
                            <option value="">— Pilih lot @Model.Application.JenisParkir —</option>
                            @foreach (var lot in Model.LotTersedia)
                            {
                                <option value="@lot.LotNumber">
                                    @lot.LotNumber — @lot.Lokasi
                                </option>
                            }
                        </select>
                        @if (!Model.LotTersedia.Any())
                        {
                            <div class="alert alert-warning py-2 small">
                                Tiada lot @Model.Application.JenisParkir yang kosong.
                            </div>
                        }

                        <label class="form-label">Syarat kelulusan (pilihan)</label>
                        <textarea name="syarat" class="form-control mb-2" rows="2"
                                  placeholder="Cth: Sah sehingga 30 Jun sahaja; perlu diperbaharui."></textarea>

                        <label class="form-label">Catatan</label>
                        <textarea name="remarks" class="form-control mb-2" rows="2"></textarea>

                        <button type="submit" class="btn btn-success w-100"
                                disabled="@(!Model.LotTersedia.Any())">
                            Luluskan & Peruntukkan Lot
                        </button>
                    </form>

                    <hr />

                    <form asp-action="Reject" method="post">
                        @Html.AntiForgeryToken()
                        <input type="hidden" name="id" value="@Model.Submission.Id" />
                        <textarea name="remarks" class="form-control mb-2" rows="2"
                                  placeholder="Sebab penolakan (WAJIB)" required></textarea>
                        <button type="submit" class="btn btn-danger w-100">Tolak</button>
                    </form>
                </div>
            </div>
        }
        else
        {
            <div class="alert alert-secondary">
                Permohonan telah diputuskan.
                @if (Model.Application.LotNumber is not null)
                {
                    <div class="mt-2"><strong>Lot: @Model.Application.LotNumber</strong></div>
                }
                @if (Model.Application.SyaratKelulusan is not null)
                {
                    <div class="mt-2 small">
                        <strong>Syarat:</strong> @Model.Application.SyaratKelulusan
                    </div>
                }
            </div>
        }

        <partial name="_AuditTrail" model="Model.AuditLogs" />
    </div>
</div>
```

> **Perhatikan:** `_AttachmentList` dan `_AuditTrail` ialah partial **kongsi**. Hanya panel keputusan yang khusus modul — kerana peruntukan lot memang khusus modul.

### ✅ Semakan

- [ ] Skrin semakan memaparkan butiran + kenderaan + lampiran
- [ ] Dropdown lot menunjukkan hanya lot **kosong** bagi jenis yang betul
- [ ] Butang lulus dilumpuhkan bila tiada lot kosong
- [ ] Guna `_AttachmentList` dan `_AuditTrail` kongsi
- [ ] Selepas keputusan, lot & syarat dipaparkan

---

## Latihan 6 — Ujian

### Langkah

Rekod dalam `docs/kumpulan-2/ujian-manual.md`:

| # | Ujian | Jangkaan | Keputusan |
|---|-------|----------|-----------|
| 1 | Applicant → `/Akses/Queue` | 403 | |
| 2 | HrAdmin → `/Akses/Queue` | 403 | |
| 3 | SecurityAdmin → `/Akses/Queue` | 200 | |
| 4 | Luluskan pelekat | Siri `SK-2026-####` diperuntukkan | |
| 5 | Luluskan pelekat kedua | Siri **berbeza** | |
| 6 | Luluskan parkir tanpa pilih lot | Ditolak dengan mesej | |
| 7 | Luluskan parkir dengan lot C-01 | Lot direkod, muncul dalam audit | |
| 8 | Luluskan parkir **lain** dengan lot C-01 | **Disekat** — lot diguna | |
| 9 | Tolak permohonan parkir dengan lot C-01, kemudian peruntuk C-01 semula | **Berjaya** — ditolak melepaskan lot | |
| 10 | Kelulusan bersyarat | Syarat direkod & dipaparkan; status `AdminApproved` | |
| 11 | Tolak tanpa sebab | Ditolak | |
| 12 | Dua SecurityAdmin luluskan permohonan sama | Kedua gagal | |

> **Ujian 9 penting** — ia mengesahkan lot dilepaskan apabila permohonan ditolak, sama seperti peraturan pendua nombor plat.

### ✅ Semakan

- [ ] Kesemua 12 ujian dijalankan dan direkod
- [ ] Ujian 1, 2 memberi 403
- [ ] Ujian 8 disekat; ujian 9 berjaya
- [ ] Ujian 12 menunjukkan `IWorkflowService` menangkap peralihan kedua

---

## Latihan 7 — Tutup blok

```bash
git diff --name-only master
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Binaan bersih; aliran semakan berfungsi hujung-ke-hujung
- [ ] `Approve` **diatasi** dengan `base.Approve` dipanggil — tiada logik disalin
- [ ] **Tiada** ahli `SubmissionStatus` baharu ditambah
- [ ] Peruntukan siri & lot unik dan disemak
- [ ] Partial kongsi digunakan
- [ ] Hanya fail Kumpulan 2 disentuh
- [ ] Ujian didokumenkan
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 7–9

| Artifak | Lokasi |
|---------|--------|
| `SyaratKelulusan` + `ParkingLot` + seed | `Models/Akses/` |
| Migration `AksesPeruntukanDanLot` | `Migrations/` |
| `IAllocationService` | `Services/Akses/` |
| `ISecurityReviewService` | `Services/Akses/` |
| `Approve` diatasi + `ApproveWithLot` | `Controllers/{VehicleSticker,Parking}Controller.cs` |
| Baris gilir + skrin semakan | `Views/Akses/`, `Views/Parking/`, … |
| Ujian manual | `docs/kumpulan-2/ujian-manual.md` |

**Seterusnya (Hari 10–12):** penjanaan **QR/Barcode**, skrin semakan rondaan, dan laporan bercetak.
