# VetMed — uruchomienie lokalne (Wariant 1: wszystko lokalnie)

Instrukcja postawienia całego środowiska na nowym komputerze: **baza + API + panel kliniki + aplikacja mobilna**, wszystko lokalnie, bez GCP i bez proxy.

> Panel kliniki znajduje się na branchu **`lokal`**. Na innych branchach (`gcp`, `master`) go nie ma.

---

## 1. Wymagania (zainstaluj raz)

| Narzędzie | Do czego | Skąd |
|---|---|---|
| **.NET 9 SDK** | API, panel, build appki | https://dotnet.microsoft.com/download |
| **Docker Desktop** | PostgreSQL | https://www.docker.com/products/docker-desktop |
| **git** | pobranie kodu | https://git-scm.com |
| **Visual Studio 2022** + workload **.NET MAUI** i **Android** | aplikacja mobilna | https://visualstudio.microsoft.com |

> Bez VS appkę można zbudować z CLI po `dotnet workload install maui-android`.

---

## 2. Pobierz kod i przełącz na branch `lokal`

```powershell
git clone https://github.com/Darmarus/VetMed.git VetMed
cd VetMed
git checkout lokal
```

---

## 3. Odpal backend (3 komendy)

Każda w katalogu repo (`VetMed`):

```powershell
docker-compose up -d                 # PostgreSQL na localhost:5432
dotnet run --project VetMed.Api      # API na :5100 — automatyczna migracja bazy + seed
dotnet run --project VetMed.Clinic   # panel kliniki na :5278
```

- API przy starcie samo migruje bazę i seeduje dane (admin, lekarze, konta demo).
- Configi deweloperskie (connection string `vetmed`/`vetmed123`, klucz JWT) są w repo w `appsettings.Development.json` — **nic nie trzeba dopisywać**.

---

## 4. Aplikacja mobilna

Visual Studio:
1. Otwórz `VetMed.sln`
2. Projekt startowy = **VetMed.App**
3. Konfiguracja = **Debug**
4. Urządzenie = **Android Emulator** (jeśli brak: Tools → Android → Device Manager → utwórz AVD)
5. **F5**

CLI (emulator musi już działać):
```powershell
dotnet build VetMed.App/VetMed.App.csproj -t:Run -f net9.0-android -c Debug
```

> W trybie **Debug** aplikacja łączy się z `http://10.0.2.2:5100` — to adres hosta widziany z **emulatora Androida**. API musi działać (krok 3).

---

## 5. Dostęp

| Usługa | Adres | Login |
|---|---|---|
| Panel kliniki | http://localhost:5278/login | `admin@vetmed.pl` / `admin123` |
| API (dev UI) | http://localhost:5100/scalar | — |
| Baza (PostgreSQL) | localhost:5432 | `vetmed` / `vetmed123` (db: `vetmed`) |

### Przepływ testowy
1. W aplikacji: rejestracja/logowanie → dodaj zwierzaka → **umów wizytę**.
2. Wizyta trafia do bazy ze statusem **Oczekująca**.
3. W panelu kliniki na `/` pojawia się na liście → **Zatwierdź** / **Odrzuć** (z powodem).

---

## 6. Warianty adresu API

Domyślnie (Debug → emulator) działa od ręki. Zmiana w `VetMed.App/MauiProgram.cs`:

| Cel | `ApiBase` (Debug) |
|---|---|
| Emulator Androida | `http://10.0.2.2:5100/` (domyślnie) |
| Fizyczny telefon | `http://<IP-komputera>:5100/` — telefon na tym samym Wi-Fi + port 5100 otwarty w firewallu |
| Aplikacja na Windows | `http://localhost:5100/` |

---

## 7. Porty

| Port | Usługa |
|---|---|
| 5100 | API |
| 5278 | Panel kliniki |
| 5432 | PostgreSQL |

---

## 8. Najczęstsze problemy

- **`git fetch/clone` → SSL certificate problem (schannel)**
  ```powershell
  git -c http.sslBackend=schannel clone https://github.com/Darmarus/VetMed.git VetMed
  ```
- **`curl` → CRYPT_E_NO_REVOCATION_CHECK** — dodaj `--ssl-no-revoke`.
- **Panel: pusta lista wizyt** — to normalne, dopóki nie umówisz wizyty (status Oczekująca).
- **CLI MAUI: brak workloadu** — `dotnet workload install maui-android` lub odpal z Visual Studio.
- **Port zajęty** — zatrzymaj proces trzymający port:
  ```powershell
  Get-NetTCPConnection -LocalPort 5100 -State Listen | ForEach-Object { Stop-Process -Id $_.OwningProcess -Force }
  ```
- **Ostrzeżenie „Failed to determine the https port"** przy panelu — nieszkodliwe (profil http).
- **Pierwszy build MAUI trwa długo** — pobiera Android SDK / obrazy emulatora.

---

## 9. Zatrzymanie

```powershell
# API / panel — Ctrl+C w ich oknach
docker-compose stop          # zatrzymaj bazę (dane zostają)
docker-compose down          # usuń kontenery (dane w wolumenie zostają)
```
