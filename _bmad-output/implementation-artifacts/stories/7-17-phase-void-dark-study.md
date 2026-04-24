# Story 7.17: PHASE_VOID Dark-Study-Grind

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Dark-Study-Research-Grind** (nur Anomaly): Anomaly-Research-Queue, Entity-Capture für Study, Psycaster-Abilities-Unlock.

## Acceptance Criteria
1. **DLC-Guard am Plan-Anfang** (MED-Fix, CC-STORIES-05): `if (!DlcCapabilities.EndingAvailable(Ending.Void)) return ResearchQueue.Empty;` — Void-Ending ist Anomaly-gated (`Ending.Void` erfordert `DlcCapabilities.HasAnomaly`).
2. `DarkStudyPlanner` Anomaly-Research-Queue
3. Entity-Capture-Prep: Holding-Platform-Build + Containment
4. Mental-Break-Mitigation für Study-Pawn (Dark-Research hat Mental-Cost)
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/DarkStudyPlanner.cs`
- [ ] Containment-Build-Plan
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Void.
**Sub-Phase (Story 7.0):** Implementiert `DarkStudy` aus `EndingSubPhaseStateMachine`.
**Vorausgesetzt:** 7.0, 7.16.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/DarkStudyPlanner.cs` | create |

## Testing
Unit: Study-Progression.

## Review-Gate
Code-Review gegen D-15.
