# Story 2.2: Wilde-Pflanzen-Erkennung (Berries, Healroot, Agave)

**Status:** in-progress
**Epic:** Epic 2 — Map-Analyzer
**Size:** S

## Story
Als Mod-Entwickler möchte ich **wilde Ess- und Medizinal-Pflanzen (Berries, Healroot, Agave, Mushrooms, Ambrosia) pro Zelle im `CellSnapshot` erkennen**, damit die Scoring-Formel (Story 2.5) `W_FOOD`-Beitrag korrekt berechnet.

## Acceptance Criteria
1. `CellSnapshot` bekommt Feld `WildPlantKind? WildPlant` (nullable enum: `Berries, Healroot, Agave, AmbrosiaBush, GlowyMushroom, PsychoidPlant, Smokeleaf, Other`)
2. `RimWorldSnapshotProvider.GetCells` erkennt Pflanzen via `map.thingGrid.ThingsAt(pos)` + `thing.def.defName`-Match gegen Whitelist (hardcoded Set)
3. Pflanzen-Whitelist via `string defName` Set (keine direkten `ThingDef`-Refs, folgt D-23 Identifier-only)
4. DLC-spezifische Pflanzen via `DlcCapabilities.IsInstalled(dlc)`-Guard — Biotech-Ambrosia nur wenn Biotech aktiv
5. Unit-Test mit fake-CellSnapshot mit verschiedenen defName-Strings
6. Integration-Test: Scan auf Temperate-Forest-Map → mindestens 1 Berries-Cell gefunden

## Tasks
- [ ] `WildPlantKind` enum erweitern mit DLC-spezifischen Varianten
- [ ] `WildPlantRegistry` mit Set `HashSet<string>` defName → WildPlantKind
- [ ] `RimWorldSnapshotProvider.GetCells` ergänzen: für jede Cell Thing-Lookup + WildPlantKind setzen
- [ ] DLC-Guards für Biotech/Odyssey-spezifische Pflanzen
- [ ] Unit-Test `WildPlantRegistry`
- [ ] Integration-Test Temperate-Forest-Seed

## Dev Notes
**Architektur-Kontext:** Erweitert `CellSnapshot` aus Story 2.1. Keine eigene Klasse nötig außer `WildPlantRegistry` als Lookup-Service.
**Nehme an, dass:** `map.thingGrid.ThingsAt(pos)` ist performant genug für alle Cells (wird intern von RimWorld gecached).
**Vorausgesetzt:** Story 2.1 (CellSnapshot existiert), Story 1.3 (`DlcCapabilities`).

## File List
| Pfad | Op |
|---|---|
| `Source/Snapshot/CellSnapshot.cs` | modify (Feld ergänzen) |
| `Source/Snapshot/WildPlantKind.cs` | create |
| `Source/Snapshot/WildPlantRegistry.cs` | create |
| `Source/Snapshot/RimWorldSnapshotProvider.cs` | modify (Plant-Lookup) |

## Testing
- Unit: Registry-Lookup mit bekannten defNames + Unknowns
- Integration: Map-Scan findet Wild-Plants

## Review-Gate
Code-Review gegen D-23 (defName-Strings, keine ThingDef-Refs), DLC-Guards korrekt. Visual: nicht relevant.
