# Lab · Kumpulan 1 · Hari 10–12 — Notifikasi, PDF & Analitik

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok & isu `shared`

```bash
git switch kump-1/lapor-diri
git pull --rebase origin master
git switch -c kump-1/feat/notifikasi-pdf-analitik
dotnet build
```

**Semakan "sudah wujud?"**

```bash
grep -rn "INotificationService" Nres.Onboarding.Web/Services/
grep -rn "AddSingleton<INotificationService" Nres.Onboarding.Web/Program.cs
```

Anda akan mendapati `ConsoleNotificationService` didaftar dalam `Program.cs` — **fail beku**.

**Buka isu `shared` SEKARANG**, sebelum menulis kod:

```markdown
Tajuk: [shared] Pemilihan INotificationService berasaskan konfigurasi — Kumpulan 1

## Apa yang kami perlukan
Kumpulan 1 memerlukan notifikasi e-mel sebenar (SMTP) menggantikan
ConsoleNotificationService.

## Kenapa modul kami perlukannya
URS-LD-012: pemohon mesti diberitahu melalui e-mel apabila permohonan
diluluskan atau ditolak.

## Modul lain yang mungkin perlukannya
SEMUA EMPAT. Kumpulan 2 (kelulusan pas), Kumpulan 3 (status akaun),
Kumpulan 4 (peringatan lewat tempoh) kesemuanya memerlukan e-mel.

## Cadangan
Jurulatih menambah pemilihan berasaskan konfigurasi ke Program.cs SEKALI:
guna SmtpNotificationService bila Notifications:Provider = "Smtp",
jika tidak Console. Kami membina pelaksanaan SMTP dalam folder kami dan
mendaftarkannya melalui AddLaporDiriModule().
```

> **Ini latihan sebenar, bukan formaliti.** Empat kumpulan memerlukan e-mel. Jika setiap satu membina penghantar e-melnya sendiri, kita mempunyai empat. Perbincangan ini ialah keseluruhan tujuan proses `shared`.

### ✅ Semakan

- [ ] Isu `shared` dibuka sebelum menulis kod
- [ ] Isu menamakan modul lain yang mungkin memerlukannya
- [ ] Jurulatih telah membuat keputusan sebelum anda meneruskan

---

## Latihan 1 — Pelaksanaan notifikasi SMTP

**Objektif:** Tambah penghantar e-mel — jangan sunting yang sedia ada.

### Langkah

1. Tambah pakej:

```bash
cd Nres.Onboarding.Web
dotnet add package MailKit
cd ..
```

2. `Services/LaporDiri/EmailOptions.cs`:

```csharp
namespace Nres.Onboarding.Web.Services.LaporDiri;

public class EmailOptions
{
    public const string Section = "Notifications:Smtp";

    public string Host { get; set; } = "localhost";
    public int Port { get; set; } = 25;
    public bool UseSsl { get; set; }
    public string FromAddress { get; set; } = "no-reply@nres.test";
    public string FromName { get; set; } = "Sistem Onboarding NRES";

    /// <summary>
    /// ⚠️ Untuk latihan sahaja, dan hanya melalui user-secrets / pembolehubah
    /// persekitaran — JANGAN sekali-kali dalam appsettings.json yang di-commit.
    /// </summary>
    public string? Username { get; set; }
    public string? Password { get; set; }
}
```

3. `Services/LaporDiri/SmtpNotificationService.cs`:

```csharp
using MailKit.Net.Smtp;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MimeKit;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Web.Services.LaporDiri;

/// <summary>
/// Pelaksanaan KEDUA INotificationService. Kami TIDAK menyunting
/// ConsoleNotificationService — tiga kumpulan lain masih bergantung padanya,
/// dan ia kekal sebagai lalai untuk pembangunan tempatan.
/// </summary>
public class SmtpNotificationService(
    IOptions<EmailOptions> options,
    ApplicationDbContext db,
    ILogger<SmtpNotificationService> logger) : INotificationService
{
    private readonly EmailOptions _opt = options.Value;

    public async Task NotifyAsync(string toUserId, string subject, string body,
        CancellationToken ct = default)
    {
        try
        {
            var email = await db.Users
                .Where(u => u.Id == toUserId)
                .Select(u => u.Email)
                .FirstOrDefaultAsync(ct);

            if (string.IsNullOrWhiteSpace(email))
            {
                logger.LogWarning("Tiada e-mel untuk pengguna {UserId}", toUserId);
                return;
            }

            var mesej = new MimeMessage();
            mesej.From.Add(new MailboxAddress(_opt.FromName, _opt.FromAddress));
            mesej.To.Add(MailboxAddress.Parse(email));
            mesej.Subject = subject;
            mesej.Body = new BodyBuilder { HtmlBody = body }.ToMessageBody();

            using var client = new SmtpClient();
            await client.ConnectAsync(_opt.Host, _opt.Port, _opt.UseSsl, ct);

            if (!string.IsNullOrWhiteSpace(_opt.Username))
                await client.AuthenticateAsync(_opt.Username, _opt.Password, ct);

            await client.SendAsync(mesej, ct);
            await client.DisconnectAsync(true, ct);

            logger.LogInformation("E-mel dihantar ke {Email}: {Subject}", email, subject);
        }
        catch (Exception ex)
        {
            // KEGAGALAN NOTIFIKASI TIDAK PERNAH MENGGAGALKAN OPERASI PERNIAGAAN.
            // Permohonan sudah dihantar/diluluskan — itu fakta tersimpan.
            // E-mel ialah kesan sampingan; log dan teruskan.
            logger.LogError(ex, "Gagal menghantar notifikasi kepada {UserId}", toUserId);
        }
    }
}
```

4. Konfigurasi dalam `appsettings.Development.json` (**jangan** commit kata laluan):

```json
{
  "Notifications": {
    "Provider": "Console",
    "Smtp": {
      "Host": "localhost",
      "Port": 1025,
      "UseSsl": false,
      "FromAddress": "no-reply@nres.test",
      "FromName": "Sistem Onboarding NRES"
    }
  }
}
```

> **Untuk ujian tempatan** guna [Papercut-SMTP](https://github.com/ChangemakerStudios/Papercut-SMTP) atau [MailHog](https://github.com/mailhog/MailHog) pada port 1025 — ia menangkap e-mel tanpa menghantarnya ke mana-mana.

5. Daftar dalam **modul anda**:

```csharp
// Services/LaporDiri/LaporDiriModule.cs
services.Configure<EmailOptions>(
    configuration.GetSection(EmailOptions.Section));
services.AddScoped<SmtpNotificationService>();
```

> Pemilihan `INotificationService` mana yang aktif ialah keputusan `shared` dari Latihan 0 — jurulatih menambahnya ke `Program.cs` sekali.

### ✅ Semakan

- [ ] `ConsoleNotificationService` **tidak** diubah suai
- [ ] Kegagalan SMTP ditangkap dan dilog, tidak dilontar
- [ ] Tiada kata laluan dalam fail yang di-commit
- [ ] `dotnet build` berjaya

---

## Latihan 2 — Templat notifikasi

**Objektif:** Mesej yang berguna, bukan "Status berubah".

### Langkah

1. `Services/LaporDiri/NotificationTemplates.cs`:

```csharp
namespace Nres.Onboarding.Web.Services.LaporDiri;

public static class NotificationTemplates
{
    public static (string Subject, string Body) Dihantar(string rujukan, string nama) =>
        ($"Permohonan Lapor Diri {rujukan} telah diterima",
         $"""
          <p>Salam sejahtera {nama},</p>
          <p>Permohonan Lapor Diri anda telah diterima dan sedang menunggu semakan
             Bahagian Pengurusan Sumber Manusia.</p>
          <p><strong>No. rujukan: {rujukan}</strong></p>
          <p>Anda akan dimaklumkan sebaik sahaja keputusan dibuat.</p>
          <hr><p style="font-size:small;color:#666">
             E-mel ini dijana komputer. Sila jangan balas.</p>
          """);

    public static (string, string) Diluluskan(string rujukan, string nama, string? catatan) =>
        ($"Permohonan Lapor Diri {rujukan} DILULUSKAN",
         $"""
          <p>Salam sejahtera {nama},</p>
          <p>Permohonan Lapor Diri anda ({rujukan}) telah <strong>diluluskan</strong>.</p>
          {(string.IsNullOrWhiteSpace(catatan) ? "" : $"<p>Catatan: {catatan}</p>")}
          <p>Sila muat turun Slip Akuan Lapor Diri anda melalui sistem.</p>
          <hr><p style="font-size:small;color:#666">
             E-mel ini dijana komputer. Sila jangan balas.</p>
          """);

    public static (string, string) Ditolak(string rujukan, string nama, string sebab) =>
        ($"Permohonan Lapor Diri {rujukan} ditolak",
         $"""
          <p>Salam sejahtera {nama},</p>
          <p>Permohonan Lapor Diri anda ({rujukan}) telah <strong>ditolak</strong>.</p>
          <p><strong>Sebab:</strong> {sebab}</p>
          <p>Sila betulkan dan hantar permohonan baharu melalui sistem.</p>
          <hr><p style="font-size:small;color:#666">
             E-mel ini dijana komputer. Sila jangan balas.</p>
          """);
}
```

2. Guna dalam `Submit` (dan kelas asas sudah memberitahu pada approve/reject — semak dengan jurulatih sama ada templat perlu masuk ke kelas asas sebagai kerja `shared`).

### ✅ Semakan

- [ ] Templat menamakan pemohon dan nombor rujukan
- [ ] Sebab penolakan disertakan
- [ ] Penafian "dijana komputer" ada
- [ ] E-mel ditangkap dalam MailHog/Papercut semasa ujian

---

## Latihan 3 — Slip Akuan PDF

**Objektif:** Dokumen rasmi untuk permohonan yang diluluskan.

### Langkah

1. Tambah QuestPDF:

```bash
cd Nres.Onboarding.Web
dotnet add package QuestPDF
cd ..
```

2. `Services/LaporDiri/ISlipAkuanService.cs`:

```csharp
namespace Nres.Onboarding.Web.Services.LaporDiri;

public interface ISlipAkuanService
{
    /// <summary>Jana PDF Slip Akuan. Hanya untuk permohonan yang diluluskan.</summary>
    Task<byte[]> GenerateAsync(int applicationId, CancellationToken ct = default);
}
```

3. `Services/LaporDiri/SlipAkuanService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.LaporDiri;
using Nres.Onboarding.Web.Models.Shared;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Nres.Onboarding.Web.Services.LaporDiri;

public class SlipAkuanService(ApplicationDbContext db) : ISlipAkuanService
{
    public async Task<byte[]> GenerateAsync(int applicationId, CancellationToken ct = default)
    {
        var app = await db.Set<OfficerReportingApplication>()
            .AsNoTracking()
            .Include(a => a.Submission)
            .Include(a => a.Department)
            .Include(a => a.Position)
            .Include(a => a.Grade)
            .FirstOrDefaultAsync(a => a.Id == applicationId, ct)
            ?? throw new InvalidOperationException("Permohonan tidak dijumpai.");

        // Slip hanya untuk permohonan yang DILULUSKAN — semakan di sini,
        // bukan hanya di controller.
        if (app.Submission!.Status is not (SubmissionStatus.AdminApproved
                                        or SubmissionStatus.Completed))
            throw new InvalidOperationException(
                "Slip Akuan hanya boleh dijana untuk permohonan yang diluluskan.");

        var kelulusan = await db.AuditLogs.AsNoTracking()
            .Where(l => l.SubmissionId == app.SubmissionId && l.Action == "Approved")
            .OrderByDescending(l => l.CreatedAt)
            .FirstOrDefaultAsync(ct);

        var dokumen = Document.Create(container =>
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(2, Unit.Centimetre);
                page.DefaultTextStyle(x => x.FontSize(11).FontFamily("Arial"));

                page.Header().Column(col =>
                {
                    col.Item().AlignCenter().Text("KEMENTERIAN SUMBER ASLI & KELESTARIAN ALAM")
                        .Bold().FontSize(13);
                    col.Item().AlignCenter().Text("SLIP AKUAN LAPOR DIRI")
                        .Bold().FontSize(15);
                    col.Item().PaddingTop(4).AlignCenter()
                        .Text($"No. Rujukan: {app.Submission.ReferenceNo}").FontSize(11);
                    col.Item().PaddingTop(8).LineHorizontal(1);
                });

                page.Content().PaddingVertical(16).Column(col =>
                {
                    col.Spacing(10);

                    col.Item().Text("MAKLUMAT PEGAWAI").Bold();
                    col.Item().Element(c => Baris(c, "Nama Penuh", app.FullName));
                    col.Item().Element(c => Baris(c, "No. Kad Pengenalan", app.IdentityNo));
                    col.Item().Element(c => Baris(c, "Jawatan", app.Position?.Name ?? "—"));
                    col.Item().Element(c => Baris(c, "Gred", app.Grade?.Name ?? "—"));
                    col.Item().Element(c => Baris(c, "Bahagian", app.Department?.Name ?? "—"));

                    col.Item().PaddingTop(10).Text("MAKLUMAT PERKHIDMATAN").Bold();
                    col.Item().Element(c => Baris(c, "Tarikh Lapor Diri",
                        app.ReportingDate?.ToString("dd MMMM yyyy") ?? "—"));
                    col.Item().Element(c => Baris(c, "Agensi Sebelum Ini",
                        string.IsNullOrWhiteSpace(app.PreviousAgency)
                            ? "Lantikan baharu" : app.PreviousAgency));

                    col.Item().PaddingTop(10).Text("PENGESAHAN").Bold();
                    col.Item().Element(c => Baris(c, "Tarikh Kelulusan",
                        kelulusan?.CreatedAt.ToLocalTime().ToString("dd MMMM yyyy") ?? "—"));
                    col.Item().Element(c => Baris(c, "Status", "DILULUSKAN"));

                    col.Item().PaddingTop(24).Text(
                        "Slip ini adalah pengesahan rasmi bahawa pegawai di atas telah " +
                        "melapor diri di Kementerian Sumber Asli & Kelestarian Alam.")
                        .Italic();
                });

                page.Footer().Column(col =>
                {
                    col.Item().LineHorizontal(1);
                    col.Item().PaddingTop(4).Row(row =>
                    {
                        row.RelativeItem().Text(
                            $"Dicetak: {DateTime.Now:dd/MM/yyyy HH:mm}").FontSize(8);
                        row.RelativeItem().AlignRight().Text(
                            "Dokumen ini dijana komputer dan tidak memerlukan tandatangan.")
                            .FontSize(8);
                    });
                });
            });
        });

        return dokumen.GeneratePdf();
    }

    private static void Baris(IContainer container, string label, string nilai) =>
        container.Row(row =>
        {
            row.ConstantItem(160).Text(label).SemiBold();
            row.ConstantItem(10).Text(":");
            row.RelativeItem().Text(nilai);
        });
}
```

4. Lesen QuestPDF — tambah sekali dalam `LaporDiriModule`:

```csharp
QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;
services.AddScoped<ISlipAkuanService, SlipAkuanService>();
```

5. Action muat turun dengan kebenaran:

```csharp
[HttpGet]
public async Task<IActionResult> SlipAkuan(int id)
{
    var app = await Db.Set<OfficerReportingApplication>()
        .AsNoTracking()
        .Include(a => a.Submission)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (app is null) return NotFound();

    // Pemohon boleh mendapat slipnya sendiri; HR boleh mendapat mana-mana.
    var milikSaya = app.Submission!.ApplicantUserId == currentUser.UserId;
    if (!milikSaya && !currentUser.IsInRole(AdminRole)) return Forbid();

    try
    {
        var pdf = await slipAkuan.GenerateAsync(id);
        return File(pdf, "application/pdf",
            $"SlipAkuan-{app.Submission.ReferenceNo}.pdf");
    }
    catch (InvalidOperationException ex)
    {
        TempData["Ralat"] = ex.Message;
        return RedirectToAction(nameof(Edit), new { id });
    }
}
```

6. Tambah butang dalam `Form.cshtml` (hanya bila diluluskan):

```cshtml
@if (Model.Status is SubmissionStatus.AdminApproved or SubmissionStatus.Completed)
{
    <a asp-action="SlipAkuan" asp-route-id="@Model.Id" class="btn btn-success">
        Muat Turun Slip Akuan (PDF)
    </a>
}
```

### ✅ Semakan

- [ ] PDF dijana untuk permohonan yang diluluskan
- [ ] Cuba menjana untuk draf **ditolak** dengan mesej jelas
- [ ] Pemohon lain mendapat 403
- [ ] PDF mengandungi nombor rujukan, maklumat pegawai, tarikh kelulusan
- [ ] Butang hanya muncul selepas kelulusan

---

## Latihan 4 — Papan pemuka analitis HR

**Objektif:** Statistik pengurusan, diagregat dalam pangkalan data.

### Langkah

1. `ViewModels/LaporDiri/HrAnalyticsViewModel.cs`:

```csharp
namespace Nres.Onboarding.Web.ViewModels.LaporDiri;

public class HrAnalyticsViewModel
{
    public int Tahun { get; set; } = DateTime.UtcNow.Year;

    public IReadOnlyList<BulananItem> MengikutBulan { get; set; } = [];
    public IReadOnlyList<StatusItem> MengikutStatus { get; set; } = [];
    public IReadOnlyList<BahagianItem> MengikutBahagian { get; set; } = [];

    public double PurataHariKelulusan { get; set; }
    public double KadarPenolakan { get; set; }

    public record BulananItem(int Bulan, string NamaBulan, int Jumlah);
    public record StatusItem(string Status, int Jumlah);
    public record BahagianItem(string Bahagian, int Jumlah);
}
```

2. Tambah ke `HrReviewService`:

```csharp
public async Task<HrAnalyticsViewModel> AnalyticsAsync(
    int tahun, CancellationToken ct = default)
{
    var submissions = db.Submissions.AsNoTracking()
        .Where(s => s.ModuleCode == ModuleCodes.LaporDiri
                 && s.CreatedAt.Year == tahun);

    // Pengagregatan berlaku di PANGKALAN DATA — tiada baris ditarik ke memori.
    var bulanan = await submissions
        .GroupBy(s => s.CreatedAt.Month)
        .Select(g => new { Bulan = g.Key, Jumlah = g.Count() })
        .ToListAsync(ct);

    var mengikutStatus = await submissions
        .GroupBy(s => s.Status)
        .Select(g => new { Status = g.Key, Jumlah = g.Count() })
        .ToListAsync(ct);

    var mengikutBahagian = await (
        from s in submissions
        join a in db.Set<OfficerReportingApplication>().AsNoTracking()
            on s.Id equals a.SubmissionId
        where a.Department != null
        group s by a.Department!.Name into g
        select new { Bahagian = g.Key, Jumlah = g.Count() })
        .OrderByDescending(x => x.Jumlah)
        .ToListAsync(ct);

    // Purata masa kelulusan: dari SubmittedAt ke baris audit "Approved".
    var tempohKelulusan = await (
        from s in submissions
        where s.SubmittedAt != null
        join l in db.AuditLogs.AsNoTracking() on s.Id equals l.SubmissionId
        where l.Action == "Approved"
        select EF.Functions.DateDiffDay(s.SubmittedAt!.Value, l.CreatedAt))
        .ToListAsync(ct);

    var jumlahDiputuskan = await submissions.CountAsync(
        s => s.Status == SubmissionStatus.AdminApproved
          || s.Status == SubmissionStatus.Rejected, ct);
    var jumlahDitolak = await submissions.CountAsync(
        s => s.Status == SubmissionStatus.Rejected, ct);

    var namaBulan = new[] { "", "Jan", "Feb", "Mac", "Apr", "Mei", "Jun",
                            "Jul", "Ogos", "Sep", "Okt", "Nov", "Dis" };

    return new HrAnalyticsViewModel
    {
        Tahun = tahun,
        MengikutBulan = Enumerable.Range(1, 12)
            .Select(m => new HrAnalyticsViewModel.BulananItem(
                m, namaBulan[m],
                bulanan.FirstOrDefault(x => x.Bulan == m)?.Jumlah ?? 0))
            .ToList(),
        MengikutStatus = mengikutStatus
            .Select(x => new HrAnalyticsViewModel.StatusItem(
                x.Status.ToString(), x.Jumlah)).ToList(),
        MengikutBahagian = mengikutBahagian
            .Select(x => new HrAnalyticsViewModel.BahagianItem(
                x.Bahagian, x.Jumlah)).ToList(),
        PurataHariKelulusan = tempohKelulusan.Count > 0
            ? tempohKelulusan.Average(d => d ?? 0) : 0,
        KadarPenolakan = jumlahDiputuskan > 0
            ? jumlahDitolak * 100.0 / jumlahDiputuskan : 0
    };
}
```

> **Nota:** `EF.Functions.DateDiffDay` ialah fungsi SQL Server. Pada SQLite ia tidak diterjemah — untuk latihan, muatkan pasangan tarikh dan kira dalam C#, dan bincangkan perbezaan penyedia. Ini contoh baik mengapa "EF Core mengabstrak pangkalan data" mempunyai had.

3. Carta mudah tanpa perpustakaan luaran — bar CSS:

```cshtml
@model Nres.Onboarding.Web.ViewModels.LaporDiri.HrAnalyticsViewModel
@{
    ViewData["Title"] = $"Analitik Lapor Diri — {Model.Tahun}";
    var maks = Model.MengikutBulan.Any() ? Model.MengikutBulan.Max(x => x.Jumlah) : 1;
}

<h2>@ViewData["Title"]</h2>

<div class="row g-3 my-3">
    <div class="col-md-6">
        <div class="card"><div class="card-body">
            <div class="display-6">@Model.PurataHariKelulusan.ToString("0.0") hari</div>
            <div class="text-muted">Purata masa kelulusan</div>
        </div></div>
    </div>
    <div class="col-md-6">
        <div class="card"><div class="card-body">
            <div class="display-6">@Model.KadarPenolakan.ToString("0.0")%</div>
            <div class="text-muted">Kadar penolakan</div>
        </div></div>
    </div>
</div>

<h5 class="mt-4">Permohonan mengikut bulan</h5>
<table class="table table-sm align-middle">
@foreach (var b in Model.MengikutBulan)
{
    <tr>
        <td style="width:60px">@b.NamaBulan</td>
        <td>
            <div class="progress" style="height:20px">
                <div class="progress-bar"
                     style="width:@(maks == 0 ? 0 : b.Jumlah * 100 / maks)%">
                    @(b.Jumlah > 0 ? b.Jumlah.ToString() : "")
                </div>
            </div>
        </td>
    </tr>
}
</table>

<h5 class="mt-4">Mengikut bahagian</h5>
<table class="table table-sm">
    <thead><tr><th>Bahagian</th><th class="text-end">Jumlah</th></tr></thead>
    <tbody>
    @foreach (var d in Model.MengikutBahagian)
    {
        <tr><td>@d.Bahagian</td><td class="text-end">@d.Jumlah</td></tr>
    }
    </tbody>
</table>
```

4. Action:

```csharp
[Authorize(Roles = "HrAdmin")]
public async Task<IActionResult> Analytics(int? tahun)
{
    return View(await hrReview.AnalyticsAsync(tahun ?? DateTime.UtcNow.Year));
}
```

### ✅ Semakan

- [ ] Analitik memaparkan carta bulanan, mengikut bahagian, purata masa, kadar penolakan
- [ ] Pengagregatan menggunakan `GroupBy` + `CountAsync` dalam pangkalan data
- [ ] Hanya `HrAdmin` boleh mengaksesnya
- [ ] Anda memahami had `DateDiffDay` pada SQLite

---

## Latihan 5 — Tutup blok

```bash
git diff --name-only master
```

Hanya fail `LaporDiri` — **kecuali** perubahan `Program.cs` yang **jurulatih** buat untuk isu `shared` notifikasi.

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Notifikasi e-mel berfungsi (ditangkap dalam MailHog/Papercut)
- [ ] Kegagalan e-mel tidak menggagalkan penghantaran/kelulusan
- [ ] `ConsoleNotificationService` **tidak** diubah suai
- [ ] Slip Akuan PDF dijana, dengan kebenaran diuji
- [ ] Analitik diagregat dalam pangkalan data
- [ ] Isu `shared` diselesaikan dengan betul, bukan diketepikan
- [ ] Tiada kata laluan di-commit
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 10–12

| Artifak | Lokasi |
|---------|--------|
| `SmtpNotificationService` + `EmailOptions` | `Services/LaporDiri/` |
| Templat notifikasi | `Services/LaporDiri/NotificationTemplates.cs` |
| `ISlipAkuanService` + PDF | `Services/LaporDiri/` |
| Analitik | `HrReviewService.AnalyticsAsync` |
| View analitik + butang PDF | `Views/OfficerReporting/` |

**Seterusnya (Hari 13–14):** xUnit, optimasi query, refactor, dan persediaan gabungan akhir.
