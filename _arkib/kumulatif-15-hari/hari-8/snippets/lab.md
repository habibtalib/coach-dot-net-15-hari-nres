# Lab Hari 8 — ID/AD/Email: Rantaian Kelulusan & Authorization

Lab ini mengiringi [`../README.md`](../README.md) Hari 8. Sambungan terus daripada [Lab Hari 7](../../hari-7/snippets/lab.md) — entiti `AccountRequest`, `RequestedSystemAccess`, `ApprovalStep`, `AccessType` mesti sudah wujud & bermigrasi sebelum mula. Rujuk [`../../projek/`](../../projek/) untuk banding.

---

## Latihan 1 — View Model & Borang Permohonan (`Create`)

**Objektif:** Bina view model borang, dan controller `Create` (GET + POST) yang simpan sebagai **Draft**.

1. Cipta `ViewModels/SystemAccessOption.cs` — satu baris pilihan akses (checkbox) dalam borang:

   ```csharp
   namespace Nres.Onboarding.Web.ViewModels;

   public class SystemAccessOption
   {
       public int AccessTypeId { get; set; }
       public string AccessTypeName { get; set; } = string.Empty;
       public bool IsSelected { get; set; }
       public string? SystemName { get; set; }
       public string? Justification { get; set; }
   }
   ```

2. Cipta `ViewModels/AccountRequestCreateViewModel.cs`:

   ```csharp
   using System.ComponentModel.DataAnnotations;
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.ViewModels;

   public class AccountRequestCreateViewModel
   {
       [Required(ErrorMessage = "Sila pilih jenis permohonan.")]
       public AccountRequestType RequestType { get; set; }

       [Required]
       [StringLength(200)]
       [Display(Name = "Nama Penuh Pemohon")]
       public string ApplicantFullName { get; set; } = string.Empty;

       [Required]
       [StringLength(20)]
       [Display(Name = "No. Kad Pengenalan")]
       public string ApplicantIcNo { get; set; } = string.Empty;

       [Required(ErrorMessage = "Sila pilih jabatan.")]
       [Display(Name = "Jabatan")]
       public int DepartmentId { get; set; }

       [Required]
       [StringLength(100)]
       [Display(Name = "Jawatan")]
       public string Position { get; set; } = string.Empty;

       [Required(ErrorMessage = "Sila pilih penyelia yang meluluskan.")]
       [Display(Name = "Penyelia (Supervisor)")]
       public string SupervisorUserId { get; set; } = string.Empty;

       [Required]
       [StringLength(1000)]
       [Display(Name = "Sebab Permohonan")]
       public string Justification { get; set; } = string.Empty;

       [StringLength(100)]
       [Display(Name = "Username Akaun Sedia Ada")]
       public string? TargetSystemUsername { get; set; }

       [DataType(DataType.Date)]
       [Display(Name = "Tarikh Berkuat Kuasa Nyahaktif")]
       public DateTime? EffectiveDate { get; set; }

       // Senarai checkbox akses — diisi semula dari AccessTypes semasa GET.
       public List<SystemAccessOption> AccessOptions { get; set; } = [];

       // Untuk dropdown Jabatan/Penyelia dalam view — diisi semula semasa GET.
       public List<(int Id, string Name)> DepartmentOptions { get; set; } = [];
       public List<(string Id, string Name)> SupervisorOptions { get; set; } = [];
   }
   ```

   > **Nota validation:** Kita **sengaja tidak** letak `[Required]` pada `AccessOptions` di peringkat DataAnnotations — semakan "sekurang-kurangnya satu akses dipilih" ialah **peraturan perniagaan bersyarat** (hanya wajib untuk jenis permohonan tertentu), jadi disemak secara manual dalam controller (Latihan 1, langkah 4), bukan attribute generik.

3. Cipta `Controllers/AccountRequestsController.cs` — mula dengan `Index` dan `Create` GET:

   ```csharp
   using Microsoft.AspNetCore.Authorization;
   using Microsoft.AspNetCore.Mvc;
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Data;
   using Nres.Onboarding.Web.Models;
   using Nres.Onboarding.Web.Services;
   using Nres.Onboarding.Web.ViewModels;

   namespace Nres.Onboarding.Web.Controllers;

   [Authorize]
   public class AccountRequestsController(
       ApplicationDbContext db,
       IReferenceNumberService referenceNumberService,
       IAuditLogService auditLogService,
       ICurrentUserService currentUserService) : Controller
   {
       public const string ModuleCode = "ICT-ID";

       public async Task<IActionResult> Index()
       {
           var myRequests = await db.AccountRequests
               .Include(x => x.Submission)
               .Where(x => x.Submission.ApplicantUserId == currentUserService.UserId)
               .OrderByDescending(x => x.CreatedAt)
               .ToListAsync();

           return View(myRequests);
       }

       public async Task<IActionResult> Create()
       {
           var model = new AccountRequestCreateViewModel();
           await PopulateOptionsAsync(model);
           return View(model);
       }

       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Create(AccountRequestCreateViewModel model)
       {
           var selectedAccessTypeIds = model.AccessOptions
               .Where(x => x.IsSelected)
               .Select(x => x.AccessTypeId)
               .ToList();

           if (model.RequestType == AccountRequestType.AdditionalSystemAccess && selectedAccessTypeIds.Count == 0)
           {
               ModelState.AddModelError(
                   nameof(model.AccessOptions),
                   "Sila pilih sekurang-kurangnya satu jenis akses untuk permohonan Akses Sistem Tambahan.");
           }

           if (!ModelState.IsValid)
           {
               await PopulateOptionsAsync(model);
               return View(model);
           }

           var submission = new Submission
           {
               ModuleCode = ModuleCode,
               ApplicantUserId = currentUserService.UserId,
               Status = SubmissionStatus.Draft,
           };
           db.Submissions.Add(submission);

           var accountRequest = new AccountRequest
           {
               Submission = submission,
               RequestType = model.RequestType,
               ApplicantFullName = model.ApplicantFullName,
               ApplicantIcNo = model.ApplicantIcNo,
               DepartmentId = model.DepartmentId,
               Position = model.Position,
               SupervisorUserId = model.SupervisorUserId,
               Justification = model.Justification,
               TargetSystemUsername = model.TargetSystemUsername,
               EffectiveDate = model.EffectiveDate,
           };

           foreach (var option in model.AccessOptions.Where(x => x.IsSelected))
           {
               accountRequest.RequestedSystemAccesses.Add(new RequestedSystemAccess
               {
                   AccessTypeId = option.AccessTypeId,
                   SystemName = option.SystemName,
                   Justification = option.Justification,
               });
           }

           db.AccountRequests.Add(accountRequest);
           await db.SaveChangesAsync();

           await auditLogService.RecordAsync(submission.Id, "Created", "Draf permohonan akaun dicipta.");

           return RedirectToAction(nameof(Details), new { id = accountRequest.Id });
       }

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

           return View(accountRequest);
       }

       private async Task PopulateOptionsAsync(AccountRequestCreateViewModel model)
       {
           var accessTypes = await db.AccessTypes
               .Where(x => x.IsActive)
               .OrderBy(x => x.Name)
               .ToListAsync();

           // Kekalkan pilihan sedia ada (kalau validation gagal & borang dipapar semula).
           var existingSelections = model.AccessOptions.ToDictionary(x => x.AccessTypeId);

           model.AccessOptions = accessTypes.Select(t => new SystemAccessOption
           {
               AccessTypeId = t.Id,
               AccessTypeName = t.Name,
               IsSelected = existingSelections.TryGetValue(t.Id, out var existing) && existing.IsSelected,
               SystemName = existingSelections.TryGetValue(t.Id, out var existing2) ? existing2.SystemName : null,
               Justification = existingSelections.TryGetValue(t.Id, out var existing3) ? existing3.Justification : null,
           }).ToList();

           model.DepartmentOptions = await db.LookupDepartments
               .OrderBy(d => d.Name)
               .Select(d => new ValueTuple<int, string>(d.Id, d.Name))
               .ToListAsync();

           // SupervisorOptions — dalam projek sebenar diisi dari AspNetUsers dalam role "Supervisor".
           // Lihat rujukan projek untuk contoh guna UserManager<IdentityUser>.
       }
   }
   ```

   > **Nota `LookupDepartment`:** Lab ini mengandaikan entiti `LookupDepartment` (jadual `LookupDepartments`) sudah wujud sejak Hari 1 sebagai sebahagian entiti kongsi. Jika projek anda guna nama medan berbeza (cth. `DepartmentName` bukan `Name`), sesuaikan mengikut definisi sedia ada anda — **jangan** cipta jadual jabatan baharu berasingan.

4. Cipta `Views/AccountRequests/Create.cshtml`:

   ```cshtml
   @model Nres.Onboarding.Web.ViewModels.AccountRequestCreateViewModel
   @using Nres.Onboarding.Web.Models
   @{
       ViewData["Title"] = "Mohon Akaun / Akses ICT";
   }

   <h1>Mohon Akaun / Akses ICT</h1>

   <div class="alert alert-warning">
       Borang ini <strong>tidak</strong> meminta atau menyimpan kata laluan. Kata laluan sebenar
       akan diserahkan oleh ICT selepas permohonan disempurnakan.
   </div>

   <form asp-action="Create" method="post">
       <div asp-validation-summary="ModelOnly" class="text-danger"></div>

       <div class="mb-3">
           <label asp-for="RequestType" class="form-label"></label>
           <select asp-for="RequestType" class="form-select"
                   asp-items="Html.GetEnumSelectList<AccountRequestType>()"></select>
           <span asp-validation-for="RequestType" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="ApplicantFullName" class="form-label"></label>
           <input asp-for="ApplicantFullName" class="form-control" />
           <span asp-validation-for="ApplicantFullName" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="ApplicantIcNo" class="form-label"></label>
           <input asp-for="ApplicantIcNo" class="form-control" />
           <span asp-validation-for="ApplicantIcNo" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="DepartmentId" class="form-label"></label>
           <select asp-for="DepartmentId" class="form-select">
               <option value="">-- Pilih Jabatan --</option>
               @foreach (var dept in Model.DepartmentOptions)
               {
                   <option value="@dept.Id">@dept.Name</option>
               }
           </select>
           <span asp-validation-for="DepartmentId" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Position" class="form-label"></label>
           <input asp-for="Position" class="form-control" />
           <span asp-validation-for="Position" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="SupervisorUserId" class="form-label"></label>
           <select asp-for="SupervisorUserId" class="form-select">
               <option value="">-- Pilih Penyelia --</option>
               @foreach (var sup in Model.SupervisorOptions)
               {
                   <option value="@sup.Id">@sup.Name</option>
               }
           </select>
           <span asp-validation-for="SupervisorUserId" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="Justification" class="form-label"></label>
           <textarea asp-for="Justification" class="form-control" rows="3"></textarea>
           <span asp-validation-for="Justification" class="text-danger"></span>
       </div>

       <div class="mb-3">
           <label asp-for="TargetSystemUsername" class="form-label"></label>
           <input asp-for="TargetSystemUsername" class="form-control" />
           <div class="form-text">Hanya isi untuk permohonan Kemas Kini / Nyahaktif Akaun.</div>
       </div>

       <div class="mb-3">
           <label asp-for="EffectiveDate" class="form-label"></label>
           <input asp-for="EffectiveDate" class="form-control" />
           <div class="form-text">Hanya isi untuk permohonan Nyahaktif Akaun.</div>
       </div>

       <fieldset class="mb-3">
           <legend class="fs-6">Akses Yang Dimohon</legend>
           @for (var i = 0; i < Model.AccessOptions.Count; i++)
           {
               <div class="form-check border rounded p-2 mb-2">
                   <input type="hidden" asp-for="AccessOptions[i].AccessTypeId" />
                   <input type="hidden" asp-for="AccessOptions[i].AccessTypeName" />
                   <input class="form-check-input" type="checkbox" asp-for="AccessOptions[i].IsSelected" />
                   <label class="form-check-label" asp-for="AccessOptions[i].IsSelected">
                       @Model.AccessOptions[i].AccessTypeName
                   </label>
                   <input asp-for="AccessOptions[i].SystemName" class="form-control form-control-sm mt-1"
                          placeholder="Nama sistem (jika Sistem Dalaman)" />
                   <input asp-for="AccessOptions[i].Justification" class="form-control form-control-sm mt-1"
                          placeholder="Sebab khusus akses ini (pilihan)" />
               </div>
           }
           <span asp-validation-for="AccessOptions" class="text-danger"></span>
       </fieldset>

       <button type="submit" class="btn btn-primary">Simpan Sebagai Draf</button>
   </form>
   ```

✅ **Semakan:** Borang `/AccountRequests/Create` boleh dihantar, satu `Submission` (status `Draft`) + `AccountRequest` + `RequestedSystemAccess` (jika dipilih) tersimpan, dan `AuditLog` "Created" direkod.

---

## Latihan 2 — `IWorkflowService`: Kuatkuasa Peraturan Peralihan Status

**Objektif:** Tulis servis kongsi yang menguatkuasakan jadual peraturan status dalam README secara automatik.

1. Cipta `Services/IWorkflowService.cs`:

   ```csharp
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.Services;

   public interface IWorkflowService
   {
       bool CanTransition(SubmissionStatus current, SubmissionStatus next);

       Task TransitionAsync(Submission submission, SubmissionStatus next, string actorUserId, string? remarks = null);
   }
   ```

2. Cipta `Services/WorkflowService.cs`:

   ```csharp
   using Nres.Onboarding.Web.Data;
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.Services;

   public class WorkflowService(ApplicationDbContext db, IAuditLogService auditLogService) : IWorkflowService
   {
       // Jadual Peraturan Status — rujuk README Hari 8. Tambah baris di sini
       // jika modul lain kelak perlukan peralihan tambahan.
       private static readonly Dictionary<SubmissionStatus, SubmissionStatus[]> AllowedTransitions = new()
       {
           [SubmissionStatus.Draft] = [SubmissionStatus.Submitted, SubmissionStatus.Cancelled],
           [SubmissionStatus.Submitted] = [SubmissionStatus.SupervisorApproved, SubmissionStatus.Rejected],
           [SubmissionStatus.SupervisorApproved] = [SubmissionStatus.Completed, SubmissionStatus.Rejected],
       };

       public bool CanTransition(SubmissionStatus current, SubmissionStatus next)
       {
           return AllowedTransitions.TryGetValue(current, out var allowedNext) && allowedNext.Contains(next);
       }

       public async Task TransitionAsync(Submission submission, SubmissionStatus next, string actorUserId, string? remarks = null)
       {
           if (!CanTransition(submission.Status, next))
           {
               throw new InvalidOperationException(
                   $"Peralihan status tidak sah: {submission.Status} -> {next}.");
           }

           var previousStatus = submission.Status;
           submission.Status = next;

           if (next == SubmissionStatus.Submitted)
           {
               submission.SubmittedAt = DateTime.UtcNow;
           }

           if (next == SubmissionStatus.Completed)
           {
               submission.CompletedAt = DateTime.UtcNow;
           }

           await db.SaveChangesAsync();
           await auditLogService.RecordAsync(submission.Id, $"StatusChanged:{previousStatus}->{next}", remarks);
       }
   }
   ```

3. Daftar servis dalam `Program.cs` (letak berdekatan pendaftaran servis kongsi sedia ada):

   ```csharp
   builder.Services.AddScoped<IWorkflowService, WorkflowService>();
   ```

✅ **Semakan:** `dotnet build` berjaya. Cuba panggil `workflowService.CanTransition(SubmissionStatus.Draft, SubmissionStatus.Completed)` dalam ujian sementara/`Console.WriteLine` — sepatutnya `false` (laluan pintasan **ditolak**).

---

## Latihan 3 — Aksyen `Submit` (Applicant)

**Objektif:** Tambah aksyen untuk pemohon hantar draf — jana nombor rujukan dan tukar status ke `Submitted` melalui `IWorkflowService`.

1. Kemas kini `Controllers/AccountRequestsController.cs` — tambah `IWorkflowService` ke primary constructor dan aksyen `Submit`:

   ```csharp
   public class AccountRequestsController(
       ApplicationDbContext db,
       IReferenceNumberService referenceNumberService,
       IAuditLogService auditLogService,
       IWorkflowService workflowService,
       ICurrentUserService currentUserService) : Controller
   {
       // ... Index, Create, Details kekal seperti Latihan 1 ...

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

           return RedirectToAction(nameof(Details), new { id });
       }
   }
   ```

   > Ingat tambah `using Microsoft.EntityFrameworkCore;` jika belum ada, untuk `Include`/`FirstOrDefaultAsync`.

2. Tambah butang "Hantar Permohonan" dalam `Views/AccountRequests/Details.cshtml` (cipta view ini jika belum wujud):

   ```cshtml
   @model Nres.Onboarding.Web.Models.AccountRequest
   @{
       ViewData["Title"] = "Butiran Permohonan";
   }

   <h1>Butiran Permohonan</h1>

   <dl class="row">
       <dt class="col-sm-3">No. Rujukan</dt>
       <dd class="col-sm-9">@(Model.Submission.ReferenceNo ?? "(belum dijana — masih draf)")</dd>

       <dt class="col-sm-3">Status</dt>
       <dd class="col-sm-9"><span class="badge bg-secondary">@Model.Submission.Status</span></dd>

       <dt class="col-sm-3">Jenis Permohonan</dt>
       <dd class="col-sm-9">@Model.RequestType</dd>

       <dt class="col-sm-3">Nama Pemohon</dt>
       <dd class="col-sm-9">@Model.ApplicantFullName</dd>

       <dt class="col-sm-3">Sebab</dt>
       <dd class="col-sm-9">@Model.Justification</dd>

       <dt class="col-sm-3">Akses Dimohon</dt>
       <dd class="col-sm-9">
           <ul>
               @foreach (var access in Model.RequestedSystemAccesses)
               {
                   <li>@access.AccessType.Name @(string.IsNullOrEmpty(access.SystemName) ? "" : $"— {access.SystemName}")</li>
               }
           </ul>
       </dd>
   </dl>

   @if (Model.Submission.Status == Nres.Onboarding.Web.Models.SubmissionStatus.Draft)
   {
       <form asp-action="Submit" asp-route-id="@Model.Id" method="post">
           <button type="submit" class="btn btn-primary">Hantar Permohonan</button>
       </form>
   }
   ```

✅ **Semakan:** Tekan "Hantar Permohonan" pada permohonan draf → status bertukar `Submitted`, nombor rujukan format `ICT-ID-2026-####` terpapar, `AuditLog` "StatusChanged:Draft->Submitted" wujud.

---

## Latihan 4 — Skrin Kelulusan Penyelia

**Objektif:** Cipta controller & view khas untuk Penyelia semak dan luluskan/tolak permohonan, dikuatkuasakan `[Authorize(Roles = "Supervisor")]`.

1. Cipta `Controllers/AccountRequestApprovalsController.cs`:

   ```csharp
   using Microsoft.AspNetCore.Authorization;
   using Microsoft.AspNetCore.Mvc;
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Data;
   using Nres.Onboarding.Web.Models;
   using Nres.Onboarding.Web.Services;

   namespace Nres.Onboarding.Web.Controllers;

   [Authorize]
   public class AccountRequestApprovalsController(
       ApplicationDbContext db,
       IWorkflowService workflowService,
       IAuditLogService auditLogService,
       ICurrentUserService currentUserService) : Controller
   {
       [Authorize(Roles = "Supervisor")]
       public async Task<IActionResult> SupervisorPending()
       {
           var pending = await db.AccountRequests
               .Include(x => x.Submission)
               .Where(x => x.SupervisorUserId == currentUserService.UserId
                        && x.Submission.Status == SubmissionStatus.Submitted)
               .OrderBy(x => x.Submission.SubmittedAt)
               .ToListAsync();

           return View(pending);
       }

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

           return RedirectToAction(nameof(SupervisorPending));
       }

       private async Task<AccountRequest?> LoadForSupervisorAsync(int id)
       {
           return await db.AccountRequests
               .Include(x => x.Submission)
               .FirstOrDefaultAsync(x => x.Id == id && x.SupervisorUserId == currentUserService.UserId);
       }
   }
   ```

   > **Kenapa `remarks` wajib pada `SupervisorReject`?** Sama seperti Modul 1/2 — penolakan **mesti** ada sebab, supaya pemohon faham apa perlu diperbetulkan, dan rekod audit lengkap.

2. Cipta `Views/AccountRequestApprovals/SupervisorPending.cshtml`:

   ```cshtml
   @model List<Nres.Onboarding.Web.Models.AccountRequest>
   @{
       ViewData["Title"] = "Kelulusan Penyelia — Menunggu Tindakan Saya";
   }

   <h1>Menunggu Kelulusan Penyelia</h1>

   @if (TempData["Error"] is string error)
   {
       <div class="alert alert-danger">@error</div>
   }

   <table class="table">
       <thead>
           <tr>
               <th>No. Rujukan</th>
               <th>Pemohon</th>
               <th>Jenis</th>
               <th>Tindakan</th>
           </tr>
       </thead>
       <tbody>
           @foreach (var item in Model)
           {
               <tr>
                   <td>@item.Submission.ReferenceNo</td>
                   <td>@item.ApplicantFullName</td>
                   <td>@item.RequestType</td>
                   <td>
                       <form asp-action="SupervisorApprove" asp-route-id="@item.Id" method="post" class="d-inline">
                           <button type="submit" class="btn btn-success btn-sm">Luluskan</button>
                       </form>
                       <form asp-action="SupervisorReject" asp-route-id="@item.Id" method="post" class="d-inline">
                           <input type="text" name="remarks" placeholder="Sebab tolak" class="form-control form-control-sm d-inline w-auto" />
                           <button type="submit" class="btn btn-danger btn-sm">Tolak</button>
                       </form>
                   </td>
               </tr>
           }
       </tbody>
   </table>
   ```

✅ **Semakan:** Log masuk sebagai pengguna dalam role `Supervisor`, akses `/AccountRequestApprovals/SupervisorPending`, dan sahkan permohonan `Submitted` di mana anda ialah `SupervisorUserId` sahaja yang kelihatan. Cuba luluskan satu — status bertukar `SupervisorApproved`, `ApprovalStep` StepOrder 1 direkod.

---

## Latihan 5 — Skrin Pemprosesan ICT

**Objektif:** Tambah skrin ICT, dikuatkuasakan `[Authorize(Roles = "IctAdmin")]`.

1. Tambah aksyen ICT dalam `Controllers/AccountRequestApprovalsController.cs` (di bawah aksyen Supervisor):

   ```csharp
       [Authorize(Roles = "IctAdmin")]
       public async Task<IActionResult> IctPending()
       {
           var pending = await db.AccountRequests
               .Include(x => x.Submission)
               .Where(x => x.Submission.Status == SubmissionStatus.SupervisorApproved)
               .OrderBy(x => x.Submission.SubmittedAt)
               .ToListAsync();

           return View(pending);
       }

       [Authorize(Roles = "IctAdmin")]
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> Complete(int id)
       {
           var accountRequest = await db.AccountRequests
               .Include(x => x.Submission)
               .FirstOrDefaultAsync(x => x.Id == id);

           if (accountRequest is null)
           {
               return NotFound();
           }

           if (accountRequest.Submission.Status != SubmissionStatus.SupervisorApproved)
           {
               TempData["Error"] = "Hanya permohonan berstatus SupervisorApproved boleh disempurnakan ICT.";
               return RedirectToAction(nameof(IctPending));
           }

           await workflowService.TransitionAsync(
               accountRequest.Submission,
               SubmissionStatus.Completed,
               currentUserService.UserId,
               "Disempurnakan oleh ICT. Kata laluan diserahkan secara berasingan di luar sistem.");

           db.ApprovalSteps.Add(new ApprovalStep
           {
               SubmissionId = accountRequest.SubmissionId,
               StepOrder = 2,
               ApproverRole = "IctAdmin",
               Status = ApprovalStepStatus.Approved,
               ActorUserId = currentUserService.UserId,
               DecidedAt = DateTime.UtcNow,
           });
           await db.SaveChangesAsync();

           return RedirectToAction(nameof(IctPending));
       }

       [Authorize(Roles = "IctAdmin")]
       [HttpPost]
       [ValidateAntiForgeryToken]
       public async Task<IActionResult> IctReject(int id, string remarks)
       {
           if (string.IsNullOrWhiteSpace(remarks))
           {
               TempData["Error"] = "Sebab penolakan wajib diisi.";
               return RedirectToAction(nameof(IctPending));
           }

           var accountRequest = await db.AccountRequests
               .Include(x => x.Submission)
               .FirstOrDefaultAsync(x => x.Id == id);

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
               StepOrder = 2,
               ApproverRole = "IctAdmin",
               Status = ApprovalStepStatus.Rejected,
               ActorUserId = currentUserService.UserId,
               DecidedAt = DateTime.UtcNow,
               Remarks = remarks,
           });
           await db.SaveChangesAsync();

           return RedirectToAction(nameof(IctPending));
       }
   ```

2. Cipta `Views/AccountRequestApprovals/IctPending.cshtml` (sama struktur seperti `SupervisorPending.cshtml`, tukar nama aksyen kepada `Complete`/`IctReject`):

   ```cshtml
   @model List<Nres.Onboarding.Web.Models.AccountRequest>
   @{
       ViewData["Title"] = "Pemprosesan ICT — Menunggu Tindakan";
   }

   <h1>Menunggu Pemprosesan ICT</h1>

   @if (TempData["Error"] is string error)
   {
       <div class="alert alert-danger">@error</div>
   }

   <table class="table">
       <thead>
           <tr>
               <th>No. Rujukan</th>
               <th>Pemohon</th>
               <th>Jenis</th>
               <th>Tindakan</th>
           </tr>
       </thead>
       <tbody>
           @foreach (var item in Model)
           {
               <tr>
                   <td>@item.Submission.ReferenceNo</td>
                   <td>@item.ApplicantFullName</td>
                   <td>@item.RequestType</td>
                   <td>
                       <form asp-action="Complete" asp-route-id="@item.Id" method="post" class="d-inline">
                           <button type="submit" class="btn btn-success btn-sm">Sempurnakan</button>
                       </form>
                       <form asp-action="IctReject" asp-route-id="@item.Id" method="post" class="d-inline">
                           <input type="text" name="remarks" placeholder="Sebab tolak" class="form-control form-control-sm d-inline w-auto" />
                           <button type="submit" class="btn btn-danger btn-sm">Tolak</button>
                       </form>
                   </td>
               </tr>
           }
       </tbody>
   </table>
   ```

3. Tambah pautan navigasi ke kedua-dua senarai dalam dashboard Hari 7 (`Views/IctDashboard/Index.cshtml`):

   ```cshtml
   <div class="mt-3">
       <a class="btn btn-outline-secondary" asp-controller="AccountRequestApprovals" asp-action="SupervisorPending">
           Senarai Kelulusan Penyelia
       </a>
       <a class="btn btn-outline-secondary" asp-controller="AccountRequestApprovals" asp-action="IctPending">
           Senarai Pemprosesan ICT
       </a>
   </div>
   ```

✅ **Semakan:** Log masuk sebagai pengguna role `IctAdmin`, akses `/AccountRequestApprovals/IctPending`, sempurnakan satu permohonan `SupervisorApproved` — status bertukar `Completed`, `ApprovalStep` StepOrder 2 direkod.

---

## Latihan 6 — Uji Authorization (Pengujian Manual)

**Objektif:** Sahkan `[Authorize(Roles=...)]` benar-benar menyekat, bukan sekadar sembunyi butang.

1. Log masuk sebagai pengguna dalam role **`Applicant`** sahaja (tiada role `Supervisor`/`IctAdmin`).
2. Cuba navigasi terus ke URL `/AccountRequestApprovals/SupervisorPending`.
3. Perhatikan respons: sepatutnya **403 Forbidden** atau redirect ke halaman "Access Denied" (bergantung konfigurasi `AccessDeniedPath` Identity anda) — **bukan** senarai permohonan.
4. Ulang untuk `/AccountRequestApprovals/IctPending`.
5. Log masuk semula sebagai `Supervisor`, cuba akses `/AccountRequestApprovals/IctPending` — sepatutnya **juga ditolak** (Supervisor bukan IctAdmin).

✅ **Semakan:** Kedua-dua percubaan akses tanpa kebenaran **ditolak** oleh ASP.NET Core sebelum sebarang kod controller dijalankan. Ini buktikan authorization dikuatkuasakan di peringkat pipeline, bukan logik UI semata-mata.

---

## Rujukan Fail

| Bahagian lab | Fail rujukan (`projek/`) |
|---|---|
| Borang & view model (Latihan 1) | `projek/Nres.Onboarding.Web/ViewModels/AccountRequestCreateViewModel.cs`, `Controllers/AccountRequestsController.cs` |
| `IWorkflowService` (Latihan 2) | `projek/Nres.Onboarding.Web/Services/IWorkflowService.cs`, `WorkflowService.cs` |
| Submit (Latihan 3) | `projek/Nres.Onboarding.Web/Views/AccountRequests/Details.cshtml` |
| Kelulusan Penyelia (Latihan 4) | `projek/Nres.Onboarding.Web/Controllers/AccountRequestApprovalsController.cs` |
| Pemprosesan ICT (Latihan 5) | sama fail, bahagian `IctPending`/`Complete`/`IctReject` |

---

## Cabaran (Pilihan)

1. Tulis satu kaedah tambahan `IWorkflowService.GetAllowedNextStatuses(SubmissionStatus current)` yang memulangkan `IReadOnlyList<SubmissionStatus>` — berguna untuk papar butang tindakan secara dinamik di UI (bukan hardcode setiap butang).
2. Fikirkan: kalau permohonan ditolak ICT (`SupervisorApproved → Rejected`), patutkah pemohon boleh **hantar semula** permohonan yang sama, atau perlu cipta permohonan baharu? Cuba tambah aksyen `Reopen` yang membenarkan `Rejected → Draft` khusus untuk pemohon asal — kemas kini `AllowedTransitions` dengan sewajarnya.
3. Cuba log masuk sebagai `IctAdmin` dan cuba akses `/AccountRequests/Create` — patutkah ICT admin dibenarkan cipta permohonan bagi pihak staf lain? Bincang dengan fasilitator.

---

> 🎤 **Nota penceramah/jurulatih:** [`../nota-penceramah.md`](../nota-penceramah.md) untuk pemasaan, poin bercakap, dan silap biasa peserta Hari 8.
