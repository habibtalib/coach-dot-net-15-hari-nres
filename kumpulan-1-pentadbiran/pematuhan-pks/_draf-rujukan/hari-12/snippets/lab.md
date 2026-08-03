# Lab Hari 12 — PKS: Semakan Admin & Laporan

Lab ini mengiringi [`../README.md`](../README.md) Hari 12. Ikut latihan **secara berurutan**. Rujuk projek rujukan penuh di [`../../projek/`](../../projek/) untuk **banding** kod anda selepas cuba sendiri dahulu.

> **Peraturan lab:** Taip kod **sendiri**. Pastikan [Hari 11](../../hari-11/) (borang checklist + kunci) sudah **selesai**, dan anda mempunyai **sekurang-kurangnya 2–3** declaration ujian yang telah dihantar (log masuk sebagai beberapa akaun ujian berbeza dan hantar declaration bagi setiap satu) supaya senarai admin ada data untuk ditapis.

---

## Senarai Semak Pra-Syarat

- [ ] Sekurang-kurangnya 2–3 `ComplianceDeclaration` wujud dalam pangkalan data (hasil ujian Hari 11).
- [ ] Akaun ujian dengan peranan `ComplianceAdmin` wujud — jika belum, tambah melalui `RoleManager<IdentityRole>`/`UserManager<IdentityUser>` (rujuk pendaftaran peranan Hari 1/Hari 8).
- [ ] `IWorkflowService` dan `IAuditLogService` sudah didaftar dalam `Program.cs` (`builder.Services.AddScoped<IWorkflowService, WorkflowService>();` dan servis audit — sepatutnya sudah wujud sejak Hari 3/Hari 8).

---

## Latihan 1 — View Models Senarai & Penapis Admin

**Objektif:** Tulis view model bagi penapis (filter) dan setiap baris senarai admin.

1. Cipta fail `ViewModels/ComplianceAdminFilterViewModel.cs`:

   ```csharp
   using Microsoft.AspNetCore.Mvc.Rendering;
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.ViewModels;

   public class ComplianceAdminFilterViewModel
   {
       public string? Department { get; set; }

       public SubmissionStatus? Status { get; set; }

       public int? PolicyVersionId { get; set; }

       [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
       public DateTime? DateFrom { get; set; }

       [System.ComponentModel.DataAnnotations.DataType(System.ComponentModel.DataAnnotations.DataType.Date)]
       public DateTime? DateTo { get; set; }

       public List<SelectListItem> PolicyVersionOptions { get; set; } = new();

       public List<SelectListItem> DepartmentOptions { get; set; } = new();
   }
   ```

2. Cipta fail `ViewModels/ComplianceAdminListItemViewModel.cs`:

   ```csharp
   using Nres.Onboarding.Web.Models;

   namespace Nres.Onboarding.Web.ViewModels;

   public class ComplianceAdminListItemViewModel
   {
       public int Id { get; set; }

       public string ReferenceNo { get; set; } = string.Empty;

       public string ApplicantUserId { get; set; } = string.Empty;

       public string ApplicantName { get; set; } = string.Empty;

       public string Department { get; set; } = "-";

       public SubmissionStatus Status { get; set; }

       public string PolicyVersionTitle { get; set; } = string.Empty;

       public DateTime? DeclarationDate { get; set; }

       public int NonCompliantCount { get; set; }
   }

   public class ComplianceAdminIndexViewModel
   {
       public ComplianceAdminFilterViewModel Filter { get; set; } = new();

       public List<ComplianceAdminListItemViewModel> Items { get; set; } = new();
   }
   ```

   > **Kenapa `ApplicantName`/`Department` diisi selepas query (bukan sebahagian daripada query EF Core terus)?** `UserProfile` dan `ComplianceDeclaration` **tidak** mempunyai hubungan navigasi langsung dalam EF Core (kita pautkan secara manual melalui `ApplicantUserId`/`UserId`, dua rentetan biasa) — ini sengaja supaya `ApplicationDbContext` tidak perlu tahu bahawa `UserProfile` "dimiliki" oleh mana-mana modul tertentu. Lab Latihan 2 akan tunjukkan cara gabungkan (*join*) kedua-dua set data ini di peringkat C#/LINQ selepas query asas.

✅ **Semakan:** Dua fail view model wujud dalam `ViewModels/`, `dotnet build` berjaya.

---

## Latihan 2 — `ComplianceAdminController`: Senarai & Penapis

**Objektif:** Tulis controller admin dengan kaedah `Index` yang menyokong penapisan jabatan, status, versi polisi, dan julat tarikh.

1. Cipta fail `Controllers/ComplianceAdminController.cs`:

   ```csharp
   using Microsoft.AspNetCore.Authorization;
   using Microsoft.AspNetCore.Mvc;
   using Microsoft.AspNetCore.Mvc.Rendering;
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Data;
   using Nres.Onboarding.Web.Models;
   using Nres.Onboarding.Web.Services;
   using Nres.Onboarding.Web.ViewModels;

   namespace Nres.Onboarding.Web.Controllers;

   [Authorize(Roles = "ComplianceAdmin")]
   public class ComplianceAdminController : Controller
   {
       private readonly ApplicationDbContext _context;
       private readonly IAuditLogService _auditLogService;
       private readonly IWorkflowService _workflowService;

       public ComplianceAdminController(
           ApplicationDbContext context,
           IAuditLogService auditLogService,
           IWorkflowService workflowService)
       {
           _context = context;
           _auditLogService = auditLogService;
           _workflowService = workflowService;
       }

       // GET: /ComplianceAdmin/Index
       public async Task<IActionResult> Index(ComplianceAdminFilterViewModel filter)
       {
           var items = await BuildFilteredQuery(filter)
               .OrderByDescending(d => d.DeclarationDate)
               .Select(d => new ComplianceAdminListItemViewModel
               {
                   Id = d.Id,
                   ReferenceNo = d.Submission.ReferenceNo,
                   ApplicantUserId = d.Submission.ApplicantUserId,
                   Status = d.Submission.Status,
                   PolicyVersionTitle = d.PolicyVersion.Title,
                   DeclarationDate = d.DeclarationDate,
                   NonCompliantCount = d.Responses.Count(r => !r.IsCompliant)
               })
               .ToListAsync();

           await AttachApplicantProfilesAsync(items);

           filter.PolicyVersionOptions = await _context.PolicyVersions
               .OrderByDescending(p => p.EffectiveDate)
               .Select(p => new SelectListItem($"{p.VersionCode} — {p.Title}", p.Id.ToString()))
               .ToListAsync();

           filter.DepartmentOptions = await _context.UserProfiles
               .Where(p => p.Department != null)
               .Select(p => p.Department!)
               .Distinct()
               .OrderBy(d => d)
               .Select(d => new SelectListItem(d, d))
               .ToListAsync();

           var viewModel = new ComplianceAdminIndexViewModel
           {
               Filter = filter,
               Items = items
           };

           return View(viewModel);
       }

       private IQueryable<ComplianceDeclaration> BuildFilteredQuery(ComplianceAdminFilterViewModel filter)
       {
           var query = _context.ComplianceDeclarations
               .Include(d => d.Submission)
               .Include(d => d.PolicyVersion)
               .Include(d => d.Responses)
               .AsQueryable();

           if (filter.Status.HasValue)
           {
               query = query.Where(d => d.Submission.Status == filter.Status.Value);
           }

           if (filter.PolicyVersionId.HasValue)
           {
               query = query.Where(d => d.PolicyVersionId == filter.PolicyVersionId.Value);
           }

           if (filter.DateFrom.HasValue)
           {
               query = query.Where(d => d.DeclarationDate >= filter.DateFrom.Value.Date);
           }

           if (filter.DateTo.HasValue)
           {
               var exclusiveEnd = filter.DateTo.Value.Date.AddDays(1);
               query = query.Where(d => d.DeclarationDate < exclusiveEnd);
           }

           if (!string.IsNullOrWhiteSpace(filter.Department))
           {
               var userIdsInDepartment = _context.UserProfiles
                   .Where(p => p.Department == filter.Department)
                   .Select(p => p.UserId);

               query = query.Where(d => userIdsInDepartment.Contains(d.Submission.ApplicantUserId));
           }

           return query;
       }

       private async Task AttachApplicantProfilesAsync(List<ComplianceAdminListItemViewModel> items)
       {
           var userIds = items.Select(i => i.ApplicantUserId).Distinct().ToList();

           var profiles = await _context.UserProfiles
               .Where(p => userIds.Contains(p.UserId))
               .ToDictionaryAsync(p => p.UserId);

           foreach (var item in items)
           {
               if (profiles.TryGetValue(item.ApplicantUserId, out var profile))
               {
                   item.ApplicantName = profile.FullName;
                   item.Department = profile.Department ?? "-";
               }
               else
               {
                   item.ApplicantName = item.ApplicantUserId;
               }
           }
       }
   }
   ```

   > **Kenapa `BuildFilteredQuery` dijadikan kaedah `private` berasingan, bukan ditulis terus dalam `Index`?** Latihan 5 (eksport CSV) memerlukan **penapisan yang sama tepat** seperti senarai admin — supaya CSV yang dieksport **sepadan** dengan apa yang admin sedang lihat di skrin. Mengasingkan logik penapisan membolehkan `Index` dan `ExportCsv` berkongsi **satu** sumber kebenaran bagi peraturan tapisan, bukan dua salinan yang boleh tersasar.

   > **Kenapa `filter.DateTo` diproses sebagai `< exclusiveEnd` (esoknya), bukan `<= filter.DateTo`?** `DeclarationDate` ialah `DateTime` (ada komponen masa, cth. `2026-03-15 14:32:00`). Jika kita guna `<= filter.DateTo` (yang nilainya `2026-03-15 00:00:00` — tengah malam), declaration yang dihantar **petang** `2026-03-15` akan **tertinggal** daripada hasil carian walaupun tarikhnya sepadan. Menggunakan `< tarikh esok tengah malam` merangkumi **seluruh** hari `DateTo` yang dipilih.

✅ **Semakan:** `dotnet build` berjaya. `ComplianceAdminController` wujud dengan kaedah `Index` yang menyokong 4 jenis penapis.

---

## Latihan 3 — View: Senarai Admin & Borang Penapis

**Objektif:** Bina halaman senarai admin dengan borang penapis (`GET`, supaya URL boleh dikongsi/bookmark) dan jadual hasil.

1. Cipta folder `Views/ComplianceAdmin/` dan fail `Views/ComplianceAdmin/Index.cshtml`:

   ```cshtml
   @model Nres.Onboarding.Web.ViewModels.ComplianceAdminIndexViewModel

   @{
       ViewData["Title"] = "Semakan PKS — Admin";
   }

   <h1>Semakan Pengisytiharan PKS</h1>

   @if (TempData["StatusMessage"] is string statusMessage)
   {
       <div class="alert alert-success">@statusMessage</div>
   }
   @if (TempData["ErrorMessage"] is string errorMessage)
   {
       <div class="alert alert-danger">@errorMessage</div>
   }

   <form asp-action="Index" method="get" class="row g-2 mb-3">
       <div class="col-md-3">
           <label class="form-label">Jabatan</label>
           <select asp-for="Filter.Department" asp-items="Model.Filter.DepartmentOptions" class="form-select">
               <option value="">-- Semua Jabatan --</option>
           </select>
       </div>
       <div class="col-md-2">
           <label class="form-label">Status</label>
           <select asp-for="Filter.Status" class="form-select">
               <option value="">-- Semua Status --</option>
               @foreach (var status in Enum.GetValues<Nres.Onboarding.Web.Models.SubmissionStatus>())
               {
                   <option value="@((int)status)">@status</option>
               }
           </select>
       </div>
       <div class="col-md-3">
           <label class="form-label">Versi Polisi</label>
           <select asp-for="Filter.PolicyVersionId" asp-items="Model.Filter.PolicyVersionOptions" class="form-select">
               <option value="">-- Semua Versi --</option>
           </select>
       </div>
       <div class="col-md-2">
           <label class="form-label">Dari Tarikh</label>
           <input asp-for="Filter.DateFrom" class="form-control" />
       </div>
       <div class="col-md-2">
           <label class="form-label">Hingga Tarikh</label>
           <input asp-for="Filter.DateTo" class="form-control" />
       </div>
       <div class="col-12">
           <button type="submit" class="btn btn-primary">Tapis</button>
           <a asp-action="Index" class="btn btn-outline-secondary">Set Semula</a>
           <a asp-action="ExportCsv" asp-all-route-data="@GetFilterRouteValues()" class="btn btn-outline-success">
               Eksport CSV
           </a>
       </div>
   </form>

   <table class="table table-striped">
       <thead>
           <tr>
               <th>No. Rujukan</th>
               <th>Pemohon</th>
               <th>Jabatan</th>
               <th>Versi Polisi</th>
               <th>Status</th>
               <th>Tarikh Pengisytiharan</th>
               <th>Item Tidak Patuh</th>
               <th></th>
           </tr>
       </thead>
       <tbody>
           @foreach (var item in Model.Items)
           {
               <tr class="@(item.NonCompliantCount > 0 ? "table-warning" : "")">
                   <td>@item.ReferenceNo</td>
                   <td>@item.ApplicantName</td>
                   <td>@item.Department</td>
                   <td>@item.PolicyVersionTitle</td>
                   <td>@item.Status</td>
                   <td>@item.DeclarationDate?.ToString("dd/MM/yyyy")</td>
                   <td>@item.NonCompliantCount</td>
                   <td>
                       <a asp-action="Details" asp-route-id="@item.Id" class="btn btn-sm btn-outline-primary">
                           Lihat
                       </a>
                   </td>
               </tr>
           }
           @if (Model.Items.Count == 0)
           {
               <tr>
                   <td colspan="8" class="text-center text-muted">Tiada pengisytiharan sepadan penapis semasa.</td>
               </tr>
           }
       </tbody>
   </table>

   @functions {
       private Dictionary<string, string?> GetFilterRouteValues()
       {
           return new Dictionary<string, string?>
           {
               ["Department"] = Model.Filter.Department,
               ["Status"] = Model.Filter.Status?.ToString(),
               ["PolicyVersionId"] = Model.Filter.PolicyVersionId?.ToString(),
               ["DateFrom"] = Model.Filter.DateFrom?.ToString("yyyy-MM-dd"),
               ["DateTo"] = Model.Filter.DateTo?.ToString("yyyy-MM-dd")
           };
       }
   }
   ```

   > **Kenapa borang penapis guna `method="get"`, bukan `method="post"`?** Penapisan senarai (baca sahaja, tiada perubahan data) sepatutnya boleh **dikongsi sebagai pautan** (cth. hantar URL kepada rakan sekerja "lihat semua PKS Jabatan Kewangan bulan ini") dan **disimpan sebagai bookmark**. Ini hanya mungkin dengan `GET` — nilai borang menjadi sebahagian daripada URL (query string), bukan tersembunyi dalam badan permintaan `POST`.

   > **Kenapa pautan "Eksport CSV" guna `asp-all-route-data` dengan kamus nilai penapis semasa?** Ini memastikan CSV yang dieksport **sepadan** dengan tapisan yang sedang admin lihat — bukan sentiasa eksport **semua** rekod tanpa mengira tapisan yang dipilih di skrin.

✅ **Semakan:** `/ComplianceAdmin/Index` papar senarai declaration, borang penapis berfungsi (cuba tapis ikut status/jabatan), dan baris dengan item Tidak Patuh ditanda warna amaran.

---

## Latihan 4 — Halaman Detail & Keputusan Semakan

**Objektif:** Papar respons checklist penuh + catatan ketidakpatuhan, dan benarkan `ComplianceAdmin` membuat keputusan (`AdminApproved`/`Rejected`) menggunakan `IWorkflowService`.

1. Tambah kaedah `Details` dan `Review` ke `Controllers/ComplianceAdminController.cs`:

   ```csharp
   // GET: /ComplianceAdmin/Details/5
   public async Task<IActionResult> Details(int id)
   {
       var declaration = await _context.ComplianceDeclarations
           .Include(d => d.Submission)
           .Include(d => d.PolicyVersion)
           .Include(d => d.Responses)
               .ThenInclude(r => r.ChecklistItem)
           .FirstOrDefaultAsync(d => d.Id == id);

       if (declaration is null)
       {
           return NotFound();
       }

       var profile = await _context.UserProfiles
           .FirstOrDefaultAsync(p => p.UserId == declaration.Submission.ApplicantUserId);

       ViewData["ApplicantName"] = profile?.FullName ?? declaration.Submission.ApplicantUserId;
       ViewData["Department"] = profile?.Department ?? "-";

       return View(declaration);
   }

   // POST: /ComplianceAdmin/Review/5
   [HttpPost]
   [ValidateAntiForgeryToken]
   public async Task<IActionResult> Review(int id, SubmissionStatus decision, string? remarks)
   {
       if (decision != SubmissionStatus.AdminApproved && decision != SubmissionStatus.Rejected)
       {
           return BadRequest("Keputusan semakan tidak sah.");
       }

       if (decision == SubmissionStatus.Rejected && string.IsNullOrWhiteSpace(remarks))
       {
           TempData["ErrorMessage"] = "Catatan ketidakpatuhan wajib diisi apabila menolak pengisytiharan.";
           return RedirectToAction(nameof(Details), new { id });
       }

       var declaration = await _context.ComplianceDeclarations
           .Include(d => d.Submission)
           .FirstOrDefaultAsync(d => d.Id == id);

       if (declaration is null)
       {
           return NotFound();
       }

       if (!_workflowService.CanTransition(declaration.Submission.Status, decision))
       {
           TempData["ErrorMessage"] =
               $"Peralihan status daripada {declaration.Submission.Status} ke {decision} tidak dibenarkan.";
           return RedirectToAction(nameof(Details), new { id });
       }

       declaration.Submission.Status = decision;
       if (decision == SubmissionStatus.AdminApproved)
       {
           declaration.Submission.CompletedAt = DateTime.UtcNow;
       }

       await _context.SaveChangesAsync();

       await _auditLogService.RecordAsync(declaration.SubmissionId, decision.ToString(), remarks);

       TempData["StatusMessage"] =
           $"Pengisytiharan {declaration.Submission.ReferenceNo} dikemas kini kepada status {decision}.";
       return RedirectToAction(nameof(Details), new { id });
   }
   ```

   > **Kenapa `Review` menyemak `_workflowService.CanTransition(...)` **sebelum** menukar `Submission.Status`, sedangkan kod di atasnya sudah menyemak `decision` mesti `AdminApproved`/`Rejected`?** Dua semakan ini berlainan tujuan: semakan pertama (`decision != AdminApproved && decision != Rejected`) menyekat **nilai input** yang tidak sah langsung (cth. seseorang cuba hantar `Draft` melalui manipulasi borang). Semakan kedua (`CanTransition`) menyekat **peralihan tidak sah dari segi peraturan perniagaan** — cth. declaration yang **sudah** `Rejected` tidak patut boleh ditukar terus ke `AdminApproved` tanpa proses semula yang betul. `IWorkflowService` ialah "penjaga pintu" peraturan perniagaan ini, dikongsi merentas semua modul sejak Hari 8.

2. Cipta fail `Views/ComplianceAdmin/Details.cshtml`:

   ```cshtml
   @model Nres.Onboarding.Web.Models.ComplianceDeclaration

   @{
       ViewData["Title"] = "Semakan PKS — " + Model.Submission.ReferenceNo;
       var applicantName = ViewData["ApplicantName"] as string;
       var department = ViewData["Department"] as string;
   }

   <h1>Semakan Pengisytiharan PKS</h1>

   @if (TempData["StatusMessage"] is string statusMessage)
   {
       <div class="alert alert-success">@statusMessage</div>
   }
   @if (TempData["ErrorMessage"] is string errorMessage)
   {
       <div class="alert alert-danger">@errorMessage</div>
   }

   <dl class="row">
       <dt class="col-sm-3">Nombor Rujukan</dt>
       <dd class="col-sm-9">@Model.Submission.ReferenceNo</dd>

       <dt class="col-sm-3">Pemohon</dt>
       <dd class="col-sm-9">@applicantName (@department)</dd>

       <dt class="col-sm-3">Status Semasa</dt>
       <dd class="col-sm-9">@Model.Submission.Status</dd>

       <dt class="col-sm-3">Versi Polisi</dt>
       <dd class="col-sm-9">@Model.PolicyVersion.Title (@Model.PolicyVersion.VersionCode)</dd>

       <dt class="col-sm-3">Tarikh Pengisytiharan</dt>
       <dd class="col-sm-9">@Model.DeclarationDate?.ToString("dd/MM/yyyy HH:mm")</dd>
   </dl>

   <table class="table table-bordered">
       <thead>
           <tr>
               <th style="width: 4%">#</th>
               <th style="width: 50%">Perkara Pematuhan</th>
               <th style="width: 12%">Status</th>
               <th>Catatan Ketidakpatuhan</th>
           </tr>
       </thead>
       <tbody>
           @foreach (var response in Model.Responses.OrderBy(r => r.ChecklistItem.SequenceNo))
           {
               <tr class="@(response.IsCompliant ? "" : "table-danger")">
                   <td>@response.ChecklistItem.SequenceNo</td>
                   <td>@response.ChecklistItem.Statement</td>
                   <td>@(response.IsCompliant ? "Patuh" : "Tidak Patuh")</td>
                   <td>@(response.Remarks ?? "-")</td>
               </tr>
           }
       </tbody>
   </table>

   @if (Model.Submission.Status == Nres.Onboarding.Web.Models.SubmissionStatus.Submitted)
   {
       <form asp-action="Review" asp-route-id="@Model.Id" method="post" class="border rounded p-3">
           <h5>Keputusan Semakan</h5>
           <div class="mb-3">
               <label class="form-label">Catatan (wajib jika Tolak)</label>
               <textarea name="remarks" class="form-control" rows="3"></textarea>
           </div>
           <button type="submit" name="decision" value="@Nres.Onboarding.Web.Models.SubmissionStatus.AdminApproved"
                   class="btn btn-success">
               Lulus (Patuh)
           </button>
           <button type="submit" name="decision" value="@Nres.Onboarding.Web.Models.SubmissionStatus.Rejected"
                   class="btn btn-danger">
               Tolak (Tidak Patuh)
           </button>
       </form>
   }
   else
   {
       <div class="alert alert-secondary">
           Pengisytiharan ini telah disemak — status semasa: <strong>@Model.Submission.Status</strong>.
       </div>
   }
   ```

   > **Kenapa borang keputusan semakan hanya dipapar jika `Model.Submission.Status == SubmissionStatus.Submitted`?** Selepas `ComplianceAdmin` membuat keputusan (`AdminApproved`/`Rejected`), declaration itu tidak patut disemak **dua kali** secara tidak sengaja. Menyembunyikan borang keputusan apabila status sudah bertukar ialah lapisan pertahanan UI **tambahan** — lapisan sebenar tetap `IWorkflowService.CanTransition` dalam controller (Latihan 4, langkah 1), yang menolak percubaan menukar status walaupun seseorang menghantar `POST` terus tanpa melalui UI.

✅ **Semakan:** Halaman detail papar semua respons checklist (baris Tidak Patuh ditanda merah), dan `ComplianceAdmin` boleh klik "Lulus"/"Tolak". Cuba klik "Tolak" **tanpa** catatan — sahkan ditolak dengan mesej ralat. Selepas keputusan dibuat, borang keputusan hilang dan status dipaparkan sahaja.

---

## Latihan 5 — Eksport CSV

**Objektif:** Jana fail CSV pematuhan menggunakan corak rasmi kursus (`../../templates/csv-export.cs.txt`), tertakluk kepada penapisan yang sama seperti senarai admin.

1. Tambah `using System.Text;` di bahagian atas `Controllers/ComplianceAdminController.cs`.

2. Tambah kaedah `ExportCsv`:

   ```csharp
   // GET: /ComplianceAdmin/ExportCsv
   public async Task<IActionResult> ExportCsv(ComplianceAdminFilterViewModel filter)
   {
       var rows = await BuildFilteredQuery(filter)
           .OrderBy(d => d.DeclarationDate)
           .Select(d => new
           {
               d.Submission.ReferenceNo,
               d.Submission.ApplicantUserId,
               d.Submission.Status,
               d.DeclarationDate
           })
           .ToListAsync();

       var userIds = rows.Select(r => r.ApplicantUserId).Distinct().ToList();
       var profiles = await _context.UserProfiles
           .Where(p => userIds.Contains(p.UserId))
           .ToDictionaryAsync(p => p.UserId);

       var sb = new StringBuilder();
       sb.AppendLine("ReferenceNo,Applicant,Department,Status,DeclarationDate");
       foreach (var r in rows)
       {
           profiles.TryGetValue(r.ApplicantUserId, out var profile);
           var applicant = profile?.FullName ?? r.ApplicantUserId;
           var department = profile?.Department ?? "-";
           var declarationDate = r.DeclarationDate?.ToString("yyyy-MM-dd") ?? "-";

           sb.AppendLine($"{Csv(r.ReferenceNo)},{Csv(applicant)},{Csv(department)},{r.Status},{declarationDate}");
       }

       return File(Encoding.UTF8.GetBytes(sb.ToString()), "text/csv", "pks-export.csv");

       static string Csv(string? v) => v is null ? "" : $"\"{v.Replace("\"", "\"\"")}\"";
   }
   ```

   > **Kenapa lajur `Status` (enum `SubmissionStatus`) **tidak** dibungkus dengan `Csv(...)`?** `Csv(...)` membungkus nilai dengan tanda petik `"..."` dan melepaskan (*escape*) tanda petik dalaman — ini hanya perlu untuk teks **bebas** yang mungkin mengandungi koma/petikan (nama pemohon, nama jabatan). Nilai `Status` ialah nama enum tetap (`Submitted`, `AdminApproved`, dsb.) yang **tidak pernah** mengandungi koma, jadi ia selamat ditulis terus tanpa pembungkusan.

   > **Kenapa fungsi tempatan (*local function*) `Csv(...)` ditulis di **bawah** `return`, bukan di atas?** Dalam C#, fungsi tempatan boleh diisytiharkan di mana-mana sahaja dalam kaedah induknya dan tetap boleh dipanggil **sebelum** pengisytiharannya (*hoisting*) — ini gaya biasa untuk meletakkan fungsi pembantu kecil "di bawah sekali" supaya logik utama kaedah kekal jelas dibaca dari atas.

3. Jalankan `dotnet run`, navigasi ke `/ComplianceAdmin/Index`, cuba tapis (cth. ikut status `AdminApproved` sahaja), kemudian klik "Eksport CSV". Fail `pks-export.csv` patut dimuat turun.

4. Buka fail CSV yang dimuat turun (Excel/teks editor) dan sahkan baris pertama tepat:

   ```csv
   ReferenceNo,Applicant,Department,Status,DeclarationDate
   ```

✅ **Semakan:** Klik "Eksport CSV" memuat turun fail `pks-export.csv` dengan header tepat `ReferenceNo,Applicant,Department,Status,DeclarationDate`, dan baris data sepadan dengan hasil tapisan semasa (bukan semua rekod tanpa mengira tapisan).

---

## Rujukan Fail Sebenar

| Fail anda (lab) | Fail rujukan (projek sebenar) |
|------------------|-------------------------------|
| `ViewModels/ComplianceAdminFilterViewModel.cs`, `ComplianceAdminListItemViewModel.cs` | `projek/Nres.Onboarding.Web/ViewModels/` |
| `Controllers/ComplianceAdminController.cs` | `projek/Nres.Onboarding.Web/Controllers/` |
| `Views/ComplianceAdmin/Index.cshtml`, `Details.cshtml` | `projek/Nres.Onboarding.Web/Views/ComplianceAdmin/` |
| Corak eksport CSV | [`../../templates/csv-export.cs.txt`](../../templates/csv-export.cs.txt) |

---

## Cabaran (Pilihan)

1. **Ringkasan statistik jabatan** — Tambah bahagian "Ringkasan" di atas jadual `Index.cshtml`, memaparkan jumlah declaration mengikut status (`Submitted`, `AdminApproved`, `Rejected`) bagi hasil tapisan semasa, menggunakan `GroupBy`.
2. **Paginasi** — Jika data ujian anda banyak, tambah `Skip`/`Take` pada `BuildFilteredQuery` hasil dalam `Index`, dengan parameter `page` (lalai `1`, 20 rekod setiap muka surat).
3. **Panel Audit pada Detail** — Dalam `Views/ComplianceAdmin/Details.cshtml`, tambah jadual kecil memaparkan `AuditLog` berkaitan `Model.SubmissionId` (query `_context.AuditLogs.Where(a => a.SubmissionId == Model.SubmissionId).OrderByDescending(a => a.CreatedAt)`), supaya admin nampak sejarah lengkap (Submitted → AdminApproved/Rejected) dalam satu skrin.

---

Nota penceramah (pemasaan sesi, silap biasa, soalan perbincangan, deliverable akhir hari): [`../nota-penceramah.md`](../nota-penceramah.md).
