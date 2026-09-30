# Cadangan Latihan 5 Hari — Dari Idea ke Perisian dengan AI
### *5-Day Training Proposal — From Idea to Software with AI (Design Thinking → URS → Agile → Jira → Claude Code)*

> **Model penyampaian:** intensif 5 hari, **≥60% hands-on**. Setiap peserta membina **satu modul kecil** hujung-ke-hujung — dari memahami masalah (Design Thinking) sehingga menghantar perisian yang disemak (PR + demo) dengan bantuan **Claude Code**.
>
> *Delivery model: a 5-day intensive, ≥60% hands-on. Each participant builds one small module end-to-end — from understanding the problem (Design Thinking) to shipping reviewed software (PR + demo) with Claude Code.*
>
> Ganti pemboleh ubah: `<ORG>` (organisasi), `<STACK>` (teknologi pilihan, cth ASP.NET Core / Laravel / Next.js), `<JIRA-SITE>`.

---

## Ringkasan Eksekutif · Executive Summary

Latihan ini mengajar **cara moden membina perisian**: fahami masalah dahulu, dokumentasikan keperluan yang boleh diuji, rancang kerja secara Agile, dan bina dengan **pembantu AI yang berdisiplin** — di mana AI mempercepat kerja yang **anda faham dan sahkan**, bukan menggantikan pemikiran.

*This training teaches the modern way to build software: understand the problem first, document testable requirements, plan work the Agile way, and build with a disciplined AI assistant — where AI accelerates work you understand and verify, not replaces your thinking.*

Berbeza dengan kursus alat biasa, peserta keluar dengan **satu modul sebenar yang siap** dan satu **aliran kerja boleh diulang** yang boleh dibawa balik ke pejabat.

---

## Objektif Pembelajaran · Learning Outcomes

Selepas 5 hari, peserta boleh:

1. Menjalankan **Design Thinking** ringkas (persona, empathy map) dan menjejak keperluan balik ke *pain* sebenar.
2. Menulis **URS/keperluan yang boleh diuji** + **ERD** ringkas (diagram sebagai kod).
3. Merancang kerja secara **Agile** dan menguruskannya dalam **Jira** (epic → story → task, board, issue key).
4. Menggunakan **Git**: cabang ciri, Pull Request, code review.
5. Menyediakan **Claude Code** dengan konteks & alat yang betul (`AGENTS.md`, memory, **MCP**, skills, subagent peranan).
6. **Membina ciri dengan AI** secara berdisiplin — mockup dahulu, tunjuk diff, semak setiap perubahan — dan menghantarnya melalui PR.
7. Mengulang **kitaran satu tugas**: Jira → cabang → bina → semak → PR → demo.

---

## Untuk Siapa & Prasyarat · Audience & Prerequisites

- **Audiens:** developer, lead teknikal, business analyst, project owner yang mahu aliran kerja moden + AI.
- **Saiz kumpulan:** 8–16 (nisbah jurulatih sihat untuk hands-on).
- **Prasyarat:** biasa dengan `<STACK>` pada tahap asas; komputer dengan Git + editor; akaun **Jira** (`<JIRA-SITE>`) dan **Claude Code** dipasang. Tiada pengalaman AI diperlukan.

---

## Pendekatan · Approach

- **Bina satu modul kecil** (contoh: *modul permohonan & kelulusan ringkas*) sebagai benang merah 5 hari — setiap hari menambah lapisan.
- **AI draf, manusia sahkan.** Setiap output AI disemak baris demi baris; tiada commit tanpa faham.
- **Data sintetik sahaja**; jangan simpan rahsia (kata laluan/token) dalam repo — titik pengajaran keselamatan.
- Setiap lab: **Objektif → langkah bernombor → ✅ Semakan**.

---

## Kurikulum 5 Hari · 5-Day Curriculum

### Hari 1 — Design Thinking → URS · *Understand & Define*
- **Fokus:** fahami masalah sebelum satu baris kod.
- **Topik:** Design Thinking (empathize/define), persona & empathy map; URS vs SRS; **keperluan boleh diuji** + acceptance criteria; **ERD** asas (Mermaid, diagram-sebagai-kod).
- **Lab:** pilih modul kecil → persona + empathy map (FigJam) → tulis URS (draf AI, **semak manusia**) → ERD ringkas.
- **✅ Hasil:** URS bertrace-ke-pain + ERD untuk satu modul.

### Hari 2 — Agile, Jira & Git · *Plan & Collaborate*
- **Fokus:** rancang kerja & sedia berkolaborasi.
- **Topik:** nilai Agile, backlog, sprint, **Definition of Done**; **Jira** (epic → story → task, board, swimlane, issue key dalam commit); **Git** (clone, cabang ciri, **Pull Request**, code review).
- **Lab:** PRD ringkas dari URS → pecah ke **backlog Jira** → AC = DoD → setup repo + cabang + **PR pertama**.
- **✅ Hasil:** backlog Jira + repo dengan aliran PR berfungsi.

### Hari 3 — Asas Claude Code · *AI-assisted Setup*
- **Fokus:** sediakan pembantu AI dengan konteks & alat yang betul.
- **Topik:** setup Claude Code; **`AGENTS.md` / `CLAUDE.md`** (konteks & **memory**); permissions; **MCP** (sambung **Jira** + alat reka bentuk); prompt **PRD → dokumentasi → diagram**; **skills / slash commands**; `#` & `/memory`.
- **Lab:** tulis `AGENTS.md` → sambung **Jira MCP** → jana dokumentasi + diagram Mermaid dari PRD → cipta satu **skill** (`/semak-modul`).
- **✅ Hasil:** repo dengan `AGENTS.md` + MCP Jira + satu skill; dokumentasi & diagram dijana.

### Hari 4 — Bina dengan AI · *Build with AI*
- **Fokus:** bina ciri berdisiplin — **mockup dahulu, tunjuk diff, semak**.
- **Topik:** **kitaran satu tugas** (Jira → cabang → mockup → bina → PR); mockup UI (AI) sebagai rujukan; bina (ViewModel/borang → paparan → controller → **validation pelayan** → entiti/migration); **subagent peranan** (PM/DEV/QA); **hooks**; **semakan pra-PR**.
- **Lab:** ambil satu **tugas Jira** → cabang `feat/…` → mockup → bina → semakan pra-PR → **buka PR** (`Closes <KEY>-n`).
- **✅ Hasil:** satu ciri lengkap dibina & PR dibuka — semua disemak manusia.

### Hari 5 — Gabung, Semak & Demo · *Ship & Demo*
- **Fokus:** gabung, uji, tunjuk.
- **Topik:** merge PR + code review; **UAT ringkas** ikut acceptance criteria; keselamatan asas (jangan simpan rahsia, semak diff); **plugins** (kongsi setup pasukan); **retrospektif** Agile.
- **Lab:** merge PR → UAT ikut AC → **demo modul** → retrospektif (apa berkesan / perlu baiki).
- **✅ Hasil:** modul kecil **siap, didemo, DoD dipenuhi**.

---

## Aliran Hujung-ke-Hujung · End-to-End Flow

```
Design Thinking → URS/ERD → PRD → Backlog Jira → [ Jira tugas → cabang → mockup →
bina (AI) → semakan pra-PR → PR → review → merge → Done ] → UAT → Demo
```

Kitaran dalam kurungan **[ ]** diulang setiap tugas — itulah kemahiran teras yang dibawa balik.

---

## Hasil untuk Peserta · Deliverables

- Satu **modul kecil** yang berfungsi (repo + PR + demo).
- **Artifak perancangan:** persona/empathy map, URS, ERD, PRD, backlog Jira.
- **Repo sedia-AI:** `AGENTS.md`, satu skill, MCP Jira, subagent peranan (PM/DEV/QA).
- **Aliran kerja boleh diulang** (rujukan satu-halaman) + templat prompt.

---

## Keperluan Teknikal · Requirements

| Perkara | Butiran |
|---------|---------|
| Perisian | `<STACK>` SDK/toolchain · Git · editor/IDE · Claude Code |
| Akaun | **Jira** (`<JIRA-SITE>`) · repositori Git (GitHub/GitLab) · Claude |
| Persekitaran | Internet; kebenaran memasang alat; data **sintetik** sahaja |

---

## Penilaian · Assessment

Modul dianggap **siap** bila memenuhi **Definition of Done**: kod berjalan & disemak, acceptance criteria lulus (UAT), validation di pelayan, tiada rahsia dalam repo, PR diluluskan seorang rakan, isu Jira → **Done**. Penilaian utama = **demo capstone** Hari 5.

---

## Logistik · Logistics

- **Tempoh:** 5 hari (≈ 6–7 jam/hari, ≥60% hands-on).
- **Format:** bersemuka atau dalam talian; jurulatih berpusing membantu setiap peserta.
- **Bahan:** README konsep + lab berangka + templat prompt + rujukan satu-halaman.
- **Pelaburan · Investment:** *(untuk dilengkapkan mengikut saiz kumpulan & lokasi — `<HARGA>`)*.

---

## Lampiran — Pemetaan Alat · Appendix: Tooling Map

| Peringkat | Alat | Peranan |
|-----------|------|---------|
| Faham/Reka | FigJam · Mermaid | persona, empathy map, ERD/aliran sebagai kod |
| Rancang | Jira · Git/GitHub | backlog, board, cabang, PR |
| Bina (AI) | Claude Code · MCP · Skills · Subagents | konteks, alat luar, aliran guna-semula, peranan |
| Kekal | `AGENTS.md` / memory · Hooks · Plugins | peraturan tetap, automasi, kongsi setup |

---

*Cadangan ini boleh disesuaikan mengikut `<ORG>`, teknologi (`<STACK>`), dan tahap peserta. Semua contoh menggunakan data sintetik.*
