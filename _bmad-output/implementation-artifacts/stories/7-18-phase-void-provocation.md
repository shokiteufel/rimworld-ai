# Story 7.18: PHASE_VOID Void-Provocation-Ritual

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Void-Provocation-Finale** (nur Anomaly): Ritual-Prep, Final-Entities-Encounter, Void-Outcome-Akzeptanz.

## Acceptance Criteria
1. `VoidProvocationManager` aktiv nach Dark-Study-Done
2. Ritual-Prep: Pawns, Materials, Combat-Readiness
3. Final-Entity-Handling (Combat-intensiv)
4. Credits-Roll-Event akzeptieren
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/VoidProvocationManager.cs`
- [ ] Ritual-Trigger
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Void Provocation.
**Vorausgesetzt:** 7.16, 7.17, 5.x (Combat).

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/VoidProvocationManager.cs` | create |

## Testing
Unit: Ritual-Step-Tracking.

## Review-Gate
Code-Review gegen D-15.
