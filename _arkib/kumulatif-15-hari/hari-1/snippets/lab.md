# Lab Hari 1 — Persediaan Projek & Seni Bina Kongsi

Lab ini mengiringi [`../README.md`](../README.md) Hari 1. Ikut latihan **secara berurutan** — setiap latihan bina di atas latihan sebelumnya. Rujuk projek rujukan penuh di [`../../projek/`](../../projek/) untuk **banding** kod anda selepas cuba sendiri dahulu (projek itu akan mengandungi hasil akhir kumulatif 15 hari kursus).

> **Peraturan lab:** Taip kod **sendiri** — jangan salin-tampal terus. Kesilapan menaip ialah latihan *debugging* pertama anda, dan menaip sendiri membantu ingatan konsep jauh lebih baik daripada salin-tampal.

---

## Senarai Semak Pra-Syarat

Sebelum mula Latihan 1, pastikan semua berikut sudah **✓**:

- [ ] `.NET 10 SDK` dipasang — sahkan dengan `dotnet --version` (patut papar `10.x.x`)
- [ ] `dotnet-ef` tool global dipasang — sahkan dengan `dotnet ef --version` (jika belum, lihat Latihan 1, Langkah 4)
- [ ] Visual Studio 2022 (17.12+) **atau** VS Code + sambungan **C# Dev Kit** dipasang
- [ ] Terminal/shell boleh jalankan arahan `dotnet` tanpa ralat "command not found"

---

## Latihan 1 — Cipta Projek & Sahkan Ia Berjalan

**Objektif:** Cipta projek ASP.NET Core MVC baharu bernama `Nres.Onboarding.Web`, tambah semua pakej NuGet yang diperlukan, dan sahkan ia boleh dijalankan sebelum menulis sebarang kod tambahan.

1. Buka terminal di lokasi di mana anda mahu simpan projek kursus (cth. folder `coach-dot-net-15-hari-nres/projek/` — rujuk struktur repo).

2. Cipta projek MVC baharu:

   ```bash
   dotnet new mvc -n Nres.Onboarding.Web
   cd Nres.Onboarding.Web
   ```

3. Jalankan aplikasi buat kali pertama, **sebelum** tambah apa-apa pakej — ini mengesahkan templat asas berfungsi:

   ```bash
   dotnet run
   ```

   Buka pelayar ke URL yang dipaparkan dalam terminal (cth. `https://localhost:5001` atau port serupa). Anda patut nampak halaman utama templat MVC lalai ("Welcome" dengan navigasi Home/Privacy). Tekan `Ctrl+C` dalam terminal untuk hentikan pelayan buat sementara.

4. Pasang (atau sahkan) `dotnet-ef` tool global — diperlukan untuk arahan migration nanti:

   ```bash
   dotnet tool install --global dotnet-ef
   ```

   Jika sudah dipasang, arahan ini akan beritahu ia sudah wujud — itu tidak mengapa, teruskan.

5. Tambah pakej NuGet EF Core, Identity, dan SQLite:

   ```bash
   dotnet add package Microsoft.EntityFrameworkCore
   dotnet add package Microsoft.EntityFrameworkCore.Design
   dotnet add package Microsoft.EntityFrameworkCore.Sqlite
   dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
   ```

6. Sahkan `Nres.Onboarding.Web.csproj` kini menyenaraikan keempat-empat pakej di atas dalam `<ItemGroup>` — buka fail itu dan semak secara visual.

7. Cuba `dotnet build` untuk pastikan tiada ralat kompil selepas tambah pakej:

   ```bash
   dotnet build
   ```

✅ **Semakan:** `dotnet run` papar halaman utama MVC lalai dalam pelayar tanpa ralat, dan `dotnet build` berjaya (`Build succeeded`) selepas keempat-empat pakej NuGet ditambah.

---

## Latihan 2 — Struktur Folder Projek

**Objektif:** Susun folder projek mengikut struktur muktamad SPEC-KURSUS.md supaya konsisten sepanjang 15 hari.

1. Dalam root `Nres.Onboarding.Web/`, cipta folder kosong berikut (folder `Controllers/`, `Views/`, `wwwroot/` sudah wujud daripada templat):

   ```bash
   mkdir Data
   mkdir Models
   mkdir ViewModels
   mkdir Services
   mkdir -p App_Data/uploads
   ```

   > Di Windows PowerShell, ganti `mkdir -p App_Data/uploads` dengan dua arahan berasingan: `mkdir App_Data` diikuti `mkdir App_Data\uploads`.

2. Buka `.gitignore` (dijana automatik oleh `dotnet new`) dan tambah baris berikut supaya fail pangkalan data & fail dimuat naik tidak masuk kawalan versi:

   ```gitignore
   # Nres.Onboarding — data latihan
   *.db
   *.db-shm
   *.db-wal
   App_Data/uploads/**
   !App_Data/uploads/.gitkeep
   ```

3. Cipta fail kosong `App_Data/uploads/.gitkeep` (supaya folder kekal dalam git walaupun kosong):

   ```bash
   touch App_Data/uploads/.gitkeep
   ```

4. Sahkan struktur akhir projek anda kelihatan seperti ini (ringkas):

   ```text
   Nres.Onboarding.Web/
     Controllers/
     Data/
     Models/
     ViewModels/
     Services/
     Views/
     wwwroot/
     App_Data/uploads/.gitkeep
     Program.cs
     Nres.Onboarding.Web.csproj
   ```

✅ **Semakan:** Struktur folder anda sepadan dengan senarai di atas, dan `App_Data/uploads/` wujud (walaupun kosong selain `.gitkeep`).

---

## Latihan 3 — Entiti Kongsi

**Objektif:** Tulis kelas entiti `SubmissionStatus`, `Submission`, `Attachment`, `AuditLog`, dan `UserProfile` dalam folder `Models/`.

1. Cipta fail `Models/SubmissionStatus.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public enum SubmissionStatus
   {
       Draft = 0,
       Submitted = 1,
       SupervisorApproved = 2,
       AdminApproved = 3,
       Rejected = 4,
       Completed = 5,
       Cancelled = 6
   }
   ```

2. Cipta fail `Models/Submission.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public class Submission
   {
       public int Id { get; set; }

       public string ReferenceNo { get; set; } = string.Empty;

       public string ModuleCode { get; set; } = string.Empty;

       public string ApplicantUserId { get; set; } = string.Empty;

       public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;

       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

       public DateTime? SubmittedAt { get; set; }

       public DateTime? CompletedAt { get; set; }

       public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();

       public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
   }
   ```

   > **Kenapa `ReferenceNo` kosong (`string.Empty`) pada peringkat draf?** Nombor rujukan sebenar (`LD-2026-0001`) hanya dijana semasa **submit**, bukan semasa cipta draf — kita bina servis ini Hari 3 (`IReferenceNumberService`). Sebelum submit, `ReferenceNo` kekal kosong.

3. Cipta fail `Models/Attachment.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public class Attachment
   {
       public int Id { get; set; }

       public int SubmissionId { get; set; }

       public Submission Submission { get; set; } = null!;

       public string OriginalFileName { get; set; } = string.Empty;

       public string StoredFileName { get; set; } = string.Empty;

       public string ContentType { get; set; } = string.Empty;

       public long FileSizeBytes { get; set; }

       public DateTime UploadedAt { get; set; } = DateTime.UtcNow;
   }
   ```

   > **Kenapa dua nama fail berasingan (`OriginalFileName` vs `StoredFileName`)?** Ini pratonton konsep Hari 3: kita **tidak** pernah guna nama fail asal yang dimuat naik pengguna sebagai nama fail fizikal di server (risiko keselamatan — nama fail boleh mengandungi aksara berbahaya atau bertindih dengan fail lain). `OriginalFileName` disimpan **hanya sebagai metadata paparan**; `StoredFileName` ialah nama selamat (cth. GUID) yang benar-benar wujud di cakera.

4. Cipta fail `Models/AuditLog.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public class AuditLog
   {
       public int Id { get; set; }

       public int SubmissionId { get; set; }

       public Submission Submission { get; set; } = null!;

       public string ActorUserId { get; set; } = string.Empty;

       public string Action { get; set; } = string.Empty;

       public string? Remarks { get; set; }

       public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
   }
   ```

5. Cipta fail `Models/UserProfile.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public class UserProfile
   {
       public int Id { get; set; }

       public string UserId { get; set; } = string.Empty;

       public string FullName { get; set; } = string.Empty;

       public string? Department { get; set; }

       public string? Position { get; set; }

       public string? Grade { get; set; }
   }
   ```

✅ **Semakan:** Lima fail wujud dalam `Models/` (`SubmissionStatus.cs`, `Submission.cs`, `Attachment.cs`, `AuditLog.cs`, `UserProfile.cs`), dan `dotnet build` masih berjaya tanpa ralat kompil.

---

## Latihan 4 — `ApplicationDbContext`

**Objektif:** Tulis `ApplicationDbContext` yang mewarisi `IdentityDbContext`, daftarkan `DbSet` untuk setiap entiti kongsi.

1. Cipta fail `Data/ApplicationDbContext.cs`:

   ```csharp
   using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.Data;

   public class ApplicationDbContext : IdentityDbContext
   {
       public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
           : base(options)
       {
       }

       public DbSet<Submission> Submissions => Set<Submission>();

       public DbSet<Attachment> Attachments => Set<Attachment>();

       public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

       public DbSet<UserProfile> UserProfiles => Set<UserProfile>();

       protected override void OnModelCreating(ModelBuilder builder)
       {
           base.OnModelCreating(builder);

           builder.Entity<Submission>(entity =>
           {
               entity.Property(s => s.ReferenceNo).HasMaxLength(50);
               entity.Property(s => s.ModuleCode).HasMaxLength(20);
               entity.HasIndex(s => s.ReferenceNo).IsUnique(false);
           });

           builder.Entity<Attachment>()
               .HasOne(a => a.Submission)
               .WithMany(s => s.Attachments)
               .HasForeignKey(a => a.SubmissionId)
               .OnDelete(DeleteBehavior.Cascade);

           builder.Entity<AuditLog>()
               .HasOne(a => a.Submission)
               .WithMany(s => s.AuditLogs)
               .HasForeignKey(a => a.SubmissionId)
               .OnDelete(DeleteBehavior.Cascade);

           builder.Entity<UserProfile>()
               .HasIndex(p => p.UserId)
               .IsUnique();
       }
   }
   ```

   > **Kenapa `IsUnique(false)` untuk `ReferenceNo` buat masa ini?** Semasa draf, banyak `Submission` boleh mempunyai `ReferenceNo` kosong (`string.Empty`) serentak — jika kita paksa unik sekarang, EF Core/SQLite akan tolak baris kedua dan seterusnya. Kita ketatkan peraturan ini di Hari 3 selepas `IReferenceNumberService` sedia (nombor rujukan hanya dijana semasa submit, satu sahaja per submission).

2. Buka `appsettings.json` dan tambah connection string SQLite:

   ```json
   {
     "ConnectionStrings": {
       "DefaultConnection": "Data Source=App_Data/nres.db"
     },
     "Logging": {
       "LogLevel": {
         "Default": "Information",
         "Microsoft.AspNetCore": "Warning"
       }
     },
     "AllowedHosts": "*"
   }
   ```

3. Buka `Program.cs` dan daftarkan `ApplicationDbContext` serta Identity **sebelum** `builder.Build()`. Fail `Program.cs` lengkap selepas langkah ini:

   ```csharp
   using Microsoft.AspNetCore.Identity;
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Data;

   var builder = WebApplication.CreateBuilder(args);

   // Tambah servis ke DI container.
   builder.Services.AddControllersWithViews();

   var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
       ?? throw new InvalidOperationException("Connection string 'DefaultConnection' tidak dijumpai.");

   builder.Services.AddDbContext<ApplicationDbContext>(options =>
       options.UseSqlite(connectionString));

   builder.Services.AddDefaultIdentity<IdentityUser>(options =>
           options.SignIn.RequireConfirmedAccount = false)
       .AddRoles<IdentityRole>()
       .AddEntityFrameworkStores<ApplicationDbContext>();

   var app = builder.Build();

   // Susunan middleware pipeline — PENTING, jangan tukar susunan.
   if (!app.Environment.IsDevelopment())
   {
       app.UseExceptionHandler("/Home/Error");
       app.UseHsts();
   }

   app.UseHttpsRedirection();
   app.UseStaticFiles();

   app.UseRouting();

   app.UseAuthentication();
   app.UseAuthorization();

   app.MapControllerRoute(
       name: "default",
       pattern: "{controller=Home}/{action=Index}/{id?}");

   app.MapRazorPages();

   app.Run();
   ```

   > **Kenapa `AddDefaultIdentity` + `.AddRoles<IdentityRole>()`?** `AddDefaultIdentity` sediakan Identity dengan halaman UI lalai (log masuk/daftar melalui Razor Pages, sebab itu `MapRazorPages()` juga diperlukan). `.AddRoles<IdentityRole>()` menghidupkan sokongan **peranan** (`Applicant`, `HrAdmin`, dsb. — lihat [SPEC-KURSUS.md](../../SPEC-KURSUS.md)) yang akan kita guna mulai Hari 3 untuk kawal capaian halaman semakan HR.

4. `dotnet build` semula untuk pastikan tiada ralat kompil.

✅ **Semakan:** `Data/ApplicationDbContext.cs` wujud, `Program.cs` mendaftarkan `ApplicationDbContext` dan Identity, `appsettings.json` ada `ConnectionStrings:DefaultConnection`, dan `dotnet build` berjaya.

---

## Latihan 5 — Migration Pertama & Cipta Pangkalan Data

**Objektif:** Jana migration `InitialShared`, jalankan `dotnet ef database update`, dan sahkan skema SQLite wujud dengan jadual yang betul.

1. Jana migration pertama:

   ```bash
   dotnet ef migrations add InitialShared
   ```

   Perhatikan output — EF Core akan cipta folder `Migrations/` dengan tiga fail baharu: `<timestamp>_InitialShared.cs`, `<timestamp>_InitialShared.Designer.cs`, dan `ApplicationDbContextModelSnapshot.cs`.

2. Buka fail `Migrations/<timestamp>_InitialShared.cs` dan **baca** kaedah `Up()` — perhatikan bagaimana setiap `DbSet` dalam `ApplicationDbContext` dipetakan kepada satu arahan `migrationBuilder.CreateTable(...)`. Anda tidak perlu edit fail ini secara manual.

3. Jalankan migration terhadap pangkalan data (SQLite akan dicipta automatik jika belum wujud):

   ```bash
   dotnet ef database update
   ```

4. Sahkan fail `App_Data/nres.db` kini wujud:

   ```bash
   ls -la App_Data/
   ```

5. (Pilihan, jika `sqlite3` CLI dipasang) Sahkan jadual yang dicipta:

   ```bash
   sqlite3 App_Data/nres.db ".tables"
   ```

   Anda patut nampak (antara lain) `AspNetUsers`, `AspNetRoles`, `Submissions`, `Attachments`, `AuditLogs`, `UserProfiles`.

✅ **Semakan:** `dotnet ef database update` berjaya tanpa ralat, fail `App_Data/nres.db` wujud, dan jadual `Submissions`, `Attachments`, `AuditLogs`, `UserProfiles` (serta jadual Identity `AspNetUsers`/`AspNetRoles`) wujud dalam skema.

---

## Latihan 6 — Navigasi Modul Placeholder & Dashboard

**Objektif:** Tambah menu navigasi untuk kelima-lima modul (walaupun kandungan sebenar belum wujud) supaya peserta faham bentuk keseluruhan aplikasi sejak hari pertama.

1. Buka `Views/Shared/_Layout.cshtml` dan cari elemen `<ul class="navbar-nav ...">` (biasanya berhampiran `<nav>` di bahagian atas). Tambah pautan modul selepas pautan `Home`/`Privacy` sedia ada:

   ```cshtml
   <li class="nav-item">
       <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="Index">Dashboard</a>
   </li>
   <li class="nav-item">
       <a class="nav-link text-dark" asp-area="" asp-controller="OfficerReporting" asp-action="Index">Lapor Diri</a>
   </li>
   <li class="nav-item">
       <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="ModulePlaceholder" asp-route-name="Pas, Parking &amp; Pelekat">Pas/Parking/Pelekat</a>
   </li>
   <li class="nav-item">
       <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="ModulePlaceholder" asp-route-name="ID, AD &amp; Email">ID/AD/Email</a>
   </li>
   <li class="nav-item">
       <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="ModulePlaceholder" asp-route-name="PKS">PKS</a>
   </li>
   <li class="nav-item">
       <a class="nav-link text-dark" asp-area="" asp-controller="Home" asp-action="ModulePlaceholder" asp-route-name="Aset ICT">Aset ICT</a>
   </li>
   ```

   > **Nota:** Pautan `OfficerReporting` di atas **belum** wujud kawalannya (`Controller`) — ia akan kita bina Hari 2. Untuk hari ini, biarkan pautan itu — ia akan papar ralat 404 sehingga esok. Ini normal dan sengaja.

2. Buka `Controllers/HomeController.cs` dan tambah kaedah `ModulePlaceholder`:

   ```csharp
   public IActionResult ModulePlaceholder(string name)
   {
       ViewData["ModuleName"] = name;
       return View();
   }
   ```

3. Cipta fail view `Views/Home/ModulePlaceholder.cshtml`:

   ```cshtml
   @{
       var moduleName = ViewData["ModuleName"] as string ?? "Modul";
       ViewData["Title"] = moduleName;
   }

   <div class="text-center">
       <h1 class="display-6">@moduleName</h1>
       <p class="lead">Modul ini akan dibina pada hari-hari seterusnya kursus DOTNET-NRES-15.</p>
       <p>Rujuk <a href="https://github.com" target="_blank" rel="noopener">jadual kursus</a> untuk pemetaan hari → modul.</p>
   </div>
   ```

4. Kemas kini `Views/Home/Index.cshtml` (halaman utama) supaya papar ringkasan lima modul — gantikan kandungan sedia ada dengan:

   ```cshtml
   @{
       ViewData["Title"] = "Dashboard NRES";
   }

   <div class="text-center">
       <h1 class="display-4">Nres.Onboarding.Web</h1>
       <p class="lead">Sistem Onboarding &amp; Khidmat Dalaman NRES — 5 modul permohonan &amp; aliran kerja kelulusan.</p>
   </div>

   <div class="row mt-4">
       <div class="col-md-4 mb-3">
           <div class="card h-100">
               <div class="card-body">
                   <h5 class="card-title">1. Lapor Diri</h5>
                   <p class="card-text">Pengurusan permohonan laporan diri pekerja baharu.</p>
               </div>
           </div>
       </div>
       <div class="col-md-4 mb-3">
           <div class="card h-100">
               <div class="card-body">
                   <h5 class="card-title">2. Pas, Parking &amp; Pelekat</h5>
                   <p class="card-text">Pengurusan akses kawasan dan kenderaan.</p>
               </div>
           </div>
       </div>
       <div class="col-md-4 mb-3">
           <div class="card h-100">
               <div class="card-body">
                   <h5 class="card-title">3. ID, AD &amp; Email</h5>
                   <p class="card-text">Pengurusan permohonan akaun pengguna sistem.</p>
               </div>
           </div>
       </div>
       <div class="col-md-4 mb-3">
           <div class="card h-100">
               <div class="card-body">
                   <h5 class="card-title">4. PKS</h5>
                   <p class="card-text">Pengisytiharan dan pemantauan pematuhan polisi.</p>
               </div>
           </div>
       </div>
       <div class="col-md-4 mb-3">
           <div class="card h-100">
               <div class="card-body">
                   <h5 class="card-title">5. Aset ICT</h5>
                   <p class="card-text">Pengurusan permohonan dan pinjaman aset ICT.</p>
               </div>
           </div>
       </div>
   </div>
   ```

5. Jalankan aplikasi dan uji setiap pautan navigasi (kecuali "Lapor Diri", yang sengaja 404 buat masa ini):

   ```bash
   dotnet run
   ```

✅ **Semakan:** Halaman utama papar 5 kad modul, navigasi atas mempunyai 6 pautan (Dashboard + 5 modul), dan pautan selain "Lapor Diri" membawa ke halaman placeholder yang papar nama modul betul. Aplikasi berjalan tanpa ralat kompil atau ralat masa jalan (selain 404 "Lapor Diri" yang dijangka).

---

## Rujukan Fail Sebenar

Untuk banding kod anda, fail rujukan lengkap (dikemas kini sepanjang kursus) ada di [`../../projek/`](../../projek/):

| Fail anda (lab) | Fail rujukan (projek sebenar) |
|------------------|-------------------------------|
| `Models/SubmissionStatus.cs`, `Submission.cs`, `Attachment.cs`, `AuditLog.cs`, `UserProfile.cs` | `projek/Nres.Onboarding.Web/Models/` |
| `Data/ApplicationDbContext.cs` | `projek/Nres.Onboarding.Web/Data/ApplicationDbContext.cs` |
| `Program.cs` | `projek/Nres.Onboarding.Web/Program.cs` |
| Migration `InitialShared` | `projek/Nres.Onboarding.Web/Migrations/` |

> Jika folder `projek/` masih kosong pada mesin anda, itu bermakna fasilitator belum salin projek rujukan penuh — teruskan lab berdasarkan penerangan di atas dan tanya fasilitator semasa sesi.

---

## Cabaran (Pilihan)

Selesaikan **sekurang-kurangnya satu** selepas Latihan 6 siap:

1. **Seed data lookup ringkas** — Tambah kaedah `SeedRolesAsync` yang dipanggil semasa aplikasi mula (`app.Run()` sebelum ini), yang mencipta tiga peranan pertama daripada [SPEC-KURSUS.md](../../SPEC-KURSUS.md): `Applicant`, `HrAdmin`, `SystemAdmin`, menggunakan `RoleManager<IdentityRole>` jika ia belum wujud.
2. **Index tambahan** — Tambah `HasIndex(s => s.ApplicantUserId)` pada `Submission` dalam `OnModelCreating` supaya carian "permohonan saya" (Hari 15) lebih pantas. Jana migration baharu (`dotnet ef migrations add AddApplicantIndex`) dan jalankan `dotnet ef database update`.
3. **`ToString()` untuk debug** — Tambah kaedah `override ToString()` pada `Submission` yang papar `$"{ModuleCode} #{Id} — {Status}"`, berguna semasa nyahpepijat dalam debugger/console.

---

Nota penceramah (pemasaan sesi, silap biasa, soalan perbincangan, deliverable akhir hari): [`../nota-penceramah.md`](../nota-penceramah.md).
