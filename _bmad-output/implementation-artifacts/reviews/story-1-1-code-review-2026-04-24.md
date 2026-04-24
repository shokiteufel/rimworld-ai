# Story 1.1 Code-Review

**Datum:** 2026-04-24
**Reviewer:** BMAD Code-Review-Subagent
**Scope:** Statische Verifikation (kein Build, SDK fehlt lokal — CI-Build-Verify über Epic-8 Story 8.10)
**Verdict:** APPROVE-WITH-CHANGES

## AC-Coverage

- **AC 1 Mod-Paket-Struktur — FAIL.** `Defs/`, `Patches/`, `Languages/Deutsch/Keyed/`, `Languages/English/Keyed/` existieren nicht im Mod-Root. AC 1 referenziert explizit Architecture §10.2 und listet alle vier als geforderte Struktur. `About/`, `Source/`, `LoadFolders.xml` vorhanden; `Assemblies/` wird beim Build erzeugt (ok).
- **AC 2 About.xml Felder — PASS.** `name`, `packageId=mediainvita.rimworldbot`, `author`, `url`, `description` (DE+EN), `supportedVersions` (1.5, 1.6), `modDependencies` (brrainz.harmony mit displayName + steamWorkshopUrl + downloadUrl). XML valide. `loadAfter` mit allen fünf DLCs + Core + Harmony ist Bonus-Robustheit.
- **AC 3 Preview.png — PASS.** 256×256 RGBA PNG vom User verifiziert.
- **AC 4 csproj kompiliert zu Assemblies/RimWorldBot.dll — PASS (statisch).** `net472`, `OutputPath=..\Assemblies\`, `AppendTargetFrameworkToOutputPath=false`, `AssemblyName=RimWorldBot`, `Krafs.Rimworld.Ref 1.6.*` mit `PrivateAssets="all"`. Nullable enable + Deterministic=true sind gute Defaults.
- **AC 5 Skeleton Load-Probe — PASS.** `[StaticConstructorOnStartup]`, exakter String `[RimWorldBot] skeleton loaded`, `using RimWorld; using Verse;`, Namespace `RimWorldBot.Core`, keine andere Laufzeit-Logik.
- **AC 6 Mod-Liste (theoretische Setup-Korrektheit) — PASS (statisch).** About.xml + Preview.png + LoadFolders.xml erlauben das Erscheinen in der Mod-Liste. Finale Verifikation nur via TC-01-SKELETON manual-test.
- **AC 7 Game-Load crasht nicht — PASS (statisch).** Keine Code-Smells in ModEntry.cs: keine Null-Refs, kein File-IO im Static-Ctor, `Log.Message` ist safe.
- **AC 8 Keine Exceptions im Output-Log — PASS (statisch).** ModEntry wirft nichts. Kein Throw-Potenzial erkennbar.

## Findings

### HIGH

**H-1: Fehlende Pflicht-Verzeichnisse (AC 1 + Architecture §10.2).** `Defs/`, `Patches/`, `Languages/Deutsch/Keyed/`, `Languages/English/Keyed/` müssen existieren. Auch wenn leer, fordert AC 1 die Struktur — Folge-Stories (1.4 Toggle-Button → `Defs/MainButtonDefs.xml`, 1.8 Localization → `Languages/`) setzen sie voraus. RimWorld toleriert leere Def-Ordner ohne Fehler. Fix: Ordner anlegen, optional jeweils eine `.gitkeep` für VCS-Tracking.

### MED

**M-1: `Lib.Harmony` PackageReference in csproj ist Story-Scope-Creep.** Story 1.1 Dev-Notes sagen explizit „Harmony wird als modDependencies deklariert, aber in Story 1.1 **nicht geladen** (das passiert ab 1.2)". Die csproj fügt dennoch `<PackageReference Include="Lib.Harmony" Version="2.3.*" ExcludeAssets="runtime" />` hinzu. Durch `ExcludeAssets="runtime"` wird die DLL nicht mit ausgeliefert, aber der Compile-Time-Ref ist da — das ermöglicht unbemerkt Harmony-Code in ModEntry.cs, was die Skeleton-Abgrenzung zu Story 1.2 aufweicht. Empfehlung: `Lib.Harmony` erst in Story 1.2 (Harmony-Bootstrap) hinzufügen. Falls aus Build-Pragma-Gründen schon jetzt gewünscht: Entscheidung explizit in `_bmad/decisions.md` festhalten.

**M-2: `<Nullable>enable</Nullable>` + net472 + `LangVersion=latest` könnte in CI-Build Warnings produzieren.** Die Kombination ist technisch gültig, aber Krafs.Rimworld.Ref-Typen sind nicht mit Nullable-Annotations versehen. Das wird in Folge-Stories zu CS8xxx-Warnings führen (Log.Message(null!) etc.). Kein Blocker für Skeleton (ModEntry.cs hat keine kritische Stelle), aber Warnung für Story 1.2+. Empfehlung: Entweder Nullable projekt-weit disable und nur per-File per `#nullable enable` opt-in, oder `<NoWarn>`-Filter definieren.

### LOW

**L-1: README.md Zeile 77 „Detail: `_bmad/sprint-status.yaml`" — falscher Pfad.** Korrekt ist `_bmad-output/implementation-artifacts/sprint-status.yaml` (laut projekt-CLAUDE.md Artefakt-Hierarchie Punkt 5). Pfad-Drift → Guardian-Finding-Risiko.

**L-2: csproj ItemGroup `<None Remove="..\About\**" />` etc. sind redundant** unter `Microsoft.NET.Sdk` mit `OutputType=Library` — SDK inkludiert per-default nichts außerhalb des csproj-Ordners. Kein Fehler, aber kosmetisch entfernbar.

**L-3: About.xml Zeile 13 „Status: Sprint 2 — Epic 1 in Entwicklung (Skeleton)" in der user-facing Description.** Das wird im RimWorld-Mod-Browser angezeigt. Für Endnutzer irrelevant und unprofessionell — gehört in README, nicht ins Mod-Manifest. Empfehlung: Vor v1.0.0-release entfernen (vermerken für Story 8.x Polish).

**L-4: LoadFolders.xml nutzt kein `IfModActive`-Attribut** für DLC-conditional-loading. Für reines Skeleton unkritisch (keine DLC-spezifischen Defs), aber Architecture §10.3 nennt `IfModActive` als Lint-Target. Erst ab Story 6.x (DLC-Defs) relevant.

## Unchanged-Risks (für TC-01-SKELETON später)

- **Krafs.Rimworld.Ref 1.6.*** löst möglicherweise ein nicht-aktuellstes Sub-Release ein (z. B. 1.6.4 statt 1.6.5) falls NuGet-Cache stale. CI sollte `--no-cache` oder Lock-File nutzen.
- **ModEntry als `public static class` im Namespace `RimWorldBot.Core`** statt Root-Namespace: RimWorld's `[StaticConstructorOnStartup]`-Scanner triggert das trotzdem (Namespace ist für den Reflektor egal), aber falls die DLL unter dem DLL-Name `RimWorldBot` liegt und jemand später `typeof(ModEntry)` via FullName erwartet, wird `RimWorldBot.Core.ModEntry` statt `RimWorldBot.ModEntry` ausgelesen. In Story 1.2 wird die Klasse eh ersetzt — kein Blocker.
- **`loadAfter` enthält `Ludeon.RimWorld.Odyssey`** — falls dieser packageId im Release-Build des Odyssey-DLC abweicht (z. B. Groß/Kleinschreibung oder ohne `.Odyssey`-Suffix in 1.5), wird `loadAfter` silently ignoriert. Kein Crash, aber Load-Order-Drift. Verify bei DLC-Matrix-Test.
- **`steam://url/CommunityFilePage/2009463077` in modDependencies** funktioniert nur mit installiertem Steam-Client. Für GitHub-only-Nutzer greift `downloadUrl`-Fallback. Akzeptabel.

## Recommendation

**APPROVE-WITH-CHANGES.** H-1 ist ein klarer AC-1-Verstoß und muss vor Status `done` gefixt sein — leere Ordner + `.gitkeep` genügt. M-1 (Lib.Harmony-Scope-Creep) muss entweder entfernt oder per Decision-Log dokumentiert werden. M-2 (Nullable-Warn-Risiko) vor Story 1.2 adressieren. L-1 (README-Pfad) trivialer Ein-Zeiler-Fix.

Nach Fix von H-1 + M-1 + L-1: Story kann auf `review` → nach Re-Review auf `done`. Build-Verify und TC-01-SKELETON-manual-Test bleiben separate Gates (CI über Story 8.10, manual-test Aufgabe der Dev-Session).

AI-4-Singleton-Invariante ist korrekt als „ab Story 1.2 aktiv" dokumentiert (Story-File §Dev-Notes + ModEntry.cs Kommentar) — kein Finding, aber sauberer Hand-off zu Story 1.2.
