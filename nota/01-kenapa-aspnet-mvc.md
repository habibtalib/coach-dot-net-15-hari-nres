# Kenapa ASP.NET Core MVC?

> Nota konsep untuk **Hari 1** — dibaca sebelum membina projek `Nres.Onboarding.Web` pertama kali. Lihat [`../JADUAL.md`](../JADUAL.md) untuk sesi Hari 1, dan [`00-setup-dotnet.md`](./00-setup-dotnet.md) jika persekitaran belum sedia.

---

## Apa itu ASP.NET Core MVC?

**ASP.NET Core MVC** ialah rangka kerja (*framework*) web dalam .NET untuk membina aplikasi web dengan corak seni bina **Model-View-Controller (MVC)**. Ia memisahkan aplikasi kepada tiga bahagian dengan tanggungjawab jelas — sesuai untuk sistem seperti `Nres.Onboarding.Web` yang mempunyai banyak borang, aliran kelulusan, dan paparan status.

```text
Pengguna (Browser)
        │  HTTP request (cth. GET /Reporting/Create)
        ▼
   ┌─────────┐
   │ Routing │  → tentukan Controller & Action mana yang patut layan
   └─────────┘
        ▼
   ┌────────────┐        baca/tulis        ┌───────┐
   │ Controller │ ───────────────────────► │ Model │  (entiti, EF Core)
   └────────────┘                          └───────┘
        ▼
   ┌──────┐
   │ View │  (Razor .cshtml)  → jana HTML
   └──────┘
        ▼
   Respons HTML dihantar balik ke Browser
```

---

## Peranan Model / View / Controller

| Bahagian | Tanggungjawab | Contoh dalam `Nres.Onboarding.Web` |
|----------|---------------|-------------------------------------|
| **Model** | Data & logik domain — entiti EF Core, view model, servis | `Submission`, `OfficerReportingApplication`, `IReferenceNumberService` |
| **View** | Paparan (Razor `.cshtml`) — HTML + sedikit logik paparan | `Views/Reporting/Create.cshtml` |
| **Controller** | Terima *request*, panggil Model, pilih View, hantar respons | `ReportingController.Create()` |

> **Kenapa dipisahkan?** Supaya logik borang (Controller), struktur data (Model), dan paparan (View) boleh diubah **secara berasingan** tanpa mengganggu satu sama lain. Untuk aplikasi 4 modul seperti NRES, ini penting — setiap modul menambah Controller & View baharu tetapi **berkongsi** Model asas (`Submission`, `AuditLog`) — lihat [`03-corak-workflow.md`](./03-corak-workflow.md).

---

## Kitaran permintaan (request pipeline) — langkah demi langkah

1. **Routing** — ASP.NET Core memadankan URL (`/Reporting/Create/5`) kepada Controller + Action mengikut corak *convention-based routing* (`{controller}/{action}/{id?}`), didaftarkan dalam `Program.cs`:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
```

2. **Controller & Action** — kelas Controller (contoh `ReportingController : Controller`) mengandungi kaedah *Action* (contoh `Create`). Action menerima input (query string, form, route value), berinteraksi dengan Model (EF Core `DbContext`), dan memutuskan respons.

```csharp
public class ReportingController : Controller
{
    private readonly ApplicationDbContext _context;

    public ReportingController(ApplicationDbContext context) => _context = context;

    public async Task<IActionResult> Create()
    {
        return View(new OfficerReportingCreateViewModel());
    }

    [HttpPost]
    public async Task<IActionResult> Create(OfficerReportingCreateViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);
        // ...simpan draf, redirect
        return RedirectToAction(nameof(Index));
    }
}
```

3. **View** — Action memanggil `View(model)`, Razor mencari fail `.cshtml` sepadan (`Views/{Controller}/{Action}.cshtml`), *render* HTML menggunakan sintaks Razor (`@Model`, `@foreach`, dsb.).

4. **Respons** — HTML terjana dihantar balik ke *browser*.

---

## Bila guna MVC vs Razor Pages vs Web API

| Pendekatan | Sesuai bila | Kenapa **tidak** dipilih untuk NRES |
|-----------|-------------|--------------------------------------|
| **ASP.NET Core MVC** ✅ (pilihan kursus) | Aplikasi dengan banyak borang, aliran kerja kompleks, Controller berkongsi logik merentas beberapa View | — |
| **Razor Pages** | Aplikasi ringkas, satu halaman = satu fail model (page-focused), CRUD mudah | 4 modul NRES berkongsi banyak logik & entiti (`Submission`, `ApprovalStep`) — struktur Controller/View memberi organisasi lebih jelas untuk aliran kelulusan berbilang peringkat |
| **Web API (Minimal API / ASP.NET Core Web API)** | Backend tanpa UI HTML — hanya JSON untuk konsumer luar (SPA, mobile app) | Kursus ini perlu UI borang & paparan terus (server-rendered HTML) untuk pegawai NRES; tiada keperluan API awam berasingan |

> **Prinsip ringkas:** MVC sesuai apabila anda perlukan **UI web penuh + logik terkawal di server** dengan struktur berskala untuk banyak *entity* & *workflow* — tepat corak `Nres.Onboarding.Web`.

---

## Kaitan dengan hari-hari seterusnya

- **Hari 1** — bina projek MVC pertama, struktur `Controllers/`, `Models/`, `Views/`.
- **Hari 4 – 14** — setiap kumpulan membina modulnya (Lapor Diri, Pas/Parkir/Pelekat, ID/AD/Email, Perisian & Aset ICT) mengikut corak Controller → View → Model yang sama.
- Lihat [`02-efcore-migrations.md`](./02-efcore-migrations.md) untuk bagaimana Model disokong oleh EF Core, dan [`03-corak-workflow.md`](./03-corak-workflow.md) untuk corak aliran kerja yang diulang di setiap modul.

---

## Sumber Rasmi

- **[Overview of ASP.NET Core MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview)**
- **[Routing in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing)**
- **[Razor Pages vs MVC](https://learn.microsoft.com/en-us/aspnet/core/mvc/razor-pages/index)**
- **[Overview of ASP.NET Core Web API](https://learn.microsoft.com/en-us/aspnet/core/web-api)**
