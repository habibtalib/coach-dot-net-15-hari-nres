# `projek/` — Projek Rujukan `Nres.Onboarding.Web`

> **Ini projek RUJUKAN, bukan projek untuk disalin.**
> Peserta membina versi sendiri langkah demi langkah mengikut lab setiap hari.
> Folder ini wujud untuk **dibanding** — bila lab anda tak jalan, buka fail yang sama di
> sini dan lihat apa yang berbeza. Menyalin keseluruhan folder ini bermakna anda
> melangkau bahagian pembelajaran yang paling penting.

Rujukan kanun: [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) · Aturcara: [`../JADUAL.md`](../JADUAL.md) · Kontrak pasukan: [`../KOLABORASI.md`](../KOLABORASI.md)

---

## Skop semasa — Fasa 1 (Hari 1–3) sahaja

Projek ini mengandungi **asas kongsi (shared foundation)** + **Modul Lapor Diri** sahaja.
Itulah keadaan aplikasi pada penghujung **Hari 3**, sebaik sebelum empat kumpulan bercabang.

| Bahagian | Status di sini |
|----------|----------------|
| Asas kongsi: `Submission`, `Attachment`, `AuditLog`, `ApprovalStep`, `UserProfile`, lookup | ✅ Siap |
| Servis kongsi: reference number, file storage, audit, **workflow**, notification, current user | ✅ Siap |
| Seni bina anti-konflik: modul mendaftar diri, `IEntityTypeConfiguration<T>`, `ModuleDescriptor` | ✅ Siap |
| ASP.NET Core Identity + 6 peranan + seed data | ✅ Siap |
| **Kumpulan 1 · Lapor Diri** (draf, sunting, lampiran, submit, semakan HR, lulus/tolak, audit) | ✅ Siap |
| Kumpulan 2 · Pas, Parkir & Pelekat | ❌ Tiada |
| Kumpulan 3 · ID, AD & Email | ❌ Tiada |
| Kumpulan 4 · Perisian & Aset ICT | ❌ Tiada |

**Modul Kumpulan 2–4 dibina oleh peserta melalui trek `kumpulan-*/hari-4` hingga `hari-13-14`.**
Ia sengaja **tidak** dimasukkan di sini — skop folder `projek/` kekal pada Fasa 1.

> **Modul PKS tiada langsung**, dan itu bukan terlepas pandang: ia di luar skop kursus
> (cadangan silibus NRES menetapkan 4 kumpulan sahaja). Draf lama ada di [`../_arkib/`](../_arkib/).

---

## Versi

`Nres.Onboarding.Web.csproj` mensasarkan **`net10.0`** dengan **EF Core 10** dan
**ASP.NET Core Identity 10** (`10.0.10`), sepadan dengan `SPEC-KURSUS.md`.

Bahasa ialah **C# 14** — lalai bagi .NET 10 SDK (Roslyn 5.0). Tiada `<LangVersion>`
ditetapkan kerana tidak perlu. Rujuk [`../nota/10-rujukan-buku.md`](../nota/10-rujukan-buku.md).

Anda memerlukan **.NET 10 SDK**:

```bash
dotnet --version   # patut papar 10.x
```

**Disahkan pada .NET SDK 10.0.302:** `dotnet build` bersih (0 ralat), migration dijana
semula dari kosong, dan `dotnet ef migrations add` semakan kewarasan menghasilkan
migration **kosong** — bermakna model dan snapshot konsisten.

---

## Cara jalankan

```bash
cd projek/Nres.Onboarding.Web

dotnet restore
dotnet ef database update      # cipta App_Data/nres.db
dotnet run
```

Buka URL yang dipapar dalam konsol.

Jika `dotnet ef` belum dipasang:

```bash
dotnet tool install --global dotnet-ef
```

> **Nota:** `dotnet ef database update` sebenarnya pilihan — aplikasi menjalankan
> `Database.MigrateAsync()` semasa startup (`Data/DbInitializer.cs`), jadi `dotnet run`
> sahaja pun mencipta pangkalan data. Langkah itu ditunjukkan supaya peserta biasa
> dengan aliran migration yang sebenar.

### Akaun demo (data seed)

| Emel | Kata laluan | Peranan |
|------|-------------|---------|
| `applicant@nres.demo` | `Password123!` | `Applicant` |
| `hradmin@nres.demo` | `Password123!` | `HrAdmin` |

Kata laluan demo ini **sengaja mudah** untuk latihan sahaja — jangan tiru corak ini
dalam sistem sebenar. (Kumpulan 3 membincangkan sebabnya secara panjang lebar.)

### Cuba aliran penuh

1. Log masuk sebagai `applicant@nres.demo`.
2. **Lapor Diri → Permohonan Baharu** — borang diisi separa daripada `UserProfile`.
3. Lampirkan satu dokumen (PDF/JPG/PNG/DOC/DOCX, maks 5 MB), klik **Simpan & Hantar**.
   Nombor rujukan `LD-2026-0001` dijana; notifikasi dipaparkan dalam konsol.
4. Log keluar, log masuk sebagai `hradmin@nres.demo`.
5. **Semakan HR** → buka permohonan → **Luluskan** atau **Tolak** (sebab wajib).
6. Kembali ke butiran permohonan untuk melihat **jejak audit** penuh.

---

## Struktur

```text
Nres.Onboarding.Web/
  Controllers/          HomeController, AccountController, OfficerReportingController
  Data/                 ApplicationDbContext (BEKU), DbInitializer, design-time factory
  Migrations/           InitialShared
  Models/
    Shared/             Entiti kongsi + Configurations/  ← BEKU selepas Hari 3
    LaporDiri/          Entiti Kumpulan 1 + Configurations/ + ModuleDescriptor
  ViewModels/
    LaporDiri/          Model borang Kumpulan 1
  Services/             Servis kongsi (workflow, audit, storage, ...)
    LaporDiri/          LaporDiriModule.cs — pendaftaran servis Kumpulan 1
  ViewComponents/       ModuleNavViewComponent
  Views/
    Shared/             _Layout (BEKU), _StatusBadge, Components/ModuleNav/
    OfficerReporting/   View Kumpulan 1
  App_Data/uploads/     Fail dimuat naik — DI LUAR wwwroot
```

---

## Seni bina anti-konflik — sebab utama projek ini wujud

Empat kumpulan menulis kod serentak dalam satu repositori selama 11 hari. Tiga fail
yang biasanya menyebabkan 90% konflik telah **direka supaya tidak perlu disentuh**.
Inilah yang patut anda perhatikan dalam kod:

| Fail | Corak | Apa yang kumpulan buat |
|------|-------|------------------------|
| `Program.cs` | `AddLaporDiriModule()` + 3 baris berkomen | Nyahkomen **satu** baris, sekali, Hari 4 |
| `Data/ApplicationDbContext.cs` | `ApplyConfigurationsFromAssembly()` | Tambah `IEntityTypeConfiguration<T>` dalam folder sendiri |
| `Views/Shared/_Layout.cshtml` | `Component.InvokeAsync("ModuleNav")` | Tambah `IModuleDescriptorProvider` dalam folder sendiri |

Buka ketiga-tiga fail itu. Setiap satu ada komen **⚠️ BEKU** yang menerangkan sebabnya.

> `ApplicationDbContext.OnModelCreating` mengandungi **tepat dua baris** selepas
> `base` — itulah keseluruhan reka bentuknya. Rujuk [`../KOLABORASI.md`](../KOLABORASI.md) §3.

---

## Keputusan reka bentuk yang patut anda perhatikan

- **`Submission` ialah induk kongsi.** Setiap modul mempunyai jadual butiran sendiri
  (di sini `OfficerReportingApplications`) yang menunjuk kembali ke `Submissions`.
  Satu enum status, satu jadual audit, satu jadual lampiran untuk semua modul.
- **`IWorkflowService` memiliki peraturan peralihan status** dan menulis audit secara
  atomik dengan perubahan. Status **tidak** ditukar terus dalam controller — itu cara
  empat modul berakhir dengan empat set peraturan yang berbeza sedikit.
- **`AuditLog` merekod `FromStatus`/`ToStatus`**, bukan hanya nama tindakan — supaya
  jejak audit boleh dibaca tanpa membaca kod.
- **`UserProfile` berasingan daripada `AspNetUsers`.** Jadual Identity kekal untuk
  authentication sahaja. Ini salah satu kesilapan paling biasa yang kursus ini betulkan.
- **View model, bukan entiti, dalam borang.** Bind terus ke entiti membuka *over-posting*.
- **Fail di luar `wwwroot`.** Nama fail di cakera ialah GUID; nama asal metadata sahaja.
  Muat turun melalui action yang menyemak kebenaran dahulu — bukan pautan statik.
- **Validation submit lebih ketat daripada validation draf.** Draf boleh separuh siap.
- **Penolakan wajib bersebab** — disahkan di **pelayan**, bukan sekadar `required` HTML.
- **Indeks unik ditapis** pada `Submission.ReferenceNo` (`WHERE ReferenceNo <> ''`) —
  supaya banyak draf boleh berkongsi rujukan kosong sementara nombor yang dikeluarkan
  kekal unik.
- **`ApprovalStep` sudah generik** walaupun Lapor Diri hanya guna satu langkah — Kumpulan 3
  menggunakan `StepOrder` sepenuhnya untuk kelulusan dua peringkat.

---

## Yang sengaja belum ada

Supaya jangkaan jelas — perkara berikut memang tiada pada penghujung Hari 3:

| Tiada di sini | Dibina bila |
|---------------|-------------|
| Carian / penapis lanjutan, print view, CSV/Excel export | Trek kumpulan, blok Hari 7–9 & 10–12 |
| Notifikasi e-mel sebenar (`ConsoleNotificationService` sahaja) | Kumpulan 1, Hari 10–12 (melalui isu `shared`) |
| Projek ujian `Nres.Onboarding.Tests` (xUnit) | Trek kumpulan, blok Hari 13–14 |
| Papan Pemuka Induk NRES | Hari 15 |
| Pendaftaran pengguna sendiri | Tidak akan ada — dalam sistem dalaman kerajaan akaun dicipta oleh `SystemAdmin` |

> **Nota jujur tentang `OfficerReportingController`:** controller ini ditulis sebelum
> `SubmissionControllerBase` wujud, jadi ia **tidak** mewarisinya — ia melaksanakan
> approve/reject sendiri. Kelas asas, `IWorkflowService`, dan partial kongsi **memang
> wujud** dan sedia untuk Kumpulan 2–4.
>
> Ini sebenarnya titik pengajaran yang berguna: tunjukkan kepada peserta kedua-dua versi
> dan tanya *"mana satu yang empat kumpulan patut ikut, dan kenapa?"* Jawapannya ialah
> kelas asas — dan `OfficerReportingController` ialah contoh kod yang **patut** direfactor.
