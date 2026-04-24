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
5. Unit-Tests + Integration mit Mountain-Biome

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
