# Nota — Simpan Kredential dalam IIS (bukan appsettings)

Panduan pengeluaran (Windows + IIS): pindahkan rahsia — **AppKey SSO, kunci RSA, connection string, SMTP** — keluar dari `appsettings.json` (dan keluar dari repo) ke **environment variables** peringkat tapak IIS.

> **Kenapa.** `appsettings.json` di-commit ke Git — sesiapa yang baca repo nampak rahsia (rujuk CM-61: kelayakan SMTP tercommit). Ini titik pengajaran keselamatan modul ID/AD/Email. **Dev tempatan** guna user-secrets (lihat [`nota-persediaan-tempatan`](./nota-persediaan-tempatan.html)); **pengeluaran IIS** guna environment variables seperti di bawah.

---

## Konsep — pemetaan kunci config → env var

ASP.NET Core membaca environment variables sebagai config, dan **env var mengatasi (override) `appsettings.json`** (susunan lalai: appsettings → appsettings.{Env} → env vars). Guna **dua garis bawah `__`** untuk paras hierarki (ganti `:`):

| Kunci config | Environment variable |
|--------------|----------------------|
| `Nres:Sso:AppKey` | `Nres__Sso__AppKey` |
| `Nres:Sso:PrivateKeyPath` | `Nres__Sso__PrivateKeyPath` |
| `ConnectionStrings:Default` | `ConnectionStrings__Default` |
| `Nres:Mail:Password` | `Nres__Mail__Password` |

Dalam `appsettings.json`, biar kunci ini **kosong atau tiada** — nilai sebenar datang dari env var pada server.

---

## Cara A (disyorkan) — IIS Configuration Editor, per-tapak

1. Buka **IIS Manager** → pilih **Site** anda (cth `NresPks`).
2. Klik dua kali **Configuration Editor**.
3. Pada **Section**, pilih: `system.webServer/aspNetCore`.
4. Cari baris **environmentVariables** → klik **(Collection) …** → butang **…**.
5. **Add** setiap satu (Name / Value):
   - `Nres__Sso__AppKey` = `<kunci-dari-pendaftaran-SSO>`
   - `Nres__Sso__PrivateKeyPath` = `C:\nres\keys\App.key`
   - `ConnectionStrings__Default` = `Server=...;Database=...;Trusted_Connection=True;`
6. Tutup dialog → klik **Apply** (kanan atas).
7. **Recycle** Application Pool tapak itu (atau `iisreset`).

Ini ditulis ke `web.config` tapak (bukan `appsettings.json`, bukan repo). Skop kepada satu tapak sahaja.

## Cara B — sunting `web.config` di server

`web.config` dijana oleh `dotnet publish`. Di server, tambah `<environmentVariables>` di bawah `<aspNetCore>`:

```xml
<aspNetCore processPath="dotnet" arguments=".\NresPks.dll" hostingModel="inprocess">
  <environmentVariables>
    <environmentVariable name="Nres__Sso__AppKey" value="&lt;kunci&gt;" />
    <environmentVariable name="Nres__Sso__PrivateKeyPath" value="C:\nres\keys\App.key" />
    <environmentVariable name="ConnectionStrings__Default" value="Server=...;Database=...;Trusted_Connection=True;" />
  </environmentVariables>
</aspNetCore>
```

> `web.config` yang mengandungi rahsia **jangan** di-commit. Ia hidup di server sahaja.

## Cara C — PowerShell (untuk automasi / CD)

```powershell
$site = "NresPks"
Import-Module WebAdministration
Add-WebConfigurationProperty -PSPath "IIS:\Sites\$site" `
  -Filter "system.webServer/aspNetCore/environmentVariables" -Name "." `
  -Value @{ name = "Nres__Sso__AppKey"; value = "<kunci>" }
Restart-WebAppPool -Name (Get-Item "IIS:\Sites\$site").applicationPool
```

## Cara D — env var peringkat mesin (fallback, kurang disyor)

```powershell
setx /M Nres__Sso__AppKey "<kunci>"
```

Kesan **semua** aplikasi pada mesin itu, perlu restart. Guna hanya jika satu app sahaja pada server.

---

## Kunci RSA (fail), bukan sekadar rentetan

Kunci peribadi `App.key` ialah **fail**, bukan rentetan dalam config:
- Letak di luar `wwwroot`, cth `C:\nres\keys\App.key`. Jangan commit.
- Set **NTFS permission**: hanya identity Application Pool (cth `IIS AppPool\NresPks` atau akaun servis) dapat **Read**; buang akses lain.
- Set `Nres__Sso__PrivateKeyPath` ke laluan itu (Cara A–C).

---

## ✅ Semakan

- [ ] `appsettings.json` di repo **tiada** rahsia (`grep -i "appkey\|password\|apikey" appsettings*.json` → kosong)
- [ ] Env var ditetapkan pada tapak; App Pool di-recycle
- [ ] App boot tanpa rahsia dalam `appsettings` (env var mengatasi)
- [ ] Fungsi SSO/Profile/DB berjalan (kredential dibaca dari env var)
- [ ] `web.config` + fail kunci: NTFS terhad kepada identity App Pool; tidak di-commit
- [ ] Rahsia lama dalam sejarah Git (jika ada) diputar (rotate)

## Rujukan

- Dev tempatan (user-secrets): [`nota-persediaan-tempatan`](./nota-persediaan-tempatan.html)
- Nota deployment Hari 15: [`hari-15-run-sheet.md`](./hari-15-run-sheet.md) (Bahagian E)
