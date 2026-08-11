# Kumpulan 1 · Hari 4 — Skema Akuan Pematuhan PKS & Borang Draf

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../../JADUAL.md`](../../../JADUAL.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md)
>
> Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

**Hari pertama Fasa 2 untuk projek ke-2 Kumpulan 1.** Anda bekerja pada cabang `kump-1/pentadbiran` (dikongsi tiga projek K1), dalam folder modul PKS anda sendiri, dengan asas kongsi Hari 3 sedia untuk digunakan.

> **PKS = Polisi Keselamatan Siber.** Modul ini ialah **Akuan Pematuhan Polisi Keselamatan Siber** — staf dan kontraktor mengaku patuh kepada Polisi Keselamatan Siber NRES, dikaitkan dengan **versi polisi semasa**, disertai NDA di bawah **Akta Rahsia Rasmi 1972**. Disemak oleh **Pegawai Keselamatan ICT** (`IctSecurityOfficer`), ditadbir oleh **BPM (Bahagian Pengurusan Maklumat)**.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `IEntityTypeConfiguration<T>` | [learn.microsoft.com/ef/core/modeling](https://learn.microsoft.com/en-us/ef/core/modeling/) |
| Hubungan satu-ke-satu | [learn.microsoft.com/ef/core/modeling/relationships/one-to-one](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-one) |
| Indeks unik & ditapis | [learn.microsoft.com/ef/core/modeling/indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes) |
| Data seed (`HasData`) | [learn.microsoft.com/ef/core/modeling/data-seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding) |
| View model & DataAnnotations | [learn.microsoft.com/aspnet/core/mvc/models/validation](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation) |

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 9.00 – 9.25 | Stand-up · `git pull --rebase origin master` · semakan silang AI |
| **9.25 – 1.00 tgh** | **Entiti & skema** — `PolicyVersion`, `ComplianceDeclaration` (staf/kontraktor), konfigurasi, pendaftaran modul, descriptor, migration + seed. 💻 Lab 1–4 |
| **2.30 – 4.30 petang** | **Borang draf** — view model dua varian, controller, Razor view, simpan draf. 💻 Lab 5–7 |
| 4.30 – 5.00 | Code review + PR + **gabungan latihan ke `master`** |

**Hasil:** Jadual `ComplianceDeclarations` dan `PolicyVersions` wujud dengan satu versi polisi semasa diseed; borang akuan pematuhan boleh dicipta (staf atau kontraktor), disunting, dan disimpan sebagai draf; modul muncul dalam navigasi.

---

## Dua entiti, dua tujuan berbeza

Modul ini mempunyai **dua** entiti, dan penting memahami kenapa keduanya wujud:

```text
PolicyVersions                    ComplianceDeclarations
├── Id                     ┌──────── SubmissionId (FK, unik) → Submission induk
├── VersionLabel  "v1.0"   │         ├── DeclarantType  (Staf / Kontraktor)
├── Title                  │         ├── FullName · IcNo · Position · Division
├── EffectiveDate          │         ├── CompanyName · CompanyRegNo  (kontraktor sahaja)
├── IsCurrent     ✓        └──────── PolicyVersionId (FK) → versi yang diakui
└── CreatedAt                        └── NdaAccepted · DeclarationAccepted
```

- **`PolicyVersion`** ialah rekod **versi Polisi Keselamatan Siber**. Ia bukan permohonan — ia data rujukan yang ditadbir BPM. Hanya **satu** versi menjadi *semasa* (`IsCurrent`) pada satu masa.
- **`ComplianceDeclaration`** ialah jadual **detail** yang memaut ke `Submission` induk (satu-ke-satu), sama seperti `OfficerReportingApplication` dalam projek Lapor Diri. Ia **juga** memaut ke `PolicyVersion` yang diakui pemohon.

**Apa yang TIDAK masuk ke `ComplianceDeclaration`:** `ReferenceNo`, `Status`, `ApplicantUserId`, `SubmittedAt` — kesemuanya milik `Submission`. Menyalinnya bermakna dua sumber kebenaran.

## Kenapa akuan mesti memaut ke versi polisi

Ini teras domain PKS. Seorang staf mengaku patuh **kepada versi polisi tertentu** — bukan kepada "polisi" secara abstrak. Apabila BPM menerbitkan versi baharu (Hari 10–12), akuan lama menjadi **usang**: pemiliknya kini perlu **akui semula** terhadap versi baharu.

Kita menangkap ini dengan menyimpan `PolicyVersionId` pada setiap akuan. Kemudian status pematuhan menjadi mudah dikira:

| Keadaan | Makna |
|---------|-------|
| `PolicyVersionId` == versi semasa | **Patuh** |
| `PolicyVersionId` < versi semasa | **Perlu akui semula** |

Hari ini kita hanya membina skema dan draf. Logik "perlu akui semula" datang pada Hari 7–9 dan pencetusnya pada Hari 10–12 — tetapi **rekaan skema hari ini yang membolehkannya**. Jika kita tidak menyimpan versi yang diakui, kita tidak akan pernah tahu siapa yang usang.

## Satu varian entiti, dua bentuk borang

Borang sumber NRES ada **dua varian**: **staf** dan **kontraktor/syarikat**. Kita **tidak** membina dua entiti berasingan — itu akan menduplikasi lapan medan yang sama. Sebaliknya, satu entiti `ComplianceDeclaration` dengan **medan diskriminator** `DeclarantType`:

- **Staf:** `FullName`, `IcNo`, `Position`, `Division`.
- **Kontraktor:** medan yang sama **tambah** `CompanyName`, `CompanyRegNo`.

Medan kontraktor `nullable` pada entiti (kontraktor sahaja mengisinya), tetapi **wajib bersyarat** pada view model apabila varian = kontraktor. Anda akan lihat corak `IValidatableObject` dalam lab — validation yang bergantung pada nilai medan lain.

## Kenapa `Draft` membenarkan data tidak lengkap

Sama seperti projek Lapor Diri: **validation berbeza mengikut niat.**

| Tindakan | Validation |
|----------|-----------|
| Simpan draf | Minimum — cukup untuk mengenal pasti rekod (nama sahaja) |
| Hantar | Penuh — setiap medan wajib, NDA ditanda, versi polisi terkait |

Satu view model, dua kumpulan validation — bukan dua view model.

## Corak modul mendaftar diri — sama seperti Lapor Diri

Anda mencipta tiga fail yang menyambungkan modul PKS ke aplikasi **tanpa menyunting fail kongsi**:

| Fail | Fungsi |
|------|--------|
| `Models/Pks/Configurations/*Configuration.cs` | EF Core menemuinya melalui `ApplyConfigurationsFromAssembly()` |
| `Services/Pks/PksModule.cs` | `AddPksModule()` — servis modul anda |
| `Models/Pks/PksModuleDescriptor.cs` | Modul muncul dalam navigasi untuk peranan yang betul |

**Satu-satunya** perubahan pada fail kongsi ialah menyahkomen satu baris dalam `Program.cs` — sekali sahaja, di bawah pengawasan jurulatih.

> ⚠️ **Slot migration.** Umumkan sebelum menjalankan `dotnet ef migrations add`. Rujuk [`../../../KOLABORASI.md`](../../../KOLABORASI.md) §5.

## Keselamatan ialah tema sebenar di sini

Modul ini membawa NDA di bawah Akta Rahsia Rasmi 1972. Dua peraturan mutlak bermula hari ini dan berterusan sepanjang trek:

- **Jangan simpan kata laluan atau rahsia sebenar** dalam mana-mana entiti — ini titik pengajaran keselamatan.
- **Semua data contoh adalah sintetik.** Jangan guna nama, No. KP, atau nama syarikat NRES sebenar.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**.
