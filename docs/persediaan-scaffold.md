# Persediaan: scaffold projek ASP.NET Core MVC (+ NuGet)

> **Bahan rujukan kursus.** Cara scaffold projek MVC .NET 10 dan tambah pakej NuGet piawai. Prasyarat: [`persediaan-dotnet.md`](./persediaan-dotnet.md). Rujukan lab: [`hari-3/snippets/lab.md`](../hari-3/snippets/lab.md) Latihan 0–1.
>
> **Poly-repo:** setiap sistem = repo sendiri; namakan projek mengikut sistem anda, cth `LaporDiri.Web`, `PasParkirPelekat.Web` — bukan satu `Nres.Onboarding.Web` bersama. Rujuk `SPEC-KURSUS.md`.

## 1. Cipta projek MVC

```bash
dotnet new mvc -o <Sistem>.Web        # cth LaporDiri.Web
cd <Sistem>.Web
dotnet run                            # buka https://localhost:7xxx → halaman selamat datang; Ctrl+C henti
```

Struktur dijana: `Controllers/` · `Models/` · `Views/` · `wwwroot/` · `Program.cs` · `appsettings.json` · `<Sistem>.Web.csproj`.

## 2. Pakej NuGet piawai

```bash
dotnet add package Microsoft.EntityFrameworkCore
dotnet add package Microsoft.EntityFrameworkCore.Design
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.AspNetCore.Identity.EntityFrameworkCore
```

| Pakej | Kegunaan |
|-------|----------|
| `EntityFrameworkCore` | Teras ORM |
| `EntityFrameworkCore.Design` | Sokongan `dotnet ef` (migration) |
| `EntityFrameworkCore.Sqlite` | Penyedia SQLite (latihan) |
| `Identity.EntityFrameworkCore` | Identity + storan EF Core |

Pakej mengikut keperluan modul (kemudian): `QuestPDF` (PDF) · `ClosedXML` (Excel) · `QRCoder` (QR — Kumpulan 2).

Sahkan:

```bash
dotnet build
grep PackageReference <Sistem>.Web.csproj    # 4 rujukan
```

## 3. `.gitignore` (.NET)

```gitignore
[Bb]in/
[Oo]bj/
*.db
*.db-shm
*.db-wal
App_Data/uploads/*
!App_Data/uploads/.gitkeep
.vs/
```

## ✅ Sedia bila

- [ ] `dotnet run` memaparkan halaman selamat datang
- [ ] Empat `PackageReference` wujud & `dotnet build` berjaya
- [ ] `git status` **tidak** menunjukkan `bin/`, `obj/`, atau `*.db`

> Prasyarat: [`persediaan-dotnet.md`](./persediaan-dotnet.md). Kanun struktur & nama: `SPEC-KURSUS.md`.
