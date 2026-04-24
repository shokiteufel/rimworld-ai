# Story 7.12: PHASE_ROYAL Imperium-Honor-Farm

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Imperium-Honor-Farming** (nur mit Royalty-DLC): Quest-Priorisierung Imperium-Quests, Bestow-Ceremony-Prep, Title-Progression auf Baron → Count → ... → Stellarch.

## Acceptance Criteria
1. `RoyalHonorFarmer.Plan(ColonySnapshot) → HonorFarmPlan`
2. Nur aktiv wenn `DlcCapabilities.HasRoyalty`
3. Quest-Acceptor priorisiert Imperium-Quests
4. Bestow-Ceremony-Vorbereitung: Throneroom, Robes, Food
5. Title-Threshold-Tracking
6. Unit-Tests

## Tasks
- [ ] `Source/Decision/RoyalHonorFarmer.cs`
- [ ] Throneroom-Build-Plan-Trigger
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Royal.
**Vorausgesetzt:** 7.1, 7.9.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/RoyalHonorFarmer.cs` | create |

## Testing
Unit: Title-Progression.

## Review-Gate
Code-Review gegen D-15, DlcCapabilities.
