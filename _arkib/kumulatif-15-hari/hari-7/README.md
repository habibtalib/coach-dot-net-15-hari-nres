# Hari 7 — ID/AD/Email: Discovery & Model

Panduan konsep untuk hari ketujuh kursus **Latihan Secara *Coaching* Pembangunan Sistem Onboarding & Khidmat Dalaman NRES Menggunakan ASP.NET Core (.NET 10)** — kod kursus **DOTNET-NRES-15**. Nota ini mengikut **aturcara rasmi SESI 20–22** — lihat [`../JADUAL.md`](../JADUAL.md) — bukan susunan bebas.

Projek kursus: **`Nres.Onboarding.Web`** — aplikasi ASP.NET Core MVC tunggal yang dibina secara kumulatif. Hari ini kita mula **Modul 3 · ID, AD & Email** — modul ketiga daripada lima, dibina di atas seni bina kongsi (`Submission`, `Attachment`, `AuditLog`) yang sudah wujud sejak Hari 1, dan corak conditional-validation/duplicate-check yang sudah dilatih di Modul 2 (Hari 4–6).

> **Nota untuk peserta:** Anda tidak perlu ulang baca kod Modul 1/2 — tetapi **corak** yang sama (`Form → Draft → Submit → Review → Approve/Reject → Audit`) berulang di sini. Apa yang **baharu** hari ini ialah struktur data permohonan akaun ICT, dan **satu prinsip keselamatan yang tidak boleh dilanggar**: aplikasi ini **tidak pernah** menyimpan kata laluan.

> **Konvensyen kod:** Nota dalam **Bahasa Melayu**; semua kod, nama kelas/pembolehubah, dan istilah teknikal (`Controller`, `DbContext`, `migration`) dalam **Bahasa Inggeris** — amalan standard industri .NET yang kita ikut sepanjang kursus.

> **Cara guna nota ini:** Bahagian ini menerangkan **konsep** — kenapa setiap entiti/keputusan reka bentuk wujud. Latihan hands-on **langkah demi langkah** (kod penuh untuk ditaip) ada di [`snippets/lab.md`](./snippets/lab.md). Baca bahagian yang sepadan di sini dahulu, kemudian pindah ke lab untuk cuba sendiri. Nota penceramah ada di [`nota-penceramah.md`](./nota-penceramah.md).

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| EF Core — Relationships (one-to-many) | [learn.microsoft.com/ef/core/modeling/relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships) |
| EF Core — Data Seeding (`HasData`) | [learn.microsoft.com/ef/core/modeling/data-seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding) |
| EF Core — Migrations (`dotnet ef`) | [learn.microsoft.com/ef/core/managing-schemas/migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/) |
| Enums dalam C# | [learn.microsoft.com/dotnet/csharp/language-reference/builtin-types/enum](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum) |
| ASP.NET Core MVC — Routing & Controllers | [learn.microsoft.com/aspnet/core/mvc/controllers/routing](https://learn.microsoft.com/en-us/aspnet/core/mvc/controllers/routing) |
| Primary constructors (C# 12) | [learn.microsoft.com/dotnet/csharp/whats-new/csharp-12#primary-constructors](https://learn.microsoft.com/en-us/dotnet/csharp/whats-new/csharp-12#primary-constructors) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran Peserta & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 20–21: Jenis Permohonan** — akaun AD baharu, email, kemas kini akaun, nyahaktif, akses sistem tambahan; entiti `AccountRequest`, `RequestedSystemAccess`, `ApprovalStep`. 💻 **Lab:** model + seed jenis akses |
| 1.00 – 2.30 petang | Rehat dan Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 22: Dashboard Modul ICT** — skrin awal, seed access types (AD, Email, Shared folder, VPN, Sistem dalaman). 💻 **Lab:** dashboard + migration |
| 5.00 petang | Bersurai |

**Hari ini tidak** merangkumi borang permohonan penuh, rantaian kelulusan, atau notifikasi — semua itu Hari 8–9 (lihat [`../JADUAL.md`](../JADUAL.md)). Fokus hari ini **semata-mata** *discovery* + pemodelan data + skrin awal.

---

## SESI 20–21 (Pagi) — Faham Aliran Permohonan Akaun ICT

### Kenapa Modul 3 wujud?

Setiap kali pekerja baharu melapor diri (Modul 1 selesai), atau pekerja sedia ada berpindah bahagian, mereka perlukan **akaun** untuk bekerja: log masuk komputer (Active Directory), e-mel rasmi, akses folder kongsi jabatan, VPN untuk kerja jauh, dan akses ke sistem dalaman (cth. sistem HR, sistem kewangan). Tanpa sistem berpusat, permohonan ini datang secara e-mel/borang kertas berselerak — ICT sukar jejak **siapa** mohon **apa**, **siapa luluskan**, dan **bila** ia disempurnakan.

Modul 3 menyatukan **lima jenis permohonan** dalam **satu** aliran:

| Jenis Permohonan | Penerangan ringkas |
|---|---|
| Akaun AD baharu | Log masuk domain (Active Directory) untuk staf baharu |
| Akaun e-mel baharu | Alamat e-mel rasmi jabatan |
| Kemas kini akaun | Tukar nama, jabatan, atau maklumat akaun sedia ada |
| Nyahaktifan akaun | Sekat/padam akaun bila staf bertukar/bersara/keluar |
| Akses sistem tambahan | Tambah akses ke folder kongsi, VPN, atau sistem dalaman tertentu |

Perhatikan: kelima-lima jenis ini **berbeza** dari segi tujuan, tetapi **sama** dari segi corak — semuanya perlukan permohonan → kelulusan Penyelia → pemprosesan ICT. Sebab itu kita model semuanya dengan **satu** entiti `AccountRequest` + medan `RequestType` (enum), bukan lima jadual berasingan. Corak ini sama seperti Modul 2 (satu `Vehicle` dikongsi tiga jenis permohonan pas/parkir/pelekat).

### 🛡️ TITIK PENGAJARAN KESELAMATAN — Jangan Sesekali Simpan Kata Laluan

**Ini peraturan paling penting Modul 3, dan ia berulang sepanjang Hari 7–9:**

> Aplikasi `Nres.Onboarding.Web` **merekodkan permohonan** untuk akaun AD/e-mel — ia **BUKAN** sistem pengurusan kata laluan. Tiada medan `Password`, `Credential`, `Pin`, atau apa-apa bentuk rahsia log masuk sepatutnya wujud **di mana-mana** dalam `AccountRequest`, `RequestedSystemAccess`, atau jadual lain modul ini.

**Kenapa?**

1. **Skop tanggungjawab.** Sistem ini ialah *request tracking*, bukan *identity provider*. Active Directory/Entra ID/sistem e-mel sudah ada mekanisme selamat sendiri untuk cipta & urus kata laluan (complexity policy, hashing, MFA). Kita tidak patut *duplicate* atau *bypass* mekanisme itu.
2. **Permukaan serangan (attack surface).** Setiap medan tambahan yang menyimpan rahsia ialah risiko baharu — jika pangkalan data `Nres.Onboarding.Web` bocor (SQLite fail, backup tidak disulitkan, log tersalah cetak), kata laluan sebenar staf terdedah.
3. **Pematuhan & audit.** Ramai polisi keselamatan agensi kerajaan (termasuk garis panduan MAMPU/NRES sendiri) melarang penyimpanan kata laluan pengguna dalam sistem sampingan.

**Apa yang kita buat sebaliknya:**

- `AccountRequest` merekod **permintaan** (siapa, jenis akaun apa, sebab, akses apa) — bukan hasil (kata laluan sebenar).
- Selepas ICT **memproses** (`Complete`), kata laluan sementara diserahkan **di luar aplikasi ini** — secara manual/selamat oleh ICT (cth. sistem AD sendiri hantar e-mel "set kata laluan pertama" terus kepada staf, tanpa melalui `Nres.Onboarding.Web`).
- Status `Completed` dalam sistem kita bermaksud *"ICT sudah proses permintaan ini"*, bukan *"berikut ialah kata laluan akaun"*.

> Ingat prinsip ini setiap kali anda reka bentuk medan baharu untuk modul ini: **"Adakah medan ini rahsia log masuk?"** Jika ya — ia **tidak** patut wujud di sini.

### Entiti Baharu Hari Ini

Tiga entiti khusus Modul 3, mengikut corak entiti kongsi (`Submission` sebagai induk) yang sudah ditubuhkan Hari 1:

```text
Submission (induk, sudah wujud sejak Hari 1)
  └─ AccountRequest (1:1 dengan Submission — corak sama macam OfficerReportingApplication)
       └─ RequestedSystemAccess (1:banyak — satu permohonan boleh minta pelbagai akses)

ApprovalStep (1:banyak dengan Submission — rekod setiap keputusan kelulusan)
```

**`AccountRequest`** — butiran khusus permohonan akaun: jenis permohonan, maklumat pemohon, jabatan, penyelia yang perlu luluskan, dan sebab permohonan.

**`RequestedSystemAccess`** — satu baris bagi **setiap** jenis akses yang diminta dalam satu permohonan (cth. permohonan "Akses Sistem Tambahan" boleh minta VPN **dan** folder kongsi serentak — dua baris `RequestedSystemAccess`, satu `AccountRequest`).

**`ApprovalStep`** — SPEC-KURSUS.md menyenaraikan `ApprovalStep` sebagai **entiti kongsi**, tetapi Modul 3 ialah modul **pertama** yang benar-benar memerlukan rantaian kelulusan berbilang langkah (Penyelia → ICT — lihat Hari 8), jadi kita cipta definisi konkritnya hari ini. Ia merekod **setiap** keputusan (langkah, peranan pelulus, status, sebab) berasingan daripada `AuditLog` am — `AuditLog` rekod *semua* tindakan (termasuk cipta/edit draf), manakala `ApprovalStep` khusus rekod **keputusan kelulusan rasmi** sahaja, berguna untuk laporan rantaian kelulusan kemudian.

### Kenapa `AccessType` sebagai jadual lookup berasingan (bukan enum)?

Kita boleh jadikan jenis akses (AD, Email, Shared folder, VPN, Sistem dalaman) sebagai `enum` C# — tetapi kita pilih **jadual lookup** (`AccessType`) sebaliknya, sebab:

- **Boleh tambah tanpa deploy semula.** ICT mungkin perkenalkan sistem dalaman baharu (cth. sistem e-Aduan) tahun depan — tambah satu baris dalam jadual, tiada perlu `migration` atau kod baharu.
- **Konsisten dengan lookup sedia ada.** SPEC-KURSUS.md sudah tetapkan corak `LookupDepartments`/`LookupGrades`/`LookupPositions` sebagai jadual, bukan enum — kita ikut corak yang sama untuk `AccessType`.

Sebaliknya, `RequestType` (jenis permohonan: AD baharu/e-mel/kemas kini/nyahaktif/akses tambahan) kita jadikan **enum** (`AccountRequestType`) — sebab lima jenis ini **tetap** (bukan operational data yang berubah kerap), dan kita perlukan `switch`/`if` logik berbeza untuk setiap jenis dalam kod (borang, validation) — enum memberi *type safety* untuk itu.

---

## SESI 22 (Petang) — Dashboard Modul ICT

Setiap modul dalam `Nres.Onboarding.Web` ada **skrin pendaratan** (landing page) — corak yang sama seperti Modul 2 Hari 4. Dashboard Modul 3 memaparkan:

- Jumlah permohonan mengikut status (draf saya, menunggu penyelia, menunggu ICT, selesai)
- Pautan pantas untuk cipta permohonan baharu
- (Hari 8–9 akan tambah senarai "menunggu tindakan saya" ikut peranan)

Dashboard ringkas ini juga tempat pertama peserta **nampak** hasil migration & seed hari ini — bukti visual bahawa `AccessType` sudah berseed dan `AccountRequests`/`RequestedSystemAccesses` sudah wujud dalam skema.

> Rujukan rasmi: [learn.microsoft.com/aspnet/core/mvc/overview](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview) · [learn.microsoft.com/ef/core/modeling/data-seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)

---

## Ringkasan Hari 7

1. ✅ Faham lima jenis permohonan Modul 3 dan kenapa satu `AccountRequest` + enum `RequestType` mencukupi untuk semuanya.
2. ✅ Faham **kenapa** aplikasi ini tidak pernah menyimpan kata laluan — dan tanggungjawab itu kekal di luar sistem.
3. ✅ Cipta entiti `AccountRequest`, `RequestedSystemAccess`, `ApprovalStep`, dan lookup `AccessType`.
4. ✅ Seed jenis akses (AD, Email, Shared folder, VPN, Sistem dalaman).
5. ✅ Migration pertama Modul 3 + dashboard skrin awal.

**Hasil Hari 7:** Model permohonan akaun & skrin awal Modul 3 wujud dalam `Nres.Onboarding.Web`, sedia untuk borang & aliran kelulusan Hari 8.

---

## Apa Seterusnya — Hari 8

Esok kita bina **borang permohonan sebenar**, **skrin kelulusan Penyelia**, **skrin pemprosesan ICT**, dan kuatkuasa **role-based authorization** (`[Authorize(Roles = "Supervisor")]` / `[Authorize(Roles = "IctAdmin")]`) melalui `IWorkflowService`. Sambung ke [Hari 8](../hari-8/README.md).

---

Mulakan hands-on: [`snippets/lab.md`](./snippets/lab.md). Nota penceramah: [`nota-penceramah.md`](./nota-penceramah.md).
