# Lab Hari 7 — ID/AD/Email: Discovery & Model

Lab ini mengiringi [`../README.md`](../README.md) Hari 7. Ikut latihan **secara berurutan** — setiap latihan bina di atas latihan sebelumnya. Rujuk projek rujukan penuh di [`../../projek/`](../../projek/) untuk **banding** jawapan anda selepas cuba sendiri dahulu.

> **Peraturan lab:** Taip kod **sendiri** — jangan salin-tampal. Lab ini mengandaikan `Nres.Onboarding.Web` sudah wujud dan berjalan (Hari 1), dengan `Submission`, `Attachment`, `AuditLog`, `SubmissionStatus`, dan servis kongsi (`IReferenceNumberService`, `IAuditLogService`, `ICurrentUserService`) sudah dilaksanakan.

---

## Latihan 0 — Semakan Persediaan

**Objektif:** Sahkan projek sedia sebelum tambah kod baharu.

1. Jalankan projek sedia ada:

   ```bash
   cd Nres.Onboarding.Web
   dotnet build
   dotnet run
   ```

2. Sahkan halaman utama masih papar navigasi Modul 1 & Modul 2 (Lapor Diri, Pas/Parking/Pelekat).
3. Sahkan `dotnet ef migrations list` menunjukkan migration Modul 1 & 2 sudah `Applied`.

✅ **Semakan:** Projek `dotnet build` berjaya (0 ralat), aplikasi berjalan tanpa ralat startup.

---

## Latihan 1 — Enum `AccountRequestType` & Lookup `AccessType`

**Objektif:** Tulis enum jenis permohonan, dan entiti lookup jenis akses (bukan enum — rujuk sebab dalam README).

1. Cipta fail `Models/AccountRequestType.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public enum AccountRequestType
   {
       NewAdAccount = 0,
       NewEmailAccount = 1,
       AccountUpdate = 2,
       AccountDeactivation = 3,
       AdditionalSystemAccess = 4
   }
   ```

2. Cipta fail `Models/AccessType.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   // Jadual lookup — BUKAN enum. Baris baharu boleh ditambah oleh SystemAdmin
   // tanpa migration/deploy semula. Lihat README bahagian "Kenapa AccessType
   // sebagai jadual lookup".
   public class AccessType
   {
       public int Id { get; set; }

       public string Code { get; set; } = string.Empty;   // cth. "AD", "EMAIL", "VPN"

       public string Name { get; set; } = string.Empty;   // label paparan Bahasa Melayu

       public bool IsActive { get; set; } = true;
   }
   ```

✅ **Semakan:** Kedua-dua fail *compile* tanpa ralat (`dotnet build`).

---

## Latihan 2 — Entiti `AccountRequest` & `RequestedSystemAccess`

**Objektif:** Model butiran permohonan akaun, dan senarai akses yang diminta dalam satu permohonan.

1. Cipta fail `Models/AccountRequest.cs`:

   ```csharp
   using System.ComponentModel.DataAnnotations;
   using System.ComponentModel.DataAnnotations.Schema;

   namespace Nres.Onboarding.Web.Models;

   // JANGAN TAMBAH medan Password/Credential/Pin di sini atau di mana-mana
   // jadual Modul 3 — rujuk README Hari 7, "Titik Pengajaran Keselamatan".
   public class AccountRequest
   {
       public int Id { get; set; }

       // 1:1 dengan Submission induk — corak sama seperti OfficerReportingApplication (Modul 1).
       public int SubmissionId { get; set; }

       [ForeignKey(nameof(SubmissionId))]
       public Submission Submission { get; set; } = null!;

       [Required]
       public AccountRequestType RequestType { get; set; }

       [Required]
       [StringLength(200)]
       public string ApplicantFullName { get; set; } = string.Empty;

       [Required]
       [StringLength(20)]
       public string ApplicantIcNo { get; set; } = string.Empty;

       // FK kepada lookup kongsi LookupDepartment (wujud sejak Hari 1).
       [Required]
       public int DepartmentId { get; set; }

       [Required]
       [StringLength(100)]
       public string Position { get; set; } = string.Empty;

       // Id pengguna (AspNetUsers) bagi Penyelia yang perlu luluskan permohonan ini.
       [Required]
       public string SupervisorUserId { get; set; } = string.Empty;

       [Required]
       [StringLength(1000)]
       public string Justification { get; set; } = string.Empty;

       // Hanya diisi untuk AccountUpdate / AccountDeactivation — username akaun sedia ada.
       [StringLength(100)]
       public string? TargetSystemUsername { get; set; }

       // Hanya relevan untuk AccountDeactivation — tarikh nyahaktif berkuat kuasa.
       public DateTime? EffectiveDate { get; set; }

       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

       public List<RequestedSystemAccess> RequestedSystemAccesses { get; set; } = [];
   }
   ```

2. Cipta fail `Models/RequestedSystemAccess.cs`:

   ```csharp
   using System.ComponentModel.DataAnnotations;
   using System.ComponentModel.DataAnnotations.Schema;

   namespace Nres.Onboarding.Web.Models;

   public class RequestedSystemAccess
   {
       public int Id { get; set; }

       public int AccountRequestId { get; set; }

       [ForeignKey(nameof(AccountRequestId))]
       public AccountRequest AccountRequest { get; set; } = null!;

       public int AccessTypeId { get; set; }

       [ForeignKey(nameof(AccessTypeId))]
       public AccessType AccessType { get; set; } = null!;

       // Hanya relevan bila AccessType == Sistem Dalaman — nama sistem spesifik
       // (cth. "e-Aduan", "eSPKB"). Kosong untuk AD/Email/VPN/Shared Folder.
       [StringLength(150)]
       public string? SystemName { get; set; }

       [StringLength(500)]
       public string? Justification { get; set; }
   }
   ```

✅ **Semakan:** `dotnet build` berjaya. Perhatikan `AccountRequest` **tiada** satu pun medan berkaitan kata laluan — semak semula fail anda jika tidak pasti.

---

## Latihan 3 — Entiti `ApprovalStep` (Kongsi)

**Objektif:** Cipta entiti kongsi untuk rekod setiap keputusan kelulusan berbilang langkah — digunakan penuh di Hari 8, tetapi jadualnya dicipta sekarang supaya migration Hari 7 lengkap.

1. Cipta enum `Models/ApprovalStepStatus.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public enum ApprovalStepStatus
   {
       Pending = 0,
       Approved = 1,
       Rejected = 2
   }
   ```

2. Cipta fail `Models/ApprovalStep.cs`:

   ```csharp
   using System.ComponentModel.DataAnnotations.Schema;

   namespace Nres.Onboarding.Web.Models;

   // Entiti KONGSI (SPEC-KURSUS.md) — direkodkan di sini untuk setiap keputusan
   // rasmi dalam rantaian kelulusan (bukan setiap tindakan seperti AuditLog).
   public class ApprovalStep
   {
       public int Id { get; set; }

       public int SubmissionId { get; set; }

       [ForeignKey(nameof(SubmissionId))]
       public Submission Submission { get; set; } = null!;

       // 1 = Penyelia, 2 = ICT — urutan langkah dalam rantaian.
       public int StepOrder { get; set; }

       // Nama role yang bertanggungjawab pada langkah ini, cth. "Supervisor", "IctAdmin".
       public string ApproverRole { get; set; } = string.Empty;

       public ApprovalStepStatus Status { get; set; } = ApprovalStepStatus.Pending;

       public string? ActorUserId { get; set; }

       public DateTime? DecidedAt { get; set; }

       public string? Remarks { get; set; }
   }
   ```

✅ **Semakan:** `dotnet build` berjaya, tiada ralat namespace/using.

---

## Latihan 4 — Kemas Kini `ApplicationDbContext` & Seed `AccessType`

**Objektif:** Daftar `DbSet` baharu, konfigurasi relationship, dan seed lima jenis akses.

1. Buka `Data/ApplicationDbContext.cs`. Tambah `DbSet` baharu (letak berdekatan `DbSet` Modul 2 sedia ada):

   ```csharp
   public DbSet<AccessType> AccessTypes => Set<AccessType>();
   public DbSet<AccountRequest> AccountRequests => Set<AccountRequest>();
   public DbSet<RequestedSystemAccess> RequestedSystemAccesses => Set<RequestedSystemAccess>();
   public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
   ```

2. Dalam `OnModelCreating(ModelBuilder modelBuilder)`, tambah konfigurasi relationship + seed. **Tambah** blok ini pada penghujung method (jangan padam konfigurasi Modul 1/2 sedia ada):

   ```csharp
   modelBuilder.Entity<AccountRequest>()
       .HasOne(x => x.Submission)
       .WithOne()
       .HasForeignKey<AccountRequest>(x => x.SubmissionId)
       .OnDelete(DeleteBehavior.Cascade);

   modelBuilder.Entity<RequestedSystemAccess>()
       .HasOne(x => x.AccountRequest)
       .WithMany(x => x.RequestedSystemAccesses)
       .HasForeignKey(x => x.AccountRequestId)
       .OnDelete(DeleteBehavior.Cascade);

   modelBuilder.Entity<RequestedSystemAccess>()
       .HasOne(x => x.AccessType)
       .WithMany()
       .HasForeignKey(x => x.AccessTypeId)
       .OnDelete(DeleteBehavior.Restrict);

   modelBuilder.Entity<ApprovalStep>()
       .HasOne(x => x.Submission)
       .WithMany()
       .HasForeignKey(x => x.SubmissionId)
       .OnDelete(DeleteBehavior.Cascade);

   // Seed jenis akses — SESI 22, mengikut JADUAL.md (AD, Email, Shared folder, VPN, Sistem dalaman).
   modelBuilder.Entity<AccessType>().HasData(
       new AccessType { Id = 1, Code = "AD", Name = "Akaun Active Directory (AD)", IsActive = true },
       new AccessType { Id = 2, Code = "EMAIL", Name = "Akaun E-mel", IsActive = true },
       new AccessType { Id = 3, Code = "SHARED_FOLDER", Name = "Folder Kongsi (Shared Folder)", IsActive = true },
       new AccessType { Id = 4, Code = "VPN", Name = "Akses VPN", IsActive = true },
       new AccessType { Id = 5, Code = "INTERNAL_SYSTEM", Name = "Sistem Dalaman", IsActive = true }
   );
   ```

> **Kenapa `HasData` dan bukan kod seed manual dalam `Program.cs`?** `HasData` menjana **migration** yang memasukkan baris ini sebagai sebahagian skema — konsisten di semua persekitaran (dev, staging, prod) tanpa perlu jalankan skrip berasingan. Rujuk [learn.microsoft.com/ef/core/modeling/data-seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding).

✅ **Semakan:** `dotnet build` berjaya.

---

## Latihan 5 — Migration & Database Update

**Objektif:** Jana migration Modul 3 pertama dan sahkan skema SQLite.

1. Jana migration:

   ```bash
   dotnet ef migrations add AddAccountRequestModule
   ```

2. Semak fail migration yang dijana dalam `Migrations/` — sahkan ia mengandungi `CreateTable` untuk `AccessTypes`, `AccountRequests`, `RequestedSystemAccesses`, `ApprovalSteps`, dan `InsertData` untuk lima `AccessType`.

3. Kemas kini pangkalan data:

   ```bash
   dotnet ef database update
   ```

4. Sahkan skema (SQLite CLI, jika dipasang):

   ```bash
   sqlite3 nres_onboarding.db ".tables"
   sqlite3 nres_onboarding.db "SELECT Id, Code, Name FROM AccessTypes;"
   ```

   Anda sepatutnya nampak 5 baris: `AD`, `EMAIL`, `SHARED_FOLDER`, `VPN`, `INTERNAL_SYSTEM`.

✅ **Semakan:** `dotnet ef database update` berjaya tanpa ralat, dan pertanyaan SQL di atas memulangkan **tepat 5 baris** `AccessType`.

---

## Latihan 6 — Dashboard Modul ICT

**Objektif:** Cipta skrin pendaratan Modul 3 — asas untuk borang & senarai kelulusan Hari 8–9.

1. Cipta view model `ViewModels/IctDashboardViewModel.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.ViewModels;

   public class IctDashboardViewModel
   {
       public int DraftCount { get; set; }
       public int PendingSupervisorCount { get; set; }
       public int PendingIctCount { get; set; }
       public int CompletedCount { get; set; }
   }
   ```

2. Cipta controller `Controllers/IctDashboardController.cs`:

   ```csharp
   using Microsoft.AspNetCore.Mvc;
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Data;
   using Nres.Onboarding.Web.Models;
   using Nres.Onboarding.Web.ViewModels;

   namespace Nres.Onboarding.Web.Controllers;

   public class IctDashboardController(ApplicationDbContext db) : Controller
   {
       // ModuleCode rasmi Modul 3 — rujuk SPEC-KURSUS.md, jadual "Prefix nombor rujukan".
       public const string ModuleCode = "ICT-ID";

       public async Task<IActionResult> Index()
       {
           var accountRequestSubmissionIds = db.AccountRequests.Select(x => x.SubmissionId);

           var submissions = db.Submissions
               .Where(s => accountRequestSubmissionIds.Contains(s.Id));

           var model = new IctDashboardViewModel
           {
               DraftCount = await submissions.CountAsync(s => s.Status == SubmissionStatus.Draft),
               PendingSupervisorCount = await submissions.CountAsync(s => s.Status == SubmissionStatus.Submitted),
               PendingIctCount = await submissions.CountAsync(s => s.Status == SubmissionStatus.SupervisorApproved),
               CompletedCount = await submissions.CountAsync(s => s.Status == SubmissionStatus.Completed),
           };

           return View(model);
       }
   }
   ```

3. Cipta view `Views/IctDashboard/Index.cshtml`:

   ```cshtml
   @model Nres.Onboarding.Web.ViewModels.IctDashboardViewModel
   @{
       ViewData["Title"] = "Modul 3 — ID, AD & Email";
   }

   <h1>Modul 3 — ID, AD & Email</h1>
   <p class="text-muted">Pengurusan permohonan akaun AD, e-mel, kemas kini, nyahaktif, dan akses sistem tambahan.</p>

   <div class="alert alert-warning">
       <strong>Peringatan keselamatan:</strong> Modul ini tidak pernah menyimpan kata laluan.
       Kata laluan sebenar diserahkan di luar sistem oleh ICT selepas permohonan diproses.
   </div>

   <div class="row g-3 mb-4">
       <div class="col-md-3">
           <div class="card text-center">
               <div class="card-body">
                   <h2>@Model.DraftCount</h2>
                   <p class="mb-0">Draf</p>
               </div>
           </div>
       </div>
       <div class="col-md-3">
           <div class="card text-center">
               <div class="card-body">
                   <h2>@Model.PendingSupervisorCount</h2>
                   <p class="mb-0">Menunggu Penyelia</p>
               </div>
           </div>
       </div>
       <div class="col-md-3">
           <div class="card text-center">
               <div class="card-body">
                   <h2>@Model.PendingIctCount</h2>
                   <p class="mb-0">Menunggu ICT</p>
               </div>
           </div>
       </div>
       <div class="col-md-3">
           <div class="card text-center">
               <div class="card-body">
                   <h2>@Model.CompletedCount</h2>
                   <p class="mb-0">Selesai</p>
               </div>
           </div>
       </div>
   </div>

   <a class="btn btn-primary" asp-controller="AccountRequests" asp-action="Create">
       + Mohon Akaun / Akses Baharu
   </a>
   ```

   > **Nota:** `Controllers/AccountRequestsController` belum wujud lagi — pautan ini akan berfungsi selepas Hari 8. Untuk hari ini, cukup sahkan halaman dashboard sendiri memaparkan 4 kad dengan nilai `0` (kerana belum ada data permohonan).

4. Tambah pautan navigasi Modul 3 dalam `Views/Shared/_Layout.cshtml` (letak berdekatan pautan Modul 1/2 sedia ada):

   ```cshtml
   <a class="nav-link text-dark" asp-controller="IctDashboard" asp-action="Index">ID/AD/Email</a>
   ```

5. Jalankan aplikasi dan navigasi ke `/IctDashboard`:

   ```bash
   dotnet run
   ```

✅ **Semakan:** Halaman `/IctDashboard` papar 4 kad (kesemuanya `0`), amaran keselamatan kelihatan, dan navigasi utama ada pautan "ID/AD/Email". `dotnet build` bersih (0 ralat, 0 amaran baharu).

---

## Rujukan Fail

| Bahagian lab | Fail rujukan (`projek/`) |
|---|---|
| Enum & lookup (Latihan 1) | `projek/Nres.Onboarding.Web/Models/AccountRequestType.cs`, `AccessType.cs` |
| Entiti permohonan (Latihan 2) | `projek/Nres.Onboarding.Web/Models/AccountRequest.cs`, `RequestedSystemAccess.cs` |
| `ApprovalStep` (Latihan 3) | `projek/Nres.Onboarding.Web/Models/ApprovalStep.cs` |
| DbContext & seed (Latihan 4–5) | `projek/Nres.Onboarding.Web/Data/ApplicationDbContext.cs` |
| Dashboard (Latihan 6) | `projek/Nres.Onboarding.Web/Controllers/IctDashboardController.cs`, `Views/IctDashboard/Index.cshtml` |

---

## Cabaran (Pilihan)

1. Tambah medan `IsActive` filter — pastikan hanya `AccessType` dengan `IsActive == true` dipaparkan bila kita bina borang Hari 8 (fikirkan query `Where(x => x.IsActive)`).
2. Tulis satu ujian ringkas (boleh guna `dotnet run` + semakan manual buat masa ini, xUnit formal di Hari 15) untuk sahkan `AccountRequest` **tiada** properti bernama `Password` — cuba guna reflection: `typeof(AccountRequest).GetProperties().Any(p => p.Name.Contains("Password"))` sepatutnya `false`.
3. Fikirkan: kalau ICT tambah jenis akses baharu (cth. "Akses Portal Vendor") tahun depan, apa langkah tepat yang perlu diambil? (Jawapan: `INSERT` satu baris `AccessType` — **tiada** migration/deploy kod diperlukan, kerana `AccessType` ialah jadual operational, bukan enum.)

---

> 🎤 **Nota penceramah/jurulatih:** [`../nota-penceramah.md`](../nota-penceramah.md) untuk pemasaan, poin bercakap, dan silap biasa peserta Hari 7.
