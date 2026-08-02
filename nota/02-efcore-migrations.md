# EF Core & Migrations

> Nota konsep untuk **Hari 3** (migration pertama, `InitialShared`) dan diulang sepanjang Fasa 2 setiap kali kumpulan menambah entiti. Lihat [`01-kenapa-aspnet-mvc.md`](./01-kenapa-aspnet-mvc.md) untuk konteks Model dalam MVC.

---

## Apa itu Entity Framework Core?

**EF Core** ialah *Object-Relational Mapper (ORM)* — ia membenarkan anda bekerja dengan pangkalan data menggunakan **kelas C# biasa** (entiti) dan pertanyaan **LINQ**, tanpa menulis SQL secara manual untuk kebanyakan operasi. Kod C# yang anda tulis "menjana" struktur pangkalan data — pendekatan ini dipanggil **code-first**.

```text
Kelas C# (entiti)  →  DbContext  →  EF Core  →  Migration  →  Pangkalan Data
Submission.cs          ApplicationDbContext      dotnet ef      SQLite (.db)
```

---

## Komponen utama

### 1. Entiti (Model)

Kelas C# biasa yang mewakili satu jadual (*table*):

```csharp
public class Submission
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public string ModuleType { get; set; } = string.Empty;
    public string ApplicantUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
```

> Untuk makna `SubmissionStatus` dan kenapa `Submission` menjadi induk kongsi merentas 4 modul, lihat [`03-corak-workflow.md`](./03-corak-workflow.md).

### 2. `DbContext`

"Jambatan" antara kod C# dan pangkalan data — mendedahkan setiap jadual sebagai `DbSet<T>`:

```csharp
public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options) { }

    public DbSet<Submission> Submissions => Set<Submission>();
    public DbSet<Attachment> Attachments => Set<Attachment>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();
    public DbSet<ApprovalStep> ApprovalSteps => Set<ApprovalStep>();
}
```

> `IdentityDbContext<ApplicationUser>` digunakan (bukan `DbContext` biasa) kerana projek turut menggunakan ASP.NET Core Identity — lihat [`05-identity-authorization.md`](./05-identity-authorization.md).

### 3. Daftar `DbContext` dalam `Program.cs`

```csharp
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
```

`appsettings.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=App_Data/nres_onboarding.db"
  }
}
```

---

## Migrations — code-first, langkah demi langkah

**Migration** ialah rekod perubahan struktur pangkalan data (jadual, kolum) yang dijana secara automatik daripada perubahan pada entiti C#.

### Cipta migration pertama

```bash
dotnet ef migrations add InitialCreate
```

Ini menjana folder `Migrations/` dengan fail `..._InitialCreate.cs` (kandungan `Up()`/`Down()` — arahan cipta/*rollback* jadual) berdasarkan entiti & `DbSet` semasa.

### Kemas kini pangkalan data sebenar

```bash
dotnet ef database update
```

Ini menjalankan migration terhadap fail SQLite (`nres_onboarding.db`), mencipta jadual sebenar.

### Bila entiti berubah (contoh: Kumpulan 2 menambah `Vehicle` pada Hari 4)

```bash
dotnet ef migrations add AddVehicleEntities
dotnet ef database update
```

> **Corak berulang sepanjang kursus:** setiap kali entiti baharu ditambah, langkah sama diulang — tambah entiti + `IEntityTypeConfiguration<T>` → `migrations add` → `database update`. Fahami sekali di Hari 3, gunakan berulang kali.
>
> ⚠️ **Dalam Fasa 2, migration mengikut SLOT bergilir** — hanya satu kumpulan menjana migration pada satu masa. Rujuk [`../KOLABORASI.md`](../KOLABORASI.md) §5.

---

## Kenapa SQLite untuk latihan?

| Sebab | Penjelasan |
|-------|-----------|
| **Sifar pemasangan** | Tiada perlu pasang server pangkalan data berasingan — fail `.db` tunggal |
| **Mula pantas** | Peserta boleh terus fokus pada C#/EF Core tanpa isu konfigurasi rangkaian |
| **Mudah alih** | Fail `.db` boleh dipadam & dijana semula bila-bila untuk *reset* semasa latihan |

### Menukar provider ke SQL Server (pengeluaran)

Ditunjukkan penuh pada **Hari 15** — lihat [`08-deployment.md`](./08-deployment.md). Ringkasnya, hanya tukar satu baris:

```csharp
// Latihan (SQLite)
options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection"));

// Pengeluaran (SQL Server)
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
```

Pasang pakej NuGet berkenaan (`Microsoft.EntityFrameworkCore.SqlServer`), tukar *connection string* format SQL Server, dan jalankan semula `dotnet ef database update` — struktur migration yang sudah ditulis **tidak perlu ditulis semula**.

---

## Arahan CLI penting (rujukan pantas)

| Arahan | Fungsi |
|--------|--------|
| `dotnet ef migrations add <Nama>` | Cipta migration baharu daripada perubahan entiti semasa |
| `dotnet ef database update` | Terapkan migration tertunggak ke pangkalan data |
| `dotnet ef migrations remove` | Buang migration terakhir (jika belum di-*update*) |
| `dotnet ef database update <MigrationSebelum>` | *Rollback* ke migration tertentu |
| `dotnet ef migrations list` | Senarai semua migration projek |

---

## Kaitan dengan hari-hari lain

- **Hari 1** — `InitialCreate` migration untuk `Submission`, `Attachment`, `AuditLog`.
- **Hari 4 dan seterusnya** — migration tambahan oleh setiap kumpulan, melalui slot bergilir.
- **Hari 15** — tukar provider SQLite → SQL Server semasa deployment, lihat [`08-deployment.md`](./08-deployment.md).
- Lihat juga [`07-testing-xunit.md`](./07-testing-xunit.md) untuk cara guna EF Core in-memory/SQLite dalam ujian automasi.

---

## Sumber Rasmi

- **[EF Core overview](https://learn.microsoft.com/en-us/ef/core/)**
- **[Migrations overview](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/)**
- **[dotnet ef CLI reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)**
- **[Database providers](https://learn.microsoft.com/en-us/ef/core/providers/)**
