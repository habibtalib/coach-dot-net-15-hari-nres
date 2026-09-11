# Prompt umum — Hari 7–9 (Aliran kelulusan & skrin admin)

> **Guna pada permulaan hari.** Prompt **am** (bukan modul-spesifik) untuk mengorientasi pembantu AI anda ke konteks kursus + matlamat hari ini. Ganti `<...>`. Prompt bernombor mengikut tugas: [`../docs/pustaka-prompt.md`](../docs/pustaka-prompt.md).

```text
Rujuk AGENTS.md dan SPEC-KURSUS.md. Saya bekerja dalam repo/modul <sistem>.
Matlamat Hari 7–9: aliran kelulusan (Review/Approve/Reject) + skrin admin, dengan peranan.
Sebelum menjana kod: warisi SubmissionControllerBase, [Authorize(Roles = "<peranan>")],
peralihan status via IWorkflowService + audit (jangan tukar status terus dalam controller),
dan ikut aliran setiap tugas — Jira → cabang feat/<ciri> → SMK-01 → PR (Closes KEY-n).
Jangan reka keperluan; yang tak pasti, tanya. Tunjuk diff dahulu.
```

- **Selepas:** ikut prompt bernombor — `DEV-06` (aliran kelulusan). Kandungan trek: `kumpulan-N-…/hari-7-9/`.
