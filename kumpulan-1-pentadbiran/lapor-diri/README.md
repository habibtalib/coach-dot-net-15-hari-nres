# Kumpulan 1 — Modul Lapor Diri

> Trek Fasa 2 (Hari 4–14). Aturcara: [`../JADUAL.md`](../../JADUAL.md) · Kanun: [`../SPEC-KURSUS.md`](../../SPEC-KURSUS.md) · Kontrak pasukan: [`../KOLABORASI.md`](../../KOLABORASI.md) · Konteks AI: [`../AGENTS.md`](../../AGENTS.md)

## Modul anda dalam satu ayat

Membolehkan pekerja baharu menghantar maklumat lapor diri dan dokumen sokongan, membolehkan HR menyemak, meluluskan atau menolaknya, dan mengeluarkan Slip Akuan Lapor Diri.

## Identiti modul

| Perkara | Nilai |
|---------|-------|
| **Cabang Git** | `kump-1/lapor-diri` |
| **Prefix rujukan** | `LD` → `LD-2026-0001` |
| **`ModuleCode`** | `ModuleCodes.LaporDiri` |
| **Peranan admin** | `HrAdmin` |
| **Jadual anda** | `OfficerReportingApplications` |
| **Aliran kelulusan** | Satu peringkat: `Draft → Submitted → AdminApproved / Rejected` |

## Folder yang anda miliki

```text
Models/LaporDiri/                 termasuk Configurations/
Controllers/OfficerReporting*
Views/OfficerReporting/
ViewModels/LaporDiri/
Services/LaporDiri/
wwwroot/css/modul-lapor-diri.css
```

**Anda tidak menyunting:** `Program.cs` · `Data/ApplicationDbContext.cs` · `Views/Shared/_Layout.cshtml` · `wwwroot/css/site.css` · `Models/Shared/`

## Blok trek

| Blok | Fokus | Deliverable |
|------|-------|-------------|
| [**Hari 4**](./hari-4/) | Skema DB & borang draf | Entiti + konfigurasi + migration; borang cipta/edit; simpan draf |
| [**Hari 5–6**](./hari-5-6/) | Muat naik & nombor rujukan | Lampiran dokumen sokongan; jana `LD-2026-####`; hantar permohonan |
| [**Hari 7–9**](./hari-7-9/) | Kelulusan HR & skrin admin | Dashboard HR; approve/reject dengan ulasan; penapisan; audit |
| [**Hari 10–12**](./hari-10-12/) | Notifikasi, PDF & dashboard | Notifikasi e-mel; Slip Akuan PDF; papan pemuka analitis HR |
| [**Hari 13–14**](./hari-13-14/) | Ujian & sedia gabung | xUnit; optimasi query EF Core; refactor; sedia merge |

## Servis kongsi yang anda GUNA (jangan tulis semula)

`IReferenceNumberService` · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` · `ICurrentUserService` · `SubmissionControllerBase` · `_StatusBadge` · `_AuditTrail` · `_AttachmentList` · `_ApprovalPanel` · `_FilterBar` · `_ValidationSummary`

Daftar penuh: [`../AGENTS.md`](../../AGENTS.md).

## Rentak harian

| Masa | Aktiviti |
|------|----------|
| 9.00 – 9.15 | Stand-up + `git pull --rebase origin master` |
| 9.15 – 9.25 | Semakan silang AI (pertindihan dengan kumpulan lain) |
| 9.25 – 1.00 · 2.30 – 4.30 | Pembangunan |
| 4.30 – 5.00 | Code review berpasangan + PR + push + kemas kini board |

**Hujung setiap blok:** gabungan latihan `kump-1/lapor-diri` → `master` melalui PR.

## Sebelum menulis apa-apa helper

1. `grep -ri "<konsep>" Nres.Onboarding.Web/`
2. Semak daftar komponen kongsi dalam [`../AGENTS.md`](../../AGENTS.md)
3. Tanya AI: *"Merujuk AGENTS.md, adakah repo ini sudah ada cara untuk `<X>`?"*
4. Jika lebih daripada satu modul perlukannya → buka isu berlabel `shared`, jangan bina sendiri

---

## 🎨 Prompt reka bentuk UI — Claude

Tampal `AGENTS.md` + README ini sebagai konteks dahulu, kemudian beri Claude prompt di bawah untuk **mereka bentuk UI** modul (Razor + Bootstrap + CSS modul). **Semak setiap cadangan — jangan terima membuta.** Reka bentuk/wireframe dahulu, baru minta markup penuh.

```text
Anda pembantu reka bentuk UI untuk sistem "Lapor Diri" NRES (ASP.NET Core MVC, .NET 10, Razor + Bootstrap 5). Baca AGENTS.md dan README modul ini dahulu.

Sempadan (WAJIB):
- Guna semula Views/Shared/_Layout.cshtml, wwwroot/css/site.css, dan partial kongsi: _StatusBadge, _AuditTrail, _AttachmentList, _ApprovalPanel, _FilterBar, _ValidationSummary. JANGAN cipta semula.
- Gaya khusus modul hanya dalam wwwroot/css/modul-lapor-diri.css. Jangan sunting _Layout atau site.css.
- Hanya folder Views/OfficerReporting/ + CSS modul anda.
- Label Bahasa Melayu, istilah teknikal English. Data contoh SINTETIK sahaja.
- Status ikut SubmissionStatus (Draft -> Submitted -> AdminApproved / Rejected) melalui _StatusBadge.
- Responsif, kontras tinggi, boleh diakses (label, aria), konsisten dengan sistem NRES lain.

Nada: Lapor Diri = sistem AWAM (public-facing) yang MENCIPTA profil pengguna dalam Profile DB. Rasa seperti wizard onboarding mesra untuk pekerja baharu.

Reka bentuk skrin:
1. Index "Permohonan saya" — kad/jadual + _FilterBar + badge status + butang "Sambung draf".
2. Borang lapor diri (cipta/edit) — maklumat peribadi pekerja baharu, jabatan, tarikh lapor diri; _AttachmentList untuk dokumen sokongan; _ValidationSummary; "Simpan draf" vs "Hantar".
3. Butiran permohonan — ringkasan + _AttachmentList + _AuditTrail + _ApprovalPanel.
4. Dashboard HR (HrAdmin) — kiraan ikut status, _FilterBar, approve/reject + ulasan wajib bila tolak.
5. Slip Akuan Lapor Diri — susun atur cetak/PDF (A4 ringkas, no. rujukan LD-2026-####).

Untuk setiap skrin beri: wireframe ringkas, komponen Bootstrap, markup Razor contoh (tag helper asp-for), dan kelas modul-lapor-diri.css yang perlu. Tanya jika andaian tentang kontrak Profile DB tak pasti.
```

> Selepas Claude jawab: sahkan ia **tidak** menyunting fail kongsi, guna partial sedia ada, dan tiada data sebenar. Baru salin ke projek.

---

## 🏗️ Bootstrap skeleton repo — `nres-bpm/lapor-diri`

> Scaffold **poly-repo** (permulaan Fasa 2): sistem ini = repo, subdomain & DB **sendiri**; hanya **Profile DB** dikongsi. Kanun: [`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md) · [`../../AGENTS.md`](../../AGENTS.md). Struktur: `src/LaporDiri.Web` + `src/LaporDiri.Profile` + `tests/LaporDiri.Tests`.

```bash
# 1) Clone repo kosong pasukan
git clone https://github.com/nres-bpm/lapor-diri.git && cd lapor-diri

# 2) Solution + 3 projek  (Lapor Diri = AWAM → Identity terbina)
dotnet new sln -n LaporDiri
dotnet new mvc      -o src/LaporDiri.Web --auth Individual
dotnet new classlib -o src/LaporDiri.Profile      # klien/kontrak Profile DB
dotnet new xunit    -o tests/LaporDiri.Tests
dotnet sln add src/LaporDiri.Web src/LaporDiri.Profile tests/LaporDiri.Tests

# 3) Rujukan projek
dotnet add src/LaporDiri.Web    reference src/LaporDiri.Profile
dotnet add tests/LaporDiri.Tests reference src/LaporDiri.Web

# 4) EF Core (DB sendiri) — Hari 4 tambah entiti & migration
dotnet add src/LaporDiri.Web package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/LaporDiri.Web package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef            # sekali per mesin

# 5) Folder modul anda
cd src/LaporDiri.Web
mkdir -p Models/LaporDiri/Configurations Views/OfficerReporting ViewModels/LaporDiri Services/LaporDiri Data App_Data/uploads
cd ../..

# 6) Sahkan ia berjalan
dotnet run --project src/LaporDiri.Web            # buka https://localhost:7xxx, Ctrl+C

# 7) Commit awal + push (repo kosong → main; selepas ini main dilindungi, PR sahaja)
git add . && git commit -m "LD: scaffold skeleton (Web + Profile + Tests)"
git push -u origin main
```

**Nota:** Peranan admin `HrAdmin` · Prefix `LD`. Lapor Diri **MENCIPTA** `UserProfile` dalam Profile DB — `LaporDiri.Profile` merujuk kontrak `nres-bpm/profile` (mekanisme paket/submodule ditetapkan Hari 3).

> ⚠️ **Namespace:** lab Hari 4 semasa masih guna `Nres.Onboarding.Web.*` (model monorepo lama, sedang dimigrasi). Dalam poly-repo namespace ialah `LaporDiri.Web.*`. Selaras dengan jurulatih.
