# Rujukan Buku — *C# 14 and .NET 10*

Buku rujukan rasmi kursus ini:

> **C# 14 and .NET 10 — Modern Cross-Platform Development Fundamentals**, Tenth Edition
> Mark J. Price · Packt Publishing, November 2025 · ISBN 978-1-83620-663-7
>
> **Repo kod:** [github.com/habibtalib/cs14net10](https://github.com/habibtalib/cs14net10)
> *(salinan repo rasmi [markjprice/cs14net10](https://github.com/markjprice/cs14net10))*

Buku ini **bukan** wajib dibeli untuk mengikuti kursus — semua lab lengkap dengan sendirinya. Ia rujukan untuk peserta yang mahu mendalami sesuatu topik, dan sumber yang jurulatih rujuk semasa menyediakan bahan.

---

## Beza skop: buku vs kursus ini

| | Buku | Kursus DOTNET-NRES-15 |
|---|------|------------------------|
| Fokus web | **Blazor** (Bab 13–14) + Web API (Bab 15) | **ASP.NET Core MVC** |
| Struktur | Bab demi bab, bebas | Satu sistem NRES, 4 modul, dibina berkumpulan |
| Data | Northwind (contoh Microsoft) | Domain NRES (sintetik) |
| Kolaborasi | Tidak diliputi | **Teras kursus** — 4 pasukan, satu repo |

**Kesimpulan:** buku ialah rujukan terbaik untuk **C# dan EF Core**. Untuk **MVC**, rujuk Microsoft Docs (dipautkan dalam setiap `README.md` hari) — buku mengutamakan Blazor.

---

## Pemetaan kursus → bab buku

### Fasa 1 — Sesi bersama

| Hari kursus | Topik | Bab buku |
|-------------|-------|----------|
| **Hari 1** | Perancangan, URS, ERD | *(tiada — buku tidak meliputi analisis keperluan)* |
| **Hari 2** | Git, Agile, kolaborasi | *(tiada — di luar skop buku)* |
| **Hari 3** | Persediaan `dotnet`, projek & solution | **Bab 1** — Hello, C#! Welcome, .NET! (m.s. 1) |
| **Hari 3** | C# asas, pemboleh ubah, `nameof` | **Bab 2** — Speaking C# (m.s. 43) |
| **Hari 3** | Aliran kawalan, pengendalian pengecualian | **Bab 3** — Controlling Flow… (m.s. 113) |
| **Hari 3** | Fungsi, ujian unit | **Bab 4** — Writing, Debugging, and Testing Functions (m.s. 173) |
| **Hari 3** | OOP, sifat (property), `record` | **Bab 5** — Building Your Own Types with OOP (m.s. 219) |
| **Hari 3** | Antara muka, warisan, nilai null | **Bab 6** — Implementing Interfaces and Inheriting Classes (m.s. 291) |
| **Hari 3** | `DbContext`, model EF Core, migration | **Bab 10** — Working with Data Using EF Core (m.s. 513) |
| **Hari 3** | DI, `Program.cs`, middleware | **Bab 12** — Introducing Modern Web Development (m.s. 609) |

### Fasa 2 — Trek kumpulan (Hari 4–14)

| Topik | Bab buku |
|-------|----------|
| Mentakrif model EF Core, Fluent API | **Bab 10**, *Defining EF Core models* (m.s. 526) |
| Query EF Core | **Bab 10**, *Querying EF Core models* (m.s. 543) |
| LINQ — asas & amalan | **Bab 11** (m.s. 567) |
| LINQ dengan EF Core | **Bab 11**, *Using LINQ with EF Core* (m.s. 586) |
| `join`, `group by`, lookup *(K2 baris gilir, K1 analitik)* | **Bab 11**, *Joining, grouping, and lookups* (m.s. 596) |
| Fail & stream *(muat naik lampiran)* | **Bab 9** (m.s. 463) |
| Regex *(validation no. plat K2, IC)* | **Bab 8**, *Pattern matching with regular expressions* (m.s. 422) |
| Koleksi | **Bab 8**, *Storing multiple objects in collections* (m.s. 433) |
| EF Core dengan ASP.NET Core | **Bab 13**, *Using EF Core with ASP.NET Core* (m.s. 683) |
| Ujian unit dengan xUnit | **Bab 4**, *Unit testing* (m.s. 201) |

### Fasa 3 — Hari 15

| Topik | Bab buku |
|-------|----------|
| Penerbitan & deployment | **Bab 7**, *Publishing your code for deployment* (m.s. 375) |
| Native AOT | **Bab 7** (m.s. 387) |

---

## Ciri bahasa C# mengikut versi

.NET 10 SDK menggunakan pengkompil **Roslyn 5.0**, yang lalainya ialah **C# 14**. Anda tidak perlu menetapkan `<LangVersion>` — ia automatik.

| .NET SDK | Roslyn | C# lalai |
|----------|--------|----------|
| 8.0 | 4.8 | 12.0 |
| 9.0 | 4.12 | 13.0 |
| **10.0** | **5.0** | **14.0** |

> Senarai penuh ciri setiap versi: [`docs/ch02-features.md`](https://github.com/habibtalib/cs14net10/blob/main/docs/ch02-features.md) dalam repo buku.

### C# 14 (November 2025) — yang kita guna

| Ciri | Guna dalam kursus | Bab buku |
|------|-------------------|----------|
| **`field` keyword** | Sifat dengan validation tanpa medan sokongan (backing field) manual | Bab 5 |
| **Null-conditional assignment** (`?.` di sebelah kiri) | `app.Submission?.ReferenceNo = ...` | Bab 3 |
| **File-based apps** (`dotnet run demo.cs`) | Demo C# pantas pada Hari 3 tanpa projek | Bab 1 |
| **`nameof` diperbaiki** | `nameof` pada generik tak terikat | Bab 2 |
| Partial constructor & event | *(tidak digunakan — untuk penjana kod)* | Bab 5 |
| Extension members | *(tidak digunakan — sifat/ahli statik sambungan)* | Bab 6 |

### C# 13 (November 2024) — yang kita guna

| Ciri | Guna dalam kursus |
|------|-------------------|
| **Collection expressions untuk `params`** | `Roles: ["Applicant", "HrAdmin"]` |
| Partial properties | *(tidak digunakan)* |

### C# 12 (November 2023) — yang kita guna

| Ciri | Guna dalam kursus |
|------|-------------------|
| **Primary constructors** | `public class VehicleService(ApplicationDbContext db)` — di mana-mana |
| Aliasing sebarang jenis | *(tidak digunakan)* |
| Default lambda parameters | *(tidak digunakan)* |

---

## Ciri C# 14 dalam konteks NRES

### `field` keyword — sifat dengan validation

Sebelum C# 14, sifat dengan logik memerlukan medan sokongan yang dinamakan:

```csharp
// Sebelum C# 14 — medan sokongan manual
private string _plateNumber = string.Empty;
public string PlateNumber
{
    get => _plateNumber;
    set => _plateNumber = value.Trim().ToUpperInvariant();
}
```

C# 14 memberi anda `field` — pengkompil mencipta medan sokongan:

```csharp
// C# 14
public string PlateNumber
{
    get => field;
    set => field = value.Trim().ToUpperInvariant();
}
```

> ⚠️ **Amaran untuk entiti EF Core:** EF Core memetakan sifat kepada lajur. `field` berfungsi, tetapi menormalkan nilai **dalam setter** menyembunyikan transformasi daripada sesiapa yang membaca entiti. Dalam kursus ini kita **kekal** dengan normalisasi eksplisit dalam servis (`Vehicle.Normalize`) supaya peserta melihat transformasi itu berlaku. Guna `field` untuk view model dan sifat bukan-EF.

### Null-conditional assignment

```csharp
// Sebelum C# 14
if (app.Submission is not null)
    app.Submission.ReferenceNo = rujukan;

// C# 14
app.Submission?.ReferenceNo = rujukan;
```

Sebelah kanan **tidak dinilai** jika sebelah kiri null — jadi ia selamat walaupun `rujukan` ialah panggilan kaedah yang mahal.

### File-based apps — demo tanpa projek

Berguna untuk Hari 3 apabila menerangkan konsep C# tanpa overhead `dotnet new`:

```bash
# demo-linq.cs — satu fail, tiada .csproj
dotnet run demo-linq.cs
```

```csharp
#:package Microsoft.EntityFrameworkCore.Sqlite@10.0.0

var nombor = new[] { 3, 9, 4, 1, 8 };
Console.WriteLine(string.Join(", ", nombor.Where(n => n > 3).OrderBy(n => n)));
```

Peserta boleh mencuba idea dalam beberapa saat, kemudian kembali ke projek sebenar.

---

## Cara menggunakan repo buku

```bash
git clone https://github.com/habibtalib/cs14net10.git
cd cs14net10/code
```

| Folder | Kandungan |
|--------|-----------|
| `code/Chapter01` … `Chapter15` | Solution setiap bab |
| `docs/` | Apendiks, errata, rujukan arahan |
| `docs/ch02-features.md` | Jadual ciri bahasa C# |
| `docs/command-lines.md` | Semua arahan CLI yang digunakan buku |

**Bab paling berguna untuk kursus ini:** `Chapter10` (EF Core), `Chapter11` (LINQ), `Chapter04` (ujian unit).

> **Nota AI:** jika anda meminta pembantu AI menjelaskan sesuatu ciri C#/EF Core, rujuk buku ini secara eksplisit — ia mengurangkan risiko AI mencampurkan idiom .NET Framework lama dengan .NET 10. Rujuk juga [`../AGENTS.md`](../AGENTS.md).

---

## Errata

Buku mempunyai halaman errata dalam repo: [`docs/errata`](https://github.com/habibtalib/cs14net10/tree/main/docs). Semak sebelum melaporkan "kod buku tidak berfungsi".
