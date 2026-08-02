# Lab · Kumpulan 2 · Hari 10–12 — QR, Ronda & Laporan

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok

```bash
git switch kump-2/akses-kenderaan
git pull --rebase origin master
git switch -c kump-2/feat/qr-ronda-laporan
dotnet build
```

**Semakan "sudah wujud?"**

```bash
grep -rn "QRCoder\|qrcode" Nres.Onboarding.Web/
grep -rn "csv\|Csv" Nres.Onboarding.Web/Services/
```

Tiada. QR khusus modul anda — **tiada kumpulan lain memerlukannya**, jadi ia milik `Services/Akses/`, bukan isu `shared`.

> **Bandingkan dengan Kumpulan 1:** mereka membuka isu `shared` untuk e-mel kerana keempat-empat kumpulan memerlukannya. Anda **tidak** membuka isu untuk QR kerana hanya anda memerlukannya. Menilai perbezaan ini dengan betul ialah kemahiran yang diajar.

### ✅ Semakan

- [ ] Anda mengesahkan tiada QR sedia ada
- [ ] Anda menilai QR sebagai khusus modul, bukan kongsi, dan boleh menyatakan sebabnya
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Token pengesahan

**Objektif:** Token rawak yang tidak boleh diteka pada permohonan yang diluluskan.

### Langkah

1. Tambah ke `AccessPassApplication` dan `VehicleStickerApplication`:

```csharp
/// <summary>
/// Token pengesahan untuk kod QR. Dijana pada KELULUSAN.
/// Kod QR mengandungi URL dengan token INI — bukan data peribadi.
/// Kekal walaupun pas dibatalkan, supaya imbasan boleh melaporkan
/// "TIDAK SAH" dan bukan "tidak dijumpai".
/// </summary>
public string? VerifyToken { get; set; }
```

2. Konfigurasi — indeks unik (kita mencari mengikut token pada setiap imbasan):

```csharp
builder.Property(a => a.VerifyToken).HasMaxLength(32);
builder.HasIndex(a => a.VerifyToken)
    .IsUnique()
    .HasFilter("\"VerifyToken\" IS NOT NULL");
```

3. `Services/Akses/VerifyTokenGenerator.cs`:

```csharp
using System.Security.Cryptography;

namespace Nres.Onboarding.Web.Services.Akses;

public static class VerifyTokenGenerator
{
    // Tiada 0/O/I/l — pengawal mungkin perlu menaipnya secara manual
    // apabila QR tidak boleh diimbas.
    private const string Aksara = "ABCDEFGHJKMNPQRSTUVWXYZ23456789";

    /// <summary>
    /// 16 aksara daripada RandomNumberGenerator.
    ///
    /// BUKAN Random (tidak selamat kriptografi — boleh diramal).
    /// BUKAN id berurutan (sesiapa boleh melayari pas orang lain).
    /// </summary>
    public static string Generate(int panjang = 16)
    {
        var hasil = new char[panjang];
        var bait = RandomNumberGenerator.GetBytes(panjang);

        for (var i = 0; i < panjang; i++)
            hasil[i] = Aksara[bait[i] % Aksara.Length];

        return new string(hasil);
    }
}
```

4. **Migration (slot!)** — umumkan, `pull --rebase`:

```bash
cd Nres.Onboarding.Web
dotnet ef migrations add AksesVerifyToken
dotnet ef database update
cd ..
```

### ✅ Semakan

- [ ] `VerifyToken` pada pas & pelekat, nullable
- [ ] Indeks unik ditapis
- [ ] Guna `RandomNumberGenerator`, bukan `Random`
- [ ] Aksara mengelirukan dikecualikan
- [ ] Migration melalui slot

---

## Latihan 2 — Jana token semasa kelulusan

**Objektif:** Sambungkan ke `override Approve` Hari 7–9.

### Langkah

Dalam `VehicleStickerController.Approve` (yang anda atasi pada Hari 7–9), tambah penjanaan token:

```csharp
public override async Task<IActionResult> Approve(int id, string? remarks)
{
    if (!User.IsInRole(AdminRole)) return Forbid();

    var app = await Db.Set<VehicleStickerApplication>()
        .Include(a => a.Submission)
        .Include(a => a.Vehicle)
        .FirstOrDefaultAsync(a => a.Submission!.Id == id);

    if (app is null) return NotFound();

    // Peruntukan siri (Hari 7–9)
    if (string.IsNullOrWhiteSpace(app.StickerSerialNo))
    {
        app.StickerSerialNo = await allocation.NextStickerSerialAsync(app.TahunPelekat);
        app.ValidFrom = new DateTime(app.TahunPelekat, 1, 1);
        app.ValidTo   = new DateTime(app.TahunPelekat, 12, 31);
    }

    // Token QR (Hari 10–12) — hanya sekali; kelulusan semula tidak menukarnya,
    // kerana pelekat fizikal mungkin sudah dicetak.
    app.VerifyToken ??= VerifyTokenGenerator.Generate();

    await Db.SaveChangesAsync();

    return await base.Approve(id,
        string.IsNullOrWhiteSpace(remarks)
            ? $"Pelekat {app.StickerSerialNo} diperuntukkan."
            : $"{remarks} (Pelekat {app.StickerSerialNo})");
}
```

Lakukan perkara yang sama dalam `AccessPassController.Approve`.

> **Perhatikan `??=`.** Token dijana **sekali sahaja**. Jika permohonan entah bagaimana diluluskan semula, pelekat fizikal yang dicetak dengan QR lama mesti kekal berfungsi.

### ✅ Semakan

- [ ] Token dijana pada kelulusan, untuk kedua-dua pas & pelekat
- [ ] `??=` digunakan — token tidak berubah pada kelulusan semula
- [ ] `base.Approve` masih dipanggil

---

## Latihan 3 — Servis QR

**Objektif:** Jana imej QR daripada token.

### Langkah

1. Tambah pakej:

```bash
cd Nres.Onboarding.Web
dotnet add package QRCoder
cd ..
```

2. `Services/Akses/IQrCodeService.cs`:

```csharp
namespace Nres.Onboarding.Web.Services.Akses;

public interface IQrCodeService
{
    /// <summary>PNG kod QR untuk URL pengesahan.</summary>
    byte[] GeneratePng(string verifyUrl, int pixelsPerModule = 10);

    /// <summary>Data URI untuk dibenamkan terus dalam &lt;img&gt;.</summary>
    string GenerateDataUri(string verifyUrl, int pixelsPerModule = 10);
}
```

3. `Services/Akses/QrCodeService.cs`:

```csharp
using QRCoder;

namespace Nres.Onboarding.Web.Services.Akses;

public class QrCodeService : IQrCodeService
{
    public byte[] GeneratePng(string verifyUrl, int pixelsPerModule = 10)
    {
        using var generator = new QRCodeGenerator();

        // ECCLevel.Q = ~25% pemulihan ralat. Pelekat kenderaan menjadi kotor,
        // calar, dan pudar di bawah matahari — pemulihan ralat yang lebih
        // tinggi bernilai kod yang sedikit lebih besar.
        using var data = generator.CreateQrCode(
            verifyUrl, QRCodeGenerator.ECCLevel.Q);

        using var qr = new PngByteQRCode(data);
        return qr.GetGraphic(pixelsPerModule);
    }

    public string GenerateDataUri(string verifyUrl, int pixelsPerModule = 10) =>
        $"data:image/png;base64,{Convert.ToBase64String(GeneratePng(verifyUrl, pixelsPerModule))}";
}
```

4. Daftar dalam `AksesModule`:

```csharp
services.AddSingleton<IQrCodeService, QrCodeService>();
```

> Singleton kerana ia tanpa keadaan dan selamat-thread — sama seperti `IFileStorageService`.

### ✅ Semakan

- [ ] Servis dalam `Services/Akses/`
- [ ] Tahap pemulihan ralat `Q` dengan justifikasi berkomen
- [ ] Didaftar sebagai singleton dalam `AksesModule`
- [ ] `Program.cs` **tidak** disunting

---

## Latihan 4 — Paparkan QR pada pas & pelekat

**Objektif:** Peserta melihat QR mereka; Keselamatan boleh mencetaknya.

### Langkah

1. Tambah action dalam `VehicleStickerController`:

```csharp
/// <summary>Halaman pelekat boleh cetak dengan QR.</summary>
[HttpGet]
public async Task<IActionResult> Sticker(int id)
{
    var app = await Db.Set<VehicleStickerApplication>()
        .AsNoTracking()
        .Include(a => a.Submission)
        .Include(a => a.Vehicle)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (app is null) return NotFound();

    // Pemohon boleh melihat miliknya; Keselamatan boleh melihat mana-mana.
    var milikSaya = app.Submission!.ApplicantUserId == currentUser.UserId;
    if (!milikSaya && !currentUser.IsInRole(AdminRole)) return Forbid();

    if (app.Submission.Status != SubmissionStatus.AdminApproved
        || app.VerifyToken is null)
    {
        TempData["Ralat"] = "Pelekat hanya tersedia selepas permohonan diluluskan.";
        return RedirectToAction(nameof(Edit), new { id });
    }

    var verifyUrl = Url.Action("Semak", "Akses",
        new { token = app.VerifyToken }, Request.Scheme)!;

    return View(new StickerViewModel
    {
        Application = app,
        QrDataUri = qrCodes.GenerateDataUri(verifyUrl),
        VerifyUrl = verifyUrl
    });
}
```

2. `Views/VehicleSticker/Sticker.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Akses.StickerViewModel
@{
    ViewData["Title"] = "Pelekat Kenderaan";
    Layout = "_Layout";
}

<div class="d-print-none mb-3">
    <button onclick="window.print()" class="btn btn-primary">Cetak Pelekat</button>
    <a asp-action="Edit" asp-route-id="@Model.Application.Id" class="btn btn-link">Kembali</a>
</div>

<div class="pelekat border border-2 border-dark p-4" style="max-width:420px">
    <div class="text-center">
        <div class="fw-bold">KEMENTERIAN SUMBER ASLI &amp; KELESTARIAN ALAM</div>
        <div class="fw-bold fs-5">PELEKAT KENDERAAN @Model.Application.TahunPelekat</div>
    </div>

    <hr />

    <div class="d-flex justify-content-between align-items-center">
        <div>
            <div class="fs-3 fw-bold">@Model.Application.Vehicle?.PlateNumber</div>
            <div class="small text-muted">
                @Model.Application.Vehicle?.Jenama @Model.Application.Vehicle?.Model
                — @Model.Application.Vehicle?.Warna
            </div>
            <div class="mt-2 small">
                <strong>Siri:</strong> @Model.Application.StickerSerialNo<br />
                <strong>Sah:</strong>
                @Model.Application.ValidFrom?.ToString("dd/MM/yyyy") –
                @Model.Application.ValidTo?.ToString("dd/MM/yyyy")
            </div>
        </div>
        <div class="text-center">
            <img src="@Model.QrDataUri" alt="Kod QR pengesahan"
                 style="width:110px;height:110px" />
            <div class="small text-muted mt-1">Imbas untuk sahkan</div>
        </div>
    </div>

    @if (Model.Application.SyaratKelulusan is not null)
    {
        <div class="mt-3 small border-top pt-2">
            <strong>Syarat:</strong> @Model.Application.SyaratKelulusan
        </div>
    }
</div>

@section Styles {
    <style>
        @@media print {
            .d-print-none { display: none !important; }
            body { margin: 0; }
            .pelekat { border-width: 2px !important; }
            nav, footer { display: none !important; }
        }
    </style>
}
```

> **Perhatikan `@@media`** — dalam Razor, `@@` melepaskan `@` literal.

### ✅ Semakan

- [ ] QR muncul pada pelekat yang diluluskan
- [ ] Pelekat untuk permohonan draf/ditolak ditolak dengan mesej
- [ ] Pemohon lain mendapat 403
- [ ] Pratonton cetakan menyembunyikan navigasi & butang

---

## Latihan 5 — Skrin semakan ronda

**Objektif:** Pengawal mengesahkan dalam < 5 saat, pada telefon.

### Langkah

1. `ViewModels/Akses/SemakViewModel.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Akses;

public class SemakViewModel
{
    public bool Dijumpai { get; set; }
    public bool Sah { get; set; }
    public string? SebabTidakSah { get; set; }

    public string? JenisNama { get; set; }
    public string? ReferenceNo { get; set; }
    public string? SerialNo { get; set; }
    public string? PlateNumber { get; set; }
    public string? HolderName { get; set; }
    public string? Pemohon { get; set; }
    public DateTime? ValidFrom { get; set; }
    public DateTime? ValidTo { get; set; }
    public string? SyaratKelulusan { get; set; }
    public SubmissionStatus? Status { get; set; }

    /// <summary>Untuk carian sandaran apabila QR tidak boleh diimbas.</summary>
    public string? Carian { get; set; }
}
```

2. Action `Semak` dalam `AksesController`:

```csharp
/// <summary>
/// Skrin semakan ronda. Dicapai dengan mengimbas QR (?token=) atau
/// dengan menaip nombor plat / rujukan (?carian=).
///
/// Memerlukan SecurityAdmin — ia mendedahkan nama pemegang dan nombor plat.
/// Jika pengawal belum log masuk, ASP.NET Core Identity mengalihkan ke
/// log masuk dan kembali ke sini selepas itu (ReturnUrl).
/// </summary>
[Authorize(Roles = "SecurityAdmin")]
[HttpGet]
public async Task<IActionResult> Semak(string? token, string? carian)
{
    var vm = new SemakViewModel { Carian = carian };

    if (string.IsNullOrWhiteSpace(token) && string.IsNullOrWhiteSpace(carian))
        return View(vm);

    // --- Cari pelekat ---
    var pelekat = await (
        from a in db.Set<VehicleStickerApplication>().AsNoTracking()
        join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
        join v in db.Set<Vehicle>().AsNoTracking() on a.VehicleId equals v.Id
        join p in db.UserProfiles.AsNoTracking() on s.ApplicantUserId equals p.UserId
        where (token != null && a.VerifyToken == token)
           || (carian != null && (v.PlateNumberNormalized == Vehicle.Normalize(carian)
                               || s.ReferenceNo == carian))
        select new { a, s, v, p.FullName }).FirstOrDefaultAsync();

    if (pelekat is not null)
    {
        vm.Dijumpai = true;
        vm.JenisNama = "Pelekat Kenderaan";
        vm.ReferenceNo = pelekat.s.ReferenceNo;
        vm.SerialNo = pelekat.a.StickerSerialNo;
        vm.PlateNumber = pelekat.v.PlateNumber;
        vm.Pemohon = pelekat.FullName;
        vm.ValidFrom = pelekat.a.ValidFrom;
        vm.ValidTo = pelekat.a.ValidTo;
        vm.SyaratKelulusan = pelekat.a.SyaratKelulusan;
        vm.Status = pelekat.s.Status;
        (vm.Sah, vm.SebabTidakSah) = NilaiKesahihan(
            pelekat.s.Status, pelekat.a.ValidFrom, pelekat.a.ValidTo);
        return View(vm);
    }

    // --- Cari pas ---
    var pas = await (
        from a in db.Set<AccessPassApplication>().AsNoTracking()
        join s in db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
        join p in db.UserProfiles.AsNoTracking() on s.ApplicantUserId equals p.UserId
        where (token != null && a.VerifyToken == token)
           || (carian != null && (s.ReferenceNo == carian || a.PassSerialNo == carian))
        select new { a, s, p.FullName }).FirstOrDefaultAsync();

    if (pas is not null)
    {
        vm.Dijumpai = true;
        vm.JenisNama = $"Pas Keselamatan ({pas.a.JenisPas})";
        vm.ReferenceNo = pas.s.ReferenceNo;
        vm.SerialNo = pas.a.PassSerialNo;
        vm.HolderName = pas.a.HolderName;
        vm.Pemohon = pas.FullName;
        vm.ValidFrom = pas.a.ValidFrom;
        vm.ValidTo = pas.a.ValidTo;
        vm.SyaratKelulusan = pas.a.SyaratKelulusan;
        vm.Status = pas.s.Status;
        (vm.Sah, vm.SebabTidakSah) = NilaiKesahihan(
            pas.s.Status, pas.a.ValidFrom, pas.a.ValidTo);
    }

    return View(vm);
}

/// <summary>
/// Sah hanya jika DILULUSKAN dan dalam tempoh. Dibatalkan/ditolak
/// mengembalikan tidak-sah dengan sebab — BUKAN "tidak dijumpai",
/// kerana pas fizikal masih wujud di tangan seseorang.
/// </summary>
private static (bool, string?) NilaiKesahihan(
    SubmissionStatus status, DateTime? dari, DateTime? hingga)
{
    if (status == SubmissionStatus.Cancelled)  return (false, "Permohonan telah DIBATALKAN.");
    if (status == SubmissionStatus.Rejected)   return (false, "Permohonan DITOLAK.");
    if (status != SubmissionStatus.AdminApproved
     && status != SubmissionStatus.Completed)  return (false, "Belum diluluskan.");

    var hariIni = DateTime.UtcNow.Date;
    if (dari is not null && hariIni < dari.Value.Date)
        return (false, $"Belum sah. Mula {dari:dd/MM/yyyy}.");
    if (hingga is not null && hariIni > hingga.Value.Date)
        return (false, $"TAMAT TEMPOH pada {hingga:dd/MM/yyyy}.");

    return (true, null);
}
```

3. `Views/Akses/Semak.cshtml` — direka untuk telefon:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Akses.SemakViewModel
@{ ViewData["Title"] = "Semakan Ronda"; }

<div class="container" style="max-width:520px">

    <form method="get" class="mb-4">
        <label class="form-label fw-bold">Semak nombor plat / rujukan</label>
        <div class="input-group input-group-lg">
            <input name="carian" value="@Model.Carian" class="form-control"
                   placeholder="WXY1234" autofocus />
            <button class="btn btn-primary px-4">Semak</button>
        </div>
        <div class="form-text">Atau imbas kod QR pada pas / pelekat.</div>
    </form>

    @if (Model.Dijumpai)
    {
        <div class="p-4 rounded text-center text-white mb-3
                    @(Model.Sah ? "bg-success" : "bg-danger")">
            <div class="display-4 fw-bold">@(Model.Sah ? "SAH" : "TIDAK SAH")</div>
            @if (!Model.Sah && Model.SebabTidakSah is not null)
            {
                <div class="fs-5 mt-2">@Model.SebabTidakSah</div>
            }
        </div>

        <div class="card">
            <div class="card-body">
                <div class="fs-2 fw-bold">
                    @(Model.PlateNumber ?? Model.HolderName)
                </div>
                <div class="text-muted mb-3">@Model.JenisNama</div>

                <dl class="row mb-0">
                    <dt class="col-5">No. Rujukan</dt>
                    <dd class="col-7">@Model.ReferenceNo</dd>
                    <dt class="col-5">No. Siri</dt>
                    <dd class="col-7">@Model.SerialNo</dd>
                    <dt class="col-5">Pemohon</dt>
                    <dd class="col-7">@Model.Pemohon</dd>
                    <dt class="col-5">Tempoh sah</dt>
                    <dd class="col-7">
                        @Model.ValidFrom?.ToString("dd/MM/yyyy") –
                        @Model.ValidTo?.ToString("dd/MM/yyyy")
                    </dd>
                </dl>

                @if (Model.SyaratKelulusan is not null)
                {
                    <div class="alert alert-warning mt-3 mb-0">
                        <strong>Syarat:</strong> @Model.SyaratKelulusan
                    </div>
                }
            </div>
        </div>
    }
    else if (!string.IsNullOrWhiteSpace(Model.Carian))
    {
        <div class="p-4 rounded text-center bg-secondary text-white">
            <div class="display-6 fw-bold">TIADA REKOD</div>
            <div class="mt-2">Tiada pas atau pelekat dijumpai untuk "@Model.Carian".</div>
        </div>
    }
</div>
```

4. **Uji pada telefon sebenar** (atau emulasi peranti dalam alat pembangun pelayar). Imbas QR daripada skrin pelekat.

### ✅ Semakan

- [ ] Mengimbas QR pelekat yang diluluskan menunjukkan jalur **SAH** hijau
- [ ] Mengimbas pas yang dibatalkan menunjukkan **TIDAK SAH** dengan sebab
- [ ] Pas tamat tempoh menunjukkan **TIDAK SAH — TAMAT TEMPOH**
- [ ] Carian nombor plat berfungsi sebagai sandaran
- [ ] Plat dengan format berbeza (`wxy 1234`) tetap dijumpai
- [ ] Applicant → `/Akses/Semak` memberi **403**
- [ ] Boleh dibaca pada skrin telefon

---

## Latihan 6 — Laporan & eksport CSV

**Objektif:** Senarai kerja yang Bahagian Keselamatan boleh cetak.

### Langkah

1. Action laporan dalam `AksesController`:

```csharp
[Authorize(Roles = "SecurityAdmin")]
public async Task<IActionResult> Laporan(string jenis = "pas-aktif")
{
    var vm = await securityReview.LaporanAsync(jenis);
    return View(vm);
}

/// <summary>
/// Eksport CSV. Nota: kami menjana CSV mudah dan bukan menggunakan
/// perpustakaan — set data kecil dan formatnya terkawal. Jika NRES
/// memerlukan Excel sebenar, Kumpulan 4 menggunakan ClosedXML; kita
/// akan bincang sama ada ia patut jadi komponen kongsi.
/// </summary>
[Authorize(Roles = "SecurityAdmin")]
public async Task<IActionResult> EksportCsv(string jenis = "pas-aktif")
{
    var vm = await securityReview.LaporanAsync(jenis);

    var sb = new System.Text.StringBuilder();
    sb.AppendLine("No. Rujukan,Jenis,No. Siri,Plat/Pemegang,Pemohon,Sah Dari,Sah Hingga,Status");

    foreach (var r in vm.Baris)
    {
        sb.AppendLine(string.Join(",",
            Escape(r.ReferenceNo), Escape(r.JenisNama), Escape(r.SerialNo),
            Escape(r.Subjek), Escape(r.Pemohon),
            r.ValidFrom?.ToString("yyyy-MM-dd"), r.ValidTo?.ToString("yyyy-MM-dd"),
            Escape(r.StatusNama)));
    }

    var bait = System.Text.Encoding.UTF8.GetPreamble()
        .Concat(System.Text.Encoding.UTF8.GetBytes(sb.ToString())).ToArray();

    return File(bait, "text/csv",
        $"laporan-{jenis}-{DateTime.Now:yyyyMMdd}.csv");

    // Medan yang mengandungi koma atau petikan mesti dipetik dan dilepaskan —
    // jika tidak, CSV rosak pada nama seperti "Ali, bin Ahmad".
    static string Escape(string? nilai) =>
        nilai is null ? "" :
        nilai.Contains(',') || nilai.Contains('"')
            ? $"\"{nilai.Replace("\"", "\"\"")}\""
            : nilai;
}
```

> **BOM UTF-8** (`GetPreamble`) diperlukan supaya Excel memaparkan aksara Bahasa Melayu dengan betul. Tanpanya, nama dengan aksara khas menjadi kacau.

2. View laporan dengan CSS cetakan:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Akses.LaporanViewModel
@{ ViewData["Title"] = Model.Tajuk; }

<div class="d-print-none mb-3 d-flex gap-2 align-items-end">
    <div>
        <label class="form-label">Laporan</label>
        <select class="form-select" onchange="location.href='?jenis='+this.value">
            <option value="pas-aktif"      selected="@(Model.Jenis == "pas-aktif")">Pas aktif</option>
            <option value="pelekat-tahun"  selected="@(Model.Jenis == "pelekat-tahun")">Pelekat mengikut tahun</option>
            <option value="lot-parkir"     selected="@(Model.Jenis == "lot-parkir")">Peruntukan lot</option>
            <option value="tamat-minggu"   selected="@(Model.Jenis == "tamat-minggu")">Tamat tempoh minggu ini</option>
        </select>
    </div>
    <button onclick="window.print()" class="btn btn-primary">Cetak</button>
    <a asp-action="EksportCsv" asp-route-jenis="@Model.Jenis" class="btn btn-outline-secondary">
        Eksport CSV
    </a>
</div>

<div class="d-none d-print-block mb-3">
    <h4>KEMENTERIAN SUMBER ASLI &amp; KELESTARIAN ALAM</h4>
    <h5>@Model.Tajuk</h5>
    <div class="small">Dicetak: @DateTime.Now.ToString("dd/MM/yyyy HH:mm")</div>
</div>

<h4 class="d-print-none">@Model.Tajuk</h4>
<p class="text-muted">@Model.Baris.Count rekod.</p>

<table class="table table-sm table-bordered">
    <thead>
        <tr>
            <th>No. Rujukan</th><th>Jenis</th><th>No. Siri</th>
            <th>Plat / Pemegang</th><th>Pemohon</th><th>Tempoh</th><th>Status</th>
        </tr>
    </thead>
    <tbody>
    @foreach (var r in Model.Baris)
    {
        <tr>
            <td>@r.ReferenceNo</td>
            <td>@r.JenisNama</td>
            <td>@r.SerialNo</td>
            <td>@r.Subjek</td>
            <td>@r.Pemohon</td>
            <td>@r.ValidFrom?.ToString("dd/MM/yy") – @r.ValidTo?.ToString("dd/MM/yy")</td>
            <td>@r.StatusNama</td>
        </tr>
    }
    </tbody>
</table>

@section Styles {
    <style>
        @@media print {
            .d-print-none { display: none !important; }
            .d-print-block { display: block !important; }
            nav, footer { display: none !important; }
            table { font-size: 10pt; }
            @@page { size: landscape; margin: 1cm; }
        }
    </style>
}
```

### ✅ Semakan

- [ ] Empat laporan berfungsi
- [ ] Pratonton cetakan menyembunyikan navigasi dan menunjukkan kepala rasmi
- [ ] CSV dimuat turun dan **dibuka dengan betul dalam Excel** (semak aksara BM)
- [ ] Nama dengan koma tidak merosakkan CSV
- [ ] Hanya `SecurityAdmin` boleh mengakses

---

## Latihan 7 — Tutup blok

```bash
git diff --name-only master
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] QR mengandungi **token**, bukan data peribadi
- [ ] Token daripada `RandomNumberGenerator`
- [ ] Skrin ronda memerlukan `SecurityAdmin`, diuji dengan 403
- [ ] Pas dibatalkan menunjukkan TIDAK SAH, bukan tidak dijumpai
- [ ] Laporan boleh dicetak; CSV betul dalam Excel
- [ ] Hanya fail Kumpulan 2 disentuh
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 10–12

| Artifak | Lokasi |
|---------|--------|
| `VerifyToken` + penjana | `Models/Akses/`, `Services/Akses/` |
| Migration `AksesVerifyToken` | `Migrations/` |
| `IQrCodeService` (QRCoder) | `Services/Akses/` |
| Halaman pelekat/pas boleh cetak dengan QR | `Views/VehicleSticker/Sticker.cshtml`, … |
| Skrin semakan ronda | `Views/Akses/Semak.cshtml` |
| Laporan + eksport CSV | `Views/Akses/Laporan.cshtml` |

**Seterusnya (Hari 13–14):** ujian hujung-ke-hujung, bug fixing, dan persediaan gabungan akhir.
