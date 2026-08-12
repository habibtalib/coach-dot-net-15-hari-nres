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

---

## 🎨 Prompt reka bentuk UI — Claude

Tampal `../../AGENTS.md` + README ini sebagai konteks dahulu. **Semak setiap cadangan — jangan terima membuta.**

```text
Anda pembantu reka bentuk UI untuk sistem "Pengurusan Kontrak ICT" NRES (ASP.NET Core MVC, .NET 10, Razor + Bootstrap 5). Baca AGENTS.md dan README modul ini dahulu.

Sempadan (WAJIB):
- Guna semula _Layout.cshtml, site.css, dan partial kongsi (_StatusBadge, _AuditTrail, _AttachmentList, _ApprovalPanel, _FilterBar, _ValidationSummary). JANGAN cipta semula.
- Gaya khusus modul dalam wwwroot/css/ (cth modul-kontrak.css). Hanya folder Views/Contract/.
- Label Bahasa Melayu, istilah teknikal English. Data SINTETIK. Sistem DALAMAN (SSO).
- Status/lifecycle ikut SubmissionStatus melalui _StatusBadge.

Entiti: ContractRecord (kontrak ICT: storan/backup, sokongan, antivirus), ContractParty (pihak terlibat), ContractMilestone (milestone bayaran).

Reka bentuk skrin:
1. Index daftar kontrak — jadual + _FilterBar + badge status kitaran hayat + no. rujukan KON-2026-####.
2. Borang daftar kontrak (cipta/edit) — butiran kontrak, tempoh (mula/tamat), + sub-borang ContractParty (tambah/buang pihak) + jadual ContractMilestone (bayaran); _ValidationSummary; "Simpan draf" vs "Hantar".
3. Butiran kontrak — ringkasan + garis masa milestone (timeline visual) + _AttachmentList (dokumen perjanjian) + _AuditTrail + _ApprovalPanel.
4. Dashboard IctAdmin — kontrak akan tamat tempoh (amaran), status bayaran milestone, approve/reject.

Untuk setiap skrin: wireframe ringkas (terutama komponen TIMELINE milestone), komponen Bootstrap, markup Razor contoh (asp-for), dan kelas CSS modul. Tanya jika andaian skema tak pasti.
```

> Selepas Claude jawab: sahkan guna partial kongsi, tiada data sebenar, dan tidak menyunting fail kongsi.

---

## 🏗️ Bootstrap skeleton repo — `nres-bpm/pengurusan-kontrak`

> Scaffold **poly-repo**: repo/DB sendiri; hanya **Profile DB** dikongsi. Kanun: [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md) · [`../../AGENTS.md`](../../AGENTS.md). Struktur: `src/PengurusanKontrak.Web` + `src/PengurusanKontrak.Profile` + `tests/PengurusanKontrak.Tests`.

```bash
# 1) Clone repo kosong
git clone https://github.com/nres-bpm/pengurusan-kontrak.git && cd pengurusan-kontrak

# 2) Solution + 3 projek  (DALAMAN → SSO kemudian)
dotnet new sln -n PengurusanKontrak
dotnet new mvc      -o src/PengurusanKontrak.Web
dotnet new classlib -o src/PengurusanKontrak.Profile
dotnet new xunit    -o tests/PengurusanKontrak.Tests
dotnet sln add src/PengurusanKontrak.Web src/PengurusanKontrak.Profile tests/PengurusanKontrak.Tests

# 3) Rujukan projek
dotnet add src/PengurusanKontrak.Web    reference src/PengurusanKontrak.Profile
dotnet add tests/PengurusanKontrak.Tests reference src/PengurusanKontrak.Web

# 4) EF Core (DB sendiri)
dotnet add src/PengurusanKontrak.Web package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/PengurusanKontrak.Web package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef

# 5) Folder modul anda
cd src/PengurusanKontrak.Web
mkdir -p Models/Kontrak/Configurations Views/Contract ViewModels/Kontrak Services/Kontrak Data App_Data/uploads
cd ../..

# 6) Sahkan & 7) push
dotnet run --project src/PengurusanKontrak.Web
git add . && git commit -m "KON: scaffold skeleton (Web + Profile + Tests)"
git push -u origin main
```

**Nota:** Peranan `IctAdmin` · Prefix `KON`. Sistem **MEMBACA** profil (via SSO/`PengurusanKontrak.Profile` → kontrak `nres-bpm/profile`).

> ⚠️ Lab Hari 4 semasa masih guna namespace `Nres.Onboarding.Web.*` (monorepo lama). Poly-repo = `PengurusanKontrak.Web.*`. Selaras dengan jurulatih.
