# Pengurusan Kontrak — Kumpulan 1 (Projek 3/3)

> ✅ **Kandungan lab lengkap** (5 blok: README + `snippets/lab.md` + `nota-penceramah.md`). Sumber kebenaran: [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md).

**Modul:** daftar & jejak **kontrak/perjanjian ICT** (perolehan: storan/backup, sokongan, antivirus) — pihak terlibat, tempoh, milestone bayaran, dan status kitaran hayat.

| Perkara | Nilai |
|---------|-------|
| Prefix nombor rujukan | `KON` (cth `KON-2026-0001`) |
| Peranan semakan | `IctAdmin` |
| Entiti utama | `ContractRecord`, `ContractParty`, `ContractMilestone` |
| Folder kod | `Models/Kontrak/`, `Controllers/Contract*`, `Views/Contract/`, `Services/Kontrak/` |
| Cabang | `kump-1/pentadbiran` (bersama projek K1 lain) |
| Daftar modul | `AddKontrakModule()` + `IEntityTypeConfiguration<T>` |

## 5 blok trek

| Blok | Fokus |
|------|-------|
| [**Hari 4**](./hari-4/) | Skema `ContractRecord` + `ContractParty` + `ContractMilestone` + borang draf |
| [**Hari 5–6**](./hari-5-6/) | Borang daftar kontrak + pihak terlibat + jadual milestone + no. rujukan `KON` |
| [**Hari 7–9**](./hari-7-9/) | Aliran kelulusan `IctAdmin` + skrin admin + jejak milestone/bayaran |
| [**Hari 10–12**](./hari-10-12/) | Peringatan tamat tempoh + laporan kontrak PDF + dashboard analitik |
| [**Hari 13–14**](./hari-13-14/) | xUnit + refactor + sedia merge |

> Setiap blok mesti sisip benang kolaborasi — lihat [`../../KOLABORASI.md`](../../KOLABORASI.md).
