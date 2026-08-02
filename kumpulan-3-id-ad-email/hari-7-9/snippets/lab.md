# Lab · Kumpulan 3 · Hari 7–9 — Pemprosesan ICT, RBAC & Simulasi AD

> Konsep: [`../README.md`](../README.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok

```bash
git switch kump-3/id-ad-email
git pull --rebase origin master
git switch -c kump-3/feat/pemprosesan-ict
dotnet build
```

**Semak keputusan isu `shared` Hari 5–6** — adakah jurulatih mengubah `SubmissionControllerBase`? Jika ya, tarik dan laraskan `Reject` anda.

### ✅ Semakan

- [ ] Isu `shared` daripada Hari 5–6 diselesaikan
- [ ] `dotnet build` berjaya
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Servis simulasi AD

**Objektif:** Sempadan integrasi yang direka betul — walaupun bahagian jauhnya palsu.

### Langkah

1. `Services/Akaun/IAdProvisioningService.cs`:

```csharp
namespace Nres.Onboarding.Web.Services.Akaun;

public record AdProvisionResult(
    bool Berjaya, string? AccountName, string? Email, string? Mesej);

/// <summary>
/// Sempadan integrasi Active Directory.
///
/// Pelaksanaan dalam kursus ini ialah SIMULASI — makmal latihan tiada AD,
/// dan mencipta akaun sebenar memerlukan kelayakan istimewa yang tidak
/// sepatutnya berada dalam kelas.
///
/// Antara muka direka supaya pelaksanaan sebenar (System.DirectoryServices
/// atau Microsoft Graph) boleh menggantikannya TANPA mengubah controller.
/// Itu tujuan sebenar latihan ini.
/// </summary>
public interface IAdProvisioningService
{
    /// <summary>Cadangkan nama akaun daripada nama penuh staf.</summary>
    string SuggestAccountName(string fullName);

    /// <summary>Adakah nama akaun ini masih tersedia?</summary>
    Task<bool> IsAccountNameAvailableAsync(string accountName,
        CancellationToken ct = default);

    /// <summary>
    /// "Cipta" akaun. Simulasi — log apa yang AKAN dihantar ke AD sebenar.
    /// </summary>
    Task<AdProvisionResult> ProvisionAsync(string accountName, string fullName,
        string department, CancellationToken ct = default);
}
```

2. `Services/Akaun/SimulatedAdProvisioningService.cs`:

```csharp
using System.Globalization;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Akaun;

namespace Nres.Onboarding.Web.Services.Akaun;

public class SimulatedAdProvisioningService(
    ApplicationDbContext db,
    ILogger<SimulatedAdProvisioningService> logger) : IAdProvisioningService
{
    private const string Domain = "nres.gov.my";

    /// <summary>
    /// "Ahmad bin Zulkifli" → "ahmad.zulkifli"
    /// Membuang gelaran Melayu (bin/binti/a/l/a/p) dan tanda diakritik.
    /// </summary>
    public string SuggestAccountName(string fullName)
    {
        string[] gelaran = ["bin", "binti", "bt", "a/l", "a/p", "al", "dr", "haji", "hajjah"];

        var bahagian = fullName
            .Split([' ', '.'], StringSplitOptions.RemoveEmptyEntries)
            .Select(BuangDiakritik)
            .Select(b => b.ToLowerInvariant())
            .Where(b => !gelaran.Contains(b))
            .Where(b => b.Length > 0)
            .ToList();

        if (bahagian.Count == 0) return "pengguna";
        if (bahagian.Count == 1) return Bersihkan(bahagian[0]);

        // Nama pertama + nama terakhir
        return Bersihkan($"{bahagian[0]}.{bahagian[^1]}");

        static string Bersihkan(string s) =>
            new(s.Where(c => char.IsLetterOrDigit(c) || c == '.').ToArray());

        static string BuangDiakritik(string s)
        {
            var normal = s.Normalize(NormalizationForm.FormD);
            var sb = new StringBuilder();
            foreach (var c in normal)
            {
                if (CharUnicodeInfo.GetUnicodeCategory(c) != UnicodeCategory.NonSpacingMark)
                    sb.Append(c);
            }
            return sb.ToString().Normalize(NormalizationForm.FormC);
        }
    }

    public async Task<bool> IsAccountNameAvailableAsync(string accountName,
        CancellationToken ct = default)
    {
        // Dalam sistem sebenar ini bertanya kepada AD. Di sini kami menyemak
        // rekod kami sendiri — indeks unik pada AdAccountName menjadikannya
        // sumber kebenaran untuk simulasi.
        return !await db.Set<AccountRequest>()
            .AnyAsync(a => a.AdAccountName == accountName, ct);
    }

    public async Task<AdProvisionResult> ProvisionAsync(string accountName,
        string fullName, string department, CancellationToken ct = default)
    {
        if (!await IsAccountNameAvailableAsync(accountName, ct))
        {
            return new AdProvisionResult(false, null, null,
                $"Nama akaun '{accountName}' telah digunakan.");
        }

        // ⚠️ SIMULASI. Pelaksanaan sebenar akan memanggil AD di sini.
        // Perhatikan apa yang TIDAK ada dalam log: kata laluan. Ia ditetapkan
        // dalam AD oleh pentadbir, tidak pernah melalui sistem ini.
        logger.LogInformation(
            "[SIMULASI AD] Akan cipta: sAMAccountName={Account}, " +
            "displayName={Nama}, department={Jabatan}, mail={Email}",
            accountName, fullName, department, $"{accountName}@{Domain}");

        await Task.Delay(300, ct);   // simulasi kependaman rangkaian

        return new AdProvisionResult(
            Berjaya: true,
            AccountName: accountName,
            Email: $"{accountName}@{Domain}",
            Mesej: "Akaun dicipta (simulasi).");
    }
}
```

3. Daftar dalam `AkaunModule`:

```csharp
services.AddScoped<IAdProvisioningService, SimulatedAdProvisioningService>();
```

4. **Uji penjanaan nama** dengan input Melayu sebenar:

| Input | Jangkaan |
|-------|----------|
| `Ahmad bin Zulkifli` | `ahmad.zulkifli` |
| `Siti Nurhaliza binti Osman` | `siti.osman` |
| `Muthu a/l Ramasamy` | `muthu.ramasamy` |
| `Chan Wei Ming` | `chan.ming` |
| `Nur Aisyah` | `nur.aisyah` |

### ✅ Semakan

- [ ] Antara muka boleh diganti dengan pelaksanaan AD sebenar
- [ ] Gelaran Melayu dibuang dengan betul
- [ ] **Tiada kata laluan** dalam log atau hasil
- [ ] Kelas ditanda jelas sebagai simulasi

---

## Latihan 2 — Baris gilir ICT

**Objektif:** ICT melihat apa yang lulus penyelia.

### Langkah

```csharp
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> IctQueue(SubmissionStatus? status)
{
    // Lalai: apa yang menunggu kami.
    var tapis = status ?? SubmissionStatus.SupervisorApproved;

    var senarai = await (
        from a in Db.Set<AccountRequest>().AsNoTracking()
        join s in Db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
        join p in Db.UserProfiles.AsNoTracking() on s.ApplicantUserId equals p.UserId
        where s.ModuleCode == ModuleCode && s.Status == tapis
        orderby s.SubmittedAt
        select new IctQueueItem(
            a.Id, s.Id, s.ReferenceNo, a.Jenis, a.StaffName,
            p.FullName, s.SubmittedAt, a.AccessRequests.Count,
            a.AdAccountName))
        .ToListAsync();

    ViewBag.StatusTapis = tapis;
    return View(senarai);
}
```

### ✅ Semakan

- [ ] Baris gilir menunjukkan `SupervisorApproved` secara lalai
- [ ] Boleh menapis mengikut status lain
- [ ] Paling lama menunggu dahulu
- [ ] `[Authorize(Roles = "IctAdmin")]`

---

## Latihan 3 — Skrin pemprosesan ICT

**Objektif:** Satu skrin di mana ICT membuat semua keputusan.

### Langkah

1. `ViewModels/Akaun/IctProcessViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.Models.Akaun;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Akaun;

public class AksesKeputusanViewModel
{
    public int RequestedAccessId { get; set; }
    public string Nama { get; set; } = string.Empty;
    public TahapAkses Tahap { get; set; }
    public string? JustifikasiPemohon { get; set; }
    public bool PerluJustifikasi { get; set; }

    // --- Keputusan ICT ---
    /// <summary>null = belum diputuskan.</summary>
    public bool? Diluluskan { get; set; }

    [StringLength(500)]
    public string? CatatanIct { get; set; }
}

public class IctProcessViewModel
{
    public int ApplicationId { get; set; }
    public int SubmissionId { get; set; }
    public string ReferenceNo { get; set; } = string.Empty;
    public JenisPermohonanAkaun Jenis { get; set; }
    public SubmissionStatus Status { get; set; }

    public string StaffName { get; set; } = string.Empty;
    public string StaffIdentityNo { get; set; } = string.Empty;
    public string? DepartmentName { get; set; }
    public string? PositionName { get; set; }
    public string Justifikasi { get; set; } = string.Empty;
    public string? SupervisorName { get; set; }
    public string? SupervisorRemarks { get; set; }

    [Display(Name = "Nama akaun AD")]
    [StringLength(100)]
    public string? AdAccountName { get; set; }

    [Display(Name = "E-mel rasmi")]
    [EmailAddress(ErrorMessage = "Format e-mel tidak sah.")]
    [StringLength(200)]
    public string? OfficialEmail { get; set; }

    [Display(Name = "Kelayakan telah diserahkan kepada staf")]
    public bool KelayakanDiserahkan { get; set; }

    [Display(Name = "Catatan ICT")]
    [StringLength(1000)]
    public string? CatatanIct { get; set; }

    public List<AksesKeputusanViewModel> Akses { get; set; } = [];

    public bool BolehDiproses { get; set; }
    public IReadOnlyList<AuditLog> AuditLogs { get; set; } = [];

    /// <summary>Cadangan daripada IAdProvisioningService.</summary>
    public string? CadanganNamaAkaun { get; set; }
}
```

2. Action `Process` (GET):

```csharp
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> Process(int id)
{
    var app = await Db.Set<AccountRequest>()
        .AsNoTracking()
        .Include(a => a.Submission)
        .Include(a => a.Department)
        .Include(a => a.Position)
        .Include(a => a.AccessRequests).ThenInclude(r => r.SystemAccess)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (app is null) return NotFound();

    var penyelia = await Db.UserProfiles.AsNoTracking()
        .Where(p => p.UserId == app.SupervisorUserId)
        .Select(p => p.FullName).FirstOrDefaultAsync();

    var langkahPenyelia = await Db.ApprovalSteps.AsNoTracking()
        .Where(s => s.SubmissionId == app.SubmissionId && s.StepOrder == 1)
        .FirstOrDefaultAsync();

    var vm = new IctProcessViewModel
    {
        ApplicationId = app.Id,
        SubmissionId = app.SubmissionId,
        ReferenceNo = app.Submission!.ReferenceNo,
        Jenis = app.Jenis,
        Status = app.Submission.Status,
        StaffName = app.StaffName,
        StaffIdentityNo = app.StaffIdentityNo,
        DepartmentName = app.Department?.Name,
        PositionName = app.Position?.Name,
        Justifikasi = app.Justifikasi,
        SupervisorName = penyelia,
        SupervisorRemarks = langkahPenyelia?.Remarks,
        AdAccountName = app.AdAccountName,
        OfficialEmail = app.OfficialEmail,
        KelayakanDiserahkan = app.KelayakanDiserahkan,
        CatatanIct = app.CatatanIct,
        BolehDiproses = app.Submission.Status == SubmissionStatus.SupervisorApproved,
        CadanganNamaAkaun = adProvisioning.SuggestAccountName(app.StaffName),
        Akses = app.AccessRequests.Select(r => new AksesKeputusanViewModel
        {
            RequestedAccessId = r.Id,
            Nama = r.SystemAccess?.Name ?? "—",
            Tahap = r.Tahap,
            JustifikasiPemohon = r.Justifikasi,
            PerluJustifikasi = r.SystemAccess?.PerluJustifikasi ?? false,
            Diluluskan = r.Diluluskan,
            CatatanIct = r.CatatanIct
        }).ToList(),
        AuditLogs = await Db.AuditLogs.AsNoTracking()
            .Where(l => l.SubmissionId == app.SubmissionId)
            .OrderByDescending(l => l.CreatedAt)
            .ToListAsync()
    };

    return View(vm);
}
```

### ✅ Semakan

- [ ] Skrin memaparkan staf, penyelia, catatan penyelia
- [ ] Setiap akses dengan justifikasi pemohon
- [ ] Cadangan nama akaun dipaparkan
- [ ] Guna `_AuditTrail` **kongsi**

---

## Latihan 4 — Pemprosesan: kelulusan separa + rekod AD

**Objektif:** Keputusan setiap akses, dalam satu transaksi.

### Langkah

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> Process(IctProcessViewModel vm)
{
    var app = await Db.Set<AccountRequest>()
        .Include(a => a.Submission)
        .Include(a => a.Department)
        .Include(a => a.AccessRequests)
        .FirstOrDefaultAsync(a => a.Id == vm.ApplicationId);

    if (app is null) return NotFound();

    if (app.Submission!.Status != SubmissionStatus.SupervisorApproved)
    {
        TempData["Ralat"] = "Permohonan ini bukan menunggu pemprosesan ICT.";
        return RedirectToAction(nameof(Process), new { id = app.Id });
    }

    // --- Setiap akses mesti diputuskan ---
    if (vm.Akses.Any(a => a.Diluluskan is null))
    {
        ModelState.AddModelError(string.Empty,
            "Setiap akses mesti diluluskan atau ditolak sebelum permohonan diproses.");
        await IsiSemulaProcessAsync(vm, app);
        return View(vm);
    }

    // --- Akses yang ditolak memerlukan sebab ---
    foreach (var a in vm.Akses.Where(a => a.Diluluskan == false
                                       && string.IsNullOrWhiteSpace(a.CatatanIct)))
    {
        ModelState.AddModelError(string.Empty,
            $"Sebab wajib diisi untuk akses yang ditolak: {a.Nama}");
    }

    var adaDiluluskan = vm.Akses.Any(a => a.Diluluskan == true);

    // --- Jika ada akses AD diluluskan, nama akaun wajib ---
    if (adaDiluluskan && string.IsNullOrWhiteSpace(vm.AdAccountName))
    {
        ModelState.AddModelError(nameof(vm.AdAccountName),
            "Nama akaun AD wajib diisi apabila akses diluluskan.");
    }

    if (!ModelState.IsValid)
    {
        await IsiSemulaProcessAsync(vm, app);
        return View(vm);
    }

    // --- Semua atau tiada: transaksi ---
    // Keputusan akses, rekod akaun, peralihan status, dan langkah kelulusan
    // mesti KESEMUANYA berjaya atau tiada langsung.
    await using var transaksi = await Db.Database.BeginTransactionAsync();
    try
    {
        // 1. Rekod keputusan setiap akses
        foreach (var keputusan in vm.Akses)
        {
            var baris = app.AccessRequests
                .FirstOrDefault(r => r.Id == keputusan.RequestedAccessId);
            if (baris is null) continue;

            baris.Diluluskan = keputusan.Diluluskan;
            baris.CatatanIct = keputusan.CatatanIct;
        }

        // 2. Sediakan akaun AD (simulasi) jika ada akses diluluskan
        if (adaDiluluskan)
        {
            var hasil = await adProvisioning.ProvisionAsync(
                vm.AdAccountName!, app.StaffName, app.Department?.Name ?? "—");

            if (!hasil.Berjaya)
            {
                ModelState.AddModelError(nameof(vm.AdAccountName), hasil.Mesej ?? "Gagal.");
                await transaksi.RollbackAsync();
                await IsiSemulaProcessAsync(vm, app);
                return View(vm);
            }

            app.AdAccountName = hasil.AccountName;
            app.OfficialEmail = vm.OfficialEmail ?? hasil.Email;
        }

        app.KelayakanDiserahkan = vm.KelayakanDiserahkan;
        app.TarikhSerahan = vm.KelayakanDiserahkan ? DateTime.UtcNow : null;
        app.CatatanIct = vm.CatatanIct;

        await Db.SaveChangesAsync();

        // 3. Rekod keputusan langkah 2
        await approvalRoute.DecideAsync(app.SubmissionId, stepOrder: 2,
            adaDiluluskan ? ApprovalDecision.Approved : ApprovalDecision.Rejected,
            currentUser.UserId!, vm.CatatanIct);

        // 4. Peralihan status
        //    Semua akses ditolak → Rejected. Sekurang-kurangnya satu → AdminApproved.
        var statusBaharu = adaDiluluskan
            ? SubmissionStatus.AdminApproved
            : SubmissionStatus.Rejected;

        var ditolak = vm.Akses.Count(a => a.Diluluskan == false);
        var diluluskan = vm.Akses.Count(a => a.Diluluskan == true);

        await Workflow.TransitionAsync(app.Submission, statusBaharu,
            adaDiluluskan ? "Approved" : "Rejected",
            $"{diluluskan} akses diluluskan, {ditolak} ditolak. " +
            (app.AdAccountName is not null ? $"Akaun: {app.AdAccountName}." : ""));

        await transaksi.CommitAsync();
    }
    catch
    {
        await transaksi.RollbackAsync();
        throw;
    }

    // --- Notifikasi jujur: senaraikan apa yang ditolak ---
    var mesejAkses = string.Join("\n", vm.Akses.Select(a =>
        $"  {(a.Diluluskan == true ? "✅" : "❌")} {a.Nama}" +
        (a.Diluluskan == false ? $" — {a.CatatanIct}" : "")));

    await notifications.NotifyAsync(app.Submission.ApplicantUserId,
        $"Permohonan akaun {app.Submission.ReferenceNo} telah diproses",
        $"Keputusan bagi setiap akses:\n{mesejAkses}");

    TempData["Mesej"] = $"Permohonan diproses. {diluluskanKiraan(vm)} akses diluluskan.";
    return RedirectToAction(nameof(IctQueue));

    static int diluluskanKiraan(IctProcessViewModel v) =>
        v.Akses.Count(a => a.Diluluskan == true);
}
```

> **Kenapa transaksi?** Empat perkara mesti berjaya bersama: keputusan akses, rekod akaun AD, keputusan langkah kelulusan, peralihan status. Jika penyediaan AD gagal selepas kita menyimpan keputusan akses, permohonan berada dalam keadaan tidak konsisten — akses ditandakan diluluskan, tetapi tiada akaun.
>
> Kumpulan 4 menghadapi masalah yang sama dengan inventori aset. Bandingkan pendekatan semasa semakan silang AI.

### ✅ Semakan

- [ ] Setiap akses mesti diputuskan sebelum pemprosesan
- [ ] Akses ditolak memerlukan sebab
- [ ] Nama akaun AD wajib bila ada akses diluluskan
- [ ] **Semua akses ditolak** → status `Rejected`
- [ ] Sekurang-kurangnya satu diluluskan → `AdminApproved`
- [ ] Keseluruhan operasi dalam transaksi
- [ ] Notifikasi menyenaraikan setiap akses dan sebab penolakan

---

## Latihan 5 — Borang keputusan akses

**Objektif:** UI untuk keputusan setiap akses.

### Langkah

`Views/Akaun/Process.cshtml` — bahagian akses:

```cshtml
<h5 class="mt-4">Keputusan Akses</h5>
<table class="table align-middle">
    <thead>
        <tr>
            <th>Akses</th><th>Tahap</th><th>Justifikasi pemohon</th>
            <th style="width:180px">Keputusan</th><th>Sebab (jika ditolak)</th>
        </tr>
    </thead>
    <tbody>
    @for (var i = 0; i < Model.Akses.Count; i++)
    {
        <tr>
            <td>
                <input type="hidden" asp-for="Akses[i].RequestedAccessId" />
                <input type="hidden" asp-for="Akses[i].Nama" />
                @Model.Akses[i].Nama
                @if (Model.Akses[i].PerluJustifikasi)
                {
                    <span class="badge bg-warning text-dark">Sensitif</span>
                }
            </td>
            <td>@Model.Akses[i].Tahap</td>
            <td class="small">@(Model.Akses[i].JustifikasiPemohon ?? "—")</td>
            <td>
                <div class="btn-group btn-group-sm w-100" role="group">
                    <input type="radio" class="btn-check"
                           name="Akses[@i].Diluluskan" id="lulus@(i)" value="true"
                           checked="@(Model.Akses[i].Diluluskan == true)" />
                    <label class="btn btn-outline-success" for="lulus@(i)">Lulus</label>

                    <input type="radio" class="btn-check"
                           name="Akses[@i].Diluluskan" id="tolak@(i)" value="false"
                           checked="@(Model.Akses[i].Diluluskan == false)" />
                    <label class="btn btn-outline-danger" for="tolak@(i)">Tolak</label>
                </div>
            </td>
            <td>
                <input asp-for="Akses[i].CatatanIct" class="form-control form-control-sm"
                       placeholder="Sebab penolakan" />
            </td>
        </tr>
    }
    </tbody>
</table>

<h5 class="mt-4">Butiran Akaun</h5>
<div class="row g-3">
    <div class="col-md-4">
        <label asp-for="AdAccountName" class="form-label"></label>
        <div class="input-group">
            <input asp-for="AdAccountName" class="form-control" id="namaAkaun" />
            <button type="button" class="btn btn-outline-secondary" id="gunaCadangan">
                Guna cadangan
            </button>
        </div>
        <div class="form-text">Cadangan: <code>@Model.CadanganNamaAkaun</code></div>
        <span asp-validation-for="AdAccountName" class="text-danger"></span>
    </div>
    <div class="col-md-4">
        <label asp-for="OfficialEmail" class="form-label"></label>
        <input asp-for="OfficialEmail" class="form-control" />
        <span asp-validation-for="OfficialEmail" class="text-danger"></span>
    </div>
    <div class="col-md-4">
        <label asp-for="CatatanIct" class="form-label"></label>
        <textarea asp-for="CatatanIct" class="form-control" rows="2"></textarea>
    </div>
</div>

<div class="alert alert-warning mt-3">
    <strong>🔒 Peringatan keselamatan:</strong> jangan sekali-kali merekod kata laluan
    dalam sistem ini. Tetapkan kata laluan dalam Active Directory dan serahkan
    kepada staf melalui saluran selamat. Tandakan kotak di bawah hanya selepas
    penyerahan selesai.
</div>

<div class="form-check">
    <input asp-for="KelayakanDiserahkan" class="form-check-input" />
    <label asp-for="KelayakanDiserahkan" class="form-check-label"></label>
</div>

@section Scripts {
    <partial name="_ValidationScriptsPartial" />
    <script>
        document.getElementById('gunaCadangan')?.addEventListener('click', () => {
            document.getElementById('namaAkaun').value = '@Model.CadanganNamaAkaun';
        });
    </script>
}
```

> **Perhatikan `asp-for="Akses[i].…`** dengan gelung `for` — bukan `foreach`. Tag helper menjana nama berindeks yang betul secara automatik apabila anda menggunakan pengindeks. Ini lebih bersih daripada nama manual yang anda gunakan pada Hari 5–6.

### ✅ Semakan

- [ ] Butang radio lulus/tolak setiap akses
- [ ] Butang "guna cadangan" mengisi nama akaun
- [ ] Amaran keselamatan kelihatan
- [ ] Menghantar dengan akses belum diputuskan **ditolak**

---

## Latihan 6 — Matriks RBAC merentas modul

**Objektif:** Tugas anda untuk seluruh sistem — sahkan RBAC berfungsi di mana-mana.

> Ini sumbangan silang. Anda menyemak modul **kumpulan lain** dan melaporkan penemuan.

### Langkah

1. Bina matriks. Log masuk sebagai setiap peranan dan cuba setiap skrin admin:

```markdown
# Matriks RBAC — disemak Kumpulan 3, Hari 9

Legenda: ✅ = akses dibenarkan (betul) · ❌ = 403 (betul) · ⚠️ = SALAH

| Peranan | K1 `/OfficerReporting/Dashboard` | K2 `/Akses/Queue` | K3 `/AccountRequest/IctQueue` | K4 `/Asset/Queue` |
|---------|----------------------------------|-------------------|-------------------------------|-------------------|
| Applicant     | | | | |
| Supervisor    | | | | |
| HrAdmin       | | | | |
| SecurityAdmin | | | | |
| IctAdmin      | | | | |
| SystemAdmin   | | | | |

## Semakan tambahan setiap modul

| # | Ujian | Jangkaan | Keputusan |
|---|-------|----------|-----------|
| 1 | K1: Pemohon A buka permohonan pemohon B | 403 | |
| 2 | K1: muat turun lampiran orang lain | 403 | |
| 3 | K2: skrin ronda `/Akses/Semak` sebagai Applicant | 403 | |
| 4 | K3: Penyelia lain luluskan permohonan bukan miliknya | 403 | |
| 5 | K3: IctAdmin proses sebelum penyelia lulus | Gagal | |
| 6 | K4: pinjam aset untuk orang lain | 403 atau disekat | |

## Penemuan
- <senarai isu, dengan kumpulan pemilik>
```

2. **Laporkan penemuan kepada kumpulan pemilik** secara bertulis. Buka isu untuk setiap ⚠️, tetapkan kepada kumpulan mereka.

3. Bawa ringkasan ke stand-up berikutnya.

> **Ini bukan mencari kesalahan.** Setiap kumpulan menyemak modulnya sendiri; pasangan mata kedua menemui apa yang mata pertama terlepas. Anda pasukan akses — ini domain anda.

### ✅ Semakan

- [ ] Matriks penuh 6 peranan × 4 modul diisi
- [ ] Enam semakan tambahan dijalankan
- [ ] Setiap ⚠️ dilaporkan sebagai isu kepada kumpulan pemilik
- [ ] Ringkasan dibawa ke stand-up

---

## Latihan 7 — Ujian modul anda

Rekod dalam `docs/kumpulan-3/ujian-manual.md`:

| # | Ujian | Jangkaan | Keputusan |
|---|-------|----------|-----------|
| 1 | Applicant → `/AccountRequest/IctQueue` | 403 | |
| 2 | Supervisor → `/AccountRequest/IctQueue` | 403 | |
| 3 | HrAdmin → `/AccountRequest/Process/1` | 403 | |
| 4 | IctAdmin proses sebelum penyelia lulus | Gagal — turutan | |
| 5 | Proses dengan akses belum diputuskan | Ditolak | |
| 6 | Tolak akses tanpa sebab | Ditolak | |
| 7 | Luluskan 3, tolak 2 | Status `AdminApproved`; notifikasi senaraikan kedua-dua | |
| 8 | Tolak **semua** akses | Status `Rejected` | |
| 9 | Nama akaun AD pendua | Ditolak — nama telah digunakan | |
| 10 | Cadangan nama: `Ahmad bin Zulkifli` | `ahmad.zulkifli` | |
| 11 | Cadangan nama: `Muthu a/l Ramasamy` | `muthu.ramasamy` | |
| 12 | Penyediaan AD gagal separuh jalan | Transaksi rollback — tiada perubahan separa | |
| 13 | Semak log — tiada kata laluan | Tiada kelayakan dalam log | |

> **Ujian 12** memerlukan anda memaksa kegagalan sementara (cth. lontar dalam `ProvisionAsync`). Lakukannya, sahkan rollback, kemudian buang.

### ✅ Semakan

- [ ] Kesemua 13 ujian dijalankan
- [ ] Ujian 12 mengesahkan transaksi berfungsi
- [ ] Ujian 13 mengesahkan tiada kelayakan dilog

---

## Latihan 8 — Tutup blok

```bash
git diff --name-only master
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Baris gilir & skrin pemprosesan ICT berfungsi
- [ ] Kelulusan **separa** berfungsi dengan status yang betul
- [ ] Rekod akaun AD/e-mel; **tiada kata laluan di mana-mana**
- [ ] Simulasi AD ditanda jelas, antara muka boleh diganti
- [ ] Transaksi melindungi operasi berbilang langkah
- [ ] **Matriks RBAC merentas modul selesai dan dilaporkan**
- [ ] Hanya fail Kumpulan 3 disentuh
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 7–9

| Artifak | Lokasi |
|---------|--------|
| `IAdProvisioningService` + simulasi | `Services/Akaun/` |
| Baris gilir & skrin pemprosesan ICT | `Controllers/`, `Views/Akaun/` |
| Kelulusan separa dalam transaksi | `AccountRequestController.Process` |
| **Matriks RBAC merentas modul** | `docs/kumpulan-3/matriks-rbac.md` |
| Ujian manual | `docs/kumpulan-3/ujian-manual.md` |

**Seterusnya (Hari 10–12):** penjejakan status, **audit trail penuh**, carian/penapis, dan papan pemuka ICT.
