# Story 1.1: Mod-Projekt anlegen (Skeleton)

**Status:** ready-for-dev
**Epic:** Epic 1 — Mod-Skeleton & Toggle
**Size:** S (Setup, keine Laufzeit-Logik)
**Sprint:** 1
**Decisions referenced:** D-01 (Harmony-C#-Mod), D-13 (Minimal-Harmony-Strategie — H1/H7/H8 raus)

---

## Story

Als **Mod-Entwickler** möchte ich **ein leeres, aber lade-fähiges Mod-Paket mit korrekter RimWorld-Verzeichnisstruktur, About.xml und einer leeren DLL**, damit **in allen Folge-Stories (1.2 Harmony-Bootstrap, 1.3 BotGameComponent, …) auf einem soliden Skeleton-Fundament aufgebaut werden kann, und der User die Mod von Anfang an in der Mod-Liste sehen und aktivieren kann**.

---

## Acceptance Criteria

1. **Mod-Paket-Struktur** entspricht Architecture §10.2 (`About/`, `Assemblies/`, `Defs/`, `Patches/`, `Languages/German (Deutsch)/Keyed/`, `Languages/English/Keyed/`, `LoadFolders.xml`). Folder-Naming "German (Deutsch)" mit Klammern ist Vanilla-RimWorld-1.6-Konvention — siehe D-39 (retroactive Story 1.8 Bug-Fix 2026-04-25).
2. **`About/About.xml`** enthält: `name`, `packageId` (`mediainvita.rimworldbot`), `author`, `url` (GitHub-Repo), `description` (DE + EN), `supportedVersions` (`1.5`, `1.6`), `modDependencies` (Harmony: `brrainz.harmony`)
3. **`About/Preview.png`** existiert als Platzhalter (256×256, wird in Story 8.x gegen finales Artwork ersetzt)
4. **`Source/RimWorldBot.csproj`** kompiliert zu `Assemblies/RimWorldBot.dll` (Target: .NET Framework 4.7.2, RimWorld-Assembly-References via NuGet-Package `Krafs.Rimworld.Ref` oder Local-Refs)
5. **DLL enthält keine Laufzeit-Logik** außer einer `[StaticConstructorOnStartup]`-Probe-Klasse die `Log.Message("[RimWorldBot] skeleton loaded")` ausgibt (verifiziert Assembly-Load)
6. **Mod erscheint in RimWorld-Mod-Liste** (Vanilla + alle DLC-Kombinationen: Royalty, Ideology, Biotech, Anomaly, Odyssey — einzeln und kombiniert)
7. **Aktivierte Mod + Game-Load crasht nicht** (kein Red-Error in Player.log, Log.Message aus AC 5 ist sichtbar)
8. **Keine Exceptions im Output-Log** — `tail -n 200 Player.log` zeigt keine `Exception`/`Error`-Einträge mit Prefix `[RimWorldBot]`

---

## Tasks

- [ ] `Source/RimWorldBot.csproj` anlegen mit Target `net472`, `OutputPath` = `../Assemblies/`, Assembly-References auf `Assembly-CSharp.dll` (RimWorld) + `UnityEngine.CoreModule.dll` via Pfad-Variable oder `Krafs.Rimworld.Ref`-NuGet
- [ ] `Source/Core/ModEntry.cs` anlegen mit `[StaticConstructorOnStartup] public static class ModEntry { static ModEntry() { Log.Message("[RimWorldBot] skeleton loaded"); } }`
- [ ] `About/About.xml` anlegen mit korrekten Feldern (siehe AC 2)
- [ ] `About/Preview.png` Platzhalter erstellen (256×256 PNG, neutrale Farbe, Text „RimWorldBot — WIP")
- [ ] `LoadFolders.xml` anlegen (minimal: `<v1.5><li>/</li></v1.5> <v1.6><li>/</li></v1.6>`)
- [ ] `.gitignore` anlegen für Visual Studio / Rider / `.vs/` / `bin/` / `obj/` — **NICHT** `Assemblies/*.dll` ignorieren (wird in Release-ZIP gebraucht)
- [ ] `README.md` mit Projekt-Overview + Links zu `_bmad-output/planning-artifacts/` (PRD, Architecture, Epics)
- [ ] Build-Probe lokal: `msbuild Source/RimWorldBot.csproj` oder `dotnet build` → `Assemblies/RimWorldBot.dll` existiert
- [ ] Integration-Test TC-01-SKELETON: Mod in RimWorld-Mod-Liste aktivieren → Neue Colony starten → Player.log prüfen (Log.Message aus AC 5 vorhanden, keine Errors)
- [ ] DLC-Matrix-Smoke-Test: Für jede DLC-Kombination (2^5 = 32, aber pragmatisch: Vanilla + All-DLCs + je 1 Single-DLC = 7 Varianten) den Integration-Test wiederholen

---

## Dev Notes

**Architektur-Kontext (v2.3):**
- Diese Story platziert **nur das Skeleton** — keine Harmony-Patches (kommt Story 1.2), kein `BotGameComponent` (Story 1.3), keine Toggle-Logik (Story 1.4+).
- Die `[StaticConstructorOnStartup]`-Probe in AC 5 ist **nicht** die finale `CompatInitializer`-Klasse aus §4.2 — sie dient nur der Load-Verifikation für dieses Skeleton-Release. In Story 1.2 wird diese Klasse durch die echte Harmony-Init ersetzt.

**Nehme an, dass:**
- Krafs.Rimworld.Ref v1.6.* NuGet-Package ist verfügbar und wird als primäre Assembly-Reference genutzt (Alternative: lokale `.dll`-Refs mit Pfad-Variable im `.csproj` — User-Präferenz klären wenn NuGet nicht geht)
- Harmony-Package wird als `modDependencies` deklariert, aber in Story 1.1 **nicht geladen** (das passiert ab 1.2). RimWorld akzeptiert nicht-geladene Harmony-Deklaration ohne Fehler.
- `packageId` `mediainvita.rimworldbot` ist eindeutig (check gegen Steam-Workshop/Ludeon-Forum vor Release).

**Vorausgesetzt:**
- Build-Environment hat .NET Framework 4.7.2 SDK oder .NET 6+ mit net472-Targeting.
- RimWorld-Installation unter `D:\SteamLibrary\steamapps\common\RimWorld\` (für lokalen Integration-Test).

**Architektur-Invariante AI-4** (kein statisches Singleton außer `RimWorldBotMod`-Entry) gilt ab Story 1.2 — die `ModEntry`-Klasse hier ist temporäres Skeleton und wird in 1.2 zu `RimWorldBotMod : Mod` ersetzt.

---

## File List

| Pfad | Operation | Zweck |
|---|---|---|
| `Source/RimWorldBot.csproj` | create | Build-Config |
| `Source/Core/ModEntry.cs` | create | Skeleton Load-Probe (wird in 1.2 überschrieben) |
| `About/About.xml` | create | RimWorld Mod-Manifest |
| `About/Preview.png` | create | Mod-Icon Platzhalter |
| `LoadFolders.xml` | create | Multi-Version-Unterstützung |
| `.gitignore` | create | VCS-Hygiene |
| `README.md` | create | Projekt-Overview + Planning-Artifact-Links |
| `Assemblies/RimWorldBot.dll` | create (via build) | Build-Output |

**Nicht in dieser Story** (verschoben auf 1.2+): Alle Source-Files in `Source/Core/` außer `ModEntry.cs`, alle `Defs/*.xml`, alle `Patches/*.xml`, Localization-Keyed-Files.

---

## Testing

**Unit-Tests:** Keine (Skeleton ohne Logik). Erste Unit-Tests kommen ab Story 1.3 (`BotGameComponent.ExposeData`).

**Integration-Tests:**
- **TC-01-SKELETON:** Mod aktivieren in Vanilla → Colony starten → Player.log prüfen.
- **TC-06-SKELETON-DLC-Matrix** (Auszug aus Epic-8 TC-06):
  - Vanilla only
  - Vanilla + Royalty
  - Vanilla + Ideology
  - Vanilla + Biotech
  - Vanilla + Anomaly
  - Vanilla + Odyssey
  - Vanilla + alle 5 DLCs
  Pro Variante: Load → Log-Message sichtbar, keine Errors.

**Manuelle Checks:**
- Mod erscheint in RimWorld-Mod-Liste mit korrektem Namen + Icon
- About-Description zeigt DE + EN (je nach Spracheinstellung)

---

## Review-Gate (BMAD-Loop)

Nach Dev-Complete:
1. `bmad-code-review` (oder manuelles Review gegen Architecture-§10.2 Paket-Struktur + AC-Coverage)
2. Keine CRIT/HIGH-Findings → Story-Status `review` → `done`
3. Re-Review bei Findings, dann erneut `review`

**Visual-Review:** Nicht relevant für Skeleton (keine UI-Komponenten — kommt ab Story 1.4 ToggleButton).

---

## Aufgelöste Entscheidungen (waren TBDs)

- **TQ-S1-01 resolved:** **NuGet-Package `Krafs.Rimworld.Ref 1.6.*`**. Begründung: Community-Standard, versioniert, CI-freundlich (öffentliches nuget.org — kein Token-Scope nötig), `actions/setup-dotnet` + Standard-Restore reicht für GitHub-Actions. Lokale `.dll`-Refs nur als Fallback dokumentiert (`RimWorldInstallDir`-Path-Variable in `Directory.Build.props`, optional, nicht Default).
- **TQ-S1-02 resolved:** **`packageId = mediainvita.rimworldbot`**. Begründung: folgt Ludeon-Konvention `vendor.modname` (analog `brrainz.harmony`, `Mlie.RocketMan`); vermeidet Namens-Kollisionen; Vendor `mediainvita` matcht Projekt-Eigentümer (info@mediainvita.de).
- **TQ-S1-03 resolved:** **Platzhalter-PNG (256×256, neutraler Hintergrund, Text „RimWorldBot — WIP")**. Begründung: finales Icon ist Scope von Story 8.x (Polish-Phase); Core-Feature-Stories sollen nicht durch Asset-Design blockiert werden.
