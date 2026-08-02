# Kumpulan 4 · Hari 13–14 — Ujian, Refactor & Sedia Gabung

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Dua hari. **Tiada ciri baharu.** Hujungnya, modul anda diuji, dibersihkan, dan bersedia untuk gabungan Hari 15.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| xUnit | [xunit.net/docs/getting-started/v3/getting-started](https://xunit.net/docs/getting-started/v3/getting-started) · Buku Bab 4 (m.s. 201) |
| Ujian dengan EF Core | [learn.microsoft.com/ef/core/testing](https://learn.microsoft.com/en-us/ef/core/testing/) |
| Token concurrency | [learn.microsoft.com/ef/core/saving/concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency) |
| Prestasi query | [learn.microsoft.com/ef/core/performance/efficient-querying](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 13** | Ujian unit — inventori, kelayakan, transaksi peruntukan, pemulangan, peringatan |
| **Hari 14** | Prestasi, refactor, dokumentasi, sedia gabung |

**Hasil:** Ujian yang lulus untuk peraturan inventori anda; keputusan prestasi didokumenkan; cabang bergabung bersih.

---

## Apa yang patut diuji dalam modul anda

| Uji ini | Kenapa |
|---------|--------|
| **Kiraan lesen** | Asas semua semakan stok |
| Lesen tanpa had tidak pernah habis | Kes tepi yang mudah dipecahkan |
| **Peruntukan aset menukar status** | Integriti inventori |
| **Peruntukan aset yang telah diambil gagal** | Perlindungan perlumbaan |
| **Rollback meninggalkan aset `Available`** | Transaksi berfungsi |
| Kondisi pemulangan → status aset | Pemetaan tiga arah |
| Aset `Lost` kekal dalam DB | Keperluan audit |
| Peraturan kelayakan (kedua-dua arah) | Peraturan perniagaan |
| **Tahap peringatan tidak berulang** | Anti-spam |

Ujian **transaksi dan perlumbaan** ialah yang paling penting — ia peraturan yang paling mahal jika salah, dan paling sukar dikesan secara manual.

## Menguji transaksi

Anda tidak boleh menguji rollback dengan berharap sesuatu gagal. Anda **memaksanya**:

```csharp
// Suntik ganti yang melontar pada titik tertentu, kemudian sahkan
// tiada perubahan separa berlaku.
```

Untuk kursus ini, pendekatan paling mudah ialah **menguji keadaan akhir**: jalankan peruntukan yang sepatutnya gagal (aset sudah `OnLoan`), dan sahkan **tiada apa** berubah — bukan permohonan, bukan aset.

Ujian rollback penuh dengan pengecualian yang disuntik ialah latihan lanjutan; jika kumpulan anda ada masa, cubalah.

## Token concurrency — perbincangan Hari 13–14

Anda menyelesaikan perlumbaan dengan **menyemak semula status di dalam transaksi**. Itu berfungsi dan mudah dibaca.

Alternatif yang lebih tegas ialah **token concurrency**:

```csharp
[Timestamp]
public byte[]? RowVersion { get; set; }
```

EF Core kemudian menyertakan `WHERE RowVersion = @asal` dalam setiap `UPDATE`. Jika seseorang mengubah baris sejak anda membacanya, sifar baris dikemas kini dan EF Core melontar `DbUpdateConcurrencyException`.

| | Semak semula (kita) | Token concurrency |
|---|---------------------|-------------------|
| Mudah dibaca | ✅ | Perlu pengendalian pengecualian |
| Melindungi semua medan | ❌ hanya yang anda semak | ✅ seluruh baris |
| Sokongan SQLite | ✅ | Terhad — memerlukan konfigurasi |

**Kita membincangkannya; kita tidak menukarnya.** Menukar pendekatan concurrency pada Hari 14 ialah tepat jenis perubahan yang memecahkan sesuatu sebelum demo. Rekod sebagai pengesyoran.

## Prestasi: apa yang perlu diukur

| Skrin | Risiko |
|-------|--------|
| Katalog (`/Aset`) | `AllLicenceStatusAsync` — N+1 jika ditulis naif |
| Dashboard | Berbilang pengagregatan |
| Baris gilir | Tiga query disatukan dalam memori |
| Eksport Excel | Memuatkan semua aset + lesen + lewat tempoh |

**Semak semula keputusan "kiraan dikira vs disimpan" (Hari 4).** Dengan 8 perisian dan beberapa ratus permohonan, mengira adalah pantas. Dokumenkan ambang di mana ia berhenti benar.

## Persediaan gabungan

- [ ] `dotnet build` bersih
- [ ] `dotnet test` semua lulus
- [ ] Digabung dengan `master` terkini
- [ ] Gabungan kering tiada konflik
- [ ] `README-modul.md` ditulis

> **Perhatian khusus Kumpulan 4:** anda menambah **ClosedXML** ke `.csproj` dan mendaftar **`IHostedService`** dalam modul anda. Kedua-duanya mesti dinyatakan dalam `README-modul.md` — tugas latar belakang akan **berjalan** selepas gabungan, dan orang lain perlu tahu mengapa e-mel dihantar.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
