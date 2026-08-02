# Lab · Kumpulan 4 · Hari 7–9 — Kelulusan, Pemulangan & Transaksi

> Konsep: [`../README.md`](../README.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok

```bash
git switch kump-4/perisian-aset
git pull --rebase origin master
git switch -c kump-4/feat/kelulusan-dan-pemulangan
dotnet build
```

**Prompt AI:**

```text
Merujuk AGENTS.md: saya Kumpulan 4. Meluluskan pinjaman aset mesti melakukan
empat perkara bersama: peruntukkan aset, tukar status aset ke OnLoan, tukar
status permohonan, dan tulis audit. Semuanya mesti berjaya atau tiada langsung.
Apakah pendekatan yang betul dalam EF Core, dan adakah transaksi mencukupi
untuk menghalang dua pentadbir memperuntukkan aset yang sama serentak?
```

> Jawapan betul: `BeginTransactionAsync` untuk atomik — **tetapi transaksi tidak menghalang perlumbaan**. Anda juga perlu menyemak semula status aset **di dalam** transaksi. Jika AI hanya menyebut transaksi, ia terlepas separuh jawapan.

### ✅ Semakan

- [ ] Anda faham transaksi ≠ perlindungan perlumbaan
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Servis peruntukan aset

**Objektif:** Peruntukan atomik dengan semakan perlumbaan.

### Langkah

`Services/Aset/IAssetAllocationService.cs`:

```csharp
using Nres.Onboarding.Web.Models.Aset;

namespace Nres.Onboarding.Web.Services.Aset;

public record AllocationResult(bool Berjaya, string? AssetTag, string? Sebab);

public interface IAssetAllocationService
{
    /// <summary>
    /// Peruntukkan aset kepada pinjaman dan tukar statusnya ke OnLoan.
    /// Keseluruhan operasi ATOMIK; menyemak semula ketersediaan di dalam
    /// transaksi untuk menghalang dua peruntukan serentak.
    /// </summary>
    Task<AllocationResult> AllocateAsync(int loanRequestId, int assetId,
        string? remarks, CancellationToken ct = default);

    /// <summary>Pulangkan aset ke stok mengikut kondisi yang direkod.</summary>
    Task<AllocationResult> ReturnAsync(int assetReturnId, KondisiPulangan kondisi,
        string? catatan, string diperiksaOlehUserId, CancellationToken ct = default);
}
```

`Services/Aset/AssetAllocationService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Aset;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Web.Services.Aset;

public class AssetAllocationService(
    ApplicationDbContext db,
    IWorkflowService workflow,
    IAuditLogService audit) : IAssetAllocationService
{
    public async Task<AllocationResult> AllocateAsync(int loanRequestId, int assetId,
        string? remarks, CancellationToken ct = default)
    {
        await using var transaksi = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var pinjaman = await db.Set<AssetLoanRequest>()
                .Include(r => r.Submission)
                .FirstOrDefaultAsync(r => r.Id == loanRequestId, ct);

            if (pinjaman is null)
                return new AllocationResult(false, null, "Permohonan tidak dijumpai.");

            if (pinjaman.Submission!.Status != SubmissionStatus.Submitted)
                return new AllocationResult(false, null,
                    "Permohonan ini bukan menunggu kelulusan.");

            // ⚠️ SEMAKAN PERLUMBAAN — dimuat DENGAN penjejakan, DI DALAM transaksi.
            // Transaksi menjadikan operasi atomik; semakan INI yang menghalang
            // dua pentadbir memperuntukkan aset yang sama.
            var aset = await db.Set<Asset>()
                .FirstOrDefaultAsync(a => a.Id == assetId, ct);

            if (aset is null)
                return new AllocationResult(false, null, "Aset tidak dijumpai.");

            if (aset.Status != AssetStatus.Available)
                return new AllocationResult(false, aset.AssetTag,
                    $"Aset {aset.AssetTag} tidak lagi tersedia " +
                    $"(status: {aset.Status}). Sila pilih unit lain.");

            if (aset.Kategori != pinjaman.KategoriDipohon)
                return new AllocationResult(false, aset.AssetTag,
                    $"Aset {aset.AssetTag} ialah {aset.Kategori}, " +
                    $"tetapi permohonan meminta {pinjaman.KategoriDipohon}.");

            // --- Empat perubahan, satu transaksi ---
            pinjaman.AssetId = aset.Id;                 // 1
            aset.Status = AssetStatus.OnLoan;           // 2
            await db.SaveChangesAsync(ct);

            await workflow.TransitionAsync(pinjaman.Submission,               // 3 + 4
                SubmissionStatus.AdminApproved, "Approved",
                $"Aset {aset.AssetTag} diperuntukkan. {remarks}".Trim(), ct);

            await transaksi.CommitAsync(ct);
            return new AllocationResult(true, aset.AssetTag, null);
        }
        catch
        {
            await transaksi.RollbackAsync(ct);
            throw;
        }
    }

    public async Task<AllocationResult> ReturnAsync(int assetReturnId,
        KondisiPulangan kondisi, string? catatan, string diperiksaOlehUserId,
        CancellationToken ct = default)
    {
        await using var transaksi = await db.Database.BeginTransactionAsync(ct);
        try
        {
            var pulangan = await db.Set<AssetReturn>()
                .Include(r => r.Submission)
                .Include(r => r.AssetLoanRequest)!.ThenInclude(l => l!.Asset)
                .FirstOrDefaultAsync(r => r.Id == assetReturnId, ct);

            if (pulangan is null)
                return new AllocationResult(false, null, "Rekod pemulangan tidak dijumpai.");

            var aset = pulangan.AssetLoanRequest?.Asset;
            if (aset is null)
                return new AllocationResult(false, null,
                    "Pinjaman ini tiada aset diperuntukkan.");

            // --- Kondisi menentukan status aset seterusnya ---
            // Aset Lost TIDAK dipadam — ia kekal dalam daftar sehingga
            // dihapus kira secara rasmi, dan audit memerlukan rekod itu.
            aset.Status = kondisi switch
            {
                KondisiPulangan.Baik   => AssetStatus.Available,
                KondisiPulangan.Rosak  => AssetStatus.UnderMaintenance,
                KondisiPulangan.Hilang => AssetStatus.Lost,
                _ => aset.Status
            };

            pulangan.Kondisi = kondisi;
            pulangan.CatatanIct = catatan;
            pulangan.DiperiksaOlehUserId = diperiksaOlehUserId;
            pulangan.TarikhPulang ??= DateTime.UtcNow;

            // Tutup pinjaman asal.
            var pinjaman = pulangan.AssetLoanRequest!;
            if (pinjaman.Submission is not null
                && pinjaman.Submission.Status == SubmissionStatus.AdminApproved)
            {
                await workflow.TransitionAsync(pinjaman.Submission,
                    SubmissionStatus.Completed, "LoanClosed",
                    $"Aset {aset.AssetTag} dipulangkan ({kondisi}).", ct);
            }

            await db.SaveChangesAsync(ct);

            await workflow.TransitionAsync(pulangan.Submission!,
                SubmissionStatus.Completed, "ReturnProcessed",
                $"Kondisi: {kondisi}. Aset {aset.AssetTag} → {aset.Status}. {catatan}".Trim(), ct);

            await transaksi.CommitAsync(ct);
            return new AllocationResult(true, aset.AssetTag, null);
        }
        catch
        {
            await transaksi.RollbackAsync(ct);
            throw;
        }
    }
}
```

Daftar dalam `AsetModule`:

```csharp
services.AddScoped<IAssetAllocationService, AssetAllocationService>();
```

> **Perhatikan servis mengembalikan `AllocationResult`, bukan melontar,** untuk kegagalan yang **dijangka** (aset diambil, kategori salah). Pengecualian dikhaskan untuk kegagalan **tidak dijangka** — dan itulah yang mencetuskan rollback.

### ✅ Semakan

- [ ] Keseluruhan operasi dalam `BeginTransactionAsync`
- [ ] Aset dimuat **dengan penjejakan** di dalam transaksi
- [ ] Status disemak semula sebelum ditetapkan
- [ ] Kategori disahkan sepadan
- [ ] Kegagalan dijangka mengembalikan hasil; tidak dijangka melontar + rollback
- [ ] Kondisi memetakan ke status aset yang betul

---

## Latihan 2 — Baris gilir & skrin semakan ICT

**Objektif:** ICT melihat permohonan menunggu merentas ketiga-tiga jenis.

### Langkah

1. Baris gilir gabungan (perisian + pinjaman + pemulangan):

```csharp
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> Queue(string? jenis, SubmissionStatus? status)
{
    var tapis = status ?? SubmissionStatus.Submitted;

    var perisian = await (
        from r in Db.Set<SoftwareRequest>().AsNoTracking()
        join s in Db.Submissions.AsNoTracking() on r.SubmissionId equals s.Id
        join p in Db.UserProfiles.AsNoTracking() on s.ApplicantUserId equals p.UserId
        where s.Status == tapis
        select new QueueItem(r.Id, s.Id, s.ReferenceNo, "Perisian", "Software",
            p.FullName, r.SoftwareCatalogItem!.Nama, s.SubmittedAt))
        .ToListAsync();

    var pinjaman = await (
        from r in Db.Set<AssetLoanRequest>().AsNoTracking()
        join s in Db.Submissions.AsNoTracking() on r.SubmissionId equals s.Id
        join p in Db.UserProfiles.AsNoTracking() on s.ApplicantUserId equals p.UserId
        where s.Status == tapis
        select new QueueItem(r.Id, s.Id, s.ReferenceNo, "Pinjaman Aset", "Asset",
            p.FullName, r.KategoriDipohon.ToString(), s.SubmittedAt))
        .ToListAsync();

    var pulangan = await (
        from r in Db.Set<AssetReturn>().AsNoTracking()
        join s in Db.Submissions.AsNoTracking() on r.SubmissionId equals s.Id
        join p in Db.UserProfiles.AsNoTracking() on s.ApplicantUserId equals p.UserId
        where s.Status == tapis
        select new QueueItem(r.Id, s.Id, s.ReferenceNo, "Pemulangan", "AssetReturn",
            p.FullName, r.AssetLoanRequest!.Asset!.AssetTag, s.SubmittedAt))
        .ToListAsync();

    var semua = perisian.Concat(pinjaman).Concat(pulangan)
        .Where(x => jenis is null || x.JenisNama == jenis)
        .OrderBy(x => x.SubmittedAt)
        .ToList();

    ViewBag.Jenis = jenis;
    ViewBag.Status = tapis;
    return View(semua);
}
```

2. Skrin semakan pinjaman menunjukkan **unit tersedia untuk dipilih**:

```csharp
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> ReviewLoan(int id)
{
    var app = await Db.Set<AssetLoanRequest>().AsNoTracking()
        .Include(r => r.Submission)
        .Include(r => r.Asset)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (app is null) return NotFound();

    var vm = new LoanReviewViewModel
    {
        Application = app,
        Pemohon = await Db.UserProfiles.AsNoTracking()
            .Where(p => p.UserId == app.Submission!.ApplicantUserId)
            .Select(p => p.FullName).FirstOrDefaultAsync(),
        // Senarai dimuat SEKARANG — tetapi disemak semula semasa peruntukan.
        UnitTersedia = await inventory.AvailableAssetsAsync(app.KategoriDipohon),
        BolehDiputuskan = app.Submission!.Status == SubmissionStatus.Submitted,
        AuditLogs = await Db.AuditLogs.AsNoTracking()
            .Where(l => l.SubmissionId == app.SubmissionId)
            .OrderByDescending(l => l.CreatedAt).ToListAsync()
    };

    return View(vm);
}
```

### ✅ Semakan

- [ ] Baris gilir menunjukkan ketiga-tiga jenis
- [ ] Boleh menapis mengikut jenis dan status
- [ ] Skrin semakan menunjukkan unit tersedia untuk dipilih
- [ ] Guna `_AuditTrail` **kongsi**

---

## Latihan 3 — Kelulusan dengan peruntukan

**Objektif:** Sambungkan skrin ke servis peruntukan atomik.

### Langkah

```csharp
/// <summary>
/// Meluluskan pinjaman DAN memperuntukkan aset — mesti atomik.
///
/// Kami TIDAK memanggil base.Approve() di sini. Sebabnya: base.Approve
/// menjalankan peralihan status DI LUAR transaksi kami, jadi kegagalan
/// peruntukan akan meninggalkan status yang telah berubah.
/// IAssetAllocationService mengendalikan keseluruhan operasi termasuk
/// peralihan status dan audit.
///
/// Bandingkan dengan Kumpulan 2, yang BOLEH memanggil base.Approve kerana
/// peruntukan siri mereka tidak memerlukan transaksi.
/// </summary>
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> ApproveLoan(int id, int assetId, string? remarks)
{
    if (assetId <= 0)
    {
        TempData["Ralat"] = "Sila pilih unit aset untuk diperuntukkan.";
        return RedirectToAction(nameof(ReviewLoan), new { id });
    }

    var hasil = await allocation.AllocateAsync(id, assetId, remarks);

    if (!hasil.Berjaya)
    {
        // Kegagalan dijangka — cth. pentadbir lain baru sahaja mengambil aset ini.
        TempData["Ralat"] = hasil.Sebab;
        return RedirectToAction(nameof(ReviewLoan), new { id });
    }

    var app = await Db.Set<AssetLoanRequest>().AsNoTracking()
        .Include(r => r.Submission)
        .FirstOrDefaultAsync(r => r.Id == id);

    await notifications.NotifyAsync(app!.Submission!.ApplicantUserId,
        $"Permohonan pinjaman {app.Submission.ReferenceNo} diluluskan",
        $"Aset {hasil.AssetTag} telah diperuntukkan untuk anda. " +
        "Sila ambil di Unit Aset ICT dan akui penerimaan dalam sistem.");

    TempData["Mesej"] = $"Diluluskan. Aset {hasil.AssetTag} diperuntukkan.";
    return RedirectToAction(nameof(Queue));
}
```

Borang dalam view:

```cshtml
@if (Model.BolehDiputuskan)
{
    <form asp-action="ApproveLoan" method="post" class="mb-3">
        @Html.AntiForgeryToken()
        <input type="hidden" name="id" value="@Model.Application.Id" />

        <label class="form-label">Peruntukkan unit</label>
        <select name="assetId" class="form-select mb-2" required>
            <option value="">— Pilih unit @Model.Application.KategoriDipohon —</option>
            @foreach (var u in Model.UnitTersedia)
            {
                <option value="@u.Id">@u.AssetTag — @u.Jenama @u.Model</option>
            }
        </select>

        @if (!Model.UnitTersedia.Any())
        {
            <div class="alert alert-warning py-2 small">
                Tiada unit @Model.Application.KategoriDipohon tersedia.
                Permohonan ini perlu ditolak atau ditangguhkan.
            </div>
        }

        <textarea name="remarks" class="form-control mb-2" rows="2"
                  placeholder="Catatan (pilihan)"></textarea>

        <button type="submit" class="btn btn-success w-100"
                disabled="@(!Model.UnitTersedia.Any())">
            Luluskan &amp; Peruntukkan
        </button>
    </form>

    <form asp-action="Reject" method="post">
        @Html.AntiForgeryToken()
        <input type="hidden" name="id" value="@Model.Application.Submission!.Id" />
        <textarea name="remarks" class="form-control mb-2" rows="2"
                  placeholder="Sebab penolakan (WAJIB)" required></textarea>
        <button type="submit" class="btn btn-danger w-100">Tolak</button>
    </form>
}
```

> Penolakan **boleh** menggunakan `base.Reject` — ia tidak menyentuh inventori.

### ✅ Semakan

- [ ] Kelulusan memperuntukkan aset dan menukar statusnya
- [ ] Kegagalan (aset diambil) memberi mesej jelas, bukan pengecualian
- [ ] Butang dilumpuhkan bila tiada unit
- [ ] `Reject` menggunakan kelas asas
- [ ] Notifikasi memberitahu pemohon untuk mengambil dan mengakui

---

## Latihan 4 — Pemulangan dengan pemeriksaan kondisi

**Objektif:** Tutup kitaran; inventori kemas kini automatik.

### Langkah

1. Pemohon memulakan pemulangan:

```csharp
[HttpGet]
public async Task<IActionResult> CreateReturn(int loanId)
{
    var pinjaman = await Db.Set<AssetLoanRequest>().AsNoTracking()
        .Include(r => r.Submission).Include(r => r.Asset)
        .FirstOrDefaultAsync(r => r.Id == loanId);

    if (pinjaman is null) return NotFound();
    if (pinjaman.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();

    if (pinjaman.Submission.Status != SubmissionStatus.AdminApproved)
    {
        TempData["Ralat"] = "Hanya pinjaman aktif boleh dipulangkan.";
        return RedirectToAction("Index", "Aset");
    }

    // Satu pemulangan setiap pinjaman — indeks unik menguatkuasakannya.
    var sudahAda = await Db.Set<AssetReturn>()
        .AnyAsync(r => r.AssetLoanRequestId == loanId);
    if (sudahAda)
    {
        TempData["Ralat"] = "Pemulangan untuk pinjaman ini telah pun direkodkan.";
        return RedirectToAction("Index", "Aset");
    }

    return View(new AssetReturnFormViewModel
    {
        AssetLoanRequestId = loanId,
        AssetTag = pinjaman.Asset?.AssetTag,
        TarikhPinjam = pinjaman.TarikhPinjam,
        TarikhJangkaPulang = pinjaman.TarikhJangkaPulang,
        Lewat = pinjaman.TarikhJangkaPulang < DateTime.Today
    });
}
```

2. ICT memproses pemulangan:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> ProcessReturn(
    int id, KondisiPulangan kondisi, string? catatan)
{
    // Kondisi selain Baik memerlukan catatan.
    if (kondisi != KondisiPulangan.Baik && string.IsNullOrWhiteSpace(catatan))
    {
        TempData["Ralat"] =
            $"Catatan wajib diisi apabila kondisi ialah {kondisi}.";
        return RedirectToAction(nameof(ReviewReturn), new { id });
    }

    var hasil = await allocation.ReturnAsync(
        id, kondisi, catatan, currentUser.UserId!);

    if (!hasil.Berjaya)
    {
        TempData["Ralat"] = hasil.Sebab;
        return RedirectToAction(nameof(ReviewReturn), new { id });
    }

    var statusAset = kondisi switch
    {
        KondisiPulangan.Baik   => "kembali ke stok",
        KondisiPulangan.Rosak  => "dihantar untuk penyelenggaraan",
        KondisiPulangan.Hilang => "ditandakan HILANG",
        _ => "dikemas kini"
    };

    TempData["Mesej"] = $"Pemulangan diproses. Aset {hasil.AssetTag} {statusAset}.";
    return RedirectToAction(nameof(Queue));
}
```

3. Borang pemeriksaan kondisi:

```cshtml
<form asp-action="ProcessReturn" method="post">
    @Html.AntiForgeryToken()
    <input type="hidden" name="id" value="@Model.Id" />

    <label class="form-label">Kondisi aset semasa diterima</label>
    <div class="mb-3">
        <div class="form-check">
            <input class="form-check-input" type="radio" name="kondisi"
                   value="1" id="baik" checked />
            <label class="form-check-label" for="baik">
                <strong>Baik</strong> — aset kembali ke stok
            </label>
        </div>
        <div class="form-check">
            <input class="form-check-input" type="radio" name="kondisi"
                   value="2" id="rosak" />
            <label class="form-check-label" for="rosak">
                <strong>Rosak</strong> — aset dihantar untuk penyelenggaraan
            </label>
        </div>
        <div class="form-check">
            <input class="form-check-input" type="radio" name="kondisi"
                   value="3" id="hilang" />
            <label class="form-check-label" for="hilang">
                <strong>Hilang</strong> — aset ditandakan hilang dalam daftar
            </label>
        </div>
    </div>

    <label class="form-label">
        Catatan pemeriksaan
        <span class="text-muted small">(wajib jika Rosak atau Hilang)</span>
    </label>
    <textarea name="catatan" class="form-control mb-3" rows="3"
              placeholder="Cth: skrin retak di penjuru kiri bawah"></textarea>

    <button type="submit" class="btn btn-primary">Rekod Pemulangan</button>
</form>
```

### ✅ Semakan

- [ ] Hanya pemohon boleh memulakan pemulangan
- [ ] Satu pemulangan setiap pinjaman
- [ ] Kondisi `Baik` → aset `Available`
- [ ] Kondisi `Rosak` → aset `UnderMaintenance`
- [ ] Kondisi `Hilang` → aset `Lost`, **tidak dipadam**
- [ ] Catatan wajib untuk Rosak/Hilang
- [ ] Pinjaman asal ditandakan `Completed`

---

## Latihan 5 — Kelulusan perisian & kunci lesen

### Langkah

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "IctAdmin")]
public override async Task<IActionResult> Approve(int id, string? remarks)
{
    if (!User.IsInRole(AdminRole)) return Forbid();

    var app = await Db.Set<SoftwareRequest>()
        .Include(r => r.Submission).Include(r => r.SoftwareCatalogItem)
        .FirstOrDefaultAsync(r => r.Submission!.Id == id);

    if (app is null) return NotFound();

    // Semak semula lesen — masa telah berlalu sejak penghantaran.
    var lesen = await inventory.LicenceStatusAsync(app.SoftwareCatalogItemId);
    if (!lesen.Tersedia)
    {
        TempData["Ralat"] =
            $"Tiada lesen {lesen.Nama} yang tinggal ({lesen.Diguna}/{lesen.Jumlah}). " +
            "Permohonan tidak boleh diluluskan.";
        return RedirectToAction(nameof(ReviewSoftware), new { id = app.Id });
    }

    app.TarikhDiaktifkan = DateTime.UtcNow;
    await Db.SaveChangesAsync();

    // Perisian tidak memerlukan transaksi — tiada unit fizikal untuk diperuntukkan.
    // Kami BOLEH menggunakan base.Approve di sini, tidak seperti pinjaman aset.
    return await base.Approve(id,
        $"Lesen {app.SoftwareCatalogItem?.Nama} diaktifkan. {remarks}".Trim());
}

/// <summary>Rekod kunci lesen. Berasingan supaya ia tidak muncul dalam senarai.</summary>
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> RecordLicenceKey(int id, string kunciLesen)
{
    var app = await Db.Set<SoftwareRequest>()
        .Include(r => r.Submission)
        .FirstOrDefaultAsync(r => r.Id == id);

    if (app is null) return NotFound();

    app.KunciLesen = kunciLesen;
    await Db.SaveChangesAsync();

    // ⚠️ Audit merekod BAHAWA kunci direkod — bukan kuncinya.
    await auditLog.LogAsync(app.SubmissionId, "LicenceKeyRecorded",
        remarks: "Kunci lesen direkodkan oleh ICT.");

    TempData["Mesej"] = "Kunci lesen direkodkan.";
    return RedirectToAction(nameof(ReviewSoftware), new { id });
}
```

> **Perhatikan dua perkara:**
> 1. Perisian **boleh** menggunakan `base.Approve` — tiada transaksi diperlukan. Pinjaman aset tidak boleh. Fahami perbezaannya.
> 2. Kunci lesen **tidak** dilog dalam audit. Rekod fakta, bukan nilai — corak yang sama seperti kelayakan Kumpulan 3.

### ✅ Semakan

- [ ] Lesen disemak semula pada kelulusan
- [ ] Perisian menggunakan `base.Approve`; pinjaman tidak
- [ ] Kunci lesen direkod tetapi **tidak dilog**
- [ ] Kunci hanya dipaparkan pada halaman butiran pemohon sendiri

---

## Latihan 6 — Ujian, termasuk perlumbaan

Rekod dalam `docs/kumpulan-4/ujian-manual.md`:

| # | Ujian | Jangkaan | Keputusan |
|---|-------|----------|-----------|
| 1 | Applicant → `/Asset/Queue` | 403 | |
| 2 | HrAdmin → `/Asset/ReviewLoan/1` | 403 | |
| 3 | Luluskan pinjaman dengan unit | Aset → `OnLoan`, permohonan `AdminApproved` | |
| 4 | Semak katalog selepas #3 | Kiraan tersedia berkurang 1 | |
| 5 | Luluskan pinjaman lain dengan **unit sama** | Disekat — tidak tersedia | |
| 6 | **Dua pelayar, luluskan unit sama serentak** | Satu berjaya, satu gagal bersih | |
| 7 | Tolak pinjaman | Aset kekal `Available` | |
| 8 | Pulangkan kondisi Baik | Aset → `Available`, kiraan naik | |
| 9 | Pulangkan kondisi Rosak | Aset → `UnderMaintenance`, **tidak** dalam senarai tersedia | |
| 10 | Pulangkan kondisi Hilang | Aset → `Lost`, rekod **kekal** | |
| 11 | Pulangkan Rosak tanpa catatan | Ditolak | |
| 12 | Pemulangan dua kali untuk pinjaman sama | Disekat | |
| 13 | Luluskan perisian bila lesen habis | Ditolak | |
| 14 | Paksa pengecualian dalam `AllocateAsync` | **Rollback** — aset kekal `Available` | |
| 15 | Semak log — tiada kunci lesen | Tiada kunci dalam log | |

> **Ujian 6 dan 14 ialah yang paling penting.** Ujian 6 membuktikan semakan perlumbaan berfungsi; ujian 14 membuktikan transaksi berfungsi.
>
> Untuk ujian 14: tambah `throw new Exception("ujian");` sementara selepas `SaveChangesAsync` dalam `AllocateAsync`, jalankan, sahkan aset masih `Available`, kemudian buang.

### ✅ Semakan

- [ ] Kesemua 15 ujian dijalankan
- [ ] Ujian 6 — satu berjaya, satu gagal dengan mesej jelas
- [ ] Ujian 14 — rollback disahkan
- [ ] Ujian 10 — aset `Lost` kekal dalam pangkalan data

---

## Latihan 7 — Tutup blok

```bash
git diff --name-only master
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Peruntukan atomik dengan semakan perlumbaan
- [ ] Transaksi diuji dengan kegagalan paksa
- [ ] Kondisi pemulangan memetakan ke status aset yang betul
- [ ] Aset `Lost` tidak dipadam
- [ ] Perisian guna `base.Approve`; pinjaman tidak — dan anda boleh terangkan kenapa
- [ ] Kunci lesen tidak dilog
- [ ] Hanya fail Kumpulan 4 disentuh
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 7–9

| Artifak | Lokasi |
|---------|--------|
| `IAssetAllocationService` (transaksi) | `Services/Aset/` |
| Baris gilir & skrin semakan ICT | `Controllers/`, `Views/Aset/` |
| Kelulusan dengan peruntukan | `AssetController.ApproveLoan` |
| Pemulangan + pemeriksaan kondisi | `AssetController.ProcessReturn` |
| Kelulusan perisian + kunci lesen | `SoftwareController` |
| Ujian manual termasuk perlumbaan | `docs/kumpulan-4/ujian-manual.md` |

**Seterusnya (Hari 10–12):** peringatan lewat tempoh, papan pemuka inventori, dan **eksport PDF/Excel**.
