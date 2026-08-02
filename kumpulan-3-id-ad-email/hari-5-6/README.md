# Kumpulan 3 · Hari 5–6 — Borang Permohonan & Kelulusan Penyelia

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Dua hari. Hujungnya, staf boleh memohon akaun dengan akses berbilang, dan **Penyelia Jabatan boleh meluluskan peringkat 1**.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| Borang dinamik & senarai | [learn.microsoft.com/aspnet/core/mvc/views/working-with-forms](https://learn.microsoft.com/en-us/aspnet/core/mvc/views/working-with-forms) |
| Model binding koleksi | [learn.microsoft.com/aspnet/core/mvc/models/model-binding](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/model-binding) |
| `IValidatableObject` | [learn.microsoft.com/dotnet/api/system.componentmodel.dataannotations.ivalidatableobject](https://learn.microsoft.com/en-us/dotnet/api/system.componentmodel.dataannotations.ivalidatableobject) |
| Role-based authorization | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |
| LINQ & EF Core | Buku Bab 11, *Using LINQ with EF Core* (m.s. 586) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 5** pagi | Borang permohonan — medan bersyarat mengikut jenis |
| **Hari 5** petang | Pemilihan akses berbilang (checkbox + tahap + justifikasi) |
| **Hari 6** pagi | Hantar — cipta laluan kelulusan, jana `ICT-ID-2026-####` |
| **Hari 6** petang | **Skrin kelulusan Penyelia (peringkat 1)** + gabungan latihan |

**Hasil:** Permohonan boleh dihantar dengan akses berbilang; Penyelia melihat baris gilirnya dan boleh meluluskan/menolak peringkat 1; status bergerak ke `SupervisorApproved`.

---

## Borang dengan senarai bersarang: cabaran sebenar

Borang anda bukan senarai medan rata. Ia mengandungi **senarai** akses yang dipohon, setiap satu dengan tahap dan justifikasinya sendiri.

```text
Permohonan
├── Maklumat staf (medan rata)
├── Penyelia (dropdown)
└── Akses dipohon (SENARAI)
    ├── [✓] AD          · Tahap: —          · Justifikasi: —
    ├── [✓] E-mel       · Tahap: —          · Justifikasi: —
    ├── [ ] VPN         · Tahap: —          · Justifikasi: (wajib jika ditanda)
    └── [✓] HRMIS       · Tahap: Baca-tulis · Justifikasi: —
```

ASP.NET Core mengikat senarai melalui **nama medan berindeks**:

```html
<input name="Akses[0].Dipilih" />
<input name="Akses[0].SystemAccessId" />
<input name="Akses[1].Dipilih" />
```

Indeks mesti **berturutan bermula dari 0**, tanpa jurang — jika tidak model binding berhenti pada jurang pertama. Kerana kita menjana baris daripada senarai lookup tetap, ini mudah: gunakan indeks gelung.

## Validation merentas senarai

Peraturan anda bergantung pada **gabungan** pilihan:

| Peraturan | Kenapa |
|-----------|--------|
| Sekurang-kurangnya satu akses mesti dipilih | Permohonan kosong tidak bermakna |
| Akses bertanda `PerluJustifikasi` mesti ada justifikasi | VPN dan folder kewangan sensitif |
| `AkaunBaharu` mesti termasuk AD dan E-mel | Anda tidak boleh mempunyai akaun tanpa keduanya |
| `Nyahaktif` mesti ada `TarikhTamat` | Bila akaun ditutup |
| Penyelia tidak boleh sama dengan pemohon | Elak kelulusan sendiri |

Kesemuanya dalam `IValidatableObject.Validate()` — sama seperti Kumpulan 2, tetapi merentas **senarai** dan bukan hanya medan.

> Peraturan terakhir — **penyelia ≠ pemohon** — patut anda perhatikan. Ini contoh khusus masalah "kelulusan sendiri" yang keempat-empat modul kongsi. Jika kumpulan lain juga menghadapinya, ia calon isu `shared`.

## Kelulusan peringkat 1: kenapa bukan `base.Approve`

`SubmissionControllerBase.Approve` menetapkan status kepada `AdminApproved` — betul untuk **peringkat akhir**, salah untuk peringkat 1.

Anda **menambah** tindakan berasingan:

```csharp
[Authorize(Roles = "Supervisor")]
public async Task<IActionResult> SupervisorApprove(int id, string? remarks)
```

...yang memanggil `IWorkflowService.TransitionAsync(..., SubmissionStatus.SupervisorApproved, ...)` terus.

**Ini bukan menulis semula kelas asas.** Anda tidak menyentuh `Approve` atau `Reject` — anda menambah tindakan ketiga yang kelas asas tidak pernah cuba sediakan. `base.Approve` kekal untuk peringkat 2 (ICT), yang anda gunakan pada Hari 7–9.

| Tindakan | Peranan | Status hasil | Datang dari |
|----------|---------|--------------|-------------|
| `SupervisorApprove` | `Supervisor` | `SupervisorApproved` | **Ditulis oleh anda** |
| `Reject` | `Supervisor` atau `IctAdmin` | `Rejected` | Kelas asas |
| `Approve` (peringkat 2) | `IctAdmin` | `AdminApproved` | Kelas asas |

> **Masalah:** `Reject` kelas asas menyemak `AdminRole` (`IctAdmin`) — jadi Penyelia tidak boleh menolak. Anda perlu mengatasinya untuk membenarkan **kedua-dua** peranan menolak pada peringkat mereka. Ini contoh baik `override` + `base.` yang Kumpulan 2 pelajari.

## Baris gilir Penyelia

Penyelia perlu melihat: *permohonan yang menunggu kelulusan **saya***.

Itu bermakna: `AccountRequest.SupervisorUserId == pengguna semasa` **dan** langkah 1 masih `Pending`.

```csharp
where a.SupervisorUserId == userId
   && s.Status == SubmissionStatus.Submitted
```

Indeks pada `SupervisorUserId` yang anda cipta pada Hari 4 menyokong tepat query ini.

## Apa yang Penyelia sebenarnya nilai

Penyelia bukan ICT. Mereka **tidak** menilai sama ada akses secara teknikal munasabah — mereka menilai sama ada **staf ini, dalam peranan ini, memerlukan akses ini**.

Skrin mereka mesti menunjukkan:

- Siapa staf itu, jawatan dan jabatannya
- Akses apa yang dipohon dan **justifikasi** setiap satu
- Jenis permohonan (baharu / tukar / nyahaktif)

Bukan: butiran teknikal AD, nama pelayan, konfigurasi. Itu skrin ICT (Hari 7–9).

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
