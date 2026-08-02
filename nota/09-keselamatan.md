# Keselamatan 🛡️

> Nota konsep merentas **keseluruhan kursus** — bukan satu hari tunggal. Dibaca semula setiap kali modul baharu melibatkan data sensitif, muat naik fail, atau kelulusan. Nota ini merumus & merujuk balik prinsip keselamatan dari [`05-identity-authorization.md`](./05-identity-authorization.md) dan [`06-file-upload.md`](./06-file-upload.md).

---

## Kenapa keselamatan bukan tambahan, tetapi keperluan asas

`Nres.Onboarding.Web` mengendalikan data pekerja kerajaan — No. KP, maklumat kenderaan peribadi, akaun sistem (AD/email), dan pengisytiharan pematuhan. Reka bentuk yang cuai boleh membocorkan data peribadi, membenarkan capaian tanpa kebenaran, atau kehilangan jejak audit yang diperlukan untuk siasatan/pematuhan.

---

## 1. Jangan sesekali simpan kata laluan sebenar

Modul **ID/AD/Email** (Kumpulan 3) mengendalikan `AccountRequest` — permohonan akaun AD/email pengguna baharu. **JANGAN** simpan kata laluan awal/sementara akaun AD dalam pangkalan data aplikasi ini:

```csharp
// ❌ JANGAN — kata laluan AD tidak patut wujud dalam sistem onboarding
public class AccountRequest
{
    public string TemporaryPassword { get; set; }  // ❌ SALAH
}

// ✅ BETUL — sistem ini hanya rekod PERMOHONAN, bukan credential sebenar
public class AccountRequest
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public string RequestedUserPrincipalName { get; set; } = string.Empty;  // cth. "ali.ahmad@nres.gov.my"
    public string Status { get; set; } = "Pending";
    // Kata laluan sebenar dikendalikan oleh Active Directory / sistem IAM, BUKAN aplikasi ini
}
```

Untuk kata laluan **log masuk aplikasi itu sendiri** (`ApplicationUser` melalui ASP.NET Core Identity), Identity meng-*hash* secara automatik (PBKDF2 + salt) — anda **tidak perlu** dan **tidak patut** menulis logik hash sendiri. Lihat [`05-identity-authorization.md`](./05-identity-authorization.md).

---

## 2. Validasi muat naik fail — jenis & saiz

Rujuk penuh di [`06-file-upload.md`](./06-file-upload.md). Ringkasan:

- Had saiz fail (cth. 5MB).
- Sekat jenis fail dibenarkan (`.pdf`, `.jpg`, `.png` sahaja).
- Semak di **server-side** — jangan bergantung validation JavaScript sahaja.

---

## 3. Jangan percaya nama fail yang dimuat naik

Nama fail asal pengguna **hanya untuk paparan**. Simpan fail dengan nama fizikal terjana (GUID), bukan nama asal — mengelakkan serangan *path traversal* dan pertindihan nama. Lihat contoh penuh `FileStorageService` di [`06-file-upload.md`](./06-file-upload.md).

---

## 4. Kuatkuasa authorization di Controller — bukan hanya UI

**Prinsip paling kritikal.** Menyembunyikan pautan menu (`@if (User.IsInRole(...))`) adalah untuk **UX sahaja**. Ia **tidak** menghalang pengguna jahat menaip URL terus.

```csharp
// ✅ Kawalan sebenar — MESTI ada pada setiap Controller/Action sensitif
[Authorize(Roles = "HrAdmin")]
public class ReportingReviewController : Controller
{
    public IActionResult Approve(int id) { /* ... */ }
}
```

```html
@* Menu — kemudahan UX sahaja, BUKAN kawalan keselamatan *@
@if (User.IsInRole("HrAdmin"))
{
    <a asp-controller="ReportingReview" asp-action="Index">Semakan Lapor Diri</a>
}
```

> Kedua-dua lapisan perlu wujud **serentak** — UI untuk pengalaman bersih, Controller untuk penguatkuasaan sebenar. Lihat [`05-identity-authorization.md`](./05-identity-authorization.md) untuk contoh Policy lanjutan.

---

## 5. Audit setiap tindakan penting

Setiap tindakan yang mengubah status permohonan **wajib** direkod dalam `AuditLog` ([`03-corak-workflow.md`](./03-corak-workflow.md)):

| Tindakan wajib diaudit | Contoh `Action` value |
|---|---|
| Cipta draf | `"Created"` |
| Hantar permohonan | `"Submitted"` |
| Lulus | `"Approved"` |
| Tolak (dengan sebab) | `"Rejected"` |
| Muat naik lampiran | `"AttachmentUploaded"` |
| Buka semula (*reopen*) selepas submit | `"Reopened"` |
| Ubah data selepas *reopen* | `"EditedAfterReopen"` |

```csharp
await _auditLogService.LogAsync(submissionId, "Approved", currentUserId,
    details: $"Diluluskan oleh {approverName} pada peringkat {stepName}");
```

> **Kenapa penting:** jejak audit membolehkan siasatan jika berlaku pertikaian ("siapa luluskan permohonan ini, bila?") — keperluan asas akauntabiliti sistem kerajaan.

---

## 6. Elak edit selepas submit — melainkan dibuka semula (*reopen*)

Sebaik sahaja `Submission.Status` berubah daripada `Draft` ke `Submitted`, **jangan** benarkan pengguna mengubah data terus melalui Controller/Action yang sama seperti draf:

```csharp
[HttpPost]
public async Task<IActionResult> Edit(int id, OfficerReportingEditViewModel vm)
{
    var submission = await _context.Submissions.FindAsync(id);

    if (submission!.Status != SubmissionStatus.Draft)
    {
        // Hanya boleh edit jika status masih Draft, ATAU telah dibuka semula secara eksplisit
        return Forbid();
    }

    // ...teruskan kemas kini
}
```

Jika permohonan perlu dibetulkan selepas dihantar (cth. pegawai semak minta pembetulan), guna tindakan **eksplisit** `ReopenAsync()` yang:

1. Menukar status kembali kepada `Draft` (atau status "boleh edit" khusus).
2. Merekod `AuditLog` (`"Reopened"`, oleh siapa, sebab apa).
3. Membenarkan edit **hanya** selepas peralihan ini berlaku secara sah — bukan secara senyap/*silent*.

```csharp
public async Task ReopenAsync(int submissionId, string reopenedByUserId, string reason)
{
    var submission = await _context.Submissions.FindAsync(submissionId);
    submission!.Status = SubmissionStatus.Draft;
    await _context.SaveChangesAsync();

    await _auditLogService.LogAsync(submissionId, "Reopened", reopenedByUserId, details: reason);
}
```

---

## Senarai Semak Keselamatan (Ringkas)

- [ ] Tiada kata laluan/*credential* sebenar disimpan dalam entiti modul ID/AD/Email
- [ ] Kata laluan log masuk aplikasi dikendalikan oleh ASP.NET Core Identity (hash automatik)
- [ ] Muat naik fail: had saiz + jenis fail disemak di server-side
- [ ] Nama fail simpanan dijana (GUID), bukan nama asal pengguna
- [ ] Fail lampiran disimpan di luar `wwwroot`, capaian melalui Action ber-*authorize*
- [ ] `[Authorize(Roles=...)]` / Policy pada **setiap** Controller/Action sensitif — UI menu hanya kemudahan
- [ ] `AuditLog` direkod untuk setiap tindakan penting (submit, approve, reject, upload, reopen, edit selepas reopen)
- [ ] Edit selepas *submit* disekat melainkan melalui `ReopenAsync()` eksplisit + audit

---

## Kaitan dengan hari-hari lain

Nota ini relevan pada **setiap** hari kursus, terutamanya:

- **Hari 3** — muat naik lampiran pertama.
- **Kumpulan 3** — akaun AD/email (jangan simpan kata laluan); security audit pada blok Hari 13–14.
- **Kumpulan 3, blok Hari 7–9** — RBAC + matriks RBAC merentas modul.
- **Semua kumpulan** — kunci borang selepas submit, dikuatkuasakan di **pelayan** bukan hanya `disabled` dalam view.
- **Hari 15** — senarai semak pelepasan turut merangkumi item keselamatan ([`08-deployment.md`](./08-deployment.md)).

---

## Sumber Rasmi

- **[Overview of ASP.NET Core security](https://learn.microsoft.com/en-us/aspnet/core/security/)**
- **[Prevent Cross-Site Request Forgery (XSRF/CSRF)](https://learn.microsoft.com/en-us/aspnet/core/security/anti-request-forgery)**
- **[Safe storage of app secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)**
- **[Security considerations for file uploads](https://learn.microsoft.com/en-us/aspnet/core/mvc/models/file-uploads#security-considerations)**
- **[Identity password hashing](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity)**
