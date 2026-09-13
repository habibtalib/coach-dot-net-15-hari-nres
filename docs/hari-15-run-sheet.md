# Hari 15 — Run Sheet, Skrip SIT & Triage

Panduan pengendalian hari akhir: integrasi rentas sistem → Papan Pemuka Induk → SIT/UAT → demo. Bahasa Melayu; istilah teknikal Inggeris.

---

## A. Malam ini (pra-hari) — checklist penyelaras

- [ ] Setiap repo `dotnet build` bersih (0 ralat) dan `dotnet test` hijau (yang ada ujian).
- [ ] Setiap sistem boleh log masuk SSO (devsso) **atau** ada mod dev off-network (data sintetik + auto-log-masuk).
- [ ] Profile DB boleh dicapai; Lapor Diri boleh **cipta** profil; sistem lain boleh **baca** (GetProfile?nric).
- [ ] **Triage PPK** selesai (lihat Bahagian C) — ini risiko terbesar.
- [ ] PKS-100 & PKS-99 ditolak ke Done; PKS-30 (SIT) sedia diuji.
- [ ] Satu **dry-run** skrip SIT (Bahagian B) dijalankan sekali hujung malam.
- [ ] Papan Pemuka Induk boleh papar 4 baris: draf saya / dihantar / menunggu kelulusan saya / selesai.

---

## B. Run sheet mengikut sesi

| Masa | Sesi | Fokus | Hasil |
|------|------|-------|-------|
| 9.30–11.00 | SESI 44 · Integrasi Rentas Sistem | Sahkan tiap sistem: sign-on SSO + baca Profile (LD cipta, lain baca); tiap repo `dotnet build` & deploy bebas ke subdomain | Semua sistem baca profil sama dengan betul |
| 11.00–12.30 | SESI 45 · Papan Pemuka Induk | Pandangan bersatu ikut peranan merentas 6 sistem via Profile DB/API | Dashboard induk berjalan |
| 12.30–2.30 | Rehat | — | — |
| 2.30–3.30 | SESI 46 · SIT & UAT Pre-Check | Jalankan skrip SIT (Bahagian B lanjutan), RBAC, muat naik, audit log; rekod lulus/gagal | Laporan SIT |
| 3.30–4.30 | Demo & Capstone | Tiap kumpulan bentang sistem + keputusan seni bina + pengajaran; nota deployment (SQLite → SQL Server); sijil | Demo + penilaian |

---

## C. Skrip SIT — satu aliran rentas sistem

**Objektif:** Satu staf baharu melalui keseluruhan ekosistem: Lapor Diri mencipta profil, sistem lain membacanya via SSO. Rekod lulus/gagal setiap langkah.

**Persediaan:** rangkaian NRES (Profile + SSO sebenar) *atau* mod dev (data sintetik + akaun ujian SSO yang NRIC-nya padan). Guna **satu NRIC ujian** yang sama sepanjang skrip.

| # | Langkah | Jangkaan (Lulus jika…) | L/G |
|---|---------|------------------------|-----|
| 1 | Log masuk SSO → buka **Lapor Diri** | Redirect SSO → balik dengan cookie; halaman [Authorize] terbuka | |
| 2 | Isi & hantar borang Lapor Diri (Lampiran A/B, peribadi, PCB) | Status → Submitted; **profil dicipta** di Profile DB; no. rujukan dijana | |
| 3 | Sahkan profil wujud: `GetProfile?nric=<NRIC>` | 200 + JSON (FullName, Designation, Organization…) | |
| 4 | Log masuk **PKS / Kontrak / ID / Fasiliti / Pas-Parkir** (NRIC sama) | Nama/jawatan/organisasi **terisi automatik** dari profil — bukan taip semula; guna GetProfile, bukan SQL terus | |
| 5 | Hantar satu permohonan dalam satu sistem trek | Status → Submitted; rekod **kekal** selepas restart (bukan in-memory) | |
| 6 | Log masuk sebagai **penyemak** (peranan modul, cth IctSecurityOfficer) → luluskan | Peralihan status via WorkflowService; **audit log** direkod | |
| 7 | Semakan **RBAC**: pengguna tanpa peranan cuba akses skrin admin | Ditolak (302 login / 403) — bukan dibenarkan | |
| 8 | Muat naik lampiran | Fail disimpan & boleh dibaca semula | |
| 9 | Buka **Papan Pemuka Induk** | Papar merentas sistem: draf saya / dihantar / menunggu kelulusan saya / selesai | |

**Matriks RBAC ringkas** (sahkan dibenarkan ✓ / ditolak ✗):

| Peranan | Hantar borang | Semak/lulus | Skrin admin |
|---------|---------------|-------------|-------------|
| Applicant | ✓ | ✗ | ✗ |
| Supervisor / penyemak modul | ✓ | ✓ (skop sendiri) | ✓ (skop sendiri) |
| Admin sistem | ✓ | ✓ | ✓ |

---

## D. Triage PPK (pas-parkir) — minimum untuk lulus SIT

PPK ketinggalan: data masih **in-memory**, tiada sekatan peranan pada kelulusan, crash off-network. Untuk hari akhir, jangan cuba siapkan semua — buat **3 pembetulan minimum** ini sahaja supaya demo/SIT selamat:

1. **Elak crash off-network** (PPK-94): balut `DbSeeder`/query startup dengan `try/catch` + fallback data sintetik, supaya `app.Run()` sentiasa dicapai walau SQL/SSO tiada.
2. **Sekatan peranan pada kelulusan** (PPK-83, keselamatan): tambah `[Authorize(Roles = "…")]` + semakan peranan pelayan pada aksi `Tindakan` (lulus/tolak). Ini isu keselamatan — mesti sebelum demo.
3. **Kekalkan minimum satu aliran** (PPK-84/93): jika masa suntuk, pastikan sekurang-kurangnya aliran borang → hantar → papar **kekal selepas restart** untuk demo (wayar simpan/baca ke `AppDbContext`, atau demo satu aliran sahaja dan nyatakan selebihnya *pasca-kursus*).

*Selebihnya (ujian PPK-85, migrasi penuh EF, integrasi SSO penuh PPK-92) → pasca-kursus; jangan cuba malam ini.*

---

## E. Nota deployment untuk demo (SQLite → SQL Server pada Windows/IIS)

Rujukan pantas bila ditanya semasa capstone:

- **DB:** dev guna SQLite (fail); pengeluaran tukar provider ke **SQL Server** + connection string; jalankan `dotnet ef database update` pada pelayan.
- **IIS:** pasang **ASP.NET Core Hosting Bundle** (ANCM); `dotnet publish -c Release`; salin ke folder site; **Application Pool = "No Managed Code"**; `web.config` dijana oleh publish.
- **Rahsia:** AppKey/kunci RSA/connection string via environment variables atau user-secrets di pelayan — **bukan** dalam repo.
- **CI (sudah dalam DNA kursus):** GitHub Actions `dotnet build` + `dotnet test` atas PR — jalan pada runner Linux, sasaran Windows tidak mengapa.
- **CD ke IIS & Observability = pasca-kursus** (self-hosted runner pada pelayan Windows / Web Deploy; health checks + Serilog→Event Log). Bukan tugas hari ini.
