# Corak Aliran Kerja NRES

> Ini ialah **tulang belakang konseptual** seluruh kursus — fahami nota ini dengan teliti sebelum Hari 1. Setiap satu daripada 4 modul (Hari 4–14) mengulang corak yang sama. Lihat [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) untuk definisi rasmi entiti & enum.

---

## Masalah: 4 modul, corak berulang

NRES ada 4 kumpulan permohonan berbeza (Lapor Diri, Pas/Parkir/Pelekat, ID/AD/Email, Perisian & Aset ICT) — tetapi **semuanya** melalui aliran yang sama:

```text
Form → Validation → Draft → Submit → Review → Approve/Reject → Audit → Report
```

Daripada menulis logik status, kelulusan, dan audit **lima kali berasingan**, kita bina **satu corak kongsi** yang setiap modul warisi. Ini konsep paling penting dalam kursus — sekali faham, peserta boleh sambung mana-mana borang NRES baharu tanpa mula dari kosong.

---

## `Submission` — induk kongsi

`Submission` ialah entiti **induk** yang setiap permohonan (tidak kira modul) berkongsi:

```csharp
public class Submission
{
    public int Id { get; set; }
    public string ReferenceNumber { get; set; } = string.Empty;  // cth. "LD-2026-0001"
    public string ModuleType { get; set; } = string.Empty;        // "OfficerReporting", "AccessPass", dsb.
    public SubmissionStatus Status { get; set; } = SubmissionStatus.Draft;
    public string ApplicantUserId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? SubmittedAt { get; set; }
    public DateTime? CompletedAt { get; set; }

    public ICollection<Attachment> Attachments { get; set; } = new List<Attachment>();
    public ICollection<ApprovalStep> ApprovalSteps { get; set; } = new List<ApprovalStep>();
    public ICollection<AuditLog> AuditLogs { get; set; } = new List<AuditLog>();
}
```

### Kenapa satu `Submission` kongsi, bukan lima jadual berasingan?

| Tanpa induk kongsi (5 jadual berasingan) | Dengan `Submission` kongsi |
|---|---|
| Status, nombor rujukan, audit ditulis **5 kali** (kod berulang) | Ditulis **sekali**, dipakai semua modul |
| Laporan merentas modul sukar (perlu `UNION` 5 jadual) | Satu pertanyaan `Submissions` merentas semua modul |
| Setiap modul perlu logik kelulusan sendiri | `ApprovalStep`/`IWorkflowService` sama untuk semua |

Setiap entiti khusus modul (contoh `OfficerReportingApplication`) mempunyai **satu-ke-satu** hubungan dengan `Submission` — ia menyimpan data **khusus** modul sahaja (cth. maklumat PCB, akuan OSA), manakala status/audit/kelulusan dikendalikan oleh `Submission`.

```csharp
public class OfficerReportingApplication
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public string FullName { get; set; } = string.Empty;
    public string PositionGrade { get; set; } = string.Empty;
    public bool OsaDeclarationAccepted { get; set; }
    // ...medan khusus Lapor Diri sahaja
}
```

---

## `SubmissionStatus` — enum kongsi semua modul

```csharp
public enum SubmissionStatus
{
    Draft = 0,
    Submitted = 1,
    SupervisorApproved = 2,
    AdminApproved = 3,
    Rejected = 4,
    Completed = 5,
    Cancelled = 6
}
```

### Rajah peralihan status

```text
Draft ──Submit──► Submitted ──Approve──► SupervisorApproved ──Approve──► AdminApproved ──► Completed
  │                    │                         │                            │
  └──Cancel──►Cancelled└────────Reject───────────┴────────────Reject──────────┘
                                          ▼
                                      Rejected
```

> **Nota:** Tidak semua modul guna semua peringkat — sesetengah permohonan (cth. Lapor Diri) mungkin hanya perlu satu kelulusan (`AdminApproved` terus ke `Completed`), manakala yang lain (ID/AD/Email — Kumpulan 3) perlu rantaian berbilang langkah. `ApprovalStep` (di bawah) menyimpan sejarah **sebenar** peringkat yang dilalui bagi setiap `Submission` individu.

---

## `ApprovalStep` — jejak kelulusan

```csharp
public class ApprovalStep
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public string StepName { get; set; } = string.Empty;   // "SupervisorReview", "AdminReview"
    public string ApproverUserId { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public string? RejectionRemarks { get; set; }
    public DateTime ActionedAt { get; set; } = DateTime.UtcNow;
}
```

Setiap kali seorang penyemak meluluskan/menolak, satu rekod `ApprovalStep` dicipta — ini membolehkan sejarah penuh siapa buat apa bila, dan berguna untuk `RejectionRemarks` wajib (lihat [`04-validation-viewmodels.md`](./04-validation-viewmodels.md) & [`07-testing-xunit.md`](./07-testing-xunit.md)).

---

## `AuditLog` — jejak audit sejagat

```csharp
public class AuditLog
{
    public int Id { get; set; }
    public int SubmissionId { get; set; }
    public Submission Submission { get; set; } = null!;

    public string Action { get; set; } = string.Empty;   // "Created", "Submitted", "Approved", "Rejected"
    public string PerformedByUserId { get; set; } = string.Empty;
    public DateTime PerformedAt { get; set; } = DateTime.UtcNow;
    public string? Details { get; set; }
}
```

> **Prinsip keselamatan:** setiap tindakan **penting** (submit, approve, reject, edit selepas *reopen*) MESTI direkod dalam `AuditLog` — lihat [`09-keselamatan.md`](./09-keselamatan.md) untuk senarai penuh tindakan yang wajib diaudit.

---

## `IWorkflowService` — logik peralihan status berpusat

Daripada menulis `if (status == ...)` bertaburan dalam setiap Controller, logik peralihan status dipusatkan dalam satu servis:

```csharp
public interface IWorkflowService
{
    Task SubmitAsync(int submissionId, string userId);
    Task ApproveAsync(int submissionId, string approverUserId, string stepName);
    Task RejectAsync(int submissionId, string approverUserId, string stepName, string remarks);
}
```

Setiap Controller modul (`ReportingController`, `AccessPassController`, dsb.) memanggil servis yang **sama** — memastikan peraturan peralihan status konsisten merentas 4 modul, dan audit log direkod secara automatik di satu tempat.

---

## Kaitan dengan hari-hari lain

- **Hari 1** — cipta `Submission`, `SubmissionStatus`, `Attachment`, `AuditLog` + migration pertama ([`02-efcore-migrations.md`](./02-efcore-migrations.md)).
- **Hari 2–3** — modul pertama (Lapor Diri) — praktik penuh corak `Form → Draft → Submit → Review → Audit`.
- **Kumpulan 3, blok Hari 7–9** — `ApprovalStep` digunakan sepenuhnya sebagai rantaian **dua peringkat** (Penyelia → ICT) untuk modul ID/AD/Email.
- Lihat [`04-validation-viewmodels.md`](./04-validation-viewmodels.md) untuk perbezaan validation draf vs final, dan [`05-identity-authorization.md`](./05-identity-authorization.md) untuk siapa boleh buat apa pada setiap status.

---

## Sumber Rasmi

- **[EF Core relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships)**
- **[Enums in C#](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/builtin-types/enum)**
- **[Domain-driven design fundamentals (konsep aggregate root, serupa peranan `Submission`)](https://learn.microsoft.com/en-us/dotnet/architecture/microservices/microservice-ddd-cqrs-patterns/identify-microservice-domain-model-boundaries)**
