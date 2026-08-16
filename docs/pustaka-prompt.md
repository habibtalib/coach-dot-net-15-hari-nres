# Pustaka Prompt AI — kursus DOTNET-NRES-15

> **Bahan rujukan kursus.** Koleksi prompt piawai untuk **PRD**, **dokumentasi**, dan **diagram Mermaid** (serta UI/UX, Jira, semakan kod). Setiap prompt difailkan dengan **ID** supaya mudah dirujuk dalam lab dan code review.
>
> Versi: v1 · Bahasa: prompt dalam **Bahasa Melayu** (istilah teknikal English).

## Cara guna

1. Cari prompt ikut **ID** atau kategori dalam indeks.
2. Salin blok prompt, ganti `<...>` dan `[tampal ...]` dengan bahan anda (PRD/URS/ERD/diff).
3. Jalankan, kemudian **semak** output ikut baris *Selepas* — AI draf, **anda sahkan**.

### Peraturan emas (terpakai semua prompt)

- **Berpaksi sumber:** ikut PRD/URS/ERD & `AGENTS.md`/`SPEC-KURSUS.md`. **Jangan reka** keperluan atau entiti.
- **Bahasa Melayu** untuk nota/UI; **English** untuk kod & nama entiti.
- **Data sintetik** sahaja — jangan tampal data NRES sebenar.
- **Semakan manusia** wajib — tiada commit tanpa faham.

---

## Indeks

| ID | Prompt | Guna bila |
|----|--------|-----------|
| **PRD-01** | Draf PRD | Ada URS/keperluan; mula satu ciri/modul |
| **PRD-02** | Semak silang PRD | Selepas draf PRD |
| **DOK-01** | Jana dokumentasi modul | PRD siap; nak README modul |
| **DIA-01** | Diagram ERD (Mermaid) | Ada ERD; nak kod Mermaid |
| **DIA-02** | Carta alir proses (Mermaid) | Ada use case/PRD; nak flowchart |
| **DIA-04** | Use case (flowchart) | Nak gambaran aktor → fungsi |
| **DIA-05** | Aliran pengguna (user flow) | Nak langkah & keputusan pengguna |
| **DIA-06** | Perjalanan pengguna (journey) | Nak peringkat + kepuasan (UX) |
| **DIA-07** | Sequence diagram | Nak interaksi antara aktor/sistem |
| **DIA-08** | State diagram (`SubmissionStatus`) | Nak kitaran status permohonan |
| **DIA-03** | Semak silang ERD | Selepas ERD siap |
| **UI-01** | Reka UI/UX modul | PRD siap; mula skrin/borang |
| **JIRA-01** | Cipta isu Jira dari user story | Board tersambung (MCP); ada user story |
| **SMK-01** | Semakan pra-PR | Sebelum setiap Pull Request |
| **DEV-01** | Borang: ViewModel + validation | Reka borang (borang-dahulu) |
| **DEV-02** | Borang: View Razor | Selepas ViewModel |
| **DEV-03** | Controller: papar + simpan draf | Selepas borang |
| **DEV-04** | Validation pelayan + peraturan | Selepas controller draf |
| **DEV-05** | Entiti + migration (simpan) | Selepas borang betul |
| **DEV-06** | Aliran kelulusan (controller) | Blok kelulusan (Hari 7–9) |
| **DEV-07** | Ujian xUnit | Blok ujian (Hari 13–14) |

---

## A · PRD

### PRD-01 — Draf PRD

- **Tujuan:** Terjemah URS/keperluan kepada PRD boleh-bina (7 bahagian).
- **Input:** URS/keperluan modul anda.

```text
Bantu saya draf PRD untuk modul <nama modul>.
Susun ikut: masalah & matlamat; pengguna & peranan; skop & luar-skop;
user story + acceptance criteria; data & entiti; bukan-fungsi & polisi; soalan terbuka.
Jangan reka keperluan — jika tidak pasti, senaraikan sebagai soalan terbuka.
Guna Bahasa Melayu, ringkas dan jelas.

[tampal ringkasan keperluan / URS di sini]
```

- **Selepas:** Semak setiap bahagian; pastikan acceptance criteria **boleh diuji**; yang tak pasti → *Soalan terbuka*.
- **Rujukan:** [`contoh-prd-tempahan-fasiliti-sukan.md`](./contoh-prd-tempahan-fasiliti-sukan.md) · `hari-1` Latihan 6b.

### PRD-02 — Semak silang PRD

- **Tujuan:** Kesan keperluan/entiti direka atau AC tak boleh diuji.
- **Input:** PRD draf + URS + ERD.

```text
Bandingkan PRD ini dengan URS dan ERD kami (dilampirkan).
1. Adakah PRD memperkenalkan keperluan atau entiti yang tiada dalam URS/ERD?
2. Adakah setiap acceptance criteria boleh diuji?
3. Adakah apa-apa yang sepatutnya "luar-skop" tetapi tertinggal?
Senaraikan isu sahaja. JANGAN tulis semula PRD.
```

- **Selepas:** Betulkan setiap isu dengan tangan.
- **Rujukan:** `hari-1` Latihan 6b.

---

## B · Dokumentasi

### DOK-01 — Jana dokumentasi modul

- **Tujuan:** Hasilkan README modul ringkas dari PRD.
- **Input:** PRD modul.

```text
Berdasarkan PRD modul kami di bawah, tulis dokumentasi ringkas (docs/README-modul-N.md):
- gambaran modul & pengguna
- senarai fungsi utama
- aliran permohonan (langkah demi langkah)
Guna Bahasa Melayu, ringkas dan jelas. Jangan tambah ciri yang tiada dalam PRD.

[tampal PRD di sini]
```

- **Selepas:** Semak fakta; buang apa-apa yang direka di luar PRD.
- **Rujukan:** [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md).

---

## C · Diagram (Mermaid)

> Semua prompt diagram: minta **HANYA satu blok kod Mermaid**, tiada nota di dalam blok (nota AI diletak **di luar** blok). **FigJam** tidak menyokong **DIA-06** (`journey`) — render dalam VS Code/GitHub, atau guna **DIA-05** (`flowchart`) untuk FigJam. Detail & sokongan penuh: [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md) § C.

### DIA-01 — Diagram ERD (Mermaid)

- **Tujuan:** Kod Mermaid `erDiagram` untuk entiti & hubungan.
- **Input:** ERD modul anda.

```text
Berdasarkan ERD kami di bawah, beri kod Mermaid `erDiagram` untuk entiti utama & hubungan.
Beri HANYA satu blok kod Mermaid (tiada nota/teks lain di dalam blok) supaya saya boleh tampal terus. Jangan reka entiti baharu.

[tampal ERD di sini]
```

- **Selepas:** Sahkan ia **merender** (VS Code/GitHub); simpan dalam `docs/`.
- **Rujukan:** [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md) · [`diagram-claude-code.md`](./diagram-claude-code.md).

### DIA-02 — Carta alir proses (Mermaid)

- **Tujuan:** Kod Mermaid `flowchart` untuk aliran permohonan.
- **Input:** use case/PRD.

```text
Berdasarkan use case/PRD kami, beri kod Mermaid `flowchart`:
Mohon → semak (bertindih / pendua / kelengkapan) → kelulusan admin → audit.
Beri HANYA satu blok kod Mermaid (tiada nota/teks lain di dalam blok). Ikut peranan & status dalam PRD.
```

- **Selepas:** Sahkan status & peranan sepadan `SubmissionStatus`/`SPEC-KURSUS.md`.
- **Rujukan:** [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md).

### DIA-04 — Use case (flowchart)

- **Tujuan:** Gambaran aktor → fungsi. *(Mermaid tiada UML use case — guna `flowchart`.)*
- **Input:** use case/PRD.

```text
Berdasarkan use case/PRD kami di bawah, beri kod Mermaid `flowchart LR` sebagai gambaran use case:
- aktor (cth Pemohon, <peranan admin>) di kiri
- setiap use case sebagai satu nod (cth "Mohon tempahan", "Semak permohonan")
- sambungkan aktor ke use case yang mereka lakukan
Beri HANYA satu blok kod Mermaid (tiada nota/teks lain di dalam blok). Ikut aktor & fungsi dalam PRD; jangan reka.

[tampal use case / PRD di sini]
```

- **Rujukan:** [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md) § C.

### DIA-05 — Aliran pengguna (user flow)

- **Tujuan:** Langkah & titik keputusan dari sudut pengguna.
- **Input:** PRD.

```text
Berdasarkan PRD kami, beri kod Mermaid `flowchart TD` untuk aliran pengguna satu tugas
(cth "hantar permohonan"): setiap langkah pengguna + titik keputusan (cth "Sah?", "Slot kosong?")
+ hasil (berjaya / ralat). Beri HANYA satu blok kod Mermaid (tiada nota/teks lain di dalam blok). Ikut peranan & peraturan dalam PRD.
```

- **Rujukan:** [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md) § C.

### DIA-06 — Perjalanan pengguna (journey)

- **Tujuan:** Peringkat + tahap kepuasan (UX).
- **Input:** PRD.

```text
Berdasarkan PRD kami, beri kod Mermaid `journey` untuk perjalanan pengguna:
title <nama tugas>; beberapa section (cth Mohon, Semak, Keputusan); setiap langkah beri
skor kepuasan (1–5) dan aktor. Beri HANYA satu blok kod Mermaid (tiada nota/teks lain di dalam blok).
```

- **Rujukan:** [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md) § C.

### DIA-07 — Sequence diagram

- **Tujuan:** Mesej antara aktor/sistem mengikut masa.
- **Input:** aliran (cth permohonan → kelulusan, SSO → baca profil).

```text
Berdasarkan aliran kami, beri kod Mermaid `sequenceDiagram` untuk <aliran>:
peserta (cth Pemohon, Sistem, <peranan admin>) dan mesej antara mereka mengikut urutan.
Beri HANYA satu blok kod Mermaid (tiada nota/teks lain di dalam blok). Jangan tambah langkah yang tiada dalam aliran.
```

- **Rujukan:** [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md) § C.

### DIA-08 — State diagram (`SubmissionStatus`)

- **Tujuan:** Kitaran hayat status permohonan.
- **Input:** `SubmissionStatus` (SPEC-KURSUS).

```text
Berdasarkan SubmissionStatus kami (Draft, Submitted, SupervisorApproved, AdminApproved,
Rejected, Completed, Cancelled), beri kod Mermaid `stateDiagram-v2`:
tunjukkan peralihan yang DIBENARKAN untuk modul kami sahaja (jangan tambah status baharu).
Beri HANYA satu blok kod Mermaid (tiada nota/teks lain di dalam blok). Ikut SubmissionStatus dalam SPEC-KURSUS.md.
```

- **Rujukan:** [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md) § C.

### DIA-03 — Semak silang ERD

- **Tujuan:** Kesan entiti/medan direka atau menduplikasi `Submission`.
- **Input:** ERD + `SPEC-KURSUS.md`.

```text
Bandingkan ERD ini dengan SPEC-KURSUS.md (dilampirkan).
1. Adakah ia memperkenalkan entiti atau medan yang tiada dalam spec?
2. Adakah ia menduplikasi mana-mana medan yang sudah ada dalam Submission?
3. Adakah kardinaliti betul bagi setiap hubungan?
4. Nama jadual mana yang tidak sepadan dengan spec?
Senaraikan percanggahan sahaja. JANGAN tulis semula ERD.
```

- **Selepas:** Betulkan setiap percanggahan dengan tangan.
- **Rujukan:** `hari-1` Latihan 6.

---

## D · UI/UX

### UI-01 — Reka UI/UX modul

- **Tujuan:** Reka susun atur skrin/borang yang konsisten dari PRD.
- **Input:** PRD modul.

```text
Reka antara muka untuk modul <nama modul> berdasarkan PRD di bawah:
- borang yang jelas (medan wajib ditanda, mesej ralat mesra)
- senarai permohonan dengan status
- skrin semakan untuk admin
Guna corak/komponen sedia ada supaya konsisten. Label Bahasa Melayu, mesra
mudah alih & mudah dicapai (accessible). Tunjukkan susun atur (atau mockup HTML) dahulu.

[tampal PRD di sini]
```

- **Selepas:** Semak konsisten dengan skrin lain; validation di pelayan ditulis kemudian (bukan mockup).
- **Rujukan:** slaid *"Prompt untuk reka UI/UX"*.

---

## E · Jira

### JIRA-01 — Cipta isu dari user story

- **Tujuan:** Cipta Task Jira dari user story PRD (melalui MCP).
- **Input:** kunci projek + PRD user story.

```text
Dalam projek Jira <KEY>, cipta satu Task untuk user story US-1 PRD kami:
tajuk ringkas + perihalan, dan salin acceptance criteria PRD sebagai kriteria penerimaan.
Tunjukkan draf dahulu sebelum mencipta.
```

- **Input tambahan:** kunci per sistem — `LD` · `PKS` · `CM` · `PPK`/`PK`/`PASP` · `ID` · `FS`.
- **Rujukan:** [`cara-sambung-jira-claude-code.md`](./cara-sambung-jira-claude-code.md) · `hari-2` Latihan 6b.

---

## F · Semakan kod

### SMK-01 — Semakan pra-PR

- **Tujuan:** Tangkap pendua, pelanggaran sempadan, dan validation/authorization tertinggal.
- **Input:** diff perubahan anda.

```text
Semak diff ini terhadap AGENTS.md dan KOLABORASI.md:
1. Adakah ia menduplikasi apa-apa dalam daftar komponen kongsi?
2. Adakah ia menyentuh fail di luar folder Kumpulan N?
3. Adakah authorization dan validation pelayan lengkap?
Senaraikan masalah. JANGAN tulis semula kod.
```

- **Selepas:** Betulkan sendiri; terangkan kepada rakan sebelum commit.
- **Rujukan:** `AGENTS.md` · `hari-2` Latihan 6.

---

---

## G · Pembangunan (build)

> Fasa 2 (Hari 4–14). Pendekatan **borang dahulu**: DEV-01→04 (borang + validation) sebelum DEV-05 (simpan). Semua: rujuk `AGENTS.md` + PRD, **cari dahulu**, **tunjuk diff**, semak (SMK-01) sebelum commit. Panduan langkah: [`mula-claude-code-borang-dahulu.md`](./mula-claude-code-borang-dahulu.md).

### DEV-01 — Borang: ViewModel + validation

- **Tujuan:** ViewModel + DataAnnotations (borang mengikat ViewModel, bukan entiti).

```text
Rujuk AGENTS.md. Cipta ViewModel untuk borang <nama> dalam ViewModels/ dengan
DataAnnotations (Required, StringLength, Range, dll.) mengikut medan PRD.
JANGAN ikat entiti terus ke borang. Tunjuk diff dahulu.
```

### DEV-02 — Borang: View Razor

- **Tujuan:** Borang Razor yang mengikat ViewModel, guna partial kongsi.

```text
Rujuk AGENTS.md. Bina Views/<Modul>/Create.cshtml yang mengikat ViewModel di atas;
guna partial kongsi _ValidationSummary; label Bahasa Melayu; butang Simpan Draf & Hantar.
Tunjuk diff dahulu.
```

### DEV-03 — Controller: papar + simpan draf

- **Tujuan:** GET borang + POST simpan sebagai `Draft` (belum kelulusan).

```text
Rujuk AGENTS.md. <Modul>Controller warisi SubmissionControllerBase; laksana:
- Create (GET) memaparkan borang
- Create (POST) sahkan ModelState.IsValid di pelayan, simpan sebagai Draft
BELUM tulis Approve/Reject. Tunjuk diff dahulu.
```

### DEV-04 — Validation pelayan + peraturan perniagaan

- **Tujuan:** Kuatkuasa peraturan di **pelayan** (bukan UI sahaja).

```text
Rujuk AGENTS.md + PRD. Kuatkuasa validation di pelayan:
- ModelState.IsValid sebelum simpan
- peraturan <slot bertindih / pendua plat / kelengkapan> dalam servis
Papar ralat inline pada borang. Tunjuk diff dahulu.
```

### DEV-05 — Entiti + migration (simpan)

- **Tujuan:** Entiti detail → `Submission` + config, petakan ViewModel → entiti, jana migration.

```text
Rujuk AGENTS.md. Tambah entiti <Nama>Application (detail) yang memaut ke Submission
via SubmissionId — jangan pendua Status/ReferenceNo/tarikh. Guna IEntityTypeConfiguration.
Petakan ViewModel → entiti dalam Create (POST). Tunjuk diff dahulu.
```

```bash
dotnet ef migrations add Add<Nama>Application
dotnet ef database update
```

### DEV-06 — Aliran kelulusan (controller)

- **Tujuan:** Review/Approve/Reject dengan peranan + peralihan status + audit.

```text
Rujuk AGENTS.md. Dalam <Modul>Controller (warisi SubmissionControllerBase), laksana
Review/Approve/Reject dengan [Authorize(Roles = "<peranan>")], kuatkuasa peralihan
SubmissionStatus + audit. Tunjuk diff dahulu.
```

### DEV-07 — Ujian xUnit

- **Tujuan:** Uji peralihan status & peraturan perniagaan.

```text
Tambah ujian xUnit untuk peralihan SubmissionStatus (Draft→Submitted→AdminApproved/Rejected)
dan <semakan bertindih / pendua> dalam repo <sistem>.
```

---

> Automasi (Claude Code): bungkus DOK-01 + DIA-01/02 sebagai skill `/dok-modul` — lihat [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md).
