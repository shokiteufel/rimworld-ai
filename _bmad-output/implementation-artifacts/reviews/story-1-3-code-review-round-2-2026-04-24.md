# Story 1.3 Code-Review Round 2

**Datum:** 2026-04-24
**Verdict:** APPROVE

## Findings-Status (nach Pass 1)

- **MED-1 (Migrate-Formula + WorldPawns-Lookup): RESOLVED**
  `BotGameComponent.MigrateV1ToV2` (L143–191) iteriert jetzt echt uber `oldDict`, parst den v1-String-Key zu `int thingIDNumber` und sucht den Pawn via `Find.WorldPawns?.AllPawnsAliveOrDead?.FirstOrDefault(p => p != null && p.thingIDNumber == thingIDNumber)`. Null-Safety durchgangig: beide `?.`-Chains plus `p != null`-Filter plus `pawn?.GetUniqueLoadID()` plus `!string.IsNullOrEmpty(newKey)` plus Duplicate-Guard via `!newDict.ContainsKey`. Threshold-Formel ist jetzt `percentBranch = oldCount > 0 && (double)droppedCount / oldCount > 0.10` (L182) — `oldCount` ist korrekt der Original-Dict-Count (Snapshot bei L146 vor Iteration), Divisor-Zero geschutzt. Tautologie aus Round 1 ist weg. Kombi-Logik `droppedCount >= 1 && (percentBranch || countBranch)` entspricht NEW-STAB-05.

- **MED-2 (excludedCells): RESOLVED**
  `BotMapComponent` (L29) deklariert `public HashSet<(int x, int z)> excludedCells = new();` plus private `_excludedCellsFlat` (L30). Scribe-Pattern korrekt gewrapped: Flatten nur im `Saving`-Branch (L46–54), `Scribe_Collections.Look` unbedingt (L55, korrekt — sonst ware Load kaputt), Rehydrate nur in `PostLoadInit` (L57–72) mit defensivem `i + 1 < Count`-Loop-Guard gegen Odd-Count-Corruption, `_excludedCellsFlat = null` nach Rehydrate (L71) um Memory nicht zu halten. `CurrentSchemaVersion = 3` (L16). Catch-Block initialisiert `excludedCells ??=` (L86). Migrate v2→v3 initialisiert leeres HashSet (L97). AC 16 erfullt.

- **LOW-1 (eventQueue Null-Guard): RESOLVED**
  Beide Aufrufe in `LoadedGame` (L214) und `StartedNewGame` (L224) nutzen `eventQueue?.Clear()`.

- **LOW-2 (MigrateV2ToV3 Log): RESOLVED**
  `BotGameComponent.MigrateV2ToV3` (L195–198) loggt expliziten No-Op-Hinweis mit Cross-Ref zu BotMapComponent. Kein silent transition mehr.

- **LOW-3 (Polyfill-Datei): RESOLVED**
  `Source/Core/IsExternalInitPolyfill.cs` existiert als eigene Datei mit sauberem Namespace `System.Runtime.CompilerServices`, `internal`-Scope, Kommentar mit Entfernungs-Bedingung. `Enums.cs` ist bereinigt (nur noch 4 Enums, kein Polyfill).

## Neue Findings (Round 2)

Keine.

Kleiner Bonus-Check: `map?.uniqueID` in Migrate-Log (BotMapComponent L100) ist null-safe. `FinalizeInit`-BuildController-Reentry durch `if (controller == null)`-Guard in LoadedGame/StartedNewGame ist idempotent.

## Recommendation

Freigabe zur Merge. Alle 5 Round-1-Findings sauber adressiert, Build grun (0W/0E per Report), keine Regressionen erkennbar. Story 1.3 kann auf `done` gesetzt werden.
