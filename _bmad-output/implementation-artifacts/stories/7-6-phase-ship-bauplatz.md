# Story 7.6: PHASE_SHIP Bauplatz-Planung

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Ship-Bauplatz-Selection + Structural-Layout**: benötigt offenes Gelände 7×7+ neben Base, mit Zugang zu Ship-Components + Defense.

## Acceptance Criteria
1. `ShipBauplatzPlanner.PlanLocation(MapAnalysisSummary) → (int x, int z) center`
2. Layout-Plan: Ship-Reactor center, Ship-Structural ring, Cryptosleep-Caskets outside, Computer + Engine attached
3. Keine Position in Choke-Points (Ship braucht offen für Belagerungs-Simulation)
4. Defense-Bau-Plan (Turrets + Walls) um Bauplatz
5. 15-Tage-Belagerung-Prep (aus Ending-Pfade.md)
6. Unit-Tests

## Tasks
- [ ] `Source/Decision/ShipBauplatzPlanner.cs`
- [ ] Open-Area-Detection (kein Mountain, keine Walls)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Ship-Ending.
**Vorausgesetzt:** 7.5, 2.6.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/ShipBauplatzPlanner.cs` | create |

## Testing
Unit: Layout-Sanity-Check. Integration: Flat-Biome.

## Review-Gate
Code-Review gegen D-15.
