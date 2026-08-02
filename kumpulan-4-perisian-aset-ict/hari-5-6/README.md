# Kumpulan 4 · Hari 5–6 — Borang Permohonan & Semakan Stok Masa-Nyata

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Dua hari. Hujungnya, staf boleh memohon lesen perisian dan pinjaman aset, dengan **semakan ketersediaan pada masa penghantaran**, dan mengakui penerimaan.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `IValidatableObject` | [learn.microsoft.com/aspnet/core/mvc/models/validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation) |
| Query cekap EF Core | [learn.microsoft.com/ef/core/performance/efficient-querying](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying) |
| Concurrency EF Core | [learn.microsoft.com/ef/core/saving/concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency) |
| LINQ dengan EF Core | Buku Bab 11 (m.s. 586) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 5** pagi | Borang permohonan lesen perisian |
| **Hari 5** petang | Borang pinjaman aset (kategori, bukan unit) |
| **Hari 6** pagi | **Semakan stok masa-nyata** pada penghantaran |
| **Hari 6** petang | Borang akuan penerimaan + gabungan latihan |

**Hasil:** Kedua-dua borang menyimpan draf dan menghantar; stok disemak pada saat penghantaran; pemohon boleh mengakui penerimaan aset.

---

## "Masa-nyata" bermaksud **pada saat penghantaran**, bukan pada paparan

Ini perbezaan penting yang mudah tersasar.

Katalog anda (Hari 4) menunjukkan baki lesen ketika halaman dimuat. Menjelang pemohon menghantar — mungkin 20 minit kemudian — nombor itu mungkin salah.

```text
10.00 pagi  Ali membuka katalog       AutoCAD: 1 lesen baki
10.05 pagi  Siti memohon AutoCAD      AutoCAD: 0 lesen baki
10.20 pagi  Ali menghantar            ← mesti disemak SEMULA di sini
```

**Peraturan:** paparan ialah petunjuk; **penghantaran ialah semakan**.

| Titik | Semakan |
|-------|---------|
| Paparan katalog | Petunjuk mesra — "3 baki" |
| Buka borang | Petunjuk — amaran jika 0 |
| **Hantar** | **Semakan sebenar** — tolak jika habis |
| Kelulusan ICT | **Semak lagi** — masa telah berlalu (Hari 7–9) |

Semakan berlaku **tiga kali**, dan itu betul. Setiap satu murah; kegagalan mahal.

## Kenapa pemohon memilih **kategori**, bukan unit

Borang pinjaman aset meminta *"satu laptop"*, bukan *"laptop NRES-LT-0042"*.

| Pemohon memilih unit | Pemohon memilih kategori |
|----------------------|---------------------------|
| Perlu tahu tag aset | Meminta apa yang mereka perlukan |
| Dua orang memilih unit sama | ICT memperuntukkan daripada stok |
| Unit mungkin rosak sebelum kelulusan | ICT memilih yang tersedia pada masa kelulusan |

Ini corak yang sama seperti nombor lot Kumpulan 2 dan nama akaun AD Kumpulan 3: **pemohon meminta, admin memperuntukkan.**

## Peraturan perniagaan modul anda

| Peraturan | Kenapa |
|-----------|--------|
| Satu pinjaman aktif setiap kategori setiap pemohon | Seorang staf tidak perlukan tiga laptop |
| Perisian `PerluJustifikasi` memerlukan justifikasi | Kos tinggi atau sekatan pematuhan |
| Tarikh jangka pulang wajib untuk pinjaman | Asas peringatan lewat tempoh |
| Tarikh jangka pulang maksimum 6 bulan | Pinjaman tanpa had ialah pemberian |
| Tidak boleh memohon lesen yang sudah dimiliki | Elak pembaziran lesen |
| Stok mesti ada pada penghantaran | Yang jelas — tetapi mudah terlepas |

Peraturan **"tidak boleh memohon lesen yang sudah dimiliki"** menjimatkan wang sebenar. Lesen AutoCAD berharga RM6,500 setahun — permohonan pendua yang diluluskan membazir satu.

## Akuan penerimaan: menutup gelung

Meluluskan pinjaman tidak bermakna staf mempunyai laptop. Ia bermakna ICT patut memberikannya.

```text
Diluluskan  →  ICT serahkan aset  →  Staf akui penerimaan
```

Tanpa langkah akuan, inventori anda berbohong — ia menunjukkan `OnLoan` untuk aset yang masih berada dalam stor.

`AkuanTerima` + `TarikhAkuanTerima` merekod bila staf benar-benar menerimanya. Ini juga bukti jika aset kemudian hilang.

> **Nota reka bentuk:** kita membenarkan status aset menjadi `OnLoan` pada kelulusan, bukan pada akuan. Sebabnya: aset diperuntukkan dan tidak sepatutnya dipinjamkan kepada orang lain sementara menunggu penyerahan. Akuan merekod penyerahan sebenar. Alternatif (status `Reserved` berasingan) lebih tepat tetapi menambah kerumitan — **dokumenkan pilihan ini**.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
