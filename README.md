# Coaching .NET 15 Hari — Sistem Dalaman NRES 🏛️💻

Bahan **Latihan Secara *Coaching* Pembangunan Sistem Onboarding & Khidmat Dalaman NRES Menggunakan ASP.NET Core (.NET 10)** — disediakan untuk **Kementerian Sumber Asli & Kelestarian Alam (NRES)**. Kod kursus **DOTNET-NRES-15**.

Nota dalam **Bahasa Melayu**; kod, nama kelas & istilah teknikal dalam **Bahasa Inggeris** (amalan standard .NET).

Sepanjang 15 hari, peserta dibahagikan kepada **4 kumpulan dedicated** yang membina **satu** aplikasi ASP.NET Core MVC bersama — `Nres.Onboarding.Web` — dalam **satu repositori**, setiap kumpulan pada **cabang Git sendiri**, digabungkan menjadi satu sistem bersepadu pada Hari 15.

> 📅 **Aturcara rasmi:** [`JADUAL.md`](./JADUAL.md) — 3 fasa, waktu sebenar, deliverable setiap hari.
>
> 📐 **Kanun teknikal tunggal:** [`SPEC-KURSUS.md`](./SPEC-KURSUS.md) — nama entiti, enum status, peranan, prefix rujukan, cabang Git.
>
> 🤝 **Kontrak kerja pasukan:** [`KOLABORASI.md`](./KOLABORASI.md) — matriks pemilikan fail, slot migration, PR & code review, Definition of Done.
>
> 🤖 **Konteks AI kongsi:** [`AGENTS.md`](./AGENTS.md) — dibaca oleh pembantu AI **setiap** kumpulan supaya 4 pasukan menjana kod yang serasi.

> **Kenapa ini penting untuk NRES:** urusan lapor diri pekerja baharu, pas & pelekat kenderaan, akaun ID/AD/email, dan pinjaman aset ICT selalunya berselerak antara borang kertas, e-mel, dan hamparan. Menyatukannya dalam **satu sistem aliran kerja** memberi **jejak audit**, **status telus**, **kelulusan berperanan**, dan **laporan** — dan peserta belajar **corak boleh guna semula** yang sama untuk mana-mana borang NRES lain.

---

## Struktur kursus — 3 fasa

```text
FASA 1 — SESI BERSAMA (Hari 1–3)
  Hari 1   Perancangan · Dokumentasi · URS/SRS · ERD · Use Case & Process Flow   (AI-assisted)
  Hari 2   Git & Branching · Agile (GitHub Projects + Jira) · Kolaborasi · Persekitaran
  Hari 3   Refresher .NET · EF Core · Identity/RBAC · Asas kongsi + migration InitialShared

FASA 2 — 4 TREK SELARI (Hari 4–14)
  Kumpulan 1        Kumpulan 2         Kumpulan 3        Kumpulan 4
  Lapor Diri        Pas/Parkir/        ID, AD &          Perisian &
                    Pelekat            Email             Aset ICT
  Blok: Hari 4 · Hari 5–6 · Hari 7–9 · Hari 10–12 · Hari 13–14

FASA 3 — SESI BERSAMA (Hari 15)
  Merge 4 cabang · Papan Pemuka Induk · SIT & UAT pre-check · Demo
```

| Fasa | Hari | Bahan |
|------|------|-------|
| **1 · Bersama** | [1](./hari-1/) · [2](./hari-2/) · [3](./hari-3/) | Perancangan & dokumentasi · Git/Agile/kolaborasi · Refresher .NET + asas kongsi |
| **2 · Trek** | 4 – 14 | [Kumpulan 1](./kumpulan-1-lapor-diri/) · [Kumpulan 2](./kumpulan-2-pas-parkir-pelekat/) · [Kumpulan 3](./kumpulan-3-id-ad-email/) · [Kumpulan 4](./kumpulan-4-perisian-aset-ict/) |
| **3 · Bersama** | [15](./hari-15/) | Integrasi, SIT, demo capstone |

---

## 4 Modul & Kumpulan

| # | Kumpulan | Modul | Kes guna | Prefix |
|---|----------|-------|----------|--------|
| 1 | [Kumpulan 1](./kumpulan-1-lapor-diri/) | **Lapor Diri** | Permohonan laporan diri pekerja baharu — profil, dokumen sokongan, slip akuan, kelulusan HR | `LD` |
| 2 | [Kumpulan 2](./kumpulan-2-pas-parkir-pelekat/) | **Pas, Parkir & Pelekat** | Akses kawasan & keselamatan kenderaan — pas pelawat/staf, pelekat, lot parkir, semakan pendua plat, QR | `PAS` `PKR` `STK` |
| 3 | [Kumpulan 3](./kumpulan-3-id-ad-email/) | **ID, AD & Email** | Akaun pengguna & akses sistem — AD, e-mel rasmi, kelulusan penyelia → ICT, audit log | `ICT-ID` |
| 4 | [Kumpulan 4](./kumpulan-4-perisian-aset-ict/) | **Perisian & Aset ICT** | Katalog aset, lesen perisian, pinjaman & pemulangan, stok masa-nyata, laporan | `SW` `AST-L` `AST-R` |

Setiap modul mengikut **corak aliran kerja yang sama** — belajar sekali, ulang empat kali:

```text
Form → Validation → Draft → Submit → Review → Approve/Reject → Audit → Report
```

Itulah sebabnya asas kongsi (`Submission` induk, `SubmissionStatus`, audit, servis) dibina **sekali sahaja bersama-sama pada Hari 3** — bukan empat kali oleh empat kumpulan.

---

## Yang membezakan kursus ini: kolaborasi 4 pasukan, semuanya dibantu AI

Empat kumpulan menulis kod serentak dalam satu repositori. Tanpa disiplin, dua perkara **pasti** berlaku: konflik gabungan pada fail kongsi, dan empat versi berlainan bagi logik yang sama. Pembantu AI memburukkan kedua-duanya — ia menjana semula apa yang sudah wujud kerana ia tidak nampak kerja kumpulan lain.

Kursus ini menanganinya secara **seni bina**, bukan sekadar peraturan:

- **Fail kongsi tidak disunting** — modul **mendaftar diri** (`AddLaporDiriModule()`), entiti membawa `IEntityTypeConfiguration<T>` sendiri, navigasi didorong data. `Program.cs`, `ApplicationDbContext`, dan `_Layout.cshtml` beku selepas Hari 3.
- **Matriks pemilikan fail** — setiap laluan ada tepat satu pemilik.
- **Slot migration bergilir** — menghalang konflik `ModelSnapshot` yang tidak boleh diselesaikan dengan tangan.
- **Daftar komponen kongsi** — "sudah wujud, jangan tulis semula"; menulis semula = gagal code review.
- **`AGENTS.md` kongsi** — keempat-empat kumpulan menghalakan AI ke fail yang sama, jadi output seragam.
- **Semakan silang AI harian** — 10 minit setiap pagi mengesan pertindihan pada hari ia berlaku, bukan pada Hari 15.
- **Gabungan latihan setiap blok** — `master` sudah mengandungi 4 modul menjelang Hari 15.

Butiran penuh: [`KOLABORASI.md`](./KOLABORASI.md) · [`AGENTS.md`](./AGENTS.md)

---

## Projek: `Nres.Onboarding.Web`

Menjelang Hari 15, aplikasi bersepadu boleh:

- **Tangkap data berstruktur** untuk keempat-empat modul NRES
- **Simpan draf** & **hantar** permohonan dengan **nombor rujukan** automatik
- **Muat naik lampiran** sokongan (disimpan selamat di luar `wwwroot`)
- **Halakan** permohonan melalui **semakan & kelulusan** berperanan
- **Jejak status** merentas modul dengan carian rujukan global
- **Rekod audit log** untuk setiap tindakan penting
- **Jana QR/Barcode**, **Slip Akuan PDF**, dan **eksport CSV/Excel**
- **Papar dashboard** peribadi & pentadbir mengikut peranan
- Kuatkuasa **validation, authentication, RBAC** & amalan **deployment** asas

---

## Nota Konsep (Latar Belakang)

Folder [`nota/`](./nota/) — nota konsep ringkas Bahasa Melayu, baca sebelum/sepanjang lab:

- [**Persediaan Persekitaran .NET 10**](./nota/00-setup-dotnet.md) 🛠️ — pasang SDK, IDE, sahkan `dotnet --info` *(baca sebelum Hari 1)*
- [**Kenapa ASP.NET Core MVC?**](./nota/01-kenapa-aspnet-mvc.md) — MVC, kitaran permintaan, bila guna Razor Pages/Web API
- [**EF Core & Migrations**](./nota/02-efcore-migrations.md) — DbContext, entiti, `migrations add`/`database update`
- [**Corak Aliran Kerja NRES**](./nota/03-corak-workflow.md) — `Submission` induk, `SubmissionStatus`, kenapa dikongsi
- [**Validation & View Models**](./nota/04-validation-viewmodels.md) — DataAnnotations, server-side, view model vs entiti
- [**Identity, Roles & Authorization**](./nota/05-identity-authorization.md) 🔒 — pengguna, peranan, `[Authorize]`, policy
- [**Muat Naik Fail Selamat**](./nota/06-file-upload.md) 📎 — simpan di luar `wwwroot`, validasi saiz/jenis, nama fail selamat
- [**Ujian dengan xUnit**](./nota/07-testing-xunit.md) ✅ — unit vs integration, EF Core in-memory/SQLite
- [**Deployment**](./nota/08-deployment.md) 🚀 — SQLite→SQL Server, HTTPS, IIS/Linux/kontena, senarai semak
- [**Keselamatan**](./nota/09-keselamatan.md) 🛡️ — jangan simpan kata laluan, validasi input, authorization di controller, audit
- [**Rujukan Buku**](./nota/10-rujukan-buku.md) 📘 — pemetaan kursus → bab *C# 14 and .NET 10* (Mark J. Price), ciri C# 12/13/14 yang digunakan

---

## Prasyarat Peserta

- Biasa dengan **C# asas & OOP** (class, property, method, interface)
- Faham konsep pangkalan data relasi (table, kunci)
- **Tiada pengalaman ASP.NET Core diperlukan** — dibina dari asas
- **Tiada pengalaman Git diperlukan** — diajar penuh pada Hari 2

## Keperluan Sistem (Per Peserta)

- **Windows 10/11, macOS, atau Linux**
- **[.NET 10 SDK](https://dotnet.microsoft.com/download)** (`dotnet --version` → `10.x`)
- **Visual Studio 2022 (17.12+)** *(atau)* **VS Code + C# Dev Kit**
- **Git** + akaun **GitHub** (akses ke repo kursus)
- Minimum **8GB RAM**, **5GB+** ruang cakera kosong
- Pelayar web moden (Chrome/Edge/Firefox)

> **Pengesahan:** Selepas pasang, jalankan `dotnet --info`. Langkah penuh: [`nota/00-setup-dotnet.md`](./nota/00-setup-dotnet.md).

## Susunan Teknologi (Tech Stack)

| Lapisan | Teknologi |
|---------|-----------|
| Bahasa | **C# 14** (lalai .NET 10 SDK) |
| Rangka web | **ASP.NET Core MVC** (.NET 10 LTS) |
| ORM | **Entity Framework Core 10** |
| Pangkalan data (latihan) | **SQLite** |
| Pangkalan data (pengeluaran) | SQL Server / PostgreSQL |
| Authentication | **ASP.NET Core Identity** |
| Authorization | Role-based + policy |
| Laporan | Razor print view · CSV · PDF (QuestPDF) · Excel (ClosedXML) |
| QR / Barcode | QRCoder |
| Ujian | **xUnit** |
| Kawalan versi | Git + GitHub (1 repo, 4 cabang kumpulan) |
| Pengurusan kerja | GitHub Projects (hands-on) + Jira (demo & pemetaan) |

## Deliverable Latihan

- **Aplikasi `Nres.Onboarding.Web`** bersepadu 4 modul — projek rujukan penuh di [`projek/`](./projek/) (untuk **banding** selepas cuba sendiri)
- **Dokumentasi projek** — URS, use case, process flow, ERD setiap modul (Hari 1)
- **Nota konsep** (`README.md`), **lab hands-on** (`snippets/lab.md` — *bahagian paling penting*), dan **nota penceramah** (`nota-penceramah.md`) bagi setiap sesi bersama & blok trek
- **Kontrak kerja pasukan** — [`KOLABORASI.md`](./KOLABORASI.md) & [`AGENTS.md`](./AGENTS.md)
- **Slaid pembentangan** — dek `slides/dotnet-nres-training.html` (self-contained) + `.pptx`
- **Templat kod** ([`templates/`](./templates/)) — snippet boleh guna semula

## Sasaran Peserta (NRES)

Pegawai Teknologi Maklumat · Penganalisis Sistem · Pembangun aplikasi dalaman · Pasukan Transformasi Digital · Juruteknik yang menyokong sistem borang & aliran kerja jabatan.

## Cara Mula

1. Baca [`nota/00-setup-dotnet.md`](./nota/00-setup-dotnet.md) & sahkan `dotnet --info` **sebelum** Hari 1.
2. Mula [Hari 1](./hari-1/) — baca `README.md` (konsep), kemudian buat `snippets/lab.md` (hands-on).
3. Selepas Hari 3, ikut trek kumpulan anda; baca `KOLABORASI.md` **sebelum** commit pertama pada cabang kumpulan.
4. Bina projek anda sendiri dari kosong; rujuk [`projek/`](./projek/) hanya untuk **banding**.

---

> ℹ️ **Nota skop:** Modul **PKS (Pematuhan Kod Setia)** tidak termasuk dalam kursus ini — cadangan silibus NRES menetapkan 4 kumpulan/modul sahaja. Draf lama disimpan di [`_arkib/`](./_arkib/).
>
> ⚠️ **Penafian data contoh:** Semua data NRES dalam kursus (jabatan, aset, senarai kenderaan) adalah **contoh sintetik untuk latihan sahaja** — bukan rekod rasmi. Untuk penggunaan sebenar, gantikan dengan data sah & terkini NRES, dan lalui semakan keselamatan jabatan.
