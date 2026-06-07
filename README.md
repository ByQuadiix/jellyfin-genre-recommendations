# Jellyfin Genre Recommendations Plugin

Zeigt **wöchentlich wechselnde Genre-Reihen** direkt auf der Jellyfin-Startseite – VOR der "Weiterschauen"-Sektion.

## Genre-Reihen

| Reihe | Erfasste Genre-Tags |
|---|---|
| Action | Action, Action & Adventure |
| Abenteuer | Adventure, Action & Adventure |
| Comedy | Comedy, Komödie |
| Drama | Drama |
| Fantasy | Fantasy, Sci-Fi & Fantasy |
| Horror | Horror |
| Mystery | Mystery |
| Romance | Romance |
| Thriller | Thriller, Suspense |
| Science Fiction | Science Fiction, Sci-Fi & Fantasy |
| Krimi | Krimi |
| Krieg | War, War & Politics |
| Romance & Comedy | Romance + Comedy + Komödie (kombiniert) |

Die Empfehlungen werden **zufällig** aus deiner Bibliothek ausgewählt und **jeden Montag um 03:00 Uhr** automatisch neu gewürfelt.

## Installation

### Automatisch (empfohlen)
1. Neuestes Release herunterladen → [Releases](../../releases)
2. ZIP in den Jellyfin Plugin-Ordner entpacken:
   - **Linux:** `~/.local/share/jellyfin/plugins/`
   - **Windows:** `%APPDATA%\jellyfin\plugins\`
   - **Docker:** `/config/plugins/`
3. Jellyfin neu starten
4. Plugin unter **Dashboard → Plugins → Genre Recommendations** konfigurieren

### Manuell bauen
```bash
git clone <dieses-repo>
cd Jellyfin.Plugin.GenreRecommendations
dotnet build --configuration Release
```
Die fertige DLL liegt unter `build-output/Jellyfin.Plugin.GenreRecommendations.dll`.

## Konfiguration

Unter **Dashboard → Plugins → Genre Recommendations**:

- **Quell-Bibliothek** – welche Jellyfin-Bibliothek als Quelle genutzt wird
- **Items pro Genre** – wie viele Titel pro Reihe angezeigt werden (Standard: 8)
- **"Jetzt aktualisieren"** – sofort neue Empfehlungen auswählen (ohne auf Montag zu warten)

## Technische Details

| Komponente | Beschreibung |
|---|---|
| **Server Plugin (C#)** | Scheduled Task, API Endpoint, Konfiguration |
| **Web Plugin (JS)** | Injiziert Genre-Reihen in den Startbildschirm |
| **API** | `GET /GenreRecommendations/Sections` – Rückgabe der Reihen |
| **Cache** | JSON-Datei im Jellyfin Data-Verzeichnis |
| **Aktualisierung** | Jeden Montag 03:00 Uhr (konfigurierbar) |

## Build-Status

[![Build & Release](../../actions/workflows/build.yml/badge.svg)](../../actions/workflows/build.yml)

## Neues Release erstellen

```bash
git tag v1.0.1
git push origin v1.0.1
```
GitHub Actions baut dann automatisch und erstellt ein Release mit der ZIP-Datei.
