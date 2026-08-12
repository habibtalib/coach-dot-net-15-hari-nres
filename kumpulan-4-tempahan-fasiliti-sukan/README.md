# Kumpulan 4 — Modul Tempahan Fasiliti Sukan

> Trek Fasa 2 (Hari 4–14). Aturcara: [`../JADUAL.md`](../JADUAL.md) · Kanun: [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) · Kontrak pasukan: [`../KOLABORASI.md`](../KOLABORASI.md) · Konteks AI: [`../AGENTS.md`](../AGENTS.md)

## Modul anda dalam satu ayat

Menguruskan tempahan gelanggang dan kemudahan sukan NRES — katalog fasiliti, borang tempahan slot masa, **semakan slot bertindih**, kalendar ketersediaan, dan kelulusan `FacilityAdmin` dengan peruntukan slot.

## Persona yang anda bina untuknya

**Encik Faizal, Setiausaha Kelab Sukan NRES.** Masalahnya hari ini (dari sesi Design Thinking Hari 1):

> *"Saya tak tahu slot mana yang kosong. Saya hantar borang, orang lain pun hantar borang — hujung minggu dua kumpulan sampai gelanggang yang sama. Semuanya manual: WhatsApp, buku log di kaunter, telefon pengawal. Bila bertindih, tiada siapa yang salah dan tiada siapa yang betul."*

Setiap ciri modul ini menyelesaikan satu ayat dalam aduan itu. **Semakan slot bertindih** ialah jantungnya.

## Identiti modul

| Perkara | Nilai |
|---------|-------|
| **Cabang Git** | `kump-4/tempahan-fasiliti` |
| **Prefix rujukan** | `TFS` (cth. `TFS-2026-0001`) |
| **`ModuleCode`** | `ModuleCodes.TempahanFasilitiSukan` (`"TFS"`) |
| **Peranan admin** | `FacilityAdmin` |
| **Jadual anda** | `SportsFacilities`, `FacilityBookingApplications`, `FacilityBookingSlots` |
| **Aliran kelulusan** | Satu peringkat + **semakan bertindih semula pada kelulusan** |

> **Ciri teras modul anda ialah semakan pertindihan slot** — analog kepada semakan pendua nombor plat Kumpulan 2, tetapi lebih halus: pertindihan **julat masa** tidak boleh dinyatakan sebagai indeks unik pangkalan data. Itu bermakna semakan aplikasi anda ialah **satu-satunya** pertahanan. Beri perhatian penuh kepadanya pada Hari 5–6 dan Hari 13–14.

## Folder yang anda miliki

```text
Models/Fasiliti/                  termasuk Configurations/
Controllers/FacilityBooking*
Views/FacilityBooking/
ViewModels/Fasiliti/
Services/Fasiliti/
wwwroot/css/modul-fasiliti.css
```

**Anda tidak menyunting:** `Program.cs` · `Data/ApplicationDbContext.cs` · `Views/Shared/_Layout.cshtml` · `wwwroot/css/site.css` · `Models/Shared/`

## Blok trek

| Blok | Fokus | Deliverable |
|------|-------|-------------|
| [**Hari 4**](./hari-4/) | Skema katalog & tempahan | `SportsFacility` + `FacilityBookingApplication` + `FacilityBookingSlot` + konfigurasi + seed katalog + migration; halaman utama modul |
| [**Hari 5–6**](./hari-5-6/) | Borang tempahan & **semakan bertindih** | Borang + akuan; validation waktu operasi; **sekat tempahan slot bertindih**; no. rujukan `TFS` |
| [**Hari 7–9**](./hari-7-9/) | Kelulusan & kalendar | Skrin `FacilityAdmin`; semak-semula bertindih pada kelulusan; peruntukan slot; kalendar ketersediaan |
| [**Hari 10–12**](./hari-10-12/) | Peringatan, dashboard & eksport | Peringatan tempahan; dashboard kalendar mingguan; eksport **PDF (QuestPDF)** + **Excel (ClosedXML)** |
| [**Hari 13–14**](./hari-13-14/) | Ujian & sedia gabung | **Ujian pertindihan slot menyeluruh** (bersebelahan OK, bertindih ditolak); refactor; sedia merge |

## Servis kongsi yang anda GUNA (jangan tulis semula)

`IReferenceNumberService` · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` · `ICurrentUserService` · `SubmissionControllerBase` · `_StatusBadge` · `_AuditTrail` · `_AttachmentList` · `_ApprovalPanel` · `_FilterBar` · `_ValidationSummary`

Daftar penuh: [`../AGENTS.md`](../AGENTS.md).

> **Andaian asas kongsi (Hari 3):** modul anda mengharapkan `ModuleCodes.TempahanFasilitiSukan` (`"TFS"`) dan peranan `FacilityAdmin` sudah wujud dan berseed. Jika salah satu tiada semasa anda memulakan Hari 4, **jangan tambah sendiri ke fail kongsi** — buka isu berlabel `shared` (lihat [`../KOLABORASI.md`](../KOLABORASI.md) §4) dan beritahu jurulatih.

## Rentak harian

| Masa | Aktiviti |
|------|----------|
| 9.00 – 9.15 | Stand-up + `git pull --rebase origin master` |
| 9.15 – 9.25 | Semakan silang AI (pertindihan dengan kumpulan lain) |
| 9.25 – 1.00 · 2.30 – 4.30 | Pembangunan |
| 4.30 – 5.00 | Code review berpasangan + PR + push + kemas kini board |

**Hujung setiap blok:** gabungan latihan `kump-4/tempahan-fasiliti` → `master` melalui PR.

## Sebelum menulis apa-apa helper

1. `grep -ri "<konsep>" Nres.Onboarding.Web/`
2. Semak daftar komponen kongsi dalam [`../AGENTS.md`](../AGENTS.md)
3. Tanya AI: *"Merujuk AGENTS.md, adakah repo ini sudah ada cara untuk `<X>`?"*
4. Jika lebih daripada satu modul perlukannya → buka isu berlabel `shared`, jangan bina sendiri

---

## 🎨 Prompt reka bentuk UI — Claude

Tampal `AGENTS.md` + README ini sebagai konteks dahulu. **Semak setiap cadangan — jangan terima membuta.**

```text
Anda pembantu reka bentuk UI untuk sistem "Tempahan Fasiliti Sukan" NRES (ASP.NET Core MVC, .NET 10, Razor + Bootstrap 5). Baca AGENTS.md dan README modul ini dahulu.

Sempadan (WAJIB):
- Guna semula _Layout.cshtml, site.css, dan partial kongsi (_StatusBadge, _AuditTrail, _AttachmentList, _ApprovalPanel, _FilterBar, _ValidationSummary). JANGAN cipta semula.
- Gaya khusus modul dalam wwwroot/css/modul-fasiliti.css. Hanya folder Views/FacilityBooking/.
- Label Bahasa Melayu, istilah teknikal English. Data SINTETIK. Sistem DALAMAN (SSO).
- Status ikut SubmissionStatus melalui _StatusBadge.

Ciri teras: semakan slot BERTINDIH (julat masa) — UI mesti bantu pengguna elak pertindihan sebelum hantar. Persona: Encik Faizal, Setiausaha Kelab Sukan (dari Design Thinking Hari 1).

Reka bentuk skrin:
1. Katalog fasiliti — kad SportsFacility (gelanggang/kemudahan) + status ketersediaan.
2. Borang tempahan — pilih fasiliti + tarikh + slot masa (mula/tamat); AMARAN INLINE bila slot bertindih dengan tempahan sedia ada; akuan; _ValidationSummary; no. rujukan TFS-2026-####.
3. Kalendar ketersediaan (paparan mingguan) — slot ditempah vs kosong, warna jelas.
4. Skrin kelulusan FacilityAdmin — semak-semula pertindihan pada kelulusan; approve/reject + peruntukan slot.
5. Dashboard + eksport — ringkasan tempahan, butang eksport PDF & Excel.

Untuk setiap skrin: wireframe ringkas (terutama KALENDAR mingguan & amaran bertindih), komponen Bootstrap, markup Razor contoh (asp-for), dan kelas modul-fasiliti.css. Tanya jika andaian skema tak pasti.
```

> Selepas Claude jawab: sahkan logik amaran bertindih hanya UI (pengesahan sebenar di pelayan), guna partial kongsi, dan tiada data sebenar.

---

## 🏗️ Bootstrap skeleton repo — `nres-bpm/tempahan-fasiliti-sukan`

> Scaffold **poly-repo**: repo/DB sendiri; hanya **Profile DB** dikongsi. Kanun: [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) · [`../AGENTS.md`](../AGENTS.md). Struktur: `src/TempahanFasilitiSukan.Web` + `src/TempahanFasilitiSukan.Profile` + `tests/TempahanFasilitiSukan.Tests`.

```bash
# 1) Clone repo kosong
git clone https://github.com/nres-bpm/tempahan-fasiliti-sukan.git && cd tempahan-fasiliti-sukan

# 2) Solution + 3 projek  (DALAMAN → SSO kemudian)
dotnet new sln -n TempahanFasilitiSukan
dotnet new mvc      -o src/TempahanFasilitiSukan.Web
dotnet new classlib -o src/TempahanFasilitiSukan.Profile
dotnet new xunit    -o tests/TempahanFasilitiSukan.Tests
dotnet sln add src/TempahanFasilitiSukan.Web src/TempahanFasilitiSukan.Profile tests/TempahanFasilitiSukan.Tests

# 3) Rujukan projek
dotnet add src/TempahanFasilitiSukan.Web    reference src/TempahanFasilitiSukan.Profile
dotnet add tests/TempahanFasilitiSukan.Tests reference src/TempahanFasilitiSukan.Web

# 4) EF Core (DB sendiri)
dotnet add src/TempahanFasilitiSukan.Web package Microsoft.EntityFrameworkCore.Sqlite
dotnet add src/TempahanFasilitiSukan.Web package Microsoft.EntityFrameworkCore.Design
dotnet tool install --global dotnet-ef

# 5) Folder modul anda
cd src/TempahanFasilitiSukan.Web
mkdir -p Models/Fasiliti/Configurations Views/FacilityBooking ViewModels/Fasiliti Services/Fasiliti Data App_Data/uploads
cd ../..

# 6) Sahkan & 7) push
dotnet run --project src/TempahanFasilitiSukan.Web
git add . && git commit -m "TFS: scaffold skeleton (Web + Profile + Tests)"
git push -u origin main
```

**Nota:** Peranan `FacilityAdmin` · Prefix `TFS`. Sistem **MEMBACA** profil (via SSO/`TempahanFasilitiSukan.Profile` → kontrak `nres-bpm/profile`). Ciri teras (semakan slot bertindih) datang Hari 5–6.

> ⚠️ Lab Hari 4 semasa masih guna namespace `Nres.Onboarding.Web.*` (monorepo lama). Poly-repo = `TempahanFasilitiSukan.Web.*`. Selaras dengan jurulatih.
