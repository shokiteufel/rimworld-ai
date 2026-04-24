# Story 4.9e: Emergency-Handler E-FOODDAYS (pro Pawn)

**Status:** ready-for-dev
**Epic:** Epic 4
**Size:** S

## Story
Als Mod-Entwickler möchte ich **E-FOODDAYS-Handler** — Variant von E-FOOD (3.5) aber pro-Pawn: wenn einzelner Pawn Food-Days<3 (z. B. Brauchtum-Precept, Weight-Loss-Trait), targeted Feeding-Priority.

## Acceptance Criteria
1. `E_FoodDays : EmergencyHandler` mit `BasePrio = 10`, `LockPriority = 70`
2. Eligibility: `I10_FoodDaysPerPawn.Violated` (pro-Pawn-Food-Check)
3. Apply: BillPlan für Pawn-Preferred-Food + Work-Priority-Addendum (Hunting für Affected-Pawn bei Carnivore-Trait)
4. Abgrenzung zu E-FOOD (3.5): E-FOOD ist colony-wide, E-FOODDAYS ist per-Pawn für Edge-Cases
5. Unit-Tests

## Tasks
- [ ] `Source/Emergency/E_FoodDays.cs`
- [ ] Pawn-Preferred-Food-Detection
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §2.
**Vorausgesetzt:** 3.1, 3.13, 4.8, 3.5 (colony-wide E-FOOD).

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_FoodDays.cs` | create |

## Testing
Unit: Trait-basierte Food-Auswahl.

## Review-Gate
Code-Review gegen D-16.

## Transient/Persistent
Transient.
