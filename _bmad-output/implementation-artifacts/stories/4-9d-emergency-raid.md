# Story 4.9d: Emergency-Handler E-RAID

**Status:** ready-for-dev
**Epic:** Epic 4
**Size:** S
**Decisions referenced:** D-16, CC-STORIES-06, CC-STORIES-04 (Plan-Arbiter)

## Story
Als Mod-Entwickler möchte ich **E-RAID-Handler** als Emergency-Wrapper für CombatCommander (5.3): Raid-Detection via RaidEvent → Claim aller Combat-Pawns → Delegate an CombatCommander für DraftOrder-Plan.

## Acceptance Criteria
1. `E_Raid : EmergencyHandler` mit `BasePrio = 12`, `LockPriority = 100` (höchste)
2. Eligibility: `I9_RaidDefense.Violated` ODER aktive RaidEvent in EventQueue (letzte 100 Ticks)
3. Apply: delegiert an `CombatCommander.Plan()` aus Story 5.3 → DraftOrder → PlanArbiter (Story 1.11) merged
4. **Pawn-Exclusivity-Lock** mit höchster LockPriority = 100 (überschreibt E-BLEED=90, E-HEALTH=80)
5. Dauerhafter Lock bis Raid-End-Event
6. Unit-Tests inkl. Lock-Konflikt-Szenarien

## Tasks
- [ ] `Source/Emergency/E_Raid.cs`
- [ ] Integration mit CombatCommander + PlanArbiter
- [ ] Raid-End-Detection (keine feindlichen Pawns auf Home-Map)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §2 E-INTRUSION wird hier zu E-RAID expanded (inkl. 5.3 Decision-Tree-Logic).
**Vorausgesetzt:** 3.1, 3.13, 4.8, 5.3, 1.11.

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_Raid.cs` | create |

## Testing
Unit: E-RAID vs. E-BLEED Konflikt (E-RAID gewinnt), Raid-End-Unlock.

## Review-Gate
Code-Review gegen D-16, CC-STORIES-04, CC-STORIES-06.

## Transient/Persistent
Transient.
