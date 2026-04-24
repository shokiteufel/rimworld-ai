# Story 7.11: PHASE_JOURNEY Weltkarten-Reise

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Weltkarten-Reise-Management**: Ambush-Detection, Supply-Replenish, Destination-Pathing.

## Acceptance Criteria
1. `WorldTravelPhaseRunner` steuert Caravan auf Weltkarte
2. Ambush-Event: auto-retreat if Ratio > 1.5, sonst fight
3. Supply-Replenish via Settlement-Trading
4. Destination erreicht → Credits-Roll-Event akzeptieren
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/WorldTravelPhaseRunner.cs`
- [ ] Ambush-Handler
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Journey.
**Sub-Phase (Story 7.0):** Implementiert `WorldTravel` aus `EndingSubPhaseStateMachine` (letzte Sub-Phase des Journey-Endings).
**Vorausgesetzt:** 5.1, 7.0, 7.10.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/WorldTravelPhaseRunner.cs` | create |

## Testing
Unit: Ambush-Ratio-Logic.

## Review-Gate
Code-Review gegen D-15.
