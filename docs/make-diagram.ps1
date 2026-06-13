Add-Type -AssemblyName System.Drawing

$W = 1700; $H = 1050
$bmp = New-Object System.Drawing.Bitmap($W, $H)
$g = [System.Drawing.Graphics]::FromImage($bmp)
$g.SmoothingMode = [System.Drawing.Drawing2D.SmoothingMode]::AntiAlias
$g.TextRenderingHint = [System.Drawing.Text.TextRenderingHint]::ClearTypeGridFit
$g.Clear([System.Drawing.Color]::White)

$fontTitle  = New-Object System.Drawing.Font("Segoe UI", 15, [System.Drawing.FontStyle]::Bold)
$fontBox    = New-Object System.Drawing.Font("Segoe UI", 13, [System.Drawing.FontStyle]::Bold)
$fontSmall  = New-Object System.Drawing.Font("Segoe UI", 11)
$fontLabel  = New-Object System.Drawing.Font("Segoe UI", 10.5, [System.Drawing.FontStyle]::Italic)

$cBlue   = [System.Drawing.Color]::FromArgb(66, 133, 244)
$cBlueBg = [System.Drawing.Color]::FromArgb(232, 240, 254)
$cGreen  = [System.Drawing.Color]::FromArgb(52, 168, 83)
$cGreenBg= [System.Drawing.Color]::FromArgb(230, 244, 234)
$cYellow = [System.Drawing.Color]::FromArgb(244, 160, 0)
$cYellBg = [System.Drawing.Color]::FromArgb(254, 247, 224)
$cRed    = [System.Drawing.Color]::FromArgb(217, 48, 37)
$cRedBg  = [System.Drawing.Color]::FromArgb(252, 232, 230)
$cGray   = [System.Drawing.Color]::FromArgb(95, 99, 104)
$cFrame  = [System.Drawing.Color]::FromArgb(248, 250, 253)
$black   = [System.Drawing.Brushes]::Black

$sfC = New-Object System.Drawing.StringFormat
$sfC.Alignment = "Center"; $sfC.LineAlignment = "Near"

function Draw-Box($x,$y,$w,$h,$bg,$border,$title,$lines) {
    $brush = New-Object System.Drawing.SolidBrush($bg)
    $pen = New-Object System.Drawing.Pen($border, 3)
    $g.FillRectangle($brush, $x, $y, $w, $h)
    $g.DrawRectangle($pen, $x, $y, $w, $h)
    $g.DrawString($title, $fontBox, $black, [System.Drawing.RectangleF]::new($x+8, $y+12, $w-16, 30), $sfC)
    $yy = $y + 44
    foreach ($ln in $lines) {
        $g.DrawString($ln, $fontSmall, $black, [System.Drawing.RectangleF]::new($x+8, $yy, $w-16, 26), $sfC)
        $yy += 26
    }
}

function Draw-Arrow($x1,$y1,$x2,$y2,$color,$label,$lx,$ly) {
    $pen = New-Object System.Drawing.Pen($color, 3.5)
    $pen.CustomEndCap = New-Object System.Drawing.Drawing2D.AdjustableArrowCap(6, 7)
    $g.DrawLine($pen, $x1, $y1, $x2, $y2)
    if ($label) {
        $br = New-Object System.Drawing.SolidBrush([System.Drawing.Color]::White)
        $sz = $g.MeasureString($label, $fontLabel)
        $g.FillRectangle($br, $lx - $sz.Width/2 - 4, $ly - 2, $sz.Width + 8, $sz.Height + 4)
        $cb = New-Object System.Drawing.SolidBrush($cGray)
        $sfL = New-Object System.Drawing.StringFormat
        $sfL.Alignment = "Center"
        $g.DrawString($label, $fontLabel, $cb, $lx, $ly, $sfL)
    }
}

# === GCP frame ===
$fx = 540; $fy = 70; $fw = 1110; $fh = 920
$fb = New-Object System.Drawing.SolidBrush($cFrame)
$fp = New-Object System.Drawing.Pen($cBlue, 3)
$fp.DashStyle = "Dash"
$g.FillRectangle($fb, $fx, $fy, $fw, $fh)
$g.DrawRectangle($fp, $fx, $fy, $fw, $fh)
$tb = New-Object System.Drawing.SolidBrush($cBlue)
$g.DrawString("Google Cloud Platform  |  projekt: gcp-deploy-492010  |  region: europe-central2 (Warszawa)", $fontTitle, $tb, $fx + 20, $fy + 14)

# === Boxes outside GCP ===
Draw-Box 60 120 380 130 ([System.Drawing.Color]::White) $cGray "VetMed.App (Android)" @(".NET MAUI Blazor Hybrid", "aplikacja mobilna wlasciciela zwierzat", "wersja 1.1.0 (build 2)")

Draw-Box 60 640 380 120 ([System.Drawing.Color]::White) $cGray "GitHub - repozytorium kodu" @("galaz produkcyjna: gcp", "pull requesty, code review")

Draw-Box 60 830 380 100 ([System.Drawing.Color]::White) $cGray "Programista" @("gcloud CLI", "(builds submit / run deploy)")

# === Boxes inside GCP ===
Draw-Box 640 130 430 150 $cBlueBg $cBlue "Cloud Run - usluga vetmed-api" @("kontener Docker (ASP.NET Core 9)", "REST API + JWT, port 8080", "autoskalowanie 0-N instancji")

Draw-Box 1220 130 380 150 $cGreenBg $cGreen "Cloud SQL" @("PostgreSQL 16", "instancja vetmed-db (db-f1-micro)", "baza: vetmed, 10 GB SSD")

Draw-Box 640 400 430 120 $cRedBg $cRed "Secret Manager" @("vetmed-db-password (haslo DB)", "vetmed-jwt-key (klucz JWT)")

Draw-Box 1220 400 380 120 $cYellBg $cYellow "Cloud Logging / Monitoring" @("logi Serilog z kontenera", "metryki uslugi Cloud Run")

Draw-Box 640 640 430 130 $cYellBg $cYellow "Cloud Build" @("cloudbuild.yaml", "docker build wg Dockerfile", "(multi-stage, .NET 9)")

Draw-Box 1220 640 380 130 $cBlueBg $cBlue "Artifact Registry" @("repozytorium: vetmed", "obraz: vetmed/api:latest")

# === Arrows ===
# App -> Cloud Run
Draw-Arrow 440 185 638 195 $cBlue "HTTPS / REST + JWT" 535 160
# Cloud Run -> Cloud SQL
Draw-Arrow 1072 205 1218 205 $cGreen "unix socket /cloudsql/..." 1145 175
# Secret Manager -> Cloud Run
Draw-Arrow 855 398 855 282 $cRed "sekrety jako zmienne srodowiskowe" 855 330
# Cloud Run -> Logging
Draw-Arrow 1072 320 1218 420 $cYellow "logi" 1135 350
# Developer -> Cloud Build
Draw-Arrow 442 870 700 772 $cGray "gcloud builds submit" 560 845
# GitHub -> Developer/Cloud Build (git)
Draw-Arrow 250 762 250 828 $cGray "git pull" 310 788
# Cloud Build -> Artifact Registry
Draw-Arrow 1072 705 1218 705 $cGray "push obrazu" 1145 678
# Artifact Registry -> Cloud Run
Draw-Arrow 1410 638 880 284 $cGray "gcloud run deploy (nowa rewizja)" 1190 480

$bmp.Save("F:\Claude apps\Apka\VetMed\docs\architektura-gcp.png", [System.Drawing.Imaging.ImageFormat]::Png)
$g.Dispose(); $bmp.Dispose()
Write-Output "OK: architektura-gcp.png"
