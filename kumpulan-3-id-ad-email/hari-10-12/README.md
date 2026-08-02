# Kumpulan 3 · Hari 10–12 — Penjejakan Status, Audit Trail & Dashboard ICT

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Tiga hari. Hujungnya, pemohon boleh menjejak permohonan mereka melalui kedua-dua peringkat, **audit trail penuh** menceritakan kisah lengkap, dan ICT mempunyai papan pemuka operasi.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| LINQ — join, group, lookup | Buku Bab 11 (m.s. 596) · [learn.microsoft.com/dotnet/csharp/linq](https://learn.microsoft.com/en-us/dotnet/csharp/linq/) |
| Query cekap EF Core | [learn.microsoft.com/ef/core/performance/efficient-querying](https://learn.microsoft.com/en-us/ef/core/performance/efficient-querying) |
| View components | [learn.microsoft.com/aspnet/core/mvc/views/view-components](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/view-components) |
| Audit & pengelogan | [learn.microsoft.com/aspnet/core/fundamentals/logging](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/logging/) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 10** | Penjejakan status — garis masa dua peringkat untuk pemohon |
| **Hari 11** | **Audit trail penuh** — siapa, bila, apa, dari mana |
| **Hari 12** | Papan pemuka ICT + carian/penapis lanjutan |

**Hasil:** Pemohon melihat garis masa yang jelas; audit trail merekod setiap tindakan termasuk keputusan setiap akses; ICT mempunyai papan pemuka dengan penapis berguna.

---

## Penjejakan: pemohon perlu tahu **di mana** permohonan berada

Modul lain mempunyai satu peringkat — "dihantar" atau "diputuskan". Anda mempunyai **empat kedudukan** yang bermakna:

```text
①  Draf                  belum dihantar
②  Menunggu Penyelia     dihantar, langkah 1 pending
③  Menunggu ICT          langkah 1 lulus, langkah 2 pending
④  Selesai / Ditolak     kedua-dua langkah diputuskan
```

`SubmissionStatus` sahaja tidak mencukupi untuk memaparkan ini dengan baik — anda memerlukan `ApprovalStep` untuk mengetahui **siapa** yang sedang memegangnya dan **berapa lama**.

Garis masa visual mengurangkan pertanyaan "di mana permohonan saya?" kepada ICT — yang merupakan sebahagian besar beban kerja meja bantuan sebenar.

## Berapa lama setiap peringkat mengambil masa

Data ini sudah ada dalam `ApprovalStep.DecidedAt` dan `Submission.SubmittedAt`. Dua metrik yang berguna:

| Metrik | Pengiraan |
|--------|-----------|
| Masa menunggu penyelia | `langkah1.DecidedAt − submission.SubmittedAt` |
| Masa pemprosesan ICT | `langkah2.DecidedAt − langkah1.DecidedAt` |

**Kenapa ini penting:** jika penyelia mengambil purata 9 hari dan ICT mengambil 1 hari, masalah bukan ICT. Sistem yang tidak mengukur setiap peringkat secara berasingan menyalahkan pasukan yang salah.

## Audit trail: apa yang `IAuditLogService` **tidak** tangkap

Servis kongsi merekod perubahan status. Modul anda memerlukan lebih:

| Peristiwa | Ditangkap oleh servis kongsi? |
|-----------|-------------------------------|
| Status berubah `Submitted` → `SupervisorApproved` | ✅ |
| Siapa membuat keputusan itu | ✅ (`ActorUserId`) |
| **Akses mana yang ditolak dan mengapa** | ❌ |
| **Nama akaun AD yang diberikan** | ❌ |
| **Bila kelayakan diserahkan** | ❌ |

Anda **tidak** mengubah `IAuditLogService` — ia dikongsi. Anda memanggilnya dengan `Remarks` yang **kaya**, dan menyimpan butiran berstruktur pada entiti anda sendiri.

```csharp
await audit.LogAsync(submissionId, "AccessDecided",
    remarks: "VPN ditolak: tiada keperluan kerja luar pejabat. " +
             "ePerolehan ditolak: bukan pegawai perolehan.");
```

> Jika kumpulan anda merasakan `AuditLog` memerlukan medan berstruktur (cth. `EntityType`, `EntityId`, `Changes` JSON), itu **isu `shared`** — dan ia idea yang munasabah. Bincang, jangan bina secara senyap.

## Audit untuk modul akses ialah keperluan pematuhan

Modul lain mempunyai audit kerana ia amalan baik. **Modul anda mempunyai audit kerana ia diperiksa.**

Sistem yang memberikan akses kepada sistem lain ialah tepat perkara yang juruaudit ICT semak. Soalan yang mereka tanya:

- Siapa meluluskan akses pentadbir untuk staf ini?
- Bila akses VPN diberikan, dan atas justifikasi apa?
- Adakah penyelia yang meluluskan benar-benar penyelia staf itu?
- Berapa lama antara permohonan dan penyediaan?

Audit trail anda mesti menjawab kesemuanya **tanpa membaca kod**.

## Papan pemuka ICT: soalan operasi

| Soalan | Paparan |
|--------|---------|
| Apa yang menunggu saya sekarang? | Kiraan `SupervisorApproved` |
| Apa yang tersekat pada penyelia? | Kiraan `Submitted` + umur purata |
| Berapa akaun dicipta bulan ini? | Kiraan `AdminApproved` 30 hari |
| Akses mana paling kerap ditolak? | Kumpulan mengikut akses, `Diluluskan == false` |
| Permohonan lebih 7 hari belum diproses | Senarai, disusun tertua dahulu |

**Baris terakhir itu paling berguna secara operasi** — ia senarai kerja, bukan statistik.

Akses yang paling kerap ditolak juga bernilai: jika VPN ditolak 80% masa, sama ada borang perlu menerangkan kriteria dengan lebih baik, atau ia tidak sepatutnya berada dalam senarai pilihan.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
