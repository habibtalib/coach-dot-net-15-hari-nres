# Senarai semak modul (build checklist)

> **Bahan rujukan kursus.** Jejak kemajuan **satu modul**: perancangan → bina → integrasi. Tanda `[x]` bila siap **dan disahkan**. Selaras dengan blok Fasa 2 dan prompt **DEV-\*** dalam [`pustaka-prompt.md`](./pustaka-prompt.md).

## Fasa 1 — Perancangan & dokumentasi (Hari 1–3)

- [ ] **URS** modul (dari borang/dokumen NRES — bukan rekaan)
- [ ] **PRD** 7 bahagian (PRD-01) + semak silang (PRD-02)
- [ ] **Use case** + process flow (Mermaid: DIA-04, DIA-02)
- [ ] **ERD** — entiti memaut ke `Submission`, **sifar pendua** — + semak (DIA-03)
- [ ] **Dokumentasi** modul (DOK-01)
- [ ] **Backlog Jira** dari user story PRD (JIRA-01); AC = kriteria penerimaan; setiap isu jejak ke ID URS
- [ ] Repo **scaffold** + `AGENTS.md` + kontrak **Profile DB** (Hari 3)

## Fasa 2 — Bina (Hari 4–14, setiap Jira story)

**Hari 4 — Skema DB**
- [ ] Entiti detail + `IEntityTypeConfiguration` (DEV-05)
- [ ] Migration dijana + `dotnet ef database update`

**Hari 5–6 — Borang (form-first)**
- [ ] Reka UI borang (UI-01)
- [ ] ViewModel + DataAnnotations (DEV-01)
- [ ] View Razor bind ViewModel + `_ValidationSummary` (DEV-02)
- [ ] Controller: papar borang + simpan draf (DEV-03)
- [ ] Validation **pelayan** + peraturan perniagaan (DEV-04)

**Hari 7–9 — Kelulusan & admin**
- [ ] Controller Review/Approve/Reject + `[Authorize(Roles)]` + peralihan `SubmissionStatus` + audit (DEV-06)
- [ ] Skrin admin

**Hari 10–12 — Notifikasi, laporan, dashboard**
- [ ] Notifikasi (`ConsoleNotificationService`)
- [ ] Laporan PDF/Excel + dashboard status

**Hari 13–14 — Ujian & refactor**
- [ ] Ujian xUnit: peralihan status + peraturan perniagaan (DEV-07)
- [ ] Refactor + sedia integrasi

## Fasa 3 — Integrasi (Hari 15)

- [ ] Integrasi via **Profile DB** (SSO)
- [ ] SIT / UAT pre-check
- [ ] Deploy ke subdomain sistem

## Definition of Done (setiap story)

- [ ] **AC PRD lulus** (bukan sekadar "kod ditulis")
- [ ] Validation di **pelayan** + authorization peranan
- [ ] **Audit** direkod
- [ ] **Tiada pendua** — guna servis/partial kongsi (`AGENTS.md`)
- [ ] **Semakan pra-PR** (SMK-01) + faham kod
- [ ] PR + review + merge; isu Jira → **Done**
