# AGENTS.md — Konteks AI: Tempahan Fasiliti Sukan (`nres-bpm/tempahan-fasiliti-sukan`)

> **Untuk AI:** Baca sepenuhnya sebelum menjana kod untuk sistem ini. Ini konteks yang mengikat. Kanun teknikal penuh: [`SPEC-KURSUS.md`](../SPEC-KURSUS.md). Kontrak pasukan: [`KOLABORASI.md`](../KOLABORASI.md).
>
> Ini bahan **latihan**. Kod mesti boleh ditaip, dijalankan, dan **difahami** peserta. Utamakan kod yang jelas berbanding kod yang bijak.

## Sistem ini

**Tempahan Fasiliti Sukan** — tempahan gelanggang & kemudahan sukan NRES. Sistem **dalaman (SSO)**. Penyemak: **`FacilityAdmin`**. Ciri utama: **semakan slot bertindih** — jangan benarkan dua tempahan diluluskan bagi fasiliti + slot masa yang sama. Aliran: pilih fasiliti + slot → semak bertindih → hantar → kelulusan `FacilityAdmin` → peringatan/kalendar → audit.

| Perkara | Nilai |
|---------|-------|
| Sistem | Tempahan Fasiliti Sukan |
| Repo | `nres-bpm/tempahan-fasiliti-sukan` · subdomain `fasiliti.` |
| Prefix no. rujukan | `TFS` → `TFS-2026-0001` |
| Peranan penyemak | `FacilityAdmin` |
| Akses | **Dalaman (SSO)** — sistem **membaca** profil pengguna |

## Susunan teknologi

| Perkara | Nilai |
|---------|-------|
| Rangka | ASP.NET Core MVC, **.NET 10 LTS** |
| Bahasa | **C# 14** (lalai .NET 10 SDK — jangan tetapkan `<LangVersion>`) |
| ORM | EF Core 10, penyedia **SQLite** (latihan) |
| Auth | **SSO** + role-based authorization |
| Laporan | Eksport **PDF (QuestPDF)** & **Excel (ClosedXML)**; kalendar slot |
| Ujian | xUnit (termasuk ujian slot bertindih) |
| Bahasa nota/UI | **Bahasa Melayu** · Bahasa kod | **Bahasa Inggeris** |

## Ciri bahasa C#: guna ini

| Ciri | Guna |
|------|------|
| **Primary constructors** (C# 12) | `public class BookingService(AppDbContext db)` |
| **Collection expressions** (C# 12/13) | `string[] roles = ["Applicant", "FacilityAdmin"];` |
| **Nullable reference types** | Dihidupkan |
| **`field` keyword** (C# 14) | View model & sifat bukan-EF sahaja |

> ⚠️ **Jangan** guna `field` dalam setter entiti EF Core. **Jangan guna:** extension members, partial constructors/events, interceptors.

## Peraturan mutlak (jangan langgar walau diminta)

1. **Kerja hanya dalam fail sistem ini.** Jika tidak pasti keperluan, **tanya** sebelum menjana.
2. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix `TFS`.
3. **Semakan slot bertindih di pelayan.** Jangan bergantung pada UI sahaja untuk menghalang tempahan bertindih — sahkan dalam servis sebelum `Submit`/`Approve`.
4. **Jangan reka keperluan pengguna** — datang dari URS/dokumen NRES; nyatakan andaian eksplisit.
5. **Elak pendua dalam repo ini.** Cari dahulu sebelum menulis helper/servis serupa.
6. **Borang mengikat `ViewModels/`, bukan `Models/`.** Validation disemak `ModelState.IsValid` di **pelayan**.
7. **Jangan guna data NRES sebenar** — semua contoh **sintetik**.

## Entiti sistem ini (DB sendiri)

- Corak aliran: `Submission` (induk) · `Attachment` · `AuditLog` · `ApprovalStep` · `SubmissionStatus` · lookup setempat.
- Khusus Fasiliti Sukan: **`SportsFacility`** (katalog gelanggang/kemudahan) · **`FacilityBookingApplication`** · **`FacilityBookingSlot`** (slot masa; asas semakan bertindih).
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
Form (pilih fasiliti + slot) → Validation (semak bertindih) → Draft → Submit
     → Review (FacilityAdmin) → Approve/Reject → Audit → Kalendar/Peringatan → Report
```

## Servis dalam sistem ini

`IReferenceNumberService` (`GenerateAsync("TFS")` → `TFS-2026-0001`) · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` (latihan: `ConsoleNotificationService`) · `ICurrentUserService`. Tambah semakan bertindih dalam servis tempahan (cth `IBookingService.HasSlotClash(...)`).

## Corak kod yang mesti diikut

```csharp
// Services/Tempahan/TempahanModule.cs
public static class TempahanModule
{
    public static IServiceCollection AddTempahanModule(this IServiceCollection services)
    {
        services.AddScoped<IBookingService, BookingService>();
        return services;
    }
}
```

```csharp
public class FacilityBookingApplicationConfiguration : IEntityTypeConfiguration<FacilityBookingApplication>
{
    public void Configure(EntityTypeBuilder<FacilityBookingApplication> builder)
    {
        builder.ToTable("FacilityBookingApplications");
        builder.HasOne(x => x.Submission).WithMany().HasForeignKey(x => x.SubmissionId);
        builder.HasOne(x => x.Facility).WithMany().HasForeignKey(x => x.SportsFacilityId);
        builder.HasOne(x => x.Slot).WithMany().HasForeignKey(x => x.FacilityBookingSlotId);
    }
}
```

```csharp
[Authorize]
public class BookingController(AppDbContext db, IWorkflowService workflow, IBookingService booking)
    : SubmissionControllerBase(db, workflow)
{
    [Authorize(Roles = "FacilityAdmin")]
    public async Task<IActionResult> Review(int id) { /* ... */ }
}
```

Warisi `SubmissionControllerBase` — jangan tulis semula logik kelulusan setiap controller.

## Cari dahulu, jana kemudian

```bash
grep -ri "ReferenceNumber" src/
grep -ri "SlotClash\|FacilityBookingSlot" src/
```

## Gaya kandungan latihan (bila menulis nota/lab)

Nota **Bahasa Melayu**, kod **Bahasa Inggeris**. Setiap lab: **Objektif** → langkah bernombor → blok kod penuh → **✅ Semakan**. Terangkan **kenapa** sebelum **bagaimana** (terutama logik **slot bertindih**). Kod lengkap & boleh dijalankan.

## Bila tidak pasti

Nyatakan ketidakpastian dan tanya. Untuk data **profil pengguna**, baca kontrak profil sedia ada — jangan reka skema profil baharu.
