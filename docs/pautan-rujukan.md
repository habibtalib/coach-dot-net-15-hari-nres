# Pautan rujukan — kursus DOTNET-NRES-15

Hab pautan untuk peserta & jurulatih. Simpan satu tempat; hari/lab hanya **merujuk** ke sini.

## Kanun kursus (mesti baca)

- [`SPEC-KURSUS.md`](../SPEC-KURSUS.md) — kanun tunggal (nama entiti, `SubmissionStatus`, peranan, prefix, cabang Git).
- [`JADUAL.md`](../JADUAL.md) — aturcara 15 hari (3 fasa).
- [`KOLABORASI.md`](../KOLABORASI.md) — kontrak pasukan (Definition of Done, PR, code review).
- [`AGENTS.md`](../AGENTS.md) — konteks AI kongsi.

## Panduan setup & perkakas (docs/)

- [Persediaan .NET 10 (SDK · EF Core · IDE)](./persediaan-dotnet.md) — pasang & sahkan sebelum Hari 3.
- [Persediaan Git (pasang & identiti)](./persediaan-git.md) — sebelum Hari 2.
- [Persediaan scaffold projek MVC (+ NuGet)](./persediaan-scaffold.md) — Hari 3.
- [Lab MCP — Jira + FigJam dari Claude Code](./lab-mcp-jira-figjam.md) — sambung & **pandu** kedua-dua (MCP rasmi Atlassian + Figma), setup + drive.
- [Sambung Jira ke Claude Code (CLI & VS Code)](./cara-sambung-jira-claude-code.md) — MCP Atlassian, untuk pelajar tanpa MCP peribadi.
- [Jana dokumentasi & diagram Mermaid dengan AI](./cara-jana-dokumentasi-diagram.md) — skill `/dok-modul` + prompt terus.
- [Pustaka prompt (berfail dengan ID)](./pustaka-prompt.md) — PRD · dokumentasi · diagram · UI/UX · Jira · semakan.
- [Mula bina dengan Claude Code — borang dahulu](./mula-claude-code-borang-dahulu.md) — UI → ViewModel → Razor → validation → entiti/migration.
- [Senarai semak modul (build checklist)](./senarai-semak-modul.md) — perancangan → bina → integrasi + DoD.
- [Diagram ekosistem Claude Code](./diagram-claude-code.md) — model · sub-agent · MCP · skills · plugins · PRD.
- [Contoh PRD (Tempahan Fasiliti Sukan)](./contoh-prd-tempahan-fasiliti-sukan.md) — 7 bahagian.

## Rujukan teknikal

- **Buku:** *C# 14 and .NET 10* (Mark J. Price, Packt 2025) — pemetaan bab: [`nota/10-rujukan-buku.md`](../nota/10-rujukan-buku.md) · repo kod [habibtalib/cs14net10](https://github.com/habibtalib/cs14net10).
- **.NET 10 / ASP.NET Core MVC:** [Microsoft Learn — MVC](https://learn.microsoft.com/aspnet/core/mvc/overview)
- **EF Core 10:** [Microsoft Learn — EF Core](https://learn.microsoft.com/ef/core/)
- **Claude Code:** [Dokumentasi rasmi](https://docs.claude.com/en/docs/claude-code) · **MCP:** [modelcontextprotocol.io](https://modelcontextprotocol.io)
- **Mermaid:** [mermaid.js.org](https://mermaid.js.org)

## Org & repo sistem

- Org GitHub (6 sistem NRES): [`nres-bpm`](https://github.com/nres-bpm)

---

## Rujukan rasmi mengikut hari (Fasa 1)

### Hari 1 — Perancangan, URS, Use Case & ERD

| Topik | Rujukan |
|-------|---------|
| Design Thinking (empathize, define, persona) | [Design Thinking — gambaran](https://en.wikipedia.org/wiki/Design_thinking) |
| URS vs SRS | [ISO/IEC/IEEE 29148 — ringkasan](https://en.wikipedia.org/wiki/ISO/IEC_IEEE_29148) |
| Use case & aktor | [UML Use Case — gambaran](https://en.wikipedia.org/wiki/Use_case_diagram) |
| Diagram sebagai kod | [Mermaid — dokumentasi rasmi](https://mermaid.js.org/intro/) |
| Mermaid flowchart | [mermaid.js.org/syntax/flowchart](https://mermaid.js.org/syntax/flowchart.html) |
| Mermaid ERD | [mermaid.js.org/syntax/entityRelationshipDiagram](https://mermaid.js.org/syntax/entityRelationshipDiagram.html) |
| Pemodelan data & hubungan | [learn.microsoft.com/ef/core/modeling/relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships) |

### Hari 2 — Agile, Git, Branching & Kolaborasi

| Topik | Rujukan |
|-------|---------|
| Git — asas | [git-scm.com/book](https://git-scm.com/book/en/v2) |
| Percabangan Git | [git-scm.com/book — Branching](https://git-scm.com/book/en/v2/Git-Branching-Branches-in-a-Nutshell) |
| Rebase vs merge | [git-scm.com/book — Rebasing](https://git-scm.com/book/en/v2/Git-Branching-Rebasing) |
| Selesaikan konflik | [docs.github.com — merge conflicts](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/addressing-merge-conflicts) |
| Pull request | [docs.github.com — about pull requests](https://docs.github.com/en/pull-requests/collaborating-with-pull-requests/proposing-changes-to-your-work-with-pull-requests/about-pull-requests) |
| GitHub Projects | [docs.github.com/issues/planning-and-tracking-with-projects](https://docs.github.com/en/issues/planning-and-tracking-with-projects) |
| Jira — konsep asas | [atlassian.com/software/jira/guides](https://www.atlassian.com/software/jira/guides) |
| .NET SDK & CLI | [learn.microsoft.com/dotnet/core/tools](https://learn.microsoft.com/en-us/dotnet/core/tools/) |

### Hari 3 — Refresher .NET (C#, EF Core, MVC, Identity)

| Topik | Rujukan |
|-------|---------|
| `dotnet new` templates | [learn.microsoft.com/dotnet/core/tools/dotnet-new](https://learn.microsoft.com/en-us/dotnet/core/tools/dotnet-new) |
| ASP.NET Core MVC — gambaran | [learn.microsoft.com/aspnet/core/mvc/overview](https://learn.microsoft.com/en-us/aspnet/core/mvc/overview) |
| `Program.cs` & minimal hosting | [learn.microsoft.com/aspnet/core/fundamentals/minimal-apis](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/minimal-apis) |
| Dependency Injection | [learn.microsoft.com/aspnet/core/fundamentals/dependency-injection](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/dependency-injection) |
| LINQ | [learn.microsoft.com/dotnet/csharp/linq](https://learn.microsoft.com/en-us/dotnet/csharp/linq/) |
| `async`/`await` | [learn.microsoft.com/dotnet/csharp/asynchronous-programming](https://learn.microsoft.com/en-us/dotnet/csharp/asynchronous-programming/) |
| EF Core — permulaan | [learn.microsoft.com/ef/core/get-started/overview/first-app](https://learn.microsoft.com/en-us/ef/core/get-started/overview/first-app) |
| EF Core — Fluent API & `IEntityTypeConfiguration` | [learn.microsoft.com/ef/core/modeling](https://learn.microsoft.com/en-us/ef/core/modeling/) |
| EF Core — relationships | [learn.microsoft.com/ef/core/modeling/relationships](https://learn.microsoft.com/en-us/ef/core/modeling/relationships) |
| EF Core Migrations | [learn.microsoft.com/ef/core/managing-schema/migrations](https://learn.microsoft.com/en-us/ef/core/managing-schema/migrations/) |
| ASP.NET Core Identity | [learn.microsoft.com/aspnet/core/security/authentication/identity](https://learn.microsoft.com/en-us/aspnet/core/security/authentication/identity) |
| Role-based authorization | [learn.microsoft.com/aspnet/core/security/authorization/roles](https://learn.microsoft.com/en-us/aspnet/core/security/authorization/roles) |
