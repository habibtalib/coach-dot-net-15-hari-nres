# Contoh PRD — Tempahan Fasiliti Sukan

> **Bahan rujukan kursus (Hari 1 · dokumentasi).** Ini **contoh** PRD sintetik untuk menunjukkan rupa PRD yang **boleh-bina**. Ia mengikut anatomi 7 bahagian yang diajar dalam slaid *"Anatomi PRD ringkas"*.
>
> **Sumber:** dibina daripada [`kumpulan-4-tempahan-fasiliti-sukan/AGENTS.md`](../kumpulan-4-tempahan-fasiliti-sukan/AGENTS.md) (kanun teknikal sistem) + [`_sumber-urs/K4-fasiliti-sukan.md`](../_sumber-urs/K4-fasiliti-sukan.md) (URS/persona). Kanun tunggal: [`SPEC-KURSUS.md`](../SPEC-KURSUS.md).
>
> ⚠️ **Data sintetik sahaja.** Jangan tampal data NRES sebenar. PRD **tidak mereka keperluan** — ia menterjemah URS kepada bentuk boleh-bina.

| Perkara | Nilai |
|---------|-------|
| Modul / sistem | Tempahan Fasiliti Sukan |
| Repo | `nres-bpm/tempahan-fasiliti-sukan` · subdomain `fasiliti.` |
| Prefix no. rujukan | `TFS` → `TFS-2026-0001` |
| Peranan penyemak | `FacilityAdmin` |
| Akses | Dalaman (SSO) — baca profil pengguna |
| Versi PRD | v0.1 (draf) · Pemilik: Kumpulan 4 |

---

## 1 · Masalah & matlamat

**Masalah (persona — Encik Faizal, Setiausaha Kelab Sukan):** "Saya tidak tahu slot mana yang kosong; tempahan sering bertindih; prosesnya manual — WhatsApp, buku log, telefon pengawal."

**Matlamat:** staf boleh menempah gelanggang/kemudahan sukan dalam talian tanpa bertindih, dan `FacilityAdmin` boleh meluluskan/menolak dengan pandangan ketersediaan yang jelas — menggantikan proses manual.

**Ukuran kejayaan:**
- 0 tempahan bertindih diluluskan (dikuatkuasa sistem, bukan manual).
- Pemohon nampak status permohonan (Draft → Submitted → Approved/Rejected) tanpa bertanya admin.
- Semua keputusan kelulusan direkod dalam audit trail.

## 2 · Pengguna & peranan

| Peranan | Siapa | Boleh buat |
|---------|-------|-----------|
| `Applicant` | Staf / setiausaha kelab | Cipta draf, hantar, batal, lihat status permohonan sendiri |
| `FacilityAdmin` | Pentadbir fasiliti | Semak, luluskan/tolak, lihat kalendar ketersediaan, urus katalog fasiliti |

*Peranan dari profil pengguna (SSO). Rujuk senarai peranan muktamad dalam `SPEC-KURSUS.md`.*

## 3 · Skop & luar-skop

**Dalam skop (MVP):**
- Katalog fasiliti + slot masa (seed).
- Borang tempahan (pilih fasiliti + tarikh + slot + tujuan + bilangan peserta).
- **Semakan slot bertindih** semasa `Submit` **dan** semasa `Approve`.
- Kelulusan `FacilityAdmin` (lulus/tolak + sebab) + audit.
- Kalendar/paparan ketersediaan slot.

**Luar-skop (tulis eksplisit supaya AI/pasukan tidak reka):**
- ❌ Pembayaran / caj sewaan fasiliti.
- ❌ Pinjaman **peralatan** sukan (katalog stok, pinjam/pulang) — lihat *Soalan terbuka*.
- ❌ Sokongan Ketua Jabatan / aliran kelulusan berbilang peringkat.
- ❌ Notifikasi SMS/e-mel sebenar (latihan guna `ConsoleNotificationService`).

## 4 · User stories + acceptance criteria

> Format: *Sebagai `<peranan>`, saya mahu `<sesuatu>` supaya `<manfaat>`.*
> **Acceptance criteria (AC) = ujian.** Ia menjadi Definition of Done + kes ujian xUnit + kriteria semakan AI.

**US-1 — Mohon tempahan**
Sebagai `Applicant`, saya mahu menempah slot fasiliti supaya gelanggang terjamin untuk aktiviti saya.
- [ ] AC1: borang wajib ada fasiliti, tarikh, slot, tujuan, bilangan peserta; validation di **pelayan** (`ModelState.IsValid`).
- [ ] AC2: no. rujukan `TFS-YYYY-####` dijana automatik pada `Submit`.
- [ ] AC3: jika slot **bertindih** dengan tempahan sedia ada (status bukan `Rejected`/`Cancelled`), `Submit` ditolak dengan mesej jelas.
- [ ] AC4: permohonan disimpan sebagai `Draft` sehingga `Submit` (status → `Submitted`).

**US-2 — Semak & luluskan**
Sebagai `FacilityAdmin`, saya mahu menyemak permohonan supaya hanya tempahan sah diluluskan.
- [ ] AC1: hanya `FacilityAdmin` boleh akses skrin semakan (`[Authorize(Roles = "FacilityAdmin")]`).
- [ ] AC2: **semak-semula** slot bertindih pada `Approve` (keadaan mungkin berubah sejak `Submit`); jika bertindih, halang kelulusan.
- [ ] AC3: `Approve` → status `AdminApproved`/`Completed`; `Reject` wajib sebab; status → `Rejected`.
- [ ] AC4: setiap tindakan (submit/approve/reject) direkod dalam `AuditLog`.

**US-3 — Lihat ketersediaan**
Sebagai `Applicant`, saya mahu melihat slot yang sudah ditempah supaya saya pilih slot kosong.
- [ ] AC1: kalendar/paparan menunjukkan slot yang telah ditempah bagi fasiliti terpilih.
- [ ] AC2: slot yang sudah penuh tidak boleh dipilih dalam borang.

## 5 · Data & entiti

Ikut corak `Submission` induk (rujuk `AGENTS.md` sistem ini). **Jangan pendua** `ReferenceNo`/`Status`/tarikh ke entiti modul — ia hidup di `Submission`.

| Entiti | Nota |
|--------|------|
| `Submission` (induk) | `ReferenceNo`, `SubmissionStatus`, `ApplicantUserId`, tarikh, audit |
| `SportsFacility` | Katalog: nama, jenis, lokasi, kapasiti (1—N `FacilityBookingSlot`) |
| `FacilityBookingSlot` | Slot masa: `StartTime`/`EndTime` bagi satu fasiliti |
| `FacilityBookingApplication` | Detail: memaut ke `Submission` via `SubmissionId`; rujukan fasiliti + slot + tujuan + bilangan peserta |

```csharp
public enum SubmissionStatus
{
    Draft = 0, Submitted = 1, SupervisorApproved = 2, AdminApproved = 3,
    Rejected = 4, Completed = 5, Cancelled = 6
}
```

**Nota reka bentuk (dari URS):** slot bertindih **tidak boleh** dijadikan indeks unik pangkalan data (ia julat masa, bukan nilai tepat) → **semakan aplikasi dalam servis** ialah satu-satunya pertahanan. Laksana dalam `IBookingService.HasSlotClash(...)`.

## 6 · Bukan-fungsi & polisi

- **Keselamatan:** authorization berasaskan peranan; setiap tindakan mengubah keadaan mesti semak peranan di pelayan.
- **Validation:** semua peraturan (termasuk slot bertindih) disahkan di **pelayan** — jangan bergantung pada UI sahaja.
- **Bahasa:** UI/nota Bahasa Melayu; kod/nama entiti Bahasa Inggeris.
- **Data:** sintetik sahaja; tiada data NRES sebenar.
- **Storan fail:** lampiran (jika ada) di `App_Data/uploads/{submissionId}/`, bukan bawah `wwwroot`.
- **Audit:** setiap peralihan status direkod melalui `IAuditLogService`.

## 7 · Soalan terbuka

- [ ] **Skop belum disahkan NRES:** "Tempahan Fasiliti" (slot bertindih) **atau** "Peralatan Sukan" (pinjam/pulang stok)? Folder sumber zip kosong — lab semasa menganggap **Tempahan**. Sahkan sebelum mendalami. *(Rujuk `_sumber-urs/K4-fasiliti-sukan.md`.)*
- [ ] Adakah slot berkokok tetap (cth 1 jam) atau julat masa bebas yang dipilih pemohon?
- [ ] Perlukah had bilangan tempahan aktif setiap pemohon?
- [ ] Perlukah kalendar menunjukkan fasiliti merentas jabatan, atau per jabatan sahaja?

---

## Bagaimana PRD ini mengalir (rujuk slaid *"Aliran: PRD → Jira → AI → kod"*)

1. **URS** → **PRD ini** (terjemahan boleh-bina).
2. Setiap **user story** (US-1…US-3) → **task/story Jira** dalam projek `FS` (board `bpm-nres`), AC menjadi acceptance criteria kad.
3. **Prompt AI** untuk setiap story — dalam bahasa biasa: nyatakan modul, apa yang hendak dibina, dan peraturan yang mesti dipatuhi; minta rancangan dahulu.
4. **PR + review** → merge; **AC = Definition of Done** (bukan sekadar kod ditulis).

**Contoh prompt (US-1, berpandukan PRD ini):**
> "Untuk modul Tempahan Fasiliti Sukan, buat fungsi tempahan: pemohon memilih fasiliti, tarikh dan slot masa, dan sistem menolak permohonan jika slot itu sudah ditempah. Ikut peraturan dan corak sedia ada dalam projek ini, dan tunjukkan rancangan anda dahulu sebelum menulis kod."
