# Kumpulan 2 · Hari 10–12 — QR/Barcode, Semakan Ronda & Laporan

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Tiga hari. Hujungnya, pas dan pelekat yang diluluskan mempunyai **kod QR**, pengawal boleh mengesahkannya di lapangan dalam beberapa saat, dan Bahagian Keselamatan boleh mencetak laporan.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| QRCoder | [github.com/codebude/QRCoder/wiki](https://github.com/codebude/QRCoder/wiki) |
| Data URI imej | [developer.mozilla.org/docs/Web/URI/Schemes/data](https://developer.mozilla.org/en-US/docs/Web/URI/Schemes/data) |
| Reka bentuk responsif (mudah alih) | [getbootstrap.com/docs/5.3/layout/breakpoints](https://getbootstrap.com/docs/5.3/layout/breakpoints/) |
| CSS cetakan | [developer.mozilla.org/docs/Web/CSS/@media](https://developer.mozilla.org/en-US/docs/Web/CSS/@media) |
| Eksport CSV | [learn.microsoft.com/aspnet/core/mvc/controllers/actions](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 10** | Penjanaan QR — token, servis, paparan pada pas & pelekat |
| **Hari 11** | Skrin semakan ronda — carian pantas + imbasan QR, mesra mudah alih |
| **Hari 12** | Laporan bercetak & eksport CSV, gabungan latihan |

**Hasil:** Pas & pelekat diluluskan mempunyai QR; pengawal boleh mengesahkan status dalam < 5 saat; laporan boleh dicetak dan dieksport.

---

## Apa yang QR sebenarnya mengandungi

Godaan pertama ialah mengekod semua butiran ke dalam QR: nama, IC, nombor plat, tarikh. **Jangan.**

| ❌ Kod QR mengandungi data | ✅ Kod QR mengandungi token |
|----------------------------|------------------------------|
| Data peribadi terdedah kepada sesiapa yang mengimbas | Token tidak bermakna tanpa sistem |
| Tidak boleh dibatalkan — pas yang dicetak kekal sah selamanya | Batalkan dalam DB, imbasan seterusnya gagal |
| Data menjadi lapuk apabila permohonan berubah | Sentiasa menunjukkan keadaan semasa |
| Boleh dipalsukan dengan menjana QR anda sendiri | Token mesti sepadan rekod sebenar |

Kod QR anda mengandungi **URL dengan token**:

```text
https://nres-onboarding/Akses/Semak?token=k3Jd9xQm2ZpL4vRt
```

Pengawal mengimbas → pelayar dibuka → sistem mencari token → memaparkan status **semasa**.

> **Ini prinsip keselamatan umum:** kod QR ialah **penunjuk**, bukan **bekas**. Perkara yang sama terpakai pada kod QR tiket, pas, dan pengesahan.

## Token mesti tidak boleh diteka

Token `1`, `2`, `3` bermakna sesiapa boleh melayari pas orang lain. Guna nilai rawak:

```csharp
// 16 aksara daripada RandomNumberGenerator — bukan Random, bukan Guid berurutan
```

`Random` tidak selamat kriptografi. `Guid.NewGuid()` boleh diterima tetapi panjang; token pendek yang dijana daripada `RandomNumberGenerator` menghasilkan QR yang lebih mudah diimbas.

## Bila QR dijana

| Peristiwa | QR |
|-----------|-----|
| Draf dicipta | ❌ Tiada |
| Dihantar | ❌ Tiada |
| **Diluluskan** | ✅ Token dijana, QR tersedia |
| Ditolak | ❌ Tiada |
| Dibatalkan selepas kelulusan | ⚠️ Token kekal, tetapi imbasan menunjukkan **TIDAK SAH** |

Token **tidak** dipadam apabila pas dibatalkan — kerana pas fizikal dengan QR yang dicetak masih wujud di dunia sebenar. Imbasan mesti memberitahu pengawal ia tidak lagi sah, bukan gagal dengan "tidak dijumpai".

## Skrin ronda: direka untuk keadaan sebenar

Pengawal menggunakan skrin ini **sambil berdiri di tempat letak kereta, pada telefon, mungkin pada waktu malam**. Reka bentuk mengikutnya:

| Keperluan | Reka bentuk |
|-----------|-------------|
| Keputusan dalam < 5 saat | Satu skrin, tiada navigasi |
| Boleh dibaca sepintas lalu | Jalur hijau/merah besar, bukan teks kecil |
| Berfungsi tanpa QR | Carian nombor plat sebagai sandaran |
| Sarung tangan / hujan | Sasaran sentuh besar |
| Mungkin isyarat lemah | Halaman ringan, tiada aset berat |

**Tiga perkara yang mesti dilihat pengawal serta-merta:** SAH atau TIDAK SAH · nombor plat / nama pemegang · tempoh sah.

Segala-galanya boleh dilihat di bawah.

## Kebenaran pada skrin ronda

Skrin semakan memerlukan `SecurityAdmin`. Ia mendedahkan nama pemegang dan nombor plat — bukan maklumat awam.

**Tetapi:** pengawal mungkin mengimbas dari telefon yang belum log masuk. Aliran mesti: imbas → halaman log masuk → kembali ke keputusan imbasan. Ini `ReturnUrl` standard ASP.NET Core Identity — pastikan ia berfungsi.

> Menjadikan skrin imbasan awam "untuk kemudahan" ialah kegagalan keselamatan biasa. Sesiapa yang menemui pas yang hilang boleh mengimbasnya dan melihat siapa pemiliknya.

## Laporan yang Bahagian Keselamatan sebenarnya guna

| Laporan | Untuk apa |
|---------|-----------|
| Pas aktif mengikut jenis | Berapa pelawat/kontraktor ada di tapak |
| Pelekat mengikut tahun | Perancangan pembaharuan |
| Peruntukan lot | Lot mana kosong, mana diguna |
| Pas tamat tempoh minggu ini | Susulan proaktif |

Cetakan menggunakan `@media print` CSS — bukan penjanaan PDF. **Kenapa berbeza daripada Kumpulan 1?** Slip Akuan ialah dokumen rasmi yang dilampirkan pada e-mel dan disimpan. Laporan Keselamatan ialah senarai kerja yang dicetak dan dilupakan. Cetakan Razor lebih ringan dan mencukupi.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
