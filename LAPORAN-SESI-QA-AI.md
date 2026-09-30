# Laporan Kerja Sesi — Bahan Kursus DOTNET-NRES-15

> **Skop:** Ringkasan penuh sesi kerja Claude Code ke atas repo bahan latihan **coaching .NET 15 hari untuk NRES** — daripada perancangan, pelaksanaan, hingga penerbitan (git). Merangkumi keseluruhan sejarah perbualan sesi ini.
>
> Repo: `github.com/habibtalib/coach-dot-net-15-hari-nres` · Cabang: `master` · Bahasa: nota BM, kod/istilah English.

---

## 1. Ringkasan eksekutif

Sesi ini membina **modul QA dipandu-AI** untuk kursus (Hari 13–15: ujian xUnit + SIT/UAT pre-check), menerbitkan bahan Hari 13–15 yang sebelum ini local-only, mencipta **prompt am setiap hari** (Hari 1–15), dan menerbitkan folder nota konsep. Semua perubahan telah **commit & push ke `master`** dalam 4 commit.

**Hasil utama:**

| Bilangan | Hasil |
|----------|-------|
| 1 | Subagent baharu `qa-uat` (ujian xUnit + pandu pelayar) |
| 1 | Skill baharu `/uji-modul` (senarai semak ujian xUnit) |
| 1 | Lab penuh `docs/lab-qa-ai-uat.md` (4 latihan) |
| 4 | Prompt QA berkod `UJI-01`…`UJI-04` dalam pustaka prompt |
| 7 | Latihan "Pemecut QA dipandu-AI" disisip dalam lab Hari 13–14 (6 blok) + Hari 15 |
| 9 | `prompt.md` am mula-hari (Hari 1 hingga 15) |
| 12 | Nota konsep (`nota/`) diterbitkan ke GitHub |
| 2 | Fail kanun dikemas kini: `CLAUDE.md`, `AGENTS.md` |
| 4 | Commit ke `master` |

---

## 2. Objektif & konteks

- **Titik masa:** kursus berada pada **Hari 13** (blok ujian). Kandungan QA/UAT diselaraskan dengan **dek slaid** kursus.
- **Model kursus:** 4 kumpulan dedicated bekerja selari (Fasa 1: Hari 1–3 bersama; Fasa 2: Hari 4–14 trek selari; Fasa 3: Hari 15 integrasi/SIT/UAT/demo).
- **Kanun:** `SPEC-KURSUS.md`, `JADUAL.md`, `KOLABORASI.md`, `AGENTS.md`.
- **Doktrin ujian (padan slaid):** xUnit + **SQLite in-memory** (bukan `UseInMemoryDatabase`); uji peralihan `SubmissionStatus`, nombor rujukan, semakan pendua, kebenaran peringkat objek (POST terus → 403); **Hari 13–14 = xUnit**, **Hari 15 = SIT/UAT pre-check**; **pre-check ≠ UAT sebenar** (UAT sebenar oleh pengguna NRES).

---

## 3. Fasa perancangan (mod rancang)

1. **Penerokaan (3 ejen Explore selari):**
   - Konvensyen rumah — struktur `.claude/agents/*.md`, `.claude/skills/*/SKILL.md`, dan lab `docs/lab-*.md`.
   - Skop ujian dalam kanun + permukaan app rujukan (controller, akaun seed, URL).
   - Kandungan ujian/UAT dalam dek slaid (untuk padankan perkataan).
2. **4 soalan penjelasan** (keputusan dikunci): cipta fail sebenar + lab · subagent baharu berasingan · kedua-dua xUnit & pelayar (pecah ikut hari) · kekal selari dengan slaid, tiada suntingan dek.
3. **Pelan ditulis & diluluskan** — fail pelan: `~/.claude/plans/wild-puzzling-heron.md`.

**Penemuan penting:** tiada automasi pelayar wujud sebelum ini; tiada `*.Tests.csproj` di-commit; `claude-in-chrome` paling sesuai sebagai **pre-check aliran SIT/UAT Hari 15**, melengkapkan (bukan menggantikan) xUnit.

---

## 4. Fasa pelaksanaan (apa yang dibina)

### 4.1 Subagent & skill (`.claude/`)
- **`.claude/agents/qa-uat.md`** — persona penguji (langkah `UJI-01`…`UJI-05`): tulis/jalankan xUnit (Hari 13–14) + pandu SIT/UAT pre-check pelayar via `claude-in-chrome` (Hari 15). Write/Edit dihadkan kepada projek `*.Tests`.
- **`.claude/skills/uji-modul/SKILL.md`** — senarai semak ujian xUnit (cermin `semak-modul`).

### 4.2 Lab & prompt (`docs/`)
- **`docs/lab-qa-ai-uat.md`** — 4 latihan: cipta `qa-uat` → cipta `/uji-modul` → xUnit dengan AI (Hari 13–14) → SIT/UAT pre-check `claude-in-chrome` (Hari 15) + pustaka prompt.
- **`docs/pustaka-prompt.md`** — seksyen baharu **I · QA & UAT** dengan `UJI-01` (tulis/jalankan xUnit), `UJI-02` (pre-check pelayar), `UJI-03` (RBAC 403), `UJI-04` (mulakan ujian dari kosong untuk modul belum diuji).
- **`docs/README.md`** — lab baharu didaftar dalam indeks.

### 4.3 Latihan disisip dalam folder hari
- **6 blok Hari 13–14** (`kumpulan-*/hari-13-14/snippets/lab.md`) — "Pemecut QA dipandu-AI" dengan prompt `qa-uat` khusus modul (LD/PKS/KON status+rujukan, K2 plat/lot, K3 RBAC dua peringkat + 403, K4 pertindihan slot).
- **`hari-15/snippets/lab.md`** — "Latihan 6b — SIT/UAT pre-check dipandu-AI" selepas latihan SIT.

### 4.4 Prompt am setiap hari (root)
- **`hari-1/`…`hari-15/prompt.md`** — prompt **am mula-hari** (bukan modul-spesifik) yang mengorientasi AI ke `AGENTS.md` + `SPEC-KURSUS.md` + matlamat hari + aliran tugas.

### 4.5 Kemas kini kanun & rujukan
- **`CLAUDE.md`** — tambah bahagian arahan build/run/test + seni bina projek rujukan.
- **`AGENTS.md`** — daftar `qa-uat` + `/uji-modul` dalam jadual subagent.
- **`nota/`** — 12 nota konsep diterbitkan (setup, MVC, EF Core, workflow, validation, identity, muat naik fail, xUnit, deployment, keselamatan, rujukan buku, alatan Fasa 1).

### 4.6 Penerbitan folder (gitignore)
- Nyah-ignore `hari-15/` dan `**/hari-13-14/` (kini dihantar); `nota/` diterbitkan.
- Root `hari-*/prompt.md` di-track; kandungan **trek** mid-block (`kumpulan-*/hari-5-6`, `hari-7-9`, `hari-10-12`) + `nota-penceramah.md` + `projek/` kekal **local-only**.

---

## 5. Penerbitan & git

| Commit | Perkara |
|--------|---------|
| `8f829e7` | `docs`: arahan build/run/test + seni bina projek rujukan → `CLAUDE.md` |
| `061c41e` | `feat(qa)`: modul QA dipandu-AI — `qa-uat` + `/uji-modul` + lab + prompt + daftar |
| `6a02b23` | `feat(hari-13-15)`: terbitkan blok Hari 13–14 & Hari 15 + sisip latihan QA |
| `dfa341c` | `feat(prompt+nota)`: prompt am setiap hari + terbitkan folder `nota/` |

Cabang `feat/qa-ai-uat-module` turut ada (kini sama dengan `master`, boleh dipadam).

---

## 6. Pemetaan 15 hari (status selepas sesi)

| Hari | Fasa | Tema | Bahan AI baharu |
|------|------|------|-----------------|
| 1 | 1 | Perancangan, URS & ERD | `hari-1/prompt.md` |
| 2 | 1 | Git, Agile & kolaborasi | `hari-2/prompt.md` |
| 3 | 1 | Refresher .NET & asas kongsi | `hari-3/prompt.md` |
| 4 | 2 | Skema DB & skrin pertama | `hari-4/prompt.md` |
| 5–6 | 2 | Borang & peraturan perniagaan | `hari-5-6/prompt.md` |
| 7–9 | 2 | Aliran kelulusan & skrin admin | `hari-7-9/prompt.md` |
| 10–12 | 2 | Notifikasi, laporan & dashboard | `hari-10-12/prompt.md` |
| 13–14 | 2 | **Ujian, refactor & sedia gabung** | `prompt.md` + Latihan "Pemecut QA" dalam 6 blok trek + lab `qa-uat`/`/uji-modul` |
| 15 | 3 | **Integrasi, SIT/UAT & demo** | `prompt.md` + Latihan 6b (SIT/UAT pre-check `claude-in-chrome`) |

---

## 7. Kronologi sesi (sejarah perbualan)

1. **`/init`** — analisis repo; `CLAUDE.md` ditambah arahan build/run/test + seni bina projek rujukan.
2. **`/plan`** — rancang modul QA dipandu-AI (3 ejen Explore, 4 soalan penjelasan, pelan diluluskan).
3. **Pelaksanaan** — cipta `qa-uat`, `/uji-modul`, lab, prompt `UJI-01`…`03`, daftar dalam `AGENTS.md`/`docs/README.md` + pointer README.
4. **"add prompt for unit testing… not yet done"** — tambah `UJI-04` (mulakan ujian dari kosong).
5. **"commit and push"** — cabang `feat/qa-ai-uat-module`, 2 commit, push.
6. **"merge locally and push"** — merge ke `master` (fast-forward), push.
7. **"i dont see it"** — didiagnosis: folder hari local-only (gitignore); sahkan `master` = default.
8. **"do in folder day 13-15"** — sisip latihan QA dalam 7 lab hari; nyah-ignore Hari 13–15; commit + push.
9. **"create root folder by days also for general prompt"** — cipta `prompt.md` am untuk semua hari.
10. **"make nota public"** — nyah-ignore `nota/`; commit + push.
11. **"write me reporting…"** — laporan ini.

---

## 8. Nota, andaian & tindakan susulan

- **Divergensi kanun:** folder root `hari-4/5-6/7-9/10-12/13-14` (untuk `prompt.md` am) menyimpang daripada `SPEC-KURSUS.md` yang meletakkan hari Fasa 2 dalam trek. Ia lapisan prompt **rentas-trek**, bukan penggantian tempat kerja modul. *Cadangan:* dokumen konvensyen `hari-*/prompt.md` dalam `SPEC-KURSUS.md`.
- **App rujukan (`projek/`) kekal local-only** — lab merujuk `https://localhost:7034` (hanya modul K1/Lapor Diri ada halaman sebenar). Klon awam tidak mempunyai `projek/`.
- **Tiada `*.Tests` di-commit dalam `projek/`** — sengaja (dibina dalam trek). Lab menciptanya sebagai langkah.
- **Belum disahkan secara langsung dalam sesi ini:** `dotnet build`/`dotnet test` app + aliran pelayar sebenar. Snippet xUnit disahkan padan jenis sebenar (`SubmissionStatus`, `IWorkflowService`, `IReferenceNumberService`, ctor `ApplicationDbContext`).
- **Belum selesai (pilihan):** padam cabang `feat/qa-ai-uat-module` (local + remote).

---

*Dijana oleh Claude Code (Opus 4.8). Data contoh kursus adalah sintetik.*
