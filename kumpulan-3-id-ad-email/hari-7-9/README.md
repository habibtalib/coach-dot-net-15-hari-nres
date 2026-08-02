# Kumpulan 3 · Hari 7–9 — Pemprosesan ICT, RBAC & Simulasi AD

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Tiga hari. Hujungnya, Pentadbir ICT boleh memproses permohonan yang lulus penyelia, meluluskan akses **secara berasingan**, dan merekod akaun AD yang dicipta — tanpa pernah menyimpan kata laluan.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Role-based authorization | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |
| Policy-based authorization | [learn.microsoft.com/aspnet/core/security/authorization/policies](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/policies) |
| Transaksi EF Core | [learn.microsoft.com/ef/core/saving/transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions) |
| Keselamatan aplikasi | [learn.microsoft.com/aspnet/core/security](https://learn.microsoft.com/en-us/aspnet/core/security/) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 7** | Baris gilir ICT + skrin pemprosesan |
| **Hari 8** | **Kelulusan akses separa** + rekod akaun AD/e-mel |
| **Hari 9** | **RBAC merentas modul** + simulasi AD + ujian |

**Hasil:** ICT boleh memproses permohonan, meluluskan sebahagian akses dan menolak yang lain, merekod nama akaun AD & e-mel yang diberikan, dan menandakan kelayakan sebagai diserahkan.

---

## Kelulusan separa: realiti ICT

Permohonan meminta lima akses. ICT meluluskan tiga, menolak dua kerana staf tidak memerlukannya untuk jawatannya.

Kebanyakan sistem mengendalikan ini dengan buruk — ia memaksa **semua atau tiada**, jadi ICT sama ada meluluskan akses yang tidak sepatutnya, atau menolak keseluruhan permohonan dan meminta staf memohon semula. Kedua-duanya salah.

Medan `bool? Diluluskan` pada `RequestedSystemAccess` (Hari 4) menjadikan ini mungkin:

```text
Permohonan ICT-ID-2026-0001    Status: AdminApproved
├── AD          ✅ Diluluskan
├── E-mel       ✅ Diluluskan
├── VPN         ❌ Ditolak — "Tiada keperluan kerja luar pejabat"
├── HRMIS       ✅ Diluluskan
└── ePerolehan  ❌ Ditolak — "Bukan pegawai perolehan"
```

**Apakah status permohonan?** Ini soalan reka bentuk sebenar:

| Pilihan | Masalah |
|---------|---------|
| `AdminApproved` jika **mana-mana** diluluskan | Pemohon fikir semuanya diluluskan |
| `Rejected` jika **mana-mana** ditolak | Menyembunyikan bahawa 3 akses berfungsi |
| `AdminApproved` + senarai jelas apa yang ditolak | ✅ Jujur |

Kita memilih yang ketiga: status ialah `AdminApproved` (permohonan diproses), dan **notifikasi serta skrin butiran menyenaraikan dengan jelas** akses mana yang ditolak dan mengapa.

> Jika **semua** akses ditolak, maka permohonan itu `Rejected` — tiada apa yang diluluskan.

## Apa yang ICT rekod (dan tidak rekod)

| ✅ Rekod ini | ❌ Jangan sekali-kali |
|--------------|------------------------|
| `AdAccountName` — `ahmad.zulkifli` | Kata laluan awal |
| `OfficialEmail` — `ahmad.zulkifli@nres.gov.my` | Kata laluan sementara |
| `KelayakanDiserahkan` — bendera benar/palsu | Cara kelayakan diserahkan |
| `TarikhSerahan` | Soalan keselamatan |
| `CatatanIct` | Apa-apa yang boleh log masuk |

**Nama akaun AD dan e-mel mempunyai indeks unik** (Hari 4) — pangkalan data menghalang dua staf mendapat `ahmad.zulkifli`.

> Bila ICT menandakan `KelayakanDiserahkan = true`, itu bermakna: *"Saya telah menyerahkan kelayakan kepada staf melalui saluran selamat di luar sistem ini."* Sistem merekod **fakta**, bukan kandungannya. Ini corak yang betul untuk mana-mana sistem yang menyentuh kelayakan.

## Simulasi integrasi AD

Kursus ini **tidak** menyambung ke Active Directory sebenar. Kita **mensimulasikannya** — dan itu keputusan yang sengaja, bukan pintasan.

| Kenapa simulasi | |
|-----------------|--|
| Makmal latihan tiada AD | Peserta tidak boleh mencipta akaun sebenar |
| AD sebenar memerlukan kelayakan istimewa | Risiko keselamatan dalam kelas |
| Ralat menjadi tidak boleh dipulihkan | Akaun rosak dalam direktori sebenar |

**Apa yang kita bina sebaliknya:** `IAdProvisioningService` dengan pelaksanaan **simulasi** yang:

- Menjana cadangan nama akaun daripada nama staf (`Ahmad bin Zulkifli` → `ahmad.zulkifli`)
- Menyemak nama tersebut belum digunakan
- Mengembalikan hasil "berjaya" selepas kelewatan pendek
- Log apa yang **akan** dihantar ke AD sebenar

Antara mukanya direka supaya pelaksanaan sebenar (`System.DirectoryServices`) boleh menggantikannya kemudian tanpa mengubah controller. Itu tujuan sebenar latihan ini: **reka sempadan integrasi dengan betul**, walaupun bahagian jauhnya palsu.

> Nyatakan dengan jelas dalam serahan Hari 15: **integrasi AD adalah simulasi**. NRES tidak boleh mengandaikan akaun sebenar dicipta.

## RBAC merentas modul — tugas anda untuk seluruh sistem

Modul anda mengurus **akses**. Itu menjadikan anda pasukan yang secara semula jadi menyemak sama ada RBAC berfungsi merentas keempat-empat modul.

Pada Hari 9 anda menjalankan **matriks RBAC** — setiap peranan × setiap skrin admin:

| Peranan | K1 Lapor Diri | K2 Akses | K3 Akaun | K4 Aset |
|---------|---------------|----------|----------|---------|
| `Applicant` | ❌ | ❌ | ❌ | ❌ |
| `Supervisor` | ❌ | ❌ | ✅ (peringkat 1) | ❌ |
| `HrAdmin` | ✅ | ❌ | ❌ | ❌ |
| `SecurityAdmin` | ❌ | ✅ | ❌ | ❌ |
| `IctAdmin` | ❌ | ❌ | ✅ | ✅ |
| `SystemAdmin` | ✅ | ✅ | ✅ | ✅ |

Sebarang ✅ yang sepatutnya ❌ ialah **kelemahan keselamatan**. Anda melaporkannya kepada kumpulan pemilik — ini sumbangan silang yang sebenar, dan ia menyediakan SIT Hari 15.

## Policy vs Roles

Setakat ini kita menggunakan `[Authorize(Roles = "IctAdmin")]`. Itu memadai untuk kebanyakan skrin.

Tetapi peraturan anda lebih halus: *"Penyelia yang **ditetapkan** untuk permohonan ini"* bukan peranan — ia semakan **data**. Peranan menjawab "siapa anda"; ini menjawab "adakah anda orangnya".

| Semakan | Alat |
|---------|------|
| "Adakah anda `IctAdmin`?" | `[Authorize(Roles = ...)]` |
| "Adakah anda penyelia **permohonan ini**?" | Semakan dalam kaedah action |
| "Adakah anda pemilik **rekod ini**?" | Semakan dalam kaedah action |

Untuk kursus ini, semakan dalam action sudah memadai dan lebih mudah dibaca. Policy berasaskan keperluan (*requirement*) ialah langkah seterusnya jika peraturan menjadi lebih kompleks — kita **membincangkannya**, tidak membinanya.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
