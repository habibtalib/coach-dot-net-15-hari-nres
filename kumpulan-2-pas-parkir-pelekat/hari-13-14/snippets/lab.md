# Lab · Kumpulan 2 · Hari 13–14 — Ujian, Bug Fixing & Sedia Gabung

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md)
>
> **Tiada ciri baharu dalam blok ini.**

---

## Latihan 0 — Mula blok & bekukan skop

```bash
git switch kump-2/akses-kenderaan
git pull --rebase origin master
git switch -c kump-2/feat/ujian-dan-pembaikan
dotnet build
```

Tandakan setiap isu backlog: ✅ siap · 🔧 pepijat diketahui · ⏸️ tidak siap (pindah, jangan mula).

Rekod dalam `docs/kumpulan-2/status-akhir.md`.

### ✅ Semakan

- [ ] Setiap isu ditandakan
- [ ] Kerja belum siap dipindahkan
- [ ] Senarai pepijat untuk dibetulkan hari ini

---

## Latihan 1 — Projek ujian

**Objektif:** Tambah folder ujian anda ke projek ujian kongsi.

> Projek `Nres.Onboarding.Tests` dan `TestDbFactory` dicipta oleh **satu** kumpulan (koordinasi jurulatih). Jika ia sudah wujud pada `master`, tarik sahaja dan tambah folder anda.

### Langkah

```bash
git pull --rebase origin master
ls Nres.Onboarding.Tests/            # sudah wujud?
mkdir -p Nres.Onboarding.Tests/Akses
```

Jika belum wujud, cipta seperti dalam lab Kumpulan 1 Hari 13–14 Latihan 1 — dan **beritahu kumpulan lain** anda telah menciptanya.

### ✅ Semakan

- [ ] `Nres.Onboarding.Tests` wujud dan dibina
- [ ] `TestDbFactory` menggunakan **SQLite**, bukan penyedia InMemory
- [ ] Folder `Akses/` dicipta

---

## Latihan 2 — Uji normalisasi nombor plat

**Objektif:** Asas semakan pendua anda betul.

### Langkah

`Nres.Onboarding.Tests/Akses/VehicleNormalizeTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Akses;

namespace Nres.Onboarding.Tests.Akses;

public class VehicleNormalizeTests
{
    [Theory]
    [InlineData("WXY 1234",  "WXY1234")]
    [InlineData("wxy1234",   "WXY1234")]
    [InlineData("WXY-1234",  "WXY1234")]
    [InlineData(" wxy 1234 ", "WXY1234")]
    [InlineData("W X Y 1234", "WXY1234")]
    [InlineData("VBA8888",   "VBA8888")]
    public void Nombor_plat_dinormalkan_secara_konsisten(string input, string dijangka)
    {
        Vehicle.Normalize(input).Should().Be(dijangka);
    }

    [Fact]
    public void Plat_yang_ditulis_berbeza_menghasilkan_normalisasi_sama()
    {
        var variasi = new[] { "WXY 1234", "wxy1234", "WXY-1234", "w x y 1 2 3 4" };
        var dinormalkan = variasi.Select(Vehicle.Normalize).Distinct().ToList();

        // Kesemuanya kenderaan yang SAMA.
        dinormalkan.Should().HaveCount(1);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData("---")]
    public void Input_tanpa_aksara_alfanumerik_menjadi_kosong(string input)
    {
        Vehicle.Normalize(input).Should().BeEmpty();
    }
}
```

### ✅ Semakan

- [ ] Semua kes `[Theory]` lulus
- [ ] Ujian "variasi menghasilkan satu" lulus
- [ ] Input kosong dikendalikan

---

## Latihan 3 — Uji semakan pendua (kedua-dua arah)

**Objektif:** Buktikan anda menyekat yang betul **dan membenarkan** yang betul.

### Langkah

`Nres.Onboarding.Tests/Akses/DuplicateCheckTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services.Akses;

namespace Nres.Onboarding.Tests.Akses;

public class DuplicateCheckTests
{
    /// <summary>Bantu: cipta kenderaan + permohonan pelekat dengan status diberi.</summary>
    private static async Task<(Vehicle, int)> SeedStickerAsync(
        TestDbFactory f, string plat, int tahun, SubmissionStatus status,
        string rujukan = "STK-2026-0001")
    {
        var v = new Vehicle
        {
            OwnerUserId = "u1",
            PlateNumber = plat,
            PlateNumberNormalized = Vehicle.Normalize(plat)
        };
        f.Db.Set<Vehicle>().Add(v);
        await f.Db.SaveChangesAsync();

        var s = new Submission
        {
            ModuleCode = ModuleCodes.PelekatKenderaan,
            ApplicantUserId = "u1",
            ReferenceNo = rujukan,
            Status = status
        };
        f.Db.Submissions.Add(s);
        await f.Db.SaveChangesAsync();

        f.Db.Set<VehicleStickerApplication>().Add(new VehicleStickerApplication
        {
            SubmissionId = s.Id, VehicleId = v.Id, TahunPelekat = tahun
        });
        await f.Db.SaveChangesAsync();

        return (v, s.Id);
    }

    // ---------- KES YANG MESTI DISEKAT ----------

    [Theory]
    [InlineData(SubmissionStatus.Submitted)]
    [InlineData(SubmissionStatus.SupervisorApproved)]
    [InlineData(SubmissionStatus.AdminApproved)]
    public async Task Permohonan_aktif_tahun_sama_DISEKAT(SubmissionStatus status)
    {
        using var f = new TestDbFactory();
        var (v, _) = await SeedStickerAsync(f, "WXY1234", 2026, status);

        var hit = await new DuplicateCheckService(f.Db).ActiveStickerAsync(v.Id, 2026);

        hit.Should().NotBeNull();
        hit!.ReferenceNo.Should().Be("STK-2026-0001");
    }

    // ---------- KES YANG MESTI DIBENARKAN ----------
    // Ini lebih penting daripada yang di atas. Semakan pendua yang terlalu
    // ketat menghalang kerja sebenar dan lulus setiap ujian yang jelas.

    [Fact]
    public async Task Tahun_BERBEZA_dibenarkan()
    {
        using var f = new TestDbFactory();
        var (v, _) = await SeedStickerAsync(f, "WXY1234", 2026, SubmissionStatus.AdminApproved);

        var hit = await new DuplicateCheckService(f.Db).ActiveStickerAsync(v.Id, 2027);

        hit.Should().BeNull("pelekat tahun berbeza ialah permohonan yang sah");
    }

    [Theory]
    [InlineData(SubmissionStatus.Rejected)]
    [InlineData(SubmissionStatus.Cancelled)]
    public async Task Status_terminal_TIDAK_menyekat(SubmissionStatus status)
    {
        using var f = new TestDbFactory();
        var (v, _) = await SeedStickerAsync(f, "WXY1234", 2026, status);

        var hit = await new DuplicateCheckService(f.Db).ActiveStickerAsync(v.Id, 2026);

        hit.Should().BeNull(
            "permohonan yang ditolak mesti boleh dibetulkan dan dihantar semula");
    }

    [Fact]
    public async Task Draf_TIDAK_menyekat()
    {
        using var f = new TestDbFactory();
        var (v, _) = await SeedStickerAsync(f, "WXY1234", 2026, SubmissionStatus.Draft);

        var hit = await new DuplicateCheckService(f.Db).ActiveStickerAsync(v.Id, 2026);

        hit.Should().BeNull();
    }

    [Fact]
    public async Task Permohonan_tidak_menyekat_DIRINYA_sendiri()
    {
        using var f = new TestDbFactory();
        var (v, submissionId) = await SeedStickerAsync(
            f, "WXY1234", 2026, SubmissionStatus.Submitted);

        var hit = await new DuplicateCheckService(f.Db)
            .ActiveStickerAsync(v.Id, 2026, kecualiSubmissionId: submissionId);

        hit.Should().BeNull("menghantar semula permohonan yang sama tidak boleh disekat");
    }

    [Fact]
    public async Task Kenderaan_BERBEZA_tidak_menyekat()
    {
        using var f = new TestDbFactory();
        await SeedStickerAsync(f, "WXY1234", 2026, SubmissionStatus.AdminApproved);

        var lain = new Vehicle
        {
            OwnerUserId = "u2",
            PlateNumber = "ABC9999",
            PlateNumberNormalized = "ABC9999"
        };
        f.Db.Set<Vehicle>().Add(lain);
        await f.Db.SaveChangesAsync();

        var hit = await new DuplicateCheckService(f.Db).ActiveStickerAsync(lain.Id, 2026);

        hit.Should().BeNull();
    }
}
```

### ✅ Semakan

- [ ] Ketiga-tiga status aktif disekat
- [ ] Tahun berbeza **dibenarkan**
- [ ] `Rejected` dan `Cancelled` **tidak** menyekat
- [ ] `Draft` tidak menyekat
- [ ] `kecualiSubmissionId` berfungsi
- [ ] Setiap `Should()` mempunyai sebab bila tidak jelas

---

## Latihan 4 — Uji peruntukan lot & kesahihan

**Objektif:** Sumber terhad diuruskan dengan betul.

### Langkah

1. `Nres.Onboarding.Tests/Akses/AllocationTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Akses;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services.Akses;

namespace Nres.Onboarding.Tests.Akses;

public class AllocationTests
{
    private static async Task<int> SeedParkingAsync(
        TestDbFactory f, string lot, SubmissionStatus status)
    {
        var v = new Vehicle
        {
            OwnerUserId = "u1", PlateNumber = "WXY1234",
            PlateNumberNormalized = "WXY1234"
        };
        f.Db.Set<Vehicle>().Add(v);

        var s = new Submission
        {
            ModuleCode = ModuleCodes.Parkir, ApplicantUserId = "u1",
            ReferenceNo = "PKR-2026-0001", Status = status
        };
        f.Db.Submissions.Add(s);
        await f.Db.SaveChangesAsync();

        f.Db.Set<ParkingApplication>().Add(new ParkingApplication
        {
            SubmissionId = s.Id, VehicleId = v.Id, LotNumber = lot
        });
        await f.Db.SaveChangesAsync();
        return s.Id;
    }

    [Fact]
    public async Task Lot_yang_diperuntukkan_TIDAK_bebas()
    {
        using var f = new TestDbFactory();
        await SeedParkingAsync(f, "C-01", SubmissionStatus.AdminApproved);

        var bebas = await new AllocationService(f.Db).IsLotFreeAsync("C-01");

        bebas.Should().BeFalse();
    }

    [Theory]
    [InlineData(SubmissionStatus.Rejected)]
    [InlineData(SubmissionStatus.Cancelled)]
    public async Task Lot_DILEPASKAN_bila_permohonan_terminal(SubmissionStatus status)
    {
        using var f = new TestDbFactory();
        await SeedParkingAsync(f, "C-01", status);

        var bebas = await new AllocationService(f.Db).IsLotFreeAsync("C-01");

        bebas.Should().BeTrue("lot mesti dilepaskan bila permohonan ditolak/dibatalkan");
    }

    [Fact]
    public async Task Lot_tidak_menyekat_permohonannya_sendiri()
    {
        using var f = new TestDbFactory();
        var submissionId = await SeedParkingAsync(f, "C-01", SubmissionStatus.AdminApproved);

        var bebas = await new AllocationService(f.Db)
            .IsLotFreeAsync("C-01", kecualiSubmissionId: submissionId);

        bebas.Should().BeTrue();
    }

    [Fact]
    public async Task Siri_pelekat_bertambah()
    {
        using var f = new TestDbFactory();
        var servis = new AllocationService(f.Db);

        var pertama = await servis.NextStickerSerialAsync(2026);
        pertama.Should().Be("SK-2026-0001");
    }
}
```

2. `Nres.Onboarding.Tests/Akses/KesahihanTests.cs` — jika anda mengekstrak `NilaiKesahihan` ke kelas boleh diuji (disyorkan; jika ia masih peribadi dalam controller, **refactor sekarang** — itu tujuan blok ini):

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services.Akses;

namespace Nres.Onboarding.Tests.Akses;

public class KesahihanTests
{
    private static readonly DateTime Semalam = DateTime.UtcNow.Date.AddDays(-1);
    private static readonly DateTime Esok    = DateTime.UtcNow.Date.AddDays(1);

    [Fact]
    public void Diluluskan_dan_dalam_tempoh_adalah_SAH()
    {
        var (sah, sebab) = PassValidator.Nilai(
            SubmissionStatus.AdminApproved, Semalam, Esok);

        sah.Should().BeTrue();
        sebab.Should().BeNull();
    }

    [Fact]
    public void Dibatalkan_adalah_TIDAK_SAH_dengan_sebab()
    {
        var (sah, sebab) = PassValidator.Nilai(
            SubmissionStatus.Cancelled, Semalam, Esok);

        sah.Should().BeFalse();
        // Pengawal mesti tahu KENAPA — bukan hanya "tidak dijumpai".
        sebab.Should().Contain("DIBATALKAN");
    }

    [Fact]
    public void Tamat_tempoh_adalah_TIDAK_SAH()
    {
        var (sah, sebab) = PassValidator.Nilai(
            SubmissionStatus.AdminApproved, Semalam.AddDays(-30), Semalam);

        sah.Should().BeFalse();
        sebab.Should().Contain("TAMAT TEMPOH");
    }

    [Fact]
    public void Belum_bermula_adalah_TIDAK_SAH()
    {
        var (sah, sebab) = PassValidator.Nilai(
            SubmissionStatus.AdminApproved, Esok, Esok.AddDays(30));

        sah.Should().BeFalse();
        sebab.Should().Contain("Belum sah");
    }

    [Fact]
    public void Belum_diluluskan_adalah_TIDAK_SAH()
    {
        var (sah, _) = PassValidator.Nilai(
            SubmissionStatus.Submitted, Semalam, Esok);

        sah.Should().BeFalse();
    }
}
```

> **Jika `NilaiKesahihan` masih kaedah peribadi dalam controller**, ekstrak ia ke `Services/Akses/PassValidator.cs` sekarang. Logik yang tidak boleh diuji ialah logik yang tidak diuji — dan ini logik yang menentukan sama ada seseorang masuk ke bangunan.

### ✅ Semakan

- [ ] Lot yang diperuntukkan tidak bebas
- [ ] Lot **dilepaskan** pada penolakan/pembatalan
- [ ] `PassValidator` diekstrak dan diuji
- [ ] Kesemua lima keadaan kesahihan diliputi

---

## Latihan 5 — Ujian E2E manual

**Objektif:** Aliran penuh merentas tiga jenis permohonan.

### Langkah

Jalankan dan rekod dalam `docs/kumpulan-2/ujian-e2e.md`:

```markdown
# Ujian E2E — Kumpulan 2

## Aliran A: Pas Keselamatan
| # | Langkah | Peranan | Jangkaan | Keputusan |
|---|---------|---------|----------|-----------|
| A1 | Mohon pas Kontraktor tanpa syarikat | Applicant | Ditolak validation | |
| A2 | Isi syarikat, tempoh 120 hari | Applicant | Ditolak — maks 90 hari | |
| A3 | Betulkan ke 60 hari, hantar | Applicant | PAS-2026-#### dijana | |
| A4 | Luluskan | SecurityAdmin | Siri PS-2026-#### + token QR | |
| A5 | Buka halaman pas, cetak | Applicant | QR kelihatan | |
| A6 | Imbas QR | SecurityAdmin | SAH hijau | |
| A7 | Batalkan pas, imbas semula | SecurityAdmin | TIDAK SAH + sebab | |

## Aliran B: Pelekat Kenderaan
| # | Langkah | Jangkaan | Keputusan |
|---|---------|----------|-----------|
| B1 | Mohon pelekat 2026 untuk WXY1234 | STK-2026-#### | |
| B2 | Mohon LAGI pelekat 2026, plat sama | Disekat, mesej namakan STK sebelumnya | |
| B3 | Taip `wxy 1234` (format berbeza) | Dikenali kenderaan sama, disekat | |
| B4 | Mohon pelekat 2027 | Berjaya | |
| B5 | Tolak B1, mohon semula 2026 | Berjaya | |
| B6 | Staf LAIN mohon untuk WXY1234 | Mesej "hubungi Bahagian Keselamatan" | |

## Aliran C: Lot Parkir
| # | Langkah | Jangkaan | Keputusan |
|---|---------|----------|-----------|
| C1 | Mohon parkir OKU tanpa justifikasi | Ditolak validation | |
| C2 | Isi justifikasi, hantar | PKR-2026-#### | |
| C3 | Luluskan tanpa pilih lot | Ditolak | |
| C4 | Luluskan dengan lot B-01 | Lot direkod dalam audit | |
| C5 | Luluskan permohonan lain dengan B-01 | Disekat — lot diguna | |
| C6 | Tolak C2, peruntuk B-01 semula | Berjaya | |
| C7 | Semak dropdown lot | B-01 tiada semasa diguna, muncul selepas dilepaskan | |

## Aliran D: Laporan & RBAC
| # | Langkah | Jangkaan | Keputusan |
|---|---------|----------|-----------|
| D1 | Applicant → /Akses/Queue | 403 | |
| D2 | Applicant → /Akses/Semak | 403 | |
| D3 | HrAdmin → /Akses/Laporan | 403 | |
| D4 | Eksport CSV, buka dalam Excel | Aksara BM betul | |
| D5 | Nama dengan koma dalam CSV | Tidak merosakkan lajur | |
```

**Bagi setiap kegagalan:** betulkan hari ini jika kecil; rekod dalam `status-akhir.md` jika tidak.

### ✅ Semakan

- [ ] Keempat-empat aliran dijalankan sepenuhnya
- [ ] Kegagalan dibetulkan atau direkod dengan jujur
- [ ] D1–D3 memberi 403

---

## Latihan 6 — Prestasi & refactor

**Objektif:** Ukur, kemudian betulkan.

### Langkah

1. Hidupkan logging SQL:

```json
"Logging": { "LogLevel": {
  "Microsoft.EntityFrameworkCore.Database.Command": "Information" } }
```

2. Jana ~200 permohonan merentas tiga jenis, kemudian periksa setiap skrin:

| Skrin | Cari |
|-------|------|
| `/Akses` | Query munasabah (kami menjangka 4) |
| `/Akses/Queue` | **Tiga query + satu untuk profil** — bukan N+1 per baris |
| `/Akses/Semak` | Satu atau dua query sahaja |
| `/Akses/Laporan` | Tiada `SELECT *` untuk lajur yang tidak dipaparkan |

3. **Semak semula keputusan tiga-query-dalam-memori** (Hari 7–9). Dengan 200 rekod ia baik. Rekod dalam `docs/kumpulan-2/prestasi.md`:

```markdown
# Semakan prestasi — Kumpulan 2

## Baris gilir Keselamatan
Pendekatan: tiga query + gabung dalam memori.
Diukur pada 200 rekod: <n> ms, <n> query.
Keputusan: KEKAL. Set data NRES ratusan, bukan ratusan ribu.
Jika ia melebihi ~5,000 rekod, tukar kepada SQL UNION atau
paging setiap jenis.
```

> Mendokumenkan **kenapa anda tidak mengubahnya** sama berharga dengan mendokumentasikan optimasi.

4. **Refactor:**

| Sasaran | Cara |
|---------|------|
| `NilaiKesahihan` peribadi dalam controller | Ekstrak ke `PassValidator` (Latihan 4) |
| Logik `Approve` didup merentas tiga controller | Kaedah pembantu kongsi dalam `Services/Akses/` |
| Rentetan ajaib (`"PS-"`, `"SK-"`) | Pemalar |
| Kod jana-AI yang tiada siapa faham | Fahami atau buang |

5. Betulkan setiap amaran pengkompil:

```bash
dotnet build 2>&1 | grep -i warning
```

### ✅ Semakan

- [ ] Kelima-lima skrin diperiksa dengan logging SQL
- [ ] Tiada N+1
- [ ] Keputusan prestasi didokumenkan (termasuk yang **tidak** diubah)
- [ ] `PassValidator` diekstrak
- [ ] Sifar amaran pengkompil
- [ ] Semua ujian masih lulus

---

## Latihan 7 — Dokumentasi modul & sedia gabung

### Langkah

1. `docs/kumpulan-2/README-modul.md`:

```markdown
# Modul Pas, Parkir & Pelekat Kenderaan (Kumpulan 2)

## Apa yang dilakukannya
Menguruskan akses kawasan dan keselamatan kenderaan — pas pelawat/staf/kontraktor,
pelekat kenderaan tahunan, dan peruntukan lot parkir — dengan semakan pendua
nombor plat, kelulusan Pegawai Keselamatan dengan peruntukan, dan pengesahan
QR di lapangan.

## Jadual
- `Vehicles` — kenderaan berdaftar; UNIK pada `PlateNumberNormalized`
- `AccessPassApplications` — pas keselamatan (tidak terikat kenderaan)
- `VehicleStickerApplications` — pelekat tahunan (terikat kenderaan)
- `ParkingApplications` — peruntukan lot (terikat kenderaan)
- `ParkingLots` — lot fizikal, data seed

## Laluan
| Laluan | Peranan | Tujuan |
|--------|---------|--------|
| `/Akses` | Applicant | Halaman utama modul |
| `/AccessPass/Create` | Applicant | Mohon pas |
| `/VehicleSticker/Create` | Applicant | Mohon pelekat |
| `/Parking/Create` | Applicant | Mohon parkir |
| `/Akses/Queue` | SecurityAdmin | Baris gilir semakan (3 jenis) |
| `/Akses/Semak` | SecurityAdmin | **Skrin ronda — imbas QR / cari plat** |
| `/Akses/Laporan` | SecurityAdmin | Laporan + eksport CSV |

## Servis
- `IVehicleService` — pendaftaran & carian kenderaan
- `IDuplicateCheckService` — semakan pendua plat/lot
- `IAllocationService` — siri pas/pelekat, ketersediaan lot
- `ISecurityReviewService` — baris gilir & laporan
- `IQrCodeService` — penjanaan QR

## Aliran status
Draft → Submitted → AdminApproved | Rejected
Kelulusan bersyarat = `AdminApproved` + medan `SyaratKelulusan`
(**tiada** ahli `SubmissionStatus` baharu ditambah)

## ⚠️ Yang perlu diketahui kumpulan lain
- Kami menambah pakej **QRCoder** ke `.csproj`
- Kami **mengatasi** `SubmissionControllerBase.Approve` dalam VehicleSticker &
  Parking untuk memperuntukkan siri/lot — `base.Approve` sentiasa dipanggil
- Parking menggunakan `ApproveWithLot`, bukan `Approve` biasa
- Prefix rujukan: PAS, STK, PKR

## Diketahui belum siap
- <senarai jujur>
```

2. Gabungan kering:

```bash
git switch master && git pull --rebase origin master
git switch -c ujian/gabungan-kering-k2
git merge kump-2/akses-kenderaan --no-commit --no-ff
# semak konflik, kemudian:
git merge --abort
git switch master && git branch -D ujian/gabungan-kering-k2
```

Selesaikan sebarang konflik **dalam cabang anda hari ini**.

3. `docs/kumpulan-2/status-akhir.md`:

```markdown
# Status akhir — Kumpulan 2

## Sedia untuk Hari 15
- [x] dotnet build bersih
- [x] dotnet test — <n> ujian lulus
- [x] Digabung dengan master terkini
- [x] Gabungan kering tiada konflik
- [x] README-modul.md ditulis

## Diketahui belum siap
- <jujur>

## Nota untuk SIT Hari 15
- Akaun ujian: applicant@nres.test / keselamatan@nres.test
- Aliran demo: mohon pelekat → cuba pendua → luluskan → cetak → imbas QR
- Skrin ronda paling baik didemo pada telefon
- Lot berseed: A-01/02 (Eksekutif), B-01/02 (OKU), C-01..04 (Biasa)
```

### ✅ Semakan

- [ ] `README-modul.md` menyatakan pakej QRCoder & `override` Approve
- [ ] Gabungan kering tiada konflik
- [ ] `status-akhir.md` jujur
- [ ] PR akhir digabung

---

## Deliverable Hari 13–14

| Artifak | Lokasi |
|---------|--------|
| Ujian normalisasi plat | `Nres.Onboarding.Tests/Akses/` |
| Ujian semakan pendua (disekat + dibenarkan) | `Nres.Onboarding.Tests/Akses/` |
| Ujian peruntukan & kesahihan | `Nres.Onboarding.Tests/Akses/` |
| `PassValidator` diekstrak | `Services/Akses/` |
| Rekod ujian E2E | `docs/kumpulan-2/ujian-e2e.md` |
| Penemuan prestasi | `docs/kumpulan-2/prestasi.md` |
| Dokumentasi modul | `docs/kumpulan-2/README-modul.md` |
| Status akhir | `docs/kumpulan-2/status-akhir.md` |

**Esok (Hari 15):** empat cabang bergabung. Bawa telefon anda — skrin ronda ialah demo yang baik.
