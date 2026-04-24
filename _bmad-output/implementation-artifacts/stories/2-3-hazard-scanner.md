# Story 2.3: Hazard-Scanner (Lava, Pollution, Toxic, Radiation)

**Status:** ready-for-dev
**Epic:** Epic 2 — Map-Analyzer
**Size:** M
**Decisions referenced:** D-09 (Hazard-Score W_HAZARD=0.30), Architecture TQ-01 (TerrainDef-Namen aus XML extrahieren)

## Story
Als Mod-Entwickler möchte ich **Hazard-Zellen (Lava, Pollution, Toxic-Terrain, Radiation-Zonen)** im `CellSnapshot.HazardKind` erkennen und einen **Hard-Filter-Mechanismus** (Lava-Tiles + hazardProximity<3 sind aus Site-Pool ausgeschlossen) bereitstellen.

## Acceptance Criteria
1. `HazardScanner.DetectHazards(Map map)` gibt `Dictionary<IntVec3, HazardKind>` zurück
2. Hazard-Matching via `compat-patterns.xml`-ähnlicher Konfig-Datei `Defs/HazardTerrainDefs.xml` mit `<HazardTerrainDef>` Einträgen (defName-Muster pro HazardKind)
3. DLC-Guards: Biotech-Pollution nur bei Biotech, Anomaly-Toxic nur bei Anomaly, Odyssey-Lava nur bei Odyssey
4. Hard-Filter: Cells mit `HazardKind != None` UND `hazardProximity < 3` (= Lava-Radius 3) sind vom Site-Pool ausgeschlossen (gesetzt als `BotMapComponent.excludedCells`)
5. W_HAZARD-Score pro Cell: `-0.30` für direkte Hazard-Cell, linear abfallend bis `-0.05` bei Proximity 3
6. Performance: Hazard-Scan < 100ms für 250×250 Map
7. Unit-Tests für `HazardScanner.DetectHazards` mit fake-Map-Data
8. Integration: Biotech-Polluted-Map zeigt korrekten Exclude-Set
9. **Schema-Bump** (HIGH-Fix): `excludedCells` in `BotMapComponent` ist neues Feld → via Story 1.9 `SchemaVersionRegistry` einen Schema-Eintrag `BotMapComponent.excludedCells` anlegen, `schemaVersion` auf 4 bumpen (falls aktueller Stand 3). Migrate v3→v4 setzt `excludedCells = new HashSet<(int,int)>()` (leer; Re-Analyse bei `Map.FinalizeInit` populiert neu).
10. **HashSet-Scribe-Format** (HIGH-Fix, D-23-konform): `excludedCells: HashSet<(int x, int z)>` — Scribe-Serialization via hilfsweise `List<int> _excludedCellsFlat` (Paare `x₁,z₁,x₂,z₂,…`), in `ExposeData()` flatten/rehydrate. Kein `IntVec3` in Persistent-Feld (D-23 verbietet RimWorld-Runtime-Typen wo vermeidbar). Identifier-only-Pattern: `(int,int)`-Tuple ist keine RimWorld-Runtime-Ref.

## Tasks
- [ ] `Source/Analysis/HazardScanner.cs` implementieren
- [ ] `Defs/HazardTerrainDefs.xml` mit known-Hazard-Patterns (Ludeon-Defs aus DLC-Registry abgeleitet)
- [ ] `HazardTerrainDef : Def` C#-Klasse für XML-Deserialisierung
- [ ] Hard-Filter-Logik in `BotMapComponent.excludedCells` speichern
- [ ] DLC-Guard-Integration
- [ ] Unit-Tests mit verschiedenen Hazard-Patterns
- [ ] Integration-Test Biotech-Pollution-Seed

## Dev Notes
**Architektur-Kontext:** §2.2 `HazardScanner`-Klasse. Arbeitet auf raw-Map statt Snapshots (Perf: TerrainDef-Lookup direkt, kein Snapshot-Overhead für reinen Hazard-Scan).
**Nehme an, dass:** `map.terrainGrid.TerrainAt(pos).defName` ist deterministisch über RimWorld-Versionen; DLC-Hazard-DefNames sind in XML kataloglierbar (Architecture TQ-01 resolved hier).
**Vorausgesetzt:** Story 2.1 (CellSnapshot), Story 1.3 (BotMapComponent, DlcCapabilities).

## File List
| Pfad | Op |
|---|---|
| `Source/Analysis/HazardScanner.cs` | create |
| `Source/Defs/HazardTerrainDef.cs` | create |
| `Defs/HazardTerrainDefs.xml` | create |
| `Source/Data/BotMapComponent.cs` | modify (excludedCells-Feld + Scribe) |

## Testing
- Unit: HazardScanner mit fake-Terrain (Lava/Pollution/Clean)
- Integration: Biotech-Polluted-Map → excludedCells enthält polluted tiles

## Review-Gate
Code-Review gegen D-09 Hazard-Score-Formel, DLC-Capabilities-Guards, Hard-Filter-Semantik. TQ-01 (TerrainDef-Namen) via Defs-XML aufgelöst.

## Aufgelöste TBDs
- **TQ-01 resolved (via diese Story):** HazardTerrainDefs.xml als RimWorld-Def-Katalog statt hardcoded-Array — XML-patchable durch Dritt-Mods bei DLC-Updates.
