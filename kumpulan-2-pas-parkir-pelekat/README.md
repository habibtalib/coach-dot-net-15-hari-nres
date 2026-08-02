# Kumpulan 2 — Modul Pas, Parkir & Pelekat Kenderaan

> Trek Fasa 2 (Hari 4–14). Aturcara: [`../JADUAL.md`](../JADUAL.md) · Kanun: [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) · Kontrak pasukan: [`../KOLABORASI.md`](../KOLABORASI.md) · Konteks AI: [`../AGENTS.md`](../AGENTS.md)

## Modul anda dalam satu ayat

Menguruskan akses kawasan dan keselamatan kenderaan — permohonan pas pelawat/staf, pelekat kenderaan dan lot parkir — dengan semakan pendua nombor plat, kelulusan Pegawai Keselamatan, dan penjanaan QR untuk semakan rondaan.

## Identiti modul

| Perkara | Nilai |
|---------|-------|
| **Cabang Git** | `kump-2/akses-kenderaan` |
| **Prefix rujukan** | `PAS` · `PKR` · `STK` |
| **`ModuleCode`** | `ModuleCodes.PasKeselamatan` · `Parkir` · `PelekatKenderaan` |
| **Peranan admin** | `SecurityAdmin` |
| **Jadual anda** | `AccessPassApplications`, `ParkingApplications`, `VehicleStickerApplications`, `Vehicles` |
| **Aliran kelulusan** | Satu peringkat + **penolakan bersyarat** |

> **Modul anda paling kompleks dari segi pemodelan** — tiga jenis permohonan berkongsi satu `Submission` induk, ditambah entiti `Vehicle` berasingan yang boleh dikongsi banyak permohonan. Beri masa pada Hari 4.

## Folder yang anda miliki

```text
Models/Akses/                     termasuk Configurations/
Controllers/AccessPass*  Controllers/Parking*  Controllers/VehicleSticker*
Views/Akses/
ViewModels/Akses/
Services/Akses/
wwwroot/css/modul-akses.css
```

**Anda tidak menyunting:** `Program.cs` · `Data/ApplicationDbContext.cs` · `Views/Shared/_Layout.cshtml` · `wwwroot/css/site.css` · `Models/Shared/`

## Blok trek

| Blok | Fokus | Deliverable |
|------|-------|-------------|
| [**Hari 4**](./hari-4/) | Skema DB akses & kenderaan | `Vehicle` + 3 jadual permohonan + konfigurasi + migration; halaman utama modul |
| [**Hari 5–6**](./hari-5-6/) | Borang & semakan pendua plat | 3 borang; conditional validation; **sekat permohonan pendua nombor plat** |
| [**Hari 7–9**](./hari-7-9/) | Semakan keselamatan & kelulusan | Skrin Pegawai Keselamatan; kelulusan bersyarat; peruntukan lot/pelekat |
| [**Hari 10–12**](./hari-10-12/) | QR/Barcode & rondaan | Jana QR pas & pelekat; skrin semakan pantas rondaan; laporan bercetak |
| [**Hari 13–14**](./hari-13-14/) | Ujian E2E & sedia gabung | Ujian hujung-ke-hujung; bug fixing; pemantapan validasi; sedia merge |

## Servis kongsi yang anda GUNA (jangan tulis semula)

`IReferenceNumberService` · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` · `ICurrentUserService` · `SubmissionControllerBase` · `_StatusBadge` · `_AuditTrail` · `_AttachmentList` · `_ApprovalPanel` · `_FilterBar` · `_ValidationSummary`

Daftar penuh: [`../AGENTS.md`](../AGENTS.md).

## Rentak harian

| Masa | Aktiviti |
|------|----------|
| 9.00 – 9.15 | Stand-up + `git pull --rebase origin master` |
| 9.15 – 9.25 | Semakan silang AI (pertindihan dengan kumpulan lain) |
| 9.25 – 1.00 · 2.30 – 4.30 | Pembangunan |
| 4.30 – 5.00 | Code review berpasangan + PR + push + kemas kini board |

**Hujung setiap blok:** gabungan latihan `kump-2/akses-kenderaan` → `master` melalui PR.

## Sebelum menulis apa-apa helper

1. `grep -ri "<konsep>" Nres.Onboarding.Web/`
2. Semak daftar komponen kongsi dalam [`../AGENTS.md`](../AGENTS.md)
3. Tanya AI: *"Merujuk AGENTS.md, adakah repo ini sudah ada cara untuk `<X>`?"*
4. Jika lebih daripada satu modul perlukannya → buka isu berlabel `shared`, jangan bina sendiri
