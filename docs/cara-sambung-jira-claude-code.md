# Cara sambung Jira ke Claude Code (CLI & VS Code)

> **Untuk peserta yang TIADA MCP Jira peribadi.** Panduan ini guna pelayan **MCP Atlassian rasmi** (OAuth) — cara standard menyambung Jira/Confluence ke Claude Code. Selepas disambung, AI boleh **senarai & cipta isu** terus dari user story PRD anda tanpa menyalin manual.

## Sebelum mula

- **Claude Code** dipasang (CLI atau sambungan VS Code).
- Akaun **Atlassian** yang ada akses ke tapak Jira projek anda.

## Fakta penting

Sambungan **VS Code** dan **CLI** **kongsi konfigurasi yang sama** (`~/.claude/settings.json` + `.mcp.json` di root projek). Jadi anda tambah pelayan **sekali** sahaja — ia berfungsi di kedua-dua tempat.

---

## A · CLI (terminal)

1. Tambah pelayan MCP Atlassian:

```bash
claude mcp add --transport http atlassian https://mcp.atlassian.com/v1/mcp
```

2. **Autentikasi.** Dalam Claude Code, taip `/mcp` → pilih **atlassian** → **Authenticate** → log masuk dalam pelayar. Guna tetingkap **incognito** jika anda ada beberapa akaun Atlassian, supaya SSO tidak pilih akaun salah.

3. **Sahkan.** `/mcp` patut menunjukkan `atlassian` **✔ Connected**. Atau tanya:

```text
Senaraikan projek Jira yang saya boleh akses.
```

---

## B · VS Code (sambungan Claude Code)

Sama sahaja — cuma tempat menaip berbeza:

1. Buka **terminal bersepadu**: `` Ctrl+` `` (Windows/Linux) atau `` Cmd+` `` (Mac), kemudian jalankan arahan `claude mcp add …` yang **sama** seperti Bahagian A.

2. Dalam **panel chat** Claude Code, taip `/mcp` → pilih **atlassian** → ikut aliran OAuth (sama seperti CLI). Status muncul dalam dialog: `✔ Connected` · `! Needs authentication` · `✘ Failed`.

> Tiada tetapan "enable MCP" khas untuk VS Code. Tambah pelayan di **terminal bersepadu**, urus/authenticate melalui `/mcp` dalam **chat**.

---

## Guna: cipta isu dari PRD

```text
Dalam projek Jira <KEY>, cipta satu Task untuk user story US-1 PRD kami:
tajuk ringkas + perihalan, dan salin acceptance criteria PRD sebagai kriteria penerimaan.
Tunjukkan draf dahulu sebelum mencipta.
```

Kunci projek per sistem: `LD` · `PKS` · `CM` · `PPK`/`PK`/`PASP` · `ID` · `FS`.

## Kongsi dengan pasukan (pilihan)

Skop pelayan ke repo supaya rakan sepasukan dapat konfigurasi sama:

```bash
claude mcp add --transport http atlassian --scope project https://mcp.atlassian.com/v1/mcp
```

- Menghasilkan **`.mcp.json`** di root repo — **commit** untuk kongsi.
- Ia kongsi **konfigurasi sahaja, bukan token**. Setiap ahli tetap **autentikasi sendiri** (`/mcp`) dengan akaun mereka.

## Skop pemasangan

| Skop | Arahan | Kesan |
|------|--------|-------|
| Local (lalai) | `claude mcp add --transport http atlassian <url>` | Projek semasa sahaja |
| Project | `claude mcp add --transport http atlassian --scope project <url>` | Tulis `.mcp.json` — kongsi pasukan |
| User | `claude mcp add --transport http atlassian -s user <url>` | Semua projek anda |

*(`<url>` = `https://mcp.atlassian.com/v1/mcp`)*

## Semak & buang

```bash
claude mcp list            # senarai pelayan MCP
claude mcp remove atlassian
```

## Masalah biasa

- **Akaun salah / tak nampak projek:** SSO guna akaun lain. Autentikasi semula (`/mcp`) dalam tetingkap **incognito** yang log masuk akaun betul.
- **`! Needs authentication`:** taip `/mcp` → pilih `atlassian` → **Authenticate**.
- **Admin org sekat OAuth pihak ketiga:** minta admin Jira benarkan aplikasi Atlassian MCP.

---

> Rujukan lab: [`hari-2/snippets/lab.md`](../hari-2/snippets/lab.md) **Latihan 6b**. Diagram ekosistem: [`diagram-claude-code.md`](./diagram-claude-code.md).
