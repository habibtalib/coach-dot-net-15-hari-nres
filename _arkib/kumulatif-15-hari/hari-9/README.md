# Hari 9 — ID/AD/Email: Notifikasi, Carian & Audit

Panduan konsep untuk hari kesembilan kursus **Latihan Secara *Coaching* Pembangunan Sistem Onboarding & Khidmat Dalaman NRES Menggunakan ASP.NET Core (.NET 10)** — kod kursus **DOTNET-NRES-15**. Nota ini mengikut **aturcara rasmi SESI 26–28** — lihat [`../JADUAL.md`](../JADUAL.md) — bukan susunan bebas.

Sambungan terus daripada [Hari 8](../hari-8/README.md): aliran `Draft → Submitted → SupervisorApproved → Completed`/`Rejected` sudah berfungsi penuh, dengan `[Authorize(Roles=...)]` dikuatkuasakan. Hari ini kita **lengkapkan** Modul 3 dengan tiga keupayaan terakhir: **notifikasi**, **carian/penapisan**, dan **panel audit** — corak yang boleh diulang untuk Modul 4 & 5 kemudian.

> **Konvensyen kod:** Nota dalam **Bahasa Melayu**; kod, nama kelas, istilah teknikal dalam **Bahasa Inggeris**.

> **Cara guna nota ini:** Konsep di sini; kod penuh langkah demi langkah di [`snippets/lab.md`](./snippets/lab.md). Nota penceramah di [`nota-penceramah.md`](./nota-penceramah.md).

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Dependency Injection — daftar & guna servis | [learn.microsoft.com/aspnet/core/fundamentals/dependency-injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) |
| LINQ — `Where` bersyarat & carian dinamik | [learn.microsoft.com/dotnet/csharp/linq](https://learn.microsoft.com/en-us/dotnet/csharp/linq/) |
| Interface & Strategy Pattern asas | [learn.microsoft.com/dotnet/csharp/fundamentals/types/interfaces](https://learn.microsoft.com/en-us/dotnet/csharp/fundamentals/types/interfaces) |
| Partial Views (Razor) | [learn.microsoft.com/aspnet/core/mvc/views/partial](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/partial) |
| Query string model binding (`[FromQuery]`) | [learn.microsoft.com/aspnet/core/mvc/models/model-binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 26–27: Notifikasi** — `INotificationService` + `ConsoleNotificationService`, cetus pada submit/approve/reject/complete. 💻 **Lab:** hook notifikasi |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 28: Carian & Audit** — carian ikut rujukan/pemohon/jabatan/status/jenis, panel audit pada halaman detail. 💻 **Lab:** carian + audit panel |
| 5.00 petang | Bersurai |

---

## SESI 26–27 (Pagi) — Notifikasi

### Kenapa notifikasi perlu, dan kenapa `Console` mencukupi untuk latihan?

Dalam sistem sebenar, setiap peralihan status penting patut memberitahu pihak berkenaan — pemohon perlu tahu permohonan diluluskan/ditolak, Penyelia perlu tahu ada permohonan menunggu tindakan, ICT perlu tahu ada kerja baharu. Tanpa notifikasi, pengguna terpaksa **log masuk berkala untuk semak** — tidak praktikal untuk sistem sebenar.

Untuk **latihan**, kita tidak mahu peserta bergelut dengan konfigurasi SMTP/API e-mel sebenar (kata laluan akaun mel, port, TLS) — itu mengalih fokus daripada **corak seni bina** yang kita ajar: *notifications should be triggered by workflow events, not hardcoded into every controller ad-hoc*. Sebab itu kita guna `ConsoleNotificationService` — implementasi **latihan** yang tulis ke Console sahaja, di belakang **interface yang sama** (`INotificationService`) yang boleh ditukar kepada implementasi e-mel sebenar (SMTP, SendGrid, dll.) dalam pengeluaran **tanpa ubah satu baris pun kod controller**.

```csharp
public interface INotificationService
{
    Task SendAsync(string recipientEmail, string subject, string message);
}
```

```csharp
public class ConsoleNotificationService : INotificationService
{
    public Task SendAsync(string recipientEmail, string subject, string message)
    {
        Console.WriteLine($"To: {recipientEmail} | {subject} | {message}");
        return Task.CompletedTask;
    }
}
```

Ini corak **Strategy Pattern** asas — `INotificationService` mendefinisikan **apa** yang perlu dilakukan (hantar mesej kepada penerima), manakala implementasi (`ConsoleNotificationService` hari ini, `SmtpNotificationService` kelak) menentukan **bagaimana**. Kod controller hanya bergantung pada interface, tidak pernah tahu (atau perlu tahu) implementasi sebenar — inilah **Dependency Inversion** dalam amalan.

### Titik Cetus Notifikasi (Trigger Points)

Empat titik dalam aliran Modul 3 yang **mesti** memicu notifikasi:

| Peristiwa | Penerima | Mesej ringkas |
|---|---|---|
| Submit | Penyelia (`SupervisorUserId`) | "Permohonan {ReferenceNo} menunggu kelulusan anda." |
| Supervisor Approve | ICT Admin (kumpulan) & Pemohon | "Permohonan {ReferenceNo} diluluskan Penyelia, kini menunggu ICT." / "Permohonan anda diluluskan Penyelia." |
| Reject (Supervisor atau ICT) | Pemohon | "Permohonan {ReferenceNo} ditolak: {Sebab}." |
| Complete | Pemohon | "Permohonan {ReferenceNo} telah disempurnakan oleh ICT." |

> **Peringatan berulang:** Mesej notifikasi **tidak sekali-kali** mengandungi kata laluan atau kredensial log masuk — walaupun pada peristiwa `Complete`. Mesej hanya memberitahu **status**, bukan **kelayakan log masuk**.

### Kenapa notifikasi dicetus di dalam controller/servis kelulusan, bukan sebagai job berasingan?

Untuk latihan, kesederhanaan diutamakan — kita panggil `INotificationService.SendAsync(...)` **terus** selepas `IWorkflowService.TransitionAsync(...)` berjaya, dalam action yang sama. Dalam sistem pengeluaran sebenar berskala besar, ini biasanya dipisahkan ke *background job*/*message queue* supaya kegagalan hantar e-mel tidak menyekat permintaan pengguna — tetapi konsep asas (notifikasi dicetus oleh **peristiwa aliran kerja**) kekal sama. Kita tidak masuk mendalam ke *background job* dalam kursus ini; itu topik lanjutan di luar skop 15 hari.

---

## SESI 28 (Petang) — Carian & Audit

### Carian & Penapisan

Bila jumlah permohonan meningkat (puluhan/ratusan), senarai mudah (`Index`) tidak praktikal — ICT/Penyelia perlukan cara **cari** permohonan tertentu dengan cepat. Modul 3 menyokong carian mengikut **lima** kriteria:

- **No. Rujukan** (`ICT-ID-2026-0001`)
- **Nama Pemohon**
- **Jabatan**
- **Status** (`Draft`/`Submitted`/`SupervisorApproved`/`Completed`/`Rejected`)
- **Jenis Permohonan** (`RequestType`)

Setiap kriteria **pilihan** (opsyenal) — pengguna boleh gabungkan mana-mana kombinasi, atau tinggal kosong untuk lihat semua. Ini dilaksanakan dengan corak `IQueryable` + `Where` bersyarat: setiap penapis hanya ditambah kepada query **jika** nilai diberikan, tanpa perlu tulis kombinasi `if/else` berasingan untuk setiap gabungan kriteria yang mungkin.

### Panel Audit

Setiap halaman detail permohonan (`AccountRequests/Details`) perlu papar **sejarah lengkap** tindakan — bila dicipta, bila dihantar, bila diluluskan/ditolak, oleh siapa, dengan sebab apa. Ini **bukan** rekod baharu — kita sudah rekod setiap peristiwa ini melalui `IAuditLogService.RecordAsync(...)` sejak Hari 8 (malah sejak Modul 1, Hari 1). Hari ini kita hanya **papar** rekod sedia ada dalam jadual `AuditLogs` sebagai satu panel kronologi pada halaman detail — tiada perubahan struktur data diperlukan.

> **Kenapa ini penting untuk ICT & audit dalaman?** Panel ini menjawab soalan "siapa buat apa, bila" tanpa perlu semak log pelayan atau pangkalan data secara manual — telus untuk pemohon (nampak status permohonan sendiri) dan boleh dipertanggungjawabkan (*accountable*) untuk pelulus.

---

## Ringkasan Hari 9

1. ✅ Faham corak Strategy/Dependency Inversion melalui `INotificationService` + `ConsoleNotificationService`.
2. ✅ Cetus notifikasi pada empat titik: submit, supervisor approve, reject (kedua-dua peringkat), complete.
3. ✅ Bina carian/penapisan berbilang kriteria menggunakan `IQueryable` + `Where` bersyarat.
4. ✅ Papar panel audit kronologi pada halaman detail, menggunakan `AuditLog` sedia ada.

**Hasil Hari 9:** Modul 3 (ID/AD/Email) **lengkap** — discovery, model, aliran kelulusan, authorization, notifikasi, carian, dan audit semuanya berfungsi hujung-ke-hujung.

---

## Apa Seterusnya — Hari 10

Modul 3 selesai. Esok kita mula **Modul 4 · PKS (Pematuhan Kod Setia)** — model pematuhan berpaksikan versi polisi (`PolicyVersion`, `ComplianceChecklistItem`, `ComplianceDeclaration`, `ComplianceResponse`). Perhatikan berapa banyak corak Modul 3 (entiti kongsi `Submission`, `IWorkflowService`, `IAuditLogService`, `INotificationService`) terus terpakai tanpa ubah — inilah kuasa seni bina kongsi yang kita bina sejak Hari 1.

---

Mulakan hands-on: [`snippets/lab.md`](./snippets/lab.md). Nota penceramah: [`nota-penceramah.md`](./nota-penceramah.md).
