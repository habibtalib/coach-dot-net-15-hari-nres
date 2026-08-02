# Kumpulan 3 · Hari 13–14 — RBAC Testing, Security Audit & Sedia Gabung

> Trek: [`../README.md`](../README.md) · Aturcara: [`../../JADUAL.md`](../../JADUAL.md) · Kontrak: [`../../KOLABORASI.md`](../../KOLABORASI.md)

Dua hari. **Tiada ciri baharu.** Hujungnya, modul anda diuji, **keselamatan diaudit**, dan cabang bersedia untuk gabungan Hari 15.

---

## Fokus Blok Ini

| Topik | Rujukan rasmi |
|-------|----------------|
| xUnit | [xunit.net/docs/getting-started/v3/getting-started](https://xunit.net/docs/getting-started/v3/getting-started) · Buku Bab 4 (m.s. 201) |
| Ujian dengan EF Core | [learn.microsoft.com/ef/core/testing](https://learn.microsoft.com/en-us/ef/core/testing/) |
| Amalan keselamatan ASP.NET Core | [learn.microsoft.com/aspnet/core/security](https://learn.microsoft.com/en-us/aspnet/core/security/) |
| OWASP Top 10 | [owasp.org/www-project-top-ten](https://owasp.org/www-project-top-ten/) |

## Jadual Blok Ini

| Hari | Fokus |
|------|-------|
| **Hari 13** | Ujian unit — laluan kelulusan, turutan langkah, kelulusan separa, penjanaan nama AD |
| **Hari 14** | **Security audit**, refactor, dokumentasi, sedia gabung |

**Hasil:** Ujian yang lulus untuk peraturan aliran kerja anda; laporan security audit; cabang bergabung bersih.

---

## Apa yang patut diuji dalam modul anda

Modul anda mempunyai **peraturan aliran kerja terkompleks** dalam kursus. Uji peraturan itu.

| Uji ini | Kenapa |
|---------|--------|
| **Turutan langkah dikuatkuasakan** | ICT tidak boleh memintas penyelia — pelanggaran keselamatan |
| Laluan dicipta dengan 2 langkah | Asas segala-galanya |
| `CreateRouteAsync` idempoten | Penghantaran semula tidak menggandakan langkah |
| Keputusan pada langkah yang sudah diputuskan ditolak | Integriti data |
| Kelulusan separa → status betul | Semua ditolak = `Rejected`; ada diluluskan = `AdminApproved` |
| Penjanaan nama akaun AD | Gelaran Melayu, diakritik, kes tepi |
| Keunikan nama akaun | Rekod rasmi |
| Peraturan validation (6 daripadanya) | Peraturan perniagaan |

## Security audit: apa yang anda semak

Anda pasukan akses. Security audit ialah **kemuncak** trek anda — bukan tugasan sampingan.

### 1. Tiada kelayakan di mana-mana

```bash
grep -rniE "password|kata.?laluan|passwd|secret|credential" \
  Nres.Onboarding.Web/Models/Akaun/ \
  Nres.Onboarding.Web/Services/Akaun/ \
  Nres.Onboarding.Web/ViewModels/Akaun/
```

Setiap padanan mesti dijustifikasi (contohnya, komen amaran) atau dibuang.

### 2. Matriks RBAC masih lulus

Anda membinanya pada Hari 9. Jalankan **semula** — modul telah berubah sejak itu, dan kumpulan lain telah menggabungkan kerja.

### 3. Kebenaran peringkat objek

Semakan peranan menjawab *"adakah anda `IctAdmin`?"*. Ia **tidak** menjawab *"adakah rekod ini milik anda?"*

| Semakan | Diuji? |
|---------|--------|
| Pemohon A membuka permohonan pemohon B | |
| Penyelia X meluluskan permohonan yang ditetapkan kepada penyelia Y | |
| `IctAdmin` memproses permohonan yang belum lulus penyelia | |

Ini **Broken Object Level Authorization** — kelemahan API #1 OWASP, dan yang paling kerap terlepas dalam kursus.

### 4. Audit tidak boleh diubah

Audit log dimaksudkan sebagai rekod. Semak:

- Tiada action controller mengemas kini atau memadam `AuditLog`
- Tiada laluan kepada `AuditLog` melalui borang
- Nilai `ActorUserId` datang dari `ICurrentUserService`, **bukan** dari input borang

Yang terakhir itu penting: jika `ActorUserId` boleh dihantar dari borang, sesiapa boleh memalsukan siapa yang meluluskan sesuatu.

### 5. Kebocoran maklumat

- Adakah mesej ralat mendedahkan sama ada pengguna wujud?
- Adakah skrin penyelia mendedahkan data yang tidak sepatutnya mereka lihat?
- Adakah ralat 403 dan 404 boleh dibezakan dengan cara yang mendedahkan kewujudan rekod?

> Yang terakhir itu halus: mengembalikan 404 untuk "wujud tetapi bukan milik anda" dan 403 untuk "milik anda tetapi tidak dibenarkan" memberitahu penyerang rekod mana yang wujud. Untuk kursus ini, konsisten memadai — tetapi **nyatakan** keputusan.

## Persediaan gabungan

Menjelang hujung Hari 14:

- [ ] `dotnet build` bersih
- [ ] `dotnet test` semua lulus
- [ ] Digabung dengan `master` terkini
- [ ] Gabungan kering tiada konflik
- [ ] `README-modul.md` ditulis
- [ ] **Laporan security audit** ditulis

> **Perhatian khusus Kumpulan 3:** anda **mengatasi `Reject`** dan menambah `SupervisorApprove`. Kedua-duanya mesti dinyatakan dalam `README-modul.md` — orang lain perlu tahu modul anda mempunyai laluan kelulusan yang berbeza.
>
> Jika isu `shared` anda tentang `SubmissionControllerBase` berbilang peranan diluluskan, sahkan perubahan itu digabung dan `Reject` anda dipermudahkan.

---

## Seterusnya

Ikuti [`snippets/lab.md`](./snippets/lab.md).

> **Nota penceramah** (`nota-penceramah.md`) — pemasaan sesi, poin bercakap, silap
> biasa, soalan perbincangan. Bahan **jurulatih sahaja**; ia tidak disertakan dalam
> repo ini (lihat `.gitignore`) dan diedarkan berasingan.
