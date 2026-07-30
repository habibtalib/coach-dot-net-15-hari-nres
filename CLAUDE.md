# CLAUDE.md — Panduan Repo Kursus DOTNET-NRES-15

Repo bahan latihan **coaching .NET 15 hari** untuk NRES. Bukan aplikasi pengeluaran — ia **bahan pengajaran**: nota, lab hands-on, nota penceramah, slaid, dan projek rujukan.

## Apa repo ini

- Membina **satu** aplikasi latihan `Nres.Onboarding.Web` (ASP.NET Core MVC, .NET 10) merentas **5 modul** NRES, secara **kumulatif** sepanjang 15 hari.
- Peserta **bina dari kosong** mengikut `hari-*/snippets/lab.md`; `projek/` ialah rujukan penuh untuk **banding**.

## Sumber kebenaran (baca sebelum edit kandungan)

1. [`SPEC-KURSUS.md`](./SPEC-KURSUS.md) — **kanun tunggal**: nama entiti, `SubmissionStatus`, peranan, prefix nombor rujukan, pemetaan 15 hari, format fail. **Semua kandungan mesti patuh.**
2. [`JADUAL.md`](./JADUAL.md) — aturcara rasmi (46 sesi, waktu). Jangan ubah skop hari tanpa menyemak.
3. Repo jiran `../coach-nres/nres-dotnet-15-day-coaching-guide.md` — sumber domain penuh (contoh entiti, checklist implementasi).

## Konvensyen

- **Bahasa:** nota/agenda dalam **Bahasa Melayu**; kod, nama kelas, istilah teknikal dalam **Bahasa Inggeris**.
- **Struktur setiap hari:** `hari-N/README.md` (konsep) + `hari-N/snippets/lab.md` (hands-on langkah demi langkah) + `hari-N/nota-penceramah.md` (nota penceramah).
- **Fokus:** ≥60% masa hands-on. Lab ialah bahagian paling penting — setiap latihan ada **Objektif**, langkah bernombor, blok kod penuh, dan **✅ Semakan**.
- **Kumulatif:** setiap hari membina di atas hari sebelumnya. Rujuk entiti/servis yang sudah wujud, jangan cipta semula dengan nama berbeza.

## Bila menulis/menyunting kandungan

- Padankan gaya jiran `../kelas-flutter-5-hari/` & `../kelas-n8n-3-hari-jpj/` (README konsep + lab berangka + nota penceramah).
- Guna nama kelas/enum/prefix **tepat** seperti `SPEC-KURSUS.md`. Jika ragu, semak spec dahulu.
- Kod C# mesti sah untuk **.NET 10 / EF Core 10** (contoh: primary constructors, `dotnet ef` CLI, nullable reference types dihidupkan).
- Setiap contoh kod hendaklah **boleh ditaip & dijalankan** oleh peserta — bukan pseudo-kod.

## Slaid

- `slides/dotnet-nres-training.html` — dek self-contained (buka dalam pelayar).
- `slides/build-pptx.py` + `_pptx_lib.py` — jana `.pptx` (venv + `python-pptx`). Kandungan slaid disimpan sebagai data (deterministik).

## Jangan

- Jangan simpan kata laluan sebenar dalam modul ID/AD/Email (ajar peserta **jangan** — ini titik pengajaran keselamatan).
- Jangan guna data NRES sebenar — semua contoh **sintetik**.
- Jangan tukar `SubmissionStatus`, peranan, atau prefix rujukan tanpa mengemas kini `SPEC-KURSUS.md` dahulu.
