# Lab — Skill, Subagent Peranan (PM · DEV · QA), Memory & MCP

> 🔧 **Khusus Claude Code · lanjutan.** Anda sudah mula bina dengan Claude Code + Jira (MCP) + `AGENTS.md`. Lab ini menaik taraf cara kerja: **skill** boleh guna semula, **subagent peranan** (PM/DEV/QA) yang memetakan kitaran satu tugas, **memory** untuk peraturan kekal, dan **MCP** yang sudah tersambung.
>
> **Persona = subagent.** Dalam Claude Code, "persona" ialah **subagent** — satu fail `.claude/agents/<nama>.md` di mana *system prompt* ialah personanya. Tiada fail `persona.md` berasingan.

## Konsep ringkas

| Bahagian | Apa ia | Di mana |
|----------|--------|---------|
| **Skill** | Aliran kerja boleh guna semula, dipanggil `/nama` | `.claude/skills/<nama>/SKILL.md` |
| **Subagent (persona)** | Ejen khusus dengan konteks & peraturan sendiri | `.claude/agents/<nama>.md` |
| **Memory** | Konteks kekal antara sesi | `CLAUDE.md` / `AGENTS.md` · `#` · `/memory` |
| **MCP** | Sambung ke Jira / Claude Design / FigJam | `/mcp` (sudah tersambung) |

## Persediaan

- Claude Code dipasang; MCP Jira + Claude Design sudah tersambung ([`lab-mcp-jira-figjam.md`](./lab-mcp-jira-figjam.md)).
- Anda tahu kitaran satu tugas ([`aliran-harian.md`](./aliran-harian.md)) & prompt (`pustaka-prompt.md`).
- Templat rujukan sudah ada dalam repo kursus: `.claude/agents/{pm,dev,qa}.md` + `.claude/skills/semak-modul/SKILL.md`.

---

## Latihan 1 — Cipta skill `/semak-modul`

**Objektif:** Bungkus semakan pra-PR jadi satu slash command yang boleh dipanggil bila-bila.

### Langkah

1. Dalam **repo modul anda**, cipta fail `.claude/skills/semak-modul/SKILL.md`. Salin dari templat kursus [`.claude/skills/semak-modul/SKILL.md`](../.claude/skills/semak-modul/SKILL.md).
2. Frontmatter mesti ada `name` + `description` (description menentukan bila Claude cadang skill ini).
3. Dalam Claude Code, panggil:

   ```text
   /semak-modul
   ```

### ✅ Semakan

- [ ] `.claude/skills/semak-modul/SKILL.md` wujud dengan `name` + `description`
- [ ] `/semak-modul` berjalan & menghasilkan senarai masalah (bukan menulis semula kod)

---

## Latihan 2 — Cipta subagent peranan: PM · DEV · QA

**Objektif:** Tiga persona yang memetakan kitaran satu tugas — PM skop, DEV bina, QA semak.

### Langkah

1. Cipta tiga fail dalam **repo modul anda** (salin dari templat kursus [`.claude/agents/`](../.claude/agents/)):
   - `.claude/agents/pm.md` — tanya Jira, skop & AC, cipta cabang `feat/`.
   - `.claude/agents/dev.md` — bina borang-dahulu (DEV-01→05), mockup UI-01 sebagai rujukan.
   - `.claude/agents/qa.md` — semakan pra-PR (SMK-01) + ujian xUnit; **baca sahaja**.
2. Setiap fail: frontmatter `name` + `description` (+ `tools` untuk hadkan capaian; DEV boleh tulis, QA baca sahaja).
3. Sahkan Claude nampak ketiga-tiganya:

   ```text
   /agents
   ```

### ✅ Semakan

- [ ] `.claude/agents/pm.md`, `dev.md`, `qa.md` wujud
- [ ] `/agents` menyenaraikan `pm`, `dev`, `qa`
- [ ] `qa` dihadkan kepada `tools: Read, Grep, Glob, Bash` (tak boleh tulis kod)

---

## Latihan 3 — Jalankan pasukan sepanjang satu tugas

**Objektif:** Guna ketiga-tiga persona untuk satu tugas sebenar, hujung ke hujung.

### Langkah

1. **PM** — mula tugas:

   ```text
   Guna subagent pm: tanya Jira tugas To-Do seterusnya untuk saya,
   sahkan skop & AC, tandakan In Progress, dan cipta cabang feat/.
   ```

2. **DEV** — bina:

   ```text
   Guna subagent dev: bina ciri ini borang-dahulu (DEV-01→05),
   guna mockup Claude Design (UI-01) sebagai rujukan. Tunjuk diff dahulu.
   ```

3. **QA** — semak sebelum PR:

   ```text
   Guna subagent qa: jalankan semakan pra-PR (SMK-01) + ujian xUnit
   untuk perubahan ini. Senaraikan masalah sahaja.
   ```

4. Betulkan isu QA (dengan `dev`), kemudian buka PR (`Closes <KEY>-n`) → review → merge → Jira **Done**.

> **Semak manusia:** subagent mempercepat, anda tetap sahkan setiap diff & jawapan. Tiada commit tanpa faham.

### ✅ Semakan

- [ ] PM menghasilkan tugas + cabang `feat/<ciri-pendek>`
- [ ] DEV membina dengan diff ditunjuk dahulu; commit ada issue key
- [ ] QA menyenaraikan masalah + ujian dijalankan (`dotnet test`)
- [ ] PR dibuka `Closes <KEY>-n`; isu Jira → Done selepas merge

---

## Latihan 4 — Simpan aliran ke memory

**Objektif:** Rakam peraturan pasukan sekali supaya Claude ingat antara sesi (MEM-01).

### Langkah

1. Mula prompt dengan `#`:

   ```text
   # Setiap tugas guna pasukan subagent: pm (Jira + cabang) → dev (bina) →
   # qa (SMK-01 + ujian) → PR (Closes KEY-n) → Jira Done.
   ```

2. Pilih lapisan **projek** (`CLAUDE.md`/`AGENTS.md`, di-commit) supaya seluruh kumpulan dapat.
3. Sahkan dengan `/memory`.

### ✅ Semakan

- [ ] Peraturan aliran subagent disimpan ke memory projek
- [ ] `/memory` menunjukkan ia di lapisan betul (dikongsi, bukan peribadi)

---

## Masalah biasa

- **Subagent tak muncul:** pastikan fail di `.claude/agents/` (repo atau `~/.claude/agents/`); jalankan `/agents`.
- **QA tersilap tulis kod:** hadkan `tools:` dalam frontmatter kepada baca + Bash sahaja.
- **PM tak nampak Jira:** MCP Jira belum authenticate — `/mcp` → atlassian → Authenticate.
- **Skill tak auto-cadang:** perbaiki `description` supaya padan dengan tugas.

## Rujukan

- Kitaran satu tugas: [`aliran-harian.md`](./aliran-harian.md) · Prompt: [`pustaka-prompt.md`](./pustaka-prompt.md)
- MCP setup: [`lab-mcp-jira-figjam.md`](./lab-mcp-jira-figjam.md) · Aliran kerja: [`../AGENTS.md`](../AGENTS.md)
- Slaid: bahagian *MCP · Skills · Memory · Subagents · Plugins*.
