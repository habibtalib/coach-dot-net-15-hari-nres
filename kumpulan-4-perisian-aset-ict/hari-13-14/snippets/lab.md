# Lab · Kumpulan 4 · Hari 13–14 — Ujian, Refactor & Sedia Gabung

> Konsep: [`../README.md`](../README.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md)
>
> **Tiada ciri baharu dalam blok ini.**

---

## Latihan 0 — Bekukan skop

```bash
git switch kump-4/perisian-aset
git pull --rebase origin master
git switch -c kump-4/feat/ujian-dan-refactor
dotnet build
```

Tandakan setiap isu backlog: ✅ · 🔧 · ⏸️. Rekod dalam `docs/kumpulan-4/status-akhir.md`.

### ✅ Semakan

- [ ] Setiap isu ditandakan
- [ ] Endpoint ujian "jalankan peringatan sekarang" dilindungi peranan atau dibuang

---

## Latihan 1 — Uji kiraan inventori

**Objektif:** Asas semua semakan stok.

### Langkah

`Nres.Onboarding.Tests/Aset/InventoryTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Aset;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services.Aset;

namespace Nres.Onboarding.Tests.Aset;

public class InventoryTests
{
    private static async Task<int> SeedSoftwareAsync(
        TestDbFactory f, int? jumlahLesen, string nama = "AutoCAD")
    {
        var sw = new SoftwareCatalogItem
        {
            Nama = nama, JenisLesen = JenisLesen.PerPengguna, JumlahLesen = jumlahLesen
        };
        f.Db.Set<SoftwareCatalogItem>().Add(sw);
        await f.Db.SaveChangesAsync();
        return sw.Id;
    }

    private static async Task SeedRequestAsync(
        TestDbFactory f, int softwareId, SubmissionStatus status, string userId = "u1")
    {
        var s = new Submission
        {
            ModuleCode = ModuleCodes.Perisian, ApplicantUserId = userId,
            ReferenceNo = $"SW-2026-{Guid.NewGuid().ToString()[..4]}", Status = status
        };
        f.Db.Submissions.Add(s);
        await f.Db.SaveChangesAsync();

        f.Db.Set<SoftwareRequest>().Add(new SoftwareRequest
        {
            SubmissionId = s.Id, SoftwareCatalogItemId = softwareId,
            Justifikasi = "ujian"
        });
        await f.Db.SaveChangesAsync();
    }

    [Fact]
    public async Task Lesen_tanpa_permohonan_penuh_tersedia()
    {
        using var f = new TestDbFactory();
        var id = await SeedSoftwareAsync(f, jumlahLesen: 5);

        var status = await new InventoryService(f.Db).LicenceStatusAsync(id);

        status.Diguna.Should().Be(0);
        status.Baki.Should().Be(5);
        status.Tersedia.Should().BeTrue();
    }

    [Theory]
    [InlineData(SubmissionStatus.Submitted)]
    [InlineData(SubmissionStatus.AdminApproved)]
    [InlineData(SubmissionStatus.Completed)]
    public async Task Permohonan_aktif_MENGGUNAKAN_lesen(SubmissionStatus status)
    {
        using var f = new TestDbFactory();
        var id = await SeedSoftwareAsync(f, jumlahLesen: 5);
        await SeedRequestAsync(f, id, status);

        var hasil = await new InventoryService(f.Db).LicenceStatusAsync(id);

        hasil.Diguna.Should().Be(1);
        hasil.Baki.Should().Be(4);
    }

    [Theory]
    [InlineData(SubmissionStatus.Draft)]
    [InlineData(SubmissionStatus.Rejected)]
    [InlineData(SubmissionStatus.Cancelled)]
    public async Task Permohonan_TIDAK_aktif_tidak_menggunakan_lesen(SubmissionStatus status)
    {
        using var f = new TestDbFactory();
        var id = await SeedSoftwareAsync(f, jumlahLesen: 5);
        await SeedRequestAsync(f, id, status);

        var hasil = await new InventoryService(f.Db).LicenceStatusAsync(id);

        hasil.Diguna.Should().Be(0, "draf/ditolak/dibatalkan tidak memegang lesen");
        hasil.Baki.Should().Be(5);
    }

    [Fact]
    public async Task Lesen_habis_TIDAK_tersedia()
    {
        using var f = new TestDbFactory();
        var id = await SeedSoftwareAsync(f, jumlahLesen: 2);
        await SeedRequestAsync(f, id, SubmissionStatus.AdminApproved, "u1");
        await SeedRequestAsync(f, id, SubmissionStatus.AdminApproved, "u2");

        var hasil = await new InventoryService(f.Db).LicenceStatusAsync(id);

        hasil.Baki.Should().Be(0);
        hasil.Tersedia.Should().BeFalse();
    }

    [Fact]
    public async Task Lesen_TANPA_HAD_sentiasa_tersedia()
    {
        using var f = new TestDbFactory();
        var id = await SeedSoftwareAsync(f, jumlahLesen: null, nama: "7-Zip");

        for (var i = 0; i < 50; i++)
            await SeedRequestAsync(f, id, SubmissionStatus.AdminApproved, $"u{i}");

        var hasil = await new InventoryService(f.Db).LicenceStatusAsync(id);

        hasil.Diguna.Should().Be(50);
        hasil.Baki.Should().BeNull("tanpa had tiada baki");
        hasil.Tersedia.Should().BeTrue("perisian tanpa had tidak pernah habis");
    }

    [Fact]
    public async Task Aset_tersedia_menapis_mengikut_status_dan_kategori()
    {
        using var f = new TestDbFactory();

        f.Db.Set<Asset>().AddRange(
            new Asset { AssetTag = "LT-1", SerialNumber = "S1", Kategori = KategoriAset.Laptop,    Nama = "L", Status = AssetStatus.Available },
            new Asset { AssetTag = "LT-2", SerialNumber = "S2", Kategori = KategoriAset.Laptop,    Nama = "L", Status = AssetStatus.OnLoan },
            new Asset { AssetTag = "LT-3", SerialNumber = "S3", Kategori = KategoriAset.Laptop,    Nama = "L", Status = AssetStatus.UnderMaintenance },
            new Asset { AssetTag = "LT-4", SerialNumber = "S4", Kategori = KategoriAset.Laptop,    Nama = "L", Status = AssetStatus.Lost },
            new Asset { AssetTag = "PJ-1", SerialNumber = "S5", Kategori = KategoriAset.Projektor, Nama = "P", Status = AssetStatus.Available });
        await f.Db.SaveChangesAsync();

        var laptop = await new InventoryService(f.Db).AvailableAssetsAsync(KategoriAset.Laptop);

        laptop.Should().HaveCount(1);
        laptop[0].AssetTag.Should().Be("LT-1");
    }
}
```

### ✅ Semakan

- [ ] Status aktif menggunakan lesen; tidak aktif tidak
- [ ] Lesen **tanpa had** sentiasa tersedia
- [ ] Aset `OnLoan`/`UnderMaintenance`/`Lost` dikecualikan daripada tersedia
- [ ] `dotnet test` hijau

---

## Latihan 2 — Uji peruntukan & perlumbaan

**Objektif:** Buktikan perlindungan inventori berfungsi.

### Langkah

`Nres.Onboarding.Tests/Aset/AllocationTests.cs`:

```csharp
using FluentAssertions;
using Nres.Onboarding.Web.Models.Aset;
using Nres.Onboarding.Web.Models.Shared;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.Services.Aset;

namespace Nres.Onboarding.Tests.Aset;

public class AllocationTests
{
    private static AssetAllocationService Buat(TestDbFactory f)
    {
        var currentUser = new FakeCurrentUser("ict1", "IctAdmin");
        var audit = new AuditLogService(f.Db, currentUser);
        var workflow = new WorkflowService(f.Db, audit);
        return new AssetAllocationService(f.Db, workflow, audit);
    }

    private static async Task<(int loanId, int assetId)> SeedAsync(
        TestDbFactory f, AssetStatus statusAset = AssetStatus.Available,
        KategoriAset kategori = KategoriAset.Laptop)
    {
        var aset = new Asset
        {
            AssetTag = "NRES-LT-0001", SerialNumber = "S1",
            Kategori = kategori, Nama = "Laptop", Status = statusAset
        };
        f.Db.Set<Asset>().Add(aset);

        var s = new Submission
        {
            ModuleCode = ModuleCodes.PinjamanAset, ApplicantUserId = "u1",
            ReferenceNo = "AST-L-2026-0001", Status = SubmissionStatus.Submitted
        };
        f.Db.Submissions.Add(s);
        await f.Db.SaveChangesAsync();

        var pinjaman = new AssetLoanRequest
        {
            SubmissionId = s.Id, KategoriDipohon = KategoriAset.Laptop,
            Justifikasi = "ujian",
            TarikhPinjam = DateTime.Today,
            TarikhJangkaPulang = DateTime.Today.AddDays(30)
        };
        f.Db.Set<AssetLoanRequest>().Add(pinjaman);
        await f.Db.SaveChangesAsync();

        return (pinjaman.Id, aset.Id);
    }

    [Fact]
    public async Task Peruntukan_berjaya_menukar_status_aset()
    {
        using var f = new TestDbFactory();
        var (loanId, assetId) = await SeedAsync(f);

        var hasil = await Buat(f).AllocateAsync(loanId, assetId, "OK");

        hasil.Berjaya.Should().BeTrue();
        hasil.AssetTag.Should().Be("NRES-LT-0001");

        f.Db.ChangeTracker.Clear();
        var aset = f.Db.Set<Asset>().Single();
        aset.Status.Should().Be(AssetStatus.OnLoan);

        var pinjaman = f.Db.Set<AssetLoanRequest>().Single();
        pinjaman.AssetId.Should().Be(assetId);

        var submission = f.Db.Submissions.Single();
        submission.Status.Should().Be(SubmissionStatus.AdminApproved);
    }

    // ---------- PERLINDUNGAN PERLUMBAAN ----------

    [Theory]
    [InlineData(AssetStatus.OnLoan)]
    [InlineData(AssetStatus.UnderMaintenance)]
    [InlineData(AssetStatus.Lost)]
    [InlineData(AssetStatus.Retired)]
    public async Task Peruntukan_aset_TIDAK_tersedia_gagal(AssetStatus status)
    {
        using var f = new TestDbFactory();
        var (loanId, assetId) = await SeedAsync(f, statusAset: status);

        var hasil = await Buat(f).AllocateAsync(loanId, assetId, null);

        hasil.Berjaya.Should().BeFalse();
        hasil.Sebab.Should().Contain("tidak lagi tersedia");
    }

    [Fact]
    public async Task Peruntukan_kedua_bagi_aset_sama_GAGAL()
    {
        using var f = new TestDbFactory();
        var (loanId1, assetId) = await SeedAsync(f);
        var servis = Buat(f);

        // Pinjaman kedua untuk aset yang sama.
        var s2 = new Submission
        {
            ModuleCode = ModuleCodes.PinjamanAset, ApplicantUserId = "u2",
            ReferenceNo = "AST-L-2026-0002", Status = SubmissionStatus.Submitted
        };
        f.Db.Submissions.Add(s2);
        await f.Db.SaveChangesAsync();

        var pinjaman2 = new AssetLoanRequest
        {
            SubmissionId = s2.Id, KategoriDipohon = KategoriAset.Laptop,
            Justifikasi = "ujian 2"
        };
        f.Db.Set<AssetLoanRequest>().Add(pinjaman2);
        await f.Db.SaveChangesAsync();

        (await servis.AllocateAsync(loanId1, assetId, null)).Berjaya.Should().BeTrue();

        f.Db.ChangeTracker.Clear();

        // Ini yang berlaku apabila dua pentadbir memilih unit yang sama.
        var kedua = await servis.AllocateAsync(pinjaman2.Id, assetId, null);

        kedua.Berjaya.Should().BeFalse("aset telah diambil oleh peruntukan pertama");
    }

    [Fact]
    public async Task Peruntukan_kategori_SALAH_gagal()
    {
        using var f = new TestDbFactory();
        // Aset ialah Projektor; permohonan meminta Laptop.
        var (loanId, assetId) = await SeedAsync(f, kategori: KategoriAset.Projektor);

        var hasil = await Buat(f).AllocateAsync(loanId, assetId, null);

        hasil.Berjaya.Should().BeFalse();
        hasil.Sebab.Should().Contain("Projektor");
    }

    [Fact]
    public async Task Peruntukan_gagal_TIDAK_meninggalkan_perubahan_separa()
    {
        using var f = new TestDbFactory();
        var (loanId, assetId) = await SeedAsync(f, statusAset: AssetStatus.OnLoan);

        await Buat(f).AllocateAsync(loanId, assetId, null);

        f.Db.ChangeTracker.Clear();

        // Tiada apa sepatutnya berubah.
        f.Db.Set<AssetLoanRequest>().Single().AssetId.Should().BeNull();
        f.Db.Submissions.Single().Status.Should().Be(SubmissionStatus.Submitted);
        f.Db.Set<Asset>().Single().Status.Should().Be(AssetStatus.OnLoan);
    }
}
```

> **`ChangeTracker.Clear()`** memastikan anda membaca daripada pangkalan data, bukan daripada cache dalam memori EF Core. Tanpanya, ujian boleh lulus secara palsu.

### ✅ Semakan

- [ ] Peruntukan berjaya menukar ketiga-tiga rekod
- [ ] Aset tidak tersedia (4 status) ditolak
- [ ] Peruntukan kedua bagi aset sama **gagal**
- [ ] Kategori salah ditolak
- [ ] Kegagalan meninggalkan **sifar** perubahan separa
- [ ] `ChangeTracker.Clear()` digunakan sebelum pengesahan

---

## Latihan 3 — Uji pemulangan & peringatan

### Langkah

```csharp
public class ReturnTests
{
    [Theory]
    [InlineData(KondisiPulangan.Baik,   AssetStatus.Available)]
    [InlineData(KondisiPulangan.Rosak,  AssetStatus.UnderMaintenance)]
    [InlineData(KondisiPulangan.Hilang, AssetStatus.Lost)]
    public async Task Kondisi_menentukan_status_aset(
        KondisiPulangan kondisi, AssetStatus dijangka)
    {
        using var f = new TestDbFactory();
        var returnId = await SeedPulanganAsync(f);

        await Buat(f).ReturnAsync(returnId, kondisi, "catatan", "ict1");

        f.Db.ChangeTracker.Clear();
        f.Db.Set<Asset>().Single().Status.Should().Be(dijangka);
    }

    [Fact]
    public async Task Aset_hilang_KEKAL_dalam_pangkalan_data()
    {
        using var f = new TestDbFactory();
        var returnId = await SeedPulanganAsync(f);

        await Buat(f).ReturnAsync(returnId, KondisiPulangan.Hilang, "tidak dijumpai", "ict1");

        f.Db.ChangeTracker.Clear();
        // Rekod KEKAL — audit memerlukannya, dan aset masih dalam daftar NRES.
        f.Db.Set<Asset>().Should().HaveCount(1);
        f.Db.Set<Asset>().Single().Status.Should().Be(AssetStatus.Lost);
    }

    // ... SeedPulanganAsync helper
}

public class OverdueTests
{
    [Theory]
    [InlineData(-5, 0, null)]                            // belum masanya
    [InlineData(-2, 0, TahapPeringatan.Awal)]            // 3 hari sebelum
    [InlineData( 0, 0, TahapPeringatan.PadaTarikh)]      // pada tarikh
    [InlineData( 8, 0, TahapPeringatan.Eskalasi)]        // lewat 8 hari
    [InlineData( 8, 3, null)]                            // eskalasi SUDAH dihantar
    [InlineData( 8, 2, TahapPeringatan.Eskalasi)]        // naik taraf dari PadaTarikh
    public async Task Tahap_peringatan_dikira_dengan_betul(
        int hariDariTarikh, int tahapDihantar, TahapPeringatan? dijangka)
    {
        using var f = new TestDbFactory();
        await SeedPinjamanAsync(f,
            tarikhJangkaPulang: DateTime.UtcNow.Date.AddDays(-hariDariTarikh),
            tahapDihantar: tahapDihantar);

        var perlu = await new OverdueService(f.Db).FindDueRemindersAsync();

        if (dijangka is null)
            perlu.Should().BeEmpty();
        else
        {
            perlu.Should().HaveCount(1);
            perlu[0].Tahap.Should().Be(dijangka);
        }
    }

    // ... SeedPinjamanAsync helper
}
```

> **Kes `[InlineData(8, 3, null)]`** ialah ujian anti-spam: pinjaman lewat 8 hari yang sudah menerima eskalasi tidak menerima satu lagi.

### ✅ Semakan

- [ ] Ketiga-tiga kondisi memetakan ke status yang betul
- [ ] Aset `Lost` kekal dalam DB
- [ ] Keenam-enam kes tahap peringatan lulus
- [ ] Ujian anti-spam lulus

---

## Latihan 4 — Prestasi

### Langkah

1. Hidupkan logging SQL, jana ~200 permohonan, periksa setiap skrin:

| Skrin | Cari |
|-------|------|
| `/Aset` | `AllLicenceStatusAsync` — **satu** query kumpulan, bukan 8+ |
| `/Aset/Dashboard` | Pengagregatan dalam SQL |
| `/Aset/Queue` | Tiga query, bukan N+1 |
| `EksportExcel` | Berapa query untuk membina buku kerja? |

2. **Semak semula keputusan Hari 4** (kiraan dikira vs disimpan):

```markdown
# Semakan prestasi — Kumpulan 4

## Kiraan lesen: dikira vs disimpan
Keputusan Hari 4: KIRA (elak medan tidak segerak).
Diukur pada 8 perisian × 200 permohonan: <n> ms, 1 query kumpulan.
Keputusan: KEKAL.
Ambang: jika katalog melebihi ~200 item ATAU permohonan melebihi ~50,000,
semak semula — pertimbangkan medan kiraan dengan kemas kini transaksi.

## Baris gilir (tiga query disatukan dalam memori)
Diukur: <n> ms, 3 query + 1 profil.
Keputusan: KEKAL — sama seperti Kumpulan 2.

## Eksport Excel
<n> query, <n> ms untuk 200 aset.
Nota: eksport ialah operasi jarang; tiada optimasi diperlukan.
```

3. Betulkan sebarang N+1 yang ditemui.

### ✅ Semakan

- [ ] Empat skrin diperiksa dengan logging SQL
- [ ] `AllLicenceStatusAsync` menggunakan satu query kumpulan
- [ ] Keputusan didokumenkan **dengan ambang**
- [ ] Semua ujian masih lulus

---

## Latihan 5 — Refactor & dokumentasi

### Langkah

1. Betulkan amaran pengkompil; semak kod jana-AI.

2. `docs/kumpulan-4/README-modul.md`:

```markdown
# Modul Perisian & Aset ICT (Kumpulan 4)

## Apa yang dilakukannya
Menguruskan permohonan lesen perisian dan pinjaman aset ICT — dari katalog
dan semakan stok, melalui kelulusan dengan peruntukan aset atomik, hingga
pemulangan dengan pemeriksaan kondisi dan kemas kini inventori automatik.

## Jadual
- `Assets` — perkakasan individu; UNIK pada `AssetTag` dan `SerialNumber`
- `SoftwareCatalogItems` — perisian, dijejak dengan KIRAAN lesen
- `SoftwareRequests`, `AssetLoanRequests`, `AssetReturns`

## ⚠️ Yang perlu diketahui kumpulan lain

**1. Aset mempunyai STATUS SENDIRI** (`AssetStatus`) yang berasingan daripada
`SubmissionStatus`. Permohonan boleh `Rejected` sementara asetnya kekal
`OnLoan` kepada orang lain. Jangan campurkan.

**2. Kami menambah pakej ClosedXML** ke `.csproj` (eksport Excel).

**3. ⚠️ Kami mendaftar `IHostedService`** (`OverdueReminderService`).
Ia BERJALAN selepas gabungan dan menghantar e-mel peringatan setiap 24 jam.
Jika anda melihat e-mel peringatan semasa ujian, itu kami.
Had diketahui: tidak berjalan jika aplikasi tidur; pendua jika berbilang contoh.

**4. Kelulusan pinjaman TIDAK menggunakan `base.Approve`** — peralihan status
mesti berada dalam transaksi yang sama dengan peruntukan aset.
Kelulusan perisian MEMANG menggunakan `base.Approve`.

## Laluan
| Laluan | Peranan | Tujuan |
|--------|---------|--------|
| `/Aset` | Applicant | Katalog + permohonan saya |
| `/Software/Create` | Applicant | Mohon lesen |
| `/Asset/Create` | Applicant | Mohon pinjaman |
| `/Asset/CreateReturn` | Applicant | Rekod pemulangan |
| `/Asset/Queue` | IctAdmin | Baris gilir (3 jenis) |
| `/Asset/Dashboard` | IctAdmin | Papan pemuka inventori |
| `/Asset/EksportExcel` | IctAdmin | Laporan Excel 3 helaian |

## Servis
- `IInventoryService` — ketersediaan aset, kiraan lesen
- `IEligibilityService` — peraturan kelayakan permohonan
- `IAssetAllocationService` — **peruntukan & pemulangan dalam transaksi**
- `IOverdueService` — pengesanan lewat tempoh
- `IAssetReportService` — eksport Excel

## Pengesyoran
- Ganti `BackgroundService` dengan penjadual luaran untuk pengeluaran
- Pertimbangkan token concurrency (`[Timestamp]`) pada `Asset`
- Lesen "Serentak" dikira sebagai per-pengguna — had diketahui

## Diketahui belum siap
- <senarai jujur>
```

3. Gabungan kering:

```bash
git switch master && git pull --rebase origin master
git switch -c ujian/gabungan-kering-k4
git merge kump-4/perisian-aset --no-commit --no-ff
git merge --abort
git switch master && git branch -D ujian/gabungan-kering-k4
```

### ✅ Semakan

- [ ] Sifar amaran pengkompil
- [ ] `README-modul.md` menyatakan keempat-empat perkara penting
- [ ] **Amaran `IHostedService` dinyatakan dengan jelas**
- [ ] Gabungan kering tiada konflik

---

## Deliverable Hari 13–14

| Artifak | Lokasi |
|---------|--------|
| Ujian inventori & kiraan lesen | `Nres.Onboarding.Tests/Aset/` |
| Ujian peruntukan & perlumbaan | Sama |
| Ujian pemulangan & peringatan | Sama |
| Penemuan prestasi dengan ambang | `docs/kumpulan-4/prestasi.md` |
| Dokumentasi modul | `docs/kumpulan-4/README-modul.md` |
| Status akhir | `docs/kumpulan-4/status-akhir.md` |

**Esok (Hari 15):** empat cabang bergabung. Demo anda ialah kitaran penuh: mohon → luluskan → akui → pulangkan rosak → sahkan aset ke penyelenggaraan.
