# Story 4.7: Killpoint-Layout

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Killpoint-Layout-Generation** (Choke-Corridor mit Traps + Sandbag-Positions für Shooters), damit Raids in vorbereiteter Kill-Zone landen.

## Acceptance Criteria
1. `KillpointPlanner.Plan(MapAnalysisSummary, ColonySnapshot) → BuildPlan`
2. Nutzt DefensibilityAnalyzer-Score (Story 2.4) um Choke-Points zu finden
3. Layout: 5-Tile-Corridor + 3×3 Shooter-Area mit Sandbags + 4-6 Trap-Slots
4. Traps: Spike-Trap (Vanilla) oder Explosive (Phase 4+)
5. **D-25-Tag-Write im BuildWriter-Apply** (HIGH-Fix): alle vom Killpoint-Plan platzierten Things werden nach `PlaceThing`/`PlaceBlueprint` in `BotMapComponent.botPlacedThings` mit Tag `(PhaseIndex=4, GoalId="killpoint-layout")` geschrieben (D-25 Goal-Phase-Tag-Schema). Ohne Tag-Write kann `CancelOrphanedDesignations` bei Phase-Exit das Layout nicht reconciliieren. Tag-Write erfolgt in `BuildWriter`-Apply (Execution-Schicht, nicht im Planner).
6. Unit-Tests + Integration mit Mountain-Biome
7. Integration-Test: Nach Apply → `botPlacedThings` enthält alle Killpoint-Things mit korrektem Tag
8. **Exception-Wrapper** (HIGH-Fix Round-2-Stability, CC-STORIES-02): `BuildWriter.Apply`-Aufruf für Killpoint-Layout (erbt Story 3.8 `BlueprintPlacer.Apply`-Wrapper) — keine eigene Exception-Handling-Neu-Implementierung, verweist auf Story 3.8 AC 8.

## Tasks
- [ ] `Source/Decision/KillpointPlanner.cs`
- [ ] Choke-Point-Pick-Heuristik
- [ ] Sandbag-Platzierung
- [ ] Trap-Kadenz
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §8.1 Raid-Recovery.
**Vorausgesetzt:** 2.4, 3.8.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/KillpointPlanner.cs` | create |

## Testing
Unit: Layout auf fake-ChokeScore. Integration: Mountain-Biome.

## Review-Gate
Code-Review gegen D-15, D-25 (Tag-Schreiben).
