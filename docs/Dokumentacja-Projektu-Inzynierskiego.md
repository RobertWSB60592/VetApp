# VetMed — System wspierania obsługi przychodni weterynaryjnej
## Dokumentacja Projektu Inżynierskiego

> Dokument roboczy. Pola oznaczone `[DO UZUPEŁNIENIA]` wymagają decyzji autora (skład zespołu, promotor, dane uczelni).

---

## 1. Strona tytułowa

- **Nazwa projektu:** VetMed — system wspierania obsługi przychodni weterynaryjnej
- **Uczelnia / kierunek:** `[DO UZUPEŁNIENIA]`
- **Skład zespołu:** `[DO UZUPEŁNIENIA]`
- **Promotor:** `[DO UZUPEŁNIENIA]`
- **Repozytorium:** https://github.com/Darmarus/VetMed
- **Rok akademicki:** `[DO UZUPEŁNIENIA]`

---

## 2. Streszczenie projektu

VetMed to rozwiązanie informatyczne wspierające proces obsługi przychodni
weterynaryjnej z dwóch perspektyw: **właściciela zwierzęcia** (aplikacja mobilna)
oraz **personelu kliniki** (panel webowy). System umożliwia rejestrację zwierząt,
umawianie i zarządzanie wizytami, prowadzenie kartoteki zdrowia (szczepienia,
recepty) oraz przegląd historii leczenia.

Rozwiązanie zbudowano w architekturze warstwowej opartej o platformę .NET 9:
wspólne REST API (ASP.NET Core), panel kliniki (Blazor Server) oraz aplikację
mobilną (MAUI Blazor Hybrid). Dane przechowywane są w bazie PostgreSQL z dostępem
przez Entity Framework Core. Bezpieczeństwo zapewnia uwierzytelnianie JWT (API)
oraz cookie (panel), a integralność danych — walidacja po stronie serwera.

---

## 3. Cel i założenia projektu

### Cel główny
Zaprojektowanie i implementacja systemu, który cyfryzuje komunikację między
właścicielem zwierzęcia a przychodnią weterynaryjną oraz porządkuje dane
medyczne pacjentów (zwierząt).

### Założenia
- Dwa interfejsy użytkownika korzystające ze wspólnego API.
- Trwałe przechowywanie danych w relacyjnej bazie danych.
- Podstawowe zabezpieczenia: uwierzytelnianie, autoryzacja i walidacja danych.
- Możliwość uruchomienia lokalnego (Docker) oraz wdrożenia w chmurze (GCP).

---

## 4. Opis użytkownika i kontekst biznesowy

### Problem
Tradycyjna obsługa przychodni (telefon, papierowa kartoteka) jest podatna na
błędy, utrudnia dostęp do historii leczenia i wymaga ręcznej koordynacji terminów.

### Aktorzy systemu
| Aktor | Opis | Interfejs |
|---|---|---|
| Właściciel zwierzęcia | Rejestruje pupile, umawia wizyty, przegląda historię | Aplikacja mobilna (MAUI) |
| Personel kliniki / lekarz | Zatwierdza i zarządza wizytami, prowadzi grafik i kartotekę | Panel webowy (Blazor) |

---

## 5. Analiza wymagań

### 5.1 Wymagania funkcjonalne
- WF-01 Rejestracja i logowanie użytkownika.
- WF-02 Dodawanie, edycja i archiwizacja zwierząt.
- WF-03 Umawianie wizyty (rodzaj, lekarz, termin).
- WF-04 Zarządzanie statusem wizyty po stronie kliniki (zatwierdzenie/odrzucenie).
- WF-05 Prowadzenie kartoteki zdrowia: szczepienia i recepty.
- WF-06 Przegląd historii leczenia i powiadomień.
- WF-07 Grafik dostępności lekarzy.

### 5.2 Wymagania niefunkcjonalne
- WNF-01 Bezpieczeństwo: JWT + cookie, hashowanie haseł (BCrypt).
- WNF-02 Walidacja danych wejściowych (FluentValidation).
- WNF-03 Wieloplatformowość klienta mobilnego (Android, Windows).
- WNF-04 Konteneryzacja i powtarzalne wdrożenie (Docker, GCP).
- WNF-05 Testowalność (testy jednostkowe, komponentowe bUnit).

---

## 6. Projekt systemu

### 6.1 Architektura rozwiązania
Architektura warstwowa, współdzielone modele/DTO i jedno API obsługujące oba
klienty:

- `VetMed.Shared` — modele domenowe, DTO (record), enumy.
- `VetMed.Api` — REST API (kontrolery, serwisy, walidatory, middleware).
- `VetMed.Infrastructure` — EF Core, DbContext, konfiguracje, migracje, repozytoria.
- `VetMed.Clinic` — panel kliniki (Blazor Server).
- `VetMed.App` — aplikacja mobilna (MAUI Blazor Hybrid).

Diagram wdrożenia w chmurze: `docs/architektura-gcp.png`.

> `[DO UZUPEŁNIENIA]` Diagramy UML — patrz sekcja 6.2.

### 6.2 Diagramy UML
Źródła diagramów (PlantUML) znajdują się w `docs/uml/`. Renderowanie do PNG:
`java -jar docs/plantuml.jar -tpng docs/uml/*.puml`.

1. **Diagram przypadków użycia** — `docs/uml/diagram-przypadkow-uzycia.puml`
   (aktorzy: Właściciel, Lekarz; przypadki WF-01..WF-09).
2. **Diagram klas** — `docs/uml/diagram-klas.puml`
   (model domenowy: `Owner`, `Pet`, `Visit`, `Doctor`, `DoctorSchedule`,
   `Vaccination`, `Prescription` + enumy — zgodny z `VetMed.Shared`).
3. **Diagram sekwencji** — `docs/uml/diagram-sekwencji-umow-wizyte.puml`
   (przepływ „Umów wizytę", zgodny z `VisitsController` / `VisitService`).

#### Szkic modelu danych (na podstawie kodu)
```
Owner 1───* Pet 1───* Visit *───1 Doctor 1───* DoctorSchedule
                 │
                 ├──* Vaccination
                 └──* Prescription
```

### 6.3 Projekt interfejsu
- Prototyp wysokiej wierności: `VETMED.html`.
- Styl: dark glassmorphism (`#0D1F1C` tło, `#6FCFB0` mint), tryb jasny/ciemny.
- Screeny ekranów do osadzenia: `[DO UZUPEŁNIENIA]`.

---

## 7. Opis implementacji

### 7.1 Technologie i narzędzia
| Obszar | Technologia |
|---|---|
| Platforma | .NET 9 |
| API | ASP.NET Core Web API |
| Panel web | Blazor Server (`@rendermode InteractiveServer`) |
| Mobile | .NET MAUI Blazor Hybrid (Android, Windows) |
| Baza danych | PostgreSQL 16 |
| ORM | Entity Framework Core 9 (Npgsql) |
| Auth | JWT Bearer + Cookie, BCrypt |
| Walidacja | FluentValidation |
| Testy | xUnit, bUnit, FluentAssertions, Moq |
| Konteneryzacja | Docker, docker-compose |
| Wdrożenie | Google Cloud Platform |

### 7.2 Struktura repozytorium
```
VetMed.Shared/         modele, DTO, enumy
VetMed.Api/            Controllers, Services, Validators, Middleware
VetMed.Infrastructure/ Data, Configurations, Migrations, Repositories
VetMed.Clinic/         Blazor Server (panel kliniki)
VetMed.App/            MAUI Blazor Hybrid (mobile)
VetMed.Tests/          testy jednostkowe i integracyjne
VetMed.Tests.Blazor/   testy komponentów bUnit
docs/                  dokumentacja, diagramy
```

---

## 8. Testowanie

Testy automatyczne znajdują się w projekcie `VetMed.Tests.Blazor` (xUnit + bUnit +
FluentAssertions + Moq). Uruchomienie: `dotnet test`.

### 8.1 Scenariusze testowe
| ID | Scenariusz | Oczekiwany wynik | Test |
|---|---|---|---|
| ST-01 | Zatwierdzenie wizyty oczekującej w panelu kliniki kończy się sukcesem | Wizyta znika z listy oczekujących (lista przeładowana) | `PendingVisitsTests.Approve_WhenServiceSucceeds_ListReloads` |
| ST-02 | Błąd serwera podczas zatwierdzania/odrzucania wizyty | Stan `busy` wraca do `false`, przyciski znów aktywne (brak zawieszenia UI) | `PendingVisitsTests.Approve_WhenServiceThrows_BusyResetToFalse`, `...ConfirmReject_WhenServiceThrows_BusyResetToFalse` |
| ST-03 | Wyznaczanie imienia/inicjału powitania z pełnej nazwy użytkownika | Zwraca pierwsze słowo; dla pustej wartości fallback „Użytkowniku"/„U" bez wyjątku | `HomeFirstNameTests` (zestaw Theory/Fact) |
| ST-04 | Nawigacja dolnym paskiem w aplikacji mobilnej | Aktywna zakładka zmienia się wraz ze zmianą lokalizacji | `BottomNavLocationChangedTests` |
| ST-05 | Lista wszystkich wizyt w panelu kliniki | Komponent poprawnie renderuje wizyty z serwisu | `AllVisitsTests` |

### 8.2 Wyniki testów
Wynik uruchomienia `dotnet test` (`VetMed.Tests.Blazor`, net9.0):

```
Powodzenie!  — niepowodzenie: 0, powodzenie: 22, pominięto: 0, łącznie: 22
```

Wszystkie 22 testy przechodzą. Pokrycie obejmuje logikę komponentów panelu
kliniki (zatwierdzanie/odrzucanie wizyt, obsługa błędów) oraz logikę
prezentacji aplikacji mobilnej (powitanie, nawigacja).

> Uwaga: projekt `VetMed.Tests` (foldery `Unit`, `Integration`) jest obecnie
> pusty — to naturalny kierunek rozbudowy (testy serwisów API i testy
> integracyjne na bazie Testcontainers/PostgreSQL).

---

## 9. Wnioski i kierunki rozwoju

### Wnioski
`[DO UZUPEŁNIENIA]`

### Kierunki rozwoju (z CLAUDE.md — „Otwarte usprawnienia")
- Powiadomienia: akcje inline („Zadzwoń", „Zmień termin").
- Rejestracja wizyty: pełna integracja grafików lekarzy z dostępnością slotów.
- Płatności online.
- Powiadomienia push (przypomnienia o wizytach i szczepieniach).
