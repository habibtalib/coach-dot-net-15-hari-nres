# SPEC-KURSUS — Kanun Rujukan Tunggal (Single Source of Truth)

> **Untuk penulis kandungan (manusia & ejen):** Setiap nama kelas, enum, peranan (role), prefix nombor rujukan, nama cabang Git, dan skop harian **MESTI** sepadan dengan dokumen ini. Jangan cipta variasi sendiri. Sumber rasmi skop: `cadangan_silibus_coaching_15hari_NRES.docx` (cadangan silibus NRES). Sumber domain penuh: `coach-nres/nres-dotnet-15-day-coaching-guide.md` (repo jiran).

Kursus: **Latihan Secara *Coaching* Pembangunan Sistem Onboarding & Khidmat Dalaman NRES Menggunakan ASP.NET Core** — 15 hari, hands-on, berpaksikan lab. Kod kursus **DOTNET-NRES-15**.

Seni bina: **poly-repo**. **Setiap modul ialah aplikasi ASP.NET Core (.NET 10) yang berasingan** — repo sendiri, subdomain sendiri, pangkalan data sendiri. **Satu-satunya komponen yang dikongsi ialah Profile DB.** Empat kumpulan latihan memiliki modul masing-masing (Kumpulan 1 memikul **tiga** repo).

---

## Seni bina — MUKTAMAD (poly-repo)

> **Perubahan seni bina (disahkan):** kursus ini **tidak lagi** satu repo / satu aplikasi / satu pangkalan data. Ia kini **poly-repo** — enam sistem `.NET` bebas, diintegrasikan **hanya** melalui **Profile DB** yang dikongsi. Semua kandungan mesti mematuhi model ini; abaikan mana-mana rujukan lama kepada "satu repositori", `Nres.Onboarding.Web`, `master` bersama, `asas/shared-foundation`, atau `InitialShared`.

- **Organisasi GitHub:** [`nres-bpm`](https://github.com/nres-bpm) — satu repo bagi setiap sistem.
- **Setiap sistem** = **repo sendiri + subdomain sendiri + pangkalan data sendiri**, semua **ASP.NET Core MVC / .NET 10 / EF Core 10**.
- **Satu-satunya yang dikongsi:** **Profile DB** (jadual `UserProfile` berpusat). Tiada lagi entiti kongsi merentas sistem — setiap sistem memiliki `Submission`/`Attachment`/`AuditLog`/`ApprovalStep`-nya sendiri dalam DB sendiri.
- **Model akses:**
  - **Lapor Diri = sistem AWAM (public-facing)** — pintu masuk untuk staf baharu; **mencipta** profil pengguna dalam Profile DB.
  - **Semua sistem lain = DALAMAN** — akses staf melalui **SSO**; pengguna **sudah ada** profil dalam Profile DB (sistem **membaca** profil). **Pematuhan PKS** dan **Pengurusan Kontrak** dalaman sepenuhnya (tiada borang luar).
- **Integrasi:** Profile DB juga titik integrasi dengan **sistem sedia ada NRES**.

| Sistem | Repo (`nres-bpm/…`) | Subdomain | Akses | Profile DB |
|--------|---------------------|-----------|-------|------------|
| Lapor Diri | [`lapor-diri`](https://github.com/nres-bpm/lapor-diri) | `lapordiri.` | **Awam** | **Cipta** profil |
| Pematuhan PKS | [`pematuhan-pks`](https://github.com/nres-bpm/pematuhan-pks) | `pks.` | Dalaman (SSO) | Guna profil |
| Pengurusan Kontrak | [`pengurusan-kontrak`](https://github.com/nres-bpm/pengurusan-kontrak) | `kontrak.` | Dalaman (SSO) | Guna profil |
| Pas/Parkir/Pelekat | [`pas-parkir-pelekat`](https://github.com/nres-bpm/pas-parkir-pelekat) | `pas.` | Dalaman (SSO) | Guna profil |
| ID/AD/Email | [`id-ad-email`](https://github.com/nres-bpm/id-ad-email) | `id.` | Dalaman (SSO) | Guna profil |
| Tempahan Fasiliti Sukan | [`tempahan-fasiliti-sukan`](https://github.com/nres-bpm/tempahan-fasiliti-sukan) | `fasiliti.` | Dalaman (SSO) | Guna profil |
| **Profil (dikongsi)** | [`profile`](https://github.com/nres-bpm/profile) | — | Perkhidmatan/skema kongsi | **Sumber** Profile DB |

---

## Model penyampaian — MUKTAMAD

Kursus ini **bukan** satu kohort yang membina semua modul berturutan. Ia mengikut model **4 kumpulan dedicated bekerja selari** seperti ditetapkan cadangan silibus NRES:

| Fasa | Hari | Mod | Kandungan |
|------|------|-----|-----------|
| **Fasa 1** | **1 – 3** | **Sesi bersama** (semua kumpulan) | Perancangan, dokumentasi, URS/SRS, ERD & diagram · Git, branching, Agile & kolaborasi · Refresher .NET + asas kongsi |
| **Fasa 2** | **4 – 14** | **4 trek selari** (setiap kumpulan modulnya sendiri) | Pembangunan modul mengikut kumpulan, dalam **repo masing-masing** (poly-repo) |
| **Fasa 3** | **15** | **Sesi bersama** | Penggabungan kod, Papan Pemuka Induk, SIT & UAT pre-check, demo |

> **Kenapa Fasa 1 tiga hari — MUKTAMAD:** cadangan NRES memperuntukkan Hari 1–2 sahaja untuk sesi bersama. Skop Fasa 1 dikembangkan untuk merangkumi **perancangan projek, dokumentasi, URS, ERD, Agile (Jira/GitHub Projects), kolaborasi pasukan, dan penggunaan AI** — topik ini tidak muat dalam dua hari bersama-sama refresher .NET dan pembinaan asas kongsi.
>
> Keputusan yang **disahkan** ialah **kekal tiga hari sesi bersama**. Kesannya: setiap trek kumpulan mendapat **11 hari** (Hari 4–14) berbanding 12 hari dalam docx; blok terakhir setiap trek dipendekkan dari 3 hari ke 2 hari. Isu ini **ditutup** — jangan tulis kandungan yang mengandaikan trek 12 hari.

### Kenapa kontrak Profile DB mesti siap Hari 3, sebelum kumpulan berpecah ke repo masing-masing

Setiap kumpulan bekerja dalam **repo dan aplikasi berasingan** — jadi **tiada konflik gabungan merentas pasukan** (itulah kelebihan poly-repo). Yang **mesti dipersetujui bersama pada Hari 3** ialah **kontrak Profile DB**: skema jadual `UserProfile`, cara **Lapor Diri mencipta** profil, dan cara sistem lain **membaca** profil (melalui SSO / capaian Profile DB). Jika setiap sistem mentafsir profil secara berlainan, integrasi akan pecah — jadi Profile DB dibina & dipersetujui **sekali** (repo [`profile`](https://github.com/nres-bpm/profile)), dan setiap sistem lain mematuhinya. Selebihnya (`Submission`, `Attachment`, `AuditLog`, corak `SubmissionStatus`) **dibina dalam setiap repo sendiri** mengikut corak kongsi yang sama, tetapi **bukan** pangkalan data yang sama.

---

## Konvensyen bahasa

Nota, penerangan & agenda dalam **Bahasa Melayu**. Semua **kod, nama kelas/pembolehubah, nama fail, istilah teknikal** (`Controller`, `DbContext`, `migration`, `view model`, `pull request`) dikekalkan dalam **Bahasa Inggeris** — amalan standard industri .NET.

## Susunan teknologi (Tech Stack) — MUKTAMAD

| Lapisan | Pilihan |
|---------|---------|
| Bahasa | **C# 14** (lalai bagi .NET 10 SDK — Roslyn 5.0; tiada `<LangVersion>` diperlukan) |
| Rangka web | **ASP.NET Core MVC** (.NET 10 LTS) |
| ORM | **Entity Framework Core 10** |
| Pangkalan data (latihan) | **SQLite** (mula pantas, sifar pemasangan) |
| Pangkalan data (pengeluaran) | SQL Server / PostgreSQL |
| Authentication | **Lapor Diri (awam):** ASP.NET Core Identity · **sistem dalaman:** **SSO** (guna profil Profile DB) |
| Authorization | Role-based + policy (peranan dari Profile DB) |
| Integrasi | **Profile DB** dikongsi (satu-satunya); setiap sistem baca/tulis mengikut kontrak `profile` |
| Storan fail | Folder peribadi luar `wwwroot` (`App_Data/uploads/`) |
| Laporan | Razor print view · CSV export · PDF (QuestPDF) · Excel (ClosedXML) |
| Kod QR / Barcode | **QRCoder** (Kumpulan 2) |
| Ujian | **xUnit** + EF Core SQLite/in-memory |
| Kawalan versi | **Git** + GitHub — **poly-repo**: 6 repo dalam org [`nres-bpm`](https://github.com/nres-bpm), satu repo/sistem |
| Pengurusan kerja | **GitHub Projects** (hands-on) + **Jira** (demo & pemetaan) |
| IDE | Visual Studio 2022 (17.12+) / VS Code + C# Dev Kit |
| SDK | **.NET 10 SDK** (`dotnet --version` → `10.x`) |

> **Nota versi — MUKTAMAD:** cadangan silibus NRES menulis **.NET 8**. Keputusan yang **disahkan** ialah **.NET 10 LTS / EF Core 10 / C# 14**. Semua contoh kod mesti sah untuk .NET 10 (primary constructors, collection expressions, nullable reference types dihidupkan, `dotnet ef` CLI). Isu ini **ditutup** — jangan tulis kandungan yang menyokong dua versi.

> **Buku rujukan rasmi:** *C# 14 and .NET 10 — Modern Cross-Platform Development Fundamentals* (Mark J. Price, Packt, Nov 2025) · repo kod [github.com/habibtalib/cs14net10](https://github.com/habibtalib/cs14net10). Pemetaan penuh kursus → bab, dan senarai ciri C# 12/13/14 yang digunakan: [`nota/10-rujukan-buku.md`](./nota/10-rujukan-buku.md). Buku **tidak wajib** — lab lengkap dengan sendirinya. Ambil perhatian buku mengutamakan **Blazor**, bukan MVC; untuk MVC rujuk Microsoft Docs.

> **Kenapa SQLite untuk latihan:** peserta boleh mula tanpa memasang SQL Server. Tukar penyedia (provider) ke SQL Server hanya dengan menukar `UseSqlite` → `UseSqlServer` + connection string. Ditunjukkan pada Hari 15.

---

## Modul & Kumpulan — MUKTAMAD

Empat kumpulan dedicated. **Kumpulan 1 membina tiga projek** (tiga repo); kumpulan lain satu trek modul (satu repo). Setiap modul ialah **repo / aplikasi / pangkalan data sendiri** dan mengikut corak aliran kerja yang sama (`Submission` induk, `SubmissionStatus`, audit) — tetapi dalam **DB sendiri**, bukan dikongsi. Pemetaan repo & subdomain: lihat jadual **Seni bina** di atas.

| Kumpulan | Modul | Prefix | Admin peranan | Trek |
|----------|-------|--------|---------------|------|
| **Kumpulan 1** | **Lapor Diri** — laporan diri pekerja baharu | `LD` | `HrAdmin` | `kumpulan-1-pentadbiran/lapor-diri/` |
| **Kumpulan 1** | **Pematuhan PKS** — Akuan Pematuhan **Polisi Keselamatan Siber** + NDA Akta Rahsia Rasmi 1972 | `PKS` | `IctSecurityOfficer` | `kumpulan-1-pentadbiran/pematuhan-pks/` |
| **Kumpulan 1** | **Pengurusan Kontrak** — daftar & jejak kontrak/perjanjian | `KON` | `IctAdmin` | `kumpulan-1-pentadbiran/pengurusan-kontrak/` |
| **Kumpulan 2** | **Pas Bangunan, Parkir & Pelekat Kenderaan** — akses kawasan & keselamatan kenderaan | `PAS` · `PKR` · `STK` | `SecurityAdmin` | `kumpulan-2-pas-parkir-pelekat/` |
| **Kumpulan 3** | **ID, AD & Email** — akaun pengguna & akses sistem (kelulusan 2 peringkat) | `ICT-ID` | `Supervisor` → `IctAdmin` | `kumpulan-3-id-ad-email/` |
| **Kumpulan 4** | **Tempahan Fasiliti Sukan** — tempahan gelanggang & kemudahan sukan | `TFS` | `FacilityAdmin` | `kumpulan-4-tempahan-fasiliti-sukan/` |

> **Beban Kumpulan 1:** kerana K1 memikul 3 projek berbanding 1 bagi kumpulan lain, agihkan ahli lebih ramai kepada K1 atau kecilkan skop setiap sub-projek supaya rentak selari Fasa 2 kekal seimbang.
>
> **Nota domain (disahkan dari dokumen sumber NRES):**
> - **PKS = Polisi Keselamatan Siber** (bukan "Kod Setia"). Modul ialah *Akuan Pematuhan Polisi Keselamatan Siber* + NDA Akta Rahsia Rasmi 1972. Penyemak: peranan **`IctSecurityOfficer`** (Pegawai Keselamatan ICT). Borang ada varian **staf** dan **kontraktor/syarikat** (`CompanyName`, `CompanyRegNo`). Ditadbir oleh **BPM = Bahagian Pengurusan Maklumat** (bahagian ICT yang juga memiliki ID/Email + Kontrak).
> - **Kumpulan 2 — realiti vs lab:** dalam sistem NRES **sebenar**, K2 ialah **dua sub-sistem** (pas keselamatan; pelekat + peruntukan parkir), dengan **sokongan Ketua Jabatan** sebelum semakan **UPKF**, **parkir diperuntukkan admin (bukan dimohon)**, dan peranan berasingan (UPKF officer / Pentadbir Parkir / Pengawal Keselamatan imbas-sahaja). **Untuk kemudahan LAB**, kursus meringkaskannya kepada satu peranan `SecurityAdmin` + entiti `ParkingApplication`. Jurulatih: nyatakan model sebenar sebagai *"dalam pengeluaran…"*.

---

## Struktur setiap repo (poly-repo — guna sepanjang kursus)

**Setiap sistem = satu repo = satu solution `.NET` bebas.** Contoh (Lapor Diri):

```text
lapor-diri/                       # repo nres-bpm/lapor-diri
  src/
    LaporDiri.Web/                # ASP.NET Core MVC (.NET 10)
      Controllers/
      Data/                       # AppDbContext — DB SENDIRI sistem ini
      Models/                     # entiti SENDIRI: Submission, Attachment, AuditLog,
                                  #   ApprovalStep + OfficerReportingApplication
      ViewModels/
      Services/                   # ReferenceNumberService, FileStorageService, dll. (dalam repo ini)
      Views/
      wwwroot/
      App_Data/uploads/           # fail dimuat naik (bukan bawah wwwroot)
    LaporDiri.Profile/            # klien/kontrak Profile DB (rujuk paket 'profile')
  tests/
    LaporDiri.Tests/              # xUnit
  README.md
```

- **Setiap repo mempunyai versi sendiri** bagi entiti aliran kerja (`Submission`, `Attachment`, `AuditLog`, `ApprovalStep`, `SubmissionStatus`) — **dalam DB sendiri**. Ia bukan lagi jadual dikongsi.
- **`profile`** ([`nres-bpm/profile`](https://github.com/nres-bpm/profile)) menyediakan **skema + kontrak Profile DB** (sebagai pustaka/paket kongsi atau API). Setiap sistem lain **merujuk** kontrak ini untuk baca profil; **Lapor Diri** merujuknya untuk **cipta** profil.

> **Anti-konflik dalam poly-repo:** kerana setiap pasukan bekerja dalam **repo berasingan**, **tiada konflik gabungan merentas pasukan** dan **tiada slot migration bersama**. Titik disiplin berpindah ke **satu tempat**: **kontrak Profile DB** — jangan ubah skema profil tanpa menyelaras dalam repo [`profile`](https://github.com/nres-bpm/profile). Kontrak kolaborasi penuh: [`KOLABORASI.md`](./KOLABORASI.md).

---

## Kolaborasi, anti-konflik & anti-redundan — TERAS KURSUS

Dalam poly-repo, setiap pasukan bekerja dalam **repo berasingan** — jadi risiko **konflik gabungan merentas pasukan hilang**. Tetapi risiko baharu muncul: sistem-sistem **bebas mesti tetap saling faham**. Tanpa disiplin, dua perkara berlaku: (1) **Profile DB ditafsir berlainan** oleh setiap sistem → integrasi pecah, dan (2) **konvensyen berselerak** (nama, `SubmissionStatus`, corak aliran kerja berbeza setiap repo) → sukar diselenggara & diaudit. AI memburukkan (2) — ia menjana gaya berbeza setiap kali melainkan diberi konteks kongsi.

Kursus menangani risiko poly-repo secara **seni bina & kontrak**, bukan sekadar peraturan:

| Risiko poly-repo | Penyelesaian | Diajar |
|------------------|--------------|--------|
| Sistem tafsir Profile DB berlainan | **Kontrak Profile DB tunggal** dalam repo [`profile`](https://github.com/nres-bpm/profile) — skema + pustaka/paket klien yang **dirujuk** semua sistem | Hari 3 |
| Lapor Diri cipta profil; lain-lain baca | Kontrak jelas: **hanya `lapor-diri` menulis** `UserProfile`; sistem dalaman **baca** (via SSO) | Hari 3 |
| Konvensyen berbeza setiap repo | **`SubmissionStatus`, corak aliran kerja, peranan** ditakrif di sini (SPEC) & diikut **setiap** repo — walaupun jadual dalam DB masing-masing | Hari 1–3 |
| AI jana gaya berbeza setiap sistem | **`AGENTS.md`** dimuat dalam **setiap** repo; peraturan *cari dahulu, jana kemudian* dalam repo itu | Hari 1–2 |
| Integrasi silang gagal lewat | **SSO + akaun ujian kongsi**; **SIT Hari 15** menguji aliran rentas sistem melalui Profile DB | Hari 15 |
| Proses tak konsisten antar pasukan | **Satu** Definition of Done, **satu** templat PR, **satu** senarai semak review — dikuatkuasa **dalam setiap repo** | Hari 2 |

**Dua fail kontrak yang mengikat semua pasukan:**

- [`KOLABORASI.md`](./KOLABORASI.md) — kontrak Profile DB, konvensyen kongsi merentas repo, Definition of Done, aliran PR & code review (per repo), protokol perubahan skema profil.
- [`AGENTS.md`](./AGENTS.md) — konteks AI kongsi. **Setiap** repo menyertakan/menghalakan pembantu AI-nya ke fail ini supaya **enam sistem** menjana kod dengan konvensyen, nama, dan struktur yang **sama** — walaupun ia repo berasingan.

> **Peraturan emas:** dalam repo sistem anda sendiri, tulis apa yang perlu (setiap sistem ada `ReferenceNumberService`, panel kelulusan, dll. **sendiri** — itu OK dalam poly-repo). Tetapi **jangan sesekali** ubah skema/kontrak **Profile DB** tanpa menyelaras dalam repo [`profile`](https://github.com/nres-bpm/profile) — itu satu-satunya perkara yang benar-benar dikongsi.

---

## Strategi Git — MUKTAMAD (poly-repo)

**Setiap sistem = repo sendiri** dalam org [`nres-bpm`](https://github.com/nres-bpm). **Tiada** cabang kumpulan bersama, **tiada** merge merentas pasukan. Setiap repo menguruskan cabangnya sendiri:

```text
<repo>  (cth nres-bpm/pas-parkir-pelekat)
main                          # dilindungi; merge melalui PR sahaja
└── feat/<ciri-pendek>        # cabang ciri → PR ke main repo ini
    cth: feat/semakan-pendua-plat
```

- Setiap repo bermula daripada scaffold `.NET` sendiri (Hari 3) dan **merujuk kontrak `profile`**.
- Kerja harian dalam **cabang ciri pendek** → PR ke `main` **repo itu**; `git pull --rebase` setiap pagi **dalam repo sendiri**.
- **Tiada gabungan cabang Hari 15.** Sebaliknya setiap sistem berdiri sendiri; **integrasi diuji melalui Profile DB (SIT Hari 15)** dan setiap sistem boleh di-deploy bebas ke subdomainnya.
- **Repo [`profile`](https://github.com/nres-bpm/profile):** sebarang perubahan skema Profile DB melalui **PR + persetujuan** semua sistem yang bergantung — ini satu-satunya titik penyelarasan merentas pasukan.

### Format commit

```text
<modul>: <apa yang berubah dalam Bahasa Melayu ringkas>

Contoh:
lapor-diri: tambah muat naik lampiran dan metadata Attachment
akses: sekat permohonan pelekat pendua bagi nombor plat sama
```

Jika kumpulan menggunakan Jira, sertakan issue key di hadapan: `NRES-42 lapor-diri: ...`

---

## Enum status — KONGSI SEMUA MODUL

```csharp
public enum SubmissionStatus
{
    Draft = 0,
    Submitted = 1,
    SupervisorApproved = 2,
    AdminApproved = 3,
    Rejected = 4,
    Completed = 5,
    Cancelled = 6
}
```

Corak aliran universal (ulang untuk setiap modul):

```text
Form → Validation → Draft → Submit → Review → Approve/Reject → Audit → Report
```

> Cadangan NRES menyebut status `Under Review` bagi Kumpulan 1. Ia dipetakan kepada `Submitted` (permohonan sudah di tangan admin, belum diputuskan) — **jangan** tambah ahli enum baharu tanpa mengemas kini dokumen ini.

## Peranan (Roles) — KONGSI

| Role | Tanggungjawab | Kumpulan berkaitan |
|------|---------------|--------------------|
| `Applicant` | Cipta draf & hantar permohonan | Semua |
| `Supervisor` | Semak permohonan staf (kelulusan peringkat 1) | 3 (utama) |
| `HrAdmin` | Semak Lapor Diri | 1 |
| `IctSecurityOfficer` | Semak Akuan Pematuhan PKS (Pegawai Keselamatan ICT) | 1 |
| `IctAdmin` | Semak Pengurusan Kontrak; ID/AD/email (peringkat 2) | 1, 3 |
| `SecurityAdmin` | Semak pas, parkir, pelekat kenderaan | 2 |
| `FacilityAdmin` | Semak & luluskan tempahan fasiliti sukan | 4 |
| `SystemAdmin` | Urus pengguna & data lookup | Semua |

> Model K2 sebenar (UPKF / Pentadbir Parkir / Pengawal berasingan) diringkaskan kepada `SecurityAdmin` dalam lab — lihat *Nota domain* di atas.

## Entiti dikongsi vs entiti setiap sistem

- **Dikongsi (Profile DB — repo [`profile`](https://github.com/nres-bpm/profile)):** **`UserProfile`** sahaja (profil pengguna berpusat) + lookup identiti asas jika perlu. **Hanya Lapor Diri menulis; sistem lain membaca.**
- **Dalam setiap repo/DB sendiri (corak sama, jadual berasingan):** `Submission` (induk), `Attachment`, `AuditLog`, `ApprovalStep`, dan lookup setempat (`LookupDepartments`, `LookupGrades`, `LookupPositions`) jika diperlukan sistem itu. Ini **bukan lagi** dikongsi — setiap sistem memilikinya dalam DB sendiri, mengikut corak `SubmissionStatus` yang sama.

## Servis dalam setiap sistem

Setiap repo mempunyai **versi sendiri**: `IReferenceNumberService`, `IFileStorageService`, `IAuditLogService`, `IWorkflowService`, `INotificationService` (guna `ConsoleNotificationService` untuk latihan), `ICurrentUserService`. **Satu-satunya servis kongsi:** klien Profile DB (cth `IProfileService` / paket `profile`) untuk baca/tulis `UserProfile`.

## Prefix nombor rujukan — MUKTAMAD

| Modul | Kumpulan | Prefix | Contoh |
|-------|----------|--------|--------|
| Lapor Diri | 1 | `LD` | `LD-2026-0001` |
| Pematuhan PKS | 1 | `PKS` | `PKS-2026-0001` |
| Pengurusan Kontrak | 1 | `KON` | `KON-2026-0001` |
| Pas Bangunan | 2 | `PAS` | `PAS-2026-0001` |
| Parkir | 2 | `PKR` | `PKR-2026-0001` |
| Pelekat Kenderaan | 2 | `STK` | `STK-2026-0001` |
| ID AD & Email | 3 | `ICT-ID` | `ICT-ID-2026-0001` |
| Tempahan Fasiliti Sukan | 4 | `TFS` | `TFS-2026-0001` |

## Jadual entiti per kumpulan

| DB | Tables |
|----|--------|
| **Profile DB** (repo `profile`, **dikongsi**) | `UserProfiles` (+ lookup identiti asas) |
| **Setiap sistem** (DB sendiri) — corak sama | `Submissions`, `Attachments`, `AuditLogs`, `ApprovalSteps`, `Lookup*` setempat |
| Lapor Diri (DB sendiri) | + `OfficerReportingApplications` |
| Pematuhan PKS (DB sendiri) | + `ComplianceDeclarations` (varian staf & kontraktor: `CompanyName`, `CompanyRegNo`), `PolicyVersions` (versi Polisi Keselamatan Siber) |
| Pengurusan Kontrak (DB sendiri) | + `ContractRecords`, `ContractParties`, `ContractMilestones` |
| Pas/Parkir/Pelekat (DB sendiri) | + `AccessPassApplications`, `ParkingApplications`, `VehicleStickerApplications`, `Vehicles` |
| ID AD & Email (DB sendiri) | + `AccountRequests`, `RequestedSystemAccesses` |
| Tempahan Fasiliti Sukan (DB sendiri) | + `SportsFacilities`, `FacilityBookingApplications`, `FacilityBookingSlots` |

---

## Pemetaan 15 Hari — MUKTAMAD

### Fasa 1 — Sesi bersama (Hari 1–3)

| Hari | Fokus utama |
|------|-------------|
| **1** | Perancangan projek & skop · dokumentasi · **URS/SRS** · **Use Case & Process Flow** · **ERD** — kesemuanya dengan bantuan AI |
| **2** | **Agile** (Jira/GitHub Projects) · **Git & strategi poly-repo** (satu repo/sistem, cabang ciri, PR per repo) · kolaborasi & code review · persediaan persekitaran (.NET 10 SDK, IDE, tools) |
| **3** | **Refresher .NET**: C# OOP/LINQ/DI/async · EF Core (DbContext, annotations, Fluent API, migration) · MVC + validation · Identity/SSO + RBAC. **Deliverable:** **kontrak Profile DB** dipersetujui (repo `profile`); setiap sistem **di-scaffold dalam repo sendiri** merujuk kontrak profil; corak aliran kerja & `SubmissionStatus` disepakati |

### Fasa 2 — 4 trek selari (Hari 4–14)

| Blok | K1 · Pentadbiran (LD·PKS·Kontrak) | K2 · Pas/Parkir/Pelekat | K3 · ID/AD/Email | K4 · Tempahan Fasiliti Sukan |
|------|-----------------------------------|--------------------------|------------------|-------------------------------|
| **Hari 4** | Skema DB 3 projek + borang draf | Skema DB akses & kenderaan | Skema DB akaun & akses | Katalog fasiliti + slot + seed |
| **Hari 5–6** | LD muat naik · PKS akuan polisi siber (kait versi) · Kontrak daftar (no. rujukan) | Borang pas/pelekat + semakan pendua no. plat | Borang akaun + kelulusan Penyelia | Borang tempahan + semakan slot bertindih |
| **Hari 7–9** | Kelulusan HR / IctSecurityOfficer / IctAdmin + skrin admin | Semakan keselamatan + kelulusan bersyarat | Pemprosesan ICT + RBAC + simulasi AD | Kelulusan FacilityAdmin + kalendar slot |
| **Hari 10–12** | Notifikasi + Slip/laporan PDF + dashboard | QR/Barcode + skrin ronda + laporan | Penjejakan status + audit trail + dashboard ICT | Peringatan tempahan + kalendar + eksport PDF/Excel |
| **Hari 13–14** | xUnit + refactor + sedia integrasi/deploy | Ujian E2E + bug fixing + sedia integrasi | RBAC testing + security audit + sedia integrasi | Ujian bertindih slot + refactor + sedia integrasi |

### Fasa 3 — Sesi bersama (Hari 15)

| Hari | Fokus utama |
|------|-------------|
| **15** | **Integrasi rentas sistem melalui Profile DB** (bukan merge cabang) · Papan Pemuka Induk NRES · **SIT + UAT pre-check** (aliran rentas sistem via profil & SSO, RBAC, muat naik fail, audit log) · **deploy bebas** setiap sistem ke subdomainnya · demo & penilaian capstone |

---

## Rentak harian (setiap hari)

Pendaftaran **9.15–9.30** · SESI PAGI **9.30–12.30** · rehat & makan **12.30–2.30** · SESI PETANG **2.30–4.30** (Isnin–Khamis). **Jumaat:** **9.00–12.00** · rehat **12.00–3.00** · **3.00–4.30**. ~5 jam kontak/hari. Setiap hari: **≥60% masa hands-on lab**.

> **Rentak Fasa 2:** setiap pagi bermula dengan **stand-up 15 minit per kumpulan** (semalam / hari ini / halangan) dan `git pull --rebase`. Setiap petang berakhir dengan **commit + push + kemas kini board**. Jurulatih berpusing antara 4 kumpulan.

---

## Format fail

### Sesi bersama — `hari-1/`, `hari-2/`, `hari-3/`, `hari-15/`

### Trek kumpulan — `kumpulan-N-<slug>/hari-4/`, `hari-5-6/`, `hari-7-9/`, `hari-10-12/`, `hari-13-14/`

Setiap folder **MESTI** mengandungi 3 fail:

1. **`README.md`** — nota konsep Bahasa Melayu: fokus, jadual waktu, penerangan **kenapa** setiap konsep wujud, rujukan rasmi Microsoft Docs. Terangkan konsep dahulu; hands-on penuh ada di `snippets/lab.md`.
2. **`snippets/lab.md`** — lab hands-on langkah demi langkah bernombor (Latihan 0, 1, 2, …), setiap satu dengan **Objektif**, langkah, blok kod penuh untuk ditaip, dan **✅ Semakan** di hujung. Ini bahagian paling penting kursus.
3. **`nota-penceramah.md`** — nota penceramah: pemasaan setiap sesi, poin bercakap, silap biasa peserta, soalan untuk cetus perbincangan, deliverable akhir.

Setiap `README.md` mula dengan pautan ke `JADUAL.md` dan (bagi trek) ke `README.md` kumpulan.

Setiap folder kumpulan turut ada **`README.md` peringkat trek** — gambaran modul, entiti, prefix, peranan, cabang Git, dan senarai 5 blok.

---

## Penggunaan AI dalam kursus

AI (Claude, Copilot, dll.) diajar sebagai **alat bantu berdisiplin**, bukan penjana jawapan. Diperkenalkan Hari 1 dan digunakan sepanjang kursus.

**Guna AI untuk:** draf pertama URS/use case/ERD · jana data ujian sintetik · terangkan mesej ralat · cadang nama & struktur · semak kod sendiri · tulis draf ujian xUnit · ringkaskan diff untuk code review.

**Jangan guna AI untuk:** menerima kod tanpa faham · reka keperluan pengguna yang sepatutnya datang daripada NRES · jana skema DB yang bercanggah dengan SPEC ini · tampal data NRES sebenar ke dalam prompt.

> **Peraturan tetap kursus:** setiap output AI mesti melalui **semakan manusia** — peserta menerangkan kod itu kepada rakan sekumpulan sebelum ia di-commit. Diperiksa semasa code review Hari 2 dan penilaian capstone.

---

## Kriteria Penilaian (Capstone)

| Kriteria | Wajaran |
|----------|---------|
| Modul kumpulan lengkap & berfungsi | 25% |
| Corak aliran kerja betul (draft→submit→approve→audit) | 15% |
| Validation, authorization & keselamatan | 15% |
| Ujian (xUnit) & kualiti kod | 15% |
| **Kolaborasi Git & Agile** (kualiti commit, PR per repo, board, integrasi bersih via Profile DB) | 15% |
| **Dokumentasi** (URS, ERD, diagram, README modul) | 10% |
| Pembentangan demo | 5% |

> Peserta yang menyiapkan semua lab treknya, menyumbang kepada integrasi Hari 15, dan membentangkan demo menerima **Sijil Penyertaan** — *Pembangunan Sistem Dalaman NRES Dengan ASP.NET Core*.

## Jangan

- Jangan simpan kata laluan sebenar dalam modul ID/AD/Email (ajar peserta **jangan** — ini titik pengajaran keselamatan).
- Jangan guna data NRES sebenar — semua contoh **sintetik**.
- Jangan tukar `SubmissionStatus`, peranan, prefix rujukan, atau **kontrak Profile DB** tanpa mengemas kini dokumen ini dahulu.
- Jangan ubah skema/kontrak **Profile DB** (repo [`profile`](https://github.com/nres-bpm/profile)) tanpa **PR + persetujuan** semua sistem yang bergantung.
- Jangan bina semula "entiti kongsi" merentas sistem — **hanya Profile DB dikongsi**; `Submission`/`Attachment`/`AuditLog` milik setiap repo sendiri.
