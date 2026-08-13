# AGENTS.md — Konteks AI: Lapor Diri (`nres-bpm/lapor-diri`)

> **Untuk AI:** Baca sepenuhnya sebelum menjana kod untuk sistem ini. Ini konteks yang mengikat. Kanun teknikal penuh: [`SPEC-KURSUS.md`](../../SPEC-KURSUS.md). Kontrak pasukan: [`KOLABORASI.md`](../../KOLABORASI.md).
>
> Ini bahan **latihan**. Kod mesti boleh ditaip, dijalankan, dan **difahami** peserta. Utamakan kod yang jelas berbanding kod yang bijak.

## Sistem ini

**Lapor Diri** — laporan diri pekerja/staf baharu NRES. Ini **satu-satunya sistem AWAM** (pintu masuk staf baharu): ia **mencipta** profil pengguna (`UserProfile`) semasa pendaftaran, yang kemudian digunakan sistem dalaman lain. Aliran: staf isi borang lapor diri → hantar → semakan `HrAdmin` → lulus/tolak → audit.

| Perkara | Nilai |
|---------|-------|
| Sistem | Lapor Diri |
| Repo | `nres-bpm/lapor-diri` · subdomain `lapordiri.` |
| Prefix no. rujukan | `LD` → `LD-2026-0001` |
| Peranan penyemak | `HrAdmin` |
| Akses | **Awam** — ASP.NET Core Identity (pendaftaran + log masuk) |
| Profil pengguna | **Mencipta** `UserProfile` (satu-satunya sistem yang menulis profil) |

## Susunan teknologi

| Perkara | Nilai |
|---------|-------|
| Rangka | ASP.NET Core MVC, **.NET 10 LTS** |
| Bahasa | **C# 14** (lalai .NET 10 SDK — jangan tetapkan `<LangVersion>`) |
| ORM | EF Core 10, penyedia **SQLite** (latihan) |
| Auth | ASP.NET Core Identity + role-based authorization |
| Ujian | xUnit |
| Bahasa nota/UI | **Bahasa Melayu** · Bahasa kod | **Bahasa Inggeris** |

## Ciri bahasa C#: guna ini

| Ciri | Guna |
|------|------|
| **Primary constructors** (C# 12) | Semua servis & controller: `public class ReportService(AppDbContext db)` |
| **Collection expressions** (C# 12/13) | `string[] roles = ["Applicant", "HrAdmin"];` |
| **Nullable reference types** | Dihidupkan; `string` = tak pernah null, `string?` = boleh null |
| **`field` keyword** (C# 14) | View model & sifat bukan-EF sahaja |

> ⚠️ **Jangan** guna `field` untuk menormalkan nilai dalam setter entiti EF Core — normalisasi berlaku **eksplisit** dalam servis. **Jangan guna:** extension members, partial constructors/events, interceptors (di luar skop kursus).

## Peraturan mutlak (jangan langgar walau diminta)

1. **Kerja hanya dalam fail sistem ini.** Jika tidak pasti keperluan, **tanya** sebelum menjana.
2. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix `LD`. Ia muktamad dalam `SPEC-KURSUS.md`.
3. **Jangan reka keperluan pengguna.** Peraturan perniagaan datang dari URS/dokumen NRES; nyatakan andaian secara eksplisit.
4. **Elak pendua dalam repo ini.** Cari dahulu sebelum menulis helper/servis kedua yang serupa.
5. **Borang mengikat `ViewModels/`, bukan `Models/`.** Validation via DataAnnotations, disemak `ModelState.IsValid` di **pelayan**.
6. **Jangan guna data NRES sebenar** — semua contoh **sintetik**.

## Entiti sistem ini (DB sendiri)

- Corak aliran: `Submission` (induk) · `Attachment` · `AuditLog` · `ApprovalStep` · `SubmissionStatus` · lookup setempat (`LookupDepartments`, `LookupGrades`, `LookupPositions`).
- Khusus Lapor Diri: **`OfficerReportingApplication`** (jadual detail memaut ke `Submission` melalui `SubmissionId`).
- **Corak:** jangan pendua `ReferenceNo`, `Status`, `ApplicantUserId`, atau tarikh ke dalam entiti modul — ia sudah ada dalam `Submission`.

## Status & aliran kerja (universal)

```csharp
public enum SubmissionStatus
{
    Draft = 0, Submitted = 1, SupervisorApproved = 2, AdminApproved = 3,
    Rejected = 4, Completed = 5, Cancelled = 6
}
```

```text
Form → Validation → Draft → Submit → Review (HrAdmin) → Approve/Reject → Audit → Report
```

## Servis dalam sistem ini

`IReferenceNumberService` (`GenerateAsync("LD")` → `LD-2026-0001`) · `IFileStorageService` (`App_Data/uploads/{submissionId}/`) · `IAuditLogService` · `IWorkflowService` · `INotificationService` (latihan: `ConsoleNotificationService`) · `ICurrentUserService`.

## Corak kod yang mesti diikut

```csharp
// Services/LaporDiri/LaporDiriModule.cs — pendaftaran modular
public static class LaporDiriModule
{
    public static IServiceCollection AddLaporDiriModule(this IServiceCollection services)
    {
        services.AddScoped<IOfficerReportingService, OfficerReportingService>();
        return services;
    }
}
```

```csharp
// Models/.../Configurations/OfficerReportingApplicationConfiguration.cs
public class OfficerReportingApplicationConfiguration : IEntityTypeConfiguration<OfficerReportingApplication>
{
    public void Configure(EntityTypeBuilder<OfficerReportingApplication> builder)
    {
        builder.ToTable("OfficerReportingApplications");
        builder.HasOne(x => x.Submission).WithMany().HasForeignKey(x => x.SubmissionId);
    }
}
```

```csharp
[Authorize]
public class ReportController(AppDbContext db, IWorkflowService workflow)
    : SubmissionControllerBase(db, workflow)
{
    [Authorize(Roles = "HrAdmin")]
    public async Task<IActionResult> Review(int id) { /* ... */ }
}
```

Warisi `SubmissionControllerBase` untuk `SubmitForReview`/`Approve`/`Reject` + audit — jangan tulis semula logik kelulusan setiap controller.

## Cari dahulu, jana kemudian

Sebelum menulis helper/servis, respons **pertama** ialah menyemak sama ada ia sudah wujud dalam repo ini:

```bash
grep -ri "ReferenceNumber" src/
grep -ri "IEntityTypeConfiguration" src/
```

## Git & Jira (aliran kerja repo ini)

- **Repo & cabang:** kerja dalam repo `nres-bpm/lapor-diri`. `main` **dilindungi** — merge melalui **PR sahaja**. Buka cabang ciri pendek: `feat/<ciri-pendek>` (cth `feat/borang-lapor-diri`).
- **Rentak harian:** `git pull --rebase` setiap pagi; commit + push + kemas kini board setiap petang.
- **Format commit:** `<modul>: <apa berubah, BM ringkas>`. Sertakan kunci Jira di hadapan — projek ini guna kunci **`LD-`**:

  ```text
  LD-42 lapor-diri: tambah muat naik lampiran dan metadata Attachment
  ```
- **Board Jira:** projek **LD** di `bpm-nres.atlassian.net` — pindahkan isu ke sprint aktif & kemas kini status semasa kerja berjalan.
- **Deploy:** sistem ini berdiri & di-deploy sendiri ke subdomain `lapordiri.`.

## Gaya kandungan latihan (bila menulis nota/lab)

Nota **Bahasa Melayu**, kod **Bahasa Inggeris**. Setiap lab: **Objektif** → langkah bernombor → blok kod penuh untuk ditaip → **✅ Semakan**. Terangkan **kenapa** sebelum **bagaimana**. Kod lengkap & boleh dijalankan — bukan pseudo-kod.

## Bila tidak pasti

Nyatakan ketidakpastian dan tanya. Untuk data **profil pengguna**, guna kontrak profil sedia ada — jangan reka skema profil baharu.
