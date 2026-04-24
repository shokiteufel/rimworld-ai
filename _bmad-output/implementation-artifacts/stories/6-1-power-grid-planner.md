# Story 6.1: Strom-Netz-Planer

**Status:** ready-for-dev
**Epic:** Epic 6 — Industrialization (Phase 5+6)
**Size:** M

## Story
Als Mod-Entwickler möchte ich **automatisches Power-Grid-Layout**: Solar + Wind + Battery + Cables zu Workbenches, mit Short-Circuit-Resilienz (Multiple Circuits).

## Acceptance Criteria
1. `PowerGridPlanner.Plan(ColonySnapshot, MapAnalysisSummary) → BuildPlan`
2. Strategie: 2 parallele Circuits (Main + Backup), 1 Battery-Bank pro Circuit
3. Solar-Placement: hellste Cells (offenes Terrain, keine Overhead-Walls)
4. Wind: wo Airflow möglich
5. Cables: kürzester Pfad Solar→Battery→Workbench
6. DLC-Guard: Wattage via `DlcCapabilities.HasBiotech` (Pollution-Power)
7. Unit-Tests Placement-Heuristik

## Tasks
- [ ] `Source/Decision/PowerGridPlanner.cs`
- [ ] Solar-Brightness-Scoring
- [ ] Cable-Routing (Dijkstra)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 5.
**Vorausgesetzt:** 2.6, 3.8.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/PowerGridPlanner.cs` | create |

## Testing
Unit: 2-Circuit-Pattern. Integration: nach Phase-5-Entry Grid baut sich.

## Review-Gate
Code-Review gegen D-15, D-25.
