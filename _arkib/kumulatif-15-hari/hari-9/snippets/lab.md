# Lab Hari 9 — ID/AD/Email: Notifikasi, Carian & Audit

Lab ini mengiringi [`../README.md`](../README.md) Hari 9. Sambungan terus daripada [Lab Hari 8](../../hari-8/snippets/lab.md) — `AccountRequestsController`, `AccountRequestApprovalsController`, dan `IWorkflowService` mesti sudah berfungsi sebelum mula. Rujuk [`../../projek/`](../../projek/) untuk banding.

---

## Latihan 1 — `INotificationService` + `ConsoleNotificationService`

**Objektif:** Tulis servis notifikasi kongsi, dan daftar dalam DI container.

1. Cipta `Services/INotificationService.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Services;

   public interface INotificationService
   {
       Task SendAsync(string recipientEmail, string subject, string message);
   }
   ```

2. Cipta `Services/ConsoleNotificationService.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Services;

   // Implementasi LATIHAN sahaja — tulis ke Console, tiada e-mel sebenar dihantar.
   // Dalam pengeluaran, gantikan dengan SmtpNotificationService (atau penyedia lain)
   // yang mengimplementasikan interface SAMA — tiada kod controller perlu berubah.
   public class ConsoleNotificationService : INotificationService
   {
       public Task SendAsync(string recipientEmail, string subject, string message)
       {
           Console.WriteLine($"To: {recipientEmail} | {subject} | {message}");
           return Task.CompletedTask;
       }
   }
   ```

3. Daftar dalam `Program.cs`, berdekatan pendaftaran `IWorkflowService` (Hari 8):

   ```csharp
   builder.Services.AddScoped<INotificationService, ConsoleNotificationService>();
   ```

✅ **Semakan:** `dotnet build` berjaya. Servis belum dipanggil di mana-mana lagi — itu Latihan 2.

---

## Latihan 2 — Cetus Notifikasi Pada Setiap Peralihan Status

**Objektif:** Panggil `INotificationService.SendAsync(...)` selepas setiap peralihan status dalam `AccountRequestsController` dan `AccountRequestApprovalsController`.

> **Nota mengambil e-mel pengguna:** Lab ini mengandaikan `ICurrentUserService`/Identity boleh diminta e-mel pengguna melalui `UserManager<IdentityUser>`. Suntik `UserManager<IdentityUser> userManager` ke dalam primary constructor kedua-dua controller di bawah.

1. Kemas kini `Controllers/AccountRequestsController.cs` — tambah `INotificationService` dan `UserManager<IdentityUser>`, kemudian cetus notifikasi selepas `Submit`:

   ```csharp
   using Microsoft.AspNetCore.Identity;

   public class AccountRequestsController(
       ApplicationDbContext db,
       IReferenceNumberService referenceNumberService,
       IAuditLogService auditLogService,
       IWorkflowService workflowService,
       INotificationService notificationService,
       UserManager<IdentityUser> userManager,
       ICurrentUserService currentUserService) : Controller
   {
       // ... Index, Create, Details kekal ...

       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Submit(int id)
       {
           var accountRequest = await db.AccountRequests
               .Include(x => x.Submission)
               .FirstOrDefaultAsync(x => x.Id == id);

           if (accountRequest is null)
           {
               return NotFound();
           }

           if (accountRequest.Submission.ApplicantUserId != currentUserService.UserId)
           {
               return Forbid();
           }

           if (accountRequest.Submission.Status != SubmissionStatus.Draft)
           {
               TempData["Error"] = "Hanya permohonan berstatus Draf boleh dihantar.";
               return RedirectToAction(nameof(Details), new { id });
           }

           accountRequest.Submission.ReferenceNo =
               await referenceNumberService.GenerateAsync(ModuleCode);

           await workflowService.TransitionAsync(
               accountRequest.Submission,
               SubmissionStatus.Submitted,
               currentUserService.UserId,
               "Permohonan dihantar oleh pemohon.");

           var supervisor = await userManager.FindByIdAsync(accountRequest.SupervisorUserId);
           if (supervisor?.Email is not null)
           {
               await notificationService.SendAsync(
                   supervisor.Email,
                   $"Permohonan {accountRequest.Submission.ReferenceNo} Menunggu Kelulusan Anda",
                   $"Permohonan {accountRequest.RequestType} daripada {accountRequest.ApplicantFullName} menunggu kelulusan anda.");
           }

           return RedirectToAction(nameof(Details), new { id });
       }
   }
   ```

2. Kemas kini `Controllers/AccountRequestApprovalsController.cs` — tambah `INotificationService` dan `UserManager<IdentityUser>`, cetus notifikasi pada `SupervisorApprove`, `SupervisorReject`, `Complete`, `IctReject`:

   ```csharp
   using Microsoft.AspNetCore.Identity;

   public class AccountRequestApprovalsController(
       ApplicationDbContext db,
       IWorkflowService workflowService,
       IAuditLogService auditLogService,
       INotificationService notificationService,
       UserManager<IdentityUser> userManager,
       ICurrentUserService currentUserService) : Controller
   {
       [Authorize(Roles = "Supervisor")]
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> SupervisorApprove(int id)
       {
           var accountRequest = await LoadForSupervisorAsync(id);
           if (accountRequest is null)
           {
               return NotFound();
           }

           if (accountRequest.Submission.Status != SubmissionStatus.Submitted)
           {
               TempData["Error"] = "Hanya permohonan berstatus Submitted boleh diluluskan Penyelia.";
               return RedirectToAction(nameof(SupervisorPending));
           }

           await workflowService.TransitionAsync(
               accountRequest.Submission,
               SubmissionStatus.SupervisorApproved,
               currentUserService.UserId,
               "Diluluskan oleh Penyelia.");

           db.ApprovalSteps.Add(new ApprovalStep
           {
               SubmissionId = accountRequest.SubmissionId,
               StepOrder = 1,
               ApproverRole = "Supervisor",
               Status = ApprovalStepStatus.Approved,
               ActorUserId = currentUserService.UserId,
               DecidedAt = DateTime.UtcNow,
           });
           await db.SaveChangesAsync();

           var applicant = await userManager.FindByIdAsync(accountRequest.Submission.ApplicantUserId);
           if (applicant?.Email is not null)
           {
               await notificationService.SendAsync(
                   applicant.Email,
                   $"Permohonan {accountRequest.Submission.ReferenceNo} Diluluskan Penyelia",
                   "Permohonan anda kini menunggu pemprosesan ICT.");
           }

           // Notifikasi kumpulan ICT — dalam projek sebenar, hantar kepada
           // senarai e-mel kumpulan IctAdmin (cth. dari konfigurasi appsettings).
           await notificationService.SendAsync(
               "ict-admin@nres.gov.my",
               $"Permohonan {accountRequest.Submission.ReferenceNo} Menunggu Pemprosesan ICT",
               $"Permohonan {accountRequest.RequestType} sudah diluluskan Penyelia.");

           return RedirectToAction(nameof(SupervisorPending));
       }

       [Authorize(Roles = "Supervisor")]
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> SupervisorReject(int id, string remarks)
       {
           if (string.IsNullOrWhiteSpace(remarks))
           {
               TempData["Error"] = "Sebab penolakan wajib diisi.";
               return RedirectToAction(nameof(SupervisorPending));
           }

           var accountRequest = await LoadForSupervisorAsync(id);
           if (accountRequest is null)
           {
               return NotFound();
           }

           await workflowService.TransitionAsync(
               accountRequest.Submission,
               SubmissionStatus.Rejected,
               currentUserService.UserId,
               remarks);

           db.ApprovalSteps.Add(new ApprovalStep
           {
               SubmissionId = accountRequest.SubmissionId,
               StepOrder = 1,
               ApproverRole = "Supervisor",
               Status = ApprovalStepStatus.Rejected,
               ActorUserId = currentUserService.UserId,
               DecidedAt = DateTime.UtcNow,
               Remarks = remarks,
           });
           await db.SaveChangesAsync();

           var applicant = await userManager.FindByIdAsync(accountRequest.Submission.ApplicantUserId);
           if (applicant?.Email is not null)
           {
               await notificationService.SendAsync(
                   applicant.Email,
                   $"Permohonan {accountRequest.Submission.ReferenceNo} Ditolak",
                   $"Sebab: {remarks}");
           }

           return RedirectToAction(nameof(SupervisorPending));
       }

       // Complete dan IctReject: tambah panggilan notificationService.SendAsync(...)
       // dengan corak SAMA seperti di atas — notifikasi kepada applicant.Email
       // selepas workflowService.TransitionAsync(...) berjaya. Cuba tulis sendiri
       // dahulu sebelum banding dengan projek rujukan.

       private async Task<AccountRequest?> LoadForSupervisorAsync(int id)
       {
           return await db.AccountRequests
               .Include(x => x.Submission)
               .FirstOrDefaultAsync(x => x.Id == id && x.SupervisorUserId == currentUserService.UserId);
       }
   }
   ```

   > **Latihan aktif:** Bahagian `Complete` dan `IctReject` **sengaja tidak** ditunjukkan penuh — tambah `notificationService.SendAsync(...)` sendiri mengikut corak yang sama seperti `SupervisorApprove`/`SupervisorReject` di atas. Ini latihan aktif supaya anda benar-benar faham polanya, bukan sekadar salin.

3. Jalankan aplikasi, hantar satu permohonan penuh (Draft → Submit → SupervisorApprove → Complete), dan perhatikan **terminal** (console output) — anda patut nampak baris seperti:

   ```text
   To: supervisor@nres.gov.my | Permohonan ICT-ID-2026-0001 Menunggu Kelulusan Anda | Permohonan NewAdAccount daripada ...
   To: applicant@nres.gov.my | Permohonan ICT-ID-2026-0001 Diluluskan Penyelia | Permohonan anda kini menunggu pemprosesan ICT.
   To: ict-admin@nres.gov.my | Permohonan ICT-ID-2026-0001 Menunggu Pemprosesan ICT | ...
   To: applicant@nres.gov.my | Permohonan ICT-ID-2026-0001 Telah Disempurnakan | ...
   ```

✅ **Semakan:** Jalankan aliran penuh satu permohonan (submit → approve → complete) dan sahkan **sekurang-kurangnya 3** baris notifikasi tercetak di console, masing-masing pada titik yang betul.

---

## Latihan 3 — Carian & Penapisan

**Objektif:** Bina carian pada `Index` yang menyokong penapisan mengikut rujukan, pemohon, jabatan, status, dan jenis permohonan — semuanya pilihan (opsyenal).

1. Cipta `ViewModels/AccountRequestSearchViewModel.cs`:

   ```csharp
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.ViewModels;

   public class AccountRequestSearchViewModel
   {
       public string? ReferenceNo { get; set; }
       public string? ApplicantName { get; set; }
       public int? DepartmentId { get; set; }
       public SubmissionStatus? Status { get; set; }
       public AccountRequestType? RequestType { get; set; }

       public List<AccountRequest> Results { get; set; } = [];
       public List<(int Id, string Name)> DepartmentOptions { get; set; } = [];
   }
   ```

2. Kemas kini `Index` dalam `Controllers/AccountRequestsController.cs` supaya menerima kriteria carian melalui query string (`[FromQuery]`), dan bina query `IQueryable` bersyarat:

   ```csharp
   public async Task<IActionResult> Index([FromQuery] AccountRequestSearchViewModel model)
   {
       IQueryable<AccountRequest> query = db.AccountRequests
           .Include(x => x.Submission);

       // Applicant biasa hanya nampak permohonan sendiri; Supervisor/IctAdmin
       // boleh cari merentas semua permohonan (guna untuk semakan carian am).
       if (!User.IsInRole("Supervisor") && !User.IsInRole("IctAdmin"))
       {
           query = query.Where(x => x.Submission.ApplicantUserId == currentUserService.UserId);
       }

       if (!string.IsNullOrWhiteSpace(model.ReferenceNo))
       {
           query = query.Where(x => x.Submission.ReferenceNo != null
               && x.Submission.ReferenceNo.Contains(model.ReferenceNo));
       }

       if (!string.IsNullOrWhiteSpace(model.ApplicantName))
       {
           query = query.Where(x => x.ApplicantFullName.Contains(model.ApplicantName));
       }

       if (model.DepartmentId.HasValue)
       {
           query = query.Where(x => x.DepartmentId == model.DepartmentId.Value);
       }

       if (model.Status.HasValue)
       {
           query = query.Where(x => x.Submission.Status == model.Status.Value);
       }

       if (model.RequestType.HasValue)
       {
           query = query.Where(x => x.RequestType == model.RequestType.Value);
       }

       model.Results = await query
           .OrderByDescending(x => x.CreatedAt)
           .ToListAsync();

       model.DepartmentOptions = await db.LookupDepartments
           .OrderBy(d => d.Name)
           .Select(d => new ValueTuple<int, string>(d.Id, d.Name))
           .ToListAsync();

       return View(model);
   }
   ```

   > **Kenapa `IQueryable` (bukan `IEnumerable`/`List`) sepanjang bina query?** `IQueryable` menangguhkan (*defer*) penjanaan SQL sehingga `.ToListAsync()` dipanggil — setiap `.Where(...)` tambahan **digabung** menjadi **satu** pertanyaan SQL akhir yang cekap, bukan berulang kali tapis dalam memori .NET selepas semua data ditarik dari pangkalan data.

3. Cipta `Views/AccountRequests/Index.cshtml`:

   ```cshtml
   @model Nres.Onboarding.Web.ViewModels.AccountRequestSearchViewModel
   @using Nres.Onboarding.Web.Models
   @{
       ViewData["Title"] = "Permohonan Akaun ICT";
   }

   <h1>Permohonan Akaun ICT</h1>

   <form method="get" class="row g-2 mb-4">
       <div class="col-md-2">
           <input asp-for="ReferenceNo" class="form-control" placeholder="No. Rujukan" />
       </div>
       <div class="col-md-2">
           <input asp-for="ApplicantName" class="form-control" placeholder="Nama Pemohon" />
       </div>
       <div class="col-md-2">
           <select asp-for="DepartmentId" class="form-select">
               <option value="">-- Semua Jabatan --</option>
               @foreach (var dept in Model.DepartmentOptions)
               {
                   <option value="@dept.Id">@dept.Name</option>
               }
           </select>
       </div>
       <div class="col-md-2">
           <select asp-for="Status" class="form-select"
                   asp-items="Html.GetEnumSelectList<SubmissionStatus>()">
               <option value="">-- Semua Status --</option>
           </select>
       </div>
       <div class="col-md-2">
           <select asp-for="RequestType" class="form-select"
                   asp-items="Html.GetEnumSelectList<AccountRequestType>()">
               <option value="">-- Semua Jenis --</option>
           </select>
       </div>
       <div class="col-md-2">
           <button type="submit" class="btn btn-primary w-100">Cari</button>
       </div>
   </form>

   <a class="btn btn-outline-primary mb-3" asp-action="Create">+ Mohon Akaun / Akses Baharu</a>

   <table class="table">
       <thead>
           <tr>
               <th>No. Rujukan</th>
               <th>Pemohon</th>
               <th>Jenis</th>
               <th>Status</th>
               <th></th>
           </tr>
       </thead>
       <tbody>
           @foreach (var item in Model.Results)
           {
               <tr>
                   <td>@(item.Submission.ReferenceNo ?? "(draf)")</td>
                   <td>@item.ApplicantFullName</td>
                   <td>@item.RequestType</td>
                   <td><span class="badge bg-secondary">@item.Submission.Status</span></td>
                   <td><a asp-action="Details" asp-route-id="@item.Id">Lihat</a></td>
               </tr>
           }
       </tbody>
   </table>

   @if (Model.Results.Count == 0)
   {
       <p class="text-muted">Tiada permohonan sepadan kriteria carian.</p>
   }
   ```

✅ **Semakan:** Cari dengan gabungan kriteria berbeza (cth. hanya `Status = Submitted`, atau `ApplicantName` + `DepartmentId` serentak) dan sahkan hasil betul untuk setiap kombinasi. Kosongkan semua kriteria — sepatutnya papar semua permohonan (dalam skop peranan anda).

---

## Latihan 4 — Panel Audit Pada Halaman Detail

**Objektif:** Papar sejarah kronologi `AuditLog` pada `AccountRequests/Details`, sebagai partial view boleh guna semula.

1. Kemas kini aksyen `Details` dalam `Controllers/AccountRequestsController.cs` untuk sertakan senarai audit log:

   ```csharp
   public async Task<IActionResult> Details(int id)
   {
       var accountRequest = await db.AccountRequests
           .Include(x => x.Submission)
           .Include(x => x.RequestedSystemAccesses)
               .ThenInclude(x => x.AccessType)
           .FirstOrDefaultAsync(x => x.Id == id);

       if (accountRequest is null)
       {
           return NotFound();
       }

       ViewBag.AuditLogs = await db.AuditLogs
           .Where(x => x.SubmissionId == accountRequest.SubmissionId)
           .OrderBy(x => x.CreatedAt)
           .ToListAsync();

       return View(accountRequest);
   }
   ```

2. Cipta partial view `Views/AccountRequests/_AuditPanel.cshtml`:

   ```cshtml
   @model List<Nres.Onboarding.Web.Models.AuditLog>

   <h3>Sejarah Audit</h3>

   @if (Model.Count == 0)
   {
       <p class="text-muted">Tiada rekod audit.</p>
   }
   else
   {
       <ul class="list-group">
           @foreach (var log in Model)
           {
               <li class="list-group-item">
                   <strong>@log.CreatedAt.ToLocalTime().ToString("dd/MM/yyyy HH:mm")</strong>
                   — @log.Action
                   @if (!string.IsNullOrWhiteSpace(log.Remarks))
                   {
                       <div class="text-muted">@log.Remarks</div>
                   }
               </li>
           }
       </ul>
   }
   ```

3. Panggil partial view di penghujung `Views/AccountRequests/Details.cshtml` (selepas butang "Hantar Permohonan" yang dibina Hari 8):

   ```cshtml
   <hr />
   @await Html.PartialAsync("_AuditPanel", (List<Nres.Onboarding.Web.Models.AuditLog>)ViewBag.AuditLogs)
   ```

   > **Kenapa partial view, bukan terus dalam `Details.cshtml`?** Panel audit ialah komponen UI yang **sama bentuknya** merentas modul (Modul 1, 2, dan kelak 4, 5 semuanya perlukan panel serupa). Menjadikan ia partial view bermakna kod Razor ini boleh **disalin/adaptasi** ke modul lain tanpa tulis semula struktur senarai dari awal.

✅ **Semakan:** Buka halaman detail mana-mana permohonan yang sudah melalui beberapa peralihan status — panel audit papar **semua** peristiwa (Created, StatusChanged berulang kali) tersusun mengikut masa, dengan sebab/remarks kelihatan bila ada.

---

## Latihan 5 — Ujian Hujung-ke-Hujung Modul 3

**Objektif:** Jalankan skrip ujian manual penuh, mengesahkan Modul 3 lengkap berfungsi.

Guna 3 pengguna ujian (`Applicant`, `Supervisor`, `IctAdmin`) yang sudah disediakan sejak Hari 8:

1. Log masuk sebagai `Applicant`. Cipta permohonan baharu (jenis "Akses Sistem Tambahan", pilih 2 jenis akses).
2. Hantar permohonan (`Submit`). Sahkan nombor rujukan `ICT-ID-2026-####` dijana, dan console papar notifikasi kepada Penyelia.
3. Log masuk sebagai `Supervisor`. Cari permohonan tadi melalui `/AccountRequestApprovals/SupervisorPending`. Luluskan.
4. Sahkan console papar **dua** notifikasi (kepada pemohon dan kumpulan ICT).
5. Log masuk sebagai `IctAdmin`. Sempurnakan permohonan melalui `/AccountRequestApprovals/IctPending`.
6. Sahkan console papar notifikasi terakhir kepada pemohon.
7. Log masuk semula sebagai `Applicant`. Buka `Details` permohonan — sahkan status `Completed`, dan panel audit papar **kesemua** peristiwa (Created → Submitted → SupervisorApproved → Completed) tersusun mengikut masa.
8. Cuba carian di `/AccountRequests` (log masuk sebagai `IctAdmin`) mengikut `Status = Completed` dan sahkan permohonan tadi kelihatan.

✅ **Semakan akhir Modul 3:** Kesemua 8 langkah di atas berjaya tanpa ralat, dan anda boleh terangkan setiap titik cetus notifikasi secara lisan.

---

## Rujukan Fail

| Bahagian lab | Fail rujukan (`projek/`) |
|---|---|
| Notifikasi (Latihan 1–2) | `projek/Nres.Onboarding.Web/Services/INotificationService.cs`, `ConsoleNotificationService.cs` |
| Carian (Latihan 3) | `projek/Nres.Onboarding.Web/ViewModels/AccountRequestSearchViewModel.cs`, `Views/AccountRequests/Index.cshtml` |
| Panel Audit (Latihan 4) | `projek/Nres.Onboarding.Web/Views/AccountRequests/_AuditPanel.cshtml` |

---

## Cabaran (Pilihan)

1. Tambah kaedah `INotificationService.SendBulkAsync(IEnumerable<string> recipientEmails, string subject, string message)` untuk hantar kepada berbilang kumpulan ICT sekaligus, tanpa gelung manual di controller.
2. Tambah penapis **julat tarikh** (`CreatedFrom`/`CreatedTo`) ke `AccountRequestSearchViewModel` — gunakan corak `IQueryable` bersyarat yang sama seperti Latihan 3.
3. Bina satu paparan ringkasan (`GroupBy(x => x.Submission.Status)`) yang kira jumlah permohonan setiap status — boleh jadi asas carta kecil di dashboard Hari 7 kelak.

---

> 🎤 **Nota penceramah/jurulatih:** [`../nota-penceramah.md`](../nota-penceramah.md) untuk pemasaan, poin bercakap, dan silap biasa peserta Hari 9.
