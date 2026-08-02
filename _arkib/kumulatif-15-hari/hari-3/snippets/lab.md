# Lab Hari 3 — Lapor Diri: Lampiran, Submit & Semakan

Lab ini mengiringi [`../README.md`](../README.md) Hari 3. Ikut latihan **secara berurutan**. Rujuk projek rujukan penuh di [`../../projek/`](../../projek/) untuk banding kod anda selepas cuba sendiri dahulu.

> **Sebelum mula:** Pastikan `dotnet run` masih berfungsi, dan anda sudah ada sekurang-kurangnya satu draf Lapor Diri (dari Hari 2) untuk diuji sepanjang lab ini.

---

## Latihan 1 — `IFileStorageService`

**Objektif:** Tulis servis penyimpanan fail yang mengesahkan saiz/jenis, menjana nama fail selamat, dan menyimpan metadata sebagai `Attachment`.

1. Cipta fail `Services/IFileStorageService.cs`:

   ```csharp
   using Microsoft.AspNetCore.Http;
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.Services;

   public interface IFileStorageService
   {
       Task<Attachment> SaveAsync(int submissionId, IFormFile file, CancellationToken cancellationToken = default);

       string GetPhysicalPath(Attachment attachment);
   }
   ```

2. Cipta fail `Services/LocalFileStorageService.cs`:

   ```csharp
   using Microsoft.AspNetCore.Http;
   using Nres.Onboarding.Web.Data;
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.Services;

   public class LocalFileStorageService : IFileStorageService
   {
       private static readonly string[] AllowedExtensions = { ".pdf", ".jpg", ".jpeg", ".png" };
       private const long MaxFileSizeBytes = 5 * 1024 * 1024; // 5 MB

       private readonly IWebHostEnvironment _environment;
       private readonly ApplicationDbContext _db;

       public LocalFileStorageService(IWebHostEnvironment environment, ApplicationDbContext db)
       {
           _environment = environment;
           _db = db;
       }

       public async Task<Attachment> SaveAsync(int submissionId, IFormFile file, CancellationToken cancellationToken = default)
       {
           if (file is null || file.Length <= 0)
           {
               throw new InvalidOperationException("Fail kosong tidak dibenarkan.");
           }

           if (file.Length > MaxFileSizeBytes)
           {
               throw new InvalidOperationException("Saiz fail tidak boleh melebihi 5 MB.");
           }

           // PENTING: nama fail asal (file.FileName) TIDAK PERNAH digunakan sebagai
           // nama fail fizikal — ia hanya disimpan sebagai metadata paparan.
           var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

           if (!AllowedExtensions.Contains(extension))
           {
               throw new InvalidOperationException(
                   "Jenis fail tidak dibenarkan. Hanya PDF, JPG, JPEG, PNG dibenarkan.");
           }

           var uploadFolder = Path.Combine(
               _environment.ContentRootPath, "App_Data", "uploads", submissionId.ToString());

           Directory.CreateDirectory(uploadFolder);

           var storedFileName = $"{Guid.NewGuid()}{extension}";
           var physicalPath = Path.Combine(uploadFolder, storedFileName);

           await using (var stream = new FileStream(physicalPath, FileMode.Create))
           {
               await file.CopyToAsync(stream, cancellationToken);
           }

           var attachment = new Attachment
           {
               SubmissionId = submissionId,
               OriginalFileName = Path.GetFileName(file.FileName),
               StoredFileName = storedFileName,
               ContentType = file.ContentType,
               FileSizeBytes = file.Length,
               UploadedAt = DateTime.UtcNow
           };

           _db.Attachments.Add(attachment);
           await _db.SaveChangesAsync(cancellationToken);

           return attachment;
       }

       public string GetPhysicalPath(Attachment attachment)
       {
           return Path.Combine(
               _environment.ContentRootPath,
               "App_Data",
               "uploads",
               attachment.SubmissionId.ToString(),
               attachment.StoredFileName);
       }
   }
   ```

   > **Perhatikan:** `Path.Combine(_environment.ContentRootPath, "App_Data", ...)` — kita guna `ContentRootPath` (root **projek**), **bukan** `WebRootPath` (`wwwroot/`). Ini yang memastikan fail tersimpan **di luar** kawasan yang boleh dicapai terus oleh pelayar.

3. Daftarkan servis dalam `Program.cs` — tambah baris ini selepas pendaftaran `ApplicationDbContext`:

   ```csharp
   builder.Services.AddScoped<IFileStorageService, LocalFileStorageService>();
   ```

4. `dotnet build` untuk pastikan tiada ralat kompil.

✅ **Semakan:** `Services/IFileStorageService.cs` dan `Services/LocalFileStorageService.cs` wujud, servis didaftar dalam `Program.cs`, `dotnet build` berjaya.

---

## Latihan 2 — Muat Naik & Muat Turun Lampiran

**Objektif:** Sambungkan `IFileStorageService` ke `OfficerReportingController`, tambah UI muat naik di halaman Details, dan tambah action muat turun yang menyemak kebenaran.

1. Buka `Controllers/OfficerReportingController.cs`. Tambah `using Nres.Onboarding.Web.Services;` di atas, dan suntik `IFileStorageService` melalui constructor:

   ```csharp
   private readonly IFileStorageService _fileStorageService;

   public OfficerReportingController(
       ApplicationDbContext db,
       UserManager<IdentityUser> userManager,
       IFileStorageService fileStorageService)
   {
       _db = db;
       _userManager = userManager;
       _fileStorageService = fileStorageService;
   }
   ```

2. Kemas kini action `Details` supaya sertakan `Attachments`:

   ```csharp
   public async Task<IActionResult> Details(int id)
   {
       var application = await _db.OfficerReportingApplications
           .Include(o => o.Submission)
               .ThenInclude(s => s.Attachments)
           .FirstOrDefaultAsync(o => o.Id == id);

       if (application is null)
       {
           return NotFound();
       }

       return View(application);
   }
   ```

3. Tambah action `UploadAttachment` dan `Download` di bawah `Details`:

   ```csharp
   [HttpPost]
   [ValidateAntiForgeryToken]
   public async Task<IActionResult> UploadAttachment(int id, IFormFile file)
   {
       var application = await _db.OfficerReportingApplications
           .Include(o => o.Submission)
           .FirstOrDefaultAsync(o => o.Id == id);

       if (application is null)
       {
           return NotFound();
       }

       if (application.Submission.Status != SubmissionStatus.Draft)
       {
           TempData["StatusMessage"] = "Lampiran hanya boleh ditambah semasa status Draf.";
           return RedirectToAction(nameof(Details), new { id });
       }

       if (file is null || file.Length == 0)
       {
           TempData["StatusMessage"] = "Sila pilih fail untuk dimuat naik.";
           return RedirectToAction(nameof(Details), new { id });
       }

       try
       {
           await _fileStorageService.SaveAsync(application.SubmissionId, file);
           TempData["StatusMessage"] = "Lampiran berjaya dimuat naik.";
       }
       catch (InvalidOperationException ex)
       {
           TempData["StatusMessage"] = ex.Message;
       }

       return RedirectToAction(nameof(Details), new { id });
   }

   public async Task<IActionResult> Download(int attachmentId)
   {
       var attachment = await _db.Attachments
           .Include(a => a.Submission)
           .FirstOrDefaultAsync(a => a.Id == attachmentId);

       if (attachment is null)
       {
           return NotFound();
       }

       var userId = _userManager.GetUserId(User);
       var isOwner = attachment.Submission.ApplicantUserId == userId;
       var isHrAdmin = User.IsInRole("HrAdmin");

       if (!isOwner && !isHrAdmin)
       {
           return Forbid();
       }

       var physicalPath = _fileStorageService.GetPhysicalPath(attachment);

       if (!System.IO.File.Exists(physicalPath))
       {
           return NotFound();
       }

       return PhysicalFile(physicalPath, attachment.ContentType, attachment.OriginalFileName);
   }
   ```

   > **Kenapa `Download` semak `isOwner || isHrAdmin` sebelum hantar fail?** Ini **gate** yang disebut dalam README — walaupun fail berada di luar `wwwroot/`, ia tetap perlu satu lapisan kebenaran eksplisit di sisi pelayan. Pemohon lain (bukan pemilik) atau pengguna tanpa peranan `HrAdmin` akan terima `403 Forbidden`, bukan kandungan fail.

4. Buka `Views/OfficerReporting/Details.cshtml` dan tambah bahagian lampiran serta butang Hantar Permohonan **selepas** senarai `<dl class="row">...</dl>` sedia ada:

   ```cshtml
   <h2>Lampiran</h2>

   @if (Model.Submission.Attachments.Any())
   {
       <ul class="list-group mb-3">
           @foreach (var attachment in Model.Submission.Attachments)
           {
               <li class="list-group-item d-flex justify-content-between align-items-center">
                   <a asp-action="Download" asp-route-attachmentId="@attachment.Id">
                       @attachment.OriginalFileName
                   </a>
                   <span class="text-muted">@(attachment.FileSizeBytes / 1024) KB</span>
               </li>
           }
       </ul>
   }
   else
   {
       <p class="text-muted">Belum ada lampiran dimuat naik.</p>
   }

   @if (Model.Submission.Status == Nres.Onboarding.Web.Models.SubmissionStatus.Draft)
   {
       <form asp-action="UploadAttachment" asp-route-id="@Model.Id" method="post" enctype="multipart/form-data" class="mb-3">
           @Html.AntiForgeryToken()
           <div class="input-group">
               <input type="file" name="file" class="form-control" accept=".pdf,.jpg,.jpeg,.png" />
               <button type="submit" class="btn btn-outline-secondary">Muat Naik Lampiran</button>
           </div>
           <div class="form-text">Format dibenarkan: PDF, JPG, PNG. Saiz maksimum 5 MB.</div>
       </form>

       <form asp-action="Submit" asp-route-id="@Model.Id" method="post" class="mb-3">
           @Html.AntiForgeryToken()
           <button type="submit" class="btn btn-success">Hantar Permohonan</button>
       </form>
   }
   ```

   > **Kenapa `enctype="multipart/form-data"` diperlukan?** Borang HTML lalai menghantar data sebagai teks (`application/x-www-form-urlencoded`) — format ini **tidak boleh** membawa kandungan binari fail. `multipart/form-data` membenarkan borang membawa **kedua-dua** medan teks biasa **dan** kandungan fail dalam satu hantaran.

5. Jalankan aplikasi, buka Details satu draf Lapor Diri, dan uji muat naik satu fail PDF/JPG kecil. Sahkan fail tersebut kelihatan dalam senarai lampiran dan boleh dimuat turun semula.

6. Uji sekatan: cuba muat naik fail > 5 MB, dan fail dengan sambungan tidak dibenarkan (cth. `.exe` — tukar nama sebarang fail teks kepada `.exe` untuk uji). Kedua-dua patut ditolak dengan mesej ralat yang jelas.

✅ **Semakan:** Lampiran boleh dimuat naik dan dimuat turun untuk draf Lapor Diri; fail terlalu besar/salah jenis ditolak; fail fizikal tersimpan di `App_Data/uploads/{submissionId}/` (sahkan dengan `ls App_Data/uploads/`) dengan nama GUID, **bukan** nama asal.

---

## Latihan 3 — `IReferenceNumberService`

**Objektif:** Jana nombor rujukan format `LD-2026-0001` yang hanya dikeluarkan semasa submit.

1. Cipta fail `Services/IReferenceNumberService.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Services;

   public interface IReferenceNumberService
   {
       Task<string> GenerateAsync(string moduleCode);
   }
   ```

2. Cipta fail `Services/SequentialReferenceNumberService.cs`:

   ```csharp
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Data;

   namespace Nres.Onboarding.Web.Services;

   public class SequentialReferenceNumberService : IReferenceNumberService
   {
       private readonly ApplicationDbContext _db;

       public SequentialReferenceNumberService(ApplicationDbContext db)
       {
           _db = db;
       }

       public async Task<string> GenerateAsync(string moduleCode)
       {
           var year = DateTime.UtcNow.Year;
           var prefix = $"{moduleCode}-{year}-";

           var countThisYear = await _db.Submissions
               .Where(s => s.ModuleCode == moduleCode && s.ReferenceNo.StartsWith(prefix))
               .CountAsync();

           var nextNumber = countThisYear + 1;

           return $"{prefix}{nextNumber:D4}";
       }
   }
   ```

   > **Kenapa kira `CountAsync()` bagi tahun semasa, bukan `MaxAsync(Id)` global?** Format nombor rujukan mesti **reset setiap tahun** (`LD-2026-0001`, kemudian `LD-2027-0001` bermula semula, bukan bersambung `LD-2027-0532`). Menapis mengikut `prefix` (yang sudah mengandungi tahun) sebelum kira memastikan setiap tahun mula dari `0001` semula. `{nextNumber:D4}` ialah format specifier C# — pad nombor kepada 4 digit dengan sifar hadapan (`7` → `"0007"`).

   > **Nota concurrency (perbincangan lanjutan, bukan wajib diselesaikan hari ini):** Pendekatan `Count + 1` ini boleh berlaku *race condition* jika dua permohonan disubmit **serentak** dalam saat yang sama (kedua-dua kira `count = 5`, kedua-dua jana `0006`). Untuk latihan, risiko ini sangat rendah (satu kelas, satu pelayan). Dalam pengeluaran sebenar, ini biasanya diselesaikan dengan jadual kaunter berasingan yang dikemas kini dalam satu transaksi terkunci, atau lajur `IDENTITY`/`SEQUENCE` pangkalan data.

3. Daftarkan servis dalam `Program.cs`:

   ```csharp
   builder.Services.AddScoped<IReferenceNumberService, SequentialReferenceNumberService>();
   ```

✅ **Semakan:** `dotnet build` berjaya; anda faham kenapa `prefix` termasuk tahun, dan kenapa nombor reset setiap tahun.

---

## Latihan 4 — `IAuditLogService`

**Objektif:** Sentralisasikan penulisan `AuditLog` supaya semua tindakan penting (submit, approve, reject) direkodkan secara konsisten.

1. Cipta fail `Services/IAuditLogService.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Services;

   public interface IAuditLogService
   {
       Task RecordAsync(int submissionId, string action, string? remarks = null);
   }
   ```

2. Cipta fail `Services/AuditLogService.cs`:

   ```csharp
   using Microsoft.AspNetCore.Identity;
   using Nres.Onboarding.Web.Data;
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.Services;

   public class AuditLogService : IAuditLogService
   {
       private readonly ApplicationDbContext _db;
       private readonly IHttpContextAccessor _httpContextAccessor;
       private readonly UserManager<IdentityUser> _userManager;

       public AuditLogService(
           ApplicationDbContext db,
           IHttpContextAccessor httpContextAccessor,
           UserManager<IdentityUser> userManager)
       {
           _db = db;
           _httpContextAccessor = httpContextAccessor;
           _userManager = userManager;
       }

       public async Task RecordAsync(int submissionId, string action, string? remarks = null)
       {
           var user = _httpContextAccessor.HttpContext?.User;
           var actorUserId = user is null ? "system" : (_userManager.GetUserId(user) ?? "unknown");

           var log = new AuditLog
           {
               SubmissionId = submissionId,
               ActorUserId = actorUserId,
               Action = action,
               Remarks = remarks,
               CreatedAt = DateTime.UtcNow
           };

           _db.AuditLogs.Add(log);
           await _db.SaveChangesAsync();
       }
   }
   ```

   > **Kenapa `IHttpContextAccessor` diperlukan?** Servis (`AuditLogService`) bukan `Controller` — ia tidak mempunyai akses terus kepada `User` semasa. `IHttpContextAccessor` membenarkan mana-mana servis yang didaftar sebagai `Scoped`/`Transient` mencapai `HttpContext` permintaan semasa, termasuk `HttpContext.User` (pengguna log masuk).

3. Daftarkan `IHttpContextAccessor` dan `IAuditLogService` dalam `Program.cs` (letak berdekatan pendaftaran servis lain):

   ```csharp
   builder.Services.AddHttpContextAccessor();
   builder.Services.AddScoped<IAuditLogService, AuditLogService>();
   ```

✅ **Semakan:** `dotnet build` berjaya; ketiga-tiga servis (`IFileStorageService`, `IReferenceNumberService`, `IAuditLogService`) kini didaftar dalam `Program.cs`.

---

## Latihan 5 — Action `Submit`

**Objektif:** Tambah action yang mengesahkan permohonan **lengkap sepenuhnya**, menjana nombor rujukan, menukar status kepada `Submitted`, dan merekod audit log.

1. Buka `Controllers/OfficerReportingController.cs`. Tambah `using Nres.Onboarding.Web.Services;` (jika belum), dan suntik dua servis baharu melalui constructor:

   ```csharp
   private readonly IReferenceNumberService _referenceNumberService;
   private readonly IAuditLogService _auditLogService;

   public OfficerReportingController(
       ApplicationDbContext db,
       UserManager<IdentityUser> userManager,
       IFileStorageService fileStorageService,
       IReferenceNumberService referenceNumberService,
       IAuditLogService auditLogService)
   {
       _db = db;
       _userManager = userManager;
       _fileStorageService = fileStorageService;
       _referenceNumberService = referenceNumberService;
       _auditLogService = auditLogService;
   }
   ```

2. Tambah kaedah pembantu (private) untuk pengesahan penuh, letak di bawah sekali kelas:

   ```csharp
   private static List<string> ValidateForSubmission(OfficerReportingApplication application)
   {
       var errors = new List<string>();

       if (string.IsNullOrWhiteSpace(application.FullName)) errors.Add("Nama penuh wajib diisi.");
       if (string.IsNullOrWhiteSpace(application.IdentityNo)) errors.Add("Nombor kad pengenalan wajib diisi.");
       if (string.IsNullOrWhiteSpace(application.Email)) errors.Add("Emel wajib diisi.");
       if (string.IsNullOrWhiteSpace(application.Phone)) errors.Add("Nombor telefon wajib diisi.");
       if (string.IsNullOrWhiteSpace(application.Department)) errors.Add("Jabatan wajib dipilih.");
       if (string.IsNullOrWhiteSpace(application.Position)) errors.Add("Jawatan wajib diisi.");
       if (string.IsNullOrWhiteSpace(application.Grade)) errors.Add("Gred wajib dipilih.");
       if (application.ReportingDate == default) errors.Add("Tarikh lapor diri wajib diisi.");

       return errors;
   }
   ```

   > **Kenapa pengesahan manual di sini, bukan `ModelState`?** `Submit` tidak menerima borang baharu daripada pengguna — ia bertindak ke atas rekod **sedia ada** dalam pangkalan data (dihantar sebagai `id` sahaja). Kita sahkan entiti yang sudah tersimpan, bukan data borang baharu, jadi `ModelState` (yang bekerja untuk model binding borang) tidak relevan di sini.

3. Tambah action `Submit`:

   ```csharp
   [HttpPost]
   [ValidateAntiForgeryToken]
   public async Task<IActionResult> Submit(int id)
   {
       var application = await _db.OfficerReportingApplications
           .Include(o => o.Submission)
               .ThenInclude(s => s.Attachments)
           .FirstOrDefaultAsync(o => o.Id == id);

       if (application is null)
       {
           return NotFound();
       }

       if (application.Submission.Status != SubmissionStatus.Draft)
       {
           TempData["StatusMessage"] = "Permohonan ini sudah dihantar.";
           return RedirectToAction(nameof(Details), new { id });
       }

       var errors = ValidateForSubmission(application);

       if (!application.Submission.Attachments.Any())
       {
           errors.Add("Sila muat naik sekurang-kurangnya satu lampiran sokongan.");
       }

       if (errors.Count > 0)
       {
           TempData["StatusMessage"] = "Permohonan tidak lengkap: " + string.Join(" ", errors);
           return RedirectToAction(nameof(Details), new { id });
       }

       application.Submission.ReferenceNo = await _referenceNumberService.GenerateAsync("LD");
       application.Submission.Status = SubmissionStatus.Submitted;
       application.Submission.SubmittedAt = DateTime.UtcNow;

       await _db.SaveChangesAsync();

       await _auditLogService.RecordAsync(
           application.SubmissionId,
           "Submitted",
           $"Nombor rujukan: {application.Submission.ReferenceNo}");

       TempData["StatusMessage"] =
           $"Lapor Diri berjaya dihantar. Nombor rujukan: {application.Submission.ReferenceNo}";

       return RedirectToAction(nameof(Details), new { id });
   }
   ```

4. Kemas kini `Views/OfficerReporting/Details.cshtml` supaya paparkan `ReferenceNo` (jika sudah wujud) berhampiran `Status` — tambah dalam `<dl class="row">`:

   ```cshtml
   <dt class="col-sm-3">Nombor Rujukan</dt>
   <dd class="col-sm-9">@(string.IsNullOrEmpty(Model.Submission.ReferenceNo) ? "— (belum dihantar)" : Model.Submission.ReferenceNo)</dd>
   ```

5. Jalankan aplikasi. Cuba **Hantar Permohonan** pada draf **tanpa** lampiran — sahkan mesej ralat "Sila muat naik sekurang-kurangnya satu lampiran sokongan." muncul. Muat naik satu lampiran, cuba semula — sahkan submit berjaya dan `ReferenceNo` dipaparkan format `LD-2026-000N`.

✅ **Semakan:** Submit ditolak jika data tidak lengkap **atau** tiada lampiran; submit berjaya menjana `ReferenceNo` betul, menukar status kepada `Submitted`, dan `AuditLogs` mempunyai rekod baharu (sahkan dengan `sqlite3 App_Data/nres.db "SELECT * FROM AuditLogs;"` jika `sqlite3` CLI tersedia).

---

## Latihan 6 — Peranan `HrAdmin` & Data Seed

**Objektif:** Cipta peranan `HrAdmin` dan satu akaun ujian supaya semakan HR boleh diuji tanpa mendaftar akaun baharu setiap kali.

1. Cipta fail `Data/SeedData.cs`:

   ```csharp
   using Microsoft.AspNetCore.Identity;

   namespace Nres.Onboarding.Web.Data;

   public static class SeedData
   {
       public static async Task EnsureRolesAndAdminAsync(IServiceProvider services)
       {
           var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
           var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

           string[] roles = { "Applicant", "HrAdmin", "SystemAdmin" };

           foreach (var role in roles)
           {
               if (!await roleManager.RoleExistsAsync(role))
               {
                   await roleManager.CreateAsync(new IdentityRole(role));
               }
           }

           const string hrAdminEmail = "hradmin@nres.training";

           var hrAdminUser = await userManager.FindByEmailAsync(hrAdminEmail);

           if (hrAdminUser is null)
           {
               hrAdminUser = new IdentityUser
               {
                   UserName = hrAdminEmail,
                   Email = hrAdminEmail,
                   EmailConfirmed = true
               };

               // Kata laluan LATIHAN sahaja — jangan guna corak ini dalam pengeluaran.
               var result = await userManager.CreateAsync(hrAdminUser, "LatihanNres@2026");

               if (result.Succeeded)
               {
                   await userManager.AddToRoleAsync(hrAdminUser, "HrAdmin");
               }
           }
       }
   }
   ```

2. Buka `Program.cs` dan panggil kaedah seed **selepas** `var app = builder.Build();` tetapi **sebelum** `app.Run()`:

   ```csharp
   using (var scope = app.Services.CreateScope())
   {
       await SeedData.EnsureRolesAndAdminAsync(scope.ServiceProvider);
   }
   ```

   > **Kenapa `CreateScope()` diperlukan di sini?** `RoleManager`/`UserManager` didaftar sebagai `Scoped` (bukan `Singleton`) oleh ASP.NET Core Identity. `Program.cs` di luar mana-mana permintaan HTTP tidak mempunyai skop sedia ada — `CreateScope()` mencipta satu skop sementara khas untuk operasi seed semasa aplikasi bermula.

3. Jalankan aplikasi (`dotnet run`) sekali untuk cetuskan seed. Sahkan log masuk sebagai `hradmin@nres.training` / `LatihanNres@2026` berjaya melalui halaman log masuk Identity lalai.

✅ **Semakan:** Akaun `hradmin@nres.training` boleh log masuk, dan ia mempunyai peranan `HrAdmin` (boleh disahkan dengan menyemak jadual `AspNetUserRoles` melalui `sqlite3`, atau tunggu Latihan 7 untuk uji akses halaman semakan).

---

## Latihan 7 — Halaman Semakan HR: Approve & Reject

**Objektif:** Bina controller & view khusus HR admin untuk menyemak permohonan `Submitted`, dengan approve dan reject (wajib sebab).

1. Cipta fail `Controllers/OfficerReportingReviewController.cs`:

   ```csharp
   using Microsoft.AspNetCore.Authorization;
   using Microsoft.AspNetCore.Mvc;
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Data;
   using Nres.Onboarding.Web.Models;
   using Nres.Onboarding.Web.Services;

   namespace Nres.Onboarding.Web.Controllers;

   [Authorize(Roles = "HrAdmin")]
   public class OfficerReportingReviewController : Controller
   {
       private readonly ApplicationDbContext _db;
       private readonly IAuditLogService _auditLogService;

       public OfficerReportingReviewController(ApplicationDbContext db, IAuditLogService auditLogService)
       {
           _db = db;
           _auditLogService = auditLogService;
       }

       // GET: /OfficerReportingReview
       public async Task<IActionResult> Index(SubmissionStatus? status)
       {
           var query = _db.OfficerReportingApplications
               .Include(o => o.Submission)
               .Where(o => o.Submission.Status != SubmissionStatus.Draft)
               .AsQueryable();

           if (status.HasValue)
           {
               query = query.Where(o => o.Submission.Status == status.Value);
           }

           var items = await query
               .OrderByDescending(o => o.Submission.SubmittedAt)
               .ToListAsync();

           ViewData["SelectedStatus"] = status;

           return View(items);
       }

       // GET: /OfficerReportingReview/Details/5
       public async Task<IActionResult> Details(int id)
       {
           var application = await _db.OfficerReportingApplications
               .Include(o => o.Submission)
                   .ThenInclude(s => s.Attachments)
               .FirstOrDefaultAsync(o => o.Id == id);

           if (application is null)
           {
               return NotFound();
           }

           var auditLogs = await _db.AuditLogs
               .Where(a => a.SubmissionId == application.SubmissionId)
               .OrderBy(a => a.CreatedAt)
               .ToListAsync();

           ViewData["AuditLogs"] = auditLogs;

           return View(application);
       }

       // POST: /OfficerReportingReview/Approve/5
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Approve(int id)
       {
           var application = await _db.OfficerReportingApplications
               .Include(o => o.Submission)
               .FirstOrDefaultAsync(o => o.Id == id);

           if (application is null)
           {
               return NotFound();
           }

           if (application.Submission.Status != SubmissionStatus.Submitted)
           {
               TempData["StatusMessage"] = "Hanya permohonan berstatus Submitted boleh diluluskan.";
               return RedirectToAction(nameof(Details), new { id });
           }

           application.Submission.Status = SubmissionStatus.Completed;
           application.Submission.CompletedAt = DateTime.UtcNow;

           await _db.SaveChangesAsync();

           await _auditLogService.RecordAsync(
               application.SubmissionId, "Approved", "Diluluskan oleh HR Admin.");

           TempData["StatusMessage"] = "Lapor Diri berjaya diluluskan.";

           return RedirectToAction(nameof(Details), new { id });
       }

       // POST: /OfficerReportingReview/Reject/5
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Reject(int id, string rejectionReason)
       {
           if (string.IsNullOrWhiteSpace(rejectionReason))
           {
               TempData["StatusMessage"] = "Sebab penolakan wajib diisi.";
               return RedirectToAction(nameof(Details), new { id });
           }

           var application = await _db.OfficerReportingApplications
               .Include(o => o.Submission)
               .FirstOrDefaultAsync(o => o.Id == id);

           if (application is null)
           {
               return NotFound();
           }

           if (application.Submission.Status != SubmissionStatus.Submitted)
           {
               TempData["StatusMessage"] = "Hanya permohonan berstatus Submitted boleh ditolak.";
               return RedirectToAction(nameof(Details), new { id });
           }

           application.Submission.Status = SubmissionStatus.Rejected;

           await _db.SaveChangesAsync();

           await _auditLogService.RecordAsync(application.SubmissionId, "Rejected", rejectionReason);

           TempData["StatusMessage"] = "Lapor Diri telah ditolak.";

           return RedirectToAction(nameof(Details), new { id });
       }
   }
   ```

   > **Kenapa Approve tukar status kepada `Completed`, bukan `AdminApproved`?** Enum `SubmissionStatus` kongsi menyediakan **kedua-dua** peringkat supaya modul dengan rantaian kelulusan berbilang langkah (contohnya Modul 3 — ID/AD/Email, Hari 8: `Supervisor` **kemudian** `IctAdmin`) boleh guna `SupervisorApproved` sebagai peringkat pertengahan. Lapor Diri hanya mempunyai **satu** peringkat semakan (`HrAdmin`) — sebaik ia diluluskan, permohonan **selesai sepenuhnya**, jadi `Completed` lebih tepat berbanding `AdminApproved` (yang tersirat masih ada langkah selepasnya).

   > **Kenapa `Reject` semak `string.IsNullOrWhiteSpace(rejectionReason)` sebelum apa-apa lagi?** Ini kuatkuasa peraturan "penolakan wajib sebab" (rejection requires reason) daripada panduan sumber — jika sebab kosong, kita **tidak** teruskan ke pangkalan data langsung, walaupun `id` sah.

2. Cipta fail `Views/OfficerReportingReview/Index.cshtml`:

   ```cshtml
   @using Nres.Onboarding.Web.Models
   @model List<Nres.Onboarding.Web.Models.OfficerReportingApplication>

   @{
       ViewData["Title"] = "Semakan HR — Lapor Diri";
       var selectedStatus = ViewData["SelectedStatus"] as SubmissionStatus?;
   }

   <h1>Semakan HR — Lapor Diri</h1>

   <form method="get" class="mb-3 d-flex align-items-center gap-2">
       <label for="status" class="form-label mb-0">Tapis Status:</label>
       <select name="status" id="status" class="form-select w-auto" onchange="this.form.submit()">
           <option value="">Semua</option>
           @foreach (SubmissionStatus s in Enum.GetValues(typeof(SubmissionStatus)))
           {
               if (s == SubmissionStatus.Draft) { continue; }
               <option value="@s" selected="@(selectedStatus == s)">@s</option>
           }
       </select>
   </form>

   <table class="table table-striped">
       <thead>
           <tr>
               <th>Nombor Rujukan</th>
               <th>Nama Penuh</th>
               <th>Jabatan</th>
               <th>Status</th>
               <th>Tarikh Hantar</th>
               <th></th>
           </tr>
       </thead>
       <tbody>
           @foreach (var item in Model)
           {
               <tr>
                   <td>@item.Submission.ReferenceNo</td>
                   <td>@item.FullName</td>
                   <td>@item.Department</td>
                   <td>@item.Submission.Status</td>
                   <td>@(item.Submission.SubmittedAt?.ToString("dd/MM/yyyy HH:mm") ?? "—")</td>
                   <td>
                       <a asp-action="Details" asp-route-id="@item.Id" class="btn btn-sm btn-outline-secondary">Semak</a>
                   </td>
               </tr>
           }
       </tbody>
   </table>

   @if (!Model.Any())
   {
       <p class="text-muted">Tiada permohonan untuk disemak buat masa ini.</p>
   }
   ```

3. Cipta fail `Views/OfficerReportingReview/Details.cshtml`:

   ```cshtml
   @using Nres.Onboarding.Web.Models
   @model Nres.Onboarding.Web.Models.OfficerReportingApplication

   @{
       ViewData["Title"] = "Semakan HR — Butiran";
       var auditLogs = ViewData["AuditLogs"] as List<AuditLog>;
   }

   <h1>Semakan HR — @Model.Submission.ReferenceNo</h1>

   @if (TempData["StatusMessage"] is string statusMessage)
   {
       <div class="alert alert-info">@statusMessage</div>
   }

   <dl class="row">
       <dt class="col-sm-3">Status</dt>
       <dd class="col-sm-9">@Model.Submission.Status</dd>

       <dt class="col-sm-3">Nama Penuh</dt>
       <dd class="col-sm-9">@Model.FullName</dd>

       <dt class="col-sm-3">Nombor Kad Pengenalan</dt>
       <dd class="col-sm-9">@Model.IdentityNo</dd>

       <dt class="col-sm-3">Emel</dt>
       <dd class="col-sm-9">@Model.Email</dd>

       <dt class="col-sm-3">Telefon</dt>
       <dd class="col-sm-9">@Model.Phone</dd>

       <dt class="col-sm-3">Jabatan</dt>
       <dd class="col-sm-9">@Model.Department</dd>

       <dt class="col-sm-3">Jawatan / Gred</dt>
       <dd class="col-sm-9">@Model.Position / @Model.Grade</dd>

       <dt class="col-sm-3">Tarikh Lapor Diri</dt>
       <dd class="col-sm-9">@Model.ReportingDate.ToString("dd/MM/yyyy")</dd>
   </dl>

   <h2>Lampiran</h2>
   <ul class="list-group mb-3">
       @foreach (var attachment in Model.Submission.Attachments)
       {
           <li class="list-group-item">
               <a asp-controller="OfficerReporting" asp-action="Download" asp-route-attachmentId="@attachment.Id">
                   @attachment.OriginalFileName
               </a>
           </li>
       }
   </ul>

   <h2>Sejarah Audit</h2>
   <ul class="list-group mb-4">
       @foreach (var log in auditLogs ?? new List<AuditLog>())
       {
           <li class="list-group-item">
               <strong>@log.Action</strong> — @log.CreatedAt.ToString("dd/MM/yyyy HH:mm")
               @if (!string.IsNullOrWhiteSpace(log.Remarks))
               {
                   <div class="text-muted">@log.Remarks</div>
               }
           </li>
       }
   </ul>

   @if (Model.Submission.Status == SubmissionStatus.Submitted)
   {
       <div class="d-flex gap-2 mb-3">
           <form asp-action="Approve" asp-route-id="@Model.Id" method="post">
               @Html.AntiForgeryToken()
               <button type="submit" class="btn btn-success">Luluskan</button>
           </form>
       </div>

       <form asp-action="Reject" asp-route-id="@Model.Id" method="post" class="mb-3">
           @Html.AntiForgeryToken()
           <div class="mb-2">
               <label for="rejectionReason" class="form-label">Sebab Penolakan (wajib)</label>
               <textarea name="rejectionReason" id="rejectionReason" class="form-control" rows="3" required></textarea>
           </div>
           <button type="submit" class="btn btn-danger">Tolak</button>
       </form>
   }

   <a asp-action="Index" class="btn btn-link">Kembali ke Senarai Semakan</a>
   ```

   > **Kenapa `required` pada `<textarea>` di sisi klien, sedangkan kita sudah semak `IsNullOrWhiteSpace` di sisi pelayan?** Dua lapisan ini melayani tujuan berbeza — `required` memberi maklum balas **segera** (elak hantaran borang kosong terus-terusan), manakala semakan pelayan ialah **pertahanan sebenar** yang tidak boleh dipintas. Ingat prinsip ini daripada Hari 2: validation sisi klien untuk pengalaman pengguna, validation sisi pelayan untuk keselamatan.

4. (Pilihan) Tambah pautan "Semakan HR" ke `Views/Shared/_Layout.cshtml`, dipaparkan hanya untuk pengguna dalam peranan `HrAdmin`:

   ```cshtml
   @if (User.IsInRole("HrAdmin"))
   {
       <li class="nav-item">
           <a class="nav-link text-dark" asp-controller="OfficerReportingReview" asp-action="Index">Semakan HR</a>
       </li>
   }
   ```

5. Log keluar daripada akaun `Applicant` biasa, log masuk sebagai `hradmin@nres.training` (kata laluan `LatihanNres@2026`), navigasi ke "Semakan HR", dan uji:
   - Approve satu permohonan `Submitted` — sahkan status bertukar `Completed` dan rekod audit "Approved" muncul.
   - Reject satu permohonan lain **tanpa** isi sebab — sahkan ditolak dengan mesej ralat.
   - Reject dengan sebab diisi — sahkan status bertukar `Rejected` dan sebab tersimpan dalam audit log.

✅ **Semakan:** Pengguna biasa (`Applicant`) menerima ralat capaian ditolak (redirect ke log masuk atau `403`) bila cuba akses `/OfficerReportingReview`; `hradmin@nres.training` boleh akses, approve, dan reject (dengan sebab wajib); sejarah audit memaparkan semua tindakan (Submitted, Approved/Rejected) secara kronologi.

---

## Latihan 8 — Uji Aliran Penuh Modul 1

**Objektif:** Sahkan keseluruhan Modul 1 (Lapor Diri) berfungsi hujung-ke-hujung, menyerupai Langkah 1–2 dalam "Manual Test Script" [`../JADUAL.md`](../JADUAL.md) Hari 15.

Sebagai `Applicant`:

1. Cipta draf Lapor Diri baharu (Hari 2).
2. Muat naik sekurang-kurangnya satu lampiran.
3. Hantar permohonan — sahkan nombor rujukan `LD-2026-000N` dipaparkan.

Sebagai `HrAdmin`:

4. Buka "Semakan HR", cari permohonan tadi mengikut nombor rujukan.
5. Semak butiran, lampiran, dan sejarah audit.
6. Luluskan **atau** tolak (dengan sebab) permohonan tersebut.

Kembali sebagai `Applicant`:

7. Buka semula Details permohonan tersebut — sahkan status terkini (`Completed`/`Rejected`) dan sebarang catatan penolakan (jika ditolak) kelihatan.

✅ **Semakan akhir:** Aliran penuh `Draft → (lampiran) → Submitted → (semakan HR) → Completed/Rejected` berfungsi tanpa ralat, dan setiap langkah penting direkod dalam `AuditLogs`.

---

## Rujukan Fail Sebenar

| Fail anda (lab) | Fail rujukan (projek sebenar) |
|------------------|-------------------------------|
| `Services/IFileStorageService.cs`, `LocalFileStorageService.cs` | `projek/Nres.Onboarding.Web/Services/` |
| `Services/IReferenceNumberService.cs`, `SequentialReferenceNumberService.cs` | `projek/Nres.Onboarding.Web/Services/` |
| `Services/IAuditLogService.cs`, `AuditLogService.cs` | `projek/Nres.Onboarding.Web/Services/` |
| `Data/SeedData.cs` | `projek/Nres.Onboarding.Web/Data/SeedData.cs` |
| `Controllers/OfficerReportingReviewController.cs` | `projek/Nres.Onboarding.Web/Controllers/OfficerReportingReviewController.cs` |
| `Views/OfficerReportingReview/*.cshtml` | `projek/Nres.Onboarding.Web/Views/OfficerReportingReview/` |

---

## Cabaran (Pilihan)

1. **Sekat submit dua kali serentak** — Guna `await using var transaction = await _db.Database.BeginTransactionAsync();` dalam `Submit` untuk pastikan jana nombor rujukan dan simpan status berlaku dalam **satu** transaksi atomik (rujuk pratonton "Transaction Example" untuk Hari 14 dalam panduan sumber).
2. **Had bilangan lampiran** — Tambah sekatan maksimum 3 lampiran setiap permohonan Lapor Diri; papar mesej ralat jelas jika melebihi.
3. **Halaman "Permohonan Saya"** — Kemas kini `OfficerReportingController.Index` supaya papar lencana (badge) warna berbeza mengikut `SubmissionStatus` (cth. kuning untuk `Draft`, biru untuk `Submitted`, hijau untuk `Completed`, merah untuk `Rejected`).

---

Nota penceramah (pemasaan sesi, silap biasa, soalan perbincangan, deliverable akhir hari): [`../nota-penceramah.md`](../nota-penceramah.md).
