# Story 2.1: MapCellData-Snapshot + Basis-Scan

**Status:** ready-for-dev
**Epic:** Epic 2 — Map-Analyzer
**Size:** M
**Decisions referenced:** D-14 (Persistence-Scoping), D-23 (Identifier-only in Plan-Records, analog für Snapshots)

## Story
Als Mod-Entwickler möchte ich den grundlegenden Map-Scan-Mechanismus mit `CellSnapshot` (POCO) und `RimWorldSnapshotProvider` implementieren, damit **alle nachfolgenden Analyse-Klassen (HazardScanner, DefensibilityScore, MapAnalyzer) auf reinen Snapshot-DTOs ohne RimWorld-Runtime-Dependency arbeiten können** (Testbarkeit-Invariante aus Architecture §2.2).

## Acceptance Criteria
1. `Source/Snapshot/CellSnapshot.cs` als immutable record mit Feldern: `(int x, int z) Position`, `string TerrainDefName`, `float Fertility`, `bool HasWater`, `HazardKind HazardKind`, `bool HasRoof`, `bool IsMountain`, `bool HasResources`, `float ChokepointScore` (später von Story 2.4 gefüllt)
2. `Source/Snapshot/HazardKind.cs` enum: `None, Lava, Pollution, Toxic, Radiation`
3. `Source/Snapshot/ISnapshotProvider.cs` interface mit `CellSnapshot[] GetCells(Map map)`, `ColonySnapshot GetColony()`, `PawnSnapshot[] GetPawns()`
4. `Source/Snapshot/RimWorldSnapshotProvider.cs` implementiert das Interface; `GetCells` iteriert `map.AllCells` und mappt TerrainDef → snapshot fields
5. Per-Cell-Daten werden **nicht persistiert** (D-14 + Architecture §2.2: `MapAnalysisSummary` persistiert nur Top-3-Sites)
6. Basis-Scan läuft in unter 500ms auf 250×250 Map (Performance-Budget §8 — hier noch ohne Coroutine-Split, das ist Story 2.9)
7. Unit-Tests prüfen `CellSnapshot`-Konstruktion und Value-Equality
8. Integration-Test: auf echter Map Scan aufrufen, Array-Länge = `map.Area`

## Tasks
- [ ] `CellSnapshot` record mit allen Feldern
- [ ] `HazardKind` enum
- [ ] `ISnapshotProvider` interface
- [ ] `RimWorldSnapshotProvider` — Mapping Map → Snapshots
- [ ] TerrainDef-Lookup für Fertility (via `TerrainDef.fertility`)
- [ ] `HasWater`: `terrain.IsWater`
- [ ] Performance-Probe: Stopwatch-Wrap um `GetCells`, log Millisekunden
- [ ] Unit-Test `CellSnapshot` Value-Equality
- [ ] Integration-Test auf 250×250 Map

## Dev Notes
**Architektur-Kontext:** §2.2 Snapshot + Analysis. Diese Story liefert Fundament für alle weiteren Epic-2-Stories.
**Nehme an, dass:** `map.AllCells` gibt `IEnumerable<IntVec3>` in row-major order; `TerrainDef.fertility` in `[0.0, 1.0]`.
**Vorausgesetzt:** Story 1.3 (BotGameComponent/BotMapComponent existieren — Snapshot ist aber transient, nicht persistiert).

## File List
| Pfad | Op |
|---|---|
| `Source/Snapshot/CellSnapshot.cs` | create |
| `Source/Snapshot/HazardKind.cs` | create |
| `Source/Snapshot/ISnapshotProvider.cs` | create |
| `Source/Snapshot/RimWorldSnapshotProvider.cs` | create |

## Testing
- Unit: CellSnapshot Value-Equality, record-semantics
- Integration: Scan auf echter Map, Array-Länge, Performance < 500ms

## Review-Gate
Code-Review gegen §2.2 ISnapshotProvider-Schema, Identifier-only-Pattern (keine ThingDef/Map-Refs in Snapshot). Visual: nicht relevant.
