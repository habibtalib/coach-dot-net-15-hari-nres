# Hari 1 — Persediaan Projek & Seni Bina Kongsi

Nota ini mengikut **aturcara rasmi HARI 1** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 1 hingga SESI 4. Bahagian ini menerangkan **konsep** (kenapa sesuatu wujud); langkah hands-on penuh, bernombor, dengan kod untuk ditaip sendiri, ada di [`snippets/lab.md`](./snippets/lab.md).

Kursus: **DOTNET-NRES-15** — *Latihan Secara Coaching Pembangunan Sistem Onboarding & Khidmat Dalaman NRES Menggunakan ASP.NET Core*. Projek tunggal yang kita bina **kumulatif** sepanjang 15 hari: `Nres.Onboarding.Web`.

> **Konvensyen bahasa:** Nota & penerangan dalam **Bahasa Melayu**; semua kod, nama kelas/pembolehubah, nama fail, dan istilah teknikal (`Controller`, `DbContext`, `migration`) dikekalkan dalam **Bahasa Inggeris** — amalan standard industri .NET.

> **Cara guna nota ini:** Bahagian di bawah menerangkan **kenapa** setiap konsep wujud dan apa peranannya dalam sistem NRES. Latihan hands-on langkah demi langkah (dengan blok kod penuh untuk ditaip) ada di [`snippets/lab.md`](./snippets/lab.md). Baca bahagian yang sepadan di sini dahulu, kemudian pindah ke lab untuk cuba sendiri.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `dotnet new` templates | [learn.microsoft.com/dotnet/core/tools/dotnet-new](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new) |
| ASP.NET Core MVC — gambaran keseluruhan | [learn.microsoft.com/aspnet/core/mvc/overview](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview) |
| `Program.cs` & minimal hosting model | [learn.microsoft.com/aspnet/core/fundamentals/minimal-apis](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) |
| Dependency Injection dalam ASP.NET Core | [learn.microsoft.com/aspnet/core/fundamentals/dependency-injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) |
| EF Core — permulaan (Get Started) | [learn.microsoft.com/ef/core/get-started/overview/first-app](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app) |
| EF Core — penyedia SQLite | [learn.microsoft.com/ef/core/providers/sqlite](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/) |
| ASP.NET Core Identity | [learn.microsoft.com/aspnet/core/security/authentication/identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) |
| EF Core Migrations | [learn.microsoft.com/ef/core/managing-schema/migrations](https://learn.microsoft.com/en-us/ef/core/managing-schema/migrations/) |
| `dotnet ef` CLI | [learn.microsoft.com/ef/core/cli/dotnet](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) |
| EF Core — relationships (foreign key) | [learn.microsoft.com/ef/core/modeling/relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran Peserta & Minum Pagi |
| **9.00 – 10.30 pagi** | **SESI 1: Gambaran Sistem NRES** — 5 modul, corak `Form → Draft → Submit → Review → Approve → Audit`, kenapa satu `Submission` induk dikongsi. 🧠 Bengkel: peta medan sama merentas 5 modul |
| **10.30 – 1.00 tgh** | **SESI 2: Cipta Projek ASP.NET Core** — `dotnet new mvc`, struktur folder, `Program.cs`, pakej EF Core + Identity. 💻 Lab: projek berjalan + halaman utama |
| 1.00 – 2.30 petang | Rehat dan Makan Tengah Hari |
| **2.30 – 3.45 petang** | **SESI 3: Entiti Kongsi & DbContext** — `Submission`, `Attachment`, `AuditLog`, `UserProfile`, `SubmissionStatus`. 💻 Lab: tulis entiti + `ApplicationDbContext` |
| **3.45 – 5.00 petang** | **SESI 4: Migration Pertama** — `dotnet ef migrations add`, `dotnet ef database update`, sahkan skema SQLite. 💻 Lab: DB dicipta + navigasi modul placeholder |
| 5.00 petang | Bersurai |

**Hasil Hari 1** (rujuk [`../JADUAL.md`](../JADUAL.md)): Aplikasi ASP.NET Core berjalan, DB tersambung, migration pertama (`InitialShared`) wujud, peserta boleh terangkan kelima-lima modul.

---

## SESI 1 — Gambaran Sistem NRES: Bukan Sekadar Borang Digital

**Kenapa mula dengan gambaran keseluruhan, bukan terus menaip kod?** Ramai peserta baharu anggap sistem ini "borang HTML yang simpan ke DB". Itu tidak salah, tetapi tidak cukup. Sistem NRES sebenarnya ialah **request workflow system** — setiap permohonan melalui kitaran hayat yang sama, tidak kira jenis permohonan:

```text
Form → Validation → Draft → Submit → Review → Approve/Reject → Audit → Report
```

Lima modul yang akan kita bina sepanjang 15 hari:

1. **Modul Lapor Diri** — Hari 2–3 (kursus kita fokus di sini)
2. **Modul Pas, Parking & Pelekat Kenderaan** — Hari 4–6
3. **Modul ID, AD & Email** — Hari 7–9
4. **Modul PKS (Pematuhan Kod Setia)** — Hari 10–12
5. **Modul Aset ICT** — Hari 13–14

**Kenapa satu `Submission` induk dikongsi merentas 5 modul?** Kalau setiap modul reka status/aliran kerja sendiri (`LaporDiriStatus`, `PasStatus`, `IctStatus`, …), setiap laporan pengurusan, setiap dashboard, setiap logik kelulusan perlu ditulis **lima kali**. Dengan satu jadual `Submissions` yang menyimpan `ReferenceNo`, `ModuleCode`, `Status` (`SubmissionStatus` — sama untuk semua modul), `ApplicantUserId`, dan tarikh penting, setiap modul cuma tambah jadual **detail** sendiri (`OfficerReportingApplications`, `AccessPassApplications`, dsb.) yang berkongsi kunci asing (`SubmissionId`) ke jadual induk. Dashboard, carian rujukan global, dan panel audit (Hari 15) boleh ditulis **sekali** dan berfungsi untuk kelima-lima modul.

Ini corak seni bina **kongsi induk, khusus detail** (*shared header, specific detail*) — sangat lazim dalam sistem permohonan kerajaan/korporat berbilang jenis borang.

### Peta medan sama merentas 5 modul (bengkel)

Sebelum menaip kod, kenali pasti medan yang **berulang** dalam hampir semua borang NRES: nama pemohon, nombor rujukan, jabatan, status, tarikh hantar, lampiran sokongan, catatan kelulusan/penolakan. Medan-medan ini itulah yang menjadi `Submission` + `Attachment` + `AuditLog` — bukan sebab kebetulan, tetapi sebab ia **benar-benar sama** merentas Lapor Diri, Pas Keselamatan, Permohonan ID AD, PKS, dan Aset ICT.

> Rujukan rasmi: [learn.microsoft.com/aspnet/core/mvc/overview](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview)

---

## SESI 2 — Cipta Projek ASP.NET Core

**Kenapa `dotnet new mvc`, bukan `webapi` atau Razor Pages kosong?** SPEC-KURSUS.md menetapkan **ASP.NET Core MVC** sebagai rangka kerja muktamad kursus ini — corak `Controller` + `View` + `ViewModel` yang jelas memisahkan logik permohonan (controller), paparan (Razor view), dan bentuk data borang (view model). Corak ini juga paling biasa diguna pakai dalam sistem dalaman kerajaan/korporat Malaysia, memudahkan peserta membawa kemahiran ini ke tempat kerja.

Templat `dotnet new mvc` menjana struktur projek asas — `Controllers/`, `Views/`, `wwwroot/`, `Program.cs` — yang akan kita kembangkan mengikut struktur muktamad SPEC:

```text
Nres.Onboarding.Web/
  Controllers/
  Data/                 # ApplicationDbContext, seed
  Models/                # entiti (domain)
  ViewModels/
  Services/              # IReferenceNumberService, IFileStorageService, dll.
  Views/
  wwwroot/
  App_Data/uploads/      # fail dimuat naik (bukan bawah wwwroot)
```

**Kenapa `App_Data/uploads/` di luar `wwwroot/`?** Apa sahaja dalam `wwwroot/` **boleh dicapai terus** oleh pelayar web tanpa melalui kod kebenaran (authorization) kita. Fail lampiran sokongan (kad pengenalan, surat lantikan, dsb.) mengandungi maklumat sensitif — ia **mesti** disalurkan melalui satu `Controller` action yang menyemak kebenaran pengguna dahulu sebelum menghantar fail. Kita bina servis ini penuh di Hari 3.

### `Program.cs` — model hosting minimal

.NET 10 (seperti .NET 6 ke atas) guna **minimal hosting model**: satu fail `Program.cs`, tiada `Startup.cs` berasingan. `WebApplication.CreateBuilder(args)` sediakan segala infrastruktur asas (konfigurasi, logging, DI container); `builder.Services.Add...()` daftar servis; `app.Map...()`/`app.Use...()` bina *middleware pipeline* yang memproses setiap request masuk secara berurutan.

**Kenapa susunan middleware penting?** Setiap `app.Use...()` ialah satu lapisan yang request/response lalui — contohnya, `UseHttpsRedirection()` mesti berjalan **sebelum** `UseAuthorization()` supaya redirect HTTPS berlaku dahulu sebelum semakan kebenaran. Kita akan lihat susunan penuh dalam lab.

### Pakej NuGet yang diperlukan

| Pakej | Kegunaan |
|-------|----------|
| `Microsoft.EntityFrameworkCore` | Teras EF Core (ORM) |
| `Microsoft.EntityFrameworkCore.Design` | Sokongan `dotnet ef` CLI (migration) |
| `Microsoft.EntityFrameworkCore.Sqlite` | Penyedia (provider) SQLite — untuk latihan |
| `Microsoft.AspNetCore.Identity.EntityFrameworkCore` | ASP.NET Core Identity + storan EF Core |

**Kenapa SQLite untuk latihan?** Peserta boleh mula tanpa memasang pelayan pangkalan data berasingan (SQL Server/PostgreSQL) — fail `.db` tunggal sudah memadai. Menukar penyedia kepada SQL Server kelak (Hari 15) hanya memerlukan menukar `UseSqlite(...)` kepada `UseSqlServer(...)` dan connection string — kod entiti dan logik perniagaan **tidak berubah**. Ini menunjukkan salah satu kelebihan utama EF Core: abstraksi di atas pangkalan data sebenar.

> Rujukan rasmi: [learn.microsoft.com/aspnet/core/fundamentals/minimal-apis](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) · [learn.microsoft.com/ef/core/providers/sqlite](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/)

---

## SESI 3 — Entiti Kongsi & DbContext

**Kenapa `SubmissionStatus` satu enum tunggal, bukan status berasingan setiap modul?** Ini keputusan seni bina paling penting kursus ini. Dengan satu enum kongsi:

```csharp
public enum SubmissionStatus
{
    Draft = 0,
    Submitted = 1,
    SupervisorApproved = 2,
    AdminApproved = 3,
    Rejected = 4,
    Completed = 5,
    Cancelled = 6
}
```

...logik seperti "papar semua permohonan menunggu kelulusan saya" atau "kira berapa permohonan `Rejected` bulan ini" boleh ditulis **sekali** dan terpakai pada kelima-lima modul, tanpa `switch`/`if` berasingan bagi setiap jenis borang.

**Entiti kongsi** yang kita cipta hari ini — setiap satu memetakan terus kepada corak "peta medan sama" dari SESI 1:

- **`Submission`** — rekod induk setiap permohonan (tidak kira modul). Menyimpan `ReferenceNo`, `ModuleCode` (cth. `"LD"` untuk Lapor Diri), `ApplicantUserId`, `Status`, dan tarikh penting.
- **`Attachment`** — metadata fail yang dimuat naik (bukan kandungan fail — kandungan disimpan sebagai fail fizikal di `App_Data/uploads/`, lihat Hari 3).
- **`AuditLog`** — sejarah tindakan penting terhadap satu `Submission` (dicipta, dihantar, diluluskan, ditolak).
- **`UserProfile`** — maklumat profil staf (nama, jabatan, jawatan, gred) yang **berasingan** daripada `AspNetUsers` (jadual Identity).

**Kenapa `UserProfile` berasingan daripada `AspNetUsers`?** `AspNetUsers` (dijana oleh ASP.NET Core Identity) direka untuk **pengesahan (authentication)** — kata laluan (hash), emel log masuk, token keselamatan. Ia **bukan** tempat sesuai untuk medan perniagaan seperti jabatan atau gred — mencampur kedua-dua tanggungjawab ini ialah salah satu **silap biasa** yang disenaraikan dalam nota penceramah. `UserProfile` menyimpan maklumat perniagaan, dipautkan ke `AspNetUsers` melalui `UserId` (kunci asing rentetan, sepadan `IdentityUser.Id`).

### `ApplicationDbContext`

`DbContext` ialah "jambatan" antara kelas C# (entiti) dan jadual pangkalan data sebenar — setiap `DbSet<T>` mewakili satu jadual. Kerana kita guna ASP.NET Core Identity, `ApplicationDbContext` **mewarisi** `IdentityDbContext` (bukan `DbContext` kosong) supaya jadual Identity (`AspNetUsers`, `AspNetRoles`, dsb.) turut serta dalam skema yang sama.

> Rujukan rasmi: [learn.microsoft.com/ef/core/get-started/overview/first-app](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app) · [learn.microsoft.com/aspnet/core/security/authentication/identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) · [learn.microsoft.com/ef/core/modeling/relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)

---

## SESI 4 — Migration Pertama

**Kenapa migration, bukan cipta jadual manual dalam SQLite?** Migration ialah "sejarah berversi" bagi skema pangkalan data anda — setiap perubahan pada entiti C# (tambah medan, tambah jadual) dijana sebagai satu fail migration yang boleh **dijalankan semula** pada mana-mana persekitaran (makmal latihan, staging, pengeluaran) secara konsisten. Tanpa migration, setiap pembangun perlu ubah skema pangkalan data secara manual — mudah tersasar/tidak segerak antara satu sama lain.

Arus kerja EF Core Migration:

```bash
dotnet ef migrations add InitialShared
dotnet ef database update
```

- `migrations add InitialShared` — EF Core **bandingkan** model C# semasa dengan snapshot skema terakhir, jana kod migration (`Up()`/`Down()`) yang memetakan perbezaan itu kepada arahan SQL.
- `database update` — jalankan migration yang belum diguna pakai terhadap pangkalan data sebenar (fail SQLite `.db` akan dicipta jika belum wujud).

Kita namakan migration pertama **`InitialShared`** — nama ini sepadan dengan [`../JADUAL.md`](../JADUAL.md) ("Pemetaan Sesi → Deliverable": *`Nres.Onboarding.Web` berjalan + migration `InitialShared`") supaya jelas migration ini mengandungi **entiti kongsi sahaja**, bukan entiti khusus modul (yang akan datang bermula Hari 2).

> Rujukan rasmi: [learn.microsoft.com/ef/core/managing-schema/migrations](https://learn.microsoft.com/en-us/ef/core/managing-schema/migrations/) · [learn.microsoft.com/ef/core/cli/dotnet](https://learn.microsoft.com/en-us/ef/core/cli/dotnet)

---

## Seterusnya

Baca dan ikuti langkah demi langkah di [`snippets/lab.md`](./snippets/lab.md) — di situ anda akan:

1. Cipta projek `Nres.Onboarding.Web` dan sahkan ia berjalan.
2. Tambah pakej EF Core + Identity + SQLite.
3. Tulis kelas entiti `SubmissionStatus`, `Submission`, `Attachment`, `AuditLog`, `UserProfile`.
4. Tulis `ApplicationDbContext` dan daftarkannya dalam `Program.cs`.
5. Jana migration `InitialShared` dan cipta pangkalan data SQLite.
6. Tambah navigasi placeholder untuk 5 modul.

Nota penceramah (pemasaan sesi, silap biasa, soalan perbincangan): [`nota-penceramah.md`](./nota-penceramah.md).
