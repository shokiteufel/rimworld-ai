# Story 7.15: PHASE_ARCHONEXUS Transition-Sequenz

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Archonexus-Transition-Management**: Archonexus-Core-Quest-Accept, Caravan zu Archonexus, Items-Mitnahme, Final-Transition.

## Acceptance Criteria
1. `ArchonexusTransitionPhaseRunner` aktiv ab Wealth-Target-Reached
2. Quest-Accept: Archonexus-Core-Quest
3. Transition-Caravan mit max 3 Items (Vanilla-Regel)
4. Item-Priorität: Bionic-Upgrades, Tech-Items, High-Quality-Waffen
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/ArchonexusTransitionPhaseRunner.cs`
- [ ] Item-Priority-Matrix
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Archonexus Transition.
**Sub-Phase (Story 7.0):** Implementiert `Transition` aus `EndingSubPhaseStateMachine` (letzte Sub-Phase des Archonexus-Endings).
**Vorausgesetzt:** 1.12 (QuestManager-Polling), 7.0, 7.14.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/ArchonexusTransitionPhaseRunner.cs` | create |

## Testing
Unit: Item-Priority.

## Review-Gate
Code-Review gegen D-15.
