# Lab Hari 10 — PKS: Model Pematuhan

Lab ini mengiringi [`../README.md`](../README.md) Hari 10. Ikut latihan **secara berurutan**. Rujuk projek rujukan penuh di [`../../projek/`](../../projek/) untuk **banding** kod anda selepas cuba sendiri dahulu (jika folder itu masih kosong pada mesin anda, teruskan berdasarkan penerangan di bawah dan tanya fasilitator).

> **Peraturan lab:** Taip kod **sendiri**. Projek `Nres.Onboarding.Web` yang kita edit hari ini **sudah wujud** sejak Hari 1 — jangan cipta projek baharu.

---

## Senarai Semak Pra-Syarat

- [ ] Projek `Nres.Onboarding.Web` daripada Hari 1–9 masih boleh `dotnet build` tanpa ralat.
- [ ] `Data/ApplicationDbContext.cs`, `Models/Submission.cs`, `Models/SubmissionStatus.cs`, `Models/UserProfile.cs` sudah wujud.
- [ ] `dotnet ef --version` berjaya dijalankan.

---

## Latihan 1 — Entiti `PolicyVersion` & `ComplianceChecklistItem`

**Objektif:** Tulis dua entiti pertama Modul 4 — versi polisi dan item checklist yang dimilikinya.

1. Cipta fail `Models/PolicyVersion.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public class PolicyVersion
   {
       public int Id { get; set; }

       public string VersionCode { get; set; } = string.Empty;

       public string Title { get; set; } = string.Empty;

       public DateTime EffectiveDate { get; set; }

       public bool IsActive { get; set; }

       public List<ComplianceChecklistItem> ChecklistItems { get; set; } = new();
   }
   ```

   > **Kenapa `IsActive` bukan `bool`kekal `true` selamanya?** Hanya **satu** `PolicyVersion` patut aktif pada satu masa — versi lama ditanda `IsActive = false` apabila versi baharu berkuat kuasa, tetapi rekod lama **tidak dipadam** (rujuk sebab "snapshot bersejarah" dalam `README.md`).

2. Cipta fail `Models/ComplianceChecklistItem.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public class ComplianceChecklistItem
   {
       public int Id { get; set; }

       public int PolicyVersionId { get; set; }

       public PolicyVersion PolicyVersion { get; set; } = null!;

       public int SequenceNo { get; set; }

       public string Statement { get; set; } = string.Empty;

       public bool IsActive { get; set; } = true;
   }
   ```

   > **Kenapa `SequenceNo` (`int`), bukan bergantung pada urutan `Id`?** `Id` ialah kunci utama auto-generate — ia **tidak semestinya** susunan paparan yang betul (cth. jika item disusun semula kemudian, atau item baharu disisipkan di tengah). `SequenceNo` ialah medan **eksplisit** yang mengawal susunan paparan checklist dalam borang (Hari 11), bebas daripada `Id`.

✅ **Semakan:** Dua fail wujud dalam `Models/`, `dotnet build` masih berjaya.

---

## Latihan 2 — Entiti `ComplianceDeclaration` & `ComplianceResponse`

**Objektif:** Tulis entiti pengisytiharan (declaration) yang berpaut kepada `Submission` induk (Hari 1) dan `PolicyVersion`, serta respons individu bagi setiap item checklist.

1. Cipta fail `Models/ComplianceDeclaration.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public class ComplianceDeclaration
   {
       public int Id { get; set; }

       public int SubmissionId { get; set; }

       public Submission Submission { get; set; } = null!;

       public int PolicyVersionId { get; set; }

       public PolicyVersion PolicyVersion { get; set; } = null!;

       public bool IsAcknowledged { get; set; }

       public DateTime? DeclarationDate { get; set; }

       public List<ComplianceResponse> Responses { get; set; } = new();
   }
   ```

   > **Kenapa `SubmissionId` (hubungan satu-ke-satu dengan `Submission`), sama seperti Modul 2 Hari 4?** `ComplianceDeclaration` ialah jadual **detail khusus** Modul 4 — semua perkara sejagat (nombor rujukan, status, siapa pemohon, bila dihantar) tetap tinggal di `Submission` induk supaya `IReferenceNumberService`, `IAuditLogService`, `SubmissionStatus`, dan (kelak) senarai admin generik boleh **digunakan semula** tanpa ubah suai, persis Modul 1–3.

2. Cipta fail `Models/ComplianceResponse.cs`:

   ```csharp
   namespace Nres.Onboarding.Web.Models;

   public class ComplianceResponse
   {
       public int Id { get; set; }

       public int ComplianceDeclarationId { get; set; }

       public ComplianceDeclaration ComplianceDeclaration { get; set; } = null!;

       public int ChecklistItemId { get; set; }

       public ComplianceChecklistItem ChecklistItem { get; set; } = null!;

       public bool IsCompliant { get; set; }

       public string? Remarks { get; set; }
   }
   ```

   > **Kenapa `Remarks` boleh null (`string?`)?** Kebanyakan respons akan `IsCompliant = true` tanpa sebarang catatan tambahan. `Remarks` hanya **relevan** apabila `IsCompliant = false` (catatan ketidakpatuhan) — ini kita kuatkuasakan pada peringkat borang (Hari 11), bukan pada peringkat model.

✅ **Semakan:** Empat fail entiti PKS kini wujud dalam `Models/` (`PolicyVersion.cs`, `ComplianceChecklistItem.cs`, `ComplianceDeclaration.cs`, `ComplianceResponse.cs`). `dotnet build` masih berjaya.

---

## Latihan 3 — Daftar Entiti Dalam `ApplicationDbContext`

**Objektif:** Tambah `DbSet` bagi keempat-empat entiti baharu, dan konfigurasikan hubungan (relationship) menggunakan Fluent API.

1. Buka `Data/ApplicationDbContext.cs` **sedia ada** (jangan tulis semula fail ini dari kosong). Tambah empat `DbSet` baharu selepas `DbSet<UserProfile>` yang sedia ada:

   ```csharp
   public DbSet<PolicyVersion> PolicyVersions => Set<PolicyVersion>();

   public DbSet<ComplianceChecklistItem> ComplianceChecklistItems => Set<ComplianceChecklistItem>();

   public DbSet<ComplianceDeclaration> ComplianceDeclarations => Set<ComplianceDeclaration>();

   public DbSet<ComplianceResponse> ComplianceResponses => Set<ComplianceResponse>();
   ```

2. Dalam kaedah `OnModelCreating`, selepas konfigurasi entiti sedia ada (`Submission`, `Attachment`, `AuditLog`, `UserProfile`), tambah blok berikut:

   ```csharp
   builder.Entity<ComplianceChecklistItem>()
       .HasOne(i => i.PolicyVersion)
       .WithMany(p => p.ChecklistItems)
       .HasForeignKey(i => i.PolicyVersionId)
       .OnDelete(DeleteBehavior.Restrict);

   builder.Entity<ComplianceDeclaration>()
       .HasOne(d => d.Submission)
       .WithOne()
       .HasForeignKey<ComplianceDeclaration>(d => d.SubmissionId)
       .OnDelete(DeleteBehavior.Cascade);

   builder.Entity<ComplianceDeclaration>()
       .HasIndex(d => d.SubmissionId)
       .IsUnique();

   builder.Entity<ComplianceDeclaration>()
       .HasOne(d => d.PolicyVersion)
       .WithMany()
       .HasForeignKey(d => d.PolicyVersionId)
       .OnDelete(DeleteBehavior.Restrict);

   builder.Entity<ComplianceResponse>()
       .HasOne(r => r.ComplianceDeclaration)
       .WithMany(d => d.Responses)
       .HasForeignKey(r => r.ComplianceDeclarationId)
       .OnDelete(DeleteBehavior.Cascade);

   builder.Entity<ComplianceResponse>()
       .HasOne(r => r.ChecklistItem)
       .WithMany()
       .HasForeignKey(r => r.ChecklistItemId)
       .OnDelete(DeleteBehavior.Restrict);
   ```

   > **Kenapa `DeleteBehavior.Restrict` untuk `PolicyVersion` dan `ChecklistItem`, tetapi `Cascade` untuk `Submission` dan `ComplianceDeclaration`?** `Submission` ialah induk **eksklusif** kepada satu `ComplianceDeclaration` — jika `Submission` dipadam, declaration itu memang patut turut hilang (`Cascade`). Sebaliknya, `PolicyVersion`/`ComplianceChecklistItem` **dikongsi** oleh berpuluh/beratus declaration lain — memadam satu `PolicyVersion` **tidak** patut secara senyap memadam semua declaration yang merujuknya (`Restrict` — EF Core/SQLite akan **menolak** pemadaman jika masih ada declaration bergantung kepadanya, melindungi rekod bersejarah).

   > **Kenapa `HasIndex(d => d.SubmissionId).IsUnique()`?** Ini menguatkuasakan hubungan **satu-ke-satu** (*one-to-one*) sebenar di peringkat pangkalan data — satu `Submission` tidak boleh mempunyai lebih daripada satu `ComplianceDeclaration`.

3. `dotnet build` untuk sahkan tiada ralat kompil (perhatikan `using Nres.Onboarding.Web.Models;` sudah wujud di bahagian atas fail sejak Hari 1 — tidak perlu ditambah semula).

✅ **Semakan:** `ApplicationDbContext.cs` kini ada 4 `DbSet` baharu dan konfigurasi hubungan PKS, `dotnet build` berjaya.

---

## Latihan 4 — Seed Versi Polisi & Item Checklist

**Objektif:** Guna `HasData` untuk memasukkan satu `PolicyVersion` aktif berserta 6 `ComplianceChecklistItem` terus ke dalam migration.

1. Masih dalam `OnModelCreating`, tambah blok seed **selepas** blok konfigurasi hubungan Latihan 3:

   ```csharp
   var policyEffectiveDate = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc);

   builder.Entity<PolicyVersion>().HasData(
       new PolicyVersion
       {
           Id = 1,
           VersionCode = "PKS-POL-2026.1",
           Title = "Kod Etika Perkhidmatan Awam & Peraturan Pematuhan NRES 2026",
           EffectiveDate = policyEffectiveDate,
           IsActive = true
       });

   builder.Entity<ComplianceChecklistItem>().HasData(
       new ComplianceChecklistItem
       {
           Id = 1,
           PolicyVersionId = 1,
           SequenceNo = 1,
           Statement = "Saya faham dan akur dengan Kod Etika Perkhidmatan Awam Malaysia.",
           IsActive = true
       },
       new ComplianceChecklistItem
       {
           Id = 2,
           PolicyVersionId = 1,
           SequenceNo = 2,
           Statement = "Saya tidak mempunyai sebarang kepentingan peribadi/kewangan yang bercanggah dengan tugas rasmi saya, atau saya telah mengisytiharkannya kepada pihak berkuasa.",
           IsActive = true
       },
       new ComplianceChecklistItem
       {
           Id = 3,
           PolicyVersionId = 1,
           SequenceNo = 3,
           Statement = "Saya mematuhi peruntukan Akta Rahsia Rasmi 1972 berkaitan maklumat rasmi jabatan.",
           IsActive = true
       },
       new ComplianceChecklistItem
       {
           Id = 4,
           PolicyVersionId = 1,
           SequenceNo = 4,
           Statement = "Saya tidak pernah meminta, menerima, atau menawarkan rasuah/hadiah yang boleh menjejaskan integriti tugas saya.",
           IsActive = true
       },
       new ComplianceChecklistItem
       {
           Id = 5,
           PolicyVersionId = 1,
           SequenceNo = 5,
           Statement = "Saya menggunakan aset, sistem ICT, dan kemudahan jabatan hanya untuk tujuan rasmi yang dibenarkan.",
           IsActive = true
       },
       new ComplianceChecklistItem
       {
           Id = 6,
           PolicyVersionId = 1,
           SequenceNo = 6,
           Statement = "Saya telah membaca dan memahami Pekeliling Perkhidmatan berkaitan pematuhan kod etika NRES tahun semasa.",
           IsActive = true
       });
   ```

   > **Kenapa `EffectiveDate` guna pembolehubah `DateTime` tetap (`new DateTime(2026, 1, 1, ...)`), bukan `DateTime.UtcNow`?** `HasData` memerlukan nilai **tetap** (*static*) yang boleh "dibekukan" ke dalam kod migration yang dijana — EF Core akan beri **ralat masa kompil** jika anda cuba guna `DateTime.UtcNow` di sini, kerana nilai itu berubah setiap kali migration dijana semula.

2. `dotnet build` sekali lagi untuk sahkan tiada ralat.

✅ **Semakan:** Blok `HasData` untuk `PolicyVersion` (1 rekod) dan `ComplianceChecklistItem` (6 rekod) wujud dalam `OnModelCreating`, `dotnet build` berjaya.

---

## Latihan 5 — Migration `Module4Initial` & Sahkan Skema

**Objektif:** Jana migration, jalankan `dotnet ef database update`, dan sahkan jadual + data seed wujud dalam SQLite.

1. Jana migration:

   ```bash
   dotnet ef migrations add Module4Initial
   ```

2. Buka fail `Migrations/<timestamp>_Module4Initial.cs` yang dijana dan cari kaedah `Up()` — perhatikan:
   - Empat arahan `migrationBuilder.CreateTable(...)` untuk `PolicyVersions`, `ComplianceChecklistItems`, `ComplianceDeclarations`, `ComplianceResponses`.
   - Arahan `migrationBuilder.InsertData(...)` untuk seed `PolicyVersion` dan `ComplianceChecklistItem` — ini hasil `HasData` daripada Latihan 4.

3. Jalankan migration terhadap pangkalan data:

   ```bash
   dotnet ef database update
   ```

4. Sahkan data seed benar-benar masuk (jika `sqlite3` CLI dipasang):

   ```bash
   sqlite3 App_Data/nres.db "SELECT Id, VersionCode, IsActive FROM PolicyVersions;"
   sqlite3 App_Data/nres.db "SELECT Id, SequenceNo, Statement FROM ComplianceChecklistItems ORDER BY SequenceNo;"
   ```

   Anda patut nampak **1** baris `PolicyVersions` (`PKS-POL-2026.1`) dan **6** baris `ComplianceChecklistItems`, tersusun ikut `SequenceNo`.

✅ **Semakan:** `dotnet ef database update` berjaya tanpa ralat; jadual `PolicyVersions`, `ComplianceChecklistItems`, `ComplianceDeclarations`, `ComplianceResponses` wujud; `PolicyVersions` ada tepat 1 rekod aktif; `ComplianceChecklistItems` ada tepat 6 rekod berkaitan `PolicyVersionId = 1`.

---

## Rujukan Fail Sebenar

| Fail anda (lab) | Fail rujukan (projek sebenar) |
|------------------|-------------------------------|
| `Models/PolicyVersion.cs`, `ComplianceChecklistItem.cs`, `ComplianceDeclaration.cs`, `ComplianceResponse.cs` | `projek/Nres.Onboarding.Web/Models/` |
| `Data/ApplicationDbContext.cs` (kemas kini) | `projek/Nres.Onboarding.Web/Data/ApplicationDbContext.cs` |
| Migration `Module4Initial` | `projek/Nres.Onboarding.Web/Migrations/` |

> Jika folder `projek/` masih kosong pada mesin anda, teruskan lab berdasarkan penerangan di atas dan tanya fasilitator semasa sesi.

---

## Cabaran (Pilihan)

Selesaikan **sekurang-kurangnya satu** selepas Latihan 5 siap:

1. **Index carian pantas** — Tambah `HasIndex(d => d.DeclarationDate)` pada `ComplianceDeclaration` dalam `OnModelCreating`, supaya penapisan julat tarikh (Hari 12) lebih pantas pada set data besar. Jana migration baharu (`dotnet ef migrations add AddDeclarationDateIndex`).
2. **Kekangan `SequenceNo` unik** — Tambah `HasIndex(i => new { i.PolicyVersionId, i.SequenceNo }).IsUnique()` pada `ComplianceChecklistItem` supaya dua item dalam **versi polisi yang sama** tidak boleh berkongsi nombor urutan yang sama.
3. **Versi polisi kedua (draf masa depan)** — Tambah satu lagi `HasData` untuk `PolicyVersion` kedua (`Id = 2`, `VersionCode = "PKS-POL-2027.1"`, `IsActive = false`, `EffectiveDate` tahun 2027) — tanpa checklist item — untuk simulasi bagaimana versi polisi akan datang disediakan **sebelum** ia diaktifkan.

---

Nota penceramah (pemasaan sesi, silap biasa, soalan perbincangan, deliverable akhir hari): [`../nota-penceramah.md`](../nota-penceramah.md).
