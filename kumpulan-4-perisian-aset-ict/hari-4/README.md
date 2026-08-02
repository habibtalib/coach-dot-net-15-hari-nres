# Kumpulan 4 · Hari 4 — Katalog Aset & Perisian

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)
>
> Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

**Hari pertama Fasa 2.** Anda kini pada cabang `kump-4/perisian-aset`, dengan asas kongsi Hari 3 sedia untuk digunakan.

> ⚠️ **Modul anda satu-satunya dengan keadaan berterusan di luar permohonan.** Aset mempunyai **statusnya sendiri** yang berasingan daripada `SubmissionStatus`. Mencampurkan kedua-duanya ialah punca kekeliruan paling biasa dalam modul ini — dan hari ini kita memisahkannya dengan jelas.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `IEntityTypeConfiguration<T>` | [learn.microsoft.com/ef/core/modeling](https://learn.microsoft.com/en-us/ef/core/modeling/) · Buku Bab 10 (m.s. 526) |
| Seed data | [learn.microsoft.com/ef/core/modeling/data-seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding) |
| Enum & penukaran nilai | [learn.microsoft.com/ef/core/modeling/value-conversions](https://learn.microsoft.com/en-us/ef/core/modeling/value-conversions) |
| Indeks | [learn.microsoft.com/ef/core/modeling/indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes) |

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 9.00 – 9.25 | Stand-up · `git pull --rebase origin master` · semakan silang AI |
| **9.25 – 1.00 tgh** | **Katalog & entiti** — `Asset`, `SoftwareCatalogItem`, status aset, migration. 💻 Lab 1–5 |
| **2.30 – 4.30 petang** | **Seed katalog + halaman utama modul.** 💻 Lab 6–7 |
| 4.30 – 5.00 | Code review + PR + **gabungan latihan ke `master`** |

**Hasil:** Katalog aset & perisian wujud dengan data berseed; status aset dimodelkan berasingan; modul muncul dalam navigasi.

---

## Dua status yang **tidak** boleh dicampurkan

Ini konsep terpenting dalam trek anda.

| | `SubmissionStatus` | `AssetStatus` |
|---|---------------------|----------------|
| Milik | `Submission` (asas kongsi) | `Asset` (jadual anda) |
| Menjejaki | Kitaran hayat **permohonan** | Kitaran hayat **barang fizikal** |
| Nilai | `Draft`, `Submitted`, `AdminApproved`, … | `Available`, `OnLoan`, `UnderMaintenance`, `Lost`, `Retired` |
| Berubah bila | Seseorang membuat keputusan | Aset bergerak secara fizikal |

**Contoh yang menjelaskannya:**

```text
Permohonan pinjaman AST-L-2026-0001    Status permohonan: AdminApproved
└── Laptop NRES-LT-0042                Status aset: OnLoan

Permohonan pinjaman AST-L-2026-0002    Status permohonan: Rejected
└── Laptop NRES-LT-0042                Status aset: OnLoan  ← masih dipinjam!
```

Permohonan kedua ditolak **kerana** aset sudah `OnLoan`. Statusnya tidak berkaitan.

> **Silap biasa:** menambah `Available`/`OnLoan` ke `SubmissionStatus`. Itu enum **kongsi** — tiga kumpulan lain tidak mempunyai inventori. Aset anda memerlukan enumnya sendiri, dalam folder anda.

## Perisian ≠ perkakasan

Modul anda mengendalikan dua perkara yang berbeza secara asasnya:

| | Lesen perisian | Pinjaman perkakasan |
|---|-----------------|----------------------|
| Barang fizikal? | ❌ Tidak | ✅ Ya |
| Boleh dikongsi? | Bergantung jenis lesen | ❌ Satu orang pada satu masa |
| Dipulangkan? | Jarang (nyahaktif) | ✅ Sentiasa |
| Dijejak dengan | Kiraan lesen tersedia | Item individu bersiri |

Ini bermakna **dua model berbeza**:

```text
SoftwareCatalogItem              Asset
├── Nama, vendor                 ├── Nama, jenama, model
├── JenisLesen                   ├── SerialNumber  (UNIK)
├── JumlahLesen    (cth. 50)     ├── AssetTag      (UNIK, cth. NRES-LT-0042)
└── LesenDiguna    (cth. 37)     └── Status        (Available/OnLoan/...)
```

Perisian dijejak dengan **kiraan**; perkakasan dijejak dengan **item**.

## Kenapa `AssetTag` **dan** `SerialNumber`

| Medan | Datang dari | Contoh |
|-------|-------------|--------|
| `SerialNumber` | Pengeluar | `5CD1234ABC` |
| `AssetTag` | NRES | `NRES-LT-0042` |

Nombor siri mengenal pasti perkakasan; tag aset mengenal pasti **rekod inventori NRES**. Aset yang dibaiki mungkin mendapat papan induk baharu (siri baharu) tetapi kekal tag yang sama.

Kedua-duanya unik, kedua-duanya diindeks — anda akan mencari mengikut kedua-duanya.

## Kiraan lesen: medan yang dikira vs disimpan

`LesenDiguna` boleh dikira (`COUNT` permohonan aktif) atau disimpan sebagai lajur.

| | Dikira | Disimpan |
|---|--------|----------|
| Sentiasa betul | ✅ | ❌ boleh tidak segerak |
| Pantas untuk paparan senarai | ❌ query per baris | ✅ |
| Perlu kemas kini transaksi | ❌ | ✅ |

Kita **mengira** — kerana ketepatan lebih penting daripada kelajuan pada saiz NRES, dan medan kiraan yang tidak segerak ialah punca aduan inventori sebenar.

> Ini keputusan yang anda **dokumenkan**, bukan hanya buat. Anda akan mengukurnya pada Hari 13–14.

## Aset berkeadaan memerlukan transaksi

Meluluskan pinjaman melakukan **dua** perkara yang mesti berlaku bersama:

```text
1. Permohonan → AdminApproved
2. Aset       → OnLoan
```

Jika langkah 1 berjaya dan langkah 2 gagal, permohonan diluluskan untuk aset yang masih menunjukkan `Available` — dan orang seterusnya meminjamnya juga.

**Anda satu-satunya kumpulan yang memerlukan transaksi sejak Hari 7–9.** Kumpulan 3 menghadapinya juga dengan penyediaan AD — bandingkan pendekatan semasa semakan silang AI.

## Sempadan modul anda

```text
✅ Models/Aset/                    termasuk Configurations/
✅ Controllers/Asset*  Software*
✅ Views/Aset/
✅ ViewModels/Aset/
✅ Services/Aset/

❌ Program.cs  (kecuali satu baris hari ini)
❌ Data/ApplicationDbContext.cs
❌ Views/Shared/_Layout.cshtml
❌ Models/Shared/  — termasuk SubmissionStatus
```

> ⚠️ **Slot migration bermula hari ini.** Umumkan sebelum `dotnet ef migrations add`. Rujuk [`../../KOLABORASI.md`](../../KOLABORASI.md) §5.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
