# Hari 3 — Refresher .NET & Asas Kongsi

Nota ini mengikut **aturcara rasmi HARI 3** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 9 hingga SESI 12. Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

> **Hari ini kita menaip kod — bersama-sama, sekali sahaja.** Semua yang dibina hari ini dikongsi oleh keempat-empat kumpulan. Selepas hari ini, setiap kumpulan bekerja dalam modulnya sendiri selama 11 hari.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `dotnet new` templates | [learn.microsoft.com/dotnet/core/tools/dotnet-new](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new) |
| ASP.NET Core MVC — gambaran | [learn.microsoft.com/aspnet/core/mvc/overview](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview) |
| `Program.cs` & minimal hosting | [learn.microsoft.com/aspnet/core/fundamentals/minimal-apis](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) |
| Dependency Injection | [learn.microsoft.com/aspnet/core/fundamentals/dependency-injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) |
| LINQ | [learn.microsoft.com/dotnet/csharp/linq](https://learn.microsoft.com/en-us/dotnet/csharp/linq/) |
| `async`/`await` | [learn.microsoft.com/dotnet/csharp/asynchronous-programming](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/) |
| EF Core — permulaan | [learn.microsoft.com/ef/core/get-started/overview/first-app](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app) |
| EF Core — Fluent API & `IEntityTypeConfiguration` | [learn.microsoft.com/ef/core/modeling](https://learn.microsoft.com/en-us/ef/core/modeling/) |
| EF Core — relationships | [learn.microsoft.com/ef/core/modeling/relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships) |
| EF Core Migrations | [learn.microsoft.com/ef/core/managing-schema/migrations](https://learn.microsoft.com/en-us/ef/core/managing-schema/migrations/) |
| ASP.NET Core Identity | [learn.microsoft.com/aspnet/core/security/authentication/identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) |
| Role-based authorization | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 9.15 – 9.30 pagi | Pendaftaran & Minum Pagi |
| **9.30 – 11.00 pagi** | **SESI 9: Teras C# & ASP.NET Core** — OOP, LINQ, `async/await`, DI; `dotnet new mvc`, `Program.cs`, middleware pipeline, corak Controller/View/ViewModel. 💻 Lab: projek berjalan |
| **11.00 – 12.30 tgh** | **SESI 10: EF Core & Entiti Kongsi** — `DbContext`, Data Annotations vs Fluent API, hubungan & kunci asing. 💻 Lab: entiti kongsi + `IEntityTypeConfiguration<T>` |
| 12.30 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.30 petang** | **SESI 11: Identity, RBAC & Servis Kongsi** — Identity, `[Authorize(Roles=...)]`, seed peranan; 6 servis kongsi; partial view + `SubmissionControllerBase`. 💻 Lab: asas kongsi lengkap |
| **3.30 – 4.30 petang** | **SESI 12: Seni Bina Anti-Konflik & Buka Cabang** — modul mendaftar diri, `ApplyConfigurationsFromAssembly`, `ModuleDescriptor`; migration `InitialShared`; gabung ke `master`; buka 4 cabang. 💻 Lab: migration + cabang |
| 4.30 petang | Bersurai |

**Hasil Hari 3:** `master` mengandungi asas kongsi lengkap + migration `InitialShared`; 4 cabang kumpulan dibuka; setiap kumpulan tahu **tepat** fail mana miliknya dan komponen kongsi mana yang **tidak boleh** ditulis semula.

---

## SESI 9 — Teras C# & ASP.NET Core

### Empat konsep C# yang kita guna setiap hari

Ini bukan tutorial C# lengkap — ini empat perkara yang muncul dalam **setiap** fail yang anda tulis selama 11 hari akan datang.

**1. Dependency Injection (DI).** Bukannya kelas mencipta kebergantungannya sendiri, ia **memintanya** melalui constructor, dan ASP.NET Core membekalkannya:

```csharp
// ❌ Terikat rapat — mustahil diuji, mustahil ditukar
public class OfficerReportingController : Controller
{
    private readonly ApplicationDbContext _db = new ApplicationDbContext();
}

// ✅ Disuntik — boleh diuji, boleh ditukar, ASP.NET Core menguruskan hayatnya
public class OfficerReportingController(ApplicationDbContext db, IAuditLogService audit) : Controller
{
    // db dan audit sedia untuk digunakan
}
```

Sintaks kedua itu ialah **primary constructor** — ciri C# 12+ yang kita gunakan sepanjang kursus. Ia mengurangkan lima baris boilerplate menjadi satu.

**Kenapa DI penting bagi kita secara khusus:** keempat-empat kumpulan akan menyuntik servis kongsi yang sama (`IReferenceNumberService`, `IAuditLogService`). Kerana ia disuntik dan bukan dicipta, terdapat **satu** pelaksanaan yang dikongsi — bukan empat.

**2. `async`/`await`.** Setiap operasi pangkalan data dan fail adalah asynchronous. Peraturan mudah: jika kaedah memanggil sesuatu yang `async`, ia sendiri mestilah `async`, dan anda `await` panggilan itu.

```csharp
public async Task<IActionResult> Index()
{
    var senarai = await _db.Submissions
        .Where(s => s.ModuleCode == "LD")
        .ToListAsync();          // ← ToListAsync, bukan ToList
    return View(senarai);
}
```

**Kenapa?** Semasa pangkalan data bekerja, thread dibebaskan untuk melayan permintaan lain. Dengan 20 pengguna serentak ini tidak kelihatan; dengan 500 ia perbezaan antara sistem yang responsif dan yang tergantung.

**3. LINQ.** Cara kita bertanya kepada pangkalan data dalam C#, bukan SQL:

```csharp
var menunggu = await _db.Submissions
    .Where(s => s.Status == SubmissionStatus.Submitted)
    .Where(s => s.ModuleCode == ModuleCodes.LaporDiri)
    .OrderByDescending(s => s.SubmittedAt)
    .Take(20)
    .ToListAsync();
```

EF Core menterjemah ini menjadi SQL. **Perangkap penting:** apa-apa yang datang **selepas** `ToListAsync()` berlaku dalam memori, bukan dalam pangkalan data. Menapis selepas memuatkan 10,000 baris memuatkan kesemua 10,000 baris.

**4. Nullable reference types.** Dihidupkan sepanjang kursus. `string` bermaksud "tidak pernah null"; `string?` bermaksud "boleh null". Pengkompil menguatkuasakannya, yang menangkap satu kelas pepijat sepenuhnya sebelum ia berjalan.

### Versi bahasa: kita menggunakan C# 14

.NET 10 SDK dilengkapi pengkompil **Roslyn 5.0**, yang lalainya ialah **C# 14**. Anda **tidak** perlu menetapkan `<LangVersion>` dalam `.csproj` — ia automatik.

| .NET SDK | Roslyn | C# lalai |
|----------|--------|----------|
| 8.0 | 4.8 | 12.0 |
| 9.0 | 4.12 | 13.0 |
| **10.0** | **5.0** | **14.0** |

Tiga ciri C# 14 yang kita gunakan dalam kursus ini:

**`field` keyword** — sifat dengan logik, tanpa medan sokongan (*backing field*) yang dinamakan:

```csharp
// Sebelum C# 14
private string _catatan = string.Empty;
public string Catatan
{
    get => _catatan;
    set => _catatan = value.Trim();
}

// C# 14 — pengkompil mencipta medan sokongan untuk anda
public string Catatan
{
    get => field;
    set => field = value.Trim();
}
```

> ⚠️ **Jangan gunakan `field` untuk menormalkan nilai dalam entiti EF Core.** Transformasi tersembunyi dalam setter mengelirukan sesiapa yang membaca entiti, dan boleh mengejutkan penjejakan perubahan EF Core. Dalam kursus ini, normalisasi berlaku **eksplisit** dalam servis. Guna `field` untuk **view model** dan sifat bukan-EF.

**Null-conditional assignment** — `?.` kini berfungsi di sebelah **kiri** umpukan:

```csharp
// Sebelum C# 14
if (app.Submission is not null)
    app.Submission.ReferenceNo = rujukan;

// C# 14
app.Submission?.ReferenceNo = rujukan;
```

Sebelah kanan **tidak dinilai** jika sebelah kiri null — jadi ia selamat walaupun `rujukan` ialah panggilan yang mahal.

**File-based apps** — jalankan satu fail `.cs` tanpa projek:

```bash
dotnet run demo-linq.cs
```

Berguna untuk mencuba idea dalam beberapa saat. Kita menggunakannya dalam lab hari ini untuk menerangkan LINQ dan `async` **sebelum** membina projek sebenar.

> **Buku rujukan:** *C# 14 and .NET 10* (Mark J. Price) — Bab 2 untuk C# asas, Bab 5 untuk `field` dan OOP, Bab 10 untuk EF Core. Pemetaan penuh: [`../nota/10-rujukan-buku.md`](../nota/10-rujukan-buku.md).

### `Program.cs` dan middleware pipeline

.NET 10 menggunakan **minimal hosting model**: satu fail `Program.cs`, tiada `Startup.cs`.

```csharp
var builder = WebApplication.CreateBuilder(args);
// ... builder.Services.Add...()   ← daftar servis (DI container)
var app = builder.Build();
// ... app.Use...()                ← bina middleware pipeline
app.Run();
```

**Middleware pipeline** ialah siri lapisan yang setiap permintaan HTTP lalui, mengikut turutan:

```text
Permintaan →  HTTPS redirect → Routing → Authentication → Authorization → Controller
Respons   ←────────────────────────────────────────────────────────────────┘
```

**Susunan penting, dan bukan sewenang-wenangnya.** `UseAuthentication()` menjawab *"siapa anda?"* dan mesti berjalan sebelum `UseAuthorization()` yang menjawab *"adakah anda dibenarkan?"*. Terbalikkannya dan setiap semakan kebenaran gagal kerana tiada siapa dikenal pasti lagi.

### Corak MVC

| Bahagian | Tanggungjawab | Contoh |
|----------|---------------|--------|
| **Model** | Data & peraturan domain | `Submission`, `OfficerReportingApplication` |
| **View** | Paparan (Razor) | `Views/OfficerReporting/Create.cshtml` |
| **Controller** | Kendalikan permintaan, selaraskan | `OfficerReportingController` |
| **ViewModel** | Bentuk data untuk **satu** skrin | `OfficerReportingCreateViewModel` |

**Kenapa ViewModel wujud secara berasingan daripada entiti?** Kerana borang anda dan jadual anda bukan benda yang sama. Borang mempunyai medan pengesahan ("Saya mengesahkan maklumat ini benar"), senarai dropdown, dan peraturan validation yang tidak tergolong dalam pangkalan data. Mengikat borang terus kepada entiti juga membuka **over-posting** — penyerang menghantar `Status=AdminApproved` bersama borang dan EF Core dengan senang hati menyimpannya.

---

## SESI 10 — EF Core & Entiti Kongsi

### `DbContext` — jambatan

`DbContext` memetakan kelas C# kepada jadual pangkalan data. Setiap `DbSet<T>` mewakili satu jadual. Kerana kita menggunakan Identity, `ApplicationDbContext` **mewarisi** `IdentityDbContext<IdentityUser>` supaya jadual Identity (`AspNetUsers`, `AspNetRoles`, …) berkongsi pangkalan data dan skop transaksi yang sama.

### Entiti kongsi yang kita bina hari ini

Kesemuanya memetakan terus kepada latihan "peta medan sama" dari Hari 1:

| Entiti | Peranan |
|--------|---------|
| **`Submission`** | Rekod induk setiap permohonan, tidak kira modul. `ReferenceNo`, `ModuleCode`, `ApplicantUserId`, `Status`, tarikh. |
| **`Attachment`** | Metadata fail (bukan kandungan — fail fizikal tinggal di `App_Data/uploads/`) |
| **`AuditLog`** | Sejarah tindakan terhadap satu `Submission` |
| **`ApprovalStep`** | Satu baris setiap kedudukan dalam laluan kelulusan — menyokong kelulusan berbilang peringkat Kumpulan 3 |
| **`UserProfile`** | Maklumat staf (nama, jabatan, gred), **berasingan** daripada `AspNetUsers` |
| Lookup | `LookupDepartments`, `LookupGrades`, `LookupPositions` |

**Kenapa `UserProfile` berasingan daripada `AspNetUsers`?** `AspNetUsers` direka untuk **pengesahan** — hash kata laluan, e-mel log masuk, token keselamatan. Ia bukan tempat untuk medan perniagaan seperti jabatan atau gred. Mencampurkan kedua-dua tanggungjawab ini menjadikan peningkatan Identity menyakitkan dan mengaburkan sempadan antara "siapa anda" dan "apa jawatan anda".

**Kenapa satu `SubmissionStatus` untuk semua modul?** Ini keputusan seni bina paling penting kursus:

```csharp
public enum SubmissionStatus
{
    Draft = 0, Submitted = 1, SupervisorApproved = 2,
    AdminApproved = 3, Rejected = 4, Completed = 5, Cancelled = 6
}
```

Dengan satu enum kongsi, logik seperti *"papar semua permohonan menunggu kelulusan saya"* atau *"kira permohonan `Rejected` bulan ini"* ditulis **sekali** dan berfungsi untuk keempat-empat modul. Dengan empat enum, ia ditulis empat kali — dan Papan Pemuka Induk Hari 15 menjadi mustahil.

### Data Annotations vs Fluent API

| | Data Annotations | Fluent API |
|---|------------------|------------|
| Di mana | Atribut pada sifat entiti | `IEntityTypeConfiguration<T>` |
| Bagus untuk | Validation borang | Pemetaan pangkalan data |
| Contoh | `[Required]`, `[MaxLength(200)]` | `.HasIndex()`, `.OnDelete()`, indeks unik ditapis |

**Peraturan kursus:** validation pada **view model** melalui Data Annotations; pemetaan pangkalan data pada **entiti** melalui Fluent API. Ini memisahkan "apa yang borang terima" daripada "apa yang pangkalan data simpan".

### `IEntityTypeConfiguration<T>` — dan kenapa ia penting untuk kolaborasi

Anda **boleh** meletakkan semua konfigurasi Fluent API di dalam `OnModelCreating` dalam `ApplicationDbContext`. Kebanyakan tutorial melakukannya. **Kita tidak** — dan sebabnya adalah kolaborasi, bukan kegemaran.

Jika keempat-empat kumpulan menambah konfigurasi ke `OnModelCreating`, keempat-empatnya mengedit fail yang sama setiap hari selama 11 hari. Itu bukan risiko konflik — itu **jaminan** konflik.

Sebaliknya, setiap entiti membawa konfigurasinya sendiri, dalam folder modul pemiliknya:

```csharp
// Models/Akses/Configurations/VehicleConfiguration.cs — milik Kumpulan 2
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.Property(v => v.PlateNumber).HasMaxLength(20).IsRequired();
        builder.HasIndex(v => v.PlateNumber);
    }
}
```

Dan `ApplicationDbContext` menemui kesemuanya dengan **satu** baris yang tidak pernah berubah:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
}
```

Setiap kumpulan menambah **fail baharu**. Tiada siapa mengedit `DbContext`. Sifar konflik.

> Ini corak yang sama yang kita gunakan untuk `Program.cs` dan navigasi. Prinsipnya: **jadikan lanjutan bermaksud menambah fail, bukan mengedit fail.**

### Migration

Migration ialah sejarah berversi bagi skema pangkalan data anda. Setiap perubahan pada entiti C# dijana sebagai fail migration yang boleh dijalankan pada mana-mana persekitaran secara konsisten.

```bash
dotnet ef migrations add InitialShared
dotnet ef database update
```

- `migrations add` — EF Core membandingkan model C# semasa dengan snapshot terakhir dan menjana kod `Up()`/`Down()`
- `database update` — menjalankan migration yang belum diguna pakai

**Migration pertama kita dinamakan `InitialShared`** — nama itu memberi isyarat bahawa ia mengandungi **entiti kongsi sahaja**, bukan apa-apa khusus modul.

> ⚠️ **Slot migration bermula esok.** EF Core menyimpan satu fail `ApplicationDbContextModelSnapshot.cs` untuk seluruh skema. Dari Hari 4, hanya **satu kumpulan pada satu masa** boleh menjana migration. Protokol: [`../KOLABORASI.md`](../KOLABORASI.md) §5.

---

## SESI 11 — Identity, RBAC & Servis Kongsi

### Identity dan peranan

ASP.NET Core Identity mengendalikan pengguna, kata laluan (di-hash), log masuk, kunci akaun, dan token. Kita menggunakan `IdentityUser` biasa — butiran staf tergolong dalam `UserProfile`.

**Enam peranan** kita seed hari ini (rujuk [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md)):

`Applicant` · `Supervisor` · `HrAdmin` · `IctSecurityOfficer` · `IctAdmin` · `SecurityAdmin` · `FacilityAdmin` · `SystemAdmin`

Kuatkuasaan berlaku pada controller:

```csharp
[Authorize(Roles = "SecurityAdmin")]
public async Task<IActionResult> Review(int id) { ... }
```

**Kenapa authorization pada pelayan, bukan hanya menyembunyikan butang?** Menyembunyikan butang menghalang klik yang jujur. Ia tidak menghalang sesiapa daripada menaip URL. Setiap tindakan yang memerlukan peranan mesti menyemaknya pada pelayan — **setiap kali**, tiada pengecualian. Ini disemak dalam setiap code review.

### Enam servis kongsi

Ini adalah **daftar anti-redundan**. Selepas hari ini, menulis semula mana-mana daripada ini ialah kegagalan code review.

| Servis | Fungsi | Hayat |
|--------|--------|-------|
| `ICurrentUserService` | Pengguna semasa, peranan, jabatan | Scoped |
| `IReferenceNumberService` | `LD-2026-0001` mengikut prefix modul | Scoped |
| `IAuditLogService` | Catat tindakan ke `AuditLogs` | Scoped |
| `IWorkflowService` | Sahkan & laksanakan peralihan status | Scoped |
| `IFileStorageService` | Simpan/baca fail di `App_Data/uploads/` | Singleton |
| `INotificationService` | Notifikasi (latihan: konsol) | Singleton |

**Kenapa Scoped vs Singleton?** Servis yang menyentuh `DbContext` (juga Scoped) atau permintaan semasa mesti Scoped — hayatnya mesti sepadan dengan satu permintaan. Servis tanpa keadaan dan selamat-thread boleh Singleton.

### `IWorkflowService` — kenapa peralihan status memerlukan servis

Ia kelihatan cukup mudah untuk menulis `submission.Status = SubmissionStatus.AdminApproved;` dan selesai. Ini punca satu kelas pepijat sepenuhnya.

Peralihan status mempunyai **peraturan**: permohonan `Draft` tidak boleh melompat terus ke `AdminApproved`. Permohonan `Rejected` tidak boleh ditolak semula. Permohonan `Cancelled` tidak boleh diapa-apakan. Jika setiap controller dalam empat modul menguatkuasakan peraturan ini sendiri, ia akan dilaksanakan dengan **empat cara sedikit berbeza** dan tiga daripadanya akan mempunyai jurang.

`IWorkflowService` menempatkan peraturan pada **satu** tempat, dan menulis audit log secara atomik dengan perubahan status — supaya mustahil mempunyai perubahan status tanpa jejak audit.

### `SubmissionControllerBase`

Corak yang sama, satu peringkat lebih tinggi. Kelulusan, penolakan (dengan sebab wajib), dan penghantaran adalah **sama** merentas keempat-empat modul. Ia ditulis sekali dalam kelas asas; controller modul mewarisinya.

```csharp
public class AccessPassController(ApplicationDbContext db, IWorkflowService workflow)
    : SubmissionControllerBase(db, workflow)
{
    // Approve/Reject/Submit sudah wujud dan sudah betul.
    // Tulis hanya apa yang khusus kepada pas keselamatan.
}
```

### Partial view kongsi

Enam komponen UI yang setiap modul perlukan, dibina sekali:

`_StatusBadge` · `_AuditTrail` · `_AttachmentList` · `_ApprovalPanel` · `_FilterBar` · `_ValidationSummary`

**Kenapa ini penting melebihi menjimatkan menaip:** ia bermakna lencana status kelihatan sama dalam keempat-empat modul, panel audit berkelakuan sama, dan sebab penolakan wajib dikuatkuasakan secara seragam. Sistem yang terasa seperti **satu** sistem, bukan empat yang dijahit bersama.

---

## SESI 12 — Seni Bina Anti-Konflik & Buka Cabang

### Tiga corak, satu prinsip

Semua yang kita bina pada waktu petang ini mengikut satu idea: **lanjutan bermaksud menambah fail, bukan mengedit fail.**

| Fail yang biasanya berkonflik | Corak kita | Setiap kumpulan… |
|-------------------------------|------------|-------------------|
| `Program.cs` | Modul mendaftar diri melalui `Add<Modul>Module()` | menambah fail modulnya sendiri |
| `ApplicationDbContext` | `IEntityTypeConfiguration<T>` + `ApplyConfigurationsFromAssembly()` | menambah fail konfigurasi sendiri |
| `_Layout.cshtml` | Navigasi didorong `ModuleDescriptor` | menambah descriptor sendiri |

Selepas hari ini, ketiga-tiga fail itu **beku**. Jika anda mendapati diri anda perlu mengeditnya, itu isyarat: baca [`../KOLABORASI.md`](../KOLABORASI.md) §4 dan buka isu `shared`.

### Navigasi didorong data

```csharp
public record ModuleDescriptor(
    string Code, string Nama, string Controller,
    string Ikon, string[] Roles, int Urutan);
```

Setiap kumpulan menyediakan satu `IModuleDescriptorProvider`. Satu view component mengumpul kesemuanya, menapis mengikut peranan pengguna semasa, dan menjana menu. Tambah modul → tambah satu fail → ia muncul dalam navigasi. Tiada siapa menyentuh layout.

Ini juga apa yang menjadikan **Papan Pemuka Induk** Hari 15 hampir percuma: ia sudah tahu tentang keempat-empat modul.

### Buka cabang

Dengan asas kongsi digabung ke `master`, setiap kumpulan bercabang:

```bash
git switch master
git pull --rebase origin master
git switch -c kump-2/akses-kenderaan
git push -u origin kump-2/akses-kenderaan
```

**Bermula esok:** setiap pagi bermula dengan `git pull --rebase origin master`. `master` akan bergerak — gabungan latihan berlaku di hujung setiap blok — dan kekal segerak setiap hari ialah perbezaan antara gabungan Hari 15 yang mudah dan yang menyakitkan.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md) — anda akan:

0. **Pemanasan:** jalankan C# tanpa projek (`dotnet run pemanasan.cs`) — ulangkaji LINQ, `async`, dan cuba ciri C# 14
1. Cipta `Nres.Onboarding.Web` dan sahkan ia berjalan
2. Tambah pakej EF Core + Identity + SQLite
3. Tulis entiti kongsi dengan `IEntityTypeConfiguration<T>`
4. Tulis `ApplicationDbContext` dengan `ApplyConfigurationsFromAssembly()`
5. Konfigurasi Identity + seed 6 peranan & pengguna demo
6. Bina enam servis kongsi + `SubmissionControllerBase` + partial view kongsi
7. Bina corak modul mendaftar diri & navigasi didorong `ModuleDescriptor`
8. Jana migration `InitialShared`, gabung ke `master`, buka 4 cabang kumpulan

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
