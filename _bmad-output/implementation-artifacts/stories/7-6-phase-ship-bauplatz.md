# Story 7.6: PHASE_SHIP Bauplatz-Planung

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Ship-Bauplatz-Selection + Structural-Layout**: benötigt offenes Gelände 7×7+ neben Base, mit Zugang zu Ship-Components + Defense.

## Acceptance Criteria
1. **DLC-Guard am Plan-Anfang** (MED-Fix, CC-STORIES-05): `if (!DlcCapabilities.EndingAvailable(Ending.Ship)) return null;` — Ship = Vanilla + no-Royalty-Remove.
2. `ShipBauplatzPlanner.PlanLocation(MapAnalysisSummary) → (int x, int z) center`
3. Layout-Plan: Ship-Reactor center, Ship-Structural ring, Cryptosleep-Caskets outside, Computer + Engine attached
4. Keine Position in Choke-Points (Ship braucht offen für Belagerungs-Simulation)
5. Defense-Bau-Plan (Turrets + Walls) um Bauplatz
6. 15-Tage-Belagerung-Prep (aus Ending-Pfade.md)
7. Unit-Tests

## Tasks
- [ ] `Source/Decision/ShipBauplatzPlanner.cs`
- [ ] Open-Area-Detection (kein Mountain, keine Walls)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Ship-Ending.
**Sub-Phase (Story 7.0):** Implementiert `ShipBauplatz` aus `EndingSubPhaseStateMachine`.
**Vorausgesetzt:** 7.0, 7.5, 2.6.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/ShipBauplatzPlanner.cs` | create |

## Testing
Unit: Layout-Sanity-Check. Integration: Flat-Biome.

## Review-Gate
Code-Review gegen D-15.
