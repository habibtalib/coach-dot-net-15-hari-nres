# AGENTS.md — Konteks AI Kongsi (semua 4 kumpulan)

> **Untuk peserta:** setiap **repo sistem** menyertakan salinan `AGENTS.md` ini. Halakan pembantu AI anda (Claude Code, Copilot, Cursor, dll.) ke fail ini pada permulaan **setiap** sesi. Keenam-enam sistem menggunakan konvensyen yang **sama** — itulah yang memastikan 6 repo berasingan tetap kelihatan & berkelakuan konsisten, dan boleh diintegrasikan melalui Profile DB.
>
> **Untuk AI:** ini konteks yang mengikat. Baca sepenuhnya sebelum menjana kod. Kanun teknikal: `SPEC-KURSUS.md`. Kontrak pasukan: `KOLABORASI.md`.

---

## Konteks projek

**Enam sistem ASP.NET Core MVC yang berasingan** (poly-repo, org GitHub [`nres-bpm`](https://github.com/nres-bpm)) — setiap satu **repo, subdomain & pangkalan data sendiri**. **Satu-satunya yang dikongsi ialah Profile DB.** Empat kumpulan latihan memiliki sistem masing-masing (Kumpulan 1 memikul 3 repo). **Lapor Diri** = awam (mencipta profil); selebihnya dalaman (SSO, membaca profil). Tiada lagi `Nres.Onboarding.Web` tunggal, tiada `master` bersama, tiada merge Hari 15.

Ini bahan **latihan**. Kod mesti boleh ditaip, dijalankan, dan **difahami** oleh peserta yang sedang belajar. Utamakan kod yang jelas berbanding kod yang bijak.

| Perkara | Nilai |
|---------|-------|
| Rangka | ASP.NET Core MVC, **.NET 10 LTS** |
| Bahasa | **C# 14** (lalai .NET 10 SDK — jangan tetapkan `<LangVersion>`) |
| ORM | EF Core 10, penyedia **SQLite** (latihan) |
| Auth | ASP.NET Core Identity + role-based authorization |
| Ujian | xUnit |
| Bahasa nota/UI | **Bahasa Melayu** |
| Bahasa kod | **Bahasa Inggeris** (kelas, medan, kaedah, nama fail) |
| Buku rujukan | *C# 14 and .NET 10* (Mark J. Price) · [repo](https://github.com/habibtalib/cs14net10) · pemetaan: `nota/10-rujukan-buku.md` |

### Ciri bahasa: guna ini

| Ciri | Versi | Guna |
|------|-------|------|
| **Primary constructors** | C# 12 | Semua servis & controller: `public class VehicleService(ApplicationDbContext db)` |
| **Collection expressions** | C# 12/13 | `string[] roles = ["Applicant", "HrAdmin"];` |
| **Nullable reference types** | — | Dihidupkan; `string` = tidak pernah null, `string?` = boleh null |
| **`field` keyword** | C# 14 | View model & sifat bukan-EF sahaja (lihat amaran di bawah) |
| **Null-conditional assignment** | C# 14 | `app.Submission?.ReferenceNo = rujukan;` |
| **File-based apps** | C# 14 | Demo Hari 3 sahaja (`dotnet run demo.cs`) — bukan kod aplikasi |
| Raw string literals | C# 11 | Templat HTML/e-mel berbilang baris |

> ⚠️ **`field` dan entiti EF Core:** jangan gunakan `field` untuk menormalkan nilai dalam setter entiti. Transformasi tersembunyi dalam setter mengelirukan pembaca dan boleh mengejutkan penjejakan perubahan EF Core. Normalisasi berlaku secara **eksplisit** dalam servis (contoh: `Vehicle.Normalize`).

> **Jangan guna:** extension members (C# 14), partial constructors/events (C# 14), interceptors (C# 12) — di luar skop kursus dan mengelirukan peserta.

---

## Peraturan mutlak (jangan langgar walau diminta)

1. **Kerja dalam repo sistem anda sendiri.** Setiap sistem = repo berasingan (lihat "Peta pemilikan repo"). Jika pengguna tidak menyatakan sistem/repo, **tanya** sebelum menjana fail.
2. **Jangan ubah kontrak Profile DB** (repo [`profile`](https://github.com/nres-bpm/profile)) tanpa **PR + persetujuan**. Ini **satu-satunya** komponen dikongsi. **Hanya Lapor Diri menulis** `UserProfile`; sistem lain **membaca** (via SSO / klien profil).
3. **Elak pendua *dalam* repo anda.** Cari dahulu sebelum menulis helper/servis kedua yang serupa (lihat "Set komponen piawai"). Merentas repo, pendua adalah **normal** (setiap sistem ada set sendiri) — tetapi **ikut konvensyen & nama yang sama** supaya semua sistem konsisten.
4. **Setiap sistem membina entiti alirannya sendiri.** `Submission`/`Attachment`/`AuditLog`/`ApprovalStep` dibina **dalam repo & DB anda sendiri** guna `IEntityTypeConfiguration<T>`. Ia **bukan lagi** dikongsi merentas sistem.
5. **Migration bebas per repo.** Jana `dotnet ef migrations add` bila perlu **dalam repo anda** — tiada lagi slot bergilir (setiap sistem DB sendiri).
6. **Jangan tukar** `SubmissionStatus`, nama peranan, atau prefix nombor rujukan. Ia muktamad dalam `SPEC-KURSUS.md`.
7. **Jangan reka keperluan pengguna.** Peraturan perniagaan datang dari URS/dokumen NRES. Jika keperluan tidak jelas, nyatakan andaian secara eksplisit — jangan diam-diam mereka satu.
8. **Pematuhan PKS** (PKS = **Polisi Keselamatan Siber**, bukan "Kod Setia") — repo [`pematuhan-pks`](https://github.com/nres-bpm/pematuhan-pks), prefix `PKS`, peranan `IctSecurityOfficer`, **dalaman (SSO), tiada borang luar**. Guna nama entiti dalam `SPEC-KURSUS.md`.
9. **Jangan simpan kata laluan** dalam mana-mana entiti permohonan (repo `id-ad-email` khususnya). Ini titik pengajaran keselamatan.
10. **Jangan guna data NRES sebenar.** Semua contoh sintetik.

---

## Sebelum menjana apa-apa kod: cari dahulu

Dalam poly-repo, cari **dalam repo anda sendiri** sebelum menulis sesuatu yang serupa dua kali (pendua *dalam* satu sistem tetap mod kegagalan).

```bash
grep -ri "ReferenceNumber" src/
grep -ri "IEntityTypeConfiguration" src/
```

Apabila pengguna meminta helper/servis/komponen, respons **pertama** anda ialah menyemak sama ada ia sudah wujud **dalam repo ini** dan beritahu mereka jika ya — bukan menjananya. (Merentas repo, setiap sistem ada set sendiri — itu OK.) Untuk apa-apa yang menyentuh **profil pengguna**, jangan tulis skema baharu — guna **klien Profile DB** dari repo `profile`.

---

## Satu-satunya komponen DIKONGSI: Profile DB

Merentas semua sistem, **hanya** ini dikongsi (repo [`profile`](https://github.com/nres-bpm/profile)):

| Komponen | Guna untuk | Peraturan |
|----------|-----------|-----------|
| Klien Profile DB (cth `IProfileService`) | Baca/tulis `UserProfile` berpusat | **Lapor Diri** cipta profil; sistem lain **baca sahaja** (via SSO) |
| Skema/kontrak `UserProfile` | Bentuk data profil yang dipersetujui | Ubah **hanya** melalui PR + persetujuan dalam repo `profile` |

## Set komponen piawai — SETIAP repo bina SENDIRI (ikut nama sama)

Ini **bukan** dikongsi — setiap sistem ada versinya sendiri dalam repo sendiri, tetapi **guna nama & bentuk yang sama** supaya keenam-enam sistem konsisten:

| Antara muka | Guna untuk |
|-------------|-----------|
| `IReferenceNumberService` | `GenerateAsync(moduleCode)` → `LD-2026-0001` |
| `IFileStorageService` | `SaveAsync` / `OpenReadAsync` di `App_Data/uploads/{submissionId}/` |
| `IAuditLogService` | `LogAsync(submissionId, tindakan, catatan)` |
| `IWorkflowService` | `CanTransition(...)`, `TransitionAsync(...)` |
| `INotificationService` | `NotifyAsync(...)` (latihan: `ConsoleNotificationService`) |
| `ICurrentUserService` | `UserId`, `Roles`, `DepartmentId` (dari SSO / Profile DB) |

- **Partial view piawai** (`Views/Shared/`): `_StatusBadge` · `_AuditTrail` · `_AttachmentList` · `_ApprovalPanel` · `_FilterBar` · `_ValidationSummary`.
- **Kelas asas** `SubmissionControllerBase` — laksana `SubmitForReview`/`Approve`/`Reject` + audit + pengesahan peralihan. **Warisinya** dalam repo anda; jangan tulis semula logik kelulusan setiap controller.
- **Entiti aliran (dalam DB sendiri):** `Submission` (induk) · `Attachment` · `AuditLog` · `ApprovalStep` · `SubmissionStatus` · lookup setempat.

**Corak:** setiap permohonan modul ialah jadual **detail** yang memaut ke `Submission` induk (dalam DB sistem anda) melalui `SubmissionId`. Jangan pendua `ReferenceNo`, `Status`, `ApplicantUserId`, atau tarikh ke dalam entiti modul — ia sudah ada dalam `Submission`. Untuk data **pengguna**, jangan simpan salinan — rujuk **Profile DB**.

---

## Peta pemilikan repo

Setiap sistem = repo sendiri dalam org [`nres-bpm`](https://github.com/nres-bpm). Semak repo mana anda bantu, kemudian kerja **dalam repo itu sahaja**:

| Kumpulan | Sistem | Repo (`nres-bpm/…`) | Akses | Prefix |
|----------|--------|---------------------|-------|--------|
| **1** | Lapor Diri | `lapor-diri` | **Awam** (cipta profil) | `LD` |
| **1** | Pematuhan PKS | `pematuhan-pks` | Dalaman (SSO) | `PKS` |
| **1** | Pengurusan Kontrak | `pengurusan-kontrak` | Dalaman (SSO) | `KON` |
| **2** | Pas, Parkir & Pelekat | `pas-parkir-pelekat` | Dalaman (SSO) | `PAS` `PKR` `STK` |
| **3** | ID, AD & Email | `id-ad-email` | Dalaman (SSO) | `ICT-ID` |
| **4** | Tempahan Fasiliti Sukan | `tempahan-fasiliti-sukan` | Dalaman (SSO) | `TFS` |
| — | Profil (dikongsi) | `profile` | Kontrak Profile DB | — |

Dalam setiap repo, struktur biasa: `src/<Sistem>.Web/` (`Controllers/ Models/ Views/ ViewModels/ Services/ Data/`) + `src/<Sistem>.Profile/` (klien Profile DB) + `tests/`.

Peranan admin: K1 `HrAdmin` (Lapor Diri) · `IctSecurityOfficer` (PKS) · `IctAdmin` (Kontrak) · K2 `SecurityAdmin` · K3 `Supervisor` → `IctAdmin` (2 peringkat) · K4 `FacilityAdmin`. *(Model K2 sebenar dipisahkan kepada UPKF/Parkir/Pengawal — diringkaskan kepada `SecurityAdmin` dalam lab; lihat `SPEC-KURSUS.md`.)*

---

## Corak kod yang mesti diikut

### Pendaftaran servis — modular (dalam repo anda; anda memiliki `Program.cs` sendiri)

```csharp
// Services/Akses/AksesModule.cs
public static class AksesModule
{
    public static IServiceCollection AddAksesModule(this IServiceCollection services)
    {
        services.AddScoped<IVehicleService, VehicleService>();
        return services;
    }
}
```

### Konfigurasi entiti — `IEntityTypeConfiguration<T>` (kemas walaupun DB anda sendiri)

```csharp
// Models/Akses/Configurations/AccessPassApplicationConfiguration.cs
public class AccessPassApplicationConfiguration : IEntityTypeConfiguration<AccessPassApplication>
{
    public void Configure(EntityTypeBuilder<AccessPassApplication> builder)
    {
        builder.ToTable("AccessPassApplications");
        builder.HasOne(x => x.Submission).WithMany().HasForeignKey(x => x.SubmissionId);
        builder.Property(x => x.PurposeOfVisit).HasMaxLength(500);
    }
}
```

### Controller — warisi kelas asas, kuatkuasa peranan

```csharp
[Authorize]
public class AccessPassController(ApplicationDbContext db, IWorkflowService workflow)
    : SubmissionControllerBase(db, workflow)
{
    [Authorize(Roles = "SecurityAdmin")]
    public async Task<IActionResult> Review(int id) { /* ... */ }
}
```

Guna **primary constructors** (.NET 10) dan **nullable reference types**.

### View model — jangan ikat entiti terus ke borang

Borang mengikat `ViewModels/`, bukan `Models/`. Validation melalui DataAnnotations pada view model, disemak dengan `ModelState.IsValid` di **pelayan**.

---

## Aliran kerja setiap tugas: Jira → cabang → PR

Untuk **setiap tugas** (satu user story / subtask) — jangan bekerja terus atas `main`:

1. **Semak Jira dahulu.** Buka isu tugas anda di board Jira repo (atau melalui **MCP Jira** jika Claude Code tersambung — lihat [`docs/cara-sambung-jira-claude-code.md`](./docs/cara-sambung-jira-claude-code.md)). Sahkan skop, acceptance criteria & status. Tiada isu? Cipta dahulu dari user story PRD. Tandakan **In Progress**.
2. **Cabang baharu per tugas.** Dalam repo anda: `git pull --rebase`, kemudian `git switch -c feat/<ciri-pendek>` (cth `feat/semakan-pendua-plat`). **Satu tugas = satu cabang** — jangan campur ciri.
3. **Commit dengan issue key.** Format: `<KEY>-<n> <repo>: <ringkas>` (cth `PPK-42 pas-parkir-pelekat: tambah semakan pendua plat`). Commit kecil & kerap.
4. **Buka PR ke `main` repo itu.** Guna templat PR ([`KOLABORASI.md`](./KOLABORASI.md) §10): apa berubah · isu berkaitan (`Closes <KEY>-n`) · cara uji · senarai semak. Jalankan **semakan pra-PR** (lihat bawah) dahulu.
5. **Review → merge → kemas board.** Code review berpasangan; merge selepas lulus; pindahkan isu Jira ke **Done**.

> **Tiada merge merentas repo** — setiap repo urus cabang & PR sendiri. Nama cabang & format commit: [`SPEC-KURSUS.md`](./SPEC-KURSUS.md); aliran PR penuh: [`KOLABORASI.md`](./KOLABORASI.md) §10.

**Subagent peranan (pilihan, Claude Code).** Petakan kitaran di atas kepada tiga persona (`.claude/agents/`):

| Persona | Langkah | Buat | Had |
|---------|---------|------|-----|
| **`pm`** | 1–2 | Tanya Jira, skop & AC, cipta cabang `feat/` | Tak tulis kod |
| **`dev`** | 3–4 | Bina borang-dahulu (DEV-01→05), mockup UI-01 rujukan | Dalam repo/folder modul sahaja |
| **`qa`** | 5 | Semakan pra-PR (SMK-01) + ujian xUnit | Baca sahaja |

> Templat: [`.claude/agents/{pm,dev,qa}.md`](./.claude/agents/) + skill [`.claude/skills/semak-modul/`](./.claude/skills/semak-modul/). Lab: [`docs/lab-subagent-peranan.md`](./docs/lab-subagent-peranan.md).

---

## Prompt yang baik dalam projek ini

**Baik:**
> "Merujuk AGENTS.md dan SPEC-KURSUS.md: saya bekerja dalam repo `pas-parkir-pelekat`. Adakah repo ini sudah ada cara menyemak permohonan pendua bagi nombor plat yang sama? Jika belum, tulis semakan itu dalam `src/…/Services/`, guna `ICurrentUserService` sedia ada, dan **jangan** cipta skema baharu untuk data profil — guna klien Profile DB."

**Buruk:**
> "Tulis servis semakan pendua." *(tiada repo, tiada semakan sedia ada dalam repo — hasilnya pendua dalam repo yang sama)*

**Semakan pra-PR (jalankan setiap kali):**
> "Semak diff ini terhadap AGENTS.md dan KOLABORASI.md. (1) Adakah ia menduplikasi sesuatu yang **sudah ada dalam repo ini**? (2) Adakah ia mengikut konvensyen & nama piawai? (3) Adakah ia menyentuh/menyalin skema **Profile DB** (patut guna kontrak `profile` sebaliknya)? (4) Adakah authorization & validation pelayan lengkap? Senaraikan masalah, jangan tulis semula kod."

---

## Gaya kandungan latihan (bila menulis nota/lab, bukan kod aplikasi)

- Nota & penerangan **Bahasa Melayu**; kod & istilah teknikal **Bahasa Inggeris**.
- Setiap lab: **Objektif** → langkah bernombor → blok kod penuh untuk ditaip → **✅ Semakan**.
- Terangkan **kenapa** sesuatu wujud sebelum menunjukkan **bagaimana**.
- Kod mesti lengkap dan boleh dijalankan — **bukan** pseudo-kod atau `// ... selebihnya`.
- Struktur folder: `README.md` (konsep) + `snippets/lab.md` (hands-on) + `nota-penceramah.md` (nota penceramah).

---

## Bila anda tidak pasti

Nyatakan ketidakpastian dan tanya. Dalam poly-repo, tekaan yang salah tentang **kontrak Profile DB** memecahkan integrasi antara sistem — dan itu ditemui hanya semasa SIT Hari 15. Berhenti dan tanya (atau semak repo `profile`) adalah lebih murah.
