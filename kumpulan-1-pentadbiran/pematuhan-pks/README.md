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

---

## 🎨 Prompt reka bentuk UI — Claude

Tampal `../../AGENTS.md` + README ini sebagai konteks dahulu. **Semak setiap cadangan — jangan terima membuta.**

```text
Anda pembantu reka bentuk UI untuk sistem "Pematuhan PKS (Polisi Keselamatan Siber)" NRES (ASP.NET Core MVC, .NET 10, Razor + Bootstrap 5). Baca AGENTS.md dan README modul ini dahulu.

Sempadan (WAJIB):
- Guna semula Views/Shared/_Layout.cshtml, site.css, dan partial kongsi (_StatusBadge, _AuditTrail, _AttachmentList, _ApprovalPanel, _FilterBar, _ValidationSummary). JANGAN cipta semula.
- Gaya khusus modul dalam wwwroot/css/ (fail CSS modul anda, cth modul-pks.css). Hanya folder Views/Compliance/.
- Label Bahasa Melayu, istilah teknikal English. Data SINTETIK sahaja.
- Status ikut SubmissionStatus melalui _StatusBadge. Sistem DALAMAN (SSO) — pengguna sudah ada profil.

Ciri khas modul: dua VARIAN borang (staf: Nama/No. KP/Jawatan/Bahagian; kontraktor: tambah CompanyName/CompanyRegNo). Setiap akuan dikait PolicyVersion semasa; bila polisi berubah, staf perlu AKUI SEMULA.

Reka bentuk skrin:
1. Index pematuhan — jadual akuan + _FilterBar + badge status pematuhan (Patuh / Perlu akui semula) + no. rujukan PKS-2026-####.
2. Borang akuan (staf/kontraktor) — pilih varian; papar TEKS polisi + NDA (Akta Rahsia Rasmi 1972); checkbox "Saya faham & akui"; _ValidationSummary.
3. Banner "Polisi telah dikemas kini — sila akui semula" untuk pengguna yang tertunggak.
4. Butiran akuan — ringkasan + versi polisi + _AuditTrail + _ApprovalPanel.
5. Dashboard Pegawai Keselamatan ICT (IctSecurityOfficer) — kadar pematuhan, senarai perlu-akui-semula, approve/reject.

Untuk setiap skrin: wireframe ringkas, komponen Bootstrap, markup Razor contoh (asp-for), dan kelas CSS modul. Tanya jika andaian skema tak pasti.
```

> Selepas Claude jawab: sahkan tiada data sebenar, guna partial kongsi, dan teks NDA/polisi ialah contoh sintetik.
