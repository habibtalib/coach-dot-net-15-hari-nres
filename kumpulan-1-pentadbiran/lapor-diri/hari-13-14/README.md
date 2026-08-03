# Kumpulan 1 · Hari 13–14 — Ujian, Refactor & Sedia Gabung

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Dua hari. **Tiada ciri baharu.** Hujungnya, modul anda diuji, dibersihkan, dan bersedia untuk gabungan Hari 15.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| xUnit | [xunit.net/docs/getting-started/v3/getting-started](https://xunit.net/docs/getting-started/v3/getting-started) |
| Ujian dengan EF Core | [learn.microsoft.com/ef/core/testing](https://learn.microsoft.com/en-us/ef/core/testing/) |
| SQLite in-memory untuk ujian | [learn.microsoft.com/ef/core/testing/testing-without-the-database](https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database) |
| Prestasi query | [learn.microsoft.com/ef/core/performance/efficient-querying](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying) |
| Logging EF Core | [learn.microsoft.com/ef/core/logging-events-diagnostics](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 13** | Persediaan projek ujian, ujian unit servis, ujian peraturan perniagaan |
| **Hari 14** | Optimasi query, refactor, pembersihan, dokumentasi modul, sedia gabung |

**Hasil:** `Nres.Onboarding.Tests` dengan ujian yang lulus untuk peraturan modul anda; query dioptimumkan; kod bersih; cabang bersedia untuk Hari 15.

---

## Apa yang patut diuji (dan apa yang tidak)

Anda tidak boleh menguji segalanya dalam dua hari. Uji perkara yang **paling mahal jika salah**.

| Uji ini | Kenapa |
|---------|--------|
| Format nombor rujukan & kenaikan | Rekod rasmi — salah bermakna rekod audit rosak |
| Peraturan peralihan status | Kelulusan tidak sah ialah kegagalan pematuhan |
| Penguatkuasaan dokumen wajib | Peraturan perniagaan teras |
| Sebab penolakan wajib | Pemohon perlu tahu sebab |
| Semakan pemilikan | Kelemahan keselamatan |
| Kunci selepas hantar | Integriti data |

| Jangan uji ini | Kenapa |
|----------------|--------|
| Bahawa EF Core menyimpan ke pangkalan data | Anda menguji Microsoft, bukan kod anda |
| Bahawa Razor merender HTML | Sama |
| Getter/setter | Tiada logik untuk gagal |
| Konfigurasi rangka kerja | Ia sama ada berjalan atau tidak |

> **Pandu ujian anda daripada URS Hari 1.** Setiap kriteria penerimaan "Mesti ada" ialah calon ujian. Ini menutup gelung: keperluan → kod → ujian.

## Kenapa SQLite in-memory, bukan penyedia InMemory

EF Core mempunyai penyedia `InMemory`, tetapi ia **bukan pangkalan data relasi** — ia tidak menguatkuasakan kunci asing, indeks unik, atau kekangan. Ujian anda akan lulus dan pengeluaran akan gagal.

Guna **SQLite in-memory** sebagai gantinya: ia pangkalan data SQL sebenar dengan kekangan sebenar, dijalankan dalam RAM.

```csharp
var connection = new SqliteConnection("DataSource=:memory:");
connection.Open();   // kekalkan terbuka — menutupnya memusnahkan DB
```

**Perbezaan itu penting untuk anda secara khusus:** indeks unik ditapis pada `ReferenceNo` dan indeks unik pada `SubmissionId` ialah tepat jenis perkara yang penyedia `InMemory` abaikan.

## Optimasi query: ukur, kemudian betulkan

Jangan optimumkan dengan tekaan. Hidupkan logging EF Core, jalankan skrin anda, dan lihat SQL yang sebenarnya berjalan.

```csharp
options.UseSqlite(cs).LogTo(Console.WriteLine, LogLevel.Information);
```

Cari tiga masalah:

| Masalah | Rupa dalam log | Pembetulan |
|---------|----------------|------------|
| **N+1 query** | Satu `SELECT`, kemudian 20 lagi | `.Include()` atau projek dalam satu query |
| **Terlalu banyak lajur** | `SELECT * ` bila anda memaparkan 5 medan | `.Select()` ke view model |
| **Menapis dalam memori** | Satu `SELECT` tanpa `WHERE` | Alihkan `.Where()` sebelum `ToListAsync()` |

Anda telah membina dengan betul sejak Hari 7 — blok ini mengesahkannya dan menangkap yang terlepas.

## Refactor: apa yang sebenarnya patut diubah

Refactor bermaksud **memperbaiki struktur tanpa mengubah tingkah laku**. Dengan ujian sedia ada, anda boleh melakukannya dengan selamat.

Sasaran yang bernilai dalam dua hari:

- **Kaedah controller yang panjang** — alihkan logik perniagaan ke servis
- **Nombor & rentetan ajaib** — namakan sebagai pemalar
- **Kod pendua dalam modul anda** — ekstrak kaedah
- **Nama yang mengelirukan** — namakan semula supaya ia bermakna
- **Kod jana-AI yang tiada siapa faham** — fahami atau buang

Sasaran yang **tidak** bernilai sekarang: seni bina semula, mengubah corak, memperkenalkan lapisan baharu. Hari 15 ialah esok.

## Persediaan gabungan

Menjelang hujung Hari 14, cabang anda mesti:

- [ ] Dibina bersih (`dotnet build` — sifar amaran jika boleh)
- [ ] Semua ujian lulus (`dotnet test`)
- [ ] Digabung dengan `master` terkini (`git pull --rebase origin master`)
- [ ] Tiada suntingan fail kongsi yang belum diselesaikan
- [ ] `README-modul.md` ditulis untuk kumpulan lain

> **Kumpulan yang menyimpan kerja sehingga Hari 15 akan gagal digabung.** Ini telah dinyatakan sejak Hari 2. Hari ini ialah peluang terakhir untuk mengejar.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
