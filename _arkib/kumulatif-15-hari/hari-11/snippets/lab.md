# Lab Hari 11 — PKS: Borang Checklist Dinamik & Kunci

Lab ini mengiringi [`../README.md`](../README.md) Hari 11. Ikut latihan **secara berurutan**. Rujuk projek rujukan penuh di [`../../projek/`](../../projek/) untuk **banding** kod anda selepas cuba sendiri dahulu.

> **Peraturan lab:** Taip kod **sendiri**. Pastikan [Hari 10](../../hari-10/) (entiti PKS + migration `Module4Initial` + seed) sudah **selesai** sebelum mula — hari ini bergantung sepenuhnya kepada data seed itu.

---

## Senarai Semak Pra-Syarat

- [ ] Migration `Module4Initial` telah dijalankan (`dotnet ef database update` berjaya) — rujuk [Hari 10](../../hari-10/snippets/lab.md).
- [ ] Jadual `PolicyVersions` mengandungi 1 rekod aktif; `ComplianceChecklistItems` mengandungi 6 rekod.
- [ ] Anda boleh log masuk sebagai pengguna ujian (mana-mana akaun Identity sedia ada daripada modul sebelumnya) untuk uji borang sebagai `Applicant`.

---

## Latihan 1 — `ComplianceDeclarationViewModel` & `ComplianceResponseInput`

**Objektif:** Tulis view model borang checklist dinamik, mengikut nama sifat yang ditetapkan dalam `README.md`.

1. Cipta fail `ViewModels/ComplianceDeclarationViewModel.cs`:

   ```csharp
   using System.ComponentModel.DataAnnotations;

   namespace Nres.Onboarding.Web.ViewModels;

   public class ComplianceDeclarationViewModel
   {
       public int PolicyVersionId { get; set; }

       public string PolicyVersionTitle { get; set; } = string.Empty;

       [Display(Name = "Saya mengesahkan akuan ini benar")]
       public bool IsAcknowledged { get; set; }

       public List<ComplianceResponseInput> Responses { get; set; } = new();
   }

   public class ComplianceResponseInput
   {
       public int ChecklistItemId { get; set; }

       public string Statement { get; set; } = string.Empty;

       public bool IsCompliant { get; set; } = true;

       [StringLength(1000)]
       public string? Remarks { get; set; }
   }
   ```

   > **Kenapa `IsCompliant` lalai `true`?** Andaian pemohon **patuh** bagi setiap perkara melainkan ditanda sebaliknya — ini mengurangkan bilangan klik untuk kes biasa (kebanyakan staf patuh sepenuhnya), tetapi tetap membenarkan penandaan `false` bagi perkara tertentu.

✅ **Semakan:** `ViewModels/ComplianceDeclarationViewModel.cs` wujud dengan dua kelas (`ComplianceDeclarationViewModel`, `ComplianceResponseInput`), `dotnet build` berjaya.

---

## Latihan 2 — `ComplianceController`: Muat Checklist Aktif (GET)

**Objektif:** Tulis controller yang memuat `PolicyVersion` aktif + checklist item aktifnya, dan menyekat capaian jika pemohon **sudah** mempunyai declaration.

1. Cipta fail `Controllers/ComplianceController.cs`:

   ```csharp
   using System.Security.Claims;
   using Microsoft.AspNetCore.Authorization;
   using Microsoft.AspNetCore.Mvc;
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Data;
   using Nres.Onboarding.Web.Models;
   using Nres.Onboarding.Web.Services;
   using Nres.Onboarding.Web.ViewModels;

   namespace Nres.Onboarding.Web.Controllers;

   [Authorize]
   public class ComplianceController : Controller
   {
       private readonly ApplicationDbContext _context;
       private readonly IReferenceNumberService _referenceNumberService;
       private readonly IAuditLogService _auditLogService;

       public ComplianceController(
           ApplicationDbContext context,
           IReferenceNumberService referenceNumberService,
           IAuditLogService auditLogService)
       {
           _context = context;
           _referenceNumberService = referenceNumberService;
           _auditLogService = auditLogService;
       }

       private string CurrentUserId =>
           User.FindFirstValue(ClaimTypes.NameIdentifier)
           ?? throw new InvalidOperationException("Pengguna tidak disahkan.");

       // GET: /Compliance/Create
       public async Task<IActionResult> Create()
       {
           var existing = await _context.ComplianceDeclarations
               .Include(d => d.Submission)
               .FirstOrDefaultAsync(d => d.Submission.ApplicantUserId == CurrentUserId);

           if (existing is not null)
           {
               // Sudah pernah mengisytiharkan PKS — declaration terkunci, terus ke paparan sahaja.
               return RedirectToAction(nameof(Details), new { id = existing.Id });
           }

           var activePolicy = await _context.PolicyVersions
               .Include(p => p.ChecklistItems.Where(i => i.IsActive))
               .Where(p => p.IsActive)
               .OrderByDescending(p => p.EffectiveDate)
               .FirstOrDefaultAsync();

           if (activePolicy is null)
           {
               return Problem("Tiada versi polisi PKS aktif dijumpai. Sila hubungi SystemAdmin.");
           }

           var viewModel = BuildViewModel(activePolicy);
           return View(viewModel);
       }

       private static ComplianceDeclarationViewModel BuildViewModel(PolicyVersion policy)
       {
           return new ComplianceDeclarationViewModel
           {
               PolicyVersionId = policy.Id,
               PolicyVersionTitle = policy.Title,
               Responses = policy.ChecklistItems
                   .OrderBy(i => i.SequenceNo)
                   .Select(i => new ComplianceResponseInput
                   {
                       ChecklistItemId = i.Id,
                       Statement = i.Statement,
                       IsCompliant = true
                   })
                   .ToList()
           };
       }
   }
   ```

   > **Kenapa semak `existing` (declaration sedia ada) di **awal** `Create()` GET, bukan hanya semasa `POST`?** Jika kita hanya semak semasa `POST`, pemohon yang sudah mengisytiharkan PKS masih boleh **melihat** borang kosong (mengelirukan) dan cuma ditolak selepas tekan hantar. Menyemak di GET memberi pengalaman yang lebih jelas — pemohon terus dibawa ke paparan (terkunci) rekod sedia ada.

✅ **Semakan:** `dotnet build` berjaya. Navigasi ke `/Compliance/Create` (selepas log masuk) sepatutnya cuba render `View(viewModel)` — kita belum tulis Razor view, jadi ralat "view tidak dijumpai" adalah **dijangka** buat masa ini; ini dibetulkan Latihan 3.

---

## Latihan 3 — Razor View: Borang Checklist Dinamik

**Objektif:** Render `List<ComplianceResponseInput>` sebagai jadual borang, dengan medan hidden `ChecklistItemId` bagi setiap baris.

1. Cipta folder `Views/Compliance/` dan fail `Views/Compliance/Create.cshtml`:

   ```cshtml
   @model Nres.Onboarding.Web.ViewModels.ComplianceDeclarationViewModel

   @{
       ViewData["Title"] = "Pengisytiharan PKS";
   }

   <h1>Pengisytiharan Pematuhan Kod Setia (PKS)</h1>
   <p class="text-muted">Versi Polisi: <strong>@Model.PolicyVersionTitle</strong></p>

   <div asp-validation-summary="All" class="text-danger"></div>

   <form asp-action="Create" method="post">
       <input type="hidden" asp-for="PolicyVersionId" />
       <input type="hidden" asp-for="PolicyVersionTitle" />

       <table class="table table-bordered align-middle">
           <thead>
               <tr>
                   <th style="width: 4%">#</th>
                   <th style="width: 48%">Perkara Pematuhan</th>
                   <th style="width: 16%">Status</th>
                   <th>Catatan (wajib jika Tidak Patuh)</th>
               </tr>
           </thead>
           <tbody>
               @for (var i = 0; i < Model.Responses.Count; i++)
               {
                   <tr>
                       <td>@(i + 1)</td>
                       <td>
                           @Model.Responses[i].Statement
                           <input type="hidden" asp-for="Responses[i].ChecklistItemId" />
                           <input type="hidden" asp-for="Responses[i].Statement" />
                       </td>
                       <td>
                           <div class="form-check">
                               <input class="form-check-input" type="radio"
                                      asp-for="Responses[i].IsCompliant" value="true" />
                               <label class="form-check-label">Patuh</label>
                           </div>
                           <div class="form-check">
                               <input class="form-check-input" type="radio"
                                      asp-for="Responses[i].IsCompliant" value="false" />
                               <label class="form-check-label">Tidak Patuh</label>
                           </div>
                       </td>
                       <td>
                           <textarea asp-for="Responses[i].Remarks" class="form-control" rows="2"
                                     placeholder="Jelaskan sebab ketidakpatuhan (jika berkaitan)"></textarea>
                       </td>
                   </tr>
               }
           </tbody>
       </table>

       <div class="form-check mb-3">
           <input class="form-check-input" type="checkbox" asp-for="IsAcknowledged" />
           <label class="form-check-label" asp-for="IsAcknowledged"></label>
           <span asp-validation-for="IsAcknowledged" class="text-danger d-block"></span>
       </div>

       <button type="submit" class="btn btn-primary">Hantar Pengisytiharan</button>
   </form>

   @section Scripts {
       <partial name="_ValidationScriptsPartial" />
   }
   ```

   > **Kenapa dua `<input type="radio">` berkongsi `asp-for="Responses[i].IsCompliant"` yang sama tetapi `value` berbeza (`"true"`/`"false"`) berfungsi tanpa `checked` manual?** Radio Tag Helper ASP.NET Core **automatik** menanda `checked` pada radio yang nilainya sepadan dengan nilai semasa model (`Model.Responses[i].IsCompliant`) — ini sebahagian daripada ciri **Tag Helper** yang menyelaraskan HTML dengan keadaan model tanpa kod C# tambahan dalam view.

   > **Kenapa `<input type="hidden" asp-for="Responses[i].Statement" />` turut disertakan?** Ini **bukan** untuk disimpan ke pangkalan data (`ComplianceResponse` tidak ada medan `Statement`) — ia hanya supaya, jika `POST` gagal validation dan borang perlu dipaparkan semula (Latihan 4), teks perkara checklist tidak hilang tanpa perlu query semula pangkalan data. Controller `POST` akan **mengabaikan** nilai ini semasa menyimpan (rujuk Latihan 4).

2. Jalankan `dotnet run`, log masuk, dan navigasi ke `/Compliance/Create`. Anda patut nampak jadual dengan **6 baris** (satu bagi setiap checklist item seed Hari 10), setiap satu dengan pilihan Patuh/Tidak Patuh dan ruang catatan.

✅ **Semakan:** Borang papar tepat 6 baris checklist (susunan ikut `SequenceNo`), radio button "Patuh" pra-pilih secara lalai, dan checkbox akuan wujud di bawah jadual.

---

## Latihan 4 — Simpan Dalam Satu Transaksi & Sahkan Akuan (POST)

**Objektif:** Tulis `POST Create` yang mengesahkan akuan, mencipta `Submission` + `ComplianceDeclaration` + semua `ComplianceResponse` dalam **satu** transaksi, dan menjana nombor rujukan `PKS-2026-####`.

1. Tambah kaedah berikut ke `Controllers/ComplianceController.cs` (selepas kaedah `Create()` GET):

   ```csharp
   // POST: /Compliance/Create
   [HttpPost]
   [ValidateAntiForgeryToken]
   public async Task<IActionResult> Create(ComplianceDeclarationViewModel model)
   {
       if (!model.IsAcknowledged)
       {
           ModelState.AddModelError(nameof(model.IsAcknowledged),
               "Anda mesti mengesahkan akuan sebelum menghantar pengisytiharan.");
       }

       foreach (var response in model.Responses)
       {
           if (!response.IsCompliant && string.IsNullOrWhiteSpace(response.Remarks))
           {
               ModelState.AddModelError(string.Empty,
                   $"Catatan wajib diisi bagi perkara \"{response.Statement}\" yang ditanda Tidak Patuh.");
           }
       }

       var alreadyDeclared = await _context.ComplianceDeclarations
           .Include(d => d.Submission)
           .AnyAsync(d => d.Submission.ApplicantUserId == CurrentUserId);

       if (alreadyDeclared)
       {
           ModelState.AddModelError(string.Empty,
               "Anda sudah menghantar pengisytiharan PKS. Rekod sedia ada tidak boleh disunting.");
       }

       if (!ModelState.IsValid)
       {
           return View(model);
       }

       await using var transaction = await _context.Database.BeginTransactionAsync();

       var submission = new Submission
       {
           ModuleCode = "PKS",
           ApplicantUserId = CurrentUserId,
           Status = SubmissionStatus.Submitted,
           SubmittedAt = DateTime.UtcNow
       };
       _context.Submissions.Add(submission);
       await _context.SaveChangesAsync();

       submission.ReferenceNo = await _referenceNumberService.GenerateAsync("PKS");

       var declaration = new ComplianceDeclaration
       {
           SubmissionId = submission.Id,
           PolicyVersionId = model.PolicyVersionId,
           IsAcknowledged = model.IsAcknowledged,
           DeclarationDate = DateTime.UtcNow,
           Responses = model.Responses.Select(r => new ComplianceResponse
           {
               ChecklistItemId = r.ChecklistItemId,
               IsCompliant = r.IsCompliant,
               Remarks = r.IsCompliant ? null : r.Remarks
           }).ToList()
       };
       _context.ComplianceDeclarations.Add(declaration);

       await _context.SaveChangesAsync();
       await transaction.CommitAsync();

       await _auditLogService.RecordAsync(
           submission.Id,
           "Submitted",
           $"Pengisytiharan PKS dihantar dengan {declaration.Responses.Count} respons checklist.");

       TempData["StatusMessage"] = $"Pengisytiharan PKS berjaya dihantar. Nombor rujukan: {submission.ReferenceNo}";
       return RedirectToAction(nameof(Details), new { id = declaration.Id });
   }
   ```

   > **Kenapa `_context.SaveChangesAsync()` dipanggil **dua kali** (selepas `Submission`, kemudian selepas `ComplianceDeclaration`), bukan sekali sahaja di penghujung?** `submission.Id` (kunci utama auto-generate) hanya **wujud** selepas `SaveChangesAsync()` pertama dijalankan — kita perlukan `submission.Id` untuk tetapkan `declaration.SubmissionId` sebelum simpan declaration. `IReferenceNumberService.GenerateAsync("PKS")` turut bergantung kepada `submission.CreatedAt`/kiraan sedia ada dalam jadual `Submissions`, jadi ia dipanggil **selepas** `Submission` asal disimpan.

   > **Kenapa panggilan kedua kepada `AnyAsync` (semak `alreadyDeclared`) walaupun sudah disemak dalam `Create()` GET?** Ini pertahanan terhadap **race condition** — jika dua tab pelayar/dua permintaan `POST` dihantar hampir serentak (jarang, tetapi mungkin), semakan GET sahaja tidak mencukupi. Semak semula sebelum simpan memastikan **tidak mungkin** dua `ComplianceDeclaration` wujud bagi pemohon yang sama walaupun dalam senario luar biasa ini (kekangan unik pada `SubmissionId`, Hari 10, turut menjadi lapisan pertahanan terakhir di peringkat pangkalan data).

   > **Kenapa `Remarks` ditetapkan `null` apabila `IsCompliant = true`?** Mengikut Hari 10, `Remarks` hanya relevan untuk item **Tidak Patuh** — walaupun pemohon secara tidak sengaja menaip sesuatu dalam ruang catatan bagi item yang ditanda Patuh, kita bersihkan (`null`)-kan supaya data kekal konsisten dengan peraturan perniagaan.

2. Jalankan `dotnet run`, isi borang (cuba tandakan **satu** item sebagai "Tidak Patuh" tanpa catatan dahulu — sahkan mesej ralat validation muncul), kemudian isi catatan dan hantar semula.

✅ **Semakan:** Borang menolak hantar jika akuan tidak ditanda, atau jika ada item Tidak Patuh tanpa catatan. Selepas hantar berjaya, `TempData["StatusMessage"]` papar nombor rujukan format `PKS-2026-0001`.

---

## Latihan 5 — Paparan Terkunci (`Details`) & Sekatan Edit

**Objektif:** Tulis halaman paparan (read-only) bagi declaration yang sudah dihantar, dan sahkan **tiada** laluan untuk menyuntingnya semula.

1. Tambah kaedah `Details` ke `Controllers/ComplianceController.cs`:

   ```csharp
   // GET: /Compliance/Details/5
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

       var isOwner = declaration.Submission.ApplicantUserId == CurrentUserId;
       if (!isOwner && !User.IsInRole("ComplianceAdmin"))
       {
           return Forbid();
       }

       return View(declaration);
   }
   ```

   > **Kenapa tiada kaedah `Edit`/`Update` langsung dalam controller ini?** Ini **bukan** kealpaan — rujuk perbincangan "kenapa declaration dikunci selepas dihantar" dalam `README.md`. Kerana declaration menjadi tidak boleh diubah (*immutable*) sebaik `Submitted`, tiada laluan `Edit` yang sepatutnya wujud pada peringkat controller — ini bentuk "kunci" yang paling kukuh: bukan sekadar sekat butang UI, tetapi **tiada langsung** kod bahagian pelayan (*server-side*) yang membenarkan operasi kemas kini.

2. Cipta fail `Views/Compliance/Details.cshtml`:

   ```cshtml
   @model Nres.Onboarding.Web.Models.ComplianceDeclaration

   @{
       ViewData["Title"] = "Pengisytiharan PKS — " + Model.Submission.ReferenceNo;
   }

   <h1>Pengisytiharan PKS</h1>

   @if (TempData["StatusMessage"] is string statusMessage)
   {
       <div class="alert alert-success">@statusMessage</div>
   }

   <div class="alert alert-secondary">
       Pengisytiharan ini telah <strong>dihantar</strong> dan <strong>dikunci</strong> — tidak boleh disunting.
   </div>

   <dl class="row">
       <dt class="col-sm-3">Nombor Rujukan</dt>
       <dd class="col-sm-9">@Model.Submission.ReferenceNo</dd>

       <dt class="col-sm-3">Status</dt>
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
               <th>Catatan</th>
           </tr>
       </thead>
       <tbody>
           @foreach (var response in Model.Responses.OrderBy(r => r.ChecklistItem.SequenceNo))
           {
               <tr class="@(response.IsCompliant ? "" : "table-warning")">
                   <td>@response.ChecklistItem.SequenceNo</td>
                   <td>@response.ChecklistItem.Statement</td>
                   <td>@(response.IsCompliant ? "Patuh" : "Tidak Patuh")</td>
                   <td>@(response.Remarks ?? "-")</td>
               </tr>
           }
       </tbody>
   </table>
   ```

3. Cipta folder `Views/Compliance/` sudah wujud daripada Latihan 3 — pastikan kedua-dua fail (`Create.cshtml`, `Details.cshtml`) berada di situ.

4. Uji kunci: cuba navigasi terus ke `/Compliance/Create` **selepas** anda sudah berjaya hantar satu declaration — anda patut **terus dibawa** ke `/Compliance/Details/{id}` (bukan borang kosong).

✅ **Semakan:** Selepas hantar declaration, `/Compliance/Create` sentiasa redirect ke `Details` bagi pengguna yang sama. Halaman `Details` papar semua respons dengan baris "Tidak Patuh" ditanda warna amaran, dan tiada butang/pautan "Edit" di mana-mana pun.

---

## Rujukan Fail Sebenar

| Fail anda (lab) | Fail rujukan (projek sebenar) |
|------------------|-------------------------------|
| `ViewModels/ComplianceDeclarationViewModel.cs` | `projek/Nres.Onboarding.Web/ViewModels/` |
| `Controllers/ComplianceController.cs` | `projek/Nres.Onboarding.Web/Controllers/` |
| `Views/Compliance/Create.cshtml`, `Details.cshtml` | `projek/Nres.Onboarding.Web/Views/Compliance/` |

---

## Cabaran (Pilihan)

1. **Papar amaran ringkasan ketidakpatuhan** — Dalam `Details.cshtml`, tambah `<div class="alert alert-danger">` di atas jadual jika `Model.Responses.Any(r => !r.IsCompliant)`, memaparkan bilangan item Tidak Patuh.
2. **Halaman "Sudah Dihantar" khas** — Bina `Views/Compliance/AlreadyDeclared.cshtml` berasingan (bukan guna `Details` semula) untuk paparan ringkas apabila pemohon cuba akses `/Compliance/Create` kali kedua, dengan pautan terus ke `Details`.
3. **Uji tanpa JavaScript** — Matikan JavaScript pelayar (atau guna `curl -X POST`) dan sahkan validation `[Required]`/`ModelState.AddModelError` bahagian pelayan (*server-side*) tetap berfungsi walaupun validation bahagian klien (*client-side*, jQuery Validation) tidak aktif.

---

Nota penceramah (pemasaan sesi, silap biasa, soalan perbincangan, deliverable akhir hari): [`../nota-penceramah.md`](../nota-penceramah.md).
