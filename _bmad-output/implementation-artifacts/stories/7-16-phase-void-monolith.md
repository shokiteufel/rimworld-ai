# Story 7.16: PHASE_VOID Monolith-Aktivierung

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Monolith-Aktivierungs-Management** (nur Anomaly): Monolith-Detection, Entity-Investigate, Monolith-Stage-Progression.

## Acceptance Criteria
1. **DLC-Guard als Early-Return am PhaseRunner-Tick** (HIGH-Fix Round-2 RimWorld, CC-STORIES-05): `if (!DlcCapabilities.EndingAvailable(Ending.Void)) { inactive = true; return; }` als allererste Zeile von `Tick()`/`Evaluate()`. Void-Ending ist Anomaly-DLC-gated; ohne Anomaly gibt es kein Monolith-Entity + keine Void-QuestScriptDef.
2. `MonolithPhaseRunner` aktiv wenn `DlcCapabilities.HasAnomaly` + Monolith vorhanden
3. Stage-1-Activation: Pawn-Investigate-Job
4. Study-Rate: kontinuierlich via Research-Priority
5. Entity-Handling bei Spawn (Combat via Epic 5)
6. Unit-Tests

## Tasks
- [ ] `Source/Decision/MonolithPhaseRunner.cs`
- [ ] Monolith-Detection (MapScan für Monolith-ThingDef)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Void. Nur Anomaly-DLC.
**Sub-Phase (Story 7.0):** Implementiert `MonolithActivation` aus `EndingSubPhaseStateMachine`.
**Vorausgesetzt:** 2.1, 7.0, 7.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/MonolithPhaseRunner.cs` | create |

## Testing
Unit: Stage-Progression.

## Review-Gate
Code-Review gegen D-15, DlcCapabilities.
