# Alatan Fasa 1 — Design Thinking, Perancangan & Diagram

Nota rujukan **alatan** (tools) yang menyokong kerja **Fasa 1** — sebelum satu baris kod ditulis. Ia melengkapi mesej kursus: seorang **software engineer menyelesaikan masalah**, bukan sekadar menaip kod. Editor kod (VS Code, Visual Studio, Rider) hanyalah **langkah terakhir** dalam aliran kerja:

```text
Faham masalah → Rancang & lukis diagram → URS → ERD → Jira/inception → Git branch & versioning → Kod
```

> **Falsafah alatan:** pilih yang **percuma**, **boleh version-control** (diagram-as-code, Markdown+Git), dan mesra **on-prem/kerajaan** bila boleh. Alatan yang sudah wujud dalam kursus ditandai **✅ (dalam stack)**.

---

## 1. Design Thinking / Whiteboarding

Empathy map, persona, affinity mapping, customer journey, brainstorming.

| Alatan | Catatan | Percuma |
|--------|---------|---------|
| **FigJam** (Figma) | All-rounder terbaik; templat empathy map, persona, journey siap sedia | Tier percuma murah hati |
| **Miro** | Standard industri untuk workshop; pustaka templat besar | Had 3 board percuma |
| **Excalidraw** | Open-source, rasa lakaran tangan, tiada login — pantas untuk sketch | ✔ (sumber terbuka) |
| **Whimsical** | Kemas & pantas; gabung sticky-notes + flowchart + wireframe | Tier percuma terhad |
| **Mural** | Alternatif Miro berorientasi enterprise | Percubaan sahaja |

**Cadangan kursus:** **FigJam** untuk sesi empati/persona, atau **Excalidraw** jika mahu sesuatu yang percuma sepenuhnya tanpa akaun.

---

## 2. Perancangan Projek / Agile

Backlog, sprint, Kanban board, Definition of Done, penjejakan kerja.

| Alatan | Catatan | Percuma |
|--------|---------|---------|
| **Jira** ✅ | Alat demo kursus (Hari 2). Epic → Story → Subtask, sprint board | Sehingga 10 pengguna |
| **GitHub Projects** ✅ | Alat hands-on kursus. Issue → board → branch → PR | ✔ (dengan repo) |
| **Trello** | Kanban paling ringkas; sesuai sebelum beralih ke Jira | Tier percuma baik |
| **Linear** | Pantas & moden; popular pasukan dev | Tier percuma terhad |
| **Notion / ClickUp** | Docs + tugasan + wiki dalam satu; boleh simpan nota kursus juga | Tier percuma baik |
| **OpenProject / Taiga / Plane** | **Sumber terbuka, boleh self-host** — penting untuk keperluan on-prem kerajaan | ✔ (sumber terbuka) |

**Cadangan kursus:** kekal dengan **GitHub Projects** (hands-on) + **Jira** (demo) seperti dalam [`JADUAL.md`](../JADUAL.md) Hari 2.

---

## 3. Diagram — Flowchart, Use Case, ERD, Seni Bina

| Alatan | Guna untuk | Catatan |
|--------|-----------|---------|
| **Mermaid** ✅ | Flowchart, use case, ERD, sequence | **Diagram-as-code** — hidup dalam Markdown/Git, boleh di-*review* & di-*version* |
| **draw.io / diagrams.net** | Flowchart, UML, ERD, C4 | Percuma, ada **aplikasi desktop offline**, tiada akaun perlu |
| **PlantUML** | UML (use case, sequence, class) | Berasaskan teks; berpasangan elok dengan VS Code |
| **dbdiagram.io / DBML** | **ERD** | Khusus ERD daripada teks ringkas — sesuai untuk skema EF Core |
| **Lucidchart** | ERD, flowchart | Kemas & kolaboratif; berbayar untuk guna serius |
| **Excalidraw** | Lakaran seni bina pantas | Rasa tangan, tiada login |

**Cadangan kursus:** **Mermaid** (kekal dalam repo, seperti Hari 1) untuk process flow, use case & ERD; **draw.io** bila perlu kanvas visual bebas; **dbdiagram.io** untuk mereka ERD sebelum menulis `IEntityTypeConfiguration<T>`.

---

## 4. Dokumentasi — URS/SRS, Spesifikasi, Nota

| Alatan | Catatan |
|--------|---------|
| **Markdown + Git** ✅ | Yang digunakan repo ini — boleh *review*, boleh *diff*, tiada *lock-in* |
| **Confluence** | Berpasangan dengan Jira; wiki URS/SRS formal |
| **Notion** | Docs + pangkalan data fleksibel |
| **Obsidian** | Nota Markdown *local-first* dengan pautan — sesuai pengetahuan peribadi |
| **Google Docs** | Draf kolaboratif mudah |

**Cadangan kursus:** tulis URS/SRS sebagai **Markdown dalam repo** (`docs/URS-modul-N.md`) supaya ia sebahagian aliran Git yang sama seperti kod.

---

## 5. Git / Kawalan Versi (pelengkap editor kod)

| Alatan | Catatan |
|--------|---------|
| **Git CLI** ✅ | Asas yang diajar kursus (Hari 2) |
| **GitHub Desktop** | Git visual mesra pemula — bagus untuk peserta baharu Hari 2 |
| **GitKraken / Sourcetree / Fork** | *Branching* visual + UI penyelesaian konflik *merge* |

**Cadangan kursus:** ajar **Git CLI** sebagai asas; benarkan **GitHub Desktop** untuk peserta yang belum selesa dengan terminal.

---

## Set minimum (kebanyakannya percuma) yang mencerminkan kursus ini

| Fasa | Alatan disyorkan |
|------|------------------|
| Design Thinking | **FigJam** (atau **Excalidraw** — percuma penuh) |
| Perancangan / Agile | **GitHub Projects** + **Jira** |
| Diagram / ERD | **Mermaid** + **draw.io** + **dbdiagram.io** |
| Dokumentasi (URS/SRS) | **Markdown dalam Git** |
| Git untuk pemula | **GitHub Desktop** |
| Editor kod | **VS Code** / **Visual Studio 2022** / **Rider** *(langkah terakhir)* |

---

## Rujukan berkaitan

- [`JADUAL.md`](../JADUAL.md) — Hari 1 (perancangan, URS, ERD) & Hari 2 (Git, Agile).
- [`hari-1/README.md`](../hari-1/README.md) — konsep URS/SRS, use case & ERD dengan Mermaid.
- [`KOLABORASI.md`](../KOLABORASI.md) — kontrak pasukan: board Agile, PR/review, Definition of Done.
- [`nota/10-rujukan-buku.md`](./10-rujukan-buku.md) — pemetaan kursus → bab buku rujukan.
