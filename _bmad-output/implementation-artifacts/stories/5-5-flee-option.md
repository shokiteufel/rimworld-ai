# Story 5.5: Flee-Option (Caravan bei Overwhelming-Threat)

**Status:** ready-for-dev
**Epic:** Epic 5 — Combat & Raids
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Flee-via-Caravan** bei ThreatLevel=Overwhelming — alle Pawns + kritische Supplies werden in Caravan gepackt und fliehen zu World-Tile.

## Acceptance Criteria
1. `FleePlanner.Plan(ColonySnapshot, PawnSnapshot[]) → CaravanPlan`
2. Alle lebenden Pawns, max-tragfähige Supplies (Food, Medicine, Weapons)
3. Ziel-Tile: nächste Player-Faction-Settlement oder Faction-Neutral-Tile
4. CaravanPlan-Record identifier-only (D-23)
5. Caravan-Forming über `CaravanFormingUtility.StartFormingCaravan`
6. Unit-Tests (Pawn-Count, Supply-Selection)
7. Integration: Overwhelming-Raid → Caravan bildet sich

## Tasks
- [ ] `Source/Decision/FleePlanner.cs`
- [ ] Supply-Prioritäten-Matrix
- [ ] Ziel-Tile-Suche via `WorldPathFinder`
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Journey-Ending-Flow, Mod-Leitfaden §8.1.
**Vorausgesetzt:** 1.3, 5.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/FleePlanner.cs` | create |

## Testing
Unit: Caravan-Plan-Generation. Integration: Overwhelming-Raid-Sim.

## Review-Gate
Code-Review gegen D-15, D-23.
