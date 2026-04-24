# Story 4.5: Phase 3 Goals + Exit (Winter Readiness)

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Phase 3 (Winter Readiness)** implementieren: 90-Tage-Food, Cooler, Heater, Backup-Kleidung.

## Acceptance Criteria
1. `Phase3_WinterReadiness : PhaseDefinition`
2. Goals: G3.1 Pemmikan/Potato-Vorrat ≥ 90 * ColonistCount Tage, G3.2 Cooler + Heater installiert, G3.3 Backup-Winterkleidung (Parka/Tuque) pro Pawn
3. Exit: alle Goals + `stableCounter >= 2` (D-26) UND **`EmergencyResolver.ActiveEmergencies.Count == 0`** (MED-Fix, CC-STORIES-12, F-AI-01)
4. Launch-Critical: G3.1, G3.3 (Überleben)
5. Unit-Tests

## Tasks
- [ ] `Source/Phases/Phase3_WinterReadiness.cs`
- [ ] ColonySnapshot: `PemmikanCount`, `PotatoCount`, `WinterClothingCount`
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 3, Ending-Pfade.md (90-Tage-Regel).
**Vorausgesetzt:** 4.2.

## File List
| Pfad | Op |
|---|---|
| `Source/Phases/Phase3_WinterReadiness.cs` | create |

## Testing
Unit: Exit-Progression.

## Review-Gate
Code-Review gegen D-11 (keine Zeitvorgabe fürs Erreichen, nur Goal-Erfüllung).
