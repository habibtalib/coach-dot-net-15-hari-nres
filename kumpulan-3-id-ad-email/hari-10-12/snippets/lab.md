# Lab · Kumpulan 3 · Hari 10–12 — Penjejakan, Audit & Dashboard ICT

> Konsep: [`../README.md`](../README.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok

```bash
git switch kump-3/id-ad-email
git pull --rebase origin master
git switch -c kump-3/feat/penjejakan-audit-dashboard
dotnet build
```

**Semakan "sudah wujud?"**

```bash
grep -rn "_AuditTrail" Nres.Onboarding.Web/Views/Shared/
grep -n "LogAsync" Nres.Onboarding.Web/Services/IAuditLogService.cs
```

Kedua-duanya wujud. Anda **memanggil** servis kongsi dengan catatan yang lebih kaya — bukan menulis sistem audit sendiri.

### ✅ Semakan

- [ ] `_AuditTrail` dan `IAuditLogService` disahkan wujud
- [ ] Anda pada cabang ciri

---

## Latihan 1 — Garis masa penjejakan

**Objektif:** Pemohon melihat di mana permohonan berada dan berapa lama.

### Langkah

1. `ViewModels/Akaun/TrackingViewModel.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Akaun;

public class TrackingViewModel
{
    public string ReferenceNo { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; }
    public IReadOnlyList<LangkahGarisMasa> Langkah { get; set; } = [];

    /// <summary>Berapa hari permohonan telah berada di peringkat semasa.</summary>
    public int? HariDiPeringkatSemasa { get; set; }

    public record LangkahGarisMasa(
        int Urutan,
        string Tajuk,
        string? Pelaku,
        DateTime? Tarikh,
        string Keadaan,          // "selesai" | "semasa" | "menunggu" | "ditolak"
        string? Catatan);
}
```

2. Bina garis masa dalam servis — `Services/Akaun/ITrackingService.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Akaun;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.ViewModels.Akaun;

namespace Nres.Onboarding.Web.Services.Akaun;

public interface ITrackingService
{
    Task<TrackingViewModel?> BuildAsync(int applicationId, CancellationToken ct = default);
}

public class TrackingService(ApplicationDbContext db) : ITrackingService
{
    public async Task<TrackingViewModel?> BuildAsync(
        int applicationId, CancellationToken ct = default)
    {
        var app = await db.Set<AccountRequest>().AsNoTracking()
            .Include(a => a.Submission)
            .FirstOrDefaultAsync(a => a.Id == applicationId, ct);

        if (app is null) return null;

        var langkah = await db.ApprovalSteps.AsNoTracking()
            .Where(s => s.SubmissionId == app.SubmissionId)
            .OrderBy(s => s.StepOrder)
            .ToListAsync(ct);

        // Nama pelaku — satu query, bukan satu per langkah (elak N+1).
        var pelakuIds = langkah
            .Where(s => s.DecidedByUserId != null)
            .Select(s => s.DecidedByUserId!)
            .Append(app.SupervisorUserId)
            .Distinct().ToList();

        var namaPelaku = await db.UserProfiles.AsNoTracking()
            .Where(p => pelakuIds.Contains(p.UserId))
            .ToDictionaryAsync(p => p.UserId, p => p.FullName, ct);

        var s0 = app.Submission!;
        var garisMasa = new List<TrackingViewModel.LangkahGarisMasa>
        {
            new(0, "Permohonan dihantar",
                namaPelaku.GetValueOrDefault(s0.ApplicantUserId),
                s0.SubmittedAt,
                s0.SubmittedAt is null ? "menunggu" : "selesai",
                null)
        };

        foreach (var st in langkah)
        {
            var tajuk = st.StepOrder switch
            {
                1 => "Kelulusan Penyelia Jabatan",
                2 => "Pemprosesan Pentadbir ICT",
                _ => $"Langkah {st.StepOrder}"
            };

            var keadaan = st.Decision switch
            {
                ApprovalDecision.Approved => "selesai",
                ApprovalDecision.Rejected => "ditolak",
                _ when garisMasa.All(g => g.Keadaan is "selesai") => "semasa",
                _ => "menunggu"
            };

            garisMasa.Add(new(st.StepOrder, tajuk,
                st.DecidedByUserId is null
                    ? namaPelaku.GetValueOrDefault(app.SupervisorUserId)
                    : namaPelaku.GetValueOrDefault(st.DecidedByUserId),
                st.DecidedAt, keadaan, st.Remarks));
        }

        // Berapa lama di peringkat semasa?
        var mulaPeringkat = langkah
            .Where(x => x.Decision != ApprovalDecision.Pending)
            .OrderByDescending(x => x.StepOrder)
            .Select(x => x.DecidedAt)
            .FirstOrDefault() ?? s0.SubmittedAt;

        int? hari = mulaPeringkat is null || s0.Status is SubmissionStatus.Completed
                        or SubmissionStatus.Rejected or SubmissionStatus.Cancelled
            ? null
            : (int)(DateTime.UtcNow - mulaPeringkat.Value).TotalDays;

        return new TrackingViewModel
        {
            ReferenceNo = s0.ReferenceNo,
            Status = s0.Status,
            Langkah = garisMasa,
            HariDiPeringkatSemasa = hari
        };
    }
}
```

3. Partial view `Views/Akaun/_GarisMasa.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Akaun.TrackingViewModel

<div class="card">
    <div class="card-header d-flex justify-content-between align-items-center">
        <span>Penjejakan Permohonan</span>
        @if (Model.HariDiPeringkatSemasa is int hari)
        {
            <span class="badge @(hari > 7 ? "bg-danger" : "bg-secondary")">
                @hari hari di peringkat ini
            </span>
        }
    </div>
    <div class="card-body">
        <ul class="list-unstyled mb-0">
        @foreach (var l in Model.Langkah)
        {
            var (ikon, warna) = l.Keadaan switch
            {
                "selesai" => ("✓", "text-success"),
                "semasa"  => ("●", "text-primary"),
                "ditolak" => ("✕", "text-danger"),
                _         => ("○", "text-muted")
            };

            <li class="d-flex mb-3">
                <div class="me-3 fs-4 @warna">@ikon</div>
                <div>
                    <div class="fw-semibold @(l.Keadaan == "menunggu" ? "text-muted" : "")">
                        @l.Tajuk
                    </div>
                    @if (l.Pelaku is not null)
                    {
                        <div class="small text-muted">
                            @(l.Keadaan is "selesai" or "ditolak" ? "Oleh" : "Menunggu"): @l.Pelaku
                        </div>
                    }
                    @if (l.Tarikh is not null)
                    {
                        <div class="small text-muted">
                            @l.Tarikh.Value.ToLocalTime().ToString("dd/MM/yyyy HH:mm")
                        </div>
                    }
                    @if (!string.IsNullOrWhiteSpace(l.Catatan))
                    {
                        <div class="small mt-1 fst-italic">"@l.Catatan"</div>
                    }
                </div>
            </li>
        }
        </ul>
    </div>
</div>
```

> **Lencana merah selepas 7 hari** ialah sentuhan kecil dengan kesan besar — ia menjadikan permohonan tersekat kelihatan kepada pemohon **dan** admin.

### ✅ Semakan

- [ ] Garis masa menunjukkan empat kedudukan
- [ ] Nama pelaku dimuat dalam **satu** query (tiada N+1)
- [ ] Peringkat semasa ditandakan dengan jelas
- [ ] Lencana hari bertukar merah selepas 7 hari
- [ ] Catatan penyelia/ICT dipaparkan

---

## Latihan 2 — Audit yang lebih kaya

**Objektif:** Rekod apa yang servis kongsi tidak tangkap — tanpa mengubahnya.

### Langkah

1. Kemas kini `Process` (Hari 7–9) untuk menulis audit terperinci **selepas** transaksi:

```csharp
// Selepas transaksi commit, tulis butiran audit yang kaya.
var diluluskanSenarai = vm.Akses.Where(a => a.Diluluskan == true)
    .Select(a => a.Nama).ToList();
var ditolakSenarai = vm.Akses.Where(a => a.Diluluskan == false)
    .Select(a => $"{a.Nama} ({a.CatatanIct})").ToList();

var catatanAudit = new StringBuilder();
if (diluluskanSenarai.Count > 0)
    catatanAudit.Append($"Diluluskan: {string.Join(", ", diluluskanSenarai)}. ");
if (ditolakSenarai.Count > 0)
    catatanAudit.Append($"Ditolak: {string.Join("; ", ditolakSenarai)}. ");
if (app.AdAccountName is not null)
    catatanAudit.Append($"Akaun AD: {app.AdAccountName}. ");
if (app.OfficialEmail is not null)
    catatanAudit.Append($"E-mel: {app.OfficialEmail}.");

// Servis KONGSI — kami memanggilnya dengan catatan kaya,
// bukan menulis sistem audit sendiri.
await auditLog.LogAsync(app.SubmissionId, "AccessDecided",
    remarks: catatanAudit.ToString());
```

2. Tambah audit untuk penyerahan kelayakan:

```csharp
if (vm.KelayakanDiserahkan && !app.KelayakanDiserahkan)
{
    // 🔒 Kami merekod FAKTA penyerahan, bukan apa yang diserahkan.
    await auditLog.LogAsync(app.SubmissionId, "CredentialsHandedOver",
        remarks: $"Kelayakan diserahkan kepada {app.StaffName} " +
                 $"oleh {currentUser.UserId} pada {DateTime.UtcNow:dd/MM/yyyy HH:mm}.");
}
```

3. **Semakan pematuhan.** Buka satu permohonan yang selesai dan baca audit trailnya. Ia mesti menjawab, **tanpa membaca kod**:

   - [ ] Siapa memohon, dan bila?
   - [ ] Siapa meluluskan peringkat 1, bila, dengan catatan apa?
   - [ ] Akses mana diluluskan? Mana ditolak, dan mengapa?
   - [ ] Nama akaun apa yang diberikan?
   - [ ] Bila kelayakan diserahkan, dan oleh siapa?

   Jika mana-mana soalan tidak terjawab, tambah audit untuk menutupnya.

> **Jika kumpulan anda merasakan `AuditLog` memerlukan medan berstruktur** (`EntityType`, `EntityId`, `Changes` JSON) dan bukan hanya `Remarks` teks bebas — itu **isu `shared`** yang munasabah. Buka, jangan bina secara senyap.

### ✅ Semakan

- [ ] Audit merekod keputusan setiap akses dengan sebab
- [ ] Audit merekod nama akaun & e-mel
- [ ] Audit merekod penyerahan kelayakan — **fakta, bukan kandungan**
- [ ] Kelima-lima soalan pematuhan dijawab oleh audit trail
- [ ] `IAuditLogService` **tidak** diubah suai

---

## Latihan 3 — Papan pemuka ICT

**Objektif:** Skrin operasi, bukan hanya statistik.

### Langkah

1. `ViewModels/Akaun/IctDashboardViewModel.cs`:

```csharp
namespace Nres.Onboarding.Web.ViewModels.Akaun;

public class IctDashboardViewModel
{
    public int MenungguIct { get; set; }
    public int TersekatPadaPenyelia { get; set; }
    public double PurataHariPenyelia { get; set; }
    public double PurataHariIct { get; set; }
    public int AkaunDiciptaBulanIni { get; set; }

    /// <summary>Senarai kerja — bukan statistik.</summary>
    public IReadOnlyList<PermohonanLambat> Lambat { get; set; } = [];

    public IReadOnlyList<AksesDitolak> AksesPalingKerapDitolak { get; set; } = [];

    public record PermohonanLambat(
        int ApplicationId, string ReferenceNo, string StaffName,
        string Peringkat, int Hari);

    public record AksesDitolak(string Nama, int Ditolak, int Jumlah, double Peratus);
}
```

2. Servis dashboard:

```csharp
public async Task<IctDashboardViewModel> DashboardAsync(CancellationToken ct = default)
{
    var sebulanLalu = DateTime.UtcNow.AddDays(-30);

    var submissions = db.Submissions.AsNoTracking()
        .Where(s => s.ModuleCode == ModuleCodes.IdAdEmail);

    var vm = new IctDashboardViewModel
    {
        MenungguIct = await submissions
            .CountAsync(s => s.Status == SubmissionStatus.SupervisorApproved, ct),
        TersekatPadaPenyelia = await submissions
            .CountAsync(s => s.Status == SubmissionStatus.Submitted, ct),
        AkaunDiciptaBulanIni = await db.Set<AccountRequest>().AsNoTracking()
            .CountAsync(a => a.AdAccountName != null
                          && a.Submission!.CompletedAt >= sebulanLalu, ct)
    };

    // --- Purata masa setiap peringkat ---
    // Kami memuatkan pasangan tarikh dan mengira dalam C# kerana fungsi
    // beza-tarikh berbeza antara penyedia DB (SQLite vs SQL Server).
    // Set data kecil, jadi ini boleh diterima — didokumenkan Hari 13–14.
    var tempoh = await (
        from s in submissions
        where s.SubmittedAt != null
        join st in db.ApprovalSteps.AsNoTracking() on s.Id equals st.SubmissionId
        where st.DecidedAt != null
        select new { s.Id, s.SubmittedAt, st.StepOrder, st.DecidedAt })
        .ToListAsync(ct);

    var penyelia = tempoh.Where(t => t.StepOrder == 1)
        .Select(t => (t.DecidedAt!.Value - t.SubmittedAt!.Value).TotalDays).ToList();

    var ictTempoh = (from t2 in tempoh.Where(t => t.StepOrder == 2)
                     join t1 in tempoh.Where(t => t.StepOrder == 1)
                         on t2.Id equals t1.Id
                     select (t2.DecidedAt!.Value - t1.DecidedAt!.Value).TotalDays)
                    .ToList();

    vm.PurataHariPenyelia = penyelia.Count > 0 ? penyelia.Average() : 0;
    vm.PurataHariIct = ictTempoh.Count > 0 ? ictTempoh.Average() : 0;

    // --- Senarai kerja: lebih 7 hari belum selesai ---
    var ambang = DateTime.UtcNow.AddDays(-7);
    vm.Lambat = await (
        from a in db.Set<AccountRequest>().AsNoTracking()
        join s in submissions on a.SubmissionId equals s.Id
        where (s.Status == SubmissionStatus.Submitted
            || s.Status == SubmissionStatus.SupervisorApproved)
           && s.SubmittedAt < ambang
        orderby s.SubmittedAt
        select new IctDashboardViewModel.PermohonanLambat(
            a.Id, s.ReferenceNo, a.StaffName,
            s.Status == SubmissionStatus.Submitted ? "Penyelia" : "ICT",
            (int)(DateTime.UtcNow - s.SubmittedAt!.Value).Days))
        .Take(20).ToListAsync(ct);

    // --- Akses paling kerap ditolak ---
    vm.AksesPalingKerapDitolak = await (
        from r in db.Set<RequestedSystemAccess>().AsNoTracking()
        where r.Diluluskan != null
        group r by r.SystemAccess!.Name into g
        select new IctDashboardViewModel.AksesDitolak(
            g.Key,
            g.Count(x => x.Diluluskan == false),
            g.Count(),
            g.Count() == 0 ? 0 : g.Count(x => x.Diluluskan == false) * 100.0 / g.Count()))
        .Where(x => x.Ditolak > 0)
        .OrderByDescending(x => x.Peratus)
        .Take(10).ToListAsync(ct);

    return vm;
}
```

3. View — utamakan **senarai kerja**, bukan kad statistik:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Akaun.IctDashboardViewModel
@{ ViewData["Title"] = "Papan Pemuka ICT — Akaun & Akses"; }

<h2>@ViewData["Title"]</h2>

<div class="row g-3 my-3">
    <div class="col-md-3"><div class="card text-bg-primary"><div class="card-body">
        <div class="display-6">@Model.MenungguIct</div>
        <div>Menunggu ICT</div></div></div></div>
    <div class="col-md-3"><div class="card text-bg-warning"><div class="card-body">
        <div class="display-6">@Model.TersekatPadaPenyelia</div>
        <div>Menunggu penyelia</div></div></div></div>
    <div class="col-md-3"><div class="card"><div class="card-body">
        <div class="display-6">@Model.PurataHariPenyelia.ToString("0.0")</div>
        <div class="text-muted">Purata hari — penyelia</div></div></div></div>
    <div class="col-md-3"><div class="card"><div class="card-body">
        <div class="display-6">@Model.PurataHariIct.ToString("0.0")</div>
        <div class="text-muted">Purata hari — ICT</div></div></div></div>
</div>

@if (Model.Lambat.Any())
{
    <div class="card border-danger mb-4">
        <div class="card-header bg-danger text-white">
            ⚠ Permohonan melebihi 7 hari (@Model.Lambat.Count)
        </div>
        <table class="table table-sm mb-0">
            <thead><tr><th>Rujukan</th><th>Staf</th><th>Tersekat pada</th><th>Hari</th><th></th></tr></thead>
            <tbody>
            @foreach (var l in Model.Lambat)
            {
                <tr>
                    <td>@l.ReferenceNo</td>
                    <td>@l.StaffName</td>
                    <td>@l.Peringkat</td>
                    <td><span class="badge bg-danger">@l.Hari</span></td>
                    <td class="text-end">
                        <a asp-action="Process" asp-route-id="@l.ApplicationId"
                           class="btn btn-sm btn-outline-primary">Buka</a>
                    </td>
                </tr>
            }
            </tbody>
        </table>
    </div>
}

<h5>Akses paling kerap ditolak</h5>
<p class="text-muted small">
    Kadar penolakan tinggi mungkin bermakna borang perlu menerangkan kriteria
    dengan lebih baik — bukan bahawa pemohon salah.
</p>
<table class="table table-sm">
    <thead><tr><th>Akses</th><th class="text-end">Ditolak</th><th class="text-end">Jumlah</th><th class="text-end">%</th></tr></thead>
    <tbody>
    @foreach (var a in Model.AksesPalingKerapDitolak)
    {
        <tr>
            <td>@a.Nama</td>
            <td class="text-end">@a.Ditolak</td>
            <td class="text-end">@a.Jumlah</td>
            <td class="text-end">
                <span class="badge @(a.Peratus > 50 ? "bg-danger" : "bg-secondary")">
                    @a.Peratus.ToString("0")%
                </span>
            </td>
        </tr>
    }
    </tbody>
</table>
```

### ✅ Semakan

- [ ] Purata masa **setiap peringkat** berasingan
- [ ] Senarai kerja "lebih 7 hari" diutamakan
- [ ] Akses paling kerap ditolak dengan peratusan
- [ ] Nota tafsiran pada kadar penolakan
- [ ] Hanya `IctAdmin`

---

## Latihan 4 — Carian & penapis lanjutan

**Objektif:** Cari permohonan mengikut apa sahaja yang ICT ingat.

### Langkah

Tambah penapis ke baris gilir ICT:

| Penapis | Kenapa ICT perlukannya |
|---------|------------------------|
| Nombor rujukan | Pemohon menelefon dengan rujukan |
| Nama staf | Pemohon menelefon tanpa rujukan |
| Nama akaun AD | "Siapa meluluskan akaun ini?" |
| Jenis permohonan | Proses semua nyahaktif sekaligus |
| Status | Baris gilir vs sejarah |
| Julat tarikh | Laporan bulanan |
| **Akses tertentu dipohon** | "Tunjukkan semua permohonan VPN" |

Penapis terakhir memerlukan join ke `RequestedSystemAccess`:

```csharp
if (penapis.SystemAccessId is not null)
{
    q = q.Where(x => db.Set<RequestedSystemAccess>()
        .Any(r => r.AccountRequestId == x.ApplicationId
               && r.SystemAccessId == penapis.SystemAccessId));
}
```

Guna `_FilterBar` **kongsi** sebagai asas dan tambah medan khusus anda.

### ✅ Semakan

- [ ] Ketujuh-tujuh penapis berfungsi
- [ ] Penapis kekal apabila menukar halaman
- [ ] Carian nama akaun AD berfungsi
- [ ] Guna `_FilterBar` kongsi

---

## Latihan 5 — Tutup blok

```bash
git diff --name-only master
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Garis masa penjejakan berfungsi untuk kedua-dua peringkat
- [ ] Audit trail menjawab kelima-lima soalan pematuhan
- [ ] `IAuditLogService` kongsi **tidak** diubah
- [ ] Dashboard ICT mengutamakan senarai kerja
- [ ] Purata masa setiap peringkat berasingan
- [ ] Carian & penapis lengkap
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 10–12

| Artifak | Lokasi |
|---------|--------|
| `ITrackingService` + garis masa | `Services/Akaun/`, `Views/Akaun/_GarisMasa.cshtml` |
| Audit kaya (akses, akaun, penyerahan) | `AccountRequestController.Process` |
| Papan pemuka ICT | `Views/Akaun/IctDashboard.cshtml` |
| Carian & penapis lanjutan | `Controllers/`, `Views/Akaun/` |

**Seterusnya (Hari 13–14):** **RBAC testing**, security audit, dan persediaan gabungan akhir.
