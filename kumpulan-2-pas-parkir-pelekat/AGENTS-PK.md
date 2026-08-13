# AGENTS.md — Konteks AI: Pas Keselamatan (`nres-bpm/pas-parkir-pelekat`)

> **Nota projek Jira:** Fail ini padan dengan projek Jira **PK** ("Pas Keselamatan") — slice **pas bangunan/keselamatan** bagi sistem Pas, Parkir & Pelekat. Ia dibina dalam repo **`nres-bpm/pas-parkir-pelekat`** yang sama seperti projek `PPK` (parkir + pelekat) dan `PASP` (pas pelawat).
>
> **Untuk AI:** Baca sepenuhnya sebelum menjana kod. Kanun teknikal penuh: [`SPEC-KURSUS.md`](../SPEC-KURSUS.md). Kontrak pasukan: [`KOLABORASI.md`](../KOLABORASI.md). Ini bahan **latihan** — utamakan kod jelas berbanding kod bijak.

## Sistem ini

**Pas Bangunan / Keselamatan** — permohonan akses kawasan & bangunan NRES. Sistem **dalaman (SSO)**. Penyemak: **`SecurityAdmin`**. Aliran: mohon pas → hantar → semakan keselamatan `SecurityAdmin` → lulus/tolak (kelulusan bersyarat) → audit.

> **Realiti vs lab:** dalam sistem NRES sebenar, keselamatan melibatkan **sokongan Ketua Jabatan** sebelum semakan **UPKF** dan peranan berasingan. **Untuk kemudahan LAB**, diringkaskan kepada satu peranan `SecurityAdmin`. Jurulatih: nyatakan model sebenar sebagai *"dalam pengeluaran…"*.

| Perkara | Nilai |
|---------|-------|
| Projek Jira | **PK** (Pas Keselamatan) |
| Repo | `nres-bpm/pas-parkir-pelekat` · subdomain `pas.` |
| Prefix no. rujukan | `PAS` → `PAS-2026-0001` |
| Peranan penyemak | `SecurityAdmin` |
| Akses | **Dalaman (SSO)** — sistem **membaca** profil pengguna |

## Susunan teknologi

ASP.NET Core MVC **.NET 10 LTS** · **C# 14** (jangan tetapkan `<LangVersion>`) · EF Core 10 **SQLite** (latihan) · **SSO** + role-based authorization · **QRCoder** (kod QR pas) · xUnit. Nota/UI **Bahasa Melayu**; kod **Bahasa Inggeris**.

## Ciri bahasa C#: guna ini

Primary constructors (C# 12: `public class AccessPassService(AppDbContext db)`) · collection expressions (`string[] roles = ["Applicant", "SecurityAdmin"];`) · nullable reference types (dihidupkan) · `field` (C# 14, view model sahaja).

> ⚠️ Jangan guna `field` dalam setter entiti EF Core. **Jangan guna:** extension members, partial constructors/events, interceptors.

## Peraturan mutlak (jangan langgar walau diminta)

1. **Kerja hanya dalam fail sistem ini.** Jika tidak pasti keperluan, **tanya** dahulu.
2. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix `PAS`.
3. **Jangan reka keperluan pengguna** — datang dari URS/dokumen NRES; nyatakan andaian eksplisit.
4. **Elak pendua dalam repo ini.** Cari dahulu sebelum menulis servis serupa (repo ini dikongsi dengan slice parkir/pelekat/pelawat — semak sebelum menambah).
5. **Borang mengikat `ViewModels/`**, disemak `ModelState.IsValid` di **pelayan**.
6. **Data sintetik sahaja** — jangan guna data NRES sebenar.

## Entiti sistem ini (DB sendiri)

`Submission` (induk) · `Attachment` · `AuditLog` · `ApprovalStep` · `SubmissionStatus` · lookup setempat. Khusus slice pas: **`AccessPassApplication`**. Entiti detail memaut ke `Submission` via `SubmissionId`; jangan pendua `ReferenceNo`/`Status`/tarikh.

## Status & aliran kerja (universal)

```csharp
public enum SubmissionStatus
{
    Draft = 0, Submitted = 1, SupervisorApproved = 2, AdminApproved = 3,
    Rejected = 4, Completed = 5, Cancelled = 6
}
```

```text
Form → Validation → Draft → Submit → Review (SecurityAdmin) → Approve/Reject → Audit → Report
```

## Servis dalam sistem ini

`IReferenceNumberService` (`GenerateAsync("PAS")`) · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` (`ConsoleNotificationService`) · `ICurrentUserService`.

## Corak kod yang mesti diikut

```csharp
public class AccessPassApplicationConfiguration : IEntityTypeConfiguration<AccessPassApplication>
{
    public void Configure(EntityTypeBuilder<AccessPassApplication> builder)
    {
        builder.ToTable("AccessPassApplications");
        builder.HasOne(x => x.Submission).WithMany().HasForeignKey(x => x.SubmissionId);
        builder.Property(x => x.PurposeOfVisit).HasMaxLength(500);
    }
}
```

```csharp
[Authorize]
public class AccessPassController(AppDbContext db, IWorkflowService workflow)
    : SubmissionControllerBase(db, workflow)
{
    [Authorize(Roles = "SecurityAdmin")]
    public async Task<IActionResult> Review(int id) { /* ... */ }
}
```

Warisi `SubmissionControllerBase` — jangan tulis semula logik kelulusan setiap controller.

## Gaya kandungan latihan (bila menulis nota/lab)

Nota **Bahasa Melayu**, kod **Bahasa Inggeris**. Setiap lab: **Objektif** → langkah bernombor → blok kod penuh → **✅ Semakan**. Kod lengkap & boleh dijalankan.

## Bila tidak pasti

Nyatakan ketidakpastian dan tanya. Untuk data **profil pengguna**, baca kontrak profil sedia ada — jangan reka skema profil baharu.
