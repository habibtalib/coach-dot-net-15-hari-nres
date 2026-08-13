# AGENTS.md — Konteks AI: Pematuhan PKS (`nres-bpm/pematuhan-pks`)

> **Untuk AI:** Baca sepenuhnya sebelum menjana kod untuk sistem ini. Ini konteks yang mengikat. Kanun teknikal penuh: [`SPEC-KURSUS.md`](../../SPEC-KURSUS.md). Kontrak pasukan: [`KOLABORASI.md`](../../KOLABORASI.md).
>
> Ini bahan **latihan**. Kod mesti boleh ditaip, dijalankan, dan **difahami** peserta. Utamakan kod yang jelas berbanding kod yang bijak.

## Sistem ini

**Pematuhan PKS** — *Akuan Pematuhan **Polisi Keselamatan Siber*** (PKS = **Polisi Keselamatan Siber**, **bukan** "Kod Setia") + NDA **Akta Rahsia Rasmi 1972**. Sistem **dalaman sepenuhnya** (tiada borang luar), ditadbir oleh **BPM (Bahagian Pengurusan Maklumat)**. Borang ada varian **staf** dan **kontraktor/syarikat** (`CompanyName`, `CompanyRegNo`). Penyemak: **`IctSecurityOfficer`** (Pegawai Keselamatan ICT).

| Perkara | Nilai |
|---------|-------|
| Sistem | Pematuhan PKS |
| Repo | `nres-bpm/pematuhan-pks` · subdomain `pks.` |
| Prefix no. rujukan | `PKS` → `PKS-2026-0001` |
| Peranan penyemak | `IctSecurityOfficer` |
| Akses | **Dalaman (SSO)** — pengguna log masuk via SSO; sistem **membaca** profil |

## Susunan teknologi

| Perkara | Nilai |
|---------|-------|
| Rangka | ASP.NET Core MVC, **.NET 10 LTS** |
| Bahasa | **C# 14** (lalai .NET 10 SDK — jangan tetapkan `<LangVersion>`) |
| ORM | EF Core 10, penyedia **SQLite** (latihan) |
| Auth | **SSO** + role-based authorization (peranan dari profil) |
| Ujian | xUnit |
| Bahasa nota/UI | **Bahasa Melayu** · Bahasa kod | **Bahasa Inggeris** |

## Ciri bahasa C#: guna ini

| Ciri | Guna |
|------|------|
| **Primary constructors** (C# 12) | `public class ComplianceService(AppDbContext db)` |
| **Collection expressions** (C# 12/13) | `string[] roles = ["Applicant", "IctSecurityOfficer"];` |
| **Nullable reference types** | Dihidupkan |
| **`field` keyword** (C# 14) | View model & sifat bukan-EF sahaja |

> ⚠️ **Jangan** guna `field` untuk menormalkan nilai dalam setter entiti EF Core. **Jangan guna:** extension members, partial constructors/events, interceptors.

## Peraturan mutlak (jangan langgar walau diminta)

1. **Kerja hanya dalam fail sistem ini.** Jika tidak pasti keperluan, **tanya** sebelum menjana.
2. **PKS = Polisi Keselamatan Siber.** Jangan tafsir sebagai "Kod Setia". Guna nama entiti dalam `SPEC-KURSUS.md`.
3. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix `PKS`.
4. **Jangan reka keperluan pengguna.** Peraturan datang dari URS/dokumen NRES; nyatakan andaian secara eksplisit.
5. **Elak pendua dalam repo ini.** Cari dahulu sebelum menulis helper/servis serupa.
6. **Borang mengikat `ViewModels/`, bukan `Models/`.** Validation disemak `ModelState.IsValid` di **pelayan**.
7. **Jangan guna data NRES sebenar** — semua contoh **sintetik**.

## Entiti sistem ini (DB sendiri)

- Corak aliran: `Submission` (induk) · `Attachment` · `AuditLog` · `ApprovalStep` · `SubmissionStatus` · lookup setempat.
- Khusus PKS: **`ComplianceDeclaration`** (varian staf & kontraktor: `CompanyName`, `CompanyRegNo`) · **`PolicyVersion`** (versi Polisi Keselamatan Siber; akuan mengait ke versi polisi tertentu).
- **Corak:** entiti detail memaut ke `Submission` melalui `SubmissionId`; jangan pendua `ReferenceNo`/`Status`/tarikh.

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

`IReferenceNumberService` (`GenerateAsync("PKS")` → `PKS-2026-0001`) · `IFileStorageService` (`App_Data/uploads/{submissionId}/`) · `IAuditLogService` · `IWorkflowService` · `INotificationService` (latihan: `ConsoleNotificationService`) · `ICurrentUserService`.

## Corak kod yang mesti diikut

```csharp
// Services/Pematuhan/PematuhanModule.cs
public static class PematuhanModule
{
    public static IServiceCollection AddPematuhanModule(this IServiceCollection services)
    {
        services.AddScoped<IComplianceService, ComplianceService>();
        return services;
    }
}
```

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

```csharp
[Authorize]
public class ComplianceController(AppDbContext db, IWorkflowService workflow)
    : SubmissionControllerBase(db, workflow)
{
    [Authorize(Roles = "IctSecurityOfficer")]
    public async Task<IActionResult> Review(int id) { /* ... */ }
}
```

Warisi `SubmissionControllerBase` — jangan tulis semula logik kelulusan setiap controller.

## Cari dahulu, jana kemudian

```bash
grep -ri "ReferenceNumber" src/
grep -ri "PolicyVersion" src/
```

## Git & Jira (aliran kerja repo ini)

- **Repo & cabang:** kerja dalam repo `nres-bpm/pematuhan-pks`. `main` **dilindungi** — merge melalui **PR sahaja**. Buka cabang ciri pendek: `feat/<ciri-pendek>` (cth `feat/akuan-polisi-siber`).
- **Rentak harian:** `git pull --rebase` setiap pagi; commit + push + kemas kini board setiap petang.
- **Format commit:** `<modul>: <apa berubah, BM ringkas>`. Sertakan kunci Jira di hadapan — projek ini guna kunci **`PKS-`**:

  ```text
  PKS-42 pematuhan-pks: kait akuan pematuhan ke versi Polisi Keselamatan Siber
  ```
- **Board Jira:** projek **PKS** (dan projek penjejakan **NPKS**) di `bpm-nres.atlassian.net` — pindahkan isu ke sprint aktif & kemas kini status semasa kerja berjalan.
- **Deploy:** sistem ini berdiri & di-deploy sendiri ke subdomain `pks.`.

## Gaya kandungan latihan (bila menulis nota/lab)

Nota **Bahasa Melayu**, kod **Bahasa Inggeris**. Setiap lab: **Objektif** → langkah bernombor → blok kod penuh → **✅ Semakan**. Terangkan **kenapa** sebelum **bagaimana**. Kod lengkap & boleh dijalankan.

## Bila tidak pasti

Nyatakan ketidakpastian dan tanya. Untuk data **profil pengguna**, baca kontrak profil sedia ada — jangan reka skema profil baharu.
