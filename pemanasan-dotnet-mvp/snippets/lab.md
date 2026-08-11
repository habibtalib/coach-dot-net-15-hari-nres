# Lab Pemanasan — Persekitaran .NET & MVP Pertama Anda

> Konsep di [`../README.md`](../README.md). Kanun teknikal: [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md) · ciri C# 14 yang dibenarkan: [`../../AGENTS.md`](../../AGENTS.md).
>
> **Hari ini kita menaip semuanya dengan tangan.** Tiada AI menjana kod sehingga Latihan 6. Salin-tampal dari lab ini dibenarkan; menjana melalui Claude Code **tidak** — sehingga bahagian pratonton.

## Persediaan

- Sambungan internet (untuk memuat turun SDK & pakej NuGet kali pertama)
- Keizinan memasang perisian pada mesin anda
- Ruang cakera ~1–2 GB

> **Projek hari ini ialah projek buangan.** Kita membinanya **di luar** repo kursus (contoh dalam `~/latihan/`), sama seperti fail `pemanasan.cs` Hari 3. Ia **tidak** di-commit ke repo kursus — projek sebenar (`Nres.Onboarding.Web` dan repo `nres-bpm`) bermula bersih pada Hari 3. Tujuan hari ini ialah **kemahiran**, bukan artifak.

---

## Latihan 0 — Pasang & sahkan persekitaran

**Objektif:** .NET 10 SDK berjalan pada mesin anda, editor sedia, sijil HTTPS dipercayai.

### Langkah

1. **Semak sama ada .NET sudah dipasang:**

```bash
dotnet --version
```

- Jika ia memaparkan `10.x` → teruskan ke langkah 3.
- Jika ia memaparkan versi lebih rendah (cth. `8.x`) → SDK 10 masih boleh dipasang bersebelahan; teruskan ke langkah 2.
- Jika `command not found` / `not recognized` → belum dipasang; langkah 2.

2. **Pasang .NET 10 SDK.** Muat turun dari halaman rasmi dan ikut pemasang untuk sistem anda:

   > **[https://dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)** → pilih **SDK** (bukan Runtime) untuk OS anda.

   - **Windows / macOS:** jalankan pemasang `.exe` / `.pkg`, terima lalai.
   - **Linux / macOS (alternatif):** ikut arahan pengurus pakej di halaman [learn.microsoft.com/dotnet/core/install](https://learn.microsoft.com/en-us/dotnet/core/install/).

   **Tutup dan buka semula terminal** selepas memasang (supaya `PATH` dimuat semula).

3. **Sahkan ketiga-tiga versi sepadan:**

```bash
dotnet --version           # 10.x
dotnet --list-sdks         # sekurang-kurangnya satu baris 10.x
dotnet --list-runtimes     # mesti ada "Microsoft.AspNetCore.App 10.x"
```

> Jika `dotnet --version` menunjukkan versi lama walaupun 10 dipasang: `PATH` anda menunjuk ke pemasangan lama. Pada macOS/Linux semak `which dotnet`; pada Windows semak `where dotnet`. Betulkan susunan `PATH`, atau buka terminal baharu.

4. **Sediakan editor.** Mana-mana satu sudah memadai:
   - **Visual Studio Code** + sambungan **C# Dev Kit** (percuma, semua OS) — [code.visualstudio.com](https://code.visualstudio.com)
   - **JetBrains Rider** (percuma untuk bukan komersial)
   - **Visual Studio 2022/2026** (Windows sahaja)

5. **Percayai sijil HTTPS pembangunan** (supaya `https://localhost` tidak memberi amaran):

```bash
dotnet dev-certs https --trust
```

   > Pada macOS/Windows ia akan meminta kebenaran — terima. Pada Linux, `--trust` mungkin tiada kesan; anda boleh guna URL `http://localhost` sebaliknya.

### ✅ Semakan

- [ ] `dotnet --version` → `10.x`
- [ ] `dotnet --list-runtimes` menyenaraikan `Microsoft.AspNetCore.App 10.x`
- [ ] Editor pilihan dibuka dan mengenali fail C#
- [ ] `dotnet dev-certs https --trust` selesai tanpa ralat (atau anda tahu guna `http://` di Linux)

---

## Latihan 1 — MVP anda: aplikasi MVC yang berjalan

**Objektif:** Satu aplikasi ASP.NET Core MVC yang benar-benar berjalan dalam pelayar — rangka berjalan MVP kita.

### Langkah

1. **Cipta folder latihan dan projek** (di luar repo kursus):

```bash
mkdir -p ~/latihan
cd ~/latihan
dotnet new mvc -o HelloNres
cd HelloNres
```

   `dotnet new mvc` menjana projek MVC lengkap dengan satu contoh (Home). `-o HelloNres` meletakkannya dalam folder `HelloNres`.

2. **Jalankan aplikasi:**

```bash
dotnet run
```

   Cari baris seperti `Now listening on: https://localhost:7xxx` dalam output. Buka URL itu dalam pelayar. Anda sepatutnya nampak halaman selamat datang lalai.

3. **Berhenti** dengan `Ctrl+C` dalam terminal.

4. **Jalankan semula dengan auto-reload** (membina semula bila anda simpan fail — berguna sepanjang hari):

```bash
dotnet watch
```

   Biarkan ia berjalan dalam satu terminal; gunakan terminal **kedua** untuk arahan `git` dan sebagainya.

### ✅ Semakan

- [ ] Halaman selamat datang lalai muncul di `https://localhost:7xxx`
- [ ] `Ctrl+C` menghentikannya
- [ ] `dotnet watch` berjalan tanpa ralat

> **Sudah selesai?** Tahniah — anda baru sahaja menjalankan MVP pertama. Ia belum melakukan apa-apa yang berguna; itu langkah seterusnya. Itulah MVP: **berjalan dahulu, berguna kemudian.**

---

## Latihan 2 — Faham apa yang di-scaffold

**Objektif:** Kotak `dotnet new mvc` bukan lagi ajaib — anda tahu setiap bahagian pentingnya.

### Langkah

1. **Lihat struktur** (dalam editor, atau `ls -R` / `dir`):

```text
HelloNres/
  Controllers/HomeController.cs      ← logik: terima permintaan, sedia data
  Models/ErrorViewModel.cs           ← kelas data
  Views/
    Home/Index.cshtml                ← templat halaman utama (Razor)
    Home/Privacy.cshtml
    Shared/_Layout.cshtml            ← rangka bersama semua halaman
    _ViewImports.cshtml              ← import + tag helpers
    _ViewStart.cshtml                ← "guna _Layout untuk setiap view"
  wwwroot/                           ← fail statik (css, js, imej)
  Program.cs                         ← titik masuk: sediakan & mulakan aplikasi
  appsettings.json                   ← konfigurasi
  HelloNres.csproj                   ← definisi projek & pakej
```

2. **Buka `Program.cs`.** Ia pendek. Baca perlahan:

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

   Dua idea untuk difahami:
   - **Baris `pattern`** menentukan URL kosong (`/`) pergi ke `HomeController.Index`. `{controller=Home}` bermaksud "Home ialah lalai". Inilah sebab halaman utama ialah Home.
   - **Susunan middleware penting.** Permintaan mengalir dari atas ke bawah. Contoh: `UseRouting` mesti datang sebelum `UseAuthorization`.

3. **Buka `Controllers/HomeController.cs`.** Perhatikan setiap kaedah `public` yang mengembalikan `IActionResult` ialah satu **action** — boleh dicapai melalui URL:

```csharp
public IActionResult Index()   // → GET /  atau  /Home/Index
{
    return View();             // paparkan Views/Home/Index.cshtml
}
```

4. **Buka `Views/Home/Index.cshtml`.** Ini HTML bercampur C# (Razor). `@` memulakan kod C#.

5. **Cuba sendiri:** ubah teks dalam `Views/Home/Index.cshtml`, simpan. Jika `dotnet watch` berjalan, pelayar dikemas kini automatik. **Kembalikan** perubahan selepas mencuba.

### ✅ Semakan

- [ ] Anda boleh menunjuk fail mana yang membuat URL `/` pergi ke Home
- [ ] Anda boleh terangkan beza antara *service registration* dan *middleware pipeline* dalam `Program.cs`
- [ ] Anda tahu action ialah kaedah `public` yang mengembalikan `IActionResult`
- [ ] Perubahan pada `Index.cshtml` muncul dalam pelayar

---

## Latihan 3 — Model & Controller anda sendiri

**Objektif:** Bina bahagian Model + Controller MVP: senarai permohonan dalam memori, dipaparkan pada URL anda sendiri.

### Langkah

1. **Cipta Model.** Fail baharu `Models/Permohonan.cs`:

```csharp
using System.ComponentModel.DataAnnotations;

namespace HelloNres.Models;

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

2. **Cipta Controller.** Fail baharu `Controllers/PermohonanController.cs`:

```csharp
using HelloNres.Models;
using Microsoft.AspNetCore.Mvc;

namespace HelloNres.Controllers;

public class PermohonanController : Controller
{
    // Senarai dalam-memori — MVP: belum ada pangkalan data.
    // static supaya ia kekal antara permintaan (tetapi hilang bila aplikasi dimula semula).
    private static readonly List<Permohonan> _senarai =
    [
        new() { Rujukan = "LD-2026-0001",     Modul = "Lapor Diri",   Status = "Submitted" },
        new() { Rujukan = "PAS-2026-0001",    Modul = "Pas & Parkir", Status = "AdminApproved" },
        new() { Rujukan = "ICT-ID-2026-0001", Modul = "ID/AD/Email",  Status = "Submitted" },
    ];

    // GET /Permohonan
    public IActionResult Index() => View(_senarai);
}
```

   Perhatikan **collection expression** `[ ... ]` dan `new()` bertaip-sasaran — sintaks C# 12+ yang kita guna sepanjang kursus.

3. Kita belum ada View — jalankan dahulu untuk melihat ralat yang **jelas**:

```bash
dotnet run
```

   Layari `https://localhost:7xxx/Permohonan`. Anda dapat ralat *"view 'Index' was not found"* yang menyenaraikan lokasi yang dicari. **Ini berguna** — ia memberitahu tepat di mana View sepatutnya berada. Langkah seterusnya menciptanya.

### ✅ Semakan

- [ ] `Models/Permohonan.cs` dan `Controllers/PermohonanController.cs` wujud
- [ ] `/Permohonan` memberi ralat "view not found" (bukan ralat pembinaan)
- [ ] Anda faham kenapa `_senarai` itu `static`

---

## Latihan 4 — View anda sendiri: gelung MVC lengkap

**Objektif:** Tambah View supaya senarai dipaparkan — melengkapkan Model → View → Controller buat kali pertama.

### Langkah

1. **Cipta folder View** untuk controller ini: `Views/Permohonan/`.

2. **Cipta `Views/Permohonan/Index.cshtml`:**

```cshtml
@model List<HelloNres.Models.Permohonan>
@{
    ViewData["Title"] = "Papan Permohonan NRES";
}

<h1>@ViewData["Title"]</h1>

<p>
    <a class="btn btn-primary" asp-action="Baharu">+ Permohonan baharu</a>
</p>

<table class="table">
    <thead>
        <tr>
            <th>No. Rujukan</th>
            <th>Modul</th>
            <th>Status</th>
        </tr>
    </thead>
    <tbody>
        @foreach (var p in Model)
        {
            <tr>
                <td>@p.Rujukan</td>
                <td>@p.Modul</td>
                <td>@p.Status</td>
            </tr>
        }
    </tbody>
</table>
```

   - `@model` mengisytihar jenis data yang View ini terima — `List<Permohonan>` yang Controller hantar.
   - `asp-action="Baharu"` ialah **tag helper**; ia menjana URL yang betul ke action `Baharu` (kita bina dalam Latihan 5).
   - Kelas `table`/`btn` datang dari Bootstrap yang sudah termasuk dalam template.

3. **Jalankan** dan layari `https://localhost:7xxx/Permohonan`. Anda sepatutnya nampak jadual dengan tiga baris seed.

4. **(Pilihan) Jadikan ia halaman utama.** Dalam `Program.cs`, tukar route lalai:

```csharp
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Permohonan}/{action=Index}/{id?}");
```

   Sekarang URL kosong `/` terus ke papan permohonan.

### ✅ Semakan

- [ ] `/Permohonan` memaparkan jadual dengan 3 baris
- [ ] Anda boleh jejak data: `_senarai` (Controller) → `Model` (View) → baris jadual (HTML)
- [ ] Butang "+ Permohonan baharu" muncul (belum berfungsi — Latihan 5)

---

## Latihan 5 — Borang: lengkapkan MVP (baca **dan** tulis)

**Objektif:** Borang untuk menambah permohonan baharu — model binding + validation. Ini menjadikan MVP kita interaktif.

### Langkah

1. **Tambah dua action** ke `PermohonanController` (dalam kelas yang sama, selepas `Index`):

```csharp
    // GET /Permohonan/Baharu — papar borang kosong
    [HttpGet]
    public IActionResult Baharu() => View(new Permohonan());

    // POST /Permohonan/Baharu — terima borang yang dihantar
    [HttpPost]
    public IActionResult Baharu(Permohonan permohonan)
    {
        if (!ModelState.IsValid)
            return View(permohonan);        // ada ralat — papar semula borang

        _senarai.Add(permohonan);           // simpan (dalam memori)
        return RedirectToAction(nameof(Index));
    }
```

   Dua kaedah nama sama, dibezakan oleh `[HttpGet]` / `[HttpPost]`. ASP.NET Core **model binding** mengisi objek `permohonan` secara automatik dari medan borang yang namanya sepadan.

2. **Cipta `Views/Permohonan/Baharu.cshtml`:**

```cshtml
@model HelloNres.Models.Permohonan
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

   - `asp-for="Rujukan"` menjana `<label>`/`<input>`/mesej ralat yang betul dari Model — termasuk `[Display(Name = "No. Rujukan")]` yang kita tetapkan.
   - `asp-validation-for` memaparkan ralat `[Required]` untuk medan itu.

3. **Jalankan dan uji:**
   - Layari `/Permohonan`, klik **+ Permohonan baharu**.
   - Hantar borang **kosong** → anda sepatutnya nampak mesej "The Rujukan field is required." (validation sisi pelayan berfungsi).
   - Isi ketiga-tiga medan, klik **Simpan** → anda kembali ke senarai dengan baris baharu anda di bawah.

4. **Bukti ia MVP (dalam memori sahaja):** hentikan aplikasi (`Ctrl+C`), jalankan semula, layari `/Permohonan`. Baris tambahan anda **hilang** — hanya 3 seed kembali. Itu dijangka: tiada pangkalan data lagi. **Hari 3 menambah EF Core** dan data mula kekal.

### ✅ Semakan

- [ ] Borang kosong menunjukkan ralat validation, tidak menambah apa-apa
- [ ] Borang sah menambah baris dan kembali ke senarai
- [ ] Selepas mula semula, tambahan hilang — dan anda boleh terangkan **kenapa**
- [ ] Anda boleh namakan tiga bahagian yang anda tulis: Model, View(s), Controller actions

---

## Latihan 6 — Simpan kerja + pratonton Claude Code

**Objektif:** Rakam MVP dalam Git, kemudian lihat apa yang AI akan lakukan dengan tugas yang sama — dan di mana sempadannya.

### Bahagian A — `git init` (masih dengan tangan)

1. Dalam folder projek:

```bash
cd ~/latihan/HelloNres
git init
```

2. Template MVC sudah menyertakan `.gitignore` yang betul (mengabaikan `bin/`, `obj/`). Sahkan:

```bash
git status        # tidak sepatutnya menyenaraikan bin/ atau obj/
```

3. Commit pertama:

```bash
git add .
git commit -m "MVP: papan permohonan NRES (senarai + borang, dalam memori)"
```

> Ingat: repo latihan ini **berasingan** daripada repo kursus. Jangan push ke `nres-bpm`.

### Bahagian B — Pratonton Claude Code (perbincangan, 15 minit)

Sekarang, dan hanya sekarang, kita lihat AI. Jurulatih memandu di skrin (peserta memerhati):

1. Dalam Claude Code, cuba satu arahan seperti:

   > *"Dalam projek MVC ini, tambah action `Butiran(string rujukan)` pada `PermohonanController` yang memaparkan satu permohonan, dan View `Butiran.cshtml` yang sepadan. Guna corak yang sama seperti `Index`."*

2. **Nilai output bersama-sama** — inilah sebab kita membina dengan tangan dahulu:
   - Adakah ia meletakkan fail di tempat yang betul (`Views/Permohonan/Butiran.cshtml`)?
   - Adakah `@model` sepadan dengan apa yang action hantar?
   - Adakah ia mereka-reka apa-apa yang tidak wujud (servis, pakej)?
   - Bolehkah anda **membaca** setiap baris dan mengesahkannya betul?

3. **Poin utama:** anda boleh menjawab soalan-soalan itu **hanya kerana** anda membina scaffold sendiri pagi tadi. Itulah tanda aras yang kita bawa ke Hari 3, di mana kerja berbantu-AI bermula secara serius.

### ✅ Semakan

- [ ] `git log` menunjukkan satu commit dengan MVP anda
- [ ] `git status` bersih (tiada `bin/`/`obj/`)
- [ ] Anda telah melihat AI menjana kod MVC dan **menilainya** terhadap yang anda tulis sendiri
- [ ] Anda boleh menyatakan satu perkara yang AI buat betul dan satu perkara yang perlu disemak

---

## Deliverable Hari Ini

- [ ] `dotnet --version` → `10.x` disahkan pada mesin anda
- [ ] Aplikasi MVC (`HelloNres`) yang anda **taip sendiri**: Model + 2 View + Controller dengan 3 action
- [ ] Borang berfungsi dengan validation; anda faham had "dalam memori" sesuatu MVP
- [ ] Commit Git pertama bagi MVP
- [ ] Anda boleh terangkan gelung Model-View-Controller kepada rakan **tanpa** melihat nota

## Bermula Hari 3

Hari 3 memulakan projek **sebenar** dari kosong (`Nres.Onboarding.Web` / repo `nres-bpm`) — kali ini dengan EF Core, Identity, servis kongsi, dan pembantu AI. Scaffold `HelloNres` hari ini ialah latihan: buang folder `~/latihan/HelloNres` bila anda selesa, atau simpan sebagai rujukan peribadi. Yang penting bukan fail itu — ia **kemahiran** dan **model mental** yang anda bawa ke Hari 3.
