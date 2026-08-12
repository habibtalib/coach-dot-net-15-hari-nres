# Kumpulan 3 — Modul ID, AD & Email

> Trek Fasa 2 (Hari 4–14). Aturcara: [`../JADUAL.md`](../JADUAL.md) · Kanun: [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) · Kontrak pasukan: [`../KOLABORASI.md`](../KOLABORASI.md) · Konteks AI: [`../AGENTS.md`](../AGENTS.md)

## Modul anda dalam satu ayat

Menguruskan permohonan akaun pengguna dan akses sistem — akaun Active Directory, e-mel rasmi, pertukaran akses dan nyahaktif — melalui kelulusan dua peringkat (Penyelia, kemudian ICT) dengan jejak audit penuh.

## Identiti modul

| Perkara | Nilai |
|---------|-------|
| **Cabang Git** | `kump-3/id-ad-email` |
| **Prefix rujukan** | `ICT-ID` → `ICT-ID-2026-0001` |
| **`ModuleCode`** | `ModuleCodes.IdAdEmail` |
| **Peranan** | `Supervisor` (peringkat 1) → `IctAdmin` (peringkat 2) |
| **Jadual anda** | `AccountRequests`, `RequestedSystemAccesses` |
| **Aliran kelulusan** | **Dua peringkat**: `Draft → Submitted → SupervisorApproved → Completed / Rejected` |

> **Modul anda mempunyai aliran paling kompleks** — satu-satunya dengan kelulusan dua peringkat. Anda akan menggunakan `ApprovalStep` (dengan `StepOrder`) secara serius, di mana kumpulan lain hanya menggunakan satu langkah.
>
> 🔒 **Modul anda juga paling sensitif dari segi keselamatan.** Anda menguruskan permohonan akaun — jangan sekali-kali menyimpan kata laluan dalam mana-mana entiti anda. Ini titik pengajaran, dan ia akan diperiksa.

## Folder yang anda miliki

```text
Models/Akaun/                     termasuk Configurations/
Controllers/AccountRequest*
Views/Akaun/
ViewModels/Akaun/
Services/Akaun/
wwwroot/css/modul-akaun.css
```

**Anda tidak menyunting:** `Program.cs` · `Data/ApplicationDbContext.cs` · `Views/Shared/_Layout.cshtml` · `wwwroot/css/site.css` · `Models/Shared/`

## Blok trek

| Blok | Fokus | Deliverable |
|------|-------|-------------|
| [**Hari 4**](./hari-4/) | Skema DB akaun & akses | `AccountRequest` + `RequestedSystemAccess`; jenis permohonan; laluan kelulusan 2 peringkat; migration |
| [**Hari 5–6**](./hari-5-6/) | Borang & kelulusan Penyelia | Borang AD/e-mel/akses; skrin kelulusan peringkat 1; pengesahan identiti & role mapping |
| [**Hari 7–9**](./hari-7-9/) | Pemprosesan ICT & RBAC | Skrin Pentadbir ICT; **kuatkuasa RBAC merentas modul**; simulasi integrasi AD |
| [**Hari 10–12**](./hari-10-12/) | Penjejakan, audit & dashboard | Penjejakan status; **audit trail penuh**; carian/penapis; papan pemuka ICT |
| [**Hari 13–14**](./hari-13-14/) | RBAC testing & security audit | Ujian kebenaran capaian setiap peranan; semakan keselamatan log audit; sedia merge |

## Servis kongsi yang anda GUNA (jangan tulis semula)

`IReferenceNumberService` · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` · `ICurrentUserService` · `SubmissionControllerBase` · `_StatusBadge` · `_AuditTrail` · `_AttachmentList` · `_ApprovalPanel` · `_FilterBar` · `_ValidationSummary`

> **Nota khusus anda:** `SubmissionControllerBase.Approve` menetapkan `AdminApproved`. Aliran dua peringkat anda memerlukan `SupervisorApproved` dahulu. Ini **bukan** alasan untuk menulis semula kelas asas — ia sebab untuk **menambah** tindakan `SupervisorApprove` dalam controller anda yang memanggil `IWorkflowService.TransitionAsync`. Bincang dengan jurulatih pada Hari 5–6.

Daftar penuh: [`../AGENTS.md`](../AGENTS.md).

## Rentak harian

| Masa | Aktiviti |
|------|----------|
| 9.00 – 9.15 | Stand-up + `git pull --rebase origin master` |
| 9.15 – 9.25 | Semakan silang AI (pertindihan dengan kumpulan lain) |
| 9.25 – 1.00 · 2.30 – 4.30 | Pembangunan |
| 4.30 – 5.00 | Code review berpasangan + PR + push + kemas kini board |

**Hujung setiap blok:** gabungan latihan `kump-3/id-ad-email` → `master` melalui PR.

## Sebelum menulis apa-apa helper

1. `grep -ri "<konsep>" Nres.Onboarding.Web/`
2. Semak daftar komponen kongsi dalam [`../AGENTS.md`](../AGENTS.md)
3. Tanya AI: *"Merujuk AGENTS.md, adakah repo ini sudah ada cara untuk `<X>`?"*
4. Jika lebih daripada satu modul perlukannya → buka isu berlabel `shared`, jangan bina sendiri

---

## 🎨 Prompt reka bentuk UI — Claude

Tampal `AGENTS.md` + README ini sebagai konteks dahulu. **Semak setiap cadangan — jangan terima membuta.**

```text
Anda pembantu reka bentuk UI untuk sistem "ID, AD & Email" NRES (ASP.NET Core MVC, .NET 10, Razor + Bootstrap 5). Baca AGENTS.md dan README modul ini dahulu.

Sempadan (WAJIB):
- Guna semula _Layout.cshtml, site.css, dan partial kongsi (_StatusBadge, _AuditTrail, _AttachmentList, _ApprovalPanel, _FilterBar, _ValidationSummary). JANGAN cipta semula.
- Gaya khusus modul dalam wwwroot/css/modul-akaun.css. Hanya folder Views/Akaun/.
- Label Bahasa Melayu, istilah teknikal English. Data SINTETIK. Sistem DALAMAN (SSO).

KESELAMATAN (WAJIB): ini modul paling sensitif. JANGAN reka apa-apa medan kata laluan / input password di mana-mana skrin — akaun dicipta di luar sistem. Ini titik pengajaran & akan diperiksa.

Aliran DUA PERINGKAT: Draft -> Submitted -> SupervisorApproved -> Completed / Rejected. Papar penunjuk peringkat (stepper) dengan jelas.

Reka bentuk skrin:
1. Index permohonan — jadual + _FilterBar + badge status + stepper 2 peringkat + no. rujukan ICT-ID-2026-####.
2. Borang permohonan akaun — jenis (AD / e-mel / akses sistem); senarai RequestedSystemAccess (tambah berbilang akses); TIADA medan kata laluan; _ValidationSummary.
3. Skrin kelulusan Penyelia (peringkat 1) — sahkan identiti & role mapping; approve/reject peringkat 1.
4. Skrin Pentadbir ICT (peringkat 2) — kelulusan SEPARA (luluskan 3 drpd 5 akses secara individu); tandakan selesai.
5. Dashboard ICT + audit — penjejakan status, _AuditTrail penuh, carian/penapis mengikut peranan (RBAC).

Untuk setiap skrin: wireframe ringkas, komponen Bootstrap (stepper, senarai akses boleh-luluskan-satu-satu), markup Razor contoh (asp-for), dan kelas modul-akaun.css. Tanya jika andaian skema tak pasti.
```

> Selepas Claude jawab: sahkan TIADA medan kata laluan, guna partial kongsi, dan tiada data sebenar.

---

## 🏗️ Bootstrap skeleton repo — `nres-bpm/id-ad-email`

> Scaffold **poly-repo**: repo/DB sendiri; hanya **Profile DB** dikongsi. Kanun: [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) · [`../AGENTS.md`](../AGENTS.md). Struktur: `src/IdAdEmail.Web` + `src/IdAdEmail.Profile` + `tests/IdAdEmail.Tests`.

```bash
# 1) Clone repo pasukan (repo sudah ada README.md)
git clone https://github.com/nres-bpm/id-ad-email.git && cd id-ad-email
dotnet new gitignore          # abaikan bin/ obj/ *.db

# 2) Solution + 3 projek  (DALAMAN → SSO kemudian)
dotnet new sln -n IdAdEmail
dotnet new mvc      -o src/IdAdEmail.Web
dotnet new classlib -o src/IdAdEmail.Profile
dotnet new xunit    -o tests/IdAdEmail.Tests
dotnet sln add src/IdAdEmail.Web src/IdAdEmail.Profile tests/IdAdEmail.Tests

# 3) Rujukan projek
dotnet add src/IdAdEmail.Web    reference src/IdAdEmail.Profile
dotnet add tests/IdAdEmail.Tests reference src/IdAdEmail.Web

# 4) EF Core (DB sendiri)
dotnet add src/IdAdEmail.Web package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/IdAdEmail.Web package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef

# 5) Folder modul anda
cd src/IdAdEmail.Web
mkdir -p Models/Akaun/Configurations Views/Akaun ViewModels/Akaun Services/Akaun Data App_Data/uploads
cd ../..

# 6) Sahkan; 7) scaffold pada cabang -> PR ke main (main ada README)
dotnet run --project src/IdAdEmail.Web
git switch -c chore/scaffold
git add . && git commit -m "ICT-ID: scaffold skeleton (Web + Profile + Tests)"
git push -u origin chore/scaffold   # buka PR ke main di GitHub
```

**Nota:** Peranan `Supervisor` → `IctAdmin` (2 peringkat) · Prefix `ICT-ID`. Sistem **MEMBACA** profil (via SSO/`IdAdEmail.Profile` → kontrak `nres-bpm/profile`).
> 🔒 **Keselamatan:** jangan sekali-kali simpan kata laluan dalam mana-mana entiti — titik pengajaran, akan diperiksa.

> ⚠️ Lab Hari 4 semasa masih guna namespace `Nres.Onboarding.Web.*` (monorepo lama). Poly-repo = `IdAdEmail.Web.*`. Selaras dengan jurulatih.
