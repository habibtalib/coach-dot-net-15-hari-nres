# Kumpulan 1 — Pentadbiran (3 Projek)

Trek **Kumpulan 1** memikul **tiga projek** dalam aplikasi `Nres.Onboarding.Web`. Ini beban lebih besar daripada kumpulan lain (1 modul) — agihkan ahli/skop sewajarnya. Sumber kebenaran: [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md).

| Projek | Prefix | Peranan semakan | Entiti utama | Folder |
|--------|--------|-----------------|--------------|--------|
| **Lapor Diri** | `LD` | `HrAdmin` | `OfficerReportingApplication` | [`lapor-diri/`](./lapor-diri/) |
| **Pematuhan PKS** | `PKS` | `BPM` | `ComplianceDeclaration`, `PolicyVersion` | [`pematuhan-pks/`](./pematuhan-pks/) |
| **Pengurusan Kontrak** | `KON` | `IctAdmin` | `ContractRecord`, `ContractParty`, `ContractMilestone` | [`pengurusan-kontrak/`](./pengurusan-kontrak/) |

## Prinsip yang dikongsi ketiga-tiga projek

- Guna corak aliran kerja kongsi: `Submission` induk · `SubmissionStatus` · `Attachment` · `AuditLog` · `ApprovalStep`.
- Setiap projek mencipta fail **hanya** dalam folder modulnya (`Models/LaporDiri/`, `Models/Pks/`, `Models/Kontrak/`) — lihat [`../AGENTS.md`](../AGENTS.md) & [`../KOLABORASI.md`](../KOLABORASI.md).
- Setiap projek mendaftar diri: `AddLaporDiriModule()`, `AddPksModule()`, `AddKontrakModule()` + `IEntityTypeConfiguration<T>`.
- Setiap projek mengikut 5 blok trek: **Hari 4 · Hari 5–6 · Hari 7–9 · Hari 10–12 · Hari 13–14**.

## Status kandungan

- ✅ **Lapor Diri** — lengkap (5 blok: README + `snippets/lab.md` + `nota-penceramah.md`).
- 🚧 **Pematuhan PKS** — scaffold; kandungan lab TODO. Draf rujukan lama: [`pematuhan-pks/_draf-rujukan/`](./pematuhan-pks/_draf-rujukan/).
- 🚧 **Pengurusan Kontrak** — scaffold; kandungan lab TODO.
