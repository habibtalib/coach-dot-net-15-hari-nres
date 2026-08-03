# Kumpulan 4 · Hari 10–12 — Peringatan Lewat Tempoh, Dashboard Inventori & Eksport

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Tiga hari. Hujungnya, sistem memberi amaran automatik tentang pinjaman yang hampir/melebihi tempoh, ICT mempunyai papan pemuka inventori, dan laporan boleh dieksport ke Excel.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `BackgroundService` | [learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) |
| Skop servis dalam hosted service | [learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services#consuming-a-scoped-service-in-a-background-task](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) |
| ClosedXML | [github.com/ClosedXML/ClosedXML/wiki](https://github.com/ClosedXML/ClosedXML/wiki) |
| Pengelompokan LINQ | Buku Bab 11 (m.s. 596) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 10** | Pengesanan lewat tempoh + peringatan automatik |
| **Hari 11** | Papan pemuka inventori |
| **Hari 12** | Eksport Excel + laporan bercetak |

**Hasil:** Peringatan dihantar secara automatik untuk pinjaman hampir/melebihi tempoh; ICT melihat keadaan inventori sepintas lalu; laporan aset boleh dieksport ke Excel.

---

## Tiga tahap peringatan

| Bila | Kepada | Nada |
|------|--------|------|
| **3 hari sebelum** tarikh jangka pulang | Pemohon | Peringatan mesra |
| **Pada** tarikh jangka pulang | Pemohon | "Sila pulangkan hari ini" |
| **7 hari selepas** tarikh | Pemohon **+ ICT** | Eskalasi |

**Kenapa tiga dan bukan satu?** Peringatan tunggal pada tarikh tamat menganggap orang menyemak e-mel pada hari itu. Amaran awal menangkap orang yang terlupa; eskalasi menangkap orang yang mengabaikan.

> **Elakkan spam.** Setiap tahap dihantar **sekali sahaja** setiap pinjaman. Kita menjejak apa yang telah dihantar — jika tidak, tugas harian menghantar peringatan yang sama setiap hari, dan orang berhenti membacanya.

## Tugas latar belakang: mengapa dan hadnya

Peringatan memerlukan sesuatu yang berjalan **tanpa pengguna**. Dalam ASP.NET Core, itu `BackgroundService`.

```csharp
public class OverdueReminderService : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken ct) { ... }
}
```

**Had yang mesti anda fahami:**

| Had | Kesan |
|-----|-------|
| Berjalan dalam proses aplikasi | Aplikasi tidur = tiada peringatan |
| Berbilang contoh = berbilang penghantar | Peringatan pendua dalam kluster |
| Mula semula = pemasa dimulakan semula | Boleh terlepas atau mengulangi larian |

Untuk pengeluaran sebenar, penjadual luaran (Hangfire, cron + endpoint, Azure Function) lebih dipercayai. **Kita menggunakan `BackgroundService` kerana ia terbina dan mengajar corak** — dan kita **mendokumenkan hadnya** dalam serahan.

> Ini corak yang berulang dalam kursus: bina sesuatu yang berfungsi, fahami hadnya, dan **nyatakannya**.

## Dashboard inventori: soalan pengurusan aset

| Soalan | Paparan |
|--------|---------|
| Berapa aset kita ada, mengikut status? | Kiraan: Available / OnLoan / UnderMaintenance / Lost |
| Kategori mana kehabisan stok? | Kategori dengan 0 tersedia |
| Apa yang lewat tempoh sekarang? | Senarai kerja, disusun paling lewat dahulu |
| Berapa nilai aset yang dipinjam? | Jumlah `Harga` bagi aset `OnLoan` |
| Lesen mana hampir habis? | Baki ≤ 2 |
| Berapa aset hilang tahun ini? | Kiraan `Lost` + jumlah nilai |

**Dua baris terakhir menarik perhatian pengurusan.** Nilai aset yang hilang ialah nombor yang muncul dalam mesyuarat.

## Kenapa Excel dan bukan hanya CSV

Kumpulan 2 mengeksport CSV. Anda mengeksport **Excel**. Perbezaannya penting untuk laporan aset:

| CSV | Excel (ClosedXML) |
|-----|-------------------|
| Satu helaian | **Berbilang helaian** — aset, lesen, lewat tempoh |
| Tiada pemformatan | Kepala tebal, lebar lajur, pembekuan panel |
| Tiada jenis data | Tarikh ialah tarikh; nombor ialah nombor |
| Tiada formula | Jumlah, kiraan |

Laporan inventori pergi kepada pengurusan aset yang akan **menapis dan mengisih** — Excel sesuai untuk itu.

> **Semasa semakan silang AI:** Kumpulan 2 mempunyai CSV, anda mempunyai Excel. Adakah eksport patut menjadi komponen kongsi? Bincangkan. Jawapannya mungkin "tidak — format berbeza, keperluan berbeza" — tetapi ia patut ditanya.

## Aset yang tidak diakui

Ciri kecil dengan nilai operasi tinggi: pinjaman **diluluskan tetapi tidak diakui** selepas 3 hari.

Ini bermakna sama ada:
- Staf tidak datang mengambilnya (aset terkunci tanpa sebab), atau
- Mereka mengambilnya tetapi terlupa mengakui (inventori betul, rekod salah)

Kedua-duanya memerlukan tindakan susulan. Ia senarai kerja pendek yang menjimatkan aset daripada duduk dalam limbo.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
