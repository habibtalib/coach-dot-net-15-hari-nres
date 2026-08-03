# KOLABORASI.md — Kontrak Kerja Pasukan

> Mengikat **semua empat kumpulan** sepanjang Hari 4–14. Kanun teknikal: [`SPEC-KURSUS.md`](./SPEC-KURSUS.md). Konteks AI kongsi: [`AGENTS.md`](./AGENTS.md).
>
> Dokumen ini menjawab satu soalan: **bagaimana 4 pasukan menulis kod serentak, semuanya dibantu AI, dan tetap bergabung bersih pada Hari 15 tanpa kod berulang?**

---

## 1. Kenapa dokumen ini wujud

Model 4 kumpulan selari mempunyai dua mod kegagalan yang boleh diramal:

**Kegagalan A — konflik gabungan (merge conflict).** Empat kumpulan mengedit fail yang sama (`Program.cs`, `ApplicationDbContext.cs`, `_Layout.cshtml`, snapshot migration). Menjelang Hari 15, `git merge` menghasilkan ratusan baris konflik dan hari integrasi habis untuk membaiki, bukan mendemo.

**Kegagalan B — kod & proses berulang (redundan).** Setiap kumpulan menulis servis nombor rujukannya sendiri, panel kelulusannya sendiri, cara auditnya sendiri. Aplikasi akhir mempunyai empat cara melakukan perkara yang sama — mimpi ngeri penyelenggaraan, dan tepat apa yang sistem dalaman kerajaan sebenar sering alami.

**AI memburukkan kedua-duanya.** Pembantu AI hanya nampak apa yang ada dalam konteksnya. Minta ia "tulis servis jana nombor rujukan" dan ia akan menulis satu — walaupun `IReferenceNumberService` sudah wujud sejak Hari 3. Empat kumpulan × pembantu AI tanpa konteks kongsi = empat penyelesaian tidak serasi, ditulis dengan pantas dan yakin.

Penyelesaiannya **bukan** "jangan guna AI" dan **bukan** "berhati-hati". Ia seni bina + kontrak yang jelas. Itulah dokumen ini.

---

## 2. Matriks pemilikan fail

Setiap laluan fail ada **tepat satu** pemilik. Jika anda bukan pemiliknya, anda tidak menyuntingnya — anda buka isu.

| Laluan | Pemilik | Boleh sunting? |
|--------|---------|----------------|
| `Models/Shared/**`, `Services/*.cs` (antara muka kongsi), `Data/ApplicationDbContext.cs`, `Program.cs`, `Views/Shared/_Layout.cshtml`, `wwwroot/css/site.css` | **Jurulatih** (dibina Hari 3) | ❌ **Beku selepas Hari 3** |
| `Models/LaporDiri/**`, `Controllers/OfficerReporting*`, `Views/OfficerReporting/**`, `ViewModels/LaporDiri/**`, `Services/LaporDiri/**` | Kumpulan 1 · Lapor Diri | ✅ Kumpulan 1 sahaja |
| `Models/Pks/**`, `Controllers/Compliance*`, `Views/Compliance/**`, `ViewModels/Pks/**`, `Services/Pks/**` | Kumpulan 1 · Pematuhan PKS | ✅ Kumpulan 1 sahaja |
| `Models/Kontrak/**`, `Controllers/Contract*`, `Views/Contract/**`, `ViewModels/Kontrak/**`, `Services/Kontrak/**` | Kumpulan 1 · Pengurusan Kontrak | ✅ Kumpulan 1 sahaja |
| `Models/Akses/**`, `Controllers/AccessPass*`, `Controllers/Parking*`, `Controllers/VehicleSticker*`, `Views/Akses/**`, `ViewModels/Akses/**`, `Services/Akses/**` | Kumpulan 2 | ✅ Kumpulan 2 sahaja |
| `Models/Akaun/**`, `Controllers/AccountRequest*`, `Views/Akaun/**`, `ViewModels/Akaun/**`, `Services/Akaun/**` | Kumpulan 3 | ✅ Kumpulan 3 sahaja |
| `Models/Fasiliti/**`, `Controllers/FacilityBooking*`, `Views/FacilityBooking/**`, `ViewModels/Fasiliti/**`, `Services/Fasiliti/**` | Kumpulan 4 | ✅ Kumpulan 4 sahaja |
| `Migrations/**` | **Bergilir** | ⚠️ Ikut slot migration (§5) |

> **Ujian ringkas sebelum commit:** `git diff --name-only master` — jika ada fail di luar folder kumpulan anda, berhenti dan baca §4.

---

## 3. Seni bina anti-konflik: modul mendaftar diri, fail kongsi tidak disunting

Tiga fail yang biasanya menjadi punca 90% konflik telah **direka supaya tidak perlu disentuh**. Ini dibina bersama pada Hari 3.

### 3.1 `Program.cs` — satu baris per modul, ditulis sekali

Daripada empat kumpulan menambah pendaftaran servis ke dalam `Program.cs`, setiap modul menyediakan **kaedah sambungan (extension method) sendiri dalam failnya sendiri**:

```csharp
// Services/LaporDiri/LaporDiriModule.cs — MILIK KUMPULAN 1 SAHAJA
namespace Nres.Onboarding.Web.Services.LaporDiri;

public static class LaporDiriModule
{
    public static IServiceCollection AddLaporDiriModule(this IServiceCollection services)
    {
        services.AddScoped<IOfficerReportingService, OfficerReportingService>();
        services.AddScoped<ISlipAkuanService, SlipAkuanService>();
        return services;
    }
}
```

`Program.cs` mengandungi empat baris ini sejak Hari 3 dan **tidak berubah lagi**:

```csharp
builder.Services.AddLaporDiriModule();      // Kumpulan 1 · Lapor Diri
builder.Services.AddPksModule();            // Kumpulan 1 · Pematuhan PKS
builder.Services.AddKontrakModule();        // Kumpulan 1 · Pengurusan Kontrak
builder.Services.AddAksesModule();          // Kumpulan 2
builder.Services.AddAkaunModule();          // Kumpulan 3
builder.Services.AddFasilitiModule();       // Kumpulan 4 · Tempahan Fasiliti Sukan
```

Setiap kumpulan menambah servis **dalam failnya sendiri**. Sifar konflik.

### 3.2 `ApplicationDbContext` — tiada `DbSet` ditambah manual

Ini punca konflik paling kerap dalam projek EF Core berbilang pasukan. Penyelesaiannya: **jangan sentuh `DbContext`**. Setiap entiti membawa konfigurasinya sendiri:

```csharp
// Models/Akses/Configurations/VehicleConfiguration.cs — MILIK KUMPULAN 2 SAHAJA
public class VehicleConfiguration : IEntityTypeConfiguration<Vehicle>
{
    public void Configure(EntityTypeBuilder<Vehicle> builder)
    {
        builder.ToTable("Vehicles");
        builder.HasIndex(v => v.PlateNumber);
        builder.Property(v => v.PlateNumber).HasMaxLength(20).IsRequired();
    }
}
```

`ApplicationDbContext` mengandungi **satu** baris yang menemui kesemuanya secara automatik, ditulis sekali pada Hari 3:

```csharp
protected override void OnModelCreating(ModelBuilder modelBuilder)
{
    base.OnModelCreating(modelBuilder);
    modelBuilder.ApplyConfigurationsFromAssembly(typeof(ApplicationDbContext).Assembly);
}
```

Akses melalui `context.Set<Vehicle>()` atau `DbSet` dalam kelas modul — bukan dengan menambah sifat baharu ke `ApplicationDbContext`.

### 3.3 Navigasi — didorong data, bukan HTML yang disunting

Daripada empat kumpulan menyunting `_Layout.cshtml`:

```csharp
// Models/Shared/ModuleDescriptor.cs — dibina Hari 3
public record ModuleDescriptor(string Code, string Nama, string Controller, string Ikon, string[] Roles, int Urutan);
```

```csharp
// Models/Akaun/AkaunModuleDescriptor.cs — MILIK KUMPULAN 3 SAHAJA
public class AkaunModuleDescriptor : IModuleDescriptorProvider
{
    public ModuleDescriptor Describe() =>
        new("ICT-ID", "ID, AD & Email", "AccountRequest", "bi-person-badge",
            ["Applicant", "Supervisor", "IctAdmin"], Urutan: 3);
}
```

View component kongsi mengumpul semua `IModuleDescriptorProvider` dan menjana menu mengikut peranan pengguna. Setiap kumpulan menambah **satu fail baharu**; tiada siapa menyunting layout.

### 3.4 CSS & JavaScript

`wwwroot/css/site.css` **beku**. Setiap kumpulan guna failnya sendiri: `wwwroot/css/modul-akses.css`, dimuat melalui seksyen `@section Styles` dalam view kumpulan itu sahaja.

---

## 4. Bila anda perlu sesuatu yang bukan milik anda

Jangan sunting. Jangan salin-tampal ke folder anda. **Buka isu.**

1. Buka isu dalam board, label **`shared`**, judul: `[shared] <apa yang diperlukan> — Kumpulan N`.
2. Nyatakan: apa yang diperlukan, kenapa modul anda perlukannya, dan **modul lain mana** yang mungkin perlukannya juga.
3. Jurulatih memutuskan dalam stand-up berikutnya:
   - **Sudah wujud** → anda diberitahu di mana. (Paling kerap berlaku.)
   - **Kongsi benar** → jurulatih membinanya dalam `Shared/`, keempat-empat kumpulan guna.
   - **Khusus modul anda** → binanya dalam folder anda.

> **Jangan sekali-kali** salin kelas dari folder kumpulan lain ke folder anda "supaya tidak mengganggu mereka". Itu tepat cara Kegagalan B berlaku, dan ia kelihatan tidak bersalah ketika anda melakukannya.

---

## 5. Slot migration — peraturan tegas

EF Core menyimpan `Migrations/ApplicationDbContextModelSnapshot.cs` — **satu fail, seluruh skema**. Dua kumpulan yang menjana migration serentak akan berkonflik pada fail ini, dan konflik snapshot **tidak boleh diselesaikan dengan tangan** secara selamat.

**Protokol:**

1. Umumkan dalam saluran kelas / board: *"Kumpulan 2 mengambil slot migration"*.
2. `git pull --rebase origin master`
3. `dotnet ef migrations add <NamaBermakna>` — awalan nama dengan modul: `AksesVehicleDanPas`, `LaporDiriAttachment`.
4. `dotnet ef database update` dan sahkan aplikasi berjalan.
5. Commit, push, lepaskan slot: *"Kumpulan 2 selesai slot migration"*.

**Jika anda tersilap berkonflik pada snapshot:**

```bash
# JANGAN cuba baiki konflik snapshot dengan tangan.
git checkout --theirs Migrations/ApplicationDbContextModelSnapshot.cs
rm Migrations/<migration_anda_yang_belum_digabung>.cs
rm Migrations/<migration_anda_yang_belum_digabung>.Designer.cs
git pull --rebase origin master
dotnet ef migrations add <NamaBermakna>   # jana semula di atas snapshot terkini
dotnet ef database update
```

Buang dan jana semula. Sentiasa. Ini diajar sebagai refleks pada Hari 3.

---

## 6. Daftar komponen kongsi — "sudah wujud, jangan tulis semula"

Dibina Hari 3, tersedia untuk **semua** kumpulan. Menulis semula mana-mana daripada ini ialah **kegagalan code review**.

### Servis

| Servis | Fungsi | Guna bila |
|--------|--------|-----------|
| `IReferenceNumberService` | Jana `LD-2026-0001` dsb. mengikut prefix modul | Bila permohonan dihantar |
| `IFileStorageService` | Simpan/dapat fail selamat di `App_Data/uploads/{submissionId}/` | Sebarang muat naik |
| `IAuditLogService` | Catat tindakan ke `AuditLogs` | Setiap perubahan status |
| `IWorkflowService` | Sahkan & laksana peralihan `SubmissionStatus` | Setiap approve/reject/submit |
| `INotificationService` | Hantar notifikasi (latihan: `ConsoleNotificationService`) | Selepas peralihan status |
| `ICurrentUserService` | Pengguna semasa, peranan, jabatan | Di mana-mana perlu identiti |

### Partial view & komponen

| Komponen | Fungsi |
|----------|--------|
| `_StatusBadge.cshtml` | Lencana `SubmissionStatus` berwarna konsisten |
| `_AuditTrail.cshtml` | Panel sejarah audit satu `Submission` |
| `_AttachmentList.cshtml` | Senarai lampiran + pautan muat turun selamat |
| `_ApprovalPanel.cshtml` | Butang Lulus/Tolak + kotak sebab wajib |
| `_FilterBar.cshtml` | Penapis status/jabatan/julat tarikh |
| `_ValidationSummary.cshtml` | Paparan ralat validation seragam |

### Kelas asas

`SubmissionControllerBase` — menyediakan `Approve`, `Reject`, `SubmitForReview`, dan penulisan audit yang **sudah betul**. Controller modul mewarisinya dan **tidak** menulis semula logik kelulusan.

> **Semakan diri sebelum menulis apa-apa helper:**
> 1. `grep -ri "<nama konsep>" projek/` — sudah wujud?
> 2. Semak jadual di atas.
> 3. Tanya AI: *"Merujuk AGENTS.md dan SPEC-KURSUS.md, adakah repo ini sudah ada cara untuk `<X>`?"*
> 4. Baru tulis — dan hanya dalam folder anda.

---

## 7. Guna AI secara berpasukan (bahagian yang paling mudah tersasar)

Keempat-empat kumpulan menggunakan AI. Tanpa konteks kongsi, empat pembantu AI menghasilkan empat gaya kod berbeza yang bertembung pada Hari 15.

**Peraturan mengikat:**

1. **Setiap sesi AI bermula dengan konteks kongsi.** Halakan pembantu AI anda ke [`AGENTS.md`](./AGENTS.md) sebelum meminta kod. Semua kumpulan menggunakan fail yang **sama** — itulah yang menyeragamkan output.
2. **Cari dahulu, jana kemudian.** Prompt pertama sentiasa: *"Adakah repo ini sudah ada `<X>`?"* — bukan *"Tulis `<X>`."*
3. **Sekat skop AI kepada folder anda.** Nyatakan dalam prompt: *"Tulis hanya fail di bawah `Models/Akses/`. Jangan ubah `Program.cs`, `ApplicationDbContext.cs`, atau `_Layout.cshtml`."*
4. **Tiada commit tanpa faham.** Sebelum commit kod jana-AI, penulis mesti **menerangkannya kepada seorang rakan sekumpulan** — apa ia buat, kenapa begitu, apa akan pecah jika dibuang. Diperiksa semasa code review.
5. **AI tidak mereka keperluan.** URS, peraturan perniagaan, dan skema datang daripada dokumen NRES dan `SPEC-KURSUS.md`. AI membantu **menulis** dan **menyemak**, bukan **memutuskan**.
6. **Tiada data NRES sebenar dalam prompt.** Semua contoh sintetik.
7. **AI sebagai penyemak, bukan hanya penjana.** Sebelum PR: *"Semak diff ini terhadap AGENTS.md dan KOLABORASI.md. Adakah ia menduplikasi apa-apa yang sudah wujud? Adakah ia menyentuh fail milik kumpulan lain?"*

**Semakan silang AI harian (10 minit, selepas stand-up).** Setiap kumpulan menunjukkan satu perkara ketara yang dijana AI semalam. Kumpulan lain menjawab satu soalan sahaja: *"Adakah kami baru sahaja membina benda yang sama?"* Ini menangkap pertindihan pada hari ia berlaku — bukan pada Hari 15.

---

## 8. Rentak harian pasukan (Hari 4–14)

| Masa | Aktiviti |
|------|----------|
| 9.00 – 9.15 | **Stand-up per kumpulan** — semalam / hari ini / halangan. `git pull --rebase origin master`. |
| 9.15 – 9.25 | **Semakan silang AI** — pertindungan antara kumpulan dikesan awal (§7). |
| 9.25 – 1.00 | Sesi pembangunan (commit kecil & kerap) |
| 2.30 – 4.30 | Sesi pembangunan |
| 4.30 – 5.00 | **Code review berpasangan** + PR + push + kemas kini board |

---

## 9. Definition of Done (satu untuk semua kumpulan)

Satu tugasan **selesai** hanya apabila **kesemua** ini benar:

- [ ] Kod berjalan — `dotnet build` bersih, aplikasi bermula, ciri berfungsi manual.
- [ ] Guna servis/komponen kongsi yang sedia ada — tiada logik didup.
- [ ] Hanya fail dalam folder kumpulan disentuh (`git diff --name-only master` disemak).
- [ ] Validation di **pelayan** (server-side), bukan pelayar sahaja.
- [ ] `[Authorize(Roles = ...)]` betul pada setiap action yang perlu.
- [ ] Perubahan status melalui `IWorkflowService`; tindakan dicatat melalui `IAuditLogService`.
- [ ] Migration dijana melalui slot yang betul (jika skema berubah).
- [ ] Kod jana-AI difahami dan boleh diterangkan oleh penulisnya.
- [ ] PR ada perihalan Bahasa Melayu + langkah cara menguji.
- [ ] Disemak dan diluluskan oleh seorang rakan sekumpulan.
- [ ] Isu board dipindah ke **Done**.

---

## 10. Aliran PR & code review

```text
kump-2/feat/semakan-pendua-plat  →  PR  →  kump-2/akses-kenderaan  →  (Hari 15)  →  master
```

**Templat PR** (semua kumpulan guna yang sama):

```markdown
## Apa yang berubah
<2–3 baris Bahasa Melayu>

## Isu berkaitan
Closes #<nombor>            <!-- atau: NRES-42 -->

## Cara uji
1. …
2. …

## Senarai semak
- [ ] Guna servis kongsi sedia ada (tiada duplikasi)
- [ ] Hanya fail folder kumpulan saya disentuh
- [ ] Validation pelayan + authorization disemak
- [ ] Migration ikut slot (jika berkenaan)
- [ ] Kod jana-AI saya faham & boleh terangkan
```

**Senarai semak penyemak** — penyemak menjawab empat soalan ini, mengikut turutan:

1. **Adakah ini sudah wujud dalam repo?** (anti-redundan — soalan paling penting)
2. **Adakah ia menyentuh fail milik orang lain?** (anti-konflik)
3. Adakah authorization & validation betul?
4. Bolehkah penulis menerangkan setiap baris?

Review adalah **wajib** dan mengambil kira 15% penilaian capstone.

---

## 11. Persediaan Hari 15 bermula pada Hari 4

Integrasi bukan aktiviti Hari 15 — ia disiapkan sedikit demi sedikit:

- **Setiap hari:** `git pull --rebase origin master`. Konflik yang ditemui hari ini ialah 5 minit; ditemui Hari 15 ialah 2 jam.
- **Setiap Jumaat (atau hujung setiap blok):** setiap kumpulan menggabungkan cabangnya ke `master` melalui PR — **gabungan latihan**. Menjelang Hari 15, `master` sudah mengandungi keempat-empat modul dan hari itu tertumpu pada Papan Pemuka Induk, SIT, dan demo.
- **Hari 13–14:** bekukan ciri baharu. Hanya pembetulan pepijat, ujian, dan pembersihan.

> Kumpulan yang menyimpan kerjanya sehingga Hari 15 **akan** gagal digabung. Ini dinyatakan awal, dan diulang.
