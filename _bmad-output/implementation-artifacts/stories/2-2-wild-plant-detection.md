# Story 2.2: Wilde-Pflanzen-Erkennung (Berries, Healroot, Agave)

**Status:** review
**Epic:** Epic 2 — Map-Analyzer
**Size:** S

## Story
Als Mod-Entwickler möchte ich **wilde Ess- und Medizinal-Pflanzen (Berries, Healroot, Agave, Mushrooms, Ambrosia) pro Zelle im `CellSnapshot` erkennen**, damit die Scoring-Formel (Story 2.5) `W_FOOD`-Beitrag korrekt berechnet.

## Acceptance Criteria
1. `CellSnapshot` bekommt Feld `WildPlantKind? WildPlant` (nullable enum: `Berries, Healroot, Agave, AmbrosiaBush, PsychoidPlant, Smokeleaf, Other`). **D-42 retroactive 2026-04-25:** `GlowyMushroom` aus Story-Original-AC entfernt — existiert nicht in Vanilla RimWorld 1.6 (auch nicht in Anomaly/Odyssey-Defs verifiziert). Verifizierte Defs siehe AC-3.
2. `RimWorldSnapshotProvider.GetCells` erkennt Pflanzen via `map.thingGrid.ThingsListAtFast(pos)` + `thing.def.defName`-Match gegen Whitelist (hardcoded Set in `WildPlantRegistry`).
3. Pflanzen-Whitelist via `string defName` Set (keine direkten `ThingDef`-Refs, folgt D-23 Identifier-only). **Verifizierte Vanilla-defNames** (cross-checked 2026-04-25, Core + Odyssey-DLC):
   - Berries: `Plant_Berry` (Core), `Plant_Berry_Leafless` (**Odyssey** Winter-Variante)
   - Healroot: `Plant_Healroot` (Core kultiviert), `Plant_HealrootWild` (Core)
   - Agave: `Plant_Agave` (Core)
   - AmbrosiaBush: `Plant_Ambrosia` (Core, kein DLC-Guard)
   - PsychoidPlant: `Plant_Psychoid` (Core kultiviert), `Plant_Psychoid_Wild` (**Odyssey**)
   - Smokeleaf: `Plant_Smokeleaf` (Core kultiviert), `Plant_Smokeleaf_Wild` (**Odyssey**)
   Total: 10 defNames (7 Core + 3 Odyssey).
4. **D-42 retroactive 2026-04-25 (mit CR-Korrektur):** Original-AC behauptete „Biotech-Ambrosia" — falsch. `Plant_Ambrosia` ist Core-Vanilla. KEIN DLC-Guard nötig in Story 2.2-Scope. Auch kein expliziter Odyssey-`ModsConfig.IsActive`-Guard für die 3 Odyssey-Defs: String-Whitelist gegen ungespawnte Defs ist no-op (Cell hat den Plant nie → kein Match). DLC-Guards-Code-Pattern bleibt als Reservation für künftige DLC-spezifische Wild-Plants die echte Pfad-Logik brauchen.
5. Unit-Test `WildPlantRegistry`: Lookup mit bekannten defNames returnt korrekte Kind, Unbekannte returnen null.
6. Integration-Test (User-Game-Test MT-6): GetCells auf Temperate-Forest-Map (z.B. Default-Tutorial-Spawn), Player.log-Output enthält Cell-Count mit `WildPlant != null` (Berries-Cells erwartet auf Temperate-Forest).

## Tasks
- [x] `WildPlantKind` enum (D-42: ohne GlowyMushroom; `Other` als Foundation für Story 2.5)
- [x] `WildPlantRegistry` mit `Dictionary<string, WildPlantKind>` (10 Core+Odyssey-defNames)
- [x] `RimWorldSnapshotProvider.GetCells` ergänzen: ClassifyCellThings single-pass für HasMineable + WildPlant
- [x] DLC-Guards-Konzept dokumentiert (D-42 + WildPlantRegistry-Comment): String-Whitelist macht Code-Guards unnötig
- [x] Unit-Tests `WildPlantRegistryTests` (12 Tests, inkl. HashSet-Spec-Lock)
- [ ] Integration-Test Temperate-Forest (User-Game-Test MT-6)

## Dev Notes
**Architektur-Kontext:** Erweitert `CellSnapshot` aus Story 2.1. Keine eigene Klasse nötig außer `WildPlantRegistry` als Lookup-Service.
**Nehme an, dass:** `map.thingGrid.ThingsListAtFast(pos)` (CR-Korrektur 2026-04-25 — Original-Note schrieb `ThingsAt` was eine andere Vanilla-Methode ist) ist performant genug für alle Cells (intern List<Thing> ohne Allokation pro Iteration).
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
