# =============================================================
#  VetMed — wdrożenie API na Google Cloud Run + Cloud SQL
#  Uruchom z katalogu repo:  .\deploy.ps1
#  Wymaga: gcloud zalogowany na konto z projektem 'apkavet'
# =============================================================

# --- UZUPEŁNIJ TE 3 WARTOŚCI -------------------------------
$INSTANCE = "free-trial-first-project"     # identyfikator z konsoli
$DB_PASS  = "VetMed2026Apka"             # to, które przed chwilą ustawiłeś
$JWT_KEY  = "TkHi2tjor1ktMxZFtbMOAtzjcdBTLvW4hhqIO2E0c93qGGx1"
# -----------------------------------------------------------

# --- Stałe (zwykle bez zmian) ------------------------------
$PROJECT  = "apkavet"
$REGION   = "europe-central2"
$DB_NAME  = "vetmed"
$DB_USER  = "postgres"
$IMAGE    = "$REGION-docker.pkg.dev/$PROJECT/vetmed/api:latest"
$CONN     = "${PROJECT}:${REGION}:${INSTANCE}"
# -----------------------------------------------------------

Write-Host "==> Projekt: $PROJECT | Instancja: $CONN" -ForegroundColor Cyan

# 1. Ustaw projekt i region
gcloud config set project $PROJECT
gcloud config set run/region $REGION

# 2. Włącz wymagane API
gcloud services enable run.googleapis.com sqladmin.googleapis.com `
  artifactregistry.googleapis.com cloudbuild.googleapis.com

# 3. Repozytorium na obrazy (ignoruj błąd, jeśli już istnieje)
gcloud artifacts repositories create vetmed `
  --repository-format=docker --location=$REGION 2>$null

# 4. Baza danych w instancji (ignoruj błąd, jeśli już istnieje)
gcloud sql databases create $DB_NAME --instance=$INSTANCE 2>$null

# 5. Build obrazu przez Cloud Build (używa Dockerfile w korzeniu)
gcloud builds submit --tag $IMAGE .
if (-not $?) { Write-Host "BUILD FAILED" -ForegroundColor Red; exit 1 }

# 6. Connection string + JWT jako zmienne środowiskowe
$connStr = "Host=/cloudsql/$CONN;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASS"
$envVars = "ConnectionStrings__DefaultConnection=$connStr,Jwt__Key=$JWT_KEY"

# 7. Deploy na Cloud Run
gcloud run deploy vetmed-api `
  --image $IMAGE `
  --region $REGION `
  --add-cloudsql-instances $CONN `
  --set-env-vars $envVars `
  --allow-unauthenticated `
  --port 8080

# 8. Pokaż URL
$url = gcloud run services describe vetmed-api --region $REGION --format="value(status.url)"
Write-Host ""
Write-Host "==> API URL: $url" -ForegroundColor Green
Write-Host "    Test:    curl --ssl-no-revoke $url/api/doctors" -ForegroundColor Green
Write-Host "    Wklej ten URL do VetMed.App/MauiProgram.cs (Release apiBase)" -ForegroundColor Yellow
