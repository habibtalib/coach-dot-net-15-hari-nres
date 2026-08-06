# KOLABORASI.md — Kontrak Kerja Pasukan (poly-repo)

> Mengikat **semua empat kumpulan** sepanjang Hari 4–14. Kanun teknikal: [`SPEC-KURSUS.md`](./SPEC-KURSUS.md). Konteks AI kongsi: [`AGENTS.md`](./AGENTS.md).
>
> Dokumen ini menjawab satu soalan: **bagaimana 4 pasukan membina 6 sistem berasingan (repo/subdomain/DB sendiri), semuanya dibantu AI, dan tetap boleh diintegrasikan bersih melalui Profile DB?**

---

## 1. Kenapa dokumen ini wujud

Seni bina **poly-repo** menghapuskan konflik gabungan merentas pasukan (setiap sistem repo sendiri). Tetapi ia memperkenalkan **dua mod kegagalan baharu** yang boleh diramal:

**Kegagalan A — kontrak Profile DB terpesong.** `UserProfile` ialah **satu-satunya** perkara yang dikongsi. Jika satu sistem menganggap medan berbeza, atau Lapor Diri menulis bentuk yang sistem lain tidak jangka, **integrasi pecah** — dan ini hanya ditemui semasa SIT Hari 15.

**Kegagalan B — konvensyen berselerak.** Enam sistem, enam gaya: `SubmissionStatus` berbeza, corak aliran kerja berbeza, nama servis berbeza. Setiap satu berfungsi sendiri, tetapi bersama-sama ia mimpi ngeri penyelenggaraan — tepat apa yang berlaku pada sistem kerajaan yang tumbuh tanpa piawai.

**AI memburukkan B.** Pembantu AI menghasilkan gaya berbeza setiap kali melainkan diberi konteks kongsi yang **sama** dalam setiap repo.

Penyelesaiannya **bukan** "jangan guna AI" dan **bukan** "berhati-hati". Ia **kontrak Profile DB yang jelas + konvensyen kongsi merentas repo**. Itulah dokumen ini.

---

## 2. Pemilikan repo

Setiap sistem = **satu repo** dalam org [`nres-bpm`](https://github.com/nres-bpm). Anda bekerja dalam repo sistem anda; anda **tidak** menyunting repo pasukan lain — anda **integrasi melalui Profile DB** atau buka isu di repo mereka.

| Repo (`nres-bpm/…`) | Pemilik | Akses | Prefix |
|---------------------|---------|-------|--------|
| [`profile`](https://github.com/nres-bpm/profile) | **Jurulatih + semua** (kontrak dipersetujui bersama) | Profile DB kongsi | — |
| [`lapor-diri`](https://github.com/nres-bpm/lapor-diri) | Kumpulan 1 | **Awam** — cipta profil | `LD` |
| [`pematuhan-pks`](https://github.com/nres-bpm/pematuhan-pks) | Kumpulan 1 | Dalaman (SSO) | `PKS` |
| [`pengurusan-kontrak`](https://github.com/nres-bpm/pengurusan-kontrak) | Kumpulan 1 | Dalaman (SSO) | `KON` |
| [`pas-parkir-pelekat`](https://github.com/nres-bpm/pas-parkir-pelekat) | Kumpulan 2 | Dalaman (SSO) | `PAS` `PKR` `STK` |
| [`id-ad-email`](https://github.com/nres-bpm/id-ad-email) | Kumpulan 3 | Dalaman (SSO) | `ICT-ID` |
| [`tempahan-fasiliti-sukan`](https://github.com/nres-bpm/tempahan-fasiliti-sukan) | Kumpulan 4 | Dalaman (SSO) | `TFS` |

> **Dalam repo anda, anda bebas.** Tiada lagi "fail beku" atau "folder milik orang lain" — anda memiliki keseluruhan repo sistem anda (`Program.cs`, `DbContext`, layout, migration, semuanya). Satu-satunya sempadan ialah **kontrak Profile DB** (§5).

---

## 3. Apa yang benar-benar dikongsi: hanya Profile DB

Dalam poly-repo, **satu sahaja** perkara dikongsi merentas semua sistem:

- **Profile DB** — jadual `UserProfile` berpusat, dalam repo [`profile`](https://github.com/nres-bpm/profile), disediakan sebagai **skema + pustaka/paket klien** yang dirujuk setiap sistem.
- **Hanya Lapor Diri menulis** profil (mencipta ketika staf baharu melapor diri). **Semua sistem dalaman membaca** profil (melalui SSO / klien Profile DB).

Semua yang lain — `Submission`, `Attachment`, `AuditLog`, `ApprovalStep`, servis, controller, view, layout, CSS — **dibina dalam setiap repo sendiri**. Menduplikasinya **antara** repo adalah normal dan dijangka. Menduplikasinya **dalam** repo yang sama masih satu kegagalan (lihat §6).

---

## 4. Bila anda perlu sesuatu daripada sistem lain

Jangan salin kod mereka. Jangan sunting repo mereka. Ada dua cara sah:

1. **Data pengguna** → guna **Profile DB** (klien `profile`). Jangan simpan salinan profil dalam DB anda.
2. **Data/fungsi sistem lain** → integrasi melalui **API/kontrak** yang mereka dedahkan (atau minta mereka dedahkan). Buka isu di **repo mereka**, bukan salin kelas mereka ke repo anda.

> **Jika anda memerlukan medan baharu dalam `UserProfile`** — itu perubahan **kontrak Profile DB**: ikut §5. Ia menyentuh semua sistem, jadi tidak boleh dibuat sendiri.

---

## 5. Protokol perubahan kontrak Profile DB (menggantikan slot migration)

Kerana Profile DB dikongsi, mengubah skemanya menjejaskan **semua** sistem yang membacanya. (Migration dalam DB **sistem anda sendiri** adalah bebas — jana bila-bila, tiada slot.)

**Protokol untuk repo `profile`:**

1. Buka isu / PR dalam repo [`profile`](https://github.com/nres-bpm/profile): apa yang berubah & kenapa.
2. **Selaras dengan sistem yang bergantung** — perubahan mesti serasi ke belakang (backward compatible) jika boleh (tambah medan nullable, bukan buang/namakan semula).
3. **Versikan** kontrak (SemVer paket profil). Sistem naik taraf bila sedia.
4. Merge PR → terbitkan versi baharu → sistem lain kemas kini rujukan bila perlu.

> **Jangan sekali-kali** ubah bentuk `UserProfile` secara senyap dalam satu sistem sahaja. Itu Kegagalan A, dan ia hanya kelihatan semasa SIT Hari 15.

---

## 6. Set komponen piawai — setiap repo bina sendiri, guna nama sama

Ini **bukan** dikongsi (setiap repo ada versinya), tetapi **guna nama & bentuk yang sama** supaya enam sistem konsisten. Menulis **dua** versi dalam **repo yang sama** ialah kegagalan code review.

### Servis (setiap repo)

| Servis | Fungsi | Guna bila |
|--------|--------|-----------|
| `IReferenceNumberService` | Jana `LD-2026-0001` dsb. mengikut prefix modul | Bila permohonan dihantar |
| `IFileStorageService` | Simpan/dapat fail selamat di `App_Data/uploads/{submissionId}/` | Sebarang muat naik |
| `IAuditLogService` | Catat tindakan ke `AuditLogs` (DB sendiri) | Setiap perubahan status |
| `IWorkflowService` | Sahkan & laksana peralihan `SubmissionStatus` | Setiap approve/reject/submit |
| `INotificationService` | Hantar notifikasi (latihan: `ConsoleNotificationService`) | Selepas peralihan status |
| `ICurrentUserService` | Pengguna semasa, peranan, jabatan (dari SSO / Profile DB) | Di mana-mana perlu identiti |
| `IProfileService` *(dari `profile`)* | **Baca** profil; **Lapor Diri** juga **tulis** | Bila perlu data pengguna |

### Partial view & komponen (setiap repo)

`_StatusBadge` · `_AuditTrail` · `_AttachmentList` · `_ApprovalPanel` · `_FilterBar` · `_ValidationSummary`

### Kelas asas (setiap repo)

`SubmissionControllerBase` — menyediakan `Approve`, `Reject`, `SubmitForReview`, dan penulisan audit yang **sudah betul**. Controller modul mewarisinya dan **tidak** menulis semula logik kelulusan.

> **Semakan diri sebelum menulis apa-apa helper (dalam repo anda):**
> 1. `grep -ri "<nama konsep>" src/` — sudah wujud dalam repo ini?
> 2. Semak jadual di atas (guna nama piawai).
> 3. Untuk apa-apa berkaitan **profil**, guna klien `profile` — jangan cipta skema baharu.

---

## 7. Guna AI secara berpasukan (bahagian yang paling mudah tersasar)

Enam sistem, semua dibantu AI. Tanpa konteks kongsi yang **sama dalam setiap repo**, enam pembantu AI menghasilkan enam gaya berbeza.

**Peraturan mengikat:**

1. **Setiap repo menyertakan `AGENTS.md`.** Halakan pembantu AI anda ke fail itu sebelum meminta kod. Semua repo guna konteks yang **sama** — itulah yang menyeragamkan output merentas sistem.
2. **Cari dahulu, jana kemudian.** Prompt pertama: *"Adakah repo ini sudah ada `<X>`?"* — bukan *"Tulis `<X>`."*
3. **Lindungi kontrak profil.** Nyatakan dalam prompt: *"Jangan cipta skema profil baharu — guna klien Profile DB dari `profile`."*
4. **Tiada commit tanpa faham.** Sebelum commit kod jana-AI, penulis mesti **menerangkannya kepada seorang rakan sekumpulan**. Diperiksa semasa code review.
5. **AI tidak mereka keperluan.** URS & peraturan perniagaan datang daripada dokumen NRES dan `SPEC-KURSUS.md`.
6. **Tiada data NRES sebenar dalam prompt.** Semua contoh sintetik.
7. **AI sebagai penyemak, bukan hanya penjana.** Sebelum PR: *"Semak diff ini terhadap AGENTS.md. Adakah ia menduplikasi apa-apa dalam repo ini? Adakah ia menyalin/mengubah skema profil?"*

**Semakan silang AI harian (10 minit, selepas stand-up).** Setiap kumpulan menunjukkan satu perkara ketara yang dijana AI semalam. Soalan: *"Adakah kita masih konsisten (nama, corak, kontrak profil)?"* — ini menyeragamkan enam sistem sepanjang jalan, bukan pada Hari 15.

---

## 8. Rentak harian pasukan (Hari 4–14)

| Masa | Aktiviti |
|------|----------|
| 9.00 – 9.15 | **Stand-up per kumpulan** — semalam / hari ini / halangan. `git pull --rebase` **dalam repo anda**. |
| 9.15 – 9.25 | **Semakan silang** — konsistensi konvensyen & kontrak profil antara sistem (§7). |
| 9.25 – 1.00 | Sesi pembangunan (commit kecil & kerap) |
| 2.30 – 4.30 | Sesi pembangunan |
| 4.30 – 5.00 | **Code review berpasangan** + PR ke `main` repo + push + kemas kini board |

---

## 9. Definition of Done (satu untuk semua kumpulan)

Satu tugasan **selesai** hanya apabila **kesemua** ini benar:

- [ ] Kod berjalan — `dotnet build` bersih, aplikasi bermula, ciri berfungsi manual.
- [ ] Guna set komponen piawai (nama sama) — tiada logik didup **dalam repo ini**.
- [ ] Data pengguna melalui **Profile DB** (klien `profile`) — tiada salinan skema profil.
- [ ] Validation di **pelayan** (server-side), bukan pelayar sahaja.
- [ ] `[Authorize(Roles = ...)]` betul pada setiap action yang perlu (peranan dari profil/SSO).
- [ ] Perubahan status melalui `IWorkflowService`; tindakan dicatat melalui `IAuditLogService`.
- [ ] Perubahan kontrak Profile DB (jika ada) ikut §5 (PR + persetujuan dalam repo `profile`).
- [ ] Kod jana-AI difahami dan boleh diterangkan oleh penulisnya.
- [ ] PR ada perihalan Bahasa Melayu + langkah cara menguji.
- [ ] Disemak dan diluluskan oleh seorang rakan sekumpulan.
- [ ] Isu board dipindah ke **Done**.

---

## 10. Aliran PR & code review (per repo)

Setiap repo menguruskan PR-nya sendiri — **tiada** merge merentas repo:

```text
feat/semakan-pendua-plat  →  PR  →  main   (dalam repo pas-parkir-pelekat)
```

**Templat PR** (semua repo guna yang sama):

```markdown
## Apa yang berubah
<2–3 baris Bahasa Melayu>

## Isu berkaitan
Closes #<nombor>            <!-- atau: NRES-42 -->

## Cara uji
1. …
2. …

## Senarai semak
- [ ] Guna komponen piawai (nama sama; tiada duplikasi dalam repo ini)
- [ ] Data pengguna via Profile DB (tiada salinan skema profil)
- [ ] Validation pelayan + authorization disemak
- [ ] Perubahan kontrak profil (jika ada) ikut §5
- [ ] Kod jana-AI saya faham & boleh terangkan
```

**Senarai semak penyemak** — penyemak menjawab empat soalan ini, mengikut turutan:

1. **Adakah ini sudah wujud dalam repo ini?** (anti-redundan dalam repo)
2. **Adakah ia menyalin/mengubah skema Profile DB?** (patut guna kontrak `profile`)
3. Adakah authorization & validation betul?
4. Bolehkah penulis menerangkan setiap baris?

Review adalah **wajib** dan mengambil kira 15% penilaian capstone.

---

## 11. Persediaan Hari 15 bermula pada Hari 4

Integrasi bukan aktiviti Hari 15 — ia disiapkan sedikit demi sedikit:

- **Setiap hari:** `git pull --rebase` **dalam repo anda** (kekal segerak dengan rakan sekumpulan pada repo yang sama).
- **Uji integrasi awal:** segera setelah Profile DB sedia (Hari 3), sahkan sistem anda boleh **baca** profil (dan Lapor Diri boleh **cipta**). Guna **akaun ujian kongsi + SSO**. Jangan tunggu Hari 15 untuk sambung ke Profile DB kali pertama.
- **Hari 13–14:** bekukan ciri baharu. Hanya pembetulan pepijat, ujian, dan pembersihan.
- **Hari 15 (integrasi, bukan merge):** setiap sistem berdiri sendiri; **SIT** menguji aliran rentas sistem melalui Profile DB & SSO; Papan Pemuka Induk menarik data; setiap sistem boleh di-**deploy bebas** ke subdomainnya.

> Pasukan yang tidak pernah menguji sambungan Profile DB sehingga Hari 15 **akan** dapati integrasi pecah. Ini dinyatakan awal, dan diulang.
