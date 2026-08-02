# Kumpulan 1 · Hari 7–9 — Aliran Kelulusan HR & Skrin Admin

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Tiga hari. Hujungnya, Pegawai HR mempunyai dashboard, boleh menyemak permohonan dengan lampirannya, dan meluluskan atau menolak dengan ulasan — semuanya diaudit.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Role-based authorization | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |
| LINQ — penapisan & paging | [learn.microsoft.com/ef/core/querying](https://learn.microsoft.com/en-us/ef/core/querying/) |
| Prestasi query EF Core | [learn.microsoft.com/ef/core/performance/efficient-querying](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying) |
| Concurrency EF Core | [learn.microsoft.com/ef/core/saving/concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 7** | Dashboard HR — baris gilir semakan, penapis, paging |
| **Hari 8** | Skrin semakan — butiran penuh, lampiran, panel kelulusan |
| **Hari 9** | Approve/reject dengan ulasan, semakan concurrency, audit, ujian manual |

**Hasil:** HR boleh menemui, membuka, menyemak, dan memutuskan permohonan; setiap keputusan diaudit dan pemohon diberitahu.

---

## Anda tidak menulis `Approve` dan `Reject`

Ini perkara paling penting dalam blok ini.

`SubmissionControllerBase` (Hari 3) sudah melaksanakan:

- `Approve(int id, string? remarks)` — semak peranan, peralihan status, audit, notifikasi
- `Reject(int id, string remarks)` — **sebab wajib**, peralihan, audit, notifikasi

Controller anda mewarisinya. Kerja anda ialah membina **skrin** yang memanggilnya, bukan menulis semula logiknya.

**Kenapa ini penting melebihi menjimatkan menaip:** apabila keempat-empat modul menggunakan kelas asas yang sama, sebab penolakan wajib berkelakuan sama di mana-mana, audit ditulis dengan cara yang sama, dan Papan Pemuka Induk Hari 15 boleh mempercayai bahawa setiap keputusan mempunyai jejak.

> Jika modul anda **benar-benar** memerlukan tingkah laku kelulusan yang berbeza, itu isu berlabel `shared` — bukan alasan untuk menulis semula. Bincang dengan jurulatih.

## Baris gilir semakan: apa yang HR sebenarnya perlukan

Dashboard HR bukan "senarai semua permohonan". Ia menjawab satu soalan: **apa yang menunggu saya?**

| Bahagian | Isi | Kenapa |
|----------|-----|--------|
| Menunggu semakan | `Status == Submitted` | Kerja HR sebenar |
| Diluluskan bulan ini | `AdminApproved`, 30 hari | Konteks & keyakinan |
| Ditolak bulan ini | `Rejected`, 30 hari | Corak — banyak penolakan bermakna borang mengelirukan |
| Jumlah draf | `Draft` | Menunjukkan permohonan yang belum siap |

Penapis yang HR perlukan: **status**, **bahagian**, **julat tarikh**, dan **carian nombor rujukan/nama**. Guna `_FilterBar` kongsi.

## Query yang cekap sejak hari pertama

Skrin senarai ialah tempat prestasi EF Core paling penting. Tiga peraturan:

**1. Tapis dalam pangkalan data, bukan dalam memori.**

```csharp
// ❌ Memuatkan SEMUA permohonan, kemudian menapis
var semua = await Db.Submissions.ToListAsync();
var menunggu = semua.Where(s => s.Status == SubmissionStatus.Submitted);

// ✅ Pangkalan data melakukan kerja
var menunggu = await Db.Submissions
    .Where(s => s.Status == SubmissionStatus.Submitted)
    .ToListAsync();
```

**2. Projek kepada apa yang anda paparkan.** Skrin senarai tidak memerlukan setiap medan setiap entiti. Guna `.Select()` ke view model — ia menjana `SELECT` yang lebih sempit.

**3. Guna `AsNoTracking()` untuk senarai baca-sahaja.** EF Core tidak perlu menjejaki perubahan pada data yang anda hanya paparkan.

Ketiga-tiganya diukur pada Hari 13–14. Bina dengan betul sekarang supaya tiada apa untuk dibaiki kemudian.

## Concurrency: dua pegawai HR, satu permohonan

Senario sebenar: dua pegawai HR membuka permohonan yang sama. Seorang meluluskan, seorang menolak, kedua-duanya menekan butang.

Tanpa perlindungan, keputusan kedua menimpa yang pertama secara senyap — dan audit log menunjukkan kedua-duanya, mengelirukan sesiapa yang membacanya kemudian.

`IWorkflowService.CanTransition` menangkap sebahagian daripada ini: setelah status ialah `Rejected`, peralihan ke `AdminApproved` ditolak (jadual `Rejected` kosong). Yang kedua mendapat ralat dan bukan kejayaan senyap.

Ini pertahanan yang **memadai untuk latihan**. Kita akan membincangkan token concurrency sebenar pada Hari 13–14.

## Kebenaran pada setiap skrin admin

Setiap action HR memerlukan `[Authorize(Roles = "HrAdmin")]`. Menyembunyikan pautan navigasi **tidak mencukupi** — sesiapa boleh menaip URL.

Uji ini secara eksplisit: log masuk sebagai `applicant@nres.test`, lawati `/OfficerReporting/Review`. Anda mesti mendapat 403.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
