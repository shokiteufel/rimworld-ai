# Story 6.6: Components-Fabrik (Self-Sustained Production)

**Status:** ready-for-dev
**Epic:** Epic 6 — Industrialization
**Size:** S

## Story
Als Mod-Entwickler möchte ich **Components-Fabrication** als automatische BillManager-Kette: Steel + Plasteel + Advanced-Components via Fabrication-Bench, Target-Count 50+.

## Acceptance Criteria
1. `BillPlanner` erweitert um Components-Recipes (Vanilla: `MakeComponent` am Fabrication-Bench)
2. Target-Count = 50 (Ending-Ship-Requirement)
3. Steel-Stock-Vorbedingung: ≥ 300 Steel (Mining-Priority auto-Anpassung)
4. Unit-Tests

## Tasks
- [ ] `BillPlanner.PlanComponents`
- [ ] Mining-Priority-Trigger
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §3 Phase 6, Ending-Pfade.md (Ship-Components).
**Vorausgesetzt:** 3.9, 6.3.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/BillPlanner.cs` | modify |

## Testing
Unit: Component-Bill-Generation.

## Review-Gate
Code-Review gegen D-15.
