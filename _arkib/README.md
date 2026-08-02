# `_arkib/` — Kandungan Versi Lama

Folder ini menyimpan bahan kursus daripada **struktur lama** yang telah digantikan. Ia dikekalkan sebagai rujukan penulis kandungan — **bukan** bahan yang diedarkan kepada peserta.

## `kumulatif-15-hari/`

Draf pertama kursus DOTNET-NRES-15, disusun sebagai **satu kohort kumulatif**: semua peserta membina satu aplikasi bersama-sama merentas 15 hari berturutan (Hari 1 asas → Hari 2–3 Lapor Diri → Hari 4–6 Pas/Parkir → Hari 7–9 ID/AD/Email → Hari 10–12 PKS → Hari 13–14 Aset ICT → Hari 15 integrasi).

**Kenapa diarkibkan:** cadangan silibus rasmi NRES (`cadangan_silibus_coaching_15hari_NRES.docx`) menetapkan model **4 kumpulan dedicated yang bekerja selari** pada Hari 3–14, bukan satu kohort berturutan. Struktur folder dan pecahan hari tidak lagi sepadan.

**Perubahan skop yang berkaitan:**

- **Modul PKS (Pematuhan Kod Setia) dibuang dari skop kursus.** Cadangan NRES hanya menyenaraikan 4 kumpulan/modul dan tidak menyebut PKS langsung. Kandungan PKS lama kekal di `kumulatif-15-hari/hari-10/`, `hari-11/`, dan `hari-12/` sekiranya NRES mahu menghidupkannya semula kemudian. Borang sumber PKS masih wujud dalam repo jiran `../coach-nres/4. PKS/`.
- **Versi .NET kekal .NET 10 LTS / EF Core 10** (cadangan NRES menulis .NET 8 — keputusan pemilik kursus ialah kekal pada .NET 10; rujuk `../SPEC-KURSUS.md`).

## Kandungan yang masih boleh diguna semula

Banyak bahan lab dalam arkib ini masih sah dari segi teknikal dan telah diserap ke dalam struktur baharu:

| Fail arkib | Diserap ke |
|------------|------------|
| `kumulatif-15-hari/hari-1/` | `../hari-1/` (persediaan) + `../hari-2/` (entiti kongsi, DbContext, migration) |
| `kumulatif-15-hari/hari-2/`, `hari-3/` | `../kumpulan-1-lapor-diri/` |
| `kumulatif-15-hari/hari-4/`–`hari-6/` | `../kumpulan-2-pas-parkir-pelekat/` |
| `kumulatif-15-hari/hari-7/`–`hari-9/` | `../kumpulan-3-id-ad-email/` |
| `kumulatif-15-hari/hari-10/`–`hari-12/` | *(PKS — di luar skop)* |
| `kumulatif-15-hari/hari-13/`, `hari-14/` | `../kumpulan-4-perisian-aset-ict/` |
| `kumulatif-15-hari/hari-15/` | `../hari-15/` (integrasi, SIT, demo) |

> Jangan sunting fail dalam `_arkib/`. Jika sesuatu perlu dibetulkan, betulkannya dalam struktur aktif.
