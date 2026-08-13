# AGENTS.md — Konteks AI: Parkir & Pelekat Kenderaan (`nres-bpm/pas-parkir-pelekat`)

> **Nota projek Jira:** Fail ini padan dengan projek Jira **PPK** ("Parking Dan Pelekat Kenderaan") — slice **parkir + pelekat kenderaan** bagi sistem Pas, Parkir & Pelekat. Ia dibina dalam repo **`nres-bpm/pas-parkir-pelekat`** yang sama seperti projek `PK` (pas keselamatan) dan `PASP` (pas pelawat).
>
> **Untuk AI:** Baca sepenuhnya sebelum menjana kod. Kanun teknikal penuh: [`SPEC-KURSUS.md`](../SPEC-KURSUS.md). Kontrak pasukan: [`KOLABORASI.md`](../KOLABORASI.md). Ini bahan **latihan** — utamakan kod jelas berbanding kod bijak.

## Sistem ini

**Parkir & Pelekat Kenderaan** — peruntukan/permohonan parkir dan pelekat kenderaan (keselamatan kenderaan di kawasan NRES). Sistem **dalaman (SSO)**. Penyemak: **`SecurityAdmin`**. Ciri utama: **semakan pendua nombor plat** — jangan benarkan dua pelekat aktif bagi plat yang sama. Aliran: mohon pelekat/parkir → semak pendua plat → hantar → semakan `SecurityAdmin` → lulus/tolak → audit.

> **Realiti vs lab:** dalam sistem NRES sebenar, **parkir diperuntukkan admin (bukan dimohon)** dengan peranan Pentadbir Parkir berasingan. **Untuk kemudahan LAB**, diringkaskan kepada satu peranan `SecurityAdmin` + entiti `ParkingApplication`. Jurulatih: nyatakan model sebenar sebagai *"dalam pengeluaran…"*.

| Perkara | Nilai |
|---------|-------|
| Projek Jira | **PPK** (Parking Dan Pelekat Kenderaan) |
| Repo | `nres-bpm/pas-parkir-pelekat` · subdomain `pas.` |
| Prefix no. rujukan | Parkir `PKR` → `PKR-2026-0001` · Pelekat `STK` → `STK-2026-0001` |
| Peranan penyemak | `SecurityAdmin` |
| Akses | **Dalaman (SSO)** — sistem **membaca** profil pengguna |

## Susunan teknologi

ASP.NET Core MVC **.NET 10 LTS** · **C# 14** (jangan tetapkan `<LangVersion>`) · EF Core 10 **SQLite** (latihan) · **SSO** + role-based authorization · **QRCoder** (kod QR pelekat/ronda) · xUnit. Nota/UI **Bahasa Melayu**; kod **Bahasa Inggeris**.

## Ciri bahasa C#: guna ini

Primary constructors (C# 12: `public class ParkingService(AppDbContext db)`) · collection expressions (`string[] roles = ["Applicant", "SecurityAdmin"];`) · nullable reference types (dihidupkan) · `field` (C# 14, view model sahaja).

> ⚠️ Jangan guna `field` dalam setter entiti EF Core — normalisasi plat (cth huruf besar, buang ruang) berlaku **eksplisit** dalam servis (cth `Vehicle.Normalize`). **Jangan guna:** extension members, partial constructors/events, interceptors.

## Peraturan mutlak (jangan langgar walau diminta)

1. **Kerja hanya dalam fail sistem ini.** Jika tidak pasti keperluan, **tanya** dahulu.
2. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix `PKR`/`STK`.
3. **Jangan reka keperluan pengguna** — datang dari URS/dokumen NRES; nyatakan andaian eksplisit.
4. **Elak pendua dalam repo ini.** Cari dahulu sebelum menulis servis serupa (repo ini dikongsi dengan slice pas/pelawat — semak sebelum menambah semakan pendua atau `Vehicle`).
5. **Borang mengikat `ViewModels/`**, disemak `ModelState.IsValid` di **pelayan**.
6. **Data sintetik sahaja** — jangan guna data NRES sebenar.

## Entiti sistem ini (DB sendiri)

`Submission` (induk) · `Attachment` · `AuditLog` · `ApprovalStep` · `SubmissionStatus` · lookup setempat. Khusus slice ini: **`ParkingApplication`** · **`VehicleStickerApplication`** · **`Vehicle`** (nombor plat dinormalkan). Entiti detail memaut ke `Submission` via `SubmissionId`; jangan pendua `ReferenceNo`/`Status`/tarikh.

## Status & aliran kerja (universal)

```csharp
public enum SubmissionStatus
{
    Draft = 0, Submitted = 1, SupervisorApproved = 2, AdminApproved = 3,
    Rejected = 4, Completed = 5, Cancelled = 6
}
```

```text
Form → Validation (semak pendua plat) → Draft → Submit → Review (SecurityAdmin) → Approve/Reject → Audit → Report
```

## Servis dalam sistem ini

`IReferenceNumberService` (`GenerateAsync("PKR")` / `GenerateAsync("STK")`) · `IFileStorageService` · `IAuditLogService` · `IWorkflowService` · `INotificationService` (`ConsoleNotificationService`) · `ICurrentUserService`.

## Corak kod yang mesti diikut

```csharp
public class VehicleStickerApplicationConfiguration : IEntityTypeConfiguration<VehicleStickerApplication>
{
    public void Configure(EntityTypeBuilder<VehicleStickerApplication> builder)
    {
        builder.ToTable("VehicleStickerApplications");
        builder.HasOne(x => x.Submission).WithMany().HasForeignKey(x => x.SubmissionId);
        builder.HasOne(x => x.Vehicle).WithMany().HasForeignKey(x => x.VehicleId);
    }
}
```

```csharp
[Authorize]
public class StickerController(AppDbContext db, IWorkflowService workflow)
    : SubmissionControllerBase(db, workflow)
{
    [Authorize(Roles = "SecurityAdmin")]
    public async Task<IActionResult> Review(int id) { /* ... */ }
}
```

Warisi `SubmissionControllerBase` — jangan tulis semula logik kelulusan setiap controller.

## Gaya kandungan latihan (bila menulis nota/lab)

Nota **Bahasa Melayu**, kod **Bahasa Inggeris**. Setiap lab: **Objektif** → langkah bernombor → blok kod penuh → **✅ Semakan**. Kod lengkap & boleh dijalankan.

## Bila tidak pasti

Nyatakan ketidakpastian dan tanya. Untuk data **profil pengguna**, baca kontrak profil sedia ada — jangan reka skema profil baharu.
