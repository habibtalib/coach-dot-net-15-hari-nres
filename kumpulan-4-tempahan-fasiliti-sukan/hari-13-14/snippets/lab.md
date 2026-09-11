# Lab · Kumpulan 4 · Hari 13–14 — Ujian, Refactor & Sedia Gabung

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md)
>
> **Tiada ciri baharu dalam blok ini.**

---

## Latihan 0 — Mula blok & bekukan skop

```bash
git switch kump-4/tempahan-fasiliti
git pull --rebase origin master
git switch -c kump-4/feat/ujian-dan-pembaikan
dotnet build
```

Tandakan setiap isu backlog: ✅ siap · 🔧 pepijat diketahui · ⏸️ tidak siap (pindah, jangan mula).

Rekod dalam `docs/kumpulan-4/status-akhir.md`.

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
mkdir -p Nres.Onboarding.Tests/Fasiliti
```

Jika belum wujud, cipta seperti dalam lab Kumpulan 1 Hari 13–14 Latihan 1 — dan **beritahu kumpulan lain** anda telah menciptanya. `TestDbFactory` mesti guna **SQLite** (bukan InMemory) supaya perbandingan `TimeOnly`/`DateOnly` berkelakuan seperti pengeluaran.

### ✅ Semakan

- [ ] `Nres.Onboarding.Tests` wujud dan dibina
- [ ] `TestDbFactory` menggunakan **SQLite**, bukan penyedia InMemory
- [ ] Folder `Fasiliti/` dicipta

---

## Latihan 2 — Uji predikat pertindihan tulen

**Objektif:** Buktikan `SlotOverlap.Overlaps` betul untuk setiap kes tepi — tanpa pangkalan data.

### Langkah

`Nres.Onboarding.Tests/Fasiliti/SlotOverlapTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Services.Fasiliti;

namespace Nres.Onboarding.Tests.Fasiliti;

public class SlotOverlapTests
{
    private static TimeOnly T(int jam, int minit = 0) => new(jam, minit);

    // ---------- KES YANG BERTINDIH ----------

    [Theory]
    [InlineData(10, 0, 11, 0, 10, 0, 11, 0)]   // serentak tepat
    [InlineData(10, 0, 11, 0, 10, 30, 11, 30)] // bertindih separa (lewat)
    [InlineData(10, 0, 11, 0, 9, 30, 10, 30)]  // bertindih separa (awal)
    [InlineData(10, 0, 12, 0, 10, 30, 11, 0)]  // baharu TERKANDUNG dalam sedia ada
    [InlineData(10, 30, 11, 0, 10, 0, 12, 0)]  // baharu MELIPUTI sedia ada
    public void Slot_bertindih_dikesan(
        int aSj, int aSm, int aTj, int aTm, int bSj, int bSm, int bTj, int bTm)
    {
        SlotOverlap.Overlaps(T(aSj, aSm), T(aTj, aTm), T(bSj, bSm), T(bTj, bTm))
            .Should().BeTrue();
    }

    // ---------- KES YANG TIDAK BERTINDIH ----------
    // Ini lebih penting. Semakan yang terlalu ketat menolak tempahan
    // bersebelahan yang sah dan menghalang kerja sebenar Encik Faizal.

    [Theory]
    [InlineData(10, 0, 11, 0, 11, 0, 12, 0)]   // BERSEBELAHAN selepas — dibenarkan
    [InlineData(10, 0, 11, 0, 9, 0, 10, 0)]    // BERSEBELAHAN sebelum — dibenarkan
    [InlineData(10, 0, 11, 0, 11, 30, 12, 0)]  // berjurang selepas
    [InlineData(10, 0, 11, 0, 8, 0, 9, 30)]    // berjurang sebelum
    public void Slot_tidak_bertindih_dibenarkan(
        int aSj, int aSm, int aTj, int aTm, int bSj, int bSm, int bTj, int bTm)
    {
        SlotOverlap.Overlaps(T(aSj, aSm), T(aTj, aTm), T(bSj, bSm), T(bTj, bTm))
            .Should().BeFalse();
    }

    [Fact]
    public void Slot_bersebelahan_tepat_pada_sempadan_tidak_bertindih()
    {
        // 11:00 tamat == 11:00 mula. Selang separuh-terbuka [mula, tamat).
        SlotOverlap.Overlaps(T(10), T(11), T(11), T(12)).Should().BeFalse(
            "selang separuh-terbuka membenarkan tempahan bersebelahan");
    }

    [Fact]
    public void Pertindihan_simetri()
    {
        // Overlaps(A, B) mesti sama dengan Overlaps(B, A).
        var ab = SlotOverlap.Overlaps(T(10), T(11), T(10, 30), T(11, 30));
        var ba = SlotOverlap.Overlaps(T(10, 30), T(11, 30), T(10), T(11));
        ab.Should().Be(ba);
    }
}
```

### ✅ Semakan

- [ ] Semua kes `[Theory]` bertindih lulus
- [ ] Semua kes bersebelahan/berjurang lulus (tidak bertindih)
- [ ] Ujian sempadan tepat (`11` == `11`) lulus
- [ ] Ujian simetri lulus

---

## Latihan 3 — Uji servis bertindih (kedua-dua arah, dengan status)

**Objektif:** Buktikan SQL menterjemah predikat dengan betul **dan** menapis status.

### Langkah

`Nres.Onboarding.Tests/Fasiliti/SlotBookingServiceTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Fasiliti;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services.Fasiliti;

namespace Nres.Onboarding.Tests.Fasiliti;

public class SlotBookingServiceTests
{
    private static readonly DateOnly Tarikh = new(2026, 7, 12);
    private static TimeOnly T(int jam, int minit = 0) => new(jam, minit);

    /// <summary>Bantu: seed satu tempahan dengan slot & status diberi.</summary>
    private static async Task<int> SeedBookingAsync(
        TestDbFactory f, int facilityId, DateOnly date,
        TimeOnly start, TimeOnly end, SubmissionStatus status,
        string rujukan = "TFS-2026-0001")
    {
        var s = new Submission
        {
            ModuleCode = ModuleCodes.TempahanFasilitiSukan,
            ApplicantUserId = "u1",
            ReferenceNo = rujukan,
            Status = status
        };
        f.Db.Submissions.Add(s);
        await f.Db.SaveChangesAsync();

        var a = new FacilityBookingApplication
        {
            SubmissionId = s.Id, FacilityId = facilityId, Purpose = "Ujian"
        };
        f.Db.Set<FacilityBookingApplication>().Add(a);
        await f.Db.SaveChangesAsync();

        f.Db.Set<FacilityBookingSlot>().Add(new FacilityBookingSlot
        {
            FacilityBookingApplicationId = a.Id, FacilityId = facilityId,
            BookingDate = date, StartTime = start, EndTime = end
        });
        await f.Db.SaveChangesAsync();

        return s.Id;
    }

    // ---------- KES YANG MESTI DISEKAT ----------

    [Fact]
    public async Task Slot_bertindih_aktif_DISEKAT()
    {
        using var f = new TestDbFactory();
        await SeedBookingAsync(f, 1, Tarikh, T(10), T(11), SubmissionStatus.Submitted);

        var hit = await new SlotBookingService(f.Db)
            .FindOverlapAsync(1, Tarikh, T(10, 30), T(11, 30));

        hit.Should().NotBeNull();
        hit!.ReferenceNo.Should().Be("TFS-2026-0001");
    }

    [Theory]
    [InlineData(SubmissionStatus.Submitted)]
    [InlineData(SubmissionStatus.AdminApproved)]
    public async Task Status_aktif_menyekat(SubmissionStatus status)
    {
        using var f = new TestDbFactory();
        await SeedBookingAsync(f, 1, Tarikh, T(10), T(11), status);

        var hit = await new SlotBookingService(f.Db)
            .FindOverlapAsync(1, Tarikh, T(10), T(11));

        hit.Should().NotBeNull();
    }

    // ---------- KES YANG MESTI DIBENARKAN ----------
    // Lebih penting daripada yang di atas.

    [Fact]
    public async Task Slot_bersebelahan_DIBENARKAN()
    {
        using var f = new TestDbFactory();
        await SeedBookingAsync(f, 1, Tarikh, T(10), T(11), SubmissionStatus.AdminApproved);

        var hit = await new SlotBookingService(f.Db)
            .FindOverlapAsync(1, Tarikh, T(11), T(12));   // bersebelahan

        hit.Should().BeNull("tempahan bersebelahan tidak bertindih");
    }

    [Fact]
    public async Task Tarikh_BERBEZA_dibenarkan()
    {
        using var f = new TestDbFactory();
        await SeedBookingAsync(f, 1, Tarikh, T(10), T(11), SubmissionStatus.Submitted);

        var hit = await new SlotBookingService(f.Db)
            .FindOverlapAsync(1, Tarikh.AddDays(1), T(10), T(11));

        hit.Should().BeNull("hari berbeza ialah slot berbeza");
    }

    [Fact]
    public async Task Fasiliti_BERBEZA_dibenarkan()
    {
        using var f = new TestDbFactory();
        await SeedBookingAsync(f, 1, Tarikh, T(10), T(11), SubmissionStatus.Submitted);

        var hit = await new SlotBookingService(f.Db)
            .FindOverlapAsync(2, Tarikh, T(10), T(11));   // fasiliti 2

        hit.Should().BeNull("fasiliti berbeza tidak berkongsi slot");
    }

    [Theory]
    [InlineData(SubmissionStatus.Rejected)]
    [InlineData(SubmissionStatus.Cancelled)]
    [InlineData(SubmissionStatus.Completed)]
    public async Task Status_terminal_TIDAK_menyekat(SubmissionStatus status)
    {
        using var f = new TestDbFactory();
        await SeedBookingAsync(f, 1, Tarikh, T(10), T(11), status);

        var hit = await new SlotBookingService(f.Db)
            .FindOverlapAsync(1, Tarikh, T(10), T(11));

        hit.Should().BeNull(
            "tempahan ditolak/dibatalkan mesti melepaskan slot untuk ditempah semula");
    }

    [Fact]
    public async Task Draf_TIDAK_menyekat()
    {
        using var f = new TestDbFactory();
        await SeedBookingAsync(f, 1, Tarikh, T(10), T(11), SubmissionStatus.Draft);

        var hit = await new SlotBookingService(f.Db)
            .FindOverlapAsync(1, Tarikh, T(10), T(11));

        hit.Should().BeNull();
    }

    [Fact]
    public async Task Tempahan_tidak_menyekat_DIRINYA_sendiri()
    {
        using var f = new TestDbFactory();
        var submissionId = await SeedBookingAsync(
            f, 1, Tarikh, T(10), T(11), SubmissionStatus.Submitted);

        var hit = await new SlotBookingService(f.Db)
            .FindOverlapAsync(1, Tarikh, T(10), T(11), kecualiSubmissionId: submissionId);

        hit.Should().BeNull("menghantar semula tempahan yang sama tidak boleh disekat");
    }
}
```

### ✅ Semakan

- [ ] Slot bertindih aktif disekat
- [ ] Kedua-dua status aktif (`Submitted`, `AdminApproved`) menyekat
- [ ] Bersebelahan **dibenarkan**
- [ ] Tarikh & fasiliti berbeza **dibenarkan**
- [ ] `Rejected`/`Cancelled`/`Completed` **tidak** menyekat
- [ ] `Draft` tidak menyekat
- [ ] `kecualiSubmissionId` berfungsi
- [ ] Setiap `Should()` mempunyai sebab bila tidak jelas

---

## Latihan 4 — Refactor: ekstrak peraturan tempahan boleh diuji

**Objektif:** Logik validation tempahan hidup dalam kelas boleh diuji, bukan terkubur dalam view model/controller.

### Langkah

1. Ekstrak peraturan masa ke `Services/Fasiliti/BookingRules.cs`:

```csharp
namespace Nres.Onboarding.Web.Services.Fasiliti;

/// <summary>
/// Peraturan kesahihan tempahan yang tulen & boleh diuji — diekstrak dari
/// view model supaya ia boleh diuji tanpa keseluruhan kitaran MVC.
/// </summary>
public static class BookingRules
{
    public const int MaksJam = 4;

    /// <summary>
    /// Sahkan slot terhadap waktu operasi & tempoh. Mengembalikan sebab
    /// ketidaksahihan, atau null jika sah.
    /// </summary>
    public static string? Validate(
        DateOnly bookingDate, TimeOnly start, TimeOnly end,
        TimeOnly openTime, TimeOnly closeTime, DateOnly today)
    {
        if (bookingDate < today)
            return "Tarikh tempahan tidak boleh dalam masa lampau.";
        if (end <= start)
            return "Masa tamat mesti selepas masa mula.";
        if (start < openTime || end > closeTime)
            return $"Tempahan mesti dalam waktu operasi ({openTime:HH\\:mm}–{closeTime:HH\\:mm}).";
        if ((end.ToTimeSpan() - start.ToTimeSpan()).TotalHours > MaksJam)
            return $"Satu tempahan tidak boleh melebihi {MaksJam} jam.";
        return null;
    }
}
```

2. Panggil `BookingRules.Validate(...)` dari `IValidatableObject` view model (gantikan peraturan sebaris) supaya logik wujud **satu tempat**.

3. `Nres.Onboarding.Tests/Fasiliti/BookingRulesTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Services.Fasiliti;

namespace Nres.Onboarding.Tests.Fasiliti;

public class BookingRulesTests
{
    private static readonly DateOnly HariIni = new(2026, 7, 12);
    private static readonly TimeOnly Buka = new(8, 0);
    private static readonly TimeOnly Tutup = new(22, 0);

    private static TimeOnly T(int j, int m = 0) => new(j, m);

    [Fact]
    public void Slot_sah_dalam_waktu_operasi()
    {
        BookingRules.Validate(HariIni, T(10), T(11), Buka, Tutup, HariIni)
            .Should().BeNull();
    }

    [Fact]
    public void Tarikh_lampau_ditolak()
    {
        BookingRules.Validate(HariIni.AddDays(-1), T(10), T(11), Buka, Tutup, HariIni)
            .Should().Contain("lampau");
    }

    [Fact]
    public void Tamat_sebelum_mula_ditolak()
    {
        BookingRules.Validate(HariIni, T(11), T(10), Buka, Tutup, HariIni)
            .Should().Contain("selepas masa mula");
    }

    [Theory]
    [InlineData(7, 0, 9, 0)]    // mula sebelum buka
    [InlineData(21, 0, 23, 0)]  // tamat selepas tutup
    public void Luar_waktu_operasi_ditolak(int sj, int sm, int tj, int tm)
    {
        BookingRules.Validate(HariIni, T(sj, sm), T(tj, tm), Buka, Tutup, HariIni)
            .Should().Contain("waktu operasi");
    }

    [Fact]
    public void Tempoh_melebihi_maksimum_ditolak()
    {
        BookingRules.Validate(HariIni, T(8), T(13), Buka, Tutup, HariIni)  // 5 jam
            .Should().Contain("melebihi");
    }

    [Fact]
    public void Sempadan_tepat_pada_waktu_tutup_sah()
    {
        // Tamat == waktu tutup dibenarkan (tutup ialah had atas termasuk).
        BookingRules.Validate(HariIni, T(20), T(22), Buka, Tutup, HariIni)
            .Should().BeNull();
    }
}
```

> **Kenapa lulus `today` sebagai parameter?** Supaya ujian deterministik — ia tidak bergantung pada tarikh mesin ujian. Corak yang sama patut digunakan di mana-mana logik bergantung "sekarang".

### ✅ Semakan

- [ ] `BookingRules` diekstrak ke `Services/Fasiliti/`
- [ ] View model memanggilnya (logik tidak diduplikasi)
- [ ] Ujian meliputi: tarikh lampau, tamat<mula, luar waktu, tempoh, sempadan
- [ ] `today` dilalukan (ujian deterministik)

---

## Latihan 5 — Ujian E2E manual

**Objektif:** Aliran penuh dari tempahan ke kelulusan ke slip.

### Langkah

Jalankan dan rekod dalam `docs/kumpulan-4/ujian-e2e.md`:

```markdown
# Ujian E2E — Kumpulan 4

## Aliran A: Tempahan & pertindihan
| # | Langkah | Peranan | Jangkaan | Keputusan |
|---|---------|---------|----------|-----------|
| A1 | Tempah Gelanggang A, 12/07 10:00–11:00 | Applicant | TFS-2026-#### dijana | |
| A2 | Tempah LAGI Gelanggang A, 12/07 10:30–11:30 | Applicant | Disekat, mesej namakan A1 | |
| A3 | Tempah Gelanggang A, 12/07 11:00–12:00 | Applicant | Berjaya (bersebelahan) | |
| A4 | Tempah Gelanggang A, 13/07 10:00–11:00 | Applicant | Berjaya (tarikh lain) | |
| A5 | Hantar tanpa akuan | Applicant | Ditolak validation | |

## Aliran B: Kelulusan & semak-semula
| # | Langkah | Peranan | Jangkaan | Keputusan |
|---|---------|---------|----------|-----------|
| B1 | Buka baris gilir | FacilityAdmin | Tempahan menunggu kelihatan | |
| B2 | Luluskan A1 | FacilityAdmin | AdminApproved | |
| B3 | Cipta tempahan bertindih (via DB), luluskan | FacilityAdmin | Disekat oleh semak-semula | |
| B4 | Luluskan dengan slot alternatif kosong | FacilityAdmin | Slot dikemas kini, diluluskan | |
| B5 | Tolak tanpa sebab | FacilityAdmin | Ditolak | |

## Aliran C: Kalendar, peringatan, eksport
| # | Langkah | Jangkaan | Keputusan |
|---|---------|----------|-----------|
| C1 | Dashboard minggu 12/07 | Grid tunjuk A1, A3 | |
| C2 | Hantar peringatan esok | Konsol tunjuk mesej peringatan | |
| C3 | Muat turun slip PDF tempahan diluluskan | PDF sah dengan butiran | |
| C4 | Eksport jadual Excel | .xlsx buka dalam Excel, aksara BM betul | |

## Aliran D: RBAC
| # | Langkah | Jangkaan | Keputusan |
|---|---------|----------|-----------|
| D1 | Applicant → /FacilityBooking/Queue | 403 | |
| D2 | Applicant → /FacilityBooking/Dashboard | 403 | |
| D3 | HrAdmin → /FacilityBooking/EksportJadual | 403 | |
| D4 | Applicant buka slip tempahan orang lain | 403 | |
```

**Bagi setiap kegagalan:** betulkan hari ini jika kecil; rekod dalam `status-akhir.md` jika tidak.

### ✅ Semakan

- [ ] Keempat-empat aliran dijalankan sepenuhnya
- [ ] A2, A3 mengesahkan pertindihan betul (disekat / bersebelahan dibenarkan)
- [ ] B3 mengesahkan semak-semula kelulusan
- [ ] D1–D4 memberi 403

---

## Latihan 6 — Prestasi & refactor

**Objektif:** Ukur, kemudian betulkan.

### Langkah

1. Hidupkan logging SQL:

```json
"Logging": { "LogLevel": {
  "Microsoft.EntityFrameworkCore.Database.Command": "Information" } }
```

2. Jana ~200 tempahan merentas fasiliti & tarikh, kemudian periksa setiap skrin:

| Skrin | Cari |
|-------|------|
| Semakan bertindih (`FindOverlapAsync`) | **Satu** query; guna indeks `(FacilityId, BookingDate)` |
| `/FacilityBooking/Queue` | Satu query, bukan N+1 per baris |
| `/FacilityBooking/Dashboard` | Satu query minggu, dikumpul dalam memori |
| Servis ketersediaan | Satu query per hari |

3. **Sahkan indeks digunakan.** Dalam log SQL untuk `FindOverlapAsync`, cari bahawa `WHERE` menapis `FacilityId` dan `BookingDate` (indeks komposit Hari 4). Rekod dalam `docs/kumpulan-4/prestasi.md`:

```markdown
# Semakan prestasi — Kumpulan 4

## Semakan pertindihan slot
Query: satu SELECT dengan join slot→application→submission.
Indeks: IX_Slots_Facility_Date (FacilityId, BookingDate).
Diukur pada 200 tempahan: <n> ms.
Keputusan: KEKAL. Indeks menapis ke satu hari satu fasiliti sebelum
menyaring julat masa. Set data NRES ratusan, bukan ratusan ribu.

## Dashboard minggu
Pendekatan: satu query julat + GroupBy dalam memori.
Keputusan: KEKAL untuk saiz semasa.
```

> Mendokumenkan **kenapa anda tidak mengubahnya** sama berharga dengan mendokumentasikan optimasi.

4. **Refactor:**

| Sasaran | Cara |
|---------|------|
| Peraturan waktu operasi dalam view model | Ekstrak ke `BookingRules` (Latihan 4) |
| Predikat pertindihan didup (memori vs SQL) | Pastikan komen mengaitkan kedua-duanya; ujian mengesahkan sepadan |
| Rentetan ajaib (`4` jam, status) | Pemalar (`BookingRules.MaksJam`, `StatusAktif`) |
| Kod jana-AI yang tiada siapa faham | Fahami atau buang |

5. Betulkan setiap amaran pengkompil:

```bash
dotnet build 2>&1 | grep -i warning
```

### ✅ Semakan

- [ ] Semua skrin diperiksa dengan logging SQL
- [ ] Semakan bertindih ialah **satu** query menggunakan indeks komposit
- [ ] Tiada N+1
- [ ] Keputusan prestasi didokumenkan (termasuk yang **tidak** diubah)
- [ ] `BookingRules` diekstrak
- [ ] Sifar amaran pengkompil
- [ ] Semua ujian masih lulus

---

## Latihan 7 — Dokumentasi modul & sedia gabung

### Langkah

1. `docs/kumpulan-4/README-modul.md`:

```markdown
# Modul Tempahan Fasiliti Sukan (Kumpulan 4)

## Apa yang dilakukannya
Menguruskan tempahan gelanggang & kemudahan sukan NRES — katalog fasiliti,
borang tempahan slot masa, semakan pertindihan slot, kalendar ketersediaan,
kelulusan FacilityAdmin dengan semak-semula pertindihan & peruntukan slot,
peringatan, dashboard, dan eksport PDF/Excel.

## Jadual
- `SportsFacilities` — katalog fasiliti (data seed); UNIK pada `Name`
- `FacilityBookingApplications` — permohonan tempahan (terikat Submission + Facility)
- `FacilityBookingSlots` — slot ditempah; INDEKS (FacilityId, BookingDate)

## Laluan
| Laluan | Peranan | Tujuan |
|--------|---------|--------|
| `/FacilityBooking` | Applicant | Halaman utama + tempahan saya |
| `/FacilityBooking/Create` | Applicant | Borang tempahan |
| `/FacilityBooking/Queue` | FacilityAdmin | Baris gilir kelulusan |
| `/FacilityBooking/Review` | FacilityAdmin | Skrin semakan + kalendar hari |
| `/FacilityBooking/Dashboard` | FacilityAdmin | Kalendar mingguan |
| `/FacilityBooking/Slip` | Applicant/FacilityAdmin | Slip PDF |
| `/FacilityBooking/EksportJadual` | FacilityAdmin | Jadual Excel |

## Servis
- `IFacilityCatalogService` — katalog fasiliti
- `ISlotBookingService` + `SlotOverlap` — **semakan pertindihan (teras)**
- `IAvailabilityService` — kalendar ketersediaan
- `IBookingReviewService` — baris gilir
- `IBookingReminderService` (+ BackgroundService) — peringatan
- `IBookingSlipPdfService` (QuestPDF) · `IBookingScheduleExcelService` (ClosedXML)
- `BookingRules` — validation tempahan boleh diuji

## Aliran status
Draft → Submitted → AdminApproved | Rejected
Peruntukan slot alternatif = `AdminApproved` + slot dikemas kini + `AllocationNote`
(**tiada** ahli `SubmissionStatus` baharu)

## ⚠️ Yang perlu diketahui kumpulan lain
- Kami menambah pakej **QuestPDF** & **ClosedXML** ke `.csproj`
- Kami **mengatasi** `SubmissionControllerBase.Approve` untuk semak-semula
  pertindihan — `base.Approve` sentiasa dipanggil
- Kami mendaftar `BackgroundService` melalui `AddFasilitiModule()`
- Lesen QuestPDF (Community) ditetapkan dalam pembina statik servis PDF
- **Tiada indeks unik untuk pertindihan** — semakan aplikasi ialah satu-satunya
  pertahanan (sebab semak-semula pada kelulusan)
- Prefix rujukan: TFS

## Diketahui belum siap
- <senarai jujur>
```

2. Gabungan kering:

```bash
git switch master && git pull --rebase origin master
git switch -c ujian/gabungan-kering-k4
git merge kump-4/tempahan-fasiliti --no-commit --no-ff
# semak konflik, kemudian:
git merge --abort
git switch master && git branch -D ujian/gabungan-kering-k4
```

Selesaikan sebarang konflik **dalam cabang anda hari ini**.

3. `docs/kumpulan-4/status-akhir.md`:

```markdown
# Status akhir — Kumpulan 4

## Sedia untuk Hari 15
- [x] dotnet build bersih
- [x] dotnet test — <n> ujian lulus
- [x] Digabung dengan master terkini
- [x] Gabungan kering tiada konflik
- [x] README-modul.md ditulis

## Diketahui belum siap
- <jujur>

## Nota untuk SIT Hari 15
- Akaun ujian: applicant@nres.test / fasiliti@nres.test
- Aliran demo: tempah slot → cuba bertindih → luluskan → slip PDF → dashboard
- Fasiliti berseed: Gelanggang Badminton A/B, Sepak Takraw, Tenis, Dewan, Padang, Gim
- Demo pertindihan paling berkesan: dua tempahan bersebelahan (dibenarkan) vs bertindih (disekat)
```

### ✅ Semakan

- [ ] `README-modul.md` menyatakan pakej QuestPDF/ClosedXML & `override` Approve
- [ ] Nota "tiada indeks unik untuk pertindihan" direkod untuk kumpulan lain
- [ ] Gabungan kering tiada konflik
- [ ] `status-akhir.md` jujur
- [ ] PR akhir digabung

---

## Deliverable Hari 13–14

| Artifak | Lokasi |
|---------|--------|
| Ujian predikat `SlotOverlap` | `Nres.Onboarding.Tests/Fasiliti/` |
| Ujian servis bertindih (disekat + dibenarkan) | `Nres.Onboarding.Tests/Fasiliti/` |
| Ujian `BookingRules` | `Nres.Onboarding.Tests/Fasiliti/` |
| `BookingRules` diekstrak | `Services/Fasiliti/` |
| Rekod ujian E2E | `docs/kumpulan-4/ujian-e2e.md` |
| Penemuan prestasi | `docs/kumpulan-4/prestasi.md` |
| Dokumentasi modul | `docs/kumpulan-4/README-modul.md` |
| Status akhir | `docs/kumpulan-4/status-akhir.md` |

## Latihan 8 — Pemecut QA dipandu-AI (Claude Code)

**Objektif:** Guna subagent `qa-uat` + skill `/uji-modul` untuk mempercepat ujian blok ini, dan menaik taraf ujian E2E manual (Latihan 5) ke pre-check pelayar dipandu-AI. Lab penuh: [`lab-qa-ai-uat.md`](../../../docs/lab-qa-ai-uat.md).

> 🔧 **Khusus Claude Code.** Guna alat AI lain? Langkau — Latihan 5 (E2E manual) sudah memadai.

### Langkah

1. Cipta (sekali) `.claude/agents/qa-uat.md` + `.claude/skills/uji-modul/SKILL.md` (salin templat kursus [`.claude/agents/`](../../../.claude/agents/)).
2. **Lengkapkan ujian** dengan `qa-uat` (UJI-01):

   ```text
   Guna subagent qa-uat: lengkapkan ujian xUnit — predikat pertindihan slot dua arah
   (dengan status), dan peraturan tempahan. Guna SQLite in-memory (bukan UseInMemoryDatabase).
   Jalankan dotnet test, tunjuk keputusan.
   ```

3. **Pre-check pelayar** menggantikan langkah manual Latihan 5 — jalankan app repo `tempahan-fasiliti-sukan` anda:

   ```text
   Guna subagent qa-uat: pre-check di app tempahan-fasiliti-sukan — log masuk applicant →
   tempah slot; cuba slot bertindih (mesti disekat), bersebelahan (mesti lepas); log masuk
   FacilityAdmin → luluskan. Sahkan applicant ke halaman semakan mesti 403. Rekod lulus/gagal + GIF.
   Jangan cetuskan dialog.
   ```

### ✅ Semakan

- [ ] `qa-uat` + `/uji-modul` wujud; `dotnet test` hijau
- [ ] Pertindihan slot dua arah (bertindih disekat, bersebelahan lepas) dilindungi
- [ ] RBAC 403 disahkan; keputusan direkod (ini **pre-check**, bukan UAT sebenar)

---

**Esok (Hari 15):** empat cabang bergabung. Demo pertindihan slot anda — bersebelahan dibenarkan, bertindih disekat — ialah salah satu demo paling jelas dalam kursus.
