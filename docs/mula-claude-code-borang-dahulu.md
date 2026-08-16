# Mula projek dengan Claude Code — borang dahulu (form-first)

> **Bahan rujukan kursus.** Cara mula membina modul anda dengan **Claude Code**, dengan **reka borang dahulu**. Prasyarat: [`persediaan-dotnet.md`](./persediaan-dotnet.md) · [`persediaan-scaffold.md`](./persediaan-scaffold.md) · PRD modul siap ([`contoh-prd-tempahan-fasiliti-sukan.md`](./contoh-prd-tempahan-fasiliti-sukan.md)).

## Kenapa borang dahulu?

Reka **borang + validation dahulu** = maklum balas pantas pada pengalaman pengguna **sebelum** melabur pada skema DB. Selepas borang betul, baru **back-fill** entiti + migration untuk menyimpan. (Data belum disimpan? Guna draf dalam memori dahulu.)

## Disiplin setiap prompt (jangan langkau)

- **Cari dahulu, jana kemudian** — guna servis/partial kongsi sedia ada (`AGENTS.md`), jangan cipta pendua.
- Sentiasa minta **"tunjuk pelan/diff dahulu"** sebelum AI menulis fail.
- **Semak (SMK-01)** & faham kod sebelum commit — tiada commit tanpa faham.
- **BM** untuk label/UI · **validation di pelayan** · **data sintetik** sahaja.

---

## Langkah (dengan prompt Claude Code)

### 0 · Buka & beri konteks

- Buka repo sistem anda dalam Claude Code (terminal: jalankan `claude` dalam folder repo; atau panel Claude Code dalam VS Code).
- Claude Code membaca `AGENTS.md` **automatik**. Sahkan konteks:

```text
Ringkaskan peraturan penting AGENTS.md repo ini dalam 5 poin, dan senaraikan
servis kongsi yang sudah ada (cth IReferenceNumberService, IWorkflowService).
```

- Lampirkan **PRD** modul apabila mula satu ciri.

### 1 · Reka rupa borang dahulu (design)

```text
Reka susun atur borang <nama borang> untuk modul <nama modul> berdasarkan PRD di bawah:
- medan wajib ditanda; mesej ralat mesra & jelas
- guna corak/komponen sedia ada supaya konsisten
Beri mockup HTML ringkas untuk pratonton — susun atur dahulu, bukan kod akhir.
Label Bahasa Melayu, mesra mudah alih & mudah dicapai (accessible).

[tampal PRD di sini]
```

**Semak reka bentuk:** konsisten dengan skrin lain? medan wajib jelas? boleh guna papan kekunci / telefon? *(Prompt penuh: UI-01 dalam [`pustaka-prompt.md`](./pustaka-prompt.md).)*

### 2 · ViewModel + DataAnnotations

```text
Rujuk AGENTS.md. Cipta ViewModel untuk borang <nama> dalam ViewModels/ dengan
DataAnnotations (Required, StringLength, Range, dll.) mengikut medan PRD.
JANGAN ikat entiti terus ke borang — borang mengikat ViewModel.
Tunjuk diff dahulu.
```

### 3 · View Razor (borang)

```text
Rujuk AGENTS.md. Bina Views/<Modul>/Create.cshtml yang mengikat ViewModel di atas;
guna partial kongsi _ValidationSummary; label Bahasa Melayu; butang Simpan Draf & Hantar.
Tunjuk diff dahulu.
```

### 4 · Controller — papar borang + simpan draf

```text
Rujuk AGENTS.md. <Modul>Controller warisi SubmissionControllerBase; laksana:
- Create (GET) memaparkan borang
- Create (POST) sahkan ModelState.IsValid di pelayan, simpan sebagai Draft
BELUM tulis Approve/Reject (datang blok kelulusan). Tunjuk diff dahulu.
```

### 5 · Validation pelayan + peraturan perniagaan

```text
Rujuk AGENTS.md + PRD. Kuatkuasa validation di pelayan:
- ModelState.IsValid sebelum simpan
- peraturan <slot bertindih / pendua plat / kelengkapan> dalam servis (bukan UI sahaja)
Papar ralat inline pada borang. Tunjuk diff dahulu.
```

### 6 · Kemudian: entiti + migration (simpan ke DB)

Bila borang & validation betul, baru sambung penyimpanan:

```text
Rujuk AGENTS.md. Tambah entiti <Nama>Application (detail) yang memaut ke Submission
via SubmissionId — jangan pendua Status/ReferenceNo/tarikh. Guna IEntityTypeConfiguration.
Petakan ViewModel → entiti dalam Create (POST). Tunjuk diff dahulu.
```

```bash
dotnet ef migrations add Add<Nama>Application
dotnet ef database update
```

---

## Gelung harian

```
Jira story → feat/<ciri> → prompt (rujuk AGENTS.md + PRD) → bina → SMK-01 (semak) → PR → AC = DoD
```

## Rujukan

- **Prompt berfail:** [`pustaka-prompt.md`](./pustaka-prompt.md) — UI-01 (reka UI), **DEV-01…07** (borang → validation → entiti/migration → kelulusan → ujian), SMK-01 (semak pra-PR).
- **Konteks AI:** `AGENTS.md` repo anda · kanun: `SPEC-KURSUS.md`.
- **Lab blok borang:** `kumpulan-N/.../hari-5-6/snippets/lab.md`.
- **Persediaan:** [`persediaan-scaffold.md`](./persediaan-scaffold.md).
