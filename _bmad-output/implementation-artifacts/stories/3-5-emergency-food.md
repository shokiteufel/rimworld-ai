# Story 3.5: Emergency-Handler E-FOOD

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** S
**Decisions referenced:** D-02 (Raw-Eating default-off nach Campfire)

## Story
Als Mod-Entwickler möchte ich den **E-FOOD-Handler** implementieren, der bei Food-Invariant-Violation (I2) Nahrungs-Sicherung koordiniert — Hunt, Forage, Cook-Priority.

## Acceptance Criteria
1. `E_Food : EmergencyHandler` mit `BasePrio = 10`
2. `Eligibility`: `I2_Food.Violated == true`
3. `Score`: `base + (days_until_starvation * -20)` (je kritischer, desto höher)
4. `Apply`: Bill-Plan für Simple Meal (Campfire/Stove), plus HuntingJob für Wild-Animals im 50-Tile-Radius
5. **D-02-Regel**: raw_eating nur wenn Campfire/Stove unreachable UND `days_until_starvation < 1`
6. `WorkPriorityPlan`: erhöht Cooking + Hunting + Plant-Cutting für Pawns mit passendem Skill
7. Unit-Tests: Starvation-Schwellenwerte, Raw-Eating-Trigger
8. Integration: Naked-Start mit 0 Food → E-Food aktiv → Pawns jagen/pflücken

## Tasks
- [ ] `Source/Emergency/E_Food.cs`
- [ ] Days-until-Starvation-Berechnung
- [ ] BillPlan-Erzeugung für Meals (verwendet Story-3.9-BillManager)
- [ ] WorkPriorityPlan-Erzeugung (Story-3.10-WorkAssigner)
- [ ] D-02 Raw-Eating-Guard
- [ ] Unit-Tests + Integration

## Dev Notes
**Architektur-Kontext:** D-02, Mod-Leitfaden §2 E-FOOD.
**Nehme an, dass:** Simple Meal benötigt Fire + Raw Ingredient (Berries/Meat); verfügbar ab Phase 0.
**Vorausgesetzt:** Story 3.1, 3.2 (I2), 3.8 (BuildPlanner für Campfire), 3.9 (BillManager).

## File List
| Pfad | Op |
|---|---|
| `Source/Emergency/E_Food.cs` | create |

## Testing
- Unit: Score-Kurve bei 3 vs. 1 vs. 0.5 Tage
- Integration: Naked-Brutality → E-Food greift

## Review-Gate
Code-Review gegen D-02, Mod-Leitfaden §2.
