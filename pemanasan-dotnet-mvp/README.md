# Pemanasan — Persekitaran .NET & Scaffold Repo Anda

> **Sesi tambahan (pemanasan), bukan hari berkanun.** Ia mendahului **Hari 3** ([`../hari-3/`](../hari-3/)) dan tidak mengubah pemetaan 15 hari dalam [`../JADUAL.md`](../JADUAL.md) atau [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md). Tujuannya: **pasang & sahkan persekitaran .NET pada setiap mesin**, dan **bina satu MVP** (Minimum Viable Product) — aplikasi ASP.NET Core MVC terkecil yang benar-benar berjalan — **dengan tangan, sebelum kita membawa masuk Claude Code**.

> **Hari ini kita menaip semuanya sendiri.** Tiada AI menulis kod untuk anda dalam sesi ini. Sebabnya di [bahagian terakhir](#kenapa-tangan-dahulu-sebelum-claude-code). Claude Code diperkenalkan sebagai **pratonton 15 minit** di hujung hari; kerja sebenar berbantu-AI bermula **Hari 3**.

Konsep di sini. Hands-on penuh (langkah demi langkah) di [`snippets/lab.md`](./snippets/lab.md). Nota jurulatih di [`nota-penceramah.md`](./nota-penceramah.md).

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Pasang .NET SDK | [learn.microsoft.com/dotnet/core/install](https://learn.microsoft.com/en-us/dotnet/core/install/) |
| Alatan `dotnet` (CLI) | [learn.microsoft.com/dotnet/core/tools](https://learn.microsoft.com/en-us/dotnet/core/tools/) |
| `dotnet new` templates | [learn.microsoft.com/dotnet/core/tools/dotnet-new](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new) |
| ASP.NET Core MVC — gambaran | [learn.microsoft.com/aspnet/core/mvc/overview](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview) |
| `Program.cs` & hosting | [learn.microsoft.com/aspnet/core/fundamentals/minimal-apis](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) |
| Routing MVC | [learn.microsoft.com/aspnet/core/mvc/controllers/routing](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing) |
| Sijil HTTPS pembangunan | [learn.microsoft.com/aspnet/core/security/enforcing-ssl](https://learn.microsoft.com/en-us/aspnet/core/security/enforcing-ssl) |

---

## Jadual Hari Ini

Mengikut waktu dalam [`../JADUAL.md`](../JADUAL.md) (Isnin–Khamis).

| Masa | Agenda |
|------|--------|
| 9.15 – 9.30 pagi | Pendaftaran & Minum Pagi |
| **9.30 – 10.30 pagi** | **BLOK A: Persekitaran** — apa itu .NET (SDK vs runtime), pasang SDK, sahkan `dotnet --version`, editor/IDE, sijil HTTPS. 💻 Lab: Latihan 0 |
| **10.30 – 12.30 tgh** | **BLOK B: Clone & scaffold** — clone repo `nres-bpm` pasukan, scaffold skeleton (`dotnet new sln/mvc/…`), `dotnet run`, faham apa yang di-scaffold. 💻 Lab: Latihan 1 + 2 |
| 12.30 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.45 petang** | **BLOK C: Gelung MVC dengan tangan** — Model → View → Controller + borang pada **cabang buangan** (skeleton kekal bersih). 💻 Lab: Latihan 3 |
| **3.45 – 4.30 petang** | **BLOK D: PR & pratonton AI** — push skeleton → PR ke `main`; pratonton Claude Code + sempadan AI. 💻 Lab: Latihan 4 + 5 |
| 4.30 petang | Bersurai |

**Hasil hari ini:** setiap peserta mempunyai .NET 10 SDK yang disahkan, telah **clone repo pasukan** & **scaffold skeleton .NET** ke dalamnya (PR ke `main` dibuka), dan telah membina gelung **Model-View-Controller dengan tangan** (cabang buangan) **tanpa** bantuan AI.

---

## Apa itu .NET, dan apa yang kita pasang

**.NET** ialah platform untuk membina & menjalankan aplikasi C#. Dua perkataan yang selalu dikelirukan:

- **SDK (Software Development Kit)** — yang **pembangun** pasang. Ia mengandungi *runtime* + pengkompil + alatan CLI (`dotnet new`, `dotnet build`, `dotnet run`, `dotnet ef`). **Ini yang kita perlukan.**
- **Runtime** — hanya cukup untuk *menjalankan* aplikasi siap. Pengguna akhir pasang ini; pembangun tidak perlu memasangnya berasingan (SDK sudah termasuk).

Kita menggunakan **.NET 10 (LTS)** dengan **C# 14** — versi muktamad kursus (rujuk [`../AGENTS.md`](../AGENTS.md)). Ujian ringkas ada tiga versi berbeza yang mesti sepadan:

```bash
dotnet --version        # SDK  → 10.x
dotnet --list-sdks      # sekurang-kurangnya satu 10.x
dotnet --list-runtimes  # termasuk Microsoft.AspNetCore.App 10.x
```

---

## Apa itu MVC — dalam tiga ayat

ASP.NET Core MVC membahagi setiap skrin kepada tiga tanggungjawab. Ingat ia sebagai perjalanan satu permintaan (*request*):

1. **Controller** menerima permintaan (`/Permohonan`), memutuskan apa nak buat, dan menyediakan data.
2. **Model** ialah data itu — kelas C# biasa (contoh: satu `Permohonan` dengan `Rujukan`, `Modul`, `Status`).
3. **View** ialah templat Razor (`.cshtml`) yang menjadikan model itu HTML untuk pelayar.

```text
  Pelayar ──GET /Permohonan──►  Controller.Index()
                                     │  sediakan data
                                     ▼
                                   Model (List<Permohonan>)
                                     │  hantar ke
                                     ▼
                                   View (Index.cshtml)  ──HTML──►  Pelayar
```

Corak yang sama inilah yang keempat-empat kumpulan gunakan selama 11 hari — hanya modelnya berbeza.

---

## Apa itu MVP, dan kenapa kita mula dengannya

**MVP = Minimum Viable Product** — versi **terkecil** yang benar-benar berfungsi hujung-ke-hujung, cukup untuk seseorang menggunakannya dan memberi maklum balas. Bukan produk siap; **rangka berjalan** (*walking skeleton*) yang kita tokok kemudian.

MVP kita hari ini sengaja kecil:

> **Papan Permohonan NRES** — satu halaman yang menyenaraikan permohonan, dan satu borang untuk menambah permohonan baharu. Data disimpan **dalam memori sahaja** (hilang bila aplikasi dimulakan semula).

Itu bukan pepijat — itu **skop MVP**. Ia membuktikan gelung Model-View-Controller berfungsi. Menambah pangkalan data sebenar (EF Core), pengesahan pengguna (Identity), dan kelulusan datang **kemudian** — bermula Hari 3. Corak fikir ini — *bina yang terkecil yang berjalan dahulu, kemudian tokok* — ialah cara kita membina keseluruhan sistem NRES.

Kenapa **dengan tangan**? Kerana anda tidak boleh mengarah AI membina sesuatu yang anda sendiri tidak faham bentuknya. Selepas anda menaip scaffold ini sekali, `dotnet new mvc` bukan lagi kotak ajaib — anda tahu setiap fail di dalamnya.

---

## Kenapa tangan dahulu, sebelum Claude Code

Kursus ini menggunakan pembantu AI setiap hari — tetapi **hari ini tidak**. Sebabnya mudah:

- **AI mempercepat kerja yang anda faham, dan menyembunyikan kerja yang anda tidak faham.** Kalau `dotnet new mvc` dan gelung MVC masih misteri, AI yang menjananya menjadikannya *lebih* misteri, bukan kurang.
- Selepas anda membina MVP ini sendiri, anda boleh **menilai** apa yang AI keluarkan — nampak bila ia betul, bila ia salah, bila ia mereka-reka fail yang tidak wujud.
- Di hujung hari kita buat **pratonton 15 minit**: minta Claude Code menjana scaffold yang *sama*, dan bandingkan dengan yang anda taip. Itu memberi anda tanda aras sebenar sebelum kita bergantung pada AI mulai Hari 3.

**Peraturan sesi:** taip setiap baris sendiri sehingga Latihan 5. Salin-tampal dibenarkan; menjana melalui AI **tidak** — sehingga bahagian pratonton.

---

## Slaid untuk pratonton Claude Code (Blok D · Latihan 5)

Bila anda sampai ke pratonton Claude Code di [`snippets/lab.md`](./snippets/lab.md) → **Latihan 5**, gunakan kluster slaid **"Claude Code & ekosistem AI"** dalam dek kursus:

- **Fail:** [`../slides/dotnet-nres-training.html`](../slides/dotnet-nres-training.html) — buka dalam pelayar, lompat ke bahagian hujung dek (10 slaid: divider → *Apa itu Claude Code* → *Anda pandu · AI draf* → CLAUDE.md/AGENTS.md → **MCP** → **Skills** → **Subagents** → **Plugins** → peta ekosistem → selamat & etika).
- Versi boleh-edit / Google Slides: [`../slides/dotnet-nres-training.pptx`](../slides/dotnet-nres-training.pptx) (nota penceramah pada setiap slaid).

**Cadangan aliran Blok D:**

1. Peserta push skeleton bersih & buka PR ke `main` (Latihan 4) — masih dengan tangan.
2. Tayang kluster slaid Claude Code (~10 minit) — konsep: ejen, MCP, skills, subagents, plugins.
3. Demo langsung satu prompt (action + view `Butiran`) di skrin, **nilai** output bersama kelas.
4. Tutup dengan slaid *selamat & etika* → sambung ke Hari 4, di mana kerja berbantu-AI bermula.

> Slaid ini **bukan** untuk ditayang di awal hari — ia datang **selepas** peserta scaffold repo & membina gelung MVC dengan tangan. Itulah sebab kluster diletakkan di hujung dek.
