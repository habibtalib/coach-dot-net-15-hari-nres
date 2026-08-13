# AGENTS.md — Konteks AI: Pengurusan Kontrak (`nres-bpm/pengurusan-kontrak`)

> **Untuk AI:** Baca sepenuhnya sebelum menjana kod untuk sistem ini. Ini konteks yang mengikat. Kanun teknikal penuh: [`SPEC-KURSUS.md`](../../SPEC-KURSUS.md). Kontrak pasukan: [`KOLABORASI.md`](../../KOLABORASI.md).
>
> Ini bahan **latihan**. Kod mesti boleh ditaip, dijalankan, dan **difahami** peserta. Utamakan kod yang jelas berbanding kod yang bijak.

## Sistem ini

**Pengurusan Kontrak** — daftar & jejak kontrak/perjanjian NRES (pihak berkontrak, tarikh, milestone/tempoh). Sistem **dalaman sepenuhnya** (tiada borang luar), ditadbir **BPM**. Aliran: daftar kontrak (jana no. rujukan) → hantar → semakan `IctAdmin` → lulus → jejak milestone → audit.

| Perkara | Nilai |
|---------|-------|
| Sistem | Pengurusan Kontrak |
| Repo | `nres-bpm/pengurusan-kontrak` · subdomain `kontrak.` |
| Prefix no. rujukan | `KON` → `KON-2026-0001` |
| Peranan penyemak | `IctAdmin` |
| Akses | **Dalaman (SSO)** — sistem **membaca** profil pengguna |

## Susunan teknologi

| Perkara | Nilai |
|---------|-------|
| Rangka | ASP.NET Core MVC, **.NET 10 LTS** |
| Bahasa | **C# 14** (lalai .NET 10 SDK — jangan tetapkan `<LangVersion>`) |
| ORM | EF Core 10, penyedia **SQLite** (latihan) |
| Auth | **SSO** + role-based authorization |
| Ujian | xUnit |
| Bahasa nota/UI | **Bahasa Melayu** · Bahasa kod | **Bahasa Inggeris** |

## Ciri bahasa C#: guna ini

| Ciri | Guna |
|------|------|
| **Primary constructors** (C# 12) | `public class ContractService(AppDbContext db)` |
| **Collection expressions** (C# 12/13) | `string[] roles = ["Applicant", "IctAdmin"];` |
| **Nullable reference types** | Dihidupkan |
| **`field` keyword** (C# 14) | View model & sifat bukan-EF sahaja |

> ⚠️ **Jangan** guna `field` dalam setter entiti EF Core. **Jangan guna:** extension members, partial constructors/events, interceptors.

## Peraturan mutlak (jangan langgar walau diminta)

1. **Kerja hanya dalam fail sistem ini.** Jika tidak pasti keperluan, **tanya** sebelum menjana.
2. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix `KON`.
3. **Jangan reka keperluan pengguna.** Peraturan datang dari URS/dokumen NRES; nyatakan andaian eksplisit.
4. **Elak pendua dalam repo ini.** Cari dahulu sebelum menulis helper/servis serupa.
5. **Borang mengikat `ViewModels/`, bukan `Models/`.** Validation disemak `ModelState.IsValid` di **pelayan**.
6. **Jangan guna data NRES sebenar** — semua contoh **sintetik**.

## Entiti sistem ini (DB sendiri)

- Corak aliran: `Submission` (induk) · `Attachment` · `AuditLog` · `ApprovalStep` · `SubmissionStatus` · lookup setempat.
- Khusus Kontrak: **`ContractRecord`** · **`ContractParty`** (pihak berkontrak) · **`ContractMilestone`** (tempoh/pencapaian).
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
Form → Validation → Draft → Submit → Review (IctAdmin) → Approve/Reject → Audit → Report
```

## Servis dalam sistem ini

`IReferenceNumberService` (`GenerateAsync("KON")` → `KON-2026-0001`) · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` (latihan: `ConsoleNotificationService`) · `ICurrentUserService`.

## Corak kod yang mesti diikut

```csharp
// Services/Kontrak/KontrakModule.cs
public static class KontrakModule
{
    public static IServiceCollection AddKontrakModule(this IServiceCollection services)
    {
        services.AddScoped<IContractService, ContractService>();
        return services;
    }
}
```

```csharp
public class ContractRecordConfiguration : IEntityTypeConfiguration<ContractRecord>
{
    public void Configure(EntityTypeBuilder<ContractRecord> builder)
    {
        builder.ToTable("ContractRecords");
        builder.HasOne(x => x.Submission).WithMany().HasForeignKey(x => x.SubmissionId);
        builder.HasMany(x => x.Milestones).WithOne().HasForeignKey(m => m.ContractRecordId);
    }
}
```

```csharp
[Authorize]
public class ContractController(AppDbContext db, IWorkflowService workflow)
    : SubmissionControllerBase(db, workflow)
{
    [Authorize(Roles = "IctAdmin")]
    public async Task<IActionResult> Review(int id) { /* ... */ }
}
```

Warisi `SubmissionControllerBase` — jangan tulis semula logik kelulusan setiap controller.

## Cari dahulu, jana kemudian

```bash
grep -ri "ReferenceNumber" src/
grep -ri "ContractMilestone" src/
```

## Git & Jira (aliran kerja repo ini)

- **Repo & cabang:** kerja dalam repo `nres-bpm/pengurusan-kontrak`. `main` **dilindungi** — merge melalui **PR sahaja**. Buka cabang ciri pendek: `feat/<ciri-pendek>` (cth `feat/daftar-kontrak`).
- **Rentak harian:** `git pull --rebase` setiap pagi; commit + push + kemas kini board setiap petang.
- **Format commit:** `<modul>: <apa berubah, BM ringkas>`. Sertakan kunci Jira di hadapan — projek ini guna kunci **`CM-`**:

  ```text
  CM-42 kontrak: tambah ContractMilestone dan penjejakan tempoh
  ```
- **Board Jira:** projek **CM** ("Kontrak") di `bpm-nres.atlassian.net` — pindahkan isu ke sprint aktif & kemas kini status semasa kerja berjalan.
- **Deploy:** sistem ini berdiri & di-deploy sendiri ke subdomain `kontrak.`.

## Gaya kandungan latihan (bila menulis nota/lab)

Nota **Bahasa Melayu**, kod **Bahasa Inggeris**. Setiap lab: **Objektif** → langkah bernombor → blok kod penuh → **✅ Semakan**. Terangkan **kenapa** sebelum **bagaimana**. Kod lengkap & boleh dijalankan.

## Bila tidak pasti

Nyatakan ketidakpastian dan tanya. Untuk data **profil pengguna**, baca kontrak profil sedia ada — jangan reka skema profil baharu.
