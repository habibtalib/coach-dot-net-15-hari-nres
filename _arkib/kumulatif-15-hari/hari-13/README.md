# Hari 13 — Aset ICT: Pemodelan

Nota ini mengikut **aturcara rasmi SESI 38–40** — lihat [`../JADUAL.md`](../JADUAL.md), bahagian **HARI 13**. Hands-on penuh langkah demi langkah ada di [`snippets/lab.md`](./snippets/lab.md) — baca bahagian konsep di bawah dahulu, kemudian pindah ke lab untuk menaip kod sendiri.

> **Konvensyen kod:** Nota dalam **Bahasa Melayu**; semua kod, nama kelas/pembolehubah, nama fail, istilah teknikal dikekalkan dalam **Bahasa Inggeris** — ikut [`SPEC-KURSUS.md`](../SPEC-KURSUS.md).

> **Kedudukan hari ini dalam projek:** Ini permulaan **Modul 5 — Aset ICT**, modul **terakhir** sebelum Hari 15 (integrasi). Hari 13–14 membina modul ini di atas 4 modul sedia ada (Lapor Diri, Pas/Parking/Pelekat, ID/AD/Email, PKS) — kita **kongsi semula** `Submission`, `Attachment`, `AuditLog`, `SubmissionStatus`, dan semua servis kongsi (`IReferenceNumberService`, `IAuditLogService`, dll.) yang sudah wujud sejak Hari 1. **Jangan** cipta semula entiti/servis ini — rujuk sahaja.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| EF Core — Relationships (one-to-many, foreign key optional/nullable) | [learn.microsoft.com/ef/core/modeling/relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships) |
| EF Core — Enum properties & conversions | [learn.microsoft.com/ef/core/modeling/value-conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions) |
| EF Core — Data seeding (`HasData`) | [learn.microsoft.com/ef/core/modeling/data-seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding) |
| EF Core — Indexes (unique `AssetTag`) | [learn.microsoft.com/ef/core/modeling/indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes) |
| EF Core — Migrations (`dotnet ef migrations add`) | [learn.microsoft.com/ef/core/managing-schemas/migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/) |
| C# `enum` | [learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/enum](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) |
| Nullable reference types (`string?`, `int?`) | [learn.microsoft.com/dotnet/csharp/nullable-references](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 38–39: Model Aset** — `Asset`, `SoftwareCatalogItem`, `SoftwareRequest`, `AssetLoanRequest`, `AssetReturn`; status permohonan vs status aset berbeza. 💻 **Lab:** entiti + relasi |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 40: Seed Katalog** — seed perisian & aset contoh, status aset `Available/OnLoan/...`. 💻 **Lab:** seed + migration |
| 5.00 petang | Bersurai |

**Hasil Hari 13** (rujuk [`JADUAL.md`](../JADUAL.md)): Model & data lookup Modul 5 wujud.

---

## Kenapa Modul Aset ICT Berbeza Daripada Modul-Modul Sebelum Ini?

Empat modul terdahulu (Lapor Diri, Pas/Parking/Pelekat, ID/AD/Email, PKS) semuanya berkisar pada **satu** rekod: peserta hantar permohonan, admin luluskan, selesai. Modul Aset ICT tambah **satu lapisan baharu** — **inventori fizikal** yang wujud **bebas** daripada mana-mana permohonan.

Fikirkan begini: satu troli/kotak boleh dipinjam oleh pelbagai orang secara bergilir sepanjang tahun. Setiap **pinjaman** ialah satu permohonan (`AssetLoanRequest`) dengan kitaran hidupnya sendiri (`Submission` → Draft → Submitted → Completed), tetapi troli **itu sendiri** (`Asset`) terus wujud dalam sistem selepas permohonan itu tamat — sedia dipinjam oleh **permohonan seterusnya**. Ini sebabnya kita perlukan **dua status berasingan** yang berkongsi konsep tapi tidak sama:

| | `SubmissionStatus` (permohonan) | `AssetStatus` (aset fizikal) |
|---|---|---|
| **Milik siapa** | `Submission` (setiap permohonan pinjaman/pemulangan) | `Asset` (setiap troli/laptop/monitor fizikal) |
| **Bila berubah** | Setiap kali permohonan bergerak dalam aliran kerja (submit, approve, complete) | Hanya bila **kejadian inventori sebenar** berlaku (troli diserahkan, troli dipulangkan, troli dihantar servis) |
| **Kitaran hidup** | **Satu kitaran, tamat** apabila `Completed`/`Rejected`/`Cancelled` — rekod kekal sebagai sejarah | **Berulang** — `Available` → `OnLoan` → `Returned`/`Available` → `OnLoan` (semula) → ... sepanjang hayat aset |
| **Nilai** | `Draft, Submitted, SupervisorApproved, AdminApproved, Rejected, Completed, Cancelled` (dikongsi 5 modul) | `Available, Reserved, OnLoan, Returned, UnderMaintenance, Retired` (khusus Modul 5) |

**Kesilapan paling biasa peserta baharu buat** ialah cuba guna **satu** enum untuk kedua-dua konsep ("permohonan pinjaman ni statusnya 'OnLoan' kan?") — ini mengelirukan kerana permohonan `AssetLoanRequest` **sendiri** ada status `Submission.Status` (contohnya `Completed` bila pinjaman selesai diproses), manakala aset yang dipinjam itu **berasingan** ada `Asset.Status = AssetStatus.OnLoan`. Satu permohonan yang sudah `Completed` (tiada apa lagi perlu dibuat pada permohonan itu) boleh wujud **serentak** dengan aset yang masih `OnLoan` (kerana belum dipulangkan) — dua fakta berbeza, dua jadual berbeza.

> Rujukan rasmi konsep `enum`: [learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/enum](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum)

---

## Lima Entiti Baharu Hari Ini

Ikut jadual entiti dalam [`SPEC-KURSUS.md`](../SPEC-KURSUS.md) (bahagian "Jadual entiti per modul") — Modul 5 mempunyai **5 jadual**: `Assets`, `SoftwareCatalogItems`, `SoftwareRequests`, `AssetLoanRequests`, `AssetReturns`.

### 1. `Asset` — inventori fizikal (BUKAN permohonan)

Medan mengikut spesifikasi ("Asset fields"):

| Medan | Jenis | Kenapa wujud |
|-------|-------|--------------|
| `AssetTag` | `string` (unik) | Label fizikal ditampal pada peranti (cth. `ICT-AST-0001`) — cara utama kakitangan gudang ICT kenal pasti aset secara manual, bebas daripada `Id` pangkalan data |
| `SerialNumber` | `string` | Nombor siri pengeluar — perlu untuk waranti/tuntutan insurans |
| `Category` | `string` | `Laptop`, `Desktop`, `Monitor`, `Printer`, `MobilePhone`, `NetworkEquipment`, dll. — asas carian aset tersedia mengikut kategori (Hari 14) |
| `BrandModel` | `string` | Cth. `Dell Latitude 5440` — maklumat penting untuk keserasian perisian |
| `Status` | `AssetStatus` (enum) | **Bukan** `SubmissionStatus` — lihat perbandingan di atas |
| `CurrentHolderUserId` | `string?` (nullable) | Siapa **sedang** memegang aset ini sekarang — `null` bermaksud aset di gudang, tiada pemegang |
| `Condition` | `string` | `Baik`, `Rosak Ringan`, `Perlu Baik Pulih` — direkod semula pada setiap pemulangan (Hari 14) |

**Kenapa `CurrentHolderUserId` nullable (`string?`)?** Aset yang `Available` di gudang **tiada** pemegang — memaksa medan ini wajib (`string`, bukan `string?`) akan memaksa nilai kosong palsu (`""`) yang mengelirukan permintaan `WHERE CurrentHolderUserId IS NOT NULL` kelak. Nullable reference types (`string?`) membolehkan pengkompil C# **memaksa** anda semak `null` sebelum guna nilai ini — cegah `NullReferenceException` semasa masa jalan.

> Rujukan rasmi: [learn.microsoft.com/dotnet/csharp/nullable-references](https://learn.microsoft.com/en-us/dotnet/csharp/nullable-references)

### 2. `SoftwareCatalogItem` — katalog perisian yang boleh dimohon

Bukan setiap perisian di pasaran boleh dimohon secara bebas — ICT hanya benarkan perisian yang **sudah diluluskan** (lesen sah, keserasian polisi keselamatan). `SoftwareCatalogItem` ialah senarai "menu" perisian yang sah dimohon — pemohon **pilih** daripada senarai ini, bukan taip nama perisian bebas (yang boleh membawa kepada permohonan perisian tidak sah/tidak berlesen).

### 3. `SoftwareRequest` — permohonan perisian (anak kepada `Submission`)

Seperti setiap modul lain, `SoftwareRequest` **tidak** simpan status/nombor rujukan sendiri — ia rujuk `Submission` induk (corak yang sama sejak Hari 2: `OfficerReportingApplication` → `Submission`). `SoftwareRequest` simpan **butiran khusus** permohonan: perisian yang dipohon (`SoftwareCatalogItemId`) dan sebab (`Justification`).

### 4. `AssetLoanRequest` — permohonan pinjaman aset

Perhatikan medan `AssetId` di sini **nullable** (`int?`). Ini penting: semasa pemohon **mula** memohon pinjaman, mereka tidak tahu (dan tidak patut pilih) **aset fizikal** yang mana — mereka hanya nyatakan **kategori** yang diperlukan (cth. "Laptop"). ICT Admin yang **kemudian** menetapkan aset sebenar semasa fulfillment (Hari 14) — barulah `AssetId` diisi. Ini mengelakkan pemohon "menempah" aset tertentu yang mungkin sudah `OnLoan` kepada orang lain.

### 5. `AssetReturn` — pemulangan aset (rujuk kembali `AssetLoanRequest`)

`AssetReturn` ialah **permohonan berasingan** (`Submission` sendiri, prefix `AST-R` berbeza daripada `AST-L`) — sebab pemulangan boleh berlaku **selepas** pinjaman selesai diproses sepenuhnya (`AssetLoanRequest.Submission.Status = Completed`), jadi ia perlu kitaran hidup sendiri (mungkin perlu kelulusan ICT untuk sahkan syarat pemulangan). Ia **mesti** rujuk `AssetLoanRequestId` supaya sistem tahu **aset mana** yang sedang dipulangkan.

---

## Kenapa `Category` Sebagai `string`, Bukan `enum`?

Peserta mungkin tertanya kenapa `Asset.Category` (`Laptop`/`Desktop`/...) bukan `enum` seperti `AssetStatus`. Sebab: **kategori aset ICT berkembang** dari semasa ke semasa (peranti baharu — tablet, dongle 5G, kamera webcam) tanpa perlu ubah **kod** aplikasi (yang perlukan `deploy` semula). `AssetStatus` pula ialah **peraturan perniagaan tetap** (enam status ini takkan berubah tanpa ubah keseluruhan aliran kerja) — sesuai `enum`. Corak ini: **enum untuk peraturan tetap, string/lookup untuk data yang berkembang** — sama seperti kenapa `LookupDepartments` (jadual, bukan enum) digunakan sejak Hari 1.

---

## Migration & Seed

Selepas entiti ditulis, kita jana migration baharu (bukan gantikan `InitialShared` Hari 1):

```bash
dotnet ef migrations add AddIctAssets
dotnet ef database update
```

Kemudian seed data contoh — beberapa perisian (`SoftwareCatalogItem`) dan beberapa aset (`Asset`) dengan pelbagai status supaya Hari 14 ada data sebenar untuk diuji semakan *availability*. Rujuk [`snippets/lab.md`](./snippets/lab.md) untuk kod seed penuh.

> **Nota keselamatan:** Data aset & perisian dalam seed adalah **contoh sintetik untuk latihan** — bukan inventori sebenar NRES. Gantikan dengan data sah jabatan sebelum guna dalam pengeluaran.

---

## Selepas Ini

Hari 14 akan bina **borang** (permohonan perisian, pinjaman aset, pemulangan aset), **semakan availability** aset sebelum fulfillment, dan **transaksi selamat** (`BeginTransactionAsync`) supaya status permohonan dan status aset dikemas kini **serentak** — tiada senario "permohonan kata selesai tapi aset masih tunjuk Available".

Mula hands-on: [`snippets/lab.md`](./snippets/lab.md).

---

> 🎤 **Nota penceramah/jurulatih:** [`nota-penceramah.md`](./nota-penceramah.md).
