# Identity, Roles & Authorization 🔒

> Nota konsep untuk **Hari 3** (Identity + 6 peranan diseed) dan diperdalam oleh **Kumpulan 3** pada blok Hari 7–9 (RBAC penuh merentas modul). Lihat [`03-corak-workflow.md`](./03-corak-workflow.md) untuk konteks siapa bertindak pada setiap status `Submission`.

---

## ASP.NET Core Identity — asas

**ASP.NET Core Identity** ialah sistem *membership* terbina-dalam untuk pengurusan pengguna: pendaftaran, log masuk, hash kata laluan, peranan (*roles*), dan *claims*. Ia disepadukan terus dengan EF Core melalui `IdentityDbContext`.

```csharp
public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? DepartmentId { get; set; }
}

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }
    // DbSet lain seperti biasa...
}
```

Daftar dalam `Program.cs`:

```csharp
builder.Services.AddDefaultIdentity<ApplicationUser>(options =>
    {
        options.SignIn.RequireConfirmedAccount = false;   // dipermudah untuk latihan
        options.Password.RequiredLength = 8;
    })
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();
```

> **Kata laluan tidak pernah disimpan sebagai teks biasa** — Identity meng-*hash*-nya secara automatik (PBKDF2 dengan salt). Lihat [`09-keselamatan.md`](./09-keselamatan.md) untuk prinsip penuh.

---

## 7 Peranan (Roles) — MUKTAMAD

Mengikut [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md), sistem `Nres.Onboarding.Web` guna 7 peranan berikut:

| Role | Tanggungjawab |
|------|---------------|
| `Applicant` | Cipta draf & hantar permohonan |
| `Supervisor` | Semak permohonan staf (jika perlu) |
| `HrAdmin` | Semak Lapor Diri |
| `SecurityAdmin` | Semak pas, parkir, pelekat kenderaan |
| `IctAdmin` | Semak AD/email, perisian, aset ICT |
| `SystemAdmin` | Urus pengguna & data lookup |

### Seed peranan semasa aplikasi mula (`Program.cs` atau `DbInitializer`)

```csharp
var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
string[] roles = ["Applicant", "Supervisor", "HrAdmin", "SecurityAdmin", "IctAdmin", "SystemAdmin"];

foreach (var role in roles)
{
    if (!await roleManager.RoleExistsAsync(role))
        await roleManager.CreateAsync(new IdentityRole(role));
}
```

---

## `[Authorize(Roles = ...)]` — kuatkuasa di Controller

```csharp
[Authorize(Roles = "HrAdmin")]
public class ReportingReviewController : Controller
{
    public async Task<IActionResult> Index() => View(await GetPendingReviewsAsync());

    [HttpPost]
    [Authorize(Roles = "HrAdmin")]
    public async Task<IActionResult> Approve(int submissionId) { /* ... */ }
}
```

Boleh juga digabungkan berbilang peranan pada satu Action:

```csharp
[Authorize(Roles = "IctAdmin,SystemAdmin")]
public IActionResult AssetInventory() { /* IctAdmin ATAU SystemAdmin boleh akses */ }
```

---

## Policy-based authorization — lebih fleksibel

Untuk peraturan yang lebih kompleks daripada senarai role mudah (contoh: hanya `IctAdmin` yang menguruskan jabatan sendiri), guna **policy**:

```csharp
builder.Services.AddAuthorization(options =>
{
    options.AddPolicy("CanApproveIctRequests", policy =>
        policy.RequireRole("IctAdmin", "SystemAdmin"));

    options.AddPolicy("CanManageUsers", policy =>
        policy.RequireRole("SystemAdmin"));
});
```

```csharp
[Authorize(Policy = "CanApproveIctRequests")]
public IActionResult Approve(int id) { /* ... */ }
```

> **Bila guna Policy berbanding `Roles=`?** `Roles=` cukup untuk semakan peranan mudah. Guna **Policy** apabila peraturan melibatkan gabungan logik (role + claim + keadaan lain) — Kumpulan 3 membincangkannya pada blok Hari 7–9; untuk kursus ini semakan dalam kaedah action sudah memadai dan lebih mudah dibaca.

---

## UI menu visibility vs controller enforcement — JANGAN CAMPUR AUR

Ini **prinsip keselamatan paling penting** dalam bab ini:

> **Sembunyikan pautan menu di UI HANYALAH kemudahan pengguna (UX) — BUKAN kawalan keselamatan.** Kawalan sebenar MESTI berada di Controller/Action melalui `[Authorize]`.

```html
@* Views/Shared/_Layout.cshtml — sembunyi pautan jika bukan HrAdmin (UX sahaja) *@
@if (User.IsInRole("HrAdmin"))
{
    <a asp-controller="ReportingReview" asp-action="Index">Semakan Lapor Diri</a>
}
```

Walaupun pautan disembunyikan, seorang pengguna jahat masih boleh menaip URL terus (`/ReportingReview/Index`) jika Controller **tidak** dilindungi `[Authorize]`. Sebab itu:

- Sembunyi menu → untuk pengalaman pengguna bersih.
- `[Authorize(Roles=...)]` pada **setiap** Controller/Action sensitif → untuk keselamatan sebenar.

Kedua-duanya perlu wujud **bersama** — jangan bergantung pada salah satu sahaja. Lihat [`09-keselamatan.md`](./09-keselamatan.md) untuk senarai semak penuh.

---

## Kaitan dengan hari-hari lain

- **Hari 1** — projek disediakan dengan Identity scaffold asas.
- **Kumpulan 3, blok Hari 7–9** — RBAC penuh + matriks RBAC merentas keempat-empat modul.
- **Kumpulan 4** — `IctAdmin` menguruskan Perisian & Aset ICT dengan Authorization yang sama.
- Lihat [`03-corak-workflow.md`](./03-corak-workflow.md) untuk peranan mana bertindak pada peringkat status yang mana.

---

## Sumber Rasmi

- **[Introduction to Identity on ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)**
- **[Role-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)**
- **[Policy-based authorization](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies)**
- **[Add, download, and delete user data (GDPR-style considerations)](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)**
