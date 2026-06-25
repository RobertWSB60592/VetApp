# =============================================================
#  VetMed — wdrożenie PANELU KLINIKI (Blazor Server) na Cloud Run
#  Uruchom z katalogu repo:  .\deploy-clinic.ps1
# =============================================================

# --- UZUPEŁNIJ (te same co w deploy.ps1) -------------------
$INSTANCE = "free-trial-first-project"     # identyfikator instancji Cloud SQL
$DB_PASS  = "VetMed2026Apka"         # hasło użytkownika 'postgres'
# -----------------------------------------------------------

$PROJECT  = "apkavet"
$REGION   = "europe-central2"
$DB_NAME  = "vetmed"
$DB_USER  = "postgres"
$IMAGE    = "$REGION-docker.pkg.dev/$PROJECT/vetmed/clinic:latest"
$CONN     = "${PROJECT}:${REGION}:${INSTANCE}"

Write-Host "==> Build panelu (Blazor Server)..." -ForegroundColor Cyan
gcloud builds submit --config cloudbuild-clinic.yaml .
if (-not $?) { Write-Host "BUILD FAILED" -ForegroundColor Red; exit 1 }

$connStr = "Host=/cloudsql/$CONN;Database=$DB_NAME;Username=$DB_USER;Password=$DB_PASS"

Write-Host "==> Deploy na Cloud Run..." -ForegroundColor Cyan
gcloud run deploy vetmed-clinic `
  --image $IMAGE `
  --region $REGION `
  --add-cloudsql-instances $CONN `
  --set-env-vars "ConnectionStrings__DefaultConnection=$connStr" `
  --allow-unauthenticated `
  --port 8080 `
  --session-affinity `
  --timeout 3600 `
  --min-instances 1

$url = gcloud run services describe vetmed-clinic --region $REGION --format="value(status.url)"
Write-Host ""
Write-Host "==> Panel kliniki URL: $url" -ForegroundColor Green
Write-Host "    Logowanie: admin@vetmed.pl / admin123" -ForegroundColor Green
