# Lab Hari 2 — Agile, Git, Branching & Kolaborasi

> ⚠️ **Nota migrasi (poly-repo):** lab ini sedang dikemas kini kepada seni bina **poly-repo** (6 repo dalam `nres-bpm`, Profile DB dikongsi). Sebahagian langkah masih mengandaikan model lama (satu repo · `master` · cabang kumpulan bersama). **Kanun terkini & muktamad:** [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md) · [`../../AGENTS.md`](../../AGENTS.md) · [`../../KOLABORASI.md`](../../KOLABORASI.md).

> Konsep di [`../README.md`](../README.md). Kontrak pasukan: [`../../KOLABORASI.md`](../../KOLABORASI.md). Konteks AI: [`../../AGENTS.md`](../../AGENTS.md).

## Persediaan

- Git dipasang (`git --version`)
- Akaun GitHub dengan akses tulis ke repo kursus
- Dokumen `docs/` dari Hari 1
- Editor teks
- Pembantu AI

---

## Latihan 0 — Bina backlog dalam GitHub Projects

**Objektif:** Tukar **user story PRD** (dan URS) Hari 1 menjadi backlog yang boleh dilaksanakan.

### Langkah

1. **Setiap kumpulan mencipta board GitHub Project sendiri** (bukan satu board dikongsi) dengan lajur:

```text
To Do  →  In Progress  →  In Review  →  Done
```

   Guna **swimlane mengikut epic**. Kumpulan 1 (3 projek) → 3 swimlane: `Lapor Diri` · `Pematuhan PKS` · `Pengurusan Kontrak`. Kumpulan lain → swimlane mengikut epic/ciri utama modul mereka.

2. **Setiap kumpulan mencipta isu dari user story PRDnya.** Bagi setiap **user story** (US-n) — atau keperluan "Mesti ada" URS yang belum ada user story — cipta satu isu. Salin **acceptance criteria** PRD terus ke medan kriteria penerimaan:

```markdown
Tajuk: [K2] Semakan pendua nombor plat

## Sumber
User story PRD: US-<n>  ·  Keperluan URS: URS-PAS-007

## Perihalan
Sistem menolak permohonan pelekat baharu jika nombor plat yang sama sudah
mempunyai permohonan berstatus Submitted, SupervisorApproved, atau AdminApproved.

## Kriteria penerimaan (dari acceptance criteria PRD)
- [ ] Semakan berjalan sebelum menyimpan permohonan
- [ ] Mesej ralat menamakan nombor plat bertindih
- [ ] Permohonan Rejected/Cancelled TIDAK menyekat permohonan baharu
- [ ] Diliputi ujian unit

## Anggaran
Sederhana (½ hari)
```

3. **Labelkan setiap isu:**
   - Epic / modul: cth `lapor-diri`, `pematuhan-pks`, `pengurusan-kontrak` — ini yang jadi swimlane
   - Jenis: `feature` · `bug` · `docs` · `test` · `spike`
   - **`shared`** — jika ia memerlukan sesuatu di luar folder kumpulan anda *(ini menjadi keputusan jurulatih, bukan kerja anda)*

4. **Susun mengikut keutamaan** dalam lajur To Do. Tertib mesti mencerminkan blok trek:

   | Blok | Isu jenis apa |
   |------|---------------|
   | Hari 4 | Skema, entiti, skrin pertama |
   | Hari 5–6 | Borang, validation, peraturan perniagaan |
   | Hari 7–9 | Aliran kelulusan, skrin admin |
   | Hari 10–12 | Notifikasi, laporan, dashboard |
   | Hari 13–14 | Ujian, refactor |

5. **Semakan silang untuk isu `shared`.** Kerana setiap pasukan ada board sendiri, isu `shared` dibawa ke **semakan silang harian** (stand-up + [`../../AGENTS.md`](../../AGENTS.md)) supaya seluruh kelas nampak. Bagi setiap satu:
   - Adakah ia sudah wujud dalam daftar komponen kongsi ([`../../AGENTS.md`](../../AGENTS.md))?
   - Adakah lebih daripada satu kumpulan memerlukannya? → ia menjadi kerja Hari 3
   - Adakah ia sebenarnya khusus modul? → nyahlabel, simpan dalam kumpulan

6. Sahkan setiap kumpulan mempunyai **sekurang-kurangnya 12 isu** dalam To Do.

### ✅ Semakan

- [ ] Setiap pasukan ada board GitHub Project sendiri (4 lajur + swimlane mengikut epic)
- [ ] Setiap kumpulan ada ≥12 isu dari user story PRD / URSnya
- [ ] Setiap isu memaut ke **user story PRD (US-n)** dan **ID URS**
- [ ] Setiap isu ada kriteria penerimaan yang boleh diuji
- [ ] Isu dilabel mengikut kumpulan & jenis
- [ ] Isu `shared` disemak seluruh kelas
- [ ] To Do disusun mengikut blok trek

---

## Latihan 1 — Petakan backlog yang sama ke Jira

**Objektif:** Faham struktur Jira supaya anda boleh membawa aliran kerja ini ke pejabat.

> Jurulatih mendemokan ini pada projek Jira demo. Peserta mengikut secara membaca — kita **tidak** menjalankan kursus dari Jira.

### Langkah

> **Setiap pasukan ada board Jira sendiri** (bukan satu board dikongsi) — sama seperti board GitHub Project mereka. Dalam board itu, **swimlane mengikut epic**; Kumpulan 1 (3 projek) ada 3 epic.
>
> **Peta PRD → Jira:** setiap **user story PRD** (US-n) = satu **Story**; pecahan teknikal = **Subtask**; **acceptance criteria** PRD = kriteria penerimaan Story. Kunci projek Jira per sistem (cth `LD-`, `PKS-`, `CM-`, `FS-`) masuk ke mesej commit untuk memaut kod ↔ isu.

1. Perhatikan hierarki Jira pada skrin:

```text
Epic:  "Modul Pas, Parkir & Pelekat"           ← satu atau lebih epic setiap kumpulan (K1 ada 3)
 ├─ Story: "Sebagai pemohon, saya mahu memohon pelekat kenderaan"
 │   ├─ Subtask: "Tambah entiti Vehicle + IEntityTypeConfiguration"
 │   ├─ Subtask: "Bina borang permohonan pelekat"
 │   └─ Subtask: "Laksana semakan pendua plat"
 └─ Story: "Sebagai Pegawai Keselamatan, saya mahu menyemak permohonan pas"
 └─ Spike: "Siasat integrasi QR/AD — berapa lama & bagaimana?"
```

2. **Petakan tiga isu GitHub anda** kepada struktur Jira. Tulis dalam `docs/kumpulan-N/pemetaan-jira.md`:

```markdown
# Pemetaan GitHub Projects ↔ Jira — Kumpulan N

| Isu GitHub | Jenis Jira | Epic induk |
|------------|-----------|------------|
| #14 Semakan pendua nombor plat | Subtask | Story: Mohon pelekat kenderaan |
| #15 Borang permohonan pas | Story | Epic: Modul Pas, Parkir & Pelekat |
| #16 Betulkan ralat validation tarikh | Bug | Story: Mohon pas keselamatan |

## Perbezaan yang kami perasan
- Jira menguatkuasakan hierarki Epic→Story→Subtask; GitHub gunakan label secara longgar
- Jira memisahkan Sprint dan Version; GitHub gabungkan menjadi Milestone
- Rujukan commit: GitHub `Closes #14` (menutup isu) vs Jira `NRES-42` (memaut, tidak menutup)
```

3. Perhatikan demo memaut commit ke Jira: mesej commit `NRES-42 akses: tambah semakan pendua plat` muncul secara automatik pada isu Jira.

4. **Perbincangan:** untuk sistem dalaman NRES sebenar, alat mana yang anda cadangkan dan kenapa? Rekod pandangan kumpulan anda dalam fail pemetaan.

### ✅ Semakan

- [ ] `docs/kumpulan-N/pemetaan-jira.md` wujud
- [ ] Tiga isu dipetakan kepada jenis Jira dengan epic induk
- [ ] Kumpulan boleh menerangkan Epic vs Story vs Subtask
- [ ] Kumpulan boleh menerangkan `Closes #14` vs `NRES-42`

---

## Latihan 2 — Konfigurasi Git & commit pertama

**Objektif:** Clone repo, konfigurasi identiti anda, dan commit dokumentasi Hari 1.

### Langkah

1. Konfigurasi identiti anda (guna nama dan e-mel sebenar — ia muncul dalam setiap commit):

```bash
git config --global user.name "Nama Penuh Anda"
git config --global user.email "emel.anda@nres.gov.my"
git config --global pull.rebase true
```

> `pull.rebase true` menjadikan `git pull` berkelakuan seperti `git pull --rebase` secara lalai — tepat apa yang kursus ini mahu.

2. Clone repo kursus (jurulatih memberi URL):

```bash
git clone https://github.com/<org>/nres-onboarding.git
cd nres-onboarding
```

3. Lihat apa yang ada:

```bash
git status
git log --oneline --graph --all
git branch -a
```

4. Salin dokumen Hari 1 anda ke dalam repo:

```bash
mkdir -p docs/kumpulan-N        # ganti N dengan nombor kumpulan anda
cp ~/laluan/ke/docs/*.md docs/kumpulan-N/
```

5. Stage dan commit:

```bash
git status                                   # lihat apa yang belum dijejak
git add docs/kumpulan-N/
git status                                   # kini di-stage (hijau)
git commit -m "docs: tambah URS, use case dan ERD Kumpulan N"
```

6. Lihat commit anda:

```bash
git log --oneline -3
git show --stat HEAD
```

### ✅ Semakan

- [ ] `git config user.name` memaparkan nama anda
- [ ] `git log` menunjukkan commit anda dengan mesej yang bermakna
- [ ] `git status` menunjukkan working tree bersih
- [ ] Mesej commit anda mengikut format `<skop>: <penerangan BM>`

---

## Latihan 3 — Cabang & pull request pertama

**Objektif:** Bekerja dalam cabang ciri, buka PR, dan gabungkannya.

### Langkah

1. Cipta cabang ciri (guna nombor kumpulan anda):

```bash
git switch -c kump-N/docs/ringkasan-modul
```

> `git switch -c` mencipta dan bertukar dalam satu langkah. (`git checkout -b` melakukan perkara sama — anda akan melihat kedua-duanya dalam tutorial.)

2. Cipta `docs/kumpulan-N/RINGKASAN.md`:

```markdown
# Kumpulan N — <Nama Modul>

**Prefix:** <PREFIX>
**Peranan admin:** <peranan>
**Cabang:** `kump-N/<slug>`

## Dalam satu ayat
<ayat modul anda dari Hari 1>

## Ahli kumpulan
- <nama> — <fokus>
- <nama> — <fokus>

## Jadual yang kami miliki
- `<JadualAnda1>`
- `<JadualAnda2>`

## Folder yang kami miliki
- `Models/<Modul>/`
- `Controllers/<Modul>*`
- `Views/<Modul>/`
```

3. Commit dan push:

```bash
git add docs/kumpulan-N/RINGKASAN.md
git commit -m "docs: tambah ringkasan modul Kumpulan N"
git push -u origin kump-N/docs/ringkasan-modul
```

4. Buka PR di GitHub. Guna templat kursus:

```markdown
## Apa yang berubah
Menambah ringkasan modul untuk Kumpulan N — pemilikan jadual, folder, dan ahli.

## Isu berkaitan
—

## Cara uji
1. Buka docs/kumpulan-N/RINGKASAN.md
2. Sahkan nama jadual sepadan SPEC-KURSUS.md

## Senarai semak
- [x] Guna servis kongsi sedia ada (tiada duplikasi)
- [x] Hanya fail folder kumpulan saya disentuh
- [ ] Validation pelayan + authorization disemak — *tidak berkenaan (dokumen)*
- [ ] Migration ikut slot — *tidak berkenaan*
- [x] Kod jana-AI saya faham & boleh terangkan
```

5. **Minta kumpulan lain menyemaknya.** Penyemak menjawab empat soalan:
   - Adakah ini sudah wujud dalam repo?
   - Adakah ia menyentuh fail milik orang lain?
   - Adakah authorization & validation betul? *(t/b untuk dokumen)*
   - Boleh penulis terangkan setiap baris?

6. Selepas diluluskan, gabungkan PR di GitHub. Kemudian tempatan:

```bash
git switch master
git pull --rebase origin master
git branch -d kump-N/docs/ringkasan-modul     # buang cabang yang sudah digabung
```

### ✅ Semakan

- [ ] Cabang ciri dicipta dengan nama mengikut konvensyen
- [ ] PR dibuka dengan templat lengkap
- [ ] PR disemak oleh kumpulan **lain**
- [ ] PR digabung dan cabang tempatan dibersihkan
- [ ] `git log --oneline --graph` menunjukkan sejarah anda

---

## Latihan 4 — Cipta konflik gabungan dengan sengaja & selesaikannya

**Objektif:** Alami konflik gabungan dalam keadaan selamat, supaya ia terasa biasa apabila ia berlaku sebenar.

> **Jangan langkau ini.** Kali pertama anda melihat konflik tidak sepatutnya pada Hari 15 dengan empat kumpulan menunggu anda.

### Langkah

1. **Berpasangan.** Panggil diri anda **A** dan **B**. Kedua-dua bermula dari `master` yang terkini:

```bash
git switch master
git pull --rebase origin master
```

2. **A** mencipta fail yang akan dikongsi:

```bash
git switch -c latihan/konflik-demo
mkdir -p docs/latihan
printf '# Senarai Modul\n\n1. Lapor Diri\n2. Pas, Parkir & Pelekat\n' > docs/latihan/modul.md
git add docs/latihan/modul.md
git commit -m "docs: tambah senarai modul latihan"
git push -u origin latihan/konflik-demo
```

3. **B** mengambilnya dan mencipta cabang **sendiri** dari titik yang sama:

```bash
git fetch origin
git switch latihan/konflik-demo
git switch -c latihan/konflik-b
```

4. **Kedua-dua sekarang mengedit BARIS YANG SAMA.**

   **A** (pada `latihan/konflik-demo`) menukar baris 3:

```markdown
# Senarai Modul

1. Lapor Diri (Kumpulan 1) — prefix LD
2. Pas, Parkir & Pelekat
```

```bash
git add docs/latihan/modul.md
git commit -m "docs: tambah nombor kumpulan pada modul 1"
git push
```

   **B** (pada `latihan/konflik-b`) menukar baris **yang sama** secara berbeza:

```markdown
# Senarai Modul

1. Lapor Diri — dikendalikan HrAdmin
2. Pas, Parkir & Pelekat
```

```bash
git add docs/latihan/modul.md
git commit -m "docs: tambah peranan admin pada modul 1"
```

5. **B** cuba menyegerak — konflik berlaku:

```bash
git pull --rebase origin latihan/konflik-demo
```

Anda akan melihat:

```text
CONFLICT (content): Merge conflict in docs/latihan/modul.md
```

6. **Buka fail.** Anda akan lihat:

```text
<<<<<<< HEAD
1. Lapor Diri (Kumpulan 1) — prefix LD
=======
1. Lapor Diri — dikendalikan HrAdmin
>>>>>>> latihan/konflik-b
```

7. **Selesaikan.** Baris yang betul menyimpan **kedua-dua** maklumat berguna — konflik biasanya diselesaikan dengan bercakap dengan orang itu, bukan dengan memilih pemenang:

```markdown
# Senarai Modul

1. Lapor Diri (Kumpulan 1) — prefix LD, dikendalikan HrAdmin
2. Pas, Parkir & Pelekat
```

Buang **ketiga-tiga** penanda (`<<<<<<<`, `=======`, `>>>>>>>`).

8. Selesaikan rebase:

```bash
git add docs/latihan/modul.md
git rebase --continue
git log --oneline --graph
```

9. **Tukar peranan dan ulang.** Setiap peserta mesti menyelesaikan sekurang-kurangnya satu konflik sendiri.

10. Bersihkan:

```bash
git switch master
git branch -D latihan/konflik-demo latihan/konflik-b
git push origin --delete latihan/konflik-demo
```

### Jika anda tersasar

```bash
git rebase --abort      # batalkan, kembali ke keadaan sebelum rebase
```

Ini selamat. Anda tidak boleh merosakkan apa-apa yang tidak boleh dibatalkan.

### ✅ Semakan

- [ ] Anda mencipta konflik sebenar
- [ ] Anda melihat penanda `<<<<<<<` / `=======` / `>>>>>>>`
- [ ] Anda menyelesaikannya dengan menyimpan kedua-dua maklumat berguna
- [ ] Ketiga-tiga penanda dibuang
- [ ] `git rebase --continue` berjaya
- [ ] **Setiap** ahli pasangan menyelesaikan satu konflik
- [ ] Anda tahu `git rebase --abort` wujud

---

## Latihan 5 — Cipta cabang kumpulan

**Objektif:** Tetapkan cabang jangka panjang yang kumpulan anda akan guna dari Hari 4 hingga 14.

### Langkah

1. Setiap kumpulan melantik **seorang** untuk mencipta cabang (elakkan cabang pendua):

```bash
git switch master
git pull --rebase origin master
```

2. Cipta cabang kumpulan anda — guna nama **tepat** dari [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md):

| Kumpulan | Arahan |
|----------|--------|
| 1 | `git switch -c kump-1/lapor-diri` |
| 2 | `git switch -c kump-2/akses-kenderaan` |
| 3 | `git switch -c kump-3/id-ad-email` |
| 4 | `git switch -c kump-4/perisian-aset` |

3. Push dan tetapkan penjejakan:

```bash
git push -u origin kump-N/<slug>
```

4. **Setiap** ahli kumpulan mengambilnya:

```bash
git fetch origin
git switch kump-N/<slug>
git branch -vv          # sahkan ia menjejak origin
```

5. Sahkan keempat-empat cabang wujud:

```bash
git branch -r
# origin/master
# origin/kump-1/lapor-diri
# origin/kump-2/akses-kenderaan
# origin/kump-3/id-ad-email
# origin/kump-4/perisian-aset
```

### ✅ Semakan

- [ ] Keempat-empat cabang kumpulan wujud pada `origin`
- [ ] Nama cabang sepadan `SPEC-KURSUS.md` **tepat**
- [ ] Setiap peserta berada pada cabang kumpulannya
- [ ] `git branch -vv` menunjukkan penjejakan dikonfigurasi

---

## Latihan 6 — Konfigurasi AI dengan konteks kongsi

**Objektif:** Sediakan pembantu AI anda dengan `AGENTS.md`, dan buktikan sendiri kenapa ia penting.

### Langkah

1. **Baca [`../../AGENTS.md`](../../AGENTS.md) sepenuhnya.** Beri perhatian khusus kepada:
   - Peraturan mutlak (10 perkara)
   - Daftar komponen kongsi
   - Peta pemilikan folder

2. **Halakan AI anda kepadanya.** Cara bergantung alat:

   | Alat | Cara |
   |------|------|
   | Claude Code | Ia membaca `AGENTS.md` secara automatik dari root repo |
   | Copilot Chat | `#file:AGENTS.md` dalam prompt anda |
   | Cursor | Tambah `AGENTS.md` ke konteks projek |
   | Sembang web | Tampal kandungan `AGENTS.md` pada awal sesi |

3. **Jalankan eksperimen ini** — ia adalah intipati latihan.

   **Prompt A — tanpa konteks** (sesi AI baharu, jangan sebut `AGENTS.md`):

```text
Tulis servis C# untuk ASP.NET Core yang menjana nombor rujukan permohonan
unik seperti LD-2026-0001.
```

   **Prompt B — dengan konteks** (sesi baharu, `AGENTS.md` dimuatkan):

```text
Merujuk AGENTS.md: saya Kumpulan N. Adakah repo ini sudah ada cara untuk
menjana nombor rujukan permohonan? Jika ya, beritahu di mana dan bagaimana
saya patut gunakannya. Jangan tulis kod baharu.
```

4. **Bandingkan kedua-dua respons.** Rekod dalam `docs/kumpulan-N/nota-ai.md`:

```markdown
# Nota penggunaan AI — Kumpulan N

## Eksperimen: konteks kongsi

**Prompt A (tanpa konteks)** menghasilkan: <ringkaskan — biasanya servis baharu penuh>
**Prompt B (dengan AGENTS.md)** menghasilkan: <ringkaskan — biasanya menunjuk ke IReferenceNumberService>

**Apa yang akan berlaku jika keempat-empat kumpulan menggunakan Prompt A:**
<jawapan anda>

## Peraturan kami
1. Setiap sesi AI bermula dengan AGENTS.md
2. Prompt pertama sentiasa "adakah ini sudah wujud?"
3. Sekat AI kepada folder kami secara eksplisit
4. Tiada commit tanpa faham — terangkan kepada rakan dahulu
```

5. **Latih prompt terikat-skop.** Tulis satu untuk modul anda dan uji:

```text
Merujuk AGENTS.md dan SPEC-KURSUS.md: saya Kumpulan N mengerjakan modul <nama>.
Tulis HANYA fail di bawah <folder anda>. Jangan ubah Program.cs,
ApplicationDbContext.cs, _Layout.cshtml, atau apa-apa dalam Models/Shared/.
Guna servis kongsi sedia ada (IReferenceNumberService, IAuditLogService,
IWorkflowService). Sebelum menulis, sahkan sama ada ini sudah wujud.

Tugasan: <perihalkan satu ciri kecil dari backlog anda>
```

6. **Latih prompt semakan.** Ini yang anda jalankan sebelum setiap PR:

```text
Semak diff ini terhadap AGENTS.md dan KOLABORASI.md:
1. Adakah ia menduplikasi apa-apa dalam daftar komponen kongsi?
2. Adakah ia menyentuh fail di luar folder Kumpulan N?
3. Adakah authorization dan validation pelayan lengkap?
Senaraikan masalah. JANGAN tulis semula kod.
```

Simpan kedua-dua prompt dalam `docs/kumpulan-N/nota-ai.md` — anda akan menggunakannya setiap hari.

### ✅ Semakan

- [ ] Setiap ahli kumpulan telah membaca `AGENTS.md`
- [ ] AI dikonfigurasi dengan `AGENTS.md` dalam konteks
- [ ] Eksperimen Prompt A vs B dijalankan dan **perbezaan diperhatikan**
- [ ] Kumpulan boleh menyatakan apa yang berlaku jika keempat-empat kumpulan mengabaikan konteks kongsi
- [ ] Prompt terikat-skop dan prompt semakan disimpan dalam `nota-ai.md`

---

## Latihan 6b — Sambung Claude Code ke Jira (MCP)

> 🔧 **Khusus Claude Code** (MCP & skills). Pilihan/peningkatan — kursus tidak dijalankan dari Jira, tetapi **MCP** membolehkan AI membaca & mencipta isu terus dari **user story PRD** anda tanpa menyalin manual.

**Objektif:** Sambungkan Claude Code ke Jira melalui MCP supaya anda boleh senarai & cipta isu dari terminal.

### Langkah

1. Tambah pelayan MCP Atlassian (Jira/Confluence):

```bash
claude mcp add --transport http atlassian https://mcp.atlassian.com/v1/mcp
```

2. **Autentikasi.** Dalam Claude Code, buka panel `/mcp` → pilih `atlassian` → ikut aliran OAuth dalam pelayar. Log masuk **akaun Atlassian projek anda** (guna tetingkap **incognito** jika ada beberapa akaun supaya SSO tidak guna akaun salah).

   > **Dalam VS Code:** jalankan arahan `claude mcp add …` (langkah 1) di **terminal bersepadu** (`` Cmd/Ctrl+` ``), kemudian taip `/mcp` dalam **panel chat** untuk authenticate. Konfigurasi dikongsi dengan CLI — tambah sekali, guna di kedua-dua. Panduan pelajar penuh: [`docs/cara-sambung-jira-claude-code.md`](../../docs/cara-sambung-jira-claude-code.md).

3. Sahkan sambungan — dalam Claude Code:

```text
Senaraikan projek Jira yang saya boleh akses.
```

4. **Cipta isu dari user story PRD anda** (guna kunci projek per sistem, cth `LD`, `PKS`, `CM`, `FS`):

```text
Dalam projek Jira <KEY>, cipta satu Task untuk user story US-1 PRD kami:
tajuk ringkas + perihalan, dan salin acceptance criteria PRD sebagai kriteria penerimaan.
Tunjukkan draf dahulu sebelum mencipta.
```

5. **(Pilihan) Kongsi dengan pasukan.** Skop pelayan ke repo ini supaya rakan sepasukan dapat konfigurasi sama:

```bash
claude mcp add --transport http atlassian --scope project https://mcp.atlassian.com/v1/mcp
# Menghasilkan .mcp.json di root repo — commit supaya pasukan kongsi konfigurasi.
```

> `.mcp.json` hanya kongsi **konfigurasi**, bukan token. Setiap ahli tetap **autentikasi sendiri** melalui `/mcp` dengan akaun mereka.

### ✅ Semakan

- [ ] Pelayan MCP `atlassian` ditambah & `/mcp` tunjuk **connected**
- [ ] Boleh senaraikan projek Jira dari Claude Code
- [ ] Sekurang-kurangnya satu isu dicipta dari user story PRD (draf disemak dahulu)

---

## Latihan 6c — Cipta skill projek (`SKILL.md`)

> 🔧 **Khusus Claude Code.** Skill = arahan berulang yang dibungkus jadi perintah `/nama` yang boleh dikongsi seluruh pasukan.

**Objektif:** Bungkus "jana dokumentasi + diagram ikut konvensyen kami" sebagai satu skill yang dipanggil `/dok-modul`.

### Langkah

1. Cipta fail `.claude/skills/dok-modul/SKILL.md` (skill ialah **folder** dengan fail `SKILL.md`, bukan satu fail longgar):

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

2. Dalam Claude Code, panggil skill:

```text
/dok-modul
```

3. **Semak output** — betulkan fakta; pastikan diagram merender; buang apa-apa yang direka di luar PRD.

4. **(Pilihan)** Commit `.claude/skills/dok-modul/` supaya seluruh pasukan guna skill yang sama.

### ✅ Semakan

- [ ] `.claude/skills/dok-modul/SKILL.md` wujud dengan frontmatter `name` + `description`
- [ ] `/dok-modul` menghasilkan dokumentasi + diagram Mermaid
- [ ] Output disemak manusia (fakta betul, tiada rekaan)

---

## Latihan 6d — Prompt dokumentasi & diagram Mermaid (terus)

**Objektif:** Hasilkan dokumentasi modul & diagram Mermaid dari PRD/URS/ERD guna **prompt terus** (tanpa skill) — supaya anda faham apa yang skill di atas lakukan.

### Langkah

1. **Dokumentasi modul.** Lampirkan PRD anda, kemudian:

```text
Berdasarkan PRD modul kami di bawah, tulis dokumentasi ringkas (docs/README-modul-N.md):
- gambaran modul & pengguna
- senarai fungsi utama
- aliran permohonan (langkah demi langkah)
Guna Bahasa Melayu, ringkas dan jelas. Jangan tambah ciri yang tiada dalam PRD.

[tampal PRD di sini]
```

2. **Diagram ERD (Mermaid).** Lampirkan ERD anda:

```text
Berdasarkan ERD kami di bawah, beri kod Mermaid `erDiagram` untuk entiti utama & hubungan.
Kod Mermaid sahaja supaya saya boleh tampal terus. Jangan reka entiti baharu.

[tampal ERD di sini]
```

3. **Carta alir proses (Mermaid).**

```text
Berdasarkan use case/PRD kami, beri kod Mermaid `flowchart`:
Mohon → semak (bertindih / pendua / kelengkapan) → kelulusan admin → audit.
Kod Mermaid sahaja. Ikut peranan & status dalam PRD.
```

4. Simpan hasil dalam `docs/` dan sahkan diagram **merender** (VS Code atau GitHub).

5. **Semak silang:** adakah dokumentasi/diagram menokok keperluan atau entiti yang tiada dalam PRD/ERD? Betulkan dengan tangan.

### ✅ Semakan

- [ ] `docs/README-modul-N.md` dijana & disemak
- [ ] Diagram **ERD** + **carta alir** (Mermaid) dihasilkan & merender
- [ ] Tiada entiti/keperluan direka di luar PRD/ERD
- [ ] Fail Mermaid disimpan dalam repo (boleh review seperti kod)

---

## Latihan 7 — Persekitaran .NET 10

**Objektif:** Setiap mesin sedia untuk menulis kod esok.

### Langkah

1. Sahkan .NET 10 SDK:

```bash
dotnet --version        # mesti 10.x
dotnet --info
```

Jika belum: [dotnet.microsoft.com/download](https://dotnet.microsoft.com/download) · langkah penuh [`../../nota/00-setup-dotnet.md`](../../nota/00-setup-dotnet.md).

2. Pasang alat EF Core:

```bash
dotnet tool install --global dotnet-ef
dotnet ef --version
```

Jika sudah dipasang: `dotnet tool update --global dotnet-ef`

3. Sahkan IDE anda membina projek .NET:

```bash
cd /tmp
dotnet new console -o ujian-persekitaran
cd ujian-persekitaran
dotnet run              # sepatutnya cetak "Hello, World!"
cd .. && rm -rf ujian-persekitaran
```

4. Sahkan akses Git ke repo:

```bash
cd <repo-kursus>
git fetch origin
git branch -r           # sepatutnya senaraikan keempat-empat cabang kumpulan
```

5. **Semakan silang berpasangan.** Anda **tidak** siap sehingga rakan anda memerhati anda menjalankan `dotnet --version` dan `dotnet ef --version` dengan jayanya. Persekitaran yang rosak yang ditemui esok pagi membuang masa seisi kumpulan.

### ✅ Semakan

- [ ] `dotnet --version` → `10.x`
- [ ] `dotnet ef --version` berjaya
- [ ] `dotnet new console` + `dotnet run` berjaya
- [ ] `git fetch` berfungsi dan keempat-empat cabang kelihatan
- [ ] Rakan sekumpulan telah **menyaksikan** semakan anda

---

## Latihan 8 — Tandatangan kontrak kolaborasi

**Objektif:** Setiap kumpulan menyatakan secara eksplisit apa yang ia bersetuju — supaya "saya tidak tahu" bukan alasan pada Hari 9.

### Langkah

1. Baca [`../../KOLABORASI.md`](../../KOLABORASI.md) sepenuhnya **sebagai kumpulan**. Baca dengan kuat bergilir-gilir bahagian.

2. Cipta `docs/kumpulan-N/kontrak.md`:

```markdown
# Kontrak kolaborasi — Kumpulan N

Kami telah membaca KOLABORASI.md dan AGENTS.md, dan bersetuju:

## Pemilikan fail
Kami hanya mencipta fail dalam:
- `Models/<Modul>/`
- `Controllers/<Modul>*`
- `Views/<Modul>/`
- `ViewModels/<Modul>/`
- `Services/<Modul>/`

Kami TIDAK akan menyunting: Program.cs · ApplicationDbContext.cs ·
_Layout.cshtml · site.css · Models/Shared/

## Migration
Kami akan mengumumkan sebelum mengambil slot migration, dan menjana semula
(bukan membaiki dengan tangan) jika snapshot berkonflik.

## Komponen kongsi
Sebelum menulis mana-mana helper, kami akan mencari repo dahulu dan bertanya
kepada AI sama ada ia sudah wujud. Jika lebih daripada satu modul perlukannya,
kami membuka isu `shared` dan bukan membinanya sendiri.

## AI
Setiap sesi AI bermula dengan AGENTS.md. Tiada kod dijana-AI di-commit
sehingga penulisnya boleh menerangkannya kepada rakan sekumpulan.

## Rentak harian
Stand-up 9.00 · git pull --rebase · semakan silang AI 9.15 ·
code review + push 4.30

## Definition of Done
Kami menerima DoD dalam KOLABORASI.md §9 tanpa pengubahsuaian.

**Ahli kumpulan:**
- <nama>
- <nama>

**Tarikh:** <tarikh>
```

3. Commit melalui PR (amalkan aliran kerja yang baru anda pelajari):

```bash
git switch kump-N/<slug>
git pull --rebase origin master
git switch -c kump-N/docs/kontrak
git add docs/kumpulan-N/kontrak.md
git commit -m "docs: kontrak kolaborasi Kumpulan N"
git push -u origin kump-N/docs/kontrak
```

Buka PR → minta kumpulan lain menyemak → gabung.

4. **Semakan seluruh kelas.** Jurulatih memaparkan keempat-empat kontrak sebelah-menyebelah dan mengesahkan tiada folder bertindih dan tiada dua kumpulan menuntut fail yang sama.

### ✅ Semakan

- [ ] Kumpulan membaca `KOLABORASI.md` sepenuhnya bersama-sama
- [ ] `docs/kumpulan-N/kontrak.md` wujud dan lengkap
- [ ] Digabung melalui PR (bukan push terus)
- [ ] Semakan seluruh kelas menemui **sifar** folder bertindih
- [ ] Setiap peserta boleh menyebut folder mana miliknya tanpa melihat

---

## Deliverable Hari 2

| Artifak | Lokasi |
|---------|--------|
| Dokumen Hari 1 di-commit | `docs/kumpulan-N/` |
| Ringkasan modul | `docs/kumpulan-N/RINGKASAN.md` |
| Pemetaan Jira | `docs/kumpulan-N/pemetaan-jira.md` |
| Nota & prompt AI | `docs/kumpulan-N/nota-ai.md` |
| Kontrak kolaborasi | `docs/kumpulan-N/kontrak.md` |
| 4 cabang kumpulan | `origin/kump-1/…` … `origin/kump-4/…` |
| Backlog | GitHub Projects, ≥12 isu setiap kumpulan |
| Persekitaran | .NET 10 SDK + `dotnet ef` pada setiap mesin |

## Sebelum esok

Esok kita menaip C#. Sahkan `dotnet --version` menunjukkan `10.x` **malam ini** — bukan pada pukul 9 pagi esok.
