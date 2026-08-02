# Lab Hari 2 — Lapor Diri: Borang & Validation

Lab ini mengiringi [`../README.md`](../README.md) Hari 2. Ikut latihan **secara berurutan**. Rujuk projek rujukan penuh di [`../../projek/`](../../projek/) untuk banding kod anda selepas cuba sendiri dahulu.

> **Sebelum mula:** Pastikan projek `Nres.Onboarding.Web` daripada Hari 1 masih boleh `dotnet run` tanpa ralat, dan migration `InitialShared` sudah dijalankan (`App_Data/nres.db` wujud). Kita **teruskan** projek yang sama — jangan cipta projek baharu.

---

## Latihan 1 — Entiti `OfficerReportingApplication`

**Objektif:** Tulis entiti khusus Modul 1 (Lapor Diri), dipautkan ke `Submission` induk daripada Hari 1.

1. Cipta fail `Models/OfficerReportingApplication.cs`:

   ```csharp
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.Models;

   public class OfficerReportingApplication
   {
       public int Id { get; set; }

       public int SubmissionId { get; set; }

       public Submission Submission { get; set; } = null!;

       public string FullName { get; set; } = string.Empty;

       public string IdentityNo { get; set; } = string.Empty;

       public string Email { get; set; } = string.Empty;

       public string Phone { get; set; } = string.Empty;

       public string Department { get; set; } = string.Empty;

       public string Position { get; set; } = string.Empty;

       public string Grade { get; set; } = string.Empty;

       public DateTime ReportingDate { get; set; }

       public string? PreviousAgency { get; set; }

       public string? EmergencyContact { get; set; }
   }
   ```

   > **Nota:** `SubmissionId` + `Submission` di sini ialah corak yang sama seperti `Attachment`/`AuditLog` Hari 1 — satu `Submission` induk, satu jadual detail. `PreviousAgency` dan `EmergencyContact` bertanda `string?` (nullable) kerana jadual "Suggested Fields" menandakan kedua-dua ini **Tidak wajib**.

2. Buka `Data/ApplicationDbContext.cs` dan tambah `DbSet` baharu:

   ```csharp
   public DbSet<OfficerReportingApplication> OfficerReportingApplications => Set<OfficerReportingApplication>();
   ```

3. Dalam `OnModelCreating`, tambah konfigurasi hubungan (relationship) untuk entiti baharu ini — letak selepas konfigurasi `AuditLog` sedia ada:

   ```csharp
   builder.Entity<OfficerReportingApplication>(entity =>
   {
       entity.Property(o => o.FullName).HasMaxLength(200);
       entity.Property(o => o.IdentityNo).HasMaxLength(20);
       entity.Property(o => o.Email).HasMaxLength(256);
       entity.Property(o => o.Phone).HasMaxLength(30);
       entity.Property(o => o.Department).HasMaxLength(100);
       entity.Property(o => o.Position).HasMaxLength(100);
       entity.Property(o => o.Grade).HasMaxLength(20);

       entity.HasOne(o => o.Submission)
           .WithOne()
           .HasForeignKey<OfficerReportingApplication>(o => o.SubmissionId)
           .OnDelete(DeleteBehavior.Cascade);

       entity.HasIndex(o => o.SubmissionId).IsUnique();
   });
   ```

   > **Kenapa `WithOne()` (satu-ke-satu), bukan `WithMany()` seperti `Attachment`?** Setiap `Submission` untuk Modul 1 hanya mempunyai **satu** `OfficerReportingApplication` (satu permohonan Lapor Diri = satu rekod detail). Bandingkan dengan `Attachment`, di mana satu `Submission` boleh ada **banyak** lampiran (`WithMany`).

4. Jana migration baharu dan kemas kini pangkalan data:

   ```bash
   dotnet ef migrations add AddOfficerReporting
   dotnet ef database update
   ```

✅ **Semakan:** `dotnet build` berjaya, migration `AddOfficerReporting` dijana dalam `Migrations/`, dan `dotnet ef database update` berjaya tanpa ralat. Jadual `OfficerReportingApplications` wujud dalam `App_Data/nres.db`.

---

## Latihan 2 — View Model: Create & Edit

**Objektif:** Tulis `OfficerReportingCreateViewModel` (dan `OfficerReportingEditViewModel`) lengkap dengan `DataAnnotations`, berasingan sepenuhnya daripada entiti.

1. Cipta fail `ViewModels/OfficerReportingCreateViewModel.cs`:

   ```csharp
   using System.ComponentModel.DataAnnotations;

   namespace Nres.Onboarding.Web.ViewModels;

   public class OfficerReportingCreateViewModel
   {
       [Required(ErrorMessage = "Nama penuh wajib diisi.")]
       [StringLength(200, ErrorMessage = "Nama penuh maksimum 200 aksara.")]
       [Display(Name = "Nama Penuh")]
       public string FullName { get; set; } = string.Empty;

       [Required(ErrorMessage = "Nombor kad pengenalan wajib diisi.")]
       [StringLength(20, ErrorMessage = "Nombor kad pengenalan maksimum 20 aksara.")]
       [Display(Name = "Nombor Kad Pengenalan (IC)")]
       public string IdentityNo { get; set; } = string.Empty;

       [Required(ErrorMessage = "Emel wajib diisi.")]
       [EmailAddress(ErrorMessage = "Format emel tidak sah.")]
       [Display(Name = "Emel")]
       public string Email { get; set; } = string.Empty;

       [Required(ErrorMessage = "Nombor telefon wajib diisi.")]
       [StringLength(30, ErrorMessage = "Nombor telefon maksimum 30 aksara.")]
       [Display(Name = "Nombor Telefon")]
       public string Phone { get; set; } = string.Empty;

       [Required(ErrorMessage = "Sila pilih jabatan.")]
       [Display(Name = "Jabatan")]
       public string Department { get; set; } = string.Empty;

       [Required(ErrorMessage = "Jawatan wajib diisi.")]
       [StringLength(100)]
       [Display(Name = "Jawatan")]
       public string Position { get; set; } = string.Empty;

       [Required(ErrorMessage = "Sila pilih gred.")]
       [Display(Name = "Gred")]
       public string Grade { get; set; } = string.Empty;

       [Required(ErrorMessage = "Tarikh lapor diri wajib diisi.")]
       [DataType(DataType.Date)]
       [Display(Name = "Tarikh Lapor Diri")]
       public DateTime ReportingDate { get; set; } = DateTime.Today;

       [StringLength(200)]
       [Display(Name = "Agensi Terdahulu (jika berkaitan)")]
       public string? PreviousAgency { get; set; }

       [StringLength(200)]
       [Display(Name = "Kenalan Kecemasan (jika berkaitan)")]
       public string? EmergencyContact { get; set; }

       // Senarai pilihan dropdown — dipenuhi oleh controller, bukan pengguna.
       public List<string> DepartmentOptions { get; set; } = new();

       public List<string> GradeOptions { get; set; } = new();
   }
   ```

   > **Kenapa `DepartmentOptions`/`GradeOptions` letak dalam view model?** Razor view **tidak sepatutnya** query pangkalan data sendiri — itu tanggungjawab controller. View model menjadi "bekas" yang membawa **kedua-dua** data yang pengguna isi (`Department`) **dan** senarai pilihan untuk dropdown (`DepartmentOptions`) dari controller ke view dalam satu perjalanan.

2. Cipta fail `ViewModels/OfficerReportingEditViewModel.cs` — sama seperti Create, **tambah** `Id`:

   ```csharp
   using System.ComponentModel.DataAnnotations;

   namespace Nres.Onboarding.Web.ViewModels;

   public class OfficerReportingEditViewModel
   {
       public int Id { get; set; }

       [Required(ErrorMessage = "Nama penuh wajib diisi.")]
       [StringLength(200)]
       [Display(Name = "Nama Penuh")]
       public string FullName { get; set; } = string.Empty;

       [Required(ErrorMessage = "Nombor kad pengenalan wajib diisi.")]
       [StringLength(20)]
       [Display(Name = "Nombor Kad Pengenalan (IC)")]
       public string IdentityNo { get; set; } = string.Empty;

       [Required(ErrorMessage = "Emel wajib diisi.")]
       [EmailAddress(ErrorMessage = "Format emel tidak sah.")]
       [Display(Name = "Emel")]
       public string Email { get; set; } = string.Empty;

       [Required(ErrorMessage = "Nombor telefon wajib diisi.")]
       [StringLength(30)]
       [Display(Name = "Nombor Telefon")]
       public string Phone { get; set; } = string.Empty;

       [Required(ErrorMessage = "Sila pilih jabatan.")]
       [Display(Name = "Jabatan")]
       public string Department { get; set; } = string.Empty;

       [Required(ErrorMessage = "Jawatan wajib diisi.")]
       [StringLength(100)]
       [Display(Name = "Jawatan")]
       public string Position { get; set; } = string.Empty;

       [Required(ErrorMessage = "Sila pilih gred.")]
       [Display(Name = "Gred")]
       public string Grade { get; set; } = string.Empty;

       [Required(ErrorMessage = "Tarikh lapor diri wajib diisi.")]
       [DataType(DataType.Date)]
       [Display(Name = "Tarikh Lapor Diri")]
       public DateTime ReportingDate { get; set; }

       [StringLength(200)]
       [Display(Name = "Agensi Terdahulu (jika berkaitan)")]
       public string? PreviousAgency { get; set; }

       [StringLength(200)]
       [Display(Name = "Kenalan Kecemasan (jika berkaitan)")]
       public string? EmergencyContact { get; set; }

       public List<string> DepartmentOptions { get; set; } = new();

       public List<string> GradeOptions { get; set; } = new();
   }
   ```

✅ **Semakan:** Kedua-dua fail `ViewModels/OfficerReportingCreateViewModel.cs` dan `ViewModels/OfficerReportingEditViewModel.cs` wujud, `dotnet build` berjaya, dan **tiada** medan `Id`/`SubmissionId` boleh diisi pengguna dalam `OfficerReportingCreateViewModel` (hanya `Edit` ada `Id`, kerana ia perlu tahu rekod **mana** yang dikemas kini).

---

## Latihan 3 — `OfficerReportingController`

**Objektif:** Bina controller dengan action `Index`, `Create` (GET/POST), `Edit` (GET/POST), `Details`.

1. Cipta fail `Controllers/OfficerReportingController.cs`:

   ```csharp
   using Microsoft.AspNetCore.Authorization;
   using Microsoft.AspNetCore.Identity;
   using Microsoft.AspNetCore.Mvc;
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Data;
   using Nres.Onboarding.Web.Models;
   using Nres.Onboarding.Web.ViewModels;

   namespace Nres.Onboarding.Web.Controllers;

   [Authorize]
   public class OfficerReportingController : Controller
   {
       private const string ModuleCode = "LD";

       private static readonly List<string> Departments = new()
       {
           "Bahagian Pentadbiran",
           "Bahagian Kewangan",
           "Bahagian Sumber Manusia",
           "Bahagian Teknologi Maklumat",
           "Bahagian Perhutanan"
       };

       private static readonly List<string> Grades = new()
       {
           "N19", "N22", "N27", "N32", "N38", "N41", "N44", "N48", "N52", "N54"
       };

       private readonly ApplicationDbContext _db;
       private readonly UserManager<IdentityUser> _userManager;

       public OfficerReportingController(ApplicationDbContext db, UserManager<IdentityUser> userManager)
       {
           _db = db;
           _userManager = userManager;
       }

       // GET: /OfficerReporting
       public async Task<IActionResult> Index()
       {
           var userId = _userManager.GetUserId(User);

           var items = await _db.OfficerReportingApplications
               .Include(o => o.Submission)
               .Where(o => o.Submission.ApplicantUserId == userId)
               .OrderByDescending(o => o.Submission.CreatedAt)
               .ToListAsync();

           return View(items);
       }

       // GET: /OfficerReporting/Create
       public IActionResult Create()
       {
           var model = new OfficerReportingCreateViewModel
           {
               DepartmentOptions = Departments,
               GradeOptions = Grades
           };

           return View(model);
       }

       // POST: /OfficerReporting/Create
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Create(OfficerReportingCreateViewModel model, string submitAction)
       {
           var isDraft = string.Equals(submitAction, "draft", StringComparison.OrdinalIgnoreCase);

           if (isDraft)
           {
               // Draf hanya perlukan nama — buang semakan medan lain buat sementara.
               ModelState.Clear();

               if (string.IsNullOrWhiteSpace(model.FullName))
               {
                   ModelState.AddModelError(nameof(model.FullName), "Nama penuh wajib diisi walaupun untuk draf.");
               }
           }

           if (!ModelState.IsValid)
           {
               model.DepartmentOptions = Departments;
               model.GradeOptions = Grades;
               return View(model);
           }

           var userId = _userManager.GetUserId(User) ?? string.Empty;

           var submission = new Submission
           {
               ModuleCode = ModuleCode,
               ApplicantUserId = userId,
               Status = SubmissionStatus.Draft,
               CreatedAt = DateTime.UtcNow
           };

           var application = new OfficerReportingApplication
           {
               Submission = submission,
               FullName = model.FullName,
               IdentityNo = model.IdentityNo,
               Email = model.Email,
               Phone = model.Phone,
               Department = model.Department,
               Position = model.Position,
               Grade = model.Grade,
               ReportingDate = model.ReportingDate,
               PreviousAgency = model.PreviousAgency,
               EmergencyContact = model.EmergencyContact
           };

           _db.OfficerReportingApplications.Add(application);
           await _db.SaveChangesAsync();

           TempData["StatusMessage"] = isDraft
               ? "Draf Lapor Diri berjaya disimpan."
               : "Lapor Diri berjaya dicipta.";

           return RedirectToAction(nameof(Details), new { id = application.Id });
       }

       // GET: /OfficerReporting/Edit/5
       public async Task<IActionResult> Edit(int id)
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
               TempData["StatusMessage"] = "Permohonan yang sudah dihantar tidak boleh disunting.";
               return RedirectToAction(nameof(Details), new { id });
           }

           var model = new OfficerReportingEditViewModel
           {
               Id = application.Id,
               FullName = application.FullName,
               IdentityNo = application.IdentityNo,
               Email = application.Email,
               Phone = application.Phone,
               Department = application.Department,
               Position = application.Position,
               Grade = application.Grade,
               ReportingDate = application.ReportingDate,
               PreviousAgency = application.PreviousAgency,
               EmergencyContact = application.EmergencyContact,
               DepartmentOptions = Departments,
               GradeOptions = Grades
           };

           return View(model);
       }

       // POST: /OfficerReporting/Edit/5
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Edit(int id, OfficerReportingEditViewModel model, string submitAction)
       {
           if (id != model.Id)
           {
               return BadRequest();
           }

           var isDraft = string.Equals(submitAction, "draft", StringComparison.OrdinalIgnoreCase);

           if (isDraft)
           {
               ModelState.Clear();

               if (string.IsNullOrWhiteSpace(model.FullName))
               {
                   ModelState.AddModelError(nameof(model.FullName), "Nama penuh wajib diisi walaupun untuk draf.");
               }
           }

           if (!ModelState.IsValid)
           {
               model.DepartmentOptions = Departments;
               model.GradeOptions = Grades;
               return View(model);
           }

           var application = await _db.OfficerReportingApplications
               .Include(o => o.Submission)
               .FirstOrDefaultAsync(o => o.Id == id);

           if (application is null)
           {
               return NotFound();
           }

           if (application.Submission.Status != SubmissionStatus.Draft)
           {
               TempData["StatusMessage"] = "Permohonan yang sudah dihantar tidak boleh disunting.";
               return RedirectToAction(nameof(Details), new { id });
           }

           application.FullName = model.FullName;
           application.IdentityNo = model.IdentityNo;
           application.Email = model.Email;
           application.Phone = model.Phone;
           application.Department = model.Department;
           application.Position = model.Position;
           application.Grade = model.Grade;
           application.ReportingDate = model.ReportingDate;
           application.PreviousAgency = model.PreviousAgency;
           application.EmergencyContact = model.EmergencyContact;

           await _db.SaveChangesAsync();

           TempData["StatusMessage"] = "Lapor Diri berjaya dikemas kini.";

           return RedirectToAction(nameof(Details), new { id });
       }

       // GET: /OfficerReporting/Details/5
       public async Task<IActionResult> Details(int id)
       {
           var application = await _db.OfficerReportingApplications
               .Include(o => o.Submission)
               .FirstOrDefaultAsync(o => o.Id == id);

           if (application is null)
           {
               return NotFound();
           }

           return View(application);
       }
   }
   ```

   > **Kenapa `submitAction` sebagai parameter, bukan dua action berasingan (`SaveDraft` / `Submit`)?** Satu borang HTML boleh menghantar butang yang berbeza (`<button name="submitAction" value="draft">` vs `<button name="submitAction" value="full">`) ke **satu** action yang sama. Ini elak pendua-an logik cipta rekod (kita hanya ada **satu** tempat menulis `_db.OfficerReportingApplications.Add(...)`), sambil membenarkan peraturan pengesahan berbeza bergantung butang yang ditekan.

   > **Kenapa semakan `Submission.Status != SubmissionStatus.Draft` dalam `Edit`?** Ini pratonton peraturan Hari 3: selepas `Submit`, rekod **terkunci** — pemohon tidak lagi boleh sunting kecuali admin buka semula. Kita letak sekatan ini awal supaya tabiat betul terbentuk sejak Hari 2, walaupun `Submit` sendiri belum wujud sehingga esok.

✅ **Semakan:** `dotnet build` berjaya. Anda faham kenapa setiap action wujud dan kenapa `ModelState.Clear()` + semakan manual digunakan untuk mod draf.

---

## Latihan 4 — Razor View: Create & Edit

**Objektif:** Bina borang HTML menggunakan Tag Helpers ASP.NET Core, dengan validation summary dan validation per-medan.

1. Cipta folder `Views/OfficerReporting/` (jika belum wujud).

2. Cipta fail `Views/OfficerReporting/Create.cshtml`:

   ```cshtml
   @model Nres.Onboarding.Web.ViewModels.OfficerReportingCreateViewModel

   @{
       ViewData["Title"] = "Lapor Diri — Permohonan Baharu";
   }

   <h1>Lapor Diri — Permohonan Baharu</h1>

   <div asp-validation-summary="ModelOnly" class="text-danger"></div>

   <form asp-action="Create" method="post">
       @Html.AntiForgeryToken()

       <div class="mb-3">
           <label asp-for="FullName" class="form-label"></label>
           <input asp-for="FullName" class="form-control" />
           <span asp-validation-for="FullName" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="IdentityNo" class="form-label"></label>
           <input asp-for="IdentityNo" class="form-control" />
           <span asp-validation-for="IdentityNo" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Email" class="form-label"></label>
           <input asp-for="Email" class="form-control" type="email" />
           <span asp-validation-for="Email" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Phone" class="form-label"></label>
           <input asp-for="Phone" class="form-control" />
           <span asp-validation-for="Phone" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Department" class="form-label"></label>
           <select asp-for="Department" asp-items="@(new SelectList(Model.DepartmentOptions))" class="form-select">
               <option value="">-- Pilih Jabatan --</option>
           </select>
           <span asp-validation-for="Department" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Position" class="form-label"></label>
           <input asp-for="Position" class="form-control" />
           <span asp-validation-for="Position" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Grade" class="form-label"></label>
           <select asp-for="Grade" asp-items="@(new SelectList(Model.GradeOptions))" class="form-select">
               <option value="">-- Pilih Gred --</option>
           </select>
           <span asp-validation-for="Grade" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="ReportingDate" class="form-label"></label>
           <input asp-for="ReportingDate" class="form-control" type="date" />
           <span asp-validation-for="ReportingDate" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="PreviousAgency" class="form-label"></label>
           <input asp-for="PreviousAgency" class="form-control" />
           <span asp-validation-for="PreviousAgency" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="EmergencyContact" class="form-label"></label>
           <input asp-for="EmergencyContact" class="form-control" />
           <span asp-validation-for="EmergencyContact" class="text-danger"></span>
       </div>

       <div class="d-flex gap-2">
           <button type="submit" name="submitAction" value="draft" class="btn btn-outline-secondary">
               Simpan Draf
           </button>
           <button type="submit" name="submitAction" value="full" class="btn btn-primary">
               Simpan &amp; Sahkan
           </button>
       </div>
   </form>

   @section Scripts {
       @{ await Html.RenderPartialAsync("_ValidationScriptsPartial"); }
   }
   ```

   > **Kenapa `@(new SelectList(Model.DepartmentOptions))` untuk dropdown, bukan senarai `<option>` ditulis manual?** `SelectList` (daripada `Microsoft.AspNetCore.Mvc.Rendering`) menjana elemen `<option>` **secara automatik** daripada senarai C#, dan `asp-for="Department"` secara automatik menandakan `<option>` yang sepadan nilai semasa sebagai `selected` — berguna terutamanya di `Edit.cshtml` di mana borang perlu papar nilai sedia ada.

3. Cipta fail `Views/OfficerReporting/Edit.cshtml` — struktur sama seperti `Create.cshtml`, tambah medan `Id` tersembunyi dan tukar `asp-action`:

   ```cshtml
   @model Nres.Onboarding.Web.ViewModels.OfficerReportingEditViewModel

   @{
       ViewData["Title"] = "Lapor Diri — Sunting Draf";
   }

   <h1>Lapor Diri — Sunting Draf</h1>

   <div asp-validation-summary="ModelOnly" class="text-danger"></div>

   <form asp-action="Edit" asp-route-id="@Model.Id" method="post">
       @Html.AntiForgeryToken()
       <input type="hidden" asp-for="Id" />

       <div class="mb-3">
           <label asp-for="FullName" class="form-label"></label>
           <input asp-for="FullName" class="form-control" />
           <span asp-validation-for="FullName" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="IdentityNo" class="form-label"></label>
           <input asp-for="IdentityNo" class="form-control" />
           <span asp-validation-for="IdentityNo" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Email" class="form-label"></label>
           <input asp-for="Email" class="form-control" type="email" />
           <span asp-validation-for="Email" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Phone" class="form-label"></label>
           <input asp-for="Phone" class="form-control" />
           <span asp-validation-for="Phone" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Department" class="form-label"></label>
           <select asp-for="Department" asp-items="@(new SelectList(Model.DepartmentOptions))" class="form-select">
               <option value="">-- Pilih Jabatan --</option>
           </select>
           <span asp-validation-for="Department" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Position" class="form-label"></label>
           <input asp-for="Position" class="form-control" />
           <span asp-validation-for="Position" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Grade" class="form-label"></label>
           <select asp-for="Grade" asp-items="@(new SelectList(Model.GradeOptions))" class="form-select">
               <option value="">-- Pilih Gred --</option>
           </select>
           <span asp-validation-for="Grade" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="ReportingDate" class="form-label"></label>
           <input asp-for="ReportingDate" class="form-control" type="date" />
           <span asp-validation-for="ReportingDate" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="PreviousAgency" class="form-label"></label>
           <input asp-for="PreviousAgency" class="form-control" />
           <span asp-validation-for="PreviousAgency" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="EmergencyContact" class="form-label"></label>
           <input asp-for="EmergencyContact" class="form-control" />
           <span asp-validation-for="EmergencyContact" class="text-danger"></span>
       </div>

       <div class="d-flex gap-2">
           <button type="submit" name="submitAction" value="draft" class="btn btn-outline-secondary">
               Simpan Draf
           </button>
           <button type="submit" name="submitAction" value="full" class="btn btn-primary">
               Simpan &amp; Sahkan
           </button>
       </div>
   </form>

   @section Scripts {
       @{ await Html.RenderPartialAsync("_ValidationScriptsPartial"); }
   }
   ```

4. Cipta fail `Views/OfficerReporting/Details.cshtml`:

   ```cshtml
   @model Nres.Onboarding.Web.Models.OfficerReportingApplication

   @{
       ViewData["Title"] = "Lapor Diri — Butiran";
   }

   <h1>Lapor Diri — Butiran</h1>

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

       <dt class="col-sm-3">Jawatan</dt>
       <dd class="col-sm-9">@Model.Position</dd>

       <dt class="col-sm-3">Gred</dt>
       <dd class="col-sm-9">@Model.Grade</dd>

       <dt class="col-sm-3">Tarikh Lapor Diri</dt>
       <dd class="col-sm-9">@Model.ReportingDate.ToString("dd/MM/yyyy")</dd>

       <dt class="col-sm-3">Agensi Terdahulu</dt>
       <dd class="col-sm-9">@(Model.PreviousAgency ?? "—")</dd>

       <dt class="col-sm-3">Kenalan Kecemasan</dt>
       <dd class="col-sm-9">@(Model.EmergencyContact ?? "—")</dd>
   </dl>

   @if (Model.Submission.Status == Nres.Onboarding.Web.Models.SubmissionStatus.Draft)
   {
       <a asp-action="Edit" asp-route-id="@Model.Id" class="btn btn-outline-primary">Sunting Draf</a>
   }

   <a asp-action="Index" class="btn btn-link">Kembali ke Senarai</a>
   ```

5. Cipta fail `Views/OfficerReporting/Index.cshtml`:

   ```cshtml
   @model List<Nres.Onboarding.Web.Models.OfficerReportingApplication>

   @{
       ViewData["Title"] = "Lapor Diri — Senarai Permohonan";
   }

   <h1>Lapor Diri</h1>

   <a asp-action="Create" class="btn btn-primary mb-3">+ Permohonan Baharu</a>

   <table class="table table-striped">
       <thead>
           <tr>
               <th>Nama Penuh</th>
               <th>Jabatan</th>
               <th>Status</th>
               <th>Tarikh Cipta</th>
               <th></th>
           </tr>
       </thead>
       <tbody>
           @foreach (var item in Model)
           {
               <tr>
                   <td>@item.FullName</td>
                   <td>@item.Department</td>
                   <td>@item.Submission.Status</td>
                   <td>@item.Submission.CreatedAt.ToString("dd/MM/yyyy HH:mm")</td>
                   <td>
                       <a asp-action="Details" asp-route-id="@item.Id" class="btn btn-sm btn-outline-secondary">Lihat</a>
                   </td>
               </tr>
           }
       </tbody>
   </table>

   @if (!Model.Any())
   {
       <p class="text-muted">Tiada permohonan Lapor Diri lagi. Klik "+ Permohonan Baharu" untuk mula.</p>
   }
   ```

6. Jalankan aplikasi dan uji aliran penuh:

   ```bash
   dotnet run
   ```

   - Daftar akaun baharu melalui `/Identity/Account/Register` (Identity UI lalai), log masuk.
   - Navigasi ke "Lapor Diri" → "+ Permohonan Baharu".
   - Cuba tekan **Simpan & Sahkan** dengan borang kosong — semua mesej ralat validation patut muncul.
   - Isi hanya **Nama Penuh**, tekan **Simpan Draf** — rekod patut berjaya dicipta dengan status `Draft`.
   - Isi semua medan dengan betul, tekan **Simpan & Sahkan** — rekod patut berjaya dicipta.
   - Dari halaman Details, klik **Sunting Draf**, ubah sesuatu medan, simpan semula.

✅ **Semakan:** Borang Create memaparkan mesej ralat validation yang betul untuk setiap medan wajib; "Simpan Draf" berjaya dengan hanya Nama Penuh diisi; "Simpan & Sahkan" menolak borang tidak lengkap; Edit berfungsi untuk rekod berstatus `Draft`.

---

## Rujukan Fail Sebenar

| Fail anda (lab) | Fail rujukan (projek sebenar) |
|------------------|-------------------------------|
| `Models/OfficerReportingApplication.cs` | `projek/Nres.Onboarding.Web/Models/OfficerReportingApplication.cs` |
| `ViewModels/OfficerReportingCreateViewModel.cs`, `OfficerReportingEditViewModel.cs` | `projek/Nres.Onboarding.Web/ViewModels/` |
| `Controllers/OfficerReportingController.cs` | `projek/Nres.Onboarding.Web/Controllers/OfficerReportingController.cs` |
| `Views/OfficerReporting/*.cshtml` | `projek/Nres.Onboarding.Web/Views/OfficerReporting/` |

---

## Cabaran (Pilihan)

1. **Validation custom** — Tambah atribut `[Range]` atau logik custom (`IValidatableObject`) yang menyekat `ReportingDate` daripada tarikh **lampau** lebih 30 hari (andaian: pegawai baharu tidak sepatutnya lapor diri lebih sebulan selepas tarikh sepatutnya).
2. **Paparan ralat lebih mesra** — Tukar mesej ralat `[EmailAddress]` lalai kepada Bahasa Melayu sepenuhnya melalui `ErrorMessage`, dan uji ia terpapar betul dalam `asp-validation-for`.
3. **Elak duplicate submission** — Sebelum cipta `Submission` baharu dalam `Create` (POST), semak jika pengguna semasa sudah ada Lapor Diri berstatus `Draft` yang belum dihantar; jika ada, redirect terus ke `Edit` rekod tersebut dan papar mesej makluman.

---

Nota penceramah (pemasaan sesi, silap biasa, soalan perbincangan, deliverable akhir hari): [`../nota-penceramah.md`](../nota-penceramah.md).
