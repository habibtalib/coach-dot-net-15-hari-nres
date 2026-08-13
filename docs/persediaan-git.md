# Persediaan Git (pasang & identiti)

> **Bahan rujukan kursus.** Pasang Git & set identiti **sebelum Hari 2**. Rujukan lab: [`hari-2/snippets/lab.md`](../hari-2/snippets/lab.md) Latihan 2.

## 1. Pasang Git

- Muat turun: [git-scm.com/downloads](https://git-scm.com/downloads).
- Sahkan: `git --version`.

## 2. Konfigurasi identiti

Nama & e-mel muncul dalam **setiap commit** — guna yang sebenar.

```bash
git config --global user.name "Nama Penuh Anda"
git config --global user.email "emel.anda@nres.gov.my"
git config --global pull.rebase true
```

> `pull.rebase true` menjadikan `git pull` berkelakuan seperti `git pull --rebase` secara lalai — tepat apa yang kursus ini mahu.

## 3. (Pilihan) Akaun GitHub & kelayakan

- Daftar/log masuk [github.com](https://github.com).
- Untuk push melalui HTTPS, guna **Git Credential Manager** atau **token akses peribadi (PAT)**.

## ✅ Sedia bila

- [ ] `git --version` berjaya
- [ ] `git config user.name` & `user.email` betul
- [ ] `git config pull.rebase` → `true`

> Persediaan .NET: [`persediaan-dotnet.md`](./persediaan-dotnet.md).
