# Nota — Pemantauan Ralat dengan Sentry (sentry.io)

Panduan menambah **pemantauan ralat & prestasi** ke aplikasi ASP.NET Core (.NET 10) menggunakan **Sentry** ([sentry.io](https://sentry.io/)). Ini kerja **pengukuhan pengeluaran** (pasca-kursus) — melengkapi health checks + logging (Serilog), bukan menggantikannya.

> **Apa Sentry beri.** Tangkap **ralat tak ditangani** (unhandled exception) automatik dengan stack trace + konteks, jejak prestasi (transaction/trace), dan kumpul ralat serupa. Anda nampak isu pengeluaran di dashboard, bukan menunggu aduan pengguna.

---

## Prasyarat

- Akaun **sentry.io** → cipta **Project** (platform: **.NET → ASP.NET Core**) → dapat **DSN** (bentuk `https://<key>@o<org>.ingest.sentry.io/<project>`).
- **Egress HTTPS** dari server ke `sentry.io` dibenarkan. Rangkaian kerajaan mungkin perlu **proxy / allowlist**, atau pertimbang **Sentry self-hosted** dalam rangkaian NRES untuk kekal data dalaman.

---

## Langkah 1 — Tambah pakej

```bash
dotnet add package Sentry.AspNetCore
```

## Langkah 2 — Wire dalam `Program.cs`

```csharp
builder.WebHost.UseSentry(options =>
{
    // DSN dibaca dari env var SENTRY_DSN jika dibiar kosong (lihat Langkah 3)
    options.Environment = builder.Environment.EnvironmentName;   // Production / Staging
    options.Release = "nres-pks@1.0.0";                          // untuk tapis ikut versi
    options.TracesSampleRate = 0.2;                              // 20% jejak prestasi
    options.SendDefaultPii = false;                             // JANGAN hantar data peribadi (lihat PII)
});
```

`UseSentry` sudah pasang middleware yang menangkap unhandled exception + integrasi `ILogger` (log paras Error/Warning jadi event).

## Langkah 3 — DSN sebagai environment variable (bukan commit)

DSN bukan kata laluan, tetapi ikut disiplin yang sama — **jangan commit** dalam `appsettings.json`. Pada IIS, set env var (Sentry baca `SENTRY_DSN` secara asli):

| Environment variable | Nilai |
|----------------------|-------|
| `SENTRY_DSN` | `https://<key>@o<org>.ingest.sentry.io/<project>` |

Cara set pada IIS (Config Editor / `web.config` / PowerShell): rujuk [`nota-kredential-iis`](./nota-kredential-iis.md). Dev tempatan: guna user-secrets (`dotnet user-secrets set "Sentry:Dsn" "…"`).

## Langkah 4 — Tangkap ralat secara manual (pilihan)

Unhandled exception ditangkap automatik. Untuk ralat yang anda kendali sendiri:

```csharp
try
{
    await _profileApi.GetByNricAsync(nric);
}
catch (HttpRequestException ex)
{
    SentrySdk.CaptureException(ex);   // hantar ke Sentry, tetap kendali dengan anggun
    // ... fallback ...
}

SentrySdk.CaptureMessage("Kuota GetProfile hampir habis", SentryLevel.Warning);
```

## Langkah 5 — Konteks berguna

```csharp
SentrySdk.ConfigureScope(scope =>
{
    scope.SetTag("modul", "pematuhan-pks");   // tapis ikut modul/kumpulan
    scope.SetTag("kumpulan", "K1");
});
```

`Environment` + `Release` (Langkah 2) membolehkan tapis "ralat dalam Production versi 1.0.0" di dashboard.

## Langkah 6 — Uji sambungan

Tambah endpoint ujian **Development sahaja**, cetus sekali, lihat ia muncul di dashboard Sentry, kemudian buang:

```csharp
if (app.Environment.IsDevelopment())
    app.MapGet("/debug/sentry", () => { throw new Exception("Ujian Sentry NRES"); });
```

---

## PII & keselamatan (penting untuk sistem kerajaan)

- **`SendDefaultPii = false`** — jangan hantar NRIC, nama, e-mel, atau IP pengguna ke Sentry secara lalai.
- **Tapis (scrub)** medan sensitif sebelum hantar:

```csharp
options.SetBeforeSend((sentryEvent, hint) =>
{
    sentryEvent.User.Email = null;      // buang PII
    sentryEvent.User.IpAddress = null;
    return sentryEvent;                 // pulangkan null untuk gugurkan event sepenuhnya
});
```

- Data **sintetik** sahaja semasa latihan.
- Untuk data kerajaan sebenar: pertimbang **Sentry self-hosted** supaya jejak ralat tidak keluar rangkaian NRES.

---

## ✅ Semakan

- [ ] `Sentry.AspNetCore` ditambah; `UseSentry(...)` di-wire dalam `Program.cs`
- [ ] DSN dari **env var** (`SENTRY_DSN`), bukan di-commit dalam `appsettings.json`
- [ ] Endpoint ujian mencetus ralat → **muncul di dashboard Sentry**
- [ ] `SendDefaultPii = false` dan `SetBeforeSend` menapis PII
- [ ] `Environment` + `Release` ditetapkan (boleh tapis ikut versi/persekitaran)
- [ ] Egress ke `sentry.io` dibenarkan (atau guna Sentry self-hosted)

## Rujukan

- Dokumentasi rasmi: Sentry for .NET / ASP.NET Core ([sentry.io](https://sentry.io/) → Docs → Platforms → .NET).
- DSN sebagai env var pada IIS: [`nota-kredential-iis`](./nota-kredential-iis.md)
- Observability dalam konteks deployment: [`hari-15-run-sheet.md`](./hari-15-run-sheet.md) (Bahagian E)
