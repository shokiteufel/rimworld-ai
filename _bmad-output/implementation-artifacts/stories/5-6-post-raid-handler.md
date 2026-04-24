# Story 5.6: Post-Raid-Handler (Triage + Repair + Corpse-Disposal)

**Status:** ready-for-dev
**Epic:** Epic 5 — Combat & Raids
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Post-Raid-Cleanup** nach Raid-Ende: Wounded-Triage (Doctor-Priority), Wall-Repair (Construction-Priority), Corpse-Disposal (Hauling), Ammo-Recovery.

## Acceptance Criteria
1. `PostRaidPlanner.Plan(ColonySnapshot, PawnSnapshot[]) → PostRaidPlan`
2. PostRaidPlan enthält: Wounded-Tending-Priority, Damaged-Walls-List, Corpses-to-Haul, DroppedWeapons-to-Recover
3. Getrennte Plan-Applies pro Category (re-verwendet BillManager, WorkPriorityWriter)
4. Trigger via PostRaidEvent (nach Raid-End-Detection)
5. Auto-Trigger wenn alle Raiders destroyed/fled
6. Unit-Tests

## Tasks
- [ ] `Source/Decision/PostRaidPlanner.cs`
- [ ] Raid-End-Detection (kein aktiver Raid → trigger)
- [ ] Apply-Orchestration
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §8.1 Raid-Recovery.
**Vorausgesetzt:** 5.3, 5.4.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/PostRaidPlanner.cs` | create |

## Testing
Unit: Plan für 5 Corpses + 3 Damaged Walls. Integration: Post-Raid-Flow.

## Review-Gate
Code-Review gegen D-15.
