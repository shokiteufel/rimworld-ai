# Story 6.4: Phase 6 Goals + Exit (Industrialization)

**Status:** ready-for-dev
**Epic:** Epic 6 — Industrialization
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Phase 6 (Industrialization)** Goals: Components-autarke Produktion, mind. 1 Specialist-Role pro Bereich (Crafter/Medic/Researcher), Fabrication-Bench operational.

## Acceptance Criteria
1. `Phase6_Industrialization : PhaseDefinition`
2. Goals: G6.1 Fabrication-Bench, G6.2 Components ≥ 50 in Stockpile, G6.3 Specialists assigned
3. Exit + stableCounter ≥ 2
4. Launch-Critical: G6.2 (Ending-Blocker)
5. Unit-Tests

## Tasks
- [ ] `Source/Phases/Phase6_Industrialization.cs`
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 6.
**Vorausgesetzt:** 6.2, 6.3, 6.5, 6.6.

## File List
| Pfad | Op |
|---|---|
| `Source/Phases/Phase6_Industrialization.cs` | create |

## Testing
Unit: Exit.

## Review-Gate
Code-Review gegen D-11, F-AI-16.
