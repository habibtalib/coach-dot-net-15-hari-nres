# Hari 3 — Lapor Diri: Lampiran, Submit & Semakan

Nota ini mengikut **aturcara rasmi HARI 3** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 8 hingga SESI 10. Bahagian ini menerangkan **konsep** (kenapa sesuatu wujud); langkah hands-on penuh, bernombor, dengan kod untuk ditaip sendiri, ada di [`snippets/lab.md`](./snippets/lab.md).

> **Sambungan Hari 1–2:** Kita meneruskan `Nres.Onboarding.Web`. Hari ini kita **lengkapkan** Modul 1 (Lapor Diri) hujung-ke-hujung — menggunakan `Attachment` (Hari 1), `OfficerReportingController` dan view sedia ada (Hari 2), **tanpa** menukar nama mana-mana entiti sedia ada.

> **Konvensyen bahasa:** Nota & penerangan dalam **Bahasa Melayu**; semua kod, nama kelas/pembolehubah, nama fail, dan istilah teknikal dikekalkan dalam **Bahasa Inggeris**.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Muat naik fail (`IFormFile`) | [learn.microsoft.com/aspnet/core/mvc/models/file-uploads](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads) |
| `IWebHostEnvironment` (laluan `ContentRootPath`) | [learn.microsoft.com/dotnet/api/microsoft.extensions.hosting.ihostenvironment](https://learn.microsoft.com/en-us/dotnet/api/microsoft.extensions.hosting.ihostenvironment) |
| Dependency Injection — daftar servis custom | [learn.microsoft.com/aspnet/core/fundamentals/dependency-injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) |
| `FileStream`/`Stream` I/O selamat (async) | [learn.microsoft.com/dotnet/api/system.io.filestream](https://learn.microsoft.com/en-us/dotnet/api/system.io.filestream) |
| Role-based Authorization (`[Authorize(Roles=...)]`) | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |
| EF Core — transaksi & `SaveChangesAsync` | [learn.microsoft.com/ef/core/saving/transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions) |
| Menyalurkan fail (`FileStreamResult`/`PhysicalFile`) | [learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.physicalfile](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.physicalfile) |
| ASP.NET Core Identity — urus peranan (`RoleManager`) | [learn.microsoft.com/aspnet/core/security/authentication/identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 8–9: Muat Naik Lampiran** — `IFileStorageService`, simpan di `App_Data/uploads/{id}/`, `Attachment` metadata, validasi saiz/jenis, nama fail selamat. 💻 Lab: muat naik + simpan metadata |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 10: Submit & Semakan HR** — `IReferenceNumberService` (`LD-2026-####`), tukar status ke `Submitted`, audit log, halaman semakan HR, approve/reject (wajib sebab). 💻 Lab: aliran penuh Modul 1 |
| 5.00 petang | Bersurai |

**Hasil Hari 3** (rujuk [`../JADUAL.md`](../JADUAL.md)): Lapor Diri menyokong draf, submit, lampiran, approve, reject, dan audit log.

---

## SESI 8–9 — Muat Naik Lampiran

### Kenapa `IFileStorageService`, bukan tulis logik fail terus dalam controller?

Menyimpan fail melibatkan beberapa langkah berulang tanpa mengira modul mana yang memuat naik fail (Lapor Diri hari ini; Pas Keselamatan, PKS, Aset ICT kemudian): sahkan saiz/jenis, jana nama fail selamat, cipta folder jika belum wujud, tulis ke cakera, cipta rekod `Attachment`. Jika logik ini ditulis terus dalam setiap controller, ia akan **diulang 5 kali** merentas 5 modul — persis masalah yang kita elak dengan `Submission` kongsi di Hari 1. `IFileStorageService` ialah **kontrak** (interface) yang menjadikan logik ini boleh diguna semula:

```csharp
public interface IFileStorageService
{
    Task<Attachment> SaveAsync(int submissionId, IFormFile file, CancellationToken cancellationToken = default);
}
```

### Kenapa "jangan sesekali percaya nama fail yang dimuat naik" (never trust the uploaded file name)?

Nama fail yang dihantar pelayar (`file.FileName`) datang terus daripada **pengguna** — ia boleh mengandungi aksara istimewa, laluan relatif (`../../etc/passwd`), atau bertindih dengan nama fail sedia ada di server. Jika kita guna nama itu **terus** sebagai nama fail fizikal, kita terdedah kepada:

- **Path traversal** — nama fail seperti `../../Program.cs` cuba menulis di luar folder yang dimaksudkan.
- **Penimpaan fail** — dua pengguna muat naik fail bernama `ic.pdf` pada masa sama, satu menimpa yang lain.
- **Aksara tidak sah sistem fail** — nama fail dengan `:`, `*`, `?` (Windows) atau lain-lain aksara khas boleh menyebabkan ralat I/O.

Penyelesaian: **jana** nama fail fizikal (`StoredFileName`) menggunakan `Guid.NewGuid()` + sambungan fail asal sahaja, dan simpan `OriginalFileName` (nama yang dimuat naik pengguna) **semata-mata sebagai metadata paparan** — inilah sebab `Attachment` (Hari 1) mempunyai dua medan nama berasingan.

### Kenapa fail disimpan di `App_Data/uploads/{submissionId}/`, bukan `wwwroot/uploads/`?

Seperti dibincang Hari 1, apa sahaja dalam `wwwroot/` boleh dicapai **terus** oleh sesiapa sahaja yang tahu URL — tiada semakan kebenaran (authorization) berlaku. Lampiran Lapor Diri (salinan kad pengenalan, surat lantikan) adalah dokumen **sensitif**; ia mesti disalurkan melalui satu action controller yang menyemak "adakah pengguna ini dibenarkan lihat lampiran submission ini?" sebelum menghantar kandungan fail. Struktur folder `{submissionId}/` (bukan folder rata/tunggal) juga memudahkan padam **semua** lampiran satu permohonan sekaligus (padam satu folder), dan mengelak pertembungan nama fail antara permohonan berlainan.

### Validasi saiz & jenis fail

```csharp
if (file.Length > 5 * 1024 * 1024)
{
    ModelState.AddModelError("Attachment", "File size must not exceed 5 MB.");
}
```

Selain saiz, kita juga sekat **jenis** fail (sambungan/`ContentType`) — hanya benarkan PDF dan imej biasa (`.pdf`, `.jpg`, `.jpeg`, `.png`) untuk lampiran Lapor Diri. Ini mengelak pemuat naikan fail boleh laku (`.exe`, `.sh`, `.dll`) yang berisiko jika kelak dibuka/dijalankan secara tidak sengaja oleh mana-mana proses.

> Rujukan rasmi: [learn.microsoft.com/aspnet/core/mvc/models/file-uploads](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads)

---

## SESI 10 — Submit & Semakan HR

### Kenapa `IReferenceNumberService` berasingan, dan kenapa nombor rujukan hanya dijana semasa Submit?

Format nombor rujukan (`LD-2026-0001`) memetakan terus kepada prefix modul dalam [SPEC-KURSUS.md](../../SPEC-KURSUS.md) ("Prefix nombor rujukan"). Jika nombor dijana **semasa draf**, dan pemohon tidak pernah submit permohonan itu (dibatalkan/ditinggalkan), nombor tersebut "terbazir" — jurang nombor (`LD-2026-0001`, terus `LD-2026-0005`) yang mengelirukan bila diaudit kemudian. Sebaliknya, nombor dijana **hanya sekali**, tepat semasa `Submit` berjaya:

```csharp
public interface IReferenceNumberService
{
    Task<string> GenerateAsync(string moduleCode);
}
```

Kita kira bilangan `Submission` sedia ada bagi `moduleCode` dan tahun semasa, tambah satu, dan format sebagai `{prefix}-{tahun}-{nombor:D4}` (`D4` = pad kepada 4 digit dengan sifar hadapan). Butiran penuh dalam lab.

### Kenapa `IAuditLogService` berasingan, bukan `_db.AuditLogs.Add(...)` terus di setiap controller?

Sama seperti `IFileStorageService`, ini corak **DRY (Don't Repeat Yourself)** — merekod audit log memerlukan gabungan medan yang sama (`SubmissionId`, `ActorUserId`, `Action`, `Remarks`, `CreatedAt`) tanpa mengira modul mana yang mencetuskannya:

```csharp
public interface IAuditLogService
{
    Task RecordAsync(int submissionId, string action, string? remarks = null);
}
```

Servis ini dipanggil pada **setiap** titik penting kitaran hayat permohonan: cipta draf, submit, approve, reject. Dengan servis ini, halaman "sejarah audit" (dibina penuh Hari 15) hanya perlu baca jadual `AuditLogs` — ia sudah lengkap tanpa mengira modul.

### Kenapa "penolakan wajib sebab" (rejection requires reason)?

Ini peraturan perniagaan asas dalam sistem kelulusan kerajaan/korporat — pemohon yang ditolak **berhak tahu kenapa**, dan HR admin perlu bertanggungjawab atas keputusan mereka. Secara teknikal, ini dikuatkuasakan dengan menjadikan medan `Remarks` **wajib** (validation) apabila tindakan ialah `Reject`, tetapi **pilihan** apabila `Approve`.

### Kenapa halaman semakan HR dilindungi `[Authorize(Roles = "HrAdmin")]`?

Peranan `HrAdmin` (daripada [SPEC-KURSUS.md](../../SPEC-KURSUS.md), jadual "Peranan") bertanggungjawab menyemak Lapor Diri. Menyekat halaman ini kepada peranan tersebut memastikan hanya kakitangan HR yang dibenarkan boleh melihat dan meluluskan/menolak permohonan — pemohon biasa (`Applicant`) tidak sepatutnya boleh akses laluan ini walaupun mereka tahu URL-nya. Ini **authorization di peringkat controller**, bukan sekadar sembunyikan pautan di UI — pautan yang disembunyikan di UI tetap boleh diakses terus melalui URL jika tiada semakan di sisi pelayan.

> Rujukan rasmi: [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) · [learn.microsoft.com/ef/core/saving/transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions)

---

## Seterusnya

Baca dan ikuti langkah demi langkah di [`snippets/lab.md`](./snippets/lab.md) — di situ anda akan:

1. Tulis `IFileStorageService` + `LocalFileStorageService`, daftarkan dalam `Program.cs`.
2. Tambah muat naik lampiran ke borang Create/Edit Lapor Diri, dengan validasi saiz/jenis.
3. Tulis `IReferenceNumberService` + `SequentialReferenceNumberService`.
4. Tulis `IAuditLogService` + `AuditLogService`.
5. Tambah action `Submit` yang menjana nombor rujukan, tukar status, dan rekod audit.
6. Bina peranan `HrAdmin` dan halaman semakan HR dengan approve/reject.
7. Uji aliran penuh: draf → lampiran → submit → semakan HR → approve/reject → audit.

Nota penceramah (pemasaan sesi, silap biasa, soalan perbincangan): [`nota-penceramah.md`](./nota-penceramah.md).
