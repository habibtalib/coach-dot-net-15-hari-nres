# Deployment 🚀

> Nota konsep untuk **Hari 15** (integrasi & demo capstone). Lihat [`02-efcore-migrations.md`](./02-efcore-migrations.md) untuk asas migration, dan [`09-keselamatan.md`](./09-keselamatan.md) untuk keselamatan pengeluaran.

---

## `appsettings` — konfigurasi per persekitaran

ASP.NET Core menyokong fail konfigurasi **berlapis** mengikut persekitaran (`Development`, `Staging`, `Production`):

```text
appsettings.json                  ← tetapan asas (semua persekitaran)
appsettings.Development.json      ← override untuk pembangunan tempatan
appsettings.Production.json       ← override untuk pengeluaran
```

`appsettings.Production.json`:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=nres-sql-prod;Database=NresOnboarding;User Id=app_user;Password=#{DB_PASSWORD}#;TrustServerCertificate=True"
  },
  "Logging": {
    "LogLevel": { "Default": "Warning" }
  }
}
```

> **Jangan sesekali** simpan kata laluan pangkalan data sebenar dalam fail `.json` yang dikawal versi (git). Guna pembolehubah persekitaran (*environment variables*), **User Secrets** (pembangunan), atau *secret manager* pengeluaran (Azure Key Vault, AWS Secrets Manager) — lihat [`09-keselamatan.md`](./09-keselamatan.md).

Persekitaran ditentukan melalui pembolehubah `ASPNETCORE_ENVIRONMENT`:

```bash
export ASPNETCORE_ENVIRONMENT=Production
```

---

## Tukar SQLite → SQL Server untuk pengeluaran

Ini ditunjukkan **secara langsung** di Hari 15, membuktikan corak *provider-agnostic* EF Core:

```bash
dotnet add package Microsoft.EntityFrameworkCore.SqlServer
```

```csharp
// Program.cs
if (builder.Environment.IsProduction())
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
}
else
{
    builder.Services.AddDbContext<ApplicationDbContext>(options =>
        options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));
}
```

> Migration yang sudah ditulis sepanjang kursus ([`02-efcore-migrations.md`](./02-efcore-migrations.md)) **tidak perlu ditulis semula** — hanya *provider* dalam `Up()`/`Down()` yang mungkin perlu sedikit penyesuaian jika ada jenis data khusus SQLite.

---

## Jalankan migration semasa deploy

```bash
dotnet ef database update --connection "Server=...;Database=NresOnboarding;..."
```

Atau, terapkan migration secara automatik semasa aplikasi mula (untuk persekitaran terkawal — bukan disyorkan untuk pengeluaran besar tanpa kawalan, tetapi berguna untuk demo/staging):

```csharp
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    await db.Database.MigrateAsync();
}
```

> **Amalan pengeluaran sebenar:** jalankan migration sebagai langkah **berasingan** dalam *pipeline* CI/CD (bukan automatik semasa *startup* aplikasi), supaya boleh disemak & di-*rollback* secara terkawal.

---

## HTTPS

ASP.NET Core menguatkuasakan HTTPS secara *default* dalam templat projek:

```csharp
app.UseHttpsRedirection();
app.UseHsts();   // untuk pengeluaran — arahkan browser guna HTTPS untuk lawatan akan datang
```

Untuk pengeluaran sebenar, sijil TLS biasanya dikendalikan oleh *reverse proxy* (IIS, Nginx) atau *load balancer* di hadapan aplikasi — bukan oleh Kestrel secara terus.

---

## Kebenaran folder muat naik

Folder `App_Data/uploads/` (lihat [`06-file-upload.md`](./06-file-upload.md)) memerlukan kebenaran tulis untuk akaun perkhidmatan yang menjalankan aplikasi:

| Platform | Arahan |
|----------|--------|
| **Linux (systemd)** | `chown -R www-data:www-data /var/www/nres/App_Data/uploads && chmod -R 750 App_Data/uploads` |
| **IIS (Windows)** | Beri kebenaran *Modify* kepada `IIS_IUSRS` pada folder `App_Data\uploads` |
| **Kontena (Docker)** | Guna *volume* berterusan (*persistent volume*) dipasang pada `/app/App_Data/uploads`, bukan lapisan kontena sementara |

---

## Hosting: IIS / Linux systemd / kontena

### Opsyen A — IIS (Windows Server)

1. Pasang **ASP.NET Core Hosting Bundle** pada server.
2. `dotnet publish -c Release -o ./publish`
3. Konfigurasi *Application Pool* — **No Managed Code** (Kestrel kendalikan runtime .NET, IIS jadi *reverse proxy*).
4. Salin kandungan `./publish` ke folder *site* IIS.

### Opsyen B — Linux dengan systemd

```bash
dotnet publish -c Release -o /var/www/nres
```

`/etc/systemd/system/nres-onboarding.service`:

```ini
[Unit]
Description=Nres Onboarding Web App

[Service]
WorkingDirectory=/var/www/nres
ExecStart=/usr/bin/dotnet /var/www/nres/Nres.Onboarding.Web.dll
Restart=always
User=www-data
Environment=ASPNETCORE_ENVIRONMENT=Production

[Install]
WantedBy=multi-user.target
```

```bash
sudo systemctl enable nres-onboarding
sudo systemctl start nres-onboarding
```

Letakkan Nginx sebagai *reverse proxy* di hadapan (HTTPS, domain, *load balancing*).

### Opsyen C — Kontena (Docker)

```dockerfile
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS base
WORKDIR /app
COPY ./publish .
ENTRYPOINT ["dotnet", "Nres.Onboarding.Web.dll"]
```

---

## Sandaran (Backup)

- **Pangkalan data**: jadualkan *backup* automatik SQL Server (`sqlcmd`/*maintenance plan*) atau `pg_dump` untuk PostgreSQL.
- **Folder muat naik**: `App_Data/uploads/` mesti turut disandarkan (bukan hanya pangkalan data) — lampiran tidak wujud dalam DB, hanya metadata.
- Uji **pemulihan** (*restore*) secara berkala, bukan hanya cipta sandaran.

---

## Seed pengguna & data lookup awal

```csharp
if (!await userManager.Users.AnyAsync())
{
    var admin = new ApplicationUser { UserName = "admin@nres.gov.my", Email = "admin@nres.gov.my" };
    await userManager.CreateAsync(admin, "TukarSelepasLogMasukPertama!1");
    await userManager.AddToRoleAsync(admin, "SystemAdmin");
}
```

> Kata laluan awal **wajib** ditukar semasa log masuk pertama — jangan biarkan kata laluan seed kekal dalam pengeluaran.

---

## Senarai Semak Pelepasan (Release Checklist)

- [ ] `appsettings.Production.json` tidak mengandungi rahsia sebenar (guna *secret manager*)
- [ ] `ASPNETCORE_ENVIRONMENT=Production` ditetapkan
- [ ] Migration EF Core dijalankan & disahkan terhadap pangkalan data pengeluaran
- [ ] HTTPS + HSTS diaktifkan
- [ ] Kebenaran folder `App_Data/uploads/` betul (bukan awam, boleh tulis oleh akaun servis)
- [ ] Pengguna & peranan awal (`SystemAdmin`) di-*seed*, kata laluan seed wajib ditukar
- [ ] Sandaran pangkalan data **dan** folder muat naik dijadualkan
- [ ] Log/audit diaktifkan dan disemak selepas *deploy*

---

## Kaitan dengan hari-hari lain

- **Hari 1–14** — semua kod dibina siap sebelum Hari 15.
- **Hari 15** — deployment ditunjukkan penuh, bersama ujian xUnit ([`07-testing-xunit.md`](./07-testing-xunit.md)) sebagai sebahagian *pipeline* pelepasan.

---

## Sumber Rasmi

- **[Host and deploy ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/)**
- **[Configuration in ASP.NET Core](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/)**
- **[Deploy ASP.NET Core apps to IIS](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/iis/)**
- **[Host ASP.NET Core on Linux with Nginx](https://learn.microsoft.com/en-us/aspnet/core/host-and-deploy/linux-nginx)**
- **[Safe storage of app secrets](https://learn.microsoft.com/en-us/aspnet/core/security/app-secrets)**
