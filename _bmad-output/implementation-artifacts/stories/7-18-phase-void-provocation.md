# Story 7.18: PHASE_VOID Void-Provocation-Ritual

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Void-Provocation-Finale** (nur Anomaly): Ritual-Prep, Final-Entities-Encounter, Void-Outcome-Akzeptanz.

## Acceptance Criteria
1. **DLC-Guard** (MED-Fix, CC-STORIES-05): PhaseRunner nur aktiv wenn `DlcCapabilities.EndingAvailable(Ending.Void)` — Void-Ending ist Anomaly-gated. Bei `false` setzt PhaseRunner `inactive`-Flag.
2. `VoidProvocationPhaseRunner` aktiv nach Dark-Study-Done
3. Ritual-Prep: Pawns, Materials, Combat-Readiness
4. Final-Entity-Handling (Combat-intensiv)
5. Credits-Roll-Event akzeptieren
6. Unit-Tests

## Tasks
- [ ] `Source/Decision/VoidProvocationPhaseRunner.cs`
- [ ] Ritual-Trigger
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Void Provocation.
**Sub-Phase (Story 7.0):** Implementiert `VoidProvocation` aus `EndingSubPhaseStateMachine` (letzte Sub-Phase des Void-Endings).
**Vorausgesetzt:** 5.x (Combat), 7.0, 7.16, 7.17.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/VoidProvocationPhaseRunner.cs` | create |

## Testing
Unit: Ritual-Step-Tracking.

## Review-Gate
Code-Review gegen D-15.
