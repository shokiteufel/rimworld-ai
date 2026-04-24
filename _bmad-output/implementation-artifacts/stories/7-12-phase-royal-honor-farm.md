# Story 7.12: PHASE_ROYAL Imperium-Honor-Farm

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Imperium-Honor-Farming** (nur mit Royalty-DLC): Quest-Priorisierung Imperium-Quests, Bestow-Ceremony-Prep, Title-Progression auf Baron → Count → ... → Stellarch.

## Acceptance Criteria
1. **DLC-Guard als Early-Return am Plan-Anfang** (HIGH-Fix Round-2 RimWorld, CC-STORIES-05): `if (!DlcCapabilities.EndingAvailable(Ending.Royal)) return HonorFarmPlan.Empty;` als allererste Zeile von `Plan()`. Royal-Ending ist Royalty-DLC-gated; ohne Royalty kein Imperium, kein Title-System.
2. `RoyalHonorFarmPlanner.Plan(ColonySnapshot) → HonorFarmPlan`
3. Quest-Acceptor priorisiert Imperium-Quests
4. Bestow-Ceremony-Vorbereitung: Throneroom, Robes, Food
5. Title-Threshold-Tracking
6. Unit-Tests

## Tasks
- [ ] `Source/Decision/RoyalHonorFarmPlanner.cs`
- [ ] Throneroom-Build-Plan-Trigger
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Royal.
**Sub-Phase (Story 7.0):** Implementiert `HonorFarm` aus `EndingSubPhaseStateMachine`.
**Vorausgesetzt:** 1.12 (QuestManager-Polling), 7.0, 7.1, 7.9.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/RoyalHonorFarmPlanner.cs` | create |

## Testing
Unit: Title-Progression.

## Review-Gate
Code-Review gegen D-15, DlcCapabilities.
