# Story 4.4: Gefangenenzelle-Layout

**Status:** ready-for-dev
**Epic:** Epic 4 — Phase-State-Machine 2-4
**Size:** S

## Story
Als Mod-Entwickler möchte ich **automatisches Prisoner-Cell-Layout** (Bed + Lamp + Floor, separate Room mit Door) im BuildPlanner-Kontext implementieren, damit Recruiting einen designierten Raum hat.

## Acceptance Criteria
1. `BuildPlanner.PlanPrisonerCell(ColonySnapshot) → BuildPlan` (Erweiterung von Story 3.8)
2. Cell-Größe: 4×3 min, mit Prisoner-Bed (flagged), Lamp, Door, Floor
3. Placement nahe Home-Area aber separat (keine direkte Wohn-Nachbarschaft)
4. Nur ausführen wenn Story-4.3 aktiv Recruiting triggert
5. Unit-Tests Layout-Heuristik

## Tasks
- [ ] `BuildPlanner` erweitern
- [ ] Prisoner-Bed-Flag: `Bed.ForPrisoners = true` via Vanilla-API
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Mod-Leitfaden §5, Story 3.8 BuildPlan-Schema.
**Vorausgesetzt:** 3.8.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/BuildPlanner.cs` | modify |

## Testing
Unit: 4×3 Layout + Door-Position.

## Review-Gate
Code-Review gegen D-15.
