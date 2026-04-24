# Story 7.5: PHASE_SHIP Research-Chain

**Status:** ready-for-dev
**Epic:** Epic 7 — Endings
**Size:** M

## Story
Als Mod-Entwickler möchte ich **Ship-Ending Research-Chain-Priority**: Ship Basics → Ship Computer → Ship Reactor → Cryptosleep Casket → Ship Structural — in optimaler Reihenfolge via ResearchScheduler.

## Acceptance Criteria
1. **DLC-Guard am Plan-Anfang** (MED-Fix, CC-STORIES-05): `if (!DlcCapabilities.EndingAvailable(Ending.Ship)) return ImmutableList<string>.Empty;` — Ship-Ending ist Vanilla aber **Royalty entfernt Ship-Reactor** (IncidentDef-Guard). `EndingFeasibility` (Story 7.1) dokumentiert: Ship = Vanilla + no-Royalty-Remove.
2. `ShipResearchPlanner.Plan → ImmutableList<string projectDefName>` in Ship-Required-Order (identifier-only D-23)
3. Dependencies respektiert (Ship Basics vor Ship Structural)
4. Auto-Switch nach Research-Done
5. Unit-Tests inkl. DLC-absent-Case (Royalty-only → Empty-Plan)

## Tasks
- [ ] `Source/Decision/ShipResearchPlanner.cs`
- [ ] Research-Chain-Definition (Def-basiert)
- [ ] Unit-Tests

## Dev Notes
**Kontext:** Ending-Pfade.md Ship-Ending, Mod-Leitfaden §7.
**Sub-Phase (Story 7.0):** Implementiert `ShipResearch` aus `EndingSubPhaseStateMachine`.
**Vorausgesetzt:** 4.1, 7.0, 7.1.

## File List
| Pfad | Op |
|---|---|
| `Source/Decision/ShipResearchPlanner.cs` | create |

## Testing
Unit: Chain-Reihenfolge korrekt.

## Review-Gate
Code-Review gegen D-15, DLC-Agnostik.
