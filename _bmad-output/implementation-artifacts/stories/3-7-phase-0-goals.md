# Story 3.7: Phase 0 Goals + Exit-Conditions

**Status:** ready-for-dev
**Epic:** Epic 3 — Invariants & Phase 0+1
**Size:** M
**Decisions referenced:** D-11 (Zeit keine Voraussetzung), D-26 (StableConsecutiveCounter), F-AI-04 (Stuck-State-Handling)

## Story
Als Mod-Entwickler möchte ich **Phase 0 (Naked Start / Basic Survival)** als `PhaseDefinition` mit expliziten Goals + Exit-Conditions implementieren, damit der Bot den Einstieg ab Tick 1 strukturiert abarbeitet.

## Acceptance Criteria
1. `Phase0_NakedStart : PhaseDefinition` in `Source/Phases/Phase0_NakedStart.cs`
2. **Goals** (priorisiert):
   - G0.1: Campfire gebaut
   - G0.2: Primitive Shelter (Wood-Walls + Door) steht, dach-bedeckt
   - G0.3: Kleidung für alle Pawns (zumindest Tribalwear)
   - G0.4: Food-Stock ≥ 3 Tage pro Pawn
3. **Exit-Conditions** (alle erfüllt):
   - G0.1–G0.4 done
   - `stableCounter >= 2` (D-26)
   - Kein Emergency aktiv für 2 konsekutive Eval-Ticks
4. **Launch-Critical-Flag** pro Goal gesetzt (F-AI-16: G0.1-G0.4 alle launch-critical)
5. **Stuck-State-Handler** (F-AI-04): wenn Phase 0 > 5000 Ticks ohne Progress → `FallbackOnUnreachable`-Hook (nur Log-Warnung, kein Abort; Phase bleibt aktiv)
6. Unit-Tests: Exit-Conditions mit fake-ColonySnapshot
7. Integration: Naked-Brutality → Phase 0 aktiv → Exit nach ~3 in-game Tage wenn Goals erreicht

## Tasks
- [ ] `Source/Phases/Phase0_NakedStart.cs`
- [ ] Goal-Definitionen als immutable records mit `(string Id, Func<ColonySnapshot, double> Progress, bool LaunchCritical)`
- [ ] Exit-Cond-Check-Logic
- [ ] Stuck-State-Hook
- [ ] Unit-Tests
- [ ] Integration Naked-Brutality

## Dev Notes
**Architektur-Kontext:** §2.1 `PhaseDefinition` + `PhaseStateMachine`, D-11 (keine Zeitvorgabe).
**Nehme an, dass:** Basic Tribal-Research-Level ist implizit (Vanilla Phase-0-Tech).
**Vorausgesetzt:** Story 3.1-3.6, Story 3.8 (BuildPlanner für Campfire + Walls).

## File List
| Pfad | Op |
|---|---|
| `Source/Phases/PhaseDefinition.cs` | create (abstract, falls nicht in 1.3) |
| `Source/Phases/Phase0_NakedStart.cs` | create |
| `Source/Phases/PhaseGoal.cs` | create (record) |

## Testing
- Unit: Exit-Cond-Progression mit fake-Snapshots
- Integration: Naked-Brutality-Scenario

## Review-Gate
Code-Review gegen D-11, D-26, F-AI-04, F-AI-16.
