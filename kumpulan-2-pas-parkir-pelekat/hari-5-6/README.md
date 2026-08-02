# Kumpulan 2 · Hari 5–6 — Borang Permohonan & Semakan Pendua No. Plat

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Dua hari. Hujungnya, ketiga-tiga borang berfungsi dengan validation bersyarat, dan sistem **menyekat permohonan pendua bagi nombor plat yang sama**.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Validation tersuai | [learn.microsoft.com/aspnet/core/mvc/models/validation#custom-attributes](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/validation#custom-attributes) |
| `IValidatableObject` | [learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations.ivalidatableobject](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.ivalidatableobject) |
| Query wujud EF Core | [learn.microsoft.com/ef/core/querying/](https://learn.microsoft.com/en-us/ef/core/querying/) |
| Kekangan unik | [learn.microsoft.com/ef/core/modeling/indexes](https://learn.microsoft.com/en-us/ef/core/modeling/indexes) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 5** pagi | Borang pas keselamatan + validation bersyarat mengikut jenis pas |
| **Hari 5** petang | Borang pelekat kenderaan + pendaftaran kenderaan |
| **Hari 6** pagi | **Semakan pendua nombor plat** — teras modul anda |
| **Hari 6** petang | Borang parkir + hantar + nombor rujukan + gabungan latihan |

**Hasil:** Tiga borang menyimpan data sah; permohonan pendua disekat dengan mesej yang menamakan konflik; penghantaran menjana `PAS`/`STK`/`PKR-2026-####`.

---

## Validation bersyarat: peraturan bergantung pilihan

Borang pas keselamatan mempunyai peraturan yang **berubah mengikut jenis pas**:

| Medan | Staf | Pelawat | Kontraktor |
|-------|------|---------|------------|
| Nama pemegang | ✅ Wajib | ✅ Wajib | ✅ Wajib |
| Tujuan lawatan | ⬜ Pilihan | ✅ Wajib | ✅ Wajib |
| Nama syarikat | ❌ Tidak berkaitan | ⬜ Pilihan | ✅ Wajib |
| Tempoh sah | Sehingga tamat perkhidmatan | Maks 7 hari | Maks 90 hari |

`[Required]` tidak boleh menyatakan "wajib **bila** jenis ialah Kontraktor". Dua cara melakukannya:

| Pendekatan | Bila guna |
|------------|-----------|
| `IValidatableObject.Validate()` | Peraturan merentas medan dalam satu view model — **kita guna ini** |
| Atribut validation tersuai | Peraturan yang berulang merentas banyak view model |

`IValidatableObject` menang di sini kerana peraturan adalah khusus kepada borang ini dan melibatkan **hubungan antara medan**.

## Semakan pendua: apa yang sebenarnya "pendua"

Ini keperluan teras modul anda, dan definisinya lebih halus daripada yang kelihatan.

**Bukan pendua:**
- Kenderaan yang sama, pelekat 2025 tamat tempoh, memohon pelekat 2026 → **dibenarkan**
- Kenderaan yang sama, permohonan sebelum ini `Rejected` → **dibenarkan**
- Kenderaan yang sama, permohonan sebelum ini `Cancelled` → **dibenarkan**

**Pendua:**
- Kenderaan yang sama, tahun yang sama, permohonan sedia ada berstatus `Submitted`, `SupervisorApproved`, atau `AdminApproved` → **disekat**

Dengan kata lain: **satu permohonan aktif setiap kenderaan setiap tahun**. Status terminal (`Rejected`, `Cancelled`, `Completed` bagi tahun lepas) tidak menyekat.

```csharp
private static readonly SubmissionStatus[] StatusAktif =
[
    SubmissionStatus.Submitted,
    SubmissionStatus.SupervisorApproved,
    SubmissionStatus.AdminApproved
];
```

> **Salah paling biasa:** menyekat pada mana-mana permohonan sedia ada. Ini bermakna permohonan yang ditolak mengunci kenderaan selamanya, dan pemohon tidak boleh membetulkan serta menghantar semula. Ia kelihatan berfungsi dalam ujian dan gagal dalam pengeluaran.

## Dua lapisan pertahanan

Semakan pendua wujud pada **dua** peringkat, dan kedua-duanya perlu:

| Lapisan | Apa ia tangkap | Mesej |
|---------|----------------|-------|
| **Semakan aplikasi** (`AnyAsync`) | Kes biasa | Mesej mesra: "Kenderaan WXY1234 sudah ada permohonan pelekat 2026 (STK-2026-0042)" |
| **Kekangan unik DB** | Perlumbaan, pepijat, akses langsung | Pengecualian — tidak sepatutnya berlaku |

Lapisan aplikasi memberi mesej berguna. Lapisan pangkalan data memastikan data **tidak pernah** salah, walaupun kod salah.

Peraturan: **semak dalam kod untuk pengalaman pengguna; kekang dalam pangkalan data untuk kebenaran.**

## Mesej ralat mesti berguna

Bandingkan:

| ❌ | ✅ |
|---|---|
| "Permohonan pendua." | "Kenderaan **WXY 1234** sudah mempunyai permohonan pelekat aktif untuk tahun 2026 (**STK-2026-0042**, status: Dihantar). Sila semak permohonan tersebut atau batalkannya sebelum memohon semula." |

Mesej kedua memberitahu pengguna **apa** yang bertindih, **rujukan mana**, dan **apa yang perlu dilakukan**. Ini mengurangkan panggilan telefon ke Bahagian Keselamatan.

## Pendaftaran kenderaan berlaku semasa permohonan

Pemohon tidak "mendaftar kenderaan" sebagai langkah berasingan — itu skrin tambahan yang tiada siapa akan gunakan. Sebaliknya:

```text
Borang pelekat → pengguna menaip nombor plat
              → RegisterOrGetAsync menemui atau mencipta Vehicle
              → permohonan memaut kepadanya
```

Jika plat sudah didaftar oleh **staf lain**, itu bukan kemudahan — itu isu keselamatan sebenar. Servis melontar dengan mesej yang menghalakan pengguna ke Bahagian Keselamatan.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
