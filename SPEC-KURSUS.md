# SPEC-KURSUS — Kanun Rujukan Tunggal (Single Source of Truth)

> **Untuk penulis kandungan (manusia & ejen):** Setiap nama kelas, enum, peranan (role), prefix nombor rujukan, dan skop harian **MESTI** sepadan dengan dokumen ini. Jangan cipta variasi sendiri. Jika satu hari perlu entiti baharu, ia mesti mengikut corak di sini. Sumber domain penuh: `coach-nres/nres-dotnet-15-day-coaching-guide.md` (repo jiran).

Kursus: **Latihan Secara *Coaching* Pembangunan Sistem Onboarding & Khidmat Dalaman NRES Menggunakan ASP.NET Core** — 15 hari, hands-on, berpaksikan lab. Kod kursus **DOTNET-NRES-15**.

Projek kursus tunggal (dibina kumulatif sepanjang 15 hari): **`Nres.Onboarding.Web`** — satu aplikasi ASP.NET Core MVC dalaman yang menyatukan **5 modul** permohonan & aliran kerja kelulusan.

## Konvensyen bahasa

Nota, penerangan & agenda dalam **Bahasa Melayu**. Semua **kod, nama kelas/pembolehubah, nama fail, istilah teknikal** (`Controller`, `DbContext`, `migration`, `view model`) dikekalkan dalam **Bahasa Inggeris** — amalan standard industri .NET.

## Susunan teknologi (Tech Stack) — MUKTAMAD

| Lapisan | Pilihan |
|---------|---------|
| Rangka web | **ASP.NET Core MVC** (.NET 10 LTS) |
| ORM | **Entity Framework Core 10** |
| Pangkalan data (latihan) | **SQLite** (mula pantas, sifar pemasangan) |
| Pangkalan data (pengeluaran) | SQL Server / PostgreSQL |
| Authentication | **ASP.NET Core Identity** |
| Authorization | Role-based + policy |
| Storan fail | Folder peribadi luar `wwwroot` (`App_Data/uploads/`) |
| Laporan | Razor print view + CSV export |
| Ujian | **xUnit** + EF Core SQLite/in-memory |
| IDE | Visual Studio 2022 (17.12+) / VS Code + C# Dev Kit |
| SDK | **.NET 10 SDK** (`dotnet --version` → `10.x`) |

> **Kenapa SQLite untuk latihan:** peserta boleh mula tanpa memasang SQL Server. Tukar penyedia (provider) ke SQL Server hanya dengan menukar `UseSqlite` → `UseSqlServer` + connection string. Ditunjukkan pada Hari 15.

## Struktur projek (monolit ringkas — guna sepanjang kursus)

```text
Nres.Onboarding.Web/
  Controllers/
  Data/                 # ApplicationDbContext, seed
  Models/               # entiti (domain)
  ViewModels/
  Services/             # IReferenceNumberService, IFileStorageService, dll.
  Views/
  wwwroot/
  App_Data/uploads/     # fail dimuat naik (bukan bawah wwwroot)
Nres.Onboarding.Tests/  # xUnit (Hari 15)
```

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

## Peranan (Roles) — KONGSI

| Role | Tanggungjawab |
|------|---------------|
| `Applicant` | Cipta draf & hantar permohonan |
| `Supervisor` | Semak permohonan staf (jika perlu) |
| `HrAdmin` | Semak Lapor Diri |
| `SecurityAdmin` | Semak pas, parkir, pelekat kenderaan |
| `IctAdmin` | Semak AD/email, perisian, aset ICT |
| `ComplianceAdmin` | Semak pengisytiharan PKS |
| `SystemAdmin` | Urus pengguna & data lookup |

## Entiti kongsi (shared)

`Submission` (induk), `Attachment`, `AuditLog`, `ApprovalStep`, `UserProfile`, dan lookup: `LookupDepartments`, `LookupGrades`, `LookupPositions`. Definisi rujukan ada dalam `coach-nres/nres-dotnet-15-day-coaching-guide.md` (bahagian "Shared Entity Examples").

## Servis kongsi (shared services)

`IReferenceNumberService`, `IFileStorageService`, `IAuditLogService`, `IWorkflowService`, `INotificationService` (guna `ConsoleNotificationService` untuk latihan), `ICurrentUserService`.

## Prefix nombor rujukan — MUKTAMAD

| Modul | Prefix | Contoh |
|-------|--------|--------|
| Lapor Diri | `LD` | `LD-2026-0001` |
| Pas Keselamatan | `PAS` | `PAS-2026-0001` |
| Parkir | `PKR` | `PKR-2026-0001` |
| Pelekat Kenderaan | `STK` | `STK-2026-0001` |
| ID AD & Email | `ICT-ID` | `ICT-ID-2026-0001` |
| PKS | `PKS` | `PKS-2026-0001` |
| Perisian | `SW` | `SW-2026-0001` |
| Pinjaman Aset | `AST-L` | `AST-L-2026-0001` |
| Pemulangan Aset | `AST-R` | `AST-R-2026-0001` |

## Jadual entiti per modul

| Modul | Tables |
|-------|--------|
| Lapor Diri | `OfficerReportingApplications` |
| Pas/Parking/Pelekat | `AccessPassApplications`, `ParkingApplications`, `VehicleStickerApplications`, `Vehicles` |
| ID AD & Email | `AccountRequests`, `RequestedSystemAccesses` |
| PKS | `PolicyVersions`, `ComplianceChecklistItems`, `ComplianceDeclarations`, `ComplianceResponses` |
| Aset ICT | `Assets`, `SoftwareCatalogItems`, `SoftwareRequests`, `AssetLoanRequests`, `AssetReturns` |

## Pemetaan 15 Hari → Modul (MUKTAMAD)

| Hari | Modul | Fokus utama |
|------|-------|-------------|
| **1** | Asas | Persediaan projek, seni bina, entiti kongsi (`Submission`/`Attachment`/`AuditLog`), migration pertama |
| **2** | 1 · Lapor Diri | Borang create/edit, view model, validation, simpan draf |
| **3** | 1 · Lapor Diri | Muat naik lampiran, submit + nombor rujukan, semakan HR, approve/reject, audit |
| **4** | 2 · Pas/Parking/Pelekat | Model 3 jenis permohonan + `Vehicle`, migration |
| **5** | 2 · Pas/Parking/Pelekat | Borang + conditional validation + semakan pendua (duplicate) |
| **6** | 2 · Pas/Parking/Pelekat | Aliran kelulusan, filter, print summary |
| **7** | 3 · ID/AD/Email | Discovery workflow, `AccountRequest`, `RequestedSystemAccess`, peranan |
| **8** | 3 · ID/AD/Email | Rantaian kelulusan berbilang langkah + role-based authorization |
| **9** | 3 · ID/AD/Email | Notifikasi, carian/filter, panel audit |
| **10** | 4 · PKS | `PolicyVersion`, checklist item, model declaration, seed |
| **11** | 4 · PKS | Borang checklist dinamik + kunci (lock) declaration selepas submit |
| **12** | 4 · PKS | Semakan admin, filter, CSV export |
| **13** | 5 · Aset ICT | `Asset`, `SoftwareCatalog`, `SoftwareRequest`, `AssetLoan`, `AssetReturn` model + seed |
| **14** | 5 · Aset ICT | Borang + semakan availability + kelulusan + transaksi inventori |
| **15** | Integrasi | Integrasi, ujian xUnit, deployment, demo akhir |

## Rentak harian (setiap hari)

Pendaftaran & minum pagi **8.30–9.00** · SESI PAGI **9.00–1.00** · rehat & makan **1.00–2.30** · SESI PETANG **2.30–5.00** · bersurai **5.00**. ~7 jam kontak/hari. Setiap hari: **≥60% masa hands-on lab**.

## Format fail setiap folder `hari-N/`

Setiap `hari-N/` MESTI mengandungi 3 fail (ikut gaya jiran `kelas-flutter-5-hari` & `kelas-n8n-3-hari-jpj`):

1. **`README.md`** — nota konsep Bahasa Melayu: fokus hari, jadual waktu, penerangan **kenapa** setiap konsep wujud, rujukan rasmi Microsoft Docs. Terangkan konsep dahulu; hands-on penuh ada di `snippets/lab.md`.
2. **`snippets/lab.md`** — lab hands-on langkah demi langkah bernombor (Latihan 0, 1, 2, …), setiap satu dengan **Objektif**, langkah, blok kod penuh untuk ditaip, dan **✅ Semakan** di hujung. Ini bahagian paling penting kursus.
3. **`nota-penceramah.md`** — nota penceramah: pemasaan setiap sesi, poin bercakap, silap biasa peserta (common mistakes), soalan untuk cetus perbincangan, deliverable akhir hari.

Setiap README mula dengan pautan ke `../JADUAL.md` dan nyatakan hari ini **mengikut aturcara rasmi**, dan pautan ke `./snippets/lab.md`.
