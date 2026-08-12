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

---

## 🏗️ Bootstrap skeleton repo — `nres-bpm/pematuhan-pks`

> Scaffold **poly-repo**: repo/DB sendiri; hanya **Profile DB** dikongsi. Kanun: [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md) · [`../../AGENTS.md`](../../AGENTS.md). Struktur: `src/PematuhanPks.Web` + `src/PematuhanPks.Profile` + `tests/PematuhanPks.Tests`.

```bash
# 1) Clone repo pasukan (repo sudah ada README.md)
git clone https://github.com/nres-bpm/pematuhan-pks.git && cd pematuhan-pks
dotnet new gitignore          # abaikan bin/ obj/ *.db

# 2) Solution + 3 projek  (DALAMAN → SSO ditambah kemudian, tiada --auth)
dotnet new sln -n PematuhanPks
dotnet new mvc      -o src/PematuhanPks.Web
dotnet new classlib -o src/PematuhanPks.Profile
dotnet new xunit    -o tests/PematuhanPks.Tests
dotnet sln add src/PematuhanPks.Web src/PematuhanPks.Profile tests/PematuhanPks.Tests

# 3) Rujukan projek
dotnet add src/PematuhanPks.Web    reference src/PematuhanPks.Profile
dotnet add tests/PematuhanPks.Tests reference src/PematuhanPks.Web

# 4) EF Core (DB sendiri)
dotnet add src/PematuhanPks.Web package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/PematuhanPks.Web package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef

# 5) Folder modul anda
cd src/PematuhanPks.Web
mkdir -p Models/Pks/Configurations Views/Compliance ViewModels/Pks Services/Pks Data App_Data/uploads
cd ../..

# 6) Sahkan; 7) scaffold pada cabang -> PR ke main (main ada README)
dotnet run --project src/PematuhanPks.Web
git switch -c chore/scaffold
git add . && git commit -m "PKS: scaffold skeleton (Web + Profile + Tests)"
git push -u origin chore/scaffold   # buka PR ke main di GitHub
```

**Nota:** Peranan `IctSecurityOfficer` · Prefix `PKS`. Sistem **MEMBACA** profil (via SSO/`PematuhanPks.Profile` → kontrak `nres-bpm/profile`).

> ⚠️ Lab Hari 4 semasa masih guna namespace `Nres.Onboarding.Web.*` (monorepo lama). Poly-repo = `PematuhanPks.Web.*`. Selaras dengan jurulatih.
