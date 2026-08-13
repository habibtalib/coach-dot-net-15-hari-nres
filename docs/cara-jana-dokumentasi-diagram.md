# Cara jana dokumentasi & diagram (Mermaid) dengan AI

> **Bahan rujukan kursus (Hari 2 · Claude Code).** Dua kaedah untuk menghasilkan dokumentasi modul + diagram Mermaid dari PRD/URS/ERD: **(A) skill** boleh guna semula, **(B) prompt terus**. Semua **berpaksi PRD** — AI draf, anda **sahkan**.

## A · Skill `/dok-modul` (Claude Code)

Skill = **folder** dengan fail `SKILL.md`, dipanggil `/nama`. Bungkus tugas berulang supaya seluruh pasukan hasilkan dokumentasi/diagram yang **konsisten**.

1. Cipta `.claude/skills/dok-modul/SKILL.md`:

````markdown
---
name: dok-modul
description: Jana/kemas kini dokumentasi & diagram Mermaid modul ikut konvensyen NRES (BM, berpaksi PRD, jangan reka keperluan)
---

Bila dipanggil:
1. Baca PRD modul (`docs/prd-modul-N.md`) dan `AGENTS.md` repo ini.
2. Jana/kemas kini `docs/README-modul-N.md`: gambaran modul & pengguna, senarai fungsi utama, aliran permohonan (langkah demi langkah).
3. Jana diagram Mermaid: `erDiagram` entiti utama & hubungan, dan `flowchart` proses permohonan → kelulusan.
4. Guna Bahasa Melayu. JANGAN reka keperluan atau entiti yang tiada dalam PRD/ERD.
5. Tunjukkan perubahan (diff) dahulu sebelum menulis fail.
````

2. Panggil skill dalam Claude Code: `/dok-modul`
3. **(Pilihan)** Commit `.claude/skills/dok-modul/` supaya pasukan kongsi skill yang sama.

## B · Prompt terus (mana-mana alat AI)

Guna ini untuk **faham** apa yang skill lakukan — atau jika alat anda bukan Claude Code.

**1. Dokumentasi modul** (lampir PRD):

```text
Berdasarkan PRD modul kami di bawah, tulis dokumentasi ringkas (docs/README-modul-N.md):
- gambaran modul & pengguna
- senarai fungsi utama
- aliran permohonan (langkah demi langkah)
Guna Bahasa Melayu, ringkas dan jelas. Jangan tambah ciri yang tiada dalam PRD.

[tampal PRD di sini]
```

**2. Diagram ERD (Mermaid)** (lampir ERD):

```text
Berdasarkan ERD kami di bawah, beri kod Mermaid `erDiagram` untuk entiti utama & hubungan.
Kod Mermaid sahaja supaya saya boleh tampal terus. Jangan reka entiti baharu.

[tampal ERD di sini]
```

**3. Carta alir proses (Mermaid):**

```text
Berdasarkan use case/PRD kami, beri kod Mermaid `flowchart`:
Mohon → semak (bertindih / pendua / kelengkapan) → kelulusan admin → audit.
Kod Mermaid sahaja. Ikut peranan & status dalam PRD.
```

## Semak (wajib)

- Simpan hasil dalam `docs/`; sahkan diagram **merender** (VS Code atau GitHub).
- **Semak silang:** adakah dokumentasi/diagram menokok keperluan atau entiti yang tiada dalam PRD/ERD? Betulkan dengan tangan.
- Fail Mermaid dalam repo = **boleh review seperti kod**. Tiada commit tanpa faham.

---

> Rujukan lab: [`hari-2/snippets/lab.md`](../hari-2/snippets/lab.md) **Latihan 6b**. Contoh diagram: [`diagram-claude-code.md`](./diagram-claude-code.md).
