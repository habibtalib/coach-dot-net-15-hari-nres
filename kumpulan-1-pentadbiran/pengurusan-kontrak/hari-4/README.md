# Kumpulan 1 · Hari 4 — Skema Kontrak, Pihak Terlibat, Milestone & Borang Draf

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../../JADUAL.md`](../../../JADUAL.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md)
>
> Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

**Hari pertama Fasa 2 untuk projek ke-3 Kumpulan 1.** Anda bekerja pada cabang `kump-1/pentadbiran` (dikongsi tiga projek K1), dalam folder modul Kontrak anda sendiri, dengan asas kongsi Hari 3 sedia untuk digunakan.

> **Modul Pengurusan Kontrak** ialah daftar & penjejak **kontrak/perjanjian ICT NRES** — kontrak perolehan seperti storan/sandaran (backup), sokongan teknikal, dan antivirus. Setiap kontrak ada **pihak terlibat** (syarikat) dan **milestone bayaran/penyerahan**. Disemak oleh **`IctAdmin`**, ditadbir oleh **BPM (Bahagian Pengurusan Maklumat)**.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `IEntityTypeConfiguration<T>` | [learn.microsoft.com/ef/core/modeling](https://learn.microsoft.com/en-us/ef/core/modeling/) |
| Hubungan satu-ke-banyak | [learn.microsoft.com/ef/core/modeling/relationships/one-to-many](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many) |
| Ketepatan `decimal` (wang) | [learn.microsoft.com/ef/core/modeling/entity-properties#precision-and-scale](https://learn.microsoft.com/en-us/ef/core/modeling/entity-properties#precision-and-scale) |
| Enum `[Flags]` | [learn.microsoft.com/dotnet/api/system.flagsattribute](https://learn.microsoft.com/en-us/dotnet/api/system.flagsattribute) |
| View model & DataAnnotations | [learn.microsoft.com/aspnet/core/mvc/models/validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation) |

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 9.00 – 9.25 | Stand-up · `git pull --rebase origin master` · semakan silang AI |
| **9.25 – 1.00 tgh** | **Entiti & skema** — `ContractRecord`, `ContractParty`, `ContractMilestone`, enum, konfigurasi, pendaftaran modul, descriptor, migration. 💻 Lab 1–4 |
| **2.30 – 4.30 petang** | **Borang draf** — view model, controller, Razor view, simpan draf. 💻 Lab 5–7 |
| 4.30 – 5.00 | Code review + PR + **gabungan latihan ke `master`** |

**Hasil:** Jadual `ContractRecords`, `ContractParties`, dan `ContractMilestones` wujud; borang pendaftaran kontrak boleh dicipta, disunting, dan disimpan sebagai draf; modul muncul dalam navigasi. Pihak terlibat & jadual milestone penuh datang Hari 5–6.

---

## Tiga entiti, satu graf kontrak

Modul ini mempunyai **tiga** entiti yang berkait — penting memahami peranan setiap satu:

```text
Submission induk (kongsi)
   │  SubmissionId (FK, unik) 1─1
   ▼
ContractRecord                          ← header kontrak
├── ContractNo   "CT250000000029728"    ← no. sistem kontrak SEBENAR
├── FileNo       "NRES.400-5/6/40(S)-7" ← no. fail rasmi
├── Title · ContractType · Amount (RM)
├── EffectiveDate · ExpiryDate · Division
├── IsTerminated (disimpan)             ← penamatan awal
│
├──< ContractParty      (1─banyak)      ← syarikat terlibat
│    └── CompanyName · RegistrationNo (SSM) · Role
│
└──< ContractMilestone  (1─banyak)      ← jadual bayaran/penyerahan
     └── PaymentNo · Amount · Deliverables · DueDate · Status
```

- **`ContractRecord`** ialah jadual **detail** yang memaut ke `Submission` induk (satu-ke-satu), sama seperti `OfficerReportingApplication` (Lapor Diri) dan `ComplianceDeclaration` (PKS). Ia header kontrak.
- **`ContractParty`** — syarikat yang terlibat dalam kontrak (satu kontrak boleh ada beberapa pihak: kontraktor utama, sub-kontraktor, vendor). Satu-ke-banyak dengan `ContractRecord`.
- **`ContractMilestone`** — jadual bayaran/penyerahan (payment milestone). Setiap milestone ada amaun, dokumen penyerahan yang diperlukan (Invois/DO/Surat Warranti/EAT/UAT/FAT), tarikh akhir, dan status. Satu-ke-banyak dengan `ContractRecord`.

**Apa yang TIDAK masuk ke `ContractRecord`:** `ReferenceNo`, `Status` (workflow), `ApplicantUserId`, `SubmittedAt` — kesemuanya milik `Submission`. Menyalinnya bermakna dua sumber kebenaran.

## Dua jenis "nombor" dan dua jenis "status" — jangan keliru

Modul ini ada dua tempat yang mudah mengelirukan. Fahaminya sekarang:

**Nombor:**

| Medan | Contoh | Datang dari | Siapa jana |
|-------|--------|-------------|------------|
| `Submission.ReferenceNo` | `KON-2026-0001` | Sistem onboarding kursus | `IReferenceNumberService` (Hari 5–6) |
| `ContractRecord.ContractNo` | `CT250000000029728` | Sistem kontrak sedia ada NRES | **Ditaip pengguna** |
| `ContractRecord.FileNo` | `NRES.400-5/6/40(S)-7` | Sistem fail rasmi NRES | **Ditaip pengguna** |

`ReferenceNo` ialah nombor rujukan **pendaftaran** kita (dijana pada hantar). `ContractNo` dan `FileNo` ialah pengecam **sedia ada** yang pengguna salin masuk — kita tidak menjananya, kita menyimpannya.

**Status:**

| Status | Nilai | Sifat |
|--------|-------|-------|
| `Submission.Status` (workflow) | `Draft` → `Submitted` → `AdminApproved` | **Disimpan**, alur kerja pendaftaran |
| Kitaran hayat kontrak | `Active` / `ExpiringSoon` / `Expired` / `Terminated` | **Dikira** dari `ExpiryDate` (Hari 10–12) |

Lajur "Status" dalam register *SENARAI KONTRAK AKTIF* NRES merujuk **kitaran hayat kontrak** (aktif / tamat tempoh), bukan status workflow. Kita **mengiranya** dari `ExpiryDate` berbanding tarikh hari ini — bukan menyimpannya — sama seperti PKS mengira "patuh / perlu akui semula". Satu-satunya bahagian yang disimpan ialah `IsTerminated` (penamatan awal, yang tidak boleh dikira dari tarikh). Kita membina pengiraan itu penuh pada Hari 10–12; hari ini kita hanya menyediakan medannya.

> **Kenapa jangan simpan status kitaran hayat?** Kerana ia berubah setiap hari tanpa kontrak itu sendiri diubah — kontrak "Aktif" hari ini menjadi "Tamat Tempoh" esok apabila `ExpiryDate` berlalu. Bendera tersimpan akan menjadi tidak segerak melainkan sesuatu mengemas kininya setiap tengah malam. Kira, jangan simpan.

## Kenapa `Draft` membenarkan data tidak lengkap

Sama seperti dua projek K1 yang lain: **validation berbeza mengikut niat.**

| Tindakan | Validation |
|----------|-----------|
| Simpan draf | Minimum — cukup untuk mengenal pasti rekod (tajuk sahaja) |
| Hantar | Penuh — setiap medan wajib, sekurang-kurangnya satu pihak, jumlah milestone = amaun kontrak |

Satu view model, dua kumpulan validation — bukan dua view model.

## Corak modul mendaftar diri — sama seperti Lapor Diri & PKS

Anda mencipta tiga fail yang menyambungkan modul Kontrak ke aplikasi **tanpa menyunting fail kongsi**:

| Fail | Fungsi |
|------|--------|
| `Models/Kontrak/Configurations/*Configuration.cs` | EF Core menemuinya melalui `ApplyConfigurationsFromAssembly()` |
| `Services/Kontrak/KontrakModule.cs` | `AddKontrakModule()` — servis modul anda |
| `Models/Kontrak/ContractModuleDescriptor.cs` | Modul muncul dalam navigasi untuk peranan yang betul |

**Satu-satunya** perubahan pada fail kongsi ialah menyahkomen satu baris dalam `Program.cs` — sekali sahaja, di bawah pengawasan jurulatih.

> ⚠️ **Slot migration.** Umumkan sebelum menjalankan `dotnet ef migrations add`. Rujuk [`../../../KOLABORASI.md`](../../../KOLABORASI.md) §5.

## Data sintetik sahaja

- **Semua data contoh adalah sintetik.** Format `CT…`, `NRES.400-…`, nama syarikat, dan amaun mesti kelihatan realistik supaya peserta mengenalinya — tetapi **jangan** guna nombor kontrak, nama syarikat, atau amaun NRES sebenar.
- **Jangan simpan kata laluan atau rahsia** dalam mana-mana entiti.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**.
