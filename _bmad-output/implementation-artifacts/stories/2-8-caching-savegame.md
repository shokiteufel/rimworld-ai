# Story 2.8: MapAnalysisSummary-Caching im BotMapComponent

**Status:** ready-for-dev
**Epic:** Epic 2 — Map-Analyzer
**Size:** S
**Decisions referenced:** D-14 (Persistence-Scoping: MapAnalysisSummary schlank, keine Per-Cell-Arrays)

## Story
Als Mod-Entwickler möchte ich das **Ergebnis der Map-Analyse (Top-3-Sites + Scores) persistent in `BotMapComponent.analysisSummary` cachen**, damit nach Save-Load der Overlay sofort verfügbar ist ohne erneuten Full-Scan.

## Acceptance Criteria
1. `MapAnalysisSummary` record mit `(ClusterResult[] TopSites, ScoreBreakdown[] Breakdowns, int ScanTick, string MapBiomeDefName)` — **kein Per-Cell-Array** (D-14)
2. Nach `MapAnalyzer.FullScan()` wird `BotMapComponent.analysisSummary = new MapAnalysisSummary(...)` gesetzt
3. `BotMapComponent.ExposeData` persistiert `analysisSummary` via `Scribe_Deep.Look` (Story 1.3 AC 8 bereits vorbereitet)
4. Bei Load: wenn `analysisSummary == null` → Trigger Re-Scan (läuft im nächsten Tick, nicht blocking)
5. Bei Biome-Change (z. B. Caravan → andere Map) → `analysisSummary.MapBiomeDefName` mismatch → Force Re-Scan
6. Schema-Version in `MapAnalysisSummary` für Forward-Compat (= 1 für jetzt)
7. Unit-Test Roundtrip-Serialization

## Tasks
- [ ] `Source/Snapshot/MapAnalysisSummary.cs` record mit `IExposable`
- [ ] Setter in `MapAnalyzer.FullScan`
- [ ] Load-Check in `BotMapComponent.FinalizeInit` (Re-Scan-Trigger)
- [ ] Biome-Mismatch-Detection
- [ ] Unit-Test Scribe-Roundtrip

## Dev Notes
**Architektur-Kontext:** §5 BotMapComponent-Snippet hat `analysisSummary` als Feld. Diese Story liefert die Klasse + Cache-Logik.
**Nehme an, dass:** `map.Biome.defName` ist save-stabil; `scanTick` von `Find.TickManager.TicksGame`.
**Vorausgesetzt:** Story 2.6 (ClusterResult existiert), Story 1.3 (BotMapComponent).

## File List
| Pfad | Op |
|---|---|
| `Source/Snapshot/MapAnalysisSummary.cs` | create |
| `Source/Analysis/MapAnalyzer.cs` | modify (setzt Summary nach Scan) |
| `Source/Data/BotMapComponent.cs` | modify (FinalizeInit Re-Scan-Trigger) |

## Testing
- Unit: Summary Scribe-Roundtrip
- Integration: Save → Load → Summary verfügbar ohne Re-Scan; Biome-Change → Force Re-Scan

## Review-Gate
Code-Review gegen D-14 (keine Per-Cell-Arrays persistiert). Unit-testbar (IExposable).
