# Lab · Kumpulan 1 · Hari 13–14 — Ujian, Refactor & Sedia Gabung

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../../../KOLABORASI.md`](../../../../KOLABORASI.md)
>
> **Tiada ciri baharu dalam blok ini.** Jika anda mendapati diri anda menambah ciri, berhenti.

---

## Latihan 0 — Mula blok & bekukan skop

```bash
git switch kump-1/pentadbiran
git pull --rebase origin master
git switch -c kump-1/feat/kontrak-ujian-dan-refactor
dotnet build
```

**Senaraikan apa yang belum siap.** Berkumpulan, semak backlog Kontrak anda dan tandakan setiap isu:

| Tanda | Maksud | Tindakan |
|-------|--------|----------|
| ✅ | Siap & diuji manual | Tiada |
| 🔧 | Siap tetapi ada pepijat diketahui | Betulkan hari ini |
| ⏸️ | Tidak siap | **Pindah ke backlog, jangan mula** |

Rekod dalam `docs/kumpulan-1/status-akhir-kontrak.md`. Kejujuran di sini lebih berharga daripada kelihatan siap — Hari 15 akan mendedahkannya juga.

### ✅ Semakan

- [ ] Setiap isu backlog ditandakan
- [ ] Kerja belum siap dipindahkan, bukan dimulakan
- [ ] Anda ada senarai pepijat yang perlu dibetulkan hari ini

---

## Latihan 1 — Projek ujian

**Objektif:** `Nres.Onboarding.Tests` dengan pangkalan data SQLite sebenar dalam memori.

> **Koordinasi:** projek ujian dikongsi keempat-empat kumpulan. **Satu** kumpulan menciptanya (jurulatih memutuskan siapa); yang lain menariknya. Kemungkinan besar projek Lapor Diri Kumpulan 1 sudah menciptanya bersama `TestDbFactory` dan `FakeCurrentUser`. **Jangan cipta yang kedua** — semak dahulu.

### Langkah

1. Semak jika projek ujian sudah wujud:

```bash
ls Nres.Onboarding.Tests/TestDbFactory.cs 2>/dev/null && echo "SUDAH ADA — guna ini"
```

2. Jika **belum** wujud (kumpulan anda yang mula), cipta seperti dalam lab Lapor Diri Hari 13–14 (`TestDbFactory` + `FakeCurrentUser`). Jika **sudah** wujud, hanya tambah folder anda:

```bash
mkdir -p Nres.Onboarding.Tests/Kontrak
```

> `TestDbFactory` menggunakan **SQLite in-memory sebenar**, bukan penyedia InMemory — supaya indeks unik ditapis pada `ContractNo` dan indeks komposit milestone benar-benar dikuatkuasakan dalam ujian.

### ✅ Semakan

- [ ] Projek ujian wujud dan dibina (tanpa pendua)
- [ ] `TestDbFactory` menggunakan SQLite, **bukan** penyedia InMemory
- [ ] Folder `Nres.Onboarding.Tests/Kontrak/` wujud
- [ ] `dotnet test` berjalan

---

## Latihan 2 — Uji nombor rujukan

**Objektif:** Peraturan rekod rasmi Kontrak betul.

### Langkah

`Nres.Onboarding.Tests/Kontrak/ReferenceNumberTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Tests.Kontrak;

public class ReferenceNumberTests
{
    [Fact]
    public async Task Rujukan_pertama_bermula_pada_0001()
    {
        using var f = new TestDbFactory();
        var servis = new ReferenceNumberService(f.Db);

        var rujukan = await servis.GenerateAsync(ModuleCodes.PengurusanKontrak);

        rujukan.Should().Be($"KON-{DateTime.UtcNow.Year}-0001");
    }

    [Fact]
    public async Task Rujukan_bertambah_bagi_setiap_penghantaran()
    {
        using var f = new TestDbFactory();
        var servis = new ReferenceNumberService(f.Db);

        f.Db.Submissions.Add(new Submission
        {
            ModuleCode = ModuleCodes.PengurusanKontrak,
            ApplicantUserId = "u1",
            ReferenceNo = $"KON-{DateTime.UtcNow.Year}-0001",
            Status = SubmissionStatus.Submitted
        });
        await f.Db.SaveChangesAsync();

        var rujukan = await servis.GenerateAsync(ModuleCodes.PengurusanKontrak);

        rujukan.Should().Be($"KON-{DateTime.UtcNow.Year}-0002");
    }

    [Fact]
    public async Task Draf_tanpa_rujukan_tidak_menambah_kiraan()
    {
        using var f = new TestDbFactory();
        var servis = new ReferenceNumberService(f.Db);

        for (var i = 0; i < 3; i++)
        {
            f.Db.Submissions.Add(new Submission
            {
                ModuleCode = ModuleCodes.PengurusanKontrak,
                ApplicantUserId = $"u{i}",
                Status = SubmissionStatus.Draft
            });
        }
        await f.Db.SaveChangesAsync();

        var rujukan = await servis.GenerateAsync(ModuleCodes.PengurusanKontrak);

        // Draf tidak mengambil nombor — pendaftaran sebenar pertama ialah 0001.
        rujukan.Should().Be($"KON-{DateTime.UtcNow.Year}-0001");
    }
}
```

### ✅ Semakan

- [ ] Ketiga-tiga ujian lulus
- [ ] Ujian "draf tidak mengambil nombor" lulus — mengesahkan reka bentuk Hari 5–6
- [ ] Prefix `KON` betul

---

## Latihan 3 — Uji peraturan peralihan status

**Objektif:** Kelulusan tidak sah adalah mustahil.

### Langkah

`Nres.Onboarding.Tests/Kontrak/WorkflowTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Tests.Kontrak;

public class WorkflowTests
{
    private static WorkflowService Buat(TestDbFactory f) =>
        new(f.Db, new AuditLogService(f.Db, new FakeCurrentUser("ictadmin1")));

    [Theory]
    [InlineData(SubmissionStatus.Draft,     SubmissionStatus.Submitted,     true)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.AdminApproved, true)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Rejected,      true)]
    [InlineData(SubmissionStatus.Draft,     SubmissionStatus.AdminApproved, false)]
    [InlineData(SubmissionStatus.Rejected,  SubmissionStatus.AdminApproved, false)]
    [InlineData(SubmissionStatus.AdminApproved, SubmissionStatus.Rejected,  false)]
    public void Peralihan_status_mengikut_peraturan(
        SubmissionStatus dari, SubmissionStatus ke, bool dibenarkan)
    {
        using var f = new TestDbFactory();
        Buat(f).CanTransition(dari, ke).Should().Be(dibenarkan);
    }

    [Fact]
    public async Task Peralihan_menulis_audit_log()
    {
        using var f = new TestDbFactory();
        await f.SeedUserAsync("ictadmin1", "ictadmin@nres.test");

        var submission = new Submission
        {
            ModuleCode = ModuleCodes.PengurusanKontrak,
            ApplicantUserId = "u1",
            ReferenceNo = "KON-2026-0001",
            Status = SubmissionStatus.Submitted
        };
        f.Db.Submissions.Add(submission);
        await f.Db.SaveChangesAsync();

        await Buat(f).TransitionAsync(submission,
            SubmissionStatus.Rejected, "Rejected", "Nilai melebihi had kelulusan");

        var log = f.Db.AuditLogs.Single();
        log.Action.Should().Be("Rejected");
        log.FromStatus.Should().Be(SubmissionStatus.Submitted);
        log.ToStatus.Should().Be(SubmissionStatus.Rejected);
        log.Remarks.Should().Be("Nilai melebihi had kelulusan");
        log.ActorUserId.Should().Be("ictadmin1");
    }
}
```

> **Perhatikan `FakeCurrentUser`** — ganti mudah untuk `ICurrentUserService`, dicipta bersama `TestDbFactory` oleh kumpulan pertama. Jika ia belum wujud, lihat lab Lapor Diri Hari 13–14 Latihan 3.

### ✅ Semakan

- [ ] Kesemua 6 kes `[Theory]` lulus
- [ ] Peralihan tidak sah dihalang
- [ ] Audit log ditulis dengan status lama/baharu dan catatan

---

## Latihan 4 — Uji peraturan unik Kontrak

**Objektif:** Tiga peraturan yang paling mudah pecah semasa refactor.

### Langkah

1. `Nres.Onboarding.Tests/Kontrak/ContractLifecycleTests.cs` — fungsi tulen, mudah diuji:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Kontrak;

namespace Nres.Onboarding.Tests.Kontrak;

public class ContractLifecycleTests
{
    private static readonly DateTime HariIni = new(2026, 6, 1);

    [Fact]
    public void Kontrak_ditamatkan_sentiasa_Terminated()
    {
        // IsTerminated mengatasi tarikh — walaupun tarikh tamat masih jauh.
        var state = ContractLifecycleInfo.Kira(
            isTerminated: true, expiryDate: new DateTime(2027, 12, 31), today: HariIni);

        state.Should().Be(ContractLifecycleState.Terminated);
    }

    [Fact]
    public void Kontrak_tamat_semalam_adalah_Expired()
    {
        var state = ContractLifecycleInfo.Kira(
            isTerminated: false, expiryDate: new DateTime(2026, 5, 31), today: HariIni);

        state.Should().Be(ContractLifecycleState.Expired);
    }

    [Fact]
    public void Kontrak_tamat_dalam_ambang_adalah_ExpiringSoon()
    {
        // Ambang lalai 60 hari; tamat 30 hari lagi.
        var state = ContractLifecycleInfo.Kira(
            isTerminated: false, expiryDate: HariIni.AddDays(30), today: HariIni);

        state.Should().Be(ContractLifecycleState.ExpiringSoon);
    }

    [Fact]
    public void Kontrak_tamat_jauh_adalah_Active()
    {
        var state = ContractLifecycleInfo.Kira(
            isTerminated: false, expiryDate: HariIni.AddDays(200), today: HariIni);

        state.Should().Be(ContractLifecycleState.Active);
    }
}
```

2. `Nres.Onboarding.Tests/Kontrak/MilestoneTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Models.Kontrak;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services.Kontrak;
using Nres.Onboarding.Web.ViewModels.Kontrak;

namespace Nres.Onboarding.Tests.Kontrak;

public class MilestoneTests
{
    /// <summary>Cipta satu kontrak draf & pulangkan ContractRecordId.</summary>
    private static async Task<int> SeedKontrakAsync(TestDbFactory f, decimal amount)
    {
        var submission = new Submission
        {
            ModuleCode = ModuleCodes.PengurusanKontrak,
            ApplicantUserId = "u1",
            Status = SubmissionStatus.Draft
        };
        f.Db.Submissions.Add(submission);
        await f.Db.SaveChangesAsync();

        var kontrak = new ContractRecord
        {
            SubmissionId = submission.Id,
            ContractNo = "CT250000000029728",
            FileNo = "NRES.400-5/6/40(S)-7",
            Title = "Perkhidmatan Storan & Sandaran",
            Amount = amount,
            EffectiveDate = new DateTime(2026, 1, 1),
            ExpiryDate = new DateTime(2027, 12, 31),
            Division = "Bahagian Pengurusan Maklumat"
        };
        f.Db.Set<ContractRecord>().Add(kontrak);
        await f.Db.SaveChangesAsync();
        return kontrak.Id;
    }

    [Fact]
    public async Task Milestone_mesti_ikut_turutan_status()
    {
        using var f = new TestDbFactory();
        var id = await SeedKontrakAsync(f, 120_000m);
        var svc = new ContractDetailService(f.Db);

        await svc.AddMilestoneAsync(id, new ContractMilestoneInput
        {
            PaymentNo = 1, Description = "Bayaran pendahuluan",
            Amount = 120_000m, DueDate = new DateTime(2026, 3, 1)
        });

        var m = (await svc.MilestonesAsync(id)).Single();

        // Tidak boleh melompat terus ke Paid.
        var act = async () => await svc.MarkPaidAsync(m.Id);
        await act.Should().ThrowAsync<InvalidOperationException>();

        // Turutan sah: Received dahulu, kemudian Paid.
        await svc.MarkDeliverablesReceivedAsync(m.Id);
        await svc.MarkPaidAsync(m.Id);

        var selepas = await f.Db.Set<ContractMilestone>().FindAsync(m.Id);
        selepas!.Status.Should().Be(MilestoneStatus.Paid);
        selepas.PaidAt.Should().NotBeNull();
    }

    [Fact]
    public async Task No_bayaran_pendua_dalam_kontrak_dihalang()
    {
        using var f = new TestDbFactory();
        var id = await SeedKontrakAsync(f, 100_000m);
        var svc = new ContractDetailService(f.Db);

        await svc.AddMilestoneAsync(id, new ContractMilestoneInput
        {
            PaymentNo = 1, Description = "Bayaran 1",
            Amount = 50_000m, DueDate = new DateTime(2026, 3, 1)
        });

        // Indeks unik komposit (ContractRecordId, PaymentNo) menghalang pendua.
        var act = async () => await svc.AddMilestoneAsync(id, new ContractMilestoneInput
        {
            PaymentNo = 1, Description = "Bayaran 1 (pendua)",
            Amount = 50_000m, DueDate = new DateTime(2026, 4, 1)
        });

        await act.Should().ThrowAsync<DbUpdateException>();
    }

    [Fact]
    public async Task Jumlah_milestone_dikira_betul()
    {
        using var f = new TestDbFactory();
        var id = await SeedKontrakAsync(f, 120_000m);
        var svc = new ContractDetailService(f.Db);

        foreach (var (no, amt) in new[] { (1, 40_000m), (2, 40_000m), (3, 40_000m) })
            await svc.AddMilestoneAsync(id, new ContractMilestoneInput
            {
                PaymentNo = no, Description = $"Bayaran {no}",
                Amount = amt, DueDate = new DateTime(2026, no + 2, 1)
            });

        var jumlah = (await svc.MilestonesAsync(id)).Sum(m => m.Amount);

        // Peraturan hantar Hari 5–6: jumlah milestone mesti = amaun kontrak.
        jumlah.Should().Be(120_000m);
    }
}
```

**Pemetaan URS → ujian.** Cipta `docs/kumpulan-1/pemetaan-ujian-kontrak.md`:

```markdown
# Pemetaan URS → Ujian — Kumpulan 1 · Kontrak

| ID URS | Keperluan | Ujian | Status |
|--------|-----------|-------|--------|
| URS-KON-001 | Simpan draf tidak lengkap | (manual) | ✅ |
| URS-KON-003 | No. rujukan unik KON-YYYY-NNNN | `ReferenceNumberTests` | ✅ |
| URS-KON-004 | Jumlah milestone = nilai kontrak | `Jumlah_milestone_dikira_betul` | ✅ |
| URS-KON-005 | Status kitaran hayat dikira | `ContractLifecycleTests` | ✅ |
| URS-KON-006 | Milestone ikut turutan status | `Milestone_mesti_ikut_turutan_status` | ✅ |
| URS-KON-008 | Peringatan tamat tempoh | (manual) | ✅ |
```

### ✅ Semakan

- [ ] Ujian kitaran hayat lulus (Terminated mengatasi tarikh)
- [ ] Ujian turutan milestone lulus (tidak boleh melompat ke Paid)
- [ ] Ujian no. bayaran pendua lulus (`DbUpdateException` — mengesahkan indeks komposit)
- [ ] Ujian jumlah milestone lulus
- [ ] Setiap keperluan URS "Mesti ada" ada baris dalam jadual pemetaan
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

2. Jana data ujian — cipta **200 kontrak** dengan pihak & milestone (kaedah seed sementara).

3. Jalankan setiap skrin dan **baca SQL dalam konsol**:

| Skrin | Cari |
|-------|------|
| `/Contract/Index` | Satu `SELECT`? |
| `/Contract/Dashboard` | Kiraan + `SUM`, tiada `SELECT *`? |
| `/Contract/Review` | Satu `COUNT` + satu `SELECT` berhalaman? |
| `/Contract/Details/1` | Query `Include` munasabah (pihak + milestone)? |
| `/Contract/Analytics` | `GROUP BY` dalam SQL, bukan dalam C#? |

4. Rekod penemuan dalam `docs/kumpulan-1/prestasi-kontrak.md`.

5. Betulkan apa yang anda temui. Ujian anda membuktikan tingkah laku tidak berubah.

### ✅ Semakan

- [ ] Kelima-lima skrin diperiksa dengan logging SQL dihidupkan
- [ ] Sebarang N+1 (biasanya `Division`/`Parties`/`Milestones`) ditemui dan dibetulkan
- [ ] `AsNoTracking()` pada setiap query baca-sahaja
- [ ] Penemuan didokumenkan
- [ ] Semua ujian masih lulus selepas pembetulan
- [ ] Logging dimatikan semula sebelum commit

---

## Latihan 6 — Refactor & pembersihan

**Objektif:** Kod yang orang lain boleh baca pada Hari 15.

### Langkah

1. **Semakan kod jana-AI.** Berkumpulan, imbas setiap fail dan tanya bagi setiap kaedah: *bolehkah seseorang di sini menerangkan ini?* Jika tidak: fahami, atau buang.

2. **Sasaran refactor:**

| Sasaran | Cara mencarinya |
|---------|-----------------|
| Action controller > 40 baris | Imbas; alihkan logik ke servis |
| Rentetan ajaib | `grep -n '"IctAdmin"\|"Submitted"' Controllers/` |
| Nama tidak jelas (`data`, `temp`, `x`) | Namakan semula |
| `using` tidak digunakan | IDE menandakannya |

3. **Betulkan setiap amaran pengkompil:**

```bash
dotnet build 2>&1 | grep -i warning
```

Amaran nullable & `decimal` khususnya.

4. **Tulis dokumentasi modul** — `docs/kumpulan-1/README-modul-kontrak.md`:

```markdown
# Modul Pengurusan Kontrak (Kumpulan 1)

## Apa yang dilakukannya
Daftar & jejak kontrak/perjanjian ICT — pihak terlibat, milestone bayaran,
status kitaran hayat, dan peringatan tamat tempoh.

## Jadual
- `ContractRecords` — header kontrak, satu-ke-satu dengan Submission
- `ContractParties` — syarikat terlibat (satu-ke-banyak)
- `ContractMilestones` — jadual bayaran/penyerahan (satu-ke-banyak)
- `ContractAttachments` — melanjutkan Attachment kongsi

## Laluan
| Laluan | Peranan | Tujuan |
|--------|---------|--------|
| `/Contract` | Applicant | Kontrak saya |
| `/Contract/Create` | Applicant | Daftar baharu |
| `/Contract/Dashboard` | IctAdmin | Baris gilir semakan |
| `/Contract/Review` | IctAdmin | Senarai bertapis |
| `/Contract/Details/{id}` | IctAdmin | Semak, putuskan, jejak milestone |
| `/Contract/Analytics` | IctAdmin | Nilai & taburan |
| `/Contract/Report/{id}` | Applicant (sendiri), IctAdmin | Laporan PDF |

## Servis
- `IContractDetailService` — pihak, milestone, penjejakan bayaran
- `IContractAttachmentService` — lampiran khusus modul
- `IContractReviewService` — query dashboard, senarai, analitik
- `IContractExpiryService` — peringatan tamat tempoh
- `IContractReportService` — jana PDF

## Aliran status
Submission: Draft → Submitted → AdminApproved | Rejected
Kitaran hayat (dikira): Active → ExpiringSoon → Expired | Terminated
Milestone: Pending → DeliverablesReceived → Paid

## Yang perlu diketahui kumpulan lain
- Kami menambah QuestPDF ke csproj (jika Lapor Diri belum)
- Kami guna INotificationService kongsi (SMTP dari isu shared jika ada)
- Prefix rujukan: KON

## Diketahui belum siap
- <senarai jujur>
```

### ✅ Semakan

- [ ] Setiap kaedah boleh diterangkan oleh sekurang-kurangnya seorang ahli kumpulan
- [ ] Sifar amaran pengkompil (atau setiap satu dijustifikasi bertulis)
- [ ] Tiada rentetan ajaib untuk status/peranan
- [ ] `README-modul-kontrak.md` ditulis
- [ ] Semua ujian masih lulus

---

## Latihan 7 — Sedia gabung

**Objektif:** Cabang anda bersedia untuk Hari 15.

### Langkah

1. Segerak akhir dan selesaikan **setiap** konflik hari ini:

```bash
git switch kump-1/pentadbiran
git pull --rebase origin master
dotnet build
dotnet test
```

2. **Latihan gabungan kering** — sahkan cabang anda bergabung bersih:

```bash
git switch master
git pull --rebase origin master
git switch -c ujian/gabungan-kering-k1-kontrak
git merge kump-1/pentadbiran --no-commit --no-ff
```

Semak konflik. Kemudian batalkan:

```bash
git merge --abort
git switch master
git branch -D ujian/gabungan-kering-k1-kontrak
```

3. Jika konflik wujud, **selesaikannya dalam cabang anda sekarang** — bukan pada Hari 15.

4. Senarai semak akhir dalam `docs/kumpulan-1/status-akhir-kontrak.md`:

```markdown
# Status akhir — Kumpulan 1 · Kontrak

## Sedia untuk Hari 15
- [x] dotnet build bersih (0 amaran)
- [x] dotnet test — <n> ujian lulus
- [x] Digabung dengan master terkini
- [x] Gabungan kering tiada konflik
- [x] README-modul-kontrak.md ditulis
- [x] Pemetaan URS → ujian lengkap

## Diketahui belum siap
- <jujur>

## Nota untuk SIT Hari 15
- Akaun ujian: applicant@nres.test / ictadmin@nres.test
- Aliran demo: daftar → tambah pihak/milestone → hantar → luluskan → jejak bayaran → laporan PDF
- Perlukan MailHog pada port 1025 untuk notifikasi (jika SMTP)
```

5. PR akhir → review → gabung ke `kump-1/pentadbiran` → push.

### ✅ Semakan

- [ ] `dotnet build` sifar amaran
- [ ] `dotnet test` semua lulus
- [ ] Gabungan kering tiada konflik
- [ ] `status-akhir-kontrak.md` dan `README-modul-kontrak.md` lengkap
- [ ] Kerja belum siap dinyatakan jujur
- [ ] Board mencerminkan realiti

---

## Deliverable Hari 13–14

| Artifak | Lokasi |
|---------|--------|
| Ujian nombor rujukan | `Nres.Onboarding.Tests/Kontrak/` |
| Ujian peralihan status | `Nres.Onboarding.Tests/Kontrak/` |
| Ujian kitaran hayat & milestone | `Nres.Onboarding.Tests/Kontrak/` |
| Pemetaan URS → ujian | `docs/kumpulan-1/pemetaan-ujian-kontrak.md` |
| Penemuan prestasi | `docs/kumpulan-1/prestasi-kontrak.md` |
| Dokumentasi modul | `docs/kumpulan-1/README-modul-kontrak.md` |
| Status akhir | `docs/kumpulan-1/status-akhir-kontrak.md` |

## Latihan 8 — Pemecut QA dipandu-AI (Claude Code)

**Objektif:** Guna subagent `qa-uat` + skill `/uji-modul` untuk mempercepat ujian blok ini, dan pratonton pre-check pelayar Hari 15. Lab penuh: [`lab-qa-ai-uat.md`](../../../../docs/lab-qa-ai-uat.md).

> 🔧 **Khusus Claude Code.** Guna alat AI lain? Langkau — ujian manual di atas sudah memadai.

### Langkah

1. Cipta (sekali) `.claude/agents/qa-uat.md` + `.claude/skills/uji-modul/SKILL.md` (salin templat kursus [`.claude/agents/`](../../../../.claude/agents/)).
2. **Lengkapkan ujian** dengan `qa-uat` (UJI-01):

   ```text
   Guna subagent qa-uat: lengkapkan ujian xUnit — peralihan status kontrak,
   nombor rujukan KON berjujukan, dan peraturan milestone. Guna SQLite in-memory
   (bukan UseInMemoryDatabase). Jalankan dotnet test, tunjuk keputusan.
   ```

3. **Pratonton pre-check pelayar** (penuh Hari 15) — jalankan app repo `pengurusan-kontrak` anda:

   ```text
   Guna subagent qa-uat: pre-check di app pengurusan-kontrak — daftar kontrak,
   log masuk IctAdmin → luluskan → jejak milestone. Sahkan peranan salah ke halaman
   semakan mesti 403. Rekod lulus/gagal. Jangan cetuskan dialog.
   ```

### ✅ Semakan

- [ ] `qa-uat` + `/uji-modul` wujud; `dotnet test` hijau
- [ ] Peralihan status, nombor rujukan KON, peraturan milestone dilindungi
- [ ] RBAC 403 disahkan; keputusan direkod (ini **pre-check**, bukan UAT sebenar)

---

**Esok (Hari 15):** empat cabang bergabung menjadi satu sistem. Bawa `README-modul-kontrak.md` anda dan bersedia untuk mendemo — daftar kontrak, luluskan, jejak milestone, jana laporan PDF.
