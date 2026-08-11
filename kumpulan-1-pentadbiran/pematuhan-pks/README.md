# Pematuhan PKS — Kumpulan 1 (Projek 2/3)

> 🚧 **SCAFFOLD — kandungan lab TODO.** Struktur & spesifikasi modul dikunci di bawah; lab hands-on (5 blok) sedang ditulis. Draf lama (format kumulatif) sebagai rujukan: [`_draf-rujukan/`](./_draf-rujukan/). Sumber kebenaran: [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md).

**Modul:** **Akuan Pematuhan Polisi Keselamatan Siber (PKS)** — pengakuan pematuhan Polisi Keselamatan Siber NRES oleh staf & kontraktor, dikaitkan dengan versi polisi semasa, disertai NDA di bawah **Akta Rahsia Rasmi 1972**.

> **PKS = Polisi Keselamatan Siber** (disahkan dari borang sumber NRES *"Borang Akuan Pematuhan Polisi Keselamatan Siber"*), **bukan** "Kod Setia".

| Perkara | Nilai |
|---------|-------|
| Prefix nombor rujukan | `PKS` (cth `PKS-2026-0001`) |
| Peranan semakan | `IctSecurityOfficer` (Pegawai Keselamatan ICT) |
| Ditadbir oleh | **BPM = Bahagian Pengurusan Maklumat** |
| Entiti utama | `ComplianceDeclaration` (varian **staf** & **kontraktor/syarikat**), `PolicyVersion` |
| Folder kod | `Models/Pks/`, `Controllers/Compliance*`, `Views/Compliance/`, `Services/Pks/` |
| Cabang | `kump-1/pentadbiran` (bersama projek K1 lain) |
| Daftar modul | `AddPksModule()` + `IEntityTypeConfiguration<T>` |

## Butiran domain (dari borang sumber)

- **Dua varian borang:** **staf** (`Nama`, `No. KP`, `Jawatan`, `Bahagian`) dan **kontraktor/syarikat** (tambah `CompanyName`, `CompanyRegNo`).
- Setiap akuan **dikaitkan dengan `PolicyVersion` semasa** — bila polisi dikemas kini, staf perlu **akui semula** (status pematuhan: patuh vs perlu-akui-semula).
- Disemak & disahkan oleh **Pegawai Keselamatan ICT** (tandatangan + cop rasmi dalam borang sebenar).

## Rangka 5 blok (TODO — tulis mengikut corak `../lapor-diri/`)

| Blok | Fokus (cadangan) |
|------|------------------|
| **Hari 4** | Skema `ComplianceDeclaration` (staf/kontraktor) + `PolicyVersion` + borang draf |
| **Hari 5–6** | Borang akuan + kait versi polisi semasa + no. rujukan `PKS` |
| **Hari 7–9** | Aliran semakan `IctSecurityOfficer` + skrin admin + status pematuhan (patuh / perlu-akui-semula) |
| **Hari 10–12** | Notifikasi (bila polisi berubah) + laporan pematuhan PDF + dashboard |
| **Hari 13–14** | xUnit + refactor + sedia merge |

> Setiap blok mesti sisip benang kolaborasi (semakan "sudah wujud?", sempadan folder, slot migration, semakan silang AI, DoD) — lihat [`../../KOLABORASI.md`](../../KOLABORASI.md).
