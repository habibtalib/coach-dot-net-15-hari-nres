# Kumpulan 2 · Hari 13–14 — Ujian E2E, Bug Fixing & Sedia Gabung

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Dua hari. **Tiada ciri baharu.** Hujungnya, modul anda diuji, pepijat dibetulkan, dan cabang bersedia untuk gabungan Hari 15.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| xUnit | [xunit.net/docs/getting-started/v3/getting-started](https://xunit.net/docs/getting-started/v3/getting-started) |
| Ujian dengan EF Core | [learn.microsoft.com/ef/core/testing](https://learn.microsoft.com/en-us/ef/core/testing/) |
| SQLite in-memory | [learn.microsoft.com/ef/core/testing/testing-without-the-database](https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database) |
| `[Theory]` & `[InlineData]` | [xunit.net/docs/getting-started/v3/getting-started#write-first-theory](https://xunit.net/docs/getting-started/v3/getting-started) |
| Prestasi query | [learn.microsoft.com/ef/core/performance/efficient-querying](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 13** | Ujian unit — semakan pendua, peruntukan lot, kesahihan pas, normalisasi plat |
| **Hari 14** | Ujian E2E manual, bug fixing, prestasi, dokumentasi, sedia gabung |

**Hasil:** Ujian yang lulus untuk peraturan modul anda; pepijat yang diketahui dibetulkan atau direkod; cabang bergabung bersih.

---

## Apa yang patut diuji dalam modul anda

Modul anda mempunyai **peraturan perniagaan terpadat** dalam kursus. Uji peraturan itu — bukan rangka kerja.

| Uji ini | Kenapa |
|---------|--------|
| **Normalisasi nombor plat** | `WXY 1234` = `wxy-1234` = `WXY1234` |
| **Semakan pendua — kes disekat** | Peraturan teras modul |
| **Semakan pendua — kes DIBENARKAN** | Tahun berbeza, ditolak, dibatalkan |
| **Ketersediaan lot** | Lot diguna disekat; lot dilepaskan tersedia |
| **Kesahihan pas** (`NilaiKesahihan`) | Lima keadaan berbeza |
| **Keunikan nombor siri** | Rekod rasmi |
| **Validation bersyarat** | Peraturan berbeza mengikut jenis pas |

Peraturan **dibenarkan** lebih penting daripada peraturan **disekat**. Semakan pendua yang terlalu ketat menghalang kerja sebenal dan lulus setiap ujian yang jelas.

## `[Theory]` sesuai dengan modul anda

Kebanyakan peraturan anda ialah "input ini → hasil itu" merentas banyak kombinasi. `[Theory]` + `[InlineData]` menyatakan itu dengan ringkas:

```csharp
[Theory]
[InlineData("WXY 1234", "WXY1234")]
[InlineData("wxy1234",  "WXY1234")]
[InlineData("WXY-1234", "WXY1234")]
```

Satu kaedah ujian, banyak kes, setiap kegagalan dinamakan secara individu dalam output ujian.

## Kenapa SQLite in-memory, bukan penyedia InMemory

Sama seperti Kumpulan 1 — tetapi ia **lebih penting** untuk anda.

Modul anda bergantung pada **kekangan unik**: `PlateNumberNormalized`, `PassSerialNo`, `StickerSerialNo`, `VerifyToken`. Penyedia `InMemory` **mengabaikan kesemuanya**.

Ujian anda akan hijau sementara pangkalan data sebenar akan menolak data yang sama. Guna SQLite.

## Skrip ujian E2E

Ujian unit meliputi peraturan. **Ujian E2E manual** meliputi aliran — dan modul anda mempunyai tiga aliran selari yang bertemu pada skrin Keselamatan yang sama.

Skrip anda mengikut satu staf melalui ketiga-tiga jenis:

```text
1. Mohon pas keselamatan → luluskan → QR → imbas → SAH
2. Mohon pelekat kenderaan → cuba pendua → disekat → luluskan asal → QR
3. Mohon parkir → luluskan dengan lot → cuba lot sama untuk staf lain → disekat
4. Batalkan pas → imbas semula → TIDAK SAH
5. Laporan menunjukkan kesemuanya; CSV dieksport bersih
```

## Persediaan gabungan

Menjelang hujung Hari 14:

- [ ] `dotnet build` bersih (sifar amaran jika boleh)
- [ ] `dotnet test` semua lulus
- [ ] Digabung dengan `master` terkini
- [ ] Gabungan kering tiada konflik
- [ ] `README-modul.md` ditulis untuk kumpulan lain
- [ ] Pepijat yang diketahui direkod dengan jujur

> **Perhatian khusus Kumpulan 2:** anda menambah pakej **QRCoder** ke `.csproj`. Nyatakan ini dalam `README-modul.md` — ia akan muncul dalam gabungan Hari 15 dan orang lain perlu tahu kenapa.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
