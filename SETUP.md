# VetMed — Setup w Visual Studio 2022

> Stack: .NET 9 · MAUI Blazor Hybrid · Blazor Server · ASP.NET Core REST API · PostgreSQL 16 · Docker · JWT

---

## Wymagania wstępne

| Narzędzie | Wersja | Sprawdzenie |
|---|---|---|
| Visual Studio 2022 | 17.9+ | Help → About |
| .NET SDK | 9.x lub 10.x preview | `dotnet --version` |
| MAUI workload | zainstalowany | `dotnet workload list` → `maui-windows` |
| Docker Desktop | dowolna | `docker --version` |

Workloady VS 2022 (Tools → Get Tools and Features):
- ✅ ASP.NET and web development
- ✅ .NET Multi-platform App UI development
- ✅ .NET desktop development

---

## Krok 1 — Utwórz solution

```
VS 2022 → File → New → Project
  Typ:        Blank Solution
  Nazwa:      VetMed
  Lokalizacja: F:\Claude apps\Apka
```

Powstaie: `F:\Claude apps\Apka\VetMed\VetMed.sln`

---

## Krok 2 — Dodaj 6 projektów

W Solution Explorer: PPM na solution → **Add → New Project**

| # | Nazwa projektu | Szablon VS | Target | Uwagi |
|---|---|---|---|---|
| 1 | `VetMed.Shared` | Class Library | net9.0 | Usuń `Class1.cs` |
| 2 | `VetMed.Infrastructure` | Class Library | net9.0 | Usuń `Class1.cs` |
| 3 | `VetMed.Api` | ASP.NET Core Web API | net9.0 | Odznacz „Use controllers", zaznacz OpenAPI/Swagger |
| 4 | `VetMed.Clinic` | Blazor Web App | net9.0 | Render mode = **Server**, Interactivity = **Global** |
| 5 | `VetMed.App` | .NET MAUI Blazor Hybrid App | net9.0 | — |
| 6 | `VetMed.Tests` | xUnit Test Project | net9.0 | — |

---

## Krok 3 — Referencje między projektami

PPM na projekt → **Add → Project Reference**

```
VetMed.Infrastructure  ←  VetMed.Shared
VetMed.Api             ←  VetMed.Shared + VetMed.Infrastructure
VetMed.Clinic          ←  VetMed.Shared + VetMed.Infrastructure
VetMed.App             ←  VetMed.Shared
VetMed.Tests           ←  VetMed.Api + VetMed.Shared
```

---

## Krok 4 — NuGet packages

Tools → NuGet Package Manager → **Manage NuGet Packages for Solution**

| Projekt | Pakiet | Wersja |
|---|---|---|
| VetMed.Infrastructure | `Microsoft.EntityFrameworkCore` | 9.x |
| VetMed.Infrastructure | `Npgsql.EntityFrameworkCore.PostgreSQL` | 9.x |
| VetMed.Infrastructure | `Microsoft.EntityFrameworkCore.Design` | 9.x |
| VetMed.Api | `Microsoft.AspNetCore.Authentication.JwtBearer` | 9.x |
| VetMed.Api | `FluentValidation.AspNetCore` | latest |
| VetMed.Api | `Serilog.AspNetCore` | latest |
| VetMed.Api | `BCrypt.Net-Next` | latest |
| VetMed.Tests | `Moq` | latest |
| VetMed.Tests | `FluentAssertions` | latest |
| VetMed.Tests | `Microsoft.AspNetCore.Mvc.Testing` | 9.x |

---

## Krok 5 — Uruchom bazę danych (Docker)

```bash
# Terminal w F:\Claude apps\Apka\VetMed\
docker compose up -d
```

| Serwis | URL | Dane logowania |
|---|---|---|
| PostgreSQL | `localhost:5432` | vetmed / vetmed123 |
| pgAdmin | http://localhost:5050 | admin@vetmed.pl / admin123 |

W pgAdmin: Add New Server → Host: `vetmed-db`, User: `vetmed`, Pass: `vetmed123`

---

## Krok 6 — Pierwszy build i uruchomienie

```bash
dotnet build VetMed.sln
dotnet run --project VetMed.Api      # → http://localhost:5100/swagger
dotnet run --project VetMed.Clinic   # → http://localhost:5200
```

MAUI (Windows): VS 2022 → ustaw `VetMed.App` jako Startup Project → **Windows Machine** → F5

---

## Krok 7 — EF Core migrations

```bash
# Zainstaluj narzędzie (raz globalnie)
dotnet tool install --global dotnet-ef

# Utwórz pierwszą migrację
dotnet ef migrations add InitialCreate \
  --project VetMed.Infrastructure \
  --startup-project VetMed.Api

# Zastosuj na bazie
dotnet ef database update \
  --project VetMed.Infrastructure \
  --startup-project VetMed.Api
```

---

## Struktura docelowa

```
VetMed.sln
├── VetMed.Shared/
│   ├── Models/        Pet.cs · Owner.cs · Visit.cs · Doctor.cs
│   ├── DTOs/          PetDto.cs · CreatePetDto.cs · AuthDto.cs
│   └── Enums/         VisitType.cs · VisitStatus.cs
│
├── VetMed.Infrastructure/
│   ├── Data/          AppDbContext.cs
│   ├── Configurations/ PetConfiguration.cs · VisitConfiguration.cs
│   ├── Repositories/  IPetRepository.cs · PetRepository.cs
│   └── Migrations/    (generowane przez EF)
│
├── VetMed.Api/
│   ├── Controllers/   AuthController.cs · PetsController.cs · VisitsController.cs
│   ├── Services/      IPetService.cs · PetService.cs
│   ├── Middleware/    ExceptionMiddleware.cs
│   └── Extensions/   ServiceCollectionExtensions.cs
│
├── VetMed.Clinic/
│   └── Components/Pages/  Home.razor · Visits.razor · Pets.razor · Login.razor
│
├── VetMed.App/
│   ├── Components/Pages/  Home.razor · PetList.razor · BookVisit.razor
│   └── Services/          ApiClient.cs · PetApiService.cs
│
├── VetMed.Tests/
│   ├── Unit/          PetServiceTests.cs
│   └── Integration/   PetsControllerTests.cs
│
└── docker-compose.yml
```

---

## Connection string (appsettings.json)

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Port=5432;Database=vetmed;Username=vetmed;Password=vetmed123"
  },
  "Jwt": {
    "Key": "zmien-na-produkcji-minimum-32-znaki!!",
    "Issuer": "VetMed.Api",
    "Audience": "VetMed.App",
    "ExpiryMinutes": 60
  }
}


Plan: Scaffold VetMed solution w Visual Studio 2022
Kontekst
Tworzymy szkielet aplikacji VetMed w F:\Claude apps\Apka\VetMed\.
Target: net9.0 (LTS). SDK .NET 10 preview obsługuje targeting net9.0.
Podejście: kroki w Visual Studio 2022 + kilka poleceń CLI dla docker i NuGet.

Krok 1 — Solution i projekty w Visual Studio
1.1 Utwórz solution
VS 2022 → File → New → Project
Typ: Blank Solution
Nazwa: VetMed
Lokalizacja: F:\Claude apps\Apka
Folder solution: VetMed   ← automatycznie tworzy F:\Claude apps\Apka\VetMed\VetMed.sln
1.2 Dodaj 6 projektów (Solution Explorer → Add → New Project)
Projekt	Szablon VS	Target
VetMed.Shared	Class Library	net9.0
VetMed.Infrastructure	Class Library	net9.0
VetMed.Api	ASP.NET Core Web API	net9.0
VetMed.Clinic	Blazor Web App	net9.0
VetMed.App	.NET MAUI Blazor Hybrid App	net9.0
VetMed.Tests	xUnit Test Project	net9.0
Przy VetMed.Api: odznacz "Use controllers" (użyjemy minimal API lub dodamy ręcznie).
Przy VetMed.Clinic: Interactive render mode = Server, Interactivity location = Global.
Usuń wygenerowane pliki Class1.cs z Shared i Infrastructure.

1.3 Referencje między projektami
VetMed.Infrastructure → VetMed.Shared
VetMed.Api            → VetMed.Shared + VetMed.Infrastructure
VetMed.Clinic         → VetMed.Shared + VetMed.Infrastructure
VetMed.App            → VetMed.Shared
VetMed.Tests          → VetMed.Api + VetMed.Shared
(PPM na projekt → Add → Project Reference)

Krok 2 — NuGet packages (Tools → NuGet Package Manager → Manage for Solution)
Projekt	Pakiet
Infrastructure	Microsoft.EntityFrameworkCore 9.x
Infrastructure	Npgsql.EntityFrameworkCore.PostgreSQL 9.x
Infrastructure	Microsoft.EntityFrameworkCore.Design 9.x
Api	Microsoft.AspNetCore.Authentication.JwtBearer 9.x
Api	FluentValidation.AspNetCore
Api	Serilog.AspNetCore
Api	BCrypt.Net-Next
Tests	Moq
Tests	FluentAssertions
Tests	Microsoft.AspNetCore.Mvc.Testing
Krok 3 — docker-compose.yml (CLI, po kroku 2)
Plik tworzę w F:\Claude apps\Apka\VetMed\docker-compose.yml:

services:
  postgres:
    image: postgres:16
    container_name: vetmed-db
    environment:
      POSTGRES_DB: vetmed
      POSTGRES_USER: vetmed
      POSTGRES_PASSWORD: vetmed123
    ports:
      - "5432:5432"
    volumes:
      - pgdata:/var/lib/postgresql/data

  pgadmin:
    image: dpage/pgadmin4
    container_name: vetmed-pgadmin
    environment:
      PGADMIN_DEFAULT_EMAIL: admin@vetmed.pl
      PGADMIN_DEFAULT_PASSWORD: admin123
    ports:
      - "5050:80"
    depends_on:
      - postgres

volumes:
  pgdata:
Uruchomienie: docker compose up -d w folderze VetMed\.

Krok 4 — Struktura katalogów (tworzę automatycznie)
VetMed.Shared/
  Models/          ← Pet.cs, Owner.cs, Visit.cs, Doctor.cs
  DTOs/            ← PetDto.cs, CreatePetDto.cs, AuthDto.cs
  Enums/           ← VisitType.cs, VisitStatus.cs
VetMed.Infrastructure/
  Data/            ← AppDbContext.cs
  Configurations/  ← PetConfiguration.cs, VisitConfiguration.cs
  Repositories/    ← IPetRepository.cs, PetRepository.cs
  Migrations/      ← (generowane przez EF)
VetMed.Api/
  Controllers/     ← AuthController.cs, PetsController.cs, VisitsController.cs
  Services/        ← IPetService.cs, PetService.cs
  Middleware/      ← ExceptionMiddleware.cs
  Extensions/      ← ServiceCollectionExtensions.cs
VetMed.Clinic/
  Components/Pages/ ← Home.razor, Visits.razor, Pets.razor, Login.razor
VetMed.App/
  Components/Pages/ ← Home.razor, PetList.razor, BookVisit.razor
  Services/         ← ApiClient.cs, PetApiService.cs
Krok 5 — Pierwsze uruchomienie (weryfikacja)
# Terminal w F:\Claude apps\Apka\VetMed\
docker compose up -d                        # baza działa → http://localhost:5050
dotnet build VetMed.sln                     # wszystkie projekty kompilują się
dotnet run --project VetMed.Api             # → http://localhost:5100/swagger
dotnet run --project VetMed.Clinic          # → http://localhost:5200
MAUI: VS 2022 → ustaw VetMed.App jako Startup Project → wybierz Windows Machine → F5.

Krok 6 — EF Core migrations (po pierwszym buildzie)
dotnet tool install --global dotnet-ef
dotnet ef migrations add InitialCreate \
  --project VetMed.Infrastructure \
  --startup-project VetMed.Api
dotnet ef database update \
  --project VetMed.Infrastructure \
  --startup-project VetMed.Api
Co tworzę automatycznie po zatwierdzeniu
Po zatwierdzeniu planu wykonam:

docker-compose.yml — plik gotowy do docker compose up -d ✅ (już gotowy)
SETUP.md w F:\Claude apps\Apka\VetMed\ — przewodnik krok po kroku dla VS 2022
Katalogi wewnątrz projektów (Models, DTOs, Controllers, etc.)
Szkielet kodu: AppDbContext, modele domenowe, Program.cs dla API z JWT + Swagger, Program.cs dla Clinic
appsettings.json z connection string do PostgreSQL
Kroki VS 2022 (1.1–1.3) i NuGet (Krok 2) wykonujesz samodzielnie — po ich zakończeniu wróć, a ja dodam kod.
```
