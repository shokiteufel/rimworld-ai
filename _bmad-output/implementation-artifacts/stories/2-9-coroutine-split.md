# Story 2.9: Tick-Budgeted-Iterator für Full-Scan (Performance-Split)

**Status:** ready-for-dev
**Epic:** Epic 2 — Map-Analyzer
**Size:** S
**Decisions referenced:** F-PERF-01 (tick-budgeted iterator statt "Coroutine"), Architecture §8 Performance-Budget (500ms Total, 2ms/Tick)

## Story
Als Mod-Entwickler möchte ich den **Full-Map-Scan** in einen `IEnumerator`-basierten **tick-budgeted iterator** packen, der pro Tick ca. 2ms arbeitet, damit auch bei 350×350-Maps oder Modded-Colony mit hoher Tick-Load keine sichtbaren Frame-Drops entstehen.

## Acceptance Criteria
1. `MapAnalyzer.FullScanIterator(Map map) → IEnumerator<ScanStage>` in `Source/Analysis/MapAnalyzer.cs`
2. `ScanStage` enum: `Init, CellDataScan, HazardScan, Defensibility, Scoring, Clustering, Finalize`
3. Yield nach jedem 50-Cell-Chunk oder nach 2ms Stopwatch (whichever first)
4. Runtime-Integration: `BotMapComponent.MapComponentTick` zieht den Iterator pro Tick um 1 Step weiter (nur wenn `ScanInProgress == true`)
5. Cancellation-Support: wenn Map disposed während Scan → Iterator abbrechen, partial-Summary verwerfen
6. Integration-Test: 350×350-Map → Full-Scan dauert ~250 Ticks (~4s in-game bei 60 TPS), aber kein Single-Tick-Spike > 5ms
7. Status-Flag `BotMapComponent.scanInProgress: bool` + `scanProgressPercent: float` für UI
8. **Exception-Wrapper** (HIGH-Fix Round-2-Stability, CC-STORIES-02): `MapComponentTick()`-Iterator-Advance via Story 1.10 `ExceptionWrapper.TickHost(...)`. Bei Exception im Iterator-Step: Iterator abbrechen + `scanInProgress=false` + partial-Summary verwerfen; bei 2 Exceptions/min → `FallbackToOff()`.

## Tasks
- [ ] `MapAnalyzer` refactor: `FullScan` splitten in `FullScanIterator`
- [ ] `ScanStage` enum
- [ ] Stopwatch-basierter Yield-Punkt
- [ ] `MapComponentTick`-Integration in BotMapComponent
- [ ] Cancellation bei Map-Dispose
- [ ] `scanProgressPercent` für optional Progress-UI
- [ ] Integration-Test Perf-Messung 350×350

## Dev Notes
**Architektur-Kontext:** §8 "tick-budgeted iterator" statt "Coroutine" (F-PERF-01-Fix).
**Nehme an, dass:** Stopwatch.ElapsedMilliseconds ist für 2ms-Checks präzise genug (Overhead ~50ns).
**Vorausgesetzt:** Story 2.1 (Basis-Scan), Story 2.6 (Clustering) — iterator orchestriert diese Phases.

## File List
| Pfad | Op |
|---|---|
| `Source/Analysis/MapAnalyzer.cs` | modify (add Iterator) |
| `Source/Data/BotMapComponent.cs` | modify (Tick-Integration, scanInProgress) |

## Testing
- Integration: 350×350-Map → scan ohne Frame-Drops; Stopwatch pro Tick < 5ms
- Cancellation: Map-Dispose während Scan → keine Exception, sauber aborted

## Review-Gate
Code-Review gegen §8 Performance-Budget, F-PERF-01 Terminologie.
