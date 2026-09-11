# Kumpulan 4 · Hari 13–14 — Ujian Pertindihan Slot, Refactor & Sedia Gabung

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md) · AI QA: [`lab-qa-ai-uat.md`](../../docs/lab-qa-ai-uat.md)
>
> **Tiada ciri baharu dalam blok ini.**

Dua hari untuk membuktikan modul anda betul, membersihkannya, dan menyediakannya untuk gabungan Hari 15. Fokus utama: **ujian pertindihan slot** — kerana ia satu-satunya pertahanan modul anda, ia mesti jadi yang paling teruji.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| xUnit | [xunit.net/docs/getting-started/v3/getting-started](https://xunit.net/docs/getting-started/v3/getting-started) |
| Ujian EF Core dengan SQLite | [learn.microsoft.com/ef/core/testing/testing-with-the-database](https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database) |
| `[Theory]` & `[InlineData]` | [learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 13** | Projek ujian · ujian predikat `SlotOverlap` · ujian servis bertindih (kedua-dua arah) |
| **Hari 14** | Refactor (ekstrak logik boleh diuji) · ujian E2E · prestasi · dokumentasi · gabungan kering |

---

## Kenapa ujian pertindihan ialah keutamaan mutlak

Kumpulan 2 mempunyai indeks unik pangkalan data sebagai jaring keselamatan — walaupun ujian mereka terlepas kes, pangkalan data menangkapnya. **Anda tidak.** Pertindihan julat tidak boleh dinyatakan sebagai kekangan unik, jadi semakan aplikasi anda ialah **satu-satunya** perkara yang berdiri antara sistem yang berfungsi dan dua kumpulan tiba di gelanggang yang sama.

Itu bermakna ujian anda bukan jaring keselamatan **tambahan** — ia jaring keselamatan **satu-satunya**. Kes yang tidak diuji ialah kes yang mungkin salah dalam pengeluaran tanpa apa-apa menangkapnya.

## Dua peringkat ujian, satu formula

Predikat pertindihan wujud dua kali (Hari 5–6): kaedah statik tulen `SlotOverlap.Overlaps`, dan syarat SQL dalam `SlotBookingService.FindOverlapAsync`. Anda menguji **kedua-duanya**:

| Peringkat | Apa ia sahkan | Kelajuan |
|-----------|---------------|----------|
| **Ujian predikat** (`SlotOverlap`) | Logik pertindihan tulen — setiap kes tepi, tiada pangkalan data | Mikrodetik |
| **Ujian servis** (`FindOverlapAsync`) | SQL menterjemah predikat dengan betul + status ditapis | Milidetik (SQLite) |

Jika kedua-duanya diuji terhadap kes yang sama dan kedua-duanya lulus, anda tahu versi memori dan versi SQL **tidak menyimpang**. Ini sebab predikat ditulis sebagai satu formula terdokumen.

## Matriks kes yang mesti diliputi

| Kes | Predikat | Servis | Jangkaan |
|-----|----------|--------|----------|
| Serentak tepat (`10–11` vs `10–11`) | ✅ | ✅ | Bertindih |
| Bertindih separa (`10–11` vs `10:30–11:30`) | ✅ | ✅ | Bertindih |
| Terkandung (`10–12` vs `10:30–11`) | ✅ | ✅ | Bertindih |
| Meliputi (`10:30–11` vs `10–12`) | ✅ | ✅ | Bertindih |
| **Bersebelahan selepas** (`10–11` vs `11–12`) | ✅ | ✅ | **Tidak** bertindih |
| **Bersebelahan sebelum** (`10–11` vs `9–10`) | ✅ | ✅ | **Tidak** bertindih |
| Berjurang (`10–11` vs `11:30–12`) | ✅ | ✅ | Tidak bertindih |
| Tarikh berbeza | — | ✅ | Tidak menyekat |
| Fasiliti berbeza | — | ✅ | Tidak menyekat |
| Status `Rejected`/`Cancelled` | — | ✅ | Tidak menyekat |
| Status `Draft` | — | ✅ | Tidak menyekat |
| Diri sendiri (`kecualiSubmissionId`) | — | ✅ | Tidak menyekat |

> **Baris "bersebelahan" ialah yang paling penting.** Ia mengesahkan anda tidak terlalu ketat. Semakan yang menolak tempahan bersebelahan menghalang kerja sebenar Encik Faizal dan lulus setiap ujian tergesa-gesa yang hanya menyemak kes "jelas bertindih".

## Refactor: jadikan logik boleh diuji

Jika mana-mana peraturan penting masih terkubur sebagai kaedah peribadi dalam controller (cth. semakan waktu operasi, kesahihan tempahan), **ekstrak ia sekarang** ke kelas boleh diuji dalam `Services/Fasiliti/`. Logik yang tidak boleh diuji dalam pengasingan ialah logik yang tidak diuji. Ini tujuan blok ini, bukan sampingannya.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
