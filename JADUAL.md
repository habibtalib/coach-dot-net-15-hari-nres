# Aturcara Rasmi — Latihan *Coaching* Pembangunan Sistem Dalaman NRES Dengan ASP.NET Core (.NET 10)

> **Sumber rasmi skop:** *Cadangan Silibus Latihan Coaching Pembangunan Sistem (.NET) — 15 Hari, Struktur 4 Kumpulan Dedicated* (NRES). Kanun teknikal tunggal: [`SPEC-KURSUS.md`](./SPEC-KURSUS.md). Kontrak kerja pasukan: [`KOLABORASI.md`](./KOLABORASI.md). Konteks AI kongsi: [`AGENTS.md`](./AGENTS.md).
>
> **Tajuk penuh:** *Membina Sistem Onboarding & Khidmat Dalaman NRES* — satu aplikasi ASP.NET Core MVC yang menyatukan **4 modul** permohonan & aliran kerja kelulusan, dibina **dari kosong** oleh **4 kumpulan dedicated yang bekerja selari** dalam satu repositori.

## Maklumat Sesi

| Perkara | Butiran |
|---------|---------|
| **Kod Kursus** | DOTNET-NRES-15 |
| **Tempoh** | 15 Hari (105 Jam) |
| **Tahap** | Pertengahan (Intermediate) — *asas C# / OOP disyorkan* |
| **Mod** | Fizikal / Maya / Hibrid — **berpaksikan lab** (≥60% masa hands-on) |
| **Struktur** | 4 kumpulan dedicated · 1 repositori · 1 aplikasi bersepadu |
| **Masa** | 9.00 pagi – 5.00 petang |
| **Anjuran** | Kementerian Sumber Asli & Kelestarian Alam (NRES) |
| **Bilangan peserta disyorkan** | 16 – 24 orang (4 – 6 setiap kumpulan) |
| **Rangka** | ASP.NET Core MVC · .NET 10 LTS · EF Core 10 · Identity · SQLite → SQL Server |
| **Alat sokongan** | Git + GitHub · GitHub Projects & Jira · pembantu AI |

> **Rentak harian:** Pendaftaran & minum pagi **8.30–9.00**; sesi pagi **9.00–1.00**; rehat & makan tengah hari **1.00–2.30**; sesi petang **2.30–5.00**; bersurai **5.00 petang**. ~7 jam kontak/hari.

> **Konvensyen bahasa:** Nota & penerangan dalam **Bahasa Melayu**; kod, nama kelas, istilah teknikal (`Controller`, `DbContext`, `migration`, `pull request`) dikekalkan dalam **Bahasa Inggeris**.

---

## Struktur kursus — 3 fasa

```text
FASA 1 — SESI BERSAMA (Hari 1–3)          semua kumpulan, satu bilik
  Hari 1   Perancangan · Dokumentasi · URS/SRS · ERD · Use Case & Process Flow   (AI-assisted)
  Hari 2   Git & Branching · Agile (GitHub Projects + Jira) · Kolaborasi · Persekitaran
  Hari 3   Refresher .NET · EF Core · Identity/RBAC · ASAS KONGSI + migration InitialShared
                                          └── 4 cabang kumpulan dibuka ──┐
                                                                         │
FASA 2 — 4 TREK SELARI (Hari 4–14)  ◄─────────────────────────────────────┘
  ┌─────────────┬─────────────┬─────────────┬─────────────┐
  │ Kumpulan 1  │ Kumpulan 2  │ Kumpulan 3  │ Kumpulan 4  │
  │ Lapor Diri  │ Pas/Parkir/ │ ID, AD &    │ Perisian &  │
  │             │ Pelekat     │ Email       │ Aset ICT    │
  └─────────────┴─────────────┴─────────────┴─────────────┘
   Blok: Hari 4 · Hari 5–6 · Hari 7–9 · Hari 10–12 · Hari 13–14
   Gabungan latihan ke master di hujung setiap blok

FASA 3 — SESI BERSAMA (Hari 15)
  Hari 15  Merge 4 cabang · Papan Pemuka Induk · SIT & UAT pre-check · Demo
```

### 4 Kumpulan & Modul

| Kumpulan | Modul | Skop | Trek |
|----------|-------|------|------|
| **1** | **Lapor Diri** | Permohonan lapor diri pekerja baharu, profil, dokumen sokongan, slip akuan, aliran kelulusan HR | [`kumpulan-1-lapor-diri/`](./kumpulan-1-lapor-diri/) |
| **2** | **Pas, Parkir & Pelekat** | Akses kawasan & keselamatan kenderaan, pas pelawat/staf, pelekat & lot parkir, semakan pendua plat, QR | [`kumpulan-2-pas-parkir-pelekat/`](./kumpulan-2-pas-parkir-pelekat/) |
| **3** | **ID, AD & Email** | Permohonan akaun pengguna, Active Directory, e-mel rasmi, kelulusan penyelia → ICT, audit log | [`kumpulan-3-id-ad-email/`](./kumpulan-3-id-ad-email/) |
| **4** | **Perisian & Aset ICT** | Katalog aset, lesen perisian, pinjaman & pemulangan aset, stok masa-nyata, laporan | [`kumpulan-4-perisian-aset-ict/`](./kumpulan-4-perisian-aset-ict/) |

> **Nota skop:** Modul **PKS** tidak termasuk dalam kursus ini (tiada dalam cadangan silibus NRES). Rujuk [`SPEC-KURSUS.md`](./SPEC-KURSUS.md).
>
> **Nota versi:** cadangan NRES menulis .NET 8; kursus ini menggunakan **.NET 10 LTS**.
>
> **Nota pemetaan hari:** cadangan NRES memperuntukkan Hari 1–2 untuk sesi bersama dan Hari 3–14 untuk trek kumpulan. Kursus ini menggunakan **Hari 1–3** bersama (bagi memuatkan perancangan, URS, ERD, Agile, kolaborasi & AI) dan **Hari 4–14** untuk trek. Kandungan setiap blok trek kekal sama; blok terakhir dipendekkan dari 3 hari ke 2 hari.

---

# FASA 1 — SESI BERSAMA

## HARI 1 — Perancangan Projek, Dokumentasi, URS & ERD

**Fokus:** Sebelum satu baris kod ditulis — faham *apa* yang dibina dan *kenapa*. Hasilkan artifak dokumentasi sebenar (URS, use case, process flow, ERD) dengan bantuan AI, dan sahkan setiap satu secara manual.

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran Peserta & Minum Pagi |
| **9.00 – 10.30 pagi** | **SESI 1: Perancangan Projek & Skop** — gambaran sistem NRES sebagai *request workflow system*; 4 modul & pembahagian kumpulan; peranan; risiko projek; apa itu "siap". 🧠 **Bengkel:** peta medan sama merentas 4 modul |
| **10.30 – 1.00 tgh** | **SESI 2: URS & SRS** — beza keperluan pengguna (URS) vs spesifikasi perisian (SRS); menulis keperluan yang boleh diuji; kriteria penerimaan. 💻 **Lab:** setiap kumpulan menulis URS modulnya — draf pertama dengan AI, kemudian **disemak & dibetulkan manusia** |
| 1.00 – 2.30 petang | Rehat dan Makan Tengah Hari |
| **2.30 – 3.45 petang** | **SESI 3: Process Flow & Use Case** — aktor, use case, aliran utama vs alternatif; diagram sebagai kod (**Mermaid**) supaya boleh diversi dalam Git. 💻 **Lab:** process flow + use case diagram modul sendiri |
| **3.45 – 5.00 petang** | **SESI 4: ERD & Reka Bentuk Data** — entiti, hubungan, kardinaliti, kunci asing; **kenapa satu `Submission` induk dikongsi**; normalisasi secukupnya. 💻 **Lab:** ERD (Mermaid `erDiagram`) modul sendiri + sahkan terhadap `SPEC-KURSUS.md` |
| 5.00 petang | Bersurai |

**Bantuan AI hari ini:** draf URS, cadang use case yang terlepas, semak ERD terhadap keperluan, jana data ujian sintetik. **Peraturan:** AI menulis draf — **peserta memutuskan**. Setiap artifak disemak baris demi baris sebelum diterima.

**Hasil Hari 1:** `docs/URS-modul-N.md`, `docs/use-case-modul-N.md`, `docs/erd-modul-N.md`, `docs/soalan-terbuka-modul-N.md` bagi setiap kumpulan; peserta boleh terangkan keempat-empat modul dan corak aliran kerja kongsi.

---

## HARI 2 — Git, Branching, Agile & Kolaborasi Pasukan

**Fokus:** Cara 4 pasukan menulis kod serentak tanpa berlanggar. Hari ini menetapkan disiplin yang menentukan sama ada Hari 15 berjalan lancar atau menjadi malapetaka gabungan.

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 10.30 pagi** | **SESI 5: Git Asas & Repositori** — `clone`, `status`, `add`, `commit`, `push`, `pull --rebase`; format mesej commit kursus; `.gitignore`. 💻 **Lab:** setiap peserta clone repo & buat commit pertama |
| **10.30 – 1.00 tgh** | **SESI 6: Strategi Percabangan & Code Review** — `master` dilindungi, `asas/shared-foundation`, 4 cabang kumpulan, cabang ciri pendek; **pull request**, templat PR, senarai semak penyemak; **selesaikan konflik gabungan** secara langsung. 💻 **Lab:** cipta konflik dengan sengaja dan selesaikannya |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.45 petang** | **SESI 7: Agile & Pengurusan Kerja** — backlog, sprint, stand-up, Definition of Done; **GitHub Projects** (hands-on: isu → board → cabang → PR → tutup) dan **Jira** (demo: epic → story → subtask, sprint board, issue key dalam commit, pemetaan konsep GitHub ↔ Jira). 💻 **Lab:** setiap kumpulan bina backlog modulnya dari URS Hari 1 |
| **3.45 – 5.00 petang** | **SESI 8: Kolaborasi, AI Berpasukan & Persekitaran** — matriks pemilikan fail, protokol fail kongsi, slot migration, peraturan AI kongsi (`AGENTS.md`); pasang .NET 10 SDK, IDE, `dotnet ef`, sahkan `dotnet --version`. 💻 **Lab:** persekitaran sedia + setiap kumpulan tandatangan kontrak `KOLABORASI.md` |
| 5.00 petang | Bersurai |

**Hasil Hari 2:** Repo dengan 4 cabang kumpulan; board Agile berisi backlog setiap modul; setiap peserta boleh commit, buka PR, dan selesaikan konflik; persekitaran .NET 10 berjalan pada setiap mesin.

---

## HARI 3 — Refresher .NET & Asas Kongsi

**Fokus:** Ulangkaji .NET **sambil membina asas sebenar** yang keempat-empat kumpulan akan guna. Hujung hari ini, semua kumpulan bercabang dari asas yang sama.

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 10.30 pagi** | **SESI 9: Teras C# & ASP.NET Core** — OOP, LINQ, `async/await`, **Dependency Injection**; `dotnet new mvc`, `Program.cs` & middleware pipeline, corak Controller/View/ViewModel. 💻 **Lab:** projek `Nres.Onboarding.Web` berjalan |
| **10.30 – 1.00 tgh** | **SESI 10: EF Core & Entiti Kongsi** — `DbContext`, Data Annotations vs **Fluent API**, hubungan & kunci asing. 💻 **Lab:** tulis `SubmissionStatus`, `Submission`, `Attachment`, `AuditLog`, `ApprovalStep`, `UserProfile` + `IEntityTypeConfiguration<T>` |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.45 petang** | **SESI 11: Identity, RBAC & Servis Kongsi** — Identity, `[Authorize(Roles=...)]`, seed 6 peranan; daftar `IReferenceNumberService`, `IFileStorageService`, `IAuditLogService`, `IWorkflowService`, `INotificationService`, `ICurrentUserService`; partial view kongsi + `SubmissionControllerBase`. 💻 **Lab:** asas kongsi lengkap |
| **3.45 – 5.00 petang** | **SESI 12: Seni Bina Anti-Konflik & Buka Cabang** — modul mendaftar diri (`Add<Modul>Module`), `ApplyConfigurationsFromAssembly`, navigasi didorong `ModuleDescriptor`; migration `InitialShared`; gabung `asas/shared-foundation` → `master`; buka 4 cabang kumpulan. 💻 **Lab:** migration + 4 cabang sedia |
| 5.00 petang | Bersurai |

**Hasil Hari 3:** `master` mengandungi asas kongsi lengkap + migration `InitialShared`; 4 cabang kumpulan dibuka; setiap kumpulan tahu **tepat** fail mana miliknya dan komponen kongsi mana yang **tidak boleh** ditulis semula.

---

# FASA 2 — 4 TREK SELARI (HARI 4–14)

Keempat-empat kumpulan mengikut **blok yang sama** pada hari yang sama, dengan kandungan modul masing-masing. Ini memudahkan jurulatih berpusing dan membolehkan sesi kongsi merentas kumpulan.

| Blok | Tema kongsi | K1 · Lapor Diri | K2 · Pas/Parkir/Pelekat | K3 · ID/AD/Email | K4 · Perisian & Aset ICT |
|------|-------------|-----------------|--------------------------|------------------|---------------------------|
| **Hari 4** | Skema DB & skrin pertama | Skema + borang draf lapor diri | Skema akses & kenderaan + halaman utama modul | Skema akaun & akses + jenis permohonan | Katalog aset & perisian + seed data |
| **Hari 5–6** | Borang & peraturan perniagaan | Muat naik dokumen + nombor rujukan `LD` + hantar | Borang pas/pelekat/parkir + **semakan pendua no. plat** | Borang akaun AD/e-mel + **kelulusan Penyelia** (peringkat 1) | Borang lesen & pinjaman + **semakan stok masa-nyata** + akuan terima |
| **Hari 7–9** | Aliran kelulusan & skrin admin | Dashboard HR + approve/reject + ulasan | Skrin Pegawai Keselamatan + kelulusan bersyarat + peruntukan lot | Pemprosesan ICT + **RBAC** + simulasi AD | Kelulusan Unit Aset + **pemulangan** (Baik/Rosak/Hilang) + kemas kini inventori |
| **Hari 10–12** | Notifikasi, laporan & dashboard | Notifikasi e-mel + **Slip Akuan PDF** + dashboard analitik HR | **QR/Barcode** + skrin semakan ronda + laporan bercetak | Penjejakan status + **audit trail** + dashboard ICT | Peringatan lewat tempoh + dashboard inventori + **eksport PDF/Excel** |
| **Hari 13–14** | Ujian, refactor & sedia gabung | xUnit + optimasi query EF Core | Ujian E2E + bug fixing + pemantapan validasi | **RBAC testing** + security audit log | Ujian pemulangan lewat/lesen/stok + pembersihan kod |

**Rentak setiap hari Fasa 2** (rujuk [`KOLABORASI.md`](./KOLABORASI.md) §8):

| Masa | Aktiviti |
|------|----------|
| 9.00 – 9.15 | Stand-up per kumpulan + `git pull --rebase origin master` |
| 9.15 – 9.25 | **Semakan silang AI** — kesan pertindihan antara kumpulan lebih awal |
| 9.25 – 1.00 | Sesi pembangunan (commit kecil & kerap) |
| 2.30 – 4.30 | Sesi pembangunan |
| 4.30 – 5.00 | Code review berpasangan + PR + push + kemas kini board |

> **Gabungan latihan:** di hujung **setiap blok**, setiap kumpulan menggabungkan cabangnya ke `master` melalui PR. Menjelang Hari 15, `master` sudah mengandungi keempat-empat modul — Hari 15 tertumpu pada integrasi dan demo, bukan menyelamatkan gabungan.

**Butiran penuh setiap blok:** lihat `README.md` dalam folder trek setiap kumpulan.

---

# FASA 3 — SESI BERSAMA

## HARI 15 — Penggabungan Kod, SIT & Persembahan Demo

**Fokus:** Satukan kerja 4 kumpulan menjadi satu sistem, uji ia secara menyeluruh, dan bentangkan.

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 10.30 pagi** | **SESI 44: Penggabungan Repositori** — gabung 4 cabang kumpulan ke `master` mengikut turutan berjadual; selesaikan konflik yang tinggal; sahkan satu migration bersepadu; `dotnet build` bersih. 💻 **Lab:** merge berjadual |
| **10.30 – 1.00 tgh** | **SESI 45: Papan Pemuka Induk NRES** — hubungkan keempat-empat modul: navigasi ikut peranan, dashboard peribadi (draf saya / dihantar / menunggu kelulusan saya / selesai), carian nombor rujukan global merentas modul. 💻 **Lab:** dashboard induk |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.45 petang** | **SESI 46: SIT & UAT Pre-Check** — skrip ujian aliran rentas modul (satu staf baharu melalui keempat-empat modul), semakan **RBAC** setiap peranan, muat naik fail, kesempurnaan **audit log**; rekod isu & keputusan lulus/gagal. 💻 **Lab:** jalankan skrip SIT |
| **3.45 – 5.00 petang** | **Demo & Penilaian Capstone** — setiap kumpulan membentangkan modul, keputusan seni bina, dan pengajaran kolaborasi; penilaian; nota deployment (SQLite → SQL Server); penyampaian sijil |
| 5.00 petang | Bersurai |

**Hasil Hari 15:** Satu aplikasi NRES bersepadu 4 modul pada `master`, lulus SIT, dibentangkan oleh keempat-empat kumpulan.

---

## Kriteria Penilaian (Capstone)

| Kriteria | Wajaran |
|----------|---------|
| Modul kumpulan lengkap & berfungsi | 25% |
| Corak aliran kerja betul (draft→submit→approve→audit) | 15% |
| Validation, authorization & keselamatan | 15% |
| Ujian (xUnit) & kualiti kod | 15% |
| **Kolaborasi Git & Agile** (kualiti commit, PR, board, merge bersih) | 15% |
| **Dokumentasi** (URS, ERD, diagram, README modul) | 10% |
| Pembentangan demo | 5% |

> Peserta yang menyiapkan lab treknya, menyumbang kepada gabungan Hari 15, dan membentangkan demo menerima **Sijil Penyertaan** — *Pembangunan Sistem Dalaman NRES Dengan ASP.NET Core*.

## Pemetaan Sesi → Deliverable

| Hari | Deliverable / Artifak |
|------|------------------------|
| 1 | `docs/URS-modul-N.md`, `use-case-modul-N.md`, `erd-modul-N.md`, `soalan-terbuka-modul-N.md` (4 set) |
| 2 | Repo + 4 cabang kumpulan · board Agile berisi backlog · `docs/kumpulan-N/` (RINGKASAN, pemetaan-jira, nota-ai, **kontrak**) · persekitaran .NET 10 sedia |
| 3 | `master` + asas kongsi + migration `InitialShared` · 4 cabang dibuka |
| 4 | Skema & skrin pertama setiap modul (4 modul) |
| 5–6 | Borang + peraturan perniagaan setiap modul |
| 7–9 | Aliran kelulusan & skrin admin setiap modul |
| 10–12 | Notifikasi, laporan & dashboard setiap modul |
| 13–14 | `Nres.Onboarding.Tests` + kod dibersihkan + cabang sedia gabung |
| 15 | `master` bersepadu 4 modul · Papan Pemuka Induk · laporan SIT · demo |
