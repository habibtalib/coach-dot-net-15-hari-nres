---
name: semak-modul
description: >-
  Semakan pra-PR modul NRES — konvensyen AGENTS.md, sempadan folder, kontrak
  Profile DB, authorization & validation pelayan, dan Definition of Done. Guna
  sebelum setiap Pull Request.
---

# Semak Modul (pra-PR)

Semak **diff semasa** terhadap `AGENTS.md` & `KOLABORASI.md`. Laporkan masalah — **jangan** tulis semula kod.

1. **Pendua** — adakah ia menduplikasi komponen piawai yang **sudah ada dalam repo ini**?
2. **Sempadan** — adakah ia menyentuh fail di luar folder/repo modul?
3. **Profile DB** — adakah ia menyalin/mengubah skema profil (patut guna klien `profile`)?
4. **Keselamatan** — `[Authorize(Roles=…)]` betul pada setiap action? Validation di **pelayan** lengkap?
5. **DoD** (`KOLABORASI.md` §9) — `dotnet build` bersih; status via `IWorkflowService`; audit via `IAuditLogService`; PR ada perihalan BM + cara uji.

**Output:** senarai bernombor masalah + cadangan pembetulan. Serah kepada penulis untuk betulkan sebelum PR.
