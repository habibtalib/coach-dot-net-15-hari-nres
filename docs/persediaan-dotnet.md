# Persediaan .NET 10 (SDK · EF Core · IDE)

> **Bahan rujukan kursus.** Pasang & sahkan persekitaran .NET **sebelum Hari 3**. Rujukan lab: [`hari-2/snippets/lab.md`](../hari-2/snippets/lab.md) Latihan 7.

## 1. Pasang .NET 10 SDK

- Muat turun: [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) (pilih **.NET 10 SDK**).
- Sahkan:

```bash
dotnet --version   # mesti 10.x
dotnet --info
```

## 2. Alat EF Core

```bash
dotnet tool install --global dotnet-ef
dotnet ef --version
# jika sudah dipasang: dotnet tool update --global dotnet-ef
```

## 3. IDE

- **Visual Studio 2022 (17.12+)** — beban kerja *"ASP.NET and web development"*, **atau**
- **VS Code** + sambungan **C# Dev Kit**.
- Sahkan IDE boleh bina & jalankan projek:

```bash
cd /tmp
dotnet new console -o ujian-persekitaran
cd ujian-persekitaran
dotnet run          # patut cetak "Hello, World!"
cd .. && rm -rf ujian-persekitaran
```

## 4. Sahkan akses Git ke repo kursus

```bash
cd <repo-kursus>
git fetch origin
git branch -r       # patut senaraikan cabang kumpulan
```

## ✅ Sedia bila

- [ ] `dotnet --version` → `10.x`
- [ ] `dotnet ef --version` berjaya
- [ ] `dotnet new console` + `dotnet run` berjaya
- [ ] `git fetch` berfungsi
- [ ] **Rakan sekumpulan menyaksikan** semakan anda (persekitaran rosak = buang masa esok)

> Persediaan Git & identiti: [`persediaan-git.md`](./persediaan-git.md).
