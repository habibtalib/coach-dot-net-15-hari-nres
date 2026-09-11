# Validation & View Models

> Nota konsep untuk **Hari 2** (borang pertama, modul Lapor Diri) — diguna pakai sepanjang kursus setiap kali borang baharu dibina. Rujuk [`03-corak-workflow.md`](./03-corak-workflow.md) untuk konteks `Submission` & status.

---

## Kenapa View Model, bukan bind terus ke entiti?

Satu kesilapan biasa pemula: hantar entiti EF Core (`Submission`, `OfficerReportingApplication`) terus ke View dan terima *binding* borang terus ke entiti itu juga. Ini **berbahaya** dan tidak fleksibel:

| Masalah bind terus ke entiti | Penyelesaian dengan View Model |
|---|---|
| **Over-posting attack** — pengguna boleh hantar medan tersembunyi (cth. `Status=Completed`) melalui *request* palsu dan mengubah data yang tidak sepatutnya boleh diubah | View Model hanya dedahkan medan yang **memang** patut diisi pengguna |
| Entiti ada medan pangkalan data (`Id`, `CreatedAt`, `SubmissionId`) yang tidak relevan untuk borang | View Model hanya ada medan borang — bersih & fokus |
| Peraturan validation borang (wajib diisi semasa submit) bercampur dengan peraturan struktur data (nullable dalam DB) | Validation borang letak di View Model; struktur DB letak di entiti |
| Satu View Model perlu berubah ikut konteks (draf vs final) — sukar jika terus guna entiti | Boleh cipta View Model berlainan (`...DraftViewModel`, `...SubmitViewModel`) mengikut keperluan |

```csharp
// ❌ Elak: bind terus ke entiti
[HttpPost]
public IActionResult Create(OfficerReportingApplication entity) { ... }

// ✅ Guna View Model
[HttpPost]
public IActionResult Create(OfficerReportingCreateViewModel vm) { ... }
```

---

## DataAnnotations — validation deklaratif

```csharp
public class OfficerReportingCreateViewModel
{
    [Required(ErrorMessage = "Nama penuh wajib diisi.")]
    [StringLength(150, MinimumLength = 3)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [RegularExpression(@"^\d{6}-\d{2}-\d{4}$", ErrorMessage = "Format No. KP tidak sah (contoh: 900101-14-5555).")]
    public string IcNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Gred jawatan wajib dipilih.")]
    public string PositionGrade { get; set; } = string.Empty;

    [Required]
    [Display(Name = "Tarikh Lapor Diri")]
    [DataType(DataType.Date)]
    public DateTime ReportingDate { get; set; }

    [Range(typeof(bool), "true", "true", ErrorMessage = "Akuan OSA mesti diterima sebelum menghantar.")]
    public bool OsaDeclarationAccepted { get; set; }
}
```

| Atribut | Fungsi |
|---------|--------|
| `[Required]` | Medan wajib diisi |
| `[StringLength]` | Had panjang teks |
| `[Range]` | Julat nilai (nombor/bool) |
| `[RegularExpression]` | Padanan corak (cth. format No. KP) |
| `[DataType]` | Petunjuk jenis input untuk UI (`Date`, `EmailAddress`, dsb.) |
| `[Display(Name=...)]` | Label paparan mesra pengguna |

---

## Validation server-side dalam Controller

**Jangan** hanya bergantung pada validation *client-side* (JavaScript) — ia boleh dipintas. Sentiasa semak `ModelState` di server:

```csharp
[HttpPost]
[ValidateAntiForgeryToken]
public async Task<IActionResult> Create(OfficerReportingCreateViewModel vm)
{
    if (!ModelState.IsValid)
    {
        return View(vm);   // papar semula borang dengan mesej ralat
    }

    var submission = new Submission
    {
        ModuleType = "OfficerReporting",
        Status = SubmissionStatus.Draft,
        ApplicantUserId = _currentUserService.UserId
    };
    // map vm → entiti, simpan...

    return RedirectToAction(nameof(Index));
}
```

> Razor View memaparkan ralat automatik melalui `<span asp-validation-for="FullName" class="text-danger"></span>` dan `<div asp-validation-summary="All"></div>`.

---

## Validation draf vs final — dua tahap ketat

Corak NRES membenarkan **simpan draf** (belum lengkap) tetapi **submit** memerlukan semua medan wajib lengkap. Ini perlukan dua peringkat validation berbeza:

| Peringkat | Bila digunakan | Ketat mana |
|-----------|----------------|------------|
| **Draf** (`Save Draft`) | Pengguna simpan kerja separuh siap | Minimum — hanya semak medan yang **wajib walaupun draf** (cth. nama) |
| **Final** (`Submit`) | Pengguna hantar untuk semakan | Ketat — **semua** medan wajib, lampiran wajib ada, akuan diterima |

### Pendekatan: dua Action / dua View Model, atau validation bersyarat

```csharp
[HttpPost]
public async Task<IActionResult> SaveDraft(OfficerReportingDraftViewModel vm)
{
    // ModelState untuk draft — kurang ketat, medan optional dibenarkan kosong
    if (!ModelState.IsValid) return View("Create", vm);
    // simpan dengan Status = Draft
}

[HttpPost]
public async Task<IActionResult> Submit(OfficerReportingSubmitViewModel vm)
{
    // ModelState untuk submit — semua [Required] dikuatkuasa penuh
    if (!ModelState.IsValid) return View("Create", vm);
    // simpan dengan Status = Submitted, jana ReferenceNumber
}
```

Alternatif: guna **`IValidatableObject`** pada satu View Model untuk validation bersyarat berdasarkan niat (draf/submit) yang dihantar bersama borang:

```csharp
public class OfficerReportingViewModel : IValidatableObject
{
    public bool IsSubmitAction { get; set; }
    public string? OsaDeclarationAccepted { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext context)
    {
        if (IsSubmitAction && OsaDeclarationAccepted != "true")
        {
            yield return new ValidationResult(
                "Akuan OSA wajib diterima sebelum menghantar.",
                new[] { nameof(OsaDeclarationAccepted) });
        }
    }
}
```

---

## Kaitan dengan hari-hari lain

- **Hari 2** — borang Lapor Diri pertama guna corak ini penuh.
- **Kumpulan 2, blok Hari 5–6** — validation bersyarat lebih kompleks (`IValidatableObject`) dan semakan pendua nombor plat.
- **Kumpulan 3, blok Hari 5–6** — senarai akses bersarang dalam satu borang, diikat melalui nama medan berindeks.
- Lihat [`06-file-upload.md`](./06-file-upload.md) untuk validation lampiran (saiz/jenis fail), dan [`07-testing-xunit.md`](./07-testing-xunit.md) untuk cara uji peraturan validation secara automatik.

---

## Sumber Rasmi

- **[Model validation in ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)**
- **[DataAnnotations namespace reference](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations)**
- **[IValidatableObject](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.ivalidatableobject)**
