# Kumpulan 3 — Modul ID, AD & Email

> Trek Fasa 2 (Hari 4–14). Aturcara: [`../JADUAL.md`](../JADUAL.md) · Kanun: [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) · Kontrak pasukan: [`../KOLABORASI.md`](../KOLABORASI.md) · Konteks AI: [`../AGENTS.md`](../AGENTS.md)

## Modul anda dalam satu ayat

Menguruskan permohonan akaun pengguna dan akses sistem — akaun Active Directory, e-mel rasmi, pertukaran akses dan nyahaktif — melalui kelulusan dua peringkat (Penyelia, kemudian ICT) dengan jejak audit penuh.

## Identiti modul

| Perkara | Nilai |
|---------|-------|
| **Cabang Git** | `kump-3/id-ad-email` |
| **Prefix rujukan** | `ICT-ID` → `ICT-ID-2026-0001` |
| **`ModuleCode`** | `ModuleCodes.IdAdEmail` |
| **Peranan** | `Supervisor` (peringkat 1) → `IctAdmin` (peringkat 2) |
| **Jadual anda** | `AccountRequests`, `RequestedSystemAccesses` |
| **Aliran kelulusan** | **Dua peringkat**: `Draft → Submitted → SupervisorApproved → Completed / Rejected` |

> **Modul anda mempunyai aliran paling kompleks** — satu-satunya dengan kelulusan dua peringkat. Anda akan menggunakan `ApprovalStep` (dengan `StepOrder`) secara serius, di mana kumpulan lain hanya menggunakan satu langkah.
>
> 🔒 **Modul anda juga paling sensitif dari segi keselamatan.** Anda menguruskan permohonan akaun — jangan sekali-kali menyimpan kata laluan dalam mana-mana entiti anda. Ini titik pengajaran, dan ia akan diperiksa.

## Folder yang anda miliki

```text
Models/Akaun/                     termasuk Configurations/
Controllers/AccountRequest*
Views/Akaun/
ViewModels/Akaun/
Services/Akaun/
wwwroot/css/modul-akaun.css
```

**Anda tidak menyunting:** `Program.cs` · `Data/ApplicationDbContext.cs` · `Views/Shared/_Layout.cshtml` · `wwwroot/css/site.css` · `Models/Shared/`

## Blok trek

| Blok | Fokus | Deliverable |
|------|-------|-------------|
| [**Hari 4**](./hari-4/) | Skema DB akaun & akses | `AccountRequest` + `RequestedSystemAccess`; jenis permohonan; laluan kelulusan 2 peringkat; migration |
| [**Hari 5–6**](./hari-5-6/) | Borang & kelulusan Penyelia | Borang AD/e-mel/akses; skrin kelulusan peringkat 1; pengesahan identiti & role mapping |
| [**Hari 7–9**](./hari-7-9/) | Pemprosesan ICT & RBAC | Skrin Pentadbir ICT; **kuatkuasa RBAC merentas modul**; simulasi integrasi AD |
| [**Hari 10–12**](./hari-10-12/) | Penjejakan, audit & dashboard | Penjejakan status; **audit trail penuh**; carian/penapis; papan pemuka ICT |
| [**Hari 13–14**](./hari-13-14/) | RBAC testing & security audit | Ujian kebenaran capaian setiap peranan; semakan keselamatan log audit; sedia merge |

## Servis kongsi yang anda GUNA (jangan tulis semula)

`IReferenceNumberService` · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` · `ICurrentUserService` · `SubmissionControllerBase` · `_StatusBadge` · `_AuditTrail` · `_AttachmentList` · `_ApprovalPanel` · `_FilterBar` · `_ValidationSummary`

> **Nota khusus anda:** `SubmissionControllerBase.Approve` menetapkan `AdminApproved`. Aliran dua peringkat anda memerlukan `SupervisorApproved` dahulu. Ini **bukan** alasan untuk menulis semula kelas asas — ia sebab untuk **menambah** tindakan `SupervisorApprove` dalam controller anda yang memanggil `IWorkflowService.TransitionAsync`. Bincang dengan jurulatih pada Hari 5–6.

Daftar penuh: [`../AGENTS.md`](../AGENTS.md).

## Rentak harian

| Masa | Aktiviti |
|------|----------|
| 9.00 – 9.15 | Stand-up + `git pull --rebase origin master` |
| 9.15 – 9.25 | Semakan silang AI (pertindihan dengan kumpulan lain) |
| 9.25 – 1.00 · 2.30 – 4.30 | Pembangunan |
| 4.30 – 5.00 | Code review berpasangan + PR + push + kemas kini board |

**Hujung setiap blok:** gabungan latihan `kump-3/id-ad-email` → `master` melalui PR.

## Sebelum menulis apa-apa helper

1. `grep -ri "<konsep>" Nres.Onboarding.Web/`
2. Semak daftar komponen kongsi dalam [`../AGENTS.md`](../AGENTS.md)
3. Tanya AI: *"Merujuk AGENTS.md, adakah repo ini sudah ada cara untuk `<X>`?"*
4. Jika lebih daripada satu modul perlukannya → buka isu berlabel `shared`, jangan bina sendiri
