# Kumpulan 1 · Hari 13–14 — Ujian, Refactor & Sedia Gabung

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../../JADUAL.md`](../../../JADUAL.md) · Kontrak: [`../../../KOLABORASI.md`](../../../KOLABORASI.md) · AI QA: [`lab-qa-ai-uat.md`](../../../docs/lab-qa-ai-uat.md)
>
> Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

Dua hari terakhir trek. **Tiada ciri baharu.** Anda menulis ujian xUnit yang mengunci tingkah laku modul PKS, mengukur dan membetulkan prestasi, membersihkan kod supaya orang lain boleh membacanya pada Hari 15, dan mengesahkan cabang anda bergabung bersih.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| xUnit | [xunit.net/docs/getting-started/v3/getting-started](https://xunit.net/docs/getting-started/v3/getting-started) |
| Ujian EF Core dengan SQLite | [learn.microsoft.com/ef/core/testing/testing-with-the-database](https://learn.microsoft.com/en-us/ef/core/testing/testing-with-the-database) |
| Logging EF Core | [learn.microsoft.com/ef/core/logging-events-diagnostics](https://learn.microsoft.com/en-us/ef/core/logging-events-diagnostics/) |

## Jadual Blok Ini

| Hari | Sesi | Fokus |
|------|------|-------|
| **Hari 13** pagi | Projek ujian & ujian teras | `TestDbFactory` kongsi, ujian nombor rujukan & peralihan status |
| **Hari 13** petang | Ujian peraturan PKS | Status pematuhan, penerbitan polisi, validation kontraktor |
| **Hari 14** pagi | Prestasi & refactor | Ukur query, betulkan N+1, bersihkan kod |
| **Hari 14** petang | Sedia gabung | Dokumentasi modul, gabungan kering, status akhir |

**Hasil:** Suite ujian PKS hijau, prestasi diukur, kod bersih dan didokumen, cabang disahkan bergabung bersih ke `master`.

---

## Kenapa SQLite in-memory, bukan penyedia InMemory

Kita menguji terhadap **SQLite sebenar dalam memori**, bukan penyedia EF Core `InMemory`. Sebabnya kritikal untuk modul ini: penyedia `InMemory` **tidak** menguatkuasakan kunci asing atau indeks unik. Dua ciri PKS bergantung sepenuhnya pada kekangan pangkalan data:

- **Indeks unik ditapis pada `IsCurrent`** — hanya satu versi polisi semasa. Penyedia `InMemory` akan membenarkan dua, jadi ujian lulus sementara pengeluaran gagal.
- **Indeks unik pada `SubmissionId`** — satu-ke-satu.

SQLite in-memory ialah pangkalan data SQL **sebenar** dengan kekangan sebenar, tetapi pantas dan tidak meninggalkan fail. `TestDbFactory` dikongsi keempat-empat kumpulan.

## Apa yang paling berharga untuk diuji dalam PKS

Setiap modul menguji nombor rujukan dan peralihan status (corak kongsi). Tetapi PKS mempunyai **dua peraturan unik** yang wajib diuji, kerana ia paling mudah pecah semasa refactor:

1. **Status pematuhan dikira dengan betul** — akuan terhadap versi lama = "perlu akui semula"; terhadap versi semasa = "patuh".
2. **Penerbitan versi baharu** — menaikkan tepat satu versi semasa, menurunkan yang lama, dan tidak melanggar indeks unik ditapis.

Ini ujian yang mengesahkan reka bentuk Hari 7–9 dan Hari 10–12. Jika seseorang "mengoptimum" dengan menyimpan bendera pematuhan (silap biasa), ujian ini menangkapnya.

## Ukur dahulu, kemudian betulkan

Optimasi tanpa pengukuran ialah tekaan. Hidupkan logging SQL EF Core, jana data ujian, dan **baca SQL yang dijana** untuk setiap skrin. Cari N+1 (biasanya pada `PolicyVersion` atau `Division`) dan projection yang menarik lebih daripada yang dipaparkan. Betulkan apa yang anda **ukur**, bukan apa yang anda syak.

## Kejujuran mengatasi kelihatan siap

Tandakan setiap item backlog dengan jujur: siap ✅, siap-dengan-pepijat 🔧, atau belum siap ⏸️. Kerja belum siap **dipindah ke backlog, bukan dimulakan** dalam dua hari ini. Hari 15 akan mendedahkan realiti — lebih baik anda mendedahkannya dahulu.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — bahan **jurulatih sahaja**.
