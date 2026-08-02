# Hari 6 — Pas/Parking/Pelekat: Kelulusan & Cetakan

Nota ini mengikut **HARI 6** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 17–19 (Modul 2: Pas, Parking & Pelekat Kenderaan). Lab hands-on penuh ada di [`snippets/lab.md`](./snippets/lab.md).

> **Sambungan projek:** Hari 5 melengkapkan tiga borang (pas, pelekat, parkir) yang boleh simpan draf, hantar (dengan nombor rujukan `PAS`/`PKR`/`STK`), dan sekat pendua. Hari ini kita tambah **sisi admin**: senarai, penapisan, semakan, kelulusan/tolak, dan ringkasan boleh cetak — melengkapkan Modul 2 hujung ke hujung.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `[Authorize(Roles = ...)]` | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |
| `Queryable.Concat` (gabung query berbeza jenis) | [learn.microsoft.com/dotnet/api/system.linq.queryable.concat](https://learn.microsoft.com/en-us/dotnet/api/system.linq.queryable.concat) |
| Query filtering & projection dinamik | [learn.microsoft.com/ef/core/querying/single-entity-results](https://learn.microsoft.com/en-us/ef/core/querying/single-entity-results) |
| Razor Layouts & `@media print` | [learn.microsoft.com/aspnet/core/mvc/views/layout](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/layout) |
| Partial views (`Views/Shared`) | [learn.microsoft.com/aspnet/core/mvc/views/partial](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/partial) |
| CSS `@media print` (MDN) | [developer.mozilla.org/en-US/docs/Web/CSS/@media/print](https://developer.mozilla.org/en-US/docs/Web/CSS/@media/print) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 17–18: Senarai & Kelulusan Admin** — halaman senarai admin, filter (jenis, status, jabatan, julat tarikh), halaman detail, approve/reject (wajib sebab), prefix `PAS`/`PKR`/`STK`. 💻 **Lab:** aliran kelulusan |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 19: Print Summary** — Razor print view, `@media print`, ringkasan boleh cetak. 💻 **Lab:** cetakan + audit |
| 5.00 petang | Bersurai |

---

## Kenapa satu senarai admin untuk TIGA jadual berbeza?

`SecurityAdmin` perlu lihat **kesemua** permohonan Modul 2 dalam **satu** senarai operasi — mereka tidak mahu buka tiga skrin berasingan untuk semak kerja harian. Cabarannya: `AccessPassApplications`, `VehicleStickerApplications`, `ParkingApplications` ialah **tiga jadual berbeza** dalam pangkalan data. Penyelesaiannya: **unjurkan (project)** setiap query kepada **bentuk (shape) yang sama** — sebuah *view model* ringkas (`ReferenceNo`, `RequestTypeLabel`, `ApplicantUserId`, `Status`, `SubmittedAt`, …) — kemudian **gabungkan** ketiga-tiga `IQueryable` yang sudah sama bentuk menggunakan `.Concat(...)`. EF Core menterjemah ini kepada `UNION ALL` di SQL — satu pertanyaan pangkalan data, bukan tiga panggilan berasingan yang digabung dalam memori C#.

**Kenapa bukan `UNION` (tanpa `ALL`)?** `UNION` buang pendua secara automatik (mahal — perlukan `SORT`/`DISTINCT` merentasi hasil gabungan); `UNION ALL` (`Concat`) tidak buang pendua. Kerana setiap rekod datang dari jadual **berbeza** dengan `Id` sendiri, tiada pendua sebenar mungkin berlaku — jadi `Concat` (lebih pantas) ialah pilihan betul, bukan `Union`.

## Kenapa filter (jenis, status, jabatan, tarikh) diguna SEBELUM `Concat`, bukan selepas?

Jika kita tapis **selepas** menggabungkan semua rekod ke senarai C# (`.ToList()` dahulu, kemudian `.Where(...)` dalam memori), pangkalan data terpaksa hantar **setiap** rekod dari ketiga-tiga jadual ke aplikasi web dahulu — walaupun 99% akan dibuang oleh filter. Dengan menyusun `.Where(...)` **sebelum** `.Concat(...)` (di setiap sub-query), EF Core hantar penapisan itu terus ke SQL (`WHERE` di setiap bahagian `UNION ALL`) — pangkalan data hanya pulangkan rekod yang **benar-benar** diperlukan. Prinsip umum: **tapis serapat mungkin ke sumber data**, jangan tarik semua data dahulu baru tapis di aplikasi.

## Kenapa kelulusan (`Approve`) terus ke `AdminApproved`, bukan `SupervisorApproved` dahulu?

`SubmissionStatus` sejagat ada tujuh nilai (`Draft`, `Submitted`, `SupervisorApproved`, `AdminApproved`, `Rejected`, `Completed`, `Cancelled`) supaya **setiap** modul boleh guna subset yang relevan mengikut keperluan sebenar. Modul 2 (Pas/Parking/Pelekat) di NRES ialah **kelulusan satu peringkat** — `SecurityAdmin` semak dan putuskan terus, tiada peringkat penyelia berasingan (berbeza dengan Modul 3 di Hari 8 yang memerlukan `Supervisor` **dan** `IctAdmin`). Jadi aliran Modul 2 ialah:

```text
Draft → Submitted → AdminApproved (atau Rejected)
```

`SupervisorApproved` **tidak** digunakan di sini — ini **normal**, bukan ralat; enum sejagat sengaja lebih luas daripada keperluan satu-satu modul supaya boleh dikongsi.

## Kenapa nombor pelekat sebenar (`StickerNoIssued`) diisi semasa Approve, bukan semasa Submit?

`ReferenceNo` (cth. `STK-2026-0007`) ialah **nombor sistem dalaman** — dijana serta-merta semasa `Submit` (Hari 5) supaya pemohon boleh jejak status permohonan mereka. Tetapi **nombor pelekat fizikal** yang akan dicetak/ditampal pada kereta hanya wujud **selepas** `SecurityAdmin` sahkan kelayakan dan keluarkan pelekat sebenar — ini realiti operasi (pelekat fizikal bernombor siri terhad, diagihkan mengikut stok). Memisahkan kedua-dua nombor ini elak sistem "menjanjikan" nombor pelekat sebelum kelulusan sebenar berlaku.

## Kenapa `@media print`, bukan terus jana PDF?

Menjana PDF (contohnya dengan pustaka pihak ketiga) menambah **kebergantungan** (dependency) dan kerumitan pemasangan yang tidak diperlukan pada peringkat latihan. `@media print` ialah ciri **CSS terbina-dalam pelayar** — kita tulis satu Razor view biasa, tambah peraturan CSS yang **hanya** terpakai semasa cetak (`@media print { .no-print { display: none; } }`), dan pengguna guna **"Print" pelayar** (`Ctrl+P`/`Cmd+P`) → "Save as PDF" jika perlu PDF. Ini corak **"print view sebelum PDF library"** yang disebut dalam panduan induk — cukup untuk kebanyakan keperluan ringkasan permohonan dalaman, dan boleh dinaik taraf ke pustaka PDF khusus kemudian tanpa mengubah struktur data.

---

Selesai baca konsep? Mula bina senarai admin, kelulusan, dan cetakan di [`snippets/lab.md`](./snippets/lab.md).

> 🎤 **Nota penceramah/jurulatih:** [`nota-penceramah.md`](./nota-penceramah.md).

**Hasil Hari 6:** Modul 2 (Pas, Parking & Pelekat Kenderaan) **lengkap hujung ke hujung** — draf → hantar → semakan admin → lulus/tolak → audit → cetak ringkasan. Peserta kini boleh terangkan dua modul penuh (Lapor Diri, Pas/Parking/Pelekat) menggunakan corak yang **sama**.
