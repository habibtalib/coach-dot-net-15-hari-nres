# Templat URS — salin ke `docs/URS-modul-N.md`

> Salin fail ini ke `docs/URS-modul-<N>.md` (N = nombor kumpulan) dan gantikan contoh dengan keperluan modul anda. Kanun: [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md) · Kaedah: [`../README.md`](../README.md) (Design Thinking → URS).
>
> **Peraturan emas:** setiap URS mesti dijejak balik ke **satu titik kesakitan** (*pain*) sebenar dari Design Thinking. Tiada pain → buang keperluan itu (ia direka).

## Maklumat modul

| Perkara | Nilai |
|---------|-------|
| Modul | _(cth Lapor Diri)_ |
| Kumpulan | _(N)_ |
| Prefix rujukan | _(cth `LD`)_ |
| Peranan terlibat | _(cth Pemohon, HrAdmin)_ |

---

## Format satu URS (ulang blok ini bagi setiap keperluan)

### URS-\<PREFIX\>-\<###\> — \<tajuk ringkas\>
- **Keutamaan:** Wajib / Sederhana / Boleh tunggu
- **Aktor:** _(siapa)_
- **Pain (punca):** _(salin dari empathy map / sticky kesakitan)_
- **Keperluan:** "Sistem **mesti** _______________." _(satu ayat, boleh diuji)_
- **Kriteria penerimaan:**
  - [ ] _______________
  - [ ] _______________
- **Diagram berkaitan:** use case _____ · ERD entiti _____

---

## Contoh (Lapor Diri) — teladan

### URS-LD-03 — Semak status permohonan
- **Keutamaan:** Wajib
- **Aktor:** Pemohon
- **Pain (punca):** "Saya dah hantar borang… tapi saya tak tahu kedudukan permohonan saya" — telefon berkali-kali tanya status.
- **Keperluan:** "Sistem **mesti** memaparkan status semasa & sejarah setiap permohonan kepada pemohon yang log masuk."
- **Kriteria penerimaan:**
  - [ ] Diberi nombor rujukan, status semasa dipaparkan
  - [ ] Setiap peralihan status + tarikh dipaparkan (Draft → Submitted → …)
  - [ ] Pemohon tidak perlu menelefon untuk tahu status
- **Diagram berkaitan:** use case "Semak status" · ERD entiti `Submission.Status`

---

## Jadual jejak (traceability) — ringkasan semua URS

| ID | Pain | URS "Sistem mesti…" | Kriteria (ringkas) | Diagram |
|----|------|----------------------|--------------------|---------|
| URS-LD-03 | Tak tahu status | Paparkan status & sejarah | ref → status+tarikh dipapar; tiada telefon | Use Case: Semak status · ERD: Submission |
| | | | | |
| | | | | |
| | | | | |

---

## ✅ Semakan "URS yang baik" (guna semasa peer review — Latihan 3)

Setiap URS mesti lulus **lima** ujian:

- [ ] **Jelas** — satu tafsiran sahaja
- [ ] **Tunggal** — satu keperluan, bukan "dan/atau" berganda
- [ ] **Boleh diuji** — anda boleh tulis ujian untuknya
- [ ] **Ada kriteria penerimaan** — bagaimana kita tahu ia dipenuhi
- [ ] **Dijejak ke pain** — ada punca sebenar di lajur *Pain*

> *"Jika anda tidak boleh menulis ujian untuknya, ia bukan keperluan — ia harapan."*
>
> **Seterusnya:** setiap baris jadual jejak menjadi **user story** (Hari 2) dan diagram (use case/ERD, SESI 3–4) — lihat [`contoh-diagram.md`](./contoh-diagram.md).
