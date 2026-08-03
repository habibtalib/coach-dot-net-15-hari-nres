# Kumpulan 1 · Hari 4 — Skema DB & Borang Draf Lapor Diri

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)
>
> Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

**Hari pertama Fasa 2.** Anda kini bekerja pada cabang `kump-1/lapor-diri`, dalam folder anda sendiri, dengan asas kongsi Hari 3 sedia untuk digunakan.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `IEntityTypeConfiguration<T>` | [learn.microsoft.com/ef/core/modeling](https://learn.microsoft.com/en-us/ef/core/modeling/) |
| Hubungan satu-ke-satu | [learn.microsoft.com/ef/core/modeling/relationships/one-to-one](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-one) |
| View model & DataAnnotations | [learn.microsoft.com/aspnet/core/mvc/models/validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation) |
| Tag helper borang | [learn.microsoft.com/aspnet/core/mvc/views/working-with-forms](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms) |
| Migration | [learn.microsoft.com/ef/core/managing-schema/migrations](https://learn.microsoft.com/en-us/ef/core/managing-schema/migrations/) |

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 9.00 – 9.25 | Stand-up · `git pull --rebase origin master` · semakan silang AI |
| **9.25 – 1.00 tgh** | **Entiti & skema** — `OfficerReportingApplication`, konfigurasi, pendaftaran modul, descriptor navigasi, migration. 💻 Lab 1–4 |
| **2.30 – 4.30 petang** | **Borang draf** — view model, controller, Razor view, simpan draf. 💻 Lab 5–7 |
| 4.30 – 5.00 | Code review + PR + **gabungan latihan ke `master`** |

**Hasil:** Jadual `OfficerReportingApplications` wujud; borang lapor diri boleh dicipta, disunting, dan disimpan sebagai draf; modul muncul dalam navigasi.

---

## Kenapa jadual detail, bukan satu jadual besar

Permohonan lapor diri anda **bukan** jadual bebas. Ia jadual **detail** yang memaut ke `Submission` induk:

```text
Submissions                      OfficerReportingApplications
├── Id                    ←──────── SubmissionId (FK, unik)
├── ReferenceNo  LD-2026-0001      ├── FullName
├── ModuleCode   "LD"              ├── IdentityNo
├── ApplicantUserId                ├── ReportingDate
├── Status       Draft             ├── PreviousAgency
└── CreatedAt                      └── EmergencyContact
```

**Apa yang TIDAK masuk ke jadual anda:** `ReferenceNo`, `Status`, `ApplicantUserId`, `SubmittedAt`. Kesemuanya sudah ada dalam `Submission`. Menyalinnya bermakna dua sumber kebenaran — dan satu hari ia akan berbeza.

Hubungan itu **satu-ke-satu**: setiap permohonan lapor diri mempunyai tepat satu `Submission`, dikuatkuasakan oleh indeks unik pada `SubmissionId`.

## Kenapa `Draft` membenarkan data tidak lengkap

Ini keputusan reka bentuk yang bertentangan dengan naluri kebanyakan pembangun.

Seorang pekerja baharu membuka borang lapor diri, mengisi separuh, dan perlu mencari salinan surat tawarannya. Jika kita menguatkuasakan validation penuh untuk **menyimpan**, mereka kehilangan segalanya.

Jadi: **validation berbeza mengikut niat.**

| Tindakan | Validation |
|----------|-----------|
| Simpan draf | Minimum — cukup untuk mengenal pasti rekod |
| Hantar | Penuh — setiap medan wajib, setiap peraturan |

Kita melaksanakannya dengan **dua kumpulan validation** pada view model yang sama, bukan dua view model. Anda akan melihat coraknya dalam lab.

## Corak modul mendaftar diri — kali pertama anda gunakannya

Hari ini anda mencipta tiga fail yang menyambungkan modul anda ke aplikasi **tanpa menyunting fail kongsi**:

| Fail | Fungsi |
|------|--------|
| `Models/LaporDiri/Configurations/OfficerReportingApplicationConfiguration.cs` | EF Core menemuinya melalui `ApplyConfigurationsFromAssembly()` |
| `Services/LaporDiri/LaporDiriModule.cs` | `AddLaporDiriModule()` — servis modul anda |
| `Models/LaporDiri/LaporDiriModuleDescriptor.cs` | Modul anda muncul dalam navigasi |

**Satu-satunya** perubahan pada fail kongsi ialah menyahkomen satu baris dalam `Program.cs` — dilakukan sekali, hari ini, di bawah pengawasan jurulatih, satu kumpulan pada satu masa.

> ⚠️ **Slot migration bermula hari ini.** Umumkan sebelum menjalankan `dotnet ef migrations add`. Rujuk [`../../KOLABORASI.md`](../../KOLABORASI.md) §5.

## View model vs entiti

Borang anda mengikat **view model**, bukan entiti.

**Kenapa?** Jika borang mengikat entiti secara langsung, penyerang boleh menghantar `Status=AdminApproved` bersama borang dan EF Core akan menyimpannya dengan senang hati. Ini dipanggil **over-posting**, dan ia satu daripada kelemahan paling biasa dalam aplikasi MVC.

View model juga menempatkan medan yang bukan milik pangkalan data — dropdown, kotak pengesahan, medan pengiraan.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
