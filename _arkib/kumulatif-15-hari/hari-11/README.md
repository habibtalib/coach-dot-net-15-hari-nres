# Hari 11 — PKS: Borang Checklist Dinamik & Kunci

Nota ini mengikut **HARI 11** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 32–34 (Modul 4: PKS). Lab hands-on penuh ada di [`snippets/lab.md`](./snippets/lab.md).

> **Konvensyen kod:** Nota dalam **Bahasa Melayu**; kod, nama kelas/pembolehubah, istilah teknikal dalam **Bahasa Inggeris** — rujuk [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) untuk kanun nama entiti/enum/peranan/prefix.

> **Sambungan projek:** Kita **tidak** mula projek baharu. Hari ini kita bina **di atas** entiti PKS (`PolicyVersion`, `ComplianceChecklistItem`, `ComplianceDeclaration`, `ComplianceResponse`) dan migration `Module4Initial` daripada [Hari 10](../hari-10/). Jangan cipta semula entiti tersebut.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Model binding — koleksi (`List<T>`) | [learn.microsoft.com/aspnet/core/mvc/models/model-binding#collections](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding#collections) |
| Working with forms (Tag Helpers) | [learn.microsoft.com/aspnet/core/mvc/views/working-with-forms](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms) |
| Model validation | [learn.microsoft.com/aspnet/core/mvc/models/validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation) |
| `ModelState.AddModelError` (validation tersuai) | [learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.modelbinding.modelstatedictionary.addmodelerror](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.modelbinding.modelstatedictionary.addmodelerror) |
| EF Core — transaksi eksplisit (`BeginTransactionAsync`) | [learn.microsoft.com/ef/core/saving/transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions) |
| Filtered `Include` (checklist aktif sahaja) | [learn.microsoft.com/ef/core/querying/related-data/eager#filtered-include](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager#filtered-include) |
| Authorization asas (`[Authorize]`) | [learn.microsoft.com/aspnet/core/security/authorization/simple](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 32–33: Borang Dinamik** — muat item checklist aktif dari DB, render dalam Razor, `ComplianceDeclarationViewModel` + senarai respons. 💻 **Lab:** borang checklist dinamik |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 34: Simpan & Kunci** — simpan semua respons dalam satu transaksi, sahkan akuan (acknowledgement), kunci edit selepas `Submitted`. 💻 **Lab:** submit + lock |
| 5.00 petang | Bersurai |

Hari ini **tidak** merangkumi semakan admin atau CSV export — itu Hari 12. Fokus semata-mata pada **pengalaman pemohon**: isi checklist dinamik, hantar, dan tidak boleh ubah selepas itu.

---

## Kenapa checklist dijana secara dinamik dari DB, bukan Razor statik dengan 6 `<input>` bertulis tangan?

Hari 10 kita sengaja simpan checklist dalam jadual `ComplianceChecklistItems` supaya ia boleh dikemas kini **tanpa** ubah kod. Jika Razor view ditulis dengan 6 `<input>` statik (satu bagi setiap perkara), keputusan seni bina Hari 10 itu jadi **sia-sia** — checklist "dalam DB" tetapi borang tetap keras (*hardcoded*). Borang mesti:

1. **Query** semua `ComplianceChecklistItem` yang `IsActive = true` bagi `PolicyVersion` yang `IsActive = true`.
2. **Susun** ikut `SequenceNo`.
3. **Jana** satu baris borang bagi **setiap** item — jika `SystemAdmin` menambah item ke-7 esok, borang esok automatik papar 7 baris tanpa sesiapa sentuh kod Razor.

Ini bermakna `ComplianceDeclarationViewModel` **tidak** boleh ada 6 sifat (`property`) tetap seperti `Item1Compliant`, `Item2Compliant`, …. Sebaliknya ia perlu satu **senarai** (`List<ComplianceResponseInput>`) yang panjangnya bergantung kepada bilangan item checklist aktif semasa borang dimuatkan:

```csharp
public class ComplianceDeclarationViewModel
{
    public int PolicyVersionId { get; set; }
    public string PolicyVersionTitle { get; set; } = string.Empty;
    public bool IsAcknowledged { get; set; }
    public List<ComplianceResponseInput> Responses { get; set; } = new();
}

public class ComplianceResponseInput
{
    public int ChecklistItemId { get; set; }
    public string Statement { get; set; } = string.Empty;
    public bool IsCompliant { get; set; }
    public string? Remarks { get; set; }
}
```

**Kenapa `ChecklistItemId` turut disimpan dalam `ComplianceResponseInput` (bukan hanya kedudukan indeks dalam senarai)?** Apabila borang dihantar semula (`POST`), ASP.NET Core Model Binding hanya tahu "ini senarai berindeks 0, 1, 2, …" — ia **tidak** tahu item checklist yang mana setiap indeks itu wakili, melainkan kita hantar `ChecklistItemId` semula sebagai medan `<input type="hidden">` bagi setiap baris. Tanpa ini, `POST` tidak dapat memetakan jawapan kembali kepada `ComplianceChecklistItem` yang betul.

## Kenapa "Model binding koleksi" perlukan medan hidden pada setiap indeks?

ASP.NET Core Model Binding memetakan nama medan borang seperti `Responses[0].ChecklistItemId`, `Responses[0].IsCompliant`, `Responses[1].ChecklistItemId`, … kembali kepada `List<ComplianceResponseInput>`. Tag Helper `asp-for="Responses[i].ChecklistItemId"` (dengan `i` gelung `for`) menjana nama medan yang **tepat** ini secara automatik — itulah sebabnya lab hari ini guna gelung `@for` (bukan `@foreach`) semasa render borang: `@for` beri kita akses kepada **indeks** `i` yang diperlukan Tag Helper.

## Kenapa "simpan semua respons dalam **satu** transaksi"?

Satu pengisytiharan PKS melibatkan **tiga** operasi pangkalan data berasingan: cipta `Submission` induk, cipta `ComplianceDeclaration`, dan cipta banyak baris `ComplianceResponse`. Jika operasi kedua/ketiga gagal (cth. ralat rangkaian, kekangan pangkalan data) **selepas** `Submission` berjaya disimpan, kita akan mempunyai `Submission` "yatim" — rekod induk wujud tetapi tiada declaration/response berkaitan, rosak dari segi integriti data. `Database.BeginTransactionAsync()` memastikan **kesemua** operasi berjaya bersama, atau **tiada satu pun** disimpan (rollback automatik jika berlaku ralat sebelum `CommitAsync()`).

## Kenapa declaration PKS **dikunci selepas dihantar**, tidak seperti Lapor Diri (Hari 2–3) yang boleh disunting semasa draf?

Pengisytiharan pematuhan ialah **akuan rasmi** (*sworn declaration*) — seorang staf mengesahkan "saya patuh/tidak patuh perkara-perkara ini pada tarikh ini". Jika ia boleh disunting bebas selepas dihantar, nilai akuan itu sebagai **rekod audit** hilang sepenuhnya (staf boleh "kemas kini" jawapan selepas isu ketidakpatuhan disiasat, contohnya). Ini sebab reka bentuk Modul 4 **tidak** ada konsep "draf boleh sunting berulang kali" seperti Modul 1 — sebaik `IsAcknowledged` disahkan dan borang dihantar, `Submission.Status` terus bertukar ke `Submitted` dan declaration menjadi **tidak boleh diubah** (*immutable*) selama-lamanya. Tiada butang "Edit" disediakan langsung untuk declaration yang sudah `Submitted` — ini keputusan reka bentuk **sengaja**, bukan ketinggalan ciri.

> Rujukan rasmi: [learn.microsoft.com/ef/core/saving/transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions) · [learn.microsoft.com/aspnet/core/mvc/models/model-binding#collections](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding#collections)

---

Selesai baca bahagian konsep? Mula lab hands-on di [`snippets/lab.md`](./snippets/lab.md) — bina `ComplianceDeclarationViewModel`, `ComplianceController`, borang checklist dinamik, dan logik kunci selepas hantar.

> 🎤 **Nota penceramah/jurulatih:** [`nota-penceramah.md`](./nota-penceramah.md).
