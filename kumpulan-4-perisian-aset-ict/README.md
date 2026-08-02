# Kumpulan 4 — Modul Perisian & Aset ICT

> Trek Fasa 2 (Hari 4–14). Aturcara: [`../JADUAL.md`](../JADUAL.md) · Kanun: [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) · Kontrak pasukan: [`../KOLABORASI.md`](../KOLABORASI.md) · Konteks AI: [`../AGENTS.md`](../AGENTS.md)

## Modul anda dalam satu ayat

Menguruskan permohonan lesen perisian dan pinjaman aset ICT — dari katalog dan semakan stok masa-nyata, melalui kelulusan dan serahan, hingga pemulangan dengan pemeriksaan kondisi dan kemas kini inventori automatik.

## Identiti modul

| Perkara | Nilai |
|---------|-------|
| **Cabang Git** | `kump-4/perisian-aset` |
| **Prefix rujukan** | `SW` · `AST-L` (pinjaman) · `AST-R` (pemulangan) |
| **`ModuleCode`** | `ModuleCodes.Perisian` · `PinjamanAset` · `PemulanganAset` |
| **Peranan admin** | `IctAdmin` |
| **Jadual anda** | `Assets`, `SoftwareCatalogItems`, `SoftwareRequests`, `AssetLoanRequests`, `AssetReturns` |
| **Aliran kelulusan** | Satu peringkat + **kitaran pinjaman/pemulangan** |

> **Modul anda satu-satunya yang mempunyai keadaan berterusan di luar permohonan.** Aset mempunyai statusnya **sendiri** (`Available` / `OnLoan` / `UnderMaintenance` / `Lost`) yang **berasingan** daripada `SubmissionStatus`. Jangan campurkan kedua-duanya — ini punca kekeliruan paling biasa dalam modul ini.
>
> Anda juga satu-satunya kumpulan yang memerlukan **transaksi pangkalan data**: meluluskan pinjaman mesti mengemas kini permohonan **dan** status aset secara atomik, atau tidak langsung.

## Folder yang anda miliki

```text
Models/Aset/                      termasuk Configurations/
Controllers/Asset*  Controllers/Software*
Views/Aset/
ViewModels/Aset/
Services/Aset/
wwwroot/css/modul-aset.css
```

**Anda tidak menyunting:** `Program.cs` · `Data/ApplicationDbContext.cs` · `Views/Shared/_Layout.cshtml` · `wwwroot/css/site.css` · `Models/Shared/`

## Blok trek

| Blok | Fokus | Deliverable |
|------|-------|-------------|
| [**Hari 4**](./hari-4/) | Katalog aset & perisian | `Asset`, `SoftwareCatalogItem` + status aset; konfigurasi; **seed katalog contoh**; migration |
| [**Hari 5–6**](./hari-5-6/) | Borang & semakan stok | Borang lesen & pinjaman; **semakan ketersediaan masa-nyata**; borang akuan penerimaan |
| [**Hari 7–9**](./hari-7-9/) | Kelulusan ICT & pemulangan | Skrin Unit Aset; **transaksi inventori**; rekod pemulangan (Baik/Rosak/Hilang); kemas kini stok automatik |
| [**Hari 10–12**](./hari-10-12/) | Peringatan & laporan | Notifikasi pinjaman hampir/lewat tempoh; papan pemuka inventori; **eksport PDF/Excel** |
| [**Hari 13–14**](./hari-13-14/) | Ujian & sedia gabung | Ujian pemulangan lewat, lesen & stok; pembersihan kod; sedia merge |

## Servis kongsi yang anda GUNA (jangan tulis semula)

`IReferenceNumberService` · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` · `ICurrentUserService` · `SubmissionControllerBase` · `_StatusBadge` · `_AuditTrail` · `_AttachmentList` · `_ApprovalPanel` · `_FilterBar` · `_ValidationSummary`

> **Nota khusus anda:** `_StatusBadge` memaparkan `SubmissionStatus`. Status **aset** anda berbeza — anda memerlukan lencana anda sendiri untuk itu, dalam `Views/Aset/`. Ini contoh sah bagi sesuatu yang **khusus modul**, bukan kongsi: tiada kumpulan lain mempunyai inventori.

Daftar penuh: [`../AGENTS.md`](../AGENTS.md).

## Rentak harian

| Masa | Aktiviti |
|------|----------|
| 9.00 – 9.15 | Stand-up + `git pull --rebase origin master` |
| 9.15 – 9.25 | Semakan silang AI (pertindihan dengan kumpulan lain) |
| 9.25 – 1.00 · 2.30 – 4.30 | Pembangunan |
| 4.30 – 5.00 | Code review berpasangan + PR + push + kemas kini board |

**Hujung setiap blok:** gabungan latihan `kump-4/perisian-aset` → `master` melalui PR.

## Sebelum menulis apa-apa helper

1. `grep -ri "<konsep>" Nres.Onboarding.Web/`
2. Semak daftar komponen kongsi dalam [`../AGENTS.md`](../AGENTS.md)
3. Tanya AI: *"Merujuk AGENTS.md, adakah repo ini sudah ada cara untuk `<X>`?"*
4. Jika lebih daripada satu modul perlukannya → buka isu berlabel `shared`, jangan bina sendiri
