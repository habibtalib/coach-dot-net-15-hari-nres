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

1. **Konfigurasi identiti Git.** Jika belum, ikut [`docs/persediaan-git.md`](../../docs/persediaan-git.md) — pasang Git + set `user.name`/`user.email` + `pull.rebase true`. Sahkan:

```bash
git config user.name && git config user.email
```

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

> Guna prompt **SMK-01** (Semakan pra-PR) — [`docs/pustaka-prompt.md`](../../docs/pustaka-prompt.md).

Simpan kedua-dua prompt dalam `docs/kumpulan-N/nota-ai.md` — anda akan menggunakannya setiap hari.

### ✅ Semakan

- [ ] Setiap ahli kumpulan telah membaca `AGENTS.md`
- [ ] AI dikonfigurasi dengan `AGENTS.md` dalam konteks
- [ ] Eksperimen Prompt A vs B dijalankan dan **perbezaan diperhatikan**
- [ ] Kumpulan boleh menyatakan apa yang berlaku jika keempat-empat kumpulan mengabaikan konteks kongsi
- [ ] Prompt terikat-skop dan prompt semakan disimpan dalam `nota-ai.md`

---

## Latihan 6b — Perkakas Claude Code: Jira MCP, skill & dokumentasi

> 🔧 **Khusus Claude Code.** Langkah **terperinci** ada dalam **`docs/`** — di sini anda **lakukan** & **sahkan**. (Guna alat AI lain? Langkau bahagian MCP/skill.)

**Objektif:** Sediakan sambungan Jira, satu skill dokumentasi, dan jana dokumentasi + diagram — ikut panduan `docs/`.

### Langkah

1. **Sambung Jira (MCP).** Ikut [`docs/cara-sambung-jira-claude-code.md`](../../docs/cara-sambung-jira-claude-code.md) (CLI **atau** VS Code). Kemudian cipta **satu isu** dari user story **US-1** PRD anda (projek `<KEY>`; draf disemak dahulu).

2. **Cipta skill `/dok-modul`.** Ikut [`docs/cara-jana-dokumentasi-diagram.md`](../../docs/cara-jana-dokumentasi-diagram.md) **Bahagian A** — cipta `.claude/skills/dok-modul/SKILL.md`, kemudian panggil `/dok-modul`.

3. **Jana dokumentasi + diagram.** Ikut panduan sama **Bahagian B** — hasilkan `docs/README-modul-N.md` + diagram Mermaid (ERD + carta alir) dari PRD/ERD anda; sahkan ia **merender**.

> **Semak manusia:** betulkan fakta; buang apa-apa yang direka di luar PRD/ERD. **Tiada commit tanpa faham.**

### ✅ Semakan

- [ ] Jira tersambung (`/mcp` **connected**) & satu isu dicipta dari US-1 PRD
- [ ] `.claude/skills/dok-modul/SKILL.md` wujud & `/dok-modul` berjalan
- [ ] `docs/README-modul-N.md` + diagram Mermaid dijana, disemak & merender
- [ ] Tiada keperluan/entiti direka di luar PRD/ERD

---

## Latihan 7 — Persekitaran .NET 10

**Objektif:** Setiap mesin sedia untuk menulis kod esok.

> **Panduan pasang penuh** (SDK · EF Core · IDE): [`docs/persediaan-dotnet.md`](../../docs/persediaan-dotnet.md) · Git & identiti: [`docs/persediaan-git.md`](../../docs/persediaan-git.md). Di sini kita **sahkan** sahaja.

### Langkah (sahkan)

```bash
dotnet --version                     # mesti 10.x
dotnet ef --version                  # alat EF Core
git fetch origin && git branch -r    # akses repo + cabang kumpulan
```

**Semakan silang berpasangan.** Anda **tidak** siap sehingga rakan anda menyaksikan `dotnet --version` dan `dotnet ef --version` berjaya. Persekitaran rosak yang ditemui esok pagi membuang masa seisi kumpulan.

### ✅ Semakan

- [ ] `dotnet --version` → `10.x`
- [ ] `dotnet ef --version` berjaya
- [ ] `dotnet new console` + `dotnet run` berjaya (lihat panduan `docs/`)
- [ ] `git fetch` berfungsi dan cabang kumpulan kelihatan
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
