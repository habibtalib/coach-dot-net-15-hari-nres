# Hari 5 — Pas/Parking/Pelekat: Borang & Peraturan

Nota ini mengikut **HARI 5** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 14–16 (Modul 2: Pas, Parking & Pelekat Kenderaan). Lab hands-on penuh ada di [`snippets/lab.md`](./snippets/lab.md).

> **Sambungan projek:** Hari 4 telah cipta `Vehicle`, `AccessPassApplication`, `VehicleStickerApplication`, `ParkingApplication`, migration `Module2Initial`, dan halaman landing `Module2Controller`. Hari ini kita **bina borang sebenar** untuk ketiga-tiga jenis permohonan di atas entiti tersebut.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Model validation (DataAnnotations) | [learn.microsoft.com/aspnet/core/mvc/models/validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation) |
| Custom validation — `IValidatableObject` | [learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations.ivalidatableobject](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.ivalidatableobject) |
| `Enumerable.Any` / `Queryable.AnyAsync` | [learn.microsoft.com/dotnet/api/microsoft.entityframeworkcore.queryableextensions.anyasync](https://learn.microsoft.com/en-us/dotnet/api/microsoft.entityframeworkcore.queryableextensions.anyasync) |
| Tag Helpers untuk borang (`asp-for`, `asp-validation-for`) | [learn.microsoft.com/aspnet/core/mvc/views/working-with-forms](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms) |
| `SelectList` & dropdown dari enum | [learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.rendering.selectlist](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.rendering.selectlist) |
| `ModelState` & validation summary | [learn.microsoft.com/aspnet/core/mvc/models/validation#validation-errors](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation#validation-errors) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 14–15: Bina 3 Borang** — pas keselamatan, pelekat kenderaan, parkir; medan `Vehicle` dikongsi antara dua borang terakhir. 💻 **Lab:** bina 3 borang |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 16: Peraturan Perniagaan** — satu pas aktif/pemohon, satu pelekat aktif/kenderaan, parkir khas perlu justifikasi. 💻 **Lab:** conditional validation + `AnyAsync` duplicate check |
| 5.00 petang | Bersurai |

---

## Kenapa `IValidatableObject`, bukan sekadar `[Required]`?

`[Required]`, `[StringLength]`, dsb. sesuai untuk peraturan **medan tunggal** ("medan ini mesti diisi"). Tetapi Modul 2 ada peraturan yang bergantung pada **medan lain**:

- `ReplacementReason` **hanya** wajib jika `PassType == Replacement` (pas baharu tidak perlukan sebab).
- `Justification` **hanya** wajib jika `ParkingType == Khas` (parkir biasa tidak perlukan justifikasi).
- Medan kenderaan baharu (`RegistrationNo`, `Type`, `MakeModel`, `OwnerName`) **hanya** wajib jika pemohon **tidak** memilih kenderaan sedia ada dari senarai.

Ini dipanggil **conditional validation** — peraturan yang bentuknya "kalau X, maka Y wajib". `DataAnnotations` sahaja tidak boleh nyatakan syarat begini secara natif merentasi dua medan berbeza. `IValidatableObject.Validate(ValidationContext)` membenarkan kita tulis **logik C# biasa** yang dijalankan **selepas** semua `[Required]`/`[StringLength]` disahkan, dan `ModelState.IsValid` di controller secara automatik mengambil kira hasilnya — tiada kod tambahan di controller diperlukan untuk "menyambungkan" pengesahan ini.

## Kenapa semakan pendua (duplicate check) guna `AnyAsync`, bukan `Count() > 0`?

`AnyAsync(predicate)` diterjemah oleh EF Core kepada `SELECT EXISTS(...)` di peringkat pangkalan data — ia **berhenti** sebaik menemui satu padanan. `Count(predicate) > 0` pula memaksa pangkalan data **kira semua** rekod sepadan dahulu sebelum dibandingkan dengan `0` — lebih perlahan untuk jadual besar walaupun hasil akhirnya sama. Untuk semakan "adakah wujud...?" — `AnyAsync` **sentiasa** pilihan betul.

### Peraturan pendua rasmi (rujuk [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) & panduan induk)

```csharp
var hasActiveApplication = await _db.VehicleStickerApplications
    .AnyAsync(x =>
        x.VehicleId == vehicleId &&
        x.Submission.Status != SubmissionStatus.Rejected &&
        x.Submission.Status != SubmissionStatus.Cancelled &&
        x.Submission.Status != SubmissionStatus.Completed);
```

Perhatikan: kita **tidak** kecualikan `Draft` — ini bermakna seorang pemohon **tidak boleh** ada dua draf pas keselamatan terbuka pada masa sama (mereka perlu teruskan/padam draf sedia ada dahulu). Ini elak kekeliruan "yang mana draf sebenar saya?" dan konsisten dengan definisi "aktif" = apa-apa status **selain** `Rejected`/`Cancelled`/`Completed`.

| Peraturan | Jadual disemak | Skop semakan |
|-----------|-----------------|----------------|
| Satu pas keselamatan aktif / pemohon | `AccessPassApplications` | `Submission.ApplicantUserId == currentUserId` |
| Satu pelekat kenderaan aktif / kenderaan | `VehicleStickerApplications` | `VehicleId == vehicleId` (kenderaan yang sama, mungkin pemohon lain jika kereta dikongsi) |
| Parkir khas perlu justifikasi | *(bukan pendua — validation medan bersyarat)* | `ParkingType == Khas` |
| Pas ganti perlu sebab | *(bukan pendua — validation medan bersyarat)* | `PassType == Replacement` |

> **Nota penting:** semakan pendua **mengecualikan rekod semasa** apabila menyunting draf sedia ada (`WHERE Id != currentSubmissionDetailId`) — jika tidak, draf yang sama akan sentiasa dianggap "pendua bagi dirinya sendiri" setiap kali disimpan semula.

## Draf vs Submit — kenapa dua tahap pengesahan berbeza?

Corak sejagat (`Form → Validation → Draft → Submit`) bermaksud **draf boleh tidak lengkap** — pemohon patut boleh simpan borang separuh isi dan sambung kemudian. Tetapi **submit rasmi mesti lengkap dan sah 100%**, termasuk semua peraturan bersyarat & semakan pendua. Kita capai ini dengan **satu** view model yang sama, tetapi *gate* validation berbeza di controller:

- **Simpan Draf** → longgarkan (`ModelState.Clear()` sebelum semak pendua sahaja) — benarkan medan kosong.
- **Hantar (Submit)** → penuh (`ModelState.IsValid` mesti `true`, **dan** semakan pendua mesti bersih) sebelum `Submission.Status` bertukar ke `Submitted` dan nombor rujukan dijana.

---

Selesai baca konsep? Mula bina tiga borang di [`snippets/lab.md`](./snippets/lab.md).

> 🎤 **Nota penceramah/jurulatih:** [`nota-penceramah.md`](./nota-penceramah.md).
