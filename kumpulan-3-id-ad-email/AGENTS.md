# AGENTS.md — Konteks AI: ID, AD & Email (`nres-bpm/id-ad-email`)

> **Untuk AI:** Baca sepenuhnya sebelum menjana kod untuk sistem ini. Ini konteks yang mengikat. Kanun teknikal penuh: [`SPEC-KURSUS.md`](../SPEC-KURSUS.md). Kontrak pasukan: [`KOLABORASI.md`](../KOLABORASI.md).
>
> Ini bahan **latihan**. Kod mesti boleh ditaip, dijalankan, dan **difahami** peserta. Utamakan kod yang jelas berbanding kod yang bijak.

## Sistem ini

**ID, AD & Email** — permohonan akaun pengguna & akses sistem (Active Directory / e-mel). Sistem **dalaman (SSO)** dengan **kelulusan 2 peringkat**: `Supervisor` (peringkat 1) → `IctAdmin` (peringkat 2). Termasuk pemprosesan ICT + RBAC + simulasi AD. Aliran: mohon akaun/akses → hantar → kelulusan Penyelia → pemprosesan `IctAdmin` → simulasi cipta AD/e-mel → audit.

| Perkara | Nilai |
|---------|-------|
| Sistem | ID, AD & Email |
| Repo | `nres-bpm/id-ad-email` · subdomain `id.` |
| Prefix no. rujukan | `ICT-ID` → `ICT-ID-2026-0001` |
| Peranan penyemak | `Supervisor` (peringkat 1) → `IctAdmin` (peringkat 2) |
| Akses | **Dalaman (SSO)** — sistem **membaca** profil pengguna |

## Susunan teknologi

| Perkara | Nilai |
|---------|-------|
| Rangka | ASP.NET Core MVC, **.NET 10 LTS** |
| Bahasa | **C# 14** (lalai .NET 10 SDK — jangan tetapkan `<LangVersion>`) |
| ORM | EF Core 10, penyedia **SQLite** (latihan) |
| Auth | **SSO** + role-based authorization (RBAC 2 peringkat) |
| Ujian | xUnit |
| Bahasa nota/UI | **Bahasa Melayu** · Bahasa kod | **Bahasa Inggeris** |

## Ciri bahasa C#: guna ini

| Ciri | Guna |
|------|------|
| **Primary constructors** (C# 12) | `public class AccountRequestService(AppDbContext db)` |
| **Collection expressions** (C# 12/13) | `string[] roles = ["Applicant", "Supervisor", "IctAdmin"];` |
| **Nullable reference types** | Dihidupkan |
| **`field` keyword** (C# 14) | View model & sifat bukan-EF sahaja |

> ⚠️ **Jangan** guna `field` dalam setter entiti EF Core. **Jangan guna:** extension members, partial constructors/events, interceptors.

## Peraturan mutlak (jangan langgar walau diminta)

1. **JANGAN SIMPAN KATA LALUAN** dalam mana-mana entiti permohonan. Ini **titik pengajaran keselamatan** utama sistem ini — ajar peserta **jangan**. Akaun AD/e-mel disimulasi; kata laluan dijana/diset di luar rekod permohonan.
2. **Kerja hanya dalam fail sistem ini.** Jika tidak pasti keperluan, **tanya** sebelum menjana.
3. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix `ICT-ID`. Kekalkan model **2 peringkat** (`Supervisor` → `IctAdmin`).
4. **Jangan reka keperluan pengguna** — datang dari URS/dokumen NRES; nyatakan andaian eksplisit.
5. **Elak pendua dalam repo ini.** Cari dahulu sebelum menulis helper/servis serupa.
6. **Borang mengikat `ViewModels/`, bukan `Models/`.** Validation disemak `ModelState.IsValid` di **pelayan**.
7. **Jangan guna data NRES sebenar** — semua contoh **sintetik**.

## Entiti sistem ini (DB sendiri)

- Corak aliran: `Submission` (induk) · `Attachment` · `AuditLog` · `ApprovalStep` (dua langkah: `SupervisorApproved` → `AdminApproved`) · `SubmissionStatus` · lookup setempat.
- Khusus ID/AD/Email: **`AccountRequest`** · **`RequestedSystemAccess`** (senarai sistem/akses yang dipohon — tiada medan kata laluan).
- **Corak:** entiti detail memaut ke `Submission` melalui `SubmissionId`; jangan pendua `ReferenceNo`/`Status`/tarikh.

## Status & aliran kerja (kelulusan 2 peringkat)

```csharp
public enum SubmissionStatus
{
    Draft = 0, Submitted = 1, SupervisorApproved = 2, AdminApproved = 3,
    Rejected = 4, Completed = 5, Cancelled = 6
}
```

```text
Form → Validation → Draft → Submit → Review (Supervisor) → SupervisorApproved
     → Process (IctAdmin) → AdminApproved → simulasi AD/e-mel → Completed → Audit
```

## Servis dalam sistem ini

`IReferenceNumberService` (`GenerateAsync("ICT-ID")`) · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` (kuatkuasa peralihan 2 peringkat) · `INotificationService` (latihan: `ConsoleNotificationService`) · `ICurrentUserService`.

## Corak kod yang mesti diikut

```csharp
// Services/Akaun/AkaunModule.cs
public static class AkaunModule
{
    public static IServiceCollection AddAkaunModule(this IServiceCollection services)
    {
        services.AddScoped<IAccountRequestService, AccountRequestService>();
        return services;
    }
}
```

```csharp
public class AccountRequestConfiguration : IEntityTypeConfiguration<AccountRequest>
{
    public void Configure(EntityTypeBuilder<AccountRequest> builder)
    {
        builder.ToTable("AccountRequests");
        builder.HasOne(x => x.Submission).WithMany().HasForeignKey(x => x.SubmissionId);
        builder.HasMany(x => x.RequestedAccesses).WithOne().HasForeignKey(a => a.AccountRequestId);
        // TIADA medan kata laluan — sengaja.
    }
}
```

```csharp
[Authorize]
public class AccountController(AppDbContext db, IWorkflowService workflow)
    : SubmissionControllerBase(db, workflow)
{
    [Authorize(Roles = "Supervisor")]
    public async Task<IActionResult> SupervisorReview(int id) { /* ... */ }

    [Authorize(Roles = "IctAdmin")]
    public async Task<IActionResult> Process(int id) { /* ... */ }
}
```

Warisi `SubmissionControllerBase` — jangan tulis semula logik kelulusan setiap controller.

## Cari dahulu, jana kemudian

```bash
grep -ri "ReferenceNumber" src/
grep -ri "RequestedSystemAccess" src/
```

## Git & Jira (aliran kerja repo ini)

- **Repo & cabang:** kerja dalam repo `nres-bpm/id-ad-email`. `main` **dilindungi** — merge melalui **PR sahaja**. Buka cabang ciri pendek: `feat/<ciri-pendek>` (cth `feat/kelulusan-2-peringkat`).
- **Rentak harian:** `git pull --rebase` setiap pagi; commit + push + kemas kini board setiap petang.
- **Format commit:** `<modul>: <apa berubah, BM ringkas>`. Sertakan kunci Jira di hadapan — projek ini guna kunci **`ID-`**:

  ```text
  ID-42 id-ad-email: tambah kelulusan penyelia (peringkat 1) tanpa simpan kata laluan
  ```
- **Board Jira:** projek **ID** ("AD Email") di `bpm-nres.atlassian.net` — pindahkan isu ke sprint aktif & kemas kini status semasa kerja berjalan.
- **Deploy:** sistem ini berdiri & di-deploy sendiri ke subdomain `id.`.

## Gaya kandungan latihan (bila menulis nota/lab)

Nota **Bahasa Melayu**, kod **Bahasa Inggeris**. Setiap lab: **Objektif** → langkah bernombor → blok kod penuh → **✅ Semakan**. Terangkan **kenapa** sebelum **bagaimana** (terutama sebab **tiada kata laluan disimpan**). Kod lengkap & boleh dijalankan.

## Bila tidak pasti

Nyatakan ketidakpastian dan tanya. Untuk data **profil pengguna**, baca kontrak profil sedia ada — jangan reka skema profil baharu.
