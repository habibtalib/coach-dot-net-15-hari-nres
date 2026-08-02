# Ujian dengan xUnit ✅

> Nota konsep untuk **Hari 15** (integrasi & ujian) — walaupun konsep asas boleh diperkenalkan lebih awal jika masa mengizinkan. Lihat [`03-corak-workflow.md`](./03-corak-workflow.md) dan [`04-validation-viewmodels.md`](./04-validation-viewmodels.md) untuk logik yang akan diuji.

---

## Kenapa ujian automasi penting untuk sistem NRES?

Sistem `Nres.Onboarding.Web` mengendalikan **peraturan perniagaan kritikal** — nombor rujukan unik, peralihan status yang tidak boleh dilangkau, semakan pendua kenderaan. Menguji ini secara manual setiap kali kod berubah memakan masa & mudah tersilap. **Ujian automasi** memastikan peraturan ini kekal betul walaupun kod berkembang merentas 15 hari.

---

## xUnit — asas

**xUnit** ialah rangka kerja ujian paling popular untuk .NET. Struktur asas:

```bash
dotnet new xunit -n Nres.Onboarding.Tests
dotnet add Nres.Onboarding.Tests reference Nres.Onboarding.Web
```

```csharp
public class ReferenceNumberServiceTests
{
    [Fact]
    public void GenerateReferenceNumber_ForLaporDiri_UsesLdPrefix()
    {
        // Arrange
        var service = new ReferenceNumberService();

        // Act
        var refNumber = service.Generate("OfficerReporting", year: 2026, sequence: 1);

        // Assert
        Assert.Equal("LD-2026-0001", refNumber);
    }

    [Theory]
    [InlineData("OfficerReporting", "LD")]
    [InlineData("AccessPass", "PAS")]
    [InlineData("Parking", "PKR")]
    [InlineData("VehicleSticker", "STK")]
    public void GenerateReferenceNumber_UsesCorrectPrefixPerModule(string moduleType, string expectedPrefix)
    {
        var service = new ReferenceNumberService();
        var refNumber = service.Generate(moduleType, 2026, 1);
        Assert.StartsWith(expectedPrefix, refNumber);
    }
}
```

| Atribut | Fungsi |
|---------|--------|
| `[Fact]` | Satu ujian tunggal, tiada parameter |
| `[Theory]` + `[InlineData]` | Satu ujian dijalankan berulang dengan set data berbeza |
| `Assert.Equal`, `Assert.True`, `Assert.Throws` | Kaedah pengesahan hasil |

Jalankan semua ujian:

```bash
dotnet test
```

---

## Unit test vs Integration test

| | Unit test | Integration test |
|---|-----------|-------------------|
| **Skop** | Satu kelas/kaedah terpencil (logik tulen) | Beberapa komponen bersama (Controller + DbContext + Service) |
| **Pangkalan data** | Tiada — guna objek palsu (*mock/fake*) atau tiada langsung | EF Core in-memory atau SQLite sebenar |
| **Kelajuan** | Sangat pantas | Lebih perlahan (tapi masih automatik) |
| **Contoh dalam kursus** | `ReferenceNumberService`, peraturan validation `IValidatableObject` | `WorkflowService.ApproveAsync()` bersama `DbContext` sebenar |

---

## EF Core dalam ujian — SQLite in-memory

Untuk *integration test* yang melibatkan `DbContext`, guna SQLite **in-memory** (bukan fail fizikal) — pantas dan setiap ujian dapat pangkalan data bersih:

```csharp
public class WorkflowServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly ApplicationDbContext _context;

    public WorkflowServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(_connection)
            .Options;

        _context = new ApplicationDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task ApproveAsync_MovesStatusFromSubmittedToSupervisorApproved()
    {
        // Arrange
        var submission = new Submission { ModuleType = "OfficerReporting", Status = SubmissionStatus.Submitted };
        _context.Submissions.Add(submission);
        await _context.SaveChangesAsync();

        var service = new WorkflowService(_context, new FakeAuditLogService());

        // Act
        await service.ApproveAsync(submission.Id, "supervisor-user-id", "SupervisorReview");

        // Assert
        var updated = await _context.Submissions.FindAsync(submission.Id);
        Assert.Equal(SubmissionStatus.SupervisorApproved, updated!.Status);
    }

    public void Dispose() => _connection.Dispose();
}
```

> **Kenapa SQLite in-memory, bukan EF Core `UseInMemoryDatabase`?** *Provider* `InMemory` EF Core **tidak** menguatkuasakan kekangan sebenar (*constraints*, *foreign key*, jenis data) — ia boleh menyembunyikan bug. SQLite in-memory lebih hampir dengan kelakuan pangkalan data sebenar sambil kekal pantas untuk ujian.

---

## Apa yang MESTI diuji dalam sistem NRES

| Keperluan | Sebab wajib diuji |
|-----------|---------------------|
| **Jana nombor rujukan** (`LD-2026-0001`, dsb.) | Prefix salah = rekod tersalah kategori merentas 9 jenis modul |
| **Peralihan status** (`Draft`→`Submitted`→...) | Melangkau status (cth. `Draft`→`Completed` terus) ialah pelanggaran peraturan perniagaan kritikal |
| **Semakan pendua** (*duplicate check*, cth. kenderaan/pelekat) | Kegagalan boleh benarkan pendaftaran pendua yang tidak sah |
| **Ketersediaan aset** (*asset availability*, modul Aset ICT) | Elak pinjaman aset yang sudah dipinjam orang lain |
| **Ulasan penolakan wajib** (*required rejection remarks*) | Peraturan perniagaan: tidak boleh tolak permohonan tanpa sebab bertulis |

### Contoh ujian: ulasan penolakan wajib

```csharp
[Fact]
public async Task RejectAsync_WithoutRemarks_ThrowsValidationException()
{
    var service = new WorkflowService(_context, new FakeAuditLogService());
    var submission = await CreateSubmittedSubmissionAsync();

    await Assert.ThrowsAsync<ValidationException>(
        () => service.RejectAsync(submission.Id, "admin-user-id", "AdminReview", remarks: ""));
}
```

### Contoh ujian: semakan pendua kenderaan

```csharp
[Fact]
public async Task IsDuplicateVehicle_SamePlateNumberAlreadyRegistered_ReturnsTrue()
{
    _context.Vehicles.Add(new Vehicle { PlateNumber = "WXY1234" });
    await _context.SaveChangesAsync();

    var duplicateCheckService = new VehicleDuplicateCheckService(_context);
    var isDuplicate = await duplicateCheckService.IsDuplicateAsync("WXY1234");

    Assert.True(isDuplicate);
}
```

---

## Kaitan dengan hari-hari lain

- **Blok Hari 13–14** — setiap kumpulan menulis ujian modulnya dalam `Nres.Onboarding.Tests`.
- **Hari 15** — semua ujian dijalankan bersama selepas gabungan.
- Peraturan yang diuji berasal daripada konsep yang dipelajari di [`03-corak-workflow.md`](./03-corak-workflow.md) (status), [`04-validation-viewmodels.md`](./04-validation-viewmodels.md) (validation), dan modul individu (trek kumpulan, Hari 4–14).

---

## Sumber Rasmi

- **[xUnit official docs](https://xunit.net/)**
- **[Unit testing C# in .NET Core using dotnet test](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-dotnet-test)**
- **[Testing EF Core applications](https://learn.microsoft.com/en-us/ef/core/testing/)**
- **[Integration tests in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests)**
