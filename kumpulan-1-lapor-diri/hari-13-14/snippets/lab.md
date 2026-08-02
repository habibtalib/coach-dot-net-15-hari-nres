# Lab · Kumpulan 1 · Hari 13–14 — Ujian, Refactor & Sedia Gabung

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md)
>
> **Tiada ciri baharu dalam blok ini.** Jika anda mendapati diri anda menambah ciri, berhenti.

---

## Latihan 0 — Mula blok & bekukan skop

```bash
git switch kump-1/lapor-diri
git pull --rebase origin master
git switch -c kump-1/feat/ujian-dan-refactor
dotnet build
```

**Senaraikan apa yang belum siap.** Berkumpulan, semak backlog anda dan tandakan setiap isu:

| Tanda | Maksud | Tindakan |
|-------|--------|----------|
| ✅ | Siap & diuji manual | Tiada |
| 🔧 | Siap tetapi ada pepijat diketahui | Betulkan hari ini |
| ⏸️ | Tidak siap | **Pindah ke backlog, jangan mula** |

Rekod dalam `docs/kumpulan-1/status-akhir.md`. Kejujuran di sini lebih berharga daripada kelihatan siap — Hari 15 akan mendedahkannya juga.

### ✅ Semakan

- [ ] Setiap isu backlog ditandakan
- [ ] Kerja belum siap dipindahkan, bukan dimulakan
- [ ] Anda ada senarai pepijat yang perlu dibetulkan hari ini

---

## Latihan 1 — Projek ujian

**Objektif:** `Nres.Onboarding.Tests` dengan pangkalan data SQLite sebenar dalam memori.

> **Koordinasi:** projek ujian dikongsi keempat-empat kumpulan. **Satu** kumpulan menciptanya (jurulatih memutuskan siapa); yang lain menariknya. Setiap kumpulan kemudian menambah **failnya sendiri** di bawah `Tests/<Modul>/`.

### Langkah

1. Jika kumpulan anda mencipta projek:

```bash
dotnet new xunit -o Nres.Onboarding.Tests
dotnet sln add Nres.Onboarding.Tests
cd Nres.Onboarding.Tests
dotnet add reference ../Nres.Onboarding.Web
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package FluentAssertions
cd ..
mkdir -p Nres.Onboarding.Tests/LaporDiri
```

2. `Nres.Onboarding.Tests/TestDbFactory.cs` — **fail kongsi, dicipta sekali**:

```csharp
using Microsoft.AspNetCore.Identity;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;

namespace Nres.Onboarding.Tests;

/// <summary>
/// SQLite in-memory — pangkalan data SQL SEBENAR dengan kekangan sebenar.
///
/// Kami TIDAK menggunakan penyedia EF Core InMemory: ia tidak menguatkuasakan
/// kunci asing atau indeks unik, jadi ujian akan lulus sementara pengeluaran
/// gagal. Indeks unik ditapis kami pada ReferenceNo ialah tepat jenis perkara
/// yang ia abaikan.
/// </summary>
public sealed class TestDbFactory : IDisposable
{
    private readonly SqliteConnection _connection;

    public ApplicationDbContext Db { get; }

    public TestDbFactory()
    {
        // Sambungan mesti kekal TERBUKA — menutupnya memusnahkan DB in-memory.
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        Db = new ApplicationDbContext(options);
        Db.Database.EnsureCreated();
    }

    public async Task<IdentityUser> SeedUserAsync(string id, string email)
    {
        var user = new IdentityUser
        {
            Id = id, UserName = email, Email = email,
            NormalizedEmail = email.ToUpperInvariant(),
            NormalizedUserName = email.ToUpperInvariant()
        };
        Db.Users.Add(user);
        await Db.SaveChangesAsync();
        return user;
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
```

### ✅ Semakan

- [ ] Projek ujian wujud dan dibina
- [ ] `TestDbFactory` menggunakan SQLite, **bukan** penyedia InMemory
- [ ] Sambungan kekal terbuka
- [ ] `dotnet test` berjalan (walaupun sifar ujian)

---

## Latihan 2 — Uji nombor rujukan

**Objektif:** Peraturan rekod rasmi anda betul.

### Langkah

`Nres.Onboarding.Tests/LaporDiri/ReferenceNumberTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Tests.LaporDiri;

public class ReferenceNumberTests
{
    [Fact]
    public async Task Rujukan_pertama_bermula_pada_0001()
    {
        using var f = new TestDbFactory();
        var servis = new ReferenceNumberService(f.Db);

        var rujukan = await servis.GenerateAsync(ModuleCodes.LaporDiri);

        rujukan.Should().Be($"LD-{DateTime.UtcNow.Year}-0001");
    }

    [Fact]
    public async Task Rujukan_bertambah_bagi_setiap_penghantaran()
    {
        using var f = new TestDbFactory();
        var servis = new ReferenceNumberService(f.Db);

        // Simulasi satu permohonan yang sudah dihantar.
        f.Db.Submissions.Add(new Submission
        {
            ModuleCode = ModuleCodes.LaporDiri,
            ApplicantUserId = "u1",
            ReferenceNo = $"LD-{DateTime.UtcNow.Year}-0001",
            Status = SubmissionStatus.Submitted
        });
        await f.Db.SaveChangesAsync();

        var rujukan = await servis.GenerateAsync(ModuleCodes.LaporDiri);

        rujukan.Should().Be($"LD-{DateTime.UtcNow.Year}-0002");
    }

    [Fact]
    public async Task Draf_tanpa_rujukan_tidak_menambah_kiraan()
    {
        using var f = new TestDbFactory();
        var servis = new ReferenceNumberService(f.Db);

        // Tiga draf, tiada satu pun dihantar — kesemuanya ReferenceNo kosong.
        for (var i = 0; i < 3; i++)
        {
            f.Db.Submissions.Add(new Submission
            {
                ModuleCode = ModuleCodes.LaporDiri,
                ApplicantUserId = $"u{i}",
                Status = SubmissionStatus.Draft
            });
        }
        await f.Db.SaveChangesAsync();

        var rujukan = await servis.GenerateAsync(ModuleCodes.LaporDiri);

        // Draf tidak mengambil nombor — permohonan sebenar pertama ialah 0001.
        rujukan.Should().Be($"LD-{DateTime.UtcNow.Year}-0001");
    }

    [Fact]
    public async Task Modul_berbeza_mempunyai_jujukan_berasingan()
    {
        using var f = new TestDbFactory();
        var servis = new ReferenceNumberService(f.Db);

        f.Db.Submissions.Add(new Submission
        {
            ModuleCode = ModuleCodes.PasKeselamatan,
            ApplicantUserId = "u1",
            ReferenceNo = $"PAS-{DateTime.UtcNow.Year}-0001",
            Status = SubmissionStatus.Submitted
        });
        await f.Db.SaveChangesAsync();

        var rujukan = await servis.GenerateAsync(ModuleCodes.LaporDiri);

        // Permohonan PAS Kumpulan 2 tidak menjejaskan jujukan LD kami.
        rujukan.Should().Be($"LD-{DateTime.UtcNow.Year}-0001");
    }
}
```

### ✅ Semakan

- [ ] Keempat-empat ujian lulus
- [ ] Ujian "draf tidak mengambil nombor" lulus — ini mengesahkan reka bentuk Hari 5–6
- [ ] Ujian pengasingan modul lulus

---

## Latihan 3 — Uji peraturan peralihan status

**Objektif:** Kelulusan tidak sah adalah mustahil.

### Langkah

`Nres.Onboarding.Tests/LaporDiri/WorkflowTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Tests.LaporDiri;

public class WorkflowTests
{
    private static WorkflowService Buat(TestDbFactory f) =>
        new(f.Db, new AuditLogService(f.Db, new FakeCurrentUser("hr1")));

    [Theory]
    [InlineData(SubmissionStatus.Draft,     SubmissionStatus.Submitted,      true)]
    [InlineData(SubmissionStatus.Draft,     SubmissionStatus.Cancelled,      true)]
    [InlineData(SubmissionStatus.Draft,     SubmissionStatus.AdminApproved,  false)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.AdminApproved,  true)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Rejected,       true)]
    [InlineData(SubmissionStatus.Rejected,  SubmissionStatus.AdminApproved,  false)]
    [InlineData(SubmissionStatus.Rejected,  SubmissionStatus.Rejected,       false)]
    [InlineData(SubmissionStatus.Completed, SubmissionStatus.Rejected,       false)]
    [InlineData(SubmissionStatus.Cancelled, SubmissionStatus.Submitted,      false)]
    public void Peralihan_status_mengikut_peraturan(
        SubmissionStatus dari, SubmissionStatus ke, bool dibenarkan)
    {
        using var f = new TestDbFactory();
        Buat(f).CanTransition(dari, ke).Should().Be(dibenarkan);
    }

    [Fact]
    public async Task Meluluskan_permohonan_yang_ditolak_dilontar()
    {
        using var f = new TestDbFactory();
        await f.SeedUserAsync("hr1", "hr@nres.test");

        var submission = new Submission
        {
            ModuleCode = ModuleCodes.LaporDiri,
            ApplicantUserId = "u1",
            ReferenceNo = "LD-2026-0001",
            Status = SubmissionStatus.Rejected
        };
        f.Db.Submissions.Add(submission);
        await f.Db.SaveChangesAsync();

        var act = async () => await Buat(f)
            .TransitionAsync(submission, SubmissionStatus.AdminApproved, "Approved");

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Peralihan_menulis_audit_log()
    {
        using var f = new TestDbFactory();
        await f.SeedUserAsync("hr1", "hr@nres.test");

        var submission = new Submission
        {
            ModuleCode = ModuleCodes.LaporDiri,
            ApplicantUserId = "u1",
            ReferenceNo = "LD-2026-0001",
            Status = SubmissionStatus.Submitted
        };
        f.Db.Submissions.Add(submission);
        await f.Db.SaveChangesAsync();

        await Buat(f).TransitionAsync(submission,
            SubmissionStatus.Rejected, "Rejected", "Sijil tidak jelas");

        var log = f.Db.AuditLogs.Single();
        log.Action.Should().Be("Rejected");
        log.FromStatus.Should().Be(SubmissionStatus.Submitted);
        log.ToStatus.Should().Be(SubmissionStatus.Rejected);
        log.Remarks.Should().Be("Sijil tidak jelas");
        log.ActorUserId.Should().Be("hr1");
    }
}

/// <summary>Ganti mudah untuk ICurrentUserService dalam ujian.</summary>
public class FakeCurrentUser(string? userId, params string[] roles) : ICurrentUserService
{
    public string? UserId => userId;
    public bool IsInRole(string role) => roles.Contains(role);
}
```

> **Perhatikan ujian terakhir:** ia mengesahkan bahawa perubahan status dan audit berlaku **bersama**. Inilah sebab `IWorkflowService` wujud dan bukan `submission.Status = ...` bertaburan.

### ✅ Semakan

- [ ] Kesemua 9 kes `[Theory]` lulus
- [ ] Meluluskan permohonan yang ditolak dilontar
- [ ] Audit log ditulis dengan status lama/baharu dan catatan
- [ ] `FakeCurrentUser` dalam folder ujian anda, bukan kod pengeluaran

---

## Latihan 4 — Uji peraturan modul anda

**Objektif:** Peraturan perniagaan Lapor Diri, dipandu URS Hari 1.

### Langkah

`Nres.Onboarding.Tests/LaporDiri/AttachmentRulesTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.LaporDiri;

namespace Nres.Onboarding.Tests.LaporDiri;

public class DokumenSokonganTests
{
    [Fact]
    public void Tiga_dokumen_adalah_wajib()
    {
        DokumenSokongan.Wajib.Should().BeEquivalentTo(new[]
        {
            JenisDokumen.KadPengenalan,
            JenisDokumen.SuratTawaran,
            JenisDokumen.SijilAkademik
        });
    }

    [Fact]
    public void Slip_gaji_dan_akuan_sumpah_TIDAK_wajib()
    {
        DokumenSokongan.Wajib.Should().NotContain(JenisDokumen.SlipGajiTerakhir);
        DokumenSokongan.Wajib.Should().NotContain(JenisDokumen.SuratAkuanSumpah);
    }

    [Theory]
    [InlineData(JenisDokumen.KadPengenalan, "Salinan Kad Pengenalan")]
    [InlineData(JenisDokumen.SuratTawaran, "Surat Tawaran / Lantikan")]
    public void Nama_dokumen_dipaparkan_dalam_Bahasa_Melayu(
        JenisDokumen jenis, string dijangka)
    {
        DokumenSokongan.Nama(jenis).Should().Be(dijangka);
    }
}
```

Tambah ujian untuk `MissingRequiredAsync` menggunakan `TestDbFactory` — corak sama seperti di atas.

**Pemetaan URS → ujian.** Cipta `docs/kumpulan-1/pemetaan-ujian.md`:

```markdown
# Pemetaan URS → Ujian — Kumpulan 1

| ID URS | Keperluan | Ujian | Status |
|--------|-----------|-------|--------|
| URS-LD-001 | Simpan draf tidak lengkap | (manual) | ✅ |
| URS-LD-004 | Dokumen wajib sebelum hantar | `MissingRequiredAsync_*` | ✅ |
| URS-LD-005 | Tolak dengan sebab wajib | `Peralihan_menulis_audit_log` | ✅ |
| URS-LD-007 | No. rujukan unik LD-YYYY-NNNN | `ReferenceNumberTests` | ✅ |
| URS-LD-009 | Kunci selepas hantar | (manual) | ✅ |
| URS-LD-012 | Notifikasi e-mel | (manual) | ✅ |
```

### ✅ Semakan

- [ ] Ujian peraturan modul lulus
- [ ] Setiap keperluan URS "Mesti ada" ada baris dalam jadual pemetaan
- [ ] Keperluan yang diuji manual ditandakan jujur sebagai manual
- [ ] `dotnet test` — semua hijau

---

## Latihan 5 — Optimasi query

**Objektif:** Ukur, kemudian betulkan. Jangan meneka.

### Langkah

1. Hidupkan logging EF Core sementara dalam `appsettings.Development.json`:

```json
"Logging": {
  "LogLevel": {
    "Microsoft.EntityFrameworkCore.Database.Command": "Information"
  }
}
```

2. Jana data ujian — tambah kaedah seed sementara atau gunakan skrip untuk mencipta **200 permohonan**.

3. Jalankan setiap skrin dan **baca SQL dalam konsol**:

| Skrin | Cari |
|-------|------|
| `/OfficerReporting/Index` | Satu `SELECT`? |
| `/OfficerReporting/Dashboard` | Empat `COUNT`, tiada `SELECT *`? |
| `/OfficerReporting/Review` | Satu `COUNT` + satu `SELECT` berhalaman? |
| `/OfficerReporting/Details/1` | Query `Include` yang munasabah? |
| `/OfficerReporting/Analytics` | `GROUP BY` dalam SQL, bukan dalam C#? |

4. Rekod penemuan dalam `docs/kumpulan-1/prestasi.md`:

```markdown
# Semakan prestasi — Kumpulan 1

| Skrin | Query sebelum | Query selepas | Pembetulan |
|-------|---------------|---------------|------------|
| Review | 21 (N+1 pada Department) | 2 | Projek nama jabatan dalam Select |
| Dashboard | 4 | 4 | Tiada perubahan diperlukan |
| Analytics | 1 + kiraan dalam memori | 3 | GroupBy dialihkan ke DB |
```

5. Betulkan apa yang anda temui. Ujian anda membuktikan tingkah laku tidak berubah.

### ✅ Semakan

- [ ] Kelima-lima skrin diperiksa dengan logging SQL dihidupkan
- [ ] Sebarang N+1 ditemui dan dibetulkan
- [ ] `AsNoTracking()` pada setiap query baca-sahaja
- [ ] Penemuan didokumenkan
- [ ] Semua ujian masih lulus selepas pembetulan
- [ ] Logging dimatikan semula sebelum commit

---

## Latihan 6 — Refactor & pembersihan

**Objektif:** Kod yang orang lain boleh baca pada Hari 15.

### Langkah

1. **Semakan kod jana-AI.** Berkumpulan, imbas setiap fail dan tanya bagi setiap kaedah: *bolehkah seseorang di sini menerangkan ini?*

   Jika tidak: fahami, atau buang. Kod yang tiada siapa faham ialah liabiliti pada Hari 15.

2. **Sasaran refactor:**

| Sasaran | Cara mencarinya |
|---------|-----------------|
| Action controller > 40 baris | Imbas; alihkan logik ke servis |
| Rentetan ajaib | `grep -n '"Approved"\|"Submitted"' Controllers/` |
| Kaedah pendua dalam modul anda | Imbas berpasangan |
| Nama tidak jelas (`data`, `temp`, `x`) | Namakan semula |
| `using` tidak digunakan | IDE menandakannya |

3. **Betulkan setiap amaran pengkompil:**

```bash
dotnet build 2>&1 | grep -i warning
```

Amaran nullable khususnya — ia menunjuk kepada `NullReferenceException` sebenar.

4. **Tulis dokumentasi modul** untuk kumpulan lain — `docs/kumpulan-1/README-modul.md`:

```markdown
# Modul Lapor Diri (Kumpulan 1)

## Apa yang dilakukannya
<satu perenggan>

## Jadual
- `OfficerReportingApplications` — detail permohonan, satu-ke-satu dengan Submission
- `OfficerReportingAttachments` — melanjutkan Attachment kongsi dengan jenis dokumen

## Laluan
| Laluan | Peranan | Tujuan |
|--------|---------|--------|
| `/OfficerReporting` | Applicant | Permohonan saya |
| `/OfficerReporting/Create` | Applicant | Borang baharu |
| `/OfficerReporting/Dashboard` | HrAdmin | Baris gilir semakan |
| `/OfficerReporting/Review` | HrAdmin | Senarai bertapis |
| `/OfficerReporting/Details/{id}` | HrAdmin | Semak & putuskan |
| `/OfficerReporting/Analytics` | HrAdmin | Statistik |
| `/OfficerReporting/SlipAkuan/{id}` | Applicant (sendiri), HrAdmin | Slip PDF |

## Servis
- `IOfficerReportingAttachmentService` — lampiran khusus modul
- `IHrReviewService` — query dashboard & senarai
- `ISlipAkuanService` — jana PDF
- `SmtpNotificationService` — e-mel (lihat isu #<n> `shared`)

## Aliran status
Draft → Submitted → AdminApproved | Rejected

## Yang perlu diketahui kumpulan lain
- Kami menambah MailKit + QuestPDF ke csproj
- Kami menyumbang pelaksanaan SMTP INotificationService (isu shared #<n>)
- Prefix rujukan: LD

## Diketahui belum siap
- <senarai jujur>
```

### ✅ Semakan

- [ ] Setiap kaedah boleh diterangkan oleh sekurang-kurangnya seorang ahli kumpulan
- [ ] Sifar amaran pengkompil (atau setiap satu dijustifikasi bertulis)
- [ ] Tiada rentetan ajaib untuk status/peranan
- [ ] `README-modul.md` ditulis
- [ ] Semua ujian masih lulus

---

## Latihan 7 — Sedia gabung

**Objektif:** Cabang anda bersedia untuk Hari 15.

### Langkah

1. Segerak akhir dan selesaikan **setiap** konflik hari ini:

```bash
git switch kump-1/lapor-diri
git pull --rebase origin master
dotnet build
dotnet test
```

2. **Latihan gabungan kering** — sahkan cabang anda bergabung bersih:

```bash
git switch master
git pull --rebase origin master
git switch -c ujian/gabungan-kering-k1
git merge kump-1/lapor-diri --no-commit --no-ff
```

Semak konflik. Kemudian batalkan:

```bash
git merge --abort
git switch master
git branch -D ujian/gabungan-kering-k1
```

3. Jika konflik wujud, **selesaikannya dalam cabang anda sekarang** — bukan pada Hari 15.

4. Senarai semak akhir dalam `docs/kumpulan-1/status-akhir.md`:

```markdown
# Status akhir — Kumpulan 1

## Sedia untuk Hari 15
- [x] dotnet build bersih (0 amaran)
- [x] dotnet test — <n> ujian lulus
- [x] Digabung dengan master terkini
- [x] Gabungan kering tiada konflik
- [x] README-modul.md ditulis
- [x] Pemetaan URS → ujian lengkap

## Diketahui belum siap
- <jujur>

## Nota untuk SIT Hari 15
- Akaun ujian: applicant@nres.test / hr@nres.test
- Aliran demo: hantar → semak → luluskan → muat turun slip
- Perlukan MailHog pada port 1025 untuk notifikasi
```

5. PR akhir → review → gabung ke `kump-1/lapor-diri` → push.

### ✅ Semakan

- [ ] `dotnet build` sifar amaran
- [ ] `dotnet test` semua lulus
- [ ] Gabungan kering tiada konflik
- [ ] `status-akhir.md` dan `README-modul.md` lengkap
- [ ] Kerja belum siap dinyatakan jujur
- [ ] Board mencerminkan realiti

---

## Deliverable Hari 13–14

| Artifak | Lokasi |
|---------|--------|
| Ujian nombor rujukan | `Nres.Onboarding.Tests/LaporDiri/` |
| Ujian peralihan status | `Nres.Onboarding.Tests/LaporDiri/` |
| Ujian peraturan modul | `Nres.Onboarding.Tests/LaporDiri/` |
| Pemetaan URS → ujian | `docs/kumpulan-1/pemetaan-ujian.md` |
| Penemuan prestasi | `docs/kumpulan-1/prestasi.md` |
| Dokumentasi modul | `docs/kumpulan-1/README-modul.md` |
| Status akhir | `docs/kumpulan-1/status-akhir.md` |

**Esok (Hari 15):** empat cabang bergabung menjadi satu sistem. Bawa `README-modul.md` anda dan bersedia untuk mendemo.
