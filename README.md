# Coaching .NET 15 Hari — Sistem Dalaman NRES 🏛️💻

Bahan **Latihan Secara *Coaching* Pembangunan Sistem Onboarding & Khidmat Dalaman NRES Menggunakan ASP.NET Core (.NET 10)** — disediakan untuk **Kementerian Sumber Asli & Kelestarian Alam (NRES)**. Kod kursus **DOTNET-NRES-15**.

Nota dalam **Bahasa Melayu**; kod, nama kelas & istilah teknikal dalam **Bahasa Inggeris** (amalan standard .NET). Sepanjang 15 hari, peserta membina **satu** aplikasi ASP.NET Core MVC — `Nres.Onboarding.Web` — yang menyatukan **5 modul** permohonan & aliran kerja kelulusan, dibina **dari kosong secara hands-on**, berperingkat, supaya faham setiap baris.

> 📅 **Aturcara rasmi:** lihat [`JADUAL.md`](./JADUAL.md) — 46 sesi merentas 15 hari, waktu sebenar, deliverable. **Modul ini mengikut aturcara tersebut.**
>
> 📐 **Kanun teknikal tunggal:** [`SPEC-KURSUS.md`](./SPEC-KURSUS.md) — nama entiti, enum status, peranan, prefix nombor rujukan. Semua lab mematuhinya.

> **Kenapa ini penting untuk NRES:** urusan lapor diri pekerja baharu, pas & pelekat kenderaan, akaun ID/AD/email, pematuhan Kod Setia (PKS), dan pinjaman aset ICT selalunya berselerak antara borang kertas, e-mel, dan hamparan. Menyatukannya dalam **satu sistem aliran kerja** memberi **jejak audit**, **status telus**, **kelulusan berperanan**, dan **laporan** — dan peserta belajar **corak boleh guna semula** yang sama untuk mana-mana borang NRES lain.

## Projek: `Nres.Onboarding.Web`

Satu aplikasi ASP.NET Core MVC yang, menjelang Hari 15, boleh:

- **Tangkap data berstruktur** untuk kelima-lima modul NRES
- **Simpan draf** & **hantar** permohonan dengan **nombor rujukan**
- **Muat naik lampiran** sokongan (disimpan selamat di luar `wwwroot`)
- **Halakan** permohonan melalui **semakan & kelulusan** berperanan
- **Jejak status** & nombor rujukan
- **Rekod audit log** untuk setiap tindakan penting
- **Cetak / eksport** ringkasan permohonan (Razor print + CSV)
- Kuatkuasa **validation, authentication, role-based authorization** & amalan **deployment** asas

## 5 Modul (Kes Guna NRES)

| # | Modul | Kes guna |
|---|-------|----------|
| 1 | **Lapor Diri** | Pengurusan permohonan laporan diri pekerja baharu (maklumat peribadi, PCB, akuan OSA, Surat Aku Janji) |
| 2 | **Pas, Parking & Pelekat Kenderaan** | Pengurusan akses kawasan & kenderaan (pas keselamatan, pelekat, parkir khas) |
| 3 | **ID, AD & Email** | Pengurusan permohonan akaun pengguna sistem (AD, email, kemas kini/nyahaktif, akses sistem) |
| 4 | **PKS (Pematuhan Kod Setia)** | Pengisytiharan & pemantauan pematuhan polisi (checklist berpaksikan versi polisi) |
| 5 | **Aset ICT** | Pengurusan permohonan & pinjaman aset ICT (perisian, pinjaman, pemulangan, inventori) |

Setiap modul mengikut **corak aliran kerja yang sama** — belajar sekali, ulang lima kali:

```text
Form → Validation → Draft → Submit → Review → Approve/Reject → Audit → Report
```

## Ringkasan Kursus

| Fasa | Hari | Modul | Hasil |
|------|------|-------|-------|
| **Asas** | [1](./hari-1/) | Seni bina kongsi | Aplikasi berjalan + entiti kongsi + migration |
| **Modul 1** | [2](./hari-2/)–[3](./hari-3/) | Lapor Diri | Borang → validation → lampiran → submit → semakan HR → audit |
| **Modul 2** | [4](./hari-4/)–[6](./hari-6/) | Pas/Parking/Pelekat | 3 jenis permohonan + `Vehicle` + duplicate check + kelulusan + cetakan |
| **Modul 3** | [7](./hari-7/)–[9](./hari-9/) | ID/AD/Email | Rantaian kelulusan berbilang langkah + authorization + notifikasi |
| **Modul 4** | [10](./hari-10/)–[12](./hari-12/) | PKS | Checklist dinamik + kunci declaration + CSV export |
| **Modul 5** | [13](./hari-13/)–[14](./hari-14/) | Aset ICT | Inventori + transaksi selamat (loan/return) |
| **Integrasi** | [15](./hari-15/) | Semua | Integrasi + ujian xUnit + deployment + demo capstone |

> **Nota:** Setiap hari **membina di atas** hari sebelumnya (kumulatif). Prinsip sepanjang kursus: **corak sama, ulang 5 kali** — sekali peserta faham `Form → Draft → Submit → Review → Audit`, mereka boleh lanjutkan sistem ke mana-mana borang NRES tanpa mula dari kosong.

## Nota Konsep (Latar Belakang)

Folder [`nota/`](./nota/) mengandungi nota konsep ringkas Bahasa Melayu — baca sebelum/sepanjang lab:

- [**Persediaan Persekitaran .NET 10**](./nota/00-setup-dotnet.md) 🛠️ — pasang SDK, IDE, sahkan `dotnet --info` *(baca dahulu sebelum Hari 1)*
- [**Kenapa ASP.NET Core MVC?**](./nota/01-kenapa-aspnet-mvc.md) — MVC, kitaran permintaan, bila guna Razor Pages/Web API
- [**EF Core & Migrations**](./nota/02-efcore-migrations.md) — DbContext, entiti, `migrations add`/`database update`
- [**Corak Aliran Kerja NRES**](./nota/03-corak-workflow.md) — `Submission` induk, `SubmissionStatus`, kenapa dikongsi
- [**Validation & View Models**](./nota/04-validation-viewmodels.md) — DataAnnotations, server-side, view model vs entiti
- [**Identity, Roles & Authorization**](./nota/05-identity-authorization.md) 🔒 — pengguna, peranan, `[Authorize]`, policy
- [**Muat Naik Fail Selamat**](./nota/06-file-upload.md) 📎 — simpan di luar `wwwroot`, validasi saiz/jenis, nama fail selamat
- [**Ujian dengan xUnit**](./nota/07-testing-xunit.md) ✅ — unit vs integration, EF Core in-memory/SQLite
- [**Deployment**](./nota/08-deployment.md) 🚀 — SQLite→SQL Server, HTTPS, IIS/Linux/kontena, senarai semak
- [**Keselamatan**](./nota/09-keselamatan.md) 🛡️ — jangan simpan kata laluan, validasi input, authorization di controller, audit

## Prasyarat Peserta

- Biasa dengan **C# asas & OOP** (class, property, method, interface)
- Faham konsep pangkalan data relasi (table, kunci)
- **Tiada pengalaman ASP.NET Core diperlukan** — dibina dari asas

## Keperluan Sistem (Per Peserta)

- **Windows 10/11, macOS, atau Linux**
- **[.NET 10 SDK](https://dotnet.microsoft.com/download)** (`dotnet --version` → `10.x`)
- **Visual Studio 2022 (17.12+)** *(atau)* **VS Code + C# Dev Kit**
- Minimum **8GB RAM**, **5GB+** ruang cakera kosong
- Pelayar web moden (Chrome/Edge/Firefox)

> **Pengesahan:** Selepas pasang, jalankan `dotnet --info`. Langkah penuh ada dalam [`nota/00-setup-dotnet.md`](./nota/00-setup-dotnet.md).

## Susunan Teknologi (Tech Stack)

| Lapisan | Teknologi |
|---------|-----------|
| Rangka web | **ASP.NET Core MVC** (.NET 10 LTS) |
| ORM | **Entity Framework Core 10** |
| Pangkalan data (latihan) | **SQLite** |
| Pangkalan data (pengeluaran) | SQL Server / PostgreSQL |
| Authentication | **ASP.NET Core Identity** |
| Authorization | Role-based + policy |
| Laporan | Razor print view + CSV export |
| Ujian | **xUnit** |

## Deliverable Latihan

- **Aplikasi `Nres.Onboarding.Web`** dibina kumulatif — projek rujukan penuh di [`projek/`](./projek/) (untuk **banding** selepas cuba sendiri)
- **Nota konsep** setiap hari (`hari-*/README.md`)
- **Lab hands-on** langkah demi langkah setiap hari (`hari-*/snippets/lab.md`) — *bahagian paling penting kursus*
- **Nota penceramah** setiap hari (`hari-*/nota-penceramah.md`)
- **Slaid pembentangan** — dek `slides/dotnet-nres-training.html` (self-contained) + `.pptx` (boleh edit)
- **Templat kod** ([`templates/`](./templates/)) — snippet boleh guna semula (entiti, servis, view)

## Sasaran Peserta (NRES)

Pegawai Teknologi Maklumat · Penganalisis Sistem · Pembangun aplikasi dalaman · Pasukan Transformasi Digital · Juruteknik yang menyokong sistem borang & aliran kerja jabatan.

## Cara Mula

1. Baca [`nota/00-setup-dotnet.md`](./nota/00-setup-dotnet.md) & sahkan `dotnet --info` **sebelum** Hari 1.
2. Mula [Hari 1](./hari-1/) — baca `README.md` (konsep), kemudian buat [`hari-1/snippets/lab.md`](./hari-1/snippets/lab.md) (hands-on).
3. Bina projek anda sendiri dari kosong; rujuk [`projek/`](./projek/) hanya untuk **banding**.

---

> ⚠️ **Penafian data contoh:** Semua data NRES dalam kursus (jabatan, aset, pekeliling PKS, senarai kenderaan) adalah **contoh sintetik untuk latihan sahaja** — bukan rekod rasmi. Untuk penggunaan sebenar, gantikan dengan data sah & terkini NRES, dan lalui semakan keselamatan jabatan.
