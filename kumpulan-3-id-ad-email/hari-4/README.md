# Kumpulan 3 · Hari 4 — Skema DB Akaun & Akses

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)
>
> Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

**Hari pertama Fasa 2.** Anda kini pada cabang `kump-3/id-ad-email`, dengan asas kongsi Hari 3 sedia untuk digunakan.

> ⚠️ **Modul anda satu-satunya dengan kelulusan DUA peringkat.** Kumpulan lain menggunakan satu `ApprovalStep`; anda menggunakan dua. Reka bentuk hari ini menentukan sama ada Hari 5–9 mudah atau menyakitkan.
>
> 🔒 **Modul anda juga paling sensitif dari segi keselamatan.** Anda menguruskan permohonan akaun. **Jangan sekali-kali simpan kata laluan** dalam mana-mana entiti anda — ini titik pengajaran kursus, dan ia diperiksa.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `IEntityTypeConfiguration<T>` | [learn.microsoft.com/ef/core/modeling](https://learn.microsoft.com/en-us/ef/core/modeling/) · Buku Bab 10 (m.s. 526) |
| Hubungan satu-ke-banyak | [learn.microsoft.com/ef/core/modeling/relationships/one-to-many](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many) |
| Enum & penukaran nilai | [learn.microsoft.com/ef/core/modeling/value-conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions) |
| Amalan keselamatan akaun | [learn.microsoft.com/aspnet/core/security](https://learn.microsoft.com/en-us/aspnet/core/security/) |

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 9.00 – 9.25 | Stand-up · `git pull --rebase origin master` · semakan silang AI |
| **9.25 – 1.00 tgh** | **Entiti & skema** — `AccountRequest`, `RequestedSystemAccess`, laluan kelulusan 2 peringkat, migration. 💻 Lab 1–5 |
| **2.30 – 4.30 petang** | **Seed jenis akses + halaman utama modul.** 💻 Lab 6–7 |
| 4.30 – 5.00 | Code review + PR + **gabungan latihan ke `master`** |

**Hasil:** Jadual `AccountRequests` & `RequestedSystemAccesses` wujud; jenis akses berseed; laluan kelulusan dua peringkat dicipta pada penghantaran; modul muncul dalam navigasi.

---

## Empat jenis permohonan, satu jadual

Tidak seperti Kumpulan 2 (tiga jadual berasingan), modul anda mempunyai **satu** jadual permohonan dengan lajur `JenisPermohonan`. Kenapa berbeza?

| | Kumpulan 2 (3 jadual) | Kumpulan 3 (1 jadual) |
|---|------------------------|------------------------|
| Medan setiap jenis | **Sangat berbeza** (`LotNumber` vs `StickerSerialNo`) | **Hampir sama** — semua tentang akaun pengguna |
| Kekangan DB | Boleh dikuatkuasakan setiap jenis | Kebanyakan medan berkongsi |

Empat jenis anda:

| Jenis | Maksud | Medan tambahan |
|-------|--------|----------------|
| `AkaunBaharu` | Staf baharu perlu AD + e-mel | Tarikh mula, penyelia |
| `TukarAkses` | Staf sedia ada perlu akses sistem tambahan | Sistem yang dipohon |
| `TukarMaklumat` | Nama/jabatan berubah | Apa yang berubah |
| `Nyahaktif` | Staf berhenti/bertukar | Tarikh akhir, sebab |

Semuanya berkongsi: pemohon, penyelia, justifikasi, laluan kelulusan. Satu jadual masuk akal **di sini** — tetapi tidak untuk Kumpulan 2. **Konteks menentukan, bukan peraturan.**

## Akses sistem ialah banyak-ke-banyak

Satu permohonan boleh meminta **banyak** akses (AD + e-mel + VPN + folder kongsi). Setiap jenis akses boleh diminta oleh **banyak** permohonan.

```text
AccountRequest ──1:N──> RequestedSystemAccess ──N:1──> LookupSystemAccess
```

Kita menggunakan jadual penghubung **eksplisit** (`RequestedSystemAccess`) dan bukan `many-to-many` tersirat EF Core, kerana setiap baris membawa **datanya sendiri**:

| Medan | Kenapa |
|-------|--------|
| `AccessLevel` | Baca sahaja / Baca-tulis / Pentadbir |
| `Justifikasi` | Kenapa akses ini diperlukan |
| `Diluluskan` | ICT boleh meluluskan **sebahagian** akses dan menolak yang lain |
| `CatatanIct` | Kenapa satu akses ditolak |

**Medan `Diluluskan` itu penting.** Ia bermakna satu permohonan boleh berakhir dengan tiga daripada lima akses diluluskan — realiti biasa dalam ICT yang kebanyakan sistem tangani dengan buruk.

## Kelulusan dua peringkat: guna `ApprovalStep`

Asas kongsi Hari 3 memberi anda `ApprovalStep` dengan `StepOrder`. **Anda satu-satunya kumpulan yang benar-benar menggunakannya.**

```text
Submission (ICT-ID-2026-0001)
├── ApprovalStep { StepOrder = 1, RoleRequired = "Supervisor", Decision = Pending }
└── ApprovalStep { StepOrder = 2, RoleRequired = "IctAdmin",   Decision = Pending }
```

Aliran:

```text
Draft ──hantar──> Submitted
                     │
                     ├── Penyelia lulus (langkah 1) ──> SupervisorApproved
                     │                                        │
                     │                                        ├── ICT proses (langkah 2) ──> Completed
                     │                                        └── ICT tolak ──> Rejected
                     └── Penyelia tolak ──> Rejected
```

**Bila langkah dicipta?** Pada **penghantaran**, bukan pada cipta draf — kerana penyelia mungkin berubah semasa draf masih disunting.

> **Kenapa jadual dan bukan dua lajur (`SupervisorDecision`, `IctDecision`)?** Kerana laluan kelulusan ialah **data**, bukan struktur. Jika NRES kemudiannya memerlukan peringkat ketiga (Ketua Bahagian), anda menambah baris — bukan lajur, bukan migration, bukan perubahan kod pada setiap query.

## `SubmissionStatus` sudah menyokong anda

Perhatikan `SupervisorApproved` sudah wujud dalam enum kongsi ([`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md)). Ia diletakkan di sana pada Hari 3 **untuk modul anda**.

Peralihan yang `IWorkflowService` benarkan:

```text
Submitted           → SupervisorApproved ✅
SupervisorApproved  → AdminApproved      ✅
SupervisorApproved  → Rejected           ✅
Submitted           → AdminApproved      ✅  (jalan pintas — kita TIDAK gunakan)
```

Anda menggunakan `Submitted → SupervisorApproved → AdminApproved → Completed`.

> `SubmissionControllerBase.Approve` menetapkan `AdminApproved` — betul untuk **peringkat 2** anda. Untuk peringkat 1, anda menambah tindakan `SupervisorApprove` yang memanggil `IWorkflowService` terus. **Bukan** menulis semula kelas asas.

## 🔒 Jangan simpan kata laluan — dan apa yang disimpan sebaliknya

Ini keperluan keselamatan paling penting dalam keseluruhan kursus, dan modul anda ialah tempat godaan itu paling kuat.

| ❌ Jangan sekali-kali simpan | ✅ Simpan ini sebaliknya |
|------------------------------|---------------------------|
| Kata laluan awal untuk akaun baharu | Tiada — ICT menetapkannya dalam AD sebenar |
| Kata laluan sementara "untuk diberitahu kepada staf" | Bendera "kata laluan telah diserahkan" + tarikh |
| Kata laluan sedia ada untuk tetapan semula | Tiada — tetapan semula berlaku dalam AD |
| Soalan/jawapan keselamatan | Tiada |

Yang **boleh** anda simpan: nama akaun AD yang dicipta (`ahmad.zulkifli`), alamat e-mel yang diberikan, tarikh penyerahan, dan siapa memprosesnya.

**Kenapa ini penting melebihi dasar:** sistem ini akan mengandungi permohonan untuk ratusan akaun. Jika ia menyimpan kata laluan — walaupun "sementara", walaupun "di-hash" — ia menjadi sasaran bernilai tinggi. Reka bentuk yang betul ialah sistem **tidak pernah tahu** kata laluan.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
