# Kumpulan 4 · Hari 4 — Skema Katalog Fasiliti & Tempahan

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)
>
> Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

**Hari pertama Fasa 2.** Anda kini pada cabang `kump-4/tempahan-fasiliti`, dengan asas kongsi Hari 3 sedia untuk digunakan.

> Modul anda mempunyai **tiga** entiti yang saling bergantung: katalog fasiliti (`SportsFacility`), permohonan tempahan (`FacilityBookingApplication`), dan **slot yang ditempah** (`FacilityBookingSlot`). Reka bentuk slot hari ini menentukan sama ada semakan bertindih Hari 5–6 menjadi mudah atau menyakitkan — sama seperti `Vehicle` menentukan semakan pendua Kumpulan 2.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| `IEntityTypeConfiguration<T>` | [learn.microsoft.com/ef/core/modeling](https://learn.microsoft.com/en-us/ef/core/modeling/) |
| Hubungan satu-ke-satu & satu-ke-banyak | [learn.microsoft.com/ef/core/modeling/relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships) |
| Jenis `DateOnly` / `TimeOnly` dalam EF Core | [learn.microsoft.com/ef/core/providers/sqlite/limitations#dateonly-and-timeonly](https://learn.microsoft.com/en-us/ef/core/providers/sqlite/limitations) |
| Data seed (`HasData`) | [learn.microsoft.com/ef/core/modeling/data-seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding) |
| Indeks komposit | [learn.microsoft.com/ef/core/modeling/indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes) |

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 9.00 – 9.25 | Stand-up · `git pull --rebase origin master` · semakan silang AI |
| **9.25 – 1.00 tgh** | **Entiti & skema** — `SportsFacility`, `FacilityBookingApplication`, `FacilityBookingSlot`, konfigurasi, seed katalog, pendaftaran modul, migration. 💻 Lab 1–5 |
| **2.30 – 4.30 petang** | **Katalog & halaman utama** — servis katalog, senarai fasiliti, tempahan saya. 💻 Lab 6–7 |
| 4.30 – 5.00 | Code review + PR + **gabungan latihan ke `master`** |

**Hasil:** Tiga jadual wujud; katalog berseed dengan fasiliti sintetik; modul muncul dalam navigasi; halaman utama menyenaraikan fasiliti + tempahan saya.

---

## Tiga entiti, satu tanggungjawab setiap satu

| Entiti | Apa ia wakili | Analog Kumpulan 2 |
|--------|---------------|-------------------|
| **`SportsFacility`** | Katalog — gelanggang/dewan yang wujud secara fizikal. Data rujukan, bukan permohonan. | `ParkingLot` (sumber terhad) |
| **`FacilityBookingApplication`** | Permohonan tempahan — siapa, untuk apa, berapa orang. Memaut ke `Submission`. | `VehicleStickerApplication` |
| **`FacilityBookingSlot`** | **Slot yang ditempah** — fasiliti + tarikh + masa mula/tamat. Objek yang semakan bertindih beroperasi ke atasnya. | `Vehicle` (medan yang disemak) |

`SportsFacility` ialah **data rujukan** — ia dimiliki NRES, bukan dimohon. Ia tiada `SubmissionId`, sama seperti `Vehicle` dan `ParkingLot` Kumpulan 2. Anda **seed** katalog; peserta tidak menciptanya.

## Kenapa slot ialah entiti berasingan — dan itu keputusan penting

Godaan semula jadi ialah meletakkan `BookingDate`, `StartTime`, `EndTime` terus dalam `FacilityBookingApplication`. Ia berfungsi hari ini. Ia **memusnahkan** Hari 5–6.

| Masa dalam permohonan | `FacilityBookingSlot` berasingan |
|-----------------------|----------------------------------|
| Semakan bertindih mengimbas jadual permohonan penuh | Query terus ke atas satu jadual slot yang berindeks |
| Tiada tempat untuk indeks `(FacilityId, BookingDate)` yang bersih | Indeks komposit tepat pada objek yang disoal |
| Satu permohonan = satu masa selamanya | Membuka jalan kepada tempahan berbilang slot (berulang mingguan) kelak |
| `FacilityId` bercampur dengan medan permohonan | Slot ialah *"fasiliti X, tarikh D, jam T1–T2"* — bersih |

**Slot membawa `FacilityId`-nya sendiri**, walaupun permohonan juga ada. Ini **denormalisasi yang disengajakan**: semakan bertindih ialah query paling panas modul anda, dan ia menyoal *"apa yang ditempah untuk fasiliti ini pada tarikh ini?"* — soalan itu patut dijawab tanpa join ke jadual permohonan. Bandingkan dengan `PlateNumberNormalized` Kumpulan 2: satu medan yang wujud khas untuk menjadikan query teras pantas dan bersih.

## Konvensyen selang separuh-terbuka `[mula, tamat)`

Ini keputusan reka bentuk paling penting hari ini, dan ia hanya satu ayat:

> Slot meliputi masa dari `StartTime` (termasuk) hingga `EndTime` (**tidak** termasuk).

Slot `10:00–11:00` dan `11:00–12:00` **tidak** bertindih — yang pertama berakhir tepat apabila yang kedua bermula. Ini dipanggil selang *separuh-terbuka*, ditulis `[10:00, 11:00)`. Ia menjadikan tempahan bersebelahan (satu selepas satu) sah secara semula jadi — yang betul-betul apa yang Encik Faizal mahu apabila kelab badminton menempah 8–9 pagi dan kelab sepak takraw menempah 9–10 pagi pada gelanggang serba guna yang sama.

Anda akan mengekod peraturan ini pada Hari 5–6. Hari ini, cuma ingat: **simpan `TimeOnly`, fikir separuh-terbuka.**

## Indeks yang anda perlukan sejak hari pertama

| Jadual | Indeks | Kenapa |
|--------|--------|--------|
| `FacilityBookingSlots` | `(FacilityId, BookingDate)` komposit | Semakan bertindih berjalan pada setiap tempahan — ia menapis fasiliti + tarikh dahulu |
| `FacilityBookingApplications` | `SubmissionId` (unik) | Satu permohonan = satu `Submission` |
| `FacilityBookingApplications` | `FacilityId` | "Tempahan untuk fasiliti ini" |
| `SportsFacilities` | `Name` (unik) | Nama katalog tidak berulang |

Anda akan mengukur ini pada Hari 13–14. Bina dengan betul sekarang.

## Sempadan modul anda

```text
✅ Models/Fasiliti/                 termasuk Configurations/
✅ Controllers/FacilityBooking*
✅ Views/FacilityBooking/
✅ ViewModels/Fasiliti/
✅ Services/Fasiliti/

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
