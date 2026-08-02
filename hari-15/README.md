# Hari 15 — Penggabungan Kod, SIT & Persembahan Demo

Nota ini mengikut **aturcara rasmi HARI 15** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 44 hingga 46. Konsep di sini; hands-on penuh di [`snippets/lab.md`](./snippets/lab.md).

> **Sesi bersama.** Empat kumpulan, satu bilik, satu sistem. Hari ini kita menyatukan 11 hari kerja selari menjadi satu aplikasi NRES yang berfungsi.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Menggabungkan cabang | [git-scm.com/book — Basic Branching and Merging](https://git-scm.com/book/en/v2/Git-Branching-Basic-Branching-and-Merging) |
| Selesaikan konflik | [docs.github.com — addressing merge conflicts](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/addressing-merge-conflicts) |
| Migration EF Core dalam pengeluaran | [learn.microsoft.com/ef/core/managing-schema/migrations/applying](https://learn.microsoft.com/en-us/ef/core/managing-schema/migrations/applying) |
| Tukar penyedia DB | [learn.microsoft.com/ef/core/providers](https://learn.microsoft.com/en-us/ef/core/providers/) |
| Deployment ASP.NET Core | [learn.microsoft.com/aspnet/core/host-and-deploy](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/) |

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 10.30 pagi** | **SESI 44: Penggabungan Repositori** — gabung 4 cabang ke `master` mengikut turutan berjadual, selesaikan konflik, sahkan satu migration bersepadu. 💻 Lab 1–3 |
| **10.30 – 1.00 tgh** | **SESI 45: Papan Pemuka Induk NRES** — navigasi ikut peranan, dashboard peribadi, carian rujukan global merentas modul. 💻 Lab 4–5 |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 3.45 petang** | **SESI 46: SIT & UAT Pre-Check** — skrip ujian rentas modul, semakan RBAC, muat naik fail, audit log. 💻 Lab 6–7 |
| **3.45 – 5.00 petang** | **Demo & Penilaian Capstone** — pembentangan kumpulan, penilaian, nota deployment, sijil |
| 5.00 petang | Bersurai |

**Hasil Hari 15:** Satu aplikasi NRES bersepadu 4 modul pada `master`, lulus SIT, dibentangkan oleh keempat-empat kumpulan.

---

## Kenapa hari ini sepatutnya membosankan

Jika kursus ini berjalan seperti yang direka, penggabungan pagi ini mengambil masa **kurang daripada sejam** dan menghasilkan sedikit kejutan.

Sebabnya bukan nasib. Ia terkumpul daripada keputusan yang dibuat sejak Hari 2:

| Keputusan | Kesan hari ini |
|-----------|----------------|
| Modul mendaftar diri (`Add<Modul>Module`) | `Program.cs` tidak pernah dipertikaikan |
| `ApplyConfigurationsFromAssembly()` | `ApplicationDbContext` tidak pernah berkonflik |
| Navigasi didorong `ModuleDescriptor` | `_Layout.cshtml` tidak pernah disunting |
| Slot migration | Snapshot kekal koheren |
| Matriks pemilikan fail | Kumpulan tidak menulis di kawasan satu sama lain |
| Gabungan latihan setiap blok | `master` sudah mengandungi kebanyakan kerja |

**Kalau hari ini ternyata sukar**, itu maklumat berguna — nyatakan **di mana** ia gagal semasa retrospektif. Itu pengajaran sebenar, bukan kegagalan.

## Turutan gabungan penting

Kita **tidak** menggabungkan keempat-empat cabang serentak. Kita menggabungkan satu demi satu, mengesahkan selepas setiap satu:

```text
master ← kump-N (gabung) → dotnet build → dotnet test → jalankan aplikasi → ✅
       ← kump-M (gabung) → ...
```

**Kenapa?** Jika sesuatu pecah selepas gabungan ketiga, anda tahu kumpulan mana yang menyebabkannya. Gabung keempat-empatnya serentak dan anda sedang mencari dalam empat set perubahan sekaligus.

Turutan dipilih **mengikut risiko**: gabungan kering paling bersih dahulu, paling berisiko terakhir — supaya masalah muncul apabila `master` sudah terbukti berfungsi dengan tiga modul.

## Migration selepas gabungan

Setiap kumpulan menjana migration pada cabangnya. Selepas gabungan, `master` mempunyai kesemuanya — dijalankan mengikut cap masa.

Dua perkara untuk disahkan:

1. **`dotnet ef database update` pada pangkalan data kosong berjaya.** Ini menguji keseluruhan rantaian migration daripada kosong — tepat apa yang deployment sebenar lakukan.
2. **Snapshak model sepadan dengan model.** Jalankan `dotnet ef migrations add Semakan` — jika ia menjana migration **kosong**, semuanya konsisten. Jika ia menjana perubahan, seseorang mengubah entiti tanpa migration. Buang migration ujian selepas menyemak.

Trik kedua itu ialah semakan kewarasan yang cepat dan bernilai.

## Papan Pemuka Induk — apa yang kita dapat secara percuma

Kerana keempat-empat modul mendaftarkan `ModuleDescriptor`, dashboard induk sudah **tahu** tentangnya. Kita tidak menulis kod khusus modul.

| Ciri | Cara ia berfungsi merentas modul |
|------|----------------------------------|
| Navigasi ikut peranan | Kumpulkan semua `IModuleDescriptorProvider`, tapis ikut peranan |
| Dashboard peribadi | Query `Submissions` mengikut `ApplicantUserId` — modul-agnostik |
| Baris gilir kelulusan saya | Query mengikut `Status` + peranan pengguna vs `ModuleCode` |
| Carian rujukan global | `Submissions.Where(s => s.ReferenceNo.Contains(cari))` |

**Ini bayaran balik `Submission` induk kongsi.** Jika setiap modul mempunyai jadual dan enum statusnya sendiri, setiap satu daripada ciri ini memerlukan empat pelaksanaan dan satu `switch`. Sebutkan ini kepada peserta — ia menutup gelung kembali ke bengkel Hari 1.

## SIT: menguji sistem, bukan modul

Setiap kumpulan telah menguji modulnya. SIT menguji apa yang **tiada siapa** menguji: sempadan antara modul.

Skrip SIT teras kita mengikut **satu pekerja baharu** melalui keempat-empat modul:

```text
1. Ali melapor diri              (Modul 1 — HR meluluskan)
2. Ali memohon pas & pelekat     (Modul 2 — Keselamatan meluluskan, QR dijana)
3. Ali memohon akaun AD & e-mel  (Modul 3 — Penyelia → ICT meluluskan)
4. Ali meminjam laptop           (Modul 4 — ICT meluluskan, stok berkurang)
5. Ali memulangkan laptop        (Modul 4 — kondisi disemak, stok pulih)
```

Kemudian sahkan sistem **secara keseluruhan**:

- Dashboard Ali menunjukkan kesemua lima permohonan
- Carian global menemui mana-mana nombor rujukan
- Setiap peranan admin melihat **hanya** modulnya
- Setiap tindakan muncul dalam audit log
- Muat turun fail dilindungi merentas keempat-empat modul

## UAT pre-check ≠ UAT

**UAT sebenar** dijalankan oleh pengguna NRES sebenar terhadap keperluan mereka sendiri. Kita tidak boleh melakukannya di sini.

Apa yang kita lakukan ialah **pre-check**: mengesahkan bahawa sistem sedia untuk UAT sebenar. Jika ia gagal di sini, ia pasti gagal dengan pengguna sebenar.

Nyatakan perbezaan ini dengan jelas kepada peserta — dan kepada NRES dalam laporan akhir.

## Deployment: SQLite → SQL Server

Menukar penyedia ialah satu baris:

```csharp
options.UseSqlite(cs)      →      options.UseSqlServer(cs)
```

**Tetapi itu bukan keseluruhan cerita**, dan kita katakan itu dengan jujur:

| Perbezaan | Kesan |
|-----------|-------|
| Migration khusus penyedia | Jana semula migration untuk SQL Server |
| Indeks bertapis | Sintaks berbeza (`HasFilter`) |
| Fungsi tarikh (`DateDiffDay`) | Diterjemah pada SQL Server, tidak pada SQLite |
| Sensitiviti huruf besar/kecil | SQLite dan SQL Server berbeza secara lalai |
| Concurrency | SQLite mengunci fail; SQL Server tidak |

Pengajarannya: EF Core menjadikan penukaran **mungkin**, bukan **automatik**. Anda menguji selepas menukar.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
