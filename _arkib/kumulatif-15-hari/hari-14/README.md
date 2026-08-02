# Hari 14 — Aset ICT: Borang, Kelulusan & Inventori

Nota ini mengikut **aturcara rasmi SESI 41–43** — lihat [`../JADUAL.md`](../JADUAL.md), bahagian **HARI 14**. Hands-on penuh langkah demi langkah ada di [`snippets/lab.md`](./snippets/lab.md) — baca bahagian konsep di bawah dahulu, kemudian pindah ke lab untuk menaip kod sendiri.

> **Konvensyen kod:** Nota dalam **Bahasa Melayu**; semua kod, nama kelas/pembolehubah, nama fail, istilah teknikal dikekalkan dalam **Bahasa Inggeris** — ikut [`SPEC-KURSUS.md`](../SPEC-KURSUS.md).

> **Sambungan Hari 13:** Semalam kita tulis 5 entiti Modul 5 (`Asset`, `SoftwareCatalogItem`, `SoftwareRequest`, `AssetLoanRequest`, `AssetReturn`) + seed data. Hari ini kita bina **3 borang**, **semakan availability**, dan **transaksi selamat** yang mengemas kini status permohonan **dan** status aset **serentak** — inilah puncak Modul 5.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| EF Core — Transactions (`BeginTransactionAsync`) | [learn.microsoft.com/ef/core/saving/transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions) |
| EF Core — Concurrency & konflik simpan serentak | [learn.microsoft.com/ef/core/saving/concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency) |
| EF Core — Related data (`Include`) | [learn.microsoft.com/ef/core/querying/related-data/eager](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager) |
| ASP.NET Core MVC — Model validation | [learn.microsoft.com/aspnet/core/mvc/models/validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation) |
| ASP.NET Core MVC — Working with forms (Tag Helpers) | [learn.microsoft.com/aspnet/core/mvc/views/working-with-forms](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms) |
| ASP.NET Core — `TempData` (mesej selepas redirect) | [learn.microsoft.com/aspnet/core/fundamentals/app-state](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/app-state#tempdata) |
| ASP.NET Core — Role-based authorization (`[Authorize(Roles=...)]`) | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 41–42: 3 Borang** — permohonan perisian, pinjaman aset, pemulangan aset; semakan availability aset. 💻 **Lab:** bina 3 borang |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 43: Transaksi Inventori** — kelulusan & fulfillment ICT, `BeginTransactionAsync`, kemas kini status aset (`OnLoan`/`Available`/`UnderMaintenance`). 💻 **Lab:** transaksi selamat |
| 5.00 petang | Bersurai |

**Hasil Hari 14** (rujuk [`JADUAL.md`](../JADUAL.md)): Aliran pinjaman & pemulangan aset berfungsi hujung-ke-hujung.

---

## Tiga Borang, Satu Corak Sedia Ada

Ketiga-tiga borang hari ini (`SoftwareRequest`, `AssetLoanRequest`, `AssetReturn`) ikut **corak universal** yang sudah kita ulang sejak Hari 2 — `Form → Validation → Draft → Submit → Review → Approve/Reject → Audit`. Tiada konsep MVC baharu di sini; yang baharu ialah **peraturan perniagaan** khusus inventori:

| Borang | Prefix rujukan | Peraturan khas |
|--------|-----------------|-----------------|
| Permohonan Perisian | `SW` | Perisian mesti dipilih daripada `SoftwareCatalogItem` yang `IsActive` |
| Pinjaman Aset | `AST-L` | Pemohon nyatakan **kategori** sahaja (`RequestedCategory`); aset sebenar ditetapkan semasa fulfillment |
| Pemulangan Aset | `AST-R` | Mesti rujuk `AssetLoanRequest` yang sedia `Completed`; rekod semula `ConditionOnReturn` |

Nombor rujukan dijana melalui `IReferenceNumberService.GenerateAsync(moduleCode)` sedia ada sejak Hari 3 — panggil dengan prefix yang betul (`"SW"`, `"AST-L"`, `"AST-R"`) mengikut [`SPEC-KURSUS.md`](../SPEC-KURSUS.md).

---

## Kenapa Semakan *Availability* Berlaku Semasa Fulfillment, Bukan Semasa Submit?

Ini keputusan reka bentuk paling penting hari ini. Bayangkan senario:

1. 9.00 pagi — Ali hantar permohonan pinjaman kategori "Laptop".
2. 9.01 pagi — Aminah **juga** hantar permohonan pinjaman kategori "Laptop", tanpa tahu permohonan Ali.
3. Pada masa **submit**, kedua-dua permohonan **sah** — ada 2 laptop `Available` dalam gudang.
4. ICT Admin proses permohonan Ali dahulu → tetapkan `ICT-AST-0001` → laptop itu jadi `OnLoan`.
5. ICT Admin proses permohonan Aminah → **masih** ada 1 laptop lain `Available` (`ICT-AST-0002`) → tetapkan itu.

Kalau kita **paksa** pemohon pilih aset **spesifik** semasa submit (langkah 1–2), dua pemohon boleh "pilih" `ICT-AST-0001` yang **sama** serentak — konflik yang hanya boleh dikesan **selepas** kedua-dua sudah submit. Dengan menangguhkan pemilihan aset sebenar ke peringkat **fulfillment** (satu-satu, oleh satu ICT Admin, satu masa), kita elakkan konflik ini sepenuhnya — inilah sebab `AssetLoanRequest.AssetId` **nullable** (Hari 13).

Semakan availability di peringkat fulfillment cuma satu query mudah:

```csharp
var availableAssets = await _db.Assets
    .Where(a => a.Category == loanRequest.RequestedCategory
             && a.Status == AssetStatus.Available)
    .ToListAsync();
```

Jika senarai kosong — ICT Admin **tidak boleh** teruskan fulfillment (UI perlu halang ini secara eksplisit, bukan cuma "harap" admin perasan).

> Rujukan rasmi related data: [learn.microsoft.com/ef/core/querying/related-data/eager](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager)

---

## Kenapa `BeginTransactionAsync` Diperlukan Secara Eksplisit?

Peserta yang biasa dengan EF Core mungkin tertanya: "Bukankah setiap panggilan `SaveChangesAsync()` **sudah pun** dibalut dalam satu transaksi implisit oleh EF Core?" — **betul**, tetapi hanya untuk **satu** panggilan `SaveChangesAsync()`. Masalahnya, penyelesaian fulfillment pinjaman aset melibatkan **lebih daripada satu** langkah yang mesti **semua berjaya atau semua gagal bersama**:

1. Kemas kini `Submission.Status` → `Completed`.
2. Kemas kini `Asset.Status` → `OnLoan`, `Asset.CurrentHolderUserId` → pemohon.
3. Rekod `AuditLog` melalui `IAuditLogService.RecordAsync(...)` (yang mungkin memanggil `SaveChangesAsync()`-nya sendiri secara berasingan).

Jika langkah 1–2 berjaya disimpan tetapi langkah 3 (audit log) **gagal** (contohnya exception luar jangka), kita akan berakhir dengan sistem yang kata "pinjaman selesai, aset dipinjamkan" **tanpa** sebarang jejak audit — pelanggaran keperluan keselamatan asas (rujuk `nota/09-keselamatan.md`: *"Rekod audit logs untuk setiap tindakan penting"*). `BeginTransactionAsync()` membalut **kesemua** langkah dalam **satu** unit atom — jika mana-mana bahagian gagal, `RollbackAsync()` (atau exception yang tidak ditangkap sebelum `CommitAsync()`) membatalkan **semuanya**, memastikan pangkalan data tidak pernah berada dalam keadaan separuh-kemas kini.

```csharp
await using var transaction = await _db.Database.BeginTransactionAsync();

submission.Status = SubmissionStatus.Completed;
submission.CompletedAt = DateTime.UtcNow;

asset.Status = AssetStatus.OnLoan;
asset.CurrentHolderUserId = submission.ApplicantUserId;

await _db.SaveChangesAsync();
await _auditLogService.RecordAsync(submission.Id, "AssetLoanCompleted",
    $"Aset {asset.AssetTag} diserahkan kepada {submission.ApplicantUserId}");

await transaction.CommitAsync();
```

**Peraturan emas:** jika ada **apa-apa** kod di antara `BeginTransactionAsync()` dan `CommitAsync()` yang boleh gagal secara separa, transaksi eksplisit adalah wajib — bukan pilihan kosmetik.

> Rujukan rasmi: [learn.microsoft.com/ef/core/saving/transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions)

---

## Peraturan Status Aset Selepas Pemulangan

Berbeza daripada pinjaman (yang sentiasa berakhir `OnLoan`), pemulangan ada **dua** kemungkinan hasil bergantung kondisi aset:

```text
AssetReturn diproses
  ├─ Condition OK          → Asset.Status = Available   (sedia dipinjam semula)
  └─ RequiresMaintenance    → Asset.Status = UnderMaintenance (perlu servis dahulu)
```

ICT Admin yang memproses pemulangan **menentukan** hasil ini berdasarkan input `RequiresMaintenance` pada borang pemulangan — bukan sesuatu yang dikira automatik oleh sistem, kerana **hanya manusia** (juruteknik ICT) boleh nilai kerosakan fizikal sebenar.

---

## Selepas Ini

Modul 5 kini **lengkap hujung-ke-hujung**. Hari 15 akan menyambungkan **kesemua 5 modul** ke dalam satu navigasi & dashboard bersepadu, menulis ujian xUnit, dan bersedia untuk demo capstone.

Mula hands-on: [`snippets/lab.md`](./snippets/lab.md).

---

> 🎤 **Nota penceramah/jurulatih:** [`nota-penceramah.md`](./nota-penceramah.md).
