# AGENTS.md — Konteks AI: Pas Pelawat (`nres-bpm/pas-parkir-pelekat`)

> **Nota projek Jira:** Fail ini padan dengan projek Jira **PASP** ("Pas Pelawat") — slice **pas pelawat** bagi sistem Pas, Parkir & Pelekat. Ia dibina dalam repo **`nres-bpm/pas-parkir-pelekat`** yang sama seperti projek `PK` (pas keselamatan) dan `PPK` (parkir + pelekat).
>
> **Untuk AI:** Baca sepenuhnya sebelum menjana kod. Kanun teknikal penuh: [`SPEC-KURSUS.md`](../SPEC-KURSUS.md). Kontrak pasukan: [`KOLABORASI.md`](../KOLABORASI.md). Ini bahan **latihan** — utamakan kod jelas berbanding kod bijak.

## Sistem ini

**Pas Pelawat** — permohonan pas masuk sementara untuk pelawat/tetamu luar ke kawasan NRES. Sistem **dalaman (SSO)** — staf tuan rumah memohon bagi pihak pelawat. Penyemak: **`SecurityAdmin`**. Aliran: staf mohon pas pelawat → hantar → semakan keselamatan `SecurityAdmin` → lulus (jana pas/kod QR) → audit.

> **Realiti vs lab:** dalam sistem NRES sebenar, kawalan pintu masuk melibatkan Pengawal Keselamatan (imbas-sahaja) berasingan. **Untuk kemudahan LAB**, diringkaskan kepada satu peranan `SecurityAdmin`. Jurulatih: nyatakan model sebenar sebagai *"dalam pengeluaran…"*.

| Perkara | Nilai |
|---------|-------|
| Projek Jira | **PASP** (Pas Pelawat) |
| Repo | `nres-bpm/pas-parkir-pelekat` · subdomain `pas.` |
| Prefix no. rujukan | `PAS` → `PAS-2026-0001` |
| Peranan penyemak | `SecurityAdmin` |
| Akses | **Dalaman (SSO)** — staf tuan rumah log masuk; sistem **membaca** profil |

## Susunan teknologi

ASP.NET Core MVC **.NET 10 LTS** · **C# 14** (jangan tetapkan `<LangVersion>`) · EF Core 10 **SQLite** (latihan) · **SSO** + role-based authorization · **QRCoder** (kod QR pas pelawat) · xUnit. Nota/UI **Bahasa Melayu**; kod **Bahasa Inggeris**.

## Ciri bahasa C#: guna ini

Primary constructors (C# 12: `public class VisitorPassService(AppDbContext db)`) · collection expressions (`string[] roles = ["Applicant", "SecurityAdmin"];`) · nullable reference types (dihidupkan) · `field` (C# 14, view model sahaja).

> ⚠️ Jangan guna `field` dalam setter entiti EF Core. **Jangan guna:** extension members, partial constructors/events, interceptors.

## Peraturan mutlak (jangan langgar walau diminta)

1. **Kerja hanya dalam fail sistem ini.** Jika tidak pasti keperluan, **tanya** dahulu.
2. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix `PAS`.
3. **Jangan reka keperluan pengguna** — datang dari URS/dokumen NRES; nyatakan andaian eksplisit.
4. **Elak pendua dalam repo ini.** Cari dahulu sebelum menulis servis serupa (repo ini dikongsi dengan slice pas keselamatan/parkir/pelekat — `AccessPassApplication` mungkin sudah wujud).
5. **Borang mengikat `ViewModels/`**, disemak `ModelState.IsValid` di **pelayan**.
6. **Data sintetik sahaja** — jangan guna data NRES sebenar (nama & no. kad pengenalan pelawat mesti sintetik).

## Entiti sistem ini (DB sendiri)

`Submission` (induk) · `Attachment` · `AuditLog` · `ApprovalStep` · `SubmissionStatus` · lookup setempat. Khusus slice pas pelawat: **`AccessPassApplication`** (varian pelawat — butiran pelawat, tempoh lawatan, tuan rumah). Entiti detail memaut ke `Submission` via `SubmissionId`; jangan pendua `ReferenceNo`/`Status`/tarikh.

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
        builder.Property(x => x.VisitorName).HasMaxLength(200);
    }
}
```

```csharp
[Authorize]
public class VisitorPassController(AppDbContext db, IWorkflowService workflow)
    : SubmissionControllerBase(db, workflow)
{
    [Authorize(Roles = "SecurityAdmin")]
    public async Task<IActionResult> Review(int id) { /* ... */ }
}
```

Warisi `SubmissionControllerBase` — jangan tulis semula logik kelulusan setiap controller.

## Git & Jira (aliran kerja repo ini)

- **Repo & cabang:** kerja dalam repo `nres-bpm/pas-parkir-pelekat` (dikongsi dengan projek PK & PPK). `main` **dilindungi** — merge melalui **PR sahaja**. Cabang ciri pendek: `feat/<ciri-pendek>` (cth `feat/pas-pelawat-qr`).
- **Rentak harian:** `git pull --rebase` setiap pagi; commit + push + kemas kini board setiap petang.
- **Format commit:** `<modul>: <apa berubah, BM ringkas>`. Sertakan kunci Jira di hadapan — slice ini guna kunci **`PASP-`**:

  ```text
  PASP-42 pas-parkir-pelekat: tambah pas pelawat dan penjanaan kod QR
  ```
- **Board Jira:** projek **PASP** ("Pas Pelawat") di `bpm-nres.atlassian.net` — pindahkan isu ke sprint aktif & kemas kini status. Selaraskan dengan projek **PK**/**PPK** kerana ketiga-tiganya menjejak repo yang sama.
- **Deploy:** sistem ini berdiri & di-deploy sendiri ke subdomain `pas.`.

## Gaya kandungan latihan (bila menulis nota/lab)

Nota **Bahasa Melayu**, kod **Bahasa Inggeris**. Setiap lab: **Objektif** → langkah bernombor → blok kod penuh → **✅ Semakan**. Kod lengkap & boleh dijalankan.

## Bila tidak pasti

Nyatakan ketidakpastian dan tanya. Untuk data **profil pengguna**, baca kontrak profil sedia ada — jangan reka skema profil baharu.
