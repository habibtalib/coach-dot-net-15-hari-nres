# Kumpulan 1 — Modul Lapor Diri

> Trek Fasa 2 (Hari 4–14). Aturcara: [`../JADUAL.md`](../JADUAL.md) · Kanun: [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) · Kontrak pasukan: [`../KOLABORASI.md`](../KOLABORASI.md) · Konteks AI: [`../AGENTS.md`](../AGENTS.md)

## Modul anda dalam satu ayat

Membolehkan pekerja baharu menghantar maklumat lapor diri dan dokumen sokongan, membolehkan HR menyemak, meluluskan atau menolaknya, dan mengeluarkan Slip Akuan Lapor Diri.

## Identiti modul

| Perkara | Nilai |
|---------|-------|
| **Cabang Git** | `kump-1/lapor-diri` |
| **Prefix rujukan** | `LD` → `LD-2026-0001` |
| **`ModuleCode`** | `ModuleCodes.LaporDiri` |
| **Peranan admin** | `HrAdmin` |
| **Jadual anda** | `OfficerReportingApplications` |
| **Aliran kelulusan** | Satu peringkat: `Draft → Submitted → AdminApproved / Rejected` |

## Folder yang anda miliki

```text
Models/LaporDiri/                 termasuk Configurations/
Controllers/OfficerReporting*
Views/OfficerReporting/
ViewModels/LaporDiri/
Services/LaporDiri/
wwwroot/css/modul-lapor-diri.css
```

**Anda tidak menyunting:** `Program.cs` · `Data/ApplicationDbContext.cs` · `Views/Shared/_Layout.cshtml` · `wwwroot/css/site.css` · `Models/Shared/`

## Blok trek

| Blok | Fokus | Deliverable |
|------|-------|-------------|
| [**Hari 4**](./hari-4/) | Skema DB & borang draf | Entiti + konfigurasi + migration; borang cipta/edit; simpan draf |
| [**Hari 5–6**](./hari-5-6/) | Muat naik & nombor rujukan | Lampiran dokumen sokongan; jana `LD-2026-####`; hantar permohonan |
| [**Hari 7–9**](./hari-7-9/) | Kelulusan HR & skrin admin | Dashboard HR; approve/reject dengan ulasan; penapisan; audit |
| [**Hari 10–12**](./hari-10-12/) | Notifikasi, PDF & dashboard | Notifikasi e-mel; Slip Akuan PDF; papan pemuka analitis HR |
| [**Hari 13–14**](./hari-13-14/) | Ujian & sedia gabung | xUnit; optimasi query EF Core; refactor; sedia merge |

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

**Hujung setiap blok:** gabungan latihan `kump-1/lapor-diri` → `master` melalui PR.

## Sebelum menulis apa-apa helper

1. `grep -ri "<konsep>" Nres.Onboarding.Web/`
2. Semak daftar komponen kongsi dalam [`../AGENTS.md`](../AGENTS.md)
3. Tanya AI: *"Merujuk AGENTS.md, adakah repo ini sudah ada cara untuk `<X>`?"*
4. Jika lebih daripada satu modul perlukannya → buka isu berlabel `shared`, jangan bina sendiri
