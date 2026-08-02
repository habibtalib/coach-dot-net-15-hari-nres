# Hari 12 — PKS: Semakan Admin & Laporan

Nota ini mengikut **HARI 12** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 35–37 (Modul 4: PKS). Lab hands-on penuh ada di [`snippets/lab.md`](./snippets/lab.md).

> **Konvensyen kod:** Nota dalam **Bahasa Melayu**; kod, nama kelas/pembolehubah, istilah teknikal dalam **Bahasa Inggeris** — rujuk [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) untuk kanun nama entiti/enum/peranan/prefix.

> **Sambungan projek:** Kita **tidak** mula projek baharu. Hari ini kita tambah paparan **`ComplianceAdmin`** di atas `ComplianceDeclaration`/`ComplianceResponse` (Hari 10) dan aliran hantar (Hari 11) yang sudah wujud — ini melengkapkan Modul 4 hujung-ke-hujung.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Sort, filter, and page (ASP.NET Core + EF Core) | [learn.microsoft.com/aspnet/core/data/ef-rp/sort-filter-page](https://learn.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page) |
| LINQ — `Where`, komposisi `IQueryable` | [learn.microsoft.com/dotnet/csharp/linq/](https://learn.microsoft.com/en-us/dotnet/csharp/linq/) |
| Role-based authorization (`[Authorize(Roles = ...)]`) | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |
| Mengembalikan fail daripada Controller (`File()`) | [learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.file](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.file) |
| `StringBuilder` (jana teks CSV) | [learn.microsoft.com/dotnet/api/system.text.stringbuilder](https://learn.microsoft.com/en-us/dotnet/api/system.text.stringbuilder) |
| Model binding — parameter dari query string (`[FromQuery]`) | [learn.microsoft.com/aspnet/core/mvc/models/model-binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 35–36: Semakan** — halaman senarai admin, filter (jabatan, status, versi polisi, tarikh), halaman detail respons checklist, catatan ketidakpatuhan. 💻 **Lab:** semakan admin |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 37: CSV Export** — jana CSV pematuhan ikut jabatan/versi polisi. 💻 **Lab:** eksport CSV |
| 5.00 petang | Bersurai |

**Hasil Hari 12** (rujuk [`../JADUAL.md`](../JADUAL.md)): Modul 4 (PKS) lengkap hujung-ke-hujung — model, seed, borang dinamik, kunci, semakan admin, dan eksport CSV.

---

## Kenapa `ComplianceAdminController` berasingan daripada `ComplianceController` (Hari 11)?

`ComplianceController` (Hari 11) ialah **antara muka pemohon** — hanya boleh cipta/lihat declaration **milik sendiri**. `ComplianceAdminController` ialah **antara muka `ComplianceAdmin`** — boleh **lihat semua** declaration merentas jabatan, tapis mengikut pelbagai kriteria, dan buat keputusan semakan (lulus/tolak). Mengasingkan dua controller ini (bukan menambah logik admin ke dalam `ComplianceController` yang sama) mengikut prinsip **satu tanggungjawab** (*single responsibility*) dan membolehkan kawalan capaian (`[Authorize(Roles = "ComplianceAdmin")]`) dikuatkuasakan pada **peringkat kelas**, bukan disemak berulang kali dalam setiap kaedah.

## Kenapa penapisan jabatan (`Department`) merujuk `UserProfile`, bukan medan pada `ComplianceDeclaration` sendiri?

`UserProfile` (entiti kongsi sejak Hari 1) sudah menyimpan `Department` bagi setiap staf sebagai sebahagian daripada **profil rasmi** mereka — bukan sesuatu yang staf pilih sendiri semasa mengisytiharkan PKS. Menyimpan semula `Department` terus pada `ComplianceDeclaration` bermakna **dua** sumber kebenaran (*source of truth*) bagi jabatan seorang staf — jika staf itu berpindah jabatan selepas mengisytiharkan PKS, dua sumber ini boleh **tidak segerak** tanpa disedari. Sebaliknya, kita cari jabatan staf secara langsung melalui `Submission.ApplicantUserId` → `UserProfile.UserId` setiap kali senarai/laporan admin dijana — satu sumber kebenaran, sentiasa terkini.

## Kenapa keputusan semakan (`AdminApproved`/`Rejected`) guna `SubmissionStatus` kongsi, bukan status baharu khas PKS?

Ini mengikut keputusan seni bina teras Hari 1: **satu** `SubmissionStatus` untuk kelima-lima modul. Corak sejagat `Form → Validation → Draft → Submit → Review → Approve/Reject → Audit → Report` terpakai juga untuk PKS — selepas `ComplianceAdmin` menyemak checklist dan catatan ketidakpatuhan, mereka membuat keputusan **sama seperti** modul lain: `AdminApproved` (patuh, tiada tindakan lanjut) atau `Rejected` (tidak patuh, wajib catatan sebab — persis peraturan "rejection must require a reason" yang berulang setiap modul). Ini bermakna dashboard/laporan status merentas 5 modul (Hari 15) turut berfungsi untuk PKS tanpa kod tambahan.

## Kenapa eksport guna CSV, bukan Excel (`.xlsx`) atau PDF?

CSV (*Comma-Separated Values*) ialah format teks **paling ringkas** yang boleh dijana tanpa pakej NuGet tambahan (`StringBuilder` + `Encoding.UTF8` sahaja) dan boleh dibuka terus oleh Excel/Google Sheets untuk analisis lanjut oleh jabatan pematuhan. SPEC menetapkan format lajur tetap: `ReferenceNo,Applicant,Department,Status,DeclarationDate` — cukup untuk keperluan latihan. PDF (dengan format sijil/surat rasmi) boleh ditambah kemudian jika organisasi memerlukan salinan bercetak berformat khusus, tetapi itu di luar skop 15 hari kursus ini.

## Kenapa `IWorkflowService.CanTransition(from, to)` (Hari 8) digunakan semula di sini?

`IWorkflowService` (diperkenalkan Hari 8 untuk Modul 3) menyimpan peraturan peralihan status yang **sah** bagi `SubmissionStatus` — cth. `Submitted` hanya boleh beralih ke `SupervisorApproved`, `AdminApproved`, atau `Rejected`; **tidak** boleh terus melompat ke `Completed` tanpa melalui salah satu peringkat itu dahulu. Menggunakan semula servis ini (bukan menulis semakan `if`/`switch` baharu khas PKS) memastikan peraturan peralihan status **konsisten** merentas kelima-lima modul — jika peraturan itu berubah suatu hari nanti, ia hanya diubah di **satu** tempat.

> Rujukan rasmi: [learn.microsoft.com/aspnet/core/data/ef-rp/sort-filter-page](https://learn.microsoft.com/en-us/aspnet/core/data/ef-rp/sort-filter-page) · [learn.microsoft.com/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.file](https://learn.microsoft.com/en-us/dotnet/api/microsoft.aspnetcore.mvc.controllerbase.file)

---

Selesai baca bahagian konsep? Mula lab hands-on di [`snippets/lab.md`](./snippets/lab.md) — bina `ComplianceAdminController`, halaman senarai + filter, halaman detail + keputusan semakan, dan eksport CSV.

> 🎤 **Nota penceramah/jurulatih:** [`nota-penceramah.md`](./nota-penceramah.md).
