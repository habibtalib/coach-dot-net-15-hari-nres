# Persediaan Persekitaran .NET 10 🛠️

> Nota persediaan **pra-kursus** — selesaikan **sebelum Hari 1**. Rujuk [`../JADUAL.md`](../JADUAL.md) untuk aturcara penuh 15 hari, dan [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) untuk susunan teknologi muktamad.

---

## Apa yang perlu dipasang

| Komponen | Keperluan |
|----------|-----------|
| **.NET 10 SDK** | wajib — `dotnet --version` mesti papar `10.x` |
| IDE | **Visual Studio 2022 (17.12+)** *atau* **VS Code + C# Dev Kit** |
| `dotnet-ef` tool | untuk migration EF Core (dipasang selepas SDK) |
| Ruang cakera | 5GB+ kosong |
| RAM | minimum 8GB |

---

## 1. Pasang .NET 10 SDK

Muat turun dari laman rasmi mengikut sistem operasi anda: **[dotnet.microsoft.com/download/dotnet/10.0](https://dotnet.microsoft.com/download/dotnet/10.0)**

### Windows

1. Muat turun pemasang **Windows x64 SDK** (`.exe`).
2. Jalankan pemasang, ikut *wizard* (Next → Next → Install).
3. Tutup dan buka semula terminal (PowerShell / Command Prompt) supaya `PATH` dikemas kini.

### macOS

Pilihan A — pemasang `.pkg` rasmi (muat turun dari laman di atas), atau Pilihan B — Homebrew:

```bash
brew install --cask dotnet-sdk
```

### Linux (Ubuntu/Debian)

```bash
wget https://dot.net/v1/dotnet-install.sh -O dotnet-install.sh
chmod +x dotnet-install.sh
./dotnet-install.sh --channel 10.0
```

> Panduan penuh ikut *distro* (Ubuntu, Fedora, dsb.): **[learn.microsoft.com/dotnet/core/install/linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux)**

---

## 2. Sahkan pemasangan

Selepas pasang, buka terminal **baharu** dan jalankan:

```bash
dotnet --version
```

Output dijangka:

```text
10.0.100
```

(nombor minor/patch mungkin berbeza, tetapi versi utama **mesti** `10.`)

Untuk maklumat penuh (SDK, runtime, OS, arkitektur) — berguna semasa *troubleshoot* di kelas:

```bash
dotnet --info
```

> **Jika `dotnet` tidak dikenali (`command not found`):** terminal belum dibuka semula selepas pemasangan, atau `PATH` tidak dikemas kini secara automatik — rujuk **[learn.microsoft.com/dotnet/core/tools/dotnet-environment-variables](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-environment-variables)**.

---

## 3. Pasang IDE

Pilih **satu** sahaja:

### Pilihan A — Visual Studio 2022 (17.12+), disyorkan untuk Windows

1. Muat turun dari **[visualstudio.microsoft.com](https://visualstudio.microsoft.com/downloads/)**.
2. Semasa *Installer*, pilih *workload* **"ASP.NET and web development"**.
3. Pastikan versi **17.12 atau lebih baharu** (semak melalui *Help → About*) — versi ini menyokong .NET 10.

### Pilihan B — VS Code + C# Dev Kit (Windows/macOS/Linux)

1. Pasang **[VS Code](https://code.visualstudio.com/)**.
2. Buka *Extensions* (`Ctrl+Shift+X` / `Cmd+Shift+X`), cari dan pasang **"C# Dev Kit"** (oleh Microsoft) — ini automatik memasang lanjutan **C#** asas sekali.
3. Buka mana-mana folder projek `.csproj`; C# Dev Kit akan mengesan SDK secara automatik.

---

## 4. Pasang `dotnet-ef` (EF Core CLI tool)

Diperlukan bermula Hari 1 (migration pertama) — lihat [`02-efcore-migrations.md`](./02-efcore-migrations.md).

```bash
dotnet tool install --global dotnet-ef
```

Sahkan:

```bash
dotnet ef --version
```

> **Jika sudah terpasang versi lama:** naik taraf dengan `dotnet tool update --global dotnet-ef`.

> **Nota PATH (Linux/macOS):** *tool* global dipasang ke `~/.dotnet/tools` — jika `dotnet ef` tidak dikenali, tambah folder itu ke `PATH` (rujuk mesej amaran semasa pemasangan tool, ia akan beritahu laluan tepat).

---

## Senarai Semak Sebelum Hari 1

- [ ] `dotnet --version` papar `10.x`
- [ ] `dotnet --info` berjalan tanpa ralat
- [ ] IDE dipasang (Visual Studio 17.12+ **atau** VS Code + C# Dev Kit)
- [ ] `dotnet ef --version` berjalan tanpa ralat
- [ ] Git terpasang (untuk klon repo templat/rujukan jika berkenaan)

Selepas semua tersemak, anda bersedia untuk [Hari 1](../hari-1/) — mula dengan [`01-kenapa-aspnet-mvc.md`](./01-kenapa-aspnet-mvc.md) untuk konsep seni bina sebelum menaip kod pertama.

---

## Sumber Rasmi

- **[.NET 10 Download](https://dotnet.microsoft.com/download/dotnet/10.0)** — pemasang rasmi semua OS
- **[Install .NET on Linux](https://learn.microsoft.com/en-us/dotnet/core/install/linux)**
- **[dotnet-ef tool reference](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)**
- **[C# Dev Kit for VS Code](https://code.visualstudio.com/docs/csharp/get-started)**
