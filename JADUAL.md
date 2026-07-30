# Aturcara Rasmi — Latihan *Coaching* Pembangunan Sistem Dalaman NRES Dengan ASP.NET Core (.NET 10)

> **Sumber rasmi:** Pelan Coaching 15 Hari NRES (`DOTNET-NRES-15`). Modul ini **mengikut** aturcara ini — jangan ubah skop hari tanpa menyemaknya. Rujukan domain penuh: repo jiran `coach-nres/`. Kanun teknikal tunggal: [`SPEC-KURSUS.md`](./SPEC-KURSUS.md).
>
> **Tajuk penuh:** *Membina Sistem Onboarding & Khidmat Dalaman NRES* — satu aplikasi ASP.NET Core MVC yang menyatukan 5 modul permohonan & aliran kerja kelulusan, dibina **dari kosong secara hands-on** sepanjang 15 hari.

## Maklumat Sesi

| Perkara | Butiran |
|---------|---------|
| **Kod Kursus** | DOTNET-NRES-15 |
| **Tempoh** | 15 Hari (105 Jam) |
| **Tahap** | Pertengahan (Intermediate) — *asas C# / OOP disyorkan* |
| **Mod** | Fizikal / Maya / Hibrid — **berpaksikan lab** (≥60% masa hands-on) |
| **Masa** | 9.00 pagi – 5.00 petang |
| **Anjuran** | Kementerian Sumber Asli & Kelestarian Alam (NRES) |
| **Bilangan peserta disyorkan** | 12 – 20 orang |
| **Rangka** | ASP.NET Core MVC · .NET 10 LTS · EF Core 10 · Identity · SQLite → SQL Server |

> **Rentak harian:** Pendaftaran & minum pagi **8.30–9.00**; sesi pagi **9.00–1.00**; rehat & makan tengah hari **1.00–2.30**; sesi petang **2.30–5.00**; bersurai **5.00 petang**. ~7 jam kontak/hari.

> **Konvensyen bahasa:** Nota & penerangan dalam **Bahasa Melayu**; kod, nama kelas, istilah teknikal (`Controller`, `DbContext`, `migration`) dikekalkan dalam **Bahasa Inggeris** (amalan standard industri .NET).

> **Projek tunggal:** Semua 15 hari membina **satu** aplikasi — `Nres.Onboarding.Web` — secara **kumulatif**. Setiap hari menambah di atas hari sebelumnya.

---

## 5 Modul (Kes Guna NRES)

1. **Modul Lapor Diri** — Pengurusan permohonan laporan diri pekerja baharu
2. **Modul Pas, Parking & Pelekat Kenderaan** — Pengurusan akses kawasan dan kenderaan
3. **Modul ID, AD & Email** — Pengurusan permohonan akaun pengguna sistem
4. **Modul PKS (Pematuhan Kod Setia)** — Pengisytiharan dan pemantauan pematuhan polisi
5. **Modul Aset ICT** — Pengurusan permohonan dan pinjaman aset ICT

---

## Ringkasan 15 Hari

| Hari | Modul | Fokus | Hasil hands-on |
|------|-------|-------|----------------|
| [**1**](./hari-1/) | Asas | Persediaan `dotnet`, seni bina, entiti kongsi, migration pertama | Aplikasi ASP.NET Core berjalan + DB tersambung |
| [**2**](./hari-2/) | 1 · Lapor Diri | Borang create/edit, view model, validation, draf | Borang Lapor Diri boleh cipta/edit/simpan draf |
| [**3**](./hari-3/) | 1 · Lapor Diri | Lampiran, submit + nombor rujukan, semakan HR, audit | Modul 1 lengkap hujung-ke-hujung |
| [**4**](./hari-4/) | 2 · Pas/Parking/Pelekat | Model 3 jenis + `Vehicle`, migration | Jadual & skrin awal Modul 2 |
| [**5**](./hari-5/) | 2 · Pas/Parking/Pelekat | Borang + conditional validation + semakan pendua | 3 borang simpan data sah, sekat pendua |
| [**6**](./hari-6/) | 2 · Pas/Parking/Pelekat | Kelulusan, filter, print summary | Modul 2 ada aliran kelulusan + cetakan |
| [**7**](./hari-7/) | 3 · ID/AD/Email | Discovery, `AccountRequest`, akses sistem, peranan | Model & skrin awal Modul 3 |
| [**8**](./hari-8/) | 3 · ID/AD/Email | Rantaian kelulusan berbilang langkah + authorization | Aliran Applicant→Supervisor→ICT |
| [**9**](./hari-9/) | 3 · ID/AD/Email | Notifikasi, carian/filter, panel audit | Modul 3 lengkap + notifikasi |
| [**10**](./hari-10/) | 4 · PKS | `PolicyVersion`, checklist, model declaration, seed | Jadual PKS + checklist berseed |
| [**11**](./hari-11/) | 4 · PKS | Borang checklist dinamik + kunci declaration | Declaration boleh isi & terkunci selepas hantar |
| [**12**](./hari-12/) | 4 · PKS | Semakan admin, filter, CSV export | Modul 4 lengkap + eksport CSV |
| [**13**](./hari-13/) | 5 · Aset ICT | Model aset/perisian/pinjaman/pemulangan + seed | Model & lookup Modul 5 |
| [**14**](./hari-14/) | 5 · Aset ICT | Borang + semakan availability + transaksi inventori | Aliran pinjaman & pemulangan berfungsi |
| [**15**](./hari-15/) | Integrasi | Integrasi, ujian xUnit, deployment, demo | Aplikasi bersepadu + demo akhir |

---

## HARI 1 — Persediaan Projek & Seni Bina Kongsi

**Fokus:** Faham bentuk keseluruhan sistem (bukan sekadar borang digital, tetapi *request workflow system*), dan cipta projek ASP.NET Core yang berjalan.

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran Peserta & Minum Pagi |
| **9.00 – 10.30 pagi** | **SESI 1: Gambaran Sistem NRES** — 5 modul, corak `Form → Draft → Submit → Review → Approve → Audit`, kenapa satu `Submission` induk dikongsi. 🧠 **Bengkel:** peta medan sama merentas 5 modul |
| **10.30 – 1.00 tgh** | **SESI 2: Cipta Projek ASP.NET Core** — `dotnet new mvc`, struktur folder, `Program.cs`, pakej EF Core + Identity. 💻 **Lab:** projek berjalan + halaman utama |
| 1.00 – 2.30 petang | Rehat dan Makan Tengah Hari |
| **2.30 – 3.45 petang** | **SESI 3: Entiti Kongsi & DbContext** — `Submission`, `Attachment`, `AuditLog`, `UserProfile`, `SubmissionStatus`. 💻 **Lab:** tulis entiti + `ApplicationDbContext` |
| **3.45 – 5.00 petang** | **SESI 4: Migration Pertama** — `dotnet ef migrations add`, `dotnet ef database update`, sahkan skema SQLite. 💻 **Lab:** DB dicipta + navigasi modul placeholder |
| 5.00 petang | Bersurai |

**Hasil Hari 1:** Aplikasi ASP.NET Core berjalan, DB tersambung, migration pertama wujud, peserta boleh terangkan kelima-lima modul.

---

## HARI 2 — Lapor Diri: Borang & Validation

**Fokus:** Bina borang create/edit pertama dengan view model & server-side validation.

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 5–6: Borang Lapor Diri** — `OfficerReportingApplication`, `OfficerReportingCreateViewModel`, controller `Index/Create/Edit/Details`, Razor view, DataAnnotations. 💻 **Lab:** borang boleh cipta & edit |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 7: Validation & Draf** — validation summary, simpan draf (data tidak lengkap dibenarkan), asingkan view model vs entiti. 💻 **Lab:** validation lengkap + simpan draf |
| 5.00 petang | Bersurai |

**Hasil Hari 2:** Lapor Diri boleh dicipta, disunting, disahkan, dan disimpan sebagai draf.

---

## HARI 3 — Lapor Diri: Lampiran, Submit & Semakan

**Fokus:** Lengkapkan modul pertama hujung-ke-hujung (lampiran → submit → semakan HR → audit).

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 8–9: Muat Naik Lampiran** — `IFileStorageService`, simpan di `App_Data/uploads/{id}/`, `Attachment` metadata, validasi saiz/jenis, nama fail selamat. 💻 **Lab:** muat naik + simpan metadata |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 10: Submit & Semakan HR** — `IReferenceNumberService` (`LD-2026-####`), tukar status ke `Submitted`, audit log, halaman semakan HR, approve/reject (wajib sebab). 💻 **Lab:** aliran penuh Modul 1 |
| 5.00 petang | Bersurai |

**Hasil Hari 3:** Lapor Diri menyokong draf, submit, lampiran, approve, reject, dan audit log.

---

## HARI 4 — Pas/Parking/Pelekat: Pemodelan

**Fokus:** Model satu modul dengan **tiga** jenis permohonan berkongsi `Submission` induk.

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 11–12: Model 3 Jenis** — `AccessPassApplication`, `VehicleStickerApplication`, `ParkingApplication`, `Vehicle` (staf boleh >1 kenderaan). 💻 **Lab:** entiti + relasi |
| **2.30 – 5.00 petang** | **SESI 13: Migration & Skrin Awal** — halaman landing modul, laluan 3 jenis, migration. 💻 **Lab:** migration + navigasi modul |

**Hasil Hari 4:** Jadual & skrin awal Modul 2 wujud.

---

## HARI 5 — Pas/Parking/Pelekat: Borang & Peraturan

**Fokus:** Conditional validation & semakan pendua (duplicate active application).

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 14–15: 3 Borang** — pas keselamatan, pelekat kenderaan, parkir; medan `Vehicle` dikongsi. 💻 **Lab:** bina 3 borang |
| **2.30 – 5.00 petang** | **SESI 16: Peraturan Perniagaan** — satu pas aktif/pemohon, satu pelekat aktif/kenderaan, parkir khas perlu justifikasi. 💻 **Lab:** conditional validation + `AnyAsync` duplicate check |

**Hasil Hari 5:** Ketiga-tiga borang simpan data sah & sekat pendua.

---

## HARI 6 — Pas/Parking/Pelekat: Kelulusan & Cetakan

**Fokus:** Semakan admin, penapisan (filter) operasi, dan print summary.

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 17–18: Senarai & Kelulusan Admin** — halaman senarai admin, filter (jenis, status, jabatan, julat tarikh), halaman detail, approve/reject (wajib sebab), prefix `PAS`/`PKR`/`STK`. 💻 **Lab:** aliran kelulusan |
| **2.30 – 5.00 petang** | **SESI 19: Print Summary** — Razor print view, `@media print`, ringkasan boleh cetak. 💻 **Lab:** cetakan + audit |

**Hasil Hari 6:** Modul 2 ada aliran kelulusan & ringkasan boleh cetak.

---

## HARI 7 — ID/AD/Email: Discovery & Model

**Fokus:** Faham aliran permohonan akaun ICT + kekangan keselamatan (**jangan sesekali simpan kata laluan**).

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 20–21: Jenis Permohonan** — akaun AD baharu, email, kemas kini akaun, nyahaktif, akses sistem tambahan; `AccountRequest`, `RequestedSystemAccess`, `ApprovalStep`. 💻 **Lab:** model + seed jenis akses |
| **2.30 – 5.00 petang** | **SESI 22: Dashboard Modul ICT** — skrin awal, seed access types (AD, Email, Shared folder, VPN, Sistem dalaman). 💻 **Lab:** dashboard + migration |

**Hasil Hari 7:** Model permohonan akaun & skrin awal wujud.

---

## HARI 8 — ID/AD/Email: Rantaian Kelulusan & Authorization

**Fokus:** Kelulusan berbilang langkah + role-based authorization sebenar.

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 23–24: Borang & Aliran** — `Applicant Draft → Submitted → SupervisorApproved → Completed`; borang permohonan, skrin kelulusan Supervisor, skrin proses ICT. 💻 **Lab:** aliran 3 peringkat |
| **2.30 – 5.00 petang** | **SESI 25: Authorization** — `[Authorize(Roles=...)]`, `IWorkflowService` semak peralihan status. 💻 **Lab:** kuatkuasa peranan pada controller |

**Hasil Hari 8:** Permohonan akaun menyokong hantar → kelulusan Supervisor → penyempurnaan ICT.

---

## HARI 9 — ID/AD/Email: Notifikasi, Carian & Audit

**Fokus:** Struktur notifikasi, carian/penapisan, dan sejarah audit.

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 26–27: Notifikasi** — `INotificationService` + `ConsoleNotificationService`, cetus pada submit/approve/reject/complete. 💻 **Lab:** hook notifikasi |
| **2.30 – 5.00 petang** | **SESI 28: Carian & Audit** — carian ikut rujukan/pemohon/jabatan/status/jenis, panel audit pada halaman detail. 💻 **Lab:** carian + audit panel |

**Hasil Hari 9:** Modul 3 ada notifikasi, carian, filter, dan sejarah audit.

---

## HARI 10 — PKS: Model Pematuhan

**Fokus:** Model checklist & pengisytiharan (declaration) berpaksikan versi polisi.

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 29–30: Model PKS** — `PolicyVersion`, `ComplianceChecklistItem`, `ComplianceDeclaration`, `ComplianceResponse`; simpan versi polisi dengan setiap declaration. 💻 **Lab:** entiti PKS |
| **2.30 – 5.00 petang** | **SESI 31: Seed Data** — seed versi polisi & item checklist dalam DB. 💻 **Lab:** seed + migration |

**Hasil Hari 10:** Jadual PKS & item checklist berseed wujud.

---

## HARI 11 — PKS: Borang Checklist Dinamik & Kunci

**Fokus:** Borang checklist dijana dari DB + kunci declaration selepas hantar.

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 32–33: Borang Dinamik** — muat item checklist aktif dari DB, render dalam Razor, `ComplianceDeclarationViewModel` + senarai respons. 💻 **Lab:** borang checklist dinamik |
| **2.30 – 5.00 petang** | **SESI 34: Simpan & Kunci** — simpan semua respons dalam satu transaksi, sahkan akuan (acknowledgement), kunci edit selepas `Submitted`. 💻 **Lab:** submit + lock |

**Hasil Hari 11:** Declaration PKS boleh dilengkap & terkunci selepas hantar.

---

## HARI 12 — PKS: Semakan Admin & Laporan

**Fokus:** Semakan pematuhan, penapisan, dan CSV export.

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 35–36: Semakan** — halaman senarai admin, filter (jabatan, status, versi polisi, tarikh), halaman detail respons checklist, catatan ketidakpatuhan. 💻 **Lab:** semakan admin |
| **2.30 – 5.00 petang** | **SESI 37: CSV Export** — jana CSV pematuhan ikut jabatan/versi polisi. 💻 **Lab:** eksport CSV |

**Hasil Hari 12:** Modul 4 ada semakan, penapisan, dan CSV export.

---

## HARI 13 — Aset ICT: Pemodelan

**Fokus:** Model permohonan perisian, pinjaman aset, dan pemulangan aset.

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 38–39: Model Aset** — `Asset`, `SoftwareCatalogItem`, `SoftwareRequest`, `AssetLoanRequest`, `AssetReturn`; status permohonan vs status aset berbeza. 💻 **Lab:** entiti + relasi |
| **2.30 – 5.00 petang** | **SESI 40: Seed Katalog** — seed perisian & aset contoh, status aset `Available/OnLoan/...`. 💻 **Lab:** seed + migration |

**Hasil Hari 13:** Model & data lookup Modul 5 wujud.

---

## HARI 14 — Aset ICT: Borang, Kelulusan & Inventori

**Fokus:** Borang ICT + kemas kini inventori dengan selamat (transaksi).

| Masa | Agenda |
|------|--------|
| **9.00 – 1.00 tgh** | **SESI 41–42: 3 Borang** — permohonan perisian, pinjaman aset, pemulangan aset; semakan availability aset. 💻 **Lab:** bina 3 borang |
| **2.30 – 5.00 petang** | **SESI 43: Transaksi Inventori** — kelulusan & fulfillment ICT, `BeginTransactionAsync`, kemas kini status aset (`OnLoan`/`Available`/`UnderMaintenance`). 💻 **Lab:** transaksi selamat |

**Hasil Hari 14:** Aliran perisian, pinjaman & pemulangan aset berfungsi hujung-ke-hujung.

---

## HARI 15 — Integrasi, Ujian & Deployment

**Fokus:** Sambung semua modul, tulis ujian, deploy, dan demo akhir.

| Masa | Agenda |
|------|--------|
| **9.00 – 10.30 pagi** | **SESI 44: Integrasi** — navigasi kongsi, dashboard (draf saya, dihantar, menunggu kelulusan, selesai), carian rujukan global, menu ikut peranan. 💻 **Lab:** integrasi |
| **10.30 – 1.00 tgh** | **SESI 45: Ujian xUnit** — uji nombor rujukan, peralihan status, semakan pendua, availability aset, sebab reject wajib. 💻 **Lab:** unit + integration tests |
| **2.30 – 3.45 petang** | **SESI 46: Deployment** — `appsettings` per persekitaran, tukar SQLite → SQL Server, HTTPS, folder muat naik, IIS/Linux/kontena. 💻 **Lab:** senarai semak keluaran |
| **3.45 – 5.00 petang** | **Projek Capstone: Demo & Sijil** — skrip ujian manual 11 langkah, pembentangan, penilaian, penyampaian sijil |
| 5.00 petang | Bersurai |

**Hasil Hari 15:** Aplikasi NRES bersepadu 5 modul, berujian, boleh-deploy, dibentangkan.

---

## Kriteria Penilaian (Capstone)

| Kriteria | Wajaran |
|----------|---------|
| Modul lengkap & berfungsi (5 modul) | 30% |
| Corak aliran kerja betul (draft→submit→approve→audit) | 20% |
| Validation, authorization & keselamatan | 20% |
| Ujian (xUnit) | 15% |
| Pembentangan & dokumentasi | 15% |

> Peserta yang menyiapkan semua lab, aliran 5 modul, ujian, dan pembentangan capstone menerima **Sijil Penyertaan** — *Pembangunan Sistem Dalaman NRES Dengan ASP.NET Core*.

## Pemetaan Sesi → Deliverable

| Hari | Deliverable / Artifak |
|------|------------------------|
| 1 | `Nres.Onboarding.Web` berjalan + migration `InitialShared` |
| 2–3 | Modul Lapor Diri lengkap (`OfficerReportingApplications`) |
| 4–6 | Modul Pas/Parking/Pelekat (`Vehicles`, 3 jadual permohonan) |
| 7–9 | Modul ID/AD/Email (`AccountRequests`, `RequestedSystemAccesses`) |
| 10–12 | Modul PKS (4 jadual pematuhan + CSV) |
| 13–14 | Modul Aset ICT (5 jadual aset/perisian) |
| 15 | `Nres.Onboarding.Tests` + senarai semak deployment |
