# Lab Pemanasan — Persekitaran .NET & Scaffold Repo Anda

> Konsep di [`../README.md`](../README.md). Kanun teknikal: [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md) · ciri C# 14 yang dibenarkan: [`../../AGENTS.md`](../../AGENTS.md).
>
> **Hari ini kita menaip semuanya dengan tangan.** Tiada AI menjana kod sehingga Latihan 5 (pratonton). Salin-tampal dari lab ini dibenarkan; menjana melalui Claude Code **tidak** — sehingga bahagian pratonton.

## Persediaan

- Sambungan internet (muat turun SDK, pakej NuGet, clone repo)
- Keizinan memasang perisian pada mesin anda · ruang cakera ~1–2 GB
- **Akses ke repo pasukan anda** dalam org [`nres-bpm`](https://github.com/nres-bpm) — jurulatih menambah anda sebagai *collaborator*

> **Hari ini menghasilkan artifak SEBENAR.** Kita **clone repo pasukan anda** (yang kini mengandungi `README.md` sahaja) dan **scaffold skeleton .NET ke dalamnya** pada satu cabang → PR. Ini bukan lagi projek buangan — ia permulaan sistem sebenar anda. Latihan gelung MVC (Latihan 3) dibuat pada **cabang buangan** yang berasingan supaya skeleton kekal bersih.

**Nilai ganti (setiap pasukan berbeza)** — ambil dari README projek anda, seksyen **"🏗️ Bootstrap skeleton repo"**:

| Simbol | Maksud | Contoh (Lapor Diri) |
|--------|--------|---------------------|
| `<repo>` | slug repo pasukan | `lapor-diri` |
| `<Sistem>` | nama projek PascalCase | `LaporDiri` |

> Repo & nama setiap pasukan: `lapor-diri`/`LaporDiri` · `pematuhan-pks`/`PematuhanPks` · `pengurusan-kontrak`/`PengurusanKontrak` · `pas-parkir-pelekat`/`PasParkirPelekat` · `id-ad-email`/`IdAdEmail` · `tempahan-fasiliti-sukan`/`TempahanFasilitiSukan`.

---

## Latihan 0 — Pasang & sahkan persekitaran

**Objektif:** .NET 10 SDK berjalan, editor sedia, sijil HTTPS dipercayai, dan akses GitHub berfungsi.

### Langkah

1. **Semak sama ada .NET sudah dipasang:**

```bash
dotnet --version
```

- `10.x` → teruskan ke langkah 3.
- Versi lebih rendah (cth. `8.x`) → SDK 10 boleh dipasang bersebelahan; langkah 2.
- `command not found` / `not recognized` → belum dipasang; langkah 2.

2. **Pasang .NET 10 SDK:**

   > **[https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)** → pilih **SDK** (bukan Runtime) untuk OS anda.

   - **Windows / macOS:** jalankan pemasang `.exe` / `.pkg`, terima lalai.
   - **Linux / macOS (alternatif):** ikut [learn.microsoft.com/dotnet/core/install](https://learn.microsoft.com/en-us/dotnet/core/install/).

   **Tutup dan buka semula terminal** selepas memasang (supaya `PATH` dimuat semula).

3. **Sahkan ketiga-tiga versi sepadan:**

```bash
dotnet --version           # 10.x
dotnet --list-sdks         # sekurang-kurangnya satu baris 10.x
dotnet --list-runtimes     # mesti ada "Microsoft.AspNetCore.App 10.x"
```

> Jika `dotnet --version` menunjukkan versi lama: `PATH` menunjuk ke pemasangan lama. macOS/Linux: `which dotnet`; Windows: `where dotnet`. Betulkan susunan `PATH` atau buka terminal baharu.

4. **Sediakan editor:** VS Code + **C# Dev Kit** · JetBrains Rider · Visual Studio 2022/2026 (Windows).

5. **Percayai sijil HTTPS pembangunan:**

```bash
dotnet dev-certs https --trust
```

   > macOS/Windows: terima bila diminta. Linux: `--trust` mungkin tiada kesan — guna `http://localhost` sebaliknya.

6. **Sahkan akses GitHub** (untuk clone repo privat pasukan). Cara mudah dengan GitHub CLI:

```bash
gh auth login            # ikut wizard: GitHub.com → HTTPS → pelayar
gh auth status           # sahkan "Logged in"
```

   > Tiada `gh`? Guna Git biasa dengan HTTPS + Personal Access Token, atau kunci SSH. Yang penting: `git clone` repo privat berjaya.

### ✅ Semakan

- [ ] `dotnet --version` → `10.x`
- [ ] `dotnet --list-runtimes` menyenaraikan `Microsoft.AspNetCore.App 10.x`
- [ ] Editor mengenali fail C#
- [ ] `dotnet dev-certs https --trust` selesai (atau anda tahu guna `http://` di Linux)
- [ ] `gh auth status` (atau setara) menunjukkan anda log masuk

---

## Latihan 1 — Clone repo pasukan & scaffold skeleton

**Objektif:** Repo `nres-bpm/<repo>` anda kini mengandungi rangka .NET yang **berjalan** — pada satu cabang, sedia untuk PR.

> Arahan penuh & khusus pasukan ada dalam README projek anda (**"🏗️ Bootstrap skeleton repo"**). Di bawah ialah bentuk generik — ganti `<repo>` dan `<Sistem>`.

### Langkah

1. **Clone repo pasukan** (ia sudah ada `README.md`) dan buka cabang scaffold:

```bash
git clone https://github.com/nres-bpm/<repo>.git
cd <repo>
git switch -c chore/scaffold
```

2. **Tambah `.gitignore`** supaya `bin/`, `obj/`, `*.db` tidak di-commit:

```bash
dotnet new gitignore
```

3. **Cipta solution + 3 projek.** Struktur kanun: `src/<Sistem>.Web` + `src/<Sistem>.Profile` + `tests/<Sistem>.Tests`.

```bash
dotnet new sln -n <Sistem>
dotnet new mvc      -o src/<Sistem>.Web        # Lapor Diri sahaja: tambah --auth Individual
dotnet new classlib -o src/<Sistem>.Profile    # klien/kontrak Profile DB
dotnet new xunit    -o tests/<Sistem>.Tests
dotnet sln add src/<Sistem>.Web src/<Sistem>.Profile tests/<Sistem>.Tests
```

4. **Rujukan projek + pakej EF Core** (Hari 4 tambah entiti & migration):

```bash
dotnet add src/<Sistem>.Web    reference src/<Sistem>.Profile
dotnet add tests/<Sistem>.Tests reference src/<Sistem>.Web
dotnet add src/<Sistem>.Web package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/<Sistem>.Web package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef         # sekali per mesin
```

5. **Cipta folder modul anda** (nama folder khusus pasukan — lihat README projek):

```bash
cd src/<Sistem>.Web
mkdir -p Models Views ViewModels Services Data App_Data/uploads
cd ../..
```

6. **Jalankan aplikasi:**

```bash
dotnet run --project src/<Sistem>.Web
```

   Buka `https://localhost:7xxx` yang dipaparkan — halaman selamat datang lalai. `Ctrl+C` untuk henti. Untuk auto-reload sepanjang hari: `dotnet watch --project src/<Sistem>.Web`.

7. **Commit skeleton** (belum push — kita push selepas faham & bersih):

```bash
git add .
git commit -m "<PREFIX>: scaffold skeleton (Web + Profile + Tests)"
```

### ✅ Semakan

- [ ] `git branch` menunjukkan anda pada `chore/scaffold`
- [ ] `dotnet run --project src/<Sistem>.Web` memaparkan halaman lalai
- [ ] `git status` bersih (tiada `bin/`/`obj/` — `.gitignore` berfungsi)
- [ ] Satu commit skeleton wujud (`git log --oneline`)

---

## Latihan 2 — Faham apa yang di-scaffold

**Objektif:** Kotak `dotnet new mvc` bukan lagi ajaib — anda tahu setiap bahagian pentingnya.

### Langkah

1. **Lihat struktur** `src/<Sistem>.Web/` (dalam editor atau `ls -R`):

```text
src/<Sistem>.Web/
  Controllers/HomeController.cs      ← logik: terima permintaan, sedia data
  Models/ErrorViewModel.cs           ← kelas data
  Views/
    Home/Index.cshtml                ← templat halaman utama (Razor)
    Shared/_Layout.cshtml            ← rangka bersama semua halaman
    _ViewImports.cshtml              ← import + tag helpers
    _ViewStart.cshtml                ← "guna _Layout untuk setiap view"
  wwwroot/                           ← fail statik (css, js, imej)
  Program.cs                         ← titik masuk: sediakan & mulakan aplikasi
  <Sistem>.Web.csproj                ← definisi projek & pakej
```

2. **Buka `Program.cs`.** Baca perlahan:

```csharp
var builder = WebApplication.CreateBuilder(args);

// --- Daftar servis (Dependency Injection container) ---
builder.Services.AddControllersWithViews();

var app = builder.Build();

// --- Middleware pipeline: setiap permintaan melalui ini, atas ke bawah ---
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();
app.UseRouting();
app.UseAuthorization();

// --- Peta URL → Controller/Action ---
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
```

   Dua idea:
   - **Baris `pattern`** membuat URL kosong (`/`) pergi ke `HomeController.Index`. `{controller=Home}` = "Home ialah lalai".
   - **Susunan middleware penting.** Permintaan mengalir atas ke bawah — cth. `UseRouting` sebelum `UseAuthorization`.

3. **Buka `Controllers/HomeController.cs`.** Setiap kaedah `public` yang mengembalikan `IActionResult` ialah satu **action**:

```csharp
public IActionResult Index()   // → GET /  atau  /Home/Index
{
    return View();             // paparkan Views/Home/Index.cshtml
}
```

4. **Buka `Views/Home/Index.cshtml`** — HTML bercampur C# (Razor); `@` memulakan kod C#.

### ✅ Semakan

- [ ] Anda boleh menunjuk fail yang membuat URL `/` pergi ke Home
- [ ] Anda boleh terangkan beza *service registration* vs *middleware pipeline* dalam `Program.cs`
- [ ] Anda tahu action ialah kaedah `public` yang mengembalikan `IActionResult`
- [ ] Anda tahu namespace projek anda ialah `<Sistem>.Web` (bukan `HelloNres`)

---

## Latihan 3 — Gelung MVC dengan tangan (cabang buangan)

**Objektif:** Buktikan anda faham gelung Model → View → Controller — pada **cabang buangan**, supaya skeleton sebenar kekal bersih.

> ⚠️ **Ini latihan PEMAHAMAN, bukan kerja modul sebenar.** Kita buat pada cabang `latihan/mvc-loop`, faham, kemudian **buang**. Modul sebenar anda bermula Hari 4.
>
> Dalam kod di bawah, ganti `<Sistem>` dengan nama projek anda (lihat baris `namespace` dalam mana-mana fail `.cs` yang di-scaffold).

### Langkah

1. **Buka cabang buangan** dari skeleton:

```bash
git switch -c latihan/mvc-loop
```

2. **Model.** Fail `src/<Sistem>.Web/Models/Permohonan.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace <Sistem>.Web.Models;

public class Permohonan
{
    [Required, Display(Name = "No. Rujukan")]
    public string Rujukan { get; set; } = string.Empty;

    [Required]
    public string Modul { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = "Submitted";
}
```

   > Data contoh **sintetik** sahaja — jangan guna data NRES sebenar (rujuk [`../../CLAUDE.md`](../../CLAUDE.md)).

3. **Controller.** Fail `src/<Sistem>.Web/Controllers/PermohonanController.cs`:

```csharp
using <Sistem>.Web.Models;
using Microsoft.AspNetCore.Mvc;

namespace <Sistem>.Web.Controllers;

public class PermohonanController : Controller
{
    // Senarai dalam-memori — belum ada pangkalan data (EF Core datang Hari 4).
    private static readonly List<Permohonan> _senarai =
    [
        new() { Rujukan = "LD-2026-0001",     Modul = "Lapor Diri",   Status = "Submitted" },
        new() { Rujukan = "PAS-2026-0001",    Modul = "Pas & Parkir", Status = "AdminApproved" },
        new() { Rujukan = "ICT-ID-2026-0001", Modul = "ID/AD/Email",  Status = "Submitted" },
    ];

    // GET /Permohonan
    public IActionResult Index() => View(_senarai);

    // GET /Permohonan/Baharu
    [HttpGet]
    public IActionResult Baharu() => View(new Permohonan());

    // POST /Permohonan/Baharu
    [HttpPost]
    public IActionResult Baharu(Permohonan permohonan)
    {
        if (!ModelState.IsValid)
            return View(permohonan);        // ada ralat — papar semula
        _senarai.Add(permohonan);           // simpan (dalam memori)
        return RedirectToAction(nameof(Index));
    }
}
```

   Perhatikan **collection expression** `[ ... ]` + `new()` bertaip-sasaran (C# 12+). `model binding` mengisi `permohonan` dari medan borang secara automatik.

4. **View senarai.** Fail `src/<Sistem>.Web/Views/Permohonan/Index.cshtml`:

```cshtml
@model List<Permohonan>
@{
    ViewData["Title"] = "Papan Permohonan NRES";
}

<h1>@ViewData["Title"]</h1>
<p><a class="btn btn-primary" asp-action="Baharu">+ Permohonan baharu</a></p>

<table class="table">
    <thead>
        <tr><th>No. Rujukan</th><th>Modul</th><th>Status</th></tr>
    </thead>
    <tbody>
        @foreach (var p in Model)
        {
            <tr><td>@p.Rujukan</td><td>@p.Modul</td><td>@p.Status</td></tr>
        }
    </tbody>
</table>
```

5. **View borang.** Fail `src/<Sistem>.Web/Views/Permohonan/Baharu.cshtml`:

```cshtml
@model Permohonan
@{
    ViewData["Title"] = "Permohonan baharu";
}

<h1>@ViewData["Title"]</h1>

<form asp-action="Baharu" method="post">
    <div asp-validation-summary="ModelOnly" class="text-danger"></div>

    <div class="mb-3">
        <label asp-for="Rujukan" class="form-label"></label>
        <input asp-for="Rujukan" class="form-control" />
        <span asp-validation-for="Rujukan" class="text-danger"></span>
    </div>
    <div class="mb-3">
        <label asp-for="Modul" class="form-label"></label>
        <input asp-for="Modul" class="form-control" />
        <span asp-validation-for="Modul" class="text-danger"></span>
    </div>
    <div class="mb-3">
        <label asp-for="Status" class="form-label"></label>
        <input asp-for="Status" class="form-control" />
        <span asp-validation-for="Status" class="text-danger"></span>
    </div>

    <button type="submit" class="btn btn-primary">Simpan</button>
    <a asp-action="Index" class="btn btn-secondary">Batal</a>
</form>
```

   > `@model Permohonan` tanpa nama penuh berfungsi kerana `_ViewImports.cshtml` sudah `@using <Sistem>.Web` dan `@addTagHelper`. `asp-for`/`asp-validation-for` ialah **tag helper**.

6. **Jalankan & uji:**

```bash
dotnet run --project src/<Sistem>.Web
```

   - Layari `/Permohonan` → jadual 3 baris seed.
   - Klik **+ Permohonan baharu**, hantar borang **kosong** → mesej "The Rujukan field is required." (validation pelayan).
   - Isi & **Simpan** → kembali ke senarai dengan baris baharu.
   - Hentikan (`Ctrl+C`), jalankan semula, layari `/Permohonan` → baris tambahan **hilang** (dalam memori sahaja). **Itu sebab kita perlukan EF Core — Hari 4.**

7. **Buang latihan ini** supaya skeleton kekal bersih:

```bash
git switch chore/scaffold
git branch -D latihan/mvc-loop      # buang cabang + demo sekali
```

### ✅ Semakan

- [ ] Anda melihat gelung penuh: `_senarai` (Controller) → `Model` (View) → HTML
- [ ] Borang kosong → ralat validation; borang sah → baris baharu
- [ ] Selepas mula semula, tambahan hilang — dan anda tahu **kenapa**
- [ ] Anda kembali pada `ch/scaffold` dan cabang `latihan/mvc-loop` sudah dibuang

---

## Latihan 4 — Push skeleton & buka PR

**Objektif:** Skeleton bersih masuk ke `nres-bpm/<repo>` melalui cabang → PR (bukan terus ke `main`).

### Langkah

1. Pastikan anda pada `chore/scaffold` dengan skeleton bersih:

```bash
git switch chore/scaffold
git status                 # bersih; hanya skeleton, tiada fail Permohonan
git log --oneline          # commit skeleton anda + README asal
```

2. **Push cabang & buka PR:**

```bash
git push -u origin chore/scaffold
```

   Buka pautan PR yang dipaparkan (atau di GitHub) → buka **Pull Request** `chore/scaffold` → `main`. Tajuk: `Scaffold skeleton <Sistem>`.

3. **Minta semakan rakan** (pair), kemudian jurulatih sahkan sebelum merge. `main` dilindungi — **tiada push terus**.

> Kenapa PR walau ini scaffold? Kerana ia melatih aliran sebenar Fasa 2: cabang pendek → PR → semakan → merge. Anda akan ulang corak ini setiap hari.

### ✅ Semakan

- [ ] Cabang `chore/scaffold` ada di GitHub
- [ ] PR ke `main` dibuka, hanya mengandungi skeleton (tiada fail `Permohonan`)
- [ ] Rakan/jurulatih menyemak sebelum merge

---

## Latihan 5 — Pratonton Claude Code (perbincangan, 15 minit)

**Objektif:** Lihat apa AI lakukan dengan tugas yang sama — dan di mana sempadannya. Jurulatih memandu di skrin; peserta memerhati.

### Langkah

1. Dalam Claude Code (pada projek anda), cuba satu arahan seperti:

   > *"Dalam projek `src/<Sistem>.Web` ini, tambah action `Butiran(string rujukan)` pada satu `PermohonanController` contoh dan View `Butiran.cshtml` yang sepadan. Guna corak MVC yang sama seperti Home."*

2. **Nilai output bersama** — inilah sebab kita membina dengan tangan dahulu:
   - Fail di tempat betul (`Views/Permohonan/Butiran.cshtml`)?
   - `@model` sepadan dengan apa yang action hantar?
   - Ada mereka-reka servis/pakej yang tak wujud?
   - Boleh anda **baca** setiap baris & sahkan ia betul?

3. **Poin utama:** anda boleh menjawab itu **kerana** anda scaffold & bina gelung MVC sendiri pagi tadi. Itulah tanda aras yang kita bawa ke Hari 4, di mana kerja berbantu-AI bermula.

> Slaid sokongan: kluster **"Claude Code & ekosistem AI"** dalam [`../../slides/dotnet-nres-training.html`](../../slides/dotnet-nres-training.html).

### ✅ Semakan

- [ ] Anda melihat AI menjana kod MVC dan **menilainya** terhadap yang anda tulis sendiri
- [ ] Anda boleh nyatakan satu perkara AI buat betul dan satu yang perlu disemak
- [ ] **Jangan** commit kod pratonton ini ke PR skeleton

---

## Deliverable Hari Ini

- [ ] `dotnet --version` → `10.x` disahkan pada mesin anda
- [ ] Repo `nres-bpm/<repo>` anda mempunyai **skeleton berjalan** (Web + Profile + Tests) pada cabang `chore/scaffold`
- [ ] **PR ke `main`** dibuka dengan skeleton bersih
- [ ] Anda membina gelung Model-View-Controller **dengan tangan** (cabang buangan) dan faham had "dalam memori"
- [ ] Anda boleh terangkan gelung MVC kepada rakan **tanpa** melihat nota

## Bermula Hari 4

Selepas PR skeleton di-merge, Hari 4 membina **modul sebenar** anda di atas rangka ini — entiti (`Submission`, `Attachment`, `AuditLog`, `ApprovalStep` + entiti modul anda), `IEntityTypeConfiguration<T>`, migration EF Core, dan borang draf. Klien `<Sistem>.Profile` menyambung kontrak **Profile DB** (`nres-bpm/profile`). Yang penting hari ini: mesin anda sedia, repo anda hidup, dan anda faham rangka sebelum AI membantu mempercepatnya.
