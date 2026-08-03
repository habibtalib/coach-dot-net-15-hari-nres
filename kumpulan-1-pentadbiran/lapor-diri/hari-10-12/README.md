# Kumpulan 1 · Hari 10–12 — Notifikasi, Slip Akuan PDF & Dashboard HR

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Tiga hari. Hujungnya, pemohon menerima notifikasi e-mel sebenar, boleh mencetak Slip Akuan Lapor Diri, dan HR mempunyai papan pemuka analitis.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Menghantar e-mel (SMTP) | [learn.microsoft.com/dotnet/api/system.net.mail.smtpclient](https://learn.microsoft.com/en-us/dotnet/api/system.net.mail.smtpclient) |
| MailKit (disyorkan) | [github.com/jstedfast/MailKit](https://github.com/jstedfast/MailKit) |
| QuestPDF | [www.questpdf.com/getting-started.html](https://www.questpdf.com/getting-started.html) |
| Konfigurasi & Options | [learn.microsoft.com/aspnet/core/fundamentals/configuration/options](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) |
| Background services | [learn.microsoft.com/aspnet/core/fundamentals/host/hosted-services](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/host/hosted-services) |
| Pengelompokan LINQ | [learn.microsoft.com/dotnet/csharp/linq/standard-query-operators/grouping-data](https://learn.microsoft.com/en-us/dotnet/csharp/linq/standard-query-operators/grouping-data) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 10** | Notifikasi e-mel — pelaksanaan `INotificationService` baharu, templat, konfigurasi |
| **Hari 11** | Slip Akuan PDF — QuestPDF, reka letak, muat turun berperanan |
| **Hari 12** | Papan pemuka analitis HR — statistik mengikut bulan, status, bahagian |

**Hasil:** Notifikasi e-mel dihantar pada setiap peralihan status; Slip Akuan PDF boleh dijana untuk permohonan yang diluluskan; HR mempunyai carta ringkasan.

---

## Menambah e-mel: tambah pelaksanaan, jangan sunting yang sedia ada

`INotificationService` dan `ConsoleNotificationService` dibina pada Hari 3 dan digunakan oleh keempat-empat modul. Anda **tidak** menyunting `ConsoleNotificationService` — tiga kumpulan lain masih bergantung padanya.

Sebaliknya anda **menambah** pelaksanaan kedua:

```csharp
public class SmtpNotificationService : INotificationService { ... }
```

...dan biarkan **konfigurasi** memutuskan mana yang digunakan. Ini corak yang sama seperti sepanjang kursus: **tambah, jangan sunting**.

> ⚠️ **Ini menyentuh pendaftaran DI kongsi.** Menukar pendaftaran `INotificationService` mempengaruhi keempat-empat modul. **Buka isu berlabel `shared`** dan bincang dengan jurulatih sebelum meneruskan — ini contoh sempurna keputusan yang bukan milik satu kumpulan. Kemungkinan besar hasilnya: jurulatih menambah pemilihan berasaskan konfigurasi ke `Program.cs` sekali, untuk semua orang.

## Kenapa e-mel tidak boleh menyekat penghantaran

Jika pelayan SMTP perlahan atau tidak dapat dihubungi, dan anda menghantar e-mel **secara segerak** dalam action `Submit`, pemohon menunggu — atau lebih teruk, penghantaran gagal kerana e-mel gagal.

**Peraturan:** kegagalan notifikasi **tidak pernah** menggagalkan operasi perniagaan. Permohonan telah dihantar; itu fakta yang tersimpan. E-mel ialah kesan sampingan.

Dua pendekatan, kedua-duanya sah untuk kursus ini:

| Pendekatan | Bila |
|------------|------|
| Cuba-tangkap sekitar hantar, log kegagalan | Mudah, memadai untuk latihan |
| Baris gilir dalam DB + `BackgroundService` | Lebih tegas, menunjukkan corak sebenar |

Kami melaksanakan yang pertama, dan **membincangkan** yang kedua. Jika kumpulan anda ada masa, bina baris gilir — ia latihan yang sangat baik.

## Kandungan Slip Akuan

Slip Akuan Lapor Diri ialah bukti rasmi bahawa pekerja telah melapor diri. Ia dijana **hanya selepas kelulusan**.

| Bahagian | Kandungan |
|----------|-----------|
| Kepala | Logo/nama NRES, tajuk, nombor rujukan |
| Pemohon | Nama, IC, jawatan, gred, bahagian |
| Perkhidmatan | Tarikh lapor diri, agensi sebelum ini |
| Pengesahan | Tarikh kelulusan, pegawai yang meluluskan |
| Kaki | Tarikh cetakan, penafian "dijana komputer" |

**Kenapa PDF dan bukan halaman cetak Razor?** Kedua-duanya sah. PDF menang apabila dokumen perlu **dilampirkan pada e-mel**, disimpan sebagai rekod, atau kelihatan sama pada setiap mesin. Slip Akuan ialah ketiga-tiganya.

## Papan pemuka analitis — bertanya soalan yang berguna

Papan pemuka Hari 7–9 menjawab *"apa yang menunggu saya?"*. Papan pemuka analitis ini menjawab soalan pengurusan:

| Soalan | Visual |
|--------|--------|
| Berapa permohonan sebulan? | Carta bar, 12 bulan |
| Berapa lama purata kelulusan mengambil masa? | Satu nombor (hari) |
| Bahagian mana paling banyak lapor diri? | Jadual, disusun |
| Berapa kadar penolakan? | Peratusan + trend |

**Peraturan prestasi:** kesemuanya ialah **pengagregatan**. Lakukan dalam pangkalan data dengan `GroupBy` dan `CountAsync` — jangan sekali-kali menarik baris ke memori untuk dikira.

> **Kadar penolakan yang tinggi bukan masalah HR — ia masalah reka bentuk borang.** Jika 40% permohonan ditolak kerana dokumen hilang, borang tidak cukup jelas. Sebutkan ini kepada kumpulan; ia menghubungkan analitik kembali kepada URS.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
