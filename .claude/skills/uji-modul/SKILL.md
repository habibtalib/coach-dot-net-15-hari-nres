---
name: uji-modul
description: >-
  Tulis & semak ujian xUnit modul NRES (Hari 13–14) — peralihan SubmissionStatus,
  nombor rujukan, semakan pendua, kebenaran peringkat objek. SQLite in-memory.
  Guna semasa blok ujian atau sebelum menyediakan modul untuk gabung.
---

# Uji Modul (xUnit)

Tulis/semak ujian **peraturan perniagaan kritikal** modul. Jalankan `dotnet test` — laporkan keputusan.

1. **Peraturan kritikal diuji** — peralihan `SubmissionStatus` (`Draft→Submitted→AdminApproved/Rejected`, termasuk peralihan terlarang mesti gagal), nombor rujukan (`GenerateAsync` berjujukan), semakan **pendua (dua arah)**.
2. **SQLite in-memory** (`Microsoft.Data.Sqlite`) — **bukan** `UseInMemoryDatabase` (ia abaikan kekangan unik seperti indeks unik `ReferenceNo`).
3. **Kebenaran peringkat objek** diuji via **POST terus** → `403`/AccessDenied — bukan sekadar mengklik UI.
4. **`[Fact]`** untuk kes tunggal; **`[Theory]` + `[InlineData]`** untuk kes berparameter (mis. senarai peralihan sah/tak sah).
5. **Jalankan `dotnet test`** — mesti **hijau**. Ujian merah = belum siap.

**Output:** senarai ujian + keputusan (lulus/gagal) + isu ditemui. **Jangan** ubah kod ciri — serah kepada penulis untuk betulkan.
