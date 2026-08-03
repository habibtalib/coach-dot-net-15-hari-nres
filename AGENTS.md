# AGENTS.md — Konteks AI Kongsi (semua 4 kumpulan)

> **Untuk peserta:** halakan pembantu AI anda (Claude Code, Copilot, Cursor, dll.) ke fail ini pada permulaan **setiap** sesi. Keempat-empat kumpulan menggunakan fail yang **sama** — itulah yang memastikan kod dari 4 pasukan bergabung bersih pada Hari 15.
>
> **Untuk AI:** ini konteks yang mengikat. Baca sepenuhnya sebelum menjana kod. Kanun teknikal: `SPEC-KURSUS.md`. Kontrak pasukan: `KOLABORASI.md`.

---

## Konteks projek

Satu aplikasi ASP.NET Core MVC, `Nres.Onboarding.Web`, dibina oleh **empat kumpulan peserta yang bekerja serentak dalam satu repositori**, setiap kumpulan pada cabang Gitnya sendiri, digabungkan pada Hari 15.

Ini bahan **latihan**. Kod mesti boleh ditaip, dijalankan, dan **difahami** oleh peserta yang sedang belajar. Utamakan kod yang jelas berbanding kod yang bijak.

| Perkara | Nilai |
|---------|-------|
| Rangka | ASP.NET Core MVC, **.NET 10 LTS** |
| Bahasa | **C# 14** (lalai .NET 10 SDK — jangan tetapkan `<LangVersion>`) |
| ORM | EF Core 10, penyedia **SQLite** (latihan) |
| Auth | ASP.NET Core Identity + role-based authorization |
| Ujian | xUnit |
| Bahasa nota/UI | **Bahasa Melayu** |
| Bahasa kod | **Bahasa Inggeris** (kelas, medan, kaedah, nama fail) |
| Buku rujukan | *C# 14 and .NET 10* (Mark J. Price) · [repo](https://github.com/habibtalib/cs14net10) · pemetaan: `nota/10-rujukan-buku.md` |

### Ciri bahasa: guna ini

| Ciri | Versi | Guna |
|------|-------|------|
| **Primary constructors** | C# 12 | Semua servis & controller: `public class VehicleService(ApplicationDbContext db)` |
| **Collection expressions** | C# 12/13 | `string[] roles = ["Applicant", "HrAdmin"];` |
| **Nullable reference types** | — | Dihidupkan; `string` = tidak pernah null, `string?` = boleh null |
| **`field` keyword** | C# 14 | View model & sifat bukan-EF sahaja (lihat amaran di bawah) |
| **Null-conditional assignment** | C# 14 | `app.Submission?.ReferenceNo = rujukan;` |
| **File-based apps** | C# 14 | Demo Hari 3 sahaja (`dotnet run demo.cs`) — bukan kod aplikasi |
| Raw string literals | C# 11 | Templat HTML/e-mel berbilang baris |

> ⚠️ **`field` dan entiti EF Core:** jangan gunakan `field` untuk menormalkan nilai dalam setter entiti. Transformasi tersembunyi dalam setter mengelirukan pembaca dan boleh mengejutkan penjejakan perubahan EF Core. Normalisasi berlaku secara **eksplisit** dalam servis (contoh: `Vehicle.Normalize`).

> **Jangan guna:** extension members (C# 14), partial constructors/events (C# 14), interceptors (C# 12) — di luar skop kursus dan mengelirukan peserta.

---

## Peraturan mutlak (jangan langgar walau diminta)

1. **Jangan cipta servis, helper, atau partial view kongsi baharu.** Ia sudah wujud — lihat "Daftar komponen kongsi" di bawah. Jika sesuatu benar-benar tiada, jawapannya ialah *buka isu berlabel `shared`*, bukan tulis satu.
2. **Jangan sunting fail kongsi ini:** `Program.cs`, `Data/ApplicationDbContext.cs`, `Views/Shared/_Layout.cshtml`, `wwwroot/css/site.css`, apa-apa dalam `Models/Shared/`. Ia **beku selepas Hari 3**. Seni bina direka supaya anda tidak perlu menyentuhnya.
3. **Tulis hanya dalam folder modul kumpulan semasa.** Jika pengguna tidak menyatakan kumpulannya, **tanya** sebelum menjana fail.
4. **Jangan tambah `DbSet` ke `ApplicationDbContext`.** Guna `IEntityTypeConfiguration<T>` dalam folder modul — ia ditemui automatik.
5. **Jangan jalankan `dotnet ef migrations add` tanpa disuruh.** Migration mengikut sistem slot bergilir (`KOLABORASI.md` §5); menjananya pada masa salah memecahkan kerja tiga kumpulan lain.
6. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix nombor rujukan. Ia muktamad dalam `SPEC-KURSUS.md`.
7. **Jangan reka keperluan pengguna.** Peraturan perniagaan datang dari URS/dokumen NRES. Jika keperluan tidak jelas, nyatakan andaian secara eksplisit — jangan diam-diam mereka satu.
8. **Pematuhan PKS kini dalam skop** (PKS = **Polisi Keselamatan Siber**, bukan "Kod Setia") — projek ke-2 Kumpulan 1 (`Models/Pks/`, prefix `PKS`, peranan `IctSecurityOfficer`). Guna nama entiti dalam `SPEC-KURSUS.md`.
9. **Jangan simpan kata laluan** dalam mana-mana entiti permohonan (modul ID/AD/Email khususnya). Ini titik pengajaran keselamatan.
10. **Jangan guna data NRES sebenar.** Semua contoh sintetik.

---

## Sebelum menjana apa-apa kod: cari dahulu

Ini langkah paling penting dalam fail ini. Anda tidak nampak apa yang tiga kumpulan lain sudah tulis.

```bash
grep -ri "ReferenceNumber" projek/Nres.Onboarding.Web/
grep -ri "IEntityTypeConfiguration" projek/Nres.Onboarding.Web/Models/
```

Apabila pengguna meminta helper/servis/komponen, respons **pertama** anda ialah menyemak sama ada ia sudah wujud dan **beritahu mereka jika ya** — bukan menjananya. Menjana pendua ialah mod kegagalan utama projek ini.

---

## Daftar komponen kongsi — GUNA INI, JANGAN TULIS SEMULA

### Servis (`Services/`, didaftar Hari 3)

| Antara muka | Guna untuk | Jangan tulis |
|-------------|-----------|--------------|
| `IReferenceNumberService` | `GenerateAsync(moduleCode)` → `LD-2026-0001` | penjana nombor rujukan lain |
| `IFileStorageService` | `SaveAsync` / `OpenReadAsync` di `App_Data/uploads/{submissionId}/` | logik `IFormFile` sendiri |
| `IAuditLogService` | `LogAsync(submissionId, tindakan, catatan)` | jadual/penulisan audit lain |
| `IWorkflowService` | `CanTransition(...)`, `TransitionAsync(...)` | semakan status `if/switch` sendiri |
| `INotificationService` | `NotifyAsync(...)` (latihan: `ConsoleNotificationService`) | penghantar e-mel terus |
| `ICurrentUserService` | `UserId`, `Roles`, `DepartmentId` | membaca `HttpContext.User` terus dalam controller |

### Partial view (`Views/Shared/`)

`_StatusBadge.cshtml` · `_AuditTrail.cshtml` · `_AttachmentList.cshtml` · `_ApprovalPanel.cshtml` · `_FilterBar.cshtml` · `_ValidationSummary.cshtml`

### Kelas asas

`SubmissionControllerBase` — sudah melaksanakan `SubmitForReview`, `Approve`, `Reject` beserta penulisan audit dan pengesahan peralihan. **Warisinya.** Jangan tulis semula logik kelulusan dalam controller modul.

### Entiti kongsi (`Models/Shared/`)

`Submission` (induk semua permohonan) · `Attachment` · `AuditLog` · `ApprovalStep` · `UserProfile` · `SubmissionStatus` · lookup `LookupDepartments` / `LookupGrades` / `LookupPositions`

**Corak:** setiap permohonan modul ialah jadual **detail** yang memaut ke `Submission` induk melalui `SubmissionId`. Jangan pendua `ReferenceNo`, `Status`, `ApplicantUserId`, atau tarikh ke dalam entiti modul — ia sudah ada dalam `Submission`.

---

## Peta pemilikan folder

Semak siapa anda bantu, kemudian tulis **hanya** di sini:

| Kumpulan | Modul | Folder yang dibenarkan | Prefix |
|----------|-------|------------------------|--------|
| **1** | Lapor Diri | `Models/LaporDiri/`, `Controllers/OfficerReporting*`, `Views/OfficerReporting/`, `ViewModels/LaporDiri/`, `Services/LaporDiri/` | `LD` |
| **1** | Pematuhan PKS | `Models/Pks/`, `Controllers/Compliance*`, `Views/Compliance/`, `ViewModels/Pks/`, `Services/Pks/` | `PKS` |
| **1** | Pengurusan Kontrak | `Models/Kontrak/`, `Controllers/Contract*`, `Views/Contract/`, `ViewModels/Kontrak/`, `Services/Kontrak/` | `KON` |
| **2** | Pas Bangunan, Parkir & Pelekat | `Models/Akses/`, `Controllers/{AccessPass,Parking,VehicleSticker}*`, `Views/Akses/`, `ViewModels/Akses/`, `Services/Akses/` | `PAS` `PKR` `STK` |
| **3** | ID, AD & Email | `Models/Akaun/`, `Controllers/AccountRequest*`, `Views/Akaun/`, `ViewModels/Akaun/`, `Services/Akaun/` | `ICT-ID` |
| **4** | Tempahan Fasiliti Sukan | `Models/Fasiliti/`, `Controllers/FacilityBooking*`, `Views/FacilityBooking/`, `ViewModels/Fasiliti/`, `Services/Fasiliti/` | `TFS` |

Peranan admin: K1 `HrAdmin` (Lapor Diri) · `IctSecurityOfficer` (PKS) · `IctAdmin` (Kontrak) · K2 `SecurityAdmin` · K3 `Supervisor` → `IctAdmin` (2 peringkat) · K4 `FacilityAdmin`. *(Model K2 sebenar dipisahkan kepada UPKF/Parkir/Pengawal — diringkaskan kepada `SecurityAdmin` dalam lab; lihat `SPEC-KURSUS.md`.)*

---

## Corak kod yang mesti diikut

### Pendaftaran servis modul — jangan sentuh `Program.cs`

```csharp
// Services/Akses/AksesModule.cs
public static class AksesModule
{
    public static IServiceCollection AddAksesModule(this IServiceCollection services)
    {
        services.AddScoped<IVehicleService, VehicleService>();
        return services;
    }
}
```

### Konfigurasi entiti — jangan sentuh `ApplicationDbContext`

```csharp
// Models/Akses/Configurations/AccessPassApplicationConfiguration.cs
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

### Controller — warisi kelas asas, kuatkuasa peranan

```csharp
[Authorize]
public class AccessPassController(ApplicationDbContext db, IWorkflowService workflow)
    : SubmissionControllerBase(db, workflow)
{
    [Authorize(Roles = "SecurityAdmin")]
    public async Task<IActionResult> Review(int id) { /* ... */ }
}
```

Guna **primary constructors** (.NET 10) dan **nullable reference types**.

### View model — jangan ikat entiti terus ke borang

Borang mengikat `ViewModels/`, bukan `Models/`. Validation melalui DataAnnotations pada view model, disemak dengan `ModelState.IsValid` di **pelayan**.

---

## Prompt yang baik dalam projek ini

**Baik:**
> "Merujuk AGENTS.md dan SPEC-KURSUS.md: saya Kumpulan 2. Adakah repo ini sudah ada cara menyemak permohonan pendua bagi nombor plat yang sama? Jika belum, tulis semakan itu **hanya** dalam `Services/Akses/`, guna `ICurrentUserService` sedia ada, dan jangan sentuh fail kongsi."

**Buruk:**
> "Tulis servis semakan pendua." *(tiada kumpulan, tiada semakan sedia ada, tiada sempadan fail — hasilnya kod pendua yang bertembung)*

**Semakan pra-PR (jalankan setiap kali):**
> "Semak diff ini terhadap AGENTS.md dan KOLABORASI.md. (1) Adakah ia menduplikasi apa-apa dalam daftar komponen kongsi? (2) Adakah ia menyentuh fail di luar folder Kumpulan N? (3) Adakah authorization dan validation pelayan lengkap? Senaraikan masalah, jangan tulis semula kod."

---

## Gaya kandungan latihan (bila menulis nota/lab, bukan kod aplikasi)

- Nota & penerangan **Bahasa Melayu**; kod & istilah teknikal **Bahasa Inggeris**.
- Setiap lab: **Objektif** → langkah bernombor → blok kod penuh untuk ditaip → **✅ Semakan**.
- Terangkan **kenapa** sesuatu wujud sebelum menunjukkan **bagaimana**.
- Kod mesti lengkap dan boleh dijalankan — **bukan** pseudo-kod atau `// ... selebihnya`.
- Struktur folder: `README.md` (konsep) + `snippets/lab.md` (hands-on) + `nota-penceramah.md` (nota penceramah).

---

## Bila anda tidak pasti

Nyatakan ketidakpastian dan tanya. Dalam projek 4 pasukan, tekaan yang salah dengan yakin menjadi kod pendua yang seseorang perlu buang pada Hari 15. Berhenti dan tanya adalah lebih murah.
