# Templat Kod Boleh Guna Semula (Reusable Snippets)

Folder ini mengandungi snippet kod rujukan yang **berulang merentas 5 modul** — supaya peserta tidak menulis semula corak yang sama dari kosong setiap kali. Ambil, salin, dan sesuaikan mengikut modul.

> **Nota:** Implementasi penuh & berfungsi bagi semua snippet ini ada dalam projek rujukan [`../projek/Nres.Onboarding.Web/`](../projek/). Templat di sini ialah **ringkasan corak** untuk rujukan pantas semasa lab.

## Kandungan

| Fail | Corak | Digunakan mula (hari) |
|------|-------|-----------------------|
| `SubmissionStatus.cs.txt` | Enum status kongsi | Hari 1 |
| `IReferenceNumberService.cs.txt` | Jana nombor rujukan (`LD-2026-0001`) | Hari 3 |
| `IFileStorageService.cs.txt` | Muat naik fail selamat (luar `wwwroot`) | Hari 3 |
| `workflow-transition.cs.txt` | Semakan peralihan status | Hari 8 |
| `csv-export.cs.txt` | Corak eksport CSV | Hari 12 |
| `print.css.txt` | CSS cetakan (`@media print`) | Hari 6 |

Corak universal setiap modul (belajar sekali, ulang lima kali):

```text
Form → Validation → Draft → Submit → Review → Approve/Reject → Audit → Report
```

Rujuk [`../SPEC-KURSUS.md`](../SPEC-KURSUS.md) untuk nama entiti, enum, peranan, dan prefix nombor rujukan yang **muktamad**.
