# Story 4.9: Emergency-Handler für I6-I12

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Emergency-Handler für Invariants I6-I12** (Mood-Break-Prevention, Emergency-Doctor, Combat-Readiness, Food-Replenish, Medicine-Craft, Sleep-Priority).

## Acceptance Criteria
1. 7 neue `E_*.cs` Handler analog Story 3.3-3.6
2. Jeder mit BasePrio, Score, Apply
3. E-MOOD: Recreation/Joy-Tasks + Work-Reduce
4. E-HEALTH: Doctor + Medicine-Use
5. E-MENTALBREAK: Drug-Use (falls Ideology erlaubt) oder Isolation
6. E-RAID: Draft + Killpoint-Retreat (Story 4.7 Killpoint)
7. E-FOODDAYS: Hunting-Priority + Cooking
8. E-MEDICINE: Herbal-Crafting-Bill
9. E-SLEEP: Force-Sleep via `pawn.jobs.StartJob(LayDown)`
10. Unit-Tests pro Handler

## Tasks
- [ ] 7 neue Handler-Files
- [ ] Score-Modifier je Handler (context-sensitiv)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §2, D-16 Utility-Scoring.
**Vorausgesetzt:** 4.8, 3.3-3.6.

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_Mood.cs` — `E_PawnSleep.cs` | create |

## Testing
Unit: Score-Logic. Integration: Parallel-Emergencies → Utility-Max-Pick.

## Review-Gate
Code-Review gegen D-16, F-AI-02.
