# Lab · Kumpulan 1 · Hari 13–14 — Ujian, Refactor & Sedia Gabung

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../../../KOLABORASI.md`](../../../../KOLABORASI.md)
>
> **Tiada ciri baharu dalam blok ini.** Jika anda mendapati diri anda menambah ciri, berhenti.

---

## Latihan 0 — Mula blok & bekukan skop

```bash
git switch kump-1/pentadbiran
git pull --rebase origin master
git switch -c kump-1/feat/pks-ujian-dan-refactor
dotnet build
```

**Senaraikan apa yang belum siap.** Berkumpulan, semak backlog PKS anda:

| Tanda | Maksud | Tindakan |
|-------|--------|----------|
| ✅ | Siap & diuji manual | Tiada |
| 🔧 | Siap tetapi ada pepijat diketahui | Betulkan hari ini |
| ⏸️ | Tidak siap | **Pindah ke backlog, jangan mula** |

Rekod dalam `docs/kumpulan-1/status-akhir-pks.md`.

### ✅ Semakan

- [ ] Setiap isu backlog ditandakan
- [ ] Kerja belum siap dipindahkan, bukan dimulakan
- [ ] Anda ada senarai pepijat yang perlu dibetulkan hari ini

---

## Latihan 1 — Projek ujian

**Objektif:** `Nres.Onboarding.Tests` dengan SQLite in-memory. Kongsi keempat-empat kumpulan.

> **Koordinasi:** projek ujian dikongsi. **Satu** kumpulan menciptanya (jurulatih memutuskan siapa; mungkin sudah wujud dari blok Lapor Diri); yang lain menariknya. Setiap kumpulan menambah **failnya sendiri** di bawah `Tests/<Modul>/`.

### Langkah

1. Jika projek belum wujud (jika sudah, langkau ke langkah 2):

```bash
dotnet new xunit -o Nres.Onboarding.Tests
dotnet sln add Nres.Onboarding.Tests
cd Nres.Onboarding.Tests
dotnet add reference ../Nres.Onboarding.Web
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package FluentAssertions
cd ..
```

2. Cipta folder anda:

```bash
mkdir -p Nres.Onboarding.Tests/Pks
```

3. `TestDbFactory` ialah **fail kongsi** — jika Lapor Diri sudah menciptanya, **guna semula**, jangan tulis versi PKS. Jika belum wujud, ini bentuknya:

```csharp
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;

namespace Nres.Onboarding.Tests;

/// <summary>
/// SQLite in-memory — pangkalan data SQL SEBENAR dengan kekangan sebenar.
/// Bukan penyedia InMemory: ia tidak menguatkuasakan indeks unik ditapis kami
/// pada IsCurrent, jadi ujian akan lulus sementara pengeluaran gagal.
/// </summary>
public sealed class TestDbFactory : IDisposable
{
    private readonly SqliteConnection _connection;
    public ApplicationDbContext Db { get; }

    public TestDbFactory()
    {
        _connection = new SqliteConnection("DataSource=:memory:");
        _connection.Open();   // mesti kekal terbuka — menutupnya memusnahkan DB

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection).Options;

        Db = new ApplicationDbContext(options);
        Db.Database.EnsureCreated();
    }

    public void Dispose()
    {
        Db.Dispose();
        _connection.Dispose();
    }
}
```

> `EnsureCreated()` menggunakan seed `HasData` — jadi setiap `TestDbFactory` bermula dengan **satu** `PolicyVersion` semasa (Id 1). Ujian anda bergantung pada ini.

### ✅ Semakan

- [ ] Projek ujian wujud dan dibina
- [ ] `TestDbFactory` menggunakan SQLite, **bukan** penyedia InMemory
- [ ] Folder `Tests/Pks/` wujud
- [ ] `dotnet test` berjalan (walaupun sifar ujian)

---

## Latihan 2 — Uji nombor rujukan PKS

**Objektif:** Prefix `PKS` dan jujukan betul.

### Langkah

`Nres.Onboarding.Tests/Pks/ReferenceNumberTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;

namespace Nres.Onboarding.Tests.Pks;

public class ReferenceNumberTests
{
    [Fact]
    public async Task Rujukan_PKS_pertama_bermula_pada_0001()
    {
        using var f = new TestDbFactory();
        var servis = new ReferenceNumberService(f.Db);

        var rujukan = await servis.GenerateAsync(ModuleCodes.PematuhanPks);

        rujukan.Should().Be($"PKS-{DateTime.UtcNow.Year}-0001");
    }

    [Fact]
    public async Task Rujukan_PKS_bertambah_bagi_setiap_penghantaran()
    {
        using var f = new TestDbFactory();
        var servis = new ReferenceNumberService(f.Db);

        f.Db.Submissions.Add(new Submission
        {
            ModuleCode = ModuleCodes.PematuhanPks,
            ApplicantUserId = "u1",
            ReferenceNo = $"PKS-{DateTime.UtcNow.Year}-0001",
            Status = SubmissionStatus.Submitted
        });
        await f.Db.SaveChangesAsync();

        var rujukan = await servis.GenerateAsync(ModuleCodes.PematuhanPks);

        rujukan.Should().Be($"PKS-{DateTime.UtcNow.Year}-0002");
    }

    [Fact]
    public async Task Modul_lain_tidak_menjejaskan_jujukan_PKS()
    {
        using var f = new TestDbFactory();
        var servis = new ReferenceNumberService(f.Db);

        // Permohonan Lapor Diri Kumpulan 1 tidak menjejaskan jujukan PKS.
        f.Db.Submissions.Add(new Submission
        {
            ModuleCode = ModuleCodes.LaporDiri,
            ApplicantUserId = "u1",
            ReferenceNo = $"LD-{DateTime.UtcNow.Year}-0001",
            Status = SubmissionStatus.Submitted
        });
        await f.Db.SaveChangesAsync();

        var rujukan = await servis.GenerateAsync(ModuleCodes.PematuhanPks);

        rujukan.Should().Be($"PKS-{DateTime.UtcNow.Year}-0001");
    }
}
```

### ✅ Semakan

- [ ] Ketiga-tiga ujian lulus
- [ ] Ujian pengasingan modul lulus (LD tidak menjejaskan PKS)

---

## Latihan 3 — Uji status pematuhan (peraturan unik PKS)

**Objektif:** Buktikan "patuh vs perlu akui semula" dikira dengan betul.

### Langkah

`Nres.Onboarding.Tests/Pks/ComplianceStateTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Pks;

namespace Nres.Onboarding.Tests.Pks;

public class ComplianceStateTests
{
    [Fact]
    public void Akuan_terhadap_versi_semasa_adalah_patuh()
    {
        ComplianceStateInfo.Kira(acknowledgedVersionId: 3, currentVersionId: 3)
            .Should().Be(ComplianceState.Compliant);
    }

    [Fact]
    public void Akuan_terhadap_versi_lama_perlu_akui_semula()
    {
        ComplianceStateInfo.Kira(acknowledgedVersionId: 2, currentVersionId: 3)
            .Should().Be(ComplianceState.NeedsReacknowledgement);
    }

    [Theory]
    [InlineData(ComplianceState.Compliant, "Patuh")]
    [InlineData(ComplianceState.NeedsReacknowledgement, "Perlu akui semula")]
    public void Nama_status_dalam_Bahasa_Melayu(ComplianceState state, string dijangka)
    {
        ComplianceStateInfo.Nama(state).Should().Be(dijangka);
    }
}
```

### ✅ Semakan

- [ ] Semua ujian status pematuhan lulus
- [ ] Ujian mengesahkan reka bentuk "status dikira" Hari 7–9

---

## Latihan 4 — Uji penerbitan versi polisi

**Objektif:** Buktikan penerbitan atomik menaikkan tepat satu versi semasa dan tidak melanggar indeks unik.

### Langkah

`Nres.Onboarding.Tests/Pks/PolicyVersionTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Models.Pks;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.Services.Pks;

namespace Nres.Onboarding.Tests.Pks;

public class PolicyVersionTests
{
    // Notifikasi tidak berkaitan di sini — guna ganti yang tidak berbuat apa-apa.
    private sealed class NullNotifications : INotificationService
    {
        public Task NotifyAsync(string toUserId, string subject, string body,
            CancellationToken ct = default) => Task.CompletedTask;
    }

    private static PolicyVersionService Buat(TestDbFactory f) =>
        new(f.Db, new NullNotifications());

    [Fact]
    public async Task Seed_memberi_satu_versi_semasa()
    {
        using var f = new TestDbFactory();

        var semasa = await Buat(f).CurrentAsync();

        semasa.VersionLabel.Should().Be("PKS-2026 v1.0");
        semasa.IsCurrent.Should().BeTrue();
    }

    [Fact]
    public async Task Terbit_versi_baharu_menaikkan_tepat_satu_semasa()
    {
        using var f = new TestDbFactory();

        await Buat(f).PublishAsync("PKS-2026 v2.0", "Polisi Dikemas Kini",
            "Kemas kini kawalan akses", new DateTime(2026, 6, 1));

        var semua = await f.Db.Set<PolicyVersion>().ToListAsync();
        semua.Count(p => p.IsCurrent).Should().Be(1);
        semua.Single(p => p.IsCurrent).VersionLabel.Should().Be("PKS-2026 v2.0");
    }

    [Fact]
    public async Task Versi_lama_diturunkan_selepas_terbit()
    {
        using var f = new TestDbFactory();

        await Buat(f).PublishAsync("PKS-2026 v2.0", "Polisi Dikemas Kini",
            null, new DateTime(2026, 6, 1));

        var lama = await f.Db.Set<PolicyVersion>()
            .FirstAsync(p => p.VersionLabel == "PKS-2026 v1.0");
        lama.IsCurrent.Should().BeFalse();
    }

    [Fact]
    public async Task Terbit_label_yang_sudah_semasa_dilontar()
    {
        using var f = new TestDbFactory();

        var act = async () => await Buat(f).PublishAsync(
            "PKS-2026 v1.0", "Sama", null, new DateTime(2026, 6, 1));

        await act.Should().ThrowAsync<InvalidOperationException>();
    }
}
```

> **Ujian kedua ialah yang paling penting:** `Count(p => p.IsCurrent).Should().Be(1)`. Jika seseorang memecahkan urutan transaksi (sisip sebelum turunkan), indeks unik ditapis melontar dan ujian ini menjadi merah — tepat seperti yang kita mahu.

### ✅ Semakan

- [ ] Semua ujian penerbitan lulus
- [ ] Ujian "tepat satu semasa" lulus — mengesahkan atomik + indeks unik ditapis
- [ ] Menerbitkan label yang sudah semasa dilontar

---

## Latihan 5 — Uji validation bersyarat kontraktor

**Objektif:** Kontraktor mesti mengisi maklumat syarikat; staf tidak.

### Langkah

`Nres.Onboarding.Tests/Pks/ContractorValidationTests.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Nres.Onboarding.Web.Models.Pks;
using Nres.Onboarding.Web.ViewModels.Pks;

namespace Nres.Onboarding.Tests.Pks;

public class ContractorValidationTests
{
    private static IReadOnlyList<ValidationResult> Validate(ComplianceFormViewModel vm)
    {
        var hasil = new List<ValidationResult>();
        Validator.TryValidateObject(vm, new ValidationContext(vm), hasil,
            validateAllProperties: true);
        return hasil;
    }

    [Fact]
    public void Kontraktor_tanpa_maklumat_syarikat_gagal()
    {
        var vm = new ComplianceFormViewModel
        {
            DeclarantType = PksDeclarantType.Contractor,
            FullName = "Ahmad Sintetik", IcNo = "800101-14-5566",
            Position = "Juruperunding", Division = "ICT"
            // CompanyName & CompanyRegNo sengaja dikosongkan
        };

        var hasil = Validate(vm);

        hasil.Should().Contain(r => r.MemberNames.Contains(nameof(vm.CompanyName)));
        hasil.Should().Contain(r => r.MemberNames.Contains(nameof(vm.CompanyRegNo)));
    }

    [Fact]
    public void Staf_tanpa_maklumat_syarikat_lulus()
    {
        var vm = new ComplianceFormViewModel
        {
            DeclarantType = PksDeclarantType.Staff,
            FullName = "Siti Sintetik", IcNo = "900202-10-3344",
            Position = "Pegawai Teknologi Maklumat", Division = "BPM"
        };

        Validate(vm).Should().BeEmpty();
    }
}
```

**Pemetaan URS → ujian.** Cipta `docs/kumpulan-1/pemetaan-ujian-pks.md`:

```markdown
# Pemetaan URS → Ujian — Kumpulan 1 · PKS

| ID URS | Keperluan | Ujian | Status |
|--------|-----------|-------|--------|
| URS-PKS-001 | Simpan draf tidak lengkap | (manual) | ✅ |
| URS-PKS-002 | Varian staf & kontraktor | `ContractorValidationTests` | ✅ |
| URS-PKS-003 | Akuan dikait versi polisi | `ComplianceStateTests` | ✅ |
| URS-PKS-004 | Patuh vs perlu akui semula | `ComplianceStateTests` | ✅ |
| URS-PKS-005 | Terbit versi → satu semasa | `PolicyVersionTests` | ✅ |
| URS-PKS-006 | No. rujukan PKS-YYYY-NNNN | `ReferenceNumberTests` | ✅ |
| URS-PKS-007 | NDA wajib pada hantar | (manual) | ✅ |
| URS-PKS-008 | Sijil PDF hanya bila diluluskan | (manual) | ✅ |
```

### ✅ Semakan

- [ ] Ujian validation kontraktor lulus
- [ ] Setiap keperluan URS "Mesti ada" ada baris dalam jadual pemetaan
- [ ] Keperluan diuji manual ditandakan jujur sebagai manual
- [ ] `dotnet test` — semua hijau

---

## Latihan 6 — Optimasi query

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

2. Jana ~200 akuan (kaedah seed sementara atau skrip), campuran staf/kontraktor terhadap dua versi polisi.

3. Jalankan setiap skrin dan **baca SQL dalam konsol**:

| Skrin | Cari |
|-------|------|
| `/Compliance` (senarai saya) | Satu `SELECT`? |
| `/Compliance/Dashboard` | Kiraan sebagai `COUNT`, tiada `SELECT *`? |
| `/Compliance/Review` | Satu `COUNT` + satu `SELECT` berhalaman? Ada N+1 pada `PolicyVersion`? |
| `/Compliance/Details/1` | Query `Include` munasabah? |
| `/Compliance/Analytics` | `GROUP BY` dalam SQL, bukan dalam C#? |

4. Rekod dalam `docs/kumpulan-1/prestasi-pks.md`:

```markdown
# Semakan prestasi — Kumpulan 1 · PKS

| Skrin | Query sebelum | Query selepas | Pembetulan |
|-------|---------------|---------------|------------|
| Review | 21 (N+1 pada PolicyVersion) | 2 | Project VersionLabel dalam Select |
| Dashboard | 5 | 5 | Tiada perubahan |
| Analytics | 3 | 3 | GroupBy sudah di DB |
```

5. Betulkan apa yang anda temui. Ujian anda membuktikan tingkah laku tidak berubah.

### ✅ Semakan

- [ ] Kelima-lima skrin diperiksa dengan logging SQL
- [ ] Sebarang N+1 ditemui dan dibetulkan
- [ ] `AsNoTracking()` pada setiap query baca-sahaja
- [ ] Semua ujian masih lulus selepas pembetulan
- [ ] Logging dimatikan semula sebelum commit

---

## Latihan 7 — Refactor, dokumentasi & sedia gabung

**Objektif:** Kod yang orang lain boleh baca pada Hari 15, dan cabang yang bergabung bersih.

### Langkah

1. **Semakan kod jana-AI.** Imbas setiap fail; bagi setiap kaedah tanya: *bolehkah seseorang di sini menerangkannya?* Jika tidak: fahami atau buang.

2. **Sasaran refactor:**

| Sasaran | Cara mencarinya |
|---------|-----------------|
| Action controller > 40 baris | Alihkan logik ke servis |
| Rentetan ajaib | `grep -n '"IctSecurityOfficer"\|"Approved"' Controllers/` |
| Nama tidak jelas | Namakan semula |
| Amaran nullable | `dotnet build 2>&1 \| grep -i warning` |

3. **Tulis dokumentasi modul** — `docs/kumpulan-1/README-modul-pks.md`:

```markdown
# Modul Pematuhan PKS (Kumpulan 1)

## Apa yang dilakukannya
Akuan Pematuhan Polisi Keselamatan Siber untuk staf & kontraktor, dikait versi
polisi semasa, dengan NDA (Akta Rahsia Rasmi 1972). Disemak oleh Pegawai
Keselamatan ICT; ditadbir BPM.

## Jadual
- `PolicyVersions` — versi Polisi Keselamatan Siber (satu semasa)
- `ComplianceDeclarations` — detail akuan, satu-ke-satu dengan Submission
- `ComplianceAttachments` — melanjutkan Attachment kongsi dengan jenis dokumen

## Laluan
| Laluan | Peranan | Tujuan |
|--------|---------|--------|
| `/Compliance` | Applicant | Akuan saya |
| `/Compliance/Create` | Applicant | Borang baharu (staf/kontraktor) |
| `/Compliance/Dashboard` | IctSecurityOfficer | Baris gilir + kiraan pematuhan |
| `/Compliance/Review` | IctSecurityOfficer | Senarai bertapis |
| `/Compliance/Details/{id}` | IctSecurityOfficer | Semak & putuskan |
| `/Compliance/Policies` | IctSecurityOfficer | Urus versi polisi |
| `/Compliance/PublishPolicy` | IctSecurityOfficer | Terbit versi baharu |
| `/Compliance/Analytics` | IctSecurityOfficer | Kadar pematuhan |
| `/Compliance/Sijil/{id}` | Applicant (sendiri), IctSecurityOfficer | Sijil PDF |

## Peraturan penting
- Status pematuhan DIKIRA (versi diakui vs semasa), tidak disimpan
- Menerbitkan versi baharu = atomik; semua akuan lama jadi "perlu akui semula"
- Prefix rujukan: PKS · Peranan penyemak: IctSecurityOfficer

## Yang perlu diketahui kumpulan lain
- Kami MENGGUNA INotificationService kongsi (SMTP dari Lapor Diri) — tidak bina sendiri
- Kami menggunakan QuestPDF (sudah dalam csproj)

## Diketahui belum siap
- <senarai jujur>
```

4. **Gabungan kering** — sahkan cabang anda bergabung bersih:

```bash
git switch kump-1/pentadbiran
git pull --rebase origin master
dotnet build
dotnet test

git switch master
git pull --rebase origin master
git switch -c ujian/gabungan-kering-k1-pks
git merge kump-1/pentadbiran --no-commit --no-ff
# semak konflik, kemudian:
git merge --abort
git switch master
git branch -D ujian/gabungan-kering-k1-pks
```

Jika konflik wujud, **selesaikannya dalam cabang anda sekarang** — bukan pada Hari 15.

5. Senarai semak akhir dalam `docs/kumpulan-1/status-akhir-pks.md`:

```markdown
# Status akhir — Kumpulan 1 · PKS

## Sedia untuk Hari 15
- [x] dotnet build bersih (0 amaran)
- [x] dotnet test — <n> ujian lulus
- [x] Digabung dengan master terkini
- [x] Gabungan kering tiada konflik
- [x] README-modul-pks.md ditulis
- [x] Pemetaan URS → ujian lengkap

## Nota untuk SIT Hari 15
- Akaun ujian: applicant@nres.test / keselamatan-ict@nres.test
- Aliran demo: hantar akuan → semak → luluskan → sijil PDF → terbit polisi baharu → perlu akui semula
- Perlukan MailHog pada port 1025 untuk notifikasi
```

6. PR akhir → review → gabung ke `kump-1/pentadbiran` → push.

### ✅ Semakan (Definition of Done)

- [ ] `dotnet build` sifar amaran
- [ ] `dotnet test` semua lulus
- [ ] Tiada rentetan ajaib untuk peranan/status
- [ ] Gabungan kering tiada konflik
- [ ] `README-modul-pks.md` dan `status-akhir-pks.md` lengkap
- [ ] Kerja belum siap dinyatakan jujur
- [ ] Board mencerminkan realiti

---

## Deliverable Hari 13–14

| Artifak | Lokasi |
|---------|--------|
| Ujian nombor rujukan | `Nres.Onboarding.Tests/Pks/` |
| Ujian status pematuhan | `Nres.Onboarding.Tests/Pks/` |
| Ujian penerbitan polisi | `Nres.Onboarding.Tests/Pks/` |
| Ujian validation kontraktor | `Nres.Onboarding.Tests/Pks/` |
| Pemetaan URS → ujian | `docs/kumpulan-1/pemetaan-ujian-pks.md` |
| Penemuan prestasi | `docs/kumpulan-1/prestasi-pks.md` |
| Dokumentasi & status akhir | `docs/kumpulan-1/README-modul-pks.md`, `status-akhir-pks.md` |

## Latihan 8 — Pemecut QA dipandu-AI (Claude Code)

**Objektif:** Guna subagent `qa-uat` + skill `/uji-modul` untuk mempercepat ujian blok ini, dan pratonton pre-check pelayar Hari 15. Lab penuh: [`lab-qa-ai-uat.md`](../../../../docs/lab-qa-ai-uat.md).

> 🔧 **Khusus Claude Code.** Guna alat AI lain? Langkau — ujian manual di atas sudah memadai.

### Langkah

1. Cipta (sekali) `.claude/agents/qa-uat.md` + `.claude/skills/uji-modul/SKILL.md` (salin templat kursus [`.claude/agents/`](../../../../.claude/agents/)).
2. **Lengkapkan ujian** dengan `qa-uat` (UJI-01):

   ```text
   Guna subagent qa-uat: lengkapkan ujian xUnit — peralihan status pematuhan PKS,
   nombor rujukan PKS berjujukan, dan penerbitan versi polisi (akuan jadi "perlu akui semula").
   Guna SQLite in-memory (bukan UseInMemoryDatabase). Jalankan dotnet test, tunjuk keputusan.
   ```

3. **Pratonton pre-check pelayar** (penuh Hari 15) — jalankan app repo `pematuhan-pks` anda:

   ```text
   Guna subagent qa-uat: pre-check di app pematuhan-pks — log masuk staf → akui polisi,
   log masuk IctSecurityOfficer → semak. Sahkan peranan bukan IctSecurityOfficer ke halaman
   semakan mesti 403. Rekod lulus/gagal. Jangan cetuskan dialog.
   ```

### ✅ Semakan

- [ ] `qa-uat` + `/uji-modul` wujud; `dotnet test` hijau
- [ ] Status pematuhan, nombor rujukan PKS, penerbitan versi polisi dilindungi
- [ ] RBAC 403 disahkan; keputusan direkod (ini **pre-check**, bukan UAT sebenar)

---

**Esok (Hari 15):** empat cabang bergabung menjadi satu sistem. Bawa `README-modul-pks.md` anda dan bersedia mendemo aliran penuh — termasuk penerbitan polisi yang menjadikan akuan "perlu akui semula".
