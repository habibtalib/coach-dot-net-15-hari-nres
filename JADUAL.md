# Aturcara Rasmi — Latihan *Coaching* Pembangunan Sistem Dalaman NRES Dengan ASP.NET Core (.NET 10)

> **Sumber rasmi skop:** *Cadangan Silibus Latihan Coaching Pembangunan Sistem (.NET) — 15 Hari, Struktur 4 Kumpulan Dedicated* (NRES). Kanun teknikal tunggal: [`SPEC-KURSUS.md`](./SPEC-KURSUS.md). Kontrak kerja pasukan: [`KOLABORASI.md`](./KOLABORASI.md). Konteks AI kongsi: [`AGENTS.md`](./AGENTS.md).
>
> **Tajuk penuh:** *Membina Sistem Onboarding & Khidmat Dalaman NRES* — **6 sistem ASP.NET Core MVC yang berasingan** (poly-repo, org `nres-bpm`), setiap satu repo/subdomain/DB sendiri, diintegrasikan melalui **Profile DB** yang dikongsi, dibina **dari kosong** oleh **4 kumpulan dedicated yang bekerja selari**.

## Maklumat Sesi

| Perkara | Butiran |
|---------|---------|
| **Kod Kursus** | DOTNET-NRES-15 |
| **Tempoh** | 15 Hari (~75 Jam kontak) |
| **Tahap** | Pertengahan (Intermediate) — *asas C# / OOP disyorkan* |
| **Mod** | Fizikal / Maya / Hibrid — **berpaksikan lab** (≥60% masa hands-on) |
| **Struktur** | 4 kumpulan dedicated · **6 repo (poly-repo, `nres-bpm`)** · Profile DB dikongsi |
| **Masa** | Isnin–Khamis 9.30 pg – 4.30 ptg · Jumaat 9.00 pg – 4.30 ptg |
| **Anjuran** | Kementerian Sumber Asli & Kelestarian Alam (NRES) |
| **Bilangan peserta disyorkan** | 16 – 24 orang (4 – 6 setiap kumpulan) |
| **Rangka** | ASP.NET Core MVC · .NET 10 LTS · EF Core 10 · Identity · SQLite → SQL Server |
| **Alat sokongan** | Git + GitHub · GitHub Projects & Jira · pembantu AI |

> **Rentak harian (Isnin–Khamis):** Pendaftaran **9.15–9.30**; sesi pagi **9.30–12.30**; rehat & makan tengah hari **12.30–2.30**; sesi petang **2.30–4.30**. **Jumaat:** sesi pagi **9.00–12.00**; rehat **12.00–3.00**; sesi petang **3.00–4.30**. ~5 jam kontak/hari (Jumaat ~4.5 jam).

> **Konvensyen bahasa:** Nota & penerangan dalam **Bahasa Melayu**; kod, nama kelas, istilah teknikal (`Controller`, `DbContext`, `migration`, `pull request`) dikekalkan dalam **Bahasa Inggeris**.

---

## Struktur kursus — 3 fasa

```text
FASA 1 — SESI BERSAMA (Hari 1–3)          semua kumpulan, satu bilik
  Hari 1   Perancangan · Dokumentasi · URS/SRS · ERD · Use Case & Process Flow   (AI-assisted)
  Hari 2   Agile (Jira + GitHub Projects) · Git & Branching · Kolaborasi · Persekitaran
  Hari 3   Refresher .NET · EF Core · Identity/SSO · KONTRAK PROFILE DB + scaffold setiap repo
                                          └── 6 repo (nres-bpm) di-scaffold ──┐
                                                                             │
FASA 2 — 4 TREK SELARI (Hari 4–14)  ◄─────────────────────────────────────────┘
  ┌─────────────┬─────────────┬─────────────┬─────────────┐
  │ Kumpulan 1  │ Kumpulan 2  │ Kumpulan 3  │ Kumpulan 4  │
  │ Pentadbiran │ Pas/Parkir/ │ ID, AD &    │ Tempahan    │
  │ LD/PKS/Kon  │ Pelekat     │ Email       │ Fasiliti    │
  │ (3 repo)    │ (1 repo)    │ (1 repo)    │ (1 repo)    │
  └─────────────┴─────────────┴─────────────┴─────────────┘
   Blok: Hari 4 · Hari 5–6 · Hari 7–9 · Hari 10–12 · Hari 13–14
   Uji integrasi Profile DB berterusan; release per repo di hujung blok

FASA 3 — SESI BERSAMA (Hari 15)
  Hari 15  Integrasi rentas sistem via Profile DB · Papan Pemuka Induk · SIT & UAT · Demo
```

### 4 Kumpulan & Modul

| Kumpulan | Modul | Skop | Trek |
|----------|-------|------|------|
| **1** | **Lapor Diri** | Permohonan lapor diri pekerja baharu, profil, dokumen sokongan, slip akuan, kelulusan HR | [`kumpulan-1-pentadbiran/lapor-diri/`](./kumpulan-1-pentadbiran/lapor-diri/) |
| **1** | **Pematuhan PKS** | Pengakuan Kod Setia, versi dasar, jejak pematuhan staf | [`kumpulan-1-pentadbiran/pematuhan-pks/`](./kumpulan-1-pentadbiran/pematuhan-pks/) |
| **1** | **Pengurusan Kontrak** | Daftar kontrak/perjanjian, pihak terlibat, milestone & jejak status | [`kumpulan-1-pentadbiran/pengurusan-kontrak/`](./kumpulan-1-pentadbiran/pengurusan-kontrak/) |
| **2** | **Pas, Parkir & Pelekat** | Akses kawasan & keselamatan kenderaan, pas pelawat/staf, pelekat & lot parkir, semakan pendua plat, QR | [`kumpulan-2-pas-parkir-pelekat/`](./kumpulan-2-pas-parkir-pelekat/) |
| **3** | **ID, AD & Email** | Permohonan akaun pengguna, Active Directory, e-mel rasmi, kelulusan penyelia → ICT, audit log | [`kumpulan-3-id-ad-email/`](./kumpulan-3-id-ad-email/) |
| **4** | **Tempahan Fasiliti Sukan** | Tempahan gelanggang/kemudahan sukan, slot masa, semakan pertindihan, kalendar, kelulusan | [`kumpulan-4-tempahan-fasiliti-sukan/`](./kumpulan-4-tempahan-fasiliti-sukan/) |

> **Nota skop:** Kumpulan 1 memikul **3 projek** (Lapor Diri · Pematuhan PKS · Pengurusan Kontrak) — agihkan ahli/skop sewajarnya supaya rentak selari Fasa 2 seimbang. Rujuk [`SPEC-KURSUS.md`](./SPEC-KURSUS.md).
>
> **Nota versi — MUKTAMAD:** cadangan NRES menulis .NET 8; kursus ini **disahkan** menggunakan **.NET 10 LTS + C# 14**.
>
> **Nota pemetaan hari — MUKTAMAD:** cadangan NRES memperuntukkan Hari 1–2 untuk sesi bersama dan Hari 3–14 untuk trek kumpulan. Kursus ini **disahkan** menggunakan **Hari 1–3** bersama (bagi memuatkan perancangan, URS, ERD, Agile, kolaborasi & AI) dan **Hari 4–14** untuk trek — 11 hari, bukan 12. Kandungan setiap blok trek kekal sama; blok terakhir dipendekkan dari 3 hari ke 2 hari.

---

# FASA 1 — SESI BERSAMA

## HARI 1 — Perancangan Projek, Dokumentasi, URS & ERD

**Fokus:** Sebelum satu baris kod ditulis — faham *apa* yang dibina dan *kenapa*. Hasilkan artifak dokumentasi sebenar (URS, use case, process flow, ERD) dengan bantuan AI, dan sahkan setiap satu secara manual.

| Masa | Agenda |
|------|--------|
| 9.15 – 9.30 pagi | Pendaftaran Peserta & Minum Pagi |
| **9.30 – 11.00 pagi** | **SESI 1: Perancangan Projek & Skop** — gambaran sistem NRES sebagai *request workflow system*; 4 modul & pembahagian kumpulan; peranan; risiko projek; apa itu "siap". 🧠 **Bengkel:** peta medan sama merentas 4 modul |
| **11.00 – 12.30 tgh** | **SESI 2: Design Thinking → URS & SRS** — empathize/define (persona & empathy map) dalam FigJam; beza URS vs SRS; **jejak setiap URS balik ke satu *pain***; kriteria penerimaan. 💻 **Lab:** empathy map + URS modul (draf AI → **semakan manusia**) |
| 12.30 – 2.30 petang | Rehat dan Makan Tengah Hari |
| **2.30 – 3.30 petang** | **SESI 3: Process Flow & Use Case** — aktor, use case, aliran utama vs alternatif; diagram sebagai kod (**Mermaid**) supaya boleh diversi dalam Git. 💻 **Lab:** process flow + use case diagram modul sendiri |
| **3.30 – 4.30 petang** | **SESI 4: ERD & Reka Bentuk Data** — entiti, hubungan, kardinaliti, kunci asing; **kenapa satu `Submission` induk dikongsi**; normalisasi secukupnya. 💻 **Lab:** ERD (Mermaid `erDiagram`) modul sendiri + sahkan terhadap `SPEC-KURSUS.md` |
| 4.30 petang | Bersurai |

**Bantuan AI hari ini:** draf URS, cadang use case yang terlepas, semak ERD terhadap keperluan, jana data ujian sintetik. **Peraturan:** AI menulis draf — **peserta memutuskan**. Setiap artifak disemak baris demi baris sebelum diterima.

**Hasil Hari 1:** `docs/URS-modul-N.md`, `docs/use-case-modul-N.md`, `docs/erd-modul-N.md`, `docs/soalan-terbuka-modul-N.md` bagi setiap kumpulan; peserta boleh terangkan keempat-empat modul dan corak aliran kerja kongsi.

---

## HARI 2 — Agile, Git, Branching & Kolaborasi Pasukan

**Fokus:** Cara 4 pasukan membina 6 sistem berasingan (repo/subdomain/DB sendiri) yang tetap **konsisten & boleh diintegrasi** melalui Profile DB. Hari ini menetapkan disiplin poly-repo + kontrak Profile DB yang menentukan sama ada integrasi Hari 15 berjalan lancar.

| Masa | Agenda |
|------|--------|
| 9.15 – 9.30 pagi | Pendaftaran & Minum Pagi |
| **9.30 – 11.00 pagi** | **SESI 5: Agile & Pengurusan Kerja** — Agile Manifesto (4 nilai), backlog, sprint, stand-up, Definition of Done; **Jira** (jenis isu: **epic → user story → task → bug → spike**, subtask, sprint board **sendiri setiap pasukan** & **swimlanes** mengikut epic, issue key dalam commit) dan **GitHub Projects** (isu → board → cabang → PR → tutup; pemetaan GitHub ↔ Jira). 💻 **Lab:** setiap kumpulan bina backlog modulnya dari URS Hari 1. *Rasional: rancang kerja dahulu — belum sentuh Git.* |
| **11.00 – 12.30 tgh** | **SESI 6: Git Asas & Repositori** — `clone`, `status`, `add`, `commit`, `push`, `pull --rebase`; format mesej commit kursus (sertakan issue key Jira); `.gitignore`. 💻 **Lab:** setiap peserta clone repo & buat commit pertama |
| 12.30 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.30 petang** | **SESI 7: Strategi Poly-repo & Code Review** — satu repo/sistem dalam `nres-bpm`, `main` dilindungi (PR sahaja), cabang ciri pendek → PR ke `main` repo; templat PR, senarai semak penyemak; **selesaikan konflik gabungan** (dalam repo sama) secara langsung. 💻 **Lab:** cipta konflik dalam repo & selesaikannya |
| **3.30 – 4.30 petang** | **SESI 8: Kolaborasi, AI Berpasukan & Persekitaran** — matriks pemilikan fail, protokol fail kongsi, slot migration, peraturan AI kongsi (`AGENTS.md`); pasang .NET 10 SDK, IDE, `dotnet ef`, sahkan `dotnet --version`. 💻 **Lab:** persekitaran sedia + setiap kumpulan tandatangan kontrak `KOLABORASI.md` |
| 4.30 petang | Bersurai |

**Hasil Hari 2:** 6 repo dalam `nres-bpm` sedia diakses; board Agile berisi backlog setiap sistem; setiap peserta boleh commit, buka PR (ke `main` repo), dan selesaikan konflik; persekitaran .NET 10 berjalan pada setiap mesin.

---

## HARI 3 — Refresher .NET & Asas Kongsi

**Fokus:** Ulangkaji .NET **sambil membina asas sebenar** yang keempat-empat kumpulan akan guna. Hujung hari ini, semua kumpulan bercabang dari asas yang sama.

| Masa | Agenda |
|------|--------|
| 9.15 – 9.30 pagi | Pendaftaran & Minum Pagi |
| **9.30 – 11.00 pagi** | **SESI 9: Teras C# & ASP.NET Core** — OOP, LINQ, `async/await`, **Dependency Injection**; `dotnet new mvc`, `Program.cs` & middleware pipeline, corak Controller/View/ViewModel. 💻 **Lab:** scaffold sistem (`dotnet new mvc`) berjalan |
| **11.00 – 12.30 tgh** | **SESI 10: EF Core & Corak Entiti** — `DbContext`, Data Annotations vs **Fluent API**, hubungan & kunci asing. 💻 **Lab:** corak `SubmissionStatus`/`Submission`/`Attachment`/`AuditLog`/`ApprovalStep` (dalam DB sistem sendiri) + skema `UserProfile` (**Profile DB**, repo `profile`) + `IEntityTypeConfiguration<T>` |
| 12.30 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.30 petang** | **SESI 11: Identity/SSO, RBAC & Set Servis Piawai** — Identity (Lapor Diri awam) & **SSO** (sistem dalaman), `[Authorize(Roles=...)]`, 6 peranan dari profil; set servis piawai (`IReferenceNumberService` … `ICurrentUserService`) + **klien Profile DB** (`IProfileService`); partial view piawai + `SubmissionControllerBase`. 💻 **Lab:** set piawai + sambung Profile DB |
| **3.30 – 4.30 petang** | **SESI 12: Kontrak Profile DB & Scaffold Repo** — setiap sistem scaffold dalam **repo sendiri** (`Add<Modul>Module`, `ApplyConfigurationsFromAssembly`, `ModuleDescriptor`); migration awal per repo; **kontrak Profile DB dipersetujui & diterbitkan** (repo `profile`); Lapor Diri sambung **tulis**, sistem lain sambung **baca**. 💻 **Lab:** 6 repo scaffold + sambung Profile DB |
| 4.30 petang | Bersurai |

**Hasil Hari 3:** **Kontrak Profile DB** dipersetujui & diterbitkan (repo `profile`); 6 repo di-scaffold dalam `nres-bpm` merujuk kontrak; setiap kumpulan tahu repo miliknya dan bahawa **hanya Profile DB dikongsi** (selebihnya dalam repo/DB sendiri).

---

# FASA 2 — 4 TREK SELARI (HARI 4–14)

Keempat-empat kumpulan mengikut **blok yang sama** pada hari yang sama, dengan kandungan modul masing-masing. Ini memudahkan jurulatih berpusing dan membolehkan sesi kongsi merentas kumpulan.

| Blok | Tema kongsi | K1 · Pentadbiran (LD·PKS·Kon) | K2 · Pas/Parkir/Pelekat | K3 · ID/AD/Email | K4 · Tempahan Fasiliti Sukan |
|------|-------------|-------------------------------|--------------------------|------------------|-------------------------------|
| **Hari 4** | Skema DB & skrin pertama | Skema DB 3 projek + borang draf | Skema akses & kenderaan + halaman utama modul | Skema akaun & akses + jenis permohonan | Katalog fasiliti + slot masa + seed data |
| **Hari 5–6** | Borang & peraturan perniagaan | LD muat naik · PKS pengakuan dasar · Kontrak daftar (no. rujukan) | Borang pas/pelekat/parkir + **semakan pendua no. plat** | Borang akaun AD/e-mel + **kelulusan Penyelia** (peringkat 1) | Borang tempahan + **semakan slot bertindih** + akuan |
| **Hari 7–9** | Aliran kelulusan & skrin admin | Kelulusan HR/BPM/ICT + skrin admin setiap projek | Skrin Pegawai Keselamatan + kelulusan bersyarat + peruntukan lot | Pemprosesan ICT + **RBAC** + simulasi AD | Kelulusan **FacilityAdmin** + **kalendar slot** + peruntukan |
| **Hari 10–12** | Notifikasi, laporan & dashboard | Notifikasi + **Slip/laporan PDF** + dashboard pentadbiran | **QR/Barcode** + skrin semakan ronda + laporan bercetak | Penjejakan status + **audit trail** + dashboard ICT | Peringatan tempahan + dashboard kalendar + **eksport PDF/Excel** |
| **Hari 13–14** | Ujian, refactor & sedia integrasi/deploy | xUnit 3 repo + optimasi query EF Core | Ujian E2E + bug fixing + pemantapan validasi | **RBAC testing** + security audit log | Ujian **pertindihan slot** + pembersihan kod |

**Rentak setiap hari Fasa 2** (rujuk [`KOLABORASI.md`](./KOLABORASI.md) §8):

| Masa | Aktiviti |
|------|----------|
| 9.00 – 9.15 | Stand-up per kumpulan + `git pull --rebase` (dalam repo sendiri) |
| 9.15 – 9.25 | **Semakan silang** — konsistensi konvensyen & kontrak Profile DB antara sistem |
| 9.25 – 1.00 | Sesi pembangunan (commit kecil & kerap) |
| 2.30 – 4.30 | Sesi pembangunan |
| 4.30 – 5.00 | Code review berpasangan + PR + push + kemas kini board |

> **Integrasi berterusan:** sebaik Profile DB sedia (Hari 3), setiap sistem menguji sambungan Profile DB & SSO awal, dan **release per repo** (tag SemVer) di hujung setiap blok. Menjelang Hari 15, setiap sistem sudah membaca/menulis profil dengan betul — Hari 15 tertumpu pada SIT rentas sistem dan demo, bukan menyelamatkan gabungan.

**Butiran penuh setiap blok:** lihat `README.md` dalam folder trek setiap kumpulan.

---

# FASA 3 — SESI BERSAMA

## HARI 15 — Integrasi Sistem, SIT & Persembahan Demo

**Fokus:** Sahkan 6 sistem berasingan berintegrasi bersih melalui Profile DB & SSO, uji menyeluruh, dan bentangkan.

| Masa | Agenda |
|------|--------|
| 9.15 – 9.30 pagi | Pendaftaran & Minum Pagi |
| **9.30 – 11.00 pagi** | **SESI 44: Integrasi Rentas Sistem** — sahkan setiap sistem sambung **Profile DB** & **SSO** dengan betul; Lapor Diri cipta profil, sistem lain baca; sahkan setiap repo `dotnet build` & deploy bebas ke subdomainnya. 💻 **Lab:** ujian integrasi Profile DB |
| **11.00 – 12.30 tgh** | **SESI 45: Papan Pemuka Induk NRES** — pandangan bersatu merentas 6 sistem (via Profile DB / API): navigasi ikut peranan, dashboard peribadi (draf saya / dihantar / menunggu kelulusan saya / selesai), carian rujukan global. 💻 **Lab:** dashboard induk |
| 12.30 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.30 petang** | **SESI 46: SIT & UAT Pre-Check** — skrip ujian aliran rentas sistem (satu staf baharu: Lapor Diri → profil dicipta → guna sistem dalaman via SSO), semakan **RBAC** setiap peranan, muat naik fail, kesempurnaan **audit log**; rekod isu & keputusan lulus/gagal. 💻 **Lab:** jalankan skrip SIT |
| **3.30 – 4.30 petang** | **Demo & Penilaian Capstone** — setiap kumpulan membentangkan sistemnya, keputusan seni bina, dan pengajaran kolaborasi; penilaian; nota deployment (SQLite → SQL Server, per sistem); penyampaian sijil |
| 4.30 petang | Bersurai |

**Hasil Hari 15:** 6 sistem NRES berasingan berintegrasi bersih melalui **Profile DB** & SSO, lulus SIT, dibentangkan oleh keempat-empat kumpulan.

---

## Kriteria Penilaian (Capstone)

| Kriteria | Wajaran |
|----------|---------|
| Modul kumpulan lengkap & berfungsi | 25% |
| Corak aliran kerja betul (draft→submit→approve→audit) | 15% |
| Validation, authorization & keselamatan | 15% |
| Ujian (xUnit) & kualiti kod | 15% |
| **Kolaborasi Git & Agile** (kualiti commit, PR per repo, board, integrasi Profile DB bersih) | 15% |
| **Dokumentasi** (URS, ERD, diagram, README modul) | 10% |
| Pembentangan demo | 5% |

> Peserta yang menyiapkan lab treknya, menyumbang kepada integrasi Hari 15, dan membentangkan demo menerima **Sijil Penyertaan** — *Pembangunan Sistem Dalaman NRES Dengan ASP.NET Core*.

## Pemetaan Sesi → Deliverable

| Hari | Deliverable / Artifak |
|------|------------------------|
| 1 | `docs/URS-modul-N.md`, `use-case-modul-N.md`, `erd-modul-N.md`, `soalan-terbuka-modul-N.md` (4 set) |
| 2 | 6 repo (`nres-bpm`) sedia · board Agile berisi backlog · `docs/kumpulan-N/` (RINGKASAN, pemetaan-jira, nota-ai, **kontrak**) · persekitaran .NET 10 sedia |
| 3 | **Kontrak Profile DB** diterbitkan (repo `profile`) + 6 repo di-scaffold · Lapor Diri tulis / lain-lain baca profil |
| 4 | Skema & skrin pertama setiap modul (4 modul) |
| 5–6 | Borang + peraturan perniagaan setiap modul |
| 7–9 | Aliran kelulusan & skrin admin setiap modul |
| 10–12 | Notifikasi, laporan & dashboard setiap modul |
| 13–14 | Ujian xUnit per repo + kod dibersihkan + setiap sistem sedia integrasi/deploy |
| 15 | 6 sistem berintegrasi via Profile DB & SSO · Papan Pemuka Induk · laporan SIT · demo |
