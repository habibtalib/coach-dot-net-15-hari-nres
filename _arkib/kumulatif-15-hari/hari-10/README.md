# Hari 10 — PKS: Model Pematuhan

Nota ini mengikut **HARI 10** dalam [`../JADUAL.md`](../JADUAL.md) — SESI 29–31 (Modul 4: PKS/Pematuhan Kod Setia). Lab hands-on penuh ada di [`snippets/lab.md`](./snippets/lab.md).

> **Konvensyen kod:** Nota dalam **Bahasa Melayu**; kod, nama kelas/pembolehubah, istilah teknikal dalam **Bahasa Inggeris** — rujuk [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) untuk kanun nama entiti/enum/peranan/prefix.

> **Sambungan projek:** Kita **tidak** mula projek baharu. `Nres.Onboarding.Web` yang sama dari Hari 1–9 (dengan `Submission`, `Attachment`, `AuditLog`, `SubmissionStatus`, `UserProfile`, `ApplicationDbContext`, Modul 1–3 lengkap) kita **tambah** Modul 4 (PKS) di atasnya. Jangan cipta semula `Submission`/`AuditLog`/`ApplicationDbContext` — kita hanya **tambah** entiti baharu dan **daftarkan** ke dalam `DbContext` yang sedia ada.

---

## Fokus Hari Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| EF Core Relationships (overview) | [learn.microsoft.com/ef/core/modeling/relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships) |
| One-to-many relationships | [learn.microsoft.com/ef/core/modeling/relationships/one-to-many](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-many) |
| One-to-one relationships | [learn.microsoft.com/ef/core/modeling/relationships/one-to-one](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/one-to-one) |
| Required & optional foreign keys | [learn.microsoft.com/ef/core/modeling/relationships/foreign-and-principal-keys](https://learn.microsoft.com/en-us/ef/core/modeling/relationships/foreign-and-principal-keys) |
| Data seeding (`HasData`) | [learn.microsoft.com/ef/core/modeling/data-seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding) |
| Migrations (`dotnet ef migrations add`) | [learn.microsoft.com/ef/core/managing-schemas/migrations](https://learn.microsoft.com/en-us/ef/core/managing-schemas/migrations/) |
| `dotnet ef` CLI rujukan penuh | [learn.microsoft.com/ef/core/cli/dotnet](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) |
| Filtered `Include` (koleksi bersyarat) | [learn.microsoft.com/ef/core/querying/related-data/eager#filtered-include](https://learn.microsoft.com/en-us/ef/core/querying/related-data/eager#filtered-include) |

---

## Jadual Hari Ini

| Masa | Agenda |
|------|--------|
| 8.30 – 9.00 pagi | Pendaftaran & Minum Pagi |
| **9.00 – 1.00 tgh** | **SESI 29–30: Model PKS** — `PolicyVersion`, `ComplianceChecklistItem`, `ComplianceDeclaration`, `ComplianceResponse`; simpan versi polisi dengan setiap declaration. 💻 **Lab:** entiti PKS |
| 1.00 – 2.30 petang | Rehat & Makan Tengah Hari |
| **2.30 – 5.00 petang** | **SESI 31: Seed Data** — seed versi polisi & item checklist dalam DB. 💻 **Lab:** seed + migration |
| 5.00 petang | Bersurai |

Hari ini **tidak** merangkumi borang checklist dinamik atau kunci selepas hantar — itu Hari 11. Fokus semata-mata pada **bentuk data** (model), hubungan antara jadual, dan **seed** data rujukan.

---

## Kenapa `PolicyVersion` berasingan daripada `ComplianceChecklistItem` yang hardcode dalam kod?

Kod Etika Perkhidmatan Awam dan peraturan pematuhan dalaman NRES **berubah dari semasa ke semasa** — pekeliling baharu dikeluarkan, item checklist ditambah/dipinda/dinyahaktifkan. Jika senarai soalan checklist ditulis terus dalam kod C# (`if`/array statik), setiap pindaan kecil memerlukan **build & deploy semula** aplikasi. Dengan menyimpan checklist dalam jadual `ComplianceChecklistItems` (dipautkan kepada `PolicyVersion`), `SystemAdmin`/`ComplianceAdmin` boleh kemas kini kandungan checklist **hanya dengan mengemas kini data**, bukan kod.

```text
PolicyVersion (1)
  └── ComplianceChecklistItem (banyak)

ComplianceDeclaration (1)
  └── ComplianceResponse (banyak)

ComplianceDeclaration ── (1-ke-1) ──> Submission   (rekod induk kongsi, dari Hari 1)
ComplianceDeclaration ── (banyak-ke-1) ──> PolicyVersion
ComplianceResponse ── (banyak-ke-1) ──> ComplianceChecklistItem
```

## Kenapa `ComplianceDeclaration` mesti simpan `PolicyVersionId`, bukan sekadar rujuk "checklist terkini"?

Ini keputusan seni bina **paling penting** hari ini. Bayangkan senario: seorang staf mengisytiharkan pematuhan pada bulan Mac 2026 menggunakan checklist versi `PKS-POL-2026.1` (6 perkara). Pada bulan Jun 2026, jabatan pematuhan mengeluarkan versi polisi baharu `PKS-POL-2026.2` (8 perkara, 2 tambahan baharu). Jika `ComplianceDeclaration` **tidak** menyimpan versi polisi yang digunakan semasa ia dihantar, dan sistem hanya memaparkan checklist "semasa" untuk semua rekod lama, maka:

- Rekod Mac 2026 akan kelihatan **tidak lengkap** (2 perkara baharu tiada jawapan) — walaupun staf itu telah lengkap mematuhi keperluan yang **sah pada masa itu**.
- Audit/legal semakan tidak dapat membuktikan checklist mana yang sebenarnya ditandatangani staf tersebut.

Dengan `ComplianceDeclaration.PolicyVersionId` (kekal, tidak berubah selepas dihantar), setiap declaration **secara kekal** merujuk kepada checklist yang **sah pada tarikh ia dihantar** — walaupun `PolicyVersion` lama kemudian ditanda `IsActive = false`. Ini corak **snapshot bersejarah** (*historical snapshot*), lazim dalam sistem kerajaan/korporat yang perlu patuh audit.

## Kenapa `ComplianceResponse` berasingan bagi setiap item checklist, bukan satu medan teks panjang?

Alternatif malas ialah simpan semua jawapan sebagai satu lajur `string Notes` dalam `ComplianceDeclaration` (cth. `"1:Ya;2:Ya;3:Tidak - sebab..."`). Ini **kelihatan** ringkas tetapi tidak boleh:

- Ditapis ("papar semua declaration dengan sekurang-kurangnya satu item Tidak Patuh" — keperluan Hari 12).
- Disahkan secara berstruktur (setiap jawapan `bool` `IsCompliant` + `string? Remarks` berasingan).
- Dikira secara agregat (cth. "berapa peratus staf Tidak Patuh perkara #3 tahun ini?").

Dengan `ComplianceResponse` sebagai jadual berasingan (satu baris = satu jawapan bagi satu `ComplianceChecklistItem`), setiap jawapan menjadi **data berstruktur** yang boleh ditapis, dikira, dan dilaporkan — persis keperluan semakan admin Hari 12.

## Kenapa migration hari ini dipanggil `Module4Initial`?

Mengikut corak yang ditetapkan sejak Hari 4 (`Module2Initial`): setiap modul baharu mendapat **migration sendiri** dinamakan mengikut modul, bukan diedit terus ke dalam migration `InitialShared` (Hari 1) atau migration modul lain yang sudah digunakan (`dotnet ef database update` sudah dijalankan). Migration hari ini kita namakan **`Module4Initial`** — mengandungi jadual `PolicyVersions`, `ComplianceChecklistItems`, `ComplianceDeclarations`, `ComplianceResponses`, **serta** data seed (versi polisi + 6 item checklist) melalui `HasData`.

## Kenapa seed guna `HasData` dalam `OnModelCreating`, bukan kod C# yang jalan semasa `app.Run()`?

`HasData` ialah cara EF Core memasukkan **data rujukan tetap** (*reference/lookup data*) terus ke dalam fail migration itu sendiri — data itu menjadi sebahagian daripada sejarah skema, dijana semula secara konsisten pada **setiap** persekitaran (makmal latihan, staging, pengeluaran) apabila `dotnet ef database update` dijalankan. Ini sesuai untuk data yang **jarang berubah** dan perlu **sentiasa wujud** (seperti versi polisi asal + checklist rasmi pertama) — berbeza dengan data transaksi (declaration sebenar staf) yang dicipta melalui aplikasi, bukan migration.

> Rujukan rasmi: [learn.microsoft.com/ef/core/modeling/data-seeding](https://learn.microsoft.com/en-us/ef/core/modeling/data-seeding)

---

Selesai baca bahagian konsep? Mula lab hands-on di [`snippets/lab.md`](./snippets/lab.md) — bina 4 entiti PKS, daftar dalam `ApplicationDbContext`, seed versi polisi & checklist, dan jana migration `Module4Initial`.

> 🎤 **Nota penceramah/jurulatih:** [`nota-penceramah.md`](./nota-penceramah.md).
