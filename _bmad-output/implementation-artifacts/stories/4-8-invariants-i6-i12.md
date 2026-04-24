# Story 4.8: Invariants I6-I12 (Mood, Health, Mental-Break, Raid-Defense, Food-Days, Medicine, Pawn-Sleep)

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** M

## Story
Als Mod-Entwickler möchte ich die **7 weiteren Invariants I6-I12** implementieren (Mood, Health, Mental-Break-Risk, Raid-Defense-Readiness, Food-Days-per-Pawn, Medicine-Stock, Pawn-Sleep-Debt).

## Acceptance Criteria
1. 7 neue `I6_*.cs` bis `I12_*.cs` Klassen
2. Jede mit Eligibility + Severity + Launch-Critical-Flag
3. I6 Mood: `AvgMood < 0.37` (D-07-like threshold)
4. I7 Health: `AvgHp < 0.5` pro Pawn
5. I8 MentalBreakRisk: ein Pawn mit `MentalBreak-Threshold` Nähe
6. I9 RaidDefense: `ColonyStrength < ExpectedRaidStrength`
7. I10 FoodDays (pro Pawn): Food < 3 Tage pro Pawn
8. I11 Medicine: Medicine < 5
9. I12 PawnSleep: einer mit Sleep-Need < 0.2
10. Unit-Tests pro Invariant
11. Launch-Critical: I7, I9, I10, I12

## Tasks
- [ ] 7 neue Invariant-Files
- [ ] ColonySnapshot-Felder: MoodAvg, HpAvg, MentalBreakRiskPawns, ColonyStrength, FoodDaysPerPawn, MedicineCount, LowSleepPawns
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §2, F-AI-16 Launch-Critical-Klassifikation.
**Vorausgesetzt:** 3.1, 3.2.

## File List
| Pfad | Op |
|---|---|
| `Source/Invariants/I6_Mood.cs` — `I12_PawnSleep.cs` | create |
| `Source/Snapshot/ColonySnapshot.cs` | modify |

## Testing
Unit: 3 Fälle pro Invariant.

## Review-Gate
Code-Review gegen Mod-Leitfaden §2, F-AI-16.
