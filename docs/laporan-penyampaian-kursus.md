% Laporan Penyampaian Kerja — Pembangunan Bahan Kursus DOTNET-NRES-15
% Modul QA Dipandu-AI, Prompt Harian & Nota Konsep

---

## Maklumat tuntutan

| Perkara | Butiran |
|---------|---------|
| Tajuk projek | Pembangunan bahan latihan *coaching* .NET 15 hari untuk NRES |
| Skop laporan | Modul QA dipandu-AI (Hari 13–15), prompt am harian (Hari 1–15), nota konsep, kemas kini kanun kursus |
| Disediakan oleh | _______________________________ |
| Untuk / Agensi | _______________________________ |
| Rujukan tuntutan | _______________________________ |
| Tempoh kerja | _______________________________ |
| Tarikh laporan | 11 September 2026 |

> Laporan ini memperincikan kerja **penyampaian sebenar** bagi tujuan sokongan tuntutan. Semua data contoh dalam bahan kursus adalah **sintetik** (bukan rekod NRES sebenar).

---

## 1. Ringkasan eksekutif

Kerja yang dilaporkan meliputi pembangunan satu **modul jaminan kualiti (QA) dipandu kecerdasan buatan** untuk kursus *coaching* .NET 15 hari, bersama bahan sokongan pembelajaran merentas kesemua 15 hari. Penyampaian merangkumi persona AI penguji, skill semakan ujian boleh guna semula, lab hands-on penuh, pustaka prompt berkod, latihan tambahan dalam blok hari sedia ada, prompt permulaan-hari untuk setiap hari kursus, serta penerbitan nota konsep rujukan.

Kaedah kerja mengikut kitaran berdisiplin: **perancangan → pelaksanaan → semakan kualiti**, dengan setiap bahan dipadankan dengan kanun kursus (`SPEC-KURSUS.md`, `JADUAL.md`, `KOLABORASI.md`) dan dek slaid rasmi.

**Jumlah penyampaian:** 1 persona AI, 1 skill, 1 lab penuh, 4 prompt QA berkod, 7 latihan tambahan dalam blok hari, 9 prompt am harian, 12 nota konsep diterbitkan, dan 2 fail kanun dikemas kini.

---

## 2. Konteks projek

Kursus membina **satu aplikasi latihan ASP.NET Core MVC (.NET 10)** merentas 4 modul NRES, disampaikan melalui model **4 kumpulan dedicated bekerja selari**:

- **Fasa 1 (Hari 1–3, bersama)** — perancangan/URS/ERD, Git/Agile/kolaborasi, refresher .NET + asas kongsi.
- **Fasa 2 (Hari 4–14, 4 trek selari)** — setiap kumpulan membina modulnya pada blok: Hari 4, 5–6, 7–9, 10–12, 13–14.
- **Fasa 3 (Hari 15, bersama)** — integrasi, Papan Pemuka Induk, SIT/UAT, demo capstone.

Kerja dalam laporan ini menyokong terutamanya **blok ujian (Hari 13–14)** dan **integrasi/SIT/UAT (Hari 15)**, serta menyediakan lapisan panduan AI merentas semua hari.

---

## 3. Skop kerja & metodologi

Kerja dijalankan dalam tiga fasa berturutan:

1. **Perancangan** — penerokaan menyeluruh terhadap struktur repo dan kanun kursus untuk memastikan bahan baharu mengikut gaya sedia ada; penetapan skop dan keputusan reka bentuk (jenis artifak, pembahagian ujian ikut hari, penjajaran dengan dek slaid); penyediaan pelan terperinci yang disahkan sebelum pelaksanaan.
2. **Pelaksanaan** — pembinaan setiap artifak mengikut pelan, dengan kod contoh yang **boleh ditaip & dijalankan** (bukan pseudo-kod) dan disahkan padan dengan jenis sebenar aplikasi rujukan.
3. **Semakan kualiti** — pengesahan struktur (frontmatter sah, pautan silang berfungsi), penjajaran doktrin ujian dengan slaid, dan pemeriksaan konsistensi bahasa (nota Bahasa Melayu; kod/istilah English).

**Doktrin ujian yang dipatuhi** (selaras dek kursus): xUnit dengan **SQLite in-memory** (bukan penyedia `UseInMemoryDatabase` yang mengabaikan kekangan unik); menguji **peralihan status permohonan, penjanaan nombor rujukan, semakan pendua, dan kebenaran peringkat objek**; pengasingan **Hari 13–14 (ujian xUnit)** daripada **Hari 15 (SIT/UAT pre-check)**; dan penegasan bahawa **pre-check bukan UAT sebenar** — UAT sebenar dijalankan oleh pengguna akhir NRES terhadap keperluan mereka.

---

## 4. Butiran penyampaian

### 4.1 Persona AI penguji — `qa-uat`
Satu subagent khusus untuk kerja QA dipandu-AI, dengan capaian alat yang dihadkan secara sengaja (boleh menulis ujian dan memandu pelayar, tetapi tidak menulis kod ciri). Merangkumi lima langkah berkod:
- Menulis ujian xUnit peraturan kritikal (Hari 13–14);
- Menjalankan set ujian dan melaporkan keputusan;
- Memandu pre-check aliran hujung-ke-hujung melalui pelayar (Hari 15);
- Semakan kawalan capaian berasaskan peranan (RBAC) — capaian salah peranan mesti ditolak;
- Serahan laporan lulus/gagal dengan peringatan pre-check ≠ UAT sebenar.

### 4.2 Skill semakan ujian — `/uji-modul`
Senarai semak ujian xUnit boleh guna semula (dipanggil sebagai perintah), meliputi peraturan kritikal yang wajib diuji, penggunaan SQLite in-memory, ujian kebenaran melalui POST terus, penggunaan `[Fact]`/`[Theory]`, dan pengesahan set ujian hijau.

### 4.3 Lab hands-on penuh — QA dipandu-AI: Ujian xUnit & SIT/UAT Pre-Check
Lab lengkap empat latihan bergaya rumah (Objektif → langkah bernombor → blok kod penuh → senarai semak):
1. Mencipta persona `qa-uat` (mengajar konsep had capaian alat);
2. Mencipta skill `/uji-modul`;
3. Menulis & menjalankan ujian xUnit dengan bantuan AI (Hari 13–14), termasuk pembantu pangkalan data SQLite in-memory dan ujian peralihan status berparameter;
4. Menjalankan pre-check SIT/UAT melalui pelayar (Hari 15) — log masuk, mengisi borang, kelulusan, jejak audit, dan semakan RBAC.

Disertakan seksyen pustaka prompt sedia-guna, senarai masalah biasa, dan rujukan.

### 4.4 Pustaka prompt QA berkod
Empat prompt berkod ditambah ke pustaka prompt kursus, lengkap dengan indeks:
- **UJI-01** — tulis & jalankan ujian xUnit;
- **UJI-02** — pre-check SIT/UAT melalui pelayar;
- **UJI-03** — semakan RBAC (capaian salah peranan mesti ditolak);
- **UJI-04** — mulakan ujian unit dari kosong bagi modul yang belum mempunyai sebarang ujian.

### 4.5 Latihan tambahan dalam blok hari
Latihan "Pemecut QA dipandu-AI" disisipkan ke dalam lab hands-on tujuh blok:
- Enam blok Hari 13–14 (satu bagi setiap trek/sub-trek), dengan prompt khusus modul masing-masing (status & nombor rujukan, semakan pendua, RBAC dua peringkat, pertindihan slot, dan sebagainya);
- Satu latihan pre-check SIT/UAT dipandu-AI dalam blok Hari 15.

### 4.6 Prompt am permulaan-hari (Hari 1–15)
Sembilan fail prompt am (satu bagi setiap folder hari) yang mengorientasi pembantu AI ke konteks kursus dan matlamat hari — merangkumi Hari 1 (perancangan/URS/ERD), 2 (Git/Agile), 3 (asas kongsi), 4 (skema & skrin pertama), 5–6 (borang & peraturan), 7–9 (kelulusan & admin), 10–12 (notifikasi/laporan/dashboard), 13–14 (ujian), dan 15 (integrasi/SIT/UAT).

### 4.7 Nota konsep rujukan
Dua belas nota konsep ringkas Bahasa Melayu diterbitkan sebagai bahan latar belakang: persediaan .NET 10, pengenalan ASP.NET Core MVC, EF Core & migrations, corak aliran kerja, validation & view model, Identity/peranan/authorization, muat naik fail selamat, ujian xUnit, deployment, keselamatan, pemetaan buku rujukan, dan alatan Fasa 1.

### 4.8 Kemas kini kanun kursus
- **Panduan repo** — ditambah bahagian arahan build/run/test dan penerangan seni bina aplikasi rujukan.
- **Konteks AI kongsi** — persona `qa-uat` dan skill `/uji-modul` didaftarkan dalam registri supaya setiap kumpulan menemuinya.

---

## 5. Pemetaan liputan 15 hari

| Hari | Fasa | Tema | Bahan disampaikan |
|------|------|------|-------------------|
| 1 | 1 | Perancangan, URS & ERD | Prompt am permulaan-hari |
| 2 | 1 | Git, Agile & kolaborasi | Prompt am permulaan-hari |
| 3 | 1 | Refresher .NET & asas kongsi | Prompt am permulaan-hari |
| 4 | 2 | Skema DB & skrin pertama | Prompt am permulaan-hari |
| 5–6 | 2 | Borang & peraturan perniagaan | Prompt am permulaan-hari |
| 7–9 | 2 | Aliran kelulusan & skrin admin | Prompt am permulaan-hari |
| 10–12 | 2 | Notifikasi, laporan & dashboard | Prompt am permulaan-hari |
| 13–14 | 2 | Ujian, refactor & sedia gabung | Prompt am + latihan Pemecut QA (6 blok) + lab + persona + skill + prompt UJI-01/04 |
| 15 | 3 | Integrasi, SIT/UAT & demo | Prompt am + latihan pre-check SIT/UAT dipandu-AI + prompt UJI-02/03 |

---

## 6. Ringkasan kuantiti penyampaian

| Item penyampaian | Kuantiti |
|------------------|----------|
| Persona AI penguji (subagent) | 1 |
| Skill semakan ujian | 1 |
| Lab hands-on penuh (4 latihan) | 1 |
| Prompt QA berkod (UJI-01…04) | 4 |
| Latihan tambahan dalam blok hari | 7 |
| Prompt am permulaan-hari (Hari 1–15) | 9 |
| Nota konsep rujukan diterbitkan | 12 |
| Fail kanun kursus dikemas kini | 2 |
| **Jumlah artifak dihasil/dikemas kini** | **37** |

---

## 7. Kawalan kualiti & pengesahan

- **Penjajaran kanun:** setiap bahan dipadankan dengan `SPEC-KURSUS.md`, `JADUAL.md`, `KOLABORASI.md` dan dek slaid rasmi.
- **Ketepatan teknikal:** blok kod ujian disahkan padan dengan jenis sebenar aplikasi rujukan (enum status, servis aliran kerja, servis nombor rujukan, pembina konteks pangkalan data).
- **Integriti struktur:** frontmatter persona/skill sah dan boleh dimuatkan; pautan silang antara dokumen berfungsi.
- **Konsistensi bahasa:** nota dan penerangan dalam Bahasa Melayu; kod, nama kelas dan istilah teknikal dalam Bahasa Inggeris.
- **Nota kejujuran:** pembinaan penuh (`dotnet build`/`dotnet test`) dan aliran pelayar sebenar belum dijalankan secara langsung dalam tempoh penyediaan bahan; blok kod disahkan pada peringkat padanan jenis. Pengesahan larian penuh dilakukan semasa penyampaian bilik kursus.

---

## 8. Rujukan luaran

Sumber rujukan teknikal rasmi yang berkaitan dengan bahan yang disampaikan (bukan pautan repositori dalaman):

| Topik | Sumber rujukan |
|-------|----------------|
| .NET 10 & C# 14 | Microsoft Learn — learn.microsoft.com/dotnet |
| ASP.NET Core MVC | Microsoft Learn — learn.microsoft.com/aspnet/core |
| ASP.NET Core Identity & authorization | Microsoft Learn — learn.microsoft.com/aspnet/core/security |
| EF Core & migrations | Microsoft Learn — learn.microsoft.com/ef/core |
| Ujian EF Core (SQLite in-memory) | Microsoft Learn — learn.microsoft.com/ef/core/testing/testing-without-the-database |
| Rangka kerja ujian xUnit | xunit.net |
| Model Context Protocol (MCP) | modelcontextprotocol.io |
| Alat AI Claude Code | docs.claude.com/en/docs/claude-code |
| Buku rujukan kursus | *C# 14 and .NET 10* — Mark J. Price (Packt Publishing, 2025) |

---

## 9. Pengesahan

| | Nama | Tandatangan | Tarikh |
|--|------|-------------|--------|
| Disediakan oleh | ____________________ | ____________________ | __________ |
| Disemak / Disahkan oleh | ____________________ | ____________________ | __________ |

---

*Laporan penyampaian kerja untuk sokongan tuntutan. Bahan kursus menggunakan data contoh sintetik sahaja.*
