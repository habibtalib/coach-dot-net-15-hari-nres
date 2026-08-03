# Kumpulan 1 · Hari 5–6 — Muat Naik Dokumen & Nombor Rujukan

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)
>
> Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

Dua hari. Hujungnya, permohonan lapor diri boleh **dihantar secara rasmi** dengan dokumen sokongan dan nombor rujukan.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Muat naik fail dalam ASP.NET Core | [learn.microsoft.com/aspnet/core/mvc/models/file-uploads](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads) |
| Keselamatan muat naik fail | [learn.microsoft.com/aspnet/core/mvc/models/file-uploads#security-considerations](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads#security-considerations) |
| Validation server-side | [learn.microsoft.com/aspnet/core/mvc/models/validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation) |
| Transaksi EF Core | [learn.microsoft.com/ef/core/saving/transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions) |

## Jadual Blok Ini

| Hari | Sesi | Fokus |
|------|------|-------|
| **Hari 5** pagi | Lampiran | `IFileStorageService` kongsi, metadata `Attachment`, jenis dokumen NRES |
| **Hari 5** petang | Muat naik & muat turun | Borang muat naik, senarai lampiran, muat turun selamat berperanan |
| **Hari 6** pagi | Nombor rujukan & hantar | `IReferenceNumberService`, validation penuh, peralihan status |
| **Hari 6** petang | Kunci & audit | Kunci selepas hantar, audit trail, ujian manual, gabungan latihan |

**Hasil:** Permohonan boleh dilampirkan dokumen, dihantar dengan nombor rujukan `LD-2026-####`, terkunci selepas dihantar, dan setiap tindakan dicatat dalam audit log.

---

## Dokumen sokongan yang NRES perlukan

| Dokumen | Wajib | Nota |
|---------|-------|------|
| Salinan Kad Pengenalan | ✅ | Depan & belakang |
| Surat Tawaran / Lantikan | ✅ | |
| Sijil Akademik | ✅ | |
| Surat Akuan Sumpah | ⬜ | Jika berkenaan |
| Slip Gaji Terakhir | ⬜ | Hanya pertukaran dari agensi lain |

> Semak senarai ini terhadap URS Hari 1 anda. Jika NRES menjawab soalan terbuka anda, kemas kini kedua-duanya.

## Kenapa fail TIDAK disimpan dalam pangkalan data

Godaan biasa: simpan fail sebagai `byte[]` dalam jadual. Jangan.

| Fail dalam DB | Fail pada cakera + metadata dalam DB |
|---------------|--------------------------------------|
| Backup membengkak dengan pantas | Backup DB kekal kecil dan pantas |
| Setiap query berisiko menarik megabait | Query menarik baris kecil |
| Sukar dipindahkan ke storan awan kemudian | Tukar `IFileStorageService` sahaja |

Kita simpan **metadata** dalam `Attachments` dan **bait** dalam `App_Data/uploads/{submissionId}/`.

## Kenapa `App_Data/`, bukan `wwwroot/`

Apa sahaja dalam `wwwroot/` **boleh dicapai terus** oleh pelayar tanpa melalui kod anda. Salinan kad pengenalan dan surat tawaran ialah data peribadi — ia mesti melalui satu action controller yang menyemak kebenaran **dahulu**.

Ini bukan teori. Jika lampiran berada dalam `wwwroot/uploads/12/ic.pdf`, sesiapa yang meneka URL itu boleh memuat turunnya — tiada log masuk diperlukan.

## Tiga pertahanan pada muat naik fail

`IFileStorageService` kongsi (Hari 3) sudah melaksanakan ketiga-tiganya. **Anda tidak menulisnya semula** — anda perlu memahaminya:

1. **Sekatan jenis** — hanya `.pdf`, `.jpg`, `.jpeg`, `.png`
2. **Had saiz** — 5 MB
3. **Nama fail dijana** — GUID, bukan nama yang pengguna beri

Pertahanan ketiga adalah yang paling kurang jelas dan paling penting. Nama fail yang pengguna beri boleh mengandungi `../../` (path traversal) atau menimpa fail sedia ada. Kita **tidak pernah** menggunakannya di cakera — hanya untuk paparan.

## Nombor rujukan: bila dijana, dan kenapa

`ReferenceNo` kekal **kosong** semasa draf. Ia dijana **pada saat penghantaran**, tidak lebih awal.

**Kenapa tidak semasa cipta draf?** Kerana pemohon mungkin mencipta lima draf dan membuang empat. Nombor rujukan ialah rekod rasmi — mengeluarkannya untuk draf yang dibuang meninggalkan jurang dalam jujukan, yang mencetuskan soalan audit yang tidak perlu.

Itulah sebabnya indeks unik pada `ReferenceNo` **ditapis** (`WHERE ReferenceNo <> ''`): banyak draf sah berkongsi rujukan kosong.

## Menghantar ialah operasi berbilang langkah

Menghantar melakukan lima perkara yang mesti **kesemuanya** berjaya atau **tiada** langsung:

```text
1. Sahkan borang (validation PENUH kali ini)
2. Sahkan lampiran wajib wujud
3. Jana nombor rujukan
4. Tukar status Draft → Submitted
5. Tulis audit log + beritahu HR
```

Jika langkah 3 berjaya dan langkah 4 gagal, anda mempunyai nombor rujukan yang dikeluarkan untuk permohonan yang masih draf. `IWorkflowService.TransitionAsync` mengendalikan langkah 4–5 secara atomik; anda mengendalikan turutan.

## Validation penuh vs validation draf

Kembali kepada corak Hari 4 — kali ini kita menggunakan **cabang penuh**:

| | Simpan Draf | Hantar |
|---|-------------|--------|
| Nama, IC, e-mel, telefon | Nama sahaja | Semua wajib |
| Tarikh lapor diri | Pilihan | Wajib |
| Bahagian/jawatan/gred | Pilihan | Wajib |
| Lampiran wajib | Tidak disemak | Ketiga-tiga mesti wujud |
| Akuan | Tidak disemak | Mesti ditanda |

## Selepas hantar: kunci

Setelah `Status != Draft`, borang menjadi baca-sahaja. Ini dikuatkuasakan pada **pelayan** (dalam action controller), bukan hanya dengan `disabled` pada view — atribut `disabled` ialah cadangan kepada pelayar, bukan kawalan keselamatan.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
