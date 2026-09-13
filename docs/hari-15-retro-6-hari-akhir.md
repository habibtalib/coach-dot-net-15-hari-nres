# Sprint Retro — 6-hari-akhir (7–13 Sep 2026)

Retro sprint akhir merentas 6 papan (satu sprint per modul), sebelum majlis penutup Hari 15.

## Ringkasan

Sprint dipacu oleh **audit hujung-ke-hujung** (bina + uji + jejak kod sebenar, bukan status Jira). Audit mendedah jurang nyata (tiada ujian, kekalan in-memory, jurang keselamatan, integrasi SSO salah tanggap) dan menjana tiket **runbook integrasi SSO/Profile** untuk setiap modul + tiket pembetulan. Kebanyakan pembetulan mendarat dalam sprint.

## Apa yang mendarat (shipped)

| Kumpulan | Modul | Menang utama |
|----------|-------|--------------|
| K1 | Lapor Diri (LD) | Runbook integrasi (LD-71) ✅; konfig mockup dibetulkan (LD-70) ✅; CI (Strix + k6) dimulakan |
| K1 | Pematuhan PKS | Log masuk devsso (PKS-101) ✅; runbook & WorkflowService (PKS-100/99) *In Review* |
| K1 | Pengurusan Kontrak (CM) | Ujian xUnit ditambah (CM-59) ✅; auto-migrate (CM-60) ✅; kelayakan SMTP dibuang (CM-61) ✅; runbook (CM-62) ✅ |
| K3 | ID / AD / Email | Projek ujian app (ID-104) ✅; runbook (ID-105) ✅; beberapa *In Review* |
| K4 | Tempahan Fasiliti Sukan | Senarai peralatan (FS-5/23/25/31/32/33) ✅; runbook (FS-152) ✅; QR di-skop pasca-kursus (FS-151) |
| K2 | Pas / Parkir / Pelekat | Integrasi SSO dimulakan (PPK-92); **hutang teknikal kekal** (lihat bawah) |

## Carry-over (ke backlog / pasca-kursus)

- **PPK (perhatian):** PPK-83 (kelulusan tiada sekatan peranan — keselamatan), PPK-84/93 (masih in-memory, belum EF), PPK-85 (tiada ujian), PPK-86 (runbook), PPK-94 (crash off-network). Triage minimum untuk Hari 15 — lihat `hari-15-run-sheet.md` Bahagian D.
- **ID:** ID-109 (penamatan akaun), ID-111 (semakan status kendiri, `mvp-gap`).
- **FS:** jurang `post-mvp`/`gap-analysis` (katalog peralatan penuh, cadangan slot alternatif, kalendar dalam aliran) — ditangguh dengan betul.
- **CM:** CM-63 (branch protection main — perlu capaian admin repo GitHub).

## Pengajaran (lessons)

1. **"Done" Jira ≠ perisian berjalan.** Audit E2E mendedah 2 tiket "Done" yang sebenarnya stub/in-memory; sahkan dengan bina + jalankan, bukan status sahaja.
2. **Runbook kongsi menyeragamkan integrasi.** Satu panduan SSO/Profile per modul menarik semua pasukan ke corak betul (GetProfile via API, bukan SQL terus; RSA sign-on sahaja).
3. **Ujian & kekalan ialah garis dasar, bukan tambahan.** Modul tanpa ujian / dengan simpanan in-memory kelihatan siap tetapi gagal SIT.
4. **CI dalam DNA, CD/observability kemudian.** CI (build+test) murah & sudah bermula; CD-ke-IIS + observability = fasa pengukuhan pengeluaran pasca-kursus.

## Tindakan lanjutan

- Fasa pengukuhan pengeluaran: CI penuh setiap repo, CD ke IIS (self-hosted runner Windows / Web Deploy), observability (health checks + Serilog → Event Log; App Insights jika Azure).
- Tutup hutang PPK selepas kursus (persistence EF penuh, ujian, sekatan peranan).
