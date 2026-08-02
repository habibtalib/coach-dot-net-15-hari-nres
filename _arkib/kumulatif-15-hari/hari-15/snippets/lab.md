# Lab Hari 15 — Integrasi, Ujian & Deployment

Lab ini mengiringi [`../README.md`](../README.md) Hari 15 dan **menyambungkan** kelima-lima modul yang sudah anda bina Hari 1–14. Ikut latihan secara berurutan. Rujuk [`../../projek/`](../../projek/) untuk banding selepas cuba sendiri.

> **Skop:** Latihan 1–3 (integrasi) andaikan controller Modul 1–4 anda ikut corak yang sama seperti Modul 5 (Hari 14) — `Index`/`Create`/`Details` + `[Authorize(Roles=...)]` pada action kelulusan. Jika nama action anda berbeza sedikit, laraskan rujukan di bawah mengikut kod sebenar anda.

---

## Senarai Semak Sebelum Mula

- [ ] Modul 1–5 (Hari 1–14) selesai & `dotnet run` berjaya tanpa ralat
- [ ] Roles Identity (`Applicant`, `Supervisor`, `HrAdmin`, `SecurityAdmin`, `IctAdmin`, `ComplianceAdmin`, `SystemAdmin`) sudah di-seed
- [ ] Sekurang-kurangnya satu akaun ujian bagi setiap role

---

# BAHAGIAN A — Integrasi (SESI 44)

## Latihan 1 — Navigasi Kongsi & Menu Ikut Peranan

**Objektif:** Satu `_Layout.cshtml` yang papar pautan **berbeza** ikut peranan pengguna log masuk.

Buka `Views/Shared/_Layout.cshtml` (fail sedia ada sejak Hari 1). Ganti bahagian `<nav>` dengan:

```cshtml
<nav class="navbar navbar-expand-lg navbar-light bg-white border-bottom mb-3">
    <div class="container-fluid">
        <a class="navbar-brand" asp-controller="Dashboard" asp-action="Index">Nres.Onboarding</a>
        <div class="navbar-collapse">
            <ul class="navbar-nav me-auto">
                @if (User.Identity?.IsAuthenticated == true)
                {
                    <li class="nav-item"><a class="nav-link" asp-controller="Dashboard" asp-action="Index">Dashboard Saya</a></li>
                    <li class="nav-item"><a class="nav-link" asp-controller="Search" asp-action="Index">Carian Rujukan</a></li>

                    @* Modul 1 — Lapor Diri: semua pemohon boleh mohon *@
                    <li class="nav-item"><a class="nav-link" asp-controller="OfficerReportingApplications" asp-action="Create">Lapor Diri</a></li>

                    @* Modul 2 — Pas/Parking/Pelekat *@
                    <li class="nav-item"><a class="nav-link" asp-controller="AccessPassApplications" asp-action="Create">Pas/Parking/Pelekat</a></li>

                    @* Modul 3 — ID/AD/Email *@
                    <li class="nav-item"><a class="nav-link" asp-controller="AccountRequests" asp-action="Create">ID AD &amp; Email</a></li>

                    @* Modul 4 — PKS *@
                    <li class="nav-item"><a class="nav-link" asp-controller="ComplianceDeclarations" asp-action="Create">Pengisytiharan PKS</a></li>

                    @* Modul 5 — Aset ICT *@
                    <li class="nav-item dropdown">
                        <a class="nav-link dropdown-toggle" href="#" role="button" data-bs-toggle="dropdown">Aset ICT</a>
                        <ul class="dropdown-menu">
                            <li><a class="dropdown-item" asp-controller="SoftwareRequests" asp-action="Create">Permohonan Perisian</a></li>
                            <li><a class="dropdown-item" asp-controller="AssetLoanRequests" asp-action="Create">Pinjaman Aset</a></li>
                            <li><a class="dropdown-item" asp-controller="AssetReturns" asp-action="Create">Pemulangan Aset</a></li>
                        </ul>
                    </li>

                    @* Menu ADMIN — hanya papar kepada role berkaitan. Ini lapisan UX sahaja;
                       [Authorize(Roles=...)] pada controller kekal pertahanan SEBENAR. *@
                    @if (User.IsInRole("HrAdmin"))
                    {
                        <li class="nav-item"><a class="nav-link" asp-controller="OfficerReportingApplications" asp-action="Index">Semakan Lapor Diri</a></li>
                    }
                    @if (User.IsInRole("SecurityAdmin"))
                    {
                        <li class="nav-item"><a class="nav-link" asp-controller="AccessPassApplications" asp-action="Index">Semakan Pas/Parking/Pelekat</a></li>
                    }
                    @if (User.IsInRole("Supervisor") || User.IsInRole("IctAdmin"))
                    {
                        <li class="nav-item"><a class="nav-link" asp-controller="AccountRequests" asp-action="Index">Semakan ID/AD/Email</a></li>
                    }
                    @if (User.IsInRole("ComplianceAdmin"))
                    {
                        <li class="nav-item"><a class="nav-link" asp-controller="ComplianceDeclarations" asp-action="Index">Semakan PKS</a></li>
                    }
                    @if (User.IsInRole("IctAdmin"))
                    {
                        <li class="nav-item"><a class="nav-link" asp-controller="AssetLoanRequests" asp-action="Index">Semakan Aset ICT</a></li>
                    }
                }
            </ul>
        </div>
    </div>
</nav>
```

**Prinsip keselamatan (ulang sejak Hari 8):** menu ini **hanya** menyembunyikan pautan — ia **tidak** menggantikan `[Authorize(Roles = "...")]` yang mesti kekal pada setiap action admin. Cuba log masuk sebagai `Applicant`, salin URL `/AssetLoanRequests/Index` terus ke bar alamat pelayar — sahkan anda ditolak (403/redirect ke akses ditolak), walaupun pautan tiada dalam menu.

✅ **Semakan:** Log masuk sebagai peranan berbeza (`Applicant`, `IctAdmin`, `HrAdmin`) — sahkan menu berubah ikut peranan setiap kali.

---

## Latihan 2 — Dashboard Bersepadu

**Objektif:** Satu skrin memapar draf saya / dihantar / menunggu kelulusan saya / selesai — merentasi **kelima-lima** modul, hanya dengan query terhadap `Submissions`.

Cipta `Services/ModuleRoleMap.cs` — peta `ModuleCode → Role` (rujuk jadual dalam README):

```csharp
namespace Nres.Onboarding.Web.Services;

/// <summary>
/// Peta ModuleCode Submission kepada Role yang bertanggungjawab meluluskannya.
/// Satu sumber kebenaran untuk penapisan "menunggu kelulusan saya" pada dashboard.
/// </summary>
public static class ModuleRoleMap
{
    private static readonly Dictionary<string, string[]> RolesByModule = new()
    {
        ["LD"] = new[] { "HrAdmin" },
        ["PAS"] = new[] { "SecurityAdmin" },
        ["PKR"] = new[] { "SecurityAdmin" },
        ["STK"] = new[] { "SecurityAdmin" },
        ["ICT-ID"] = new[] { "Supervisor", "IctAdmin" },
        ["PKS"] = new[] { "ComplianceAdmin" },
        ["SW"] = new[] { "IctAdmin" },
        ["AST-L"] = new[] { "IctAdmin" },
        ["AST-R"] = new[] { "IctAdmin" }
    };

    public static string[] ModuleCodesForRole(string role)
    {
        return RolesByModule
            .Where(kvp => kvp.Value.Contains(role))
            .Select(kvp => kvp.Key)
            .ToArray();
    }
}
```

Cipta `ViewModels/DashboardViewModel.cs`:

```csharp
using Nres.Onboarding.Web.Models;

namespace Nres.Onboarding.Web.ViewModels;

public class DashboardViewModel
{
    public List<Submission> MyDrafts { get; set; } = new();
    public List<Submission> MySubmitted { get; set; } = new();
    public List<Submission> PendingMyApproval { get; set; } = new();
    public List<Submission> MyCompleted { get; set; } = new();
}
```

Cipta `Controllers/DashboardController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;
using Nres.Onboarding.Web.Services;
using Nres.Onboarding.Web.ViewModels;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly ApplicationDbContext _db;
    private readonly ICurrentUserService _currentUserService;

    public DashboardController(ApplicationDbContext db, ICurrentUserService currentUserService)
    {
        _db = db;
        _currentUserService = currentUserService;
    }

    public async Task<IActionResult> Index()
    {
        var userId = _currentUserService.UserId;

        var model = new DashboardViewModel
        {
            MyDrafts = await _db.Submissions
                .Where(s => s.ApplicantUserId == userId && s.Status == SubmissionStatus.Draft)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(),

            MySubmitted = await _db.Submissions
                .Where(s => s.ApplicantUserId == userId && s.Status == SubmissionStatus.Submitted)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync(),

            MyCompleted = await _db.Submissions
                .Where(s => s.ApplicantUserId == userId && s.Status == SubmissionStatus.Completed)
                .OrderByDescending(s => s.CompletedAt)
                .ToListAsync()
        };

        // "Menunggu kelulusan saya" — gabungkan SEMUA role pengguna semasa.
        var moduleCodes = _currentUserService.Roles
            .SelectMany(ModuleRoleMap.ModuleCodesForRole)
            .Distinct()
            .ToArray();

        if (moduleCodes.Length > 0)
        {
            model.PendingMyApproval = await _db.Submissions
                .Where(s => moduleCodes.Contains(s.ModuleCode)
                         && (s.Status == SubmissionStatus.Submitted || s.Status == SubmissionStatus.SupervisorApproved))
                .OrderBy(s => s.SubmittedAt)
                .ToListAsync();
        }

        return View(model);
    }
}
```

> **Nota:** `ICurrentUserService.Roles` diandaikan mengembalikan `IReadOnlyList<string>` peranan pengguna semasa (perluasan kecil kepada servis kongsi sedia ada sejak Hari 1 — tambah property ini jika belum wujud).

Cipta `Views/Dashboard/Index.cshtml`:

```cshtml
@model Nres.Onboarding.Web.ViewModels.DashboardViewModel
@{
    ViewData["Title"] = "Dashboard Saya";
}

<h1>Dashboard Saya</h1>

<div class="row">
    <div class="col-md-3">
        <h4>Draf Saya (@Model.MyDrafts.Count)</h4>
        <ul class="list-group">
            @foreach (var s in Model.MyDrafts)
            {
                <li class="list-group-item">@s.ModuleCode — <em>belum dihantar</em></li>
            }
        </ul>
    </div>
    <div class="col-md-3">
        <h4>Dihantar (@Model.MySubmitted.Count)</h4>
        <ul class="list-group">
            @foreach (var s in Model.MySubmitted)
            {
                <li class="list-group-item">@s.ReferenceNo</li>
            }
        </ul>
    </div>
    <div class="col-md-3">
        <h4>Menunggu Kelulusan Saya (@Model.PendingMyApproval.Count)</h4>
        <ul class="list-group">
            @foreach (var s in Model.PendingMyApproval)
            {
                <li class="list-group-item">@s.ReferenceNo (@s.ModuleCode)</li>
            }
        </ul>
    </div>
    <div class="col-md-3">
        <h4>Selesai (@Model.MyCompleted.Count)</h4>
        <ul class="list-group">
            @foreach (var s in Model.MyCompleted)
            {
                <li class="list-group-item">@s.ReferenceNo</li>
            }
        </ul>
    </div>
</div>
```

✅ **Semakan:** Log masuk sebagai `Applicant` yang ada beberapa permohonan pelbagai status — sahkan setiap lajur papar bilangan & rekod yang betul. Log masuk sebagai `IctAdmin` — sahkan lajur "Menunggu Kelulusan Saya" **hanya** papar permohonan `SW`/`AST-L`/`AST-R`, bukan modul lain.

---

## Latihan 3 — Carian Rujukan Global

**Objektif:** Satu medan carian, cari merentasi semua 9 jadual modul melalui `Submission.ReferenceNo`.

Cipta `Controllers/SearchController.cs`:

```csharp
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;

namespace Nres.Onboarding.Web.Controllers;

[Authorize]
public class SearchController : Controller
{
    private readonly ApplicationDbContext _db;

    public SearchController(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task<IActionResult> Index(string? q)
    {
        List<Submission> results = new();

        if (!string.IsNullOrWhiteSpace(q))
        {
            results = await _db.Submissions
                .Where(s => s.ReferenceNo.Contains(q))
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        ViewData["Query"] = q;
        return View(results);
    }
}
```

Cipta `Views/Search/Index.cshtml`:

```cshtml
@model List<Nres.Onboarding.Web.Models.Submission>
@{
    ViewData["Title"] = "Carian Rujukan";
}

<h1>Carian Rujukan</h1>

<form method="get" asp-action="Index" class="mb-3">
    <div class="input-group">
        <input type="text" name="q" class="form-control" placeholder="Cth: AST-L-2026-0001" value="@ViewData["Query"]" />
        <button type="submit" class="btn btn-primary">Cari</button>
    </div>
</form>

@if (ViewData["Query"] != null)
{
    <p>@Model.Count keputusan untuk "@ViewData["Query"]"</p>
    <table class="table">
        <thead><tr><th>Rujukan</th><th>Modul</th><th>Status</th><th>Tarikh</th></tr></thead>
        <tbody>
            @foreach (var s in Model)
            {
                <tr>
                    <td>@s.ReferenceNo</td>
                    <td>@s.ModuleCode</td>
                    <td>@s.Status</td>
                    <td>@s.CreatedAt.ToString("dd/MM/yyyy")</td>
                </tr>
            }
        </tbody>
    </table>
}
```

✅ **Semakan:** Carian `"AST-L"` memulangkan semua permohonan pinjaman aset merentasi semua pemohon; carian nombor penuh (cth. `"AST-L-2026-0001"`) memulangkan tepat satu rekod.

---

# BAHAGIAN B — Ujian xUnit (SESI 45)

## Latihan 4 — Cipta Projek `Nres.Onboarding.Tests`

```bash
cd ..
dotnet new xunit -n Nres.Onboarding.Tests
cd Nres.Onboarding.Tests
dotnet add reference ../Nres.Onboarding.Web/Nres.Onboarding.Web.csproj
dotnet add package Microsoft.AspNetCore.Mvc.Testing
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
```

Jika anda ada fail `.sln`, daftarkan projek ujian:

```bash
cd ..
dotnet sln add Nres.Onboarding.Tests/Nres.Onboarding.Tests.csproj
```

✅ **Semakan:** `dotnet test` (dijalankan dari root solution) berjaya, memulangkan "Passed! - Failed: 0" untuk ujian templat lalai (`UnitTest1`).

---

## Latihan 5 — Recap: Servis Kongsi Yang Diuji Hari Ini

> Servis berikut **sudah wujud** sejak Hari 1/3 (`IReferenceNumberService`, `IWorkflowService`). Ditunjukkan semula di sini **sekadar konteks** supaya kod ujian di bawah boleh dirujuk terus — **jangan** tulis semula fail ini jika sudah wujud dalam projek anda.

`Services/IReferenceNumberService.cs` & implementasi:

```csharp
namespace Nres.Onboarding.Web.Services;

public interface IReferenceNumberService
{
    Task<string> GenerateAsync(string moduleCode);
}
```

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;

namespace Nres.Onboarding.Web.Services;

public class ReferenceNumberService : IReferenceNumberService
{
    private readonly ApplicationDbContext _db;

    public ReferenceNumberService(ApplicationDbContext db) => _db = db;

    public async Task<string> GenerateAsync(string moduleCode)
    {
        var year = DateTime.UtcNow.Year;
        var prefix = $"{moduleCode}-{year}-";

        var countThisYear = await _db.Submissions
            .Where(s => s.ModuleCode == moduleCode && s.ReferenceNo.StartsWith(prefix))
            .CountAsync();

        var sequence = countThisYear + 1;
        return $"{prefix}{sequence:D4}";
    }
}
```

`Services/IWorkflowService.cs` & implementasi:

```csharp
namespace Nres.Onboarding.Web.Services;

public interface IWorkflowService
{
    bool CanTransition(SubmissionStatus from, SubmissionStatus to);
}
```

```csharp
using Nres.Onboarding.Web.Models;

namespace Nres.Onboarding.Web.Services;

public class WorkflowService : IWorkflowService
{
    private static readonly Dictionary<SubmissionStatus, SubmissionStatus[]> AllowedTransitions = new()
    {
        [SubmissionStatus.Draft] = new[] { SubmissionStatus.Submitted, SubmissionStatus.Cancelled },
        [SubmissionStatus.Submitted] = new[]
        {
            SubmissionStatus.SupervisorApproved, SubmissionStatus.AdminApproved,
            SubmissionStatus.Completed, SubmissionStatus.Rejected, SubmissionStatus.Cancelled
        },
        [SubmissionStatus.SupervisorApproved] = new[] { SubmissionStatus.AdminApproved, SubmissionStatus.Completed, SubmissionStatus.Rejected },
        [SubmissionStatus.AdminApproved] = new[] { SubmissionStatus.Completed, SubmissionStatus.Rejected },
        [SubmissionStatus.Rejected] = Array.Empty<SubmissionStatus>(),
        [SubmissionStatus.Completed] = Array.Empty<SubmissionStatus>(),
        [SubmissionStatus.Cancelled] = Array.Empty<SubmissionStatus>()
    };

    public bool CanTransition(SubmissionStatus from, SubmissionStatus to) =>
        AllowedTransitions.TryGetValue(from, out var allowed) && allowed.Contains(to);
}
```

---

## Latihan 6 — Unit Tests

**Objektif:** Uji **peraturan**, bukan HTTP/UI — pantas, tiada pelayan diperlukan.

Cipta fail bantuan `Nres.Onboarding.Tests/TestDbContextFactory.cs`:

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Data;

namespace Nres.Onboarding.Tests;

/// <summary>
/// Setiap panggilan Create() memulangkan ApplicationDbContext SQLite in-memory
/// yang BAHARU & KOSONG — ujian tidak berkongsi keadaan antara satu sama lain.
/// </summary>
public static class TestDbContextFactory
{
    public static ApplicationDbContext Create()
    {
        var connection = new Microsoft.Data.Sqlite.SqliteConnection("DataSource=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseSqlite(connection)
            .Options;

        var db = new ApplicationDbContext(options);
        db.Database.EnsureCreated();
        return db;
    }
}
```

Cipta `Nres.Onboarding.Tests/ReferenceNumberServiceTests.cs`:

```csharp
using Nres.Onboarding.Web.Models;
using Nres.Onboarding.Web.Services;
using Xunit;

namespace Nres.Onboarding.Tests;

public class ReferenceNumberServiceTests
{
    [Theory]
    [InlineData("SW")]
    [InlineData("AST-L")]
    [InlineData("AST-R")]
    public async Task GenerateAsync_MenghasilkanFormatBetul_UntukPrefixBaharu(string moduleCode)
    {
        using var db = TestDbContextFactory.Create();
        var service = new ReferenceNumberService(db);

        var referenceNo = await service.GenerateAsync(moduleCode);

        var year = DateTime.UtcNow.Year;
        Assert.Equal($"{moduleCode}-{year}-0001", referenceNo);
    }

    [Fact]
    public async Task GenerateAsync_NaikkanNomborSiri_UntukPrefixSamaModuleCodeSama()
    {
        using var db = TestDbContextFactory.Create();
        var service = new ReferenceNumberService(db);
        var year = DateTime.UtcNow.Year;

        db.Submissions.Add(new Submission
        {
            ModuleCode = "AST-L",
            ReferenceNo = $"AST-L-{year}-0001",
            ApplicantUserId = "user-1",
            Status = SubmissionStatus.Submitted
        });
        await db.SaveChangesAsync();

        var second = await service.GenerateAsync("AST-L");

        Assert.Equal($"AST-L-{year}-0002", second);
    }
}
```

Cipta `Nres.Onboarding.Tests/WorkflowServiceTests.cs`:

```csharp
using Nres.Onboarding.Web.Models;
using Nres.Onboarding.Web.Services;
using Xunit;

namespace Nres.Onboarding.Tests;

public class WorkflowServiceTests
{
    private readonly WorkflowService _sut = new();

    [Theory]
    [InlineData(SubmissionStatus.Draft, SubmissionStatus.Submitted, true)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Completed, true)]
    [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Rejected, true)]
    [InlineData(SubmissionStatus.Completed, SubmissionStatus.Submitted, false)]
    [InlineData(SubmissionStatus.Rejected, SubmissionStatus.Completed, false)]
    [InlineData(SubmissionStatus.Draft, SubmissionStatus.Completed, false)]
    public void CanTransition_MemulangkanNilaiBetul(SubmissionStatus from, SubmissionStatus to, bool expected)
    {
        var result = _sut.CanTransition(from, to);
        Assert.Equal(expected, result);
    }
}
```

Cipta `Nres.Onboarding.Tests/AssetAvailabilityTests.cs` (uji semakan availability Hari 14):

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Models;
using Xunit;

namespace Nres.Onboarding.Tests;

public class AssetAvailabilityTests
{
    [Fact]
    public async Task SemakanAvailability_HanyaMemulangkanAsetKategoriBetulDanStatusAvailable()
    {
        using var db = TestDbContextFactory.Create();

        db.Assets.AddRange(
            new Asset { AssetTag = "A1", SerialNumber = "S1", Category = "Laptop", BrandModel = "X", Status = AssetStatus.Available, Condition = "Baik" },
            new Asset { AssetTag = "A2", SerialNumber = "S2", Category = "Laptop", BrandModel = "X", Status = AssetStatus.OnLoan, Condition = "Baik" },
            new Asset { AssetTag = "A3", SerialNumber = "S3", Category = "Monitor", BrandModel = "Y", Status = AssetStatus.Available, Condition = "Baik" }
        );
        await db.SaveChangesAsync();

        var availableLaptops = await db.Assets
            .Where(a => a.Category == "Laptop" && a.Status == AssetStatus.Available)
            .ToListAsync();

        Assert.Single(availableLaptops);
        Assert.Equal("A1", availableLaptops[0].AssetTag);
    }

    [Fact]
    public async Task SemakanAvailability_MemulangkanSenaraiKosong_BilaTiadaAsetAvailable()
    {
        using var db = TestDbContextFactory.Create();

        db.Assets.Add(new Asset
        {
            AssetTag = "P1",
            SerialNumber = "SP1",
            Category = "Printer",
            BrandModel = "Canon",
            Status = AssetStatus.UnderMaintenance,
            Condition = "Rosak"
        });
        await db.SaveChangesAsync();

        var availablePrinters = await db.Assets
            .Where(a => a.Category == "Printer" && a.Status == AssetStatus.Available)
            .ToListAsync();

        Assert.Empty(availablePrinters);
    }
}
```

Cipta `Nres.Onboarding.Tests/DuplicateStickerCheckTests.cs` (uji peraturan pendua Modul 2, Hari 5):

```csharp
using Microsoft.EntityFrameworkCore;
using Nres.Onboarding.Web.Models;
using Xunit;

namespace Nres.Onboarding.Tests;

public class DuplicateStickerCheckTests
{
    [Fact]
    public async Task AnyAsync_MengesanPermohonanPelekatAktifSediaAda()
    {
        using var db = TestDbContextFactory.Create();

        var submission = new Submission
        {
            ModuleCode = "STK",
            ApplicantUserId = "user-1",
            Status = SubmissionStatus.Submitted
        };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        db.VehicleStickerApplications.Add(new VehicleStickerApplication
        {
            SubmissionId = submission.Id,
            VehicleRegistrationNo = "ABC1234"
        });
        await db.SaveChangesAsync();

        var hasActiveApplication = await db.VehicleStickerApplications
            .Include(x => x.Submission)
            .AnyAsync(x =>
                x.VehicleRegistrationNo == "ABC1234" &&
                x.Submission.Status != SubmissionStatus.Rejected &&
                x.Submission.Status != SubmissionStatus.Cancelled &&
                x.Submission.Status != SubmissionStatus.Completed);

        Assert.True(hasActiveApplication);
    }

    [Fact]
    public async Task AnyAsync_TidakMengesanPermohonanYangSudahRejected()
    {
        using var db = TestDbContextFactory.Create();

        var submission = new Submission
        {
            ModuleCode = "STK",
            ApplicantUserId = "user-1",
            Status = SubmissionStatus.Rejected
        };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        db.VehicleStickerApplications.Add(new VehicleStickerApplication
        {
            SubmissionId = submission.Id,
            VehicleRegistrationNo = "XYZ9999"
        });
        await db.SaveChangesAsync();

        var hasActiveApplication = await db.VehicleStickerApplications
            .Include(x => x.Submission)
            .AnyAsync(x =>
                x.VehicleRegistrationNo == "XYZ9999" &&
                x.Submission.Status != SubmissionStatus.Rejected &&
                x.Submission.Status != SubmissionStatus.Cancelled &&
                x.Submission.Status != SubmissionStatus.Completed);

        Assert.False(hasActiveApplication);
    }
}
```

> **Nota:** Ujian ini rujuk `VehicleStickerApplication` daripada Modul 2 (Hari 4–5). Jika medan/nama kelas sebenar anda berbeza sedikit, laraskan mengikut kod anda — konsep ujian (bukan nama tepat) yang penting.

Cipta `Nres.Onboarding.Tests/RejectRequiresRemarksTests.cs` (uji DataAnnotations `RejectViewModel` Hari 14):

```csharp
using System.ComponentModel.DataAnnotations;
using Nres.Onboarding.Web.ViewModels;
using Xunit;

namespace Nres.Onboarding.Tests;

public class RejectRequiresRemarksTests
{
    [Fact]
    public void RejectViewModel_TidakSah_BilaRejectReasonKosong()
    {
        var model = new RejectViewModel { RejectReason = string.Empty };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.False(isValid);
        Assert.Contains(results, r => r.MemberNames.Contains(nameof(RejectViewModel.RejectReason)));
    }

    [Fact]
    public void RejectViewModel_Sah_BilaRejectReasonDiisi()
    {
        var model = new RejectViewModel { RejectReason = "Dokumen sokongan tidak lengkap." };
        var context = new ValidationContext(model);
        var results = new List<ValidationResult>();

        var isValid = Validator.TryValidateObject(model, context, results, validateAllProperties: true);

        Assert.True(isValid);
    }
}
```

Jalankan:

```bash
dotnet test
```

✅ **Semakan:** Semua ujian unit **Passed**. Cuba **sengaja** rosakkan `WorkflowService` (cth. buang satu transisi sah) dan jalankan semula `dotnet test` — sahkan ujian berkaitan **Failed**, membuktikan ujian anda benar-benar menguji sesuatu (bukan "selalu lulus").

---

## Latihan 7 — Sediakan Integration Test (`WebApplicationFactory`)

**Objektif:** Uji aplikasi **sebenar** (routing, controller, Identity, EF Core) hujung-ke-hujung, tanpa pelayar sebenar.

Buka `Program.cs` (projek `Nres.Onboarding.Web`). Tambah **satu baris** di **hujung sekali** fail (selepas `app.Run();`):

```csharp
// Membenarkan WebApplicationFactory<Program> daripada projek ujian
// mengakses kelas Program (top-level statements jana kelas 'internal' secara lalai).
public partial class Program { }
```

Cipta `Nres.Onboarding.Tests/CustomWebApplicationFactory.cs`:

```csharp
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.AspNetCore.TestHost;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nres.Onboarding.Web.Data;

namespace Nres.Onboarding.Tests;

public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    public SqliteConnection Connection { get; } = new("DataSource=:memory:");

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        Connection.Open();

        builder.ConfigureServices(services =>
        {
            // Buang pendaftaran ApplicationDbContext asal (SQLite fail sebenar)...
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<ApplicationDbContext>));
            if (descriptor is not null)
            {
                services.Remove(descriptor);
            }

            // ...gantikan dengan SQLite in-memory KHUSUS UJIAN.
            services.AddDbContext<ApplicationDbContext>(options =>
            {
                options.UseSqlite(Connection);
            });

            // Skim authentication palsu — benarkan ujian "log masuk" sebagai
            // mana-mana role tanpa borang log masuk Identity sebenar.
            services.AddAuthentication(TestAuthHandler.SchemeName)
                .AddScheme<AuthenticationSchemeOptions, TestAuthHandler>(
                    TestAuthHandler.SchemeName, options => { });

            var sp = services.BuildServiceProvider();
            using var scope = sp.CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            db.Database.EnsureCreated();
        });
    }
}
```

Cipta `Nres.Onboarding.Tests/TestAuthHandler.cs` (corak rasmi Microsoft untuk "mock authentication" dalam ujian integrasi):

```csharp
using System.Security.Claims;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Nres.Onboarding.Tests;

public class TestAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string SchemeName = "TestScheme";

    public TestAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        // Header ujian: X-Test-UserId dan X-Test-Roles (dipisah koma).
        var userId = Request.Headers["X-Test-UserId"].FirstOrDefault() ?? "test-applicant";
        var roles = (Request.Headers["X-Test-Roles"].FirstOrDefault() ?? "Applicant").Split(',');

        var claims = new List<Claim>
        {
            new(ClaimTypes.NameIdentifier, userId),
            new(ClaimTypes.Name, userId)
        };
        claims.AddRange(roles.Select(r => new Claim(ClaimTypes.Role, r)));

        var identity = new ClaimsIdentity(claims, SchemeName);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, SchemeName);

        return Task.FromResult(AuthenticateResult.Success(ticket));
    }
}
```

> Rujukan rasmi corak "mock authentication for testing": [learn.microsoft.com/aspnet/core/test/integration-tests](https://learn.microsoft.com/en-us/aspnet/core/test/integration-tests#mock-authentication)

✅ **Semakan:** `dotnet build` bersih untuk kedua-dua projek.

---

## Latihan 8 — Integration Tests

**Objektif:** Uji aliran **sebenar** — HTTP request, routing, EF Core, transaksi — bukan sekadar logik terpencil.

Cipta `Nres.Onboarding.Tests/AssetLoanIntegrationTests.cs` (paling penting — liputi keperluan "complete asset loan updates asset status"):

```csharp
using System.Net;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;
using Xunit;

namespace Nres.Onboarding.Tests;

public class AssetLoanIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AssetLoanIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(string userId, string roles)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions
        {
            AllowAutoRedirect = false
        });
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId);
        client.DefaultRequestHeaders.Add("X-Test-Roles", roles);
        return client;
    }

    private async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string url)
    {
        var response = await client.GetAsync(url);
        response.EnsureSuccessStatusCode();
        var html = await response.Content.ReadAsStringAsync();

        var marker = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        var end = html.IndexOf('"', start);
        return html[start..end];
    }

    [Fact]
    public async Task SelesaikanPinjamanAset_MengemasKiniStatusAsetDanSubmission()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var asset = new Asset
        {
            AssetTag = "TEST-AST-001",
            SerialNumber = "TEST-SN-001",
            Category = "Laptop",
            BrandModel = "Test Laptop",
            Status = AssetStatus.Available,
            Condition = "Baik"
        };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();

        var applicantClient = CreateClient("applicant-1", "Applicant");
        var applicantToken = await GetAntiForgeryTokenAsync(applicantClient, "/AssetLoanRequests/Create");

        var createResponse = await applicantClient.PostAsync("/AssetLoanRequests/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = applicantToken,
            ["RequestedCategory"] = "Laptop",
            ["Purpose"] = "Kerja lapangan",
            ["NeededFrom"] = DateTime.Today.ToString("yyyy-MM-dd"),
            ["submitAction"] = "Submit"
        }));

        Assert.Equal(HttpStatusCode.Redirect, createResponse.StatusCode);

        var loanRequest = await db.AssetLoanRequests
            .Include(l => l.Submission)
            .FirstAsync(l => l.RequestedCategory == "Laptop");

        Assert.Equal(SubmissionStatus.Submitted, loanRequest.Submission.Status);
        Assert.Null(loanRequest.AssetId);

        var ictClient = CreateClient("ict-admin-1", "IctAdmin");
        var fulfillToken = await GetAntiForgeryTokenAsync(ictClient, $"/AssetLoanRequests/Fulfill/{loanRequest.Id}");

        var fulfillResponse = await ictClient.PostAsync("/AssetLoanRequests/Fulfill", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = fulfillToken,
            ["AssetLoanRequestId"] = loanRequest.Id.ToString(),
            ["AssetId"] = asset.Id.ToString()
        }));

        Assert.Equal(HttpStatusCode.Redirect, fulfillResponse.StatusCode);

        db.ChangeTracker.Clear();
        var updatedAsset = await db.Assets.FirstAsync(a => a.Id == asset.Id);
        var updatedSubmission = await db.Submissions.FirstAsync(s => s.Id == loanRequest.SubmissionId);

        Assert.Equal(AssetStatus.OnLoan, updatedAsset.Status);
        Assert.Equal("applicant-1", updatedAsset.CurrentHolderUserId);
        Assert.Equal(SubmissionStatus.Completed, updatedSubmission.Status);
    }

    [Fact]
    public async Task Fulfillment_MenolakAsetYangBukanAvailable()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var takenAsset = new Asset
        {
            AssetTag = "TEST-AST-002",
            SerialNumber = "TEST-SN-002",
            Category = "Monitor",
            BrandModel = "Test Monitor",
            Status = AssetStatus.OnLoan,
            CurrentHolderUserId = "someone-else",
            Condition = "Baik"
        };
        db.Assets.Add(takenAsset);

        var submission = new Submission
        {
            ModuleCode = "AST-L",
            ApplicantUserId = "applicant-2",
            Status = SubmissionStatus.Submitted,
            ReferenceNo = $"AST-L-{DateTime.UtcNow.Year}-9001",
            SubmittedAt = DateTime.UtcNow
        };
        var loanRequest = new AssetLoanRequest
        {
            Submission = submission,
            RequestedCategory = "Monitor",
            Purpose = "Ujian"
        };
        db.AssetLoanRequests.Add(loanRequest);
        await db.SaveChangesAsync();

        var ictClient = CreateClient("ict-admin-1", "IctAdmin");
        var token = await GetAntiForgeryTokenAsync(ictClient, $"/AssetLoanRequests/Fulfill/{loanRequest.Id}");

        var response = await ictClient.PostAsync("/AssetLoanRequests/Fulfill", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["AssetLoanRequestId"] = loanRequest.Id.ToString(),
            ["AssetId"] = takenAsset.Id.ToString()
        }));

        // Ditolak dengan validation error — kekal 200 (render semula View), BUKAN redirect.
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);

        db.ChangeTracker.Clear();
        var stillTaken = await db.Assets.FirstAsync(a => a.Id == takenAsset.Id);
        Assert.Equal(AssetStatus.OnLoan, stillTaken.Status);
        Assert.Equal("someone-else", stillTaken.CurrentHolderUserId);
    }
}
```

Cipta `Nres.Onboarding.Tests/LaporDiriIntegrationTests.cs` (liputi "submit Lapor Diri", "approve", "reject dengan sebab", "invalid file type"):

```csharp
using System.Net;
using System.Net.Http.Headers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Nres.Onboarding.Web.Data;
using Nres.Onboarding.Web.Models;
using Xunit;

namespace Nres.Onboarding.Tests;

// Andaian: OfficerReportingApplicationsController (Hari 2-3) ikut corak
// submitAction=Draft/Submit yang sama seperti controller Hari 14. Laraskan
// nama field/route di bawah jika kod sebenar anda berbeza.
public class LaporDiriIntegrationTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public LaporDiriIntegrationTests(CustomWebApplicationFactory factory)
    {
        _factory = factory;
    }

    private HttpClient CreateClient(string userId, string roles)
    {
        var client = _factory.CreateClient(new WebApplicationFactoryClientOptions { AllowAutoRedirect = false });
        client.DefaultRequestHeaders.Add("X-Test-UserId", userId);
        client.DefaultRequestHeaders.Add("X-Test-Roles", roles);
        return client;
    }

    private async Task<string> GetAntiForgeryTokenAsync(HttpClient client, string url)
    {
        var html = await (await client.GetAsync(url)).Content.ReadAsStringAsync();
        var marker = "name=\"__RequestVerificationToken\" type=\"hidden\" value=\"";
        var start = html.IndexOf(marker, StringComparison.Ordinal) + marker.Length;
        return html[start..html.IndexOf('"', start)];
    }

    [Fact]
    public async Task Submit_LaporDiri_MenjanaNomborRujukanLD()
    {
        var client = CreateClient("applicant-ld-1", "Applicant");
        var token = await GetAntiForgeryTokenAsync(client, "/OfficerReportingApplications/Create");

        var response = await client.PostAsync("/OfficerReportingApplications/Create", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["FullName"] = "Ahmad bin Ali",
            ["IdentityNo"] = "900101015555",
            ["Email"] = "ahmad@example.gov.my",
            ["ReportingDate"] = DateTime.Today.ToString("yyyy-MM-dd"),
            ["submitAction"] = "Submit"
        }));

        Assert.Equal(HttpStatusCode.Redirect, response.StatusCode);

        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
        var submission = await db.Submissions.FirstAsync(s => s.ModuleCode == "LD" && s.ApplicantUserId == "applicant-ld-1");

        Assert.StartsWith("LD-", submission.ReferenceNo);
        Assert.Equal(SubmissionStatus.Submitted, submission.Status);
    }

    [Fact]
    public async Task Reject_TanpaSebab_DitolakOlehValidation()
    {
        using var scope = _factory.Services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();

        var submission = new Submission
        {
            ModuleCode = "LD",
            ApplicantUserId = "applicant-ld-2",
            Status = SubmissionStatus.Submitted,
            ReferenceNo = $"LD-{DateTime.UtcNow.Year}-9002"
        };
        db.Submissions.Add(submission);
        await db.SaveChangesAsync();

        var client = CreateClient("hr-admin-1", "HrAdmin");
        var token = await GetAntiForgeryTokenAsync(client, $"/OfficerReportingApplications/Details/{submission.Id}");

        var response = await client.PostAsync("/OfficerReportingApplications/Reject", new FormUrlEncodedContent(new Dictionary<string, string>
        {
            ["__RequestVerificationToken"] = token,
            ["id"] = submission.Id.ToString(),
            ["RejectReason"] = "" // sengaja kosong
        }));

        db.ChangeTracker.Clear();
        var reloaded = await db.Submissions.FirstAsync(s => s.Id == submission.Id);

        // Status TIDAK boleh bertukar Rejected tanpa sebab.
        Assert.NotEqual(SubmissionStatus.Rejected, reloaded.Status);
    }
}
```

> **Nota jenis fail muat naik tidak sah:** Ujian ini bergantung pada implementasi `IFileStorageService` Hari 3 anda (validasi `ContentType`/lanjutan fail). Corak ujiannya sama seperti di atas — `POST` ke action upload dengan `MultipartFormDataContent` mengandungi fail `.exe`, sahkan `ModelState` mengembalikan ralat ("Jenis fail tidak dibenarkan") dan **tiada** rekod `Attachment` dicipta. Laraskan nama action/medan mengikut kod Hari 3 sebenar anda.

Jalankan:

```bash
dotnet test
```

✅ **Semakan:** Semua integration test **Passed**. Sahkan `AssetLoanIntegrationTests.SelesaikanPinjamanAset_...` khususnya — ini ujian **paling penting** hari ini kerana ia membuktikan transaksi Hari 14 benar-benar konsisten hujung-ke-hujung.

---

# BAHAGIAN C — Deployment (SESI 46)

## Latihan 9 — `appsettings` Per Persekitaran

Cipta/kemas kini `appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=YOUR_SQL_SERVER;Database=NresOnboarding;User Id=YOUR_USER;Password=YOUR_PASSWORD;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": {
      "Default": "Warning",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "FileStorage": {
    "UploadRootPath": "/var/nres-onboarding/uploads"
  }
}
```

Kemas kini `Program.cs` untuk pilih penyedia pangkalan data ikut persekitaran:

```csharp
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (builder.Environment.IsDevelopment())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(connectionString));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(connectionString));
}
```

> **Perhatikan:** hanya **baris pemilihan provider** yang berubah — `ApplicationDbContext`, semua `Model`, semua `Controller` **kekal sama**. Ini bukti abstraksi EF Core berbaloi.

---

## Latihan 10 — Migration Bundle Untuk Pengeluaran

Bina "migration bundle" — satu fail boleh laku (`.exe`/binari) yang menjalankan migration **tanpa** perlu `dotnet ef` dipasang di pelayan pengeluaran:

```bash
dotnet ef migrations bundle --output efbundle --self-contained -r linux-x64
```

Di pelayar pengeluaran:

```bash
./efbundle --connection "Server=...;Database=NresOnboarding;..."
```

> Rujukan rasmi: [learn.microsoft.com/ef/core/managing-schemas/migrations/applying](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/applying#bundles)

---

## Latihan 11 — Senarai Semak Keluaran (Deployment Checklist)

Salin senarai ini ke dalam repositori projek anda (`DEPLOYMENT.md`) dan tandakan setiap item sebelum go-live:

- [ ] `appsettings.Production.json` diisi dengan connection string SQL Server/PostgreSQL sebenar (bukan SQLite latihan)
- [ ] `dotnet ef migrations bundle` dijana & dijalankan terhadap pangkalan data pengeluaran (atau `dotnet ef database update` dijalankan sekali oleh admin berkebenaran)
- [ ] Folder `App_Data/uploads/` (atau lokasi setara pengeluaran) wujud dengan **kebenaran tulis** untuk akaun perkhidmatan aplikasi sahaja (bukan `777`/world-writable)
- [ ] HTTPS dikuatkuasakan (`app.UseHttpsRedirection()`, sijil SSL sah dipasang, `UseHsts()` diaktifkan untuk pengeluaran)
- [ ] Pelayan hosting ditetapkan — **IIS** (`web.config` + ASP.NET Core Module), **Linux systemd** (unit file + Nginx reverse proxy), atau **kontena** (Dockerfile + orkestrasi)
- [ ] Dasar sandaran (backup) pangkalan data ditetapkan (kekerapan, lokasi simpanan, ujian pemulihan)
- [ ] Pengguna & peranan admin awal (`HrAdmin`, `SecurityAdmin`, `IctAdmin`, `ComplianceAdmin`, `SystemAdmin`) di-seed dengan kata laluan sementara yang **mesti** ditukar log masuk pertama
- [ ] Data seed latihan/sintetik (Hari 1–14) **dibuang atau digantikan** dengan data sah jabatan
- [ ] Konfigurasi `Logging` pengeluaran ditetapkan ke tahap sesuai (`Warning`/`Error`) — bukan `Debug`/`Trace` (risiko kebocoran maklumat sensitif dalam log)
- [ ] Variabel persekitaran sensitif (connection string, secret) **tidak** disimpan dalam kod sumber — guna `dotnet user-secrets` (dev) atau pengurusan secret pelayan (pengeluaran)

Contoh unit `systemd` (Linux):

```ini
[Unit]
Description=Nres Onboarding Web App

[Service]
WorkingDirectory=/var/www/nres-onboarding
ExecStart=/usr/bin/dotnet /var/www/nres-onboarding/Nres.Onboarding.Web.dll
Restart=always
RestartSec=10
KillSignal=SIGINT
Environment=ASPNETCORE_ENVIRONMENT=Production
Environment=DOTNET_PRINT_TELEMETRY_MESSAGE=false

[Install]
WantedBy=multi-user.target
```

Contoh `Dockerfile` ringkas:

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
EXPOSE 8080

FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish "Nres.Onboarding.Web/Nres.Onboarding.Web.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=build /app/publish .
ENTRYPOINT ["dotnet", "Nres.Onboarding.Web.dll"]
```

✅ **Semakan:** Anda boleh terangkan lisan **setiap** item senarai semak — kenapa ia wujud, apa risiko jika terlepas pandang.

---

# BAHAGIAN D — Capstone (SESI Capstone)

## Latihan 12 — Skrip Ujian Manual 11 Langkah

Jalankan skrip ini **secara langsung** merentasi kelima-lima modul, dengan sekurang-kurangnya 2 peserta berlainan peranan (atau satu peserta *switch* akaun):

1. Pemohon hantar **Lapor Diri**.
2. `HrAdmin` luluskan Lapor Diri.
3. Pemohon hantar permohonan **pelekat kenderaan**.
4. `SecurityAdmin` tolak dengan sebab.
5. Pemohon hantar permohonan **ID AD/Email**.
6. `Supervisor` luluskan.
7. `IctAdmin` selesaikan (`Completed`).
8. Pemohon isi & hantar **pengisytiharan PKS**.
9. `ComplianceAdmin` eksport CSV pematuhan.
10. Pemohon hantar **permohonan pinjaman aset**.
11. `IctAdmin` selesaikan pinjaman — sahkan status aset bertukar.

✅ **Semakan:** Kesemua 11 langkah berjaya **tanpa** ralat 500, setiap langkah menghasilkan entri `AuditLog`, dan dashboard bersepadu (Latihan 2) mencerminkan status terkini selepas setiap langkah.

---

## Latihan 13 — Persediaan Demo Capstone

Sediakan pembentangan pendek (10–15 minit setiap kumpulan/individu) merangkumi:

1. **Demo langsung** — jalankan skrip 11 langkah di atas di hadapan penilai.
2. **Terangkan seni bina** — lukis rajah `Submission` induk + 5 modul anak, jelaskan kenapa dikongsi.
3. **Tunjuk ujian** — jalankan `dotnet test` langsung, terangkan perbezaan unit vs integration test yang ditulis.
4. **Tunjuk senarai semak deployment** — terangkan apa yang perlu berubah untuk go-live sebenar.
5. **Refleksi** — apa cabaran paling sukar sepanjang 15 hari, dan bagaimana corak `Form → Draft → Submit → Review → Approve → Audit` membantu menyelesaikannya.

Rujuk kriteria penilaian rasmi dalam [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) (bahagian "Kriteria Penilaian (Capstone)"):

| Kriteria | Wajaran |
|----------|---------|
| Modul lengkap & berfungsi (5 modul) | 30% |
| Corak aliran kerja betul (draft→submit→approve→audit) | 20% |
| Validation, authorization & keselamatan | 20% |
| Ujian (xUnit) | 15% |
| Pembentangan & dokumentasi | 15% |

✅ **Semakan akhir kursus:** Peserta yang menyiapkan semua lab, aliran 5 modul, ujian, dan pembentangan capstone menerima **Sijil Penyertaan — Pembangunan Sistem Dalaman NRES Dengan ASP.NET Core**.

---

**Cross-ref rujukan:** Banding struktur integrasi/ujian/deployment anda dengan `../../projek/` (projek rujukan penuh) dan `../../projek/Nres.Onboarding.Tests/` selepas cuba sendiri.

Tahniah — anda telah menamatkan **DOTNET-NRES-15**. Baca [`../nota-penceramah.md`](./nota-penceramah.md) (bahagian **Mesej Coaching Akhir**) sebelum sesi capstone bermula.
