# Lab — QA dipandu-AI: Ujian xUnit & SIT/UAT Pre-Check

> 🔧 **Khusus Claude Code · lanjutan.** Lab ini menyatukan cara kerja QA berbantu-AI untuk **Hari 13–14** (ujian) dan **Hari 15** (SIT/UAT). Anda akan cipta **satu subagent** (`qa-uat`) dan **satu skill** (`/uji-modul`), kemudian gunakannya untuk: (1) menulis & menjalankan **ujian xUnit** peraturan kritikal, dan (2) memandu **pre-check aliran hujung-ke-hujung** melalui pelayar dengan **MCP claude-in-chrome**.
>
> **AI menguji, anda mengesahkan.** Subagent mempercepat penulisan ujian dan pemanduan pelayar — keputusan lulus/gagal tetap anda sahkan. Tiada modul diluluskan atas dakwaan AI sahaja.
>
> Guna alat AI lain (bukan Claude Code)? Ikut ujian xUnit manual dalam blok `hari-13-14` trek anda; bahagian claude-in-chrome khusus Claude Code.

## Konsep ringkas

| Bahagian | Apa ia | Bila |
|----------|--------|------|
| **Subagent `qa-uat`** | Persona penguji: tulis/jalankan xUnit + pandu SIT/UAT pre-check pelayar | `.claude/agents/qa-uat.md` |
| **Skill `/uji-modul`** | Senarai semak ujian xUnit boleh guna semula | `.claude/skills/uji-modul/SKILL.md` |
| **MCP claude-in-chrome** | Pandu pelayar (log masuk, isi borang, klik, baca halaman) | Hari 15 — SIT/UAT pre-check |

**Pembahagian hari (padan dek slaid):**

- **Hari 13–14** — `qa-uat` tulis **ujian xUnit** automasi: peralihan `SubmissionStatus`, nombor rujukan, semakan pendua. Kebenaran diuji dengan **POST terus → 403**.
- **Hari 15** — `qa-uat` pandu **SIT/UAT pre-check** aliran hujung-ke-hujung melalui pelayar. **Ini pre-check, bukan UAT sebenar** — UAT sebenar dijalankan pengguna NRES terhadap keperluan mereka.

## Persediaan

- **App rujukan berjalan.** Dari root repo kursus:

  ```bash
  cd projek/Nres.Onboarding.Web
  dotnet run
  ```

  Buka `https://localhost:7034` (profil `http` → `http://localhost:5202`, tetapi `UseHttpsRedirection` akan lencongkan ke HTTPS). Log masuk di `https://localhost:7034/Account/Login`.
- **Akaun demo (data seed):** `applicant@nres.demo` / `Password123!` (Applicant) · `hradmin@nres.demo` / `Password123!` (HrAdmin).
- **.NET 10 SDK** (`dotnet --version` → `10.x`) + `dotnet-ef` jika perlu.
- **Claude for Chrome** dipasang, dan tapak `localhost` **dibenarkan** dalam kebenaran sambungan (extension permissions).
- App rujukan hanya ada halaman **Kumpulan 1 (Lapor Diri)** — Home, Account, OfficerReporting. Untuk modul lain, jalankan app repo anda sendiri dan sasar aliran setaranya.
- Cross-link: DoD [`../KOLABORASI.md`](../KOLABORASI.md) §9 · senarai semak [`senarai-semak-modul.md`](./senarai-semak-modul.md) · cipta subagent [`lab-subagent-peranan.md`](./lab-subagent-peranan.md) · sambung MCP [`lab-mcp-jira-figjam.md`](./lab-mcp-jira-figjam.md).

---

## Latihan 1 — Cipta subagent `qa-uat`

**Objektif:** Satu persona penguji dengan capaian alat yang **betul** — boleh jalankan ujian & pandu pelayar, tetapi tak menulis kod ciri.

### Langkah

1. Cipta `.claude/agents/qa-uat.md` (salin dari templat kursus [`../.claude/agents/qa-uat.md`](../.claude/agents/qa-uat.md)).
2. Perhatikan `tools:` — inilah pengajarannya:
   - `Read, Write, Edit, Bash, Grep, Glob` — Write/Edit **hanya** untuk projek ujian (`*.Tests`), dikuatkuasa oleh `## Peraturan`.
   - `mcp__claude-in-chrome__*` — alat pelayar untuk pre-check Hari 15.
3. Sahkan Claude nampak persona:

   ```text
   /agents
   ```

### ✅ Semakan

- [ ] `.claude/agents/qa-uat.md` wujud dengan `name` + `description` + `tools` + `model`
- [ ] `/agents` menyenaraikan `qa-uat`
- [ ] `qa-uat` **berasingan** daripada `qa` sedia ada (pra-PR) — dua persona, dua tujuan

---

## Latihan 2 — Cipta skill `/uji-modul`

**Objektif:** Bungkus senarai semak ujian xUnit jadi satu slash command.

### Langkah

1. Cipta `.claude/skills/uji-modul/SKILL.md` (salin dari templat kursus [`../.claude/skills/uji-modul/SKILL.md`](../.claude/skills/uji-modul/SKILL.md)). Frontmatter = **hanya** `name` + `description`.
2. Panggil:

   ```text
   /uji-modul
   ```

### ✅ Semakan

- [ ] `.claude/skills/uji-modul/SKILL.md` wujud dengan `name` + `description`
- [ ] `/uji-modul` menghasilkan senarai ujian + keputusan (bukan menulis kod ciri)

---

## Latihan 3 — Tulis & jalankan xUnit dengan AI (Hari 13–14)

**Objektif:** Guna `qa-uat` untuk menulis ujian peraturan kritikal, kemudian jalankan `dotnet test` sehingga **hijau**.

> Projek ujian `Nres.Onboarding.Tests` **tidak** disertakan dalam `projek/` (sengaja — dibina dalam trek). Cipta ia bersebelahan app rujukan untuk berlatih, seperti dalam blok `hari-13-14` Kumpulan 1.

### Langkah

1. Cipta projek ujian & rujuk app + pakej SQLite:

   ```bash
   dotnet new xunit -o Nres.Onboarding.Tests
   cd Nres.Onboarding.Tests
   dotnet add reference ../projek/Nres.Onboarding.Web/Nres.Onboarding.Web.csproj
   dotnet add package Microsoft.Data.Sqlite
   ```

2. **Helper SQLite in-memory** (`TestDb.cs`) — **bukan** `UseInMemoryDatabase` (ia abaikan kekangan unik seperti indeks unik `ReferenceNo`):

   ```csharp
   using Microsoft.Data.Sqlite;
   using Microsoft.EntityFrameworkCore;
   using Nres.Onboarding.Web.Data;

   public static class TestDb
   {
       public static ApplicationDbContext Create()
       {
           var connection = new SqliteConnection("DataSource=:memory:");
           connection.Open();                       // kekal terbuka sepanjang ujian
           var options = new DbContextOptionsBuilder<ApplicationDbContext>()
               .UseSqlite(connection)
               .Options;
           var db = new ApplicationDbContext(options);
           db.Database.EnsureCreated();
           return db;
       }
   }
   ```

3. **Ujian peralihan status** — `CanTransition` ialah fungsi tulen (tiada DB), sesuai untuk `[Theory]`:

   ```csharp
   using Nres.Onboarding.Web.Models.Shared;
   using Nres.Onboarding.Web.Services;
   using Xunit;

   public class WorkflowTransitionTests
   {
       // CanTransition guna jadual statik sahaja — tak sentuh DB/audit.
       private readonly IWorkflowService _workflow = new WorkflowService(null!, null!);

       [Theory]
       [InlineData(SubmissionStatus.Draft, SubmissionStatus.Submitted, true)]
       [InlineData(SubmissionStatus.Submitted, SubmissionStatus.AdminApproved, true)]
       [InlineData(SubmissionStatus.Submitted, SubmissionStatus.Rejected, true)]
       [InlineData(SubmissionStatus.Draft, SubmissionStatus.AdminApproved, false)] // langkau peringkat
       [InlineData(SubmissionStatus.Rejected, SubmissionStatus.Submitted, false)]  // terminal
       public void CanTransition_ikut_peraturan(SubmissionStatus from, SubmissionStatus to, bool expected)
           => Assert.Equal(expected, _workflow.CanTransition(from, to));
   }
   ```

4. **Prompt `qa-uat`** untuk melengkapkan ujian bergantung-DB (nombor rujukan berjujukan + peralihan tak sah membaling `InvalidOperationException`), guna `TestDb` di atas:

   ```text
   Guna subagent qa-uat: tulis ujian xUnit dalam Nres.Onboarding.Tests untuk:
   - IReferenceNumberService.GenerateAsync("LD") → jujukan LD-{tahun}-0001, 0002 (guna TestDb SQLite in-memory)
   - WorkflowService.TransitionAsync membaling InvalidOperationException untuk peralihan tak sah (cth Draft→AdminApproved)
   Jalankan dotnet test dan tunjuk keputusan. Jangan ubah kod ciri.
   ```

5. Jalankan:

   ```bash
   dotnet test
   ```

### ✅ Semakan

- [ ] `TestDb` guna **SQLite in-memory** (bukan `UseInMemoryDatabase`)
- [ ] Ujian meliputi: peralihan status (sah + terlarang), nombor rujukan berjujukan, semakan pendua
- [ ] `dotnet test` **hijau**; setiap ujian merah difahami & dibetulkan (oleh `dev`, bukan `qa-uat`)

---

## Latihan 4 — SIT/UAT pre-check dengan claude-in-chrome (Hari 15)

**Objektif:** Pandu aliran hujung-ke-hujung sebenar melalui pelayar, dan rekod lulus/gagal — termasuk semakan RBAC 403.

### Langkah

1. **Pastikan app berjalan** (`dotnet run` di `projek/Nres.Onboarding.Web`) dan `https://localhost:7034` boleh dibuka dalam Chrome.
2. **Benarkan tapak.** Dalam sambungan Claude for Chrome, benarkan `localhost`. Alat pelayar muncul sebagai `mcp__claude-in-chrome__*`.
3. **Pandu aliran penuh** dengan `qa-uat`:

   ```text
   Guna subagent qa-uat: jalankan SIT/UAT pre-check aliran hujung-ke-hujung di
   https://localhost:7034. Buka tab baharu, kemudian:
   1. Log masuk applicant@nres.demo / Password123!
   2. OfficerReporting → Permohonan Baharu → isi borang → lampir satu fail → Simpan & Hantar
   3. Catat nombor rujukan LD-… yang dijana
   4. Log keluar, log masuk hradmin@nres.demo / Password123!
   5. Semakan HR → buka permohonan → Luluskan
   6. Buka Details → sahkan jejak audit lengkap
   Rekod lulus/gagal setiap langkah + tangkapan GIF. Jangan cetuskan sebarang dialog/alert.
   ```

4. **Semakan RBAC 403.** Masih sebagai `applicant`:

   ```text
   Guna subagent qa-uat: sebagai applicant, cuba buka https://localhost:7034/OfficerReporting/Review.
   Sahkan ia DITOLAK (AccessDenied/403). Rekod hasil.
   ```

5. **Rekod keputusan** dalam `docs/<modul>/sit-uat-precheck.md` (lulus/gagal + isu).

> **Semak manusia:** AI memandu klik, **anda** sahkan setiap keputusan lulus/gagal. Satu langkah "hijau" yang AI salah baca ialah pepijat yang terlepas.

> ⚠️ **Pre-check ≠ UAT sebenar.** Semakan ini mengesahkan aliran & sempadan berfungsi. **UAT sebenar dijalankan pengguna NRES** terhadap keperluan mereka sendiri — nyatakan itu dalam serahan.

### ✅ Semakan

- [ ] Aliran penuh applicant → hantar → HR lulus → audit **berjaya** direkod
- [ ] Nombor rujukan `LD-…` dijana & jejak audit lengkap
- [ ] `applicant` ke `/OfficerReporting/Review` → **403/AccessDenied** (RBAC dikuatkuasa)
- [ ] Keputusan direkod; isu diserah kepada `dev`; pre-check ditanda **bukan** UAT sebenar

---

## Pustaka prompt (guna dengan `qa-uat`)

Prompt sedia-guna (juga difailkan dalam [`pustaka-prompt.md`](./pustaka-prompt.md) sebagai `UJI-01`…`UJI-03`).

**UJI-01 · Tulis & jalankan xUnit:**

```text
Guna subagent qa-uat: tulis ujian xUnit untuk peralihan SubmissionStatus
(Draft→Submitted→AdminApproved/Rejected, termasuk peralihan terlarang mesti gagal),
nombor rujukan berjujukan, dan semakan pendua dua arah dalam repo <sistem>.
Guna SQLite in-memory (bukan UseInMemoryDatabase). Jalankan dotnet test, tunjuk keputusan.
```

**UJI-02 · SIT/UAT pre-check pelayar:**

```text
Guna subagent qa-uat: jalankan SIT/UAT pre-check di https://localhost:7034 —
log masuk applicant, hantar satu permohonan (catat nombor rujukan), log masuk hradmin,
luluskan, sahkan jejak audit. Rekod lulus/gagal + GIF. Jangan cetuskan dialog.
```

**UJI-03 · Semakan RBAC 403:**

```text
Guna subagent qa-uat: sebagai applicant, cuba capai setiap halaman semakan admin
(/OfficerReporting/Review). Setiap satu mesti 403/AccessDenied. Senaraikan hasil setiap semakan.
```

**UJI-04 · Mulakan ujian unit dari kosong** (untuk modul yang belum ada projek ujian — K2/K3/K4 atau mana-mana repo `tests/` masih kosong):

```text
Guna subagent qa-uat: modul <sistem> belum ada projek ujian. Mulakan dari kosong:
1. Cipta projek xUnit dalam tests/, rujuk projek Web, tambah pakej Microsoft.Data.Sqlite.
2. Tambah helper TestDb (SQLite in-memory, BUKAN UseInMemoryDatabase).
3. Tulis ujian PERTAMA untuk peraturan paling kritikal modul: peralihan SubmissionStatus,
   nombor rujukan berjujukan, dan satu peraturan perniagaan utama (cth <slot bertindih / pendua plat>).
Jalankan dotnet test dan tunjuk keputusan. Jangan ubah kod ciri.
```

---

## Masalah biasa

- **Amaran sijil / lencongan HTTPS:** app guna `UseHttpsRedirection` → guna `https://localhost:7034` terus, atau jalankan profil `http` dan terima sijil dev (`dotnet dev-certs https --trust`).
- **Pelayar tak bertindak balas:** dialog/alert JavaScript membekukan sesi claude-in-chrome. Elak butang yang mencetuskan `confirm()`; jika terlanjur, tutup dialog manual dalam Chrome.
- **`UseInMemoryDatabase` "lulus" sepatutnya gagal:** ia mengabaikan indeks unik — semakan pendua/rujukan takkan diuji betul. Guna **SQLite in-memory**.
- **POST mentah gagal (anti-forgery):** borang guna `[ValidateAntiForgeryToken]`. Pandu borang sebenar melalui pelayar (token dihantar automatik); jangan POST mentah tanpa token.
- **Subagent tersilap tulis kod ciri:** pastikan `## Peraturan` `qa-uat` hadkan Write/Edit kepada `*.Tests` sahaja.

## Rujukan

- Cipta subagent/skill: [`lab-subagent-peranan.md`](./lab-subagent-peranan.md) · Sambung MCP: [`lab-mcp-jira-figjam.md`](./lab-mcp-jira-figjam.md)
- Prompt ujian: [`pustaka-prompt.md`](./pustaka-prompt.md) (`UJI-01`…`UJI-03`, `DEV-07`) · DoD: [`../KOLABORASI.md`](../KOLABORASI.md) §9 · Senarai semak: [`senarai-semak-modul.md`](./senarai-semak-modul.md)
- Blok trek Hari 13–14 (xUnit) & Hari 15 (SIT/UAT): [`../hari-15/README.md`](../hari-15/README.md)
- Rasmi — **xUnit:** [xunit.net](https://xunit.net/docs/getting-started/v3/getting-started) · **EF Core SQLite testing:** [learn.microsoft.com/ef/core/testing/testing-without-the-database](https://learn.microsoft.com/en-us/ef/core/testing/testing-without-the-database) · **Claude in Chrome:** [docs.claude.com/claude-code](https://docs.claude.com/en/docs/claude-code/overview)
