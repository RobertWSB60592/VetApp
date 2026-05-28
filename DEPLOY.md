# VetMed — Uruchomienie na nowym urządzeniu

> Instrukcja dla deweloperów i testerów. Obejmuje setup środowiska, konfigurację sieci i uruchomienie wszystkich komponentów.

---

## Wymagania systemowe

| Narzędzie | Wersja | Pobierz |
|---|---|---|
| Windows 10/11 (64-bit) | 10.0.17763+ | — |
| Visual Studio 2022 | 17.9+ | https://visualstudio.microsoft.com |
| .NET SDK | 9.0.x | https://dotnet.microsoft.com/download/dotnet/9.0 |
| Docker Desktop | dowolna | https://www.docker.com/products/docker-desktop |
| Git | dowolna | https://git-scm.com |

### Workloady Visual Studio 2022

Podczas instalacji VS 2022 (lub przez **Tools → Get Tools and Features**):

- ✅ ASP.NET and web development
- ✅ .NET Multi-platform App UI development
- ✅ .NET desktop development

### Workloady .NET (terminal)

```bash
dotnet workload install maui
dotnet workload install android
dotnet tool install --global dotnet-ef
```

Sprawdzenie:

```bash
dotnet --version          # 9.0.x
dotnet workload list      # maui-windows, android
dotnet ef --version       # 9.x
docker --version          # dowolna
```

---

## 1. Pobranie kodu

```bash
git clone <URL_REPO> VetMed
cd VetMed
```

Lub skopiuj folder `F:\Claude apps\Apka\VetMed` na nowe urządzenie.

---

## 2. Konfiguracja adresu IP (ważne dla innych urządzeń)

> **Domyślnie API nasłuchuje na `localhost:5100`.**
> Jeśli MAUI app działa na tym samym komputerze co API — nic nie zmieniaj.
> Jeśli uruchamiasz app na telefonie / innym PC — zmień adres na IP serwera.

### Znajdź IP komputera z API

```powershell
# Windows PowerShell
(Get-NetIPAddress -AddressFamily IPv4 | Where-Object { $_.InterfaceAlias -notlike "*Loopback*" })[0].IPAddress
```

Przykład: `192.168.1.105`

### Zmień adres w MauiProgram.cs

```
VetMed.App/MauiProgram.cs  →  linia z AddHttpClient
```

```csharp
// Localhost (ten sam komputer):
c.BaseAddress = new Uri("http://localhost:5100/");

// Inne urządzenie w sieci LAN:
c.BaseAddress = new Uri("http://192.168.1.105:5100/");

// Android Emulator (wbudowany emulator Google):
c.BaseAddress = new Uri("http://10.0.2.2:5100/");
```

### Odblokuj port w Windows Firewall (jeśli inne urządzenie)

```powershell
# Uruchom jako Administrator
New-NetFirewallRule -DisplayName "VetMed API" -Direction Inbound -Protocol TCP -LocalPort 5100 -Action Allow
```

### Zmień nasłuch API (jeśli inne urządzenie)

W `VetMed.Api/Program.cs` zmień ostatnią linię:

```csharp
// Tylko localhost (domyślnie):
app.Run("http://localhost:5100");

// Dostępne z sieci LAN:
app.Run("http://0.0.0.0:5100");
```

---

## 3. Uruchomienie bazy danych

Upewnij się że **Docker Desktop jest uruchomiony** (ikona w zasobniku systemowym).

```bash
cd "ścieżka/do/VetMed"
docker compose up -d
```

Weryfikacja (poczekaj ~15 sekund):

```bash
docker ps --filter "name=vetmed"
# Oczekiwany wynik:
# vetmed-db      Up ... (healthy)
# vetmed-pgadmin Up ...
```

| Serwis | URL | Login |
|---|---|---|
| PostgreSQL | localhost:5432 | vetmed / vetmed123 |
| pgAdmin | http://localhost:5050 | admin@vetmed.pl / admin123 |

---

## 4. Migracja bazy danych

> Wymagane tylko przy pierwszym uruchomieniu lub po zmianie modelu.

```bash
cd "ścieżka/do/VetMed"

dotnet ef migrations add InitialCreate \
  --project VetMed.Infrastructure \
  --startup-project VetMed.Api

dotnet ef database update \
  --project VetMed.Infrastructure \
  --startup-project VetMed.Api
```

---

## 5. Uruchomienie API

```bash
dotnet run --project VetMed.Api
```

API automatycznie przy starcie:
- stosuje migracje (`db.Database.MigrateAsync()`)
- seeduje dane demo jeśli baza pusta

Sprawdzenie: **http://localhost:5100/scalar/v1**

### Dane demo

| Email | Hasło | Opis |
|---|---|---|
| demo@vetmed.pl | demo123 | Właściciel: Robert Demo, 4 pupile |

---

## 6. Uruchomienie MAUI App (Windows)

W **Visual Studio 2022**:

1. Otwórz `VetMed.sln`
2. W Solution Explorer: PPM na `VetMed.App` → **Set as Startup Project**
3. Z listy wybierz **Windows Machine**
4. **F5** (lub Ctrl+F5 bez debuggera — szybciej)

---

## 7. Uruchomienie MAUI App (Android)

### Opcja A — Emulator Android

1. VS 2022 → **Tools → Android → Android Device Manager**
2. Utwórz nowe urządzenie (Pixel 6, API 34)
3. Uruchom emulator
4. W MauiProgram.cs użyj adresu `http://10.0.2.2:5100/` (emulator łączy się z hostem przez `10.0.2.2`)
5. Wybierz emulator z listy → **F5**

### Opcja B — Fizyczny telefon Android

1. Na telefonie: **Ustawienia → Opcje deweloperskie → Debugowanie USB** ✅
2. Podłącz kabel USB
3. W MauiProgram.cs użyj IP komputera: `http://192.168.1.105:5100/`
4. Telefon i komputer muszą być w tej samej sieci Wi-Fi
5. Wybierz telefon z listy urządzeń → **F5**

---

## 8. Uruchomienie Blazor Clinic (panel kliniki)

```bash
dotnet run --project VetMed.Clinic
```

→ http://localhost:5200

---

## Kolejność uruchamiania (skrót)

```
1. Docker Desktop (start ręczny)
2. docker compose up -d
3. dotnet run --project VetMed.Api
4. VS 2022 → VetMed.App → F5
```

---

## Rozwiązywanie problemów

| Problem | Rozwiązanie |
|---|---|
| `docker compose: no configuration file` | Musisz być w folderze `VetMed/` z plikiem `docker-compose.yml` |
| `dotnet-ef not found` | `dotnet tool install --global dotnet-ef` + otwórz nowy terminal |
| API: `Connection refused` | Docker Desktop nie jest uruchomiony lub migracja nie zastosowana |
| App: `Nieprawidłowy email lub hasło` | API nie nasłuchuje lub zły adres IP w MauiProgram.cs |
| App: `Błąd połączenia` | Sprawdź IP, firewall, czy API działa na `0.0.0.0:5100` |
| Android: nie widać urządzenia | Włącz debugowanie USB, zainstaluj sterowniki ADB |
| Build: `NETSDK1202` net8.0 EOL | Użyj net9.0 — zmień TargetFrameworks w VetMed.App.csproj |
