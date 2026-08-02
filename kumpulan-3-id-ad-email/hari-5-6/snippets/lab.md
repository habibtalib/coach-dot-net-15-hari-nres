# Lab · Kumpulan 3 · Hari 5–6 — Borang & Kelulusan Penyelia

> Konsep: [`../README.md`](../README.md) · AI: [`../../../AGENTS.md`](../../../AGENTS.md)

---

## Latihan 0 — Mula blok

```bash
git switch kump-3/id-ad-email
git pull --rebase origin master
git switch -c kump-3/feat/borang-dan-kelulusan-penyelia
dotnet build
```

**Semakan "sudah wujud?"**

```bash
grep -n "virtual" Nres.Onboarding.Web/Controllers/SubmissionControllerBase.cs
grep -rn "IApprovalRouteService" Nres.Onboarding.Web/Services/Akaun/
```

`Approve`/`Reject` ialah `virtual` — anda akan **mengatasi** `Reject` supaya Penyelia juga boleh menolak.

### ✅ Semakan

- [ ] `IApprovalRouteService` anda dari Hari 4 wujud
- [ ] Anda mengesahkan `Reject` ialah `virtual`
- [ ] Anda pada cabang ciri

---

## Latihan 1 — View model dengan senarai akses

**Objektif:** Satu view model yang mengendalikan medan rata **dan** senarai bersarang.

### Langkah

`ViewModels/Akaun/AccountRequestFormViewModel.cs`:

```csharp
using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;
using Nres.Onboarding.Web.Models.Akaun;
using Nres.Onboarding.Web.Models.Shared;

namespace Nres.Onboarding.Web.ViewModels.Akaun;

/// <summary>Satu baris dalam senarai akses pada borang.</summary>
public class AksesBarisViewModel
{
    public int SystemAccessId { get; set; }
    public string Kod { get; set; } = string.Empty;
    public string Nama { get; set; } = string.Empty;
    public KategoriAkses Kategori { get; set; }
    public bool PerluJustifikasi { get; set; }

    // --- Input pengguna ---
    public bool Dipilih { get; set; }
    public TahapAkses Tahap { get; set; } = TahapAkses.BacaSahaja;

    [StringLength(500)]
    public string? Justifikasi { get; set; }
}

public class AccountRequestFormViewModel : IValidatableObject
{
    public int? Id { get; set; }

    [Display(Name = "Jenis permohonan")]
    public JenisPermohonanAkaun Jenis { get; set; } = JenisPermohonanAkaun.AkaunBaharu;

    [Display(Name = "Nama staf")]
    [Required(ErrorMessage = "Nama staf wajib diisi.")]
    [StringLength(200)]
    public string StaffName { get; set; } = string.Empty;

    [Display(Name = "No. kad pengenalan staf")]
    [Required(ErrorMessage = "No. kad pengenalan wajib diisi.")]
    [RegularExpression(@"^\d{6}-\d{2}-\d{4}$", ErrorMessage = "Format: 010203-14-5678")]
    public string StaffIdentityNo { get; set; } = string.Empty;

    [Display(Name = "Bahagian")]
    [Required(ErrorMessage = "Sila pilih bahagian.")]
    public int? DepartmentId { get; set; }

    [Display(Name = "Jawatan")]
    [Required(ErrorMessage = "Sila pilih jawatan.")]
    public int? PositionId { get; set; }

    [Display(Name = "Penyelia Jabatan")]
    [Required(ErrorMessage = "Sila pilih penyelia yang akan meluluskan.")]
    public string? SupervisorUserId { get; set; }

    [Display(Name = "Justifikasi permohonan")]
    [Required(ErrorMessage = "Justifikasi wajib diisi.")]
    [StringLength(1000)]
    public string Justifikasi { get; set; } = string.Empty;

    [Display(Name = "Tarikh mula bertugas")]
    [DataType(DataType.Date)]
    public DateTime? TarikhMula { get; set; }

    [Display(Name = "Tarikh akhir perkhidmatan")]
    [DataType(DataType.Date)]
    public DateTime? TarikhTamat { get; set; }

    [Display(Name = "Butiran perubahan")]
    [StringLength(1000)]
    public string? ButiranPerubahan { get; set; }

    /// <summary>
    /// Senarai akses. Diikat melalui nama berindeks: Akses[0].Dipilih, dst.
    /// Indeks MESTI berturutan dari 0 — model binding berhenti pada jurang.
    /// </summary>
    public List<AksesBarisViewModel> Akses { get; set; } = [];

    // --- Data sokongan ---
    public IEnumerable<SelectListItem> Departments { get; set; } = [];
    public IEnumerable<SelectListItem> Positions { get; set; } = [];
    public IEnumerable<SelectListItem> Supervisors { get; set; } = [];

    public string? ReferenceNo { get; set; }
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public bool IsEditable { get; set; } = true;

    /// <summary>Id pengguna semasa — untuk semakan penyelia ≠ pemohon.</summary>
    public string? CurrentUserId { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        var dipilih = Akses.Where(a => a.Dipilih).ToList();

        // 1. Sekurang-kurangnya satu akses.
        if (dipilih.Count == 0)
        {
            yield return new ValidationResult(
                "Sila pilih sekurang-kurangnya satu akses sistem.",
                [nameof(Akses)]);
        }

        // 2. Akses sensitif memerlukan justifikasi.
        foreach (var a in dipilih.Where(a => a.PerluJustifikasi
                                          && string.IsNullOrWhiteSpace(a.Justifikasi)))
        {
            yield return new ValidationResult(
                $"Justifikasi wajib diisi untuk akses '{a.Nama}'.",
                [nameof(Akses)]);
        }

        // 3. Akaun baharu mesti termasuk AD dan e-mel.
        if (Jenis == JenisPermohonanAkaun.AkaunBaharu)
        {
            var kod = dipilih.Select(a => a.Kod).ToHashSet();
            if (!kod.Contains("AD"))
                yield return new ValidationResult(
                    "Permohonan akaun baharu mesti termasuk akaun Active Directory.",
                    [nameof(Akses)]);
            if (!kod.Contains("EMAIL"))
                yield return new ValidationResult(
                    "Permohonan akaun baharu mesti termasuk e-mel rasmi.",
                    [nameof(Akses)]);

            if (TarikhMula is null)
                yield return new ValidationResult(
                    "Tarikh mula bertugas wajib diisi untuk akaun baharu.",
                    [nameof(TarikhMula)]);
        }

        // 4. Nyahaktif memerlukan tarikh tamat.
        if (Jenis == JenisPermohonanAkaun.Nyahaktif && TarikhTamat is null)
        {
            yield return new ValidationResult(
                "Tarikh akhir perkhidmatan wajib diisi untuk permohonan nyahaktif.",
                [nameof(TarikhTamat)]);
        }

        // 5. Tukar maklumat memerlukan butiran.
        if (Jenis == JenisPermohonanAkaun.TukarMaklumat
            && string.IsNullOrWhiteSpace(ButiranPerubahan))
        {
            yield return new ValidationResult(
                "Sila nyatakan butiran perubahan.",
                [nameof(ButiranPerubahan)]);
        }

        // 6. Penyelia tidak boleh sama dengan pemohon — elak kelulusan sendiri.
        if (!string.IsNullOrWhiteSpace(SupervisorUserId)
            && SupervisorUserId == CurrentUserId)
        {
            yield return new ValidationResult(
                "Anda tidak boleh memilih diri sendiri sebagai penyelia yang meluluskan.",
                [nameof(SupervisorUserId)]);
        }
    }
}
```

> **Peraturan 6** ialah versi khusus modul anda bagi masalah "kelulusan sendiri". Bawa ini ke semakan silang AI — jika kumpulan lain menghadapinya juga, ia calon isu `shared`.

### ✅ Semakan

- [ ] View model dalam `ViewModels/Akaun/`
- [ ] Keenam-enam peraturan validation dilaksanakan
- [ ] `yield return` — semua ralat dilaporkan sekaligus
- [ ] Senarai `Akses` ialah `List<T>`, bukan `IReadOnlyList<T>` *(model binding perlu menulis kepadanya)*

---

## Latihan 2 — Controller: cipta, sunting, simpan draf

### Langkah

Tambah ke `AccountRequestController`:

```csharp
[HttpGet]
public async Task<IActionResult> Create(JenisPermohonanAkaun jenis
    = JenisPermohonanAkaun.AkaunBaharu)
{
    var vm = new AccountRequestFormViewModel { Jenis = jenis };
    await IsiSokonganAsync(vm);
    return View("Form", vm);
}

[HttpGet]
public async Task<IActionResult> Edit(int id)
{
    var app = await Db.Set<AccountRequest>()
        .Include(a => a.Submission)
        .Include(a => a.AccessRequests)
        .FirstOrDefaultAsync(a => a.Id == id);

    if (app is null) return NotFound();

    var milikSaya = app.Submission!.ApplicantUserId == currentUser.UserId;
    var sayaPenyelia = app.SupervisorUserId == currentUser.UserId;
    if (!milikSaya && !sayaPenyelia && !currentUser.IsInRole(AdminRole))
        return Forbid();

    var vm = KeViewModel(app);
    await IsiSokonganAsync(vm, app);
    return View("Form", vm);
}

[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> SaveDraft(AccountRequestFormViewModel vm)
{
    if (string.IsNullOrWhiteSpace(vm.StaffName))
    {
        ModelState.Clear();
        ModelState.AddModelError(nameof(vm.StaffName),
            "Nama staf diperlukan walaupun untuk draf.");
        await IsiSokonganAsync(vm);
        return View("Form", vm);
    }
    ModelState.Clear();

    var app = await MuatAtauCiptaAsync(vm);
    if (app is null) return Forbid();

    SalinKeEntiti(vm, app);
    await SelaraskanAksesAsync(vm, app);
    await Db.SaveChangesAsync();

    TempData["Mesej"] = "Draf disimpan.";
    return RedirectToAction(nameof(Edit), new { id = app.Id });
}

// ----- pembantu -----

private async Task IsiSokonganAsync(
    AccountRequestFormViewModel vm, AccountRequest? app = null)
{
    vm.CurrentUserId = currentUser.UserId;

    vm.Departments = await Db.LookupDepartments.AsNoTracking()
        .Where(l => l.IsActive).OrderBy(l => l.Name)
        .Select(l => new SelectListItem(l.Name, l.Id.ToString())).ToListAsync();

    vm.Positions = await Db.LookupPositions.AsNoTracking()
        .Where(l => l.IsActive).OrderBy(l => l.Name)
        .Select(l => new SelectListItem(l.Name, l.Id.ToString())).ToListAsync();

    // Penyelia = pengguna dengan peranan Supervisor. Kami memuatkan profil
    // untuk memaparkan nama sebenar, bukan e-mel.
    var penyeliaIds = await (from ur in Db.UserRoles
                             join r in Db.Roles on ur.RoleId equals r.Id
                             where r.Name == "Supervisor"
                             select ur.UserId).ToListAsync();

    vm.Supervisors = await Db.UserProfiles.AsNoTracking()
        .Where(p => penyeliaIds.Contains(p.UserId))
        .OrderBy(p => p.FullName)
        .Select(p => new SelectListItem(p.FullName, p.UserId))
        .ToListAsync();

    // Bina senarai akses daripada lookup, tandakan yang sudah dipilih.
    var sedia = app?.AccessRequests.ToDictionary(r => r.SystemAccessId) ?? [];

    vm.Akses = await Db.Set<LookupSystemAccess>().AsNoTracking()
        .Where(l => l.IsActive)
        .OrderBy(l => l.Kategori).ThenBy(l => l.Name)
        .Select(l => new AksesBarisViewModel
        {
            SystemAccessId = l.Id,
            Kod = l.Code,
            Nama = l.Name,
            Kategori = l.Kategori,
            PerluJustifikasi = l.PerluJustifikasi
        })
        .ToListAsync();

    foreach (var baris in vm.Akses)
    {
        if (sedia.TryGetValue(baris.SystemAccessId, out var r))
        {
            baris.Dipilih = true;
            baris.Tahap = r.Tahap;
            baris.Justifikasi = r.Justifikasi;
        }
    }
}

/// <summary>Selaraskan akses yang dipilih dengan baris dalam DB.</summary>
private async Task SelaraskanAksesAsync(
    AccountRequestFormViewModel vm, AccountRequest app)
{
    var dipilih = vm.Akses.Where(a => a.Dipilih).ToList();
    var dipilihIds = dipilih.Select(a => a.SystemAccessId).ToHashSet();

    var sedia = await Db.Set<RequestedSystemAccess>()
        .Where(r => r.AccountRequestId == app.Id)
        .ToListAsync();

    // Buang yang tidak lagi dipilih.
    foreach (var r in sedia.Where(r => !dipilihIds.Contains(r.SystemAccessId)))
        Db.Set<RequestedSystemAccess>().Remove(r);

    // Tambah atau kemas kini.
    foreach (var a in dipilih)
    {
        var r = sedia.FirstOrDefault(x => x.SystemAccessId == a.SystemAccessId);
        if (r is null)
        {
            Db.Set<RequestedSystemAccess>().Add(new RequestedSystemAccess
            {
                AccountRequestId = app.Id,
                SystemAccessId = a.SystemAccessId,
                Tahap = a.Tahap,
                Justifikasi = a.Justifikasi
            });
        }
        else
        {
            r.Tahap = a.Tahap;
            r.Justifikasi = a.Justifikasi;
        }
    }
}
```

> **`SelaraskanAksesAsync` menggunakan corak "buang yang hilang, tambah yang baharu, kemas kini yang kekal"** — bukan "padam semua kemudian sisip semula". Yang kedua lebih mudah ditulis tetapi kehilangan `Diluluskan` dan `CatatanIct` yang ICT tetapkan pada Hari 7–9.

### ✅ Semakan

- [ ] Dropdown penyelia memaparkan **nama**, bukan e-mel
- [ ] Senarai akses dibina daripada lookup dan menandakan yang dipilih
- [ ] `SelaraskanAksesAsync` mengekalkan baris sedia ada
- [ ] Penyelia boleh membuka permohonan yang dia perlu luluskan

---

## Latihan 3 — Hantar: cipta laluan kelulusan

**Objektif:** Penghantaran mencipta laluan dua peringkat.

### Langkah

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Submit(AccountRequestFormViewModel vm)
{
    if (vm.Id is null) return NotFound();

    var app = await Db.Set<AccountRequest>()
        .Include(a => a.Submission)
        .Include(a => a.AccessRequests)
        .FirstOrDefaultAsync(a => a.Id == vm.Id);

    if (app is null) return NotFound();
    if (app.Submission!.ApplicantUserId != currentUser.UserId) return Forbid();
    if (app.Submission.Status != SubmissionStatus.Draft)
    {
        TempData["Ralat"] = "Permohonan ini telah pun dihantar.";
        return RedirectToAction(nameof(Edit), new { id = app.Id });
    }

    // Validation PENUH — IValidatableObject berjalan di sini.
    vm.CurrentUserId = currentUser.UserId;
    if (!ModelState.IsValid)
    {
        await IsiSokonganAsync(vm, app);
        return View("Form", vm);
    }

    SalinKeEntiti(vm, app);
    await SelaraskanAksesAsync(vm, app);
    await Db.SaveChangesAsync();

    // --- Jana nombor rujukan (servis KONGSI) ---
    app.Submission.ReferenceNo = await referenceNumbers.GenerateAsync(ModuleCode);

    // --- Cipta laluan kelulusan DUA peringkat ---
    // Pada PENGHANTARAN, bukan pada cipta draf — penyelia mungkin berubah
    // semasa draf masih disunting.
    await approvalRoute.CreateRouteAsync(app.SubmissionId, app.SupervisorUserId);

    // --- Peralihan + audit (atomik) ---
    await Workflow.TransitionAsync(app.Submission, SubmissionStatus.Submitted,
        "Submitted",
        $"{app.Jenis} untuk {app.StaffName}. " +
        $"{app.AccessRequests.Count} akses dipohon.");

    // --- Beritahu penyelia, bukan pemohon ---
    await notifications.NotifyAsync(app.SupervisorUserId,
        $"Permohonan akaun {app.Submission.ReferenceNo} menunggu kelulusan anda",
        $"{app.StaffName} memohon {app.Jenis}. Sila semak dalam sistem.");

    TempData["Mesej"] =
        $"Permohonan dihantar. No. rujukan: {app.Submission.ReferenceNo}. " +
        "Menunggu kelulusan penyelia.";

    return RedirectToAction(nameof(Edit), new { id = app.Id });
}
```

> **Perhatikan notifikasi pergi kepada penyelia**, bukan pemohon. Ini berbeza daripada kumpulan lain — permohonan anda memerlukan tindakan segera daripada orang tertentu.

### ✅ Semakan

- [ ] Penghantaran mencipta **dua** `ApprovalStep`
- [ ] Nombor rujukan `ICT-ID-2026-####` dijana
- [ ] Status → `Submitted`
- [ ] Penyelia diberitahu
- [ ] Menghantar tanpa akses **ditolak**
- [ ] Memilih diri sendiri sebagai penyelia **ditolak**

---

## Latihan 4 — Borang Razor dengan senarai akses

**Objektif:** Senarai akses yang mengikat dengan betul.

### Langkah

Bahagian akses dalam `Views/Akaun/Form.cshtml`:

```cshtml
<h5 class="mt-4">Akses Sistem Dipohon</h5>
<div asp-validation-for="Akses" class="text-danger"></div>

@foreach (var kategori in Model.Akses.GroupBy(a => a.Kategori))
{
    <div class="card mb-3">
        <div class="card-header">@kategori.Key</div>
        <div class="card-body">
        @foreach (var baris in kategori)
        {
            @* INDEKS mesti berturutan dari 0 merentas SELURUH senarai —
               bukan setiap kumpulan. Kami mencarinya dalam senarai penuh. *@
            var i = Model.Akses.IndexOf(baris);

            <div class="row g-2 align-items-start mb-3 pb-3 border-bottom">
                <input type="hidden" name="Akses[@i].SystemAccessId" value="@baris.SystemAccessId" />
                <input type="hidden" name="Akses[@i].Kod" value="@baris.Kod" />
                <input type="hidden" name="Akses[@i].Nama" value="@baris.Nama" />
                <input type="hidden" name="Akses[@i].Kategori" value="@((int)baris.Kategori)" />
                <input type="hidden" name="Akses[@i].PerluJustifikasi" value="@baris.PerluJustifikasi.ToString().ToLower()" />

                <div class="col-md-4">
                    <div class="form-check">
                        <input type="checkbox" name="Akses[@i].Dipilih" value="true"
                               class="form-check-input akses-pilih"
                               data-indeks="@i"
                               @(baris.Dipilih ? "checked" : "") />
                        @* Nilai palsu memastikan "false" dihantar bila tidak ditanda *@
                        <input type="hidden" name="Akses[@i].Dipilih" value="false" />
                        <label class="form-check-label">
                            @baris.Nama
                            @if (baris.PerluJustifikasi)
                            {
                                <span class="badge bg-warning text-dark">Perlu justifikasi</span>
                            }
                        </label>
                    </div>
                </div>

                <div class="col-md-3">
                    <select name="Akses[@i].Tahap" class="form-select form-select-sm">
                        <option value="1" selected="@(baris.Tahap == TahapAkses.BacaSahaja)">Baca sahaja</option>
                        <option value="2" selected="@(baris.Tahap == TahapAkses.BacaTulis)">Baca &amp; tulis</option>
                        <option value="3" selected="@(baris.Tahap == TahapAkses.Pentadbir)">Pentadbir</option>
                    </select>
                </div>

                <div class="col-md-5">
                    <input name="Akses[@i].Justifikasi" value="@baris.Justifikasi"
                           class="form-control form-control-sm"
                           placeholder="@(baris.PerluJustifikasi ? "Justifikasi (WAJIB)" : "Justifikasi (pilihan)")" />
                </div>
            </div>
        }
        </div>
    </div>
}
```

> **Dua perkara penting:**
>
> 1. **Medan tersembunyi `Dipilih=false` selepas checkbox.** Checkbox yang tidak ditanda menghantar **tiada apa-apa** — tanpa medan palsu ini, model binding melihat jurang dan senarai anda rosak.
> 2. **`Model.Akses.IndexOf(baris)`** — indeks mesti berturutan merentas **seluruh** senarai, bukan bermula semula setiap kumpulan kategori.

### ✅ Semakan

- [ ] Menanda kotak dan menghantar mengekalkan pilihan
- [ ] Menyahtanda kotak membuang akses daripada permohonan
- [ ] Tahap dan justifikasi disimpan setiap akses
- [ ] Menanda VPN tanpa justifikasi **ditolak** semasa hantar
- [ ] Akses dikumpulkan mengikut kategori dan **indeks masih betul**

---

## Latihan 5 — Kelulusan Penyelia (peringkat 1)

**Objektif:** Tindakan baharu — bukan menulis semula kelas asas.

### Langkah

1. Baris gilir Penyelia:

```csharp
[Authorize(Roles = "Supervisor")]
public async Task<IActionResult> SupervisorQueue()
{
    var userId = currentUser.UserId!;

    var senarai = await (
        from a in Db.Set<AccountRequest>().AsNoTracking()
        join s in Db.Submissions.AsNoTracking() on a.SubmissionId equals s.Id
        join p in Db.UserProfiles.AsNoTracking() on s.ApplicantUserId equals p.UserId
        where a.SupervisorUserId == userId
           && s.Status == SubmissionStatus.Submitted
        orderby s.SubmittedAt
        select new SupervisorQueueItem(
            a.Id, s.Id, s.ReferenceNo, a.Jenis, a.StaffName,
            p.FullName, s.SubmittedAt, a.AccessRequests.Count))
        .ToListAsync();

    return View(senarai);
}
```

2. **Tindakan kelulusan peringkat 1** — ini yang anda tambah:

```csharp
/// <summary>
/// Kelulusan PERINGKAT 1 oleh Penyelia Jabatan.
///
/// Kami TIDAK menggunakan base.Approve() di sini — ia menetapkan
/// AdminApproved, yang betul untuk peringkat AKHIR (ICT), bukan peringkat 1.
/// Ini tindakan TAMBAHAN, bukan penulisan semula kelas asas.
/// </summary>
[HttpPost]
[ValidateAntiForgeryToken]
[Authorize(Roles = "Supervisor")]
public async Task<IActionResult> SupervisorApprove(int id, string? remarks)
{
    var app = await Db.Set<AccountRequest>()
        .Include(a => a.Submission)
        .FirstOrDefaultAsync(a => a.Submission!.Id == id);

    if (app is null) return NotFound();

    // Hanya penyelia YANG DITETAPKAN boleh meluluskan — bukan mana-mana Supervisor.
    if (app.SupervisorUserId != currentUser.UserId) return Forbid();

    if (app.Submission!.Status != SubmissionStatus.Submitted)
    {
        TempData["Ralat"] = "Permohonan ini bukan lagi menunggu kelulusan penyelia.";
        return RedirectToAction(nameof(SupervisorQueue));
    }

    // Rekod keputusan pada langkah 1 (menguatkuasakan turutan).
    await approvalRoute.DecideAsync(app.SubmissionId, stepOrder: 1,
        ApprovalDecision.Approved, currentUser.UserId!, remarks);

    // Peralihan status + audit (atomik).
    await Workflow.TransitionAsync(app.Submission,
        SubmissionStatus.SupervisorApproved, "SupervisorApproved", remarks);

    // Beritahu ICT bahawa ia kini menunggu mereka.
    await notifications.NotifyAsync("ict-queue",
        $"Permohonan {app.Submission.ReferenceNo} lulus penyelia",
        $"{app.StaffName} — menunggu pemprosesan ICT.");

    TempData["Mesej"] = $"Permohonan {app.Submission.ReferenceNo} diluluskan. " +
                        "Dihantar ke ICT untuk pemprosesan.";

    return RedirectToAction(nameof(SupervisorQueue));
}
```

3. **Atasi `Reject`** supaya Penyelia **dan** ICT boleh menolak pada peringkat masing-masing:

```csharp
/// <summary>
/// Kelas asas hanya membenarkan AdminRole (IctAdmin) menolak.
/// Modul kami mempunyai DUA peranan yang boleh menolak, pada peringkat
/// berbeza. Kami MENGATASI untuk membenarkan kedua-duanya, kemudian
/// mendelegasikan kepada base untuk peralihan status + audit + notifikasi.
/// </summary>
[HttpPost]
[ValidateAntiForgeryToken]
public override async Task<IActionResult> Reject(int id, string remarks)
{
    var app = await Db.Set<AccountRequest>()
        .Include(a => a.Submission)
        .FirstOrDefaultAsync(a => a.Submission!.Id == id);

    if (app is null) return NotFound();
    if (string.IsNullOrWhiteSpace(remarks))
    {
        TempData["Ralat"] = "Sebab penolakan wajib diisi.";
        return RedirectToAction(nameof(Edit), new { id = app.Id });
    }

    var status = app.Submission!.Status;

    // Siapa boleh menolak, pada peringkat mana?
    var (stepOrder, dibenarkan) = status switch
    {
        SubmissionStatus.Submitted =>
            (1, app.SupervisorUserId == currentUser.UserId),
        SubmissionStatus.SupervisorApproved =>
            (2, currentUser.IsInRole("IctAdmin")),
        _ => (0, false)
    };

    if (!dibenarkan) return Forbid();

    await approvalRoute.DecideAsync(app.SubmissionId, stepOrder,
        ApprovalDecision.Rejected, currentUser.UserId!, remarks);

    // base.Reject mengendalikan peralihan status, audit, dan notifikasi —
    // kami tidak menulis semula logik itu. Tetapi ia menyemak AdminRole,
    // jadi kami memintas semakan itu dengan menetapkan status terus di sini.
    await Workflow.TransitionAsync(app.Submission,
        SubmissionStatus.Rejected, "Rejected", remarks);

    await notifications.NotifyAsync(app.Submission.ApplicantUserId,
        $"Permohonan {app.Submission.ReferenceNo} ditolak", remarks);

    return RedirectToAction(nameof(Edit), new { id = app.Id });
}
```

> **Nota jujur:** di sini kami **tidak** memanggil `base.Reject` kerana semakan `AdminRole`-nya akan menolak Penyelia. Kami menduplikasi tiga baris (peralihan + notifikasi). Ini **kelemahan sebenar** dalam kelas asas — ia menganggap satu peranan admin.
>
> **Ini calon isu `shared`.** Bincang dengan jurulatih: patutkah `SubmissionControllerBase` menyokong berbilang peranan yang boleh menolak? Jika ya, ia perubahan kongsi yang menjadikan kod anda lebih bersih. Buka isu.

### ✅ Semakan

- [ ] Baris gilir menunjukkan hanya permohonan **saya** sebagai penyelia
- [ ] `SupervisorApprove` menetapkan `SupervisorApproved`, bukan `AdminApproved`
- [ ] Penyelia **lain** tidak boleh meluluskan permohonan yang bukan miliknya
- [ ] `Reject` berfungsi untuk **kedua-dua** Penyelia dan ICT
- [ ] Isu `shared` dibuka tentang berbilang peranan penolak

---

## Latihan 6 — Skrin semakan Penyelia

**Objektif:** Penyelia melihat apa yang mereka perlukan — bukan butiran teknikal ICT.

### Langkah

`Views/Akaun/SupervisorReview.cshtml` — bahagian penting:

```cshtml
<div class="card mb-3">
    <div class="card-header">Akses Dipohon (@Model.Akses.Count)</div>
    <table class="table table-sm mb-0">
        <thead>
            <tr><th>Akses</th><th>Tahap</th><th>Justifikasi</th></tr>
        </thead>
        <tbody>
        @foreach (var a in Model.Akses)
        {
            <tr>
                <td>
                    @a.Nama
                    @if (a.PerluJustifikasi)
                    {
                        <span class="badge bg-warning text-dark">Sensitif</span>
                    }
                </td>
                <td>@a.Tahap</td>
                <td>@(a.Justifikasi ?? "—")</td>
            </tr>
        }
        </tbody>
    </table>
</div>

<div class="alert alert-info small">
    <strong>Peranan anda:</strong> nilai sama ada staf ini, dalam jawatan ini,
    memerlukan akses yang dipohon. Butiran teknikal (nama akaun AD, konfigurasi
    pelayan) dikendalikan ICT selepas kelulusan anda.
</div>

<form asp-action="SupervisorApprove" method="post" class="mb-3">
    @Html.AntiForgeryToken()
    <input type="hidden" name="id" value="@Model.Submission.Id" />
    <textarea name="remarks" class="form-control mb-2" rows="2"
              placeholder="Catatan (pilihan)"></textarea>
    <button type="submit" class="btn btn-success">Luluskan (Peringkat 1)</button>
</form>

<form asp-action="Reject" method="post">
    @Html.AntiForgeryToken()
    <input type="hidden" name="id" value="@Model.Submission.Id" />
    <textarea name="remarks" class="form-control mb-2" rows="2"
              placeholder="Sebab penolakan (WAJIB)" required></textarea>
    <button type="submit" class="btn btn-danger">Tolak</button>
</form>
```

> Kotak `alert-info` bukan hiasan — ia memberitahu penyelia **apa yang mereka nilai**, yang mengurangkan kelulusan getah-cap.

### ✅ Semakan

- [ ] Skrin menunjukkan staf, jawatan, akses + justifikasi
- [ ] Akses sensitif ditanda
- [ ] Butiran teknikal ICT **tidak** dipaparkan
- [ ] Panel keputusan berfungsi untuk lulus dan tolak

---

## Latihan 7 — Ujian

Rekod dalam `docs/kumpulan-3/ujian-manual.md`:

| # | Ujian | Jangkaan | Keputusan |
|---|-------|----------|-----------|
| 1 | Hantar tanpa pilih akses | Ditolak | |
| 2 | Akaun baharu tanpa AD | Ditolak — AD wajib | |
| 3 | Tanda VPN tanpa justifikasi | Ditolak | |
| 4 | Pilih diri sendiri sebagai penyelia | Ditolak | |
| 5 | Nyahaktif tanpa tarikh tamat | Ditolak | |
| 6 | Hantar sah | `ICT-ID-2026-####` + 2 ApprovalStep | |
| 7 | Penyelia ditetapkan melihat baris gilir | Permohonan kelihatan | |
| 8 | Penyelia **lain** melihat baris gilir | Permohonan **tidak** kelihatan | |
| 9 | Penyelia lain cuba luluskan (URL terus) | **403** | |
| 10 | Penyelia luluskan | Status → `SupervisorApproved`, langkah 1 = Approved | |
| 11 | IctAdmin cuba luluskan sebelum penyelia | Gagal — turutan langkah | |
| 12 | Penyelia tolak dengan sebab | Status → `Rejected`, langkah 1 = Rejected | |
| 13 | Nyahtanda akses, simpan draf | Akses dibuang dari permohonan | |
| 14 | Applicant → `/AccountRequest/SupervisorQueue` | **403** | |

### ✅ Semakan

- [ ] Kesemua 14 ujian dijalankan
- [ ] Ujian 9, 11, 14 memberi 403 / gagal dengan betul
- [ ] Ujian 8 mengesahkan pengasingan baris gilir

---

## Latihan 8 — Tutup blok

```bash
git diff --name-only master
```

Semakan AI → PR → review → gabung → **gabungan latihan ke `master`** → board.

### ✅ Semakan (Definition of Done)

- [ ] Borang menyimpan draf dengan senarai akses
- [ ] Validation penuh pada hantar (6 peraturan)
- [ ] Penghantaran mencipta laluan 2 peringkat
- [ ] Kelulusan peringkat 1 berfungsi; hanya penyelia ditetapkan
- [ ] `Reject` berfungsi untuk kedua-dua peranan
- [ ] Isu `shared` dibuka tentang kelas asas berbilang peranan
- [ ] Hanya fail Kumpulan 3 disentuh
- [ ] **Gabungan latihan ke `master` selesai**

---

## Deliverable Hari 5–6

| Artifak | Lokasi |
|---------|--------|
| View model + 6 peraturan validation | `ViewModels/Akaun/` |
| Controller: cipta/sunting/draf/hantar | `Controllers/AccountRequestController.cs` |
| `SupervisorApprove` + `Reject` diatasi | Sama |
| Borang dengan senarai akses berindeks | `Views/Akaun/Form.cshtml` |
| Baris gilir + skrin semakan Penyelia | `Views/Akaun/` |
| Ujian manual | `docs/kumpulan-3/ujian-manual.md` |

**Seterusnya (Hari 7–9):** pemprosesan ICT (peringkat 2), **RBAC merentas modul**, dan simulasi integrasi AD.
