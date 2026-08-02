# Hari 2 — Lapor Diri: Borang & Validation

Nota ini mengikut **aturcara rasmi HARI 2** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 5 hingga SESI 7. Bahagian ini menerangkan **konsep** (kenapa sesuatu wujud); langkah hands-on penuh, bernombor, dengan kod untuk ditaip sendiri, ada di [`snippets/lab.md`](./snippets/lab.md).

> **Sambungan Hari 1:** Kita meneruskan projek `Nres.Onboarding.Web` yang sama. Entiti kongsi `Submission`, `Attachment`, `AuditLog`, `UserProfile`, dan enum `SubmissionStatus` yang anda tulis semalam **tidak berubah nama** — kita hanya **tambah** entiti baharu khusus Modul 1 (Lapor Diri) di atasnya.

> **Konvensyen bahasa:** Nota & penerangan dalam **Bahasa Melayu**; semua kod, nama kelas/pembolehubah, nama fail, dan istilah teknikal (`Controller`, `ViewModel`, `DataAnnotations`) dikekalkan dalam **Bahasa Inggeris**.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| ASP.NET Core MVC — Controller | [learn.microsoft.com/aspnet/core/mvc/controllers/actions](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions) |
| Model Binding | [learn.microsoft.com/aspnet/core/mvc/models/model-binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding) |
| Model Validation & DataAnnotations | [learn.microsoft.com/aspnet/core/mvc/models/validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation) |
| `System.ComponentModel.DataAnnotations` API | [learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations) |
| Razor Views & Tag Helpers | [learn.microsoft.com/aspnet/core/mvc/views/overview](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/overview) |
| Bina borang dengan Tag Helpers | [learn.microsoft.com/aspnet/core/mvc/views/working-with-forms](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms) |
| `asp-validation-summary` & `asp-validation-for` | [learn.microsoft.com/aspnet/core/mvc/models/validation#validation-summary-tag-helper](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation#validation-summary-tag-helper) |
| EF Core — tambah migration baharu | [learn.microsoft.com/ef/core/managing-schema/migrations](https://learn.microsoft.com/en-us/ef/core/managing-schema/migrations/) |
| Corak view model (Post-Redirect-Get) | [learn.microsoft.com/aspnet/core/mvc/overview#post-redirect-get](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 5–6: Borang Lapor Diri** — `OfficerReportingApplication`, `OfficerReportingCreateViewModel`, controller `Index/Create/Edit/Details`, Razor view, DataAnnotations. 💻 Lab: borang boleh cipta & edit |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 7: Validation & Draf** — validation summary, simpan draf (data tidak lengkap dibenarkan), asingkan view model vs entiti. 💻 Lab: validation lengkap + simpan draf |
| 5.00 petang | Bersurai |

**Hasil Hari 2** (rujuk [`../JADUAL.md`](../JADUAL.md)): Lapor Diri boleh dicipta, disunting, disahkan, dan disimpan sebagai draf.

---

## SESI 5–6 — Borang Lapor Diri

### Kenapa `OfficerReportingApplication` entiti berasingan, bukan tambah lajur ke `Submission`?

`Submission` (Hari 1) ialah entiti **kongsi** — ia perlu kekal generik supaya dikongsi oleh kelima-lima modul. Medan khusus Lapor Diri (nombor IC, jabatan, tarikh lapor diri, dsb.) **tidak relevan** kepada modul lain (Pas Keselamatan, PKS, dsb.) — meletakkannya dalam `Submission` akan mencemari jadual induk dengan lajur kosong (`NULL`) untuk 4 daripada 5 modul setiap kali.

Sebaliknya, kita cipta `OfficerReportingApplication` sebagai jadual **detail**, dipautkan ke `Submission` induk melalui `SubmissionId` (kunci asing satu-ke-satu). Corak ini — **kongsi induk, khusus detail** — yang sama diulang untuk Modul 2 hingga 5 (Hari 4 dan seterusnya).

Medan Lapor Diri (daripada `coach-nres/nres-dotnet-15-day-coaching-guide.md`, "Suggested Fields"):

| Medan | Jenis | Wajib |
|-------|-------|-------|
| Full name | Teks | Ya |
| IC number | Teks | Ya |
| Email | Emel | Ya |
| Phone | Teks | Ya |
| Department | Dropdown | Ya |
| Position | Teks/dropdown | Ya |
| Grade | Dropdown | Ya |
| Reporting date | Tarikh | Ya |
| Previous agency | Teks | Tidak |
| Emergency contact | Teks | Tidak |

### Kenapa `OfficerReportingCreateViewModel` berasingan daripada entiti `OfficerReportingApplication`?

Ini salah satu keputusan seni bina **paling penting** dalam ASP.NET Core MVC, dan sering disalah faham oleh pemula. Entiti (`OfficerReportingApplication`) memetakan terus kepada jadual pangkalan data — ia mempunyai medan seperti `Id`, `SubmissionId` yang **tidak sepatutnya** boleh diisi terus oleh pengguna melalui borang HTML. View model (`OfficerReportingCreateViewModel`) pula ialah bentuk data **khusus untuk satu borang** — ia hanya mengandungi medan yang pengguna **sepatutnya** boleh isi, dengan peraturan pengesahan (`DataAnnotations`) yang sesuai untuk borang tersebut.

Jika kita bind terus ke entiti (`[Bind(...)] OfficerReportingApplication model` dalam parameter action), kita terdedah kepada risiko **over-posting** — pengguna jahat boleh hantar medan tambahan (cth. `Id=999`) dalam permintaan HTTP mentah yang menimpa rekod lain. View model menutup risiko ini secara automatik kerana ia **tidak mempunyai** medan seperti `Id`/`SubmissionId` untuk ditimpa.

```csharp
public class OfficerReportingCreateViewModel
{
    [Required]
    [StringLength(200)]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    public string IdentityNo { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    [DataType(DataType.Date)]
    public DateTime ReportingDate { get; set; }
}
```

*(Contoh ringkas di atas daripada panduan sumber — versi penuh dengan **semua** medan ada di [`snippets/lab.md`](./snippets/lab.md).)*

### Controller actions: `Index`, `Create`, `Edit`, `Details`

Setiap action mempunyai tanggungjawab jelas mengikut corak MVC standard:

- **`Index`** — senarai permohonan Lapor Diri milik pengguna log masuk (atau semua, untuk admin — dibina Hari 3).
- **`Create` (GET)** — papar borang kosong.
- **`Create` (POST)** — terima `OfficerReportingCreateViewModel`, sahkan, cipta `Submission` + `OfficerReportingApplication` baharu.
- **`Edit` (GET)** — papar borang diisi dengan data sedia ada (hanya jika status masih `Draft`).
- **`Edit` (POST)** — kemas kini rekod sedia ada.
- **`Details`** — papar butiran (baca sahaja) satu permohonan.

> **Kenapa dua action `Create` (GET dan POST)?** Ini corak **Post-Redirect-Get** standard ASP.NET Core MVC — `GET Create` papar borang kosong; `POST Create` proses hantaran borang. Selepas `POST` berjaya, kita **redirect** (bukan return view terus) ke `Details`/`Index` supaya *refresh* pelayar tidak menghantar semula borang yang sama (`F5` selepas submit tidak akan cipta rekod pendua).

> Rujukan rasmi: [learn.microsoft.com/aspnet/core/mvc/controllers/actions](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/actions) · [learn.microsoft.com/aspnet/core/mvc/models/model-binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding)

---

## SESI 7 — Validation & Draf

### Kenapa validation mesti di **server-side**, bukan cukup JavaScript sahaja?

Validation sisi klien (HTML5 `required`, JavaScript) memberi maklum balas **segera** kepada pengguna, tetapi ia **mudah dipintas** — pengguna boleh nyahaktifkan JavaScript, guna alat seperti Postman/curl untuk hantar data terus ke pelayan. `[Required]`, `[StringLength]`, `[EmailAddress]` dalam `DataAnnotations` disemak semula di **pelayan** (dalam `ModelState.IsValid`) — ini pertahanan sebenar. ASP.NET Core MVC secara automatik menjana kedua-dua (validation sisi klien **dan** semakan sisi pelayan) daripada atribut yang sama, jadi anda tidak perlu tulis logik dua kali.

### Kenapa "simpan draf" membenarkan data tidak lengkap?

Realiti proses kerja: seorang pegawai baharu mungkin belum ada semua maklumat (contohnya, nombor kenalan kecemasan) semasa mula mengisi borang — mereka patut boleh **simpan kerja separuh jalan** dan sambung kemudian, bukan dipaksa lengkapkan semuanya dalam satu sesi. Ini sebabnya kita asingkan dua peringkat pengesahan:

- **Simpan draf** — pengesahan **minimum** (contohnya, nama sahaja wajib) supaya rekod boleh dicipta walaupun tidak lengkap. Status kekal `SubmissionStatus.Draft`.
- **Submit** (Hari 3) — pengesahan **penuh** (semua medan wajib mengikut jadual di atas) sebelum status bertukar kepada `SubmissionStatus.Submitted`.

Corak ini dicapai dengan **view model berasingan** untuk setiap peringkat, atau dengan menandakan medan berkenaan `[Required(...)]` tetapi menyemak `ModelState.IsValid` secara **selektif** bergantung pada butang yang ditekan (`Simpan Draf` vs `Hantar`). Kita guna pendekatan kedua dalam lab — lebih ringkas untuk satu borang, dan pratonton corak yang sama dipakai untuk semua modul lain.

> Rujukan rasmi: [learn.microsoft.com/aspnet/core/mvc/models/validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation)

---

## Seterusnya

Baca dan ikuti langkah demi langkah di [`snippets/lab.md`](./snippets/lab.md) — di situ anda akan:

1. Cipta entiti `OfficerReportingApplication` dan tambah `DbSet` ke `ApplicationDbContext`.
2. Jana migration baharu untuk jadual `OfficerReportingApplications`.
3. Tulis `OfficerReportingCreateViewModel` dan `OfficerReportingEditViewModel` lengkap dengan `DataAnnotations`.
4. Bina `OfficerReportingController` dengan action `Index`, `Create`, `Edit`, `Details`.
5. Bina Razor view `Create.cshtml` dan `Edit.cshtml` menggunakan Tag Helpers.
6. Tambah logik "Simpan Draf" vs pengesahan penuh.

Nota penceramah (pemasaan sesi, silap biasa, soalan perbincangan): [`nota-penceramah.md`](./nota-penceramah.md).
