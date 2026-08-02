# Kumpulan 4 · Hari 7–9 — Kelulusan ICT, Pemulangan & Transaksi Inventori

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Tiga hari. Kemuncak teknikal trek anda. Hujungnya, Unit Aset ICT boleh meluluskan pinjaman dengan **peruntukan aset atomik**, dan merekod pemulangan dengan pemeriksaan kondisi yang mengemas kini inventori secara automatik.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| **Transaksi EF Core** | [learn.microsoft.com/ef/core/saving/transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions) |
| Concurrency & token | [learn.microsoft.com/ef/core/saving/concurrency](https://learn.microsoft.com/en-us/ef/core/saving/concurrency) |
| Mengatasi kaedah maya | [learn.microsoft.com/dotnet/csharp/language-reference/keywords/override](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/override) |
| Role-based authorization | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 7** | Baris gilir ICT + skrin semakan |
| **Hari 8** | **Kelulusan dengan peruntukan aset — dalam transaksi** |
| **Hari 9** | **Pemulangan + pemeriksaan kondisi + kemas kini inventori** |

**Hasil:** Kelulusan memperuntukkan aset dan menukar statusnya secara atomik; pemulangan memeriksa kondisi dan mengembalikan aset ke stok (atau ke penyelenggaraan/hilang).

---

## Kenapa transaksi wajib di sini

Meluluskan pinjaman melakukan **empat** perubahan yang mesti berlaku bersama:

```text
1. AssetLoanRequest.AssetId  = 42          ← peruntukkan unit
2. Asset(42).Status          = OnLoan      ← keluarkan dari stok
3. Submission.Status         = AdminApproved
4. AuditLog                  ← rekod
```

**Apa yang berlaku tanpa transaksi**, jika langkah 2 gagal selepas langkah 1 berjaya:

| Rekod | Nilai | Masalah |
|-------|-------|---------|
| Permohonan | Diluluskan, aset 42 | Pemohon dimaklumkan ia dilulus |
| Aset 42 | Masih `Available` | **Orang seterusnya meminjamnya juga** |

Dua orang, satu laptop, dan tiada siapa tahu sehingga seseorang datang mengambilnya.

Kumpulan 3 menghadapi masalah bentuk yang sama dengan penyediaan AD. **Bandingkan pendekatan semasa semakan silang AI** — kedua-dua kumpulan menggunakan `BeginTransactionAsync`, tetapi konteksnya berbeza.

## Transaksi tidak menyelesaikan perlumbaan

Ini nuansa yang penting, dan ia mudah disalahfahamkan.

Transaksi memastikan **empat perubahan berlaku bersama atau tidak langsung**. Ia **tidak** menghalang dua pentadbir ICT daripada memperuntukkan aset yang sama secara serentak:

```text
ICT-A: baca Asset 42 → Available
ICT-B: baca Asset 42 → Available     ← kedua-dua melihat "tersedia"
ICT-A: transaksi → Asset 42 = OnLoan  ✅
ICT-B: transaksi → Asset 42 = OnLoan  ✅ ← menimpa, tiada ralat
```

Dua permohonan, satu laptop, kedua-duanya "berjaya".

Tiga cara mengendalikannya:

| Pendekatan | Kesesuaian |
|------------|------------|
| Semak semula status dalam transaksi | ✅ **Kita guna ini** — mudah, memadai |
| Token concurrency (`[Timestamp]`) | Lebih tegas; dibincangkan Hari 13–14 |
| Kunci pesimis (`SELECT … FOR UPDATE`) | Berlebihan untuk saiz NRES |

Semakan dalam transaksi bermakna: **muat aset dengan penjejakan, sahkan `Status == Available`, kemudian tetapkan**. Jika ICT-B menyemak selepas ICT-A commit, ia melihat `OnLoan` dan gagal dengan bersih.

## Pemulangan menentukan status aset seterusnya

Pemeriksaan kondisi bukan hanya rekod — ia **memandu inventori**:

| Kondisi | Status aset selepas | Kenapa |
|---------|---------------------|--------|
| `Baik` | `Available` | Kembali ke stok |
| `Rosak` | `UnderMaintenance` | Tidak boleh dipinjamkan sehingga dibaiki |
| `Hilang` | `Lost` | Kekal dalam rekod; tidak pernah `Available` |

> **Aset `Lost` tidak dipadam.** Ia kekal dalam pangkalan data dengan status `Lost` — kerana ia masih dalam daftar aset NRES sehingga dihapus kira secara rasmi, dan audit memerlukan rekod itu.

## Kelulusan perisian: peruntukan kunci lesen

Permohonan perisian lebih mudah — tiada unit fizikal untuk diperuntukkan. Tetapi ICT masih merekod:

- Kunci lesen (jika berkenaan)
- Tarikh diaktifkan

Kunci lesen ialah data sensitif. Ia **bukan** kata laluan, tetapi ia bernilai — layan dengan berhati-hati:

| | |
|---|---|
| Simpan? | Ya — ICT memerlukannya untuk pemasangan semula |
| Papar kepada pemohon? | Ya — mereka memerlukannya |
| Papar dalam senarai/laporan? | **Tidak** — hanya pada halaman butiran permohonan mereka sendiri |
| Log? | **Tidak** |

## Anda mengatasi `Approve`, seperti Kumpulan 2

`SubmissionControllerBase.Approve` menukar status dan menulis audit. Ia **tidak** memperuntukkan aset.

Corak yang sama seperti Kumpulan 2:

```csharp
public override async Task<IActionResult> Approve(int id, string? remarks)
{
    // Peruntukan aset dalam transaksi...
    // ...kemudian delegasikan
    return await base.Approve(id, catatan);
}
```

Kecuali — anda mempunyai komplikasi: peruntukan **mesti berada dalam transaksi yang sama** dengan perubahan status. Memanggil `base.Approve` selepas commit transaksi anda bermakna perubahan status berada di **luar** transaksi.

**Penyelesaian:** jalankan keseluruhan operasi dalam transaksi anda sendiri, gunakan `IWorkflowService` terus untuk peralihan, dan **jangan** panggil `base.Approve` untuk kes ini. Ini salah satu daripada beberapa tempat dalam kursus di mana kelas asas tidak sesuai — dan lab menerangkan sebabnya.

> Bandingkan dengan Kumpulan 2 (yang **boleh** memanggil `base.Approve`) dan Kumpulan 3 (yang menambah tindakan baharu). Tiga situasi berbeza, tiga penyelesaian berbeza — dan **memahami mana yang mana** ialah pengajarannya.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
