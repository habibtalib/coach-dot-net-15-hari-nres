# AGENTS.md — Konteks AI: NRES-PKS / Pematuhan PKS (`nres-bpm/pematuhan-pks`)

> **Nota projek Jira:** Fail ini padan dengan projek Jira **NPKS** ("NRES-PKS"). NPKS ialah projek penjejakan bagi **sistem yang sama** seperti projek `PKS` — kedua-duanya dibina dalam repo **`nres-bpm/pematuhan-pks`**. Konteks teknikal adalah **sama** dengan [`AGENTS.md`](./AGENTS.md) sistem Pematuhan PKS. Gunakan fail ini apabila kerja dijejak di bawah kunci projek NPKS.
>
> **Untuk AI:** Baca sepenuhnya sebelum menjana kod. Kanun teknikal penuh: [`SPEC-KURSUS.md`](../../SPEC-KURSUS.md). Kontrak pasukan: [`KOLABORASI.md`](../../KOLABORASI.md). Ini bahan **latihan** — utamakan kod jelas berbanding kod bijak.

## Sistem ini

**Pematuhan PKS** — *Akuan Pematuhan **Polisi Keselamatan Siber*** (PKS = **Polisi Keselamatan Siber**, **bukan** "Kod Setia") + NDA **Akta Rahsia Rasmi 1972**. Sistem **dalaman sepenuhnya**, ditadbir **BPM (Bahagian Pengurusan Maklumat)**. Borang ada varian **staf** dan **kontraktor/syarikat** (`CompanyName`, `CompanyRegNo`). Penyemak: **`IctSecurityOfficer`**.

| Perkara | Nilai |
|---------|-------|
| Projek Jira | **NPKS** (NRES-PKS) — sama sistem dengan projek `PKS` |
| Repo | `nres-bpm/pematuhan-pks` · subdomain `pks.` |
| Prefix no. rujukan | `PKS` → `PKS-2026-0001` |
| Peranan penyemak | `IctSecurityOfficer` |
| Akses | **Dalaman (SSO)** — sistem **membaca** profil pengguna |

## Susunan teknologi

ASP.NET Core MVC **.NET 10 LTS** · **C# 14** (jangan tetapkan `<LangVersion>`) · EF Core 10 **SQLite** (latihan) · **SSO** + role-based authorization · xUnit. Nota/UI **Bahasa Melayu**; kod **Bahasa Inggeris**.

## Ciri bahasa C#: guna ini

Primary constructors (C# 12) · collection expressions (`string[] roles = ["Applicant", "IctSecurityOfficer"];`) · nullable reference types (dihidupkan) · `field` (C# 14, view model sahaja).

> ⚠️ Jangan guna `field` dalam setter entiti EF Core. **Jangan guna:** extension members, partial constructors/events, interceptors.

## Peraturan mutlak (jangan langgar walau diminta)

1. **Kerja hanya dalam fail sistem ini.** Jika tidak pasti keperluan, **tanya** dahulu.
2. **PKS = Polisi Keselamatan Siber** — bukan "Kod Setia". Guna nama entiti dalam `SPEC-KURSUS.md`.
3. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix `PKS`.
4. **Jangan reka keperluan pengguna** — datang dari URS/dokumen NRES; nyatakan andaian eksplisit.
5. **Elak pendua dalam repo ini.** Cari dahulu sebelum menulis servis serupa.
6. **Borang mengikat `ViewModels/`**, disemak `ModelState.IsValid` di **pelayan**.
7. **Data sintetik sahaja** — jangan guna data NRES sebenar.

## Entiti sistem ini (DB sendiri)

`Submission` (induk) · `Attachment` · `AuditLog` · `ApprovalStep` · `SubmissionStatus` · lookup setempat. Khusus PKS: **`ComplianceDeclaration`** (varian staf & kontraktor) · **`PolicyVersion`**. Entiti detail memaut ke `Submission` via `SubmissionId`; jangan pendua `ReferenceNo`/`Status`/tarikh.

## Status & aliran kerja (universal)

```csharp
public enum SubmissionStatus
{
    Draft = 0, Submitted = 1, SupervisorApproved = 2, AdminApproved = 3,
    Rejected = 4, Completed = 5, Cancelled = 6
}
```

```text
Form → Validation → Draft → Submit → Review (IctSecurityOfficer) → Approve/Reject → Audit → Report
```

## Servis dalam sistem ini

`IReferenceNumberService` (`GenerateAsync("PKS")`) · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` (`ConsoleNotificationService`) · `ICurrentUserService`.

## Corak kod yang mesti diikut

```csharp
public class ComplianceDeclarationConfiguration : IEntityTypeConfiguration<ComplianceDeclaration>
{
    public void Configure(EntityTypeBuilder<ComplianceDeclaration> builder)
    {
        builder.ToTable("ComplianceDeclarations");
        builder.HasOne(x => x.Submission).WithMany().HasForeignKey(x => x.SubmissionId);
        builder.Property(x => x.CompanyName).HasMaxLength(200);
    }
}
```

Warisi `SubmissionControllerBase` untuk `SubmitForReview`/`Approve`/`Reject` + audit; kuatkuasa `[Authorize(Roles = "IctSecurityOfficer")]` pada tindakan semakan.

## Git & Jira (aliran kerja repo ini)

- **Repo & cabang:** kerja dalam repo `nres-bpm/pematuhan-pks` (sama seperti projek PKS). `main` **dilindungi** — merge melalui **PR sahaja**. Cabang ciri pendek: `feat/<ciri-pendek>`.
- **Rentak harian:** `git pull --rebase` setiap pagi; commit + push + kemas kini board setiap petang.
- **Format commit:** `<modul>: <apa berubah, BM ringkas>`. Sertakan kunci Jira di hadapan — projek ini guna kunci **`NPKS-`** (atau `PKS-` jika dijejak di sana):

  ```text
  NPKS-13 pematuhan-pks: tambah AGENTS.md dan konfigurasi ComplianceDeclaration
  ```
- **Board Jira:** projek **NPKS** di `bpm-nres.atlassian.net` — selaraskan dengan projek **PKS** kerana kedua-duanya menjejak repo yang sama.
- **Deploy:** sistem ini berdiri & di-deploy sendiri ke subdomain `pks.`.

## Gaya kandungan latihan (bila menulis nota/lab)

Nota **Bahasa Melayu**, kod **Bahasa Inggeris**. Setiap lab: **Objektif** → langkah bernombor → blok kod penuh → **✅ Semakan**. Kod lengkap & boleh dijalankan.

## Bila tidak pasti

Nyatakan ketidakpastian dan tanya. Untuk data **profil pengguna**, baca kontrak profil sedia ada — jangan reka skema profil baharu.
