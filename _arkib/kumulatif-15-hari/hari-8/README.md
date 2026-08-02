# Hari 8 — ID/AD/Email: Rantaian Kelulusan & Authorization

Panduan konsep untuk hari kelapan kursus **Latihan Secara *Coaching* Pembangunan Sistem Onboarding & Khidmat Dalaman NRES Menggunakan ASP.NET Core (.NET 10)** — kod kursus **DOTNET-NRES-15**. Nota ini mengikut **aturcara rasmi SESI 23–25** — lihat [`../JADUAL.md`](../JADUAL.md) — bukan susunan bebas.

Sambungan terus daripada [Hari 7](../hari-7/README.md): entiti `AccountRequest`, `RequestedSystemAccess`, `ApprovalStep`, dan lookup `AccessType` sudah wujud & bermigrasi. Hari ini kita bina **borang permohonan sebenar** dan **rantaian kelulusan tiga peringkat** — dengan **authorization** dikuatkuasakan di peringkat `Controller`, bukan sekadar sembunyi butang di UI.

> **Konvensyen kod:** Nota dalam **Bahasa Melayu**; kod, nama kelas, istilah teknikal dalam **Bahasa Inggeris**.

> **Cara guna nota ini:** Konsep di sini; kod penuh langkah demi langkah di [`snippets/lab.md`](./snippets/lab.md). Nota penceramah di [`nota-penceramah.md`](./nota-penceramah.md).

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| ASP.NET Core Identity — Roles | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |
| `[Authorize]` attribute (simple authorization) | [learn.microsoft.com/aspnet/core/security/authorization/simple](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/simple) |
| Policy-based authorization (rujukan lanjutan) | [learn.microsoft.com/aspnet/core/security/authorization/policies](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies) |
| Dependency Injection — pendaftaran servis | [learn.microsoft.com/aspnet/core/fundamentals/dependency-injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) |
| Model binding — senarai/koleksi dalam borang | [learn.microsoft.com/aspnet/core/mvc/models/model-binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding) |
| Data Annotations — validation | [learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 23–24: Borang & Aliran** — `Applicant Draft → Submitted → SupervisorApproved → Completed`; borang permohonan akaun, skrin kelulusan Supervisor, skrin proses ICT. 💻 **Lab:** aliran 3 peringkat |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 25: Authorization** — `[Authorize(Roles=...)]`, `IWorkflowService` semak peralihan status. 💻 **Lab:** kuatkuasa peranan pada controller |
| 5.00 petang | Bersurai |

---

## SESI 23–24 (Pagi) — Aliran Kelulusan Tiga Peringkat

### Kenapa aliran ini **berbeza** daripada Modul 1/2?

Modul 1 (Lapor Diri) dan Modul 2 (Pas/Parking/Pelekat) hanya perlukan **satu** peringkat kelulusan (HR admin, atau Security admin). Modul 3 pula melibatkan **dua** pihak berbeza yang mesti bersetuju berturutan sebelum ICT bertindak:

1. **Penyelia (Supervisor)** — sahkan permohonan ini **wajar** dari segi keperluan kerja staf (cth. "ya, staf saya betul-betul perlukan akses VPN untuk kerja jauh").
2. **ICT Admin** — laksanakan permohonan **selepas** kelulusan Penyelia (cipta akaun sebenar dalam AD/sistem e-mel, di luar aplikasi ini).

Ini **rantaian kelulusan berbilang langkah** — konsep yang belum pernah kita bina secara eksplisit di Modul 1/2. `ApprovalStep` (Hari 7) wujud khusus untuk merekod **setiap** langkah dalam rantaian ini secara berasingan.

### Aliran Status

```text
Applicant Draft → Submitted → SupervisorApproved → Completed
```

Penolakan (`Rejected`) boleh berlaku pada **dua** titik: semasa Penyelia semak (`Submitted → Rejected`), atau semasa ICT proses (`SupervisorApproved → Rejected`).

### Jadual Peraturan Status (Status Rules)

Jadual ini **kanun** untuk `IWorkflowService` — setiap peralihan status **mesti** sepadan salah satu baris di bawah; peralihan lain **ditolak**:

| Status Semasa | Tindakan | Status Seterusnya |
|---|---|---|
| `Draft` | Submit (Applicant) | `Submitted` |
| `Submitted` | Supervisor approve | `SupervisorApproved` |
| `Submitted` | Supervisor reject | `Rejected` |
| `SupervisorApproved` | ICT complete | `Completed` |
| `SupervisorApproved` | ICT reject | `Rejected` |
| `Draft` | Cancel (Applicant) | `Cancelled` |

**Kenapa jadual ini penting?** Tanpa peraturan eksplisit, tiada apa menghalang kod dari (contohnya) tukar status terus `Draft → Completed`, memintas kedua-dua kelulusan. Peraturan ini dikuatkuasakan dalam **kod** (`IWorkflowService`), bukan sekadar didokumentasikan — supaya tiada laluan pintasan walaupun ada bug di controller.

### `IWorkflowService` — Kenapa Perlu Servis Berasingan?

SPEC-KURSUS.md menyenaraikan `IWorkflowService` sebagai servis **kongsi** — dan hari ini modul pertama yang benar-benar memerlukannya secara eksplisit (Modul 1/2 hanya guna peralihan status ringkas 2 peringkat, boleh ditulis terus dalam controller; Modul 3 dengan 4 peringkat + 2 titik penolakan terlalu kompleks untuk diulang setiap kali secara manual).

Tanpa `IWorkflowService`, setiap controller perlu tulis semula logik "adakah peralihan ini sah" — berulang kod (*duplicated logic*), dan risiko satu controller lupa semak sementara controller lain menyemak (kebocoran peraturan perniagaan). Dengan `IWorkflowService` berpusat:

- **Satu tempat** peraturan didefinisikan (jadual status di atas → kod).
- Semua controller (Applicant, Supervisor, ICT) **wajib** panggil servis yang sama — mustahil "terlepas pandang" peraturan.
- Mudah diuji (unit test Hari 15) tanpa perlu simulasi keseluruhan MVC pipeline.

### Role-Based Authorization — Kenapa Di Peringkat Controller, Bukan Hanya UI?

Corak biasa pemula: sembunyikan sahaja butang "Luluskan" di Razor view untuk pengguna bukan Penyelia (`@if (User.IsInRole("Supervisor")) { <button>...</button> }`). **Ini tidak mencukupi** — sesiapa yang tahu URL `/AccountRequestApprovals/SupervisorApprove/5` boleh hantar `POST` terus (guna Postman, curl, atau devtools) dan **memintas** semakan UI sepenuhnya.

Sebab itu `[Authorize(Roles = "Supervisor")]` diletak **pada method controller** — ASP.NET Core menyemak peranan pengguna **sebelum** method controller sempat dijalankan langsung, tidak kira macam mana permintaan itu dihantar. UI (sembunyi butang) hanya **pengalaman pengguna yang lebih baik** — bukan mekanisme keselamatan sebenar. Keduanya perlu wujud bersama: UI untuk kemudahan, `[Authorize]` untuk penguatkuasaan sebenar.

```csharp
[Authorize(Roles = "Supervisor")]
public async Task<IActionResult> SupervisorApprove(int id)
{
    // Sah status semasa, kemudian panggil IWorkflowService untuk transisi.
}
```

```csharp
[Authorize(Roles = "IctAdmin")]
public async Task<IActionResult> Complete(int id)
{
    // Sah SupervisorApproved, kemudian panggil IWorkflowService untuk transisi ke Completed.
}
```

> Rujukan rasmi: [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles)

---

## SESI 25 (Petang) — Kuatkuasa Peranan Pada Controller

Petang ini kita sambungkan semua bahagian: `IWorkflowService` diimplementasi penuh, `[Authorize(Roles=...)]` diletak pada setiap action sensitif, dan kita uji **secara manual** bahawa pengguna dengan peranan salah **ditolak** (bukan sekadar butang hilang).

Peserta akan lihat pertama kali dalam kursus ini **kegagalan authorization sebenar** — cuba log masuk sebagai `Applicant` dan cuba akses `SupervisorApprove` terus melalui URL. Sepatutnya dapat mesej **403 Forbidden** (atau redirect ke Access Denied, bergantung konfigurasi Identity).

---

## Ringkasan Hari 8

1. ✅ Faham kenapa Modul 3 perlukan rantaian kelulusan dua peringkat (Penyelia → ICT), berbeza daripada Modul 1/2.
2. ✅ Faham jadual peraturan status dan kenapa ia perlu dikuatkuasakan dalam kod (`IWorkflowService`), bukan sekadar dokumentasi.
3. ✅ Faham kenapa `[Authorize(Roles=...)]` mesti di controller, bukan sekadar sembunyi UI.
4. ✅ Bina borang permohonan, skrin kelulusan Penyelia, skrin pemprosesan ICT — ketiga-tiganya berfungsi hujung-ke-hujung.

**Hasil Hari 8:** Permohonan akaun menyokong hantar → kelulusan Penyelia → penyempurnaan ICT, dengan authorization dikuatkuasakan pada setiap langkah.

---

## Apa Seterusnya — Hari 9

Esok kita tambah **notifikasi** (cetus pada setiap peralihan status), **carian/penapisan** permohonan, dan **panel audit** pada halaman detail — melengkapkan Modul 3. Sambung ke [Hari 9](../hari-9/README.md).

---

Mulakan hands-on: [`snippets/lab.md`](./snippets/lab.md). Nota penceramah: [`nota-penceramah.md`](./nota-penceramah.md).
