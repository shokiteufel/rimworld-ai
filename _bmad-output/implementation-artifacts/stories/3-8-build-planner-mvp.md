# Story 3.8: BuildPlanner MVP (Primitive Walls, Door, Bed, Campfire)

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** M
**Decisions referenced:** D-15 (Plan/Apply-Trennung), D-21/D-23 (Plan-Records identifier-only), D-25 (Goal-Phase-Tag-Schema)

## Story
Als Mod-Entwickler möchte ich den **`BuildPlanner`** implementieren, der basierend auf `PhaseGoals` und `MapAnalysisSummary` einen `BuildPlan` mit Blueprint-Intents für Primitive Walls, Door, Bed und Campfire erzeugt — rein pure, keine Map-Mutation.

## Acceptance Criteria
1. `BuildPlanner.Plan(ColonySnapshot, PhaseGoals, MapAnalysisSummary) → BuildPlan` in `Source/Decision/BuildPlanner.cs`
2. Für Phase-0-Goals: erzeugt Blueprint-Intents für:
   - Campfire (1× central)
   - Primitive Walls 3×3 um Campfire (mit 1 Door)
   - 1 Bed pro Pawn
3. Cell-Selection: nutzt `MapAnalysisSummary.TopSites[0].Center` als Basis-Mittelpunkt
4. Blueprint-Intent-Schema (D-23 identifier-only): `(string DefName, (int x, int z) Position, byte Rotation)`
5. **Plan/Apply-Trennung** (D-15): BuildPlanner mutiert keine Designations; `BlueprintPlacer.Apply(plan, map)` setzt Designations + trägt Goal-Tag ein (D-25)
6. Unit-Tests: verschiedene Center-Positionen, Pawn-Count-Variationen
7. Integration: Phase-0-Simulation → Plan enthält alle erwarteten Blueprints

## Tasks
- [ ] `Source/Decision/BuildPlanner.cs`
- [ ] `Source/Execution/BlueprintPlacer.cs` (Apply-Seite mit Goal-Tag-Write D-25)
- [ ] Layout-Helper: 3×3-Umrahmung, Door-Platzierung
- [ ] Bed-Count pro Pawn
- [ ] Unit-Tests
- [ ] Integration Phase-0

## Dev Notes
**Architektur-Kontext:** §2.3 + §2.3a (Plan-Schemas) + §2.3b (Goal-Tag-Schema).
**Nehme an, dass:** Wood ist in Phase 0 verfügbar (Start-Inventory + Tree-Chopping von Pawns).
**Vorausgesetzt:** Story 1.3, Story 2.6/2.8, Story 3.7.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/BuildPlanner.cs` | create |
| `Source/Execution/BlueprintPlacer.cs` | create |

## Testing
- Unit: Plan enthält 9 Walls + 1 Door + 1 Campfire + N Beds
- Integration: Apply erzeugt Vanilla-Blueprints

## Review-Gate
Code-Review gegen D-15, D-21, D-23, D-25.
