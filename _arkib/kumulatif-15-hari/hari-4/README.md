# Hari 4 — Pas/Parking/Pelekat: Pemodelan

Nota ini mengikut **HARI 4** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 11–13 (Modul 2: Pas, Parking & Pelekat Kenderaan). Lab hands-on penuh ada di [`snippets/lab.md`](./snippets/lab.md).

> **Konvensyen kod:** Nota dalam **Bahasa Melayu**; kod, nama kelas/pembolehubah, istilah teknikal dalam **Bahasa Inggeris** — rujuk [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) untuk kanun nama entiti/enum/peranan.

> **Sambungan projek:** Kita **tidak** mula projek baharu. `Nres.Onboarding.Web` yang sama dari Hari 1–3 (dengan `Submission`, `Attachment`, `AuditLog`, `SubmissionStatus`, dan modul Lapor Diri lengkap) kita **tambah** Modul 2 di atasnya. Jangan cipta semula `Submission`/`AuditLog`/`ApplicationDbContext` — kita hanya **daftar** entiti baharu ke dalam `DbContext` yang sedia ada.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| EF Core Relationships (overview) | [learn.microsoft.com/ef/core/modeling/relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships) |
| One-to-one relationships | [learn.microsoft.com/ef/core/modeling/relationships/one-to-one](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-one) |
| One-to-many relationships | [learn.microsoft.com/ef/core/modeling/relationships/one-to-many](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many) |
| Indexes & unique constraints | [learn.microsoft.com/ef/core/modeling/indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes) |
| Enum properties (backed as `int`) | [learn.microsoft.com/ef/core/modeling/value-conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions) |
| Migrations (`dotnet ef migrations add`) | [learn.microsoft.com/ef/core/managing-schemas/migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/) |
| `dotnet ef` CLI rujukan penuh | [learn.microsoft.com/ef/core/cli/dotnet](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) |
| Conventional routing (MVC) | [learn.microsoft.com/aspnet/core/mvc/controllers/routing](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 11–12: Model 3 Jenis Permohonan** — `AccessPassApplication`, `VehicleStickerApplication`, `ParkingApplication`, `Vehicle` (staf boleh >1 kenderaan). 💻 **Lab:** entiti + relasi |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 13: Migration & Skrin Awal** — halaman landing modul, laluan 3 jenis, migration. 💻 **Lab:** migration + navigasi modul |
| 5.00 petang | Bersurai |

Hari ini **tidak** merangkumi borang input, validation, atau kelulusan — itu Hari 5 & Hari 6. Fokus semata-mata pada **bentuk data** (model) dan **navigasi awal**.

---

## Kenapa satu `Submission` dikongsi tiga jenis permohonan?

Modul 2 unik berbanding Modul 1 (Lapor Diri) kerana ia bukan **satu** jenis borang — ia **tiga**: pas keselamatan, pelekat kenderaan, dan parkir khas. Tanpa corak yang betul, godaan biasa pemula ialah cipta tiga sistem status berasingan, tiga jadual audit berasingan, tiga cara nombor rujukan berasingan. Ini **membazir** kerana ketiga-tiga jenis permohonan sebenarnya mengikut **aliran kerja yang sama**:

```text
Form → Validation → Draft → Submit → Review → Approve/Reject → Audit → Report
```

Penyelesaiannya: **satu jadual induk `Submissions`** (sudah wujud sejak Hari 1) menyimpan perkara **sejagat** — status, nombor rujukan, siapa pemohon, bila dihantar — dan **tiga jadual anak** (`AccessPassApplications`, `VehicleStickerApplications`, `ParkingApplications`) menyimpan perkara **khusus** kepada setiap jenis (kawasan akses, kenderaan berkaitan, justifikasi parkir khas). Setiap jadual anak ada **satu-ke-satu** (`one-to-one`) hubungan dengan `Submissions` — satu rekod anak = tepat satu rekod induk.

Corak ini bermakna:

- `IReferenceNumberService`, `IAuditLogService`, `SubmissionStatus`, halaman kelulusan generik — **semua digunakan semula** tanpa ubah suai untuk ketiga-tiga jenis.
- Hanya **bahagian borang** (medan input) dan **peraturan pengesahan (validation)** yang berbeza mengikut jenis (Hari 5).
- Hari 6 (kelulusan & senarai admin) boleh papar kesemua tiga jenis dalam **satu** senarai admin, kerana semuanya "hanyalah" `Submission` dengan `ModuleCode` berbeza.

### Keputusan reka bentuk: `ModuleCode` = prefix nombor rujukan terus

Rujuk jadual prefix dalam [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md): `PAS` (Pas Keselamatan), `PKR` (Parkir), `STK` (Pelekat Kenderaan). Kerana setiap **jenis permohonan** (bukan "Modul 2" secara keseluruhan) ada prefix sendiri, kita tetapkan `Submission.ModuleCode` kepada prefix tersebut secara terus (`"PAS"`, `"PKR"`, atau `"STK"`) — sama seperti Hari 3 menetapkan `"LD"` untuk Lapor Diri. `IReferenceNumberService.GenerateAsync(moduleCode)` menerima nilai ini terus dan menjana `PAS-2026-0001`, dsb. Tiada logik baharu diperlukan dalam servis itu — ia sudah generik sejak Hari 3.

## `Vehicle` — kenapa entiti berasingan, bukan medan dalam borang?

Seorang staf boleh **memiliki atau menggunakan lebih daripada satu kenderaan** (kereta sendiri, motosikal, kereta pasangan). Jika medan kenderaan (no. pendaftaran, jenis, warna) ditanam terus dalam `VehicleStickerApplication` dan `ParkingApplication`, staf terpaksa **menaip semula** butiran kenderaan yang sama setiap kali memohon pelekat baharu atau parkir baharu untuk kenderaan yang sama — dan sistem tidak dapat mengesan "kenderaan ini sudah ada pelekat aktif" tanpa membandingkan teks no. pendaftaran secara rapuh.

Penyelesaian: `Vehicle` ialah entiti **berasingan**, dimiliki oleh satu `ApplicantUserId` (staf), dan **kedua-dua** `VehicleStickerApplication` dan `ParkingApplication` merujuk kepadanya melalui `VehicleId` (hubungan **satu-ke-banyak**: satu kenderaan boleh ada banyak permohonan pelekat/parkir sepanjang hayatnya — walaupun peraturan perniagaan Hari 5 hanya membenarkan **satu** yang aktif pada satu masa). `AccessPassApplication` **tidak** perlukan `Vehicle` — pas keselamatan berkaitan orang, bukan kenderaan.

## Kenapa migration hari ini dipanggil `Module2Initial`, bukan tambah terus ke `InitialShared`?

Setiap modul baharu mendapat **migration sendiri** dinamakan mengikut modul (`Module2Initial`), bukan diubah suai ke dalam migration `InitialShared` Hari 1. Ini ialah amalan EF Core standard — migration yang sudah **digunakan** (`dotnet ef database update` sudah dijalankan) tidak patut diedit; sebarang perubahan skema baharu mesti jadi migration **baharu**. Ini juga bermakna sejarah git migration anda mencerminkan sejarah pembangunan modul demi modul — berguna semasa debug kelak ("bila medan ini ditambah?").

## Skrin Awal (Landing Page) — kenapa perlu sebelum borang wujud?

SESI 13 petang ini kita tambah **halaman landing** Modul 2 (`Module2Controller`) yang memaparkan tiga pautan (Pas Keselamatan / Pelekat Kenderaan / Parkir) — walaupun borang sebenar (Hari 5) belum wujud. Ini ikut prinsip **"navigasi dahulu, borang kemudian"**: peserta nampak **struktur keseluruhan** modul (tiga laluan berasingan berkongsi satu menu) sebelum tenggelam dalam butiran satu borang. Ia juga bermakna `dotnet run` sentiasa **boleh dijalankan** dan dinavigasi hujung ke hujung selepas setiap hari — tiada "separuh siap yang pecah".

---

Selesai baca bahagian konsep? Mula lab hands-on di [`snippets/lab.md`](./snippets/lab.md) — bina `Vehicle`, tiga entiti permohonan, migration, dan skrin landing Modul 2.

> 🎤 **Nota penceramah/jurulatih:** [`nota-penceramah.md`](./nota-penceramah.md).
