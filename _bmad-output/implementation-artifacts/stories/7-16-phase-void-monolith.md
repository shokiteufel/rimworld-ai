# Story 7.16: PHASE_VOID Monolith-Aktivierung

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Monolith-Aktivierungs-Management** (nur Anomaly): Monolith-Detection, Entity-Investigate, Monolith-Stage-Progression.

## Acceptance Criteria
1. `MonolithManager` aktiv wenn `DlcCapabilities.HasAnomaly` + Monolith vorhanden
2. Stage-1-Activation: Pawn-Investigate-Job
3. Study-Rate: kontinuierlich via Research-Priority
4. Entity-Handling bei Spawn (Combat via Epic 5)
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/MonolithManager.cs`
- [ ] Monolith-Detection (MapScan für Monolith-ThingDef)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Void. Nur Anomaly-DLC.
**Vorausgesetzt:** 2.1, 7.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/MonolithManager.cs` | create |

## Testing
Unit: Stage-Progression.

## Review-Gate
Code-Review gegen D-15, DlcCapabilities.
