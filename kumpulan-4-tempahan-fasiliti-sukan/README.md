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
