# Lab MCP — Sambung & Pandu Jira + FigJam dari Claude Code

> 🔧 **Khusus Claude Code.** Lab ini menyatukan **dua sambungan MCP** dalam satu tempat: **Jira** (Atlassian) dan **FigJam** (Figma). Kedua-duanya guna **pelayan MCP rasmi (jauh)** dengan `--transport http` + OAuth — cara standard yang boleh anda pasang sendiri. Selepas siap, Claude Code boleh **baca & cipta isu Jira** dan **lukis board FigJam** terus dari terminal/chat, tanpa salin manual.
>
> **Bila jalankan:** bahagian **FigJam** menyokong **Hari 1** (Design Thinking → URS → ERD), bahagian **Jira** menyokong **Hari 2** (backlog Agile). Boleh jalankan sekali gus sebagai lab perkakas, atau ikut hari masing-masing.
>
> Guna alat AI lain (bukan Claude Code)? Langkau lab ini — teruskan dengan langkah manual FigJam ([`hari-1`](../hari-1/snippets/lab.md) Latihan 1b) dan Jira demo ([`hari-2`](../hari-2/snippets/lab.md) Latihan 1).

## Konsep ringkas

**MCP (Model Context Protocol)** = piawai terbuka yang menyambung Claude ke sistem luar melalui **pelayan (server)**. Alat pelayan muncul dalam Claude sebagai `mcp__<server>__<tool>`, dan **anda tetap luluskan setiap tindakan** — AI tak boleh cipta isu atau ubah board tanpa izin anda.

> Peta ekosistem penuh (model · subagent · MCP · skills · plugins): [`diagram-claude-code.md`](./diagram-claude-code.md). Slaid rujukan: dek kursus, bahagian *MCP · Skills · Plugins*.

## Persediaan

- **Claude Code** dipasang (CLI **atau** sambungan VS Code). VS Code & CLI **kongsi konfigurasi sama** (`~/.claude/settings.json` + `.mcp.json` di root repo) — tambah pelayan **sekali** sahaja.
- Akaun **Atlassian** dengan akses ke tapak Jira projek anda.
- Akaun **Figma** (log masuk biasa memadai untuk beta).
- PRD/URS/ERD Hari 1 modul anda (sumber untuk isu Jira & board FigJam).

> **Fakta CLI vs VS Code:** arahan `claude mcp add …` sama di kedua-dua tempat. Dalam VS Code, jalankan di **terminal bersepadu** (`` Ctrl+` `` / `` Cmd+` ``), dan authenticate melalui `/mcp` dalam **panel chat**.

---

## Latihan 1 — Sambung & pandu Jira (MCP Atlassian)

**Objektif:** Sambung Jira melalui MCP rasmi, sahkan ia connected, dan cipta **satu isu** dari user story PRD anda — draf disemak dahulu.

### Langkah

1. **Tambah pelayan MCP Atlassian:**

   ```bash
   claude mcp add --transport http atlassian https://mcp.atlassian.com/v1/mcp
   ```

2. **Autentikasi.** Dalam Claude Code taip `/mcp` → pilih **atlassian** → **Authenticate** → log masuk dalam pelayar. Guna tetingkap **incognito** jika anda ada beberapa akaun Atlassian, supaya SSO tidak pilih akaun salah.

3. **Sahkan sambungan.** `/mcp` patut tunjuk `atlassian` **✔ Connected**. Uji dengan prompt:

   ```text
   Senaraikan projek Jira yang saya boleh akses.
   ```

4. **Pandu Jira — cipta isu dari PRD.** Kunci projek per sistem: `LD` · `PKS` · `CM` · `PPK`/`PK`/`PASP` · `ID` · `FS`.

   ```text
   Dalam projek Jira <KEY>, cipta satu Task untuk user story US-1 PRD kami:
   tajuk ringkas + perihalan, dan salin acceptance criteria PRD sebagai
   kriteria penerimaan. Tunjukkan draf dahulu sebelum mencipta.
   ```

5. **Semak draf → luluskan.** Betulkan tajuk/AC jika perlu, kemudian benarkan Claude cipta isu. Buka Jira dan sahkan isu muncul.

> **Semak manusia:** jangan biar AI reka acceptance criteria di luar PRD. Draf dahulu, luluskan kemudian.

### ✅ Semakan

- [ ] `/mcp` tunjuk `atlassian` **✔ Connected**
- [ ] Claude boleh **senaraikan** projek Jira anda
- [ ] Satu **Task** dicipta dalam projek `<KEY>` dari **US-1** PRD, dengan kriteria penerimaan
- [ ] Tiada AC/keperluan direka di luar PRD

---

## Latihan 2 — Sambung & pandu FigJam (MCP Figma)

**Objektif:** Sambung Figma melalui MCP rasmi, dan minta Claude **lukis board FigJam** (ERD + carta alir) dari ERD/PRD Hari 1 — bukan seret sticky manual.

### Langkah

1. **Tambah pelayan MCP Figma:**

   ```bash
   claude mcp add --transport http figma https://mcp.figma.com/mcp
   ```

2. **Autentikasi.** Taip `/mcp` → pilih **figma** → **Authenticate** → benarkan akses akaun Figma dalam pelayar.

3. **Sahkan sambungan.** `/mcp` patut tunjuk `figma` **✔ Connected**.

4. **Pandu FigJam — jana ERD sebagai board.** Beri Claude ERD Mermaid Hari 1 modul anda:

   ```text
   Dalam FigJam, cipta board baharu bertajuk "ERD — Modul <nama> (Kumpulan N)".
   Berdasarkan ERD di bawah: lukis setiap entiti sebagai sticky/shape, dan
   connector untuk setiap hubungan (kardinaliti pada label). Susun kemas.
   Jangan tambah entiti yang tiada dalam ERD.

   [tampal erDiagram Mermaid Hari 1 di sini]
   ```

5. **Jana carta alir proses** (pilihan) dari process flow Hari 1:

   ```text
   Dalam FigJam board sama, tambah satu section "Aliran permohonan":
   carta alir permohonan → semakan → kelulusan/tolak, guna connector.
   ```

6. **Semak di Figma.** Buka board FigJam dan sahkan entiti, hubungan & aliran betul. Betulkan apa-apa yang direka.

> **Nota beta:** menulis ke kanvas (write-to-canvas) Figma **percuma semasa beta**, tetapi dijangka menjadi **ciri berbayar (usage-based)** kemudian. Untuk lab ini, beta memadai.

### ✅ Semakan

- [ ] `/mcp` tunjuk `figma` **✔ Connected**
- [ ] Board FigJam ERD dijana dari ERD Mermaid Hari 1 (entiti + connector hubungan)
- [ ] (Pilihan) Section carta alir permohonan ditambah
- [ ] Board disemak di Figma; tiada entiti/aliran direka di luar sumber

---

## Latihan 3 — Kongsi dengan pasukan (skop projek)

**Objektif:** Buat rakan sepasukan dapat **konfigurasi MCP sama** tanpa setup manual.

### Langkah

1. Tambah pelayan dengan **skop projek** (tulis `.mcp.json` di root repo):

   ```bash
   claude mcp add --transport http atlassian --scope project https://mcp.atlassian.com/v1/mcp
   claude mcp add --transport http figma    --scope project https://mcp.figma.com/mcp
   ```

2. **Commit `.mcp.json`.** Ia kongsi **konfigurasi sahaja, bukan token** — setiap ahli tetap `/mcp` → **Authenticate** dengan akaun sendiri.

3. Sahkan senarai pelayan:

   ```bash
   claude mcp list
   ```

### ✅ Semakan

- [ ] `.mcp.json` di root repo mengandungi `atlassian` + `figma` (skop projek)
- [ ] `.mcp.json` di-commit; rakan lain hanya perlu `/mcp` Authenticate
- [ ] `claude mcp list` tunjuk kedua-dua pelayan

| Skop | Kesan |
|------|-------|
| Local (lalai) | Projek semasa sahaja |
| `--scope project` | Tulis `.mcp.json` — kongsi pasukan |
| `-s user` | Semua projek anda |

---

## Masalah biasa

- **`! Needs authentication`:** taip `/mcp` → pilih pelayan → **Authenticate**.
- **Akaun salah / tak nampak projek (Jira):** SSO guna akaun lain. Authenticate semula dalam tetingkap **incognito** yang log masuk akaun betul.
- **Admin org sekat OAuth pihak ketiga (Jira):** minta admin Atlassian benarkan aplikasi MCP.
- **FigJam tak berubah:** pastikan `figma` **✔ Connected**; sesetengah tindakan tulis-kanvas terhad kepada klien MCP tertentu — pastikan Claude Code versi terkini.
- **Buang pelayan:** `claude mcp remove atlassian` / `claude mcp remove figma`.

---

## Rujukan

- Jira MCP (panduan terperinci): [`cara-sambung-jira-claude-code.md`](./cara-sambung-jira-claude-code.md)
- Jana dokumentasi & diagram dengan skill: [`cara-jana-dokumentasi-diagram.md`](./cara-jana-dokumentasi-diagram.md)
- Ekosistem Claude Code: [`diagram-claude-code.md`](./diagram-claude-code.md)
- Rasmi — **Atlassian MCP:** `https://mcp.atlassian.com/v1/mcp` · **Figma MCP:** [developers.figma.com/docs/figma-mcp-server](https://developers.figma.com/docs/figma-mcp-server/remote-server-installation/) · **MCP:** [modelcontextprotocol.io](https://modelcontextprotocol.io)
