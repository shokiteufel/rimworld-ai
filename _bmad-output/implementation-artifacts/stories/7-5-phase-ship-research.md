# Story 7.5: PHASE_SHIP Research-Chain

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Ship-Ending Research-Chain-Priority**: Ship Basics → Ship Computer → Ship Reactor → Cryptosleep Casket → Ship Structural — in optimaler Reihenfolge via ResearchScheduler.

## Acceptance Criteria
1. `ShipResearchPlanner.Plan → List<string projectDefName>` in Ship-Required-Order
2. Dependencies respektiert (Ship Basics vor Ship Structural)
3. Auto-Switch nach Research-Done
4. DLC-Check: Ship-Tech ohne DLC (Vanilla-Pfad verfügbar)
5. Unit-Tests

## Tasks
- [ ] `Source/Decision/ShipResearchPlanner.cs`
- [ ] Research-Chain-Definition (Def-basiert)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Ship-Ending, Mod-Leitfaden §7.
**Vorausgesetzt:** 4.1, 7.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/ShipResearchPlanner.cs` | create |

## Testing
Unit: Chain-Reihenfolge korrekt.

## Review-Gate
Code-Review gegen D-15, DLC-Agnostik.
