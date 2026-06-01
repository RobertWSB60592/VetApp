# Wdrożenie VetMed na Google Cloud (GCP)

Przewodnik krok po kroku: postawienie API + bazy w chmurze, tak aby apka na
telefonie (VetMed.App) miała dostęp do danych przez internet.

## Architektura docelowa

```
📱 VetMed.App (telefon)
      │ HTTPS  →  https://vetmed-api-xxxx.run.app
      ▼
☁️  Cloud Run  ── VetMed.Api (kontener Docker)
      │ Cloud SQL connector (gniazdo unix)
      ▼
🗄️  Cloud SQL for PostgreSQL 16  (baza: vetmed)
      🔑 hasło + klucz JWT → Secret Manager
```

Telefon NIE łączy się bezpośrednio z bazą — gada tylko z API po HTTPS.
Baza nie jest wystawiona publicznie.

---

## 0. Co przygotować zanim zaczniesz

- Konto Google + karta (do aktywacji **$300 kredytu / 90 dni** — bez tego nie ma Cloud SQL)
- Zainstalowany `gcloud` CLI: https://cloud.google.com/sdk/docs/install
- Docker Desktop (do zbudowania obrazu) — lub użyjemy Cloud Build (bez lokalnego Dockera)

Zaloguj się i ustaw projekt:
```bash
gcloud auth login
gcloud projects create vetmed-prod-2026 --name="VetMed"
gcloud config set project vetmed-prod-2026
# Podepnij konto rozliczeniowe (z UI Console: Billing → link project)
```

Włącz potrzebne API:
```bash
gcloud services enable \
  run.googleapis.com \
  sqladmin.googleapis.com \
  secretmanager.googleapis.com \
  artifactregistry.googleapis.com \
  cloudbuild.googleapis.com
```

Ustaw region (Europa, blisko PL):
```bash
gcloud config set run/region europe-central2   # Warszawa
```

---

## 1. Baza danych — Cloud SQL for PostgreSQL

Stwórz najmniejszą (najtańszą) instancję:
```bash
gcloud sql instances create vetmed-db \
  --database-version=POSTGRES_16 \
  --tier=db-f1-micro \
  --region=europe-central2 \
  --storage-size=10GB \
  --storage-auto-increase
```

Utwórz bazę i użytkownika (użyj MOCNEGO hasła, nie `vetmed123`):
```bash
gcloud sql databases create vetmed --instance=vetmed-db

gcloud sql users create vetmed \
  --instance=vetmed-db \
  --password='WSTAW_MOCNE_HASLO'
```

Zapisz nazwę połączenia instancji (przyda się niżej):
```bash
gcloud sql instances describe vetmed-db --format='value(connectionName)'
# np. vetmed-prod-2026:europe-central2:vetmed-db
```

---

## 2. Sekrety — Secret Manager

Hasło do bazy i klucz JWT nie mogą siedzieć w appsettings.json.
```bash
# Hasło DB
printf 'WSTAW_MOCNE_HASLO' | gcloud secrets create vetmed-db-password --data-file=-

# Nowy klucz JWT (min 32 znaki — wygeneruj losowy!)
printf 'WSTAW_LOSOWY_KLUCZ_MIN_32_ZNAKI' | gcloud secrets create vetmed-jwt-key --data-file=-
```

---

## 3. Zmiany w kodzie (PRZED deployem)

### 3a. Dockerfile dla VetMed.Api
Plik `VetMed.Api/Dockerfile` (przygotujemy osobno — multi-stage build .NET 9).

### 3b. Connection string z socketu Cloud SQL
Cloud Run łączy się z Cloud SQL przez gniazdo unix:
`/cloudsql/INSTANCE_CONNECTION_NAME`. Connection string (przez zmienną
środowiskową `ConnectionStrings__DefaultConnection`):
```
Host=/cloudsql/vetmed-prod-2026:europe-central2:vetmed-db;Database=vetmed;Username=vetmed;Password=<z_secret>
```

### 3c. Migracje na produkcji ⚠️
Obecnie `db.Database.MigrateAsync()` odpala się TYLKO w Development
(Program.cs:30). Na produkcji baza zostanie pusta. Opcje:
- (A) Wyciągnąć migrację poza blok `IsDevelopment()` (najprościej), albo
- (B) Odpalić migracje raz ręcznie przez Cloud SQL Auth Proxy.
Zalecane: A — przeniesiemy `MigrateAsync` + `SeedAsync` tak, by działały też na produkcji.

### 3d. Port dla Cloud Run
Cloud Run podaje port przez zmienną `PORT` (zwykle 8080). Linia
`app.Run("http://0.0.0.0:5100")` musi czytać `PORT`:
```csharp
var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";
app.Run($"http://0.0.0.0:{port}");
```

---

## 4. Build obrazu (Cloud Build — bez lokalnego Dockera)

```bash
# Repozytorium na obrazy
gcloud artifacts repositories create vetmed \
  --repository-format=docker \
  --location=europe-central2

# Build + push (uruchom z katalogu solution: Apka/VetMed)
gcloud builds submit \
  --tag europe-central2-docker.pkg.dev/vetmed-prod-2026/vetmed/api:latest \
  --file VetMed.Api/Dockerfile .
```

---

## 5. Deploy API na Cloud Run

```bash
gcloud run deploy vetmed-api \
  --image europe-central2-docker.pkg.dev/vetmed-prod-2026/vetmed/api:latest \
  --add-cloudsql-instances vetmed-prod-2026:europe-central2:vetmed-db \
  --set-env-vars "ConnectionStrings__DefaultConnection=Host=/cloudsql/vetmed-prod-2026:europe-central2:vetmed-db;Database=vetmed;Username=vetmed" \
  --set-secrets "ConnectionStrings__DefaultConnection_PASSWORD=vetmed-db-password:latest,Jwt__Key=vetmed-jwt-key:latest" \
  --allow-unauthenticated \
  --port 8080
```
(Dokładne mapowanie hasła doprecyzujemy — zwykle składamy całe connection
string z hasłem w jednym secret, żeby uniknąć łączenia.)

Po deployu dostaniesz URL, np. `https://vetmed-api-xxxx.run.app`. Sprawdź:
```bash
curl https://vetmed-api-xxxx.run.app/api/doctors
```

---

## 6. Apka na telefon (VetMed.App)

1. W `MauiProgram.cs` podmień `apiBase` na URL z Cloud Run:
   ```csharp
   var apiBase = "https://vetmed-api-xxxx.run.app/";
   ```
2. Zbuduj APK (Android):
   ```bash
   dotnet publish VetMed.App -f net9.0-android -c Release
   ```
3. Przerzuć APK na telefon i zainstaluj (sideload). Apka łączy się z API w chmurze.

---

## 7. Koszty (realne)

| Usługa | Na $300 kredycie | Po kredycie |
|---|---|---|
| Cloud SQL db-f1-micro | $0 | ~$8–10 / mies |
| Cloud Run (mały ruch) | $0 | często $0 (darmowy tier + skala do zera) |
| Secret Manager / Artifact Registry | grosze | grosze |

⚠️ Cloud SQL NIE jest w „Always Free" — po wyczerpaniu kredytu zacznie kosztować.
Jeśli chcesz $0 długoterminowo: baza na Neon/Supabase (darmowy Postgres) + API na Cloud Run.

---

## Checklista zmian w repo (do zrobienia przed deployem)
- [ ] `VetMed.Api/Dockerfile`
- [ ] Czytanie `PORT` w Program.cs
- [ ] Migracje + seed działające na produkcji
- [ ] Connection string + JWT z env/Secret Manager (usunąć hasło z appsettings.json)
- [ ] `apiBase` w MauiProgram.cs → URL Cloud Run
- [ ] `.dockerignore`
