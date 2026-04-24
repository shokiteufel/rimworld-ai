# Story 7.11: PHASE_JOURNEY Weltkarten-Reise

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Weltkarten-Reise-Management**: Ambush-Detection, Supply-Replenish, Destination-Pathing.

## Acceptance Criteria
1. `WorldTravelManager` steuert Caravan auf Weltkarte
2. Ambush-Event: auto-retreat if Ratio > 1.5, sonst fight
3. Supply-Replenish via Settlement-Trading
4. Destination erreicht → Credits-Roll-Event akzeptieren
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/WorldTravelManager.cs`
- [ ] Ambush-Handler
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Journey.
**Vorausgesetzt:** 7.10, 5.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/WorldTravelManager.cs` | create |

## Testing
Unit: Ambush-Ratio-Logic.

## Review-Gate
Code-Review gegen D-15.
