# Kumpulan 2 · Hari 7–9 — Semakan Keselamatan & Kelulusan Bersyarat

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Tiga hari. Hujungnya, Pegawai Keselamatan boleh menyemak permohonan, meluluskan **dengan syarat**, dan memperuntukkan nombor siri pas/pelekat dan nombor lot.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Role-based authorization | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |
| Mengatasi kaedah maya | [learn.microsoft.com/dotnet/csharp/language-reference/keywords/override](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/keywords/override) |
| Transaksi EF Core | [learn.microsoft.com/ef/core/saving/transactions](https://learn.microsoft.com/en-us/ef/core/saving/transactions) |
| Query & penapisan | [learn.microsoft.com/ef/core/querying](https://learn.microsoft.com/en-us/ef/core/querying/) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 7** | Dashboard Keselamatan — baris gilir tiga jenis, penapis, paging |
| **Hari 8** | Skrin semakan — butiran permohonan + dokumen kenderaan + panel keputusan |
| **Hari 9** | Kelulusan bersyarat, peruntukan siri/lot, ujian, gabungan latihan |

**Hasil:** Pegawai Keselamatan boleh menyemak ketiga-tiga jenis, meluluskan dengan peruntukan, menolak dengan sebab, dan meluluskan **secara bersyarat** dengan catatan.

---

## Kelulusan di modul anda berbeza — dan itu sah

Kumpulan 1 dan 4 mempunyai kelulusan mudah: lulus atau tolak. **Anda tidak.**

Meluluskan permohonan pelekat bermakna juga **memberikan nombor siri pelekat**. Meluluskan permohonan parkir bermakna **memperuntukkan nombor lot**. Kelulusan tanpa peruntukan meninggalkan permohonan dalam keadaan tidak berguna: diluluskan, tetapi tiada siapa tahu pelekat mana atau lot mana.

Ini contoh pertama kursus di mana kelas asas kongsi **tidak cukup** — dan ia menunjukkan cara mengendalikannya dengan betul.

### Cara betul: `override`, bukan tulis semula

`SubmissionControllerBase.Approve` ialah `virtual`. Anda **mengatasinya**, memanggil peraturan tambahan anda, kemudian mendelegasikan kepada asas:

```csharp
public override async Task<IActionResult> Approve(int id, string? remarks)
{
    // Peraturan modul kami DAHULU: peruntukan wajib
    // ... kemudian panggil base.Approve untuk peralihan status + audit + notifikasi
    return await base.Approve(id, remarks);
}
```

**Bukan** salin logik kelas asas ke dalam controller anda. Peralihan status, penulisan audit, dan notifikasi kekal ditakrifkan **sekali**. Anda hanya menambah apa yang khusus modul anda.

> Jika anda mendapati diri anda menyalin kod dari kelas asas, berhenti — itu isyarat untuk `override` + `base.` panggilan, atau isu `shared`.

## Kelulusan bersyarat: keadaan ketiga

Cadangan NRES menyebut **"penolakan bersyarat"** untuk modul anda. Dalam praktik, apa yang Bahagian Keselamatan perlukan ialah:

| Keputusan | Maksud | Status |
|-----------|--------|--------|
| **Lulus** | Semua baik, pas/pelekat diperuntukkan | `AdminApproved` |
| **Lulus bersyarat** | Diluluskan, tetapi dengan sekatan bertulis | `AdminApproved` + catatan syarat |
| **Tolak** | Tidak diluluskan, sebab wajib | `Rejected` |

**Kenapa kelulusan bersyarat bukan status baharu?** Kerana `SubmissionStatus` dikongsi keempat-empat modul ([`../../SPEC-KURSUS.md`](../../SPEC-KURSUS.md)). Menambah `ConditionallyApproved` bermakna tiga kumpulan lain mesti mengendalikannya dalam dashboard dan laporan mereka — untuk keadaan yang hanya modul anda mempunyai.

Sebaliknya: status kekal `AdminApproved`, dan syarat disimpan sebagai **medan pada permohonan anda**:

```csharp
public string? SyaratKelulusan { get; set; }   // null = kelulusan penuh
```

Ini corak yang berulang: **jangan kembangkan konsep kongsi untuk keperluan satu modul.** Lanjutkan jadual anda sendiri.

> Jika kumpulan anda percaya `ConditionallyApproved` benar-benar diperlukan sebagai status, itu isu berlabel `shared` — bukan keputusan setempat.

## Peruntukan: siapa memilih apa

| Perkara | Dipilih oleh | Bila |
|---------|--------------|------|
| Jenis pas, nama pemegang | Pemohon | Semasa memohon |
| Nombor siri pas | **Sistem/Keselamatan** | Semasa kelulusan |
| Nombor siri pelekat | **Sistem/Keselamatan** | Semasa kelulusan |
| Nombor lot parkir | **Keselamatan** | Semasa kelulusan |
| Kawasan yang dibenarkan | **Keselamatan** | Semasa kelulusan |

Peruntukan mesti **unik** dan **atomik**: dua kelulusan serentak tidak boleh mendapat nombor lot yang sama. Indeks unik yang anda cipta pada Hari 4 ialah pertahanan; transaksi ialah pencegahan.

## Satu lot, satu peruntukan aktif

Nombor lot parkir ialah sumber fizikal terhad. Sebelum memperuntukkan lot `A-12`, sahkan tiada permohonan aktif lain sudah memegangnya.

Ini semakan pendua **kedua** modul anda — sama coraknya dengan nombor plat, sumber berbeza. Guna semula corak `IDuplicateCheckService`; **jangan** tulis servis baharu.

## Dokumen sokongan kenderaan

Permohonan pelekat memerlukan salinan geran/kad pendaftaran. Anda menggunakan `IFileStorageService` **kongsi** — sama seperti Kumpulan 1, dengan jadual lanjutan anda sendiri untuk jenis dokumen.

Semak `AGENTS.md` sebelum membina apa-apa berkaitan fail. Ia sudah wujud.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
