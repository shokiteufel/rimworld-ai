# Story 4.9f: Emergency-Handler E-MEDICINE

**Status:** ready-for-dev
**Epic:** Epic 4
**Size:** S

## Story
Als Mod-Entwickler möchte ich **E-MEDICINE** implementieren: bei Medicine-Stock<5 Herbal-Medicine-Crafting + Harvest-Priority für Healroot-Wild-Plants.

## Acceptance Criteria
1. `E_Medicine : EmergencyHandler` mit `BasePrio = 7`, `LockPriority = 50`
2. Eligibility: `I11_Medicine.Violated`
3. Apply: BillPlan für Herbal-Medicine-Craft; WorkPriority für Healroot-Harvest auf Map
4. Score-Modifier: `+50 wenn bleedende Pawns aktiv` (kritischer Medicine-Bedarf)
5. Unit-Tests

## Tasks
- [ ] `Source/Emergency/E_Medicine.cs`
- [ ] Healroot-Scanner-Integration via Snapshot-Provider
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §2.
**Vorausgesetzt:** 3.1, 3.13, 4.8, 3.9 (BillManager), 2.2 (WildPlant-Detection).

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_Medicine.cs` | create |

## Testing
Unit: Bleed-Boost-Score, Healroot-Harvest-Priority.

## Review-Gate
Code-Review gegen D-16.

## Transient/Persistent
Transient.
