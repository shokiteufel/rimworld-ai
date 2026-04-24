# Story 4.2: Phase 2 Goals + Exit (Food Security + Research)

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Phase 2 (Food Security + Research)** mit Goals (ausreichend Anbau, Stonecutting-Research, erster Koch-Workbench) und Exit-Conditions implementieren.

## Acceptance Criteria
1. `Phase2_FoodResearch : PhaseDefinition`
2. Goals: G2.1 Cooking-Stove, G2.2 Stonecutting Research done, G2.3 FoodStockDays ≥ 30
3. Exit: alle Goals + stableCounter ≥ 2 (D-26)
4. Launch-Critical: G2.1, G2.3 (G2.2 nicht, weil Tech-Progression)
5. Unit-Tests + Integration

## Tasks
- [ ] `Source/Phases/Phase2_FoodResearch.cs`
- [ ] Goal-Definitionen
- [ ] Unit-Tests
- [ ] Integration

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 2, F-AI-16 (Launch-Critical-Split).
**Vorausgesetzt:** 3.11, 4.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Phases/Phase2_FoodResearch.cs` | create |

## Testing
Unit: Exit-Progression. Integration: Phase-2-Start.

## Review-Gate
Code-Review gegen D-11, D-26, F-AI-16.
