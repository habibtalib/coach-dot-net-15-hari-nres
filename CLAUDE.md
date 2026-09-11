# CLAUDE.md — Panduan Repo Kursus DOTNET-NRES-15

Repo bahan latihan **coaching .NET 15 hari** untuk NRES. Bukan aplikasi pengeluaran — ia **bahan pengajaran**: nota, lab hands-on, nota penceramah, slaid, dan projek rujukan.

## Apa repo ini

- Membina **satu** aplikasi latihan `Nres.Onboarding.Web` (ASP.NET Core MVC, .NET 10) merentas **4 modul** NRES.
- Model penyampaian: **4 kumpulan dedicated bekerja selari**, bukan satu kohort kumulatif.
  - **Fasa 1 (Hari 1–3, bersama):** perancangan/URS/ERD → Git/Agile/kolaborasi → refresher .NET + asas kongsi.
  - **Fasa 2 (Hari 4–14, 4 trek selari):** setiap kumpulan membina modulnya pada cabang Git sendiri. Blok: `hari-4`, `hari-5-6`, `hari-7-9`, `hari-10-12`, `hari-13-14`.
  - **Fasa 3 (Hari 15, bersama):** merge 4 cabang, Papan Pemuka Induk, SIT/UAT, demo.
- Peserta **bina dari kosong** mengikut `snippets/lab.md`; `projek/` ialah rujukan penuh untuk **banding**.

## Sumber kebenaran (baca sebelum edit kandungan)

1. [`SPEC-KURSUS.md`](./SPEC-KURSUS.md) — **kanun tunggal**: model penyampaian, nama entiti, `SubmissionStatus`, peranan, prefix rujukan, cabang Git, pemetaan 15 hari, format fail. **Semua kandungan mesti patuh.**
2. [`JADUAL.md`](./JADUAL.md) — aturcara rasmi (3 fasa, waktu). Jangan ubah skop hari tanpa menyemak.
3. [`KOLABORASI.md`](./KOLABORASI.md) — kontrak pasukan: pemilikan fail, slot migration, PR/review, DoD.
4. [`AGENTS.md`](./AGENTS.md) — konteks AI kongsi yang dibaca pembantu AI setiap kumpulan.
5. [`nota/10-rujukan-buku.md`](./nota/10-rujukan-buku.md) — pemetaan kursus → bab buku rujukan *C# 14 and .NET 10* (Mark J. Price, Packt 2025; repo [habibtalib/cs14net10](https://github.com/habibtalib/cs14net10)) + ciri C# 12/13/14 yang digunakan.
6. Repo jiran `../coach-nres/nres-dotnet-15-day-coaching-guide.md` — sumber domain penuh.
7. Sumber skop asal: `cadangan_silibus_coaching_15hari_NRES.docx` (cadangan silibus NRES).

## Struktur folder

```text
hari-1/ hari-2/ hari-3/            # Fasa 1 — sesi bersama
kumpulan-1-pentadbiran/            # Fasa 2 — trek K1: 3 projek
  lapor-diri/ pematuhan-pks/ pengurusan-kontrak/   # setiap satu ada 5 blok
kumpulan-2-pas-parkir-pelekat/     # (README trek + 5 blok: hari-4 … hari-13-14)
kumpulan-3-id-ad-email/
kumpulan-4-tempahan-fasiliti-sukan/
hari-15/                           # Fasa 3 — sesi bersama
_arkib/kumulatif-15-hari/          # struktur lama + PKS (jangan sunting)
```

## Arahan (projek rujukan, slaid)

Repo ini **bahan pengajaran** — kebanyakan fail ialah Markdown, tiada langkah "build" untuk kandungan. Dua bahagian sahaja yang boleh dilaksanakan:

**Projek rujukan `.NET 10`** (`projek/Nres.Onboarding.Web/` — ASP.NET Core MVC + EF Core 10 + SQLite + Identity). Perlu **.NET 10 SDK** (`dotnet --version` → `10.x`):

```bash
cd projek/Nres.Onboarding.Web
dotnet restore
dotnet build                       # patut 0 ralat
dotnet ef database update          # cipta App_Data/nres.db (pilihan — startup pun migrate)
dotnet run                         # buka URL yang dipapar
dotnet ef migrations add <Nama>    # semakan kewarasan: migration patut KOSONG jika model konsisten
```

- `dotnet ef` belum ada → `dotnet tool install --global dotnet-ef`.
- Akaun demo seed: `applicant@nres.demo` / `hradmin@nres.demo`, kata laluan `Password123!`.
- **Tiada projek ujian** dalam `projek/` — ujian xUnit (`Nres.Onboarding.Tests`) ialah kandungan yang peserta bina pada blok Hari 13–14. Bila menulis lab ujian, semak sintaks xUnit terhadap .NET 10.

**Slaid** (`slides/`):

```bash
cd slides
python3 -m venv venv && ./venv/bin/pip install python-pptx   # sekali sahaja
./venv/bin/python build-pptx.py                              # jana dotnet-nres-training.pptx
```

HTML (`dotnet-nres-training.html`) dan PPTX diselaraskan: susunan `slide_*()` dalam `build-pptx.py` **mesti** sepadan dengan `<section class="slide">` dalam HTML. Ubah satu → cerminkan di kedua-dua. Sahkan: `grep -c '<section' dotnet-nres-training.html` = bilangan `slide_*()`.

## Seni bina projek rujukan (baca sebelum menulis lab trek)

`projek/` ialah keadaan aplikasi **pada penghujung Hari 3**: asas kongsi + **Modul Lapor Diri** (Kumpulan 1) sahaja. Modul Kumpulan 2–4 sengaja tiada — peserta membinanya. Butiran penuh: [`projek/README.md`](./projek/README.md). Titik penting yang merentas banyak fail:

- **Satu induk kongsi `Submission`** + satu `SubmissionStatus`, satu `AuditLog`, satu `Attachment`, satu `ApprovalStep` untuk semua modul. Setiap modul ada jadual butiran sendiri (cth. `OfficerReportingApplications`) yang menunjuk kembali ke `Submissions`.
- **Seni bina anti-konflik** — sebab utama projek wujud. Tiga fail yang biasanya punca konflik direka supaya **tidak perlu disentuh** oleh trek:
  - `Program.cs` — modul daftar melalui `Add<Modul>Module()` (baris berkomen; kumpulan nyahkomen **satu** baris sekali pada Hari 4). **BEKU.**
  - `Data/ApplicationDbContext.cs` — `OnModelCreating` hanya `ApplyConfigurationsFromAssembly(...)`; modul tambah `IEntityTypeConfiguration<T>` dalam folder sendiri. **BEKU.**
  - `Views/Shared/_Layout.cshtml` — `Component.InvokeAsync("ModuleNav")`; modul tambah `IModuleDescriptorProvider`. **BEKU.**
- **`IWorkflowService` memiliki peraturan peralihan status** dan menulis audit secara atomik — status **tidak** ditukar terus dalam controller.
- **`UserProfile` berasingan daripada `AspNetUsers`** (Identity = authentication sahaja); ViewModel (bukan entiti) dalam borang untuk elak over-posting; fail dimuat naik ke `App_Data/uploads` (DI LUAR `wwwroot`, nama fizikal = GUID, muat turun melalui action bersemak kebenaran).
- **Nota jujur:** `OfficerReportingController` ditulis sebelum `SubmissionControllerBase` wujud, jadi ia **tidak** mewarisinya — ini contoh kod yang *patut* direfactor, dan Kumpulan 2–4 ikut kelas asas.

## Konvensyen

- **Bahasa:** nota/agenda dalam **Bahasa Melayu**; kod, nama kelas, istilah teknikal dalam **Bahasa Inggeris**.
- **Struktur setiap folder sesi/blok:** `README.md` (konsep) + `snippets/lab.md` (hands-on langkah demi langkah) + `nota-penceramah.md` (nota penceramah).
- **Fokus:** ≥60% masa hands-on. Lab ialah bahagian paling penting — setiap latihan ada **Objektif**, langkah bernombor, blok kod penuh, dan **✅ Semakan**.
- **Kumulatif dalam trek:** setiap blok membina di atas blok sebelumnya *dalam modul yang sama*. Merentas kumpulan, hanya asas kongsi Hari 3 yang boleh diandaikan wujud.

## Bila menulis/menyunting kandungan

- Padankan gaya jiran `../kelas-flutter-5-hari/` & `../kelas-n8n-3-hari-jpj/` (README konsep + lab berangka + nota penceramah).
- Guna nama kelas/enum/prefix **tepat** seperti `SPEC-KURSUS.md`. Jika ragu, semak spec dahulu.
- Kod C# mesti sah untuk **.NET 10 / EF Core 10 / C# 14** (primary constructors, collection expressions, `dotnet ef` CLI, nullable reference types dihidupkan). Ciri C# 14 yang dibenarkan & yang dielakkan: lihat `AGENTS.md`.
- Setiap contoh kod hendaklah **boleh ditaip & dijalankan** — bukan pseudo-kod.
- **Setiap blok trek mesti menyisip benang kolaborasi:** semakan "sudah wujud?" sebelum menulis helper, sempadan folder kumpulan, slot migration bila skema berubah, semakan silang AI harian, gabungan latihan di hujung blok, dan Definition of Done. Ini bukan nota sampingan — ia sebahagian lab.
- Modul kumpulan **hanya** mencipta fail dalam folder modulnya; fail kongsi beku selepas Hari 3 (modul mendaftar diri melalui `Add<Modul>Module()` + `IEntityTypeConfiguration<T>`).

## Slaid

- `slides/dotnet-nres-training.html` — dek self-contained (buka dalam pelayar).
- `slides/build-pptx.py` + `_pptx_lib.py` — jana `.pptx` (venv + `python-pptx`). Kandungan slaid disimpan sebagai data (deterministik).

## Jangan

- Jangan simpan kata laluan sebenar dalam modul ID/AD/Email (ajar peserta **jangan** — ini titik pengajaran keselamatan).
- Jangan guna data NRES sebenar — semua contoh **sintetik**.
- Jangan tukar `SubmissionStatus`, peranan, prefix rujukan, atau nama cabang tanpa mengemas kini `SPEC-KURSUS.md` dahulu.
- **Pematuhan PKS kini DALAM skop** (PKS = **Polisi Keselamatan Siber**, bukan "Kod Setia") — projek ke-2 Kumpulan 1 (`kumpulan-1-pentadbiran/pematuhan-pks/`, prefix `PKS`, peranan `IctSecurityOfficer`). Rujuk `SPEC-KURSUS.md`.
- Jangan sunting fail dalam `_arkib/` — betulkan dalam struktur aktif.
- Jangan cipta servis/komponen kongsi baharu dalam kandungan trek — guna daftar dalam `AGENTS.md`.
