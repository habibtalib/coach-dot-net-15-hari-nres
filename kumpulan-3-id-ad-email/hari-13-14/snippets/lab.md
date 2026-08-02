# Lab · Kumpulan 3 · Hari 13–14 — Ujian, Security Audit & Sedia Gabung

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md)
>
> **Tiada ciri baharu dalam blok ini.**

---

## Latihan 0 — Bekukan skop

```bash
git switch kump-3/id-ad-email
git pull --rebase origin master
git switch -c kump-3/feat/ujian-dan-security-audit
dotnet build
```

Tandakan setiap isu backlog: ✅ siap · 🔧 pepijat · ⏸️ tidak siap (pindah).

Rekod dalam `docs/kumpulan-3/status-akhir.md`.

### ✅ Semakan

- [ ] Setiap isu ditandakan
- [ ] Isu `shared` daripada Hari 5–6 diselesaikan atau direkod

---

## Latihan 1 — Uji laluan kelulusan

**Objektif:** Peraturan aliran kerja terkompleks dalam kursus, disahkan.

### Langkah

`Nres.Onboarding.Tests/Akaun/ApprovalRouteTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services.Akaun;

namespace Nres.Onboarding.Tests.Akaun;

public class ApprovalRouteTests
{
    private static async Task<int> SeedSubmissionAsync(TestDbFactory f)
    {
        var s = new Submission
        {
            ModuleCode = ModuleCodes.IdAdEmail,
            ApplicantUserId = "u1",
            ReferenceNo = "ICT-ID-2026-0001",
            Status = SubmissionStatus.Submitted
        };
        f.Db.Submissions.Add(s);
        await f.Db.SaveChangesAsync();
        return s.Id;
    }

    [Fact]
    public async Task Laluan_dicipta_dengan_DUA_langkah()
    {
        using var f = new TestDbFactory();
        var id = await SeedSubmissionAsync(f);

        await new ApprovalRouteService(f.Db).CreateRouteAsync(id, "penyelia1");

        var langkah = f.Db.ApprovalSteps.OrderBy(s => s.StepOrder).ToList();
        langkah.Should().HaveCount(2);
        langkah[0].StepOrder.Should().Be(1);
        langkah[0].RoleRequired.Should().Be("Supervisor");
        langkah[1].StepOrder.Should().Be(2);
        langkah[1].RoleRequired.Should().Be("IctAdmin");
        langkah.Should().AllSatisfy(s =>
            s.Decision.Should().Be(ApprovalDecision.Pending));
    }

    [Fact]
    public async Task CreateRoute_adalah_idempoten()
    {
        using var f = new TestDbFactory();
        var id = await SeedSubmissionAsync(f);
        var servis = new ApprovalRouteService(f.Db);

        await servis.CreateRouteAsync(id, "penyelia1");
        await servis.CreateRouteAsync(id, "penyelia1");   // dipanggil dua kali

        f.Db.ApprovalSteps.Count().Should().Be(2, "penghantaran semula tidak boleh menggandakan langkah");
    }

    // ---------- PERATURAN KESELAMATAN TERAS ----------

    [Fact]
    public async Task ICT_TIDAK_boleh_memutuskan_sebelum_Penyelia()
    {
        using var f = new TestDbFactory();
        var id = await SeedSubmissionAsync(f);
        var servis = new ApprovalRouteService(f.Db);
        await servis.CreateRouteAsync(id, "penyelia1");

        // Cuba memutuskan langkah 2 semasa langkah 1 masih Pending.
        var act = async () => await servis.DecideAsync(id, stepOrder: 2,
            ApprovalDecision.Approved, "ict1", null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*Langkah terdahulu belum diluluskan*");
    }

    [Fact]
    public async Task ICT_boleh_memutuskan_selepas_Penyelia_lulus()
    {
        using var f = new TestDbFactory();
        var id = await SeedSubmissionAsync(f);
        var servis = new ApprovalRouteService(f.Db);
        await servis.CreateRouteAsync(id, "penyelia1");

        await servis.DecideAsync(id, 1, ApprovalDecision.Approved, "penyelia1", "OK");
        await servis.DecideAsync(id, 2, ApprovalDecision.Approved, "ict1", "Diproses");

        f.Db.ApprovalSteps.Should().AllSatisfy(s =>
            s.Decision.Should().Be(ApprovalDecision.Approved));
    }

    [Fact]
    public async Task ICT_TIDAK_boleh_memutuskan_selepas_Penyelia_TOLAK()
    {
        using var f = new TestDbFactory();
        var id = await SeedSubmissionAsync(f);
        var servis = new ApprovalRouteService(f.Db);
        await servis.CreateRouteAsync(id, "penyelia1");

        await servis.DecideAsync(id, 1, ApprovalDecision.Rejected, "penyelia1", "Tidak perlu");

        var act = async () => await servis.DecideAsync(id, 2,
            ApprovalDecision.Approved, "ict1", null);

        await act.Should().ThrowAsync<InvalidOperationException>();
    }

    [Fact]
    public async Task Langkah_tidak_boleh_diputuskan_DUA_kali()
    {
        using var f = new TestDbFactory();
        var id = await SeedSubmissionAsync(f);
        var servis = new ApprovalRouteService(f.Db);
        await servis.CreateRouteAsync(id, "penyelia1");

        await servis.DecideAsync(id, 1, ApprovalDecision.Approved, "penyelia1", null);

        var act = async () => await servis.DecideAsync(id, 1,
            ApprovalDecision.Rejected, "penyelia2", null);

        await act.Should().ThrowAsync<InvalidOperationException>()
            .WithMessage("*telah pun diputuskan*");
    }

    [Fact]
    public async Task CurrentStep_mengembalikan_langkah_pending_terawal()
    {
        using var f = new TestDbFactory();
        var id = await SeedSubmissionAsync(f);
        var servis = new ApprovalRouteService(f.Db);
        await servis.CreateRouteAsync(id, "penyelia1");

        (await servis.CurrentStepAsync(id))!.StepOrder.Should().Be(1);

        await servis.DecideAsync(id, 1, ApprovalDecision.Approved, "penyelia1", null);

        (await servis.CurrentStepAsync(id))!.StepOrder.Should().Be(2);

        await servis.DecideAsync(id, 2, ApprovalDecision.Approved, "ict1", null);

        (await servis.CurrentStepAsync(id)).Should().BeNull("semua langkah selesai");
    }
}
```

> **Ujian `ICT_TIDAK_boleh_memutuskan_sebelum_Penyelia` ialah yang paling penting dalam trek anda.** Tanpa semakan itu, kelulusan dua peringkat ialah teater.

### ✅ Semakan

- [ ] Keenam-enam ujian lulus
- [ ] Ujian turutan langkah lulus
- [ ] Ujian idempoten lulus
- [ ] `dotnet test` hijau

---

## Latihan 2 — Uji penjanaan nama akaun AD

**Objektif:** Nama Melayu dikendalikan dengan betul.

### Langkah

`Nres.Onboarding.Tests/Akaun/AdProvisioningTests.cs`:

```csharp
using FluentAssertions;
using Microsoft.Extensions.Logging.Abstractions;
using Nres.Onboarding.Web.Models.Akaun;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services.Akaun;

namespace Nres.Onboarding.Tests.Akaun;

public class AdProvisioningTests
{
    private static SimulatedAdProvisioningService Buat(TestDbFactory f) =>
        new(f.Db, NullLogger<SimulatedAdProvisioningService>.Instance);

    [Theory]
    [InlineData("Ahmad bin Zulkifli",          "ahmad.zulkifli")]
    [InlineData("Siti Nurhaliza binti Osman",  "siti.osman")]
    [InlineData("Muthu a/l Ramasamy",          "muthu.ramasamy")]
    [InlineData("Chan Wei Ming",               "chan.ming")]
    [InlineData("Nur Aisyah",                  "nur.aisyah")]
    [InlineData("Dr Haji Abdullah bin Karim",  "abdullah.karim")]
    public void Nama_akaun_dijana_daripada_nama_Melayu(string penuh, string dijangka)
    {
        using var f = new TestDbFactory();
        Buat(f).SuggestAccountName(penuh).Should().Be(dijangka);
    }

    [Fact]
    public void Nama_tunggal_dikendalikan()
    {
        using var f = new TestDbFactory();
        Buat(f).SuggestAccountName("Ravi").Should().Be("ravi");
    }

    [Fact]
    public void Nama_kosong_tidak_melontar()
    {
        using var f = new TestDbFactory();
        Buat(f).SuggestAccountName("   ").Should().Be("pengguna");
    }

    [Fact]
    public async Task Nama_akaun_yang_telah_digunakan_TIDAK_tersedia()
    {
        using var f = new TestDbFactory();

        var s = new Submission
        {
            ModuleCode = ModuleCodes.IdAdEmail,
            ApplicantUserId = "u1",
            ReferenceNo = "ICT-ID-2026-0001",
            Status = SubmissionStatus.AdminApproved
        };
        f.Db.Submissions.Add(s);
        await f.Db.SaveChangesAsync();

        f.Db.Set<AccountRequest>().Add(new AccountRequest
        {
            SubmissionId = s.Id,
            StaffName = "Ahmad bin Zulkifli",
            StaffIdentityNo = "800101-14-1234",
            SupervisorUserId = "penyelia1",
            Justifikasi = "Staf baharu",
            AdAccountName = "ahmad.zulkifli"
        });
        await f.Db.SaveChangesAsync();

        var servis = Buat(f);
        (await servis.IsAccountNameAvailableAsync("ahmad.zulkifli")).Should().BeFalse();
        (await servis.IsAccountNameAvailableAsync("ahmad.zainal")).Should().BeTrue();
    }

    [Fact]
    public async Task Provision_menolak_nama_pendua()
    {
        using var f = new TestDbFactory();
        // (seed seperti di atas)
        // ...
        var hasil = await Buat(f).ProvisionAsync("ahmad.zulkifli", "Ahmad", "BPM");
        // Bergantung pada seed — sesuaikan mengikut data ujian anda.
        hasil.Should().NotBeNull();
    }

    [Fact]
    public async Task Hasil_provision_TIDAK_mengandungi_kata_laluan()
    {
        using var f = new TestDbFactory();
        var hasil = await Buat(f).ProvisionAsync("nama.baharu", "Nama Baharu", "BPM");

        // 🔒 Rekod dikembalikan hanya mempunyai AccountName, Email, Mesej.
        // Tiada medan kelayakan wujud — disahkan oleh jenis itu sendiri.
        hasil.Berjaya.Should().BeTrue();
        hasil.AccountName.Should().Be("nama.baharu");
        hasil.Email.Should().Be("nama.baharu@nres.gov.my");
    }
}
```

### ✅ Semakan

- [ ] Gelaran Melayu (`bin`, `binti`, `a/l`, `Dr`, `Haji`) dibuang
- [ ] Nama tunggal dan kosong dikendalikan
- [ ] Semakan ketersediaan berfungsi
- [ ] Hasil provision tiada kelayakan

---

## Latihan 3 — Uji validation & kelulusan separa

### Langkah

`Nres.Onboarding.Tests/Akaun/ValidationTests.cs` — uji keenam-enam peraturan:

```csharp
using System.ComponentModel.DataAnnotations;
using FluentAssertions;
using Nres.Onboarding.Web.Models.Akaun;
using Nres.Onboarding.Web.ViewModels.Akaun;

namespace Nres.Onboarding.Tests.Akaun;

public class ValidationTests
{
    private static List<ValidationResult> Sahkan(AccountRequestFormViewModel vm) =>
        vm.Validate(new ValidationContext(vm)).ToList();

    private static AccountRequestFormViewModel BorangSah() => new()
    {
        Jenis = JenisPermohonanAkaun.AkaunBaharu,
        StaffName = "Ahmad bin Zulkifli",
        StaffIdentityNo = "800101-14-1234",
        Justifikasi = "Staf baharu",
        SupervisorUserId = "penyelia1",
        CurrentUserId = "pemohon1",
        TarikhMula = DateTime.Today.AddDays(14),
        Akses =
        [
            new() { SystemAccessId = 1, Kod = "AD",    Nama = "AD",    Dipilih = true },
            new() { SystemAccessId = 2, Kod = "EMAIL", Nama = "E-mel", Dipilih = true }
        ]
    };

    [Fact]
    public void Borang_sah_tiada_ralat() =>
        Sahkan(BorangSah()).Should().BeEmpty();

    [Fact]
    public void Tiada_akses_dipilih_ditolak()
    {
        var vm = BorangSah();
        vm.Akses.ForEach(a => a.Dipilih = false);

        Sahkan(vm).Should().Contain(r =>
            r.ErrorMessage!.Contains("sekurang-kurangnya satu akses"));
    }

    [Fact]
    public void Akaun_baharu_tanpa_AD_ditolak()
    {
        var vm = BorangSah();
        vm.Akses.First(a => a.Kod == "AD").Dipilih = false;

        Sahkan(vm).Should().Contain(r =>
            r.ErrorMessage!.Contains("Active Directory"));
    }

    [Fact]
    public void Akses_sensitif_tanpa_justifikasi_ditolak()
    {
        var vm = BorangSah();
        vm.Akses.Add(new() {
            SystemAccessId = 3, Kod = "VPN", Nama = "VPN",
            Dipilih = true, PerluJustifikasi = true, Justifikasi = null });

        Sahkan(vm).Should().Contain(r =>
            r.ErrorMessage!.Contains("Justifikasi wajib") &&
            r.ErrorMessage!.Contains("VPN"));
    }

    [Fact]
    public void Akses_sensitif_DENGAN_justifikasi_diterima()
    {
        var vm = BorangSah();
        vm.Akses.Add(new() {
            SystemAccessId = 3, Kod = "VPN", Nama = "VPN",
            Dipilih = true, PerluJustifikasi = true,
            Justifikasi = "Kerja lapangan mingguan" });

        Sahkan(vm).Should().BeEmpty();
    }

    [Fact]
    public void Nyahaktif_tanpa_tarikh_tamat_ditolak()
    {
        var vm = BorangSah();
        vm.Jenis = JenisPermohonanAkaun.Nyahaktif;
        vm.TarikhTamat = null;

        Sahkan(vm).Should().Contain(r =>
            r.ErrorMessage!.Contains("Tarikh akhir perkhidmatan"));
    }

    [Fact]
    public void Penyelia_sama_dengan_pemohon_ditolak()
    {
        var vm = BorangSah();
        vm.SupervisorUserId = "pemohon1";   // sama dengan CurrentUserId

        Sahkan(vm).Should().Contain(r =>
            r.ErrorMessage!.Contains("diri sendiri sebagai penyelia"));
    }
}
```

### ✅ Semakan

- [ ] Keenam-enam peraturan validation diuji
- [ ] Kes **positif** (borang sah, akses sensitif dengan justifikasi) juga diuji
- [ ] Ujian kelulusan sendiri lulus

---

## Latihan 4 — Security audit

**Objektif:** Kemuncak trek anda. Sahkan sistem selamat.

### Langkah

Cipta `docs/kumpulan-3/security-audit.md` dan lengkapkan setiap bahagian.

#### Bahagian 1 — Tiada kelayakan disimpan

```bash
grep -rniE "password|kata.?laluan|passwd|secret|credential|kelayakan" \
  Nres.Onboarding.Web/Models/Akaun/ \
  Nres.Onboarding.Web/Services/Akaun/ \
  Nres.Onboarding.Web/ViewModels/Akaun/ \
  Nres.Onboarding.Web/Views/Akaun/
```

```markdown
### 1. Kelayakan
| Padanan | Fail | Justifikasi | Tindakan |
|---------|------|-------------|----------|
| "kelayakan" | AccountRequest.cs | Komen keselamatan | Kekal |
| "Kelayakan" | IctProcessViewModel.cs | Bendera penyerahan (bukan nilai) | Kekal |
| … | | | |

**Kesimpulan:** ✅ Tiada kelayakan disimpan / ⚠️ isu ditemui
```

#### Bahagian 2 — Matriks RBAC (jalankan semula)

Jalankan semula matriks Hari 9 — modul telah berubah dan kumpulan lain telah menggabungkan kerja.

```markdown
### 2. Matriks RBAC (disemak semula Hari 14)
| Peranan | K1 Dashboard | K2 Queue | K3 IctQueue | K4 Queue |
|---------|--------------|----------|-------------|----------|
| Applicant     | ❌ | ❌ | ❌ | ❌ |
| Supervisor    | | | | |
| … | | | | |

**Perubahan sejak Hari 9:** <senarai>
```

#### Bahagian 3 — Kebenaran peringkat objek

Ini **Broken Object Level Authorization** — kelemahan yang paling kerap terlepas.

```markdown
### 3. Kebenaran peringkat objek
| # | Ujian | Jangkaan | Keputusan |
|---|-------|----------|-----------|
| 1 | Pemohon A buka `/AccountRequest/Edit/{id-B}` | 403 | |
| 2 | Penyelia X luluskan permohonan penyelia Y (POST terus) | 403 | |
| 3 | IctAdmin proses permohonan `Submitted` (belum lulus penyelia) | Gagal | |
| 4 | Pemohon buka `/AccountRequest/Process/{id}` | 403 | |
| 5 | Pemohon POST `SupervisorApprove` | 403 | |
```

> **Ujian 2 dan 5 mesti dilakukan dengan POST terus** (curl atau alat pembangun), bukan dengan mengklik — butang tersembunyi bukan kawalan keselamatan.

#### Bahagian 4 — Integriti audit

```markdown
### 4. Integriti audit
- [ ] Tiada action mengemas kini atau memadam `AuditLog`
- [ ] `ActorUserId` datang dari `ICurrentUserService`, BUKAN input borang
- [ ] Tiada laluan borang kepada `AuditLog`

Semakan:
```bash
grep -rn "AuditLogs.Remove\|AuditLogs.Update" Nres.Onboarding.Web/
grep -rn "ActorUserId" Nres.Onboarding.Web/Controllers/
```
```

#### Bahagian 5 — Kebocoran maklumat

```markdown
### 5. Kebocoran maklumat
| Semakan | Penemuan |
|---------|----------|
| Mesej ralat mendedahkan kewujudan pengguna? | |
| Skrin penyelia mendedahkan data ICT? | |
| 403 vs 404 mendedahkan kewujudan rekod? | |
| Surih tindanan dipaparkan dalam ralat? | |

**Keputusan 403/404:** <nyatakan pendekatan yang dipilih dan sebabnya>
```

#### Bahagian 6 — Ringkasan & pengesyoran

```markdown
### 6. Ringkasan
**Isu Blocker:** <n>   **Major:** <n>   **Minor:** <n>

### Pengesyoran untuk NRES
1. **Integrasi AD adalah SIMULASI** — akaun sebenar tidak dicipta.
   Sebelum pengeluaran, sambungkan `IAdProvisioningService` ke AD sebenar.
2. Kata laluan tidak pernah disimpan — kekalkan sifat ini.
3. <penemuan lain>
```

### ✅ Semakan

- [ ] Keenam-enam bahagian lengkap
- [ ] Grep kelayakan bersih atau setiap padanan dijustifikasi
- [ ] Ujian kebenaran objek dijalankan dengan **POST terus**
- [ ] Isu difailkan kepada kumpulan pemilik
- [ ] Pengesyoran ditulis untuk serahan NRES

---

## Latihan 5 — Refactor & dokumentasi

### Langkah

1. Betulkan amaran pengkompil:

```bash
dotnet build 2>&1 | grep -i warning
```

2. **Semakan kod jana-AI** — setiap kaedah mesti boleh diterangkan oleh seseorang dalam kumpulan.

3. `docs/kumpulan-3/README-modul.md`:

```markdown
# Modul ID, AD & Email (Kumpulan 3)

## Apa yang dilakukannya
Menguruskan permohonan akaun pengguna dan akses sistem melalui kelulusan
DUA peringkat (Penyelia Jabatan → Pentadbir ICT), dengan kelulusan akses
separa dan jejak audit penuh.

## Jadual
- `AccountRequests` — permohonan; UNIK pada `AdAccountName` dan `OfficialEmail`
- `RequestedSystemAccesses` — akses dipohon; `Diluluskan` ialah `bool?` (3 keadaan)
- `LookupSystemAccesses` — katalog akses, 8 baris berseed

## ⚠️ Yang perlu diketahui kumpulan lain

**1. Kami satu-satunya modul dengan kelulusan DUA peringkat.**
Kami menggunakan `ApprovalStep` dengan `StepOrder` 1 (Supervisor) dan 2 (IctAdmin).

**2. Kami menambah tindakan `SupervisorApprove`** — ia menetapkan
`SupervisorApproved`, bukan `AdminApproved`. `SubmissionControllerBase.Approve`
digunakan untuk peringkat 2 sahaja.

**3. Kami MENGATASI `Reject`** supaya kedua-dua Supervisor dan IctAdmin boleh
menolak pada peringkat masing-masing. Lihat isu #<n> `shared`.

**4. Integrasi AD adalah SIMULASI.** `SimulatedAdProvisioningService` tidak
menyentuh Active Directory sebenar. Antara muka `IAdProvisioningService`
direka untuk digantikan.

**5. 🔒 Kami tidak menyimpan kata laluan** — reka bentuk yang disengajakan.
Sistem merekod bahawa kelayakan diserahkan, bukan kelayakan itu.

## Laluan
| Laluan | Peranan | Tujuan |
|--------|---------|--------|
| `/AccountRequest` | Applicant | Permohonan saya |
| `/AccountRequest/Create` | Applicant | Borang baharu |
| `/AccountRequest/SupervisorQueue` | Supervisor | Baris gilir peringkat 1 |
| `/AccountRequest/IctQueue` | IctAdmin | Baris gilir peringkat 2 |
| `/AccountRequest/Process/{id}` | IctAdmin | Pemprosesan + keputusan akses |
| `/AccountRequest/IctDashboard` | IctAdmin | Papan pemuka operasi |

## Servis
- `IApprovalRouteService` — laluan 2 peringkat, kuatkuasa turutan
- `IAdProvisioningService` — **simulasi** AD
- `ITrackingService` — garis masa penjejakan

## Diketahui belum siap
- <senarai jujur>
```

4. Gabungan kering:

```bash
git switch master && git pull --rebase origin master
git switch -c ujian/gabungan-kering-k3
git merge kump-3/id-ad-email --no-commit --no-ff
# semak, kemudian:
git merge --abort
git switch master && git branch -D ujian/gabungan-kering-k3
```

### ✅ Semakan

- [ ] Sifar amaran pengkompil
- [ ] `README-modul.md` menyatakan kelima-lima perkara penting
- [ ] Gabungan kering tiada konflik
- [ ] `status-akhir.md` jujur

---

## Deliverable Hari 13–14

| Artifak | Lokasi |
|---------|--------|
| Ujian laluan kelulusan (6) | `Nres.Onboarding.Tests/Akaun/` |
| Ujian penjanaan nama AD | Sama |
| Ujian validation (7) | Sama |
| **Laporan security audit** | `docs/kumpulan-3/security-audit.md` |
| Dokumentasi modul | `docs/kumpulan-3/README-modul.md` |
| Status akhir | `docs/kumpulan-3/status-akhir.md` |

**Esok (Hari 15):** empat cabang bergabung. Bawa laporan security audit anda — ia menyumbang terus kepada bahagian RBAC dalam SIT.
