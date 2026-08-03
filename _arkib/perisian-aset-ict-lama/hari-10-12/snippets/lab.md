# Lab · Kumpulan 4 · Hari 10–12 — Peringatan, Dashboard & Eksport

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok

```bash
git switch kump-4/perisian-aset
git pull --rebase origin master
git switch -c kump-4/feat/peringatan-dashboard-eksport
dotnet build
```

**Semakan "sudah wujud?"**

```bash
grep -rn "INotificationService" Nres.Onboarding.Web/Services/
grep -rn "BackgroundService\|IHostedService" Nres.Onboarding.Web/
grep -rn "ClosedXML\|csv" Nres.Onboarding.Web/
```

`INotificationService` wujud — dan Kumpulan 1 mungkin telah menambah pelaksanaan SMTP melalui isu `shared` mereka. **Guna apa yang ada**; jangan bina penghantar e-mel anda sendiri.

### ✅ Semakan

- [ ] Anda mengesahkan `INotificationService` sedia ada (dan pelaksanaan mana yang aktif)
- [ ] Tiada `BackgroundService` sedia ada — anda yang pertama
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Jejak peringatan yang dihantar

**Objektif:** Hantar setiap tahap peringatan **sekali sahaja**.

### Langkah

1. Tambah ke `AssetLoanRequest`:

```csharp
/// <summary>
/// Tahap peringatan tertinggi yang telah dihantar.
/// 0 = tiada · 1 = awal (3 hari sebelum) · 2 = pada tarikh · 3 = eskalasi.
///
/// Tanpa ini, tugas harian menghantar peringatan yang SAMA setiap hari
/// dan orang berhenti membacanya.
/// </summary>
public int TahapPeringatanDihantar { get; set; }

public DateTime? TarikhPeringatanTerakhir { get; set; }
```

2. Kemas kini konfigurasi + **migration (slot!)**:

```bash
cd Nres.Onboarding.Web
dotnet ef migrations add AsetPeringatan
dotnet ef database update
cd ..
```

### ✅ Semakan

- [ ] Medan penjejakan ditambah
- [ ] Migration melalui slot
- [ ] Komen menjelaskan kenapa penjejakan diperlukan

---

## Latihan 2 — Servis pengesanan lewat tempoh

**Objektif:** Cari pinjaman yang memerlukan peringatan.

### Langkah

`Services/Aset/IOverdueService.cs`:

```csharp
namespace Nres.Onboarding.Web.Services.Aset;

public enum TahapPeringatan { Awal = 1, PadaTarikh = 2, Eskalasi = 3 }

public record PinjamanPerluPeringatan(
    int LoanRequestId, int SubmissionId, string ReferenceNo,
    string ApplicantUserId, string? AssetTag,
    DateTime TarikhJangkaPulang, int HariLewat, TahapPeringatan Tahap);

public interface IOverdueService
{
    /// <summary>Pinjaman yang memerlukan peringatan pada tahap yang belum dihantar.</summary>
    Task<IReadOnlyList<PinjamanPerluPeringatan>> FindDueRemindersAsync(
        CancellationToken ct = default);

    /// <summary>Tandakan peringatan sebagai dihantar.</summary>
    Task MarkReminderSentAsync(int loanRequestId, TahapPeringatan tahap,
        CancellationToken ct = default);

    /// <summary>Semua pinjaman lewat tempoh — untuk dashboard.</summary>
    Task<IReadOnlyList<PinjamanPerluPeringatan>> AllOverdueAsync(
        CancellationToken ct = default);

    /// <summary>Pinjaman diluluskan tetapi belum diakui selepas N hari.</summary>
    Task<IReadOnlyList<PinjamanPerluPeringatan>> UnacknowledgedAsync(
        int selepasHari = 3, CancellationToken ct = default);
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Aset;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.Services.Aset;

public class OverdueService(ApplicationDbContext db) : IOverdueService
{
    public async Task<IReadOnlyList<PinjamanPerluPeringatan>> FindDueRemindersAsync(
        CancellationToken ct = default)
    {
        var hariIni = DateTime.UtcNow.Date;

        var aktif = await (
            from r in db.Set<AssetLoanRequest>().AsNoTracking()
            join s in db.Submissions.AsNoTracking() on r.SubmissionId equals s.Id
            where s.Status == SubmissionStatus.AdminApproved
               && r.TarikhJangkaPulang != null
            select new
            {
                r.Id, r.SubmissionId, s.ReferenceNo, s.ApplicantUserId,
                AssetTag = r.Asset!.AssetTag,
                Tarikh = r.TarikhJangkaPulang!.Value,
                r.TahapPeringatanDihantar
            }).ToListAsync(ct);

        var hasil = new List<PinjamanPerluPeringatan>();

        foreach (var p in aktif)
        {
            var hariLewat = (hariIni - p.Tarikh.Date).Days;

            // Tahap tertinggi yang LAYAK sekarang.
            TahapPeringatan? tahap = hariLewat switch
            {
                >= 7  => TahapPeringatan.Eskalasi,
                >= 0  => TahapPeringatan.PadaTarikh,
                >= -3 => TahapPeringatan.Awal,
                _     => null
            };

            // Hantar hanya jika tahap ini LEBIH TINGGI daripada yang telah dihantar.
            if (tahap is not null && (int)tahap > p.TahapPeringatanDihantar)
            {
                hasil.Add(new PinjamanPerluPeringatan(
                    p.Id, p.SubmissionId, p.ReferenceNo, p.ApplicantUserId,
                    p.AssetTag, p.Tarikh, hariLewat, tahap.Value));
            }
        }

        return hasil;
    }

    public async Task MarkReminderSentAsync(int loanRequestId, TahapPeringatan tahap,
        CancellationToken ct = default)
    {
        var pinjaman = await db.Set<AssetLoanRequest>()
            .FirstOrDefaultAsync(r => r.Id == loanRequestId, ct);
        if (pinjaman is null) return;

        pinjaman.TahapPeringatanDihantar = (int)tahap;
        pinjaman.TarikhPeringatanTerakhir = DateTime.UtcNow;
        await db.SaveChangesAsync(ct);
    }

    public async Task<IReadOnlyList<PinjamanPerluPeringatan>> AllOverdueAsync(
        CancellationToken ct = default)
    {
        var hariIni = DateTime.UtcNow.Date;

        var senarai = await (
            from r in db.Set<AssetLoanRequest>().AsNoTracking()
            join s in db.Submissions.AsNoTracking() on r.SubmissionId equals s.Id
            where s.Status == SubmissionStatus.AdminApproved
               && r.TarikhJangkaPulang != null
               && r.TarikhJangkaPulang < hariIni
            orderby r.TarikhJangkaPulang
            select new { r.Id, r.SubmissionId, s.ReferenceNo, s.ApplicantUserId,
                         AssetTag = r.Asset!.AssetTag, Tarikh = r.TarikhJangkaPulang!.Value })
            .ToListAsync(ct);

        return senarai.Select(p => new PinjamanPerluPeringatan(
            p.Id, p.SubmissionId, p.ReferenceNo, p.ApplicantUserId, p.AssetTag,
            p.Tarikh, (hariIni - p.Tarikh.Date).Days, TahapPeringatan.Eskalasi))
            .ToList();
    }

    public async Task<IReadOnlyList<PinjamanPerluPeringatan>> UnacknowledgedAsync(
        int selepasHari = 3, CancellationToken ct = default)
    {
        var ambang = DateTime.UtcNow.Date.AddDays(-selepasHari);

        var senarai = await (
            from r in db.Set<AssetLoanRequest>().AsNoTracking()
            join s in db.Submissions.AsNoTracking() on r.SubmissionId equals s.Id
            where s.Status == SubmissionStatus.AdminApproved
               && r.AssetId != null
               && !r.AkuanTerima
               && s.CompletedAt < ambang
            orderby s.CompletedAt
            select new { r.Id, r.SubmissionId, s.ReferenceNo, s.ApplicantUserId,
                         AssetTag = r.Asset!.AssetTag,
                         Tarikh = r.TarikhJangkaPulang ?? DateTime.UtcNow })
            .ToListAsync(ct);

        return senarai.Select(p => new PinjamanPerluPeringatan(
            p.Id, p.SubmissionId, p.ReferenceNo, p.ApplicantUserId, p.AssetTag,
            p.Tarikh, 0, TahapPeringatan.Awal)).ToList();
    }
}
```

> **Logik "tahap lebih tinggi daripada yang dihantar"** ialah bahagian penting. Ia bermakna pinjaman yang lewat 10 hari menerima eskalasi **sekali**, bukan setiap hari.

### ✅ Semakan

- [ ] Tiga tahap dikesan dengan betul
- [ ] Peringatan dihantar hanya jika tahap **lebih tinggi** daripada terakhir
- [ ] `UnacknowledgedAsync` mencari pinjaman dalam limbo
- [ ] Didaftar dalam `AsetModule`

---

## Latihan 3 — Tugas latar belakang

**Objektif:** Peringatan automatik — dengan had yang difahami.

### Langkah

`Services/Aset/OverdueReminderService.cs`:

```csharp
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Web.Services.Aset;

/// <summary>
/// Menghantar peringatan pinjaman secara automatik.
///
/// ⚠️ HAD YANG DIKETAHUI (dokumenkan dalam serahan NRES):
///  · Berjalan dalam proses aplikasi — jika aplikasi tidur, tiada peringatan.
///  · Berbilang contoh aplikasi = peringatan pendua.
///  · Mula semula aplikasi memulakan semula pemasa.
///
/// Untuk pengeluaran, penjadual luaran (Hangfire, cron + endpoint,
/// Azure Function) lebih dipercayai. Kami menggunakan BackgroundService
/// kerana ia terbina dan mengajar corak.
/// </summary>
public class OverdueReminderService(
    IServiceProvider services,
    ILogger<OverdueReminderService> logger) : BackgroundService
{
    private static readonly TimeSpan Selang = TimeSpan.FromHours(24);

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        // Tunggu sebentar selepas permulaan supaya aplikasi sedia sepenuhnya.
        await Task.Delay(TimeSpan.FromSeconds(30), stoppingToken);

        while (!stoppingToken.IsCancellationRequested)
        {
            try
            {
                await JalankanPusinganAsync(stoppingToken);
            }
            catch (Exception ex)
            {
                // Tugas latar belakang TIDAK BOLEH mati kerana satu pusingan gagal.
                logger.LogError(ex, "Pusingan peringatan gagal.");
            }

            await Task.Delay(Selang, stoppingToken);
        }
    }

    private async Task JalankanPusinganAsync(CancellationToken ct)
    {
        // BackgroundService ialah singleton; DbContext ialah scoped.
        // Kita MESTI mencipta skop untuk setiap pusingan.
        using var scope = services.CreateScope();
        var overdue = scope.ServiceProvider.GetRequiredService<IOverdueService>();
        var notify = scope.ServiceProvider.GetRequiredService<INotificationService>();

        var perlu = await overdue.FindDueRemindersAsync(ct);
        logger.LogInformation("Pusingan peringatan: {Kiraan} pinjaman.", perlu.Count);

        foreach (var p in perlu)
        {
            var (subjek, mesej) = Templat(p);

            await notify.NotifyAsync(p.ApplicantUserId, subjek, mesej, ct);

            // Eskalasi juga memberitahu ICT.
            if (p.Tahap == TahapPeringatan.Eskalasi)
            {
                await notify.NotifyAsync("ict-queue",
                    $"ESKALASI: {p.ReferenceNo} lewat {p.HariLewat} hari",
                    $"Aset {p.AssetTag} belum dipulangkan. Sila susuli.", ct);
            }

            await overdue.MarkReminderSentAsync(p.LoanRequestId, p.Tahap, ct);
        }
    }

    private static (string, string) Templat(PinjamanPerluPeringatan p) => p.Tahap switch
    {
        TahapPeringatan.Awal => (
            $"Peringatan: pinjaman {p.ReferenceNo} akan tamat tempoh",
            $"""
             <p>Salam sejahtera,</p>
             <p>Pinjaman aset <strong>{p.AssetTag}</strong> anda dijangka
                dipulangkan pada <strong>{p.TarikhJangkaPulang:dd/MM/yyyy}</strong>
                ({Math.Abs(p.HariLewat)} hari lagi).</p>
             <p>Sila rekod pemulangan dalam sistem apabila anda memulangkannya.</p>
             """),

        TahapPeringatan.PadaTarikh => (
            $"Pinjaman {p.ReferenceNo} perlu dipulangkan hari ini",
            $"""
             <p>Salam sejahtera,</p>
             <p>Pinjaman aset <strong>{p.AssetTag}</strong> perlu dipulangkan
                <strong>hari ini</strong> ({p.TarikhJangkaPulang:dd/MM/yyyy}).</p>
             <p>Sila pulangkan ke Unit Aset ICT dan rekod dalam sistem.</p>
             """),

        _ => (
            $"LEWAT TEMPOH: pinjaman {p.ReferenceNo} — {p.HariLewat} hari",
            $"""
             <p>Salam sejahtera,</p>
             <p>Pinjaman aset <strong>{p.AssetTag}</strong> telah
                <strong>lewat {p.HariLewat} hari</strong>
                (sepatutnya dipulangkan {p.TarikhJangkaPulang:dd/MM/yyyy}).</p>
             <p>Unit Aset ICT telah dimaklumkan. Sila pulangkan segera.</p>
             """)
    };
}
```

Daftar dalam `AsetModule`:

```csharp
services.AddHostedService<OverdueReminderService>();
```

> **`CreateScope()` wajib.** `BackgroundService` ialah singleton; `DbContext` ialah scoped. Menyuntik `DbContext` terus ke dalam tugas latar belakang ialah pepijat klasik — ia berfungsi sekali kemudian gagal.

**Untuk menguji tanpa menunggu 24 jam:** tambah endpoint manual sementara:

```csharp
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> RunRemindersNow()
{
    var perlu = await overdue.FindDueRemindersAsync();
    // ... hantar seperti dalam tugas latar belakang
    TempData["Mesej"] = $"{perlu.Count} peringatan dihantar.";
    return RedirectToAction(nameof(Dashboard));
}
```

### ✅ Semakan

- [ ] `CreateScope()` digunakan setiap pusingan
- [ ] Pengecualian ditangkap — tugas tidak mati
- [ ] Eskalasi memberitahu ICT juga
- [ ] Peringatan ditandakan sebagai dihantar
- [ ] Had didokumenkan dalam komen kelas
- [ ] Endpoint ujian manual berfungsi

---

## Latihan 4 — Papan pemuka inventori

### Langkah

```csharp
public async Task<InventoryDashboardViewModel> DashboardAsync(CancellationToken ct = default)
{
    var tahunIni = DateTime.UtcNow.Year;

    var aset = db.Set<Asset>().AsNoTracking().Where(a => a.IsActive);

    var vm = new InventoryDashboardViewModel
    {
        MengikutStatus = await aset
            .GroupBy(a => a.Status)
            .Select(g => new StatusKiraan(g.Key, g.Count()))
            .ToListAsync(ct),

        MengikutKategori = await aset
            .GroupBy(a => a.Kategori)
            .Select(g => new KategoriKiraan(
                g.Key,
                g.Count(),
                g.Count(a => a.Status == AssetStatus.Available)))
            .ToListAsync(ct),

        // Nombor yang muncul dalam mesyuarat pengurusan.
        NilaiDipinjam = await aset
            .Where(a => a.Status == AssetStatus.OnLoan)
            .SumAsync(a => a.Harga ?? 0, ct),

        NilaiHilang = await aset
            .Where(a => a.Status == AssetStatus.Lost)
            .SumAsync(a => a.Harga ?? 0, ct),

        LewatTempoh = await overdue.AllOverdueAsync(ct),
        BelumDiakui = await overdue.UnacknowledgedAsync(3, ct),

        LesenHampirHabis = (await inventory.AllLicenceStatusAsync(ct))
            .Where(l => l.Baki is not null && l.Baki <= 2)
            .ToList()
    };

    return vm;
}
```

View — senarai kerja diutamakan:

```cshtml
@if (Model.LewatTempoh.Any())
{
    <div class="card border-danger mb-4">
        <div class="card-header bg-danger text-white">
            ⚠ Pinjaman lewat tempoh (@Model.LewatTempoh.Count)
        </div>
        <table class="table table-sm mb-0">
            <thead><tr><th>Rujukan</th><th>Aset</th><th>Sepatutnya pulang</th><th>Lewat</th></tr></thead>
            <tbody>
            @foreach (var p in Model.LewatTempoh)
            {
                <tr>
                    <td>@p.ReferenceNo</td>
                    <td>@p.AssetTag</td>
                    <td>@p.TarikhJangkaPulang.ToString("dd/MM/yyyy")</td>
                    <td><span class="badge bg-danger">@p.HariLewat hari</span></td>
                </tr>
            }
            </tbody>
        </table>
    </div>
}

@if (Model.BelumDiakui.Any())
{
    <div class="card border-warning mb-4">
        <div class="card-header bg-warning">
            Diluluskan tetapi belum diakui (@Model.BelumDiakui.Count)
        </div>
        <div class="card-body small text-muted">
            Aset ini telah diperuntukkan tetapi pemohon belum mengakui penerimaan
            selepas 3 hari. Sama ada aset masih dalam stor, atau pemohon terlupa
            mengakui — kedua-duanya perlu susulan.
        </div>
        <table class="table table-sm mb-0">
            @foreach (var p in Model.BelumDiakui)
            {
                <tr><td>@p.ReferenceNo</td><td>@p.AssetTag</td></tr>
            }
        </table>
    </div>
}

<div class="row g-3 mb-4">
    <div class="col-md-6"><div class="card"><div class="card-body">
        <div class="display-6">RM @Model.NilaiDipinjam.ToString("N2")</div>
        <div class="text-muted">Nilai aset sedang dipinjam</div>
    </div></div></div>
    <div class="col-md-6"><div class="card border-danger"><div class="card-body">
        <div class="display-6 text-danger">RM @Model.NilaiHilang.ToString("N2")</div>
        <div class="text-muted">Nilai aset hilang</div>
    </div></div></div>
</div>
```

### ✅ Semakan

- [ ] Kiraan mengikut status & kategori
- [ ] Senarai lewat tempoh dan belum diakui **diutamakan**
- [ ] Nilai dipinjam & hilang dipaparkan
- [ ] Lesen hampir habis (baki ≤ 2)
- [ ] Hanya `IctAdmin`

---

## Latihan 5 — Eksport Excel

**Objektif:** Laporan berbilang helaian yang pengurusan aset boleh gunakan.

### Langkah

1. Tambah pakej:

```bash
cd Nres.Onboarding.Web
dotnet add package ClosedXML
cd ..
```

2. `Services/Aset/IAssetReportService.cs`:

```csharp
namespace Nres.Onboarding.Web.Services.Aset;

public interface IAssetReportService
{
    /// <summary>Buku kerja Excel berbilang helaian: aset, lesen, lewat tempoh.</summary>
    Task<byte[]> BuildWorkbookAsync(CancellationToken ct = default);
}
```

```csharp
using ClosedXML.Excel;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Aset;

namespace Nres.Onboarding.Web.Services.Aset;

public class AssetReportService(
    ApplicationDbContext db,
    IInventoryService inventory,
    IOverdueService overdue) : IAssetReportService
{
    public async Task<byte[]> BuildWorkbookAsync(CancellationToken ct = default)
    {
        using var wb = new XLWorkbook();

        // --- Helaian 1: Daftar aset ---
        var aset = await db.Set<Asset>().AsNoTracking()
            .Where(a => a.IsActive).OrderBy(a => a.AssetTag).ToListAsync(ct);

        var ws1 = wb.Worksheets.Add("Daftar Aset");
        string[] kepala1 = ["Tag Aset", "No. Siri", "Kategori", "Nama",
                            "Jenama", "Model", "Status", "Lokasi",
                            "Tarikh Perolehan", "Harga (RM)"];

        for (var c = 0; c < kepala1.Length; c++)
            ws1.Cell(1, c + 1).Value = kepala1[c];

        for (var i = 0; i < aset.Count; i++)
        {
            var a = aset[i];
            var r = i + 2;
            ws1.Cell(r, 1).Value = a.AssetTag;
            ws1.Cell(r, 2).Value = a.SerialNumber;
            ws1.Cell(r, 3).Value = a.Kategori.ToString();
            ws1.Cell(r, 4).Value = a.Nama;
            ws1.Cell(r, 5).Value = a.Jenama;
            ws1.Cell(r, 6).Value = a.Model;
            ws1.Cell(r, 7).Value = a.Status.ToString();
            ws1.Cell(r, 8).Value = a.Lokasi;
            // Tarikh sebagai TARIKH, bukan teks — supaya Excel boleh mengisih.
            if (a.TarikhPerolehan is not null)
                ws1.Cell(r, 9).Value = a.TarikhPerolehan.Value;
            if (a.Harga is not null)
                ws1.Cell(r, 10).Value = a.Harga.Value;
        }

        Formatkan(ws1, kepala1.Length, aset.Count + 1);
        ws1.Column(9).Style.DateFormat.Format = "dd/mm/yyyy";
        ws1.Column(10).Style.NumberFormat.Format = "#,##0.00";

        // Jumlah nilai di bawah lajur harga.
        if (aset.Count > 0)
        {
            var barisJumlah = aset.Count + 2;
            ws1.Cell(barisJumlah, 9).Value = "JUMLAH";
            ws1.Cell(barisJumlah, 9).Style.Font.Bold = true;
            ws1.Cell(barisJumlah, 10).FormulaA1 = $"SUM(J2:J{aset.Count + 1})";
            ws1.Cell(barisJumlah, 10).Style.Font.Bold = true;
        }

        // --- Helaian 2: Lesen perisian ---
        var lesen = await inventory.AllLicenceStatusAsync(ct);
        var ws2 = wb.Worksheets.Add("Lesen Perisian");
        string[] kepala2 = ["Perisian", "Jumlah Lesen", "Diguna", "Baki", "Status"];

        for (var c = 0; c < kepala2.Length; c++)
            ws2.Cell(1, c + 1).Value = kepala2[c];

        for (var i = 0; i < lesen.Count; i++)
        {
            var l = lesen[i];
            var r = i + 2;
            ws2.Cell(r, 1).Value = l.Nama;
            ws2.Cell(r, 2).Value = l.Jumlah?.ToString() ?? "Tanpa had";
            ws2.Cell(r, 3).Value = l.Diguna;
            ws2.Cell(r, 4).Value = l.Baki?.ToString() ?? "—";
            ws2.Cell(r, 5).Value = l.Tersedia ? "Tersedia" : "HABIS";

            if (!l.Tersedia)
                ws2.Row(r).Style.Fill.BackgroundColor = XLColor.LightPink;
        }
        Formatkan(ws2, kepala2.Length, lesen.Count + 1);

        // --- Helaian 3: Lewat tempoh ---
        var lewat = await overdue.AllOverdueAsync(ct);
        var ws3 = wb.Worksheets.Add("Lewat Tempoh");
        string[] kepala3 = ["No. Rujukan", "Tag Aset", "Sepatutnya Pulang", "Hari Lewat"];

        for (var c = 0; c < kepala3.Length; c++)
            ws3.Cell(1, c + 1).Value = kepala3[c];

        for (var i = 0; i < lewat.Count; i++)
        {
            var p = lewat[i];
            var r = i + 2;
            ws3.Cell(r, 1).Value = p.ReferenceNo;
            ws3.Cell(r, 2).Value = p.AssetTag;
            ws3.Cell(r, 3).Value = p.TarikhJangkaPulang;
            ws3.Cell(r, 4).Value = p.HariLewat;
        }
        Formatkan(ws3, kepala3.Length, lewat.Count + 1);
        ws3.Column(3).Style.DateFormat.Format = "dd/mm/yyyy";

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();

        static void Formatkan(IXLWorksheet ws, int lajur, int baris)
        {
            var kepala = ws.Range(1, 1, 1, lajur);
            kepala.Style.Font.Bold = true;
            kepala.Style.Fill.BackgroundColor = XLColor.LightGray;

            ws.SheetView.FreezeRows(1);            // kepala kekal semasa skrol
            ws.Columns().AdjustToContents();

            if (baris > 1)
                ws.Range(1, 1, baris, lajur).SetAutoFilter();   // penapis Excel
        }
    }
}
```

3. Action:

```csharp
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> EksportExcel()
{
    var bait = await reports.BuildWorkbookAsync();

    return File(bait,
        "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet",
        $"laporan-aset-ict-{DateTime.Now:yyyyMMdd}.xlsx");
}
```

> **Tiga perkara yang menjadikan ini berguna** dan bukan hanya CSV yang berlebihan: tarikh sebagai jenis tarikh (boleh diisih), penapis auto (pengurusan aset boleh menapis sendiri), dan panel beku (kepala kekal semasa skrol).

### ✅ Semakan

- [ ] Tiga helaian dijana
- [ ] Tarikh ialah **tarikh**, bukan teks
- [ ] Harga diformat sebagai mata wang dengan jumlah formula
- [ ] Lesen habis diserlahkan
- [ ] Penapis auto & panel beku berfungsi
- [ ] Fail dibuka bersih dalam Excel

---

## Latihan 6 — Tutup blok

```bash
git diff --name-only master
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Tiga tahap peringatan, setiap satu dihantar sekali
- [ ] `BackgroundService` menggunakan `CreateScope()`
- [ ] Had tugas latar belakang didokumenkan
- [ ] Dashboard mengutamakan senarai kerja
- [ ] Excel berbilang helaian dengan pemformatan betul
- [ ] Guna `INotificationService` kongsi (bukan penghantar sendiri)
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 10–12

| Artifak | Lokasi |
|---------|--------|
| Penjejakan peringatan + migration | `Models/Aset/`, `Migrations/` |
| `IOverdueService` | `Services/Aset/` |
| `OverdueReminderService` (BackgroundService) | `Services/Aset/` |
| Papan pemuka inventori | `Views/Aset/Dashboard.cshtml` |
| `IAssetReportService` (ClosedXML) | `Services/Aset/` |

**Seterusnya (Hari 13–14):** ujian pemulangan lewat, lesen & stok; refactor; sedia gabung.
