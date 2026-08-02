# Kumpulan 2 · Hari 4 — Skema DB Akses & Kenderaan

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)
>
> Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

**Hari pertama Fasa 2.** Anda kini pada cabang `kump-2/akses-kenderaan`, dengan asas kongsi Hari 3 sedia untuk digunakan.

> ⚠️ **Modul anda paling kompleks dari segi pemodelan.** Tiga jenis permohonan berkongsi satu `Submission` induk, ditambah entiti `Vehicle` berasingan yang **dikongsi merentas permohonan**. Beri masa pada reka bentuk hari ini — ia menjimatkan tiga hari kemudian.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `IEntityTypeConfiguration<T>` | [learn.microsoft.com/ef/core/modeling](https://learn.microsoft.com/en-us/ef/core/modeling/) |
| Hubungan satu-ke-banyak | [learn.microsoft.com/ef/core/modeling/relationships/one-to-many](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many) |
| Indeks & prestasi | [learn.microsoft.com/ef/core/modeling/indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes) |
| Nilai lalai & pengiraan | [learn.microsoft.com/ef/core/modeling/generated-properties](https://learn.microsoft.com/en-us/ef/core/modeling/generated-properties) |

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 9.00 – 9.25 | Stand-up · `git pull --rebase origin master` · semakan silang AI |
| **9.25 – 1.00 tgh** | **Entiti & skema** — `Vehicle`, tiga jadual permohonan, konfigurasi, pendaftaran modul, migration. 💻 Lab 1–5 |
| **2.30 – 4.30 petang** | **Halaman utama modul** — landing tiga jenis, senarai permohonan saya. 💻 Lab 6–7 |
| 4.30 – 5.00 | Code review + PR + **gabungan latihan ke `master`** |

**Hasil:** Empat jadual wujud; modul muncul dalam navigasi; halaman utama menunjukkan tiga laluan permohonan.

---

## Tiga jenis permohonan, satu corak

Modul anda mengendalikan tiga perkara berbeza yang mengikut aliran yang sama:

| Jenis | Prefix | Untuk apa | Terikat kenderaan? |
|-------|--------|-----------|--------------------|
| **Pas Keselamatan** | `PAS` | Akses kawasan (staf/pelawat/kontraktor) | ❌ Tidak |
| **Pelekat Kenderaan** | `STK` | Pelekat pada cermin kereta | ✅ Ya |
| **Lot Parkir** | `PKR` | Peruntukan lot khas | ✅ Ya |

Setiap satu ialah jadual **detail** yang memaut ke `Submission` induknya sendiri. Tiga `Submission` berasingan, tiga nombor rujukan berasingan — kerana ia **permohonan berasingan** yang boleh diluluskan atau ditolak secara bebas.

> **Silap biasa:** cuba menjadikan ketiga-tiganya satu jadual dengan lajur `Jenis`. Ia kelihatan lebih kemas, tetapi setiap jenis mempunyai medan yang berbeza — pas mempunyai `PurposeOfVisit`, pelekat mempunyai `StickerSerialNo`, parkir mempunyai `LotNumber`. Satu jadual bermakna kebanyakan lajur `NULL` kebanyakan masa, dan tiada validation peringkat pangkalan data.

## `Vehicle` ialah entiti bebas — dan itu keputusan penting

Seorang staf boleh mempunyai **lebih daripada satu** kenderaan. Setiap kenderaan boleh mempunyai **banyak** permohonan dari masa ke masa (pelekat tahun ini, pelekat tahun depan, permohonan parkir).

```text
UserProfile ──1:N──> Vehicle ──1:N──> VehicleStickerApplication
                        └────1:N──> ParkingApplication
```

**Kenapa bukan hanya medan `PlateNumber` dalam setiap permohonan?**

| Medan `PlateNumber` dalam setiap permohonan | Entiti `Vehicle` berasingan |
|---------------------------------------------|------------------------------|
| Semakan pendua mesti mengimbas semua jadual permohonan | Satu tempat untuk bertanya "kenderaan ini milik siapa?" |
| Nombor plat yang sama ditaip berbeza (`WXY 1234` vs `WXY1234`) | Dinormalkan sekali, semasa pendaftaran kenderaan |
| Tiada tempat menyimpan model/warna/jenis | Butiran kenderaan hidup dengan kenderaan |
| Sejarah kenderaan mustahil dijejak | `vehicle.Applications` memberi anda kesemuanya |

Semakan pendua nombor plat (Hari 5–6) ialah keperluan teras modul anda. Ia **jauh** lebih mudah dengan entiti `Vehicle`.

## Normalisasi nombor plat

Nombor plat Malaysia ditulis dengan cara yang tidak konsisten: `WXY 1234`, `wxy1234`, `WXY-1234`.

Bagi semakan pendua berfungsi, anda **mesti** menyimpan bentuk yang dinormalkan:

```text
Input pengguna:  "wxy 1234"  →  Disimpan: "WXY1234"  (huruf besar, tiada ruang/sengkang)
```

Simpan **kedua-dua**: `PlateNumber` (seperti ditaip, untuk paparan) dan `PlateNumberNormalized` (untuk carian & kekangan unik). Indeks pada yang dinormalkan.

> Ini contoh corak yang berulang dalam sistem sebenar: **satu medan untuk manusia, satu untuk mesin**.

## Indeks yang anda perlukan sejak hari pertama

| Jadual | Indeks | Kenapa |
|--------|--------|--------|
| `Vehicles` | `PlateNumberNormalized` (unik) | Semakan pendua berjalan pada setiap permohonan |
| `Vehicles` | `OwnerUserId` | "kenderaan saya" pada setiap borang |
| `VehicleStickerApplications` | `VehicleId` | Semakan pelekat aktif |
| `ParkingApplications` | `LotNumber` | Semakan peruntukan lot |

Anda akan mengukur ini pada Hari 13–14. Bina dengan betul sekarang.

## Sempadan modul anda

```text
✅ Models/Akses/                   termasuk Configurations/
✅ Controllers/AccessPass*  Parking*  VehicleSticker*
✅ Views/Akses/
✅ ViewModels/Akses/
✅ Services/Akses/

❌ Program.cs  (kecuali satu baris hari ini)
❌ Data/ApplicationDbContext.cs
❌ Views/Shared/_Layout.cshtml
❌ Models/Shared/
```

> ⚠️ **Slot migration bermula hari ini.** Umumkan sebelum `dotnet ef migrations add`. Rujuk [`../../KOLABORASI.md`](../../KOLABORASI.md) §5.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
