# Prompt umum — Hari 13–14 (Ujian, refactor & sedia integrasi/deploy)

> **Guna pada permulaan hari.** Prompt **am** (bukan modul-spesifik) untuk mengorientasi pembantu AI anda ke konteks kursus + matlamat hari ini. Ganti `<...>`. Prompt bernombor mengikut tugas: [`../docs/pustaka-prompt.md`](../docs/pustaka-prompt.md). QA dipandu-AI: [`../docs/lab-qa-ai-uat.md`](../docs/lab-qa-ai-uat.md).

```text
Rujuk AGENTS.md dan SPEC-KURSUS.md. Saya bekerja dalam repo/modul <sistem>.
Matlamat Hari 13–14: ujian xUnit peraturan kritikal + refactor + sedia gabung.
TIADA ciri baharu — hanya pembetulan pepijat, ujian, dan pembersihan.
Uji: peralihan SubmissionStatus, nombor rujukan, semakan pendua; guna SQLite in-memory
(bukan UseInMemoryDatabase). Kebenaran diuji via POST terus → 403.
Ikut aliran setiap tugas — Jira → cabang feat/<ciri> → SMK-01 → PR (Closes KEY-n).
Jangan reka keperluan; yang tak pasti, tanya. Tunjuk diff dahulu.
```

- **Selepas:** ikut prompt bernombor — `DEV-07`, `UJI-01`…`UJI-04` (subagent `qa-uat` + `/uji-modul`). Kandungan trek: `kumpulan-N-…/hari-13-14/`.
