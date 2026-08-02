# SPEC-KURSUS — Kanun Rujukan Tunggal (Single Source of Truth)

> **Untuk penulis kandungan (manusia & ejen):** Setiap nama kelas, enum, peranan (role), prefix nombor rujukan, nama cabang Git, dan skop harian **MESTI** sepadan dengan dokumen ini. Jangan cipta variasi sendiri. Sumber rasmi skop: `cadangan_silibus_coaching_15hari_NRES.docx` (cadangan silibus NRES). Sumber domain penuh: `coach-nres/nres-dotnet-15-day-coaching-guide.md` (repo jiran).

Kursus: **Latihan Secara *Coaching* Pembangunan Sistem Onboarding & Khidmat Dalaman NRES Menggunakan ASP.NET Core** — 15 hari, hands-on, berpaksikan lab. Kod kursus **DOTNET-NRES-15**.

Projek kursus tunggal: **`Nres.Onboarding.Web`** — satu aplikasi ASP.NET Core MVC dalaman yang menyatukan **4 modul** permohonan & aliran kerja kelulusan.

---

## Model penyampaian — MUKTAMAD

Kursus ini **bukan** satu kohort yang membina semua modul berturutan. Ia mengikut model **4 kumpulan dedicated bekerja selari** seperti ditetapkan cadangan silibus NRES:

| Fasa | Hari | Mod | Kandungan |
|------|------|-----|-----------|
| **Fasa 1** | **1 – 3** | **Sesi bersama** (semua kumpulan) | Perancangan, dokumentasi, URS/SRS, ERD & diagram · Git, branching, Agile & kolaborasi · Refresher .NET + asas kongsi |
| **Fasa 2** | **4 – 14** | **4 trek selari** (setiap kumpulan modulnya sendiri) | Pembangunan modul mengikut kumpulan, pada cabang Git masing-masing |
| **Fasa 3** | **15** | **Sesi bersama** | Penggabungan kod, Papan Pemuka Induk, SIT & UAT pre-check, demo |

> **Kenapa Fasa 1 tiga hari (bukan dua seperti docx):** cadangan NRES memperuntukkan Hari 1–2 sahaja untuk sesi bersama. Skop Fasa 1 telah dikembangkan atas permintaan pemilik kursus untuk merangkumi **perancangan projek, dokumentasi, URS, ERD, Agile (Jira/GitHub Projects), kolaborasi pasukan, dan penggunaan AI** — topik ini tidak muat dalam dua hari bersama-sama refresher .NET dan pembinaan asas kongsi. Kesannya: setiap trek kumpulan mendapat **11 hari** (Hari 4–14) berbanding 12 hari dalam docx; blok terakhir setiap trek dipendekkan dari 3 hari ke 2 hari.

### Kenapa asas kongsi mesti siap Hari 3, sebelum kumpulan bercabang

Keempat-empat kumpulan bekerja dalam **satu repositori dan satu aplikasi**, kemudian bergabung pada Hari 15. Entiti kongsi (`Submission`, `Attachment`, `AuditLog`, `ApprovalStep`, `UserProfile`), `ApplicationDbContext`, konfigurasi Identity, dan migration `InitialShared` **mesti dibina sekali sahaja, bersama-sama, pada Hari 3**. Jika setiap kumpulan membinanya sendiri, Hari 15 akan menjadi konflik gabungan (merge conflict) yang tidak boleh diselesaikan.

---

## Konvensyen bahasa

Nota, penerangan & agenda dalam **Bahasa Melayu**. Semua **kod, nama kelas/pembolehubah, nama fail, istilah teknikal** (`Controller`, `DbContext`, `migration`, `view model`, `pull request`) dikekalkan dalam **Bahasa Inggeris** — amalan standard industri .NET.

## Susunan teknologi (Tech Stack) — MUKTAMAD

| Lapisan | Pilihan |
|---------|---------|
| Bahasa | **C# 14** (lalai bagi .NET 10 SDK — Roslyn 5.0; tiada `<LangVersion>` diperlukan) |
| Rangka web | **ASP.NET Core MVC** (.NET 10 LTS) |
| ORM | **Entity Framework Core 10** |
| Pangkalan data (latihan) | **SQLite** (mula pantas, sifar pemasangan) |
| Pangkalan data (pengeluaran) | SQL Server / PostgreSQL |
| Authentication | **ASP.NET Core Identity** |
| Authorization | Role-based + policy |
| Storan fail | Folder peribadi luar `wwwroot` (`App_Data/uploads/`) |
| Laporan | Razor print view · CSV export · PDF (QuestPDF) · Excel (ClosedXML) |
| Kod QR / Barcode | **QRCoder** (Kumpulan 2) |
| Ujian | **xUnit** + EF Core SQLite/in-memory |
| Kawalan versi | **Git** + GitHub (repo tunggal, 4 cabang kumpulan) |
| Pengurusan kerja | **GitHub Projects** (hands-on) + **Jira** (demo & pemetaan) |
| IDE | Visual Studio 2022 (17.12+) / VS Code + C# Dev Kit |
| SDK | **.NET 10 SDK** (`dotnet --version` → `10.x`) |

> **Nota versi — MUKTAMAD:** cadangan silibus NRES menulis **.NET 8**. Keputusan yang **disahkan** ialah **.NET 10 LTS / EF Core 10 / C# 14**. Semua contoh kod mesti sah untuk .NET 10 (primary constructors, collection expressions, nullable reference types dihidupkan, `dotnet ef` CLI). Isu ini **ditutup** — jangan tulis kandungan yang menyokong dua versi.

> **Buku rujukan rasmi:** *C# 14 and .NET 10 — Modern Cross-Platform Development Fundamentals* (Mark J. Price, Packt, Nov 2025) · repo kod [github.com/habibtalib/cs14net10](https://github.com/habibtalib/cs14net10). Pemetaan penuh kursus → bab, dan senarai ciri C# 12/13/14 yang digunakan: [`nota/10-rujukan-buku.md`](./nota/10-rujukan-buku.md). Buku **tidak wajib** — lab lengkap dengan sendirinya. Ambil perhatian buku mengutamakan **Blazor**, bukan MVC; untuk MVC rujuk Microsoft Docs.

> **Kenapa SQLite untuk latihan:** peserta boleh mula tanpa memasang SQL Server. Tukar penyedia (provider) ke SQL Server hanya dengan menukar `UseSqlite` → `UseSqlServer` + connection string. Ditunjukkan pada Hari 15.

---

## 4 Modul & Kumpulan — MUKTAMAD

| Kumpulan | Modul | Prefix | Admin peranan | Trek |
|----------|-------|--------|---------------|------|
| **Kumpulan 1** | **Lapor Diri** — permohonan laporan diri pekerja baharu | `LD` | `HrAdmin` | `kumpulan-1-lapor-diri/` |
| **Kumpulan 2** | **Pas, Parkir & Pelekat Kenderaan** — akses kawasan & keselamatan kenderaan | `PAS` · `PKR` · `STK` | `SecurityAdmin` | `kumpulan-2-pas-parkir-pelekat/` |
| **Kumpulan 3** | **ID, AD & Email** — akaun pengguna & akses sistem | `ICT-ID` | `IctAdmin` | `kumpulan-3-id-ad-email/` |
| **Kumpulan 4** | **Perisian & Aset ICT** — permohonan, pinjaman & pemulangan aset | `SW` · `AST-L` · `AST-R` | `IctAdmin` | `kumpulan-4-perisian-aset-ict/` |

> **Modul PKS (Pematuhan Kod Setia) berada DI LUAR SKOP.** Cadangan silibus NRES hanya menyenaraikan empat kumpulan dan tidak menyebut PKS. Jangan tulis kandungan PKS baharu, jangan rujuk `ComplianceDeclaration` / `PolicyVersion` / peranan `ComplianceAdmin` dalam bahan aktif. Draf lama disimpan di `_arkib/kumulatif-15-hari/hari-10/`–`hari-12/`.

---

## Struktur projek (monolit ringkas — guna sepanjang kursus)

```text
Nres.Onboarding.Web/
  Controllers/
  Data/                 # ApplicationDbContext, seed
  Models/               # entiti (domain)
    Shared/             # Hari 3 — dikongsi semua kumpulan
    LaporDiri/          # Kumpulan 1
    Akses/              # Kumpulan 2
    Akaun/              # Kumpulan 3
    Aset/               # Kumpulan 4
  ViewModels/
  Services/             # IReferenceNumberService, IFileStorageService, dll.
  Views/
    Shared/
    OfficerReporting/   # Kumpulan 1
    AccessPass/ ...     # Kumpulan 2
    AccountRequest/     # Kumpulan 3
    Asset/ Software/    # Kumpulan 4
  wwwroot/
  App_Data/uploads/     # fail dimuat naik (bukan bawah wwwroot)
Nres.Onboarding.Tests/  # xUnit
```

> **Peraturan anti-konflik:** setiap kumpulan **hanya** mencipta fail dalam folder modulnya sendiri. Fail kongsi (`Program.cs`, `ApplicationDbContext.cs`, `_Layout.cshtml`) **tidak disentuh langsung selepas Hari 3** — ia direka supaya modul mendaftar diri, bukan diedit. Kontrak penuh: [`KOLABORASI.md`](./KOLABORASI.md).

---

## Kolaborasi, anti-konflik & anti-redundan — TERAS KURSUS

Empat kumpulan menulis kod serentak dalam satu repositori, keempat-empatnya dibantu AI. Tanpa disiplin, dua perkara **pasti** berlaku menjelang Hari 15: (1) konflik gabungan pada fail kongsi, dan (2) empat versi berlainan bagi logik yang sama (empat servis nombor rujukan, empat panel kelulusan, empat cara audit). AI **memburukkan** kedua-duanya — ia dengan senang hati menjana semula sesuatu yang sudah wujud kerana ia tidak tahu apa yang pasukan lain sudah tulis.

Kursus ini menangani risiko itu secara **seni bina**, bukan sekadar peraturan:

| Sumber konflik | Penyelesaian seni bina | Diajar |
|----------------|------------------------|--------|
| 4 kumpulan edit `Program.cs` | Setiap modul ada `Add<Modul>Module(this IServiceCollection)` sendiri; `Program.cs` memanggil 4 baris yang ditulis **sekali** pada Hari 3 | Hari 2–3 |
| 4 kumpulan edit `ApplicationDbContext` | Tiada `DbSet` ditambah manual — guna `IEntityTypeConfiguration<T>` dalam folder modul + `ApplyConfigurationsFromAssembly()` dipanggil sekali pada Hari 3 | Hari 3 |
| 4 kumpulan edit menu `_Layout.cshtml` | Navigasi dijana dari `ModuleDescriptor` — setiap kumpulan tambah **fail baharunya sendiri** | Hari 3 |
| 4 kumpulan jana migration serentak (`ModelSnapshot` bertembung) | **Slot migration**: satu kumpulan sahaja `dotnet ef migrations add` pada satu masa, diumumkan di board; wajib `pull --rebase` dahulu | Hari 2–3 |
| 4 versi logik yang sama | **Daftar servis & komponen kongsi** — dibina Hari 3, disenaraikan sebagai "sudah wujud, jangan tulis semula" | Hari 3 |
| AI jana semula kod sedia ada | **`AGENTS.md` kongsi** dibaca oleh AI setiap kumpulan + peraturan *cari dahulu, jana kemudian* | Hari 1–2 |
| Proses berulang antara kumpulan | **Satu** board, **satu** Definition of Done, **satu** templat PR, **satu** senarai semak review | Hari 2 |

**Dua fail kontrak yang mengikat semua kumpulan:**

- [`KOLABORASI.md`](./KOLABORASI.md) — matriks pemilikan fail, protokol fail kongsi, slot migration, Definition of Done, aliran PR & code review, proses permintaan komponen kongsi.
- [`AGENTS.md`](./AGENTS.md) — konteks AI kongsi. **Setiap** kumpulan menghalakan pembantu AI-nya ke fail ini supaya keempat-empat pasukan menjana kod dengan konvensyen, nama, dan struktur yang **sama** — ini yang menghalang empat gaya kod berbeza bergabung pada Hari 15.

> **Peraturan emas anti-redundan:** sebelum menulis mana-mana helper, servis, atau partial view — **cari dalam repo dahulu** (`grep`/`Ctrl+T`), kemudian tanya AI *"adakah ini sudah wujud dalam repo ini?"* sambil merujuk `AGENTS.md`. Jika sesuatu berguna untuk lebih daripada satu modul, ia **bukan** milik folder kumpulan anda — buka isu berlabel `shared` (lihat `KOLABORASI.md`).

---

## Strategi percabangan Git — MUKTAMAD

```text
master                        # integrasi; dilindungi, merge melalui PR sahaja
├── asas/shared-foundation    # Hari 3 — digabung ke master hujung Hari 3
├── kump-1/lapor-diri         # Kumpulan 1, Hari 4–14
├── kump-2/akses-kenderaan    # Kumpulan 2, Hari 4–14
├── kump-3/id-ad-email        # Kumpulan 3, Hari 4–14
└── kump-4/perisian-aset      # Kumpulan 4, Hari 4–14
```

- Setiap kumpulan bercabang dari `master` **selepas** `asas/shared-foundation` digabung (hujung Hari 3).
- Kerja harian dalam cabang ciri pendek: `kump-2/feat/semakan-pendua-plat` → PR ke cabang kumpulan.
- `git pull --rebase origin master` **setiap pagi** untuk kekal segerak dan mengecilkan konflik Hari 15.
- Hari 15: keempat-empat cabang kumpulan digabung ke `master` mengikut turutan berjadual.

### Format commit

```text
<modul>: <apa yang berubah dalam Bahasa Melayu ringkas>

Contoh:
lapor-diri: tambah muat naik lampiran dan metadata Attachment
akses: sekat permohonan pelekat pendua bagi nombor plat sama
```

Jika kumpulan menggunakan Jira, sertakan issue key di hadapan: `NRES-42 lapor-diri: ...`

---

## Enum status — KONGSI SEMUA MODUL

```csharp
public enum SubmissionStatus
{
    Draft = 0,
    Submitted = 1,
    SupervisorApproved = 2,
    AdminApproved = 3,
    Rejected = 4,
    Completed = 5,
    Cancelled = 6
}
```

Corak aliran universal (ulang untuk setiap modul):

```text
Form → Validation → Draft → Submit → Review → Approve/Reject → Audit → Report
```

> Cadangan NRES menyebut status `Under Review` bagi Kumpulan 1. Ia dipetakan kepada `Submitted` (permohonan sudah di tangan admin, belum diputuskan) — **jangan** tambah ahli enum baharu tanpa mengemas kini dokumen ini.

## Peranan (Roles) — KONGSI

| Role | Tanggungjawab | Kumpulan berkaitan |
|------|---------------|--------------------|
| `Applicant` | Cipta draf & hantar permohonan | Semua |
| `Supervisor` | Semak permohonan staf (kelulusan peringkat 1) | 3 (utama), 1 |
| `HrAdmin` | Semak Lapor Diri | 1 |
| `SecurityAdmin` | Semak pas, parkir, pelekat kenderaan | 2 |
| `IctAdmin` | Semak AD/email, perisian, aset ICT | 3, 4 |
| `SystemAdmin` | Urus pengguna & data lookup | Semua |

## Entiti kongsi (shared) — dibina Hari 3

`Submission` (induk), `Attachment`, `AuditLog`, `ApprovalStep`, `UserProfile`, dan lookup: `LookupDepartments`, `LookupGrades`, `LookupPositions`.

## Servis kongsi (shared services) — dibina Hari 3

`IReferenceNumberService`, `IFileStorageService`, `IAuditLogService`, `IWorkflowService`, `INotificationService` (guna `ConsoleNotificationService` untuk latihan), `ICurrentUserService`.

## Prefix nombor rujukan — MUKTAMAD

| Modul | Kumpulan | Prefix | Contoh |
|-------|----------|--------|--------|
| Lapor Diri | 1 | `LD` | `LD-2026-0001` |
| Pas Keselamatan | 2 | `PAS` | `PAS-2026-0001` |
| Parkir | 2 | `PKR` | `PKR-2026-0001` |
| Pelekat Kenderaan | 2 | `STK` | `STK-2026-0001` |
| ID AD & Email | 3 | `ICT-ID` | `ICT-ID-2026-0001` |
| Perisian | 4 | `SW` | `SW-2026-0001` |
| Pinjaman Aset | 4 | `AST-L` | `AST-L-2026-0001` |
| Pemulangan Aset | 4 | `AST-R` | `AST-R-2026-0001` |

## Jadual entiti per kumpulan

| Kumpulan | Tables |
|----------|--------|
| Kongsi (Hari 3) | `Submissions`, `Attachments`, `AuditLogs`, `ApprovalSteps`, `UserProfiles`, `Lookup*` |
| 1 · Lapor Diri | `OfficerReportingApplications` |
| 2 · Pas/Parkir/Pelekat | `AccessPassApplications`, `ParkingApplications`, `VehicleStickerApplications`, `Vehicles` |
| 3 · ID AD & Email | `AccountRequests`, `RequestedSystemAccesses` |
| 4 · Perisian & Aset ICT | `Assets`, `SoftwareCatalogItems`, `SoftwareRequests`, `AssetLoanRequests`, `AssetReturns` |

---

## Pemetaan 15 Hari — MUKTAMAD

### Fasa 1 — Sesi bersama (Hari 1–3)

| Hari | Fokus utama |
|------|-------------|
| **1** | Perancangan projek & skop · dokumentasi · **URS/SRS** · **Use Case & Process Flow** · **ERD** — kesemuanya dengan bantuan AI |
| **2** | **Git & strategi percabangan** · **Agile** (GitHub Projects hands-on + Jira demo) · kolaborasi pasukan & code review · persediaan persekitaran (.NET 10 SDK, IDE, tools) |
| **3** | **Refresher .NET**: C# OOP/LINQ/DI/async · EF Core (DbContext, annotations, Fluent API, migration) · MVC + validation · Identity + RBAC. **Deliverable:** entiti kongsi + `ApplicationDbContext` + migration `InitialShared` digabung ke `master`; 4 cabang kumpulan dibuka |

### Fasa 2 — 4 trek selari (Hari 4–14)

| Blok | K1 · Lapor Diri | K2 · Pas/Parkir/Pelekat | K3 · ID/AD/Email | K4 · Perisian & Aset ICT |
|------|-----------------|--------------------------|------------------|---------------------------|
| **Hari 4** | Skema DB + borang draf | Skema DB akses & kenderaan | Skema DB akaun & akses | Katalog aset & perisian + seed |
| **Hari 5–6** | Muat naik dokumen + no. rujukan `LD` | Borang + semakan pendua no. plat | Borang akaun + kelulusan Penyelia | Borang + semakan stok masa-nyata |
| **Hari 7–9** | Aliran kelulusan HR + skrin admin | Semakan keselamatan + kelulusan bersyarat | Pemprosesan ICT + RBAC + simulasi AD | Kelulusan ICT + pemulangan aset |
| **Hari 10–12** | Notifikasi e-mel + Slip Akuan PDF + dashboard HR | QR/Barcode + skrin ronda + laporan | Penjejakan status + audit trail + dashboard ICT | Peringatan lewat tempoh + dashboard inventori + eksport PDF/Excel |
| **Hari 13–14** | xUnit + refactor + sedia merge | Ujian E2E + bug fixing + sedia merge | RBAC testing + security audit + sedia merge | Ujian + refactor + sedia merge |

### Fasa 3 — Sesi bersama (Hari 15)

| Hari | Fokus utama |
|------|-------------|
| **15** | Penggabungan 4 cabang ke `master` · Papan Pemuka Induk NRES · **SIT + UAT pre-check** (aliran rentas modul, RBAC, muat naik fail, audit log) · demo & penilaian capstone |

---

## Rentak harian (setiap hari)

Pendaftaran & minum pagi **8.30–9.00** · SESI PAGI **9.00–1.00** · rehat & makan **1.00–2.30** · SESI PETANG **2.30–5.00** · bersurai **5.00**. ~7 jam kontak/hari. Setiap hari: **≥60% masa hands-on lab**.

> **Rentak Fasa 2:** setiap pagi bermula dengan **stand-up 15 minit per kumpulan** (semalam / hari ini / halangan) dan `git pull --rebase`. Setiap petang berakhir dengan **commit + push + kemas kini board**. Jurulatih berpusing antara 4 kumpulan.

---

## Format fail

### Sesi bersama — `hari-1/`, `hari-2/`, `hari-3/`, `hari-15/`

### Trek kumpulan — `kumpulan-N-<slug>/hari-4/`, `hari-5-6/`, `hari-7-9/`, `hari-10-12/`, `hari-13-14/`

Setiap folder **MESTI** mengandungi 3 fail:

1. **`README.md`** — nota konsep Bahasa Melayu: fokus, jadual waktu, penerangan **kenapa** setiap konsep wujud, rujukan rasmi Microsoft Docs. Terangkan konsep dahulu; hands-on penuh ada di `snippets/lab.md`.
2. **`snippets/lab.md`** — lab hands-on langkah demi langkah bernombor (Latihan 0, 1, 2, …), setiap satu dengan **Objektif**, langkah, blok kod penuh untuk ditaip, dan **✅ Semakan** di hujung. Ini bahagian paling penting kursus.
3. **`nota-penceramah.md`** — nota penceramah: pemasaan setiap sesi, poin bercakap, silap biasa peserta, soalan untuk cetus perbincangan, deliverable akhir.

Setiap `README.md` mula dengan pautan ke `JADUAL.md` dan (bagi trek) ke `README.md` kumpulan.

Setiap folder kumpulan turut ada **`README.md` peringkat trek** — gambaran modul, entiti, prefix, peranan, cabang Git, dan senarai 5 blok.

---

## Penggunaan AI dalam kursus

AI (Claude, Copilot, dll.) diajar sebagai **alat bantu berdisiplin**, bukan penjana jawapan. Diperkenalkan Hari 1 dan digunakan sepanjang kursus.

**Guna AI untuk:** draf pertama URS/use case/ERD · jana data ujian sintetik · terangkan mesej ralat · cadang nama & struktur · semak kod sendiri · tulis draf ujian xUnit · ringkaskan diff untuk code review.

**Jangan guna AI untuk:** menerima kod tanpa faham · reka keperluan pengguna yang sepatutnya datang daripada NRES · jana skema DB yang bercanggah dengan SPEC ini · tampal data NRES sebenar ke dalam prompt.

> **Peraturan tetap kursus:** setiap output AI mesti melalui **semakan manusia** — peserta menerangkan kod itu kepada rakan sekumpulan sebelum ia di-commit. Diperiksa semasa code review Hari 2 dan penilaian capstone.

---

## Kriteria Penilaian (Capstone)

| Kriteria | Wajaran |
|----------|---------|
| Modul kumpulan lengkap & berfungsi | 25% |
| Corak aliran kerja betul (draft→submit→approve→audit) | 15% |
| Validation, authorization & keselamatan | 15% |
| Ujian (xUnit) & kualiti kod | 15% |
| **Kolaborasi Git & Agile** (kualiti commit, PR, board, merge bersih) | 15% |
| **Dokumentasi** (URS, ERD, diagram, README modul) | 10% |
| Pembentangan demo | 5% |

> Peserta yang menyiapkan semua lab treknya, menyumbang kepada gabungan Hari 15, dan membentangkan demo menerima **Sijil Penyertaan** — *Pembangunan Sistem Dalaman NRES Dengan ASP.NET Core*.

## Jangan

- Jangan simpan kata laluan sebenar dalam modul ID/AD/Email (ajar peserta **jangan** — ini titik pengajaran keselamatan).
- Jangan guna data NRES sebenar — semua contoh **sintetik**.
- Jangan tukar `SubmissionStatus`, peranan, prefix rujukan, atau nama cabang tanpa mengemas kini dokumen ini dahulu.
- Jangan tulis kandungan modul PKS dalam bahan aktif — ia di luar skop.
- Jangan biarkan satu kumpulan mengubah fail kongsi tanpa protokol Hari 2.
