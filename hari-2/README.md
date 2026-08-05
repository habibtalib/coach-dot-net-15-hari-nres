# Hari 2 — Agile, Git, Branching & Kolaborasi Pasukan

Nota ini mengikut **aturcara rasmi HARI 2** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 5 hingga SESI 8. Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

> **Hari ini menentukan sama ada Hari 15 berjalan lancar atau menjadi malapetaka gabungan.** Empat kumpulan akan menulis kod dalam satu repositori selama 11 hari. Disiplin yang kita tetapkan hari ini ialah satu-satunya perkara yang menghalang kerja itu daripada bertembung.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Git — asas | [git-scm.com/book/ms/v2](https://git-scm.com/book/en/v2) |
| Percabangan Git | [git-scm.com/book — Branching](https://git-scm.com/book/en/v2/Git-Branching-Branches-in-a-Nutshell) |
| Rebase vs merge | [git-scm.com/book — Rebasing](https://git-scm.com/book/en/v2/Git-Branching-Rebasing) |
| Selesaikan konflik | [docs.github.com — resolving merge conflicts](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/addressing-merge-conflicts) |
| Pull request | [docs.github.com — about pull requests](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests) |
| GitHub Projects | [docs.github.com/issues/planning-and-tracking-with-projects](https://docs.github.com/en/issues/planning-and-tracking-with-projects) |
| Jira — konsep asas | [atlassian.com/software/jira/guides](https://www.atlassian.com/software/jira/guides) |
| .NET SDK & CLI | [learn.microsoft.com/dotnet/core/tools](https://learn.microsoft.com/en-us/dotnet/core/tools/) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 9.15 – 9.30 pagi | Pendaftaran & Minum Pagi |
| **9.30 – 11.00 pagi** | **SESI 5: Agile & Pengurusan Kerja** — Agile Manifesto (4 nilai), backlog, sprint, stand-up, DoD; **Jira** (epic → user story → task → bug → spike, subtask, board sendiri setiap pasukan & **swimlanes** mengikut epic, issue key) & **GitHub Projects**. 💻 Lab: backlog modul dari URS Hari 1. *Rancang kerja dahulu — belum sentuh Git.* |
| **11.00 – 12.30 tgh** | **SESI 6: Git Asas & Repositori** — `clone`, `status`, `add`, `commit`, `push`, `pull --rebase`; format mesej commit (sertakan issue key); `.gitignore`. 💻 Lab: commit pertama |
| 12.30 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.30 petang** | **SESI 7: Strategi Percabangan & Code Review** — cabang kumpulan, cabang ciri, pull request, templat PR, senarai semak penyemak; **selesaikan konflik gabungan secara langsung**. 💻 Lab: cipta konflik & selesaikannya |
| **3.30 – 4.30 petang** | **SESI 8: Kolaborasi, AI Berpasukan & Persekitaran** — matriks pemilikan fail, slot migration, `AGENTS.md`; pasang .NET 10 SDK & tools. 💻 Lab: persekitaran sedia + tandatangan kontrak |
| 4.30 petang | Bersurai |

**Hasil Hari 2:** Repo dengan 4 cabang kumpulan; board Agile berisi backlog setiap modul; setiap peserta boleh commit, buka PR, dan selesaikan konflik; persekitaran .NET 10 berjalan pada setiap mesin.

---

## SESI 5 — Agile & Pengurusan Kerja

> 🎬 **Video rujukan:** [Agile — pengenalan](https://www.youtube.com/watch?v=8eVXTyIZ1Hs) (sprint, backlog, board).

### Konsep yang benar-benar penting

Kita tidak mengajar sijil Scrum. Kita mengajar lima perkara yang menjadikan pasukan 4-kumpulan berfungsi:

| Konsep | Maksud dalam kursus kita |
|--------|--------------------------|
| **Backlog** | Senarai kerja yang belum siap, disusun mengikut keutamaan — datang terus dari URS Hari 1 |
| **Sprint** | Satu blok kerja bermatlamat. Bagi kita, setiap **blok trek** ialah satu sprint (Hari 4, 5–6, 7–9, 10–12, 13–14) |
| **Stand-up** | 15 minit setiap pagi: semalam / hari ini / halangan |
| **Definition of Done** | Bila sesuatu benar-benar siap — satu definisi, semua kumpulan ([`../KOLABORASI.md`](../KOLABORASI.md) §9) |
| **Board** | Papan lihat-pandang: To Do → In Progress → In Review → Done |

**Kenapa ini penting di sini secara khusus:** setiap pasukan mengurus **board sendiri**, dan board itu ialah cara jurulatih menyemak kemajuan pasukan itu. Isu yang tidak ada pada board ialah kerja yang tiada siapa tahu sedang berlaku. Penyelarasan **antara** pasukan (elak dua pasukan bina benda sama) datang dari stand-up harian + semakan silang AI + [`../AGENTS.md`](../AGENTS.md) — bukan satu board dikongsi.

### GitHub Projects — hands-on dalam kelas

Kita gunakan GitHub Projects untuk kerja sebenar kursus kerana ia hidup di tempat yang sama dengan kod:

```text
Isu #14 "Semakan pendua nombor plat"
   ↓  cipta cabang dari isu
kump-2/feat/semakan-pendua-plat
   ↓  commit dengan "Closes #14"
Pull request
   ↓  disemak & digabung
Isu ditutup automatik → kad bergerak ke Done
```

Rantaian itu — isu → cabang → PR → tutup — ialah keseluruhan aliran kerja. Semuanya boleh dijejak, tiada langkah manual.

### Jira — demo & pemetaan

Ramai peserta akan menggunakan **Jira** di pejabat, jadi kita tunjukkan pemetaannya:

| GitHub Projects | Jira | Nota |
|-----------------|------|------|
| Isu | Story / Task / Bug | Jira mempunyai jenis isu yang lebih kaya |
| Label | Label / Component | |
| Milestone | Sprint / Version | Jira memisahkan kedua-duanya |
| Papan Projects | Sprint board / Kanban board | |
| `Closes #14` dalam commit | `NRES-42` dalam mesej commit | Jira memadan issue key secara automatik |
| Epic (melalui label) | **Epic** (jenis isu terbina) | Hierarki Jira lebih tegas |

**Hierarki Jira** yang perlu diketahui:

```text
Epic:    "Modul Pas, Parkir & Pelekat"
 └─ Story:   "Sebagai pemohon, saya mahu memohon pelekat kenderaan"
     └─ Subtask: "Tambah entiti Vehicle + konfigurasi"
     └─ Subtask: "Bina borang permohonan pelekat"
     └─ Subtask: "Laksana semakan pendua plat"
```

**Jenis isu Jira** yang kita guna: **Epic** (matlamat/modul, payung banyak story) · **User Story** (*Sebagai… saya mahu… supaya…* — nilai pengguna) · **Task** (kerja teknikal tanpa muka pengguna) · **Bug** (kelakuan salah) · **Spike** (kotak-masa untuk siasat/belajar sebelum anggar — hasilnya keputusan, bukan ciri) · **Subtask** (pecahan kecil di bawah story/task).

**Setiap pasukan ada board sendiri** — bukan satu board dikongsi. Dalam board sesebuah pasukan, **swimlane** ialah baris yang mengumpulkan kad, biasanya **mengikut epic**. Contoh Kumpulan 1 (yang memikul 3 projek) — 3 epic menjadi 3 swimlane:

```text
Board KUMPULAN 1        │ To Do  │ In Progress │ In Review │ Done   │
 Epic: Lapor Diri       │ LD-14  │ LD-12       │ LD-08     │ LD-03  │
 Epic: Pematuhan PKS    │ PKS-06 │ PKS-04      │           │ PKS-01 │
 Epic: Pengurusan Kontrak│ KON-09 │            │ KON-05    │        │
```

Lajur ialah status (To Do → In Progress → In Review → Done); kad bergerak kiri → kanan dan "Done" bermaksud lulus DoD. Penyelarasan **antara** 4 pasukan datang dari stand-up harian + [`../AGENTS.md`](../AGENTS.md), bukan board dikongsi.

**Peraturan yang terpakai pada kedua-dua alat:** setiap kerja mempunyai kad. Jika anda mengerjakan sesuatu yang tiada pada board, hentikan dan buat kadnya dahulu — bukan kerana proses, tetapi kerana itulah cara tiga kumpulan lain tahu anda sedang membinanya.

---

## SESI 6 — Git Asas & Repositori

### Kenapa Git, bukan folder kongsi

Sebelum Git, pasukan berkongsi kod melalui folder rangkaian dan fail zip bernama `sistem_final_v2_FIXED_guna_ini.zip`. Masalahnya bukan kesusahan — masalahnya **tiada siapa boleh menjawab soalan asas**: siapa mengubah baris ini? kenapa? apa keadaan kod minggu lepas? bolehkah dua orang bekerja pada hari yang sama tanpa menimpa satu sama lain?

Git menjawab kesemuanya. Bagi kursus ini, satu perkara paling penting: **Git membolehkan empat kumpulan bekerja serentak dan menggabungkan kerja mereka dengan cara yang boleh diperiksa.**

### Model mental yang perlu

Git menyimpan **snapshot**, bukan perbezaan. Setiap `commit` ialah gambar lengkap projek pada satu masa, ditambah rujukan kepada commit sebelumnya. Cabang (`branch`) hanyalah **penunjuk** ke satu commit — itulah sebabnya mencipta cabang hampir serta-merta.

```text
master     A───B───C
                    ╲
kump-2              D───E───F      ← cabang kumpulan anda
```

Tiga "kawasan" yang perlu difahami:

| Kawasan | Maksud | Arahan |
|---------|--------|--------|
| **Working directory** | Fail yang anda edit sekarang | — |
| **Staging area** | Perubahan yang anda pilih untuk commit seterusnya | `git add` |
| **Repository** | Sejarah commit yang tersimpan | `git commit` |

**Kenapa staging area wujud?** Ia membolehkan anda commit **sebahagian** kerja anda. Anda membetulkan pepijat dan menambah ciri dalam sesi yang sama — stage dan commit secara berasingan, dan sejarah anda kekal bermakna.

### Arahan harian

```bash
git status                    # apa yang berubah?
git add <fail>                # stage perubahan tertentu
git commit -m "mesej"         # simpan snapshot
git pull --rebase origin master   # ambil kerja orang lain, letak kerja saya di atas
git push origin <cabang>      # hantar ke GitHub
git log --oneline --graph     # lihat sejarah
```

### `pull --rebase`, bukan `pull` biasa

Ini penting untuk kursus ini.

`git pull` biasa mencipta **commit gabungan** setiap kali anda menyegerak. Dengan empat kumpulan yang menarik beberapa kali sehari, sejarah menjadi jalinan commit gabungan yang mustahil dibaca.

`git pull --rebase` mengambil commit **anda**, menyimpannya sementara, mengemas kini cabang anda kepada kerja terkini orang lain, kemudian **meletakkan semula commit anda di atasnya**. Sejarah kekal linear dan boleh dibaca.

```text
Tanpa rebase:  A───B───────M       Dengan rebase:  A───B───C───D'
                ╲         ╱                        (bersih, linear)
                 C───D───╱
```

> **Peraturan kursus:** `git pull --rebase origin master` **setiap pagi**, sebelum apa-apa kerja. Konflik yang ditemui hari ini mengambil 5 minit; konflik yang sama ditemui pada Hari 15 mengambil 2 jam.

### Mesej commit yang baik

Mesej commit ialah nota yang anda tinggalkan untuk diri anda tiga minggu akan datang — dan untuk penyemak PR anda esok.

**Format kursus** (rujuk [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md)):

```text
<modul>: <apa yang berubah dalam Bahasa Melayu ringkas>
```

| ❌ Buruk | ✅ Baik |
|---------|--------|
| `update` | `lapor-diri: tambah muat naik lampiran dan metadata Attachment` |
| `fix bug` | `akses: sekat permohonan pelekat pendua bagi nombor plat sama` |
| `wip` | `akaun: tambah skrin kelulusan Penyelia peringkat 1` |
| `asdf` | `aset: kemas kini status aset kepada OnLoan selepas kelulusan` |

Jika kumpulan anda menggunakan Jira, letak issue key di hadapan: `NRES-42 lapor-diri: ...`

**Commit kecil dan kerap.** Satu commit = satu perubahan bermakna. Commit besar mustahil disemak dan mustahil dipatah balik dengan selamat.

### `.gitignore`

Sesetengah fail **tidak sepatutnya** berada dalam Git: output binaan, pangkalan data tempatan, fail muat naik, tetapan IDE. Repo kursus sudah mempunyainya:

```gitignore
[Bb]in/
[Oo]bj/
*.db
App_Data/uploads/
.vs/
```

**Kenapa ini penting bagi kita secara khusus:** jika seseorang meng-commit `*.db` mereka, setiap orang lain mendapat pangkalan data **mereka** pada `pull` seterusnya — dan setiap `pull` selepas itu menjadi konflik binari yang tidak boleh diselesaikan. Ini berlaku dalam kursus sebenar. `.gitignore` menghalangnya.

---

## SESI 7 — Percabangan & Code Review

### Strategi percabangan kita

```text
master                        # integrasi; dilindungi, PR sahaja
├── asas/shared-foundation    # Hari 3 — digabung ke master hujung Hari 3
├── kump-1/lapor-diri         # Kumpulan 1, Hari 4–14
├── kump-2/akses-kenderaan    # Kumpulan 2, Hari 4–14
├── kump-3/id-ad-email        # Kumpulan 3, Hari 4–14
└── kump-4/perisian-aset      # Kumpulan 4, Hari 4–14
```

Kerja harian berlaku dalam **cabang ciri pendek** yang bercabang dari cabang kumpulan anda:

```text
kump-2/akses-kenderaan
└── kump-2/feat/semakan-pendua-plat   → PR → kump-2/akses-kenderaan
```

**Kenapa cabang ciri, bukan terus ke cabang kumpulan?** Kerana ia memberi anda tempat semula jadi untuk **code review**. PR ialah perbualan tentang perubahan sebelum ia menjadi sebahagian kerja pasukan.

**Kenapa `master` dilindungi?** Kerana `master` mesti sentiasa boleh dibina. Jika sesiapa boleh push terus, seseorang akan memecahkannya pada pukul 4.55 petang dan tiga kumpulan lain akan tersekat.

### Konflik gabungan — apa ia sebenarnya

Konflik berlaku apabila dua orang mengubah **baris yang sama** dalam fail yang sama, dan Git tidak boleh memutuskan mana yang betul. Git tidak cuba meneka — ia menandakan konflik dan meminta anda memutuskan.

```text
<<<<<<< HEAD
var pas = await _db.AccessPassApplications.ToListAsync();
=======
var pas = await _db.AccessPassApplications.Where(x => x.IsActive).ToListAsync();
>>>>>>> kump-2/feat/tapisan-aktif
```

- Di atas `=======` — versi **anda** (cabang semasa)
- Di bawah — versi **masuk**
- Anda memilih satu, kedua-duanya, atau menulis sesuatu yang baharu; kemudian **buang penanda**

**Konflik bukan kegagalan.** Ia isyarat bahawa dua orang mengerjakan bahagian yang sama. Penyelesaian sebenar selalunya bukan teknikal — ia bercakap dengan orang itu.

### Kenapa kita akan mencipta konflik dengan sengaja hari ini

Kali pertama anda melihat konflik gabungan **tidak sepatutnya** pada Hari 15 dengan empat kumpulan menunggu. Dalam lab, kita akan menciptanya dengan sengaja dan menyelesaikannya sehingga ia terasa biasa.

### Bagaimana seni bina kita mengelakkan kebanyakan konflik

Sebahagian besar konflik dalam projek berbilang pasukan berlaku pada beberapa fail sahaja: `Program.cs`, `DbContext`, layout, snapshot migration. Kita **mereka bentuknya supaya hilang** (dibina Hari 3):

| Punca konflik biasa | Penyelesaian kita |
|---------------------|-------------------|
| 4 kumpulan edit `Program.cs` | Setiap modul ada `Add<Modul>Module()` sendiri; `Program.cs` ada 4 baris, ditulis sekali |
| 4 kumpulan edit `ApplicationDbContext` | `IEntityTypeConfiguration<T>` dalam folder modul + `ApplyConfigurationsFromAssembly()` |
| 4 kumpulan edit menu `_Layout.cshtml` | Navigasi didorong `ModuleDescriptor` — tambah fail baharu, bukan edit |
| Snapshot migration bertembung | **Slot migration** bergilir |

Butiran penuh: [`../KOLABORASI.md`](../KOLABORASI.md) §3.

### Code review

Setiap PR memerlukan seorang penyemak. Penyemak menjawab **empat soalan mengikut turutan**:

1. **Adakah ini sudah wujud dalam repo?** — soalan paling penting; ini yang menghalang duplikasi
2. **Adakah ia menyentuh fail milik orang lain?** — ini yang menghalang konflik
3. Adakah authorization & validation betul?
4. Bolehkah penulis menerangkan setiap baris? *(khususnya kod jana-AI)*

> Code review mengambil kira **15%** penilaian capstone. Ia bukan formaliti.

---

## SESI 8 — Kolaborasi, AI Berpasukan & Persekitaran

### Matriks pemilikan fail

Setiap laluan fail ada **tepat satu** pemilik. Anda tidak menyunting apa yang bukan milik anda — anda buka isu.

Matriks penuh: [`../KOLABORASI.md`](../KOLABORASI.md) §2. Ringkasan:

| Anda | Anda memiliki |
|------|---------------|
| Kumpulan 1 | `Models/LaporDiri/`, `Controllers/OfficerReporting*`, `Views/OfficerReporting/`, `ViewModels/LaporDiri/`, `Services/LaporDiri/` |
| Kumpulan 2 | `Models/Akses/`, `Controllers/{AccessPass,Parking,VehicleSticker}*`, `Views/Akses/`, … |
| Kumpulan 3 | `Models/Akaun/`, `Controllers/AccountRequest*`, `Views/Akaun/`, … |
| Kumpulan 4 | `Models/Aset/`, `Controllers/{Asset,Software}*`, `Views/Aset/`, … |
| **Tiada siapa** (beku selepas Hari 3) | `Program.cs`, `Data/ApplicationDbContext.cs`, `Views/Shared/_Layout.cshtml`, `wwwroot/css/site.css`, `Models/Shared/` |

**Semakan sebelum setiap commit:**

```bash
git diff --name-only master
```

Jika ada fail di luar folder kumpulan anda dalam senarai itu — berhenti dan baca [`../KOLABORASI.md`](../KOLABORASI.md) §4.

### Slot migration

EF Core menyimpan **satu** fail snapshot bagi seluruh skema. Dua kumpulan yang menjana migration serentak akan berkonflik padanya, dan **konflik snapshot tidak boleh diselesaikan dengan tangan secara selamat**.

Protokol: umumkan → `pull --rebase` → jana → uji → push → lepaskan slot. Jika anda tetap berkonflik: **buang migration anda dan jana semula** di atas snapshot terkini. Jangan sekali-kali membaikinya dengan tangan.

Langkah penuh: [`../KOLABORASI.md`](../KOLABORASI.md) §5.

### AI dalam pasukan — masalah yang tidak jelas

Keempat-empat kumpulan akan menggunakan AI. Inilah yang berlaku tanpa koordinasi:

> Kumpulan 1 meminta AI: *"Tulis servis untuk menjana nombor rujukan."* Ia menulis satu yang baik.
> Kumpulan 3 meminta AI perkara yang sama pada hari yang sama. Ia menulis satu lagi yang baik — dengan nama berbeza, corak berbeza, format berbeza.
> Kedua-duanya berfungsi. Kedua-duanya lulus ujian. Pada Hari 15 kita mempunyai dua servis nombor rujukan dan tiada siapa tahu yang mana kanonik.

AI tidak melakukan kesilapan di sini. Ia menjawab soalan yang diajukan. **Masalahnya ialah tiada satu pun daripada kedua-dua sesi AI tahu tentang yang satu lagi.**

**Penyelesaian kita: satu fail konteks kongsi.** Keempat-empat kumpulan menghalakan pembantu AI mereka ke [`../AGENTS.md`](../AGENTS.md), yang mengandungi daftar komponen kongsi, peta pemilikan folder, corak kod, dan peraturan mutlak. Konteks yang sama masuk = output yang serasi keluar.

**Tujuh peraturan AI** (penuh: [`../KOLABORASI.md`](../KOLABORASI.md) §7):

1. Setiap sesi AI bermula dengan `AGENTS.md`
2. **Cari dahulu, jana kemudian** — prompt pertama sentiasa *"adakah ini sudah wujud?"*
3. Sekat skop AI kepada folder kumpulan anda secara eksplisit
4. **Tiada commit tanpa faham** — terangkan kepada rakan sekumpulan dahulu
5. AI tidak mereka keperluan
6. Tiada data NRES sebenar dalam prompt
7. Guna AI sebagai **penyemak** sebelum PR, bukan hanya penjana

### Semakan silang AI harian

Setiap pagi selepas stand-up, 10 minit: setiap kumpulan menunjukkan satu perkara ketara yang dijana AI semalam. Kumpulan lain menjawab satu soalan: *"Adakah kami baru sahaja membina benda yang sama?"*

Ini menangkap pertindihan pada **hari** ia berlaku. Tanpa ini, ia ditemui pada Hari 15, apabila membuangnya bermakna membuang kerja tiga hari.

### Persediaan persekitaran

Sebelum Hari 3, setiap mesin memerlukan:

| Alat | Sahkan dengan |
|------|---------------|
| .NET 10 SDK | `dotnet --version` → `10.x` |
| `dotnet ef` CLI | `dotnet ef --version` |
| Git | `git --version` |
| IDE | VS 2022 (17.12+) atau VS Code + C# Dev Kit |
| Akses GitHub | `git clone` repo kursus berjaya |

Panduan penuh: [`../nota/00-setup-dotnet.md`](../nota/00-setup-dotnet.md).

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md) — anda akan:

1. Bina backlog modul anda dalam GitHub Projects dari URS Hari 1 — **board sendiri, swimlane mengikut epic**
2. Petakan backlog yang sama kepada struktur Jira (**board Jira sendiri setiap pasukan**)
3. Clone repo & buat commit pertama (dokumen Hari 1 anda)
4. Buka pull request, semak PR kumpulan lain, dan cipta cabang kumpulan
5. Bina & selesaikan konflik gabungan sebenar dengan sengaja
6. Konfigurasi pembantu AI anda dengan `AGENTS.md` dan uji peraturan "cari dahulu"
7. Sahkan persekitaran .NET 10 & tandatangani kontrak `KOLABORASI.md`

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
