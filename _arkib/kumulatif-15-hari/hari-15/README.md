# Hari 15 — Integrasi, Ujian & Deployment

Nota ini mengikut **aturcara rasmi SESI 44–46 + Capstone** — lihat [`../JADUAL.md`](../JADUAL.md), bahagian **HARI 15**. Hands-on penuh langkah demi langkah ada di [`snippets/lab.md`](./snippets/lab.md) — baca bahagian konsep di bawah dahulu, kemudian pindah ke lab untuk menaip kod sendiri.

> **Konvensyen kod:** Nota dalam **Bahasa Melayu**; semua kod, nama kelas/pembolehubah, nama fail, istilah teknikal dikekalkan dalam **Bahasa Inggeris** — ikut [`SPEC-KURSUS.md`](../SPEC-KURSUS.md).

> **Hari terakhir, bukan modul baharu.** Hari 15 **tidak** memperkenalkan entiti atau peraturan perniagaan baharu. Tugas hari ini ialah **menyambungkan** 5 modul yang sudah dibina (Hari 1–14) menjadi **satu** aplikasi koheren, membuktikannya betul dengan **ujian automatik**, dan menyediakannya untuk **pengeluaran (production)**.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| ASP.NET Core — Layout & partial views (navigasi kongsi) | [learn.microsoft.com/aspnet/core/mvc/views/layout](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/layout) |
| ASP.NET Core — View Components (dashboard widget) | [learn.microsoft.com/aspnet/core/mvc/views/view-components](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/view-components) |
| ASP.NET Core Identity — `User.IsInRole()` di Razor | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |
| xUnit — Unit testing asas .NET | [learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) |
| ASP.NET Core — Integration tests (`WebApplicationFactory`) | [learn.microsoft.com/aspnet/core/test/integration-tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests) |
| EF Core — Testing (SQLite in-memory) | [learn.microsoft.com/ef/core/testing](https://learn.microsoft.com/en-us/ef/core/testing/) |
| ASP.NET Core — Configuration & `appsettings` per persekitaran | [learn.microsoft.com/aspnet/core/fundamentals/configuration](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/) |
| EF Core — Applying migrations di pengeluaran | [learn.microsoft.com/ef/core/managing-schemas/migrations/applying](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying) |
| ASP.NET Core — Deploy ke IIS | [learn.microsoft.com/aspnet/core/host-and-deploy/iis](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/) |
| ASP.NET Core — Deploy ke Linux (systemd/Nginx) | [learn.microsoft.com/aspnet/core/host-and-deploy/linux-nginx](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx) |
| ASP.NET Core — Deploy dengan Docker | [learn.microsoft.com/aspnet/core/host-and-deploy/docker](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/docker/) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 10.30 pagi** | **SESI 44: Integrasi** — navigasi kongsi, dashboard (draf saya, dihantar, menunggu kelulusan, selesai), carian rujukan global, menu ikut peranan. 💻 **Lab:** integrasi |
| **10.30 – 1.00 tgh** | **SESI 45: Ujian xUnit** — uji nombor rujukan, peralihan status, semakan pendua, availability aset, sebab reject wajib. 💻 **Lab:** unit + integration tests |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.45 petang** | **SESI 46: Deployment** — `appsettings` per persekitaran, tukar SQLite → SQL Server, HTTPS, folder muat naik, IIS/Linux/kontena. 💻 **Lab:** senarai semak keluaran |
| **3.45 – 5.00 petang** | **Projek Capstone: Demo & Sijil** — skrip ujian manual 11 langkah, pembentangan, penilaian, penyampaian sijil |
| 5.00 petang | Bersurai |

**Hasil Hari 15** (rujuk [`JADUAL.md`](../JADUAL.md)): Aplikasi NRES bersepadu 5 modul, berujian, boleh-deploy, dibentangkan.

---

## Kenapa Integrasi Perlu Hari Berasingan?

Sepanjang Hari 2–14, setiap modul dibina **secara berasingan** — peserta fokus satu modul pada satu masa. Tetapi pengguna sebenar sistem NRES **tidak** peduli modul mana permohonan mereka tergolong; mereka mahu **satu** tempat untuk lihat "apa status semua permohonan saya" tanpa perlu ingat 5 URL berbeza. Integrasi hari ini menyambungkan modul-modul melalui **satu** titik kongsi yang sudah wujud sejak Hari 1: jadual `Submission`.

Kerana **setiap** modul (Lapor Diri, Pas/Parking/Pelekat, ID/AD/Email, PKS, Aset ICT) menyimpan status & nombor rujukannya dalam `Submission` induk yang **sama**, dashboard bersepadu hanyalah **satu** query terhadap `Submissions` — **bukan** 5 query berasingan digabung secara manual:

```csharp
var myDrafts = await _db.Submissions
    .Where(s => s.ApplicantUserId == userId && s.Status == SubmissionStatus.Draft)
    .OrderByDescending(s => s.CreatedAt)
    .ToListAsync();
```

Inilah **bukti nyata** kenapa corak "satu `Submission` induk dikongsi 5 modul" (diperkenalkan Hari 1) berbaloi — tanpanya, dashboard bersepadu memerlukan `UNION` manual merentasi 9 jadual berbeza (`OfficerReportingApplications`, `AccessPassApplications`, ..., `AssetReturns`).

---

## Dashboard: Empat Kategori, Satu Query Asas

| Kategori | Syarat |
|----------|--------|
| **Draf Saya** | `ApplicantUserId == userId && Status == Draft` |
| **Dihantar** | `ApplicantUserId == userId && Status == Submitted` |
| **Menunggu Kelulusan Saya** | (untuk admin/supervisor) `Status == Submitted` **dan** `ModuleCode` sepadan tanggungjawab role — cth. `IctAdmin` hanya nampak `ModuleCode IN ("SW", "AST-L", "AST-R")` |
| **Selesai** | `ApplicantUserId == userId && Status == Completed` |

**Kenapa "Menunggu Kelulusan Saya" ditapis ikut `ModuleCode`, bukan papar semua permohonan `Submitted`?** Seorang `IctAdmin` tiada kelayakan (dan tiada minat) meluluskan permohonan Lapor Diri — memaparkan semua permohonan tanpa tapisan role akan menyesakkan skrin dengan kerja bukan tanggungjawab mereka. Peta `ModuleCode → Role` (jadual di bawah) ialah **satu** sumber kebenaran untuk penapisan ini.

| `ModuleCode` | Role bertanggungjawab |
|--------------|------------------------|
| `LD` | `HrAdmin` |
| `PAS`, `PKR`, `STK` | `SecurityAdmin` |
| `ICT-ID` | `IctAdmin` (selepas `Supervisor`) |
| `PKS` | `ComplianceAdmin` |
| `SW`, `AST-L`, `AST-R` | `IctAdmin` |

---

## Carian Rujukan Global

Kakitangan sering ingat **nombor rujukan** (cth. `AST-L-2026-0007`) tetapi tidak ingat **modul mana**. Carian global membenarkan satu medan input mencari merentasi **semua** `Submission`, tanpa pengguna perlu tahu ke controller/jadual mana rujukan itu tergolong:

```csharp
var result = await _db.Submissions
    .Where(s => s.ReferenceNo.Contains(query))
    .OrderByDescending(s => s.CreatedAt)
    .ToListAsync();
```

Selepas jumpa `Submission`, halaman keputusan carian guna `ModuleCode` untuk tahu **butiran** mana perlu dimuatkan (`SoftwareRequest`, `AssetLoanRequest`, dll.) dan URL detail mana untuk pautkan.

---

## Menu Ikut Peranan — Kongsi Satu Prinsip Dengan Authorization Controller

Sejak Hari 8, kita sudah kuatkuasa `[Authorize(Roles = "...")]` pada **controller actions** — ini kekal **wajib**, kerana pengguna nakal boleh cuba akses URL secara terus walaupun pautan menu disembunyikan. Menu ikut peranan (`User.IsInRole("IctAdmin")` dalam `_Layout.cshtml`) ialah **lapisan UX tambahan**, bukan pengganti kepada authorization sebenar — ia sekadar **elak kekeliruan** (kenapa tunjuk pautan yang klik pun akan ditolak 403).

> **Prinsip keselamatan berulang (rujuk `nota/09-keselamatan.md`):** *"Kuatkuasa authorization di controller, bukan hanya di UI."* Menyembunyikan butang di Razor **tidak** menghalang sesiapa hantar `POST` terus ke URL action — authorization sebenar mesti berada di `[Authorize]` pada controller/action.

---

## Kenapa Ujian Automatik Sekarang, Bukan Dari Hari 1?

Secara ideal, ujian ditulis **serentak** dengan kod (Test-Driven Development). Tetapi untuk kursus coaching 15 hari, kita sengaja tangguhkan xUnit ke Hari 15 supaya peserta **fokus** kuasai satu konsep pada satu masa (EF Core, MVC, Identity, transaksi) sebelum tambah lapisan ujian. **Ini bukan amalan produksi disyorkan** — dalam projek sebenar, tulis ujian **seiring** pembangunan. Hari ini kita tunjuk **cara** menulis ujian untuk kod yang sudah wujud, konsep yang boleh terus dipakai bermula esok pada projek sebenar peserta.

Dua jenis ujian:

| Jenis | Contoh dalam projek ini | Alat |
|-------|--------------------------|------|
| **Unit test** | Nombor rujukan, peralihan status, semakan pendua, availability aset, sebab reject wajib | xUnit tulen, mock/stub minimum |
| **Integration test** | Submit Lapor Diri, approve, reject dengan sebab, muat naik jenis fail tidak sah, selesaikan pinjaman aset & sahkan status aset berubah | xUnit + EF Core SQLite in-memory (`DbContextOptionsBuilder.UseSqlite("DataSource=:memory:")`) |

> Rujukan rasmi: [learn.microsoft.com/dotnet/core/testing/unit-testing-with-dotnet-test](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test) · [learn.microsoft.com/ef/core/testing](https://learn.microsoft.com/en-us/ef/core/testing/)

---

## Kenapa SQLite In-Memory, Bukan Fail SQLite Sebenar, Untuk Ujian?

Ujian mesti **boleh ulang** (*repeatable*) dan **terasing** (*isolated*) — setiap ujian mula dengan pangkalan data **kosong**, tanpa kesan sisa daripada ujian lain. Fail SQLite sebenar (`nres_onboarding.db`) berkongsi keadaan merentasi larian ujian (dan merentasi mesin pembangun lain) — punca ujian "kadang lulus, kadang gagal" bergantung susunan larian. SQLite in-memory (`:memory:`) cipta pangkalan data **baharu** setiap kali ujian dijalankan, dan ia hilang sepenuhnya selepas sambungan ditutup — sempurna untuk ujian.

---

## Kenapa Deployment Perlu Senarai Semak, Bukan Sekadar "`dotnet publish`"?

`dotnet publish` menghasilkan binari boleh jalan, tetapi **tidak** secara automatik: jalankan migration di pangkalan data pengeluaran, tetapkan kebenaran folder muat naik, konfigurasi HTTPS, atau tukar penyedia pangkalan data. Setiap langkah ini **mesti** disahkan secara manual (atau via skrip CI/CD) sebelum sistem sedia untuk pengguna sebenar. Senarai semak Hari 15 (lihat [`snippets/lab.md`](./snippets/lab.md)) memastikan tiada langkah kritikal terlepas pandang.

**Penukaran SQLite → SQL Server** — perhatikan betapa **kecil** perubahan kod yang diperlukan, buah hasil daripada reka bentuk EF Core berasaskan abstraksi `DbContext`:

```csharp
// Latihan (appsettings.Development.json / Program.cs):
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));

// Pengeluaran (appsettings.Production.json / Program.cs):
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
```

Tiada kod **controller**, **view model**, atau **entiti** perlu diubah — inilah nilai sebenar ORM (Object-Relational Mapper): kod aplikasi tidak bergantung terus pada pangkalan data spesifik.

---

## Corak Aliran Kerja — Rumusan Muktamad

Sepanjang 15 hari, kita ulang **satu** corak, lima kali, merentasi lima modul berbeza:

```text
Form → Validation → Draft → Submit → Review → Approve/Reject → Audit → Report
```

Bacaan penuh mesej penutup kursus (**Mesej Coaching Akhir**) ada dalam [`nota-penceramah.md`](./nota-penceramah.md) — penceramah **wajib** sampaikan ini sebelum sesi capstone bermula.

---

## Selepas Ini

Tiada "hari 16". Hari ini berakhir dengan **skrip ujian manual 11 langkah** merentasi kelima-lima modul, **demo capstone**, dan **penyampaian sijil**. Mula hands-on: [`snippets/lab.md`](./snippets/lab.md).

---

> 🎤 **Nota penceramah/jurulatih:** [`nota-penceramah.md`](./nota-penceramah.md) — termasuk **Mesej Coaching Akhir**.
