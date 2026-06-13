# -*- coding: utf-8 -*-
"""Generuje VetMed-Wdrozenie-GCP.docx (czysty OOXML, bez zewn. bibliotek)."""
import zipfile, os

OUT = os.path.join(os.path.dirname(__file__), "VetMed-Wdrozenie-GCP.docx")
IMG = os.path.join(os.path.dirname(__file__), "architektura-gcp.png")

def esc(t):
    return (t.replace("&", "&amp;").replace("<", "&lt;").replace(">", "&gt;")
             .replace('"', "&quot;"))

def run(t, bold=False, italic=False, sz=None, color=None, font=None):
    rpr = ""
    if font:
        rpr += f'<w:rFonts w:ascii="{font}" w:hAnsi="{font}" w:cs="{font}"/>'
    if bold: rpr += "<w:b/>"
    if italic: rpr += "<w:i/>"
    if color: rpr += f'<w:color w:val="{color}"/>'
    if sz: rpr += f'<w:sz w:val="{sz}"/><w:szCs w:val="{sz}"/>'
    rpr = f"<w:rPr>{rpr}</w:rPr>" if rpr else ""
    return f'<w:r>{rpr}<w:t xml:space="preserve">{esc(t)}</w:t></w:r>'

def p(runs_xml, style=None, jc=None, spacing=None, numid=None, ilvl=0,
      page_break_before=False, shd=None, ind=None, keep_next=False):
    ppr = ""
    if style: ppr += f'<w:pStyle w:val="{style}"/>'
    if keep_next: ppr += "<w:keepNext/>"
    if page_break_before: ppr += "<w:pageBreakBefore/>"
    if numid is not None:
        ppr += f'<w:numPr><w:ilvl w:val="{ilvl}"/><w:numId w:val="{numid}"/></w:numPr>'
    if shd: ppr += f'<w:shd w:val="clear" w:color="auto" w:fill="{shd}"/>'
    if spacing: ppr += spacing
    if ind: ppr += ind
    if jc: ppr += f'<w:jc w:val="{jc}"/>'
    ppr = f"<w:pPr>{ppr}</w:pPr>" if ppr else ""
    return f"<w:p>{ppr}{runs_xml}</w:p>"

def text(t, **kw):           # zwykly akapit wyjustowany
    kw.setdefault("jc", "both")
    return p(run(t), **kw)

def h1(t): return p(run(t), style="Heading1")
def h2(t): return p(run(t), style="Heading2")
def bullet(t, bold_prefix=None):
    if bold_prefix:
        rx = run(bold_prefix, bold=True) + run(t)
    else:
        rx = run(t)
    return p(rx, numid=1, jc="both")
def step(t, numid):
    return p(run(t), numid=numid, jc="both")
def code(lines):
    out = []
    for i, ln in enumerate(lines):
        sp = '<w:spacing w:before="0" w:after="0" w:line="240" w:lineRule="auto"/>'
        if i == len(lines) - 1:
            sp = '<w:spacing w:before="0" w:after="160" w:line="240" w:lineRule="auto"/>'
        out.append(p(run(ln, font="Consolas", sz="18"), shd="F2F2F2", spacing=sp,
                     ind='<w:ind w:left="284"/>'))
    return "".join(out)
def caption(t):
    return p(run(t, italic=True, sz="20"), jc="center",
             spacing='<w:spacing w:before="60" w:after="240"/>')
def empty(n=1):
    return "".join("<w:p/>" for _ in range(n))

BORD = ('<w:tcBorders><w:top w:val="single" w:sz="4" w:color="999999"/>'
        '<w:left w:val="single" w:sz="4" w:color="999999"/>'
        '<w:bottom w:val="single" w:sz="4" w:color="999999"/>'
        '<w:right w:val="single" w:sz="4" w:color="999999"/></w:tcBorders>')

def cell(content_xml, w, header=False):
    shd = '<w:shd w:val="clear" w:color="auto" w:fill="D5E8F0"/>' if header else ""
    return (f'<w:tc><w:tcPr><w:tcW w:w="{w}" w:type="dxa"/>{BORD}{shd}'
            f'<w:tcMar><w:top w:w="60" w:type="dxa"/><w:left w:w="100" w:type="dxa"/>'
            f'<w:bottom w:w="60" w:type="dxa"/><w:right w:w="100" w:type="dxa"/></w:tcMar>'
            f'<w:vAlign w:val="center"/></w:tcPr>{content_xml}</w:tc>')

def table(headers, rows, widths):
    total = sum(widths)
    grid = "".join(f'<w:gridCol w:w="{w}"/>' for w in widths)
    xml = (f'<w:tbl><w:tblPr><w:tblW w:w="{total}" w:type="dxa"/>'
           f'<w:tblLayout w:type="fixed"/></w:tblPr><w:tblGrid>{grid}</w:tblGrid>')
    hr = "".join(cell(p(run(h, bold=True, sz="20")), w, header=True)
                 for h, w in zip(headers, widths))
    xml += f"<w:tr><w:trPr><w:tblHeader/></w:trPr>{hr}</w:tr>"
    for r in rows:
        cs = "".join(cell(p(run(c, sz="20"), jc="left"), w) for c, w in zip(r, widths))
        xml += f"<w:tr>{cs}</w:tr>"
    xml += "</w:tbl>"
    return xml + "<w:p/>"

# ---------- spis tresci (pole TOC + statyczna tresc zapasowa) ----------
def toc():
    entries = [
        "1. Krótki opis aplikacji i jej funkcjonalności",
        "2. Architektura aplikacji w Google Cloud Platform",
        "3. Technologie wykorzystane do wdrożenia aplikacji",
        "4. CI/CD — proces ciągłej integracji i wdrażania",
        "5. Wnioski",
    ]
    x = p(run("Spis treści", bold=True, sz="32"), jc="left",
          spacing='<w:spacing w:before="0" w:after="240"/>')
    x += ('<w:p><w:r><w:fldChar w:fldCharType="begin" w:dirty="true"/></w:r>'
          '<w:r><w:instrText xml:space="preserve"> TOC \\o &quot;1-2&quot; \\h \\z \\u </w:instrText></w:r>'
          '<w:r><w:fldChar w:fldCharType="separate"/></w:r>'
          + run(entries[0]) + "</w:p>")
    for e in entries[1:]:
        x += p(run(e))
    x += ('<w:p>' + run("(spis treści aktualizuje się automatycznie po otwarciu dokumentu w programie MS Word)",
                        italic=True, sz="18")
          + '<w:r><w:fldChar w:fldCharType="end"/></w:r></w:p>')
    return x

# ---------- strona tytulowa ----------
def title_page():
    c = ""
    c += p(run("Uniwersytet WSB Merito", bold=True, sz="40"), jc="center",
           spacing='<w:spacing w:before="600" w:after="60"/>')
    c += p(run("Wydział Studiów Inżynierskich", sz="26"), jc="center")
    c += p(run("Kierunek: Informatyka", sz="26"), jc="center")
    c += p(run("[miejsce na logo uczelni — zgodnie ze wzorem WSB]", italic=True, sz="18", color="808080"),
           jc="center", spacing='<w:spacing w:before="240" w:after="600"/>')
    c += p(run("PROJEKT ZALICZENIOWY", sz="28"), jc="center",
           spacing='<w:spacing w:before="600" w:after="360"/>')
    c += p(run("Wdrożenie aplikacji mobilnej VetMed", bold=True, sz="40"), jc="center")
    c += p(run("w oparciu o infrastrukturę chmurową Google Cloud Platform", bold=True, sz="32"),
           jc="center", spacing='<w:spacing w:after="600"/>')
    c += p(run("Przedmiot: [nazwa przedmiotu]", sz="24"), jc="center",
           spacing='<w:spacing w:before="600"/>')
    c += p(run("Prowadzący: [tytuł, imię i nazwisko prowadzącego]", sz="24"), jc="center",
           spacing='<w:spacing w:after="400"/>')
    c += p(run("Autorzy (grupa projektowa):", bold=True, sz="24"), jc="center")
    c += p(run("Robert [nazwisko], nr albumu: [.....]", sz="24"), jc="center")
    c += p(run("[imię i nazwisko 2, nr albumu]", sz="24", color="808080"), jc="center")
    c += p(run("[imię i nazwisko 3, nr albumu]", sz="24", color="808080"), jc="center")
    c += p(run("[imię i nazwisko 4, nr albumu]", sz="24", color="808080"), jc="center")
    c += p(run("[Miasto] 2026", sz="24"), jc="center",
           spacing='<w:spacing w:before="1200"/>')
    c += '<w:p><w:r><w:br w:type="page"/></w:r></w:p>'
    return c

# ---------- obraz (diagram) ----------
def image_para():
    # PNG 1700x1050; szerokosc w dokumencie 6.2"
    cx = int(6.2 * 914400)
    cy = int(6.2 * 914400 * 1050 / 1700)
    return (f'<w:p><w:pPr><w:jc w:val="center"/></w:pPr><w:r><w:drawing>'
            f'<wp:inline distT="0" distB="0" distL="0" distR="0">'
            f'<wp:extent cx="{cx}" cy="{cy}"/>'
            f'<wp:docPr id="1" name="Diagram architektury" descr="Architektura VetMed w GCP"/>'
            f'<a:graphic xmlns:a="http://schemas.openxmlformats.org/drawingml/2006/main">'
            f'<a:graphicData uri="http://schemas.openxmlformats.org/drawingml/2006/picture">'
            f'<pic:pic xmlns:pic="http://schemas.openxmlformats.org/drawingml/2006/picture">'
            f'<pic:nvPicPr><pic:cNvPr id="1" name="architektura-gcp.png"/><pic:cNvPicPr/></pic:nvPicPr>'
            f'<pic:blipFill><a:blip r:embed="rIdImg1"/><a:stretch><a:fillRect/></a:stretch></pic:blipFill>'
            f'<pic:spPr><a:xfrm><a:off x="0" y="0"/><a:ext cx="{cx}" cy="{cy}"/></a:xfrm>'
            f'<a:prstGeom prst="rect"><a:avLst/></a:prstGeom></pic:spPr>'
            f'</pic:pic></a:graphicData></a:graphic></wp:inline></w:drawing></w:r></w:p>')

# =====================================================================
# TRESC DOKUMENTU
# =====================================================================
body = title_page()
body += toc()
body += '<w:p><w:r><w:br w:type="page"/></w:r></w:p>'

# ---------- 1 ----------
body += h1("1. Krótki opis aplikacji i jej funkcjonalności")
body += text("VetMed to system informatyczny wspierający obsługę przychodni weterynaryjnej od strony "
             "właściciela zwierzęcia. Sercem rozwiązania jest aplikacja mobilna na system Android, która "
             "komunikuje się przez internet z interfejsem REST API wdrożonym w chmurze Google Cloud "
             "Platform. Dane przechowywane są w zarządzanej bazie PostgreSQL (Cloud SQL). Projekt został "
             "zrealizowany w całości w ekosystemie .NET 9 i języku C#.")
body += h2("1.1. Struktura rozwiązania")
body += table(
    ["Moduł", "Technologia", "Rola"],
    [
        ["VetMed.App", ".NET MAUI Blazor Hybrid (Android)", "aplikacja mobilna dla właściciela zwierząt — wersja 1.1.0 (build 2)"],
        ["VetMed.Api", "ASP.NET Core 9 (REST + JWT)", "interfejs API wdrażany w chmurze (Cloud Run)"],
        ["VetMed.Clinic", "Blazor Server", "panel administracyjny kliniki (uruchamiany lokalnie, planowany do wdrożenia)"],
        ["VetMed.Infrastructure", "EF Core 9 + Npgsql", "warstwa dostępu do danych, migracje, seeder"],
        ["VetMed.Shared", "biblioteka klas (.NET 9)", "wspólne modele i DTO (rekordy)"],
        ["VetMed.Tests", "xUnit", "testy jednostkowe"],
    ],
    [2200, 3000, 3826])
body += h2("1.2. Najważniejsze funkcjonalności aplikacji mobilnej")
body += bullet("rejestracja i logowanie użytkownika (uwierzytelnianie tokenem JWT),")
body += bullet("zarządzanie zwierzętami: dodawanie ze zdjęciem, edycja profilu, archiwizacja, automatyczne wyliczanie wieku z daty urodzenia,")
body += bullet("karta pacjenta z zakładkami: opis, lista wizyt, szczepienia,")
body += bullet("rezerwacja wizyty w krokach: wybór zwierzęcia, rodzaju wizyty (szczepienie / kontrola / zabieg / nagły przypadek), lekarza, daty w kalendarzu i wolnej godziny (zajęte sloty są blokowane),")
body += bullet("zmiana terminu i odwoływanie zarezerwowanych wizyt,")
body += bullet("historia leczenia w formie osi czasu z filtrowaniem po zwierzęciu,")
body += bullet("powiadomienia pogrupowane czasowo (dziś / ten tydzień / starsze),")
body += bullet("wyszukiwarka (zwierzęta, historia wizyt), profil użytkownika, ustawienia z trybem ciemnym i jasnym,")
body += bullet("informacje o przychodni z nawigacją do Map Google i natywnym połączeniem telefonicznym.")

# ---------- 2 ----------
body += h1("2. Architektura aplikacji w Google Cloud Platform")
body += text("Architektura opiera się na usługach zarządzanych (serverless i PaaS), dzięki czemu projekt nie "
             "wymaga utrzymywania własnych maszyn wirtualnych. Wszystkie zasoby chmurowe znajdują się w "
             "projekcie gcp-deploy-492010, w regionie europe-central2 (Warszawa) — najbliższym geograficznie "
             "użytkownikom aplikacji, co minimalizuje opóźnienia i zapewnia przetwarzanie danych na terenie "
             "Unii Europejskiej.")
body += h2("2.1. Diagram architektury")
body += text("Poniższy diagram (przygotowany w narzędziu draw.io — edytowalny plik "
             "VetMed-Architektura-GCP.drawio znajduje się w repozytorium projektu) przedstawia komponenty "
             "systemu oraz przepływy danych i procesu wdrożenia.")
body += image_para()
body += caption("Rys. 1. Architektura systemu VetMed w Google Cloud Platform (opracowanie własne, draw.io)")
body += h2("2.2. Komponenty infrastruktury")
body += table(
    ["Usługa GCP", "Konfiguracja w projekcie", "Zadanie"],
    [
        ["Cloud Run", "usługa vetmed-api, kontener Docker, port 8080, autoskalowanie 0–N instancji", "hostowanie REST API; publiczny adres HTTPS dla aplikacji mobilnej"],
        ["Cloud SQL", "PostgreSQL 16, instancja vetmed-db (db-f1-micro, 10 GB SSD), baza vetmed", "zarządzana baza danych; brak publicznego dostępu z internetu"],
        ["Artifact Registry", "repozytorium vetmed, obraz europe-central2-docker.pkg.dev/.../vetmed/api:latest", "przechowywanie i wersjonowanie obrazów Docker"],
        ["Cloud Build", "plik cloudbuild.yaml w repozytorium", "budowanie obrazu Docker w chmurze (bez lokalnego Dockera)"],
        ["Secret Manager", "sekrety: vetmed-db-password, vetmed-jwt-key", "bezpieczne przechowywanie hasła do bazy i klucza JWT"],
        ["Cloud Logging", "logi Serilog ze standardowego wyjścia kontenera", "centralne logowanie i diagnostyka usługi"],
        ["IAM", "konto usługi Cloud Run z rolami Cloud SQL Client i Secret Manager Secret Accessor", "kontrola dostępu zgodna z zasadą najmniejszych uprawnień"],
    ],
    [1800, 3700, 3526])
body += h2("2.3. Przepływ żądania i bezpieczeństwo")
body += text("Aplikacja mobilna komunikuje się wyłącznie z API poprzez HTTPS — telefon nigdy nie łączy się "
             "bezpośrednio z bazą danych. Cloud Run łączy się z Cloud SQL przez dedykowany łącznik (gniazdo "
             "unix /cloudsql/<nazwa instancji>), więc baza nie posiada publicznego adresu IP. Hasło do bazy "
             "oraz klucz podpisujący tokeny JWT nie znajdują się w repozytorium kodu (pola w appsettings.json "
             "są puste) — są wstrzykiwane do kontenera jako zmienne środowiskowe z Secret Manager "
             "(ConnectionStrings__DefaultConnection, Jwt__Key). Dostęp użytkownika do danych chroniony jest "
             "tokenem JWT o ważności 120 minut.")
body += h2("2.4. Koszty infrastruktury")
body += table(
    ["Usługa", "W okresie kredytu 300 USD", "Po wyczerpaniu kredytu"],
    [
        ["Cloud SQL (db-f1-micro)", "0 USD", "ok. 8–10 USD / mies."],
        ["Cloud Run (mały ruch)", "0 USD", "zwykle 0 USD (darmowy limit + skalowanie do zera)"],
        ["Artifact Registry, Secret Manager, Cloud Build", "marginalne", "marginalne (groszowe kwoty)"],
    ],
    [3026, 2800, 3200])
body += text("Projekt świadomie wykorzystuje najtańsze warianty usług — wystarczające na potrzeby "
             "prototypu i obrony projektu, a jednocześnie identyczne architektonicznie z konfiguracją "
             "produkcyjną, którą można skalować bez zmian w kodzie.")

# ---------- 3 ----------
body += h1("3. Technologie wykorzystane do wdrożenia aplikacji")
body += h2("3.1. Konteneryzacja — Docker")
body += text("API jest pakowane do obrazu Docker zdefiniowanego w pliku Dockerfile w trybie multi-stage: "
             "etap build używa obrazu mcr.microsoft.com/dotnet/sdk:9.0 (przywrócenie pakietów NuGet i "
             "publikacja w konfiguracji Release), a etap finalny — lekkiego obrazu uruchomieniowego "
             "mcr.microsoft.com/dotnet/aspnet:9.0. Dzięki temu finalny obraz nie zawiera SDK ani kodu "
             "źródłowego. Kontener nasłuchuje na porcie 8080, a numer portu odczytywany jest ze zmiennej "
             "środowiskowej PORT przekazywanej przez Cloud Run:")
body += code(['var port = Environment.GetEnvironmentVariable("PORT") ?? "5100";',
              'app.Run($"http://0.0.0.0:{port}");'])
body += h2("3.2. Baza danych i migracje")
body += text("Warstwa danych korzysta z Entity Framework Core 9 ze sterownikiem Npgsql. Schemat bazy "
             "wersjonowany jest migracjami EF Core, które wykonują się automatycznie przy starcie aplikacji "
             "(db.Database.MigrateAsync()), po czym uruchamiany jest seeder wypełniający bazę danymi "
             "startowymi (m.in. lekarze i rodzaje wizyt). Dzięki temu świeżo wdrożona rewizja samodzielnie "
             "aktualizuje schemat produkcyjnej bazy — nie jest potrzebny ręczny krok migracji.")
body += h2("3.3. Konfiguracja i sekrety")
body += text("Aplikacja realizuje zasadę konfiguracji przez środowisko (podejście 12-factor): parametry "
             "połączenia z bazą i ustawienia JWT pochodzą ze zmiennych środowiskowych ustawianych podczas "
             "wdrożenia usługi Cloud Run, a wartości wrażliwe — z Secret Manager. Lokalna konfiguracja "
             "deweloperska (appsettings.json) nie zawiera żadnych haseł.")
body += h2("3.4. Pozostałe narzędzia")
body += bullet("gcloud CLI — tworzenie zasobów, budowanie obrazów (gcloud builds submit) i wdrażanie (gcloud run deploy),", )
body += bullet("docker-compose — lokalna baza PostgreSQL 16 do pracy deweloperskiej,")
body += bullet("Serilog — strukturalne logowanie do konsoli, automatycznie zbierane przez Cloud Logging,")
body += bullet("Scalar / OpenAPI — interaktywna dokumentacja API w środowisku deweloperskim,")
body += bullet("dotnet publish dla platformy net9.0-android — budowanie pakietu APK aplikacji mobilnej.")

# ---------- 4 ----------
body += h1("4. CI/CD — proces ciągłej integracji i wdrażania")
body += h2("4.1. Narzędzia w pipeline")
body += table(
    ["Narzędzie", "Rola w procesie"],
    [
        ["Git + GitHub", "kontrola wersji; gałęzie funkcyjne (feature/*), pull requesty z przeglądem kodu, gałąź wdrożeniowa gcp"],
        ["Cloud Build", "budowanie obrazu Docker w chmurze według pliku cloudbuild.yaml"],
        ["Artifact Registry", "rejestr obrazów — przechowuje kolejne wersje obrazu api"],
        ["Cloud Run (rewizje)", "każde wdrożenie tworzy nową, niemodyfikowalną rewizję usługi; możliwy natychmiastowy rollback do poprzedniej"],
        ["gcloud CLI", "uruchamianie kroków pipeline (builds submit, run deploy)"],
    ],
    [2600, 6426])
body += h2("4.2. Przebieg wdrożenia API")
body += step("Programista rozwija funkcjonalność na gałęzi feature/* i otwiera pull request; po przeglądzie kodu zmiany trafiają do gałęzi wdrożeniowej gcp.", 2)
body += step("Polecenie gcloud builds submit wysyła źródła do Cloud Build, który wykonuje docker build zgodnie z Dockerfile (multi-stage, .NET 9).", 2)
body += step("Zbudowany obraz jest publikowany w Artifact Registry jako europe-central2-docker.pkg.dev/gcp-deploy-492010/vetmed/api:latest.", 2)
body += step("Polecenie gcloud run deploy vetmed-api tworzy nową rewizję usługi Cloud Run — z podpiętą instancją Cloud SQL oraz sekretami z Secret Manager jako zmiennymi środowiskowymi.", 2)
body += step("Cloud Run przełącza ruch na nową rewizję; w razie problemów możliwy jest natychmiastowy powrót (rollback) do rewizji poprzedniej.", 2)
body += step("Przy starcie kontenera EF Core wykonuje migracje bazy danych i seeding — wdrożenie nie wymaga żadnych ręcznych kroków po stronie bazy.", 2)
body += text("Polecenia używane w procesie:")
body += code(["gcloud builds submit  # build obrazu wg cloudbuild.yaml",
              "gcloud run deploy vetmed-api \\",
              "  --image europe-central2-docker.pkg.dev/gcp-deploy-492010/vetmed/api:latest \\",
              "  --add-cloudsql-instances <projekt>:europe-central2:vetmed-db \\",
              "  --set-secrets Jwt__Key=vetmed-jwt-key:latest ...",])
body += h2("4.3. Wdrożenie aplikacji mobilnej")
body += step("Zmiana adresu API nie jest potrzebna — wersja Release ma wkompilowany produkcyjny adres Cloud Run, a wersja Debug łączy się z lokalnym API (emulator Android: 10.0.2.2).", 3)
body += step("Budowanie pakietu: dotnet publish VetMed.App -f net9.0-android -c Release.", 3)
body += step("Dystrybucja pakietu APK na urządzenia testowe (sideload); docelowo publikacja w Google Play.", 3)
body += h2("4.4. Środowiska i częstotliwość wydań")
body += table(
    ["Środowisko", "Infrastruktura", "Przeznaczenie i częstotliwość"],
    [
        ["Development (lokalne)", "PostgreSQL 16 w Dockerze (docker-compose), API na localhost:5100, emulator Androida", "codzienna praca deweloperska; każda zmiana kodu"],
        ["Produkcja (GCP)", "Cloud Run + Cloud SQL + Secret Manager, region europe-central2", "wydania API po zakończeniu pakietu funkcji — średnio 1–2 razy w tygodniu"],
        ["Aplikacja mobilna", "APK budowany lokalnie, wersjonowanie semantyczne (obecnie 1.1.0, build 2)", "wydania rzadsze, skorelowane ze stabilnymi wersjami API"],
    ],
    [2200, 3500, 3326])
body += text("Dzięki rozdzieleniu API i klienta mobilnego wydania serwera mogą odbywać się często i "
             "bez udziału użytkowników — usługa Cloud Run podmienia rewizję bez przerwy w działaniu, "
             "a aplikacja na telefonie nadal korzysta z tego samego, stabilnego adresu HTTPS.")

# ---------- 5 ----------
body += h1("5. Wnioski")
body += h2("5.1. Uzasadnienie założeń projektowych")
body += text("Przyjęliśmy klasyczny trójwarstwowy podział: aplikacja mobilna nie ma żadnego dostępu do bazy — "
             "całość logiki i autoryzacji przechodzi przez API. Upraszcza to bezpieczeństwo (jeden punkt "
             "wejścia, JWT) i pozwala w przyszłości podłączyć kolejnych klientów (panel kliniki) bez zmian w "
             "infrastrukturze. Region europe-central2 wybraliśmy ze względu na minimalne opóźnienia dla "
             "użytkowników w Polsce i przetwarzanie danych w UE. Trzecim założeniem była minimalizacja "
             "kosztów: usługi serverless skalujące się do zera oraz najmniejsza instancja bazy w pełni "
             "pokrywają potrzeby prototypu w ramach kredytu 300 USD dla nowych kont GCP.")
body += h2("5.2. Uzasadnienie wyboru technologii")
body += bullet("jeden język w całym projekcie — C#/.NET 9 dla aplikacji mobilnej, API i warstwy danych ogranicza koszt utrzymania i pozwala współdzielić modele (VetMed.Shared) między klientem a serwerem,")
body += bullet(".NET MAUI Blazor Hybrid — komponenty UI pisane raz działają na Androidzie i Windows; zespół wykorzystuje umiejętności webowe (Blazor) do budowy aplikacji natywnej,")
body += bullet("Cloud Run zamiast maszyn wirtualnych (Compute Engine) czy Kubernetes (GKE) — zero administracji serwerami, płatność wyłącznie za faktyczne żądania, automatyczny HTTPS i skalowanie; przy ruchu prototypu koszt bliski zeru,")
body += bullet("Cloud SQL zamiast własnego PostgreSQL na VM — automatyczne kopie zapasowe, aktualizacje i łącznik eliminujący publiczną ekspozycję bazy,")
body += bullet("Cloud Build + Artifact Registry — budowanie obrazów w chmurze bez wymogu posiadania Dockera na maszynie deweloperskiej, spójne z resztą ekosystemu GCP,")
body += bullet("Secret Manager — sekrety poza repozytorium kodu, z kontrolą dostępu IAM i wersjonowaniem.")
body += h2("5.3. Czego nauczyliśmy się podczas projektu")
body += bullet("konteneryzacji aplikacji .NET (multi-stage build, minimalizacja obrazu, kontrakt portu przez zmienną PORT),")
body += bullet("praktycznej pracy z usługami GCP: Cloud Run, Cloud SQL, Cloud Build, Artifact Registry, Secret Manager i IAM,")
body += bullet("bezpiecznego łączenia usług (łącznik Cloud SQL przez gniazdo unix, role konta usługi, sekrety jako zmienne środowiskowe),")
body += bullet("konfiguracji aplikacji zgodnie z podejściem 12-factor i automatyzacji migracji bazy przy wdrożeniu,")
body += bullet("modelu rewizji Cloud Run — wdrożeń bez przerwy w działaniu i szybkiego rollbacku,")
body += bullet("szacowania i kontroli kosztów chmury (darmowe limity, skalowanie do zera, dobór rozmiaru instancji bazy).")
body += h2("5.4. Możliwości rozwoju i poprawa efektywności wdrażania")
body += text("Przy większych nakładach i większej grupie odbiorców wprowadzilibyśmy następujące zmiany:")
body += bullet("pełna automatyzacja pipeline — trigger Cloud Build podpięty do GitHub: każdy merge do gałęzi gcp automatycznie buduje obraz, uruchamia testy (xUnit, testy integracyjne z Testcontainers) i wdraża nową rewizję bez ręcznych poleceń gcloud,")
body += bullet("środowisko staging — druga usługa Cloud Run i osobna baza do testów akceptacyjnych przed wydaniem produkcyjnym,")
body += bullet("infrastruktura jako kod (Terraform) — odtwarzalność całego środowiska z repozytorium i przeglądy zmian infrastruktury w pull requestach,")
body += bullet("wdrożenia kanarkowe — stopniowe przełączanie ruchu między rewizjami Cloud Run (np. 10% / 90%) ograniczające ryzyko regresji,")
body += bullet("monitoring i alerty — Cloud Monitoring z testami dostępności (uptime checks) i powiadomieniami o błędach; budżety i alerty kosztowe,")
body += bullet("skalowanie bazy — większa instancja Cloud SQL z trybem wysokiej dostępności i replikami do odczytu; cache Memorystore (Redis) dla najczęstszych zapytań,")
body += bullet("własna domena z Cloud Armor (WAF, limity żądań) przed publicznym API,")
body += bullet("dystrybucja mobilna przez Google Play — podpisany pakiet AAB budowany w pipeline, kanały testowe (internal/closed testing) i aktualizacje OTA,")
body += bullet("wdrożenie panelu kliniki (VetMed.Clinic) jako drugiej usługi Cloud Run korzystającej z tego samego API i bazy.")
body += h2("5.5. Plany dalszego rozwoju aplikacji")
body += bullet("publikacja aplikacji w sklepie Google Play oraz wydanie wersji na Windows,")
body += bullet("powiadomienia push (Firebase Cloud Messaging) o zbliżających się wizytach i szczepieniach,")
body += bullet("płatności online za wizyty oraz dokumenty PDF (wyniki badań, zalecenia) przechowywane w Cloud Storage,")
body += bullet("uruchomienie panelu kliniki w chmurze i synchronizacja kalendarzy lekarzy w czasie rzeczywistym,")
body += bullet("rozszerzenie o konta wielu przychodni (model multi-tenant) po zdobyciu pierwszych użytkowników.")

# ---------- sekcja / dokument ----------
SECT = ('<w:sectPr><w:footerReference w:type="default" r:id="rIdFtr1"/>'
        '<w:footerReference w:type="first" r:id="rIdFtr2"/><w:titlePg/>'
        '<w:pgSz w:w="11906" w:h="16838"/>'
        '<w:pgMar w:top="1440" w:right="1440" w:bottom="1440" w:left="1440" '
        'w:header="708" w:footer="708" w:gutter="0"/>'
        '<w:cols w:space="708"/><w:docGrid w:linePitch="360"/></w:sectPr>')

document = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
            '<w:document xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main" '
            'xmlns:r="http://schemas.openxmlformats.org/officeDocument/2006/relationships" '
            'xmlns:wp="http://schemas.openxmlformats.org/drawingml/2006/wordprocessingDrawing">'
            f'<w:body>{body}{SECT}</w:body></w:document>')

styles = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
'<w:styles xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">'
'<w:docDefaults><w:rPrDefault><w:rPr>'
'<w:rFonts w:ascii="Times New Roman" w:hAnsi="Times New Roman" w:cs="Times New Roman"/>'
'<w:sz w:val="24"/><w:szCs w:val="24"/><w:lang w:val="pl-PL"/></w:rPr></w:rPrDefault>'
'<w:pPrDefault><w:pPr><w:spacing w:after="120" w:line="312" w:lineRule="auto"/></w:pPr></w:pPrDefault>'
'</w:docDefaults>'
'<w:style w:type="paragraph" w:default="1" w:styleId="Normal"><w:name w:val="Normal"/></w:style>'
'<w:style w:type="paragraph" w:styleId="Heading1"><w:name w:val="heading 1"/>'
'<w:basedOn w:val="Normal"/><w:next w:val="Normal"/><w:qFormat/>'
'<w:pPr><w:keepNext/><w:pageBreakBefore/><w:spacing w:before="240" w:after="240"/><w:outlineLvl w:val="0"/></w:pPr>'
'<w:rPr><w:b/><w:sz w:val="32"/><w:szCs w:val="32"/></w:rPr></w:style>'
'<w:style w:type="paragraph" w:styleId="Heading2"><w:name w:val="heading 2"/>'
'<w:basedOn w:val="Normal"/><w:next w:val="Normal"/><w:qFormat/>'
'<w:pPr><w:keepNext/><w:spacing w:before="240" w:after="160"/><w:outlineLvl w:val="1"/></w:pPr>'
'<w:rPr><w:b/><w:sz w:val="26"/><w:szCs w:val="26"/></w:rPr></w:style>'
'<w:style w:type="character" w:styleId="Hyperlink"><w:name w:val="Hyperlink"/>'
'<w:rPr><w:color w:val="0563C1"/><w:u w:val="single"/></w:rPr></w:style>'
'</w:styles>')

numbering = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
'<w:numbering xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">'
'<w:abstractNum w:abstractNumId="0"><w:multiLevelType w:val="singleLevel"/>'
'<w:lvl w:ilvl="0"><w:start w:val="1"/><w:numFmt w:val="bullet"/><w:lvlText w:val="&#8226;"/>'
'<w:lvlJc w:val="left"/><w:pPr><w:ind w:left="720" w:hanging="360"/></w:pPr>'
'<w:rPr><w:rFonts w:ascii="Symbol" w:hAnsi="Symbol" w:hint="default"/></w:rPr></w:lvl></w:abstractNum>'
'<w:abstractNum w:abstractNumId="1"><w:multiLevelType w:val="singleLevel"/>'
'<w:lvl w:ilvl="0"><w:start w:val="1"/><w:numFmt w:val="decimal"/><w:lvlText w:val="%1."/>'
'<w:lvlJc w:val="left"/><w:pPr><w:ind w:left="720" w:hanging="360"/></w:pPr></w:lvl></w:abstractNum>'
'<w:abstractNum w:abstractNumId="2"><w:multiLevelType w:val="singleLevel"/>'
'<w:lvl w:ilvl="0"><w:start w:val="1"/><w:numFmt w:val="decimal"/><w:lvlText w:val="%1."/>'
'<w:lvlJc w:val="left"/><w:pPr><w:ind w:left="720" w:hanging="360"/></w:pPr></w:lvl></w:abstractNum>'
'<w:num w:numId="1"><w:abstractNumId w:val="0"/></w:num>'
'<w:num w:numId="2"><w:abstractNumId w:val="1"/></w:num>'
'<w:num w:numId="3"><w:abstractNumId w:val="2"/></w:num>'
'</w:numbering>')

settings = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
'<w:settings xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">'
'<w:updateFields w:val="true"/></w:settings>')

footer1 = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
'<w:ftr xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main">'
'<w:p><w:pPr><w:jc w:val="center"/></w:pPr>'
'<w:r><w:rPr><w:sz w:val="20"/></w:rPr><w:fldChar w:fldCharType="begin"/></w:r>'
'<w:r><w:rPr><w:sz w:val="20"/></w:rPr><w:instrText xml:space="preserve"> PAGE </w:instrText></w:r>'
'<w:r><w:rPr><w:sz w:val="20"/></w:rPr><w:fldChar w:fldCharType="end"/></w:r></w:p></w:ftr>')

footer2 = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
'<w:ftr xmlns:w="http://schemas.openxmlformats.org/wordprocessingml/2006/main"><w:p/></w:ftr>')

content_types = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
'<Types xmlns="http://schemas.openxmlformats.org/package/2006/content-types">'
'<Default Extension="rels" ContentType="application/vnd.openxmlformats-package.relationships+xml"/>'
'<Default Extension="xml" ContentType="application/xml"/>'
'<Default Extension="png" ContentType="image/png"/>'
'<Override PartName="/word/document.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.document.main+xml"/>'
'<Override PartName="/word/styles.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.styles+xml"/>'
'<Override PartName="/word/numbering.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.numbering+xml"/>'
'<Override PartName="/word/settings.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.settings+xml"/>'
'<Override PartName="/word/footer1.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml"/>'
'<Override PartName="/word/footer2.xml" ContentType="application/vnd.openxmlformats-officedocument.wordprocessingml.footer+xml"/>'
'<Override PartName="/docProps/core.xml" ContentType="application/vnd.openxmlformats-package.core-properties+xml"/>'
'<Override PartName="/docProps/app.xml" ContentType="application/vnd.openxmlformats-officedocument.extended-properties+xml"/>'
'</Types>')

rels = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
'<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">'
'<Relationship Id="rId1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument" Target="word/document.xml"/>'
'<Relationship Id="rId2" Type="http://schemas.openxmlformats.org/package/2006/relationships/metadata/core-properties" Target="docProps/core.xml"/>'
'<Relationship Id="rId3" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/extended-properties" Target="docProps/app.xml"/>'
'</Relationships>')

doc_rels = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
'<Relationships xmlns="http://schemas.openxmlformats.org/package/2006/relationships">'
'<Relationship Id="rIdStyles" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/styles" Target="styles.xml"/>'
'<Relationship Id="rIdNum" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/numbering" Target="numbering.xml"/>'
'<Relationship Id="rIdSet" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings" Target="settings.xml"/>'
'<Relationship Id="rIdFtr1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer" Target="footer1.xml"/>'
'<Relationship Id="rIdFtr2" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/footer" Target="footer2.xml"/>'
'<Relationship Id="rIdImg1" Type="http://schemas.openxmlformats.org/officeDocument/2006/relationships/image" Target="media/image1.png"/>'
'</Relationships>')

core = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
'<cp:coreProperties xmlns:cp="http://schemas.openxmlformats.org/package/2006/metadata/core-properties" '
'xmlns:dc="http://purl.org/dc/elements/1.1/">'
'<dc:title>Wdrożenie aplikacji mobilnej VetMed w Google Cloud Platform</dc:title>'
'<dc:creator>Robert</dc:creator><dc:language>pl-PL</dc:language></cp:coreProperties>')

app_xml = ('<?xml version="1.0" encoding="UTF-8" standalone="yes"?>'
'<Properties xmlns="http://schemas.openxmlformats.org/officeDocument/2006/extended-properties">'
'<Application>Microsoft Office Word</Application></Properties>')

with zipfile.ZipFile(OUT, "w", zipfile.ZIP_DEFLATED) as z:
    z.writestr("[Content_Types].xml", content_types)
    z.writestr("_rels/.rels", rels)
    z.writestr("word/document.xml", document)
    z.writestr("word/_rels/document.xml.rels", doc_rels)
    z.writestr("word/styles.xml", styles)
    z.writestr("word/numbering.xml", numbering)
    z.writestr("word/settings.xml", settings)
    z.writestr("word/footer1.xml", footer1)
    z.writestr("word/footer2.xml", footer2)
    z.writestr("docProps/core.xml", core)
    z.writestr("docProps/app.xml", app_xml)
    with open(IMG, "rb") as f:
        z.writestr("word/media/image1.png", f.read())

print("OK:", OUT)
