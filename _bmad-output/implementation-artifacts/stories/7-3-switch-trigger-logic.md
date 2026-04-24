# Story 7.3: Switch-Trigger-Logik (Hysterese + Phase-7-Lock)

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M
**Decisions referenced:** D-17, F-AI-10 (sunk_cost normalisiert), F-AI-11 (Phase-7-Escape-Hatches), F-AI-15 (autoEscapeStableCounter), F-AI-17 (Auto-Release bei Phase-Regression)

## Story
Als Mod-Entwickler möchte ich **Switch-Trigger-Logik** mit Hysterese-Gate + Sunk-Cost-Penalty + Phase-7-Lock + Auto-Escape-Counter genau wie in Architecture §6.3.

## Acceptance Criteria
1. `EndingSwitchEvaluator.EvaluateSwitch(current, candidate, colony) → SwitchDecision`
2. Implementiert den Pseudocode aus §6.3 komplett
3. `sunk_cost_penalty = clamp(resources_invested / max_resources_for_ending, 0, 0.5)` (F-AI-10)
4. `autoEscapeStableCounter` wird inkrementiert/reset gemäß D-26 StableConsecutiveCounter (F-AI-15)
5. Phase-7-Lock Escape-Hatches: Forced, Auto-Escape, Game-Breaking-Event (F-AI-11)
6. Phase-Regression von 7→6 löst Auto-Release (F-AI-17) via `PhaseStateMachine.TransitionBackward`
7. Unit-Tests für alle Hysterese-/Escape-/Regression-Pfade
8. Integration: Simuliertes Game-Breaking-Event

## Tasks
- [ ] `Source/Decision/EndingSwitchEvaluator.cs`
- [ ] `SwitchDecision` record
- [ ] `PhaseStateMachine.TransitionBackward`-Hook (F-AI-17)
- [ ] Unit-Tests
- [ ] Integration

## Dev Notes
**Kontext:** §6.3 komplett, D-17, D-26.
**Vorausgesetzt:** 7.1, 7.2, 1.3.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/EndingSwitchEvaluator.cs` | create |
| `Source/Phases/PhaseStateMachine.cs` | modify (TransitionBackward) |

## Testing
Unit: Alle Hysterese/Escape-Pfade. Integration: Reactor-Destroyed-Event.

## Review-Gate
Code-Review gegen D-17, F-AI-10/11/15/17, §6.3 Pseudocode exakt.
