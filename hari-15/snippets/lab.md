# Lab Hari 15 — Gabungan, Papan Pemuka Induk, SIT & Demo

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md) · Kanun: [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md)

## Persediaan

- Keempat-empat cabang kumpulan telah di-push dan gabungan kering bersih (Hari 14)
- `README-modul.md` setiap kumpulan tersedia
- MailHog/Papercut berjalan pada port 1025
- Jurulatih telah menetapkan turutan gabungan

---

## Latihan 1 — Sediakan gabungan

**Objektif:** Titik permulaan yang diketahui baik untuk semua orang.

### Langkah

1. **Semua orang** menyegerak:

```bash
git switch master
git pull --rebase origin master
dotnet build
dotnet test
```

2. Rekod garis dasar dalam `docs/hari-15-integrasi.md`:

```markdown
# Integrasi Hari 15

## Garis dasar sebelum gabungan
- Commit master: <sha>
- Binaan: ✅ / ❌
- Ujian: <n> lulus
- Turutan gabungan: K<?> → K<?> → K<?> → K<?>
```

3. **Turutan gabungan** diumumkan jurulatih, berdasarkan keputusan gabungan kering Hari 14 (paling bersih dahulu).

4. Satu orang setiap kumpulan bertindak sebagai **pemandu gabungan**; yang lain memerhati skrin projektor.

### ✅ Semakan

- [ ] Semua orang pada `master` yang sama
- [ ] Binaan garis dasar dan ujian lulus
- [ ] Turutan gabungan diumumkan dan direkod

---

## Latihan 2 — Gabung empat cabang, satu demi satu

**Objektif:** `master` mengandungi keempat-empat modul, disahkan pada setiap langkah.

> **Jangan gabung kesemuanya serentak.** Jika sesuatu pecah selepas gabungan ketiga, anda mahu tahu kumpulan mana yang menyebabkannya.

### Langkah

**Ulang blok ini untuk setiap kumpulan mengikut turutan:**

1. Gabung melalui PR (bukan push terus):

```bash
git switch master
git pull --rebase origin master
git merge origin/kump-N/<slug> --no-ff
```

2. Jika konflik berlaku:

| Fail berkonflik | Kemungkinan punca | Tindakan |
|-----------------|-------------------|----------|
| `Program.cs` | Dua kumpulan menyahkomen baris berdekatan | Simpan **kedua-dua** baris pendaftaran |
| `*.csproj` | Dua kumpulan menambah pakej | Simpan **kedua-dua** `PackageReference` |
| `ApplicationDbContextModelSnapshot.cs` | Migration bertindih | **Jangan baiki dengan tangan** — lihat langkah 5 |
| Fail modul | Sepatutnya mustahil | Seseorang menulis di luar folder mereka — siasat |

3. Selepas **setiap** gabungan, sahkan:

```bash
dotnet build
dotnet test
dotnet run     # semak aplikasi bermula, log masuk berfungsi
```

4. Rekod hasilnya:

```markdown
| # | Kumpulan | Konflik | Fail | Binaan | Ujian | Nota |
|---|----------|---------|------|--------|-------|------|
| 1 | K2 | 0 | — | ✅ | 18 ✅ | Bersih |
| 2 | K1 | 1 | Program.cs | ✅ | 33 ✅ | Simpan kedua-dua baris |
| 3 | K4 | 2 | csproj, snapshot | ✅ | 51 ✅ | Snapshot dijana semula |
| 4 | K3 | 0 | — | ✅ | 67 ✅ | Bersih |
```

5. **Jika snapshot berkonflik:**

```bash
git checkout --ours Migrations/ApplicationDbContextModelSnapshot.cs
git add Migrations/ApplicationDbContextModelSnapshot.cs
git commit
# kemudian jana semula snapshot dari model gabungan:
cd Nres.Onboarding.Web
dotnet ef migrations add SelarasSnapshot
# Jika migration yang dijana KOSONG, buang ia — snapshot sudah betul:
dotnet ef migrations remove
cd ..
```

6. Push `master` selepas setiap gabungan berjaya.

### ✅ Semakan

- [ ] Keempat-empat cabang digabung mengikut turutan
- [ ] `dotnet build` bersih selepas setiap gabungan
- [ ] `dotnet test` lulus selepas setiap gabungan
- [ ] Setiap konflik direkod dengan punca dan penyelesaian
- [ ] `master` di-push

---

## Latihan 3 — Sahkan skema bersepadu

**Objektif:** Migration daripada kosong berfungsi — apa yang deployment sebenar lakukan.

### Langkah

1. **Ujian pangkalan data kosong:**

```bash
cd Nres.Onboarding.Web
mv App_Data/nres-onboarding.db App_Data/backup.db
dotnet ef database update
dotnet run
```

Aplikasi mesti bermula, menyemai peranan & pengguna demo, dan log masuk mesti berfungsi.

2. **Semakan kewarasan snapshot:**

```bash
dotnet ef migrations add SemakanKewarasan
```

Buka fail yang dijana. Jika kaedah `Up()` **kosong**, model dan snapshot konsisten — bagus. Buangnya:

```bash
dotnet ef migrations remove
```

Jika ia **tidak** kosong, seseorang mengubah entiti tanpa migration. Cari siapa, dan tambah migration itu dengan betul.

3. Sahkan semua jadual wujud:

```bash
sqlite3 App_Data/nres-onboarding.db ".tables"
```

Jangkakan: jadual Identity · `Submissions`, `Attachments`, `AuditLogs`, `ApprovalSteps`, `UserProfiles`, `Lookup*` · dan jadual keempat-empat modul.

### ✅ Semakan

- [ ] `dotnet ef database update` pada DB kosong berjaya
- [ ] Semakan kewarasan menjana migration **kosong**
- [ ] Semua jadual daripada keempat-empat modul wujud
- [ ] Seeding berjalan; log masuk berfungsi

---

## Latihan 4 — Papan Pemuka Induk NRES

**Objektif:** Satu skrin yang menunjukkan sistem sebagai **satu** sistem.

> Ini kerja **kongsi**. Jurulatih memandu; peserta menyumbang. Ia hidup dalam `Controllers/DashboardController.cs` dan `Views/Dashboard/` — bukan folder mana-mana kumpulan.

### Langkah

1. `ViewModels/Shared/MasterDashboardViewModel.cs`:

```csharp
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Shared;

public class MasterDashboardViewModel
{
    // --- Pandangan pemohon ---
    public int DrafSaya { get; set; }
    public int DihantarSaya { get; set; }
    public int DiluluskanSaya { get; set; }
    public int DitolakSaya { get; set; }
    public IReadOnlyList<SubmissionRingkas> PermohonanSaya { get; set; } = [];

    // --- Pandangan admin ---
    public IReadOnlyList<SubmissionRingkas> MenungguKelulusanSaya { get; set; } = [];

    // --- Modul yang boleh diakses pengguna ---
    public IReadOnlyList<ModuleDescriptor> Modul { get; set; } = [];

    public record SubmissionRingkas(
        int SubmissionId, string ReferenceNo, string ModuleCode,
        string ModuleNama, string Controller,
        SubmissionStatus Status, DateTime CreatedAt, DateTime? SubmittedAt);
}
```

2. `Controllers/DashboardController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels.Shared;

namespace Nres.Onboarding.Web.Controllers;

/// <summary>
/// Papan Pemuka Induk NRES. Perhatikan: TIADA kod khusus modul di sini.
/// Kerana keempat-empat modul berkongsi Submission induk dan mendaftarkan
/// ModuleDescriptor, dashboard ini berfungsi untuk kesemuanya — dan akan
/// berfungsi untuk modul ke-5 tanpa perubahan.
/// </summary>
[Authorize]
public class DashboardController(
    ApplicationDbContext db,
    ICurrentUserService currentUser,
    IEnumerable<IModuleDescriptorProvider> moduleProviders) : Controller
{
    public async Task<IActionResult> Index()
    {
        var userId = currentUser.UserId!;

        var modul = moduleProviders.Select(p => p.Describe())
            .Where(m => m.Roles.Any(r => User.IsInRole(r)))
            .OrderBy(m => m.Urutan)
            .ToList();

        var petaModul = moduleProviders.Select(p => p.Describe())
            .ToDictionary(m => m.Code, m => m);

        var saya = db.Submissions.AsNoTracking()
            .Where(s => s.ApplicantUserId == userId);

        var vm = new MasterDashboardViewModel
        {
            Modul = modul,
            DrafSaya       = await saya.CountAsync(s => s.Status == SubmissionStatus.Draft),
            DihantarSaya   = await saya.CountAsync(s => s.Status == SubmissionStatus.Submitted),
            DiluluskanSaya = await saya.CountAsync(s =>
                s.Status == SubmissionStatus.AdminApproved
             || s.Status == SubmissionStatus.Completed),
            DitolakSaya    = await saya.CountAsync(s => s.Status == SubmissionStatus.Rejected)
        };

        var permohonanSaya = await saya
            .OrderByDescending(s => s.CreatedAt).Take(10)
            .Select(s => new { s.Id, s.ReferenceNo, s.ModuleCode,
                               s.Status, s.CreatedAt, s.SubmittedAt })
            .ToListAsync();

        vm.PermohonanSaya = permohonanSaya.Select(s =>
            new MasterDashboardViewModel.SubmissionRingkas(
                s.Id, s.ReferenceNo, s.ModuleCode,
                petaModul.GetValueOrDefault(s.ModuleCode)?.Nama ?? s.ModuleCode,
                petaModul.GetValueOrDefault(s.ModuleCode)?.Controller ?? "Home",
                s.Status, s.CreatedAt, s.SubmittedAt)).ToList();

        // Baris gilir admin: modul mana yang peranan saya boleh luluskan?
        var kodModulSaya = modul.Select(m => m.Code).ToList();
        var menunggu = await db.Submissions.AsNoTracking()
            .Where(s => kodModulSaya.Contains(s.ModuleCode)
                     && (s.Status == SubmissionStatus.Submitted
                      || s.Status == SubmissionStatus.SupervisorApproved))
            .OrderBy(s => s.SubmittedAt).Take(15)
            .Select(s => new { s.Id, s.ReferenceNo, s.ModuleCode,
                               s.Status, s.CreatedAt, s.SubmittedAt })
            .ToListAsync();

        vm.MenungguKelulusanSaya = menunggu.Select(s =>
            new MasterDashboardViewModel.SubmissionRingkas(
                s.Id, s.ReferenceNo, s.ModuleCode,
                petaModul.GetValueOrDefault(s.ModuleCode)?.Nama ?? s.ModuleCode,
                petaModul.GetValueOrDefault(s.ModuleCode)?.Controller ?? "Home",
                s.Status, s.CreatedAt, s.SubmittedAt)).ToList();

        return View(vm);
    }

    /// <summary>Carian nombor rujukan global merentas KEEMPAT-EMPAT modul.</summary>
    public async Task<IActionResult> Cari(string? q)
    {
        if (string.IsNullOrWhiteSpace(q))
            return View(Array.Empty<MasterDashboardViewModel.SubmissionRingkas>());

        var petaModul = moduleProviders.Select(p => p.Describe())
            .ToDictionary(m => m.Code, m => m);

        var userId = currentUser.UserId!;
        var query = db.Submissions.AsNoTracking()
            .Where(s => s.ReferenceNo.Contains(q));

        // Pemohon melihat hanya miliknya; admin melihat modul yang dibenarkan.
        var kodDibenarkan = petaModul.Values
            .Where(m => m.Roles.Any(r => User.IsInRole(r)) && r_IsAdminRole(m))
            .Select(m => m.Code).ToList();

        query = query.Where(s => s.ApplicantUserId == userId
                              || kodDibenarkan.Contains(s.ModuleCode));

        var hasil = await query.OrderByDescending(s => s.CreatedAt).Take(50)
            .Select(s => new { s.Id, s.ReferenceNo, s.ModuleCode,
                               s.Status, s.CreatedAt, s.SubmittedAt })
            .ToListAsync();

        return View(hasil.Select(s =>
            new MasterDashboardViewModel.SubmissionRingkas(
                s.Id, s.ReferenceNo, s.ModuleCode,
                petaModul.GetValueOrDefault(s.ModuleCode)?.Nama ?? s.ModuleCode,
                petaModul.GetValueOrDefault(s.ModuleCode)?.Controller ?? "Home",
                s.Status, s.CreatedAt, s.SubmittedAt)).ToList());

        bool r_IsAdminRole(ModuleDescriptor m) =>
            m.Roles.Any(r => r != "Applicant" && User.IsInRole(r));
    }
}
```

3. `Views/Dashboard/Index.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.Shared.MasterDashboardViewModel
@{ ViewData["Title"] = "Papan Pemuka NRES"; }

<h2>Papan Pemuka NRES</h2>

<form asp-action="Cari" method="get" class="row g-2 my-3">
    <div class="col-md-6">
        <input name="q" class="form-control"
               placeholder="Cari nombor rujukan (LD-2026-0001, PAS-2026-0012, ...)" />
    </div>
    <div class="col-md-2">
        <button class="btn btn-outline-primary w-100">Cari</button>
    </div>
</form>

<h5 class="mt-4">Permohonan Saya</h5>
<div class="row g-3">
    <div class="col-md-3"><div class="card text-bg-secondary"><div class="card-body">
        <div class="display-6">@Model.DrafSaya</div><div>Draf</div></div></div></div>
    <div class="col-md-3"><div class="card text-bg-primary"><div class="card-body">
        <div class="display-6">@Model.DihantarSaya</div><div>Dihantar</div></div></div></div>
    <div class="col-md-3"><div class="card text-bg-success"><div class="card-body">
        <div class="display-6">@Model.DiluluskanSaya</div><div>Diluluskan</div></div></div></div>
    <div class="col-md-3"><div class="card text-bg-danger"><div class="card-body">
        <div class="display-6">@Model.DitolakSaya</div><div>Ditolak</div></div></div></div>
</div>

<h5 class="mt-4">Permohonan Terkini Saya</h5>
<table class="table table-hover">
    <thead><tr><th>Rujukan</th><th>Modul</th><th>Status</th><th>Tarikh</th><th></th></tr></thead>
    <tbody>
    @if (!Model.PermohonanSaya.Any())
    {
        <tr><td colspan="5" class="text-muted">Tiada permohonan lagi.</td></tr>
    }
    @foreach (var s in Model.PermohonanSaya)
    {
        <tr>
            <td>@(string.IsNullOrEmpty(s.ReferenceNo) ? "(draf)" : s.ReferenceNo)</td>
            <td>@s.ModuleNama</td>
            <td><partial name="_StatusBadge" model="s.Status" /></td>
            <td>@s.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy")</td>
            <td class="text-end">
                <a asp-controller="@s.Controller" asp-action="Index"
                   class="btn btn-sm btn-outline-primary">Buka modul</a>
            </td>
        </tr>
    }
    </tbody>
</table>

@if (Model.MenungguKelulusanSaya.Any())
{
    <h5 class="mt-4">Menunggu Kelulusan Saya</h5>
    <table class="table table-hover">
        <thead><tr><th>Rujukan</th><th>Modul</th><th>Status</th><th>Dihantar</th><th></th></tr></thead>
        <tbody>
        @foreach (var s in Model.MenungguKelulusanSaya)
        {
            <tr>
                <td>@s.ReferenceNo</td>
                <td>@s.ModuleNama</td>
                <td><partial name="_StatusBadge" model="s.Status" /></td>
                <td>@s.SubmittedAt?.ToLocalTime().ToString("dd/MM/yyyy")</td>
                <td class="text-end">
                    <a asp-controller="@s.Controller" asp-action="Index"
                       class="btn btn-sm btn-primary">Semak</a>
                </td>
            </tr>
        }
        </tbody>
    </table>
}

<h5 class="mt-4">Modul Tersedia</h5>
<div class="row g-3">
@foreach (var m in Model.Modul)
{
    <div class="col-md-3">
        <div class="card h-100">
            <div class="card-body">
                <h6 class="card-title"><i class="@m.Ikon"></i> @m.Nama</h6>
                <p class="text-muted small">Prefix: @m.Code</p>
                <a asp-controller="@m.Controller" asp-action="Index"
                   class="btn btn-sm btn-outline-primary">Buka</a>
            </div>
        </div>
    </div>
}
</div>
```

4. Jadikan dashboard halaman utama — kemas kini laluan lalai dalam `Program.cs`:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Dashboard}/{action=Index}/{id?}");
```

### ✅ Semakan

- [ ] Dashboard memaparkan kiraan peribadi merentas keempat-empat modul
- [ ] Baris gilir kelulusan menunjukkan hanya modul yang peranan pengguna boleh sentuh
- [ ] Carian global menemui rujukan daripada mana-mana modul
- [ ] Kad modul menunjukkan hanya modul yang dibenarkan pengguna
- [ ] **Sifar kod khusus modul** dalam `DashboardController`

---

## Latihan 5 — Sahkan navigasi bersepadu

**Objektif:** Setiap peranan melihat perkara yang betul.

### Langkah

Log masuk sebagai setiap pengguna demo dan rekod apa yang kelihatan:

| Pengguna | Peranan | Modul yang patut kelihatan |
|----------|---------|-----------------------------|
| `applicant@nres.test` | Applicant | Keempat-empat (sebagai pemohon) |
| `hr@nres.test` | HrAdmin | Lapor Diri |
| `keselamatan@nres.test` | SecurityAdmin | Pas/Parkir/Pelekat |
| `ict@nres.test` | IctAdmin | ID/AD/Email + Perisian/Aset |
| `penyelia@nres.test` | Supervisor | ID/AD/Email (kelulusan peringkat 1) |
| `admin@nres.test` | SystemAdmin | Semua |

Rekod dalam `docs/hari-15-integrasi.md`. Sebarang ketidakpadanan bermakna `Roles` dalam `ModuleDescriptor` sesuatu kumpulan salah — betulkan sekarang.

### ✅ Semakan

- [ ] Keenam-enam peranan diuji
- [ ] Setiap satu melihat tepat modul yang dijangka
- [ ] Ketidakpadanan dibetulkan

---

## Latihan 6 — SIT: skrip rentas modul

**Objektif:** Uji sempadan yang tiada kumpulan uji.

### Langkah

**Ikuti satu pekerja baharu melalui keempat-empat modul.** Jalankan sebagai kelas, dengan skrin diprojekkan. Rekod setiap langkah.

```markdown
# Skrip SIT — Hari 15

## Bahagian A: Aliran hujung-ke-hujung (Ali bin Ahmad, staf baharu)

| # | Langkah | Peranan | Jangkaan | Keputusan |
|---|---------|---------|----------|-----------|
| A1 | Hantar lapor diri + 3 dokumen | Applicant | LD-2026-#### dijana | |
| A2 | Semak & luluskan | HrAdmin | Status AdminApproved, e-mel dihantar | |
| A3 | Muat turun Slip Akuan | Applicant | PDF dengan no. rujukan | |
| A4 | Mohon pas keselamatan + pelekat | Applicant | PAS/STK-2026-#### dijana | |
| A5 | Cuba pelekat kedua, plat sama | Applicant | **Ditolak** — pendua | |
| A6 | Luluskan pas | SecurityAdmin | QR dijana | |
| A7 | Imbas QR pada skrin ronda | SecurityAdmin | Pas sah dipaparkan | |
| A8 | Mohon akaun AD + e-mel | Applicant | ICT-ID-2026-#### dijana | |
| A9 | Luluskan peringkat 1 | Supervisor | Status SupervisorApproved | |
| A10 | Proses & lengkapkan | IctAdmin | Status Completed | |
| A11 | Mohon pinjaman laptop | Applicant | AST-L-2026-#### dijana | |
| A12 | Luluskan pinjaman | IctAdmin | Status aset → OnLoan, stok −1 | |
| A13 | Pulangkan laptop (kondisi: Baik) | IctAdmin | Status aset → Available, stok +1 | |

## Bahagian B: Semakan peringkat sistem

| # | Semakan | Jangkaan | Keputusan |
|---|---------|----------|-----------|
| B1 | Dashboard Ali | Kesemua 5 permohonan kelihatan | |
| B2 | Carian global "LD-2026" | Menemui permohonan lapor diri | |
| B3 | Carian global "AST-L-2026" | Menemui pinjaman aset | |
| B4 | Audit log setiap permohonan | Setiap peralihan status direkod | |
| B5 | Jumlah baris audit | ≥ 13 (satu setiap tindakan) | |

## Bahagian C: RBAC (setiap satu MESTI 403)

| # | Ujian | Jangkaan | Keputusan |
|---|-------|----------|-----------|
| C1 | HrAdmin → skrin semakan Keselamatan | 403 | |
| C2 | SecurityAdmin → skrin semakan HR | 403 | |
| C3 | IctAdmin → skrin semakan HR | 403 | |
| C4 | Applicant → mana-mana skrin admin | 403 | |
| C5 | Pemohon A → permohonan pemohon B | 403 | |
| C6 | Applicant → muat turun lampiran orang lain | 403 | |
| C7 | Supervisor → skrin pemprosesan ICT | 403 | |

## Bahagian D: Muat naik fail (setiap modul)

| # | Ujian | Jangkaan | Keputusan |
|---|-------|----------|-----------|
| D1 | Muat naik .exe | Ditolak | |
| D2 | Muat naik 6 MB | Ditolak | |
| D3 | Muat naik PDF sah | Diterima | |
| D4 | Fail wujud di App_Data, BUKAN wwwroot | Sahkan pada cakera | |
| D5 | Akses URL langsung ke fail | 404 (tiada di wwwroot) | |
```

**Bagi setiap kegagalan:** rekod, kelaskan (Blocker / Major / Minor), dan tetapkan kepada kumpulan pemilik. Blocker dibetulkan hari ini; yang lain menjadi backlog serahan.

### ✅ Semakan

- [ ] Bahagian A selesai — aliran hujung-ke-hujung berfungsi
- [ ] Bahagian B selesai — dashboard, carian, audit
- [ ] Bahagian C selesai — **kesemua tujuh 403 disahkan**
- [ ] Bahagian D selesai untuk sekurang-kurangnya dua modul
- [ ] Kegagalan direkod, dikelaskan, ditetapkan

---

## Latihan 7 — Deployment: SQLite → SQL Server

**Objektif:** Faham apa yang penukaran sebenarnya melibatkan.

> Demo jurulatih. Peserta memerhati dan mencatat — kita **tidak** menyediakan SQL Server dalam kelas.

### Langkah

1. Tunjuk perubahan satu baris:

```csharp
// Program.cs
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(connectionString));   // dahulunya UseSqlite
```

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=.;Database=NresOnboarding;Trusted_Connection=True;TrustServerCertificate=True"
}
```

2. **Kemudian tunjuk apa lagi yang berubah** — ini bahagian penting:

```bash
# Migration adalah KHUSUS PENYEDIA. Jana semula untuk SQL Server:
rm -rf Migrations/
dotnet ef migrations add InitialSqlServer
dotnet ef database update
```

3. Bincangkan apa yang mungkin pecah:

| Perkara | SQLite | SQL Server |
|---------|--------|------------|
| Indeks bertapis | `"ReferenceNo" <> ''` | `[ReferenceNo] <> ''` |
| `EF.Functions.DateDiffDay` | Tidak disokong | Disokong |
| Perbandingan rentetan | Sensitif huruf besar/kecil lalai | Tidak sensitif lalai |
| Jenis tarikh | Disimpan sebagai teks | `datetime2` sebenar |
| Concurrency | Kunci fail | Kunci baris |

4. Senarai semak keluaran — rekod dalam `docs/hari-15-integrasi.md`:

```markdown
## Senarai semak keluaran
- [ ] Connection string dari konfigurasi selamat (bukan appsettings yang di-commit)
- [ ] HTTPS dikuatkuasakan; HSTS dihidupkan
- [ ] App_Data/uploads di luar akar web, dengan sandaran
- [ ] Migration dijalankan sebagai langkah deployment yang jelas, bukan pada permulaan aplikasi
- [ ] Kata laluan pengguna demo dibuang / tetapan semula dipaksa
- [ ] Logging dikonfigurasi; ralat tidak mendedahkan surih tindanan
- [ ] Perkhidmatan SMTP sebenar dikonfigurasi
- [ ] Strategi sandaran pangkalan data disahkan
- [ ] Pengujian beban pada saiz data yang dijangka
```

### ✅ Semakan

- [ ] Peserta boleh menyatakan perubahan satu baris
- [ ] Peserta boleh menamakan **tiga** perkara lain yang perlu berubah
- [ ] Senarai semak keluaran direkod

---

## Latihan 8 — Demo capstone

**Objektif:** Setiap kumpulan membentangkan kerjanya.

### Langkah

**10 minit setiap kumpulan**, kemudian 5 minit soal jawab.

Struktur pembentangan:

```markdown
1. Modul kami dalam satu ayat                          (30 saat)
2. Demo langsung — satu aliran hujung-ke-hujung        (4 minit)
3. Satu keputusan seni bina yang kami banggakan        (2 minit)
4. Satu perkara yang kami akan lakukan secara berbeza  (1.5 minit)
5. Kolaborasi: satu konflik/duplikasi yang kami elak, dan bagaimana  (2 minit)
```

> **Item 5 wajib.** Ini kursus tentang kolaborasi seperti juga tentang .NET. Setiap kumpulan mesti menamakan satu perkara khusus — isu `shared` yang mereka buka, duplikasi yang semakan silang AI tangkap, konflik yang seni bina hilangkan.

**Retrospektif kelas (15 minit)** selepas keempat-empat pembentangan:

| Soalan | Kenapa ia penting |
|--------|-------------------|
| Berapa konflik gabungan yang kita hadapi hari ini? | Mengukur sama ada seni bina berfungsi |
| Berapa banyak kod pendua yang kita temui? | Mengukur sama ada proses `shared` berfungsi |
| Apa yang akan berlaku tanpa `AGENTS.md`? | Menjadikan pengajaran AI eksplisit |
| Apa yang paling banyak berubah tentang cara anda akan bekerja dalam pasukan? | Pemindahan ke pejabat |

### ✅ Semakan

- [ ] Keempat-empat kumpulan membentangkan
- [ ] Setiap satu mendemokan aliran hujung-ke-hujung yang berfungsi
- [ ] Setiap satu menamakan satu kemenangan kolaborasi tertentu
- [ ] Retrospektif kelas selesai dan direkod

---

## Deliverable Hari 15

| Artifak | Lokasi |
|---------|--------|
| `master` bersepadu 4 modul | Repo |
| Log gabungan | `docs/hari-15-integrasi.md` |
| Papan Pemuka Induk | `Controllers/DashboardController.cs`, `Views/Dashboard/` |
| Keputusan SIT (Bahagian A–D) | `docs/hari-15-integrasi.md` |
| Senarai semak keluaran | `docs/hari-15-integrasi.md` |
| Backlog serahan (isu belum selesai) | GitHub Projects |
| Nota retrospektif | `docs/hari-15-integrasi.md` |

---

## Serahan kepada NRES

Kumpulkan menjadi satu pek serahan:

- [ ] Pautan repo, `master` pada commit bersepadu
- [ ] `README-modul.md` keempat-empat kumpulan
- [ ] Dokumen URS / use case / ERD Hari 1 (4 set)
- [ ] Keputusan SIT dengan isu yang diketahui dikelaskan
- [ ] Senarai semak keluaran
- [ ] Soalan terbuka Hari 1 yang **masih** belum dijawab NRES
- [ ] Nota jelas: **UAT sebenar masih diperlukan dengan pengguna NRES sebenar**

> Bahagian terakhir itu penting. Kita menjalankan UAT **pre-check**, bukan UAT. Nyatakan itu secara terbuka dalam serahan.
